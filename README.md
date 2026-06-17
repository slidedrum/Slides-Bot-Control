### Preface:
This started as continuation of [Zombified Initiative](https://thunderstore.io/c/gtfo/p/hirnukuono/Zombified_Initiative/) by [hirnukuono](https://thunderstore.io/c/gtfo/p/hirnukuono/) however at this point there is very little code from that mod left.

### Press X to open and navigate the menu.
### Press V to use smart select.

# Intro:
Have you ever been frustrated that a bot just used all of your disinfect as soon as you get out of the fog, even though you're going to have to go right back?!  That's the catalyst for me making this mod.  The goal of this mod is to give you the ability to have more control over the bots in GTFO, but not to give them any direct buffs or new abilities.  I want to allow you to tell them exactly what you do, and don't want them to be able to do.  

# How to use:
Press X to open the menu, and then press X again on "automatic actions" to choose what the bots are allowed to do.  Single tap on an action to toggle if it's allowed.  Double tap an action to change advanced settings. tap the center node to go back to the previous menu. press and hold to reset to default settings. Advanced menus have things like resource share thresholds, or what bots are and aren't allowed to pick up.  Some menus allow you to use the scroll wheel to change additional settings.  Things like the priority of the action, or the min/max follow distance.  You can also use the scroll wheel on the center menu node to change the category of the current menu, to show/hide some icons.

There is also smart select!  Press V while looking at a bot to select that bot.  And then you can tell them to do one of 4 different actions at any given time.  You can tell them to do things like, stand in a spesific spot, hack a lock, pick up a spesific item, deploy their turret, and many more!  See How To Use Smart Select for the full list of things they can do.

## Important note about changing priorities
One of the major features of this mod is letting you change the priority of different actions and things the bots can do.  This can be very useful, but it can also cause some unexpected and unintuitive behavior.  I generally recommend making minimal changes to these settings.  If you're having problems with the bots not doing something, or acting strangely, try setting all priorities back to their default values.

## Current features: 
 - Directly tell a bot to pick up a specific item with smart select.
 - Directly tell a bot to use their item on a specific player with smart select
 - Allow bots to do brand new actions like open lockers/doors, insert cells, and more!
 - Control if and how closely bots follow you.
 - Control if and when bots are allowed to pick up items
   - You can choose what items they do or don't pick up, or even change the priority of different items.
 - Control if and when bots are allowed to share items with others.
   - You can choose exactly what threshold bots are allowed to share their resources.  Only want ammo when you're below 50%?  No problem.
 - Control if bots are allowed to attack
   - Control what they are allowed to attack with, do you want them to save ammo and go melee only?  No problem! (this feature works, but seems to have issues not caused by this mod.)
 - Control if bots are allowed to revive players or bots.
 - Control what things bots are allowed to ping.
 - Control if bots are allowed to smash locks
 
## How to use Smart select!
 - Depending on what you're looking at and context, you can tell the bots to do one of 4 different actions at any given time.  Tap V while looking at a bot to select them, then you can see what they can do at any time on the bottom of your screen.  You can command them by (from left to right) Tapping V, Holding V, Double tapping V, and Tapping then holding V.  There are about 2 dozen different things you can tell them to do!  With more coming eventually.  Below is the full chart of what they can do and how to tell them to do it:
            (      TAP     /     HOLD      /   DOUBLE TAP  /  TAP & HOLD   ) 
            ( ------------------------------------------------------------ ) 
 Player/Bot ( ---Select--- / ----Share---- / Follow/Cancel / ---Send To--- )
       Item ( ------------ / ---Pickup---- / ------------- / ------------- )
  Equipment ( ---Pickup--- / ---Refill---- / -Pickup All-- / ------------- )
  Container ( ----Open---- / ------------- / --*Place?*--- / ------------- )
 Floor/Wall ( ------------ / -Consumable-- / --Equipment-- / ----Move----- )
    Holding ( ------------ / -*Drop Here*- / --Drop Now--- / ------------- )
       Door ( -Open/Close- / -Throw cFoam- / --*Break?*--- / ------------- )
       Lock ( ---Unlock--- / -Lock Melter- / ------------- / ------------- )
 Enemy/Quiet( ------------ / Sneak Attack- / -*All Sync*-- / ---*Sync*---- )
 Enemy/Loud ( --*Target*-- / ------------- / ------------- / *All Target*- )
  Generator ( ------------ / -Place Cell-- / ------------- / ------------- )
    Look Up ( Cancel Last- / --Deselect--- / -Cancel All-- / -*Select A*-- ) 
  Look Down ( ---Follow--- / -Share Self-- / ------------- / --A Follow--- ) 
 - Items surrounded by * mean that it's not in the current version, but coming eventually.
 - When you tell a bot to move to a location, they will no longer follow you.  You must double tap them to tell them to follow you again, they will never come back untill you do. I plan to make some sort of option to have they return to you if you go some distnace away, or if they get attacked.  That will come eventually.

### Important note about how Smart Select works under the hood.
 - This system may seem to be inconsistant or not pick up on things you think you're looking at, here's why:
 - The system checks a sphere arround the point you're looking at, NOT a cone infront of you.  This allows the system to run faster and perform better.
 - The system only updates about 10 times per second. 
 - Keep this in mind if you feel like selection is inconsistant.
 
## Planned features:
 - Better in game explanation of how to use the menu and what it can do.
 - Integration with [TheArchive Core](https://thunderstore.io/c/gtfo/p/AuriRex/TheArchive_Core/) for a settings menu with settings like:
   - Key rebinding
   - Default settings
   - Favorite settings
   - binding specific toggles to a hotkey
 - Show what the bot is currently doing above their head.
 - Allow you to have per bot permissions, so Dauda is allowed to use ammo but Hacket is not for example.
 - Optionally replace the Q menu entirely with my menu with all of it's features.
 
## Known issues:
 - Changing attack means mid combat can cause some jank.  TODO look into what's actually going on.
 - Bots will have trouble reaching some containers, FIXED?  Let me know if you still find a spot where this happens.

## Features I'd like to add:
 - Option to dynamically ignore specific lockers, or lockers in a specific room, or prioritize/deprioritize lockers near a point.
 - Custom bot actions like "explore" or "find ammo" or even "use terminal to ping for items"

Here's an unscripted preview video:

https://www.youtube.com/watch?v=IsuM1OC3DAQ

Here's two (old) videos of the mod in action.  

https://www.youtube.com/watch?v=X5RWMQyUgTY

https://www.youtube.com/watch?v=lrDWroqC-R0

There is A LOT of unused code and extra stuff in this mod.  I got a little bit too ambitious with some features. I may or may not return to some of them later.


### Changelog
V1.2.0 - The Custom Actions update!
 - Added completely modded actions the bots can do, things like opening doors, or inserting cells.
 - This lays the groundwork for huge potential in the future!
 - Currently limited to smart select only.  Will look into making them trigger automatically eventually.

V1.1.0 - The Smart Select Update.
 - Completely overhauled the smart select system!  
 - Added (possibly too many) options to control when the bots talk in chat.

V1.0.3
 - Stopped bots from repeatedly spamming chat with failed actions.
 - Added submenu for 'bots talking settings', letting you disable individual things they say.
 - Added drop permissions to the pickup submenu. This will let you tell bots to only replace their resource packs once they are gone.
 - Fixed a compatibility bug with Better Bots where bots would't revive you after you go down.
 - Fixed minor typo.

V1.0.2
 - Fixed disinfect pickup threshold being inverted.
 - Bots no longer use auto disinfect pacs in fog.  
 - Added a (WIP) option to stop bots from dropping their items in the pickup submenu.

V1.0.1
 - Updated readme.
