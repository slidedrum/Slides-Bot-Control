//using Agents;
//using AIGraph;
//using Enemies;
//using Il2CppInterop.Runtime;
//using Player;
//using SNetwork;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;

//namespace BotControl.CustomActions
//{
//    /// <summary>
//    /// Full managed replacement for bot AI state and logic. The IL2CPP <see cref="PlayerAIBot"/>
//    /// component is only a Unity/network identity handle exposed via <see cref="Component"/>.
//    /// </summary>
//    public class ManagedPlayerAIbot : PlayerAIBot
//    {
//		public PlayerAgent Agent
//		{
//			get
//			{
//				return this.m_playerAgent;
//			}
//		}

//		public PlayerBackpack Backpack
//		{
//			get
//			{
//				return this.m_backpack;
//			}
//		}

//		public PlayerInventorySynced Inventory
//		{
//			get
//			{
//				return this.m_inventory;
//			}
//		}

//		public List<PlayerBotActionBase> Actions
//		{
//			get
//			{
//				return this.m_actions;
//			}
//		}

//		public ManagedPlayerAIbot.SyncTable SyncValues
//		{
//			get
//			{
//				return this.m_syncValues;
//			}
//		}
//		private readonly PlayerAIBot _component;

//		public PlayerAIBot Component => _component;

//		public ManagedPlayerAIbot(PlayerAIBot component)
//		{
//			_component = component;
//			this.m_actions = new List<PlayerBotActionBase>();
//			this.m_queuedActions = new List<PlayerBotActionBase.Descriptor>();
//			this.m_syncValues = new ManagedPlayerAIbot.SyncTable();
//			this.m_lastSyncedPosition = Vector3.zero;
//			this.m_deployedItems = new List<ItemEquippable>();
//			this.m_lastSleeperCheckPosition = Vector3.zero;
//			this.m_failEntries = new Dictionary<Il2CppSystem.Type, List<PlayerBotActionBase.FailEntry>>();
//			this.m_idleChatterCheckList = new List<bool>();
//		}

//		static ManagedPlayerAIbot()
//		{
//			ManagedPlayerAIbot.s_testPosReservation = new PlayerManager.PositionReservation
//			{
//				Radius = 0.5f
//			};
//			ManagedPlayerAIbot.s_effectVolumeQuery = new EV_TargetData
//			{
//				lastModificationTime = 0f
//			};
//			ManagedPlayerAIbot.s_recognisedItemTypes = new List<uint>
//			{
//				139U, 144U, 117U, 115U, 114U, 130U, 167U, 116U, 102U, 101U, 127U, 132U
//			};
//			ManagedPlayerAIbot.s_sleeperCheckResetDistance = 8f;
//			ManagedPlayerAIbot.s_sleeperCheckResetDistanceSQ = 64f;
//			ManagedPlayerAIbot.s_sleeperCheckMaxDistance = 25f;
//			ManagedPlayerAIbot.s_sleeperCheckMaxDistanceSQ = 625f;
//			ManagedPlayerAIbot.s_twitchingSleeperCheckDistance = 10f;
//			ManagedPlayerAIbot.s_twitchingSleeperCheckDistanceSQ = 100f;
//			ManagedPlayerAIbot.s_sleeperCheckIntervalNeg = 10f;
//			ManagedPlayerAIbot.s_sleeperCheckIntervalPos = 1.5f;
//			ManagedPlayerAIbot.s_playerInSightMinCos = 0.9f;
//			ManagedPlayerAIbot.s_playerInSightMaxDistance = 15f;
//			ManagedPlayerAIbot.s_actionFailRegistryUpdateInterval = new float[]
//			{
//				1f,
//				2f
//			};
//			ManagedPlayerAIbot.s_chatterGlobalTimeout = 0f;
//			ManagedPlayerAIbot.s_chatterGlobalTimeoutDelay = 8f;
//			ManagedPlayerAIbot.s_tmpActionsCpy = new PlayerBotActionBase[10];
//			ManagedPlayerAIbot.s_tmpQueuedActionsCpy = new PlayerBotActionBase.Descriptor[10];
//			ManagedPlayerAIbot.s_updatingAction = null;
//		}

//		public void Setup(PlayerAgent agent, RootPlayerBotAction.Descriptor rootAction)
//		{
//			this.m_playerAgent = agent;
//			if (SNet.IsMaster)
//			{
//				this.AimFromAlign = agent.transform.FindChildRecursive("Head", true);
//				SyncAimFromAlignToComponent();
//				_component.enabled = false;
//				this.m_idleChatterCheckList.Add(true);
//				this.m_idleChatterCheckList.Add(true);
//			}
//			this.m_rootAction = rootAction;
//			if (this.m_rootAction != null)
//			{
//				this.StartAction(this.m_rootAction);
//			}
//			this.m_leaderPacket = this.m_playerAgent.GetReplicator().CreatePacket<pLeaderStruct>(new Action<pLeaderStruct>(ManagedPlayerAIbot.SyncLeader), null);
//			this.m_deployedItemPacket = this.m_playerAgent.GetReplicator().CreatePacket<pDeployedItemStruct>(new Action<pDeployedItemStruct>(this.SyncDeployedItem), null);
//		}

//		public void Update()
//		{
//			this.UpdateFailureEntryExpiry();
//			this.CheckLoadCaptureData();
//			this.InitValues();
//			this.SleeperCheck();
//			this.UpdateActions();
//			this.StartQueuedActions();
//			this.ApplyValues();
//		}

