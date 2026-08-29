using Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionPlaceMine : IPressAction
    {
        private static readonly List<string> MineArchetypes = new() { "C-Foam Tripmine", "Explosive Trip Mine" };
        public string FriendlyName => "Place Mine";
        public string FriendlyIdentifier => "Place Mine";
        private string _FriendlyNameShort = "Mine";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            if (BestBot == null) return false;
            Pose minePose = new Pose(zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos, Quaternion.LookRotation(-zStaticRefrences.LocalPlayer.FPSCamera.CameraRayNormal));
            zBotActions.SendBotToPlaceMine(BestBot, minePose, InventorySlot.Consumable, zStaticRefrences.LocalPlayer, 0);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PUTATRIPMINEHERE);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Put a mine here.", 1);
            zChatHandler.sendChatMessage("Placing trip mine.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }

        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!IsConsumableMine(BestBot)) return false;
            if (!IsPlacementValid(BestBot)) return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;
        }

        private static bool IsConsumableMine(PlayerAIBot BestBot)
        {
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(BestBot.Agent.Owner);
            if (backpack == null) return false;
            if (backpack.AmmoStorage.ConsumableAmmo.BulletsInPack <= 0) return false;
            BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.Consumable);
            if (item == null || item.Instance == null) return false;
            if (item.ItemID == 139U) return true;
            return MineArchetypes.Contains(item.Instance.ArchetypeName);
        }

        private static bool IsPlacementValid(PlayerAIBot BestBot)
        {
            Transform PlayerCameraTransform = zStaticRefrences.CameraTransform;
            if (BestBot == null || PlayerCameraTransform == null)
                return false;
            Vector3 cameraPos = PlayerCameraTransform.position;
            Vector3 cameraForward = PlayerCameraTransform.forward;
            if (!Physics.Raycast(
                    cameraPos,
                    cameraForward,
                    out RaycastHit hit,
                    LayerManager.MASK_TRIPMINE_CAMERARAY))
            {
                return false;
            }
            if (Physics.Linecast(
                    cameraPos,
                    hit.point,
                    LayerManager.MASK_TRIPMINE_PLACEMENT_BLOCKERS))
            {
                return false;
            }
            if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 3f, 17))
                return false;
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(BestBot.Agent.GoodPosition, navHit.position, 17, path)) return false;
            if (path.status == NavMeshPathStatus.PathInvalid)
                return false;
            Vector3 lastCorner = path.corners[path.corners.Length - 1];
            bool positionValid = (lastCorner - navHit.position).sqrMagnitude < PlayerBotActionDeployTripMine.s_ApproachRadius * PlayerBotActionDeployTripMine.s_ApproachRadius * PlayerBotActionDeployTripMine.s_VerifyRadiusMul * PlayerBotActionDeployTripMine.s_VerifyRadiusMul;
            if (!positionValid) return false;
            return true;
        }
    }
}
