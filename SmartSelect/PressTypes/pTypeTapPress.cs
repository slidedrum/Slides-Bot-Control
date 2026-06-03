using BotControl.SmartSelect.PressActions;
using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using PrioritySet;
using SlideDrum.sInputSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BotControl.SmartSelect.PressTypes.IPressType;

namespace BotControl.SmartSelect.PressTypes
{
    public class pTypeTapPress : IPressType
    {
        // ── Current State ─────────────────────────────────────────────────────────
        public Component CurrentComponent { get => _CurrentComponent; set { _CurrentComponent = value; } }
        public IPressAction CurrentAction { get => _CurrentAction; set { _CurrentAction = value; } }

        // ── Action Maps ───────────────────────────────────────────────────────────
        public PrioritySet<IPressAction> NullTypeActions { get { return _NullTypeActions; } set { _NullTypeActions = value; } }
        public Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> TypeActionMap { get { return _TypeActionMap; } set { _TypeActionMap = value; } }
        
        // ── Identity / Configuration ──────────────────────────────────────────────
        public string FriendlyName => "Tap";
        public fallbackType FallbackType => fallbackType.PlayerAiBot;
        public sSequenceDefinition PressSequence => _PressSequences;
        public HashSet<Il2CppSystem.Type> SelectableTypes
        {
            get
            {
                if (SelectableTypes == null)
                {
                    _SelectableTypes = new HashSet<Il2CppSystem.Type>();
                    _SelectableTypes.Add(Il2CppType.Of<PlayerAIBot>());
                    _SelectableTypes.Add(Il2CppType.Of<SentryGunInstance>());
                    _SelectableTypes.Add(Il2CppType.Of<LG_WeakDoor>());
                }
                return _SelectableTypes;
            }
        }

        // ── Private Backing Fields ────────────────────────────────────────────────
        private Component _CurrentComponent = null;
        private IPressAction _CurrentAction = null;
        private sSequenceDefinition _PressSequences = sInputSystemDefaults.OnTappedExclusive;
        private PrioritySet<IPressAction> _NullTypeActions = new();
        private Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> _TypeActionMap = new();
        private HashSet<Il2CppSystem.Type> _SelectableTypes = null;

        // this should be bound to:
        // Deselect all Bots (tap empty space)
        // Select Bot (tap bot)
        // Pickup Sentry (tap sentry)
        // Open/Close Door (tap door)
        [Obsolete("This method is no longer used.  The logic has been made generic and moved to the action types themselves with IPressType.Update() and IPressAction.IsActionValid()")]
        public bool SetCurrentAction()
        {
            Il2CppSystem.Type type = CurrentComponent?.GetIl2CppType();
            PlayerAIBot bot;
            if (type == null)
            {
                bool facingUp = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.up) < 15f;
                if (facingUp && zSmartSelect.MainSelection.Selected<PlayerAIBot>())
                {
                    //DeselectAllBots();
                    _CurrentAction = PressActionManager.GetAction("Deselect All Bots");
                    return true;
                }
            }
            else if (zHelpers.IsOfType<PlayerAIBot>(type))
            {
                _CurrentAction = PressActionManager.GetAction("Select Bot");
                return true;
            }
            else if (zHelpers.IsOfType<SentryGunInstance>(type))
            {
                SentryGunInstance sentry = CurrentComponent.Cast<SentryGunInstance>();
                bot = sentry?.Owner?.GetComponent<PlayerAIBot>();
                if (bot != null)
                {
                    _CurrentAction = PressActionManager.GetAction("Pickup Sentry");
                    return true;
                }
            }
            else if (zHelpers.IsOfType<LG_WeakDoor>(type))
            {
                LG_WeakDoor Door = CurrentComponent.Cast<LG_WeakDoor>();
                if (!zSmartSelect.MainSelection.GetSelected<PlayerAIBot>().FirstOrDefault())
                {
                    if (!Door.InteractionAllowed)
                    {
                        _CurrentAction = null;
                        return false;
                    }
                    if (Door.Gate.IsTraversable)
                    {
                        _CurrentAction = PressActionManager.GetAction("Close Door");
                    }
                    _CurrentAction = PressActionManager.GetAction("Open Door");
                    return true;
                }
            }
            bot = zSmartSelect.GetBotLookingAt();
            if (bot != null)
            {
                _CurrentAction = PressActionManager.GetAction("Select Bot");
                return true;
            }
            _CurrentAction = null;
            return false;

            // TODO the other action sets.  This one SHOULD be done.  but double check my work, I was very tired.
        }
    }
}
