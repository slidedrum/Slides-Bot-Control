using Agents;
using AIGraph;
using Enemies;
using HarmonyLib;
using Player;
using UnityEngine;

namespace BotControl.CustomActions
{
    internal class CustomWakeManager
    {
        internal static float walkNoiseCheckInterval = 1;
        // This maybe can be changed to use vanilla systems. 
        // Loop all enemies, 
        private static float walkNoiseChance = 1f / 50f;
        private static float hitNoiseChance = 1f / 10f;
        internal static void WalkNoiseCheck(PlayerAgent Agent)
        {
            if (UnityEngine.Random.value < walkNoiseChance)
                MakePlayerNoise(Agent);
        }
        internal static void HitNoiseCheck(PlayerAgent Agent, float Multiplier = 1f)
        {
            if (UnityEngine.Random.value < hitNoiseChance)
            {
                MakePlayerNoise(Agent);
            }
        }
        private static void MakePlayerNoise(PlayerAgent player, float radius = 15f)
        {
            if (player == null || !player.Alive)
                return;

            AIG_CourseNode node = player.CourseNode;
            if (node == null)
                return;

            NM_NoiseData noise = new();
            noise.noiseMaker = player.Cast<INM_NoiseMaker>();
            noise.position = player.transform.position;
            noise.radiusMin = 0f;
            noise.radiusMax = radius;
            noise.yScale = 1f;
            noise.node = node;
            noise.type = NM_NoiseType.InstaDetect;
            noise.includeToNeightbourAreas = true;
            noise.raycastFirstNode = false;

            NoiseManager.MakeNoise(noise);
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
    }
}
