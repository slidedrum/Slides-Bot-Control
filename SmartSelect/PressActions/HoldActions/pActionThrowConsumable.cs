using Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionThrowConsumable : IPressAction
    {
        private static List<string> ThrowableArchatipes = new() { "Glow Stick", "C-Foam Grenade" , "Fog Repeller" };
        public string FriendlyName => "Throw Consumable";
        public string FriendlyIdentifier => "Throw Consumable";
        public string FriendlyNameShort => "Throw";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            PlayerAgent LocalPlayer = zStaticRefrences.LocalPlayer;
            PlayerBackpack Backpack = PlayerBackpackManager.GetBackpack(BestBot.Agent.Owner);
            if (Backpack.AmmoStorage.ConsumableAmmo.BulletsInPack <= 0) return false;
            BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.Consumable);
            string Archatype = item.Instance.ArchetypeName;
            if (!ThrowableArchatipes.Contains(Archatype)) return false;
            //Networking.pStructs.pThrowType ThrowType = Networking.pStructs.pThrowType.cFoam;
            if (Archatype == "Glow Stick")
            {
                //ThrowType = Networking.pStructs.pThrowType.Glowstick;
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_THROWSOMEGLOWSTICKSHERE);
                zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Throw a glowstick here.", 1f);
            }
            if (Archatype == "C-Foam Grenade")
            {
                //ThrowType = Networking.pStructs.pThrowType.cFoam;
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CFOAMHERE);
                zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Throw C-Foam here.", 1f);
            }
            if (Archatype == "Fog Repeller")
            {
                //ThrowType = Networking.pStructs.pThrowType.FogRepeller;
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PUTAFOGREPELLERHERE);
                zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Put a fog reppeler here.", 1f);
            }
            zBotActions.SendBotToThrowItem(LocalPlayer, BestBot.Agent, LocalPlayer.transform.position, LocalPlayer.FPSCamera.CameraRayPos);
            //zBotActions.SendBotToThrowItem(LocalPlayer, BestBot.Agent, ThrowType, LocalPlayer.transform.position, LocalPlayer.FPSCamera.CameraRayPos);
            zChatHandler.sendChatMessage("Will do.", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            if (zStaticRefrences.LocalPlayer.FPSCamera.CameraRayDist > 30) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            // find out what item they have, if any.
            PlayerBackpack Backpack = PlayerBackpackManager.GetBackpack(BestBot.Agent.Owner);
            if (Backpack.AmmoStorage.ConsumableAmmo.BulletsInPack <= 0) return false;
            BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.Consumable);
            if (!ThrowableArchatipes.Contains(item.Instance.ArchetypeName)) return false;
            if (!zHelpers.CanBotReach(BestBot, zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos)) return false;
            return true;
        }
    }
}
