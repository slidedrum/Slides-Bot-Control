using Agents;
using BotControl.SmartSelect.PressActions.DoubleTapActions;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapAndHoldActions
{
    public class pActionFollowOtherDirect : IPressAction
    {
        public string FriendlyName => "Follow Other Direct";
        private string _FriendlyNameShort = "Send";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAgent>();
        public int? Priority => 15;
        public string pressTypeIdentifier => "Tap and Hold";
        public bool Invoke(Component BestComponent)
        {
            PlayerAIBot Follower = zSmartSelect.MainSelection.GetBestBot();
            if (Follower == null) return false;
            if (!Follower.Agent.Alive) return false;
            PlayerAgent Followeee = BestComponent.TryCast<PlayerAgent>();
            if (Followeee == null) return false;
            if (Followeee == Follower.Agent) return false;
            pActionFollowDirect.StaticCallAgentToFollow(Follower.Agent, Followeee);
            //PressActionManager.GetAction("Follow me").Invoke(Follower);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {

            PlayerAIBot Follower = zSmartSelect.MainSelection.GetBestBot();
            if (Follower == null) return false;
            if (!Follower.Agent.Alive) return false;
            PlayerAgent Folowee = candidate.Cast<PlayerAgent>();
            if (Folowee == null) return false;
            if (Folowee == zStaticRefrences.LocalPlayer) return false;
            PlayerAgent leader = Follower.SyncValues?.Leader;
            if (leader != null && leader == Folowee) return false; // Are they already following this agent?
            if (Follower.Agent == Folowee) return false;
            Color = Folowee.Owner.PlayerColor;
            return true;
        }
    }
}
