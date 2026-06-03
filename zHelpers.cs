using Il2CppInterop.Runtime;
using Player;
using UnityEngine;
using UnityEngine.AI;
#nullable enable
namespace BotControl
{
    public static class zHelpers
    {
        public static float Round(float value, int decimalPlaces)
        {
            float multiplier = Mathf.Pow(10f, decimalPlaces);
            return Mathf.Round(value * multiplier) / multiplier;
        }
        public static uint HashString(string str)
        {
            unchecked
            {
                uint hash = 2166136261;

                for (int i = 0; i < str.Length; i++)
                {
                    hash ^= str[i];
                    hash *= 16777619;
                }

                return hash;
            }
        }
        public static bool IsOfType<T>(Il2CppSystem.Type type)
        {
            Il2CppSystem.Type target = Il2CppType.Of<T>();
            return type == target || type.IsSubclassOf(target);
        }
        public static uint GetAgentBackpackItemId(PlayerAgent agent, InventorySlot slot)
        {
            return GetAgentBackpackItem(agent, slot)?.ItemID ?? 0;
        }
        public static BackpackItem GetAgentBackpackItem(PlayerAgent agent, InventorySlot slot)
        {
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(agent.Owner);
            if (backpack.TryGetBackpackItem(slot, out BackpackItem backpackItem))
                return backpackItem;
            return null;
        }
        public static bool PositionIsValidForAgent(PlayerAgent Agent, ref Vector3 Position)
        {
            NavMeshHit navMeshHit;
            if (!NavMesh.SamplePosition(Position, out navMeshHit, 0.2f, -1))
                return false;
            Position = navMeshHit.position;
            NavMeshPath navMeshPath = new NavMeshPath();
            if (!NavMesh.CalculatePath(Agent.GoodPosition, Position, 17, navMeshPath))
                return false;
            if (navMeshPath.status != NavMeshPathStatus.PathComplete)
                return false;
            return true;
        }
        public static bool CanBotReach(PlayerAIBot bot, Vector3 location)
        {
            if (!NavMesh.SamplePosition(location, out NavMeshHit hit, 1.5f, 17))
                return false;
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(bot.Agent.GoodPosition, hit.position, 17, path))
                return false;
            return path.status == NavMeshPathStatus.PathComplete;
        }
    }

}
