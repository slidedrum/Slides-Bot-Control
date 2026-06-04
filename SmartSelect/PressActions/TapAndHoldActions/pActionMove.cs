using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapAndHoldActions
{
    public class pActionMove : IPressAction
    {
        public string FriendlyName => "Move To";
        public string FriendlyNameShort => "Move";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Tap and Hold";

        public bool Invoke(Component BestComponent)
        {
            // NOTE BestComponent is null!
            //TODO
            return false;
        }

        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            var destinationPosition = zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos;
            if (!zHelpers.PositionIsValidForAgent(BestBot.Agent, ref destinationPosition)) return false;
            if (!zHelpers.CanBotReach(BestBot, destinationPosition)) return false;
            return true;
        }
    }
}
