using BotControl.SmartSelect.PressActions;
using Il2CppSystem;
using Player;
using PrioritySet;
using SlideDrum.sInputSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BotControl.SmartSelect.PressTypes.IPressType;

namespace BotControl.SmartSelect.PressTypes
{
    public class pTypeTapAndHoldPress : IPressType
    {
        // ── Identity / Configuration ──────────────────────────────────────────────
        public string FriendlyName => "Tap and Hold";
        public string FriendlyNameShort => "T-Hold";
        //public fallbackType FallbackType => fallbackType.Nothing;
        public sSequenceDefinition PressSequence
        {
            get
            {
                if (_PressSequences == null)
                {
                    _PressSequences = sInputSystemDefaults.OnHoldImmediateExclusive;
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
                    _SelectableTypes = new HashSet<Il2CppSystem.Type>();
                    //_SelectableTypes.Add(Il2CppType.Of<PlayerAIBot>());
                    //_SelectableTypes.Add(Il2CppType.Of<SentryGunInstance>());
                    //_SelectableTypes.Add(Il2CppType.Of<LG_WeakDoor>());
                }
                return _SelectableTypes;
            }
        }

        // ── Current State ─────────────────────────────────────────────────────────
        public Component CurrentComponent { get => _CurrentComponent; set { _CurrentComponent = value; } }
        public IPressAction CurrentAction { get => _CurrentAction; set { _CurrentAction = value; } }

        // ── Action Maps ───────────────────────────────────────────────────────────
        public PrioritySet<IPressAction> NullTypeActions { get { return _NullTypeActions; } set { _NullTypeActions = value; } }
        public Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> TypeActionMap { get { return _TypeActionMap; } set { _TypeActionMap = value; } }

        public Type Type => throw new System.NotImplementedException();

        // ── Private Backing Fields ────────────────────────────────────────────────
        private Component _CurrentComponent = null;
        private IPressAction _CurrentAction = null;
        private sSequenceDefinition _PressSequences = null;
        private PrioritySet<IPressAction> _NullTypeActions = new();
        private Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> _TypeActionMap = new(new Il2CppTypePtrComparer());
        private HashSet<Il2CppSystem.Type> _SelectableTypes = null;

        //public override bool SetCurrentAction()
        //{
        //    PlayerAIBot BestBot = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>().FirstOrDefault();
        //    if (BestBot == null)
        //    {
        //        _CurrentAction = null;
        //        return false;
        //    }
        //    var destinationPosition = zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos;
        //    if (zHelpers.PositionIsValidForAgent(BestBot.Agent, ref destinationPosition))
        //    {
        //        _CurrentAction = PressActionManager.GetAction("Move To");
        //        return true;
        //    }
        //    _CurrentAction = null;
        //    return false;
        //}
    }
}
