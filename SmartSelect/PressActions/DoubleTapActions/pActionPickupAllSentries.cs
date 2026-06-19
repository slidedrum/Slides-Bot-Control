using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionPickupAllSentries : IInputAction
    {
        public string FriendlyName => "Pickup all Sentries";
        public string _FriendlyNameShort => "Pickup-A";
        public string FriendlyIdentifier => "Pickup Equipmenet";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<SentryGunInstance>();
        public string pressTypeIdentifier => "Double Tap";
        public int cycleOffset = 0;

        public bool Invoke(Component BestComponent)
        {
            var Botlist = ZiMain.GetBotList();
            List<PlayerAIBot> AliveBotsWithTurretsOut = GetBotsWithTurretsOut();
            if (AliveBotsWithTurretsOut.Count == 0) return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PICKUPYOURDEPLOYABLES);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Pick up your deployables.", 1);
            zUpdater.Instance.StartCoroutine(BotsPickupTurrets(AliveBotsWithTurretsOut));
            return true;
        }
        public static IEnumerator BotsPickupTurrets(List<PlayerAIBot> Bots)
        {
            foreach (var bot in Bots)
            {
                zChatHandler.sendChatMessage("Picking up my sentry.", "Pickup Equipmenet" + IInputAction.chatPermSuffix, bot.Agent, zStaticRefrences.LocalPlayer);
                zBotActions.SendBotToPickUpSentry(bot, zStaticRefrences.LocalPlayer);
                yield return new WaitForSeconds(0.25f);
            }
        }

        public bool IsActionValid(Component candidate)
        {
            List<PlayerAIBot> AliveBotsWithTurretsOut = GetBotsWithTurretsOut();
            if (AliveBotsWithTurretsOut.Count <= 1) return false;

            cycleOffset++;
            int groupIndex = (cycleOffset - 1) / 1;
            AliveBotsWithTurretsOut.Sort((a, b) =>
                a.GetInstanceID().CompareTo(b.GetInstanceID()));
            int index = groupIndex % AliveBotsWithTurretsOut.Count;
            Color = AliveBotsWithTurretsOut[index].Agent.Owner.PlayerColor;
            return true;
        }
        public List<PlayerAIBot> GetBotsWithTurretsOut()
        {
            List<PlayerAIBot> AliveBotsWithTurretsOut = new();
            var Botlist = ZiMain.GetBotList();
            List<PlayerAIBot> BotsWithTurretsOut = new();
            foreach (PlayerAIBot Bot in Botlist)
            {
                ItemEquippable[] deployedItems = Bot.GetDeployedItems().ToArray();
                if (deployedItems.Length == 0)
                    continue;
                if (zHelpers.TryGetAgentBackpackItem(Bot.Agent, InventorySlot.GearClass, out BackpackItem item))
                {
                    if (item.Instance.ArchetypeName == "Sentry Gun")
                        BotsWithTurretsOut.Add(Bot);
                }

            }
            if (BotsWithTurretsOut.Count == 0) return AliveBotsWithTurretsOut;
            foreach (var Bot in BotsWithTurretsOut)
            {
                if (Bot.Agent.Alive)
                    AliveBotsWithTurretsOut.Add(Bot);
            }
            return AliveBotsWithTurretsOut;
        }
    }
}
