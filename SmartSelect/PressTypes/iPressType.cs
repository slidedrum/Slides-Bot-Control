using BotControl.CustomActions;
using BotControl.SmartSelect.PressActions;
using Il2CppInterop.Runtime;
using Player;
using PrioritySet;
using SlideDrum.sInputSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BotControl.SmartSelect.PressTypes
{
    public interface IPressType
    {
        // ── Current State ─────────────────────────────────────────────────────────
        public abstract Component CurrentComponent { get; set; } // Holds the componenet that the action will be performed on if invoked right now.s
        public abstract PlayerAIBot CurrentBot { get; set; } // Holds the current bot that will perform the action
        public abstract IPressAction CurrentAction { get; set; } // Holds the current action that will be invoked if the press type is triggered right now.

        // ── Action Maps ───────────────────────────────────────────────────────────
        public abstract PrioritySet<IPressAction> NullTypeActions { get; set; } // Backing field for Type action map of key null.
        public abstract Dictionary<Il2CppSystem.Type, PrioritySet<IPressAction>> TypeActionMap { get; set; } // What actions can be performed on each type?

        // ── Identity / Configuration ──────────────────────────────────────────────
        public abstract string FriendlyName { get; } // Used for logging and configuration.
        public virtual string FriendlyNameShort => FriendlyName; // Used in the UI and hud.
        public virtual float SelectionAngle => 30f; // How wide of a angle is acceptable.  Remember that it does not search in a cone, it's a sphere arround the raycast point.
        public virtual fallbackType FallbackType => fallbackType.Default; // What to do if we don't find any valid actions from looking at selectable types.  See fallbackType enum for options.
        public abstract sSequenceDefinition PressSequence { get; } // When should this press be triggered?
        public HashSet<Il2CppSystem.Type> SelectableTypes { get; } // What types should we be looking for when trying to select a component to perform actions on?  Must be a Component.  Can be empty if FallbackType is not default, but if it is default then it should have at least one type.

        // ── Methods / Enums ───────────────────────────────────────────────────────

        // TODO handle defintions that can look at ANYTHING. Not jus actions that look at nothing, 
        public enum fallbackType // Defines what to do if no selectable type is found
        {
            Default,     // Do nothing.
            //Nothing,   // Literally select the nothing
            PlayerAgent, // Select a player agent through walls if we can't find anything else.
            PlayerAiBot, // Select a player ai bot through walls if we can't find anything else.
        }
        public virtual PrioritySet<IPressAction> GetAllActions()
        {
            if (TypeActionMap.Count == 0)
                PressActionManager.Initialize();
            PrioritySet<IPressAction> combined = new PrioritySet<IPressAction>();
            foreach (PrioritySet<IPressAction> list in TypeActionMap.Values)
            {
                combined.UnionWith(list);
            }
            combined.UnionWith(NullTypeActions);
            return combined;
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
            if (action.Type != null)
                SelectableTypes.Add(action.Type);
            set.Add(action, priority);
        } // For adding an action to this press type
        public virtual bool Invoke() // Triggerd when the PressSequence triggers.
        {
            bool ret = false;
            if (CurrentAction != null && CurrentAction.IsActionValid(CurrentComponent, CurrentBot))
                ret = CurrentAction.Invoke(CurrentComponent, CurrentBot);
            if (ret)
                ZiMain.PlayUiSound(zSmartSelect.CorrectSound);
            else
                ZiMain.PlayUiSound(zSmartSelect.InvalidSound);
            return ret;
        }
        public virtual bool Update() // Triggered on slow update, responsible for updating the current action and component based on where the player is looking and what actions are valid.
        {
            CurrentAction = null;
            var FirstSelectedBot = zSmartSelect.MainSelection.GetFirstSelectedBot();
            CurrentBot = FirstSelectedBot;
            List<PlayerAIBot> BotList = ZiMain.GetBotList().OrderBy(b => (b.transform.position - zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos).sqrMagnitude).ToList();
            if (zSmartSelect.MainSelection.AnyBotsSelected())
            {
                BotList.Remove(FirstSelectedBot);
                BotList.Insert(0, FirstSelectedBot);
            }
            CurrentComponent = null;
            // first we find all of the candiates from selectable types.
            PrioritySet<Component> candidates = zSearch.FindAllInViewSorted(zStaticRefrences.CameraTransform, SelectableTypes, MaxAngle: SelectionAngle);
            List<PlayerAIBot> SecondaryBotList = new();
            foreach (PlayerAIBot Bot in BotList)
            {
                if (zActions.DoingAnyManualAction(Bot.Agent) && (!zSmartSelect.MainSelection.AnyBotsSelected() || Bot.Pointer != FirstSelectedBot?.Pointer))
                {
                    SecondaryBotList.Add(Bot);
                    continue;
                }
                if (TryFindAction(candidates, Bot))
                {
                    CurrentBot = Bot;
                    return true;
                }
            }
            foreach (PlayerAIBot Bot in SecondaryBotList)
            {
                if (TryFindAction(candidates, Bot))
                {
                    CurrentBot = Bot;
                    return true;
                }
            }
            return false;
        }
        private bool TryFindAction(PrioritySet<Component> candidates, PlayerAIBot Bot)
        {
            Component candidate = null;
            for (int i = 0; i < candidates.Count; i++) // loop through them all in order of how close they are to the center of the screen.
            {
                candidate = candidates[i];
                Il2CppSystem.Type candidateType = candidate.GetIl2CppType();
                for (Il2CppSystem.Type type = candidateType; type != null; type = type.BaseType) // Also check against parrent types
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
                foreach (IPressAction action in actionSet) // loop through all of the actions for that type with the selected bot
                {
                    if (action.IsActionValid(candidate, Bot))
                    {
                        CurrentAction = action; // if it's valid, then we're good we can set and stop.
                        CurrentComponent = candidate;
                        return true;
                    }
                }
            }
            foreach (IPressAction action in NullTypeActions)
            {
                if (action.IsActionValid(null, Bot))
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
                    PlayerAIBot BotLookingAt = zSmartSelect.GetBotLookingAt();
                    if (BotLookingAt == null)
                        return false;
                    candidate = BotLookingAt.GetComponent<PlayerAIBot>();
                    if (candidate == null)
                        return false;
                    break;
            }
            foreach (IPressAction action in set)  // loop through actions in the set 
            {
                if (action.IsActionValid(candidate, Bot))
                {
                    CurrentAction = action;
                    CurrentComponent = candidate;
                    return true;
                }
            }
            // If we didn't find 
            return false;
        }
        public virtual void OnRegister() { }// this is for if the press type needs to do anything when it's registered, like add default actions or something idk.
    }
}
