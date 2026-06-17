using AIGraph;
using Enemies;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Player;
using System;
using UnityEngine;
namespace BotControl.CustomActions.CustomActions
{

    public class CustomBotActionManualAttack : CustomActionBase
    {
        //This is an example of how you can set up your own custom action!
        public static new bool Setup() //This will be called when your class is regestered, it should return true if your action will even activate on it's own, or false if it's an exclusively manual action.
        {
            return true;
        }
        public new class Descriptor : CustomActionBase.Descriptor
        {
            //public static PlayerBotActionBase.AccessLayers s_RequiredLayers = PlayerBotActionBase.AccessLayers.LeftArm | PlayerBotActionBase.AccessLayers.RightArm | PlayerBotActionBase.AccessLayers.LookDirection | PlayerBotActionBase.AccessLayers.WieldPrimary;
            public EnemyAgent TargetAgent;
            public PlayerBotActionAttack.AttackMeansEnum Means = PlayerBotActionAttack.AttackMeansEnum.Melee;
            public PlayerBotActionWalk.Descriptor.PostureEnum Posture =  PlayerBotActionWalk.Descriptor.PostureEnum.Crouch;
            public PlayerBotActionAttack.StanceEnum stance = PlayerBotActionAttack.StanceEnum.All;
            public float Haste = 0.5f;
            public bool MovementAllowed = true;
            public PlayerAgent Commander;
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
                this.RequiredLayers = AccessLayers.None;
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
                return new CustomBotActionManualAttack(this);
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
        public enum State
        {
            Idle,
            Move,
            Attack,
            Finished,
            Failed,
        }
        private State state;
        private PlayerBotActionAttack.StanceEnum stance;
        private EnemyAgent TargetAgent;
        private PlayerBotActionAttack.AttackMeansEnum Means;
        private PlayerBotActionWalk.Descriptor.PostureEnum Posture;
        private float Haste = 0.85f;
        private bool MovementAllowed;
        private PlayerAgent Commander;
        private Descriptor m_desc;
        private PlayerBotActionAttack.Descriptor AttackAction;
        private PlayerBotActionTravel.Descriptor TravelAction;
        private PlayerBotActionMelee.Descriptor  MeleAction;
        private static float walkNoiseCheckInterval = 1;
        private static float nextWalkNoiseCheckTimestamp = 0;
        private static float walkNoiseChance = 1f / 50f;
        private static float hitNoiseChance = 1f / 10f;
        private Vector3 m_lastSleeperCheckPosition = Vector3.zero;
        private PlayerBotActionAttack AttackBase;
        private static INM_NoiseMaker noisemaker;
        private bool wasCooldownState = false;

        public CustomBotActionManualAttack() : base(ClassInjector.DerivedConstructorPointer<CustomBotActionManualAttack>())// Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);

        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionManualAttack(IntPtr ptr) : base(ptr) // Don't use this!  Needed for il2cpp nonsense.
        {
            ClassInjector.DerivedConstructorBody(this);
        }// Don't use this!  Needed for il2cpp nonsense.
        public CustomBotActionManualAttack(Descriptor desc) : base(ClassInjector.DerivedConstructorPointer<CustomBotActionManualAttack>())
        {
            ClassInjector.DerivedConstructorBody(this);
            InitFromDescriptor(desc);
            this.m_desc = desc;
            this.TargetAgent = desc.TargetAgent;
            this.Means = desc.Means;
            this.Posture = desc.Posture;
            this.Haste = desc.Haste;
            this.MovementAllowed = desc.MovementAllowed;
            this.Commander = desc.Commander;
            this.state = State.Idle;
            this.stance = desc.stance;
            //Use this constructor.
            //This means your action is starting!
        }
        public override void Stop()
        {
            //This is called when your action is told to stop.
            //Be sure to do any cleanup if you need to.
            base.Stop();
            SafeStopAction(AttackAction);
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
                case State.Attack:
                    UpdateStateAttack();
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

        private void UpdateStateIdle()
        {
            if (!VerifyTarget())
            {
                state = State.Failed;
            }
            PlayerBotActionAttack.Descriptor Desc = new(m_bot)
            {
                ParentActionBase = this,
                Prio = m_desc.Prio,
                Haste = Haste,
                TargetAgent = TargetAgent,
                Means = Means,
                Posture = Posture,
                Stance = stance,
                MovementAllowed = MovementAllowed,
            };
            if (this.m_bot.RequestAction(Desc))
            {
                this.AttackAction = Desc;
                this.state = State.Move;
            }
            else
            {
                this.state = State.Failed;
            }
        }

        private bool VerifyTarget()
        {
            if (TargetAgent == null)
                return false;
            if (!TargetAgent.gameObject.activeInHierarchy)
                return false;
            if (Commander == null)
                Commander = m_bot.SyncValues.Leader;
            if (!zHelpers.CanBotReach(m_bot, TargetAgent.transform.position))
                return false;
            return true;
        }

        private void UpdateStateMove()
        {
            //AttackAction?.ActionBase?.Cast<PlayerBotActionAttack>().m_meleeAction.ActionBase.Cast<PlayerBotActionMelee>().m_travelAction
            if (MeleAction == null || MeleAction.TargetAgent == null)
                MeleAction = AttackAction?.ActionBase?.Cast<PlayerBotActionAttack>()?.m_meleeAction?.Cast<PlayerBotActionMelee.Descriptor>();
            if (TravelAction == null || TravelAction.DestinationPos == Vector3.zero)
                TravelAction = MeleAction?.ActionBase?.Cast<PlayerBotActionMelee>()?.m_travelAction?.Cast<PlayerBotActionTravel.Descriptor>();
            if (TravelAction == null || (TravelAction.DestinationPos == Vector3.zero && TravelAction.DestinationObject == null))
                return; // attack instance not created yet; try again next frame
            if (TravelAction.IsTerminated())
                state = State.Attack;
            if (Time.time > nextWalkNoiseCheckTimestamp)
            {
                nextWalkNoiseCheckTimestamp = Time.time + walkNoiseCheckInterval;
                if (UnityEngine.Random.value < walkNoiseChance)
                    MakePlayerNoise(m_agent);
            }
        }
        private static void MakePlayerNoise(PlayerAgent player, float radius = 15f)
        {
            if (player == null || !player.Alive)
                return;

            AIG_CourseNode node = player.CourseNode;
            if (node == null)
                return;

            NM_NoiseData noise = new();
            noise.noiseMaker = player.Cast<INM_NoiseMaker>();
            noise.position = player.transform.position;
            noise.radiusMin = 0f;
            noise.radiusMax = radius;
            noise.yScale = 1f;
            noise.node = node;
            noise.type = NM_NoiseType.InstaDetect;
            noise.includeToNeightbourAreas = true;
            noise.raycastFirstNode = false;

            NoiseManager.MakeNoise(noise);
        }
        private void UpdateStateAttack()
        {
            if (MeleAction.Strike == false)
                MeleAction.Strike = true;
            //m_strikeDelayTimestamp = Time.time;

            if (AttackAction.IsTerminated())
            {
                if (AttackAction.Status == PlayerBotActionBase.Descriptor.StatusType.Successful)
                {
                    if (UnityEngine.Random.value < hitNoiseChance)
                    {
                        MakePlayerNoise(m_agent);
                    }
                    state = State.Finished;
                }
                else
                    state = State.Failed;
                return;
            }
            if (!wasCooldownState == false && MeleAction.State == PlayerBotActionMelee.Descriptor.StateEnum.Cooldown && MeleAction.TargetAgent.Alive == true)
            {
                wasCooldownState = true;
                if (UnityEngine.Random.value < hitNoiseChance * 2)
                {
                    MakePlayerNoise(m_agent);
                }
            }
            if (MeleAction.State != PlayerBotActionMelee.Descriptor.StateEnum.Cooldown)
                wasCooldownState = false;

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
    public static class StrikeFix
    {
        [HarmonyPatch(typeof(PlayerBotActionAttack), nameof(PlayerBotActionAttack.UpdateMeleeAttack))]
        [HarmonyPostfix]
        public static void PostUpdateMeleeAttack(PlayerBotActionAttack __instance) // This is needed to fix the "staring at an enemy" bug
        {
            if (__instance.m_meleeAction == null)
                return;
            if (!zActions.isManualAction(__instance.m_meleeAction))
                return;
            if (__instance.m_meleeAction.Strike == false)
                __instance.m_meleeAction.Strike = true;
        }
        //[HarmonyPatch(typeof(PlayerBotActionMelee), nameof(PlayerBotActionMelee.UpdateStateCharge))]
        //[HarmonyPrefix]
        //public static void PreUpdateStateCharge(PlayerBotActionMelee __instance)
        //{
        //    var desc = __instance.m_desc;
        //    if (!zActions.isManualAction(desc))
        //        return;
        //    if (desc.Strike == false)
        //        desc.Strike = true;
        //}
    }
}