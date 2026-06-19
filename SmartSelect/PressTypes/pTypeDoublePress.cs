using BotControl.SmartSelect.PressActions;
using PrioritySet;
using sInputSystem;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressTypes
{
    public class pTypeDoublePress : IInputType
    {
        // ── Current State ─────────────────────────────────────────────────────────
        public Component CurrentComponent { get => _CurrentComponent; set { _CurrentComponent = value; } }
        public IInputAction CurrentAction { get => _CurrentAction; set { _CurrentAction = value; } }

        // ── Action Maps ───────────────────────────────────────────────────────────
        public PrioritySet<IInputAction> NullTypeActions { get { return _NullTypeActions; } set { _NullTypeActions = value; } }
        public Dictionary<Il2CppSystem.Type, PrioritySet<IInputAction>> TypeActionMap { get { return _TypeActionMap; } set { _TypeActionMap = value; } }

        // ── Identity / Configuration ──────────────────────────────────────────────
        public string FriendlyName => "Double Tap";
        public string FriendlyNameShort => "D-Tap";
        //public fallbackType FallbackType => fallbackType.Nothing;
        public sSequenceDefinition InputSequence
        {
            get
            {
                if (_PressSequences == null)
                {
                    _PressSequences = sInputSystemDefaults.OnDoubleTappedExclusive;
                }
                return _PressSequences;
            }
        }
        public HashSet<Il2CppSystem.Type> SelectableTypes
        {
            get
            {
                if (_SelectableTypes == null)
                {
                    _SelectableTypes = new HashSet<Il2CppSystem.Type>(new Il2CppTypePtrComparer());
                    //_SelectableTypes.Add(Il2CppType.Of<PlayerAgent>());
                    //_SelectableTypes.Add(Il2CppType.Of<SentryGunInstance>());
                    //_SelectableTypes.Add(Il2CppType.Of<LG_WeakResourceContainer>());
                    //_SelectableTypes.Add(Il2CppType.Of<LG_WeakDoor>());
                    //_SelectableTypes.Add(Il2CppType.Of<EnemyAgent>());
                    //_SelectableTypes.Add(Il2CppType.Of<MineDeployerInstance>());
                }
                return _SelectableTypes;
            }
        }

        // ── Private Backing Fields ────────────────────────────────────────────────
        private Component _CurrentComponent = null;
        private IInputAction _CurrentAction = null;
        private sSequenceDefinition _PressSequences = null;
        private PrioritySet<IInputAction> _NullTypeActions = new();
        private Dictionary<Il2CppSystem.Type, PrioritySet<IInputAction>> _TypeActionMap = new(new Il2CppTypePtrComparer());
        private HashSet<Il2CppSystem.Type> _SelectableTypes = null;
        //public bool SetCurrentAction()
        //{
        //    PlayerAIBot BestBot = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>().FirstOrDefault();
        //    Il2CppSystem.Type type = _CurrentComponent?.GetIl2CppType();
        //    if (type == null)
        //    {
        //        if (BestBot == null)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.GearClass);
        //        bool isSentry = item.Instance.ArchetypeName == "Sentry Gun"; //TODO handle mine deployer
        //        if (!isSentry)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        bool isDeployed = item.Status == eInventoryItemStatus.Deployed;
        //        Vector3 origin = zStaticRefrences.CameraTransform.position;
        //        Vector3 direction = zStaticRefrences.CameraTransform.forward;
        //        if (!Physics.Raycast(origin, direction, out RaycastHit hit, 100f, LayerManager.MASK_SENTRYGUN_CAMERARAY_MOVERHELPER))
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        Vector3 placePosition = hit.point;
        //        Quaternion placeRotation = Quaternion.LookRotation(pActionPlaceSentry.FlatForward(zStaticRefrences.CameraTransform));
        //        Pose sentryPose = new Pose(placePosition, placeRotation);
        //        if (!pActionPlaceSentry.CanPlaceTurret(ref sentryPose))
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        if (isDeployed)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        _CurrentAction = PressActionManager.GetAction("Deploy Sentry");
        //        return true; // TODO fallback to Follow Me
        //    }
        //    else if (zHelpers.IsOfType<PlayerAgent>(type))
        //    {
        //        PlayerAgent Agent = _CurrentComponent.Cast<PlayerAgent>();
        //        _CurrentAction = PressActionManager.GetAction("Follow Me");
        //        return true;
        //    }
        //    else if (zHelpers.IsOfType<SentryGunInstance>(type))
        //    {
        //        SentryGunInstance Sentry = _CurrentComponent.Cast<SentryGunInstance>();
        //        _CurrentAction = PressActionManager.GetAction("Pickup All Sentries");
        //        return true;
        //    }
        //    else if (zHelpers.IsOfType<LG_WeakResourceContainer>(type))
        //    {
        //        LG_WeakResourceContainer Container = _CurrentComponent.Cast<LG_WeakResourceContainer>();
        //        if (BestBot == null)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        uint consumableId = zHelpers.GetAgentBackpackItemId(BestBot.Agent, InventorySlot.Consumable);
        //        uint resourceId = zHelpers.GetAgentBackpackItemId(BestBot.Agent, InventorySlot.ResourcePack);
        //        bool haveItem = (consumableId != 0 || resourceId != 0);
        //        if (!haveItem)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        _CurrentAction = PressActionManager.GetAction("Place Item In Container");
        //        return true;
        //    }
        //    else if (zHelpers.IsOfType<LG_WeakDoor>(type))
        //    {
        //        if (BestBot == null)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        LG_WeakDoor Door = _CurrentComponent.Cast<LG_WeakDoor>();
        //        if (Door.Gate.IsTraversable)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        if (Door.LastStatus == eDoorStatus.Destroyed)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        _CurrentAction = PressActionManager.GetAction("Destroy Door");
        //        return true;
        //    }
        //    else if (zHelpers.IsOfType<EnemyAgent>(type))
        //    {
        //        if (BestBot == null)
        //        {
        //            _CurrentAction = null;
        //            return false;
        //        }
        //        EnemyAgent Enemy = _CurrentComponent.Cast<EnemyAgent>();
        //        _CurrentAction = PressActionManager.GetAction("Attack Countdown Enemy");
        //        return true;
        //    }
        //    var bot = zSmartSelect.GetBotLookingAt();
        //    if (bot != null)
        //    {
        //        _CurrentAction = PressActionManager.GetAction("Follow Me");
        //        return true;
        //    }
        //    _CurrentAction = null;
        //    return false;
        //}
    }
}
