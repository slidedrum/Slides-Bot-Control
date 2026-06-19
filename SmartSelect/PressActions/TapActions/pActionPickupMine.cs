using Il2CppInterop.Runtime;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionPickupMine : IInputAction
    {
        public string FriendlyName => "Pickup Mine";
        public string _FriendlyNameShort => "Pickup";
        public string FriendlyIdentifier => "Pickup Equipment";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<MineDeployerInstance>();
        public string pressTypeIdentifier => "Tap";
        public bool Invoke(Component BestComponent)
        {
            MineDeployerInstance mine = BestComponent.TryCast<MineDeployerInstance>();
            PlayerAIBot bot = mine?.Owner?.GetComponent<PlayerAIBot>();
            if (bot == null)
                return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PICKUPYOURDEPLOYABLES);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Pick up your deployables.", 1);
            zBotActions.SendBotToPickUpMine(bot, mine, zStaticRefrences.LocalPlayer);
            zChatHandler.sendChatMessage("Picking up mine.", FriendlyIdentifier + IInputAction.chatPermSuffix, bot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            MineDeployerInstance mine = candidate.TryCast<MineDeployerInstance>();
            if (mine == null)
                return false;
            PlayerAIBot bot = mine?.Owner?.GetComponent<PlayerAIBot>();
            if (bot == null)
                return false;
            if (!bot.Agent.Alive)
                return false;
            if (!zHelpers.CanBotReach(bot, mine.transform.position)) return false;
            Color = bot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
