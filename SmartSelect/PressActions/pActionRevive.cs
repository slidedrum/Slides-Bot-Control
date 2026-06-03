using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions
{
    public class pActionRevive : PressActionManager
    {
        public override string FriendlyName => "Revive";
        public override string FriendlyNameShort => "Revive";
        public override bool Invoke(Component BestComponent)
        {
            PlayerAgent Agent = BestComponent.TryCast<PlayerAgent>();
            //TODO
            return false;
        }
    }
}
