using Il2CppInterop.Runtime;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionPickupMines : IPressAction
    {
        public string FriendlyName => "Pickup All Agent Mines";
        public string _FriendlyNameShort => "Pickup-A";
        public string FriendlyIdentifier => "Pickup Equipmenet";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<MineDeployerInstance>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            MineDeployerInstance mine = BestComponent.TryCast<MineDeployerInstance>();
            PlayerAIBot bot = mine?.Owner?.GetComponent<PlayerAIBot>();
            if (bot == null)
                return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PICKUPYOURDEPLOYABLES);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Pick up your deployables.", 1);
            zBotActions.SendBotToPickUpMine(bot, null, zStaticRefrences.LocalPlayer);
            zChatHandler.sendChatMessage("Picking up all of my mines.", FriendlyIdentifier + IPressAction.chatPermSuffix, bot.Agent, zStaticRefrences.LocalPlayer);
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
            if (bot.GetDeployedItems().Count <= 1) return false;
            Color = bot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
