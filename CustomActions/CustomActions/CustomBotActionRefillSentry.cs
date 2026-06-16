using AK;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Player;
using System;
using UnityEngine;
namespace BotControl.CustomActions.CustomActions
{

    public class CustomBotActionRefillSentry : CustomActionBase // TODO handle sound, and get rid of item when empty.
    {
        //This is an example of how you can set up your own custom action!
        public static new bool Setup() //This will be called when your class is regestered, it should return true if your action will even activate on it's own, or false if it's an exclusively manual action.
        {
            return true;
        }
        public new class Descriptor : CustomActionBase.Descriptor
        {
            public SentryGunInstance TargetSentry;
            public float Haste;
            private PlayerManager.PositionReservation m_posReservation;
            private PlayerManager.ObjectReservation m_objReservation;
            public static AccessLayers s_RequiredLayers = AccessLayers.Legs | AccessLayers.Hip | AccessLayers.RootPosition | AccessLayers.LookDirection | AccessLayers.WieldPrimary;
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
                this.RequiredLayers = s_RequiredLayers;
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
                return new CustomBotActionRefillSentry(this);
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
                if (this.m_posReservation == null)
                {
                    this.m_posReservation = new PlayerManager.PositionReservation
                    {
                        CharacterID = this.Bot.Agent.CharacterID,
                        Position = this.TargetSentry.transform.position,
                        Radius = 0.5f
                    };
                }
                else
                {
                    this.m_posReservation.Position = this.TargetSentry.transform.position;
                }
                PlayerManager.Current.AddPositionReservation(this.m_posReservation);
                GameObject gameObject = null;
                if (this.TargetSentry != null && this.TargetSentry.gameObject != null)
                {
                    gameObject = this.TargetSentry.gameObject;
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
        protected enum State
        {
            Idle,
            Move,
            StartApply,
            Applying,
            Finished,
            Failed,
        }
        private SentryGunInstance TargetSentry;
        private float Haste;
        private static float s_ApproachRadius = 1.1f;
        private static float s_VerifyRadiusMul = 1.15f;
        private static float s_duration = 1.5f;
        private Descriptor m_desc;
        private PlayerInventorySynced m_inventory;
        private State state;
        private PlayerBotActionTravel.Descriptor m_travelAction;
        private PlayerBotActionEquipItem.Descriptor m_equipAction;
        private PlayerBotActionLook.Descriptor m_lookAction;
        private float ammoutCanGivePercent;
        private ItemEquippable m_item;
        private PlayerAmmoStorage AmmoStorage;
        private PlayerBackpack backpack;
        private float m_applyStartTime;
        public CustomBotActionRefillSentry() : base(ClassInjector.DerivedConstructorPointer<CustomBotActionRefillSentry>())// Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionRefillSentry(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionRefillSentry(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<CustomBotActionRefillSentry>())
        {
            ClassInjector.DerivedConstructorBody(this);
            InitFromDescriptor(desc);
            //Use this constructor.
            //This means your action is starting!
            m_desc = desc;
            TargetSentry = m_desc.TargetSentry;
            Haste = m_desc.Haste;
            this.state = State.Idle;
        }
        public override void Stop()
        {
            //This is called when your action is told to stop.
            //Be sure to do any cleanup if you need to.
            base.Stop();
            SafeStopAction(m_travelAction);
            SafeStopAction(m_lookAction);
            SafeStopAction(m_equipAction);
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
                case State.StartApply:
                    UpdateStateStartApply();
                    break;
                case State.Applying:
                    UpdateStateApplying();
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
                DestinationPos = TargetSentry.transform.position,
                DestinationType = PlayerBotActionTravel.Descriptor.DestinationEnum.Position,
                ParentActionBase = this,
                Prio = m_desc.Prio,
                Haste = Haste,
                Radius = 1f,
                Persistent = true,
            };
            Desc.EventDelegate = DelegateSupport.ConvertDelegate<PlayerBotActionBase.Descriptor.EventDelegateFunc>(new Action<PlayerBotActionBase.Descriptor>(OnTravelCompleted));
            SafeStopAction(m_lookAction);
            m_lookAction = null;
            if (this.m_bot.RequestAction(Desc))
            {
                this.m_travelAction = Desc;
                this.state = State.Move;
                //FlexibleMethodDefinition callback = new(OnTravelCompleted, [m_travelAction]);
                //zActionSub.addOnTerminated(m_travelAction, callback);
            }
            else
            {
                this.state = State.Failed;
            }
        }
        private bool VerifyTarget()
        {
            if (TargetSentry == null)
                return false;
            if (!TargetSentry.NeedToolAmmo())
                return false;
            backpack = PlayerBackpackManager.GetBackpack(m_agent.Owner);
            if (!backpack.TryGetBackpackItem(InventorySlot.ResourcePack, out BackpackItem item))
                return false;
            if (item == null)
                return false;
            if (item.ItemID != 127) // Tool refill pack
                return false;
            this.m_inventory = this.m_agent?.Inventory.Cast<PlayerInventorySynced>();
            if (m_inventory == null)
                return false;
            m_item = item.Instance.Cast<ItemEquippable>();
            AmmoStorage = PlayerBackpackManager.GetBackpack(m_bot.Agent.Owner).AmmoStorage;
            if (AmmoStorage == null) 
                return false;
            ammoutCanGivePercent = AmmoStorage.ResourcePackAmmo.CostOfBullet / AmmoStorage.ResourcePackAmmo.AmmoMaxCap;
            if (!AmmoStorage.ResourcePackAmmo.HasBulletsLeft)
                return false;
            if (!zHelpers.CanBotReach(m_bot, TargetSentry.transform.position))
                return false;
            return true;
        }
        public void OnTravelCompleted(PlayerBotActionBase.Descriptor descBase)
        {
            if (this.state == State.Move && descBase.Status != PlayerBotActionBase.Descriptor.StatusType.Successful)
                this.state = State.Failed;
        }
        private void UpdateLookAction()
        {
            if (this.m_lookAction != null)
            {
                return;
            }
            PlayerBotActionLook.Descriptor descriptor = new PlayerBotActionLook.Descriptor(this.m_bot, true)
            {
                ParentActionBase = this,
                Prio = this.m_desc.Prio,
                Haste = 0.5f,
                TargetType = PlayerBotActionLook.TargetTypeEnum.Object,
                TargetObj = TargetSentry.transform
            };
            descriptor.EventDelegate = DelegateSupport.ConvertDelegate<PlayerBotActionBase.Descriptor.EventDelegateFunc>(new Action<PlayerBotActionBase.Descriptor>(OnLookActionEvent));
            descriptor.Turn.Prio = (descriptor.Turn.SoftPrio = descriptor.Prio);
            if (this.m_bot.RequestAction(descriptor))
            {
                this.m_lookAction = descriptor;
            }
        }
        private void UpdateStateMove()
        {
            if (!VerifyTarget())
                state = State.Failed;
            UpdateLookAction();
            UpdateEquipAction();
            if ((m_bot.transform.position - TargetSentry.transform.position).magnitude < 1.5f)
                state = State.StartApply;
        }
        private bool UpdateEquipAction()
        {
            if (this.m_equipAction == null && this.m_inventory.WieldedItem != this.m_item)
            {
                PlayerBotActionEquipItem.Descriptor descriptor = new PlayerBotActionEquipItem.Descriptor(this.m_bot)
                {
                    ParentActionBase = this,
                    Prio = this.m_desc.Prio,
                    Haste = this.m_desc.Haste,
                    Item = this.m_item
                };
                descriptor.EventDelegate = DelegateSupport.ConvertDelegate<PlayerBotActionBase.Descriptor.EventDelegateFunc>(new Action<PlayerBotActionBase.Descriptor>(OnEquipActionEvent));
                if (this.m_bot.RequestAction(descriptor))
                {
                    this.m_equipAction = descriptor;
                }
                return false;
            }
            return true;
        }
        public void OnLookActionEvent(PlayerBotActionBase.Descriptor descBase)
        {
            if (descBase.Pointer != this.m_lookAction.Pointer)
            {
                base.PrintError("Rogue action.");
            }
            if (this.m_lookAction.Status != PlayerBotActionBase.Descriptor.StatusType.Active)
            {
                this.m_lookAction = null;
            }
        }
        public void OnEquipActionEvent(PlayerBotActionBase.Descriptor descBase)
        {
            if (descBase.Pointer != this.m_equipAction.Pointer)
            {
                base.PrintError("Rogue action.");
            }
            if (descBase.IsTerminated())
            {
                this.m_equipAction = null;
            }
        }
        private void UpdateStateStartApply()
        {
            m_applyStartTime = Time.time;
            state = State.Applying;
        }

        private void UpdateStateApplying()
        {
            if (!this.UpdateEquipAction())
                return;
            if (Time.time - this.m_applyStartTime > PlayerBotActionShareResourcePack.s_duration)
            {
                Apply();
            }
        }
        private void Apply()
        {
            AmmoStorage.UpdateBulletsInPack(AmmoType.ResourcePackRel, -1);
            TargetSentry.GiveAmmoRel(m_agent, 0, 0, ammoutCanGivePercent);
            state = State.Finished;
            m_agent.Sound.Post(EVENTS.AMMOPACK_APPLY, true);
            if (AmmoStorage.GetBulletsInPack(AmmoType.ResourcePackRel) < 1)
            {
                PlayerDialogManager.WantToStartDialog(138U, m_agent);
                backpack.TryClearSlot(InventorySlot.ResourcePack);
                m_agent.Sound.Post(EVENTS.PACK_DISCARD, true);
            }
            this.m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Successful);
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