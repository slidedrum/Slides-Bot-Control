using HarmonyLib;
using Player;

namespace BotControl.Patches
{
    [HarmonyPatch]
    internal class CarryExpeditionItemPatch
    {
        [HarmonyPatch(typeof(PlayerBotActionCarryExpeditionItem), nameof(PlayerBotActionCarryExpeditionItem.GetHoldActionPrio))]
        [HarmonyPostfix]
        static void PostGetHoldActionPrio(PlayerBotActionCarryExpeditionItem __instance, ref float __result, ref bool isHighPrio)
        {//needed because if the bot stays near the leader for 3 seconds then it drops the item for some reason.
            
            var Leader = __instance.m_bot.SyncValues.Leader;
            PlayerAgent Self = __instance.m_bot.Agent;
            if (Self.Damage.Health <= 0)
                return;
            if (Leader == Self)
            {
                // Force stable carry state
                if (__result <= 0f)
                {
                    __result = 0.1f; // or lowest non-zero priority
                    isHighPrio = true;
                }
            }
        }
    }
}
