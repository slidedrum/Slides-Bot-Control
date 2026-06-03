using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionSelect : IPressAction
    {
        public string FriendlyName => "Select Bot";
        public string FriendlyNameShort => "Select";
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
            var voiceID = zSmartSelect.GetVoiceId(Bot);

            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, voiceID);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Hey {botName}!", 1);
            ZiMain.BotBarkBack(botId, AK.EVENTS.PLAY_CL_YES, "Yes?");
            if ((bool)zSlideComputer.ActionPermissions.ValueAt("Notify smart selected"))
                ZiMain.sendChatMessage("I'm ready", Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot Bot = candidate.Cast<PlayerAIBot>();
            if (Bot == null)
                return false;
            if (zSmartSelect.MainSelection.Selected(Bot))
                return false;
            return true;
        }
    }
}
