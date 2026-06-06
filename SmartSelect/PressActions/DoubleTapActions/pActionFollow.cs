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
    public class pActionFollow : IPressAction
    {
        public string FriendlyName => "Follow me";
        private string _FriendlyNameShort = "Follow";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => null;
        private List<Il2CppSystem.Type> Types = new() { null, Il2CppType.Of<PlayerAgent>() };
        public string pressTypeIdentifier => "Double Tap";

        public void Register() 
        { 
            IPressType PressType = PressTypeManager.GetPressType(pressTypeIdentifier);
            if (PressType == null)
                throw new Exception($"PressAction {FriendlyName} tried to register to non existant press type {pressTypeIdentifier}");
            foreach (Il2CppSystem.Type Type in Types)
            {
                if (Type == null)
                    PressType.RegisterAction(this, Type);
                else
                    PressType.RegisterAction(this, Type, priority: -10);
            }
        }
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
        public static IEnumerator CallAgentToFollow(PlayerAgent Agent)
        {
            uint voidID = zSmartSelect.GetVoiceId(Agent);
            if (Agent.Owner.IsBot)
            {
                PlayerAIBot Bot = Agent?.GetComponent<PlayerAIBot>();
                string botname = Bot.Agent.PlayerName;
                zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Hey {botname}, Follow me!", 2);
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, voidID);
                yield return new WaitForSeconds(1f);
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_FOLLOWME);
                zStaticRefrences.CommsMenu.OnButtonPressedCall(null, Bot.Agent);
                ZiMain.sendChatMessage($"On the way.", Bot.Agent, zStaticRefrences.LocalPlayer);
            }
            else
            {
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, voidID);
                yield return new WaitForSeconds(1f);
                PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_FOLLOWME);
            }

        }

        public bool IsActionValid(Component candidate)
        {
            PlayerAgent Agent;
            if (candidate != null)
                Agent = candidate.TryCast<PlayerAgent>();
            else
                Agent = zSmartSelect.GetPlayerAgentLookingAt();
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
