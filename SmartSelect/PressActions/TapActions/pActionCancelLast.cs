using BotControl.zRootBotPlayerAction;
using Player;
using SNetwork;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionCancelLast : IPressAction
    {
        public string FriendlyName => "Cancel Last";
        public string FriendlyNameShort => "Cancel";
        public string FriendlyIdentifier => "Cancel Action";
        public Il2CppSystem.Type Type => null;
        public bool Enabled => false;
        public string pressTypeIdentifier => "Tap";
        public bool Invoke(Component BestComponent) // Todo handle this on clients.  // TODO keep track of WHO gave the command and only cancel your actions.
        {
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1);
            //if (zBotActions.LastAction.IsTerminated()) return false;
            //zBotActions.LastAction.Bot.StopAction(zBotActions.LastAction);
            //zChatHandler.sendChatMessage("Nevermind.", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", zBotActions.LastAction.Bot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            // Candidate is irrelevant for this action, we just need to check if we have any bots selected
            bool facingUp = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.up) < 15f;
            if (!facingUp) return false;
            if (zActions.manualActions.Count == 0) return false;
            //if (zBotActions.LastAction.IsTerminated()) return false;
            return true;
        }
    }
}
