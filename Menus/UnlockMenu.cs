using BotControl.CustomActions.CustomActions;
using Player;
using SlideDrum;
using SlideMenu;

namespace BotControl.Menus
{
    public static class UnlockMenuClass
    {
        public const string UnlockMethodMeltKey = "unlockMethodMelt";
        public const string UnlockMethodMeleeKey = "unlockMethodMelee";
        public static sMenu unlockMenu;
        public static sMenu.sMenuNode unlockNode;
        public static void Setup(sMenu menu)
        {
            unlockMenu = menu;
            unlockNode = unlockMenu.GetNode();
            unlockNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            unlockNode.AddListener(sMenuManager.nodeEvent.OnDoubleTapped, unlockMenu.Open);
            unlockMenu.AddPannel(sMenu.sMenuPannel.Side.top, "This controls if the bots are allowed to smash locks on doors/containers");
            unlockMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "Melee and Lock Melter only affect automatic use. A direct lock-melter order still works.");
            unlockMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "TODO: Control if bots will auto unlock containers or doors or both");

            AddMethodNode(UnlockMethodMeleeKey, "Melee");
            AddMethodNode(UnlockMethodMeltKey, "Lock Melter");
        }

        private static void AddMethodNode(string actionKey, string label)
        {
            OverrideTree<bool?>.Node overrideNode = zSlideComputer.ActionPermissions.AddNode(actionKey, null, "Unlock", defaultValue: null, hasDefaultValue: true);
            sMenu.sMenuNode menuNode = unlockMenu.AddNode(label);
            overrideNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
            overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(PlayerBotActionUnlock)]);
            overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(CustomBotActionOpenContainer)]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            unlockMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
        }
    }
}
