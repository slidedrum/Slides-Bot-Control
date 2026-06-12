using BotControl.zRootBotPlayerAction;
using Player;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionCancelAll : IPressAction
    {
        public string FriendlyName => "Cancel All";
        public string FriendlyNameShort => "Cancel-A";
        public string FriendlyIdentifier => "Cancel Action";
        public Il2CppSystem.Type Type => null;
        public bool Enabled => true;
        public int? Priority => 20;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent)
        {
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1);
            //if (zActions.manualActions.Count <= 1) return false;
            HashSet<PlayerAIBot> BotsWithManualActions = new();
            foreach (var action in zActions.manualActions[zStaticRefrences.LocalPlayer.CharacterID])
            {
                zBotActions.CancelBotAction(action.ID);
                BotsWithManualActions.Add(action.Bot);
            }
            foreach (var bot in BotsWithManualActions)
            {
                zChatHandler.sendChatMessage("Nevermind.", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", bot.Agent, zStaticRefrences.LocalPlayer);
            }
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            // Candidate is irrelevant for this action, we just need to check if we have any bots selected
            bool facingUp = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.up) < 15f;
            if (!facingUp) return false;
            if (zActions.manualActions[zStaticRefrences.LocalPlayer.CharacterID].Count <= 1) return false;
            return true;
        }
    }
}