//		private void UpdateActions()
//		{
//			if (this.m_actions.Count == 0)
//			{
//				return;
//			}
//			int count = this.m_actions.Count;
//			if (ManagedPlayerAIbot.s_tmpActionsCpy.Length < count)
//			{
//				ManagedPlayerAIbot.s_tmpActionsCpy = new PlayerBotActionBase[(int)((float)count * 1.2f)];
//			}
//			this.m_actions.CopyTo(ManagedPlayerAIbot.s_tmpActionsCpy);
//			for (int i = 0; i < count; i++)
//			{
//				PlayerBotActionBase playerBotActionBase = ManagedPlayerAIbot.s_tmpActionsCpy[i];
//				ManagedPlayerAIbot.s_updatingAction = playerBotActionBase.DescBase;
//				if (!playerBotActionBase.IsActive() || playerBotActionBase.Update())
//				{
//					if (playerBotActionBase.DescBase.ActionBase == playerBotActionBase)
//					{
//						this.DisposeOfTerminatedAction(playerBotActionBase, i);
//					}
//				}
//			}
//			ManagedPlayerAIbot.s_updatingAction = null;
//		}

//		private void DisposeOfTerminatedAction(PlayerBotActionBase action, int searchIndex)
//		{
//			if (searchIndex < this.m_actions.Count && this.m_actions[searchIndex] == action)
//			{
//				this.m_actions.RemoveAt(searchIndex);
//				if (action != null && action.DescBase != null)
//				{
//					action.DescBase.OnExpired();
//				}
//				action.Stop();
//				return;
//			}
//			for (int i = 0; i < this.m_actions.Count; i++)
//			{
//				if (this.m_actions[i] == action)
//				{
//					this.m_actions.RemoveAt(i);
//					if (action != null && action.DescBase != null)
//					{
//						action.DescBase.OnExpired();
//					}
//					action.Stop();
//					return;
//				}
//			}
//			if (action != null)
//			{
//				action.Stop();
//			}
//		}

//		private void StartQueuedActions()
//		{
//			if (this.m_queuedActions.Count == 0)
//			{
//				return;
//			}
//			int count = this.m_queuedActions.Count;
//			if (ManagedPlayerAIbot.s_tmpQueuedActionsCpy.Length < count)
//			{
//				ManagedPlayerAIbot.s_tmpQueuedActionsCpy = new PlayerBotActionBase.Descriptor[(int)((float)count * 1.2f)];
//			}
//			this.m_queuedActions.CopyTo(ManagedPlayerAIbot.s_tmpQueuedActionsCpy);
//			this.m_queuedActions.Clear();
//			for (int i = 0; i < count; i++)
//			{
//				PlayerBotActionBase.Descriptor descriptor = ManagedPlayerAIbot.s_tmpQueuedActionsCpy[i];
//				if (descriptor.Status == PlayerBotActionBase.Descriptor.StatusType.Queued)
//				{
//					descriptor.OnStarted();
//					PlayerBotActionBase playerBotActionBase = descriptor.CreateAction();
//					this.RemoveCollidingActions(descriptor);
//					this.m_actions.Add(playerBotActionBase);
//				}
//			}
//		}

//		private void RemoveCollidingActions(PlayerBotActionBase.Descriptor desc)
//		{
//			bool flag;
//			do
//			{
//				flag = false;
//				int i = 0;
//				while (i < this.m_queuedActions.Count)
//				{
//					PlayerBotActionBase.Descriptor descriptor = this.m_queuedActions[i];
//					if (descriptor.Status == PlayerBotActionBase.Descriptor.StatusType.Queued && descriptor.CheckCollision(desc))
//					{
//						this.m_queuedActions.RemoveAt(i);
//						descriptor.OnAborted();
//						flag = true;
//					}
//					else
//					{
//						i++;
//					}
//				}
//				int j = 0;
//				while (j < this.m_actions.Count)
//				{
//					PlayerBotActionBase playerBotActionBase = this.m_actions[j];
//					if (playerBotActionBase.CheckCollision(desc))
//					{
//						if (playerBotActionBase.DescBase == ManagedPlayerAIbot.s_updatingAction)
//						{
//							Debug.LogError("Action was interrupted during its update: " + playerBotActionBase.DescBase);
//						}
//						this.m_actions.RemoveAt(j);
//						playerBotActionBase.DescBase.OnInterrupted();
//						playerBotActionBase.Stop();
//						flag = true;
//					}
//					else
//					{
//						j++;
//					}
//				}
//			}
//			while (flag && !desc.IsTerminated());
//		}

//		public bool IsActionAllowed(PlayerBotActionBase.Descriptor desc)
//		{
//			return !this.IsActionForbidden(desc);
//		}

//		public bool IsActionForbidden(PlayerBotActionBase.Descriptor desc)
//		{
//			for (int i = 0; i < this.m_queuedActions.Count; i++)
//			{
//				if (!this.m_queuedActions[i].IsActionAllowed(desc))
//				{
//					return true;
//				}
//			}
//			for (int j = 0; j < this.m_actions.Count; j++)
//			{
//				if (!this.m_actions[j].IsActionAllowed(desc))
//				{
//					return true;
//				}
//			}
//			return false;
//		}

//		public bool RequestAction(PlayerBotActionBase.Descriptor desc)
//		{
//			if (this.IsActionForbidden(desc))
//			{
//				return false;
//			}
//			this.StartAction(desc);
//			return true;
//		}

//		public void StartAction(PlayerBotActionBase.Descriptor desc)
//		{
//			if (!desc.IsTerminated())
//			{
//				Debug.LogError("Action was queued while active: " + desc);
//			}
//			for (int i = 0; i < this.m_actions.Count; i++)
//			{
//				if (this.m_actions[i].DescBase == desc)
//				{
//					this.m_actions.RemoveAt(i);
//					break;
//				}
//			}
//			desc.OnQueued();
//			this.RemoveCollidingActions(desc);
//			this.m_queuedActions.Add(desc);
//		}

//		public void StopAction(PlayerBotActionBase.Descriptor desc)
//		{
//			if (desc == ManagedPlayerAIbot.s_updatingAction)
//			{
//				Debug.LogError("Action was removed during its update: " + desc);
//			}
//			if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Queued)
//			{
//				desc.OnAborted();
//				for (int i = 0; i < this.m_queuedActions.Count; i++)
//				{
//					if (this.m_queuedActions[i] == desc)
//					{
//						this.m_queuedActions.RemoveAt(i);
//						return;
//					}
//				}
//				return;
//			}
//			if (desc.Status == PlayerBotActionBase.Descriptor.StatusType.Active)
//			{
//				if (desc.ActionBase == null)
//				{
//					Debug.LogError("Active descriptor is missing action: " + desc);
//				}
//				this.m_actions.Remove(desc.ActionBase);
//				desc.ActionBase.Stop();
//				desc.OnStopped();
//			}
//		}

