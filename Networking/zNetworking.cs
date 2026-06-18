using BotControl.CustomActions;
using Enemies;
using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using PlayFab.ClientModels;
using SlideDrum;
using SNetwork;
using System;
using UnityEngine;
using static Player.PlayerBotActionUnlock.Descriptor;

namespace BotControl.Networking
{
    public class zNetworking
    { //This class will handle all incoming and outgoing network requests.
      //todo only Update values every 100ms.  
      //if not host ask for host's value after change
        internal static void ReciveSetBoolOverideTree(ulong netSender, pStructs.pBoolOverideTreeInfo info)
        {
            ZiMain.log.LogDebug("Recived request to update bool override tree!");
            ZiMain.log.LogDebug($"treeID:{info.treeID}, keyId:{info.keyId}, isNull:{info.isNull}, value:{info.value}");
            uint treeID = info.treeID;
            uint keyId = info.keyId;
            bool isNull = info.isNull;
            bool? value;
            if (isNull)
                value = null;
            else
                value = info.value;
            OverrideTree<bool?> tree = OverrideTree<bool?>.GetTreeFromID(info.treeID);
            tree.SetValue(keyId, value, netSender);
        }
        internal static void ReciveSetIntOverideTree(ulong netSender, pStructs.pIntOverideTreeInfo info)
        {
            ZiMain.log.LogDebug("Recived request to update int override tree!");
            ZiMain.log.LogDebug($"treeID:{info.treeID}, keyId:{info.keyId}, isNull:{info.isNull}, value:{info.value}");
            uint treeID = info.treeID;
            uint keyId = info.keyId;
            bool isNull = info.isNull;
            int? value;
            if (isNull)
                value = null;
            else
                value = info.value;
            OverrideTree<int?> tree = OverrideTree<int?>.GetTreeFromID(info.treeID);
            tree.SetValue(keyId, value, netSender);
        }
        internal static void ReciveSetFloatOverideTree(ulong netSender, pStructs.pFloatOverideTreeInfo info)
        {
            ZiMain.log.LogDebug("Recived request to update float override tree!");
            ZiMain.log.LogDebug($"treeID:{info.treeID}, keyId:{info.keyId}, isNull:{info.isNull}, value:{info.value}");
            uint treeID = info.treeID;
            uint keyId = info.keyId;
            bool isNull = info.isNull;
            float? value;
            if (isNull)
                value = null;
            else
                value = info.value;
            OverrideTree<float?> tree = OverrideTree<float?>.GetTreeFromID(info.treeID);
            tree.SetValue(keyId, value, netSender);
        }
        internal static void ReciveRequestToMoveToLocation(ulong netsender,  pStructs.pLocationInfo info)
        {
            ZiMain.log.LogInfo("Recived request to move!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent bot = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            PlayerAgent commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            Vector3 position = info.position;
            if (bot == null)
            {
                ZiMain.log.LogError("Invalid request to move: bot agent is null.");
                return;
            }
            if (commander == null)
            {
                ZiMain.log.LogError("Invalid request to move: Commander is null.");
                return;
            }
            ZiMain.log.LogInfo($"{commander.PlayerName} wants to tell {bot.PlayerName} to move to {position}");
            if (!bot.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to move, You can't tell a player what to do.");
                return;
            }
            PlayerAIBot aiBot = bot.GetComponent<PlayerAIBot>();
            if (aiBot == null) return;
            zBotActions.SendbotToMoveToLocation(aiBot, position, commander, netsender, info.ID);
        }
        internal static void ReciveRequestToPickupMine(ulong sender, pStructs.pPickupMineInfo info)
        {
            ZiMain.log.LogInfo("Recived request to pickup mine!");
            if (!SNet.IsMaster)
                return;
            //Component Comp = zSearch.FindNearest(info.MineCords, Il2CppType.Of<MineDeployerInstance>(), 0.1f);
            //if (Comp == null) return;
            //MineDeployerInstance Mine = Comp.GetComponent<MineDeployerInstance>();
            PlayerAgent agent;
            PlayerAgent commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            MineDeployerInstance Mine = null;
            if (info.MineReplicatorKey != 0)
            {
                SNet_Replication.TryGetReplicator(info.MineReplicatorKey, out var replicator);
                var Supplier = replicator.ReplicatorSupplier;
                GameObject Gobject = Supplier.gameObject;
                Mine = Gobject.GetComponent<MineDeployerInstance>();
                agent = Mine.Owner;
            }
            else
            {
                agent = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            }

            //if (mineGobject != null)
            //    ZiMain.log.LogInfo($"Gobject name: {mineGobject.name}");

            if (agent == null || commander == null)
            {
                ZiMain.log.LogError("Invalid request to pickup mine: agent, item or commander is null.");
                return;
            }
            if (Mine != null)
                ZiMain.log.LogInfo($"{commander.PlayerName} wants to tell {agent.PlayerName} to pickup their mine at {Mine.transform.position}");
            else
                ZiMain.log.LogInfo($"{commander.PlayerName} wants to tell {agent.PlayerName} to pickup all their mines");
            if (!agent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to pickup mine, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToPickUpMine(agent.GetComponent<PlayerAIBot>(), Mine, commander, sender, info.ID);
        }
        internal static void ReciveRequestToPickupItem(ulong sender, pStructs.pPickupItemInfo info)
        {
            ZiMain.log.LogInfo("Recived request to pickup item!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent agent = pStructs.Get_RefFrom_pStruct(info.playerAgent);
            PlayerAgent commander = pStructs.Get_RefFrom_pStruct(info.commander);
            GameObject itemGobject = pStructs.Get_RefFrom_pStruct(info.item);
            if (itemGobject != null)
                ZiMain.log.LogInfo($"Gobject name: {itemGobject.name}");
            ItemInLevel item = itemGobject.GetComponent<ItemInLevel>();
            if (item == null)
                ZiMain.log.LogInfo($"Item in level is null");
            if (item == null || agent == null || commander == null)
            {
                ZiMain.log.LogError("Invalid request to pickup item: agent, item or commander is null.");
                return;
            }
            ZiMain.log.LogInfo($"{commander.PlayerName} wants to tell {agent.PlayerName} to pickup a {item.PublicName}");
            if (!agent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to pickup item, You can't tell a player what to do.");
                return;
            }
            PlayerAIBot aiBot = agent.gameObject.GetComponent<PlayerAIBot>();
            zBotActions.SendBotToPickupItem(aiBot, item, commander, sender, info.ID);
        }
        internal static void ReciveRequestToReviveAgent(ulong sender, pStructs.pReviveAgentInfo info)
        {
            ZiMain.log.LogInfo("Recived request to revive agent!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Reviver = pStructs.Get_RefFrom_pStruct(info.Revier);
            PlayerAgent Downed = pStructs.Get_RefFrom_pStruct(info.Downed);
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.commander);
            if (Reviver == null || Downed == null || Commander == null)
            {
                ZiMain.log.LogError("Invalid request to revive agent: Reviver, Downed or Commander is null.");
                return;
            }
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {Reviver.PlayerName} to revive {Downed.PlayerName}");
            if (!Reviver.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to revive agent. You can't tell a player what to do.");
                return;
            }
            PlayerAIBot Bot = Reviver.GetComponent<PlayerAIBot>();
            if (Bot == null)
            {
                ZiMain.log.LogError($"Invalid Request to revive agent. PlayerAiBot not found on revier.  This should never happen!");
                return;
            }
            zBotActions.SendBotToReviveAgent(Bot, Downed, Commander, sender, info.ID);
        }
        internal static void ReciveRequestToShareResource(ulong netSender, pStructs.pShareResourceInfo info) 
        {
            ZiMain.log.LogInfo("Recived request to share resoruce!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent sender = pStructs.Get_RefFrom_pStruct(info.sender);
            PlayerAgent receiver = pStructs.Get_RefFrom_pStruct(info.receiver);
            PlayerAgent commander = pStructs.Get_RefFrom_pStruct(info.commander);
            
            if (sender == null || receiver == null || commander == null)
            {
                ZiMain.log.LogError("Invalid request to share resource: sender, reciver or commander is null.");
                return;
            }
            ZiMain.log.LogInfo($"{commander.PlayerName} wants to tell {sender.PlayerName} to share resoruces with {receiver.PlayerName}");
            if (!sender.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to share resource, You can't tell a player what to do.");
                return;
            }
            PlayerAIBot aiBot = sender.gameObject.GetComponent<PlayerAIBot>();
            zBotActions.SendBotToShareResourcePack(aiBot, receiver, commander, netSender, info.ID);
        }
        [Obsolete]
        internal static void ReciveRequestToKillEnemy(ulong netSender, pStructs.pAttackEnemyInfo info)
        {
            //ZiMain.log.LogInfo("Recived request to kill enemy!");
            //if (!SNet.IsMaster)
            //    return;
            //PlayerAgent aiBotAgent = pStructs.Get_RefFrom_pStruct(info.aiBot);
            //PlayerAgent commander = pStructs.Get_RefFrom_pStruct(info.commander);
            //EnemyAgent enemy = pStructs.Get_RefFrom_pStruct(info.enemy);

            //if (aiBotAgent == null || enemy == null || commander == null)
            //{
            //    ZiMain.log.LogError("Invalid request to share resource: aiBot, reciver or enemy is null.");
            //    return;
            //}
            //ZiMain.log.LogInfo($"{commander.PlayerName} wants to tell {aiBotAgent.PlayerName} to kill an enemy.");
            //if (!aiBotAgent.Owner.IsBot)
            //{
            //    ZiMain.log.LogWarning("Invalid request to kill enemy, You can't tell a player what to do.");
            //    return;
            //}
            //PlayerAIBot aiBot = aiBotAgent.gameObject.GetComponent<PlayerAIBot>();
            //zBotActions.SendBotToKillEnemy(aiBot, enemy, commander, netSender);
        }
        internal static void ReciveRequestToPickupSentry(ulong netSender, pStructs.pPickupSentryInfo info)
        {
            ZiMain.log.LogInfo("Recived request to pick up sentry!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent agent = pStructs.Get_RefFrom_pStruct(info.playerAgent);
            PlayerAgent commander = pStructs.Get_RefFrom_pStruct(info.commander);

            if (agent == null || commander == null)
            {
                ZiMain.log.LogError("Invalid request to pick up sentry: agent or commander is null.");
                return;
            }
            ZiMain.log.LogInfo($"{commander.PlayerName} wants to tell {agent.PlayerName} to pick up their turret!");
            if (!agent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to pickup sentry, You can't tell a player what to do.");
                return;
            }
            PlayerAIBot aiBot = agent.gameObject.GetComponent<PlayerAIBot>();
            PlayerVoiceManager.WantToSay(commander.CharacterID, AK.EVENTS.PLAY_CL_PICKUPYOURDEPLOYABLES);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Pick up your deployables.", 1);
            zBotActions.SendBotToPickUpSentry(aiBot, commander, netSender, info.ID);
        }
        internal static void ReciveRequestToPlaceSentry(ulong netSender, pStructs.pPlaceToolInfo info)
        {
            ZiMain.log.LogInfo("Recived request to place a sentry!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent agent = pStructs.Get_RefFrom_pStruct(info.playerAgent);
            PlayerAgent commander = pStructs.Get_RefFrom_pStruct(info.commander);
            Pose pose = info.Pose;

            if (agent == null || commander == null || pose == null)
            {
                ZiMain.log.LogError("Invalid request to place turret: agent, commander or pose is null.");
                return;
            }

            ZiMain.log.LogInfo($"{commander.PlayerName} wants to tell {agent.PlayerName} to place their turret!");
            if (!agent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to place turret, You can't tell a player what to do.");
                return;
            }

            PlayerAIBot aiBot = agent.gameObject.GetComponent<PlayerAIBot>();
            zBotActions.SendBotToPlaceSentry(aiBot, pose, commander, netSender, info.ID);
        }
        internal static void ReciveRequestToThrowItem(ulong netSender, pStructs.pThrowDataInfo info)
        {
            ZiMain.log.LogInfo("Recived request to throw item!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.Agent);
            //pStructs.pThrowType ThrowType = info.ThrowType;
            Vector3 MovePostion = info.MovePosition;
            Vector3 TargetPosition = info.TargetPosition;
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to throw their consumable from {MovePostion} to {TargetPosition}");
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to throw item, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToThrowItem(Commander, BotAgent, MovePostion, TargetPosition, netSender, info.ID);
            //zBotActions.SendBotToThrowItem(Commander, BotAgent, ThrowType, MovePostion, TargetPosition, netSender);
        }
        internal static void ReciveRequestToUseCfoam(ulong netSender, pStructs.pUseCfoamInfo info)
        {
            ZiMain.log.LogInfo("Recived request to use cfoam launcher!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.Agent);
            Vector3 MovePostion = Commander.transform.position;
            Vector3 TargetPosition = info.position;
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to use their cfoam gun from {MovePostion} to {TargetPosition}");
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to use cfoam launcher, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToUseCfoamGun(BotAgent.GetComponent<PlayerAIBot>(), TargetPosition, Commander, netSender, info.ID);
        }
        internal static void ReciveRequestToPlaceMine(ulong netSender, pStructs.pPlaceMineInfo info)
        {
            ZiMain.log.LogInfo("Recived request to place mine!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.Agent);
            Pose pose = info.pose;
            InventorySlot slot = info.slot;
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to use place their mine from {slot} to {info.pose}");
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to place mines, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToPlaceMine(BotAgent.GetComponent<PlayerAIBot>(), pose, slot, Commander, netSender, info.ID);
        }
        internal static void ReciveRequestToBreakLock(ulong netSender, pStructs.pBreakLockInfo info)
        {
            ZiMain.log.LogInfo("Recived request to break lock!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            info.Lock.pRep.TryGetID(out IReplicator rep);
            GameObject LockGO = rep.ReplicatorSupplier.gameObject;
            LG_WeakLock Lock = LockGO.GetComponent<LG_WeakLock>();
            MethodEnum method = info.Method;
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to use break the lock at {LockGO.transform.position}");
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to break lock, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendbotToBreakLock(BotAgent.GetComponent<PlayerAIBot>(), Lock, method, Commander, netSender, info.ID);
        }
        internal static void ReciveRequestToSetLeader(ulong netSender, pStructs.pLeaderInfo info)
        {
            ZiMain.log.LogInfo("Recived request to set leader!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent Follower = pStructs.Get_RefFrom_pStruct(info.follower);
            PlayerAgent Leader = pStructs.Get_RefFrom_pStruct(info.leader);
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {Follower.PlayerName} to follow {Leader.PlayerName}");
            zBotActions.SetLeader(Follower, Leader, Commander, netSender);
        }
        internal static void ReciveActionTerminated(ulong netsender, pStructs.pActionTerminatedInfo info) // TODO confirm this works.
        {
            if (SNet.IsMaster)
                return;
            foreach (ManualAction action in zActions.manualActions[zStaticRefrences.LocalPlayer.CharacterID])
            {
                if (action.ID == info.ID)
                {
                    zActions.manualActions[zStaticRefrences.LocalPlayer.CharacterID].Remove(action);
                    break;
                }
            }
        }
        internal static void ReciveRequestActionCancel(ulong netsender, pStructs.pActionTerminatedInfo info)
        {
            ZiMain.log.LogInfo($"Recived request to cancel action {info.ID}!");
            if (!SNet.IsMaster)
                return;
            foreach (var key in zActions.manualActions.Keys)
            {
                foreach (var action in zActions.manualActions[key]) 
                { 
                    if (action.ID == info.ID)
                    {
                        zBotActions.CancelBotAction(info.ID, netsender);
                        ZiMain.log.LogInfo($"Remotely canceld {action.Commander.PlayerName}'s command for {action.Bot.Agent.PlayerName} to {action.ActionDescriptor.GetIl2CppType().FullName}");
                        return;
                    }
                }
            }
            ZiMain.log.LogWarning($"Could not find action {info.ID}");
        }
        internal static void ReciveRequestToKillSleeper(ulong netSender, pStructs.pAttackEnemyInfo info)
        {
            ZiMain.log.LogInfo("Recived request to kill sleeper!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            EnemyAgent Enemy = pStructs.Get_RefFrom_pStruct(info.Enemy);
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to kill the {Enemy.EnemyData.name} at {Enemy.transform.position}");
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to attack sleeper, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToAttackSleeper(BotAgent.GetComponent<PlayerAIBot>(), Enemy, Commander, netSender, info.ID);
        }
        internal static void ReciveRequestToDropHere(ulong netSender, pStructs.pLocationInfo info)
        {
            ZiMain.log.LogInfo("Recived request to drop here!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            Vector3 DropPosition = info.position;
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to drop their item at {DropPosition}");
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to drop here, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToDropHere(BotAgent.GetComponent<PlayerAIBot>(), DropPosition, Commander, netSender, info.ID);
        }
        internal static void ReciveRequestToInsertCell(ulong netSender, pStructs.pLocationInfo info)
        {
            ZiMain.log.LogInfo("Recived request to insert cell!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            Vector3 GeneratorPosition = info.position;
            LG_PowerGenerator_Core Generator = zSearch.FindNearest(GeneratorPosition, Il2CppType.Of<LG_PowerGenerator_Core>(), 0.5f)?.TryCast<LG_PowerGenerator_Core>();
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to insert cell at {GeneratorPosition}");
            if (Generator == null)
            {
                ZiMain.log.LogError($"Failed to find generator at {GeneratorPosition}");
                return;
            }
            
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to insert cell, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToInsertCell(BotAgent.GetComponent<PlayerAIBot>(), Generator, Commander, netSender, info.ID);
        }
        internal static void ReciveRequestToOpenContainer(ulong netSender, pStructs.pLocationInfo info)
        {
            ZiMain.log.LogInfo("Recived request to open container!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            Vector3 ContainerPosition = info.position;
            LG_WeakResourceContainer Container = zSearch.FindNearest(ContainerPosition, Il2CppType.Of<LG_WeakResourceContainer>(), 0.5f)?.TryCast<LG_WeakResourceContainer>();
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to open the container at {ContainerPosition}");
            if (Container == null)
            {
                ZiMain.log.LogError($"Failed to find container at {ContainerPosition}");
                return;
            }

            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to open container, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToOpenContainer(BotAgent.GetComponent<PlayerAIBot>(), Container, Commander, netSender, info.ID);
        }
        internal static void ReciveRequestToRefillSentry(ulong netSender, pStructs.pLocationInfo info)
        {
            ZiMain.log.LogInfo("Recived request to refill sentry!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            Vector3 SentryPosition = info.position;
            SentryGunInstance Sentry = zSearch.FindNearest(SentryPosition, Il2CppType.Of<SentryGunInstance>(), 0.5f)?.TryCast<SentryGunInstance>();
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to refill the sentry at {SentryPosition}");
            if (Sentry == null)
            {
                ZiMain.log.LogError($"Failed to find sentry at {SentryPosition}");
                return;
            }
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to refill sentry, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToRefillSentry(BotAgent.GetComponent<PlayerAIBot>(), Sentry, Commander, netSender, info.ID);
        }
        internal static void ReciveRequestToInteractDoor(ulong netSender, pStructs.pDoorInteractInfo info)
        {
            ZiMain.log.LogInfo("Recived request to interact with door!");
            if (!SNet.IsMaster)
                return;
            PlayerAgent Commander = pStructs.Get_RefFrom_pStruct(info.Commander);
            PlayerAgent BotAgent = pStructs.Get_RefFrom_pStruct(info.BotAgent);
            Vector3 DoorPosition = info.DoorPosition;
            Vector3 TargetPosition = info.TargetPosition;
            MethodEnum Method = info.Method;
            LG_WeakDoor Door = zSearch.FindNearest(DoorPosition, Il2CppType.Of<LG_WeakDoor>(), 0.5f)?.TryCast<LG_WeakDoor>();
            ZiMain.log.LogInfo($"{Commander.PlayerName} wants to tell {BotAgent.PlayerName} to refill the sentry at {DoorPosition}");
            if (Door == null)
            {
                ZiMain.log.LogError($"Failed to find door at {DoorPosition}");
                return;
            }
            if (!BotAgent.Owner.IsBot)
            {
                ZiMain.log.LogWarning("Invalid request to interact with door, You can't tell a player what to do.");
                return;
            }
            zBotActions.SendBotToInteractDoor(BotAgent.GetComponent<PlayerAIBot>(), Door, TargetPosition, Method, Commander, netSender, info.ID);
        }
    }
}
