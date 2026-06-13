//using HarmonyLib;
//using Player;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace BotControl.CustomActions
//{
//    public static class Instances
//    {
//        private static readonly Dictionary<IntPtr, ManagedPlayerAIbot> InstancesByBot = new();
//        internal static readonly Dictionary<IntPtr, ManagedPlayerAIbot> SyncTableToManaged = new();

//        public static ManagedPlayerAIbot GetManaged(PlayerAIBot component)
//        {
//            if (!InstancesByBot.TryGetValue(component.Pointer, out ManagedPlayerAIbot managed))
//            {
//                managed = new ManagedPlayerAIbot(component);
//                InstancesByBot[component.Pointer] = managed;
//                RegisterSyncTable(component, managed);
//            }

//            return managed;
//        }

//        internal static PlayerAIBot.SyncTable GetIl2CppSyncTable(PlayerAIBot component) =>
//            Traverse.Create(component).Field<PlayerAIBot.SyncTable>("m_syncValues").Value;

//        private static void RegisterSyncTable(PlayerAIBot component, ManagedPlayerAIbot managed)
//        {
//            PlayerAIBot.SyncTable syncTable = GetIl2CppSyncTable(component);
//            if (syncTable != null)
//                SyncTableToManaged[syncTable.Pointer] = managed;
//        }
//    }

//    /// <summary>
//    /// Call <see cref="Apply"/> from plugin Load. Do not rely on Harmony.PatchAll() for these patches —
//    /// void and non-void originals need different prefix signatures.
//    /// </summary>
//    public static class PlayerAIBotPatch
//    {
//        private static readonly Dictionary<MethodBase, MethodInfo> MirrorCache = new();

//        public static void Apply(Harmony harmony, Action<string> log = null)
//        {
//            int patched = 0;
//            int skipped = 0;
//            int missingMirror = 0;

//            foreach (MethodInfo method in EnumerateTargetMethods())
//            {
//                if (!TryGetMirrorMethod(method, out _))
//                {
//                    missingMirror++;
//                    log?.Invoke($"PlayerAIBotPatch: skipping {Describe(method)} (no mirror)");
//                    continue;
//                }

//                try
//                {
//                    if (IsVoidReturn(method))
//                    {
//                        harmony.Patch(
//                            method,
//                            prefix: new HarmonyMethod(typeof(PlayerAIBotPatch), nameof(VoidPrefix)));
//                    }
//                    else
//                    {
//                        harmony.Patch(
//                            method,
//                            prefix: new HarmonyMethod(typeof(PlayerAIBotPatch), nameof(NonVoidPrefix)));
//                    }

//                    patched++;
//                }
//                catch (Exception ex)
//                {
//                    skipped++;
//                    log?.Invoke($"PlayerAIBotPatch: failed to patch {Describe(method)}: {ex.Message}");
//                }
//            }

//            int accessorPatched = PlayerAIBotAccessorPatch.Apply(harmony, log);
//            int syncTablePatched = PlayerAIBotSyncTablePatch.Apply(harmony, log);

//            log?.Invoke(
//                $"PlayerAIBotPatch: applied {patched} method patches, {accessorPatched} accessor patches, {syncTablePatched} SyncTable patches, skipped {skipped} failures, {missingMirror} without mirror");
//        }

//        public static bool VoidPrefix(MethodBase __originalMethod, object __instance, object[] __args)
//        {
//            if (!TryForward(__originalMethod, __instance, __args, out _))
//                return true;

//            return false;
//        }

//        public static bool NonVoidPrefix(
//            MethodBase __originalMethod,
//            object __instance,
//            object[] __args,
//            ref object __result)
//        {
//            if (!TryForward(__originalMethod, __instance, __args, out __result))
//                return true;

//            return false;
//        }

//        private static IEnumerable<MethodInfo> EnumerateTargetMethods()
//        {
//            var seen = new HashSet<string>(StringComparer.Ordinal);

//            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(PlayerAIBot)))
//            {
//                if (!ShouldPatch(method))
//                    continue;

