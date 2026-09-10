using SlideMenu;

namespace BotControl.Menus
{
    public static class ExploreMenuClass
    {
        private static sMenu exploreMenu;
        private static sMenu.sMenuNode exploreNode;
        internal static void Setup(sMenu menu)
        {
            exploreMenu = menu;
            exploreNode = exploreMenu.parrentMenu.GetNode(exploreMenu.centerNode.text);
            exploreNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            exploreNode.AddListener(sMenuManager.nodeEvent.OnTapped, ToggleExplorePerms);
            exploreNode.AddListener(sMenuManager.nodeEvent.OnDoubleTapped, exploreMenu.Open);
            exploreMenu.AddPannel(sMenu.sMenuPannel.Side.top, "Nothing here yet\nThis will be used to let you customize the new explore action");
            exploreMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "This action tells bots to explore the area when no enemies are detected.\nThey will leave the follow radius and pickup/interact with objects.\nAs soon as an enemy is found, they will all return.\nTODO allow you to tell them to explore even with enemies (with the potential for them to wake up the room)");
        }
        private static void ToggleExplorePerms()
        {
            return;
            //var bots = zSearch.GetAllBotAgents();
            //foreach (var bot in bots)
            //{
            //    ExploreAction.ToggleExplorePerm(bot);
            //}
            //if (ExploreAction.GetExplorePerm(bots[0]))
            //    exploreNode.SetColor(sMenuManager.defaultColor);
            //else
            //    exploreNode.SetColor(new Color(0.25f, 0f, 0f));
        }
    }
}
