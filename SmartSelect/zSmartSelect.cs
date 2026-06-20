using BotControl.SmartSelect.PressActions;
using BotControl.SmartSelect.PressTypes;
using FlexMethodDefinition;
using InControl;
using Player;
using sInputSystem;
using UnityEngine;

namespace BotControl.SmartSelect
{
    public static class zSmartSelect
    {
// This class handles everything with the smart select button (V)
//
//            (      TAP     /     HOLD      /   DOUBLE TAP  /  TAP & HOLD   ) 
//            ( ------------------------------------------------------------ ) 
// Player/Bot ( ---Select--- / ----Share---- / Follow/Cancel / ---Send To--- )
//       Item ( ------------ / ---Picup---- / ------------- / ------------- )
//  Equipment ( ---Pickup--- / ---Refill---- / -Pickup All-- / ------------- )
//  Container ( ----Open---- / ------------- / --*Place?*--- / ------------- )
// Floor/Wall ( ------------ / -Consumable-- / --Equipment-- / ----Move----- )
//    Holding ( ------------ / -*Drop Here*- / --Drop Now--- / ------------- )
//       Door ( -Open/Close- / -Throw cFoam- / --*Break?*--- / ------------- )
//       Lock ( ---Unlock--- / -Lock Melter- / ------------- / ------------- )
// Enemy/Quiet( ------------ / Sneak Attack- / -*All Sync*-- / ---*Sync*---- )
// Enemy/Loud ( --*Target*-- / ------------- / ------------- / *All Target*- )
//  Generator ( ------------ / -Place Cell-- / ------------- / ------------- )
//    Look Up ( Cancel Last- / --Deselect--- / -Cancel All-- / -*Select A*-- ) 
//  Look Down ( ---Follow--- / -Share Self-- / ------------- / --A Follow--- ) 

        // TODO lock melter
        // TODO cancel bot's last
        // TODO cancel client last
        // TODO cancel client all

        public static Selection MainSelection = new();
        internal static bool IsSetUp = false;
        public static uint InvalidSound = AK.EVENTS.MENU_HOST_EXPEDITION_BUTTON_RELEASE;
        public static uint CorrectSound = AK.EVENTS.MENU_HOST_EXPEDITION_BUTTON_FULL;
        private static float lastSlowUpdateTime = 0;
        private static float SelectionAngle = 30f;
        public static bool FallbackToClosest = true;
        private static float now => Time.time;
        private static float roundedTime => now - (now % slowupdateinterval);
        private const float slowupdateinterval = 0.33f;
        public enum PressTypes
        {
            Tap,
            Hold,
            DoubleTap,
            TapAndHold,
        }
        public enum ActionTypes
        {
            Agent,
            Item,
            Sentry,
            Container,
            Nothing,
            Door,
            Enemy,
            Generator,
        }
        public static void Update()
        {
            bool ready = FocusStateManager.CurrentState == eFocusState.FPS || FocusStateManager.CurrentState == eFocusState.Dead;
            if (!ready) return;
            if (!IsSetUp) SetUp();
            if (roundedTime > lastSlowUpdateTime)
                SlowUpdate();
        }
        public static void SlowUpdate()
        {
            PressTypeManager.Update();
            zSmartSelectHud.Update();
            lastSlowUpdateTime = roundedTime;
        }
        private static void SetUp()
        {
            PressActionManager.Initialize();
            foreach (IInputType InputType in PressTypeManager.TypeMap.Values)
            {
                var seq = InputType.InputSequence;
                seq.ResetMatchState();
                InputSystem.AddListener(
                    seq,
                    (KeyCode)zSlideComputer.SmartSelectButton.Value,
                    new FlexibleMethodDefinition(InputType.Invoke)
                    );
                //InputSystem.AddListener(InputType.InputSequence, (KeyCode?)zSlideComputer.SmartSelectButton.Value, new FlexibleMethodDefinition(InputType.Invoke));
            }
            IsSetUp = true;
        }
        public static uint GetVoiceId(PlayerAgent Agent)
        {
            var botName = Agent.PlayerName;
            var botId = Agent.CharacterID;
            uint voiceID = 0u;

            if (botName.ToUpper().Contains("BISHOP"))
                voiceID = AK.EVENTS.PLAY_ADDRESSBISHOPIRRITATED01;
            if (botName.ToUpper().Contains("DAUDA"))
                voiceID = AK.EVENTS.PLAY_ADDRESSDAUDAIRRITATED01;
            if (botName.ToUpper().Contains("HACKET"))
                voiceID = AK.EVENTS.PLAY_ADDRESSHACKETTIRRITATED01;
            if (botName.ToUpper().Contains("WOODS"))
                voiceID = AK.EVENTS.PLAY_ADDRESSWOODSIRRITATED01;
            return voiceID;
        }
        public static PlayerAIBot GetBotLookingAt()
        {
            PlayerAIBot bot = zSearch.FindBestAligned(zStaticRefrences.CameraTransform, zStaticRefrences.AllBotObjects, SelectionAngle)?.GetComponent<PlayerAIBot>();
            return bot;
        }
        public static PlayerAgent GetPlayerAgentLookingAt()
        {
            PlayerAgent agent = zSearch.FindBestAligned(zStaticRefrences.CameraTransform, zStaticRefrences.AllPlayerAgentObjectsInLevel, SelectionAngle)?.GetComponent<PlayerAgent>();
            return agent;
        }
        //public static void OnKeyTap()
        //{
        //    if (TapPress.Invoke())
        //        ZiMain.PlayUiSound(CorrectSound);
        //    else
        //        ZiMain.PlayUiSound(InvalidSound);
        //}
        //public static void OnKeyHold()
        //{
        //    if (HoldPress.Invoke())
        //        ZiMain.PlayUiSound(CorrectSound);
        //    else
        //        ZiMain.PlayUiSound(InvalidSound);
        //}
        //public static void OnKeyDoubleTap()
        //{
        //    if (DoubleTapPress.Invoke())
        //        ZiMain.PlayUiSound(CorrectSound);
        //    else
        //        ZiMain.PlayUiSound(InvalidSound);
        //}
        //public static void OnTapAndHold()
        //{
        //    if (TapAndHoldPress.Invoke())
        //        ZiMain.PlayUiSound(CorrectSound);
        //    else
        //        ZiMain.PlayUiSound(InvalidSound);
        //}
    }
}
