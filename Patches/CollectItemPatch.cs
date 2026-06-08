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
            Vector3 newDestination = travel.DestinationPos;
            __instance.SnapPositionToNav(travel.DestinationPos, out newDestination);
            travel.DestinationPos = newDestination;
            travel.Radius = (__instance.m_desc.TargetContainer != null) ? 1.5f : 0f; // This probably fixes the bug where bots will run back and forth and never reach the item.
            //travel.Radius = (__instance.m_desc.TargetContainer == null) ? 1.5f : 0f; // original
        }
    }
}
