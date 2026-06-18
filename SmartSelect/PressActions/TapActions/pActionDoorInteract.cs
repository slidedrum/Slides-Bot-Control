using BotControl.CustomActions.CustomActions;
using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionDoorInteract : IPressAction
    {
        public string FriendlyName => "Open Door";
        private string _FriendlyNameShort = "Open";
        public string FriendlyNameShort => _FriendlyNameShort;
        public string FriendlyIdentifier => "Interact";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakDoor>();
        public string pressTypeIdentifier => "Tap";
        public bool Enabled => false;
        public bool Invoke(Component BestComponent)
        {
            LG_WeakDoor Door = BestComponent.TryCast<LG_WeakDoor>();
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            zBotActions.SendBotToInteractDoor(BestBot, Door, zStaticRefrences.LocalPlayer.transform.position, PlayerBotActionUnlock.Descriptor.MethodEnum.Any, zStaticRefrences.LocalPlayer);
            if (Door.Gate.IsTraversable)
                zChatHandler.sendChatMessage("Closing the door.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            else
                zChatHandler.sendChatMessage("Opening the door.", FriendlyIdentifier + IPressAction.chatPermSuffix, BestBot.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            if (!zSmartSelect.MainSelection.AnySelectedBotsAlive())
                return false;
            LG_WeakDoor Door = candidate.TryCast<LG_WeakDoor>();
            if (!Door.InteractionAllowed)
                return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (!BestBot.Agent.Alive) 
                return false;
            if (!zHelpers.CanBotReach(BestBot, Door.transform.position)) 
                return false;
            if (Door.Gate.IsTraversable)
                _FriendlyNameShort = "Close";
            else
            {
                var button = GetButton(Door);
                if (button == null)
                    return false;
                var Lock = GetLock(button);
                if (Lock == null || Lock.Status == eWeakLockStatus.Unlocked)
                    _FriendlyNameShort = "Open";
                else
                    _FriendlyNameShort = "Unlock-O";
            }
            return true;
        }
        private LG_DoorButton GetButton(LG_WeakDoor TargetDoor)
        {
            LG_DoorButton BestButton = null;
            if (TargetDoor == null)
                return null;
            float bestDot = float.MinValue;
            foreach (var button in TargetDoor.m_buttons)
            {
                float dot = Vector3.Dot(button.transform.forward, zStaticRefrences.LocalPlayer.transform.position - button.transform.position);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    BestButton = button;
                }
            }
            return BestButton;
        }
        public LG_WeakLock GetLock(LG_DoorButton TargetButton)
        {
            LG_WeakDoor door = TargetButton.m_door.Cast<LG_WeakDoor>();
            if (door.WeakLocks == null)
                return null;
            if (door.WeakLocks.Count == 0)
                return null;
            foreach (LG_WeakLock Lock in door.WeakLocks)
            {
                if (Lock.m_holder.Pointer == TargetButton.Pointer)
                {
                    return Lock;
                }
            }
            return null;
        }
    }
}
