using Player;
using UnityEngine;
using System;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionDropHere : IPressAction
    {
        public string FriendlyName => "Drop Here";
        public string FriendlyNameShort => "Drop-H";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Hold";
        public int? Priority => 10;
        public bool Invoke(Component BestComponent)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.InLevelCarry);
            if (item == null) return false;
            PlayerBackpackManager.WantToDropItem(BestBot.Agent.Owner, item.Instance.Get_pItemData(), zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos, BestBot.Agent.Rotation, true);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.InLevelCarry) == null) return false;
            return false;
        }
    }
}
