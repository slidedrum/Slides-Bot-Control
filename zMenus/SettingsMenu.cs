using SlideMenu;
using System;
using UnityEngine;

namespace BotControl.Menus
{
    public static class SettingsMenuClass
    {
        public static float menuSizeStep = 0.1f;
        public static sMenu SettingsMenu;
        public static sMenu ChatPermsMenu;
        public static sMenu.sMenuNode scaleNode;
        public static sMenu.sMenuNode ChatNode;
        public static Color onColor = new Color(0, 0.2f, 0);

        public static void Setup(sMenu menu)
        {
            SettingsMenu = menu;
            scaleNode = SettingsMenu.AddNode("Scale");
            ChatPermsMenu = sMenuManager.createMenu("Bots talk in chat", SettingsMenu);
            ChatNode = ChatPermsMenu.GetNode();
            ChatNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            ChatNode.AddListener(sMenuManager.nodeEvent.OnDoubleTapped, ChatPermsMenu.Open);
            zSlideComputer.ActionPermissions.AddNode("TalkInChat", true, (string)null, defaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: ["TalkInChat", ChatNode, onColor]);
            ChatNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: ["TalkInChat", ChatNode]);
            ChatNode.SetColor(onColor);
            scaleNode.AddListener(sMenuManager.nodeEvent.WhileSelected, UpdateScaleByScroll);
            ChatNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: ["TalkInChat"]);
            SettingsMenu.centerNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            SettingsMenu.centerNode.AddListener(sMenuManager.nodeEvent.WhileSelected, UpdateScaleByScroll);
            SettingsMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnTapped, SettingsMenu.parrentMenu.Open);
            SettingsMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, ResetAllSettings);
            scaleNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetScale);
            ChatPermsMenu.centerNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            ChatPermsMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnTapped, ChatPermsMenu.parrentMenu.Open);
            ChatPermsMenu.AddListener(sMenuManager.menuEvent.OnOpened, AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: ["TalkInChat", ChatPermsMenu.centerNode, onColor]);

            ChatSettingsMenu.Setup(ChatPermsMenu);

            UpdateScaleNodeSubtitle();
            SettingsMenu.AddPannel(sMenu.sMenuPannel.Side.top, "More settings coming 'soon'!");
        }

        private static void toggleTalk()
        {
            bool allowed = !(bool)zSlideComputer.ActionPermissions.ValueAt("TalkInChat");
            zSlideComputer.ActionPermissions.SetValue("TalkInChat", allowed);
        }
        private static void ResetAllSettings()
        {
            zSlideComputer.ActionPermissions.ResetToDefault("TalkInChat");
            ResetScale();
        }
        private static void ResetTalkInChat()
        {
            zSlideComputer.ActionPermissions.ResetToDefault("TalkInChat");
        }
        private static void ResetScale()
        {
            sMenuManager.menuSizeScaler = 1;
            UpdateScaleNodeSubtitle();
            sMenuManager.SetMenusScale(sMenuManager.menuSizeScaler);
        }
        private static void UpdateScaleByScroll()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            int normalizedScroll = (int)Mathf.Sign(scroll);
            if (scroll == 0f)
                return;
            UpdateScaleNodeSubtitle();
            sMenuManager.menuSizeScaler += normalizedScroll * menuSizeStep;
            sMenuManager.menuSizeScaler = zHelpers.Round(sMenuManager.menuSizeScaler, 1);
            sMenuManager.menuSizeScaler = Math.Clamp(sMenuManager.menuSizeScaler, 0.3f, 5f);
            sMenuManager.SetMenusScale(sMenuManager.menuSizeScaler);
        }
        private static void UpdateScaleNodeSubtitle()
        {
            scaleNode.SetSubtitle($"<color=#CC840066>[ </color>{sMenuManager.menuSizeScaler}<color=#CC840066> ]</color>");
        }
    }
}
