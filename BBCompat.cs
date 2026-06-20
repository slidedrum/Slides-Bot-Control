using BetterBots.Components;
using BetterBots.Managers;
using BotControl.CustomActions;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BotControl
{
    [HarmonyPatch]
    public static class BBCompat
    {

        private static Dictionary<int, BotRecorder> botRecorders = new();
        public static void OnInit()
        {
            var original = AccessTools.Method(typeof(RootPlayerBotAction), nameof(RootPlayerBotAction.UpdateActionCollectItem));
            ZiMain.m_Harmony.Unpatch(original, HarmonyPatchType.Prefix, "com.east.bb");
            var method = AccessTools.Method(
                typeof(PlayerBotActionAttack),
                nameof(PlayerBotActionAttack.ChooseAttackOption));

            ZiMain.m_Harmony.Unpatch(
                method,
                HarmonyPatchType.Postfix,
                "com.east.bb");
        }
        public static void SetBotsOpenContainersToFalse()
        {
            ConfigurationPluginManager.BetterBotsOpenContainers = false;
        }
        public static BotRecorder GetBotRecorder(PlayerAIBot bot)
        {
            return GetBotRecorder(bot.Agent);
        }
        public static BotRecorder GetBotRecorder(PlayerAgent agent)
        {
            return GetBotRecorder(agent.PlayerSlotIndex);
        }
        public static BotRecorder GetBotRecorder(int index)
        {
            if (!PlayerManager.TryGetPlayerAgent(ref index, out var agent))
                throw new InvalidOperationException($"Could not find bot at index {index}");
            if (!botRecorders.ContainsKey(index) || botRecorders[index] == null)
                botRecorders[index] = agent.gameObject.GetComponent<BotRecorder>();
            return botRecorders[index];
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool CheckDanger(PlayerAgent agent)
        {
            BotRecorder recorder = GetBotRecorder(agent);
            if (recorder != null)
                return recorder.IsInDangerousSituation();
            return false;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool CheckReviveAllowed(PlayerAgent m_agent)
        {
            BotRecorder recorder = BBCompat.GetBotRecorder(m_agent);
            if (recorder != null)
                return !recorder.Brain.ReviveRestricted;
            return true;
        }
    }
    [HarmonyPatch]
    public static class BBPatches
    {
        [HarmonyPatch(typeof(PlayerBotManagerExtended), "IsBotCurrentlyBusyWithAction")]
        [HarmonyPostfix]
        public static void PostIsBotCurrentlyBusyWithAction(PlayerAIBot bot, ref bool __result)
        {
            if (__result)
                return;
            var key = bot.Agent.CharacterID;
            foreach(var kvp in zActions.manualActions)
            {
                foreach (var action in kvp.Value)
                {
                    if (action.Bot.Pointer == bot.Pointer)
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
        //[HarmonyPostfix]
        //[HarmonyAfter("com.east.bb")]
        //[HarmonyPatch(typeof(PlayerBotActionAttack), nameof(PlayerBotActionAttack.ChooseAttackOption))]
        //static void Post_ChooseAttackOption(PlayerBotActionAttack __instance, ref bool __result)
        //{
        //    bool allowedToMele = (bool)zSlideComputer.ActionPermissions.ValueAt("attackMeansMelee");
        //    bool allowedToShoot = (bool)zSlideComputer.ActionPermissions.ValueAt("attackMeansBullet");
        //    if (!allowedToMele)
        //    {
        //        __instance.m_currentAttackOption.Means &=
        //            ~PlayerBotActionAttack.AttackMeansEnum.Melee;
        //    }
        //    if (!allowedToShoot)
        //    {
        //        __instance.m_currentAttackOption.Means &=
        //            ~PlayerBotActionAttack.AttackMeansEnum.Bullet;
        //    }
        //    if (allowedToMele &&
        //        (__instance.m_currentAttackOption.Means &
        //         (PlayerBotActionAttack.AttackMeansEnum.Melee |
        //          PlayerBotActionAttack.AttackMeansEnum.Bullet)) == 0)
        //    {
        //        __instance.m_currentAttackOption.Means |=
        //            PlayerBotActionAttack.AttackMeansEnum.Melee;
        //    }
        //    //zSlideComputer.RemoveActionsOfType(__instance.m_agent, typeof(PlayerBotActionAttack));
        //}
    }

}
