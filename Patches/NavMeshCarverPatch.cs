using Enemies;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace BotControl.Patches
{
    [HarmonyPatch]
    internal class NavMeshCarverPatch
    {
        static float carveRadius = 1f;
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

        private static void EnableCarve(EnemyAgent m_enemyAgent)
        {
            if (m_enemyAgent == null)
                return;
            var obstacle = m_enemyAgent.gameObject.GetComponent<NavMeshObstacle>() ?? m_enemyAgent.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.radius = carveRadius;  // carveRadius + botRadius must be > 1 m
            obstacle.height = 2f;
            obstacle.center = Vector3.up; // or whatever matches the capsule
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
