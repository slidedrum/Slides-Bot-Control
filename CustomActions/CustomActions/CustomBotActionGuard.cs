using Il2CppInterop.Runtime.Injection;
using Player;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
namespace BotControl.CustomActions.CustomActions
{

    public class CustomBotActionGuard : CustomActionBase
    {

        //This is an example of how you can set up your own custom action!
        public static new bool Setup() //This will be called when your class is regestered, it should return true if your action will even activate on it's own, or false if it's an exclusively manual action.
        {
            return true;
        }
        public new class Descriptor : CustomActionBase.Descriptor
        {
            internal static PlayerBotActionBase.AccessLayers s_RequiredLayers = PlayerBotActionBase.AccessLayers.RestrictionRadius;
            public bool LineOfSight;
            public float minDistance;
            public float maxDistance;
            public float Angle;
            public float AngleRange;
            public GameObject GuardObject;
            public Vector3 GuardPosition;
            public Mode mode;
            public float Haste;
            public enum Mode
            {
                GameObject,
                Position,
            }
            //This is an example of how you can set up your own custom descriptor!
            public Descriptor() : base(ClassInjector.DerivedConstructorPointer<Descriptor>()) // Don't use this!  Needed for il2cpp nonsense.
            {
                ClassInjector.DerivedConstructorBody(this);
            } // Don't use this!  Needed for il2cpp nonsense.
            public Descriptor(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
            {
                ClassInjector.DerivedConstructorBody(this);
            }  // Don't use this!  Needed for il2cpp nonsense.
            public Descriptor(PlayerAIBot bot) : base(ClassInjector.DerivedConstructorPointer<Descriptor>())
            {
                ClassInjector.DerivedConstructorBody(this);
                InitDescriptor(bot);
                this.m_accessLayers = s_RequiredLayers;
                //Use this is your descriptor constructor.
                //The descriptor is used to describe everything about your action.
                //Any paramaters are set up by the calling class.  
                //Be sure to add any you need to this class.
                //Some paramaters are inherited, like Prio (priority). 
            }
            public override PlayerBotActionBase CreateAction()
            {
                //This converts your descriptor into an action instance.
                //This means your action is starting!
                //You probably won't need to do anything else here.
                return new CustomBotActionGuard(this);
            }
            public override bool IsActionAllowed(PlayerBotActionBase.Descriptor desc)
            {
                //Does your action play nice with desc?
                if (desc.TryCast<PlayerBotActionFollow.Descriptor>() != null)
                {
                    return false;
                }
                return base.IsActionAllowed(desc);
            }
            public override bool CheckCollision(PlayerBotActionBase.Descriptor desc)
            {
                //Should this action abort if desc is active?
                return base.CheckCollision(desc);
            }
            public override void OnQueued()
            {
                //This gets called when your action is added to the que.
                base.OnQueued();
            }
            public override AccessLayers GetAccessLayersRuntime()
            {
                //A mostly simple getter method, tbh I don't really understand access layers yet.
                return base.GetAccessLayersRuntime();
            }
            public override void InternalOnTerminated()
            {
                //This gets called when your action is getting terminated.
                //This includes any form of interuption, but does not include finishing the action.
                base.InternalOnTerminated();
            }
            public override void CompareAction(PlayerAIBot bot, ref PlayerBotActionBase.Descriptor bestAction)
            {
                //Should your action be queued?
                //This gets called every frame
                //Be sure to compare priority against the current best action.
                //Best action inludes vanilla actions.
                //Be sure to not set this to best action if it's already active.
            }
        }
        public enum State
        {
            Idle,
            Move,
        }
        private State state;
        private Descriptor m_desc;
        private PlayerBotActionTravel.Descriptor TravelAction;
        Vector3 TargetPos => mode == Descriptor.Mode.Position ? GuardPosition : GuardObject.transform.position;
        private bool LineOfSight;
        private float minDistance;
        private float maxDistance;
        private float Angle;
        private float AngleRange;
        private GameObject GuardObject;
        private Vector3 GuardPosition;
        private Descriptor.Mode mode;
        private float Haste;
        private float Prio;
        private bool intialized = false;
        private float LastMovedTimestamp = 0;
        private Vector3 lastMovedPosition = Vector3.zero;
        private const float resetTime = 5;

        public CustomBotActionGuard() : base(ClassInjector.DerivedConstructorPointer<CustomBotActionGuard>())// Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionGuard(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionGuard(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<CustomBotActionGuard>())
        {
            ClassInjector.DerivedConstructorBody(this);
            InitFromDescriptor(desc);
            m_desc        = desc;
            GuardObject   = desc.GuardObject;
            LineOfSight   = desc.LineOfSight;
            minDistance   = desc.minDistance;
            maxDistance   = desc.maxDistance;
            Angle         = desc.Angle;
            AngleRange    = desc.AngleRange;
            GuardPosition = desc.GuardPosition;
            mode          = desc.mode;
            Haste         = desc.Haste;
            Prio          = desc.Prio;

            //Use this constructor.
            //This means your action is starting!
        }
        public override void Stop()
        {
            //This is called when your action is told to stop.
            //Be sure to do any cleanup if you need to.
            this.m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Successful);
            if (TravelAction != null && !TravelAction.IsTerminated())
                m_bot.StopAction(TravelAction);
            intialized = false;
            base.Stop();
        }
        public override bool Update()
        {
            //This is called every frame when your action is active.
            if (base.Update())
                return true;

            var root = m_bot.m_rootAction.ActionBase.TryCast<RootPlayerBotAction>();
            if (root != null)
            {
                var follow = root.m_followLeaderAction;
                if (!intialized)
                {
                    intialized = true;
                    m_bot.StopAction(follow);
                    zBotActions.SetLeader(m_bot.Agent, m_bot.Agent, zStaticRefrences.LocalPlayer, 0);
                }
            }
            if (m_bot.SyncValues.Leader != m_bot.Agent)
            {
                Stop();
                return true;
            }
                
            switch (state)
            {
                case State.Idle:
                    UpdateStateIdle();
                    break;
                case State.Move:
                    UpdateStateMove();
                    break;
            }
            //Your stuff goes here
            return !base.IsActive();
        }
        private void UpdateStateIdle()
        {
            float DistanceToTarget = Vector3.Distance(TargetPos, m_bot.transform.position);
            if (DistanceToTarget > maxDistance)
                ReturnToPosition(minDistance);
            if (DistanceToTarget < minDistance)
                return;
            if (Vector3.Distance(lastMovedPosition, m_bot.transform.position) > 0.01)
                UpdateLastMoved();
                
            if (Time.time - LastMovedTimestamp > resetTime)
                ReturnToPosition();
        }
        private void UpdateLastMoved()
        {
            LastMovedTimestamp = Time.time;
            lastMovedPosition = m_bot.transform.position;
        }
        private void ReturnToPosition(float radius = 0.1f)
        {
            state = State.Move;
            if (TravelAction == null)
            {
                TravelAction = new(m_bot)
                {
                    Haste = Haste,
                    DestinationType = PlayerBotActionTravel.Descriptor.DestinationEnum.Position,
                    Prio = Prio,
                    Persistent = false,
                    DestinationPos = mode == Descriptor.Mode.Position ? GuardPosition : GuardObject.transform.position,
                    Radius = radius,
                    ParentActionBase = this,
                };
            }
            TravelAction.DestinationPos = TargetPos;
            TravelAction.Radius = radius;
            m_bot.RequestAction(TravelAction);
            return;
        }
        private void UpdateStateMove()
        {
            UpdateLastMoved();
            if (TravelAction.IsTerminated())
                state = State.Idle;
        }
        public override bool IsActionAllowed(PlayerBotActionBase.Descriptor desc)
        {
            //This just calls the descriptor version of this method.
            //Not sure why this is virtual, but it is.
            return base.IsActionAllowed(desc);
        }
        public override bool CheckCollision(PlayerBotActionBase.Descriptor desc)
        {
            //This does NOT call the descriptor version of this method
            //This re-implements the exact same thing as the descriptor version.
            //Not sure why this is virtual, but it is.
            return base.CheckCollision(desc);
        }
        public override AccessLayers GetAccessLayersRuntime()
        {
            //This tries to call the descriptor version of this method.
            //falls back to RequiredLayers
            return base.GetAccessLayersRuntime();
        }
        public override void OnWarped(Vector3 position)
        {
            //Called when the bot is warped, duh.
            //This will set completion status to failed by deafult.
            base.OnWarped(position);
        }
    }
}