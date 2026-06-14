using BotControl.CustomActions.CustomActions;
using Player;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionDropHere : IPressAction
    {
        public string FriendlyName => "Drop Here";
        public string FriendlyNameShort => "Drop-H";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Hold";
        public string FriendlyIdentifier => "Drop Objective";
        public bool Enabled => false;
        public int? Priority => 10;
        public bool Invoke(Component BestComponent)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.InLevelCarry);
            if (item == null) return false;
            //PlayerBackpackManager.WantToDropItem(BestBot.Agent.Owner, item.Instance.Get_pItemData(), zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos, BestBot.Agent.Rotation, true);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            DropHereAction.Descriptor desc = new DropHereAction.Descriptor(BestBot)
            {
                DropPosition = zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos,
                Prio = 13
            };
            BestBot.StartAction(desc);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.InLevelCarry) == null) return false;
            return true;
        }
    }
}
