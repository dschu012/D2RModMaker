# This project is a WIP

Reimagination of https://github.com/tlentz/d2modmaker with a pluginable interface.

TODOs:

* item generator
* no drop
* use oskills
* remove lvl requirements
* remove attr requirements
* stack gems?
* safe unsocket recipe
* token recipe
* melee splash dmg

## Plugins

Ideally writing your own plugin you should be able to do anything possible w/ txt/json editing. Some random sample plugins are listed below:

### D2RModMaker.QualityOfLife

Adds general quality of life enhancements to D2R.
* expand inventories (stash/inv/cube)
* allow mercs to equip additional items
* see item levels
* start w/ cube
* use skills in town
* skip launch screen intro videos
* change size of stacks (tp/id/key/quivers)

### D2RModMaker.Difficulty

Lets you scale number of mobs, hp of mobs, damage of mobs, and xp gained.

### D2RModMaker.Speed

Lets you scale player, monster, missiles speeds

### D2RModMaker.Corruption

Adds a new item "Scroll of Corruption" that lets you corrupt items (like POE). The UI lets you modify the different tiers of stats corruption can and the chances of getting that tier when corrupting an item. Right now the recipe for "Scroll of Corruption" is `set or unq + any potion` in the cube.