//		public bool TestDangerZones(Vector3 position, AIDangerZone.SeverityEnum maxAllowedSeverity, out AIDangerZone.SeverityEnum severity)
//		{
//			severity = PlayerManager.Current.TestDangerZones(AIDangerZone.CharacterFilterEnum.Player, _component.gameObject, position, maxAllowedSeverity);
//			return severity <= maxAllowedSeverity;
//		}

//		public bool GroundRay(Vector3 fromPos, float height, int layerMask, out RaycastHit hitInfo)
//		{
//			return Physics.Raycast(new Ray
//			{
//				origin = fromPos,
//				direction = Vector3.down
//			}, out hitInfo, height, layerMask);
//		}

//		public bool CanSeePosition(Vector3 eyePos, Vector3 targetPos, int layerMask, out RaycastHit hitInfo)
//		{
//			Ray ray = new Ray
//			{
//				origin = eyePos
//			};
//			Vector3 vector = targetPos - eyePos;
//			float magnitude = vector.magnitude;
//			vector /= magnitude;
//			ray.direction = vector;
//			return !Physics.Raycast(ray, out hitInfo, magnitude, layerMask);
//		}

//		public bool HasLineOfFireToObject(Vector3 eyePos, GameObject targetObj)
//		{
//			RaycastHit raycastHit;
//			if (this.CanSeePosition(eyePos, targetObj.transform.position, LayerManager.MASK_BULLETWEAPON_RAY, out raycastHit))
//			{
//				return false;
//			}
//			return raycastHit.transform.gameObject == targetObj;
//		}

//		public bool CanSeeObject(Vector3 eyePos, GameObject targetObj)
//		{
//			Vector3 position = targetObj.transform.position;
//			RaycastHit raycastHit;
//			return this.CanSeePosition(eyePos, position, LayerManager.MASK_WORLD, out raycastHit) || !(raycastHit.transform.gameObject != targetObj) || raycastHit.transform.IsChildOf(targetObj.transform);
//		}

//		public void SetEnabled(bool state)
//		{
//			if (state == _component.enabled)
//			{
//				return;
//			}
//			if (state)
//			{
//				Vector3 position = this.m_playerAgent.Position;
//				Vector3 vector;
//				ManagedPlayerAIbot.SnapPositionToNav(position, out vector, 3f, 1f);
//				this.m_syncValues.Position = vector;
//				this.m_syncValues.Forward = this.m_playerAgent.Forward;
//				this.m_syncValues.LookDirection = this.m_playerAgent.TargetLookDir;
//				this.m_syncValues.Ladder = this.m_playerAgent.Locomotion.CurrentLadder;
//				this.InitValues();
//				this.m_lastSyncedPosition = this.m_syncValues.Position;
//				this.m_backpack = PlayerBackpackManager.GetBackpack(this.m_playerAgent.m_replicator.OwningPlayer);
//				this.m_inventory = (this.m_playerAgent.Inventory as PlayerInventorySynced);
//			}
//			else
//			{
//				bool flag;
//				do
//				{
//					flag = false;
//					for (int i = 0; i < this.m_queuedActions.Count; i++)
//					{
//						if (this.m_queuedActions[i] != this.m_rootAction)
//						{
//							this.m_queuedActions[i].OnAborted();
//							this.m_queuedActions.RemoveAt(i);
//							flag = true;
//						}
//					}
//					for (int j = 0; j < this.m_actions.Count; j++)
//					{
//						if (this.m_actions[j].DescBase != this.m_rootAction)
//						{
//							this.m_actions[j].Stop();
//							this.m_actions[j].DescBase.OnStopped();
//							this.m_actions.RemoveAt(j);
//							flag = true;
//						}
//					}
//				}
//				while (flag);
//			}
//			_component.enabled = state;
//		}

//		public bool WantsCrouch()
//		{
//			DRAMA_State currentStateEnum = DramaManager.CurrentStateEnum;
//			int num = (int)currentStateEnum;
//			if (num > 1 && num - 2 < 7 && this.m_hasSleeperNearby)
//			{
//				return !Mastermind.HasSurvivalWaveSituation;
//			}
//			return false;
//		}

//		public bool IsAllowedToMakeNoise()
//		{
//			switch (DramaManager.CurrentStateEnum)
//			{
//			case DRAMA_State.ElevatorIdle:
//			case DRAMA_State.ElevatorGoingDown:
//				return true;
//			case DRAMA_State.Encounter:
//			case DRAMA_State.Combat:
//			case DRAMA_State.Survival:
//			case DRAMA_State.IntentionalCombat:
//				if (this.m_hasSleeperNearby)
//				{
//					return Mastermind.HasSurvivalWaveSituation;
//				}
//				return true;
//			default:
//				return false;
//			}
//		}

//		public bool IsInCombat()
//		{
//			int num = (int)DramaManager.CurrentStateEnum;
//			return num - 5 < 4;
//		}

//		public bool HasTwitcherNearby()
//		{
//			return this.m_hasTwitcherNearby;
//		}

//		public bool HasHumanPlayerInSignt(float angleCos, float maxDistance)
//		{
//			Vector3 forward = this.m_playerAgent.Forward;
//			for (int i = 0; i < PlayerManager.PlayerAgentsInLevel.Count; i++)
//			{
//				PlayerAgent playerAgent = PlayerManager.PlayerAgentsInLevel[i];
//				if (playerAgent != null && !playerAgent.Owner.IsBot)
//				{
//					Vector3 vector = playerAgent.Position - this.m_playerAgent.Position;
//					float sqrMagnitude = vector.sqrMagnitude;
//					if (sqrMagnitude < maxDistance * maxDistance)
//					{
//						vector /= Mathf.Sqrt(sqrMagnitude);
//						if (Vector3.Dot(new Vector3(forward.x, 0f, forward.z), new Vector3(vector.x, 0f, vector.z)) > angleCos)
//						{
//							return true;
//						}
//					}
//				}
//			}
//			return false;
//		}

