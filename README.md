### Preface:
This started as continuation of [Zombified Initiative](https://thunderstore.io/c/gtfo/p/hirnukuono/Zombified_Initiative/) by [hirnukuono](https://thunderstore.io/c/gtfo/p/hirnukuono/) however at this point there is very little code from that mod left.

### Press X to open and navigate the menu.
### Press V to use smart select.

# Intro:
Have you ever been frustrated that a bot just used all of your disinfect as soon as you get out of the fog, even though you're going to have to go right back?!  That's the catalyst for me making this mod.  The goal of this mod is to give you the ability to have more control over the bots in GTFO, but not to give them any direct buffs or new abilities.  I want to allow you to tell them exactly what you do, and don't want them to be able to do.  When this mod is done, I want you to be able to ask a bot to do just about anything you could ask a human to do.

# How to use:
Press X to open the menu, and then press X again on a node like "automatic actions" to choose what the bots are allowed to do.  Single tap on an action to toggle if it's allowed.  Double tap a node to open it's sub menu.  Tap the center node to go back to the previous menu.  Press and hold to reset to default settings.  Sub menus have things like resource share thresholds, or what bots are and aren't allowed to pick up.  Some menus allow you to use the scroll wheel to change additional settings.  Things like the priority of the action, or the min/max follow distance.  You can also use the scroll wheel on the center menu node to change the category of the current menu, to show/hide some icons.

There is also smart select!  In the bottom of your screen you will see 4 potential actions you could tell a bot to do (see table below) By default the closest bot to the action will do it, or you can tap on a bot to select it, and then that bot will be the one doing the action.  Look up and hold V to deselect.  You can tell them to do things like, stand in a specific spot, hack a lock, pick up a specific item, deploy their turret, and many more!  See How To Use Smart Select for the full list of things they can do.

## Important note about changing priorities
One of the major features of this mod is letting you change the priority of different actions and item pickups.  This can be very useful, but it can also cause some unexpected and unintuitive behavior.  I generally recommend making minimal changes to these settings.  If you're having problems with the bots not doing something, or acting strangely, try setting all priorities back to their default values.

## Current features: 
 - Directly tell a bot to pick up a specific item with smart select.
 - Directly tell a bot to use their item on a specific player with smart select
 - Allow bots to do brand new actions like open lockers/doors, insert cells, and more!
 - Control if and how closely bots follow you.
 - Control if and when and where bots are allowed to pick up items
   - You can choose what items they do or don't pick up, or even change the priority of different items.
   - You can change what zones and areas bots are allowed to automatically pick up items in.
 - Control if and when bots are allowed to share items with others.
   - You can choose exactly what threshold bots are allowed to share their resources.  Only want ammo when you're below 50%?  No problem.
 - Control if bots are allowed to attack.
   - Control what they are allowed to attack with, do you want them to save ammo and go melee only?  No problem!
 - Control if bots are allowed to revive players or bots.
 - Control what things bots are allowed to ping.
 - Control if bots are allowed to smash locks and use lock melters
 - All disabled actions can still be done if you manually tell a bot to do that with smart select!
 

 
## Planned features:
 - Better in game explanation of how to use the menu and what it can do.
 - Integration with [TheArchive Core](https://thunderstore.io/c/gtfo/p/AuriRex/TheArchive_Core/) for a settings menu with settings like:
   - Key rebinding
   - Default settings
   - Favorite settings
   - Binding specific toggles to a hotkey
 - Show what the bot is currently doing above their head.
 - Allow you to have per bot permissions, so Dauda is allowed to use ammo but Hackett is not for example.
 - Optionally replace the Q menu entirely with my menu with all of it's features.
 - Allow bots to use terminals via chat commands.
 - Allow bots to remember where items are, and you could say "I need ammo" and they would go looking for ammo, or directly to the ammo if they remember where it is.
 
## Known issues:
 - Bots will have trouble reaching some containers, FIXED?  Let me know if you still find a spot where this happens.
 - Bots leaving joining mid game is untested and may break things.  This will be supported "soon"
 - Checkpoints are untested and may have unexpected results.  But should be fine. This will be supported "soon"

Here's an unscripted preview video:

https://www.youtube.com/watch?v=IsuM1OC3DAQ

Here's two (old) videos of the mod in action.  

https://www.youtube.com/watch?v=X5RWMQyUgTY

https://www.youtube.com/watch?v=lrDWroqC-R0

There is A LOT of unused code and extra stuff in this mod.  I got a little bit too ambitious with some features. I may or may not return to some of them later.

## Feature details:

### Smart select!
 - Depending on what you're looking at and context, you can tell the bots to do one of 4 different actions at any given time.  Tap V while looking at a bot to select them, then you can see what they can do at any time on the bottom of your screen.  You can command them by (from left to right) Tapping V, Holding V, Double tapping V, and Tapping then holding V.  There are about 2 dozen different things you can tell them to do!  With more coming eventually.  Below is the full chart of what they can do and how to tell them to do it:
```
            (      TAP     /     HOLD      /   DOUBLE TAP  /  TAP & HOLD   ) 
            ( ------------------------------------------------------------ ) 
 Player/Bot ( ---Select--- / ----Share---- / -Follow/Stop- / ---Send To--- )
       Item ( ------------ / ---Pickup---- / ------------- / ------------- )
  Equipment ( ---Pickup--- / ---Refill---- / -Pickup All-- / ------------- )
  Container ( ----Open---- / ------------- / --*Place?*--- / ------------- )
 Floor/Wall ( ------------ / -Consumable-- / --Equipment-- / ----Move----- )
    Holding ( ------------ / --Drop Here-- / --Drop Now--- / ------------- )
       Door ( -Open/Close- / -Throw cFoam- / --*Break?*--- / ------------- )
       Lock ( ---Unlock--- / -Lock Melter- / ------------- / ------------- )
 Enemy/Quiet( ------------ / Sneak Attack- / --All Sync--- / ----Sync----- )
 Enemy/Loud ( ------------ / ---Target---- / ------------- / *All Target*- )
  Generator ( ------------ / -Place Cell-- / ------------- / ------------- )
    Look Up ( Cancel Last- / --Deselect--- / -Cancel All-- / -*Select A*-- ) 
  Look Down ( ---Follow--- / -Share Self-- / ------------- / -All Follow-- ) 
```
 - Items surrounded by * mean that it's not in the current version, but coming eventually.
 - When you tell a bot to move to a location, they will no longer follow you.  You must double tap them to tell them to follow you again, they will never come back until you do. I plan to make some sort of option to have they return to you if you go some distance away, or if they get attacked.  That will come eventually.
 - When you tell a bot to follow you, they will stop all other actions, try this if a bot gets stuck for some reason.

#### Important note about how Smart Select works under the hood.
 - This system may seem to be inconsistent or not pick up on things you think you're looking at, here's why:
 - The system checks a sphere around the point you're looking at, NOT a cone infront of you.  This allows the system to run faster and perform better.
 - The system only updates about 10 times per second. 
 - Keep this in mind if you feel like selection is inconsistent.

### Sync attack
 - Tap then hold on an enemy and a bot will walk up to that enemy without attacking.  As soon as any other enemy takes damage from any source, the bot will attack!  You can use this to make sure bots attack at the same time.

### Explore action
 - When the team doesn't know about any alive enemies, bots will leave the follow radius and explore the area!  They will do this untill everything reachable has been explored, or an alive enemy has been found.  Then they will return to their leader.
 - Dissabled by default to match vanilla behavior.  You can enable it in the automatic actions menu.

### Stop command
 - This command resets the bots brain, stopping all current actions, automatic and manual.
 - If the bots are doing something weird, try this.

## AI Usage in this project:
 - This project is **not** vibe coded, but it would not have been possible without AI.  When I do use AI 90% of the time it's to understand GTFO's code, I ask it things like "How does this mechanic work" And then write the code myself.  Occasionally I will ask it to write a small method, or at most a single class.  When I do, I always read the code and make sure I understand what it's doing.  I do (or did at the time it was written) understand how 99% of the code in this project works.