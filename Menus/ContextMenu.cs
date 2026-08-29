using BotControl.Patches;
using LevelGeneration;
using SlideDrum;
using SlideMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BotControl.Menus
{
    internal class ContextMenu
    {
        public static sMenu contextMenu;
        public static sMenu ManualAreaMenu;
        public static sMenu PickupZoneOveridesMenu;
        public static sMenu.sMenuNode contextNode;

        public static void Setup(sMenu menu)
        {
            contextMenu = menu;
            contextNode = menu.GetNode();
            PickupZoneOveridesMenu = sMenuManager.createMenu("Pickup zone overrides", menu);
            PickupZoneOveridesMenu.AddNode("Room Manual", AddCurrentRoomManual);
            PickupZoneOveridesMenu.AddNode("Zone Manual", AddCurrentZoneManual);
            ManualAreaMenu = sMenuManager.createMenu("Manual Areas", PickupZoneOveridesMenu);
        }
        internal static void AddCurrentRoomManual()
        {
            PickupActionPatch.AreaPermOverideLocations.Add(zStaticRefrences.LocalPlayer.CourseNode.m_area);
            if (ManualAreaMenu.HasNode(zStaticRefrences.LocalPlayer.CourseNode.m_area.m_geoArea)) 
            {
                ManualAreaMenu.EnableNode(zStaticRefrences.LocalPlayer.CourseNode.m_area.m_geoArea);
            }
            else
            {
                ManualAreaMenu.AddNode(zStaticRefrences.LocalPlayer.CourseNode.m_area.m_geoArea, RemoveManualRoom, zStaticRefrences.LocalPlayer.CourseNode.m_area);
            }
        }
        internal static void AddCurrentZoneManual()
        {
            PickupActionPatch.ZonePermOverideLocations.Add(zStaticRefrences.LocalPlayer.CourseNode.m_zone);
            if (ManualAreaMenu.HasNode(zStaticRefrences.LocalPlayer.CourseNode.m_zone.AliasName))
            {
                ManualAreaMenu.EnableNode(zStaticRefrences.LocalPlayer.CourseNode.m_zone.AliasName);
            }
            else
            {
                ManualAreaMenu.AddNode(zStaticRefrences.LocalPlayer.CourseNode.m_zone.AliasName, RemoveManualZone, zStaticRefrences.LocalPlayer.CourseNode.m_zone);
            }
        }
        internal static void RemoveManualRoom(LG_Area area)
        {
            PickupActionPatch.AreaPermOverideLocations.Remove(area);
            if (ManualAreaMenu.HasNode(zStaticRefrences.LocalPlayer.CourseNode.m_area.m_geoArea))
            {
                ManualAreaMenu.DisableNode(zStaticRefrences.LocalPlayer.CourseNode.m_area.m_geoArea);
            }

        }
        internal static void RemoveManualZone(LG_Zone zone)
        {
            PickupActionPatch.ZonePermOverideLocations.Remove(zone);
            if (ManualAreaMenu.HasNode(zStaticRefrences.LocalPlayer.CourseNode.m_zone.AliasName))
            {
                ManualAreaMenu.DisableNode(zStaticRefrences.LocalPlayer.CourseNode.m_zone.AliasName);
            }
        }
    }
}
