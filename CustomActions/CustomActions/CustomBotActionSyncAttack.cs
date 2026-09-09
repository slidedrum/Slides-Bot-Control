using BotControl.SmartSelect.PressActions;
using Enemies;
using Gear;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Player;
using System;
using UnityEngine;
namespace BotControl.CustomActions.CustomActions
{

    public class CustomBotActionSyncAttack : CustomActionBase // TODO make sure they line up a headshot, might reuqire bots to re-position.  might require my own implementation of findvunerabletarget
    {
        public enum State
        {
            Idle,
            Move,
            Charge,
            Wait,
            Strike,
            Striking,
            Finished,
            Failed,
        }
        private State state;
        private Descriptor m_desc;
        internal static float LastDamageDeltTimestamp = 0f;
        private EnemyAgent TargetAgent;
        private PlayerBotActionWalk.Descriptor.PostureEnum Posture;
        private PlayerBotActionTravel.Descriptor TravelAction;
        private PlayerBotActionMelee.Descriptor MeleAction;
        private float Haste;
        public static new bool Setup() //This will be called when your class is regestered, it should return true if your action will even activate on it's own, or false if it's an exclusively manual action.
        {
            return true;
        }
        public new class Descriptor : CustomActionBase.Descriptor
        {
            //This is an example of how you can set up your own custom descriptor!
            public float Haste = 1f;
            public PlayerBotActionWalk.Descriptor.PostureEnum Posture;
            public EnemyAgent TargetAgent;
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
                this.RequiredLayers = AccessLayers.None;
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
                return new CustomBotActionSyncAttack(this);
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
            public override void CompareAction(PlayerAIBot bot, ref PlayerBotActionBase.Descriptor bestAction)
            {
                //Should your action be queued?
                //This gets called every frame
                //Be sure to compare priority against the current best action.
                //Best action inludes vanilla actions.
                //Be sure to not set this to best action if it's already active.
            }

        }

        private static float MeleDistance = 2f;//PlayerBotActionMelee.s_distanceCheckThresholdSQ * PlayerBotActionMelee.s_chargeMaxDistanceSQ;

