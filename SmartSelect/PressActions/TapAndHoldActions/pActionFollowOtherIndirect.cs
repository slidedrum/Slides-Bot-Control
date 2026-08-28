using Agents;
using BotControl.SmartSelect.PressActions.DoubleTapActions;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapAndHoldActions
{
    public class pActionFollowOtherIndirect : IPressAction
    {
        public string FriendlyName => "Follow Other Indirect";
        private string _FriendlyNameShort = "Send";
        public string FriendlyIdentifier => "Follow";
        public string FriendlyNameShort => $"<color=#{TargetColorHex}>:</color><color=#{ColorHex}>{_FriendlyNameShort}</color><color=#{TargetColorHex}>:</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        private Color TargetColor = new Color(1f, 1f, 1f, 0.25f);
        private string TargetColorHex => ColorUtility.ToHtmlStringRGB(TargetColor);
        public Il2CppSystem.Type Type => null;
        public int? Priority => 5;
        public string pressTypeIdentifier => "Tap and Hold";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            PlayerAIBot Follower = zSmartSelect.MainSelection.GetBestBot();
            if (Follower == null) return false;
            if (!Follower.Agent.Alive) return false;
            PlayerAgent Followeee;
            if (BestComponent != null)
                Followeee = BestComponent.TryCast<PlayerAgent>();
            else
                Followeee = zSmartSelect.GetPlayerAgentLookingAt();
            if (Followeee == null) return false;
            if (Followeee == Follower.Agent) return false;
            pActionFollowDirect.StaticCallAgentToFollow(Follower.Agent, Followeee);
            //PressActionManager.GetAction("Follow me").Invoke(Follower);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            PlayerAIBot Follower = zSmartSelect.MainSelection.GetBestBot();
            if (Follower == null) return false;
            if (!Follower.Agent.Alive) return false;
            PlayerAgent Folowee = zSmartSelect.GetPlayerAgentLookingAt();
            if (Folowee == null) return false;
            if (Folowee == zStaticRefrences.LocalPlayer) return false;
            if (!zHelpers.IsObstructed(zStaticRefrences.CameraTransform.position, Folowee.gameObject, zStaticRefrences.LocalPlayer.gameObject)) return false;
            PlayerAgent leader = Follower.SyncValues?.Leader;
            if (leader != null && leader == Folowee) return false; // Are they already following this agent?
            if (Follower.Agent == Folowee) return false;
            Color = Follower.Agent.Owner.PlayerColor;
            TargetColor = Folowee.Owner.PlayerColor;
            return true;
        }
    }
}
