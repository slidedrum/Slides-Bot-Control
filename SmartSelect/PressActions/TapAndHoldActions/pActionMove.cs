using Agents;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapAndHoldActions
{
    public class pActionMove : IPressAction
    {
        public static int collisionLayer = LayerManager.MASK_ENEMY_PROJECTILE_COLLIDERS;
        public string FriendlyName => "Move To";
        public string FriendlyNameShort => "Move";
        public string FriendlyIdentifier => "Move";
        public Il2CppSystem.Type Type => null;
        public int? Priority => 10;
        public string pressTypeIdentifier => "Tap and Hold";
        public bool Invoke(Component BestComponent)
        {
            // NOTE BestComponent is null!
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            Vector3 TaregetLocation = zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos;
            zBotActions.SendbotToMoveToLocation(BestBot, TaregetLocation, zStaticRefrences.LocalPlayer);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_HURRY);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Hurry.", 1f);
            zChatHandler.sendChatMessage("On the way.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            //if (!Physics.Raycast(zStaticRefrences.LocalPlayer.Position, zStaticRefrences.CameraTransform.forward, out RaycastHit hit, 10000f, collisionLayer))
            //    return false;
            var destinationPosition = zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos;
            if (!zHelpers.PositionIsValidForAgent(BestBot.Agent, ref destinationPosition)) return false;
            if (!zHelpers.CanBotReach(BestBot, destinationPosition)) return false;
            return true;
        }
    }
}
