using Agents;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionSelect : IPressAction
    {
        public string FriendlyName => "Select Bot";
        public string _FriendlyNameShort => "Select";
        public string FriendlyIdentifier => "Select";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAIBot>();
        public string pressTypeIdentifier => "Tap";
        public bool Invoke(Component BestComponent)
        {
            PlayerAIBot Bot = BestComponent.TryCast<PlayerAIBot>();
            if (Bot == null)
                return false;
            zSmartSelect.MainSelection.Select(Bot);
            var Agent = Bot.Agent;
            var botName = Agent.PlayerName;
            var botId = Agent.CharacterID;
            var voiceID = zSmartSelect.GetVoiceId(Agent);

            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, voiceID);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Hey {botName}!", 1);
            ZiMain.BotBarkBack(botId, AK.EVENTS.PLAY_CL_YES, "Yes?");
            zChatHandler.sendChatMessage("I'm ready", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot Bot = candidate.Cast<PlayerAIBot>();
            if (Bot == null)
                return false;
            if (zSmartSelect.MainSelection.Selected(Bot))
                return false;
            Color = Bot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
