using Enemies;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionSyncAttack : IPressAction
    {
        public string FriendlyName => "Sync Attack";
        private string _FriendlyNameShort = "Sync";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<EnemyAgent>();
        public string pressTypeIdentifier => "Tap and Hold";
        public string FriendlyIdentifier => "Sync";
        public bool Enabled => true;

        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            EnemyAgent Enemy = BestComponent.TryCast<EnemyAgent>();
            if (Enemy == null || BestBot == null) return false;
            if (BestBot.Agent.Alive == false) return false;
            zBotActions.SendBotToStealthAttack(BestBot, Enemy, true, zStaticRefrences.LocalPlayer);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_HURRY);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Hurry.", 1f);
            zChatHandler.sendChatMessage("On the way.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            if (DramaManager.CurrentStateEnum != DRAMA_State.Exploration && DramaManager.CurrentStateEnum != DRAMA_State.Sneaking)
                return false;
            EnemyAgent Enemy = candidate.TryCast<EnemyAgent>();
            if (Enemy == null || BestBot == null)
                return false;
            if (!BestBot.Agent.Alive)
                return false;
            if (!Enemy.Alive)
                return false;
            if (!zHelpers.CanBotReach(BestBot, Enemy.transform.position))
                return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
