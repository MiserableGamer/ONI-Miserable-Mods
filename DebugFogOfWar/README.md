# Debug Fog of War

Prevents the fog of war from being removed when debug mode is activated, preserving map exploration and improving performance.

## Features

- **Preserves Fog of War** - Map discovery stays intact when debug mode is activated
- **Full Debug Functionality** - All debug tools still work (free camera, debug tools, element painting, etc.)
- **Performance Preservation** - Prevents unnecessary cell simulation from automatic map reveal

## How to Use

1. **Install the Mod** - Enable in the Mods menu
2. **Activate Debug Mode** - Press Backspace (default) to toggle debug mode
3. **Use Debug Tools** - All debug features work normally
4. **Map Stays Hidden** - Unexplored areas remain covered by fog of war

### Tips

- Debug mode still provides access to all tools and features
- Great for testing without spoiling map exploration
- Significantly improves performance compared to revealed maps
- Works with free camera mode - you can see unexplored areas but they won't be "discovered"

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\DebugFogOfWar\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Compatible with most mods

## Performance

**Improves Performance**
- **Prevents Cell Discovery** - Undiscovered cells don't need full simulation
- **Lightweight Patch** - Only intercepts debug toggle, no runtime overhead
- **No Continuous Monitoring** - Patch only triggers when debug mode is toggled

## Future Updates

- Option to toggle map discovery behavior
- Keyboard shortcut to manually trigger map discovery if desired

## Support & Issues

Need help, found a bug, or have a suggestion? We're here to help!

### Community

- **💬 Discord**: [Join our Discord server](https://discord.com/channels/1452947938304200861/1452947939927392398) for discussions, questions, and community support
- **📝 GitHub Discussions**: [Discuss on GitHub](https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions) - share ideas, ask questions, or get help with modding

### Reporting Issues

Found a bug or have a feature request? Please report it on GitHub using our issue templates:

- **🐛 Bug Reports**: [Report a Bug](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml) - Use this for crashes, errors, or unexpected behavior
- **💡 Feature Requests**: [Suggest a Feature](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml) - Have an idea for a new feature or improvement?
- **❓ Questions**: [Ask a Question](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml) - Need help understanding how something works?
- **📝 Other Issues**: [Other Issue](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml) - Something else that doesn't fit the above categories

Please mention "Debug Fog of War" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching
- Inspired by [Cairath's DebugDoesNotDiscoverMap mod](https://github.com/Cairath/ONI-Mods/tree/master/src/DebugDoesNotDiscoverMap)

## Version History

- **1.0.1.19**: Initial release
- **1.0.1.17**: Code cleaning
