using BotControl;
using BotControl.CustomActions;
using Il2CppInterop.Runtime.Injection;
using Player;
using SlideMenu;
using System;
using UnityEngine;
namespace BotControl.CustomActions.CustomActions
{


    public class DropHereAction : CustomActionBase
    {
        public static void TestStart(PlayerAIBot bot)
        {
            DropHereAction.Descriptor desc = new DropHereAction.Descriptor(bot)
            {
                DropPosition = zStaticRefrences.LocalPlayer.Position,
                Prio = 13
            };
            bot.StartAction(desc);
        }

        //This is an example of how you can set up your own custom action!
        public static new bool Setup() //This will be called when your class is regestered, it should return true if your action will even activate on it's own, or false if it's an exclusively manual action.
        {
            return false;
        }
        public new class Descriptor : CustomActionBase.Descriptor
        {
            public Vector3 DropPosition;
            //public static PlayerBotActionBase.AccessLayers s_RequiredLayers = PlayerBotActionBase.AccessLayers.Legs | PlayerBotActionBase.AccessLayers.Hip | PlayerBotActionBase.AccessLayers.Spine | PlayerBotActionBase.AccessLayers.LeftArm | PlayerBotActionBase.AccessLayers.RightArm | PlayerBotActionBase.AccessLayers.RootPosition | PlayerBotActionBase.AccessLayers.RootRotation | PlayerBotActionBase.AccessLayers.WieldPrimary;
            //This is an example of how you can set up your own custom descriptor!
            [Obsolete]
            public Descriptor() : base(ClassInjector.DerivedConstructorPointer<Descriptor>()) // Don't use this!  Needed for il2cpp nonsense.
            {
                ClassInjector.DerivedConstructorBody(this);
            } // Don't use this!  Needed for il2cpp nonsense.
            [Obsolete]
            public Descriptor(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
            {
                ClassInjector.DerivedConstructorBody(this);
            }  // Don't use this!  Needed for il2cpp nonsense.
            public Descriptor(PlayerAIBot bot) : base(ClassInjector.DerivedConstructorPointer<Descriptor>())
            {
                ClassInjector.DerivedConstructorBody(this);
                InitDescriptor(bot);
                //this.RequiredLayers = DropHereAction.Descriptor.s_RequiredLayers;
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
                return new DropHereAction(this);
            }
            public override bool IsActionAllowed(PlayerBotActionBase.Descriptor desc)
            {
                //Does your action play nice with this desc?
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
            Drop,
            Finished,
            Failed,
        }
        public PlayerBotActionTravel.Descriptor TravelAction;
        public PlayerBotActionCarryExpeditionItem.Descriptor DropAction;
        private DropHereAction.Descriptor m_desc;
        private State state;
        [Obsolete]
        public DropHereAction() : base(ClassInjector.DerivedConstructorPointer<DropHereAction>())// Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        [Obsolete]
        public DropHereAction(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public DropHereAction(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<DropHereAction>())
        {
            InitFromDescriptor(desc);
            this.m_desc = desc;
            this.state = State.Idle;
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
                    UpdateIdle();
                    break;
                case State.Move:
                    UpdateMove();
                    break;
                case State.Drop:
                    UpdateDrop();
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
        private bool VerifyDropPosition()
        {
            if (zHelpers.CanBotReach(m_bot, m_desc.DropPosition))
            {
                return true;
            }
            return false;
        }
        private bool VerifyBotCarrying()
        {
            if (!zHelpers.TryGetAgentBackpackItem(m_agent, InventorySlot.InLevelCarry, out var item))
            {
                return false;
            }
            return true;
        }
        private void UpdateIdle()
        {
            if (!VerifyDropPosition())
            {
                state = State.Failed;
            }
            if (!VerifyBotCarrying())
            {
                state = State.Failed;
            }
            PlayerBotActionTravel.Descriptor Desc = new(m_bot)
            {
                DestinationPos = m_desc.DropPosition,
                DestinationType = PlayerBotActionTravel.Descriptor.DestinationEnum.Position,
                ParentActionBase = this,
                Prio = m_desc.Prio,
                Persistent = false,
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
        private void UpdateMove()
        {

        }
        private void UpdateDrop()
        {
            foreach (var action in m_bot.Actions)
            {
                if (action.TryCast<PlayerBotActionCarryExpeditionItem>() == null)
                    continue;
                m_bot.StopAction(action.DescBase);
                break;
            }
            this.state = State.Finished;
        }
        public void OnTravelCompleted(PlayerBotActionBase.Descriptor descBase)
        {
            if (descBase.Status == PlayerBotActionBase.Descriptor.StatusType.Successful)
            {
                this.state = State.Drop;
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