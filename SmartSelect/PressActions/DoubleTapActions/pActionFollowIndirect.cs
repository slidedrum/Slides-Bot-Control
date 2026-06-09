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
    public class pActionFollowIndirect : IPressAction
    {
        public string FriendlyName => "Follow me through walls";
        private string _FriendlyNameShort = "Follow";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => null;
        public int? Priority => -15;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent)
        {
            PlayerAgent Agent;
            if (BestComponent != null)
                Agent = BestComponent.TryCast<PlayerAgent>();
            else
                Agent = zSmartSelect.GetPlayerAgentLookingAt();
            if (Agent == null) return false;
            PressActionManager.GetAction("Follow me").Invoke(Agent);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            PlayerAgent Agent = zSmartSelect.GetPlayerAgentLookingAt();
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
