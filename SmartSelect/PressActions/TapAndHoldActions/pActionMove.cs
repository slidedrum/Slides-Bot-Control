using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapAndHoldActions
{
    public class pActionMove : IPressAction
    {
        public static int collisionLayer = LayerManager.MASK_SENTRYGUN_CAMERARAY_MOVERHELPER;
        public string FriendlyName => "Move To";
        public string FriendlyNameShort => "Move";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Tap and Hold";
        public bool Invoke(Component BestComponent)
        {
            // NOTE BestComponent is null!
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            Vector3 TaregetLocation = zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos;
            zBotActions.SendbotToMoveToLocation(BestBot, TaregetLocation, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!Physics.SphereCast(zStaticRefrences.LocalPlayer.Position, 0.5f ,zStaticRefrences.CameraTransform.forward, out RaycastHit hit, 100f, collisionLayer))
                return false;
            var destinationPosition = hit.point;
            if (!zHelpers.PositionIsValidForAgent(BestBot.Agent, ref destinationPosition)) return false;
            if (!zHelpers.CanBotReach(BestBot, destinationPosition)) return false;
            return true;
        }
    }
}
