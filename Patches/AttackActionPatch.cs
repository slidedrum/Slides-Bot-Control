using Gear;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public static Dictionary<IntPtr, List<InventorySlot>> AllowedGuns = new(); // TODO fix the memory leak with this dict not removing old items.
        [HarmonyPatch(typeof(RootPlayerBotAction), nameof(RootPlayerBotAction.UpdateActionAttack))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)] //Needed for betterbots compat?
        public static bool PreUpdateActionAttack(RootPlayerBotAction __instance, ref PlayerBotActionBase.Descriptor bestAction)
        {
            originalBestAction = bestAction;
            if (!FollowActionPatch.IsRecalling(__instance))
                return true;
            if (!__instance.m_attackAction.IsTerminated())
                __instance.m_bot.StopAction(__instance.m_attackAction);
            return false;
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
                if (!zSlideComputer.ActionPermissions.HasKey(actionKey))
                    continue;
                bool allowed = (bool)zSlideComputer.ActionPermissions.ValueAt(actionKey);
                if (allowed)
                    newMeans |= means;
            }
            if (newMeans == __instance.m_attackAction.Means)
                return;
            __instance.m_attackAction.Means = newMeans;
            //zSlideComputer.RemoveActionsOfType(__instance.m_agent, typeof(PlayerBotActionAttack));
        }
            [HarmonyPatch(typeof(PlayerBotActionAttack), nameof(PlayerBotActionAttack.IsWithinMeleeReach))]
            [HarmonyPrefix]
            public static bool PreIsWithinMeleeReach(PlayerBotActionAttack __instance, Vector3 testPosition, float reachMultiplier, ref bool __result) // Why did I do this?
            {
                if ((__instance.m_desc.Means & PlayerBotActionAttack.AttackMeansEnum.Bullet) != 0)
                    return true; // guns on: vanilla

                var leader = __instance?.m_bot?.SyncValues?.Leader;
                if (leader == null)
                {
                    __result = true;
                    return false;
                }

                float max = RootPlayerBotAction.s_followLeaderMaxDistance;
                __result = (__instance.m_bot.Agent.Position - leader.Position).sqrMagnitude <= max * max;
                return false;
            }
        [HarmonyPatch(typeof(PlayerBotActionAttack.__c__DisplayClass27_0) , nameof(PlayerBotActionAttack.__c__DisplayClass27_0._ChooseAttackOption_b__1))] //PlayerBotActionAttack.ChooseAttackOptionLocals.ScoreBullet
        [HarmonyPrefix]
        public static bool PreChooseBulletPatch(PlayerBotActionAttack.__c__DisplayClass27_0 __instance, BulletWeaponSynced weapon, ref float __result)  // Restrict the weapon to only ones in the list.
        {
            IntPtr pointer = __instance.__4__this.m_desc.Pointer; //attack descriptor
            if (!AllowedGuns.ContainsKey(pointer))
                return true;
            if (AllowedGuns[pointer].Contains(weapon.ItemDataBlock.inventorySlot))
                return true;
            __result = -1;
            return false;
        }
        [HarmonyPatch(typeof(PlayerBotActionAttack), nameof(PlayerBotActionAttack.CurrentAttackOptionNeedsReevaluation))]
        [HarmonyPrefix]
        public static bool PreCurrentAttackOptionNeedsReevaluationPatch(PlayerBotActionAttack __instance, bool allowPush, ref bool __result) // Force re-eval if holding an invalid weapon.  Will call choose bullet
        {
            IntPtr pointer = __instance.m_desc.Pointer; //attack descriptor
            if (!AllowedGuns.ContainsKey(pointer))
                return true;
            if (!AllowedGuns[pointer].Contains(__instance.m_inventory.WieldedSlot))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
