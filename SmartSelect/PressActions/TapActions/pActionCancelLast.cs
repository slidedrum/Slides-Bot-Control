using BotControl.CustomActions;
using FluffyUnderware.DevTools.Extensions;
using Player;
using System.Linq;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionCancelLast : IPressAction
    {
        public string FriendlyName => "Cancel Last";
        private string _FriendlyNameShort = "Cancel";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public string FriendlyIdentifier => "Cancel Action";
        public Il2CppSystem.Type Type => null;
        public bool Enabled => true;
        public string pressTypeIdentifier => "Tap";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot) 
        {
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1);
            var Actions = zActions.GetPlayersManualActions(zStaticRefrences.LocalPlayer.Pointer);
            if (Actions.Count == 0) return false;
            ManualAction mAction = Actions.Last();
            zBotActions.CancelBotAction(mAction.ID);
            zChatHandler.sendChatMessage("Nevermind.", FriendlyIdentifier + IPressAction.chatPermSuffix, mAction.Bot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            // Candidate is irrelevant for this action, we just need to check if we have any bots selected
            bool facingUp = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.up) < 15f;
            if (!facingUp) return false;
            var Actions = zActions.GetPlayersManualActions(zStaticRefrences.LocalPlayer.Pointer);
            if (Actions.Count == 0) return false;
            if (Actions.Last().Bot?.Agent?.Owner == null) return false;
            Color = Actions.Last().Bot.Agent.Owner.PlayerColor;
            //if (zActions.manualActions.Last().IsTerminated()) return false;
            return true;
        }
    }
}
