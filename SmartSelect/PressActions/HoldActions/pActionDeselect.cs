using Player;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionDeselect : IPressAction
    {
        public string FriendlyName => "Deselect All Bots";
        public string FriendlyNameShort => "Deselect";
        public string FriendlyIdentifier => "Select";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            if (!zSmartSelect.MainSelection.Selected<PlayerAIBot>())
                return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1);
            if ((bool)zSlideComputer.ActionPermissions.ValueAt("Notify smart selected"))
            {
                HashSet<PlayerAIBot> selectedBots = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>();
                foreach (PlayerAIBot selectedBot in selectedBots)
                {
                    zChatHandler.sendChatMessage("Nevermind.", FriendlyNameShort + "TalkInChatNotifyActionAcknowlage", selectedBot.Agent, zStaticRefrences.LocalPlayer);
                }
            }
            zSmartSelect.MainSelection.Deselect<PlayerAIBot>();
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            // Candidate is irrelevant for this action, we just need to check if we have any bots selected
            if (!zSmartSelect.MainSelection.Selected<PlayerAIBot>())
                return false;
            bool facingUp = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.up) < 15f;
            if (facingUp)
                return true;
            return false;
        }
    }
}
