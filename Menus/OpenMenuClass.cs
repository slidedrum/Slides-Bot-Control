using BotControl.CustomActions.CustomActions;
using Player;
using SlideDrum;
using SlideMenu;
using System;

namespace BotControl.Menus
{
    internal class OpenMenuClass
    {
        public static sMenu OpenMenu;
        public static sMenu.sMenuNode OpenNode;
        public static void Setup(sMenu menu)

        {
            OpenMenu = menu;
            OpenNode = menu.GetNode();
            OpenMenu.centerNode.RemoveListener(sMenuManager.nodeEvent.OnUnpressedSelected);
            OpenMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnTapped, OpenMenu.parrentMenu.Open);
            foreach(PlayerBotActionUnlock.Descriptor.MethodEnum Method in Enum.GetValues<PlayerBotActionUnlock.Descriptor.MethodEnum>())
            {
                if (Method == PlayerBotActionUnlock.Descriptor.MethodEnum.None || Method == PlayerBotActionUnlock.Descriptor.MethodEnum.Any)
                    continue;
                string actionKey = "openMethod" + Method.ToString();
                OverrideTree<bool?>.Node overrideNode = zSlideComputer.ActionPermissions.AddNode(actionKey, null, "Open", defaultValue: null, hasDefaultValue: true);
                sMenu.sMenuNode menuNode = OpenMenu.AddNode(Method.ToString());
                overrideNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
                overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(CustomBotActionOpenContainer)]);
                menuNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey, menuNode]);
                menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
                OpenMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            }
        }
    }
}
