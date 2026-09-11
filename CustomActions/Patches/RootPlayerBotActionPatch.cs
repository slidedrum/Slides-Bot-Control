using HarmonyLib;
using Player;

namespace BotControl.CustomActions.Patches
{
    [HarmonyPatch]
    public class RootPlayerBotActionPatch
    {
        [HarmonyPatch(typeof(RootPlayerBotAction), nameof(RootPlayerBotAction.Update))]
        [HarmonyPrefix]
        public static bool PreUpdate(RootPlayerBotAction __instance, ref bool __result)
        {
            var data = zActions.GetOrCreateData(__instance);
            data.bestAction = null;
            if (!__instance.IsActive()) // Mirrors base.update
            {
                __result = true;
                return false;
            }
            __instance.RefreshGearAvailability();
            data.bestAction = null;
            __instance.UpdateActionEvadeProjectiles(ref data.bestAction);
            __instance.UpdateActionTagEnemies(ref data.bestAction);
            __instance.UpdateActionAttack(ref data.bestAction);
            __instance.UpdateActionReviveTeammate(ref data.bestAction);
            __instance.UpdateActionUseBioscan(ref data.bestAction);
            __instance.UpdateActionShareResoursePack(ref data.bestAction);
            __instance.UpdateActionHighlight(ref data.bestAction);
            __instance.UpdateActionUseEnemyScanner(ref data.bestAction);
            __instance.UpdateActionCollectItem(ref data.bestAction);
            __instance.UpdateActionUnlock(ref data.bestAction);
            __instance.UpdateActionFollowPlayer(ref data.bestAction);
            __instance.UpdateActionIdle(ref data.bestAction);
            __instance.UpdateFlashlightState();
            __instance.UpdateDropExpeditionItem();
            foreach (var act in data.customActionDescriptors)
            {
                if (!__instance.m_bot.IsActionForbidden(act))
                    act.CompareAction(__instance.m_bot, ref data.bestAction);
            }
            if (data.bestAction != null && data.bestAction.IsTerminated())
            {
                __instance.StartAction(data.bestAction);
            }
            __result = !__instance.IsActive();
            return false;
        }
        //[HarmonyPatch(typeof(RootPlayerBotAction), nameof(RootPlayerBotAction.Update))]
        //[HarmonyPostfix]
        //public static void PostUpdate(RootPlayerBotAction __instance, ref bool __result)
        //{
        //    //after vanilla actions eval we need to eval custom actions.
        //    //Whatever vanilla action is best still gets called no matter what, might want to chagne that?  Might not be a problem?
        //    var data = zActions.GetOrCreateData(__instance);
        //    foreach (var act in data.customActionDescriptors)
        //    {
        //        act.CompareAction(__instance.m_bot, ref data.bestAction);
        //    }
        //    if (data.bestAction != null && data.bestAction.IsTerminated())
        //    {
        //        __instance.m_bot.StartAction(data.bestAction);
        //    }
        //}
    }
}