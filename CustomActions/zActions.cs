using Player;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.zRootBotPlayerAction
{
    public class dataStore
    {
        //public OrderedSet<CustomActionBase.Descriptor> customActions = new();
        public PlayerBotActionBase.Descriptor bestAction = null;
        //public bool consideringActions = false;
        //public PlayerAgent actualLeader = null;
        public Il2CppSystem.Collections.Generic.List<PlayerBotActionBase> m_actions { get; set; } = new();
        public Il2CppSystem.Collections.Generic.List<PlayerBotActionBase.Descriptor> m_queuedActions { get; set; } = new();
    }
    public class ManualAction
    {
        public PlayerBotActionBase.Descriptor ActionDescriptor;
        public PlayerAgent Commander;
        public PlayerAIBot Bot;
        public uint ID;
        private ManualAction() { }
        public ManualAction(PlayerBotActionBase.Descriptor ActionDescriptor, PlayerAgent Commander, PlayerAIBot Bot, uint ID)
        {
            this.ActionDescriptor = ActionDescriptor;
            this.Commander = Commander;
            this.Bot = Bot;
            //this.ID = zHelpers.HashString($"{ActionDescriptor.GetIl2CppType().FullName}{Commander.PlayerName}{Bot.Agent.PlayerName}{Time.time}");
        }
    }
    public static class zActions
    {
        public static List<ManualAction> manualActions = new();
        internal static readonly Dictionary<int, dataStore> ActionDataStore = new();
        internal static dataStore GetOrCreateData(PlayerBotActionBase.Descriptor desc)
        {
            PlayerAIBot bot = desc.Bot;
            return GetOrCreateData(bot);
        }
        internal static dataStore GetOrCreateData(PlayerAgent agent)
        {
            if (!agent.Owner.IsBot)
                return null;
            PlayerAIBot bot = agent.GetComponent<PlayerAIBot>();
            return GetOrCreateData(bot);
        }
        internal static dataStore GetOrCreateData(PlayerBotActionBase botBase)
        {
            PlayerAIBot bot = botBase.m_bot;
            return GetOrCreateData(bot);
        }
        internal static dataStore GetOrCreateData(PlayerAIBot botBase)
        {
            int botId = botBase.GetInstanceID();
            if (!ActionDataStore.TryGetValue(botId, out var data))
            {
                data = new dataStore();
                ActionDataStore[botId] = data;
            }
            return data;
        }
        public static bool isManualAction(PlayerBotActionBase.Descriptor descriptor)
        {
            if (descriptor == null) return false;
            if (manualActions == null) return false;

            foreach (ManualAction Action in manualActions)
            {
                var desc = Action.ActionDescriptor;
                if (desc == null) continue;

                if (desc.Pointer == descriptor.Pointer)
                    return true;
            }

            if (descriptor.ParentActionBase != null)
            {
                return isManualAction(descriptor.ParentActionBase.DescBase);
            }

            return false;
        }
    }
}
