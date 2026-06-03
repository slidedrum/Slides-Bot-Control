using Il2CppInterop.Runtime;
using LevelGeneration;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionInsertCell : IPressAction
    {
        public string FriendlyName => "Insert Cell";
        public string FriendlyNameShort => "Insert";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_PowerGenerator_Core>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            LG_PowerGenerator_Core Generator = BestComponent.TryCast<LG_PowerGenerator_Core>();
            //TODO
            return false;
        }

        public bool IsActionValid(Component candidate)
        {
            return false; //todo
        }
    }
}
