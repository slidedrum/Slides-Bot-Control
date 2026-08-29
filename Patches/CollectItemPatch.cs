using HarmonyLib;
using Player;
using UnityEngine;

namespace BotControl.Patches
{
    [HarmonyPatch]
    internal class CollectItemPatch
    {
        [HarmonyPatch(typeof(PlayerBotActionCollectItem), nameof(PlayerBotActionCollectItem.MoveOut))]
        [HarmonyPostfix]
        public static void PostMoveOut(PlayerBotActionCollectItem __instance)
        {
            var travel = __instance.m_travelAction;
            if (travel == null)
                return;
            zHelpers.SnapPositionToNav(travel.DestinationPos, out Vector3 newDestination, maxdistance: 5f, areamask: -1);
            travel.DestinationPos = newDestination;
            // Ground pickups: arrive within 2m. Locker stand points stay tight so they reach the interact.
            travel.Radius = (__instance.m_desc.TargetContainer == null) ? 2f : 1.5f;
        }
    }
}
