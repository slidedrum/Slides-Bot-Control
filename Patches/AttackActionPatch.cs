using BotControl.CustomActions;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Player.PlayerBotActionAttack;

namespace BotControl.Patches
{
    [HarmonyPatch]
    public class AttackActionPatch
    {
        private static PlayerBotActionBase.Descriptor originalBestAction;
        public static List<PlayerBotActionAttack.AttackMeansEnum> meansList =
            Enum.GetValues<PlayerBotActionAttack.AttackMeansEnum>()
                .Where(x =>
                    x != PlayerBotActionAttack.AttackMeansEnum.None &&
                    ((int)x & ((int)x - 1)) == 0)
                .ToList();
        [HarmonyPatch(typeof(RootPlayerBotAction), nameof(RootPlayerBotAction.UpdateActionAttack))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)] //Needed for betterbots compat?
        public static void PreUpdateActionAttack(RootPlayerBotAction __instance, ref PlayerBotActionBase.Descriptor bestAction)
        {
            originalBestAction = bestAction;
        }
        [HarmonyPatch(typeof(RootPlayerBotAction), nameof(RootPlayerBotAction.UpdateActionAttack))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)] //Needed for betterbots compat?
        public static void PostUpdateActionAttack(RootPlayerBotAction __instance, ref PlayerBotActionBase.Descriptor bestAction)
        { //this is used to restrict the means of the bots, so they can only use the selected means.
            if (bestAction == null)
                return;
            if (bestAction.TryCast<PlayerBotActionAttack.Descriptor>() == null)
                return;
            bool allowedToMele = (bool)zSlideComputer.ActionPermissions.ValueAt("attackMeansMelee");
            bool allowedToShoot = (bool)zSlideComputer.ActionPermissions.ValueAt("attackMeansBullet");
            if (allowedToMele == false && allowedToShoot == false)
            {
                bestAction = originalBestAction;
                return;
            }
            var newMeans = PlayerBotActionAttack.AttackMeansEnum.None;
            foreach (var means in meansList)
            {
                string actionKey = "attackMeans" + means.ToString();
                bool allowed = (bool)zSlideComputer.ActionPermissions.ValueAt(actionKey);
                if (allowed)
                    newMeans |= means;
            }

            if (newMeans == __instance.m_attackAction.Means)
                return;
            __instance.m_attackAction.Means = newMeans;
            zSlideComputer.RemoveActionsOfType(__instance.m_agent, typeof(PlayerBotActionAttack));
        }
        [HarmonyPatch(typeof(PlayerBotActionAttack), nameof(PlayerBotActionAttack.IsWithinMeleeReach))]
        [HarmonyPrefix]
        public static bool PreIsWithinMeleeReach(PlayerBotActionAttack __instance, Vector3 testPosition, float reachMultiplier, ref bool __result)
        {
            if ((__instance.m_desc.Means & AttackMeansEnum.Bullet) == 0)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
