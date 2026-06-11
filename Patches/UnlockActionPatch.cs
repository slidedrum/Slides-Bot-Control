// THIS IS ENTIRELY AI GENERATED.  
// It was way more complicated than I thought.
// All this changes is let bots unlock with hacking

using Gear;
using HarmonyLib;
using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using System;

[HarmonyPatch]
public static class UnlockActionPatch
{
    private static float s_hackSuccessChance = 0.25f;
    private static int s_hackMaxNrFaults = 3;

    private const uint LockMelterBackpackItemId = 0x74;
    private const uint HackingToolBackpackItemId = 0x35;

    private static bool useNewMethod = true;

    [HarmonyPatch(typeof(PlayerBotActionUnlock), "ChooseMethod")]
    [HarmonyPrefix]
    public static bool PreChooseMethod(PlayerBotActionUnlock __instance, ref bool __result)
    {
        if (!useNewMethod)
            return true;
        __result = ChooseMethod(__instance);
        return false;
    }

    private static PlayerBotActionBase.Descriptor.EventDelegateFunc CreateEventDelegate(
        PlayerBotActionUnlock action)
    {
        return DelegateSupport.ConvertDelegate<PlayerBotActionBase.Descriptor.EventDelegateFunc>(
            new Action<PlayerBotActionBase.Descriptor>(action.OnMethodActionEvent));
    }

    private static bool ChooseMethod(PlayerBotActionUnlock action)
    {
        if (action.m_method != PlayerBotActionUnlock.Descriptor.MethodEnum.None)
            return CommitChosenMethod(action);

        if (action.m_desc == null)
            throw new System.NullReferenceException();

        int descriptorMethod = (int)action.m_desc.Method;

        if (UsesFlagResolution(descriptorMethod))
        {
            if (TryResolveFlagMethod(action, descriptorMethod, out PlayerBotActionUnlock.Descriptor.MethodEnum resolved))
            {
                action.m_method = resolved;
                if (!TryAssignMethodDescriptor(action, resolved))
                    return false;

                return CommitChosenMethod(action);
            }

            action.m_method = PlayerBotActionUnlock.Descriptor.MethodEnum.None;
            return false;
        }

        action.m_method = (PlayerBotActionUnlock.Descriptor.MethodEnum)descriptorMethod;

        if (descriptorMethod == 0)
        {
            MarkDescriptorFailed(action);
            return false;
        }

        if (!TryAssignMethodDescriptor(action, action.m_method))
            return false;

        return CommitChosenMethod(action);
    }

    private static bool CommitChosenMethod(PlayerBotActionUnlock action)
    {
        if (action.m_desc == null)
            throw new System.NullReferenceException();

        action.m_desc.UsedMethod = action.m_method;
        return true;
    }