//		public bool IsFlashlightAllowed()
//		{
//			switch (DramaManager.CurrentStateEnum)
//			{
//			case DRAMA_State.Exploration:
//			case DRAMA_State.Encounter:
//			case DRAMA_State.Combat:
//			case DRAMA_State.Survival:
//				if (!this.m_hasSleeperNearby)
//				{
//					return !this.HasHumanPlayerInSignt(ManagedPlayerAIbot.s_playerInSightMinCos, ManagedPlayerAIbot.s_playerInSightMaxDistance);
//				}
//				break;
//			}
//			return false;
//		}

//		public void ChkScheduleChatter(PlayerAIBot.ChatterDelegateFunc func, PlayerAIBot.ChatterEnum chatterType)
//		{
//			if (!this.m_idleChatterCheckList[(int)chatterType])
//			{
//				return;
//			}
//			float time = Time.time;
//			if (ManagedPlayerAIbot.s_chatterGlobalTimeout <= time && time != ManagedPlayerAIbot.s_chatterGlobalTimeout)
//			{
//				this.m_idleChatterCheckList[(int)chatterType] = false;
//				ManagedPlayerAIbot.s_chatterGlobalTimeout = time + ManagedPlayerAIbot.s_chatterGlobalTimeoutDelay;
//                CoroutineManager.Callback(
//                    DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(
//                        new System.Action(() => func.Invoke(_component, chatterType))),
//                    UnityEngine.Random.Range(1.5f, 3f));
//            }
//		}

//		public void EnableChatter(PlayerAIBot.ChatterEnum chatterType)
//		{
//			if (!this.m_idleChatterCheckList[(int)chatterType])
//			{
//				this.m_idleChatterCheckList[(int)chatterType] = true;
//			}
//		}

//		public static bool KnowsHowToUseItem(uint itemID_gearCRC)
//		{
//			return ManagedPlayerAIbot.s_recognisedItemTypes.Contains(itemID_gearCRC);
//		}

//		public void AddDeployedItem(ItemEquippable item)
//		{
//			this.m_deployedItems.Add(item);
//			if (!SNet.IsMaster)
//			{
//				return;
//			}
//			pDeployedItemStruct pDeployedItemStruct = default(pDeployedItemStruct);
//			pDeployedItemStruct.BotAgent.Set(this.Agent);
//			pItemData item2 = item.Get_pItemData();
//			pDeployedItemStruct.Item = item2;
//			if (item.ReplicationWrapper != null)
//			{
//				pDeployedItemStruct.Item.replicatorRef.SetID(item.ReplicationWrapper.Replicator);
//			}
//			else
//			{
//				SentryGunInstance sentryGunInstance = item as SentryGunInstance;
//				if (sentryGunInstance != null)
//				{
//					pDeployedItemStruct.Item.replicatorRef.SetID(sentryGunInstance.Replicator);
//				}
//			}
//			this.m_deployedItemPacket.Send(pDeployedItemStruct, SNet_ChannelType.GameOrderCritical);
//		}

//		public List<ItemEquippable> GetDeployedItems()
//		{
//			int i = 0;
//			while (i < this.m_deployedItems.Count)
//			{
//				if (this.m_deployedItems[i] == null)
//				{
//					this.m_deployedItems.RemoveAt(i);
//				}
//				else
//				{
//					i++;
//				}
//			}
//			return this.m_deployedItems;
//		}

//		public void OnWarped(Vector3 position)
//		{
//			if (this.m_actions.Count > 0)
//			{
//				int count = this.m_actions.Count;
//				if (ManagedPlayerAIbot.s_tmpActionsCpy.Length < count)
//				{
//					ManagedPlayerAIbot.s_tmpActionsCpy = new PlayerBotActionBase[(int)((float)count * 1.2f)];
//				}
//				this.m_actions.CopyTo(ManagedPlayerAIbot.s_tmpActionsCpy);
//				for (int i = 0; i < count; i++)
//				{
//					PlayerBotActionBase playerBotActionBase = ManagedPlayerAIbot.s_tmpActionsCpy[i];
//					if (!playerBotActionBase.DescBase.IsTerminated())
//					{
//						playerBotActionBase.OnWarped(position);
//					}
//					if (playerBotActionBase.DescBase.IsTerminated() && playerBotActionBase.DescBase.ActionBase == playerBotActionBase)
//					{
//						this.DisposeOfTerminatedAction(playerBotActionBase, i);
//					}
//				}
//			}
//			this.m_syncValues.Position = position;
//			this.ApplyValues();
//			this.m_playerAgent.Locomotion.UpdateMovementFromSync(true);
//		}

//		public static bool SnapPositionToNav(Vector3 originalPosition, out Vector3 resultPosition, float max_radius, float nrSteps)
//		{
//			float num = 1f;
//			if (num <= nrSteps)
//			{
//				do
//				{
//					NavMeshHit navMeshHit;
//					if (NavMesh.SamplePosition(originalPosition, out navMeshHit, num / nrSteps * max_radius, -1))
//					{
//						resultPosition = navMeshHit.position;
//						return true;
//					}
//					num += 1f;
//				}
//				while (num <= nrSteps);
//			}
//			resultPosition = originalPosition;
//			return false;
//		}

