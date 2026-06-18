using Player;
using UnityEngine;
using System;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionDropNow : IPressAction
    {
        public string FriendlyName => "Drop Now";
        public string FriendlyNameShort => "Drop";
        public string FriendlyIdentifier => "Drop Objective";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Double Tap";
        public int? Priority => 10;
        public bool Invoke(Component BestComponent)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.InLevelCarry);
            if (item == null) return false;
            //PlayerBackpackManager.WantToDropItem(BestBot.Agent.Owner, item.Instance.Get_pItemData(), BestBot.Agent.Position, BestBot.Agent.Rotation, true);
            //var Desc = zBotActions.TryGetDescriptor<PlayerBotActionCarryExpeditionItem.Descriptor>(BestBot);
            //BestBot.StopAction(Desc);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CANCELTHAT);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Cancel that.", 1f);
            zStaticRefrences.CommsMenu.OnButtonPressedDrop(null, BestBot.Agent);
            zChatHandler.sendChatMessage("Dropping item.", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.InLevelCarry) == null) return false;
            return true;
        }
    }
}
