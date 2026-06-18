using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using Player;
using System;
using UnityEngine;
namespace BotControl.CustomActions.CustomActions
{

    public class CustomBotActionInsertPowerCell : CustomActionBase
    {
        //This is an example of how you can set up your own custom action!
        public static new bool Setup() //This will be called when your class is regestered, it should return true if your action will even activate on it's own, or false if it's an exclusively manual action.
        {
            return true;
        }
        public new class Descriptor : CustomActionBase.Descriptor // TODO interact animation
        {
            //This is an example of how you can set up your own custom descriptor!
            public LG_PowerGenerator_Core TargetGenerator;
            public float Haste = 1f;
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
                return new CustomBotActionInsertPowerCell(this);
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
            StartInsert,
            Inserting,
            Finished,
            Failed,
        }

        private Descriptor m_desc;
        private LG_PowerGenerator_Core TargetGenerator;
        private PlayerBotActionTravel.Descriptor TravelAction;
        private PlayerBotActionLook.Descriptor LookAction;
        private GameObject m_powerCellInteractionObject;
        private BackpackItem item;
        private Vector3? TargetPosition = null;
        private float startInsertTimestamp = 0;
        private float insertTime = 1f;
        private float Haste;
        State state;
        public CustomBotActionInsertPowerCell() : base(ClassInjector.DerivedConstructorPointer<CustomBotActionInsertPowerCell>())// Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionInsertPowerCell(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionInsertPowerCell(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<CustomBotActionInsertPowerCell>())
        {
            ClassInjector.DerivedConstructorBody(this);
            InitFromDescriptor(desc);
            this.m_desc = desc;
            this.TargetGenerator = desc.TargetGenerator;
            this.Haste = desc.Haste;
            this.state = State.Idle;
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
        }
        public override bool Update()
        {
            //This is called every frame when your action is active.
            if (base.Update())
                return true;
            switch (state)
            {
                case State.Idle:
                    UpdateIdle();
                    break;
                case State.Move:
                    UpdateMove();
                    break;
                case State.StartInsert:
                    UpdateStartInsert();
                    break;
                case State.Inserting:
                    UpdateInserting();
                    break;
                case State.Finished:
                    this.m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Successful);
                    return true;
                case State.Failed:
                    this.m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Failed);
                    return true;
            }
            //Your stuff goes here
            return !base.IsActive();
        }

        private void UpdateIdle()
        {
            if (!VerifyTarget())
            {
                state = State.Failed;
                return;
            }
            PlayerBotActionTravel.Descriptor Desc = new(m_bot)
            {
                DestinationPos = (Vector3)TargetPosition,
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
            }
            else
            {
                this.state = State.Failed;
            }
        }

        private bool VerifyTarget()
        {
            if (TargetGenerator == null)
                return false;
            if (m_powerCellInteractionObject == null)
                m_powerCellInteractionObject = TargetGenerator.m_powerCellInteraction.Cast<LG_GenericCarryItemInteractionTarget>().gameObject;
            if (!m_powerCellInteractionObject.activeInHierarchy)
                return false;
            if (!zHelpers.TryGetAgentBackpackItem(m_agent, InventorySlot.InLevelCarry, out item))
                return false;
            if (item.ItemID != 131) // Power Cell
                return false;
            GetTargetPosition();
            if(!zHelpers.CanBotReach(m_bot, (Vector3)TargetPosition))
                return false;
            return true;

        }
        private bool GetTargetPosition()
        {
            if (this.TargetPosition != null)
                return true;
            GameObject gobject = TargetGenerator.gameObject;
            if (gobject == null)
                return false;
            Transform transform = gobject.transform;
            Vector3 location = transform.position + transform.forward * 1.5f;
            if (!SnapPositionToNav(location, out Vector3 TargetPosition))
                return false;
            this.TargetPosition = TargetPosition;
            return true;
        }
        private void UpdateMove()
        {
            UpdateLookAction();
            if ((m_bot.transform.position - (Vector3)TargetPosition).magnitude < 0.1f)
                state = State.StartInsert;
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
                TargetObj = TargetGenerator.transform,
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

        private void UpdateStartInsert()
        {
            startInsertTimestamp = Time.time;
            pGenericInteractAnimation.TypeEnum reachHeight = base.GetReachHeight(TargetGenerator.transform.position.y);
            this.m_agent.Sync.SendGenericInteract(reachHeight, true);
            state = State.Inserting;
        }

        private void UpdateInserting()
        {
            UpdateLookAction();
            if (Time.time > startInsertTimestamp + insertTime)
            {
                state = State.Finished;
                TargetGenerator.AttemptPowerCellInsert(m_agent.Owner, item.Instance);
                foreach (var action in m_bot.Actions)
                {
                    if (action.TryCast<PlayerBotActionCarryExpeditionItem>() == null)
                        continue;
                    m_bot.StopAction(action.DescBase);
                    break;
                }
                SafeStopAction(TravelAction);
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