//		public static bool SnapSegmentToNav(Vector3 fromPosition, Vector3 toPosition, out Vector3 resultPosition, float max_radius, float stepLength)
//		{
//			Vector3 vector = toPosition - fromPosition;
//			float magnitude = vector.magnitude;
//			float num = magnitude / stepLength;
//			if (num != Mathf.Floor(num))
//			{
//				num += 1f;
//			}
//			vector /= num;
//			Vector3 walkTarget = toPosition;
//			float stepIndex = 1f;
//			while (true)
//			{
//				if ((float)(int)num < stepIndex)
//				{
//					resultPosition = walkTarget;
//					return true;
//				}
//				walkTarget = fromPosition + vector * stepIndex;
//				if (!ManagedPlayerAIbot.SnapPositionToNav(walkTarget, out resultPosition, max_radius, 1f))
//				{
//					return false;
//				}
//				stepIndex += 1f;
//			}
//		}

//		public Vector3 FindPositionAroundCenter(Vector3 centerPos3D, float optimalRadius, float prio, bool useCurrentAngle)
//		{
//			Vector3 vector;
//			if (!ManagedPlayerAIbot.SnapPositionToNav(centerPos3D, out vector, 3f, 5f))
//			{
//				return centerPos3D;
//			}
//			centerPos3D = vector;
//			float num = optimalRadius * optimalRadius;
//			Vector2 vector2;
//			if (useCurrentAngle)
//			{
//				vector2 = new Vector2(this.m_playerAgent.Position.x - centerPos3D.x, this.m_playerAgent.Position.z - centerPos3D.z);
//				vector2.Normalize();
//			}
//			else
//			{
//				vector2 = default(Vector2);
//				vector2.x = UnityEngine.Random.Range(-1f, 1f);
//				vector2.y = (float)Math.Sqrt((double)(1f - vector2.x * vector2.x));
//				vector2.y *= ((UnityEngine.Random.Range(0f, 1f) > 0.5f) ? 1f : -1f);
//			}
//			vector2 *= optimalRadius;
//			Vector3 vector3 = new Vector3(vector2.x, 0f, vector2.y);
//			Vector3 vector4 = centerPos3D;
//			bool flag = false;
//			ManagedPlayerAIbot.s_testPosReservation.CharacterID = this.m_playerAgent.CharacterID;
//			ManagedPlayerAIbot.s_testPosReservation.Prio = prio;
//			for (float num2 = 0f; num2 < 5f; num2 += 1f)
//			{
//				if (num2 != 0f)
//				{
//					vector2 = MathUtil.RotateVector2(vector2, 72f);
//					vector3.Set(vector2.x, 0f, vector2.y);
//				}
//				Vector3 vector5 = centerPos3D + vector3;
//				if (!ManagedPlayerAIbot.SnapSegmentToNav(centerPos3D, vector5, out vector5, 3f, 0.5f))
//				{
//					continue;
//				}
//				NavMeshHit navMeshHit;
//				if (NavMesh.Raycast(centerPos3D, vector5, out navMeshHit, -1))
//				{
//					vector5 = navMeshHit.position;
//				}
//				float num3 = prio;
//				if (this.ApplyRestrictionsToRootPosition(ref vector5, ref num3) && (vector5 - centerPos3D).sqrMagnitude > num)
//				{
//					continue;
//				}
//				ManagedPlayerAIbot.s_testPosReservation.Position = vector5;
//				if (!PlayerManager.Current.IsPositionReserved(ManagedPlayerAIbot.s_testPosReservation))
//				{
//					return vector5;
//				}
//				if (!flag)
//				{
//					vector4 = vector5;
//					flag = true;
//				}
//			}
//			if (!flag)
//			{
//				return centerPos3D;
//			}
//			return vector4;
//		}

//		public bool ApplyRestrictionsToRootPosition(ref Vector3 testPosition, ref float restrictionPrio)
//		{
//			float resultPrio = restrictionPrio;
//			Vector3 resultPos = testPosition;
//			Vector3 tmpPos;
//			for (int i = 0; i < this.m_queuedActions.Count; i++)
//			{
//				tmpPos = testPosition;
//				PlayerBotActionBase.Descriptor descriptor = this.m_queuedActions[i];
//				if (descriptor.Prio > resultPrio && descriptor.ApplyPositionRestriction(ref tmpPos))
//				{
//					resultPrio = descriptor.Prio;
//					resultPos = tmpPos;
//				}
//			}
//			for (int j = 0; j < this.m_actions.Count; j++)
//			{
//				tmpPos = testPosition;
//				PlayerBotActionBase.Descriptor descriptor2 = this.m_actions[j].DescBase;
//				if (descriptor2.Prio > resultPrio && descriptor2.ApplyPositionRestriction(ref tmpPos))
//				{
//					resultPrio = descriptor2.Prio;
//					resultPos = tmpPos;
//				}
//			}
//			if (resultPrio > restrictionPrio)
//			{
//				restrictionPrio = resultPrio;
//				testPosition = resultPos;
//				return true;
//			}
//			return false;
//		}

//		public float IsPositionInfected(Vector3 testPosition)
//		{
//			ManagedPlayerAIbot.s_effectVolumeQuery.referencePosition = testPosition;
//			return EffectVolumeManager.QueryEffects(ManagedPlayerAIbot.s_effectVolumeQuery).infection;
//		}

