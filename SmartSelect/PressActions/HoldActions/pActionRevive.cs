using AK;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions
{
    public class pActionRevive : IPressAction
    {
        public string FriendlyName => "Revive Agent";
        private string _FriendlyNameShort = "Revive";
        public string FriendlyNameShort => $"<color=#{TargetColorHex}>:</color><color=#{ColorHex}>{_FriendlyNameShort}</color><color=#{TargetColorHex}>:</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        private Color TargetColor = new Color(1f, 1f, 1f, 0.25f);
        private string TargetColorHex => ColorUtility.ToHtmlStringRGB(TargetColor);
        public string FriendlyIdentifier => "Revive";
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAgent>();
        public string pressTypeIdentifier => "Hold";
        public int? Priority => 100;
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            PlayerAgent Agent = BestComponent.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            //PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            zBotActions.SendBotToReviveAgent(BestBot, Agent, zStaticRefrences.LocalPlayer, 0);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, EVENTS.PLAY_CL_INEEDHELP);
            zChatHandler.sendChatMessage($"Reving {Agent.PlayerName}.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            PlayerAgent Agent = candidate.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            if (Agent.Alive) return false;
            //PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!zHelpers.CanBotReach(BestBot, Agent.Position)) return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            TargetColor = Agent.Owner.PlayerColor;
            return true;
        }
    }
}
