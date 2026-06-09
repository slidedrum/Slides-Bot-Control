using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionFollowSelf : IPressAction
    {
        public string FriendlyName => "Follow Self";
        private string _FriendlyNameShort = "Follow";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => null;
        //public int? Priority => 0;
        public string pressTypeIdentifier => "Tap";
        public bool Invoke(Component BestComponent)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            PressActionManager.GetAction("Follow me").Invoke(BestBot.Agent);
            //zUpdater.Instance.StartCoroutine(CallAgentToFollow(BestBot.Agent));
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            if (!zSmartSelect.MainSelection.AnySelectedBotsAlive())
                return false;
            bool LookingDown = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.down) < 15f;
            if (!LookingDown)
                return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            PlayerAgent leader = BestBot.SyncValues?.Leader;
            if (leader != null && leader == zStaticRefrences.LocalPlayer) return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
