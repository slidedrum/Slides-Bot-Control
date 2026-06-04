using BotControl.SmartSelect.PressTypes;
using System;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions
{
    public interface IPressAction
    {
        public abstract string FriendlyName { get; } // Used for logs and confuguration.
        public virtual string FriendlyNameShort => FriendlyName; // Used for the hud.
        public abstract Il2CppSystem.Type Type { get; } // The type of component this action is meant to be used on.  Must be a UnityEngine.Componenet.  Or null if the pressType has a fallback type of "nothing"
        public virtual string? pressTypeIdentifier => null; // Could be null if this action handles it's own registration.  Might want to do that if you're regestering to multiple types, or something else funky.
        public virtual bool ActionImplementationComplete => true;
        public virtual void Register() // Used to register this action to its press type.  Will be called automatically if pressTypeIdentifier is null, the implementing class MUST overide this mehtod.
        {
            if (pressTypeIdentifier.ToLower().Contains("example"))
                return;
            if (pressTypeIdentifier == null)
                throw new Exception($"PressAction {FriendlyName} does not specify a press type. It must handle it's own registration.");
            if (Type != null && !zHelpers.IsOfType<Component>(Type))
                throw new Exception($"PressAction {FriendlyName} has a type that is not a Component.");
            var PressType = PressTypeManager.GetPressType(pressTypeIdentifier);
            if (PressType == null)
                throw new Exception($"PressAction {FriendlyName} tried to register to non existant press type {pressTypeIdentifier}");
            PressType.RegisterAction(this);
        }
        public abstract bool Invoke(Component BestComponenet); // Perform this action right now!  Assume that IsActionValid has already been run and is true. Do not re-run the check.
        public abstract bool IsActionValid(Component candidate); // Can this action be performed right now on this component?  Used for determining which action to select, and also for checking if the current action is still valid in the update loop.
    }
}
