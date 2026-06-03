using LevelGeneration;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions
{
    public class pActionDestroyDoor : PressActionManager
    {
        public override string FriendlyName => "Destroy Door";
        public override string FriendlyNameShort => "Destroy";
        public override bool Invoke(Component BestComponent)
        {
            LG_WeakDoor Door = BestComponent.TryCast<LG_WeakDoor>();
            //TODO
            return false;
        }
    }
}
