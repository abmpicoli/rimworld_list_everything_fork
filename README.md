# RimWorld-ListEverything: Improved.

Search, order and potentially act on all things inside Rimworld Colony/Encampment maps. Version 3.0.

# Description:

List all sort of "things" on the map with various filters.

**3.0 Important change: the lists are no longer sorted in any way, unless you explicitly add a order clause: Look for "Order and limits" in this text. This means
the original natural ordering by distance to Search Spot no longer work 'out of the box' since it is now a query option.**

The following filters are available:

* **Search by name**: show all items that has the provided text somewhere in its name.
* **Specific Thing**: search for a specific item type, like Wood, steel, compacted steel, etc.
* **Category**: search by a specific generic category : Person, Animal, Item, Natural, Plant, Other
* **Personal details**: some filters regarding colonist/animals info, such as thoughts, missing body parts, etc.
* **Animal details**: information regarding animals covering hunting and handling: milk fullness, egg hatching, meat amount, etc.
* **Buildings** : Hm... open close doors, mineable stuff?? Well, they are there...
* **Plants and food**: albucc's personal favorite. Find full grown plants that can be harvested. Find food about to spoil.
  Specially good for Naked Brutality/ Rich Explorer Scenarios.
* **location**: filter things at areas / zones / and "things with other things nearby".
* **Health**: general health % of things, objects or not.
* **Inventory**: find things that are holding other things, or things this thing is holding.
* **Designated**: find things that are designated to be acted upon.
* **faction**: the faction the thing belongs to.
* **from a mod**: things belonging to a specific mod.
* **currently on screen**: things visible on screen.
* **filter group**: allows to perform union / intersection between multiple search criteria.
* **Order an limits**: *(new to 3.0)*: a series of criterias that tell to reorder the outcomes according to some criteria.
  The available options are:
  * **Order by distance from Search Spots**: a new item in the Architect/ Miscelaneous tab, the "Search Spot" ![Search Spot](Textures/find_center_small.png) can be placed 
    on the map. You can place as many of those as you want, at multiple maps. This filter will order the findings around that search spot. 
  * **Order by distance to map edge(center first)**: itens more distant to the map edges come first.
  * **Order by item name**: the items are sorted alphabetically by name.
  * **Limit results**: allow to only pick a determined amount of results.
  * **Reverse order**: invert the ordering of the findings. 
  
## Odd runtime behavior:

* As part of a future support for scripting, and for debug diagnostics, a special folder named this package id (uuugggg.albucc.ListEverything) will be created into your config folder.
  this will issue a small message in the logs showing that you can enable debug mode for this mod if you want, by creating a file, which will make very verbose file-base logging for this mod.
  
  
## Usage hints:

* The filters are always executed in the order they are listed. Make simple searches first, to narrow results, and only then use the more complex filters to gather information.

# Links and authoring:

This is a permanent fork of the mod https://github.com/alextd/RimWorld-ListEverything created by Uuuggggh.

This mod on Steam: https://steamcommunity.com/sharedfiles/filedetails/?id=2896175723.

Uuuggggh have abandoned this mod in favor of a full refactor into 4 new mods. You may check his actual work at https://steamcommunity.com/sharedfiles/filedetails/?id=2895300634 

# Compilation hints:

1) Follow the typical configuration for mods, as discussed in the wiki 
https://rimworldwiki.com/wiki/Modding_Tutorials/Setting_up_a_solution 

The most important of all is the fact that *if you don't place the repository inside the rimworld Mods directory, references to required libraries will break*.

The shellscript ./release.sh allows to build a "clean release" at another mod folder. It can be executed with Git Bash or a typical shellscript terminal. Personally, I use Ubuntu at my windows machine (WSL).

# Contributing:

* Please be free to check the code and contribute. Also be welcome to fork this code and create your own changes (just as I did).

# How to get help: 

I don't see a problem if you post questions in the Comments section.

If for some reason this is not possible for you, consider opening an issue on github https://github.com/abmpicoli/rimworld_list_everything_fork/issues. 
