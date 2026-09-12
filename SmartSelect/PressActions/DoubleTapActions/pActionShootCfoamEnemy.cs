using Enemies;
using Il2CppInterop.Runtime;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.DoubleTapActions
{
    public class pActionShootCfoamEnemy : IPressAction
    {
        public string FriendlyName => "Shoot Cfoam Enemy";
        private string _FriendlyNameShort = "cFoam-E";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public string FriendlyIdentifier => "Deploy Equipment";
        public Il2CppSystem.Type Type => Il2CppType.Of<EnemyAgent>();
        public int? Priority => 2;
        public string pressTypeIdentifier => "Double Tap";
        public bool Invoke(Component BestComponent, PlayerAIBot BestBot)
        {
            EnemyAgent Enemy = BestComponent.TryCast<EnemyAgent>();
            if (Enemy == null || BestBot == null) return false;
            if (!Evaluate(BestBot.Agent)) return false;
            zBotActions.SendBotToUseCfoamGun(BestBot, zStaticRefrences.LocalPlayer.FPSCamera.CameraRayPos, Enemy, zStaticRefrences.LocalPlayer);
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_CFOAMHERE);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Put foam here.", 1);
            ZiMain.BotBarkBack(BestBot.Agent.CharacterID, AK.EVENTS.PLAY_CL_WILLDO, "Will Do.", 2f);
            zChatHandler.sendChatMessage("Shooting c-foam at enemy.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool Evaluate(PlayerAgent ownerAgent)
        {
            PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(ownerAgent.Owner);
            if (backpack == null)
                return false;
            if (backpack.AmmoStorage.ClassAmmo.BulletsInPack > 0)
            {
                BackpackItem backpackItem = backpack.Slots[3];
                if (backpackItem != null && backpackItem.Instance != null && backpackItem.ItemID == 73U)
                    return true;
            }
            return false;
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
            if (!Evaluate(BestBot.Agent))
                return false;
            if (!zHelpers.CanBotReach(BestBot, Enemy.transform.position))
                return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;
        }
    }
}
