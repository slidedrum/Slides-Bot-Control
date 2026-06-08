using Il2CppInterop.Runtime;
using Il2CppSystem.Runtime.Remoting.Messaging;
using LevelGeneration;
using Player;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionUnlock : IPressAction
    {
        public string FriendlyName => "Break Lock";
        public string FriendlyNameShort => "Unlock";
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakLock>();
        public string pressTypeIdentifier => "Tap";
        public static bool strike = false;
        public static bool travel = true;
        public bool Invoke(Component BestComponent)
        {
            LG_WeakLock Lock = BestComponent.TryCast<LG_WeakLock>();
            if (Lock == null) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            iLG_WeakLockHolder holder = Lock.m_holder;
            LG_DoorButton Button = holder.TryCast<LG_DoorButton>();
            LG_WeakResourceContainer container = holder.TryCast<LG_WeakResourceContainer>();
            PlayerBotActionUnlock.Descriptor.TargetTypeEnum targetType;
            GameObject targetObject;
            if (Button != null)
            {
                LG_WeakDoor door = Button.m_door.TryCast<LG_WeakDoor>();
                targetObject = door.gameObject;
                targetType = PlayerBotActionUnlock.Descriptor.TargetTypeEnum.Door;
            }
            else if (container != null)
            {
                targetObject = container.gameObject;
                targetType = PlayerBotActionUnlock.Descriptor.TargetTypeEnum.Container;
            }
            else 
                return false;
            PlayerBotActionUnlock.Descriptor Desc = new(BestBot)
            {
                TargetType = targetType,
                TargetGO = targetObject,
                Prio = 13,
                TargetPosition = targetObject.transform.position,
                Method = PlayerBotActionUnlock.Descriptor.MethodEnum.Any,
                Lock = Lock,
            };
            zBotActions.StartAction(BestBot, Desc);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            LG_WeakLock Lock = candidate.TryCast<LG_WeakLock>();
            if(Lock == null) return false;
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (Lock == null) return false;
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
