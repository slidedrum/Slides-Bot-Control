using Agents;
using BepInEx.Unity.IL2CPP.Utils;
using Enemies;
using Il2CppInterop.Runtime;
using Player;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionFollow : IPressAction
    {
        public string FriendlyName => "Follow me";
        private string _FriendlyNameShort = "Follow";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<PlayerAgent>();
        public string pressTypeIdentifier => "Double Tap";


        public bool Invoke(Component BestComponent)
        {
            PlayerAgent Agent = BestComponent.TryCast<PlayerAgent>();
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
            PlayerAgent Agent = candidate.TryCast<PlayerAgent>();
            if (Agent == null) return false;
            if (!Agent.Alive) return false;
            Color = Agent.Owner.PlayerColor;
            return true;
        }
    }
}
