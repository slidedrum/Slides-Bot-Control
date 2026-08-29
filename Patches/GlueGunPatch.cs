using HarmonyLib;
using Player;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.Patches
{
    [HarmonyPatch]
    public static class GlueGunPatch
    {
        public static Dictionary<int, Vector3> standPos = new();

        public static Vector3 GetMovePosition(PlayerBotActionUseGlueGun action)
        {
            if (standPos.TryGetValue(action.m_bot.Agent.CharacterID, out var pos))
                return pos;
            var desc = action.m_desc;
            return desc.TargetType == PlayerBotActionUseGlueGun.TargetTypeEnum.Position
                ? desc.TargetObject.position
                : desc.TargetPosition;
        }

        [HarmonyPatch(typeof(PlayerBotActionUseGlueGun), nameof(PlayerBotActionUseGlueGun.VerifyCurrentPosition))]
        [HarmonyPrefix]
        static bool PreVerify(PlayerBotActionUseGlueGun __instance, ref bool __result)
        {
            Vector3 delta = __instance.m_bot.transform.position - GetMovePosition(__instance);
            __result = delta.sqrMagnitude < 0.2f;
            return false;
        }
    }
}