//		private void SleeperCheck()
//		{
//			if (Time.time < this.m_nextSleeperCheckTime && (this.m_playerAgent.Position - this.m_lastSleeperCheckPosition).sqrMagnitude < ManagedPlayerAIbot.s_sleeperCheckResetDistanceSQ)
//			{
//				return;
//			}
//			bool flag = false;
//			bool flag2 = false;
//			List<EnemyAgent> list = new List<EnemyAgent>();
//			AIG_CourseNode.GetEnemiesInNodes(this.m_playerAgent.CourseNode, 2, list.ToIl2CppList());
//			for (int i = 0; i < list.Count; i++)
//			{
//				EnemyAgent enemyAgent = list[i];
//				bool flag3;
//				bool flag4;
//				if (enemyAgent.AI != null && enemyAgent.AI.IsHibernating(out flag3, out flag4) && !flag3)
//				{
//					AIG_CourseNode courseNode = enemyAgent.CourseNode;
//					if (courseNode != null && courseNode.m_playerCoverage.GetNodeDistanceToClosestPlayer_Unblocked() < 3)
//					{
//						float sqrMagnitude = (this.m_playerAgent.Position - enemyAgent.Position).sqrMagnitude;
//						if (sqrMagnitude < ManagedPlayerAIbot.s_sleeperCheckMaxDistanceSQ)
//						{
//							flag = true;
//							if (sqrMagnitude < ManagedPlayerAIbot.s_twitchingSleeperCheckDistanceSQ && enemyAgent.IsHibernationDetecting)
//							{
//								flag2 = true;
//								break;
//							}
//						}
//					}
//				}
//			}
//			this.m_hasSleeperNearby = flag;
//			this.m_hasTwitcherNearby = flag2;
//			this.m_nextSleeperCheckTime = Time.time + (this.m_hasSleeperNearby ? ManagedPlayerAIbot.s_sleeperCheckIntervalPos : ManagedPlayerAIbot.s_sleeperCheckIntervalNeg);
//			this.m_lastSleeperCheckPosition = this.m_playerAgent.Position;
//		}

//		public void OnDowned()
//		{
//			PlayerBotActionHurt.Descriptor descriptor = new PlayerBotActionHurt.Descriptor(this);
//			descriptor.Prio = 14f;
//			if (!this.IsActionForbidden(descriptor))
//			{
//				this.StartAction(descriptor);
//			}
//		}

//		public void OnInteractionStarted()
//		{
//			this.m_interactionCounter++;
//		}

//		public void OnInteractionFinished()
//		{
//			this.m_interactionCounter--;
//		}

//		public bool IsInteractedWith()
//		{
//			return this.m_interactionCounter > 0;
//		}

//		public void UpdateFailureEntryExpiry()
//		{
//			float time = Time.time;
//			if (time < this.m_nextActionFailureRegistryUpdateTime && time != this.m_nextActionFailureRegistryUpdateTime)
//			{
//				return;
//			}
//			foreach (List<PlayerBotActionBase.FailEntry> list in this.m_failEntries.Values)
//			{
//				int i = 0;
//				while (i < list.Count)
//				{
//					if (list[i].ExpiryTime <= time && time != list[i].ExpiryTime)
//					{
//						list.RemoveAt(i);
//					}
//					else
//					{
//						i++;
//					}
//				}
//			}
//			this.m_nextActionFailureRegistryUpdateTime = time + UnityEngine.Random.Range(ManagedPlayerAIbot.s_actionFailRegistryUpdateInterval[0], ManagedPlayerAIbot.s_actionFailRegistryUpdateInterval[1]);
//		}

//		public bool TestFailureRetry(PlayerBotActionBase.Descriptor descBase)
//		{
//			List<PlayerBotActionBase.FailEntry> list;
//			if (descBase == null || !this.m_failEntries.TryGetValue(descBase.GetIl2CppType(), out list) || list == null)
//			{
//				return true;
//			}
//			for (int i = 0; i < list.Count; i++)
//			{
//				if (!list[i].Test(descBase))
//				{
//					return false;
//				}
//			}
//			return true;
//		}

//		public void ReportFailure(PlayerBotActionBase.FailEntry entry)
//		{
//			if (entry == null)
//			{
//				return;
//			}
//			List<PlayerBotActionBase.FailEntry> list;
//			if (!this.m_failEntries.TryGetValue(entry.ActionType, out list) || list == null) // gettype may not work
//            {
//				list = new List<PlayerBotActionBase.FailEntry>();
//				this.m_failEntries.Add(entry.ActionType, list); // gettype may not work
//			}
//			list.Add(entry);
//		}

//		public void Capture(ref pBotCaptureData data)
//		{
//			int i = 0;
//			while (i < this.m_deployedItems.Count)
//			{
//				if (this.m_deployedItems[i] == null)
//				{
//					this.m_deployedItems.RemoveAt(i);
//				}
//				else
//				{
//					i++;
//				}
//			}
//			data.Leader.Set(this.m_syncValues.Leader);
//			int num = Mathf.Min(20, this.m_deployedItems.Count);
//			data.DeployedTools = new pItemData[20];
//			for (int j = 0; j < num; j++)
//			{
//				ItemEquippable itemEquippable = this.m_deployedItems[j];
//				pItemData pItemData = itemEquippable.Get_pItemData();
//				if (itemEquippable.ReplicationWrapper != null)
//				{
//					pItemData.replicatorRef.SetID(itemEquippable.ReplicationWrapper.Replicator);
//				}
//				else
//				{
//					SentryGunInstance sentryGunInstance = itemEquippable as SentryGunInstance;
//					if (sentryGunInstance != null)
//					{
//						pItemData.replicatorRef.SetID(sentryGunInstance.Replicator);
//					}
//				}
//				data.DeployedTools[j] = pItemData;
//			}
//		}

//		public void OnSpawned(ref pBotCaptureData data)
//		{
//			this.m_hasCaptureData = true;
//			this.m_captureData = data;
//			if (data.DeployedTools != null && data.DeployedTools.Length > 0)
//			{
//				this.m_captureData.DeployedTools = new pItemData[data.DeployedTools.Length];
//				data.DeployedTools.CopyTo(this.m_captureData.DeployedTools, 0);
//				return;
//			}
//			this.m_captureData.DeployedTools = null;
//		}

//		private void CheckLoadCaptureData()
//		{
//			if (!this.m_hasCaptureData)
//			{
//				return;
//			}
//			this.m_hasCaptureData = false;
//			PlayerAgent playerAgent;
//			if (this.m_captureData.Leader.TryGet(out playerAgent))
//			{
//				this.m_syncValues.Leader = playerAgent;
//				PlaceNavMarkerOnGO componentInChildren = _component.GetComponentInChildren<PlaceNavMarkerOnGO>();
//				if (componentInChildren != null)
//				{
//					componentInChildren.OnPlayerInfoUpdated(true);
//				}
//			}
//			if (this.m_captureData.DeployedTools != null && this.m_captureData.DeployedTools.Length > 0)
//			{
//				for (int i = 0; i < this.m_captureData.DeployedTools.Length; i++)
//				{
//					pItemData pItemData = this.m_captureData.DeployedTools[i];
//					SNetwork.IReplicator replicator;
//					if (pItemData.replicatorRef.TryGetID(out replicator))
//					{
//						ItemEquippable component = replicator.ReplicatorSupplier.gameObject.GetComponent<ItemEquippable>();
//						if (component != null)
//						{
//							this.m_deployedItems.Add(component);
//						}
//					}
//				}
//			}
//		}

