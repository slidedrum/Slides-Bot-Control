using BotControl.Patches;
using Player;
using UnityEngine;
namespace BotControl.SmartSelect.PressActions.HoldActions
{
    internal class pActionShareSelf : IPressAction
    {
        public string FriendlyName => "Share Self";
        public string FriendlyNameShort => "Share";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            PressActionManager.GetAction("Share Resource").Invoke(zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            // Candidate is null
            if (!zSmartSelect.MainSelection.AnySelectedBotsAlive())
                return false;
            bool LookingDown = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.down) < 15f;
            if (!LookingDown)
                return false;
            PlayerAgent LocalAgent = zStaticRefrences.LocalPlayer;
            if (!LocalAgent.Alive)
                return false;
            foreach (PlayerAIBot selectedBot in zSmartSelect.MainSelection.GetSelected<PlayerAIBot>())
            {
                uint resourcePackID = zHelpers.GetAgentBackpackItemId(selectedBot.Agent, InventorySlot.ResourcePack);
                bool needsResourceIhave = false;
                switch (resourcePackID)
                {
                    case (uint)ShareActionPatch.ResourceIDs.MediPack:
                        needsResourceIhave = LocalAgent.NeedHealth();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.AmmoPack:
                        needsResourceIhave = LocalAgent.NeedWeaponAmmo();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.ToolPack:
                        needsResourceIhave = LocalAgent.NeedToolAmmo();
                        break;
                    case (uint)ShareActionPatch.ResourceIDs.DisinfectPack:
                        needsResourceIhave = LocalAgent.NeedDisinfection();
                        break;
                }
                if (needsResourceIhave)
                    return true;
            }
            return false;
        }
    }
}