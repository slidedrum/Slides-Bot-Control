using Il2CppInterop.Runtime;
using LevelGeneration;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionDestroyDoor : IPressAction
    {
        public string FriendlyName => "Destroy Door";
        public string FriendlyNameShort => "Destroy";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakDoor>();
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent)
        {
            LG_WeakDoor Door = BestComponent.TryCast<LG_WeakDoor>();
            //TODO
            return false;
        }
        public bool IsActionValid(Component candidate)
        {
            return false;
        }
    }
}
