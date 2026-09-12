using Enemies;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapAndHoldActions
{
    public class pActionAllTarget : IPressAction
    {
        public string FriendlyName => "All Target";
        private string _FriendlyNameShort = "All-Tgt";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public string FriendlyIdentifier => "Attack";
        public Il2CppSystem.Type Type => Il2CppType.Of<EnemyAgent>();
        public string pressTypeIdentifier => "Tap and Hold";
        public int? Priority => 15;

        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            EnemyAgent Enemy = BestComponent.TryCast<EnemyAgent>();
            if (Enemy == null) return false;
            bool any = false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_HURRY);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Hurry.", 1f);
            foreach (PlayerAIBot bot in ZiMain.GetBotList())
            {
                if (bot == null || !bot.Agent.Alive)
                    continue;
                if (!zHelpers.CanBotReach(bot, Enemy.transform.position))
                    continue;
                var AttackAction = bot.m_rootAction.ActionBase.TryCast<RootPlayerBotAction>()?.m_attackAction;
                if (AttackAction == null)
                    continue;
                AttackAction.TargetAgent = Enemy;
                if (AttackAction.IsTerminated())
                {
                    bot.StartAction(AttackAction);
                    zChatHandler.sendChatMessage("Attacking target.", FriendlyIdentifier + IPressAction.chatPermSuffix, bot.Agent, zStaticRefrences.LocalPlayer);
                }
                else
                {
                    zChatHandler.sendChatMessage("Switching target.", FriendlyIdentifier + IPressAction.chatPermSuffix, bot.Agent, zStaticRefrences.LocalPlayer);
                }
                any = true;
            }
            return any;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            EnemyAgent Enemy = candidate.TryCast<EnemyAgent>();
            if (Enemy == null || BestBot == null)
                return false;
            if (!BestBot.Agent.Alive)
                return false;
            if (!Enemy.Alive)
                return false;
            if (!zHelpers.CanBotReach(BestBot, Enemy.transform.position))
                return false;
            if (Enemy.AI.IsHibernating(out bool isDisturbed, out bool isWakingUp) && !isWakingUp)
                return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
