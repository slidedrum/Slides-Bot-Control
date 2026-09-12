using Enemies;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionThrowCfoamEnemy : IPressAction
    {
        public string FriendlyName => "Throw Cfoam Enemy";
        public string FriendlyIdentifier => "Throw Consumable";
        private string _FriendlyNameShort = "Throw-E";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<EnemyAgent>();
        public int? Priority => 1;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            EnemyAgent Enemy = BestComponent.TryCast<EnemyAgent>();
            if (Enemy == null || BestBot == null) return false;
            PlayerAgent LocalPlayer = zStaticRefrences.LocalPlayer;
            PlayerVoiceManager.WantToSay(LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CFOAMHERE);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Throw C-Foam here.", 1f);
            zBotActions.SendBotToThrowItem(LocalPlayer, BestBot.Agent, LocalPlayer.transform.position, LocalPlayer.FPSCamera.CameraRayPos, Enemy);
            zChatHandler.sendChatMessage("Throwing C-Foam at enemy.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate, PlayerAIBot BestBot)
        {
            EnemyAgent Enemy = candidate.TryCast<EnemyAgent>();
            if (Enemy == null || BestBot == null)
                return false;
            if (!BestBot.Agent.Alive)
                return false;
            if (!Enemy.Alive)
                return false;
            if (Enemy.AI.IsHibernating(out bool isDisturbed, out bool isWakingUp) && !isWakingUp)
                return false;
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(BestBot.Agent.Owner);
            if (backpack.AmmoStorage.ConsumableAmmo.BulletsInPack <= 0)
                return false;
            BackpackItem item = zHelpers.GetAgentBackpackItem(BestBot.Agent, InventorySlot.Consumable);
            if (item == null || item.ItemID != 115u)
                return false;
            if (!zHelpers.CanBotReach(BestBot, Enemy.transform.position))
                return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