//                string key = MethodKey(method);
//                if (!seen.Add(key))
//                    continue;

//                yield return method;
//            }
//        }

//        private static bool ShouldPatch(MethodInfo method)
//        {
//            if (method.IsConstructor || method.IsAbstract)
//                return false;
//            if (method.Name == ".cctor")
//                return false;

//            // IL2CPP field/property accessors cannot be safely detoured.
//            if (method.IsSpecialName)
//                return false;
//            if (method.Name.StartsWith("get_", StringComparison.Ordinal) ||
//                method.Name.StartsWith("set_", StringComparison.Ordinal))
//                return false;

//            return true;
//        }

//        private static string MethodKey(MethodInfo method)
//        {
//            string parameters = string.Join(
//                ",",
//                method.GetParameters().Select(p => p.ParameterType.FullName));
//            return $"{method.Name}|{method.IsStatic}|{parameters}|{method.ReturnType.FullName}";
//        }

//        private static bool IsVoidReturn(MethodInfo method)
//        {
//            Type returnType = method.ReturnType;
//            return returnType == typeof(void) || returnType.FullName == "System.Void";
//        }

//        private static bool TryForward(MethodBase original, object instance, object[] args, out object result)
//        {
//            result = null;
//            if (!TryGetMirrorMethod(original, out MethodInfo mirror))
//                return false;

//            object target = original.IsStatic ? null : Instances.GetManaged((PlayerAIBot)instance);
//            result = mirror.Invoke(target, args);
//            return true;
//        }

//        private static bool TryGetMirrorMethod(MethodBase original, out MethodInfo mirror)
//        {
//            if (MirrorCache.TryGetValue(original, out mirror))
//                return mirror != null;

//            Type[] paramTypes = original.GetParameters().Select(p => p.ParameterType).ToArray();
//            mirror = AccessTools.Method(typeof(ManagedPlayerAIbot), original.Name, paramTypes);
//            if (mirror != null && mirror.IsStatic != original.IsStatic)
//                mirror = null;

//            if (mirror == null)
//                mirror = FindMirrorBySignature(original);

//            MirrorCache[original] = mirror;
//            return mirror != null;
//        }

//        private static MethodInfo FindMirrorBySignature(MethodBase original)
//        {
//            ParameterInfo[] origParams = original.GetParameters();
//            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
//                (original.IsStatic ? BindingFlags.Static : BindingFlags.Instance);

//            foreach (MethodInfo candidate in typeof(ManagedPlayerAIbot).GetMethods(flags))
//            {
//                if (!ShouldPatch(candidate))
//                    continue;
//                if (candidate.Name != original.Name)
//                    continue;
//                if (candidate.IsStatic != original.IsStatic)
//                    continue;

//                ParameterInfo[] candParams = candidate.GetParameters();
//                if (candParams.Length != origParams.Length)
//                    continue;

//                if (original is MethodInfo origMethod && !TypesMatch(candidate.ReturnType, origMethod.ReturnType))
//                    continue;

//                bool match = true;
//                for (int i = 0; i < origParams.Length; i++)
//                {
//                    if (!TypesMatch(candParams[i].ParameterType, origParams[i].ParameterType))
//                    {
//                        match = false;
//                        break;
//                    }
//                }

//                if (match)
//                    return candidate;
//            }

//            return null;
//        }

//        private static bool TypesMatch(Type a, Type b) =>
//            a == b || string.Equals(a.FullName, b.FullName, StringComparison.Ordinal);

//        private static string Describe(MethodBase method)
//        {
//            string parameters = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
//            string returns = method is MethodInfo info ? info.ReturnType.Name : "?";
//            return $"{method.DeclaringType?.Name}::{method.Name}({parameters}) -> {returns}";
//        }
//    }

