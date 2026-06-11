using Agents;
using Enemies;
using LevelGeneration;
using Player;
using SNetwork;
using UnityEngine;

namespace BotControl.Networking
{
    public class pStructs
    {
        //This handles encoding and decoding objects for network transfer.
        public static PlayerAgent Get_RefFrom_pStruct(pPlayerAgent pStruct)
        {
            if (!pStruct.TryGet(out PlayerAgent refrence))
                return null;
            return refrence;
        }
        public static Agent Get_RefFrom_pStruct(pAgent pStruct)
        {
            if (!pStruct.TryGet(out Agent refrence))
                return null;
            return refrence;
        }
        public static EnemyAgent Get_RefFrom_pStruct(pEnemyAgent pStruct)
        {
            if (!pStruct.TryGet(out EnemyAgent refrence))
                return null;
            return refrence;
        }
        public static PlayerAgent Get_RefFrom_pStruct(SNetStructs.pPlayer pStruct)
        {
            if (!pStruct.TryGetPlayer(out SNet_Player refrence))
                return null;
            return refrence.PlayerAgent.TryCast<PlayerAgent>();
        }
        public static GameObject Get_RefFrom_pStruct(pItemData pStruct)
        {
            GameObject item = null;
            if (pStruct.replicatorRef.TryGetID(out IReplicator rep))
                item = rep.ReplicatorSupplier.gameObject;
            return item;
        }
        public static SNetStructs.pPlayer Get_pPlayerFromRefrence(PlayerAgent refrence)
        {
            SNetStructs.pPlayer pStruct = new();
            pStruct.SetPlayer(refrence.Owner);
            return pStruct;
        }
        public static pStateReplicatorProvider Get_pStructFromRef<S, I>(iSNet_StateReplicatorProvider original) where S : struct where I : struct
        {
            iSNet_StateReplicator stateReplicator = original.GetStateReplicator();
            SNet_StateReplicator<S, I> Replicator = stateReplicator.TryCast<SNet_StateReplicator<S,I>>();
            if (Replicator == null)
                return default;
            return Replicator.GetProviderSyncStruct();
        }
        public static pStateReplicatorProvider Get_pStructFromRef<S>(iSNet_StateReplicatorProvider original) where S : struct
        {
            iSNet_StateReplicator stateReplicator = original.GetStateReplicator();
            SNet_StateReplicator<S> Replicator = stateReplicator.TryCast<SNet_StateReplicator<S>>();
            if (Replicator == null)
                return default;
            return Replicator.GetProviderSyncStruct();
        }
        public static pAgent Get_pStructFromRefrence(Agent refrence)
        {
            pAgent pStruct = new();
            pStruct.Set(refrence);
            return pStruct;
        }
        public static pPlayerAgent Get_pStructFromRefrence(PlayerAgent refrence)
        {
            pPlayerAgent pStruct = new();
            pStruct.Set(refrence);
            return pStruct;
        }
        public static pEnemyAgent Get_pStructFromRefrence(EnemyAgent refrence)
        {
            pEnemyAgent pStruct = new();
            pStruct.Set(refrence);
            return pStruct;
        }
        public static pItemData Get_pStructFromRefrence(Item refrence)
        {
            pItemData pStruct = refrence.Get_pItemData();
            return pStruct;
        }
        public struct pGenericPermission
        {
            public int playerID;
            public int actionID;
            public bool allowed;
        }
        public struct pPickupMineInfo
        {
            //public Vector3 MineCords;
            public ushort MineReplicatorKey;
            public pPlayerAgent BotAgent;
            public pPlayerAgent Commander;
        }
        public struct pAttackEnemyInfo
        {
            public pEnemyAgent enemy;
            public pPlayerAgent aiBot;
            public pPlayerAgent commander;
        }
        public struct pLeaderInfo
        {
            public pPlayerAgent leader;
            public pPlayerAgent follower;
            public pPlayerAgent Commander;
        }
        public struct pBreakLockInfo
        {
            public pStateReplicatorProvider Lock;
            public pPlayerAgent BotAgent;
            public pPlayerAgent Commander;
        }
        internal struct pPlaceToolInfo
        {
            public pPlayerAgent playerAgent;
            public pPlayerAgent commander;
            public Pose Pose;
        }
        internal struct pPickupSentryInfo
        {
            public pPlayerAgent playerAgent;
            public pPlayerAgent commander;
        }
        internal struct pPickupItemInfo
        {
            public pItemData item;
            public pPlayerAgent playerAgent;
            public pPlayerAgent commander;
        }
        internal struct pShareResourceInfo
        {
            public pPlayerAgent sender;
            public pPlayerAgent receiver;
            public pPlayerAgent commander;
        }
        internal struct pReviveAgentInfo
        {
            public pPlayerAgent Revier;
            public pPlayerAgent Downed;
            public pPlayerAgent commander;
        }
        internal struct pUseCfoamInfo
        {
            public pPlayerAgent Agent;
            public pPlayerAgent Commander;
            public Vector3 position;
        }
        internal struct pPlaceMineInfo
        {
            public pPlayerAgent Agent;
            public pPlayerAgent Commander;
            public Pose pose;
            public InventorySlot slot;
        }
        internal struct pMoveToLocationInfo
        {
            public Vector3 position;
            public pPlayerAgent BotAgent;
            public pPlayerAgent Commander;
        }
        internal struct pBoolOverideTreeInfo
        {
            public uint treeID;
            public uint keyId;
            public bool value;
            public bool isNull;
        }
        internal struct pIntOverideTreeInfo
        {
            public uint treeID;
            public uint keyId;
            public int value;
            public bool isNull;
        }
        internal struct pFloatOverideTreeInfo
        {
            public uint treeID;
            public uint keyId;
            public float value;
            public bool isNull;
        }
        //public enum pThrowType : uint
        //{
        //    FogRepeller,
        //    Glowstick,
        //    cFoam,
        //}
        public struct pThrowDataInfo
        {
            public pPlayerAgent Commander;
            public pPlayerAgent Agent;
            //public pThrowType ThrowType;
            public Vector3 MovePosition;
            public Vector3 TargetPosition;
        }
    }
}
