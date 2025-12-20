# Debug Fog of War Mod

A mod for Oxygen Not Included that prevents the fog of war from being removed when debug mode is activated.

## Features

- **Preserves Fog of War**: When debug mode is activated, the fog of war remains intact
- **Full Debug Functionality**: All debug mode features still work (free camera, debug tools, element painting, etc.) - only map discovery is prevented

## How It Works

When you press the debug toggle key (Backspace by default), the mod intercepts the debug mode activation and handles the UI toggling without triggering the map discovery code. This means:

- ✅ Debug tools and UI still work normally
- ✅ Free camera mode still works
- ✅ Element painting and other debug features still function
- ❌ Map discovery is prevented (fog of war stays intact)

## Why Use This Mod?

When debug mode is activated in the base game, it automatically reveals the entire map by discovering all cells. This causes:

- **Performance Issues**: All cells on the map need to be simulated, even if you haven't explored them, which can significantly impact game performance
- **Unwanted Discovery**: You may not want to see the entire map when using debug tools

This mod allows you to use debug mode for its intended purpose (testing, building, element painting) without the side effect of revealing the entire map, saving you from the performance impact of simulating all discovered cells.

## Compatibility

- **Supported Content**: All DLCs
- **Minimum Build**: 700386
- **API Version**: 2
- Uses PLib and Harmony

## Inspiration

This mod was inspired by [Cairath's DebugDoesNotDiscoverMap mod](https://github.com/Cairath/ONI-Mods/tree/master/src/DebugDoesNotDiscoverMap), which unfortunately no longer functions and is no longer being updated. This is a reimplementation that works with current game versions.

Special thanks to [Cairath](https://github.com/Cairath/ONI-Mods) for the original concept and approach.

## Support & Issues

Found a bug or have a feature request? Please report it on GitHub:

- **Issues**: [Report an Issue](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues)
- **Bug Reports**: [Create a Bug Report](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new)
- **Feature Requests**: [Suggest a Feature](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new)

Please mention "Debug Fog of War" in your issue title or description so we can identify it easily.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Developed for Oxygen Not Included
- Uses PLib by Peter Han
- Uses Harmony by pardeike
- Inspired by Cairath's DebugDoesNotDiscoverMap mod

## Version History

- **0.1.15**: Current version
  - Initial release
  - Prevents fog of war removal when debug mode is activated
  - Maintains all debug functionality except map discovery