        public CustomBotActionSyncAttack() : base(ClassInjector.DerivedConstructorPointer<CustomBotActionSyncAttack>())// Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionSyncAttack(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionSyncAttack(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<CustomBotActionSyncAttack>())
        {
            ClassInjector.DerivedConstructorBody(this);
            InitFromDescriptor(desc);
            m_desc = desc;
            this.m_desc = desc;
            this.TargetAgent = desc.TargetAgent;
            this.Haste = desc.Haste;
            this.state = State.Idle;
            this.Posture = desc.Posture;
            //Use this constructor.
            //This means your action is starting!
        }
        public override void Stop()
        {
            //This is called when your action is told to stop.
            //Be sure to do any cleanup if you need to.
            base.Stop();
            if (TravelAction != null && !TravelAction.IsTerminated())
                SafeStopAction(TravelAction);
            if (MeleAction != null && !MeleAction.IsTerminated())
                SafeStopAction(MeleAction);
            if (this.IsActive())
                m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Failed);
        }
        private bool Verify(bool ignoreWake = false)
        {
            if (!VerifyTarget(ignoreWake))
            {
                state = State.Failed;
                return false;
            }
            if (!VerifyPosition())
            {
                state = State.Move;
                return false;
            }
            if (m_bot.Agent.m_attackers.Count != 0)
            {
                state = State.Failed;
                return false;
            }
            return true;
        }
        private bool VerifyTarget(bool ignoreWake = false)
        {
            if (TargetAgent == null)
                return false;
            if (!TargetAgent.gameObject.activeInHierarchy)
                return false;
            if (!zHelpers.CanBotReach(m_bot, TargetAgent.transform.position))
                return false;
            if (!m_desc.TargetAgent.Alive)
                return false;
            if (!ignoreWake && (!m_desc.TargetAgent.AI.IsHibernating(out bool _, out bool isWakingUp) || isWakingUp))
                return false;
            //todo verify that not under attack
            return true;
        }
        private bool VerifyPosition()
        {
            float Distance = (m_agent.transform.position - TargetAgent.Position).sqrMagnitude;
            return Distance <= MeleDistance * MeleDistance;
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
                case State.Charge:
                    UpdateStateCharge();
                    break;
                case State.Wait:
                    UpdateStateWait();
                    break;
                case State.Strike:
                    UpdateStateStrike();
                    break;
                case State.Striking:
                    UpdateStateStriking();
                    break;
                case State.Finished:
                    UpdateStateFinished();
                    break;
                case State.Failed:
                    UpdateStateFailed();
                    break;
            }
            //Your stuff goes here
            return !base.IsActive();
        }

        private void UpdateStateIdle()
        {
            if (!VerifyTarget())
            {
                state = State.Failed;
                return;
            }
            if (VerifyPosition())
            {
                state = State.Charge;
                return;
            }
            state = State.Move;
        }
        private void UpdateStateMove()
        {
            if (!VerifyTarget())
            {
                state = State.Failed;
                return;
            }
            if (VerifyPosition())
            {
                state = State.Charge;
                return;
            }
            if (TravelAction == null || TravelAction.IsTerminated())
            {
                PlayerAgent agent = m_bot.Agent;
                TravelAction = new(m_bot)
                {
                    DestinationObject = TargetAgent.gameObject,
                    Haste = Haste,
                    Radius = 0.7f * MeleDistance,
                    DestinationType = PlayerBotActionTravel.Descriptor.DestinationEnum.GameObject,
                    WalkPosture = Posture,
                    Persistent = false,
                    ParentActionBase = this,
                    Prio = m_desc.Prio,
                };
                m_bot.RequestAction(TravelAction);
            }
        }
        private void UpdateStateCharge()
        {
            Verify();
            if (TravelAction != null)
                m_bot.StopAction(TravelAction);
            if (MeleAction == null || MeleAction.IsTerminated())
            {
                PlayerAgent agent = m_bot.Agent;
                if (!m_bot.m_backpack.TryGetBackpackItem(InventorySlot.GearMelee, out BackpackItem meleeBackpackItem))
                    Stop();
                // ...
                MeleeWeaponThirdPerson meleeWeapon = meleeBackpackItem.Instance.TryCast<MeleeWeaponThirdPerson>();
                MeleAction = new(m_bot)
                {
                    TargetAgent = TargetAgent,
                    Haste = m_desc.Haste,
                    Strike = false,
                    Travel = false,
                    TargetGameObject = TargetAgent.m_headLimb.gameObject,
                    Weapon = meleeWeapon,
                    ParentActionBase = this,
                    Prio = m_desc.Prio,
                };
                m_bot.RequestAction(MeleAction);
            }
            if (MeleAction.IsCharged)
            {
                zChatHandler.sendChatMessage("Ready to strike!", "Sync" + IPressAction.chatPermSuffix, m_bot.Agent);
                state = State.Wait;
            }
        }
        private void UpdateStateWait()
        {
            if (!Verify(true))
                return;
            if (!m_desc.TargetAgent.AI.IsHibernating(out bool _, out bool isWakingUp) || isWakingUp)
            {
                state = State.Strike;
                return;
            }
            if (MeleAction == null || MeleAction.IsTerminated())
            {
                state = State.Charge;
                return;
            }
            if (Time.time - LastDamageDeltTimestamp < 1)
                state = State.Strike;
        }
        private void UpdateStateStrike()
        {
            if (!Verify())
                return;
            if (MeleAction == null || MeleAction.IsTerminated())
            {
                state = State.Charge;
                return;
            }
            MeleAction.Strike = true;
            state = State.Striking;
        }
        private void UpdateStateStriking()
        {
            if (!Verify())
                return;
            if (MeleAction.IsTerminated())
            {
                if (MeleAction.IsCompleted())
                    state = State.Finished;
                else
                    state = State.Failed;
                return;
            }
        }
        private void UpdateStateFinished()
        {
            m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Successful);
            Stop();
        }
        private void UpdateStateFailed()
        {
            m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Failed);
            Stop();
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
    [HarmonyPatch]
    public static class EnemyTakesDamagePatch
    {
        [HarmonyPatch(typeof(EnemyAgent), nameof(EnemyAgent.OnTakeDamage))]
        [HarmonyPostfix]
        public static void PostOnTakeDamagePatch(PlayerBotActionAttack __instance)
        {
            CustomBotActionSyncAttack.LastDamageDeltTimestamp = Time.time;
        }
    }
}