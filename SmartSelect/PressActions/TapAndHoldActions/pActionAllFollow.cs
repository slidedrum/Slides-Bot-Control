using BepInEx.Unity.IL2CPP.Utils;
using BotControl.SmartSelect.PressTypes;
using Il2CppInterop.Runtime;
using Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionAllFollow : IPressAction
    {
        public string FriendlyName => "All Folow";
        private string _FriendlyNameShort = "A-Follow";
        public string FriendlyIdentifier => "Follow";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        private static List<PlayerAIBot> NonFollowingBots = new();
        private int cycleOffset;
        public Il2CppSystem.Type Type => null;
        public int? Priority => 20;
        public string pressTypeIdentifier => "Tap and Hold";
        public bool Invoke(Component BestComponent)
        {
            List<PlayerAIBot> AllBots = ZiMain.GetBotList();
            NonFollowingBots.Clear();
            foreach (PlayerAIBot Bot in AllBots)
                if (Bot.SyncValues.Leader != zStaticRefrences.LocalPlayer)
                    NonFollowingBots.Add(Bot);
            if (NonFollowingBots.Count == 0) return false;
            zUpdater.Instance.StartCoroutine(AllBotsFollow());
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_FOLLOWME);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Follow me!", 1f);
            return true;
        }
        public static IEnumerator AllBotsFollow()
        {
            List<PlayerAIBot> Botlist = new List<PlayerAIBot>(NonFollowingBots);
            foreach (var bot in Botlist)
            {
                pActionFollowDirect.StaticCallAgentToFollow(bot.Agent, zStaticRefrences.LocalPlayer, false);
                //zChatHandler.sendChatMessage("On the way.", "Follow" + "NotifyActionAcknowlage", bot.Agent, zStaticRefrences.LocalPlayer);
                yield return new WaitForSeconds(1f);
            }
        }
        public bool IsActionValid(Component candidate)
        {
            bool LookingDown = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.down) < 15f;
            if (!LookingDown)
                return false;
            List<PlayerAIBot> AllBots = ZiMain.GetBotList();
            NonFollowingBots.Clear();
            foreach (PlayerAIBot Bot in AllBots)
                if (Bot.SyncValues.Leader != zStaticRefrences.LocalPlayer)
                    NonFollowingBots.Add(Bot);
            if (NonFollowingBots.Count == 0) return false;
            cycleOffset++;
            int groupIndex = (cycleOffset - 1) / 1;
            NonFollowingBots.Sort((a, b) =>
                a.GetInstanceID().CompareTo(b.GetInstanceID()));
            int index = groupIndex % NonFollowingBots.Count;
            Color = NonFollowingBots[index].Agent.Owner.PlayerColor;
            return true;
        }
    }
}
