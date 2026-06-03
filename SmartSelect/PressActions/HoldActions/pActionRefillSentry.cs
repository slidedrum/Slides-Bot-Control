using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionRefillSentry :  IPressAction
    {
        public string FriendlyName => "Refill Sentry";
        public string FriendlyNameShort => "Refill";
        public Il2CppSystem.Type Type => Il2CppType.Of<SentryGunInstance>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            SentryGunInstance Sentry = BestComponent.TryCast<SentryGunInstance>();
            if (Sentry == null) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            //TODO.  This might have to wait untill I get custom actions working again.
            zBotActions.SendBotToRefillSentry(BestBot, Sentry, zStaticRefrences.LocalPlayer, 0);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            SentryGunInstance Sentry = candidate.TryCast<SentryGunInstance>();
            if (Sentry == null) 
                return false;
            if (!Sentry.NeedToolAmmo()) 
                return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) 
                return false;
            if (!BestBot.Agent.Alive)
                return false;
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(BestBot.Agent.Owner);
            if (!backpack.TryGetBackpackItem(InventorySlot.ResourcePack, out BackpackItem pack))
                return false;
            if (pack == null || pack.ItemID != 127u) // tool refill
                return false;
            if (backpack.AmmoStorage.ResourcePackAmmo.BulletsInPack <= 0)
                return false;
            if (!zHelpers.CanBotReach(BestBot, Sentry.transform.position)) 
                return false;
            return true;
        }
    }
}
