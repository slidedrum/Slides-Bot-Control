using BotControl.CustomActions.CustomActions;
using Enemies;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionAttackEnemy : IPressAction
    {
        public string FriendlyName => "Attack Enemy";
        private string _FriendlyNameShort = "Attack";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public string FriendlyIdentifier => "Attack";
        public Il2CppSystem.Type Type => Il2CppType.Of<EnemyAgent>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            EnemyAgent Enemy = BestComponent.TryCast<EnemyAgent>();
            if (Enemy == null || BestBot == null) return false;
            if (BestBot == null) return false;
            if (BestBot.Agent.Alive == false) return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_HURRY);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Hurry.", 1f);
            if (Enemy.AI.IsHibernating(out bool isDisturbed, out bool isWakingUp) && !isWakingUp)
            {
                zBotActions.SendBotToStealthAttack(BestBot, Enemy, false, zStaticRefrences.LocalPlayer);
                zChatHandler.sendChatMessage("Attacking sleeper.", "Attack" + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            }
            else
            {
                var AttackAction = BestBot.m_rootAction.ActionBase.TryCast<RootPlayerBotAction>()?.m_attackAction;
                if (AttackAction == null)
                    return false;
                AttackAction.TargetAgent = Enemy;
                if (AttackAction.IsTerminated())
                {
                    BestBot.StartAction(AttackAction);
                    zChatHandler.sendChatMessage("Attacking target.", "Attack" + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
                }
                else 
                {
                    zChatHandler.sendChatMessage("Switching target.", "Attack" + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
                }
            }
            return true;
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
            //if (Enemy.EnemyData.EnemyType != eEnemyType.Standard)
            //    return false;
            if (!zHelpers.CanBotReach(BestBot, Enemy.transform.position)) 
                return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            if (Enemy.AI.IsHibernating(out bool isDisturbed, out bool isWakingUp) && !isWakingUp)
                _FriendlyNameShort = "Sneak-Att";
            else 
                _FriendlyNameShort = "Attack";
            return true;
        }
    }
}
