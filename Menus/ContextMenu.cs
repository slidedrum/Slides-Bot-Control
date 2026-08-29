using BotControl.Patches;
using LevelGeneration;
using SlideMenu;
using System.Collections.Generic;
using static SlideMenu.sMenu;

namespace BotControl.Menus
{
    internal class ContextMenu
    {
        public static sMenu contextMenu;
        public static sMenu ManualAreaMenu;
        public static sMenu PickupZoneOveridesMenu;
        public static Dictionary<string, sMenu> zoneMenus = new Dictionary<string, sMenu>();
        public static sMenu.sMenuNode contextNode;

        public static void Setup(sMenu menu)
        {
            contextMenu = menu;
            contextNode = menu.GetNode();
            PickupZoneOveridesMenu = sMenuManager.createMenu("Pickup zone overrides", menu);
            //PickupZoneOveridesMenu.AddNode("Room Manual", AddCurrentRoomManual);
            //PickupZoneOveridesMenu.AddNode("Zone Manual", AddCurrentZoneManual);
            //ManualAreaMenu = sMenuManager.createMenu("Manual Areas", PickupZoneOveridesMenu);
            zSlideComputer.ActionPermissions.AddNode("PickupAreas", null, hasDefaultValue: true, parent: "Pickup");
            foreach (LG_Zone zone in Builder.CurrentFloor.allZones)
            {
                zoneMenus[zone.AliasName] = sMenuManager.createMenu(zone.AliasName, PickupZoneOveridesMenu);
                sMenuNode ZoneNode = PickupZoneOveridesMenu.GetNode(zone.AliasName);
                ZoneNode.RemoveListener(sMenuManager.nodeEvent.OnUnpressedSelected);
                PickupZoneOveridesMenu.centerNode.RemoveListener(sMenuManager.nodeEvent.OnUnpressedSelected);
                PickupZoneOveridesMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnTapped, PickupZoneOveridesMenu.parrentMenu.Open);
                PickupZoneOveridesMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, ResetNode, zone.AliasName);
                ZoneNode.AddListener(sMenuManager.nodeEvent.OnTapped, ToggleNode, zone.AliasName);
                ZoneNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetNode, zone.AliasName);
                ZoneNode.AddListener(sMenuManager.nodeEvent.OnDoubleTapped, zoneMenus[zone.AliasName].Open);
                zSlideComputer.ActionPermissions.AddNode(zone.AliasName, null, hasDefaultValue: true, parent: "PickupAreas", onChanged: new FlexibleMethodDefinition(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [zone.AliasName, ZoneNode]));
                foreach (LG_Area area in zone.m_areas)
                {
                    string name = $"{zone.AliasName} {area.m_geoArea}";
                    if (zoneMenus[zone.AliasName].GetNode(name) != null)
                        continue;
                    sMenuNode AreaNode = zoneMenus[zone.AliasName].AddNode(name);
                    AreaNode.AddListener(sMenuManager.nodeEvent.OnTapped, ToggleNode, name);
                    AreaNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediate, ResetNode, name);
                    zoneMenus[zone.AliasName].centerNode.RemoveListener(sMenuManager.nodeEvent.OnUnpressedSelected);
                    zoneMenus[zone.AliasName].centerNode.AddListener(sMenuManager.nodeEvent.OnTapped, PickupZoneOveridesMenu.parrentMenu.Open);
                    zoneMenus[zone.AliasName].centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, ResetNode, name);
                    zSlideComputer.ActionPermissions.AddNode(name, null, hasDefaultValue: true, parent: zone.AliasName, onChanged: new FlexibleMethodDefinition(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [name, AreaNode]));
                }
            }
        }
        private static void ToggleNode(string nodeName)
        {
            bool? own = zSlideComputer.ActionPermissions.GetValue(nodeName);
            zSlideComputer.ActionPermissions.SetValue(nodeName, own == false);
        }
        private static void ResetNode(string nodeName)
        {
            zSlideComputer.ActionPermissions.ResetToDefault(nodeName);
        }
    }
}
