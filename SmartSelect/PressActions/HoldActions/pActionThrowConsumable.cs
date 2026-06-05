using Player;
using UnityEngine;
using System;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionThrowConsumable : IPressAction
    {
        public string FriendlyName => "Throw Consumable";
        public string FriendlyNameShort => "Throw";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            return false;
        }
        public bool IsActionValid(Component candidate)
        {
            return false;
        }
    }
}
