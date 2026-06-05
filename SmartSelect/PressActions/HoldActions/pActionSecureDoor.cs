using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace BotControl.SmartSelect.PressActions.HoldActions
{
    public class pActionSecureDoor : IPressAction
    {
        public string FriendlyName => "Secure Door";
        public string _FriendlyNameShort => "Secure";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => Il2CppType.Of<LG_WeakDoor>();
        public string pressTypeIdentifier => "Hold";
        public bool Invoke(Component BestComponent)
        {
            LG_WeakDoor Door = BestComponent.TryCast<LG_WeakDoor>();
            PlayerAIBot BestBot = GetBestBot(Door);
            if (Door == null || BestBot == null) return false;
            if (Door.Gate.IsTraversable) return false; //if the door is open, don't do anything.
            // todo have the bot interact with the door to close it before securing it.
            Vector3 MovePosition = BestBot.Agent.Position;
            LG_Gate gate = Door.Gate;
            Vector3 vecToGate = gate.transform.position - BestBot.Agent.Position;
            Vector3 forward = gate.transform.forward;
            if (Vector3.Dot(forward, vecToGate) > 0f)
                forward = -forward;
            Vector3 candidate = gate.transform.position + forward * 1f; // weak door = 1m
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, -1))
                MovePosition = hit.position;
            else
                MovePosition = candidate;
            zBotActions.SendBotToThrowItem(zStaticRefrences.LocalPlayer, BestBot.Agent, Networking.pStructs.pThrowType.cFoam, MovePosition, Door.transform.position, 0);
            return false;
        }
        public bool IsActionValid(Component candidate)
        {
            LG_WeakDoor Door = candidate.TryCast<LG_WeakDoor>();
            if (Door == null) return false;
            PlayerAIBot BestBot = GetBestBot(Door);
            if (BestBot == null) return false;
            if (!BestBot.Agent.Alive) return false;
            if (!PlayerBotActionThrowItem.Descriptor.Evaluate(BestBot, 115u, out var bpItem)) return false;
            if (PlayerBackpackManager.GetBackpack(BestBot.Agent.Owner).AmmoStorage.ConsumableAmmo.BulletsInPack <= 0) return false;
            if (Door.LastStatus == eDoorStatus.Destroyed)  return false; 
            if (Door.LastStatus == eDoorStatus.GluedMax)  return false; 
            if (Door.LastStatus == eDoorStatus.TryOpenStuckInGlue)  return false;
            bool doorOpen = Door.Gate.IsTraversable;
            if (doorOpen && !Door.InteractionAllowed) return false;
            Color = BestBot.Agent.Owner.PlayerColor;
            return true;
        }
        private PlayerAIBot GetBestBot(LG_WeakDoor Door)
        {
            List<PlayerAIBot> BotList = ZiMain.GetBotList();
            List<PlayerAIBot> Candidates = new();
            PlayerAIBot BestBot = zSmartSelect.MainSelection.GetBestBot();
            foreach (PlayerAIBot bot in BotList)
            {
                if (!bot.Agent.Alive)
                    continue;
                if (!PlayerBotActionThrowItem.Descriptor.Evaluate(bot, 115u, out var bpItem))
                    continue;
                if (PlayerBackpackManager.GetBackpack(bot.Agent.Owner).AmmoStorage.ConsumableAmmo.BulletsInPack <= 0) 
                    continue;
                if (!zHelpers.CanBotReach(bot, Door.transform.position))
                    continue;
                if (bot.Agent.Pointer == BestBot.Agent.Pointer)
                    return BestBot;
                Candidates.Add(bot);
            }
            return Candidates.FirstOrDefault();
        }
    }
}
