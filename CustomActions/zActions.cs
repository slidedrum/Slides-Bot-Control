using Player;
using PrioritySet;
using System;
using System.Collections.Generic;
using BotControl.Patches;
//using Zombified_Initiative;

namespace BotControl.CustomActions
{
    public class dataStore
    {
        public PrioritySet<CustomActionBase.Descriptor> customActionDescriptors = new();
        public PrioritySet<CustomActionBase> customActionBases = new();
        public PlayerBotActionBase.Descriptor bestAction = null;
        //public Il2CppSystem.Collections.Generic.List<PlayerBotActionBase> m_actions { get; set; } = new();
        //public Il2CppSystem.Collections.Generic.List<PlayerBotActionBase.Descriptor> m_queuedActions { get; set; } = new();
    }
    public class ManualAction
    {
        public PlayerBotActionBase.Descriptor ActionDescriptor;
        public PlayerAgent Commander;
        public PlayerAIBot Bot;
        public uint ID;
        public float StartTimestamp;
        private ManualAction() { }
        public ManualAction(PlayerBotActionBase.Descriptor ActionDescriptor, PlayerAgent Commander, PlayerAIBot Bot, float StartTimestamp, uint ID)
        {
            this.ActionDescriptor = ActionDescriptor;
            this.Commander = Commander;
            this.Bot = Bot;
            this.ID = ID;
            this.StartTimestamp = StartTimestamp;
        }
    }
    public static class zActions
    {
        
        internal static readonly Dictionary<IntPtr, dataStore> ActionDataStore = new();
        private static Dictionary<IntPtr, List<ManualAction>> manualActions = new();
        public static List<ManualAction> GetPlayersManualActions(PlayerAgent playerAgent)
        {
            if (playerAgent == null)
                return null;
            return GetPlayersManualActions(playerAgent.Pointer);
        }
        public static List<ManualAction> GetPlayersManualActions(IntPtr playerPointer)
        {
            DropStaleCommanders();
            foreach (PlayerAgent playerAgent in PlayerManager.PlayerAgentsInLevel)
            {
                if (playerAgent == null)
                    continue;
                if (playerAgent.Pointer != playerPointer)
                    continue;
                if (!manualActions.TryGetValue(playerPointer, out var actions) || actions == null)
                {
                    actions = new List<ManualAction>();
                    manualActions[playerPointer] = actions;
                }
                return actions;
            }
            manualActions.Remove(playerPointer);
            return null;
        }
        private static void DropStaleCommanders()
        {
            var livePointers = new HashSet<IntPtr>();
            foreach (PlayerAgent agent in PlayerManager.PlayerAgentsInLevel)
            {
                if (agent != null)
                    livePointers.Add(agent.Pointer);
            }
            var live = new Dictionary<IntPtr, List<ManualAction>>();
            foreach (var ptr in livePointers)
            {
                if (!manualActions.TryGetValue(ptr, out var list) || list == null)
                    continue;
                list.RemoveAll(a => a == null || a.Bot == null || a.Bot.Agent == null || !livePointers.Contains(a.Bot.Agent.Pointer));
                if (list.Count > 0)
                    live[ptr] = list;
            }
            manualActions = live;
        }
        internal static dataStore GetOrCreateData(PlayerBotActionBase.Descriptor desc)
        {
            PlayerAIBot bot = desc.Bot;
            return GetOrCreateData(bot);
        }
        internal static dataStore GetOrCreateData(PlayerBotActionBase botBase)
        {
            PlayerAIBot bot = botBase.m_bot;
            return GetOrCreateData(bot);
        }
        internal static dataStore GetOrCreateData(PlayerAIBot botBase)
        {
            IntPtr botPtr = botBase.Pointer;
            if (!ActionDataStore.TryGetValue(botPtr, out var data))
            {
                data = new dataStore();
                ActionDataStore[botPtr] = data;
            }
            return data;
        }
        internal static void DropStaleActionData(HashSet<IntPtr> liveBots)
        {
            DropKeysNotIn(ActionDataStore, liveBots);
        }
        internal static void DropBotMaps(PlayerAIBot bot)
        {
            if (bot == null)
                return;
            ActionDataStore.Remove(bot.Pointer);
            zActionSub.botActionMap.Remove(bot.Pointer);
            TravelActionPatch.DropAgentData(bot.Pointer);
            var root = bot.m_rootAction?.ActionBase?.TryCast<RootPlayerBotAction>();
            if (root?.m_attackAction != null)
                AttackActionPatch.AllowedGuns.Remove(root.m_attackAction.Pointer);
        }
        internal static void PruneBotMaps(List<PlayerAIBot> liveBots)
        {
            var live = new HashSet<IntPtr>();
            var liveAttack = new HashSet<IntPtr>();
            if (liveBots != null)
            {
                foreach (var bot in liveBots)
                {
                    if (bot == null)
                        continue;
                    live.Add(bot.Pointer);
                    var root = bot.m_rootAction?.ActionBase?.TryCast<RootPlayerBotAction>();
                    if (root?.m_attackAction != null)
                        liveAttack.Add(root.m_attackAction.Pointer);
                }
            }
            DropStaleActionData(live);
            zActionSub.DropStaleBotActionMap(live);
            TravelActionPatch.DropStaleAgentData(live);
            AttackActionPatch.DropStaleAllowedGuns(liveAttack);
        }
        internal static void DropKeysNotIn<T>(Dictionary<IntPtr, T> dict, HashSet<IntPtr> live)
        {
            List<IntPtr> dead = null;
            foreach (var key in dict.Keys)
            {
                if (live.Contains(key))
                    continue;
                dead ??= new List<IntPtr>();
                dead.Add(key);
            }
            if (dead == null)
                return;
            foreach (var key in dead)
                dict.Remove(key);
        }
        public static bool AnyCustomActionRunning(PlayerAIBot Bot)
        {
            var data = zActions.GetOrCreateData(Bot);
            foreach (var act in data.customActionDescriptors)
                if (!act.IsTerminated())
                    return true;
            return false;
        }
        public static bool DoingAnyManualAction(PlayerAgent bot)
        {
            DropStaleCommanders();
            if (manualActions == null) return false;
            foreach(List<ManualAction> actions in manualActions.Values)
            {
                foreach( ManualAction action in actions)
                {
                    if (action?.Bot?.Agent?.Pointer == bot.Pointer && !(action.ActionDescriptor != null && action.ActionDescriptor.IsTerminated()))
                        return true;
                }
            }
            return false;
        }
        public static PlayerAgent isManualAction(PlayerBotActionBase.Descriptor descriptor)
        {
            DropStaleCommanders();
            if (descriptor == null) return null;
            if (manualActions == null) return null;
            foreach (var key in manualActions.Keys)
                foreach (ManualAction Action in manualActions[key])
                {
                    var desc = Action.ActionDescriptor;
                    if (desc == null) continue;

                    if (desc.Pointer == descriptor.Pointer)
                        return Action.Commander;
                }

            if (descriptor.ParentActionBase != null)
            {
                return isManualAction(descriptor.ParentActionBase.DescBase);
            }

            return null;
        }
    }
}
