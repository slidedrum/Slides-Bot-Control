using AIGraph;
using BotControl.CustomActions;
using Enemies;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Player.PlayerBotActionTravel;

namespace BotControl.Patches
{
    [HarmonyPatch]
    internal class TravelActionPatch
    {
        private static Dictionary<IntPtr, AgentData> agentData = new();
        private class AgentData
        {
            public List<EnemyAgent> NearbyTwitchers = new();
            public List<EnemyAgent> NearbySleepers = new();
            public EnemyAgent EnemyLookingAt = null;
            public float LastTimeNotWaitingForTwitcher = 0f;
            public float LookingSince = 0f;
        }
        private static AgentData GetOrCreateData(IntPtr bot)
        {
            if (agentData == null)
                agentData = new();
            if (!agentData.ContainsKey(bot))
                agentData[bot] = new AgentData();
            return agentData[bot];
        }
        [HarmonyPatch(typeof(PlayerBotActionWalk), nameof(PlayerBotActionWalk.UpdateMovement))]
        [HarmonyPrefix]
        private static bool Pre_UpdateMovement(PlayerBotActionWalk __instance)
        {
            //return true;
            AgentData data = GetOrCreateData(__instance.m_bot.Pointer);
            if (DramaManager.CurrentStateEnum != DRAMA_State.Sneaking)
            {
                data.LastTimeNotWaitingForTwitcher = Time.time;
                return true;
            }
            bool HoldForTwicher = DramaManager.CurrentStateEnum == DRAMA_State.Sneaking && zActions.DoingAnyManualAction(__instance.m_bot.Agent) && __instance.m_bot.m_hasTwitcherNearby;
            if (!HoldForTwicher)
                data.LastTimeNotWaitingForTwitcher = Time.time;
            else
            {
                if (Time.time - data.LastTimeNotWaitingForTwitcher > 5f)
                {
                    data.LastTimeNotWaitingForTwitcher = Time.time + 1f;
                }
            }
            return !HoldForTwicher || data.LastTimeNotWaitingForTwitcher > Time.time;
        }
        [HarmonyPatch(typeof(PlayerBotActionTravel), nameof(PlayerBotActionTravel.UpdateStateMove))]
        [HarmonyPrefix]
        public static bool Pre_PlayerBotActionTravel_UpdateStateMove_Patch(PlayerBotActionTravel __instance)
        {
            if (__instance.m_journey.Count == 0)
                return true;
            JourneyPartBase part = __instance.m_journey[0];
            if (part.Type == JourneyPartBase.TypeEnum.Walk)
            {
                JourneyPartWalk walkPart = part.TryCast<JourneyPartWalk>();
                if (DramaManager.CurrentStateEnum == DRAMA_State.Sneaking && zActions.DoingAnyManualAction(__instance.m_bot.Agent))
                {
                    if (__instance.m_bot.m_hasSleeperNearby)
                        walkPart.Action.Posture = PlayerBotActionWalk.Descriptor.PostureEnum.Crouch;
                }
            }
            return true;
        }
        [HarmonyPatch(typeof(PlayerBotActionWalk), nameof(PlayerBotActionWalk.UpdateLookAction))]
        [HarmonyPrefix]
        private static bool UpdateLook(PlayerBotActionWalk __instance)
        {
            var data = GetOrCreateData(__instance.m_bot.Pointer);
            if (Time.time - data.LookingSince < 2 && data.EnemyLookingAt != null && data.EnemyLookingAt.Alive)
                return false;
            EnemyAgent LookingAt = data.NearbyTwitchers.Count > 0 ? data.NearbyTwitchers[UnityEngine.Random.Range(0, data.NearbyTwitchers.Count)] : null;
            if (LookingAt == null)
                LookingAt = data.NearbySleepers.Count > 0 ? data.NearbySleepers[UnityEngine.Random.Range(0, data.NearbySleepers.Count)] : null;
            data.EnemyLookingAt = LookingAt;
            if (LookingAt == null)//todo && simple raycast from bot head to enemy position to see if the bot can see the enemy)
            {
                if (__instance.m_lookAction != null && !__instance.m_lookAction.IsTerminated())
                    __instance.m_bot.StopAction(__instance.m_lookAction);
                return true;
            }
            if (__instance.m_lookAction == null)
                __instance.m_lookAction = new(__instance.m_bot, true)
                {
                    TargetType = PlayerBotActionLook.TargetTypeEnum.Object,
                    Haste = 1f,
                    Prio = __instance.DescBase.Prio,
                    ParentActionBase = __instance,
                };
            __instance.m_lookAction.TargetObj = LookingAt.transform;
            __instance.m_lookAction.TargetType = PlayerBotActionLook.TargetTypeEnum.Object;
            data.LookingSince = Time.time;
            if (__instance.m_lookAction.IsTerminated())
                __instance.m_bot.RequestAction(__instance.m_lookAction);
            return false;
        }
        [HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.SleeperCheck))]
        [HarmonyPrefix]
        private static bool Pre_PlayerAIBot_SleeperCheck_Patch(PlayerAIBot __instance) 
        { // This is effectively exactly the same as the original method.  The only difference is now we keep track of the enemies it foudn.
            if (Time.time < __instance.m_nextSleeperCheckTime && (__instance.m_playerAgent.Position - __instance.m_lastSleeperCheckPosition).sqrMagnitude < PlayerAIBot.s_sleeperCheckResetDistanceSQ)
            {
                return false;
            }
            List<EnemyAgent> SleepersNearby = new List<EnemyAgent>();
            List<EnemyAgent> TwitchersNearby = new List<EnemyAgent>();
            Il2CppSystem.Collections.Generic.List<EnemyAgent> enemies = new ();
            AIG_CourseNode.GetEnemiesInNodes(__instance.m_playerAgent.CourseNode, 2, enemies);
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyAgent enemyAgent = enemies[i];
                if (enemyAgent.CourseNode != null)
                {
                    bool isDisturbed;
                    bool isWakingUp;
                    if (enemyAgent.AI.IsHibernating(out isDisturbed, out isWakingUp) && !isWakingUp)
                    {
                        if (enemyAgent.CourseNode.m_playerCoverage.GetNodeDistanceToClosestPlayer_Unblocked() < 3)
                        {
                            float sqrMagnitude = (__instance.m_playerAgent.Position - enemyAgent.Position).sqrMagnitude;
                            if (sqrMagnitude < PlayerAIBot.s_sleeperCheckMaxDistanceSQ)
                            {
                                SleepersNearby.Add(enemyAgent);
                                if (sqrMagnitude < PlayerAIBot.s_twitchingSleeperCheckDistanceSQ && enemyAgent.IsHibernationDetecting)
                                {
                                    TwitchersNearby.Add(enemyAgent);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            GetOrCreateData(__instance.Pointer).NearbyTwitchers = TwitchersNearby;
            GetOrCreateData(__instance.Pointer).NearbySleepers = SleepersNearby;
            __instance.m_hasSleeperNearby = SleepersNearby.Count > 0;
            __instance.m_hasTwitcherNearby = TwitchersNearby.Count > 0;
            __instance.m_nextSleeperCheckTime = Time.time + (__instance.m_hasSleeperNearby ? PlayerAIBot.s_sleeperCheckIntervalPos : PlayerAIBot.s_sleeperCheckIntervalNeg);
            __instance.m_lastSleeperCheckPosition = __instance.m_playerAgent.Position;
            return false;
        }
    }
}
