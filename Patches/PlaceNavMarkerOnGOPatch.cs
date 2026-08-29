using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;

namespace BotControl.Patches
{
    [HarmonyPatch]
    internal class PlaceNavMarkerOnGOPatch
    {
        static Dictionary<IntPtr, string> s_lines = new();
        private static readonly Dictionary<string, string> actionNameMap = new()
        {
            { "Player.PlayerBotActionAttack", "Attack" },
            { "Player.PlayerBotActionBackstab", "Backstab" },
            { "Player.PlayerBotActionBioscanProximity", "BioscanProxi" },
            { "Player.PlayerBotActionCarryExpeditionItem", "Carry" },
            { "Player.PlayerBotActionCollectItem", "Pickup" },
            { "Player.PlayerBotActionDeploySentryGun", "Sentry" },
            { "Player.PlayerBotActionDeployTripMine", "TripMine" },
            { "Player.PlayerBotActionFollow", "Follow" },
            { "Player.PlayerBotActionGatherDeployables", "Gather" },
            { "Player.PlayerBotActionHighlight", "Highlight" },
            { "Player.PlayerBotActionHurt", "Hurt" },
            { "Player.PlayerBotActionIdle", "Idle" },
            { "Player.PlayerBotActionLook", "Look" },
            { "Player.PlayerBotActionMele", "Mele" },
            { "Player.PlayerBotActionReloadWeapon", "Reload" },
            { "Player.PlayerBotActionRevive", "Revive" },
            { "Player.PlayerBotActionShareResourcePack", "Share" },
            { "Player.PlayerBotActionThrowItem", "Throw" },
            { "Player.PlayerBotActionTravel", "Travel" },
            { "Player.PlayerBotActionTurn", "Turn" },
            { "Player.PlayerBotActionUseBioscan", "Bioscan" },
            { "Player.PlayerBotActionUseEnemyScanner", "Scanner" },
            { "Player.PlayerBotActionUseFirearm", "Firearm" },
            { "Player.PlayerBotActionUseUseGlueGun", "C-Foarm" },
            { "Player.PlayerBotActionUseUseLadder", "Ladder" },
            { "Player.PlayerBotActionUseWalk", "Walk" },
        };
        [HarmonyPatch(typeof(PlaceNavMarkerOnGO), nameof(PlaceNavMarkerOnGO.UpdateName))]
        [HarmonyPrefix]
        static void Prefix(PlaceNavMarkerOnGO __instance, ref string extraInfo)
        {
            if (string.IsNullOrEmpty(extraInfo))
                return;
            if (!s_lines.TryGetValue(__instance.Pointer, out var line) || string.IsNullOrEmpty(line))
                return;
            extraInfo = extraInfo + "\n" + line;
        }
        internal static void UpdateNavMarker(PlaceNavMarkerOnGO navMarker, string newText) 
        {
            s_lines[navMarker.Pointer] = newText;
            navMarker.OnPlayerInfoUpdated(true);
        }
        internal static void OnBotActionChanged(PlayerAIBot bot)
        {
            string currentAction = GetCurrentAction(bot);
            if (actionNameMap.ContainsKey(currentAction))
                currentAction = actionNameMap[currentAction];
            UpdateNavMarker(bot.Agent.NavMarker, currentAction);
        }
        public static string GetCurrentAction(PlayerAIBot bot)
        {
            PlayerBotActionBase MostImportantAction = null;
            //ZiMain.log.LogInfo("---");
            foreach (PlayerBotActionBase action in bot.Actions)
            {
                //ZiMain.log.LogInfo($"{action.ToString()} - {action.DescBase.Prio} - {action.DescBase.ParentActionBase?.ToString()}");
                if (action.DescBase.ParentActionBase == null)
                    MostImportantAction = action;
            }
            //ZiMain.log.LogInfo(MostImportantAction.ToString());
            //ZiMain.log.LogInfo("---");
            return MostImportantAction.ToString();
        }
    }
}
