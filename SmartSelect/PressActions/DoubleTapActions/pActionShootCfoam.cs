using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionShootCFoam : IPressAction
    {
        public string FriendlyName => "Shoot Cfoam";
        public string FriendlyNameShort => "cFoam";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent)
        { 
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!Evaluate(BestBot.Agent)) return false;
            zBotActions.SendBotToUseCfoamGun(BestBot, zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos, zStaticRefrences.LocalPlayer, 0);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CFOAMHERE);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Put a mine here.", 1);
            ZiMain.BotBarkBack(BestBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 2f);
            return true;
        }
        public bool Evaluate(PlayerAgent ownerAgent)
        {
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(ownerAgent.Owner);
            if (backpack == null)
            {
                return false;
            }
            if (backpack.AmmoStorage.ClassAmmo.BulletsInPack > 0)
            {
                BackpackItem backpackItem = backpack.Slots[3];
                if (backpackItem != null && backpackItem.Instance != null && backpackItem.ItemID == 73U)
                {
                    //backpackItem.Instance as ItemEquippable != null;
                    return true;
                }
            }
            return false;
        }

        public bool IsActionValid(Component candidate)
        {
            // candidate is null
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!Evaluate(BestBot.Agent)) return false;
            return true;
        }
    }
}
