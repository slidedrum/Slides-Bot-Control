using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#nullable enable

namespace BotControl
{
    public static class zHelpers
    {
        private static readonly NavMeshPath s_path = new NavMeshPath();

        /// <summary>
        /// Finds a navmesh position along the path from <paramref name="start"/> to <paramref name="end"/>
        /// that is <paramref name="standoffDistance"/> path-units before the end.
        /// </summary>
        /// <returns>
        /// False when no complete path exists. True when a standoff point was resolved
        /// (including when the path is shorter than the standoff — then <paramref name="standoffPosition"/> is start).
        /// </returns>
        public static bool TryGetStandoffPosition(
            Vector3 start,
            Vector3 end,
            float standoffDistance,
            out Vector3 standoffPosition,
            int areaMask = -1)
        {
            standoffPosition = end;

            if (!NavMesh.CalculatePath(start, end, areaMask, s_path))
            {
                return false;
            }

            if (s_path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            Il2CppStructArray<Vector3> corners = s_path.corners;
            if (corners == null || corners.Length == 0)
            {
                return false;
            }

            float totalLength = GetPathLength(corners);
            float stopAt = totalLength - standoffDistance;

            if (stopAt <= 0f)
            {
                standoffPosition = corners[0];
                return true;
            }

            GetPointOnPath(corners, stopAt, out standoffPosition);
            return true;
        }

        public static void GetPointOnPath(Il2CppStructArray<Vector3> corners, float maxDistance, out Vector3 point)
        {
            if (corners == null || corners.Length == 0)
            {
                point = Vector3.zero;
                return;
            }

            if (corners.Length == 1)
            {
                point = corners[0];
                return;
            }

            float accumulated = 0f;
            Vector3 previousCorner = corners[0];

            for (int i = 1; i < corners.Length; i++)
            {
                Vector3 corner = corners[i];
                float segmentLength = Vector3.Distance(previousCorner, corner);

                if (accumulated + segmentLength >= maxDistance)
                {
                    float t = segmentLength <= Mathf.Epsilon
                        ? 0f
                        : (maxDistance - accumulated) / segmentLength;

                    point = Vector3.Lerp(previousCorner, corner, Mathf.Clamp01(t));
                    return;
                }

                accumulated += segmentLength;
                previousCorner = corner;
            }

            point = corners[corners.Length - 1];
        }

        private static float GetPathLength(Il2CppStructArray<Vector3> corners)
        {
            if (corners == null || corners.Length < 2)
            {
                return 0f;
            }

            float length = 0f;

            for (int i = 1; i < corners.Length; i++)
            {
                length += Vector3.Distance(corners[i - 1], corners[i]);
            }

            return length;
        }

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

        public static bool SnapPositionToNav(
            Vector3 originalPosition,
            out Vector3 resultPosition,
            float maxdistance = 1.5f,
            int areamask = -1)
        {
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(originalPosition, out navMeshHit, maxdistance, areamask))
            {
                resultPosition = navMeshHit.position;
                return true;
            }

            resultPosition = originalPosition;
            return false;
        }

        public static bool IsObstructed(
            Vector3 from,
            GameObject target,
            GameObject excludedObject,
            int layerMask = ~0)
        {
            return IsObstructed(
                from,
                target,
                layerMask,
                new[] { excludedObject });
        }

        public static bool IsObstructed(
            Vector3 from,
            GameObject target,
            int layerMask = ~0,
            IEnumerable<GameObject>? excludedObjects = null)
        {
            if (target == null)
                return true;

            Vector3 to = target.transform.position;
            Vector3 dir = to - from;
            float dist = dir.magnitude;

            if (dist <= 0.0001f)
                return false;

            dir /= dist;

            RaycastHit[] hits = Physics.RaycastAll(from, dir, dist, layerMask);

            foreach (var hit in hits)
            {
                Transform hitTransform = hit.transform;

                // Ignore target and its children
                if (hitTransform == target.transform ||
                    hitTransform.IsChildOf(target.transform))
                {
                    continue;
                }

                // Ignore additional excluded objects and their children
                if (excludedObjects != null)
                {
                    bool excluded = false;

                    foreach (GameObject obj in excludedObjects)
                    {
                        if (obj == null)
                            continue;

                        if (hitTransform == obj.transform ||
                            hitTransform.IsChildOf(obj.transform))
                        {
                            excluded = true;
                            break;
                        }
                    }

                    if (excluded)
                        continue;
                }

                return true;
            }

            return false;
        }

        public static bool IsOfType<T>(Il2CppObjectBase instance)
        {
            if (instance == null)
                return false;

            var type = IL2CPP.il2cpp_object_get_class(instance.Pointer);
            var target = Il2CppClassPointerStore<T>.NativeClassPtr;

            return type == target || IL2CPP.il2cpp_class_is_assignable_from(target, type);
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
            if (!NavMesh.SamplePosition(Position, out navMeshHit, 1f, -1))
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

            Il2CppStructArray<Vector3> corners = path.corners;
            if (corners == null || corners.Length == 0)
                return false;

            Vector3 lastCorner = corners[corners.Length - 1];
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