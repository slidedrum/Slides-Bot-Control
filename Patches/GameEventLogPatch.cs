using BotControl.SmartSelect;
using HarmonyLib;
using SlideMenu;
using SNetwork;
using UnityEngine;

namespace BotControl.Patches
{
    [HarmonyPatch]
    public class GameEventLogPatch
    {
        [HarmonyPatch(typeof(PUI_GameEventLog), nameof(PUI_GameEventLog.UpdateHelpText))]
        [HarmonyPostfix]
        public static void PostUpdateHelpText(PUI_GameEventLog __instance, SNet_Player speaker)
        {
            if (speaker != null)
                return;
            UpdateHelpText(__instance);
        }
        [HarmonyPatch(typeof(PUI_GameEventLog), nameof(PUI_GameEventLog.Update))] // Extra patch for Better Text Chat compatability.
        [HarmonyPostfix]
        public static void PostUpdateHelpText(PUI_GameEventLog __instance)
        {
            UpdateHelpText(__instance);
        }
        private static void UpdateHelpText(PUI_GameEventLog __instance)
        {
            if (__instance.m_checkPushToTalk && __instance.m_lastPushToTalkStatus)
                return;

            string chat = GetBindText(InputAction.TextChatOpenSend);
            string map =  GetBindText(InputAction.ToggleMap);
            string menu = GetBindText(sMenuManager.keybind);
            string bots = GetBindText(zSmartSelect.keybind);

            if (__instance.m_checkPushToTalk)
            {
                string ptt = GetBindText(InputAction.VoiceChatPushToTalk);
                __instance.m_txtHelp.m_text = chat + " Chat  " + ptt + " Talk  " + map + " Map  " + menu + " Bot Menu  " + bots + " Bot Commands";
            }
            else
            {
                __instance.m_txtHelp.m_text = chat + " Chat  " + map + " Map  " + menu + " Bot Menu  " + bots + " Bot Commands";
            }
        }

        public static string GetBindText(InputAction action)
        {
            return "<color=orange>[" + InputMapper.GetBindingName(action) + "]</color>";
        }

        public static string GetBindText(KeyCode key)
        {
            return "<color=orange>[" + key.ToString() + "]</color>";
        }
    }
}