using BotControl.SmartSelect.PressActions;
using BotControl.SmartSelect.PressTypes;
using PrioritySet;
using SlideMenu;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BotControl.Menus
{
    public static class ChatSettingsMenu
    {
        public static sMenu SettingsMenu;
        public static Color onColor = new Color(0, 0.2f, 0);
        public static sMenu Menu;
        public static List<(sMenu.sMenuNode Node,string ActionKey)> subNodes = new();
        // NotifyActionFail
        // NotifyActionSuccess
        // NotifyActionAcknowlage
        // Append action name to the end for overrides.
        public static void Setup(sMenu _Menu)
        {
            // TODO fix the network spam whenenever you do anything here.
            sMenu.sMenuNode FailNode = _Menu.AddNode("Fail");
            sMenu.sMenuNode SuccessNode = _Menu.AddNode("Success");
            sMenu.sMenuNode AcknowlageNode = _Menu.AddNode("Acknowlage");

            _Menu.AddCatagory("States");
            _Menu.AddCatagory("Actions");
            _Menu.AddNodeToCatagory("States", "Fail");
            _Menu.AddNodeToCatagory("States", "Success");
            _Menu.AddNodeToCatagory("States", "Acknowlage");

            FailNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: ["NotifyActionFail", FailNode]);
            FailNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: ["NotifyActionFail"]);
            _Menu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [FailNode, "NotifyActionFail"]);
            SuccessNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: ["NotifyActionSuccess", SuccessNode]);
            SuccessNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: ["NotifyActionSuccess"]);
            _Menu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [SuccessNode, "NotifyActionSuccess"]);
            AcknowlageNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: ["NotifyActionAcknowlage", AcknowlageNode]);
            AcknowlageNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: ["NotifyActionAcknowlage"]);
            _Menu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [AcknowlageNode, "NotifyActionAcknowlage"]);
            zSlideComputer.ActionPermissions.AddNode("NotifyActionFail", null, "TalkInChat", defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: ["NotifyActionFail", FailNode]);
            zSlideComputer.ActionPermissions.GetNodeFromIdent("NotifyActionFail").onChanged.Listen(UpdateAllSubnodes);
            zSlideComputer.ActionPermissions.GetNodeFromIdent("NotifyActionFail").onChanged.Listen(AutomaticActionMenuClass.ApplyTextEffectsToNode, args: [FailNode, "NotifyActionFail"]);
            zSlideComputer.ActionPermissions.AddNode("NotifyActionSuccess", null, "TalkInChat", defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: ["NotifyActionSuccess", SuccessNode]);
            zSlideComputer.ActionPermissions.GetNodeFromIdent("NotifyActionSuccess").onChanged.Listen(UpdateAllSubnodes);
            zSlideComputer.ActionPermissions.GetNodeFromIdent("NotifyActionSuccess").onChanged.Listen(AutomaticActionMenuClass.ApplyTextEffectsToNode, args: [SuccessNode, "NotifyActionSuccess"]);
            zSlideComputer.ActionPermissions.AddNode("NotifyActionAcknowlage", null, "TalkInChat", defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: ["NotifyActionAcknowlage", AcknowlageNode]);
            zSlideComputer.ActionPermissions.GetNodeFromIdent("NotifyActionAcknowlage").onChanged.Listen(UpdateAllSubnodes);
            zSlideComputer.ActionPermissions.GetNodeFromIdent("NotifyActionAcknowlage").onChanged.Listen(AutomaticActionMenuClass.ApplyTextEffectsToNode, args: [AcknowlageNode, "NotifyActionAcknowlage"]);
            //_Menu.centerNode.AddListener(nodeEvent.WhileSelected, _Menu.UpdateCatagoryByScroll);

            Menu = _Menu;
            HashSet<IPressType> AllPressTypes = PressTypeManager.GetAllPressTypes();
            PrioritySet<IPressAction> AllPressActions = new();
            foreach (IPressType pressType in AllPressTypes)
            {
                PrioritySet<IPressAction> PressActions = pressType.GetAllActions();
                AllPressActions.UnionWith(PressActions);
            }
            HashSet<string> uniqueNames = AllPressActions.Where(x => x.Enabled).Select(x => StripRichText(x.FriendlyIdentifier)).ToHashSet();
            foreach (string name in uniqueNames)
            {
                SetupNode(_Menu, name);
            }
            _Menu.radius = 130f;
            _Menu.setNodeSize(0.75f);
            _Menu.SetCatagory("States");
        }
        public static void ResetToDefault(sMenu.sMenuNode Node, string ActonKey)
        {
            if (Node.gameObject.activeInHierarchy)
                zSlideComputer.ActionPermissions.ResetToDefault(ActonKey);
        }
        public static string StripRichText(string text)
        {
            return Regex.Replace(text, "<.*?>", string.Empty);
        }
        public static void UpdateAllSubnodes()
        {
            foreach(var node in subNodes)
            {
                AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay(node.ActionKey, node.Node);
            }
        }
        private static void SetupNode(sMenu parentMenu, string nodeName)
        {
            string actionKey = nodeName+"TalkInChat";
            sMenu.sMenuNode menuNode = parentMenu.AddNode(nodeName);
            zSlideComputer.ActionPermissions.AddNode(actionKey, null, "TalkInChat", defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
            sMenu subMenu = sMenuManager.createMenu(nodeName, parentMenu, false);
            menuNode.AddListener(sMenuManager.nodeEvent.OnDoubleTapped, subMenu.Open);// TODO Why does opening this submenu not close when you look away?
            subMenu.centerNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            subMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnTapped, subMenu.parrentMenu.Open);

            sMenu.sMenuNode subFailNode = subMenu.AddNode("Fail");
            subFailNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey + "NotifyActionFail", subFailNode]);
            subFailNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey + "NotifyActionFail"]);
            subMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [subFailNode, actionKey + "NotifyActionFail"]);
            zSlideComputer.ActionPermissions.AddNode(actionKey + "NotifyActionFail", null, actionKey, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey + "NotifyActionFail", subFailNode]);
            subNodes.Add((subFailNode, actionKey + "NotifyActionFail"));

            sMenu.sMenuNode subSuccessNode = subMenu.AddNode("Success");
            subSuccessNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey + "NotifyActionSuccess", subSuccessNode]);
            subSuccessNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey + "NotifyActionSuccess"]);
            subMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [subSuccessNode, actionKey + "NotifyActionSuccess"]);
            zSlideComputer.ActionPermissions.AddNode(actionKey + "NotifyActionSuccess", null, actionKey, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey + "NotifyActionSuccess", subSuccessNode]);
            subNodes.Add((subSuccessNode, actionKey + "NotifyActionSuccess"));

            sMenu.sMenuNode subAcknowlageNode = subMenu.AddNode("Acknowlage");
            subAcknowlageNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey + "NotifyActionAcknowlage", subAcknowlageNode]);
            subAcknowlageNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey + "NotifyActionAcknowlage"]);
            subMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [subAcknowlageNode, actionKey + "NotifyActionAcknowlage"]);
            zSlideComputer.ActionPermissions.AddNode(actionKey + "NotifyActionAcknowlage", null, actionKey, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey + "NotifyActionAcknowlage", subAcknowlageNode]);
            subNodes.Add((subAcknowlageNode, actionKey + "NotifyActionAcknowlage"));

            zSlideComputer.ActionPermissions.AddFallback(actionKey + "NotifyActionFail", "NotifyActionFail");
            zSlideComputer.ActionPermissions.AddFallback(actionKey + "NotifyActionSuccess", "NotifyActionSuccess");
            zSlideComputer.ActionPermissions.AddFallback(actionKey + "NotifyActionAcknowlage", "NotifyActionAcknowlage");

            menuNode.AddListener(sMenuManager.nodeEvent.OnTappedExclusive, zSlideComputer.GenericToggleAllowed, args: [actionKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            parentMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [menuNode, actionKey]);
            parentMenu.AddNodeToCatagory("Actions", nodeName);
        }
    }
}
