using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionDeploySentry : IPressAction
    {
        public string FriendlyName => "Deploy Sentry";
        public string FriendlyNameShort => "Sentry";
        public string FriendlyIdentifier => "Deploy Equipmenet";
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent)
        { // This logic should not be done on client, send to host over network.  Maybe works now?

            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (!zHelpers.TryGetAgentBackpackItem(BestBot.Agent, InventorySlot.GearClass, out BackpackItem backpackItem)) // this new method might work as client?  Idk.
                return false;
            bool isSentry = backpackItem.Instance.ArchetypeName == "Sentry Gun";
            bool isDeployed = backpackItem.Status == eInventoryItemStatus.Deployed;
            if (!isSentry || isDeployed)
                return false;

            // TODO make it so that if the turret is deployed, pick it up.  then replace it in the new locaiton.

            //raycast from camera to find hit position and normal,
            //place sentry at hit position, oriented based on normal.
            Vector3 origin = zStaticRefrences.CameraTransform.position;
            Vector3 direction = zStaticRefrences.CameraTransform.forward;
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, 100f, LayerManager.MASK_SENTRYGUN_CAMERARAY_MOVERHELPER))
                return false;
            Vector3 placePosition = hit.point;
            Quaternion placeRotation = Quaternion.LookRotation(FlatForward(zStaticRefrences.CameraTransform));
            Pose sentryPose = new Pose(placePosition, placeRotation);
            if (!CanPlaceTurret(ref sentryPose))
                return false;
            zBotActions.SendBotToPlaceSentry(BestBot, sentryPose, zStaticRefrences.LocalPlayer);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PUTASENTRYGUNHERE);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Put a sentry here.", 1);
            ZiMain.BotBarkBack(BestBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 2f);
            zChatHandler.sendChatMessage("Placing sentry.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool CanPlaceTurret(ref Pose pose)
        {
            bool hasRayHit = false;
            pose.position = pose.position + Vector3.up * 0.3f;
            if (Physics.Raycast(pose.position, Vector3.down, out RaycastHit hit, 3f, LayerManager.MASK_SENTRYGUN_CAMERARAY_MOVERHELPER))
            {
                float angle = Vector3.Dot(hit.normal, Vector3.up);
                if (angle > 0.9f)
                {
                    hasRayHit = true;
                    pose.position = hit.point;
                }
                else if (angle > 0.7f && Physics.Raycast(pose.position, Vector3.down, out RaycastHit hit2, 3f, LayerManager.MASK_SENTRYGUN_CAMERARAY))
                {
                    hasRayHit = true;
                    pose.position = hit2.point;
                }
                else
                {
                    return false;
                }
            }
            Bounds localBounds = new Bounds();

            for (int i = 0; i < zStaticRefrences.SentryRaycastCorners.Length; i++)
            {
                Vector3 local = zStaticRefrences.SentryRaycastCorners[i].localPosition;
                localBounds.Encapsulate(local);
            }
            Vector3 halfExtents = localBounds.size * 0.5f;
            pose.position = pose.position + Vector3.up * 0.1f;
            Collider[] hits = Physics.OverlapBox(
                pose.position,
                halfExtents,
                pose.rotation,
                LayerManager.MASK_SENTRYGUN_CAMERARAY_MOVERHELPER
            );

            return hits.Length == 0;
        }
        public Vector3 FlatForward(Transform transform)
        {
            Vector3 dir = transform.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                return Vector3.forward;
            return dir.normalized;
        }
        public bool IsActionValid(Component candidate)
        {
            // candidate is null
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!Evaluate(BestBot.Agent)) return false;
            if (!IsSentryValid(BestBot)) return false;
            return true;
        }
        public bool Evaluate(PlayerAgent ownerAgent)
        {
            bool isDeployed = false;
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(ownerAgent.Owner);
            if (backpack != null)
            {
                BackpackItem backpackItem = backpack.Slots[3];
                if (backpackItem != null && backpackItem.Instance != null && backpackItem.ItemID == 97U)
                {
                    if (backpackItem.Status == eInventoryItemStatus.Deployed)
                    {
                        isDeployed = true;
                    }
                    return true;
                }
            }
            return false;
        }
        private bool IsSentryValid(PlayerAIBot BestBot)
        {
            ItemEquippable[] deployedItems = BestBot.GetDeployedItems().ToArray();
            if (deployedItems.Length != 0) return false;
            if (!zHelpers.TryGetAgentBackpackItem(BestBot.Agent, InventorySlot.GearClass, out BackpackItem item)) return false;
            if (item.Instance.ArchetypeName != "Sentry Gun") return false;
            Vector3 origin = zStaticRefrences.CameraTransform.position;
            Vector3 direction = zStaticRefrences.CameraTransform.forward;
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, 100f, LayerManager.MASK_SENTRYGUN_CAMERARAY_MOVERHELPER))
                return false;
            Vector3 placePosition = hit.point;
            if (!zHelpers.CanBotReach(BestBot, placePosition)) return false;
            Quaternion placeRotation = Quaternion.LookRotation(FlatForward(zStaticRefrences.CameraTransform));
            Pose sentryPose = new Pose(placePosition, placeRotation);
            if (!CanPlaceTurret(ref sentryPose)) return false;
            return true;
        }
    }
}
