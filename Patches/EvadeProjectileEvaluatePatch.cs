using HarmonyLib;
using Player;
using UnityEngine;

namespace BotControl.Patches
{
    [HarmonyPatch]
    internal static class EvadeProjectileEvaluatePatch
    {
        const float SideStepDistance = 2f;

        [HarmonyPatch(typeof(PlayerBotActionEvadeProjectile.Descriptor), nameof(PlayerBotActionEvadeProjectile.Descriptor.Evaluate))]
        [HarmonyPostfix]
        static void PostEvaluate(PlayerAIBot bot, ref bool __result)
        {
            if (!__result)
                return;

            __result = false;
            if (bot?.Agent == null || ProjectileManager.Current?.m_projectiles == null)
                return;

            Vector3 botPos = bot.Agent.Position;
            float missLimit = SideStepDistance * 0.85f;

            foreach (ProjectileTargeting proj in ProjectileManager.Current.m_projectiles)
            {
                if (proj == null)
                    continue;

                Vector3 offset = botPos - proj.transform.position;
                float along = Vector3.Dot(offset, proj.TravelDirection);
                if (along <= 0f)
                    continue;

                float miss = Mathf.Sqrt(Mathf.Max(0f, offset.sqrMagnitude - along * along));
                if (miss < missLimit)
                {
                    __result = true;
                    return;
                }
            }
        }
    }
}