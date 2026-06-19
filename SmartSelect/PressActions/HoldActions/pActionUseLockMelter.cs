using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using UnityEngine;
using UnityEngine.UIElements;
using static Player.PlayerBotActionUnlock.Descriptor;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionUseLockMelter : IInputAction
    {
        public string FriendlyName => "Lock Melter";
        public string FriendlyNameShort => "Melt";
        public string FriendlyIdentifier => "Interact";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakLock>();
        public string pressTypeIdentifier => "Hold";
        public static bool strike = false;
        public static bool travel = true;
        public bool Invoke(Component BestComponent)
        {
            LG_WeakLock Lock = BestComponent.TryCast<LG_WeakLock>();
            if (Lock == null) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PLEASE);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle("Please.", 1f);
            zBotActions.SendbotToBreakLock(BestBot, Lock, MethodEnum.Melt, zStaticRefrences.LocalPlayer, 0);
            zChatHandler.sendChatMessage("Using my lock melter.", FriendlyIdentifier + IInputAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            LG_WeakLock Lock = candidate.TryCast<LG_WeakLock>();
            if (Lock == null) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (Lock == null) return false;
            if (!zHelpers.TryGetAgentBackpackItem(BestBot.Agent, InventorySlot.Consumable, out BackpackItem item)) return false;
            if (item.ItemID != 116) return false; // is it a lock melter?
            iLG_WeakLockHolder holder = Lock.m_holder;
            LG_DoorButton Button = holder.TryCast<LG_DoorButton>();
            LG_WeakResourceContainer locker = holder.TryCast<LG_WeakResourceContainer>();
            if (Button != null)
            {
                LG_WeakDoor door = Button.m_door.TryCast<LG_WeakDoor>();
                if (door != null
                    && !door.Gate.IsTraversable
                    && Vector3.Dot(Lock.transform.forward, zStaticRefrences.CameraTransform.position - Lock.transform.position) <= 0f)
                {
                    return false;
                }
            }
            if (!zHelpers.CanBotReach(BestBot, Lock.transform.position)) return false;
            return true;
        }
    }
}
