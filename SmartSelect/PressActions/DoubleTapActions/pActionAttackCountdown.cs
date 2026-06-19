using Enemies;
using Il2CppInterop.Runtime;
using LevelGeneration;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionAttackCountdown : IInputAction
    {
        public string FriendlyName => "Attack Countdown";
        public string FriendlyNameShort => "Countdown";
        public Il2CppSystem.Type Type => Il2CppType.Of<EnemyAgent>();
        public string pressTypeIdentifier => "Double Tap";
        public string FriendlyIdentifier => "Attack";
        public bool Enabled => false;
        public bool Invoke(Component BestComponent)
        {
            EnemyAgent Enemy = BestComponent.TryCast<EnemyAgent>();
            //TODO
            // Make sure to handle sleepers and big enemy types.
            // Sleepers should have a chance of being woke up the longer they are moving.
            // Big enemies should be handled differently somehow.
            // Scouts should have a very low chance of working somehow.  idk how that'll work tho.
            return false;
        }

        public bool IsActionValid(Component candidate)
        {
            return false;
        }
    }
}
