using BotControl.Patches;
using Player;
using UnityEngine;
namespace BotControl.SmartSelect.PressActions.HoldActions
{
    internal class pActionShareSelf : IPressAction
    {
        public string FriendlyName => "Share Self";
        private string _FriendlyNameShort = "Share";
        public string FriendlyNameShort => $"<color=#{TargetColorHex}>:</color><color=#{ColorHex}>{_FriendlyNameShort}</color><color=#{TargetColorHex}>:</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        private Color TargetColor = new Color(1f, 1f, 1f, 0.25f);
        private string TargetColorHex => ColorUtility.ToHtmlStringRGB(TargetColor);
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Hold";
        public string FriendlyIdentifier => "Share Resources";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            PressActionManager.GetAction("Share Resource").Invoke(zStaticRefrences.LocalPlayer, BestBot);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            // Candidate is null
            //if (!zSmartSelect.MainSelection.AnySelectedBotsAlive())
            //    return false;
            bool LookingDown = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.down) < 15f;
            if (!LookingDown)
                return false;
            PlayerAgent LocalAgent = zStaticRefrences.LocalPlayer;
            if (!LocalAgent.Alive)
                return false;
            //foreach (PlayerAIBot selectedBot in zSmartSelect.MainSelection.GetSelected<PlayerAIBot>())
            //{
            uint resourcePackID = zHelpers.GetAgentBackpackItemId(BestBot.Agent, InventorySlot.ResourcePack);
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
            Color = BestBot.Agent.Owner.PlayerColor;
            TargetColor = LocalAgent.Owner.PlayerColor;
            return needsResourceIhave;
            //if (needsResourceIhave)
            //    return true;
            //}
            //return false;
        }
    }
}