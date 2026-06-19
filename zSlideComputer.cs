using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP.UnityEngine;
using BotControl.Menus;
using BotControl.SmartSelect;
using GameData;
using Player;
using SlideDrum;
using SlideMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BotControl
{
    public static class zSlideComputer
    {
        //This class is for handling things like stopping bots from do unwanted actions

        //public static Il2CppSystem.Collections.Generic.Dictionary<uint, float> itemPrios = new();
        //public static Il2CppSystem.Collections.Generic.Dictionary<uint, int> resourceThresholds = new();
        //public static Il2CppSystem.Collections.Generic.Dictionary<uint, bool> enabledItemPrios = new ();
        //public static Il2CppSystem.Collections.Generic.Dictionary<uint, bool> enabledResourceShares = new ();
        //public static Il2CppSystem.Collections.Generic.Dictionary<uint, float> OriginalItemPrios = new();
        public static OverrideTree<float?> ActionPriorities;
        public static OverrideTree<bool?> ActionPermissions;
        public static ConfigEntry<KeyCode> MenuButton;
        public static ConfigEntry<KeyCode> SmartSelectButton;
        public static ConfigFile ConfigurationFile;
        public static string CustomConfigName = "BotControl.Slide";

        //public static OverrideTree<int?> ItemPickupPriorities;
        //public static Dictionary<string, sMenu.sMenuNode> actionNameToMenuNodes;
        public static class PermissionDefinitions
        {
            private static Dictionary<string, PermissionDefinition> permissionDeffinitions = new();

            private static List<string> _actionKeysCache = new();
            private static List<string> ActionKeys
            {
                get
                {
                    // Check semantic equality (ignoring order)
                    bool matches =
                        _actionKeysCache.Count == permissionDeffinitions.Count &&
                        !_actionKeysCache.Except(permissionDeffinitions.Keys).Any() &&
                        !permissionDeffinitions.Keys.Except(_actionKeysCache).Any();

                    // Rebuild cache if mismatch
                    if (!matches)
                    {
                        _actionKeysCache = permissionDeffinitions.Keys
                            .OrderBy(k => k, StringComparer.Ordinal)
                            .ToList();
                    }

                    return _actionKeysCache;
                }
            }
            private class PermissionDefinition
            {
                //Might be able to remove the PermissionDefinition entirely?
                //Even if not, probably want to rename it, as it doesn't really describe what it is anymore.
                //public Dictionary<int, bool?> perms;
                public sMenu.sMenuNode node;
                public List<Type> actionTypesToCull;
                //public float defaultPriority;
                public string key;
                internal PermissionDefinition(string key, sMenu.sMenuNode node = null, List<Type> ActionTypesToCull = null)
                {
                    this.key = key;
                    //this.perms = new Dictionary<int, bool?>();
                    //for (int i = 1; i < 4; i++)
                    //    perms[i] = defaultPerm;
                    this.node = node;
                    this.actionTypesToCull = ActionTypesToCull;
                    if (this.actionTypesToCull == null)
                        actionTypesToCull = new List<Type>();
                    //this.defaultPriority = defaultPriority ?? 0f;
                }
            }
            public static void ClearPermissionDefinitions()
            {
                permissionDeffinitions.Clear();
            }
            public static void CreatePermissionDeffinition(string key, bool? defaultPerm = true, sMenu.sMenuNode node = null, sMenu menu = null, Type ActionTypeToCull = null, float? defaultPriority = null, string parrentKey = null, bool hasDefaultValue = false)
            {
                List<Type> actionTypesToCull = new();
                if (ActionTypeToCull != null)
                    actionTypesToCull.Add(ActionTypeToCull);
                CreatePermissionDeffinition(key, defaultPerm, node, menu, actionTypesToCull, defaultPriority, parrentKey, hasDefaultValue);
            }
            public static void CreatePermissionDeffinition(string key, bool? defaultPerm = true, sMenu.sMenuNode node = null, sMenu menu = null, List<Type> ActionTypesToCull = null, float? defaultPriority = null, string parrentKey = null, bool hasDefaultValue = false)
            {
                //PermissionDefinition permissionDef = new PermissionDefinition(key, node, ActionTypesToCull);
                //permissionDeffinitions.Add(key, permissionDef);
                //Might be able to remove the PermissionDefinition entirely?
                if (defaultPriority != null)
                {
                    zSlideComputer.ActionPriorities.AddNode("Default" + key, defaultPriority, (string?)null, defaultValue: defaultPriority);
                    zSlideComputer.ActionPriorities.AddNode(key, defaultPriority, "Default" + key, defaultValue: defaultPriority).onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodePrioDisplay, [node]);
                }
                var permissionsNode = ActionPermissions.AddNode(key, defaultPerm, parrentKey, defaultValue: defaultPerm, hasDefaultValue: hasDefaultValue);
                permissionsNode.onChanged.Listen((Action<List<Type>>)zSlideComputer.RemoveActionsOfType, args: [ActionTypesToCull]);
                if (node != null)
                {
                    //actionNameToMenuNodes[key] = node;
                    permissionsNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [key, node]);
                }
                if (menu != null)
                    permissionsNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [key, menu.centerNode]);

            }
            public static bool KeyExists(string key)
            {
                return ActionKeys.Contains(key);
            }
            public static int KeyToId(string key)
            {
                if (!KeyExists(key))
                {
                    ZiMain.log.LogWarning($"Unknown actionKey '{key}' when converting to id.");
                    return -1;
                }
                return ActionKeys.IndexOf(key);
            }
            public static string IdToKey(int id)
            {
                if (id < 0 || id >= ActionKeys.Count)
                {
                    ZiMain.log.LogWarning($"Unknown actionId '{id}' when converting to key.");
                    return null;
                }
                return ActionKeys[id];
            }
        }

        

        

        public static void Init()
        {
            if (ZiMain.HasBetterBots)
                BBCompat.SetBotsOpenContainersToFalse();
            zSmartSelectHud.Setup();
            ConfigurationFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "BotControl.cfg"), true);
            MenuButton = ConfigurationFile.Bind<KeyCode>(
                "Keybinds",
                "MenuButton",
                KeyCode.X,
                "Button to open and navigate the menu"
            );
            SmartSelectButton = ConfigurationFile.Bind<KeyCode>(
                "Keybinds",
                "SmartSelectButton",
                KeyCode.V,
                "Button to open use Smart Select"
            );
            ConfigurationFile.Save();
            zSmartSelect.IsSetUp = false;
        }
        public static Dictionary<string, float> GetBotItems()
        {
            Dictionary<string,float> botItems = new();
            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<uint, float> item in RootPlayerBotAction.s_itemBasePrios)
            {
                uint id = item.Key;
                float priority = item.Value;
                ItemDataBlock block = ItemDataBlock.s_blockByID[id];
                string name = block.publicName;
                botItems[name] = priority;
                ZiMain.log.LogMessage($"{name}:{priority}");
            }
            return botItems;
        }
        internal static void GenericToggleAllowed(string actionKey, sMenu.sMenuNode node, bool allowDissabled = false)
        {
            if (!allowDissabled && !node.gameObject.activeInHierarchy)
                return;
            bool allowed = !(bool)zSlideComputer.ActionPermissions.ValueAt(actionKey);
            zSlideComputer.ActionPermissions.SetValue(actionKey, allowed);
        }
        public static void RemoveActionsOfType(List<Type> actionTypes)
        {
            foreach (var actionType in actionTypes)
                RemoveActionsOfType(actionType);
        }
        public static void RemoveActionsOfType(Type actionType)
        { 
            var allBots = ZiMain.GetBotList();
            foreach(var bot in allBots)
                RemoveActionsOfType(bot.Agent, actionType);
        }
        public static void RemoveActionsOfType(PlayerAgent agent, Type actionType)
        {
            //todo add more variants of this method with different arguments
            if (!typeof(PlayerBotActionBase).IsAssignableFrom(actionType))
                return;
            if (!agent.Owner.IsBot)
                return;
            List<PlayerBotActionBase> actionsToRemove = new List<PlayerBotActionBase>();
            PlayerAIBot aiBot = agent.gameObject.GetComponent<PlayerAIBot>();
            foreach (var action in aiBot.Actions)
            {
                if (action.GetIl2CppType().Name == actionType.Name)
                {
                    actionsToRemove.Add(action);
                }
            }
            foreach (var action in actionsToRemove)
            {
                aiBot.StopAction(action.DescBase);
            }
        }
    }
}
