using Agents;
using Enemies;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.Test
{
    [HarmonyPatch]
    internal static class DetectionDebugPatch
    {
        const float LabelLifetime = 5f;

        class Label
        {
            public GameObject go;
            public TextMesh mesh;
            public float until;
        }

        static readonly Dictionary<IntPtr, Label> s_labels = new();
        static bool s_tickSubscribed;

        [HarmonyPatch(typeof(EnemyDetection), nameof(EnemyDetection.UpdateHibernationDetection))]
        [HarmonyPostfix]
        static void PostHibernateDetect(EnemyDetection __instance, bool __result, ref AgentTarget target)
        {
            if (!ZiMain.debugMode || !__result)
                return;
            Log("HibernateDetect", __instance.m_ai, target);
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SetDetectedAgent))]
        [HarmonyPrefix]
        static void PreSetDetectedAgent(EnemyAI __instance, Agent agent, AgentTargetDetectionType detectionType)
        {
            if (!ZiMain.debugMode)
                return;
            if (__instance.m_lastAgentDetected == agent)
                return;
            var player = agent.TryCast<PlayerAgent>();
            if (player == null)
                return;
            ZiMain.log.LogInfo(
                $"[Detect:SetDetectedAgent] enemy={EnemyName(__instance)} mode={__instance.Mode} " +
                $"eb={__instance.m_behaviour?.CurrentState} es={__instance.m_locomotion?.CurrentStateEnum} " +
                $"player={player.PlayerName} why={detectionType}");
            Show(__instance, "SetDetectedAgent", player.PlayerName, detectionType.ToString());
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.InjectPropagatedTarget))]
        [HarmonyPrefix]
        static void PreInjectPropagatedTarget(EnemyAI __instance, Agent agent, AgentTargetPropagationType propagation)
        {
            if (!ZiMain.debugMode)
                return;
            var player = agent.TryCast<PlayerAgent>();
            if (player == null)
                return;
            ZiMain.log.LogInfo(
                $"[Detect:Propagate] enemy={EnemyName(__instance)} mode={__instance.Mode} " +
                $"eb={__instance.m_behaviour?.CurrentState} es={__instance.m_locomotion?.CurrentStateEnum} " +
                $"player={player.PlayerName} why={AgentTargetDetectionType.PropagatedDetection} prop={propagation}");
            Show(__instance, "Propagate", player.PlayerName, AgentTargetDetectionType.PropagatedDetection.ToString());
        }

        [HarmonyPatch(typeof(ScoutAntennaDetection), nameof(ScoutAntennaDetection.RegisterTarget))]
        [HarmonyPrefix]
        static void PreScoutRegisterTarget(ScoutAntennaDetection __instance, AgentTarget detectedTarget)
        {
            if (!ZiMain.debugMode)
                return;
            Log("Scout", __instance.m_owner?.AI, detectedTarget);
        }

        [HarmonyPatch(typeof(EB_Hibernating), nameof(EB_Hibernating.SetNoiseTarget))]
        [HarmonyPostfix]
        static void PostSetNoiseTarget(EB_Hibernating __instance, bool __result)
        {
            if (!ZiMain.debugMode || !__result)
                return;
            Log("Noise", __instance.m_ai, __instance.m_ai?.Target);
        }

        static void Log(string src, EnemyAI ai, AgentTarget target)
        {
            if (target?.m_agent == null)
                return;
            var player = target.m_agent.TryCast<PlayerAgent>();
            if (player == null)
                return;
            ZiMain.log.LogInfo(
                $"[Detect:{src}] enemy={EnemyName(ai)} mode={ai?.Mode} " +
                $"eb={ai?.m_behaviour?.CurrentState} es={ai?.m_locomotion?.CurrentStateEnum} " +
                $"player={player.PlayerName} why={target.m_detectionType} prop={target.m_propagation} " +
                $"auto={target.m_autoDetect} dist={target.m_distance:F2} light={target.m_lightDetectionBuildup:F2} " +
                $"noise={target.noiseType}/{target.m_noiseDetectWindow_NoiseType} occ={target.m_isOccluded}");
            Show(ai, src, player.PlayerName, target.m_detectionType.ToString());
        }

        static void Show(EnemyAI ai, string src, string player, string why)
        {
            if (!ZiMain.debugMode)
                return;
            var enemy = ai?.m_enemyAgent;
            if (enemy == null)
                return;

            EnsureTick();

            IntPtr key = enemy.Pointer;
            if (!s_labels.TryGetValue(key, out var label) || label.go == null)
            {
                var go = new GameObject("DetectLabel");
                go.transform.SetParent(enemy.transform, false);
                go.transform.localPosition = Vector3.up * 2.1f;

                var mesh = go.AddComponent<TextMesh>();
                mesh.characterSize = 0.1f;
                mesh.anchor = TextAnchor.LowerCenter;
                mesh.alignment = TextAlignment.Center;
                mesh.color = Color.white * 0.2f;

                label = new Label { go = go, mesh = mesh };
                s_labels[key] = label;
            }

            label.mesh.text = $"{src} {player} {why}";
            label.mesh.color = Color.white * 0.2f;
            label.until = Time.time + LabelLifetime;
        }

        static void EnsureTick()
        {
            if (s_tickSubscribed)
                return;
            zUpdater.onUpdate.Listen(Tick);
            s_tickSubscribed = true;
        }

        static void Tick()
        {
            if (s_labels.Count == 0)
                return;

            float now = Time.time;
            Camera cam = Camera.main;
            List<IntPtr> dead = null;

            foreach (var kv in s_labels)
            {
                Label label = kv.Value;
                if (label.go == null || label.mesh == null)
                {
                    dead ??= new List<IntPtr>();
                    dead.Add(kv.Key);
                    continue;
                }

                float remaining = label.until - now;
                if (remaining <= 0f)
                {
                    UnityEngine.Object.Destroy(label.go);
                    dead ??= new List<IntPtr>();
                    dead.Add(kv.Key);
                    continue;
                }

                Color c = label.mesh.color;
                c.a = remaining / LabelLifetime;
                label.mesh.color = c;

                if (cam != null)
                    label.go.transform.rotation = Quaternion.LookRotation(label.go.transform.position - cam.transform.position);
            }

            if (dead == null)
                return;
            for (int i = 0; i < dead.Count; i++)
                s_labels.Remove(dead[i]);
        }

        static string EnemyName(EnemyAI ai)
        {
            return ai?.m_enemyAgent != null ? ai.m_enemyAgent.name : "?";
        }
    }
}
