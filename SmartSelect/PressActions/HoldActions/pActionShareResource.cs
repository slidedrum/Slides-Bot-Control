using BotControl.Patches;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionShareResource : IPressAction
    {
        public string FriendlyName => "Share Resource";
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAgent>();
        public string pressTypeIdentifier => "Hold";
        private string _FriendlyNameShort = "Share";
        public string FriendlyIdentifier => "Share Resources";
        public string FriendlyNameShort => $"<color=#{TargetColorHex}>:</color><color=#{ColorHex}>{_FriendlyNameShort}</color><color=#{TargetColorHex}>:</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        private Color TargetColor = new Color(1f, 1f, 1f, 0.25f);
        private string TargetColorHex => ColorUtility.ToHtmlStringRGB(TargetColor);
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            bool Success = false;
            PlayerAgent Agent = BestComponent.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            float offset = 0;
            //var selection = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>();
            //foreach (PlayerAIBot selectedBot in selection)
            //{
                var item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.ResourcePack);
                uint resourcePackID = item.ItemID;
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
                    return false;
                if (!zHelpers.CanBotReach(BestBot, Agent.Position))
                    return false;
                Success = true;
                zBotActions.SendBotToShareResourcePack(BestBot, Agent, zStaticRefrences.LocalPlayer);
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PLEASE);
                zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Please", 1);
                ZiMain.BotBarkBack(BestBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f + offset);
                //if (Agent.Pointer != selectedBot.Agent.Pointer)
                //    zChatHandler.sendChatMessage($"Sharing my {item.Instance.ArchetypeName} with {Agent.PlayerName}.", FriendlyIdentifier + IPressAction.chatPermSuffix, selectedBot.Agent, zStaticRefrences.LocalPlayer);
                //else
                //{
                //    zChatHandler.sendChatMessage($"Using my {item.Instance.ArchetypeName}.", FriendlyIdentifier + IPressAction.chatPermSuffix, selectedBot.Agent, zStaticRefrences.LocalPlayer);
                //}
                offset += 0.25f;
            //}
            return Success;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            PlayerAgent Agent = candidate.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            var selection = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>();
            //foreach (PlayerAIBot selectedBot in selection)
            //{
                uint resourcePackID = zHelpers.GetAgentBackpackItemId(BestBot.Agent, InventorySlot.ResourcePack);
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
                    return false;
                if (!zHelpers.CanBotReach(BestBot, Agent.Position))
                    return false;
                Color = BestBot.Agent.Owner.PlayerColor;
                TargetColor = Agent.Owner.PlayerColor;
                return true;
            //}
            //return false;
        }
    }
}
