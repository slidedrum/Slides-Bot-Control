using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionOpenContainer : IPressAction
    {
        public string FriendlyName => "Open Container";
        public string FriendlyNameShort => "Open";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakResourceContainer>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            LG_WeakResourceContainer container = BestComponent.TryCast<LG_WeakResourceContainer>();
            //TODO
            // This might require custom action framework before i can do this.
            return false;
        }

        public bool IsActionValid(Component candidate)
        {
            LG_WeakResourceContainer container = candidate.TryCast<LG_WeakResourceContainer>();
            if (container == null) return false;
            if (container.ISOpen) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!zHelpers.CanBotReach(BestBot, container.transform.position)) return false;
            return true;

        }
    }
}
