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
        public string FriendlyNameShort => "Attack";
        public string FriendlyIdentifier => "Attack";
        public Il2CppSystem.Type Type => Il2CppType.Of<EnemyAgent>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            EnemyAgent Enemy = BestComponent.TryCast<EnemyAgent>();
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (Enemy == null || BestBot == null) return false;
            if (BestBot == null) return false;
            if (BestBot.Agent.Alive == false) return false;
            zBotActions.SendBotToAttackSleeper(BestBot, Enemy, zStaticRefrences.LocalPlayer);
            zChatHandler.sendChatMessage("Attacking sleeper.", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            if (DramaManager.CurrentStateEnum != DRAMA_State.Exploration && DramaManager.CurrentStateEnum != DRAMA_State.Sneaking)
                return false;
            EnemyAgent Enemy = candidate.TryCast<EnemyAgent>();
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (Enemy == null || BestBot == null) 
                return false;
            if (!BestBot.Agent.Alive) 
                return false;
            if (!Enemy.Alive) 
                return false;
            if (!zHelpers.CanBotReach(BestBot, Enemy.transform.position)) 
                return false;
            return true;
        }
    }
}