//		private void InitValues()
//		{
//			this.m_syncValues.InitValues(this.m_playerAgent);
//		}

//		private void ApplyValues()
//		{
//			if (this.m_syncValues.IsLocomotionDirty || this.m_syncValues.IsFireDirty)
//			{
//				if (this.m_syncValues.IsFireDirty)
//				{
//					if (GlobalCallbacks.OnPlayerShotWeapon != null)
//					{
//                        GlobalCallbacks.OnPlayerShotWeapon?.Invoke(this.m_playerAgent);
//                    }
//					this.m_playerAgent.Sync.RegisterFiredBullets(1);
//				}
//				float num = 0f;
//				float num2 = 0f;
//				if (this.m_syncValues.Position != this.m_lastSyncedPosition)
//				{
//					Vector2 vector = new Vector2(this.m_syncValues.Position.x - _component.transform.position.x, this.m_syncValues.Position.z - _component.transform.position.z);
//					vector /= Time.deltaTime;
//					num = Vector2.Dot(new Vector2(this.m_syncValues.LookDirection.x, this.m_syncValues.LookDirection.z), vector);
//					num2 = Mathf.Sqrt(1f - num * num);
//				}
//				this.m_lastSyncedPosition = this.m_syncValues.Position;
//				this.m_playerAgent.Sync.SendLocomotion_Bot(this.m_syncValues.LocomotionState, this.m_syncValues.Position, this.m_syncValues.LookDirection, num, num2);
//			}
//			if (this.m_syncValues.IsLadderDirty)
//			{
//                this.m_playerAgent.Sync.SendClimbLadder_Bot(
//					this.m_syncValues.Ladder.SyncID,
//					this.m_syncValues.LadderRelPosition,
//					this.m_syncValues.ExitLadder);
//            }
//			if (this.m_syncValues.IsLeaderDirty)
//			{
//				this.m_syncValues.IsLeaderDirty = false;
//				pLeaderStruct pLeaderStruct = default(pLeaderStruct);
//				pLeaderStruct.BotAgent.Set(this.Agent);
//				pLeaderStruct.LeaderAgent.Set(this.m_syncValues.Leader);
//				this.m_leaderPacket.Send(pLeaderStruct, SNet_ChannelType.GameOrderCritical);
//				PlaceNavMarkerOnGO componentInChildren = _component.gameObject.GetComponentInChildren<PlaceNavMarkerOnGO>();
//				if (componentInChildren != null)
//				{
//					componentInChildren.OnPlayerInfoUpdated(true);
//				}
//			}
//		}

//		private static void SyncLeader(pLeaderStruct data)
//		{
//			if (SNet.IsMaster)
//			{
//				return;
//			}
//			PlayerAgent playerAgent;
//			PlayerAgent playerAgent2;
//			if (data.BotAgent.TryGet(out playerAgent) && data.LeaderAgent.TryGet(out playerAgent2))
//			{
//				PlayerAIBot bot = playerAgent.GetComponent<PlayerAIBot>();
//				if (bot == null)
//				{
//					return;
//				}
//				Instances.GetManaged(bot).SyncValues.Leader = playerAgent2;
//				PlaceNavMarkerOnGO componentInChildren = playerAgent.gameObject.GetComponentInChildren<PlaceNavMarkerOnGO>();
//				if (componentInChildren != null)
//				{
//					componentInChildren.OnPlayerInfoUpdated(true);
//				}
//			}
//		}

//		private void SyncDeployedItem(pDeployedItemStruct data)
//		{
//			if (SNet.IsMaster)
//			{
//				return;
//			}
//			PlayerAgent playerAgent;
//			if (!data.BotAgent.TryGet(out playerAgent))
//			{
//				return;
//			}
//			PlayerAIBot bot = playerAgent.GetComponent<PlayerAIBot>();
//			if (bot == null)
//			{
//				return;
//			}
//			ManagedPlayerAIbot managed = Instances.GetManaged(bot);
//			SNetwork.IReplicator replicator;
//			if (!data.Item.replicatorRef.TryGetID(out replicator))
//			{
//				return;
//			}
//			ItemEquippable component2 = replicator.ReplicatorSupplier.gameObject.GetComponent<ItemEquippable>();
//			if (component2 != null)
//			{
//				managed.AddDeployedItem(component2);
//			}
//		}

//		internal void PushSyncValuesToIl2Cpp()
//		{
//			PlayerAIBot.SyncTable il2cppTable = Instances.GetIl2CppSyncTable(_component);
//			if (il2cppTable == null)
//			{
//				return;
//			}

//			il2cppTable.LocomotionState = m_syncValues.LocomotionState;
//			il2cppTable.Position = m_syncValues.Position;
//			il2cppTable.Forward = m_syncValues.Forward;
//			il2cppTable.LookDirection = m_syncValues.LookDirection;
//			il2cppTable.LadderRelPosition = m_syncValues.LadderRelPosition;
//			il2cppTable.Ladder = m_syncValues.Ladder;
//			il2cppTable.ExitLadder = m_syncValues.ExitLadder;
//			il2cppTable.Shoot = m_syncValues.Shoot;
//			il2cppTable.Leader = m_syncValues.Leader;
//		}

//		private void SyncAimFromAlignToComponent()
//		{
//			HarmonyLib.Traverse.Create(_component).Field<Transform>("AimFromAlign").Value = AimFromAlign;
//		}

