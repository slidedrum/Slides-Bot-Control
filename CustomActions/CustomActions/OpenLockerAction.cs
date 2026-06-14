using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using Player;
using SlideMenu;
using System;
using UnityEngine;
namespace BotControl.CustomActions.CustomActions
{

    public class OpenLockerAction : CustomActionBase
    {
        //This is an example of how you can set up your own custom action!
        public static new bool Setup() //This will be called when your class is regestered, it should return true if your action will even activate on it's own, or false if it's an exclusively manual action.
        {
            return false;
        }
        public new class Descriptor : CustomActionBase.Descriptor
        {
            public LG_WeakResourceContainer TargetContainer;
            public float Haste = 1f;
            private PlayerManager.PositionReservation m_posReservation;
            private PlayerManager.ObjectReservation m_objReservation;
            public static PlayerBotActionBase.AccessLayers s_RequiredLayers = PlayerBotActionBase.AccessLayers.Legs | PlayerBotActionBase.AccessLayers.Hip | PlayerBotActionBase.AccessLayers.RootPosition | PlayerBotActionBase.AccessLayers.RootRotation;
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
                this.RequiredLayers = PlayerBotActionCollectItem.Descriptor.s_RequiredLayers;
                //Use this is your descriptor constructor.
                //The descriptor is used to describe everything about your action.
                //Any paramaters are set up by the calling class.  
                //Be sure to add any you need to this class.
                //Some paramaters are inhareted, like Prio (priority). 
            }
            public override PlayerBotActionBase CreateAction()
            {
                //This converts your descriptor into an action instance.
                //This means your action is starting!
                //You probably won't need to do anything else here.
                return new OpenLockerAction(this);
            }
            public override bool IsActionAllowed(PlayerBotActionBase.Descriptor desc)
            {
                //Does your action play nice with desc?
                if (desc.TryCast<PlayerBotActionHighlight.Descriptor>() != null)
                    return false;
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
                if (this.m_posReservation == null)
                {
                    this.m_posReservation = new PlayerManager.PositionReservation
                    {
                        CharacterID = this.Bot.Agent.CharacterID,
                        Position = this.TargetContainer.transform.position,
                        Radius = 0.5f
                    };
                }
                else
                {
                    this.m_posReservation.Position = this.TargetContainer.transform.position;
                }
                PlayerManager.Current.AddPositionReservation(this.m_posReservation);
                GameObject gameObject = null;
                if (this.TargetContainer != null && this.TargetContainer.gameObject != null)
                {
                    gameObject = this.TargetContainer.gameObject;
                }
                if (gameObject != null)
                {
                    if (this.m_objReservation == null)
                    {
                        this.m_objReservation = new PlayerManager.ObjectReservation
                        {
                            CharacterID = this.Bot.Agent.CharacterID,
                            Object = gameObject
                        };
                    }
                    else
                    {
                        this.m_objReservation.Object = gameObject;
                    }
                    PlayerManager.Current.AddGameObjectReservation(this.m_objReservation);
                }
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
            public virtual void CompareAction(ref PlayerBotActionBase.Descriptor bestAction)
            {
                //Should your action be queued?
                //This gets called every frame
                //Be sure to compare priority against the current best action.
                //Best action inludes vanilla actions.
                //Be sure to not set this to best action if it's already active.
            }

        }
        private enum State
        {
            Idle,
            Move,
            StartOpening,
            Opening,
            Open,
            Finished,
            Failed,
        }
        private PlayerBotActionTravel.Descriptor TravelAction;
        private PlayerBotActionLook.Descriptor LookAction;
        private float Haste = 1f;
        private LG_WeakResourceContainer TargetContainer;
        private Vector3 TargetLoction;
        private Descriptor m_desc;
        private State state;
        private float openingTime = 1f;
        private float startOpeningTimestamp = 0f;
        public OpenLockerAction() : base(ClassInjector.DerivedConstructorPointer<OpenLockerAction>())// Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public OpenLockerAction(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public OpenLockerAction(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<OpenLockerAction>())
        {
            ClassInjector.DerivedConstructorBody(this);
            InitFromDescriptor(desc);
            this.m_desc = desc;
            this.state = State.Idle;
            this.TargetContainer = m_desc.TargetContainer;
            this.Haste = m_desc.Haste;
            //Use this constructor.
            //This means your action is starting!
        }
        public override void Stop()
        {
            //This is called when your action is told to stop.
            //Be sure to do any cleanup if you need to.
            base.Stop();
        }
        public override bool Update()
        {
            //This is called every frame when your action is active.
            if (base.Update())
                return true;
            switch (state)
            {
                case State.Idle:
                    UpdateStateIdle();
                    break;
                case State.Move:
                    UpdateStateMove();
                    break;
                case State.StartOpening:
                    UpdateStateStartOpening();
                    break;
                case State.Opening:
                    UpdateStateOpening();
                    break;
                case State.Open:
                    UpdateStateOpen();
                    break;
                case State.Finished:
                    this.m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Successful);
                    return true;
                case State.Failed:
                    this.m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Failed);
                    return true;
            }
            return !base.IsActive();
        }
        private void UpdateStateIdle()
        {
            if (!VerifyTarget())
            {
                state = State.Failed;
            }
            PlayerBotActionTravel.Descriptor Desc = new(m_bot)
            {
                DestinationPos = TargetLoction,
                DestinationType = PlayerBotActionTravel.Descriptor.DestinationEnum.Position,
                ParentActionBase = this,
                Prio = m_desc.Prio,
                Haste = Haste,
                Persistent = true,
            };
            if (this.m_bot.RequestAction(Desc))
            {
                this.TravelAction = Desc;
                this.state = State.Move;
                FlexibleMethodDefinition callback = new(OnTravelCompleted, [TravelAction]);
                zActionSub.addOnTerminated(TravelAction, callback);
            }
            else
            {
                this.state = State.Failed;
            }
        }
        private void UpdateStateMove()
        {
            UpdateLookAction();
            if ((m_bot.transform.position - TargetLoction).magnitude < 0.1f)
                state = State.StartOpening;
        }
        private void UpdateStateStartOpening()
        {
            UpdateLookAction();
            startOpeningTimestamp = Time.time;
            pGenericInteractAnimation.TypeEnum reachHeight = base.GetReachHeight(TargetContainer.transform.position.y);
            this.m_agent.Sync.SendGenericInteract(reachHeight, true);
            state = State.Opening;
        }
        private void UpdateStateOpening()
        {
            UpdateLookAction();
            if (Time.time > startOpeningTimestamp + openingTime)
                state = State.Open;
        }
        private void UpdateStateOpen()
        {
            TargetContainer.TriggerOpen();
            m_bot.StopAction(TravelAction);
            m_bot.StopAction(LookAction);
            state = State.Finished;
        }
        public void OnTravelCompleted(PlayerBotActionBase.Descriptor descBase)
        {
            if (descBase.Status == PlayerBotActionBase.Descriptor.StatusType.Successful)
            {
                this.state = State.StartOpening;
            }
            else
            {
                this.state = State.Failed;
            }
        }
        private void UpdateLookAction()
        {
            if (this.LookAction != null)
            {
                return;
            }
            PlayerBotActionLook.Descriptor descriptor = new PlayerBotActionLook.Descriptor(this.m_bot, true)
            {
                ParentActionBase = this,
                Prio = this.m_desc.Prio,
                Haste = 0.5f,
                TargetType = PlayerBotActionLook.TargetTypeEnum.Object,
                TargetObj = TargetContainer.transform
            };
            descriptor.EventDelegate = DelegateSupport.ConvertDelegate<PlayerBotActionBase.Descriptor.EventDelegateFunc>( new Action<PlayerBotActionBase.Descriptor>(OnLookActionEvent));
            descriptor.Turn.Prio = (descriptor.Turn.SoftPrio = descriptor.Prio);
            if (this.m_bot.RequestAction(descriptor))
            {
                this.LookAction = descriptor;
            }
        }
        public void OnLookActionEvent(PlayerBotActionBase.Descriptor descBase)
        {
            if (descBase != this.LookAction)
            {
                base.PrintError("Rogue action.");
            }
            if (descBase.IsTerminated())
            {
                this.LookAction = null;
            }
        }
        private bool VerifyTarget()
        {
            if (TargetContainer == null)
                return false;
            if (!GetTargetLocation())
                return false;
            if (!zHelpers.CanBotReach(m_bot, TargetLoction))
                return false;
            return true;
        }
        private bool GetTargetLocation()
        {
            GameObject gobject = TargetContainer.gameObject;
            if (gobject == null)
                return false;
            Transform transform = gobject.transform;
            Vector3 location = transform.position - transform.up * 1.5f;
            if (!SnapPositionToNav(location, out TargetLoction))
                return false;
            return true;
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