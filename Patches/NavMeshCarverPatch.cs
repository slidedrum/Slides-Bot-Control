using Enemies;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BotControl.Patches
{
    [HarmonyPatch]
    internal class NavMeshCarverPatch
    {
        static float carveRadius = 1f;
        private static readonly HashSet<IntPtr> CarvedSleepers = new();

        [HarmonyPatch(typeof(ES_Hibernate), nameof(ES_Hibernate.Enter))]
        [HarmonyPostfix]
        static void Post_HibernateEnter(ES_Hibernate __instance)
        {
            EnableCarve(__instance.m_enemyAgent);
        }

        [HarmonyPatch(typeof(ES_Hibernate), "CommonExit")]
        [HarmonyPostfix]
        static void Post_HibernateCommonExit(ES_Hibernate __instance)
        {
            DisableCarve(__instance.m_enemyAgent);
        }

        // Skip SetNavMeshAgent(true) while this sleeper's pose is still inside a carve hole.
        // Warp / SamplePosition would snap them to the rim; disable-agent calls still run.
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SetNavMeshAgent))]
        [HarmonyPrefix]
        static bool PreSetNavMeshAgent(EnemyAI __instance, bool mode)
        {
            if (__instance == null || __instance.m_enemyAgent == null)
                return true;

            IntPtr key = __instance.m_enemyAgent.Pointer;
            if (!CarvedSleepers.Contains(key))
                return true;

            if (!mode)
                return true;

            Vector3 pos = __instance.m_enemyAgent.transform.position;
            if (IsOnNavMesh(pos))
            {
                CarvedSleepers.Remove(key);
                return true;
            }

            return false;
        }

        private static bool IsOnNavMesh(Vector3 position, float maxDistance = 0.1f)
        {
            return NavMesh.SamplePosition(position, out _, maxDistance, -1);
        }

        private static void EnableCarve(EnemyAgent m_enemyAgent)
        {
            if (m_enemyAgent == null)
                return;
            CarvedSleepers.Add(m_enemyAgent.Pointer);
            var obstacle = m_enemyAgent.gameObject.GetComponent<NavMeshObstacle>() ?? m_enemyAgent.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.radius = carveRadius;  // carveRadius + botRadius must be > 1 m
            obstacle.height = 2f;
            obstacle.center = Vector3.up;
            obstacle.carveOnlyStationary = true;
            obstacle.carving = true;
            obstacle.enabled = true;
        }

        private static void DisableCarve(EnemyAgent m_enemyAgent)
        {
            if (m_enemyAgent == null)
                return;
            var obstacle = m_enemyAgent.gameObject.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
                return;
            obstacle.carving = false;
            obstacle.enabled = false;
        }
    }
}
