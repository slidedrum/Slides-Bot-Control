using System;
using BepInEx.Unity.IL2CPP.Hook;
using BotControl.Patches;
using GTFO.API;
using Il2CppInterop.Runtime.Runtime;
using Player;
using UnityEngine;

namespace BotControl.CustomActions
{
    internal static class PlayerAIBotApplyRestrictionsNativePatch
    {
        private static INativeDetour? ApplyRestrictionsDetour;

        public static unsafe void Apply()
        {
            ApplyRestrictionsDetour = INativeDetour.CreateAndApply(
                (nint)Il2CppAPI.GetIl2CppMethod<PlayerAIBot>(
                    nameof(PlayerAIBot.ApplyRestrictionsToRootPosition),
                    typeof(bool).Name,
                    isGeneric: false,
                    new[]
                    {
                        typeof(Vector3).MakeByRefType().Name,
                        typeof(float).MakeByRefType().Name
                    }),
                ApplyRestrictionsPatch,
                out d_ApplyRestrictionsToRootPosition _);
        }

        private unsafe delegate bool d_ApplyRestrictionsToRootPosition(
            IntPtr _this,
            IntPtr testPosition,
            IntPtr restrictionPrio,
            Il2CppMethodInfo* methodInfo);

        private static unsafe bool ApplyRestrictionsPatch(
            IntPtr _this,
            IntPtr testPositionPtr,
            IntPtr restrictionPrioPtr,
            Il2CppMethodInfo* methodInfo)
        {
            PlayerAIBot bot = new PlayerAIBot(_this);

            Vector3 testPosition = ReadVector3(testPositionPtr);
            float restrictionPrio = *(float*)restrictionPrioPtr;

            bool result = ApplyRestrictionsToRootPositionImpl(
                bot,
                ref testPosition,
                ref restrictionPrio);

            WriteVector3(testPositionPtr, testPosition);
            *(float*)restrictionPrioPtr = restrictionPrio;
            return result;
        }

        private static unsafe Vector3 ReadVector3(IntPtr ptr)
        {
            float* p = (float*)ptr;
            return new Vector3(p[0], p[1], p[2]);
        }

        private static unsafe void WriteVector3(IntPtr ptr, Vector3 value)
        {
            float* p = (float*)ptr;
            p[0] = value.x;
            p[1] = value.y;
            p[2] = value.z;
        }
        internal static bool ApplyRestrictionsToRootPositionImpl(
            PlayerAIBot __instance,
            ref Vector3 testPosition,
            ref float restrictionPrio)
        {
            dataStore data = zActions.GetOrCreateData(__instance);
            float resultPrio = restrictionPrio;
            Vector3 resultPos = testPosition;

            for (int i = 0; i < data.m_queuedActions.Count; i++)
            {
                Vector3 tmpPos = testPosition;
                PlayerBotActionBase.Descriptor descriptor = PlayerAiBotPatch.Canon(data.m_queuedActions[i]);
                if (descriptor.Prio > resultPrio && descriptor.ApplyPositionRestriction(ref tmpPos))
                {
                    resultPrio = descriptor.Prio;
                    resultPos = tmpPos;
                }
            }

            for (int j = 0; j < data.m_actions.Count; j++)
            {
                Vector3 tmpPos = testPosition;
                PlayerBotActionBase.Descriptor descriptor = PlayerAiBotPatch.Canon(PlayerAiBotPatch.Canon(data.m_actions[j]).DescBase);
                if (descriptor.Prio > resultPrio && descriptor.ApplyPositionRestriction(ref tmpPos))
                {
                    resultPrio = descriptor.Prio;
                    resultPos = tmpPos;
                }
            }

            if (resultPrio <= restrictionPrio)
                return false;

            restrictionPrio = resultPrio;
            testPosition = resultPos;
            return true;
        }
    }
}
