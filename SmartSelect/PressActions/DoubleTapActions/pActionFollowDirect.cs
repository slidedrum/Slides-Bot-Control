using Agents;
using BepInEx.Unity.IL2CPP.Utils;
using BotControl.SmartSelect.PressTypes;
using Il2CppInterop.Runtime;
using Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionFollowDirect : IPressAction
    {
        public string FriendlyName => "Follow me";
        private string _FriendlyNameShort = "Follow";
        public string FriendlyIdentifier => "Follow";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAgent>();
        //public int? Priority => 0;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent)
        {
            PlayerAgent Agent;
            if (BestComponent != null)
                Agent = BestComponent.TryCast<PlayerAgent>();
            else
                Agent = zSmartSelect.GetPlayerAgentLookingAt();
            if (Agent == null) return false;
            zUpdater.Instance.StartCoroutine(CallAgentToFollow(Agent));
            return true;
        }
        public static void StaticCallAgentToFollow(PlayerAgent Follower, PlayerAgent Followee = null, bool voicelines = true)
        {
            zUpdater.Instance.StartCoroutine(CallAgentToFollow(Follower, Followee, voicelines));
        }
        public static IEnumerator CallAgentToFollow(PlayerAgent Follower, PlayerAgent Leader = null, bool voicelines = true)
        {
            if (Leader == null)
                Leader = zStaticRefrences.LocalPlayer;
            uint voidID = zSmartSelect.GetVoiceId(Follower);
            if (Follower.Owner.IsBot)
            {
                PlayerAIBot Bot = Follower?.GetComponent<PlayerAIBot>();
                string botname = Bot.Agent.PlayerName;
                string followName = Leader.PlayerName;
                if (Leader == zStaticRefrences.LocalPlayer)
                    followName = "me";
                if (voicelines)
                {
                    zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Hey {botname}, Follow {followName}!", 2);
                    PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, voidID);
                    yield return new WaitForSeconds(1f);
                    PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_FOLLOWME);
                }
                //zStaticRefrences.CommsMenu.OnButtonPressedCall(null, Bot.Agent);
                //Bot.SyncValues.Leader = Leader;
                zBotActions.SetLeader(Follower, Leader, zStaticRefrences.LocalPlayer, 0);

                ZiMain.BotBarkBack(Bot.Agent.CharacterID, AK.EVENTS.PLAY_CL_ILLFOLLOWYOURLEAD, "I will follow your lead.", 2f);
                zChatHandler.sendChatMessage($"On the way.", "Follow"+ "TalkInChatNotifyActionAcknowlage", Bot.Agent, zStaticRefrences.LocalPlayer);
            }
            else
            {
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, voidID);
                yield return new WaitForSeconds(1f);
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_FOLLOWME);
            }
            yield return new WaitForSeconds(1f);
            Follower.NavMarker.UpdateExtraInfo();
        }

        public bool IsActionValid(Component candidate)
        {
            PlayerAgent Agent = candidate.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            if (!Agent.Alive) return false;
            if (Agent == zStaticRefrences.LocalPlayer) return false;
            PlayerAgent leader = Agent?.GetComponent<PlayerAIBot>()?.SyncValues?.Leader;
            if (leader != null && leader == zStaticRefrences.LocalPlayer) return false; // I don't like calling get compoennet this much, but it's PROBABLY fine?
            Color = Agent.Owner.PlayerColor;
            return true;
        }
    }
}
