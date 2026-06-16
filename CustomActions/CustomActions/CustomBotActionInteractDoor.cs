using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using Player;
using SlideMenu;
using System;
using UnityEngine;
namespace BotControl.CustomActions.CustomActions
{

    public class CustomBotActionInteractDoor : CustomActionBase
    {
        //This is an example of how you can set up your own custom action!
        public static new bool Setup() //This will be called when your class is regestered, it should return true if your action will even activate on it's own, or false if it's an exclusively manual action.
        {
            return false;
        }
        public new class Descriptor : CustomActionBase.Descriptor
        {
            public LG_DoorButton TargetButton;
            public LG_WeakDoor TargetDoor;
            public float Haste = 1f;
            public Vector3? TargetPosition;
            public PlayerBotActionUnlock.Descriptor.MethodEnum method = PlayerBotActionUnlock.Descriptor.MethodEnum.Any;
            private PlayerManager.PositionReservation m_posReservation;
            private PlayerManager.ObjectReservation m_objReservation;
            public static PlayerBotActionBase.AccessLayers s_RequiredLayers = PlayerBotActionBase.AccessLayers.Legs | PlayerBotActionBase.AccessLayers.Hip | PlayerBotActionBase.AccessLayers.RootPosition | PlayerBotActionBase.AccessLayers.RootRotation | AccessLayers.RightArm;
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
                return new CustomBotActionInteractDoor(this);
            }
            public override bool IsActionAllowed(PlayerBotActionBase.Descriptor desc)
            {
                //Does your action play nice with desc?
                return base.IsActionAllowed(desc);
            }
            public override bool CheckCollision(PlayerBotActionBase.Descriptor desc)
            {
                //Should this action abort if desc is active?
                return base.CheckCollision(desc);
            }
            private void GetTargetPosition()
            {
                if (TargetPosition != null)
                    return;
                TargetPosition = Bot.transform.position;
                if (Bot.SyncValues?.Leader != null)
                    TargetPosition = Bot.SyncValues?.Leader.transform.position;
            }
            private void GetButton()
            {
                GetTargetPosition();
                if (TargetButton != null)
                {
                    TargetDoor = TargetButton.m_door.Cast<LG_WeakDoor>();
                    return;
                }
                if (TargetDoor == null)
                    return;
                float bestDot = float.MinValue;
                LG_DoorButton bestButton = null; 
                foreach (var button in TargetDoor.m_buttons)
                {
                    float dot = Vector3.Dot(button.transform.forward, (Vector3)TargetPosition - button.transform.position);
                    if (dot > bestDot)
                    {
                        bestDot = dot;
                        bestButton = button;
                    }
                }
                TargetButton = bestButton;
            }
            public override void OnQueued()
            {
                //This gets called when your action is added to the que.

                //TargetButton might be null.
                base.OnQueued();
                GetButton();
                if (this.m_posReservation == null)
                {
                    this.m_posReservation = new PlayerManager.PositionReservation
                    {
                        CharacterID = this.Bot.Agent.CharacterID,
                        Position = this.TargetButton.transform.position,
                        Radius = 0.5f
                    };
                }
                else
                {
                    this.m_posReservation.Position = this.TargetButton.transform.position;
                }
                PlayerManager.Current.AddPositionReservation(this.m_posReservation);
                GameObject gameObject = null;
                if (this.TargetButton != null && this.TargetButton.gameObject != null)
                {
                    gameObject = this.TargetButton.gameObject;
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
                PlayerManager.Current.RemovePositionReservation(this.m_posReservation);
                PlayerManager.Current.RemoveGameObjectReservation(this.m_objReservation);
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
            StartUnlock,
            Unlocking,
            Press,
            InteractTime,
            Finished,
            Failed,
        }
        private PlayerBotActionTravel.Descriptor TravelAction;
        private PlayerBotActionLook.Descriptor LookAction;
        private PlayerBotActionUnlock.Descriptor UnlockAction;
        private float Haste = 1f;
        private PlayerBotActionUnlock.Descriptor.MethodEnum Method;
        private LG_DoorButton TargetButton;
        private LG_WeakDoor TargetDoor;
        private LG_WeakLock TargetLock;
        private Vector3 TargetLoction;
        private Descriptor m_desc;
        private State state;
        private static float interactTime = 1f;
        private float startInteractTimestamp = 0f;

        public CustomBotActionInteractDoor() : base(ClassInjector.DerivedConstructorPointer<CustomBotActionInteractDoor>())// Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionInteractDoor(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionInteractDoor(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<CustomBotActionInteractDoor>())
        {
            ClassInjector.DerivedConstructorBody(this);
            InitFromDescriptor(desc);
            this.m_desc = desc;
            this.state = State.Idle;
            this.TargetButton = m_desc.TargetButton;
            this.TargetDoor = m_desc.TargetDoor;
            this.Haste = m_desc.Haste;
            this.Method = m_desc.method;
            //Use this constructor.
            //This means your action is starting!
        }
        public override void Stop()
        {
            //This is called when your action is told to stop.
            //Be sure to do any cleanup if you need to.
            base.Stop();
            SafeStopAction(TravelAction);
            SafeStopAction(LookAction);
            SafeStopAction(UnlockAction);
        }
        public override bool Update()
        {
            //This is called every frame when your action is active.
            if (base.Update())
                return true;
            //Your stuff goes here
            switch (state)
            {
                case State.Idle:
                    UpdateStateIdle();
                    break;
                case State.Move:
                    UpdateStateMove();
                    break;
                case State.StartUnlock:
                    UpdateStateStartUnlock();
                    break;
                case State.Unlocking:
                    UpdateStateUnlocking();
                    break;
                case State.Press:
                    UpdateStatePress();
                    break;
                case State.InteractTime:
                    UpdateStateInteractTime();
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
            SafeStopAction(LookAction);
            LookAction = null;
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

        private bool VerifyTarget()
        {
            if (TargetButton == null)
                return false;
            //if (!TargetButton.CanInteract())
            //    return false;
            if (TargetDoor == null)
                return false;
            if (!GetTargetLocation())
                return false;
            if (!zHelpers.CanBotReach(m_bot, TargetLoction))
                return false;
            return true;
        }
        private bool GetTargetLocation()
        {
            GameObject gobject = TargetButton.gameObject;
            if (gobject == null)
                return false;
            Transform transform = gobject.transform;
            Vector3 location = transform.position + transform.forward * 1.5f;
            if (!SnapPositionToNav(location, out TargetLoction))
                return false;
            return true;
        }
        public void OnTravelCompleted(PlayerBotActionBase.Descriptor descBase)
        {
            if (this.state == State.Move && descBase.Status != PlayerBotActionBase.Descriptor.StatusType.Successful)
                this.state = State.Failed;
        }

        private void UpdateStateMove()
        {
            UpdateLookAction();
            if ((m_bot.transform.position - TargetLoction).magnitude < 0.1f)
            {
                GetLock();
                if (TargetLock == null)
                    state = State.Press;
                else
                {
                    state = State.StartUnlock;
                    m_bot.StopAction(TravelAction);
                }
            }
        }
        public void GetLock()
        {
            LG_WeakDoor door = TargetButton.m_door.Cast<LG_WeakDoor>();
            if (door.WeakLocks == null)
                return;
            if (door.WeakLocks.Count == 0)
                return;
            foreach (LG_WeakLock Lock in door.WeakLocks)
            {
                if (Lock.m_holder.Pointer == TargetButton.Pointer)
                {
                    TargetLock = Lock;
                    return;
                }
            }
        }
        private void UpdateStateStartUnlock()
        {
            float Prop = m_desc.Prio;
            GameObject targetObject = TargetButton.gameObject;
            PlayerBotActionUnlock.Descriptor.TargetTypeEnum targetType = PlayerBotActionUnlock.Descriptor.TargetTypeEnum.Door;
            PlayerBotActionUnlock.Descriptor Desc = new(m_bot)
            {
                TargetType = targetType,
                TargetGO = targetObject,
                Prio = 13,
                ParentActionBase = this,
                TargetPosition = targetObject.transform.position,
                Method = Method,
                Lock = TargetLock,
            };
            SafeStopAction(LookAction);
            SafeStopAction(TravelAction);
            if (this.m_bot.RequestAction(Desc))
            {
                this.UnlockAction = Desc;
                this.state = State.Unlocking;
                FlexibleMethodDefinition callback = new(OnUnlockCompleted, [UnlockAction]);
                zActionSub.addOnTerminated(UnlockAction, callback);
            }
            else
            {
                this.state = State.Failed;
            }
        }
        private void UpdateStateUnlocking()
        {

        }

        private void UpdateStatePress()
        {
            if (!TargetButton.Interact(m_bot.Agent))
            {
                state = State.Failed;
                return;
            }
            startInteractTimestamp = Time.time;
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
            state = State.InteractTime;
            pGenericInteractAnimation.TypeEnum reachHeight = base.GetReachHeight(TargetButton.transform.position.y);
            this.m_agent.Sync.SendGenericInteract(reachHeight, true);
        }

        private void UpdateStateInteractTime()
        {
            UpdateLookAction();
            if (Time.time > startInteractTimestamp + interactTime)
            {
                state = State.Finished;
                SafeStopAction(TravelAction);
                SafeStopAction(LookAction);
                SafeStopAction(UnlockAction);
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
                TargetObj = TargetButton.transform
            };
            descriptor.EventDelegate = DelegateSupport.ConvertDelegate<PlayerBotActionBase.Descriptor.EventDelegateFunc>(new Action<PlayerBotActionBase.Descriptor>(OnLookActionEvent));
            descriptor.Turn.Prio = (descriptor.Turn.SoftPrio = descriptor.Prio);
            if (this.m_bot.RequestAction(descriptor))
            {
                this.LookAction = descriptor;
            }
        }
        public void OnLookActionEvent(PlayerBotActionBase.Descriptor descBase)
        {
            if (descBase.Pointer != this.LookAction.Pointer)
            {
                base.PrintError("Rogue action.");
            }
            if (descBase.IsTerminated())
            {
                this.LookAction = null;
            }
        }
        public void OnUnlockCompleted(PlayerBotActionBase.Descriptor descBase)
        {
            if (descBase.Status == PlayerBotActionBase.Descriptor.StatusType.Successful)
            {
                this.state = State.Press;
            }
            else
            {
                this.state = State.Failed;
            }
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