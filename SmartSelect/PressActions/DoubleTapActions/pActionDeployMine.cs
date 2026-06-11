using Player;
using UnityEngine;
using UnityEngine.AI;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionDeployMine : IPressAction
    {
        public string FriendlyName => "Deploy Mine";
        public string FriendlyNameShort => "Mine";
        public string FriendlyIdentifier => "Deploy Equipmenet";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent)
        { // This logic should not be done on client, send to host over network.  Maybe works now?

            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            Pose minePose = new Pose(zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos, Quaternion.LookRotation(-zStaticRefrences.LocalPlayer.FPSCamera.CameraRayNormal));
            zBotActions.SendBotToPlaceMine(BestBot, minePose, InventorySlot.GearClass, zStaticRefrences.LocalPlayer, 0);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PUTATRIPMINEHERE);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Put a mine here.", 1);
            ZiMain.BotBarkBack(BestBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 2f);
            zChatHandler.sendChatMessage("Will do.", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        
        public bool IsActionValid(Component candidate)
        {
            // candidate is null
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!Evaluate(BestBot)) return false;
            if (!IsMineValid(BestBot)) return false;
            return true;
        }
        private bool Evaluate(PlayerAIBot bot)
        {
            BackpackItem foundBackpackItem = null;
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(bot.Agent.Owner);
            if (backpack == null)
            {
                return false;
            }
            if (backpack.AmmoStorage.ConsumableAmmo.BulletsInPack > 0)
            {
                BackpackItem backpackItem = backpack.Slots[5];
                if (backpackItem != null && backpackItem.Instance != null && backpackItem.ItemID == 139U && backpackItem.Instance as ItemEquippable != null)
                {
                    foundBackpackItem = backpackItem;
                    return true;
                }
            }
            if (backpack.AmmoStorage.ClassAmmo.BulletsInPack > 0)
            {
                BackpackItem backpackItem2 = backpack.Slots[3];
                if (backpackItem2 != null && backpackItem2.Instance != null && backpackItem2.ItemID == 37U && zHelpers.IsOfType<ItemEquippable>(backpackItem2.Instance))
                {
                    foundBackpackItem = backpackItem2;
                    return true;
                }
            }
            return false;
        }
        private bool IsMineValid(PlayerAIBot BestBot)
        {
            PlayerBackpack Backpack = PlayerBackpackManager.GetBackpack(BestBot.Agent.Owner);
            if (Backpack == null) return false;
            if (Backpack.AmmoStorage.ClassAmmo.BulletsInPack <= 0) return false;
            BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.GearClass);
            if (item == null) return false;
            string Archetype = item.Instance.ArchetypeName;
            if (Archetype != "Mine deployer") // We only want to run this on the mine deployer, pickupable mines are different.
                return false;
            Transform PlayerCameraTransform = zStaticRefrences.CameraTransform;
            if (BestBot == null || PlayerCameraTransform == null)
                return false;
            Vector3 cameraPos = PlayerCameraTransform.position;
            Vector3 cameraForward = PlayerCameraTransform.forward;
            // 1) Raycast from camera along look direction to find a placement surface.
            if (!Physics.Raycast(
                    cameraPos,
                    cameraForward,
                    out RaycastHit hit,
                    LayerManager.MASK_TRIPMINE_CAMERARAY))
            {
                return false;
            }
            // 2) Ensure nothing blocks the line from camera to the hit point.
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
