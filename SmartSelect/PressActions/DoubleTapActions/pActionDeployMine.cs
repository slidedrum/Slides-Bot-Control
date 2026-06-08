using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionDeployMine : IPressAction
    {
        public string FriendlyName => "Deploy Mine";
        public string FriendlyNameShort => "Mine";
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
            return true;
        }
        
        public bool IsActionValid(Component candidate)
        {
            // candidate is null
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!PlayerBotActionDeployTripMine.Descriptor.Evaluate(BestBot, out BackpackItem _)) return false;
            if (!IsMineValid(BestBot)) return false;
            return true;
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
            // 3) Bot must be able to path to the install point.
            if (!zHelpers.CanBotReach(BestBot, hit.point, 3))
                return false;
            return true;
        }
    }
}
