Controls:
-Use the mouse to select/direct units.

How to play:
-Units must move the number of spaces equal to the tile value of their starting position.
-When a unit enters a tile, the number of spaces that the unit has moved thus far is added to the tile's value.
	-e.g. If a unit on a tile value of 3 moves and ends its path on a tile of 4, the former becomes a 7.
-Tiles are captured by a player if they cause a tile's value to become a multiple of 10.
	-Doing so resets the tile's value to 1.
-Units cannot enter tiles captured by another player.
-Units cannot enter a tile occupied by another player.
-If a player has no available tiles for their units to move to, they are eliminated.
-The final player standing wins.

Known issues:
-Sometimes the game announces the losing player as the winner.
-High values movement values can take long time to process pathfinding for.
	-The game might freeze if this happens.
