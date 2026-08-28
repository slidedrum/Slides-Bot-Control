using BotControl.SmartSelect.PressActions;
using Player;
using PrioritySet;
using SlideDrum.sInputSystem;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressTypes
{
    public class pTypeHoldPress : IPressType
    {
        // ── Identity / Configuration ──────────────────────────────────────────────
        public string FriendlyName => "Hold";
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
        public HashSet<Il2CppSystem.Type> SelectableTypes // MUST BE CHANGED
        {
            get
            {
                if (_SelectableTypes == null)
                {
                    _SelectableTypes = new HashSet<Il2CppSystem.Type>(new Il2CppTypePtrComparer());
                    //_SelectableTypes.Add(Il2CppType.Of<PlayerAgent>());
                    //_SelectableTypes.Add(Il2CppType.Of<MineDeployerInstance>()); // I think this is conflicting with iteminlevel
                    //_SelectableTypes.Add(Il2CppType.Of<ItemInLevel>());
                    //_SelectableTypes.Add(Il2CppType.Of<SentryGunInstance>());
                    //_SelectableTypes.Add(Il2CppType.Of<LG_WeakResourceContainer>());
                    //_SelectableTypes.Add(Il2CppType.Of<LG_WeakDoor>());
                    //_SelectableTypes.Add(Il2CppType.Of<EnemyAgent>());
                    //_SelectableTypes.Add(Il2CppType.Of<LG_PowerGenerator_Core>());

                }
                return _SelectableTypes;
            }
        }

        // ── Current State ─────────────────────────────────────────────────────────
        public Component CurrentComponent { get => _CurrentComponent; set { _CurrentComponent = value; } }
        public PlayerAIBot CurrentBot { get => _CurrentBot; set { _CurrentBot = value; } }
        public IPressAction CurrentAction { get => _CurrentAction; set { _CurrentAction = value; } }

        // ── Action Maps ───────────────────────────────────────────────────────────
        public PrioritySet<IPressAction> NullTypeActions { get { return _NullTypeActions; } set { _NullTypeActions = value; } }
        public Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> TypeActionMap { get { return _TypeActionMap; } set { _TypeActionMap = value; } }

        // ── Private Backing Fields ────────────────────────────────────────────────
        private Component _CurrentComponent = null;
        private PlayerAIBot _CurrentBot = null;
        private IPressAction _CurrentAction = null;
        private sSequenceDefinition _PressSequences = null;
        private PrioritySet<IPressAction> _NullTypeActions = new();
        private Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> _TypeActionMap = new(new Il2CppTypePtrComparer());
        private HashSet<Il2CppSystem.Type> _SelectableTypes = null;
    }
}
