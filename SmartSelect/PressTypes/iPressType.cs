using BotControl.SmartSelect.PressActions;
using Il2CppInterop.Runtime;
using Player;
using PrioritySet;
using SlideDrum.sInputSystem;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressTypes
{
    public interface IPressType
    {
        // ── Current State ─────────────────────────────────────────────────────────
        public abstract Component CurrentComponent { get; set; } // Holds the componenet that the action will be performed on if invoked right now.s
        public abstract IPressAction CurrentAction { get; set; } // Holds the current action that will be invoked if the press type is triggered right now.

        // ── Action Maps ───────────────────────────────────────────────────────────
        public abstract PrioritySet<IPressAction> NullTypeActions { get; set; } // Backing field for Type action map of key null.
        public abstract Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> TypeActionMap { get; set; } // What actions can be performed on each type?
        // todo why doesn't TypeActionMap get populated?
        // ── Identity / Configuration ──────────────────────────────────────────────
        public abstract string FriendlyName { get; } // Used for logging and configuration.
        public virtual string FriendlyNameShort => FriendlyName; // Used in the UI and hud.
        public virtual float SelectionAngle => 30f; // How wide of a angle is acceptable.  Remember that it does not search in a cone, it's a sphere arround the raycast point.
        public virtual fallbackType FallbackType => fallbackType.Default; // What to do if we don't find any valid actions from looking at selectable types.  See fallbackType enum for options.
        public abstract sSequenceDefinition PressSequence { get; } // When should this press be triggered?
        public abstract HashSet<Il2CppSystem.Type> SelectableTypes { get; } // What types should we be looking for when trying to select a component to perform actions on?  Must be a Component.  Can be empty if FallbackType is not default, but if it is default then it should have at least one type.

        // ── Methods / Enums ───────────────────────────────────────────────────────

        // TODO handle defintions that can look at ANYTHING. Not jus actions that look at nothing, 
        public enum fallbackType // Defines what to do if no selectable type is found
        {
            Default,     // Do nothing.
            //Nothing,     // Literally select the nothing
            PlayerAgent, // Select a player agent through walls if we can't find anything else.
            PlayerAiBot, // Select a player ai bot through walls if we can't find anything else.
        }
        public virtual void RegisterAction(IPressAction action, int? priority = null)
        {
            RegisterAction(action, action.Type, priority);
        } // For adding an action to this press type
        public virtual void RegisterAction(IPressAction action, Il2CppSystem.Type Type, int? priority = null)
        {
            PrioritySet<IPressAction> set = new();
            if (Type == null)
            {
                if (NullTypeActions == null)
                    NullTypeActions = new();
                set = NullTypeActions;
            }
            else
            {
                if (TypeActionMap == null)
                    TypeActionMap = new(new Il2CppTypePtrComparer());
                if (!TypeActionMap.TryGetValue(Type, out set) || set == null)
                {
                    set = new();
                    TypeActionMap[Type] = set;
                }
            }
            set.Add(action, priority);
        } // For adding an action to this press type
        public virtual bool Invoke() // Triggerd when the PressSequence triggers.
        {
            bool ret = false;
            if (CurrentAction != null && CurrentAction.IsActionValid(CurrentComponent))
                ret = CurrentAction.Invoke(CurrentComponent);
            if (ret)
                ZiMain.PlayUiSound(zSmartSelect.CorrectSound);
            else
                ZiMain.PlayUiSound(zSmartSelect.InvalidSound);
            return ret;
        }
        public virtual bool Update() // Triggered on slow update, responsible for updating the current action and component based on where the player is looking and what actions are valid.
        {
            CurrentAction = null;
            CurrentComponent = null;
            // first we find all of the candiates from selectable types.
            PrioritySet<Component> candidates = zSearch.FindAllInViewSorted(zStaticRefrences.CameraTransform, SelectableTypes, MaxAngle: SelectionAngle);
            Component candidate = null;
            for (int i = 0; i < candidates.Count; i++) // loop through them all in order of how close they are to the center of the screen.
            {
                candidate = candidates[i];
                Il2CppSystem.Type candidateType = candidate.GetIl2CppType();
                for (Il2CppSystem.Type type = candidateType; type != null; type = type.BaseType)
                {
                    foreach (Il2CppSystem.Type typeToMatch in SelectableTypes)
                    {
                        if (typeToMatch.Pointer == type.Pointer)
                        {
                            candidateType = type;
                            break;
                        }
                    }
                }

                if (!TypeActionMap.TryGetValue(candidateType, out var actionSet) || actionSet == null)
                    continue;
                foreach (IPressAction action in actionSet) // loop through all of the actions for that type
                {
                    if (action.IsActionValid(candidate))
                    {
                        CurrentAction = action; // if it's valid, then we're good we can set and stop.
                        CurrentComponent = candidate;
                        return true;
                    }
                }
            }
            foreach (IPressAction action in NullTypeActions)
            {
                if (action.IsActionValid(null))
                {
                    CurrentAction = action;
                    CurrentComponent = null;
                    return true;
                }
            }
            PrioritySet<IPressAction> set = new(); // if we didn't find anything we need to check the fallback type.
            switch (FallbackType)
            {
                case fallbackType.Default: // defaults to no fallback.
                    return false;
                //case fallbackType.Nothing: // fallback type nothing is for when we intentionally want to check if actions are valid when looking at nothing.
                //    if (NullTypeActions == null)
                //        return false;
                //    foreach (IPressAction action in NullTypeActions)
                //    {
                //        if (action.IsActionValid(null))
                //        {
                //            CurrentAction = action;
                //            CurrentComponent = null;
                //            return true;
                //        }
                //    }
                //    break;
                case fallbackType.PlayerAgent: // fallback type player agent is for when we want to look at player agents through walls.
                    if (!TypeActionMap.TryGetValue(Il2CppType.Of<PlayerAgent>(), out set) || set == null || set.Count == 0)
                        return false;
                    PlayerAgent Agent = zSmartSelect.GetPlayerAgentLookingAt();
                    if (Agent == null)
                        return false;
                    candidate = Agent.GetComponent<PlayerAgent>();
                    if (candidate == null)
                        return false;
                    break;
                case fallbackType.PlayerAiBot: // fallback type player ai bot is for when we want to look at bots through walls.
                    if (!TypeActionMap.TryGetValue(Il2CppType.Of<PlayerAIBot>(), out set) || set == null || set.Count == 0)
                        return false;
                    PlayerAIBot Bot = zSmartSelect.GetBotLookingAt();
                    if (Bot == null)
                        return false;
                    candidate = Bot.GetComponent<PlayerAIBot>();
                    if (candidate == null)
                        return false;
                    break;
            }
            foreach (IPressAction action in set)  // loop through actions in the set 
            {
                if (action.IsActionValid(candidate)) // TODO loop through all bots in order of closeness and see if any others are valid.  Should't be relvent for actiosn I have now, but would be good to check.
                {
                    CurrentAction = action;
                    CurrentComponent = candidate;
                    return true;
                }
            }

            return false;
        }
        public virtual void OnRegister() { }// this is for if the press type needs to do anything when it's registered, like add default actions or something idk.
    }
}
