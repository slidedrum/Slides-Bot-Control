using BotControl.CustomActions.CustomActions;
using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using TMPro;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionInsertCell : IPressAction
    {
        public string FriendlyName => "Insert Cell";
        public string FriendlyNameShort => "Insert";
        public string FriendlyIdentifier => "Insert";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_PowerGenerator_Core>();
        public string pressTypeIdentifier => "Hold";
        public bool Enabled => false;
        public int? Priority => 15;
        public bool Invoke(Component BestComponent)
        {
            LG_PowerGenerator_Core Generator = BestComponent.TryCast<LG_PowerGenerator_Core>();
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false; 
            if (BestBot.Agent.Alive == false) return false;
            CustomBotActionInsertPowerCell.Descriptor desc = new CustomBotActionInsertPowerCell.Descriptor(BestBot)
            {
                TargetGenerator = Generator,
                Prio = 13
            };
            zBotActions.StartAction(BestBot, desc, zStaticRefrences.LocalPlayer, 0); ;
            return false;
        }

        public bool IsActionValid(Component candidate)
        {
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (BestBot.Agent.Alive == false) return false;
            LG_PowerGenerator_Core Generator = candidate.TryCast<LG_PowerGenerator_Core>();
            if (Generator == null)
                return false;
            var m_powerCellInteractionObject = Generator.m_powerCellInteraction.Cast<LG_GenericCarryItemInteractionTarget>().gameObject;
            if (!m_powerCellInteractionObject.activeInHierarchy)
                return false;
            if (!zHelpers.TryGetAgentBackpackItem(BestBot.Agent, InventorySlot.InLevelCarry, out var item))
                return false;
            if (item.ItemID != 131) // Power Cell
                return false;
            var TargetPosition = GetTargetPosition(Generator);
            if (TargetPosition == null)
                return false;
            if (!zHelpers.CanBotReach(BestBot, (Vector3)TargetPosition))
                return false;
            return true; //todo
        }
        private Vector3? GetTargetPosition(LG_PowerGenerator_Core TargetGenerator)
        {
            GameObject gobject = TargetGenerator.gameObject;
            if (gobject == null)
                return null;
            Transform transform = gobject.transform;
            Vector3 location = transform.position + transform.forward * 1.5f;
            if (!zHelpers.SnapPositionToNav(location, out Vector3 TargetPosition))
                return null;
            return TargetPosition;
        }
    }
}
