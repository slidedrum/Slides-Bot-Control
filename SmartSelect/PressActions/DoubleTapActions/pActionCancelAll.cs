using BotControl.zRootBotPlayerAction;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionCancelAll : IPressAction
    {
        public string FriendlyName => "Cancel All";
        public string FriendlyNameShort => "Cancel-A";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent)
        {
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1);
            //if (zActions.manualActions.Count <= 1) return false;
            for (int i = 0; i < zActions.manualActions.Count; i++)
                if (zActions.manualActions.Count - 1 >= i && !zActions.manualActions[i].IsTerminated())
                    zActions.manualActions[i].Bot.StopAction(zActions.manualActions[i]);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            // Candidate is irrelevant for this action, we just need to check if we have any bots selected
            bool facingUp = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.up) < 15f;
            if (!facingUp) return false;
            if (zActions.manualActions.Count <= 1) return false;
            for (int i = 0; i < zActions.manualActions.Count; i++)
                if (zActions.manualActions.Count - 1 >= i && !zActions.manualActions[i].IsTerminated())
                    return true;
            return false;
        }
    }
}
