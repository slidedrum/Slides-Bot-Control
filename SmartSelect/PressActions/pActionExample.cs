using Player;
using UnityEngine;
using System;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionExample : IPressAction
    {
        public string FriendlyName => "EXAMPLE ACTION"; // MUST BE CHANGED
        public string FriendlyNameShort => "Exmpl"; // Can be changed
        public Il2CppSystem.Type Type => null; // MUST BE CHANGED
        public string pressTypeIdentifier => "EXAMPLE PRESS"; //MUST BE CHANGED
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
