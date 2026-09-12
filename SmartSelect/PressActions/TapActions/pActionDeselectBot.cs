using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionDeselectBot : IPressAction
    {
        public string FriendlyName => "Deselect Bot";
        public string _FriendlyNameShort => "Deselect";
        public string FriendlyIdentifier => "Select";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAIBot>();
        public string pressTypeIdentifier => "Tap";
        public int? Priority => -10;
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            PlayerAIBot Bot = BestComponent.TryCast<PlayerAIBot>();
            if (Bot == null)
                return false;
            zSmartSelect.MainSelection.Deselect(Bot);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1);
            zChatHandler.sendChatMessage("Nevermind.", FriendlyIdentifier + IPressAction.chatPermSuffix, Bot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            PlayerAIBot Bot = candidate.TryCast<PlayerAIBot>();
            if (Bot == null)
                return false;
            if (!zSmartSelect.MainSelection.Selected(Bot))
                return false;
            Color = Bot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
