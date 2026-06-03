using Il2CppInterop.Runtime;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionPickupSentry : IPressAction
    {
        public string FriendlyName => "Pickup Sentry";
        public string FriendlyNameShort => "Pickup";
        public Il2CppSystem.Type Type => Il2CppType.Of<SentryGunInstance>();
        public string pressTypeIdentifier => "Tap";
        public bool Invoke(Component BestComponent)
        {
            SentryGunInstance sentry = BestComponent.TryCast<SentryGunInstance>();
            PlayerAIBot bot = sentry?.Owner?.GetComponent<PlayerAIBot>();
            if (bot == null)
                return false;
            PlayerVoiceManager.WantToSay(zStaticRefrences.LocalPlayer.CharacterID, AK.EVENTS.PLAY_CL_PICKUPYOURDEPLOYABLES);
            zStaticRefrences.Subtitles.ShowSingleLineSubtitle($"Pick up your deployables.", 1);
            zBotActions.SendBotToPickUpSentry(bot, zStaticRefrences.LocalPlayer);
            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            SentryGunInstance sentry = candidate.TryCast<SentryGunInstance>();
            if (sentry == null)
                return false;
            PlayerAIBot bot = sentry?.Owner?.GetComponent<PlayerAIBot>();
            if (bot == null)
                return false;
            if (!bot.Agent.Alive)
                return false;
            if (!zHelpers.CanBotReach(bot, sentry.transform.position)) return false;
            return true;
        }
    }
}
