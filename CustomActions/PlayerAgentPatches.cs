//using HarmonyLib;
//using Player;
//using System;
//using System.Reflection;

//namespace BotControl.CustomActions
//{
//    /// <summary>
//    /// Minimal patches for <see cref="PlayerAgent"/> bypass paths that do not go through
//    /// patched <see cref="PlayerAIBot"/> methods.
//    /// </summary>
//    public static class PlayerAgentPatch
//    {
//        public static void Apply(Harmony harmony, Action<string> log = null)
//        {
//            int patched = 0;
//            if (TryPatch(harmony, log, AccessTools.DeclaredMethod(typeof(PlayerAgent), "OnInteractionStarted", new[] { typeof(PlayerAgent) }), nameof(OnInteractionStarted_Prefix))) patched++;
//            if (TryPatch(harmony, log, AccessTools.DeclaredMethod(typeof(PlayerAgent), "OnInteractionFinished"), nameof(OnInteractionFinished_Prefix))) patched++;
//            log?.Invoke($"PlayerAgentPatch: applied {patched} patches");
//        }

//        private static bool TryPatch(Harmony harmony, Action<string> log, MethodInfo method, string prefixName)
//        {
//            if (method == null)
//            {
//                log?.Invoke($"PlayerAgentPatch: missing method for {prefixName}");
//                return false;
//            }

//            try
//            {
//                harmony.Patch(method, prefix: new HarmonyMethod(typeof(PlayerAgentPatch), prefixName));
//                return true;
//            }
//            catch (Exception ex)
//            {
//                log?.Invoke($"PlayerAgentPatch: failed {method.Name}: {ex.Message}");
//                return false;
//            }
//        }

//        /// <summary>
//        /// Original increments IL2CPP <c>m_interactionCounter</c> on <see cref="PlayerAIBot"/> directly.
//        /// </summary>
//        public static bool OnInteractionStarted_Prefix(PlayerAgent __instance)
//        {
//            if (!TryGetBot(__instance, out PlayerAIBot bot))
//                return true;

//            Instances.GetManaged(bot).OnInteractionStarted();
//            return false;
//        }

//        /// <summary>
//        /// Original decrements IL2CPP <c>m_interactionCounter</c> on <see cref="PlayerAIBot"/> directly.
//        /// </summary>
//        public static bool OnInteractionFinished_Prefix(PlayerAgent __instance)
//        {
//            if (!TryGetBot(__instance, out PlayerAIBot bot))
//                return true;

//            Instances.GetManaged(bot).OnInteractionFinished();
//            return false;
//        }

//        private static bool TryGetBot(PlayerAgent agent, out PlayerAIBot bot)
//        {
//            bot = null;
//            if (agent?.Owner == null || !agent.Owner.IsBot)
//                return false;

//            bot = agent.GetComponent<PlayerAIBot>();
//            return bot != null;
//        }
//    }
//}