//		private SNet_Packet<pLeaderStruct> m_leaderPacket;
//		private SNet_Packet<pDeployedItemStruct> m_deployedItemPacket;
//		private static PlayerManager.PositionReservation s_testPosReservation;
//		private static EV_TargetData s_effectVolumeQuery;
//		private pBotCaptureData m_captureData;
//		private bool m_hasCaptureData;
//		private static List<uint> s_recognisedItemTypes;
//		private static float s_sleeperCheckResetDistance;
//		private static float s_sleeperCheckResetDistanceSQ;
//		private static float s_sleeperCheckMaxDistance;
//		private static float s_sleeperCheckMaxDistanceSQ;
//		private static float s_twitchingSleeperCheckDistance;
//		private static float s_twitchingSleeperCheckDistanceSQ;
//		private static float s_sleeperCheckIntervalNeg;
//		private static float s_sleeperCheckIntervalPos;
//		private static float s_playerInSightMinCos;
//		private static float s_playerInSightMaxDistance;
//		private static float[] s_actionFailRegistryUpdateInterval;
//		private static float s_chatterGlobalTimeout;
//		private static float s_chatterGlobalTimeoutDelay;
//		private static PlayerBotActionBase[] s_tmpActionsCpy;
//		private static PlayerBotActionBase.Descriptor[] s_tmpQueuedActionsCpy;
//		private PlayerBackpack m_backpack;
//		private PlayerInventorySynced m_inventory;
//		private List<PlayerBotActionBase> m_actions;
//		private List<PlayerBotActionBase.Descriptor> m_queuedActions;
//		private PlayerAgent m_playerAgent;
//		private PlayerBotActionBase.Descriptor m_rootAction;
//		private ManagedPlayerAIbot.SyncTable m_syncValues;
//		private Vector3 m_lastSyncedPosition;
//		private static PlayerBotActionBase.Descriptor s_updatingAction;
//		public Transform AimFromAlign;
//		private List<ItemEquippable> m_deployedItems;
//		private Vector3 m_lastSleeperCheckPosition;
//		private float m_nextSleeperCheckTime;
//		private bool m_hasSleeperNearby;
//		private bool m_hasTwitcherNearby;
//		private int m_interactionCounter;
//		private Dictionary<Il2CppSystem.Type, List<PlayerBotActionBase.FailEntry>> m_failEntries;
//		private float m_nextActionFailureRegistryUpdateTime;
//		private List<bool> m_idleChatterCheckList;

//		public class SyncTable
//		{
//			public bool IsLocomotionDirty
//			{
//				get
//				{
//					return this.isLocomotionDirty;
//				}
//			}

//			public bool IsLadderDirty
//			{
//				get
//				{
//					return this.isLadderDirty;
//				}
//			}

//			public bool IsFireDirty
//			{
//				get
//				{
//					return this.isFireDirty;
//				}
//			}

//			public PlayerLocomotion.PLOC_State LocomotionState
//			{
//				get
//				{
//					return this.m_locState;
//				}
//				set
//				{
//					this.m_locState = value;
//					this.isLocomotionDirty = true;
//				}
//			}

//			public Vector3 Position
//			{
//				get
//				{
//					return this.m_position;
//				}
//				set
//				{
//					this.m_position = value;
//					this.isLocomotionDirty = true;
//				}
//			}

//			public Vector3 Forward
//			{
//				get
//				{
//					return this.m_forward;
//				}
//				set
//				{
//					this.m_forward = value;
//					this.isLocomotionDirty = true;
//				}
//			}

//			public Vector3 LookDirection
//			{
//				get
//				{
//					return this.m_lookDirection;
//				}
//				set
//				{
//					this.m_lookDirection = value;
//					this.isLocomotionDirty = true;
//				}
//			}

//			public float LadderRelPosition
//			{
//				get
//				{
//					return this.m_ladderRelPosition;
//				}
//				set
//				{
//					this.m_ladderRelPosition = value;
//					this.isLadderDirty = true;
//				}
//			}

//			public LG_Ladder Ladder
//			{
//				get
//				{
//					return this.m_ladder;
//				}
//				set
//				{
//					this.m_ladder = value;
//					this.isLadderDirty = true;
//				}
//			}

//			public bool ExitLadder
//			{
//				get
//				{
//					return this.m_exitLadder;
//				}
//				set
//				{
//					this.m_exitLadder = value;
//					this.isLadderDirty = true;
//				}
//			}

//			public bool Shoot
//			{
//				get
//				{
//					return this.isFireDirty;
//				}
//				set
//				{
//					this.isFireDirty = value;
//				}
//			}

//			public PlayerAgent Leader
//			{
//				get
//				{
//					return this.m_leader;
//				}
//				set
//				{
//					if (this.m_leader != value)
//					{
//						this.m_leader = value;
//						this.IsLeaderDirty = true;
//					}
//				}
//			}

//			public void InitValues(PlayerAgent agent)
//			{
//				this.isLocomotionDirty = false;
//				this.isLadderDirty = false;
//				this.isFireDirty = false;
//				this.m_locState = (PlayerLocomotion.PLOC_State)agent.Locomotion.CurrentState.ENUM_ID;
//				this.m_exitLadder = false;
//			}

//			private bool isLocomotionDirty;
//			private bool isLadderDirty;
//			private bool isFireDirty;
//			private PlayerLocomotion.PLOC_State m_locState;
//			private Vector3 m_position;
//			private Vector3 m_forward;
//			private Vector3 m_lookDirection;
//			private float m_ladderRelPosition;
//			private LG_Ladder m_ladder;
//			private bool m_exitLadder;
//			private PlayerAgent m_leader;
//			public bool IsLeaderDirty;
//		}

//		private struct pLeaderStruct
//		{
//			public pPlayerAgent BotAgent;
//			public pPlayerAgent LeaderAgent;
//		}

//		private struct pDeployedItemStruct
//		{
//			public pPlayerAgent BotAgent;
//			public pItemData Item;
//		}
//	}
//}
    