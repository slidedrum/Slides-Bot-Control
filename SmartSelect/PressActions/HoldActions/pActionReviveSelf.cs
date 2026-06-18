using BotControl.SmartSelect.PressTypes;
using Il2CppInterop.Runtime;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BotControl.SmartSelect.PressActions
{
    internal class pActionReviveSelf : IPressAction
    {
        // TODO revert to closest bot if none selected.
        public string FriendlyName => "Revive Self";
        public string FriendlyNameShort => "Revive";
        public string FriendlyIdentifier => "Revive";
        public int? Priority => 100;
        public Il2CppSystem.Type Type => null;
        private List<Il2CppSystem.Type> Types = new() { null, Il2CppType.Of<PlayerAIBot>() };
        //public string pressTypeIdentifier => null;
        private List<string> PressTypeIdentifiers = new() { "Hold", "Double Tap" };
        public void Register()
        {
            foreach (string ident in PressTypeIdentifiers)
            {
                IPressType PressType = PressTypeManager.GetPressType(ident);
                if (PressType == null)
                    throw new Exception($"PressAction {FriendlyName} tried to register to non existant press type {ident}");
                foreach (Il2CppSystem.Type Type in Types)
                {
                    PressType.RegisterAction(this, Type, 100);
                }
            }
        }
        public bool Invoke(Component BestComponent)
        {
            // BestComponent MIGHT be null.
            // if it is, we must use the selected bot instead.
            // if that's null, maybe use the closest bot?
            PlayerAIBot BotToUse = null;
            if (BestComponent != null) //We're looking at a bot.
            {
                PlayerAIBot Bot = BestComponent.TryCast<PlayerAIBot>();
                if (Bot.Agent.Alive) // are they alive?
                    return false;
                bool reachable = zHelpers.CanBotReach(Bot, zStaticRefrences.LocalPlayer.Position);
                if (!reachable)
                    BotToUse = Bot;
            }
            else
            {
                BotToUse = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>().FirstOrDefault();
            }
            if (BotToUse == null)
                return false;
            zBotActions.SendBotToReviveAgent(BotToUse, zStaticRefrences.LocalPlayer, zStaticRefrences.LocalPlayer, 0);
            zChatHandler.sendChatMessage($"Reviving {zStaticRefrences.LocalPlayer.PlayerName}.", FriendlyIdentifier + "TalkInChatNotifyActionAcknowlage", BotToUse.Agent, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            if (zStaticRefrences.LocalPlayer.Alive) // We must be dead to be revived.
                return false;
            if (candidate == null) // Are we looking at nothing?
            {
                bool LookingDown = Vector3.Angle(zStaticRefrences.CameraTransform.forward, Vector3.down) < 80f;
                if (!LookingDown) // if we're not looking down, action not valid.
                    return false;
                HashSet<PlayerAIBot> SelectedBots = zSmartSelect.MainSelection.GetSelected<PlayerAIBot>();
                foreach (PlayerAIBot Bot in SelectedBots) // loop through all bots in selection
                {
                    if (!Bot.Agent.Alive) // if it's dead, not valid.
                        continue;
                    bool reachable = zHelpers.CanBotReach(Bot, zStaticRefrences.LocalPlayer.Position);
                    if (reachable)
                        return true; // if we are reachable, then it's valid!
                }
            }
            else // are we looking at a bot?
            {
                PlayerAIBot Bot = candidate.TryCast<PlayerAIBot>();
                if (Bot == null)
                    return false;
                if (!Bot.Agent.Alive)
                    return false;
                bool reachable = zHelpers.CanBotReach(Bot, zStaticRefrences.LocalPlayer.Position);
                if (reachable)
                    return true;
            }
            return false;
        }
    }
}
