using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionPickupAllMines : IPressAction
    {
        public string FriendlyName => "Pickup All Mines";
        public string _FriendlyNameShort => "A-Pickup-A";
        public string FriendlyIdentifier => "Pickup Equipmenet";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<MineDeployerInstance>();
        public string pressTypeIdentifier => "Double Tap";
        public int cycleOffset = 0;
        public bool Invoke(Component BestComponent)
        {
            List<PlayerAIBot> BotsWithDeployedMines = GetBotsWithDeployedMines();
            if (BotsWithDeployedMines.Count == 0) return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PICKUPYOURDEPLOYABLES);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Pick up your deployables.", 1);
            zUpdater.Instance.StartCoroutine(BotsPickupMines(BotsWithDeployedMines));
            return true;
        }
        public static IEnumerator BotsPickupMines(List<PlayerAIBot> Bots)
        {
            foreach (var bot in Bots)
            {
                zChatHandler.sendChatMessage("Picking up mines.", "Pickup Equipmenet" + "TalkInChatNotifyActionAcknowlage", bot.Agent, zStaticRefrences.LocalPlayer);
                zBotActions.SendBotToPickUpMine(bot, null, zStaticRefrences.LocalPlayer);
                yield return new WaitForSeconds(0.25f);
            }
        }
        public bool IsActionValid(Component candidate)
        {
            List<PlayerAIBot> BotsWithDeployedMines = GetBotsWithDeployedMines();
            if (BotsWithDeployedMines.Count <= 1)
                return false;
            cycleOffset++;
            int groupIndex = (cycleOffset - 1) / 1;
            BotsWithDeployedMines.Sort((a, b) =>
                a.GetInstanceID().CompareTo(b.GetInstanceID()));
            int index = groupIndex % BotsWithDeployedMines.Count;
            Color = BotsWithDeployedMines[index].Agent.Owner.PlayerColor;
            return true;
        }
        public List<PlayerAIBot> GetBotsWithDeployedMines()
        {
            List<PlayerAIBot> BotsWithDeployedMines = new();
            List<PlayerAIBot> allbots = ZiMain.GetBotList();
            foreach (PlayerAIBot bot in allbots)
            {
                if (!bot.Agent.Alive)
                    continue;
                if (bot.GetDeployedItems().Count <= 0)
                    continue;
                if (!zHelpers.TryGetAgentBackpackItem(bot.Agent, InventorySlot.GearClass, out BackpackItem item))
                    continue;
                if (item == null) 
                    continue;
                if (item.Instance.ArchetypeName != "Mine deployer")
                    continue;
                BotsWithDeployedMines.Add(bot);
            }
            return BotsWithDeployedMines;
        }
    }
}
