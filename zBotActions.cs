using BotControl.CustomActions;
using BotControl.CustomActions.CustomActions;
using BotControl.Networking;
using BotControl.SmartSelect.PressActions;
using Enemies;
using FlexMethodDefinition;
using GTFO.API;
using LevelGeneration;
using Player;
using SlideMenu;
using SNetwork;
using System;
using UnityEngine;
using static BotControl.Networking.pStructs;

namespace BotControl
{
    public static class zBotActions
    {
        public static float defaultPrio = 14f;
        public static void StartAction(ManualAction manualAction)
        {
            if (!zActions.manualActions.ContainsKey(manualAction.Commander.CharacterID))
                zActions.manualActions[manualAction.Commander.CharacterID] = new();
            zActions.manualActions[manualAction.Commander.CharacterID].Add(manualAction);
            if (SNet.IsMaster)
                manualAction.Bot.StartAction(manualAction.ActionDescriptor);
        }
        public static void StartAction(PlayerAIBot aiBot, PlayerBotActionBase.Descriptor Desc, PlayerAgent Commander, uint ID)
        {
            ManualAction manualAction = new ManualAction(Desc, Commander, aiBot, ID);
            StartAction(manualAction);
        }

        public static PlayerBotActionBase.Descriptor TryGetDescriptor<Type>(PlayerAIBot Bot) where Type : PlayerBotActionBase.Descriptor
        {
            if (Bot == null) return null;
            foreach (PlayerBotActionBase action in Bot.Actions)
            {
                var desc = action.DescBase;
                if (desc == null)
                    continue;
                if (!zHelpers.IsOfType<Type>(desc.GetIl2CppType()))
                    continue;
                if (desc.IsTerminated())
                    continue;
                return desc;
            }
            
            return null;
        }
        public static void SetLeader(PlayerAgent Follower, PlayerAgent Leader, PlayerAgent Commander = null, ulong netsender = 0)
        {
            if (!Follower.Owner.IsBot)
                return;
            PlayerAIBot Bot = Follower.GetComponent<PlayerAIBot>();
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                {
                    Bot.Agent.NavMarker.UpdateExtraInfo();
                    return;
                }
                //request host
                pLeaderInfo info = new pLeaderInfo();
                info.leader = pStructs.Get_pStructFromRefrence(Leader);
                info.follower = pStructs.Get_pStructFromRefrence(Follower);
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                NetworkAPI.InvokeEvent<pLeaderInfo>("RequestToSetLeader", info);
                return;
            }
            Bot.SyncValues.Leader = Leader;
            Bot.Agent.NavMarker.UpdateExtraInfo();
        }
        public static void SendbotToMoveToLocation(PlayerAIBot aiBot,Vector3 TargetLocation, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (aiBot == null) return;
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            PlayerBotActionTravel.Descriptor Desc = new PlayerBotActionTravel.Descriptor(aiBot)
            {
                Prio = defaultPrio,
                Haste = 1,
                DestinationType = PlayerBotActionTravel.Descriptor.DestinationEnum.Position,
                DestinationPos = TargetLocation,
                Persistent = false,
                Bulletproof = PlayerBotActionTravel.Descriptor.BulletproofEnum.None,
            };
            StartAction(aiBot, Desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                //request host
                pLocationInfo info = new pLocationInfo();                                                                      
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.position = zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos;
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pLocationInfo>("RequestToMoveToLocation", info);
                return;
            }
            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_IWILLDOIT, "I will do it.", 2f);
            zBotActions.SetLeader(aiBot.Agent, aiBot.Agent, zStaticRefrences.LocalPlayer, 0);
            //Bot.SyncValues.Leader = Bot.Agent;
            aiBot.Agent.NavMarker.UpdateExtraInfo();
            //var desc = zBotActions.TryGetDescriptor<PlayerBotActionCarryExpeditionItem.Descriptor>(Bot);
            //if (desc == null)
            //    return;
            //desc.ActionBase.Cast<PlayerBotActionCarryExpeditionItem>().m_leaderProximityEnterTime = float.MaxValue; 
        }
        public static void SendBotToPickUpMine(PlayerAIBot aiBot, MineDeployerInstance Mine, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            PlayerBotActionGatherDeployables.Descriptor desc = new(aiBot)
            {
                Prio = defaultPrio,
                TargetItem = Mine,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pPickupMineInfo info = new pPickupMineInfo();                                                                             
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.MineReplicatorKey = Mine?.Replicator?.Key ?? 0;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pPickupMineInfo>("RequestToPickupMine", info);
                return;
            }
            if (Mine?.Owner != null && !Mine.Owner.Owner.IsBot) return;
            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);
        }
        public static void SendBotToPickUpSentry(PlayerAIBot aiBot, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            PlayerBotActionDeploySentryGun.Descriptor desc = new(aiBot)
            {
                Prio = defaultPrio,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pPickupSentryInfo info = new pPickupSentryInfo();                                                                            
                info.playerAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.commander = pStructs.Get_pStructFromRefrence(Commander); 
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pPickupSentryInfo>("RequestToPickupSentry", info);
                return;
            }

            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);
            // todo check if the sentry is even deployed first.
            // Though it should never get called if it's not deployed already.

        }
        public static void SendBotToPlaceSentry(PlayerAIBot aiBot, Pose sentryPose, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            PlayerBotActionDeploySentryGun.Descriptor desc = new(aiBot)
            {
                Prio = defaultPrio,
                InstallationPose = sentryPose
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pPlaceToolInfo info = new pPlaceToolInfo();                                                                             //info.item = pStructs.Get_pStructFromRefrence(item);
                info.playerAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.commander = pStructs.Get_pStructFromRefrence(Commander); //This might be a problem in commander is null?  Not sure. TODO look into it.
                info.Pose = sentryPose;
                info.ID  = actionID;
                NetworkAPI.InvokeEvent<pPlaceToolInfo>("RequestToPlaceSentry", info);
                return;
            }
            // todo check if the sentry is deployed first.
            // Though it should never get called if it's not deployed already.

            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);

        }
        public static void SendBotToPlaceMine(PlayerAIBot aiBot, Pose minePose, InventorySlot slot, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            PlayerBotActionDeployTripMine.Descriptor desc = new(aiBot)
            {
                Prio = defaultPrio,
                InstallationPose = minePose,
                BackpackItem = zHelpers.GetAgentBackpackItem(aiBot.Agent, slot)
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pPlaceMineInfo info = new pPlaceMineInfo();                                                                             //info.item = pStructs.Get_pStructFromRefrence(item);
                info.Agent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.Commander = pStructs.Get_pStructFromRefrence(Commander); //This might be a problem in commander is null?  Not sure. TODO look into it.
                info.pose = minePose;
                info.slot = slot;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pPlaceMineInfo>("RequestToPlaceMine", info);
                return;
            }
            // todo check if the sentry is deployed first.
            // Though it should never get called if it's not deployed already.

            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);

        }
        public static void SendBotToUseCfoamGun(PlayerAIBot aiBot, Vector3 targetPosition, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToUseCfoamGun{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            PlayerBotActionUseGlueGun.Descriptor desc = new(aiBot)
            {
                Prio = 15f,
                TargetType = PlayerBotActionUseGlueGun.TargetTypeEnum.Position,
                TargetObject = Commander.transform,
                TargetPosition = targetPosition,
                Haste = 1f,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pUseCfoamInfo info = new pUseCfoamInfo();                                                                             //info.item = pStructs.Get_pStructFromRefrence(item);
                info.Agent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.Commander = pStructs.Get_pStructFromRefrence(Commander); //This might be a problem in commander is null?  Not sure. TODO look into it.
                info.position = targetPosition;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pUseCfoamInfo>("RequestToUseCfoamGun", info);
                return;
            }
            // todo check if the sentry is deployed first.
            // Though it should never get called if it's not deployed already.

            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);
            
        }

        public static void SendBotToPickupItem(PlayerAIBot aiBot, ItemInLevel item, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToPickupItem{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            float prio = defaultPrio;
            float standDist = RootPlayerBotAction.s_collectItemStandDistance; // 1.0f
            var Container = item.GetComponentInParent<LG_ResourceContainer_Storage>();
            Vector3 standCandidate = item.transform.position;
            Vector3 TargetPosition;
            if (Container != null)
            {
                standCandidate = Container.transform.position - Container.transform.up * standDist;
                zHelpers.SnapPositionToNav(standCandidate, out TargetPosition);
            }
            else
            {
                zHelpers.TryGetStandoffPosition(aiBot.transform.position, item.transform.position, 1f, out TargetPosition);
            }

            float haste = 1f;
            PlayerBotActionCollectItem.Descriptor desc = new(aiBot)
            {
                TargetItem = item,
                TargetContainer = item.container,
                TargetPosition = TargetPosition,
                Prio = prio,
                Haste = haste,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                //request host
                pPickupItemInfo info = new pPickupItemInfo();
                info.item.replicatorRef.SetID(item.GetComponent<LG_PickupItem_Sync>().m_stateReplicator.Replicator);
                info.playerAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.commander = pStructs.Get_pStructFromRefrence(Commander);
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pPickupItemInfo>("RequestToPickupItem", info);
                return;
            }
            //Is this an item we should carry?
            var carrycore = item.gameObject.GetComponent<CarryItemPickup_Core>();
            if (carrycore != null)
            {
                SendBotToCarryItem(aiBot, carrycore, Commander, netsender);
                return;
            }
            ZiMain.log.LogInfo($"{Commander.PlayerName} is sending {aiBot.Agent.PlayerName} to pick up {item.PublicName}");

            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 2f);
            zChatHandler.sendChatMessage($"Picking up {item.PublicName}", PressActionManager.GetAction("Pickup Item").FriendlyIdentifier + IInputAction.chatPermSuffix, aiBot.Agent, Commander);
        }
        public static void SendBotToReviveAgent(PlayerAIBot Reviver, PlayerAgent Downed, PlayerAgent Commander = null, ulong netsender  = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToReviveAgent{Commander.PlayerName}{Reviver.Agent.PlayerName}{Time.time}");
            PlayerBotActionRevive.Descriptor desc = new(Reviver)
            {
                Client = Downed,
                Prio = 15,
            };
            StartAction(Reviver, desc, Commander, actionID);
            if (!SNet.IsMaster)
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pReviveAgentInfo info = new pReviveAgentInfo();
                info.Revier = pStructs.Get_pStructFromRefrence(Reviver.Agent);
                info.Downed = pStructs.Get_pStructFromRefrence(Downed);
                info.commander = pStructs.Get_pStructFromRefrence(Commander);
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pReviveAgentInfo>("RequestToReviveAgent", info);
                return;
            }
            zChatHandler.sendChatMessage($"Reving {Downed.PlayerName}", PressActionManager.GetAction("Revive Agent").FriendlyIdentifier + IInputAction.chatPermSuffix, Reviver.Agent, Commander);

            ZiMain.BotBarkBack(Reviver.Agent.CharacterID, AK.EVENTS.PLAY_CL_IWILLDOIT, "I will do it.", 1f);
            //ZiMain.sendChatMessage($"I would have revived {downedAgent.PlayerName}, but I'm stupid.", aiBot.Agent, commander);
            //todo
        }
        public static void SendBotToCarryItem(PlayerAIBot aiBot, CarryItemPickup_Core item, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            float prio = 11;
            PlayerBotActionCarryExpeditionItem.Descriptor desc = new(aiBot)
            {
                TargetItem = item,
                Prio = prio,
            };
            StartAction(aiBot, desc, Commander, actionID);
            //TODO split this up into it's own netaction instead of piggybacking on sendbottopickupitem.
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                //request host
                pPickupItemInfo info = new pPickupItemInfo();
                info.item.replicatorRef.SetID(item.GetComponent<LG_PickupItem_Sync>().m_stateReplicator.Replicator); 
                info.playerAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.commander = pStructs.Get_pStructFromRefrence(Commander); 
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pPickupItemInfo>("RequestToPickupItem", info);
                return;
            }
            ZiMain.log.LogInfo($"{Commander.PlayerName} is sending {aiBot.Agent.PlayerName} to carry {item._PublicName_k__BackingField} with the new method");

            zChatHandler.sendChatMessage($"Carrying {item._PublicName_k__BackingField}", PressActionManager.GetAction("Pickup Item").FriendlyIdentifier + IInputAction.chatPermSuffix, aiBot.Agent, Commander);
            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);

        }
        public static void SendBotToShareResourcePack(PlayerAIBot aiBot, PlayerAgent receiver, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            //todo add to manual action list for refrence later.
            float prio = defaultPrio;
            float haste = 1f;
            BackpackItem backpackItem = null;
            ZiMain.log.LogInfo($"{aiBot.Agent.PlayerName} was told by {Commander?.PlayerName ?? "someone"} with netid {netsender} to try to share resource pack to {receiver.PlayerName}");
            //var gotBackpackItem = aiBot.Backpack.HasBackpackItem(InventorySlot.ResourcePack) &&
            //                      aiBot.Backpack.TryGetBackpackItem(InventorySlot.ResourcePack, out backpackItem);
            bool gotBackpackItem = zHelpers.TryGetAgentBackpackItem(aiBot.Agent, InventorySlot.ResourcePack, out backpackItem);
            if (!gotBackpackItem)
                return;
            ItemEquippable resourcePack = backpackItem.Instance.Cast<ItemEquippable>();

            PlayerBotActionShareResourcePack.Descriptor desc = new(aiBot)
            {
                Receiver = receiver,
                Item = resourcePack,
                Prio = prio,
                Haste = haste,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster)//Are we a client?
            {
                if (netsender != 0)//Is this request coming from a different client?
                    return;
                //request host
                pStructs.pShareResourceInfo info = new pStructs.pShareResourceInfo();
                info.sender = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.receiver = pStructs.Get_pStructFromRefrence(receiver);
                info.commander = pStructs.Get_pStructFromRefrence(Commander);
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pStructs.pShareResourceInfo>("RequestToShareResourcePack", info);
                return;
            }
            aiBot.Inventory.DoEquipItem(resourcePack);//is this needed?  Does the action not handle this?
            float ammoLeft = aiBot.Backpack.AmmoStorage.GetAmmoInPack(AmmoType.ResourcePackRel);
            zChatHandler.sendChatMessage($"Sharing my {resourcePack.PublicName} ({ammoLeft}%) with {receiver.PlayerName}.", PressActionManager.GetAction("Share Resource").FriendlyIdentifier + IInputAction.chatPermSuffix, aiBot.Agent, Commander);
            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);

        }
        [Obsolete]
        public static void SendBotToClearCurrentRoom(PlayerAIBot aiBot = null, PlayerAgent commander = null, ulong netsender = 0, PlayerBotActionBase.Descriptor arg_descriptor = null)
        {

            if (arg_descriptor != null && arg_descriptor.Status != PlayerBotActionBase.Descriptor.StatusType.Successful)
            {
                ZiMain.log.LogInfo($"Unsucsefull last kill {arg_descriptor.Status}");
                return;
            }
            if (commander == null)
                commander = PlayerManager.GetLocalPlayerAgent();
            var allEnemies = commander.CourseNode.m_enemiesInNode;
            if (aiBot == null)
            {
                PlayerAgent closestBot = null;
                float closestBotDistnace = float.MaxValue;
                foreach (var botCandidate in PlayerManager.PlayerAgentsInLevel)
                {
                    if (!botCandidate.Owner.IsBot)
                        continue;
                    float distanceToBot = (commander.gameObject.transform.position - botCandidate.gameObject.transform.position).sqrMagnitude;
                    if (distanceToBot < closestBotDistnace)
                    {
                        closestBotDistnace = distanceToBot;
                        closestBot = botCandidate;
                    }
                }
                if (closestBot == null)
                    return;
                aiBot = closestBot.gameObject.GetComponent<PlayerAIBot>();
            }
            if (allEnemies.Count <= 0)
            {
                zChatHandler.sendChatMessage("I have killed all enemies in the room", "Clear Room" + "TalkInChatNotifyActionSuccess", aiBot.gameObject.GetComponent<PlayerAgent>(), commander);
                return;
            }

            EnemyAgent closestEnemy = null;
            float closestEnemyDistnace = float.MaxValue;
            foreach (var enemy in allEnemies)
            {
                float distanceToEnemy = (aiBot.gameObject.transform.position - enemy.gameObject.transform.position).sqrMagnitude;
                if (distanceToEnemy < closestEnemyDistnace)
                {
                    closestEnemyDistnace = distanceToEnemy;
                    closestEnemy = enemy;
                }
            }
            //var descriptor = SendBotToKillEnemy(aiBot, closestEnemy, commander);
            FlexibleMethodDefinition callback = new(SendBotToClearCurrentRoom, [aiBot, commander, netsender]);
            //zActionSub.addOnTerminated(descriptor, callback);
        }
        public static bool SendBotToThrowItem(PlayerAgent Commander, PlayerAgent botAgent, Vector3 MovePosition, Vector3 TargetPosition, ulong netSender = 0, uint actionID = 0)
        {
            // TODO Alow you to supply a target object, or target position.
            // If you supply a target poisition, then move position will be set to commanders location.
            // TODO low priority add option to revert back to old system where they throw as soon as they can see the target.
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{botAgent.PlayerName}{Time.time}");
            PlayerAIBot aiBot = botAgent.GetComponent<PlayerAIBot>();
            zHelpers.TryGetAgentBackpackItem(aiBot.Agent, InventorySlot.Consumable, out var item);
            if (item == null)
            {
                ZiMain.log.LogWarning($"Wanted to throw an item but found nothing.");
                return false;
            }
            PlayerBotActionThrowItem.Descriptor desc = new(aiBot)
            {
                Prio = defaultPrio,
                Haste = 0.8f,
                TargetPosition = TargetPosition,
                TargetObject = Commander.transform,
                TargetType = PlayerBotActionThrowItem.TargetTypeEnum.Position,
                Item = item.Instance.Cast<ItemEquippable>(),
                MovementAllowed = true
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster)
            {
                if (netSender != 0) //Is this request coming from a different client?
                    return false;
                pThrowDataInfo info = new pThrowDataInfo();
                //info.ThrowType = pThrowType.cFoam;
                info.Agent = pStructs.Get_pStructFromRefrence(botAgent);
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.TargetPosition = TargetPosition;
                info.MovePosition = MovePosition;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pThrowDataInfo>("RequestToThrowItem", info);
                return false;
            }


            //if (item.Name != ThrowItemPatch.ThrowMappings[ThrowType])
            //{
            //    ZiMain.log.LogWarning($"Invalid throw item to throw.  Wanted to throw {ThrowType} but found {item.Name}");
            //    return false;
            //}


            ZiMain.BotBarkBack(botAgent.CharacterID, AK.EVENTS.PLAY_CL_IWILLDOIT, "I will do it.", 1f);
            return false;
        }
        internal static void SendbotToBreakLock(PlayerAIBot aiBot, LG_WeakLock Lock, PlayerBotActionUnlock.Descriptor.MethodEnum method, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToMoveToLocation{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            float Prop = defaultPrio;
            iLG_WeakLockHolder holder = Lock.m_holder;
            LG_DoorButton Button = holder.TryCast<LG_DoorButton>();
            LG_WeakResourceContainer container = holder.TryCast<LG_WeakResourceContainer>();
            PlayerBotActionUnlock.Descriptor.TargetTypeEnum targetType;
            GameObject targetObject;
            if (Button != null)
            {
                LG_WeakDoor door = Button.m_door.TryCast<LG_WeakDoor>();
                targetObject = door.gameObject;
                targetType = PlayerBotActionUnlock.Descriptor.TargetTypeEnum.Door;
            }
            else if (container != null)
            {
                targetObject = container.gameObject;
                targetType = PlayerBotActionUnlock.Descriptor.TargetTypeEnum.Container;
            }
            else
                return;
            PlayerBotActionUnlock.Descriptor Desc = new(aiBot)
            {
                TargetType = targetType,
                TargetGO = targetObject,
                Prio = 13,
                TargetPosition = targetObject.transform.position,
                Method = method,
                Lock = Lock,
            };
            StartAction(aiBot, Desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                //request host

                pBreakLockInfo info = new pBreakLockInfo();
                iSNet_StateReplicator stateReplicator = Lock.GetStateReplicator();
                SNet_StateReplicator<pWeakLockState, pWeakLockInteraction> Replicator = stateReplicator.TryCast<SNet_StateReplicator<pWeakLockState, pWeakLockInteraction>>();
                info.Lock = Replicator.GetProviderSyncStruct();
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.ID = actionID;
                info.Method = method;
                NetworkAPI.InvokeEvent<pBreakLockInfo>("RequestToBreakLock", info);
            }



            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_IMONMYWAY, "On my way.", 1f);
        }
        internal static void CancelBotAction(uint actionID, ulong netsender = 0)
        {
            if (!SNet.IsMaster)
            {
                if (netsender != 0) return;
                pActionTerminatedInfo info = new()
                {
                    ID = actionID,
                };
                NetworkAPI.InvokeEvent<pActionTerminatedInfo>("RequestActionCancel", info);
                return;
            }
            bool found = false;
            foreach (var key in zActions.manualActions.Keys)
            {
                foreach (ManualAction mAction in zActions.manualActions[key])
                {
                    if (actionID == mAction.ID)
                    {
                        mAction.Bot.StopAction(mAction.ActionDescriptor);
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
        }
        public static void SendBotToAttackSleeper(PlayerAIBot aiBot, EnemyAgent Enemy, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToAttackSleeper{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            CustomBotActionManualAttack.Descriptor desc = new CustomBotActionManualAttack.Descriptor(aiBot)
            {
                TargetAgent = Enemy,
                Haste = 0.5f,
                Prio = defaultPrio
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pAttackEnemyInfo info = new pAttackEnemyInfo();
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.Enemy = pStructs.Get_pStructFromRefrence(Enemy);
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pAttackEnemyInfo>("RequestToAttackSleeper", info);
                return;
            }

            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);
        }
        public static void SendBotToDropHere(PlayerAIBot aiBot, Vector3 DropPosition, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToDropHere{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            CustomBotActionDropHere.Descriptor desc = new CustomBotActionDropHere.Descriptor(aiBot)
            {
                DropPosition = zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos,
                Haste = 1f,
                Prio = defaultPrio,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pLocationInfo info = new pLocationInfo();
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.position = DropPosition;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pLocationInfo>("RequestToDropHere", info);
                return;
            }
            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);
        }
        public static void SendBotToInsertCell(PlayerAIBot aiBot, LG_PowerGenerator_Core Generator, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            var m_powerCellInteractionObject = Generator?.m_powerCellInteraction?.Cast<LG_GenericCarryItemInteractionTarget>()?.gameObject; // TODO make the bot walk up to the generator THEN say they can't do it.
            if (m_powerCellInteractionObject == null || !m_powerCellInteractionObject.activeInHierarchy)
                ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_ICANTDOTHAT, "I can't do that.", 1f);
            else
                ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToInsertCell{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            CustomBotActionInsertPowerCell.Descriptor desc = new CustomBotActionInsertPowerCell.Descriptor(aiBot)
            {
                TargetGenerator = Generator,
                Haste = 1f,
                Prio = defaultPrio,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pLocationInfo info = new pLocationInfo();
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.position = Generator.transform.position;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pLocationInfo>("RequestToInsertCell", info);
                return;
            }
        }
        public static void SendBotToOpenContainer(PlayerAIBot aiBot, LG_WeakResourceContainer Container, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToOpenContainer{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            CustomBotActionOpenContainer.Descriptor desc = new CustomBotActionOpenContainer.Descriptor(aiBot)
            {
                TargetContainer = Container,
                Haste = 1f,
                Prio = defaultPrio,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pLocationInfo info = new pLocationInfo();
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.position = Container.transform.position;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pLocationInfo>("RequestToOpenContainer", info);
                return;
            }
            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_OPENING, "Opening.", 1f);
        }
        public static void SendBotToRefillSentry(PlayerAIBot aiBot, SentryGunInstance Sentry, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToRefillSentry{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            CustomBotActionRefillSentry.Descriptor desc = new CustomBotActionRefillSentry.Descriptor(aiBot)
            {
                TargetSentry = Sentry,
                Haste = 1f,
                Prio = defaultPrio,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pLocationInfo info = new pLocationInfo();
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.position = Sentry.transform.position;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pLocationInfo>("RequestToRefillSentry", info);
                return;
            }
            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);
        }
        public static void SendBotToInteractDoor(PlayerAIBot aiBot, LG_WeakDoor Door, Vector3 TargetPosition, PlayerBotActionUnlock.Descriptor.MethodEnum method, PlayerAgent Commander = null, ulong netsender = 0, uint actionID = 0)
        {
            if (actionID == 0)
                actionID = zHelpers.HashString($"RequestToInteractDoor{Commander.PlayerName}{aiBot.Agent.PlayerName}{Time.time}");
            CustomBotActionInteractDoor.Descriptor desc = new CustomBotActionInteractDoor.Descriptor(aiBot)
            {
                TargetDoor = Door,
                method = method,
                TargetPosition = TargetPosition,
                Prio = defaultPrio,
            };
            StartAction(aiBot, desc, Commander, actionID);
            if (!SNet.IsMaster) //Are we a client?
            {
                if (netsender != 0) //Is this request coming from a different client?
                    return;
                pDoorInteractInfo info = new pDoorInteractInfo();
                info.Commander = pStructs.Get_pStructFromRefrence(Commander);
                info.BotAgent = pStructs.Get_pStructFromRefrence(aiBot.Agent);
                info.DoorPosition = Door.transform.position;
                info.TargetPosition = TargetPosition;
                info.Method = method;
                info.ID = actionID;
                NetworkAPI.InvokeEvent<pDoorInteractInfo>("RequestToInteractDoor", info);
                return;
            }
            ZiMain.BotBarkBack(aiBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 1f);
        }
    }
}
