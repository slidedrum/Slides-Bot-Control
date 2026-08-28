using BotControl.CustomActions.CustomActions;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionRefillSentry :  IPressAction
    {
        public string FriendlyName => "Refill Sentry";
        private string _FriendlyNameShort = "Refill";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public string FriendlyIdentifier => "Share Resources";
        public Il2CppSystem.Type Type => Il2CppType.Of<SentryGunInstance>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            SentryGunInstance Sentry = BestComponent.TryCast<SentryGunInstance>();
            if (Sentry == null) return false;
            //PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            zBotActions.SendBotToRefillSentry(BestBot, Sentry, zStaticRefrences.LocalPlayer);
            zChatHandler.sendChatMessage("Refilling sentry.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            SentryGunInstance Sentry = candidate.TryCast<SentryGunInstance>();
            if (Sentry == null) 
                return false;
            if (!Sentry.NeedToolAmmo()) 
                return false;
            //PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) 
                return false;
            if (!BestBot.Agent.Alive)
                return false;
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(BestBot.Agent.Owner);
            if (!zHelpers.TryGetAgentBackpackItem(BestBot.Agent, InventorySlot.ResourcePack, out BackpackItem pack))
                return false;
            if (pack == null || pack.ItemID != 127u) // tool refill
                return false;
            if (backpack.AmmoStorage.ResourcePackAmmo.BulletsInPack <= 0)
                return false;
            if (!zHelpers.CanBotReach(BestBot, Sentry.transform.position)) 
                return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
