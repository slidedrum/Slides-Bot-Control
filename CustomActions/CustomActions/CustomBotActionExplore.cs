using Il2CppInterop.Runtime.Injection;
using Player;
using SlideMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using Zombified_Initiative;

namespace BotControl.CustomActions.CustomActions
{
    internal class CustomBotActionExplore : CustomActionBase
    {
        private StateEnum state = StateEnum.None;
        public static float Prio = 3.1f;
        VisitNode UnexploredNode = null;
        PlayerAgent OriginalLeader = null;
        public static PlayerBotActionBase.AccessLayers s_RequiredLayers = PlayerBotActionBase.AccessLayers.Legs | PlayerBotActionBase.AccessLayers.RootPosition;
        //public static Dictionary<int, bool> ExplorePerms = new(); //bot.Agent.Owner.PlayerSlotIndex()
        public static new bool Setup()
        {
            return true;
        }
        public static bool GetExplorePerm(PlayerAIBot bot)
        {
            return (bool)zSlideComputer.ActionPermissions.ValueAt("Explore");
        }
        public PlayerBotActionTravel.Descriptor travelAction = null;
        public new class Descriptor : CustomActionBase.Descriptor
        {
            float lastLooked = 0;
            public bool canExplore = true;
            float lookCooldown = 5;
            List<string> typeIgnoreList = [
                typeof(RootPlayerBotAction).FullName,
                typeof(PlayerBotActionFollow).FullName,
                typeof(PlayerBotActionIdle).FullName,
                typeof(PlayerBotActionLook).FullName,
            ];
            List<string> typeBlackList = [
                typeof(PlayerBotActionCollectItem).FullName,
                typeof(PlayerBotActionAttack).FullName,
                typeof(PlayerBotActionRevive).FullName,
                typeof(PlayerBotActionHighlight).FullName,
                typeof(PlayerBotActionShareResourcePack).FullName,
            ];
            public Descriptor() : base(ClassInjector.DerivedConstructorPointer<Descriptor>())
            {
                ClassInjector.DerivedConstructorBody(this);
                //Don't use.  This is needed for Il2cpp nonsnse.
            }
            public Descriptor(IntPtr ptr) : base(ptr)
            {
                ClassInjector.DerivedConstructorBody(this);
                //Don't use.  This is needed for Il2cpp nonsnse.
            }
            public Descriptor(PlayerAIBot bot) : base(ClassInjector.DerivedConstructorPointer<Descriptor>())
            {
                ClassInjector.DerivedConstructorBody(this);
                InitDescriptor(bot);
                this.RequiredLayers = s_RequiredLayers;
                //Use this
            }
            public override void CompareAction(PlayerAIBot bot, ref PlayerBotActionBase.Descriptor bestAction)
            {
                if (!canExplore)
                    return;
                if (lastLooked == 0)
                    lastLooked = Time.time;
                if (DramaManager.CurrentStateEnum != DRAMA_State.Exploration && DramaManager.CurrentStateEnum != DRAMA_State.Sneaking)
                    return;
                //if (DramaManager.EnemiesAreClose)
                //    return;
                if (Time.time - lastLooked < lookCooldown)
                    return;
                if (!IsTerminated())
                    return;
                if (!GetExplorePerm(Bot))
                    return;
                bool foundEnemy = HasFoundEnemies();
                if (foundEnemy)
                    return;
                float maxprio = 0f;
                foreach (var act in Bot.Actions)
                {
                    if (typeBlackList.Contains(act.GetIl2CppType().FullName))
                        return;
                    if (typeIgnoreList.Contains(act.GetIl2CppType().FullName))
                        continue;
                    var desc = act.DescBase;
                    maxprio = Math.Max(desc.Prio, maxprio);
                }
                if (maxprio > CustomBotActionExplore.Prio)
                    return;
                if (zVisitedManager.GetUnexploredLocation(Bot.Agent.Position, 0, 30) == null)
                    return;
                if (bestAction == null || CustomBotActionExplore.Prio > bestAction.Prio)
                {
                    bestAction = this;
                    this.Prio = CustomBotActionExplore.Prio;
                    lastLooked = Time.time;
                }
            }
            public override void OnQueued()
            {
                ZiMain.log.LogWarning("Hello Explore has been queued." + Bot.Agent.PlayerName);
                base.OnQueued();
            }
            public override PlayerBotActionBase CreateAction()
            {
                return new CustomBotActionExplore(this);
            }
        }
        public CustomBotActionExplore() : base(ClassInjector.DerivedConstructorPointer<CustomBotActionExplore>())
        {//Don't use!
            ClassInjector.DerivedConstructorBody(this);
            
        }
        public CustomBotActionExplore(IntPtr ptr) : base(ptr)
        {//Don't use!
            ClassInjector.DerivedConstructorBody(this);
        }
        public CustomBotActionExplore(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<CustomBotActionExplore>())
        {// Use this.
            ClassInjector.DerivedConstructorBody(this);
            InitFromDescriptor(desc);
            //ZiMain.sendChatMessage("Here I go exploring because I feel like it.",m_bot.Agent);
            state = StateEnum.lookingForUnexplored;
        }
        public override bool Update()
        {
            base.Update();
            if (OriginalLeader == null)
            {
                OriginalLeader = m_bot.SyncValues.Leader;
                m_bot.SyncValues.Leader = m_bot.Agent;
            }
            if (m_bot.SyncValues.Leader != m_bot.Agent)
            {
                DescBase.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Failed);
                Stop();
            }
            if (!GetExplorePerm(m_bot))
            {
                DescBase.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Successful);
                state = StateEnum.Finished;
                return true;
            }
            if (state == StateEnum.lookingForUnexplored)
            {
                if (UnexploredNode == null)
                {
                    UnexploredNode = zVisitedManager.GetUnexploredLocation(m_bot.Agent.Position);
                    if (UnexploredNode == null)
                    {
                        DescBase.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Successful);
                        state = StateEnum.Finished;
                        return false;
                    }
                    state = StateEnum.Idle;
                    return false;
                }
                state = StateEnum.Idle;
                return false;
            }
            else if (state == StateEnum.Idle)
            {
                if (travelAction == null || travelAction.IsTerminated())
                {
                    PlayerAgent agent = m_bot.Agent;
                    Vector3 Unexplored = UnexploredNode.position;
                    travelAction = new(m_bot)
                    {
                        DestinationPos = Unexplored,
                        Haste = 0.5f,
                        WalkPosture = PlayerBotActionWalk.Descriptor.PostureEnum.None,
                        Radius = 0.5f,
                        DestinationType = PlayerBotActionTravel.Descriptor.DestinationEnum.Position,
                        Persistent = false,
                        ParentActionBase = this,
                        Prio = CustomBotActionExplore.Prio,
                    };
                    m_bot.StartAction(travelAction);
                    //FlexibleMethodDefinition callback = new(OnTravelActionEvent, [travelAction]);
                    //zActionSub.addOnTerminated(travelAction, callback);
                    state = StateEnum.Moving;
                    return false;
                }
                state = StateEnum.Moving;
                return !IsActive(); //Waiting for travel action to finish.
            }
            else if (state == StateEnum.Finished)
            {
                if (travelAction.Status == PlayerBotActionBase.Descriptor.StatusType.Successful)
                    zChatHandler.sendChatMessage("I have looked everywhere!", "");
                if (travelAction.Status == PlayerBotActionBase.Descriptor.StatusType.Active)
                    ZiMain.log.LogWarning("Travel action still active somehow.");
                DescBase.SetCompletionStatus(travelAction.Status == PlayerBotActionBase.Descriptor.StatusType.Successful ? PlayerBotActionBase.Descriptor.StatusType.Successful : PlayerBotActionBase.Descriptor.StatusType.Failed);
                Stop();
                return true;
            }
            else if (state == StateEnum.Moving) 
            {
                if (HasFoundEnemies())
                {
                    state = StateEnum.Finished;                    
                    return false;
                }
                if (UnexploredNode != null && UnexploredNode.discovered)
                {
                    state = StateEnum.lookingForUnexplored;
                    UnexploredNode = null;
                    return false;
                }
                else if (travelAction.DestinationPos != UnexploredNode.position)
                {
                    travelAction.DestinationPos = UnexploredNode.position;
                }
            }
            return !IsActive();
        }
        public static bool HasFoundEnemies()
        {
            //foreach (var findable in zSearch.FindableObjects.Values)
            //{
            //    if (findable.type == typeof(EnemyAgent) && findable.found)
            //    {
            //        return true;
            //    }
            //}
            return zFindableManager.AllFoundFindables.Any(obj => obj.found && obj.pingSyle == eNavMarkerStyle.PlayerPingEnemy && obj.gameObject != null && obj.gameObject.activeInHierarchy); ;
        }
        //public void OnTravelActionEvent(PlayerBotActionBase.Descriptor descBase)
        //{
        //    PlayerBotActionTravel.Descriptor IncomingTravelAction = (PlayerBotActionTravel.Descriptor)descBase;
        //    if (IncomingTravelAction.Pointer != this.travelAction.Pointer)
        //        return;
        //    UnexploredNode = null;
        //    if (IncomingTravelAction.Status == PlayerBotActionBase.Descriptor.StatusType.Successful)
        //    {
        //        state = StateEnum.lookingForUnexplored;
        //    }
        //    else if (IncomingTravelAction.IsTerminated())
        //    {
        //        state = StateEnum.Finished;
        //    }
        //}
        public override void Stop()
        {
            if (travelAction != null && !travelAction.IsTerminated())
                m_bot.StopAction(travelAction);
            if (m_bot.SyncValues.Leader == m_bot.Agent)
                m_bot.SyncValues.Leader = OriginalLeader;
            OriginalLeader = null;
            base.Stop();
        }
        public enum StateEnum
        {
            None,
            Finished,
            lookingForUnexplored,
            Moving,
            Idle,
        }
    }
}
