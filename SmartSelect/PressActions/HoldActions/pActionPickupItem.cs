using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using System.Linq;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions
{
    public class pActionPickupItem : IPressAction
    {
        public string FriendlyName => "Pickup Item";
        public string FriendlyNameShort => "Pickup";
        public string FriendlyIdentifier => "Pickup Item";
        public Il2CppSystem.Type Type => Il2CppType.Of<ItemInLevel>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent) // TODO make sure bots don't instantly drop the item you just told them to pick up.  
                                                    // But then how do you tell bots they are allowed to pick stuff up again?
        {
            ItemInLevel Item = BestComponent.TryCast<ItemInLevel>();
            if (Item == null)
                return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>().FirstOrDefault();
            if (BestBot == null)
                return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_GRABTHEITEM);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Grab the item.", 1);
            zBotActions.SendBotToPickupItem(BestBot, Item, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            ItemInLevel Item = candidate.TryCast<ItemInLevel>();
            if (Item == null) 
                return false;
            var status = Item?.GetSyncComponent()?.GetCurrentState().status;
            if (status == null) 
                return false;
            if (status != ePickupItemStatus.PlacedInLevel) 
                return false;
            CarryItemPickup_Core pickupCore = Item.TryCast<CarryItemPickup_Core>();
            if (pickupCore != null && !pickupCore.IsInteractable)
                return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) 
                return false;
            if (!BestBot.Agent.Alive) 
                return false;
            if (!zHelpers.CanBotReach(BestBot, Item.transform.position)) 
                return false;
            return true;
        }
    }
}
