using BotControl.Patches;
using Player;
using SlideDrum;
using SlideMenu;
using System;
using System.Collections.Generic;

namespace BotControl.Menus
{
    public static class AttackMenuClass
    {
        public static sMenu attackMenu;
        public static sMenu.sMenuNode attackNode;
        public static sMenu BulletMenu;


        public static void Setup(sMenu menu)
        {
            attackMenu = menu;
            attackNode = menu.GetNode();
            attackMenu.centerNode.ClearListeners(sMenuManager.nodeEvent.OnUnpressedSelected);
            attackMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnTapped, attackMenu.parrentMenu.Open);
            string actionKey;
            OverrideTree<bool?>.Node overrideNode;


            string MeleString = PlayerBotActionAttack.AttackMeansEnum.Melee.ToString();
            actionKey = "attackMeans" + MeleString;
            overrideNode = zSlideComputer.ActionPermissions.AddNode(actionKey, null, "Attack", defaultValue: null, hasDefaultValue: true);
            sMenu.sMenuNode menuNode = attackMenu.AddNode(MeleString);
            overrideNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
            overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(PlayerBotActionAttack)]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            attackMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);

            string BulletString = PlayerBotActionAttack.AttackMeansEnum.Bullet.ToString();
            actionKey = "attackMeans" + BulletString;
            BulletMenu = sMenuManager.createMenu(BulletString);
            overrideNode = zSlideComputer.ActionPermissions.AddNode(actionKey, null, "Attack", defaultValue: null, hasDefaultValue: true);
            menuNode = attackMenu.AddNode(BulletMenu);
            overrideNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
            overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(PlayerBotActionAttack)]);
            menuNode.RemoveListener(sMenuManager.nodeEvent.OnUnpressedSelected);
            menuNode.AddListener(sMenuManager.nodeEvent.OnDoubleTapped, BulletMenu.Open);
            menuNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            attackMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);

            //foreach (var means in AttackActionPatch.meansList)
            //{
                //actionKey = "attackMeans" + means.ToString();
                //overrideNode = zSlideComputer.ActionPermissions.AddNode(actionKey, null, "Attack", defaultValue: null, hasDefaultValue: true);
                //    if (meansBlackList.Contains(means))
                //        continue;
                //    sMenu.sMenuNode menuNode = attackMenu.AddNode(means.ToString());
                //    overrideNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
                //    overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(PlayerBotActionAttack)]);
                //    menuNode.AddListener(sMenuManager.nodeEvent.OnTapped, zSlideComputer.GenericToggleAllowed, args: [actionKey, menuNode]);
                //    menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
                //    attackMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            //}

            actionKey = "MainWeapon";
            overrideNode = zSlideComputer.ActionPermissions.AddNode(actionKey, null, "attackMeans" + BulletString, defaultValue: null, hasDefaultValue: true);
            menuNode = BulletMenu.AddNode(actionKey);
            overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(PlayerBotActionAttack)]);
            overrideNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnTapped, ToggleWeaponSlotPerms, args: [InventorySlot.GearStandard, actionKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            BulletMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);

            actionKey = "SpecialWeapon";
            overrideNode = zSlideComputer.ActionPermissions.AddNode(actionKey, null, "attackMeans" + BulletString, defaultValue: null, hasDefaultValue: true);
            menuNode = BulletMenu.AddNode(actionKey);
            overrideNode.onChanged.Listen(AutomaticActionMenuClass.GenericUpdateNodeAllowedDisplay, args: [actionKey, menuNode]);
            overrideNode.onChanged.Listen(zBotActions.RemoveActions, args: [typeof(PlayerBotActionAttack)]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnTapped, ToggleWeaponSlotPerms, args: [InventorySlot.GearSpecial, actionKey, menuNode]);
            menuNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);
            BulletMenu.centerNode.AddListener(sMenuManager.nodeEvent.OnHeldImmediateSelected, zSlideComputer.ActionPermissions.ResetToDefault, args: [actionKey]);

            BulletMenu.AddPannel(sMenu.sMenuPannel.Side.top, "What weapons are the bots allowed to use?");
            BulletMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "TODO: Make this per bot.");
            BulletMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "May take a while as I want per bot overrides to be global for all permisions, not just weapons.");

            attackMenu.AddPannel(sMenu.sMenuPannel.Side.top, "This controls if the bots are allowed to atack");
            attackMenu.AddPannel(sMenu.sMenuPannel.Side.top, "And what they are allowed to attack with");
            attackMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "These settings should no longer be janky anymore.");
            attackMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "Even when changed in the middle of combat.");
            attackMenu.AddPannel(sMenu.sMenuPannel.Side.bottom, "You can change what weapon they are allowed to use by double tapping bullet.");
        }
        private static void ToggleWeaponSlotPerms(InventorySlot slot, string actionKey, sMenu.sMenuNode node)
        {
            bool allowed = zSlideComputer.GenericToggleAllowed(actionKey, node);
            foreach (PlayerAgent bot in zStaticRefrences.AllBotAgents)
            {
                PlayerAIBot aiBot = bot.GetComponent<PlayerAIBot>();
                RootPlayerBotAction root = aiBot.m_rootAction.ActionBase.TryCast<RootPlayerBotAction>();
                IntPtr pointer = root.m_attackAction.Pointer;
                if (!AttackActionPatch.AllowedGuns.ContainsKey(pointer))
                    AttackActionPatch.AllowedGuns[pointer] = new List<InventorySlot> { InventorySlot.GearSpecial, InventorySlot.GearStandard };
                if (allowed)
                {
                    if (!AttackActionPatch.AllowedGuns[pointer].Contains(slot))
                        AttackActionPatch.AllowedGuns[pointer].Add(slot);
                }
                else
                    AttackActionPatch.AllowedGuns[pointer].Remove(slot);
            }
        }
    }
   
}
