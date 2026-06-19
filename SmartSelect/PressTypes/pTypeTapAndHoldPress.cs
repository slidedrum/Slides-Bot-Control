using BotControl.SmartSelect.PressActions;
using PrioritySet;
using sInputSystem;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressTypes
{
    public class pTypeTapAndHoldPress : IInputType
    {
        // ── Identity / Configuration ──────────────────────────────────────────────
        public string FriendlyName => "Tap and Hold";
        public string FriendlyNameShort => "T-Hold";
        //public fallbackType FallbackType => fallbackType.Nothing;
        public sSequenceDefinition InputSequence
        {
            get
            {
                if (_PressSequences == null)
                {
                    _PressSequences = sInputSystemDefaults.OnTapAndHoldImmediateExclusive;
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
                    ////_SelectableTypes.Add(Il2CppType.Of<SentryGunInstance>());
                    ////_SelectableTypes.Add(Il2CppType.Of<LG_WeakDoor>());
                }
                return _SelectableTypes;
            }
        }

        // ── Current State ─────────────────────────────────────────────────────────
        public Component CurrentComponent { get => _CurrentComponent; set { _CurrentComponent = value; } }
        public IInputAction CurrentAction { get => _CurrentAction; set { _CurrentAction = value; } }

        // ── Action Maps ───────────────────────────────────────────────────────────
        public PrioritySet<IInputAction> NullTypeActions { get { return _NullTypeActions; } set { _NullTypeActions = value; } }
        public Dictionary<Il2CppSystem.Type, PrioritySet<IInputAction>> TypeActionMap { get { return _TypeActionMap; } set { _TypeActionMap = value; } }

        //public Type Type => throw new System.NotImplementedException();

        // ── Private Backing Fields ────────────────────────────────────────────────
        private Component _CurrentComponent = null;
        private IInputAction _CurrentAction = null;
        private sSequenceDefinition _PressSequences = null;
        private PrioritySet<IInputAction> _NullTypeActions = new();
        private Dictionary<Il2CppSystem.Type, PrioritySet<IInputAction>> _TypeActionMap = new(new Il2CppTypePtrComparer());
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
