using BotControl.SmartSelect.PressActions;
using PrioritySet;
using SlideDrum.sInputSystem;
using System.Collections.Generic;
using UnityEngine;
using static BotControl.SmartSelect.PressTypes.IPressType;

namespace BotControl.SmartSelect.PressTypes
{
    internal abstract class PressTypeExample : IPressType
    {
        // ── Current State ─────────────────────────────────────────────────────────
        public Component CurrentComponent { get => _CurrentComponent; set { _CurrentComponent = value; } }
        public IPressAction CurrentAction { get => _CurrentAction; set { _CurrentAction = value; } }

        // ── Action Maps ───────────────────────────────────────────────────────────
        public PrioritySet<IPressAction> NullTypeActions { get { return _NullTypeActions; } set { _NullTypeActions = value; } }
        public Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> TypeActionMap { get { return _TypeActionMap; } set { _TypeActionMap = value; } }

        // ── Identity / Configuration ──────────────────────────────────────────────
        public string FriendlyName => "EXAMPLE PRESS"; // MUST BE CAHNGED
        //public string FriendlyNameShort => "Exmpl"; // MUST BE CAHNGED
        public fallbackType FallbackType => fallbackType.Default; // Can be changed.
        public sSequenceDefinition PressSequence // MUST BE CHANGED
        {
            get
            {
                if (_PressSequences == null)
                {
                    _PressSequences = null; // MUST BE CHANGED
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
                    _SelectableTypes = new HashSet<Il2CppSystem.Type>();
                    //_SelectableTypes.Add(Il2CppType.Of<PlayerAIBot>()); // Example
                    // MUST BE ADDED TO
                }
                return _SelectableTypes;
            }
        }

        // ── Private Backing Fields ────────────────────────────────────────────────
        private Component _CurrentComponent = null;
        private IPressAction _CurrentAction = null;
        private sSequenceDefinition _PressSequences = null;
        private PrioritySet<IPressAction> _NullTypeActions = new();
        private Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> _TypeActionMap = new(new Il2CppTypePtrComparer());
        private HashSet<Il2CppSystem.Type> _SelectableTypes = null;
    }
}
