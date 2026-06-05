using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionDoorClose : IPressAction
    {
        public string FriendlyName => "Close Door";
        public string FriendlyNameShort => "Close";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakDoor>();
        public string pressTypeIdentifier => "Tap";
        public bool Invoke(Component BestComponent)
        {
            LG_WeakDoor Door = BestComponent.TryCast<LG_WeakDoor>();
            //TODO
            // Make sure the bot always ends up on the players side of the door.
            return false;
        }
        public bool IsActionValid(Component candidate)
        {
            if (!zSmartSelect.MainSelection.AnySelectedBotsAlive())
                return false;
            LG_WeakDoor Door = candidate.TryCast<LG_WeakDoor>();
            if (!Door.InteractionAllowed)
                return false;
            if (Door.Gate.IsTraversable)
                return true;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (!BestBot.Agent.Alive) return false;
            if (!zHelpers.CanBotReach(BestBot, Door.transform.position)) return false;
            return true;
        }
    }
}
