using Agents;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionStopAll : IPressAction
    {
        public string FriendlyName => "Stop All";
        private string _FriendlyNameShort = "Stop";
        public string FriendlyIdentifier => "Stop All";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAgent>();
        public bool Enabled => true;
        public int? Priority => 5;
        public string pressTypeIdentifier => "Double Tap";
        internal static float MaxLookAngle = 5f;
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            PlayerAgent Agent = BestComponent.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            PlayerAIBot Bot = Agent.GetComponent<PlayerAIBot>();
            if (Bot == null) return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1);
            zBotActions.StopAllActions(Bot);
            zChatHandler.sendChatMessage("Nevermind.", FriendlyIdentifier + IPressAction.chatPermSuffix, Bot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            PlayerAgent Agent = candidate.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            if (!Agent.Alive) return false;
            if (!Agent.Owner.IsBot) return false;
            if (Vector3.Angle(zStaticRefrences.CameraTransform.forward, Agent.EyePosition - zStaticRefrences.CameraTransform.position) > MaxLookAngle)
                return false;
            Color = Agent.Owner.PlayerColor;
            return true;
        }
    }
}
