using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions
{
    public class pActionRevive : IPressAction
    {
        public string FriendlyName => "Revive Agent";
        public string FriendlyNameShort => "Revive";
        public string FriendlyIdentifier => "Revive";
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAgent>();
        public string pressTypeIdentifier => "Hold";
        public int? Priority => 100;
        public bool Invoke(Component BestComponent)
        {
            PlayerAgent Agent = BestComponent.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            zBotActions.SendBotToReviveAgent(BestBot, Agent, zStaticRefrences.LocalPlayer, 0);
            zChatHandler.sendChatMessage("Will do.", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAgent Agent = candidate.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            if (Agent.Alive) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!zHelpers.CanBotReach(BestBot, Agent.Position)) return false;
            return true;
        }
    }
}
