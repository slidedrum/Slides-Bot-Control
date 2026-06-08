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
            if (!PlayerBotActionUseGlueGun.Descriptor.Evaluate(BestBot)) return false;
            zBotActions.SendBotToUseCfoamGun(BestBot, zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos, zStaticRefrences.LocalPlayer, 0);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CFOAMHERE);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Put a mine here.", 1);
            ZiMain.BotBarkBack(BestBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 2f);
            return true;
        }
        
        public bool IsActionValid(Component candidate)
        {
            // candidate is null
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!PlayerBotActionUseGlueGun.Descriptor.Evaluate(BestBot)) return false;
            return true;
        }
    }
}
