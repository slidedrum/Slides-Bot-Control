using BotControl.CustomActions;
using BotControl.CustomActions.CustomActions;
using BotControl.Menus;
using HarmonyLib;
using Player;

namespace BotControl.Patches
{
    [HarmonyPatch]
    public class FollowActionPatch
    {
        public static void Setup()
        {
            //defaultFollowSettings = new();
            //followerSettings = new();
            //followSettingsOverides.Clear();
            //myFollowSettingsOverides.Clear();
            //myFollowSettingsOverides[DRAMA_State.Exploration] = new()
            //{
            //    prio = 1,
            //    followLeaderRadius = 15,
            //    followLeaderMaxDistance = 30,
            //};
            //myFollowSettingsOverides[DRAMA_State.Alert] = new()
            //{
            //    prio = 5,
            //    followLeaderRadius = 10,
            //    followLeaderMaxDistance = 30,
            //};
            //myFollowSettingsOverides[DRAMA_State.Sneaking] = new()
            //{
            //    prio = 2,
            //    followLeaderRadius = 5,
            //    followLeaderMaxDistance = 30,
            //};
            //myFollowSettingsOverides[DRAMA_State.Encounter] = new()
            //{
            //    prio = 7,
            //    followLeaderRadius = 4,
            //    followLeaderMaxDistance = 5,
            //};
            //myFollowSettingsOverides[DRAMA_State.Combat] = new()
            //{
            //    prio = 14,
            //    followLeaderRadius = 7,
            //    followLeaderMaxDistance = 10,
            //};
            //myFollowSettingsOverides[DRAMA_State.Survival] = new()
            //{
            //    prio = 14,
            //    followLeaderRadius = 7,
            //    followLeaderMaxDistance = 10,
            //};
            //myFollowSettingsOverides[DRAMA_State.IntentionalCombat] = new()
            //{
            //    prio = 14,
            //    followLeaderRadius = 7,
            //    followLeaderMaxDistance = 10,
            //};
        }

        [HarmonyPatch(typeof(RootPlayerBotAction), nameof(RootPlayerBotAction.Update))]
        [HarmonyPrefix]
        public static bool PreUpdate(RootPlayerBotAction __instance, ref bool __result)
        {
            ////We need to reset the best action watcher before we start calling vanilla actions.
            //var data = zActions.GetOrCreateData(__instance);
            //data.consideringActions = true;
            //data.bestAction = null;

            //TODO set up parralell overideTrees for each bot
            //TODO if this gets called every frame, maybe cache the values untill something changes in overide tree
            __instance.         m_followLeaderAction.Prio =         (float)zSlideComputer.ActionPriorities.ValueAt(DramaManager.CurrentStateEnum.ToString());
            RootPlayerBotAction.m_prioSettings.FollowLeaderRadius = (float)FollowMenuClass.followRadius.GetValue();
            RootPlayerBotAction.s_followLeaderRadius =              (float)FollowMenuClass.followRadius.GetValue();
            RootPlayerBotAction.s_followLeaderMaxDistance =         (float)FollowMenuClass.maxDistance.GetValue();
            var follow = __instance.m_followLeaderAction;
            var bot = __instance.m_bot;
            var agent = bot.Agent;
            bool recall = follow.Client != null
                && !zActions.DoingAnyManualAction(agent)
                && !zActions.AnyCustomActionRunning(bot)
                && (follow.Client.Position - agent.Position).sqrMagnitude
                    > RootPlayerBotAction.s_followLeaderMaxDistance
                    * RootPlayerBotAction.s_followLeaderMaxDistance;

            //follow.FormationPrio = recall ? follow.Prio : RootPlayerBotAction.m_prioSettings.FollowLeaderFormation;
            return true;
        }
        [HarmonyPatch(typeof(RootPlayerBotAction), nameof(RootPlayerBotAction.UpdateActionFollowPlayer))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)] //Needed for betterbots compat
        public static bool UpdateActionFollowPlayerPrePatch(RootPlayerBotAction __instance, ref PlayerBotActionBase.Descriptor bestAction)
        {
            var dramaState = DramaManager.CurrentStateEnum;
            bool Allowed = (bool)zSlideComputer.ActionPermissions.ValueAt(dramaState.ToString());
            if (Allowed)
                return true;
            zSlideComputer.RemoveActionsOfType(typeof(PlayerBotActionFollow));
            return false;
        }
    }
}