//    /// <summary>
//    /// Redirect IL2CPP field accessors so external code reads managed bot state.
//    /// </summary>
//    public static class PlayerAIBotAccessorPatch
//    {
//        public static int Apply(Harmony harmony, Action<string> log = null)
//        {
//            int patched = 0;
//            if (TryPatchGetter(harmony, log, "get_Agent", nameof(GetAgent_Prefix))) patched++;
//            if (TryPatchGetter(harmony, log, "get_Backpack", nameof(GetBackpack_Prefix))) patched++;
//            if (TryPatchGetter(harmony, log, "get_Inventory", nameof(GetInventory_Prefix))) patched++;
//            if (TryPatchGetter(harmony, log, "get_Actions", nameof(GetActions_Prefix))) patched++;
//            if (TryPatchGetter(harmony, log, "get_SyncValues", nameof(GetSyncValues_Prefix))) patched++;
//            return patched;
//        }

//        private static bool TryPatchGetter(
//            Harmony harmony,
//            Action<string> log,
//            string getterName,
//            string prefixName)
//        {
//            MethodInfo getter = AccessTools.Method(typeof(PlayerAIBot), getterName);
//            if (getter == null)
//            {
//                log?.Invoke($"PlayerAIBotAccessorPatch: missing {getterName}");
//                return false;
//            }

//            try
//            {
//                harmony.Patch(getter, prefix: new HarmonyMethod(typeof(PlayerAIBotAccessorPatch), prefixName));
//                return true;
//            }
//            catch (Exception ex)
//            {
//                log?.Invoke($"PlayerAIBotAccessorPatch: failed {getterName}: {ex.Message}");
//                return false;
//            }
//        }

//        public static bool GetAgent_Prefix(PlayerAIBot __instance, ref PlayerAgent __result)
//        {
//            __result = Instances.GetManaged(__instance).Agent;
//            return false;
//        }

//        public static bool GetBackpack_Prefix(PlayerAIBot __instance, ref PlayerBackpack __result)
//        {
//            __result = Instances.GetManaged(__instance).Backpack;
//            return false;
//        }

//        public static bool GetInventory_Prefix(PlayerAIBot __instance, ref PlayerInventorySynced __result)
//        {
//            __result = Instances.GetManaged(__instance).Inventory;
//            return false;
//        }

//        public static bool GetActions_Prefix(PlayerAIBot __instance, ref List<PlayerBotActionBase> __result)
//        {
//            __result = Instances.GetManaged(__instance).Actions;
//            return false;
//        }

//        public static bool GetSyncValues_Prefix(PlayerAIBot __instance, ref PlayerAIBot.SyncTable __result)
//        {
//            ManagedPlayerAIbot managed = Instances.GetManaged(__instance);
//            managed.PushSyncValuesToIl2Cpp();
//            __result = Instances.GetIl2CppSyncTable(__instance);
//            return false;
//        }
//    }

//    /// <summary>
//    /// External code (e.g. PlayerAgent.TryWarpTo) writes SyncTable.Leader via the setter on the
//    /// IL2CPP table instance — forward those writes into managed state.
//    /// </summary>
//    public static class PlayerAIBotSyncTablePatch
//    {
//        public static int Apply(Harmony harmony, Action<string> log = null)
//        {
//            MethodInfo setter = AccessTools.Method(typeof(PlayerAIBot.SyncTable), "set_Leader");
//            if (setter == null)
//            {
//                log?.Invoke("PlayerAIBotSyncTablePatch: missing set_Leader");
//                return 0;
//            }

//            try
//            {
//                harmony.Patch(setter, prefix: new HarmonyMethod(typeof(PlayerAIBotSyncTablePatch), nameof(SetLeader_Prefix)));
//                return 1;
//            }
//            catch (Exception ex)
//            {
//                log?.Invoke($"PlayerAIBotSyncTablePatch: failed set_Leader: {ex.Message}");
//                return 0;
//            }
//        }

//        public static bool SetLeader_Prefix(PlayerAIBot.SyncTable __instance, PlayerAgent value)
//        {
//            if (!Instances.SyncTableToManaged.TryGetValue(__instance.Pointer, out ManagedPlayerAIbot managed))
//                return true;

//            managed.SyncValues.Leader = value;
//            return false;
//        }
//    }
//}
