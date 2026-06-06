using Agents;
using Il2CppInterop.Runtime;
using Player;
using System.Collections.Generic;
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
            return type != null && (type == target || type.IsSubclassOf(target));
        }
        public static uint GetAgentBackpackItemId(PlayerAgent agent, InventorySlot slot)
        {
            return GetAgentBackpackItem(agent, slot)?.ItemID ?? 0;
        }
        public static bool TryGetAgentBackpackItem(PlayerAgent agent, InventorySlot slot, out BackpackItem backpackItem)
        {
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(agent.Owner);
            if (backpack.TryGetBackpackItem(slot, out backpackItem))
                return true;
            return false;
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
        public static bool CanBotReach(PlayerAIBot bot, Vector3 location, float maxDistance = 6f)
        {
            if (!NavMesh.SamplePosition(location, out NavMeshHit hit, 3f, 17))
                return false;
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(bot.Agent.GoodPosition, hit.position, 17, path))
                return false;
            if (path.status == NavMeshPathStatus.PathComplete)
                return true;
            if (path.status == NavMeshPathStatus.PathInvalid)
                return false;
            Vector3 lastCorner = path.corners[path.corners.Length - 1];
            return Vector3.Distance(lastCorner + Vector3.up * 1.5f, location) < maxDistance;
        }
    }
    public class ComponentInstanceIdComparer : IEqualityComparer<Component>
    {
        public bool Equals(Component x, Component y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x == null || y == null)
                return false;

            return x.GetInstanceID() == y.GetInstanceID();
        }

        public int GetHashCode(Component obj)
        {
            return obj?.GetInstanceID() ?? 0;
        }
    }
    public class Il2CppTypePtrComparer : IEqualityComparer<Il2CppSystem.Type>
    {
        public bool Equals(Il2CppSystem.Type x, Il2CppSystem.Type y)
        {
            if (x == null || y == null) return false;
            return x.Pointer == y.Pointer;
        }

        public int GetHashCode(Il2CppSystem.Type obj)
        {
            return obj.Pointer.GetHashCode();
        }
    }

}
