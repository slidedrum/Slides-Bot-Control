using SlideMenu;

namespace BotControl.Menus
{
    internal class ContextMenu
    {
        public static sMenu contextMenu;
        public static sMenu.sMenuNode contextNode;

        public static void Setup(sMenu menu)
        {
            contextMenu = menu;
            contextNode = menu.GetNode();
        }
    }
}
