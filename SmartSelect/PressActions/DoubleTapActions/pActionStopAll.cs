using BotControl.CustomActions;
using Il2CppInterop.Runtime;
using Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionStopAll : IPressAction
    {
        public string FriendlyName => "Stop All";
        public string FriendlyNameShort => "Stop";
        public string FriendlyIdentifier => "Stop All";
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAIBot>();
        public bool Enabled => true;
        public int? Priority => 20;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            PlayerAIBot Bot = BestComponent.TryCast<PlayerAIBot>();
            if (Bot == null) return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1);

            zChatHandler.sendChatMessage("Nevermind.", FriendlyIdentifier + IPressAction.chatPermSuffix, mAction.Bot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            PlayerAIBot Bot = candidate.TryCast<PlayerAIBot>();
            if (Bot == null) return false;
            return true;
        }
    }
}
