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
        public static string chatPermsString = "TalkInChat";
        public static string AcknowlageString = "NotifyActionAcknowlage";
        public static string SuccessString = "NotifyActionSuccess";
        public static string FailString = "NotifyActionFail";
        // NotifyActionFail
        // NotifyActionSuccess
        // NotifyActionAcknowlage
        // Append action name to the end for overrides.
        public static void Setup(sMenu _Menu) // TODO add a "manual only" option.  Where they only talk in chat if it's a manual action.
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

            FailNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [FailString, FailNode]);
            FailNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [FailString]);
            _Menu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [FailNode, FailString]);
            SuccessNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [SuccessString, SuccessNode]);
            SuccessNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [SuccessString]);
            _Menu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [SuccessNode, SuccessString]);
            AcknowlageNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [AcknowlageString, AcknowlageNode]);
            AcknowlageNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [AcknowlageString]);
            _Menu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [AcknowlageNode, AcknowlageString]);
            zSlideComputer.ActionPermissions.AddNode(FailString, null, chatPermsString, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [FailString, FailNode]);
            zSlideComputer.ActionPermissions.GetNodeFromIdent(FailString).onChanged.Listen(UpdateAllSubnodes);
            zSlideComputer.ActionPermissions.GetNodeFromIdent(FailString).onChanged.Listen(AutomaticActionMenuClass.ApplyTextEffectsToNode, args: [FailNode, FailString]);
            zSlideComputer.ActionPermissions.AddNode(SuccessString, null, chatPermsString, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [SuccessString, SuccessNode]);
            zSlideComputer.ActionPermissions.GetNodeFromIdent(SuccessString).onChanged.Listen(UpdateAllSubnodes);
            zSlideComputer.ActionPermissions.GetNodeFromIdent(SuccessString).onChanged.Listen(AutomaticActionMenuClass.ApplyTextEffectsToNode, args: [SuccessNode, SuccessString]);
            zSlideComputer.ActionPermissions.AddNode(AcknowlageString, null, chatPermsString, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [AcknowlageString, AcknowlageNode]);
            zSlideComputer.ActionPermissions.GetNodeFromIdent(AcknowlageString).onChanged.Listen(UpdateAllSubnodes);
            zSlideComputer.ActionPermissions.GetNodeFromIdent(AcknowlageString).onChanged.Listen(AutomaticActionMenuClass.ApplyTextEffectsToNode, args: [AcknowlageNode, AcknowlageString]);
            //_Menu.centerNode.AddListener(nodeEvent.WhileSelected, _Menu.UpdateCatagoryByScroll);

            Menu = _Menu;
            HashSet<IInputType> AllPressTypes = PressTypeManager.GetAllPressTypes();
            PrioritySet<IInputAction> AllPressActions = new();
            foreach (IInputType pressType in AllPressTypes)
            {
                PrioritySet<IInputAction> PressActions = pressType.GetAllActions();
                AllPressActions.UnionWith(PressActions);
            }
            HashSet<string> uniqueNames = AllPressActions.Where(x => x.Enabled).Select(x => x.FriendlyIdentifier).ToHashSet();
            //HashSet<string> uniqueNames = AllPressActions.Where(x => x.Enabled).Select(x => StripRichText(x.FriendlyIdentifier)).ToHashSet();
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
            string actionKey = nodeName+ chatPermsString;
            sMenu.sMenuNode menuNode = parentMenu.AddNode(nodeName);
            zSlideComputer.ActionPermissions.AddNode(actionKey, null, chatPermsString, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
            sMenu subMenu = sMenuManager.createMenu(nodeName, parentMenu, false);
            menuNode.AddListener(sMenuManager.nodeEvent.OnDoubleTapped, subMenu.Open);// TODO Why does opening this submenu not close when you look away?
            subMenu.centerNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            subMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnTapped, subMenu.parrentMenu.Open);

            sMenu.sMenuNode subFailNode = subMenu.AddNode("Fail");
            subFailNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey + FailString, subFailNode]);
            subFailNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey + FailString]);
            subMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [subFailNode, actionKey + FailString]);
            zSlideComputer.ActionPermissions.AddNode(actionKey + FailString, null, actionKey, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey + FailString, subFailNode]);
            subNodes.Add((subFailNode, actionKey + FailString));

            sMenu.sMenuNode subSuccessNode = subMenu.AddNode("Success");
            subSuccessNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey + SuccessString, subSuccessNode]);
            subSuccessNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey + SuccessString]);
            subMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [subSuccessNode, actionKey + SuccessString]);
            zSlideComputer.ActionPermissions.AddNode(actionKey + SuccessString, null, actionKey, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey + SuccessString, subSuccessNode]);
            subNodes.Add((subSuccessNode, actionKey + SuccessString));

            sMenu.sMenuNode subAcknowlageNode = subMenu.AddNode("Acknowlage");
            subAcknowlageNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey + AcknowlageString, subAcknowlageNode]);
            subAcknowlageNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey + AcknowlageString]);
            subMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [subAcknowlageNode, actionKey + AcknowlageString]);
            zSlideComputer.ActionPermissions.AddNode(actionKey + AcknowlageString, null, actionKey, defaultValue: null, hasDefaultValue: true).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey + AcknowlageString, subAcknowlageNode]);
            subNodes.Add((subAcknowlageNode, actionKey + AcknowlageString));

            zSlideComputer.ActionPermissions.AddFallback(actionKey + FailString, FailString);
            zSlideComputer.ActionPermissions.AddFallback(actionKey + SuccessString, SuccessString);
            zSlideComputer.ActionPermissions.AddFallback(actionKey + AcknowlageString, AcknowlageString);

            menuNode.AddListener(sMenuManager.nodeEvent.OnTappedExclusive, zSlideComputer.GenericToggleAllowed, args: [actionKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            parentMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetToDefault, args: [menuNode, actionKey]);
            parentMenu.AddNodeToCatagory("Actions", nodeName);
        }
    }
}
