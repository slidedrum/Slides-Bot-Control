using BotControl.SmartSelect.PressActions;
using BotControl.SmartSelect.PressTypes;
using Player;
using SlideDrum.sInputSystem;
using SlideMenu;
using UnityEngine;

namespace BotControl.SmartSelect
{
    public static class zSmartSelect
    {
        // This class handles everything with the smart select button (V)
        // Tapping and holding the button will tell the bot to move there.
        // You can also do a bunch of other actions:
        //
//            (   TAP        /     HOLD      /   DOUBLE TAP  /  TAP & HOLD   )
//            ( ------------------------------------------------------------ )
// Player/Bot ( ---Select--- / ----Share---- / ---Follow---- / ---Send To--- ) PlayerAgent
//       Item ( ------------ / ---Pickup---- / ------------- / ------------- ) ItemInLevel
//  Equipment ( ---Pickup--- / --*Refill*--- / -Pickup All-- / ------------- ) SentryGunInstance
//  Container ( ------------ / ---*Open*---- / --*Place?*--- / ------------- ) LG_WeakResourceContainer
// Floor/Wall ( ------------ / -Consumable-- / --Equipment-- / ----Move----- ) Raycast normal
//    Holding ( ------------ / -*Drop Here*- / --Drop Now--- / ------------- ) Raycast normal
//       Door ( ----Open---- / -Throw cFoam- / --*Break?*--- / ------------- ) LG_WeakDoor
//       Lock ( ---Unlock--- / -Lock Melter- / ------------- / ------------- ) LG_WeakLock
//      Enemy ( ------------ / --*Attack*--- / -*Countdown*- / ------------- ) EnemyAgent //use voiceline PLAY_CL_THREETWOONEGO
//  Generator ( ------------ / *Place Cell*- / ------------- / ------------- ) LG_PowerGenerator_Core 
//    Look Up ( ---Cancel--- / --Deselect--- / -Cancel All-- / -*Select A*-- ) 
//  Look Down ( ---Follow--- / -Share Self-- / ------------- / --A Follow--- )

        //TODO lock melter

        public static Selection MainSelection = new();
        private static bool IsSetUp = false;
        public static uint InvalidSound = AK.EVENTS.MENU_HOST_EXPEDITION_BUTTON_RELEASE;
        public static uint CorrectSound = AK.EVENTS.MENU_HOST_EXPEDITION_BUTTON_FULL;
        private static float lastSlowUpdateTime = 0;
        private static float SelectionAngle = 30f;
        public static bool FallbackToClosest = true;
        private static float now => Time.time;
        private static float roundedTime => now - (now % slowupdateinterval);
        private const float slowupdateinterval = 0.1f;
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
            sInputSystem.Update();
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
            foreach (IPressType pressType in PressTypeManager.TypeMap.Values)
            {
                sInputSystem.AddListener(pressType.PressSequence, new FlexibleMethodDefinition(pressType.Invoke), KeyCode.V);
            }
            //sInputSystem.AddListener(sInputSystemDefaults.OnTappedExclusive, new FlexibleMethodDefinition(OnKeyTap), KeyCode.V);
            //sInputSystem.AddListener(sInputSystemDefaults.OnHoldImmediateExclusive, new FlexibleMethodDefinition(OnKeyHold), KeyCode.V);
            //sInputSystem.AddListener(sInputSystemDefaults.OnDoubleTappedExclusive, new FlexibleMethodDefinition(OnKeyDoubleTap), KeyCode.V);
            //sInputSystem.AddListener(sInputSystemDefaults.OnTapAndHoldImmediateExclusive, new FlexibleMethodDefinition(OnTapAndHold), KeyCode.V);
            
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
