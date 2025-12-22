[h1]Debug Fog of War[/h1]

Prevents fog of war from being removed when debug mode is activated.

[h2]Features[/h2]

[list]
[*][b]Preserves Fog of War[/b] - When debug mode is activated, the fog of war remains intact
[*][b]Full Debug Functionality[/b] - All debug mode features still work (free camera, debug tools, element painting, etc.) - only map discovery is prevented
[/list]

[h2]How It Works[/h2]

When you press the debug toggle key (Backspace by default), the mod intercepts the debug mode activation and handles the UI toggling without triggering the map discovery code. This means:

[list]
[*]Debug tools and UI still work normally
[*]Free camera mode still works
[*]Element painting and other debug features still function
[*]Map discovery is prevented (fog of war stays intact)
[/list]

[h2]Why Use This Mod?[/h2]

When debug mode is activated in the base game, it automatically reveals the entire map by discovering all cells. This causes:

[list]
[*][b]Performance Issues[/b]: All cells on the map need to be simulated, even if you haven't explored them, which can significantly impact game performance
[*][b]Unwanted Discovery[/b]: You may not want to see the entire map when using debug tools
[/list]

This mod allows you to use debug mode for its intended purpose (testing, building, element painting) without the side effect of revealing the entire map, saving you from the performance impact of simulating all discovered cells.

[h2]Inspiration[/h2]

This mod was inspired by [url=https://github.com/Cairath/ONI-Mods/tree/master/src/DebugDoesNotDiscoverMap]Cairath's DebugDoesNotDiscoverMap mod[/url], which unfortunately no longer functions and is no longer being updated. This is a reimplementation that works with current game versions.

Special thanks to [url=https://github.com/Cairath/ONI-Mods]Cairath[/url] for the original concept and approach.

[h2]Compatibility[/h2]

[list]
[*]All DLCs supported
[*]Minimum Build: 700386
[*]API Version: 2
[/list]

[h2]Support & Issues[/h2]

Found a bug or have a feature request? Please report it on GitHub:

[url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues]Issues[/url]

Please mention "Debug Fog of War" in your issue title or description.

[h2]Mod Collection[/h2]

This mod is part of the [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 200+ Ultimate Mods collection[/url] on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

[h2]Credits[/h2]

[list]
[*]Developed for Oxygen Not Included
[*]Uses PLib by Peter Han
[*]Uses Harmony by pardeike
[*]Inspired by Cairath's DebugDoesNotDiscoverMap mod
[/list]

[h2]Version History[/h2]

[list]
[*][b]0.1.15[/b]: Current version
[list]
[*]Initial release
[*]Prevents fog of war removal when debug mode is activated
[*]Maintains all debug functionality except map discovery
[/list]
[/list]
