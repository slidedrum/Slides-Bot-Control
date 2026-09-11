 ## v1.3 - The Automatic Actions update!
 ### This update's headline feature is two new automatic actions, Open and Explore.  Along with more manual actions, the most notable being the Sync attack! And a TON of backend fixes.
 ### Please give me some feedback on how balanced the new sync/stealth attack is.  When bots do *any* manual action, they follow the exact same detection rules as players now, so they can and will be detected sometimes.  When not doing manual actions, bots still can't be detected, so they will only mess you up if you tell them to!
 
 - When doing manual actions, Bots can now be detected just like players.  When doing automatic actions, bots still can not be detected no matter what.
 - You can now command unselected bots with smart select.  Commands will fall back to the nearest bot.
 - Added option to restrict what guns bots attack with, via a new sub menu in bullet attack menu.
 - Added Sync attack action to smart select.  Bots will charge a melee attack, and strike when any enemy takes damage.
 - Added new automatic action, open locker.
 - Added new automatic action, explore.  When enabled, bots will explore the current area outside of their follow range.
 - Added new Zone override option in pickup permissions, letting you restrict pickups by zone/area.
 - Stationary sleepers are now nav mesh carvers.  This was done so that bots don't walk over and wake up enemies durring manual actions. This may have some unexpected consiquences with other mods.
 - Changed how move action works, they will now return to that spot if they have to move.
 - Follow now cancels all actions, if they somehow get stuck or have problems, try telling the bot to follow you.  This should reset their brain.
 - Fixed some miscellaneous bugs with attack action restrictions.
 - Fixed menu node backgrounds rendering 1 frame late.
 - Fixed SO MANY edge cases where bots would misbehave, or act unintuitively.
 - Removed excessive debug logging.
 - Moved changelog into it's own file. (This file!)

## V1.2.6
 - Fixed and re-added the betterbots compatibility layer.  Bots should behave when changing attack means now!

## V1.2.5
 - Temporarily removed misbehaving better bots compatibility patch that was causing bots not to attack.

## V1.2.4
 - Fixed unintentional better bots dependency. 

## V1.2.3
 - Fixed readme chart

## V1.2.2
 - Made it so you can no longer send bots to attack anything other than standard enemies. (other enemy types coming eventually)

## V1.2.0 - The Custom Actions update!
### This update's headline feature is the brand new actions the bots could never do before, and the groundwork to easly add more in the future!

 - Added completely modded actions the bots can do, things like opening doors, or inserting cells.
 - This lays the groundwork for huge potential in the future!
 - Currently limited to smart select only.  Will look into making them trigger automatically eventually.
 - Updated what the bots say in chat to be more specific.

## V1.1.0 - The Smart Select Update.
### This update's headline feature is the overhaul of the smart select system.  New ways to interact and command the bots.
 
 - Completely overhauled the smart select system!  
 - Smart select now has 4 different ways to activate it, tap/hold/double tap/tap then hold.
 - Depending on context, they will all do something different.
 - Added (possibly too many) options to control when the bots talk in chat.

## V1.0.3
 - Stopped bots from repeatedly spamming chat with failed actions.
 - Added submenu for 'bots talking settings', letting you disable individual things they say.
 - Added drop permissions to the pickup submenu. This will let you tell bots to only replace their resource packs once they are gone.
 - Fixed a compatibility bug with Better Bots where bots wouldn't revive you after you go down.
 - Fixed minor typo.

## V1.0.2
 - Fixed disinfect pickup threshold being inverted.
 - Bots no longer use auto disinfect packs in fog.  
 - Added a (WIP) option to stop bots from dropping their items in the pickup submenu.

## V1.0.1
 - Updated readme.
