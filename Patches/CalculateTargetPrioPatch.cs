using Agents;
using BepInEx.Unity.IL2CPP.Hook;
using GTFO.API;
using Il2CppInterop.Runtime.Runtime;
using Player;
using System;

namespace BotControl.Patches
{
    // Enemies past follow maxDistance are not valid attack targets (same as no line of sight).
    // Without this, a bot on the outer follow ring keeps picking the closest enemy just outside
    // that range, melee travel clamps to the rim, and Follow cannot walk back to the inner radius.
    // Harmony cannot detour this method (float + out stance); INativeDetour matches the IL2CPP ABI.
    public static class CalculateTargetPrioPatch
    {
        private static INativeDetour Detour;
        private static d_CalculateTargetPrio Original;

        private unsafe delegate float d_CalculateTargetPrio(
            IntPtr self,
            IntPtr targetAgent,
            IntPtr prioRange,
            bool isActiveTarget,
            uint* stance,
            Il2CppMethodInfo* methodInfo);

        internal unsafe static void ApplyNativePatch()
        {
            if (Detour != null)
                return;

            Detour = INativeDetour.CreateAndApply(
                (nint)Il2CppAPI.GetIl2CppMethod<RootPlayerBotAction>(
                    nameof(RootPlayerBotAction.CalculateTargetPrio),
                    typeof(float).Name,
                    false,
                    new[]
                    {
                        typeof(Agent).Name,
                        typeof(float[]).Name,
                        typeof(bool).Name,
                        typeof(PlayerBotActionAttack.StanceEnum).MakeByRefType().Name,
                    }),
                CalculateTargetPrio_Patch,
                out Original);
        }

        private unsafe static float CalculateTargetPrio_Patch(
            IntPtr self,
            IntPtr targetAgent,
            IntPtr prioRange,
            bool isActiveTarget,
            uint* stance,
            Il2CppMethodInfo* methodInfo)
        {
            if (self != IntPtr.Zero && targetAgent != IntPtr.Zero)
            {
                RootPlayerBotAction root = new(self);
                Agent target = new(targetAgent);
                PlayerAgent leader = root.m_bot?.SyncValues?.Leader;
                if (leader != null)
                {
                    float max = RootPlayerBotAction.s_followLeaderMaxDistance;
                    if ((target.Position - leader.Position).sqrMagnitude > max * max)
                    {
                        if (stance != null)
                            *stance = (uint)PlayerBotActionAttack.StanceEnum.All;
                        return 0f;
                    }
                }
            }
            return Original(self, targetAgent, prioRange, isActiveTarget, stance, methodInfo);
        }
    }
}
