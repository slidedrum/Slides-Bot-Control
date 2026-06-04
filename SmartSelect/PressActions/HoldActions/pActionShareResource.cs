using BotControl.Patches;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionShareResource : IPressAction
    {
        public string FriendlyName => "Share Resource";
        public string FriendlyNameShort => "Share";
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAgent>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            bool sucsess = false;
            PlayerAgent Agent = BestComponent.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            float offset = 0;
            var selection = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>();
            foreach (PlayerAIBot selectedBot in selection)
            {
                uint resourcePackID = zHelpers.GetAgentBackpackItemId(selectedBot.Agent, InventorySlot.ResourcePack);
                bool needsResourceIhave = false;
                switch (resourcePackID)
                {
                    case (uint)ShareActionPatch.ResourceIDs.MediPack:
                        needsResourceIhave = Agent.NeedHealth();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.AmmoPack:
                        needsResourceIhave = Agent.NeedWeaponAmmo();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.ToolPack:
                        needsResourceIhave = Agent.NeedToolAmmo();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.DisinfectPack:
                        needsResourceIhave = Agent.NeedDisinfection();
                        break;
                }
                if (!needsResourceIhave)
                    continue;
                if (!zHelpers.CanBotReach(selectedBot, Agent.Position))
                    continue;
                sucsess = true;
                zBotActions.SendBotToShareResourcePack(selectedBot, Agent, zStaticRefrences.LocalPlayer);
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PLEASE);
                zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Please", 1);
                ZiMain.BotBarkBack(selectedBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f + offset);
                offset += 0.25f;
            }
            return sucsess;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAgent Agent = candidate.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            var selection = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>();
            foreach (PlayerAIBot selectedBot in selection)
            {
                uint resourcePackID = zHelpers.GetAgentBackpackItemId(selectedBot.Agent, InventorySlot.ResourcePack);
                bool needsResourceIhave = false;
                switch (resourcePackID)
                {
                    case (uint)ShareActionPatch.ResourceIDs.MediPack:
                        needsResourceIhave = Agent.NeedHealth();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.AmmoPack:
                        needsResourceIhave = Agent.NeedWeaponAmmo();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.ToolPack:
                        needsResourceIhave = Agent.NeedToolAmmo();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.DisinfectPack:
                        needsResourceIhave = Agent.NeedDisinfection();
                        break;
                }
                if (!needsResourceIhave)
                    continue;
                if (!zHelpers.CanBotReach(selectedBot, Agent.Position))
                    continue;
                return true;
            }
            return false;
        }
    }
}
