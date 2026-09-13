using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionOpenContainer : IPressAction
    {
        public string FriendlyName => "Open Container";
        private string _FriendlyNameShort = "Open";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public string FriendlyIdentifier => "Open";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakResourceContainer>();
        public string pressTypeIdentifier => "Hold";
        public int? Priority => 2;
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            LG_WeakResourceContainer container = BestComponent.TryCast<LG_WeakResourceContainer>();
            zBotActions.SendBotToOpenContainer(BestBot, container, zStaticRefrences.LocalPlayer);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PLEASE);
            zChatHandler.sendChatMessage("Opening container.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }

        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            LG_WeakResourceContainer container = candidate.TryCast<LG_WeakResourceContainer>();
            if (container == null) return false;
            if (container.m_currentStatus == eResourceContainerStatus.Open) return false;
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!zHelpers.CanBotReach(BestBot, container.transform.position)) return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;

        }
    }
}
