using Il2CppInterop.Runtime;
using LevelGeneration;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionPlaceItem : IPressAction
    {
        public string FriendlyName => "Place Item";
        public string FriendlyNameShort => "Place";
        public string FriendlyIdentifier => "Place Item";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakResourceContainer>();
        public string pressTypeIdentifier => "Double Tap";
        public bool Enabled => false;
        public bool Invoke(Component BestComponent)
        {
            LG_WeakResourceContainer Container = BestComponent.TryCast<LG_WeakResourceContainer>();
            // TODO
            return false;
        }

        public bool IsActionValid(Component candidate)
        {
            return false;
        }
    }
}
