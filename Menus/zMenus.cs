using Player;
using SlideDrum;
using SlideMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using ZombieTweak2.zRootBotPlayerAction.CustomActions;

namespace BotControl.Menus
{
    public static class zMenus
    {
        //This is the class that actually creates the menue instances
        public static List<sMenu> botMenus;

        public static void CreateMenus()
        {
            sMenuManager.ClearAllMenus();
            OverrideTree<bool?>.ResetTrees();
            OverrideTree<float?>.ResetTrees();
            OverrideTree<int?>.ResetTrees();
            AutomaticActionMenuClass.Setup(sMenuManager.createMenu("Automatic Actions", sMenuManager.mainMenu));
            sMenuManager.mainMenu.AddPannel(sMenu.sMenuPannel.Side.top, "<size=150><color=#CC840066>Slide's Bot Control</color></size>");
            sMenuManager.mainMenu.AddPannel(sMenu.sMenuPannel.Side.top, $"<color=#CC840066>[ </color><color=#26262c>V{ZiMain.version}</color><color=#CC840066> ]</color>");
            sMenuManager.mainMenu.radius = 100f;
            if (ZiMain.extraActionMenus) 
            { 
                ManualActionMenuClass.Setup(sMenuManager.createMenu("Manual Actions", sMenuManager.mainMenu));
                sMenuManager.createMenu("Contextual Actions", sMenuManager.mainMenu);
            }
            SettingsMenuClass.Setup(sMenuManager.createMenu("Settings", sMenuManager.mainMenu));
            ContextMenu.Setup(sMenuManager.createMenu("Context Menu", sMenuManager.mainMenu));
            if (ZiMain.VoiceMenu)
                sMenuManager.createMenu("Voice menu", sMenuManager.mainMenu);
            if (ZiMain.debugMode)
                DebugMenuClass.Setup(sMenuManager.createMenu("Debug", sMenuManager.mainMenu));
        }
    }
    public static class ManualActionMenuClass
    {
        public static sMenu manualActionMenu;
        public static sMenu.sMenuNode manuActionNode;
        public static void Setup(sMenu menu)
        {
            manualActionMenu = menu;
            manuActionNode = manualActionMenu.GetNode();
            manualActionMenu.AddNode("Clear Room", StartClearRoomAction);
        }
        public static void StartClearRoomAction()
        {
            return;
            //PlayerAgent playerAgent = PlayerManager.GetLocalPlayerAgent();
            //PlayerAIBot bot = null;
            //PlayerAgent botAgent = null;
            //float closestDistance = float.MaxValue;
            //foreach (var agent in PlayerManager.PlayerAgentsInLevel)
            //{
            //    if (!agent.Owner.IsBot || !agent.Alive)
            //        continue;
            //    float distance = Vector3.Distance(agent.Position, playerAgent.Position);
            //    if (distance < closestDistance)
            //    {
            //        closestDistance = distance;
            //        botAgent = agent;
            //        bot = agent.gameObject.GetComponent<PlayerAIBot>();
            //    }
            //}
            //var action = new ClearRoomAction.Descriptor(bot);
            //bot.StartAction(action);
        }
    }
    


    [Obsolete]
    public static class SelectionMenuClass
    {
        public static Dictionary<int, bool> botSelection = new();
        public static Dictionary<int, sMenu.sMenuNode> selectionBotNodes = new();
        public static Color selectedColor = new Color(0.25f, 0.16175f, 0.0f);
        public static PlayerAIBot toggleBotSelection(PlayerAIBot bot)
        {
            if (checkForUntrackedBot(bot))
                return null;
            botSelection[bot.Agent.Owner.PlayerSlotIndex()] = !botSelection[bot.Agent.Owner.PlayerSlotIndex()];
            return bot;
        }
        public static List<PlayerAIBot> getSelectedBots()
        {
            List<PlayerAIBot> selectedBots = new();
            var allBots = ZiMain.GetBotList();
            foreach (PlayerAIBot bot in allBots)
            {
                bool botSelected = botSelection[bot.Agent.Owner.PlayerSlotIndex()];
                if (botSelected)
                {
                    selectedBots.Add(bot);
                }
            }
            return selectedBots;
        }
        public static void SelectionFlipAllBots()
        {
            foreach (var bot in ZiMain.GetBotList())
            {
                toggleBotSelection(bot);
                updateColorBaesdOnSelection(selectionBotNodes[bot.Agent.Owner.PlayerSlotIndex()], bot);
            }
        }
        public static void SelectionToggleAllBots()
        {
            int selectedCount = botSelection.Values.Count(value => value);
            int unselectedCount = botSelection.Values.Count() - selectedCount;
            bool majority = selectedCount > unselectedCount;
            foreach (var bot in ZiMain.GetBotList())
            {
                setBotSelection(bot, !majority);
                updateColorBaesdOnSelection(selectionBotNodes[bot.Agent.Owner.PlayerSlotIndex()], bot);
            }
        }
        public static PlayerAIBot setBotSelection(PlayerAIBot bot, bool selected)
        {
            if (checkForUntrackedBot(bot))
                return null;
            botSelection[bot.Agent.Owner.PlayerSlotIndex()] = selected;
            return bot;
        }
        private static bool checkForUntrackedBot(PlayerAIBot bot)
        {
            if (bot == null)
            {
                ZiMain.log.LogError("Can't toggle bot selection of null!  This should not happen.");
                return true;
            }
            if (!botSelection.ContainsKey(bot.Agent.Owner.PlayerSlotIndex()))
                throw new KeyNotFoundException($"The bot {bot} is not tracked for selection.  This should't happen.");
            return false;
        }
        public static sMenu.sMenuNode updateColorBaesdOnSelection(sMenu.sMenuNode node, PlayerAIBot bot)
        {
            if (checkForUntrackedBot(bot))
                return null;
            if (botSelection[bot.Agent.Owner.PlayerSlotIndex()])
                node.SetColor(selectedColor);
            else
                node.SetColor(sMenuManager.defaultColor);
            return node;
        }
    }
}
