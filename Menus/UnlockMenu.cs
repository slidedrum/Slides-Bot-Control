using BotControl.CustomActions.CustomActions;
using Player;
using SlideDrum;
using SlideMenu;

namespace BotControl.Menus
{
    public static class UnlockMenuClass
    {
        public const string UnlockMethodMeltKey = "unlockMethodMelt";
        public static sMenu unlockMenu;
        public static sMenu.sMenuNode unlockNode;
        public static void Setup(sMenu menu)
        {
            unlockMenu = menu;
            unlockNode = unlockMenu.GetNode();
            unlockNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            unlockNode.AddListener(sMenuManager.nodeEvent.OnDoubleTapped, unlockMenu.Open);
            unlockMenu.AddPannel(sMenu.sMenuPannel.Side.top, "This controls if the bots are allowed to smash locks on doors/containers");
            unlockMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "Lock Melter only affects automatic use. A direct lock-melter order still works.");
            unlockMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "TODO: Control if bots will auto unlock containers or doors or both");

            OverrideTree<bool?>.Node overrideNode = zSlideComputer.ActionPermissions.AddNode(UnlockMethodMeltKey, null, "Unlock", defaultValue: null, hasDefaultValue: true);
            sMenu.sMenuNode menuNode = unlockMenu.AddNode("Lock Melter");
            overrideNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [UnlockMethodMeltKey, menuNode]);
            overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(PlayerBotActionUnlock)]);
            overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(CustomBotActionOpenContainer)]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [UnlockMethodMeltKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [UnlockMethodMeltKey]);
            unlockMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [UnlockMethodMeltKey]);
        }
    }
}