    private static void MarkDescriptorFailed(PlayerBotActionUnlock action)
    {
        if (action.m_desc == null)
            throw new System.NullReferenceException();

        action.m_desc.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Failed);
    }

    private static bool UsesFlagResolution(int descriptorMethod)
    {
        if (descriptorMethod == (int)PlayerBotActionUnlock.Descriptor.MethodEnum.Any)
            return true;

        return (descriptorMethod & (descriptorMethod - 1)) != 0;
    }

    private static bool TryResolveFlagMethod(
        PlayerBotActionUnlock action,
        int methodFlags,
        out PlayerBotActionUnlock.Descriptor.MethodEnum resolved)
    {
        resolved = PlayerBotActionUnlock.Descriptor.MethodEnum.None;
        action.m_method = (PlayerBotActionUnlock.Descriptor.MethodEnum)methodFlags;

        LG_WeakLock weakLock = action.m_desc.Lock;
        PlayerAIBot bot = action.m_bot;

        if (bot == null)
            throw new System.NullReferenceException();

        if (!bot.WantsCrouch() && (methodFlags & (int)PlayerBotActionUnlock.Descriptor.MethodEnum.Melee) != 0)
        {
            if (bot.Backpack == null)
                throw new System.NullReferenceException();

            if (bot.Backpack.TryGetBackpackItem(InventorySlot.GearMelee, out BackpackItem anyMeleeBackpackItem))
            {
                if (weakLock == null)
                    throw new System.NullReferenceException();

                if (weakLock.Status == eWeakLockStatus.LockedMelee)
                {
                    resolved = PlayerBotActionUnlock.Descriptor.MethodEnum.Melee;
                    return true;
                }
            }
        }

        if ((methodFlags & (int)PlayerBotActionUnlock.Descriptor.MethodEnum.Hack) != 0)
        {
            if (bot.Backpack == null)
                throw new System.NullReferenceException();

            if (bot.Backpack.TryGetBackpackItem(InventorySlot.HackingTool, out BackpackItem hackingToolBackpackItem))
            {
                if (hackingToolBackpackItem == null)
                    throw new System.NullReferenceException();

                if (hackingToolBackpackItem.ItemID == HackingToolBackpackItemId)
                {
                    if (weakLock == null)
                        throw new System.NullReferenceException();

                    if (weakLock.Status == eWeakLockStatus.LockedHackable)
                    {
                        resolved = PlayerBotActionUnlock.Descriptor.MethodEnum.Hack;
                        return true;
                    }
                }
            }
        }

        if ((methodFlags & (int)PlayerBotActionUnlock.Descriptor.MethodEnum.Melt) != 0)
        {
            if (bot.Backpack == null)
                throw new System.NullReferenceException();

            if (bot.Backpack.TryGetBackpackItem(InventorySlot.Consumable, out BackpackItem melterBackpackItem))
            {
                if (melterBackpackItem == null)
                    throw new System.NullReferenceException();

                if (melterBackpackItem.ItemID == LockMelterBackpackItemId)
                {
                    if (weakLock == null)
                        throw new System.NullReferenceException();

                    eWeakLockStatus lockStatus = weakLock.Status;
                    if (lockStatus == eWeakLockStatus.LockedMelee
                        || lockStatus == eWeakLockStatus.LockedHackable)
                    {
                        resolved = PlayerBotActionUnlock.Descriptor.MethodEnum.Melt;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool TryAssignMethodDescriptor(
        PlayerBotActionUnlock action,
        PlayerBotActionUnlock.Descriptor.MethodEnum method)
    {
        switch (method)
        {
            case PlayerBotActionUnlock.Descriptor.MethodEnum.Melee:
                if (!TryCreateMeleeDescriptor(action, out PlayerBotActionBase.Descriptor meleeDesc))
                    return false;

                action.m_chosenMethod = meleeDesc;
                return true;
            case PlayerBotActionUnlock.Descriptor.MethodEnum.Hack:
                action.m_chosenMethod = CreateHackDescriptor(action);
                return true;
            case PlayerBotActionUnlock.Descriptor.MethodEnum.Melt:
                action.m_chosenMethod = CreateMeltDescriptor(action);
                return true;
            default:
                MarkDescriptorFailed(action);
                return false;
        }
    }

    private static bool TryCreateMeleeDescriptor(
        PlayerBotActionUnlock action,
        out PlayerBotActionBase.Descriptor descriptor)
    {
        descriptor = null;

        if (action.m_backpack == null)
            throw new System.NullReferenceException();

        if (!action.m_backpack.TryGetBackpackItem(InventorySlot.GearMelee, out BackpackItem meleeBackpackItem))
            return false;

        if (meleeBackpackItem == null)
            throw new System.NullReferenceException();

        // Il2Cpp: `is MeleeWeaponThirdPerson` fails in mod code; use TryCast (matches native type check in ChooseMethod.c).
        MeleeWeaponThirdPerson meleeWeapon =
            meleeBackpackItem.Instance.TryCast<MeleeWeaponThirdPerson>();

        PlayerBotActionMelee.Descriptor meleeDesc =
            new PlayerBotActionMelee.Descriptor(action.m_bot);

        meleeDesc.ParentActionBase = action;
        meleeDesc.Prio = action.m_desc.Prio;
        meleeDesc.EventDelegate = CreateEventDelegate(action);

        meleeDesc.Haste = 0.75f;
        meleeDesc.Force = 1.0f;
        meleeDesc.Strike = true;
        meleeDesc.Loop = true;
        meleeDesc.Travel = true;

        if (action.m_desc == null)
            throw new System.NullReferenceException();

        LG_WeakLock lockForMelee = action.m_desc.Lock;
        if (lockForMelee == null)
            throw new System.NullReferenceException();

        LG_WeakLockDamage weakLockDamage =
            lockForMelee.GetComponentInChildren<LG_WeakLockDamage>();
        if (weakLockDamage == null)
            throw new System.NullReferenceException();

        meleeDesc.TargetGameObject = weakLockDamage.gameObject;
        meleeDesc.Weapon = meleeWeapon;

        descriptor = meleeDesc;
        return true;
    }

    private static PlayerBotActionUseHackingTool.Descriptor CreateHackDescriptor(PlayerBotActionUnlock action)
    {
        PlayerBotActionUseHackingTool.Descriptor hackDesc =
            new PlayerBotActionUseHackingTool.Descriptor(action.m_bot);

        hackDesc.ParentActionBase = action;
        hackDesc.Prio = action.m_desc.Prio;
        hackDesc.EventDelegate = CreateEventDelegate(action);

        if (action.m_desc == null)
            throw new System.NullReferenceException();

        hackDesc.Lock = action.m_desc.Lock;
        hackDesc.MaxNrFaults = s_hackMaxNrFaults;
        hackDesc.SuccessChance = s_hackSuccessChance;

        return hackDesc;
    }

    private static PlayerBotActionUseLockMelter.Descriptor CreateMeltDescriptor(PlayerBotActionUnlock action)
    {
        PlayerBotActionUseLockMelter.Descriptor meltDesc =
            new PlayerBotActionUseLockMelter.Descriptor(action.m_bot);

        meltDesc.ParentActionBase = action;
        meltDesc.Prio = action.m_desc.Prio;
        meltDesc.EventDelegate = CreateEventDelegate(action);

        if (action.m_desc == null)
            throw new System.NullReferenceException();

        meltDesc.Lock = action.m_desc.Lock;

        return meltDesc;
    }
}
