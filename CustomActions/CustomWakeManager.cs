using Agents;
using AIGraph;
using Enemies;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BotControl.CustomActions
{
    internal class CustomWakeManager
    {
        internal static float walkNoiseCheckInterval = 1;
        // This maybe can be changed to use vanilla systems. 
        // Loop all enemies, 
        private static float walkNoiseChance = 1f / 50f;
        private static float hitNoiseChance = 1f / 10f;
        public static bool IsDetectable(PlayerAIBot bot)
        {
            return zActions.DoingAnyManualAction(bot.Agent);
        }
        internal static void ApplyToExistingTargets(PlayerAgent bot, bool detectable)
        {
            var nodes = AIG_CourseNode.s_allNodes;
            if (nodes == null)
                return;

            for (int n = 0; n < nodes.Count; n++)
            {
                AIG_CourseNode node = nodes[n];
                var enemies = node?.m_enemiesInNode;
                if (enemies == null)
                    continue;

                for (int e = 0; e < enemies.Count; e++)
                {
                    EnemyAgent enemy = enemies[e];
                    EnemyBehaviourData data = enemy?.AI?.m_behaviourData;
                    if (data == null)
                        continue;

                    Apply(data.GetTarget(bot), bot, detectable);
                }
            }
        }
        internal static void Apply(AgentTarget target, PlayerAgent bot, bool detectable)
        {
            if (target == null)
                return;

            target.m_wakesHibernators = detectable;
            if (detectable)
            {
                Transform aim = bot.AimTarget;
                if (aim != null)
                    target.m_aimTargetPosition = aim.position;
                target.m_autoDetect = false;
            }
            else
            {
                target.m_aimTargetPosition = Vector3.zero;
                target.m_autoDetect = false;
            }
        }
    }
    [HarmonyPatch]
    public static class TargetPatch 
    {
        [HarmonyPatch(typeof(EnemyBehaviourData), nameof(EnemyBehaviourData.GetTarget))]
        static class GetTargetPatch
        {
            static void Postfix(Agent agent, AgentTarget __result)
            {
                var player = agent as PlayerAgent;
                if (__result == null || player == null || player.Owner == null || !player.Owner.IsBot)
                    return;

                CustomWakeManager.Apply(__result, player, zActions.DoingAnyManualAction(player));
            }
        }
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.InjectPropagatedTarget))]
        [HarmonyPostfix]
        static void PostInject(EnemyAI __instance, Agent agent)
        {
            PlayerAgent player = agent.TryCast<PlayerAgent>();
            if (player?.Owner == null || !player.Owner.IsBot)
                return;
            if (!__instance.IsHibernating(out _, out bool waking) || waking)
                return;
            __instance.SetTarget(agent);
            AgentTarget target = __instance.Target;
            __instance.m_locomotion.HibernateWakeup.ActivateState(target.m_dir, target.m_distance, 0f, true);
        }
    }
}
