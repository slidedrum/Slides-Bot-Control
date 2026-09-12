using BotControl.SmartSelect.PressActions.TapActions;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionStopAllIndirect : IPressAction
    {
        public string FriendlyName => "Stop All through walls";
        private string _FriendlyNameShort = "Stop";
        public string FriendlyIdentifier => "Stop All";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => null;
        public int? Priority => -20;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            PlayerAgent Agent;
            if (BestComponent != null)
                Agent = BestComponent.TryCast<PlayerAgent>();
            else
                Agent = zSmartSelect.GetPlayerAgentLookingAt();
            if (Agent == null) return false;
            return PressActionManager.GetAction("Stop All").Invoke(Agent, BestBot);
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            PlayerAgent Agent = zSmartSelect.GetPlayerAgentLookingAt();
            if (Agent == null) return false;
            if (!Agent.Alive) return false;
            if (!Agent.Owner.IsBot) return false;
            if (Vector3.Angle(zStaticRefrences.CameraTransform.forward, Agent.EyePosition - zStaticRefrences.CameraTransform.position) > pActionStopAll.AllowedLookAngle(Agent))
                return false;
            Color = Agent.Owner.PlayerColor;
            return true;
        }
    }
}
