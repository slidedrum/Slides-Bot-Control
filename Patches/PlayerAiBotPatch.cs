//using ZombieTweak2.zRootBotPlayerAction.CustomActions;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Player;
using System;
using UnityEngine;


//using ZombieTweak2.zRootBotPlayerAction.CustomActions;

namespace BotControl.Patches
{
    [HarmonyPatch]
    internal class PlayerAiBotPatch
    {
		public static T Canon<T>(T obj) where T : Il2CppSystem.Object
		{
			if (obj == null || obj.Pointer == IntPtr.Zero)
				return null;
			return IL2CPP.PointerToValueGeneric<T>(obj.Pointer, isFieldPointer: false, valueTypeWouldBeBoxed: false);
		}
		[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.StartQueuedActions))]
        [HarmonyPrefix]
        public static bool StartQueuedActions(PlayerAIBot __instance)
        {
            var data = zActions.GetOrCreateData(__instance);
            if (data.m_queuedActions.Count == 0)
            {
                return false;
            }
            PlayerBotActionBase.Descriptor[] array = new PlayerBotActionBase.Descriptor[data.m_queuedActions.Count];
            data.m_queuedActions.CopyTo(array);
            data.m_queuedActions.Clear();
            foreach (PlayerBotActionBase.Descriptor descriptor in array)
            {
                var desc = Canon(descriptor);
                if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Queued)
                {

					desc.OnStarted();
                    PlayerBotActionBase playerBotActionBase = desc.CreateAction();
                    __instance.RemoveCollidingActions(desc);
					data.m_actions.Add(Canon(playerBotActionBase));
                }
            }
            return false;
        }
        [HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.UpdateActions))]
        [HarmonyPrefix]
        public static bool UpdateActions(PlayerAIBot __instance)
        {
            var data = zActions.GetOrCreateData(__instance);
            if (data.m_actions.Count == 0)
            {
                return false;
            }

			PlayerBotActionBase[] array = new PlayerBotActionBase[data.m_actions.Count];
			//var array = new Il2CppReferenceArray<PlayerBotActionBase>(data.m_actions.Count);
            data.m_actions.CopyTo(array);
            for (int i = 0; i < array.Length; i++)
            {
                PlayerBotActionBase playerBotActionBase = array[i];
                playerBotActionBase = Canon(playerBotActionBase);
				PlayerAIBot.s_updatingAction = Canon(playerBotActionBase.DescBase);
                var actionbase = Canon(PlayerAIBot.s_updatingAction?.ActionBase);
				if (!playerBotActionBase.IsActive() || playerBotActionBase.Update())
                { //Has the action completed?

                    if (actionbase != null && actionbase == playerBotActionBase)
                    {
						data.m_actions.Remove(array[i]);
						PlayerAIBot.s_updatingAction.OnExpired();
                        playerBotActionBase.Stop();
                    }
                }
            }
            PlayerAIBot.s_updatingAction = null;
            return false;
        }
        [HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.Setup))]
        [HarmonyPrefix]
        public static void Setup(PlayerAIBot __instance, PlayerAgent agent, RootPlayerBotAction.Descriptor rootAction)
        {
            var data = zActions.GetOrCreateData(__instance);
			__instance.m_actions = data.I_actions;
			__instance.m_queuedActions = data.I_queuedActions;
			ZiMain.log.LogMessage("init playerbot");
        }

		// ── queue API ───────────────────────────────────────────────────────

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.IsActionForbidden))]
		static bool IsActionForbiddenPrefix(PlayerAIBot __instance, PlayerBotActionBase.Descriptor desc, ref bool __result)
		{
			var data = zActions.GetOrCreateData(__instance);
			desc = Canon(desc);

			for (int i = 0; i < data.m_queuedActions.Count; i++)
			{
				if (!Canon(data.m_queuedActions[i]).IsActionAllowed(desc))
				{
					__result = true;
					return false;
				}
			}

			for (int j = 0; j < data.m_actions.Count; j++)
			{
				if (!Canon(data.m_actions[j]).IsActionAllowed(desc))
				{
					__result = true;
					return false;
				}
			}

			__result = false;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.StartAction))]
		static bool StartActionPrefix(PlayerAIBot __instance, PlayerBotActionBase.Descriptor desc)
		{
			var data = zActions.GetOrCreateData(__instance);
			desc = Canon(desc);

			if (!desc.IsTerminated())
				Debug.LogError("Action was queued while active: " + desc);

			for (int i = 0; i < data.m_actions.Count; i++)
			{
				if (Canon(Canon(data.m_actions[i]).DescBase) == desc)
				{
					data.m_actions.RemoveAt(i);
					break;
				}
			}

			desc.OnQueued();
			RemoveCollidingActions(data, desc);
			data.m_queuedActions.Add(desc);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.StopAction))]
		static bool StopActionPrefix(PlayerAIBot __instance, PlayerBotActionBase.Descriptor desc)
		{
			var data = zActions.GetOrCreateData(__instance);
			desc = Canon(desc);

			if (desc == Canon(PlayerAIBot.s_updatingAction))
				Debug.LogError("Action was removed during its update: " + desc);

			if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Queued)
			{
				desc.OnAborted();
				for (int i = 0; i < data.m_queuedActions.Count; i++)
				{
					if (Canon(data.m_queuedActions[i]) == desc)
					{
						data.m_queuedActions.RemoveAt(i);
						return false;
					}
				}

				return false;
			}

			if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Active)
			{
				PlayerBotActionBase actionBase = Canon(desc.ActionBase);
				if (actionBase == null)
					Debug.LogError("Active descriptor is missing action: " + desc);

				if (actionBase != null)
				{
					for (int i = 0; i < data.m_actions.Count; i++)
					{
						if (Canon(data.m_actions[i]) == actionBase)
						{
							data.m_actions.RemoveAt(i);
							break;
						}
					}
				}

				actionBase?.Stop();
				desc.OnStopped();
			}

			return false;
		}

		// ── restrictions / warp / enable ────────────────────────────────────

		//[HarmonyPrefix]
		//[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.ApplyRestrictionsToRootPosition))]
		//static bool ApplyRestrictionsToRootPositionPrefix(
		//	PlayerAIBot __instance,
		//	ref Vector3 testPosition,
		//	ref float restrictionPrio,
		//	ref bool __result)
		//{
		//	var data = zActions.GetOrCreateData(__instance);
		//	float resultPrio = restrictionPrio;
		//	Vector3 resultPos = testPosition;

		//	for (int i = 0; i < data.m_queuedActions.Count; i++)
		//	{
		//		Vector3 tmpPos = testPosition;
		//		PlayerBotActionBase.Descriptor descriptor = Canon(data.m_queuedActions[i]);
		//		if (descriptor.Prio > resultPrio && descriptor.ApplyPositionRestriction(ref tmpPos))
		//		{
		//			resultPrio = descriptor.Prio;
		//			resultPos = tmpPos;
		//		}
		//	}

		//	for (int j = 0; j < data.m_actions.Count; j++)
		//	{
		//		Vector3 tmpPos = testPosition;
		//		PlayerBotActionBase.Descriptor descriptor = Canon(Canon(data.m_actions[j]).DescBase);
		//		if (descriptor.Prio > resultPrio && descriptor.ApplyPositionRestriction(ref tmpPos))
		//		{
		//			resultPrio = descriptor.Prio;
		//			resultPos = tmpPos;
		//		}
		//	}

		//	if (resultPrio > restrictionPrio)
		//	{
		//		restrictionPrio = resultPrio;
		//		testPosition = resultPos;
		//		__result = true;
		//		return false;
		//	}

		//	__result = false;
		//	return false;
		//}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.OnWarped))]
		static bool OnWarpedPrefix(PlayerAIBot __instance, Vector3 position)
		{
			var data = zActions.GetOrCreateData(__instance);

			if (data.m_actions.Count > 0)
			{
				PlayerBotActionBase[] array = new PlayerBotActionBase[data.m_actions.Count];
				data.m_actions.CopyTo(array);

				for (int i = 0; i < array.Length; i++)
				{
					PlayerBotActionBase action = array[i];
					PlayerBotActionBase bot = Canon(action);
					PlayerBotActionBase.Descriptor desc = Canon(bot.DescBase);

					if (!desc.IsTerminated())
						bot.OnWarped(position);

					if (desc.IsTerminated() && Canon(desc.ActionBase) == bot)
						DisposeOfTerminatedAction(data, action, i);
				}
			}

			__instance.SyncValues.Position = position;
			Traverse.Create(__instance).Method("ApplyValues").GetValue();
			__instance.Agent.Locomotion.UpdateMovementFromSync(true);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.SetEnabled))]
		static bool SetEnabledPrefix(PlayerAIBot __instance, bool state)
		{
			if (state == __instance.enabled)
				return false;

			if (state)
				return true; // enable path does not touch action lists

			var data = zActions.GetOrCreateData(__instance);
			PlayerBotActionBase.Descriptor root = Canon(__instance.m_rootAction);

			bool changed;
			do
			{
				changed = false;

				for (int i = 0; i < data.m_queuedActions.Count; i++)
				{
					PlayerBotActionBase.Descriptor queued = Canon(data.m_queuedActions[i]);
					if (queued != root)
					{
						queued.OnAborted();
						data.m_queuedActions.RemoveAt(i);
						changed = true;
					}
				}

				for (int j = 0; j < data.m_actions.Count; j++)
				{
					PlayerBotActionBase action = Canon(data.m_actions[j]);
					PlayerBotActionBase.Descriptor desc = Canon(action.DescBase);
					if (desc != root)
					{
						action.Stop();
						desc.OnStopped();
						data.m_actions.RemoveAt(j);
						changed = true;
					}
				}
			}
			while (changed);

			__instance.enabled = state;
			return false;
		}

		// ── helpers (mirror private PlayerAIBot methods) ──────────────────

		static void DisposeOfTerminatedAction(dataStore data, PlayerBotActionBase action, int searchIndex)
		{
			if (searchIndex < data.m_actions.Count && data.m_actions[searchIndex] == action)
			{
				data.m_actions.RemoveAt(searchIndex);
				PlayerBotActionBase.Descriptor desc = Canon(action?.DescBase);
				desc?.OnExpired();
				action?.Stop();
				return;
			}

			for (int i = 0; i < data.m_actions.Count; i++)
			{
				if (data.m_actions[i] == action)
				{
					data.m_actions.RemoveAt(i);
					PlayerBotActionBase.Descriptor desc = Canon(action?.DescBase);
					desc?.OnExpired();
					action?.Stop();
					return;
				}
			}

			action?.Stop();
		}

		static void RemoveCollidingActions(dataStore data, PlayerBotActionBase.Descriptor desc)
		{
			desc = Canon(desc);
			bool changed;

			do
			{
				changed = false;

				int i = 0;
				while (i < data.m_queuedActions.Count)
				{
					PlayerBotActionBase.Descriptor queued = Canon(data.m_queuedActions[i]);
					if (queued.Status == PlayerBotActionBase.Descriptor.StatusType.Queued && queued.CheckCollision(desc))
					{
						data.m_queuedActions.RemoveAt(i);
						queued.OnAborted();
						changed = true;
					}
					else
					{
						i++;
					}
				}

				int j = 0;
				while (j < data.m_actions.Count)
				{
					PlayerBotActionBase action = Canon(data.m_actions[j]);
					if (action.CheckCollision(desc))
					{
						PlayerBotActionBase.Descriptor activeDesc = Canon(action.DescBase);
						if (activeDesc == Canon(PlayerAIBot.s_updatingAction))
							Debug.LogError("Action was interrupted during its update: " + activeDesc);

						data.m_actions.RemoveAt(j);
						activeDesc.OnInterrupted();
						action.Stop();
						changed = true;
					}
					else
					{
						j++;
					}
				}
			}
			while (changed && !desc.IsTerminated());
		}




















	//[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.Update))]
	//[HarmonyPrefix]
	//public static void Update(PlayerAIBot instance)
	//{

	//}
	//[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.ApplyRestrictionsToRootPosition))]
	//[HarmonyPrefix]
	//public static bool ApplyRestrictionsToRootPosition(PlayerAIBot instance, ref Vector3 testPosition, ref float restrictionPrio, ref bool __result)
	//{
	//    var data = zActions.GetOrCreateData(instance);
	//    float resultPrio = restrictionPrio;
	//    Vector3 resultPos = testPosition;
	//    Vector3 tmpPos;
	//    Func<PlayerBotActionBase.Descriptor, Vector3, bool> ApplyRestriction = delegate (PlayerBotActionBase.Descriptor desc, Vector3 prevPos)
	//    {
	//        tmpPos = prevPos;
	//        if (desc.Prio > resultPrio && desc.ApplyPositionRestriction(ref tmpPos))
	//        {
	//            resultPrio = desc.Prio;
	//            resultPos = tmpPos;
	//        }
	//        return true;
	//    };
	//    for (int i = 0; i < data.m_queuedActions.Count; i++)
	//    {
	//        ApplyRestriction(data.m_queuedActions[i], testPosition);
	//    }
	//    for (int j = 0; j < data.m_actions.Count; j++)
	//    {
	//        ApplyRestriction(data.m_actions[j].DescBase, testPosition);
	//    }
	//    if (resultPrio > restrictionPrio)
	//    {
	//        restrictionPrio = resultPrio;
	//        testPosition = resultPos;
	//        __result = true;
	//        return false;
	//    }
	//    __result = false;
	//    return false;
	//}
	//[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.IsActionForbidden))]
	//[HarmonyPrefix]
	//public static bool IsActionForbidden(PlayerAIBot instance, PlayerBotActionBase.Descriptor desc, ref bool __result)
	//{
	//    var data = zActions.GetOrCreateData(instance);
	//    for (int i = 0; i < data.m_queuedActions.Count; i++)
	//    {
	//        if (!data.m_queuedActions[i].IsActionAllowed(desc))
	//        {
	//            __result = true;
	//            return false;
	//        }
	//    }
	//    for (int j = 0; j < data.m_actions.Count; j++)
	//    {
	//        if (!data.m_actions[j].IsActionAllowed(desc))
	//        {
	//            __result = true;
	//            return false;
	//        }
	//    }
	//    __result = false;
	//    return false;
	//}
	//[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.OnWarped))]
	//[HarmonyPrefix]
	//public static bool OnWarped(PlayerAIBot instance, Vector3 position)
	//{
	//    var data = zActions.GetOrCreateData(instance);
	//    for (int i = 0; i < data.m_actions.Count; i++)
	//    {
	//        PlayerBotActionBase playerBotActionBase = data.m_actions[i];
	//        if (playerBotActionBase.IsActive())
	//        {
	//            playerBotActionBase.OnWarped(position);
	//        }
	//    }
	//    instance.m_syncValues.Position = position;
	//    instance.ApplyValues();
	//    return false;
	//}
	//     [HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.RemoveCollidingActions))]
	//     [HarmonyPrefix]
	//     public static bool RemoveCollidingActions(PlayerAIBot __instance, PlayerBotActionBase.Descriptor desc)
	//     {
	//         desc = Canon(desc);
	//var data = zActions.GetOrCreateData(__instance);
	//         bool hasRemoved;
	//         do
	//         {
	//             hasRemoved = false;
	//             int i = 0;
	//             while (i < data.m_queuedActions.Count)
	//             {
	//                 PlayerBotActionBase.Descriptor descriptor = Canon(data.m_queuedActions[i]);
	//                 if (descriptor.Status == PlayerBotActionBase.Descriptor.StatusType.Queued && descriptor.CheckCollision(desc))
	//                 {
	//                     data.m_queuedActions.RemoveAt(i);
	//                     descriptor.OnAborted();
	//                     hasRemoved = true;
	//                 }
	//                 else
	//                 {
	//                     i++;
	//                 }
	//             }
	//             int j = 0;
	//             while (j < data.m_actions.Count)
	//             {
	//                 PlayerBotActionBase playerBotActionBase = Canon(data.m_actions[j]);
	//                 if (playerBotActionBase.CheckCollision(desc)) //this might be a problem with pointers?
	//                 {
	//                     data.m_actions.RemoveAt(j);
	//                     Canon(playerBotActionBase.DescBase).OnInterrupted();
	//                     playerBotActionBase.Stop();
	//                     hasRemoved = true;
	//                 }
	//                 else
	//                 {
	//                     j++;
	//                 }
	//             }
	//         }
	//         while (hasRemoved && !desc.IsTerminated());
	//         return false;
	//     }
	//[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.SetEnabled))]
	//[HarmonyPrefix]
	//public static bool SetEnabled(PlayerAIBot instance, bool State)
	//{
	//    var data = zActions.GetOrCreateData(instance);
	//    if (State == instance.enabled)
	//    {
	//        return false;
	//    }
	//    if (State)
	//    {
	//        NavMeshHit navMeshHit;
	//        if (NavMesh.SamplePosition(instance.m_playerAgent.Position, out navMeshHit, 3f, -1))
	//        {
	//            instance.m_syncValues.Position = navMeshHit.position;
	//        }
	//        else
	//        {
	//            instance.m_syncValues.Position = instance.m_playerAgent.Position;
	//        }
	//        instance.m_syncValues.Forward = instance.m_playerAgent.Forward;
	//        instance.m_syncValues.LookDirection = instance.m_playerAgent.TargetLookDir;
	//        instance.m_syncValues.Ladder = instance.m_playerAgent.Locomotion.CurrentLadder;
	//        instance.InitValues();
	//        instance.m_lastSyncedPosition = instance.m_syncValues.Position;
	//    }
	//    else
	//    {
	//        bool hasRemoved;
	//        do
	//        {
	//            hasRemoved = false;
	//            for (int i = 0; i < data.m_queuedActions.Count; i++)
	//            {
	//                if (data.m_queuedActions[i].Pointer != instance.m_rootAction.Pointer)
	//                {
	//                    data.m_queuedActions[i].OnAborted();
	//                    data.m_queuedActions.RemoveAt(i);
	//                    hasRemoved = true;
	//                }
	//            }
	//            for (int j = 0; j < data.m_actions.Count; j++)
	//            {
	//                if (data.m_actions[j].DescBase.Pointer != instance.m_rootAction.Pointer)
	//                {
	//                    data.m_actions[j].Stop();
	//                    data.m_actions[j].DescBase.OnStopped();
	//                    data.m_actions.RemoveAt(j);
	//                    hasRemoved = true;
	//                }
	//            }
	//        }
	//        while (hasRemoved);
	//    }
	//    instance.enabled = State;
	//    return false;
	//}
	//     [HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.StartAction))]
	//     [HarmonyPrefix]
	//     public static bool StartAction(PlayerAIBot __instance, PlayerBotActionBase.Descriptor desc)
	//     {
	//         desc = Canon(desc);
	//var data = zActions.GetOrCreateData(__instance);
	//         //if (data. consideringActions)
	//         //{
	//         //    data.bestAction = desc;
	//         //    return false;
	//         //}

	//         if (!desc.IsTerminated())
	//         {
	//             Debug.LogError("Action was queued while active: " + desc);
	//             return false;
	//         }
	//         for (int i = 0; i < data.m_actions.Count; i++)
	//         {
	//             if (Canon(Canon(data.m_actions[i]).DescBase) == desc)
	//             {
	//                 data.m_actions.RemoveAt(i);
	//                 break;
	//             }
	//         }
	//         Canon(desc).OnQueued();
	//         __instance.RemoveCollidingActions(desc);
	//         data.m_queuedActions.Add(desc);
	//         return false;
	//     }
	//     public static void Eval(CP_Bioscan_Core Bioscan, PlayerAIBot bot)
	//     {
	//         float standRadius;
	//         int nrOthers;
	//         bool ret = PlayerBotActionUseBioscan.Descriptor.Evaluate(Bioscan, bot, out standRadius, out nrOthers);
	//     }
	//     [HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.StopAction))]
	//     [HarmonyPrefix]
	//     public static bool PreStopAction(PlayerAIBot __instance, PlayerBotActionBase.Descriptor desc)
	//     {
	//         desc = Canon(desc);
	//         if (desc == Canon(PlayerAIBot.s_updatingAction))
	//         {
	//             Debug.LogError("Action was removed during its update: " + desc);
	//         }
	//         if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Queued)
	//         {
	//             desc.OnAborted();
	//             for (int i = 0; i < __instance.m_queuedActions.Count; i++)
	//             {
	//                 if (Canon(__instance.m_queuedActions[i]) == desc)
	//                 {
	//                     __instance.m_queuedActions.RemoveAt(i);
	//                     return false;
	//                 }
	//             }
	//             return false;
	//         }
	//         if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Active)
	//         {
	//             if (desc.ActionBase == null)
	//             {
	//                 Debug.LogError("Active descriptor is missing action: " + desc);
	//             }
	//             __instance.m_actions.Remove(desc.ActionBase);
	//             desc.ActionBase.Stop();
	//             desc.OnStopped();
	//         }
	//         return false;
	//     }

	//[HarmonyPatch(typeof(PlayerAIBot), nameof(PlayerAIBot.StopAction))]
	//[HarmonyPrefix]
	//public static bool StopAction(PlayerAIBot instance, PlayerBotActionBase.Descriptor desc)
	//{
	//    var data = zActions.GetOrCreateData(instance);
	//    if (desc == PlayerAIBot.s_updatingAction)
	//    {
	//        VisDebug.LogError("Action was removed during its update: " + desc);
	//    }
	//    if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Queued)
	//    {
	//        desc.OnAborted();
	//        for (int i = 0; i < data.m_queuedActions.Count; i++)
	//        {
	//            if (data.m_queuedActions[i] == desc)
	//            {
	//                data.m_queuedActions.RemoveAt(i);
	//                return false;
	//            }
	//        }
	//        return false;
	//    }
	//    if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Active)
	//    {
	//        if (desc.ActionBase == null)
	//        {
	//            VisDebug.LogError("Active descriptor is missing action: " + desc);
	//        }
	//        data.m_actions.Remove(desc.ActionBase);
	//        desc.ActionBase.Stop();
	//        desc.OnStopped();
	//    }
	//    return false;
	//}
} // class
}// namespace
