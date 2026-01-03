# Longer Arms

Extends duplicant reach distance, allowing them to reach over chasms and other areas that would normally be out of reach.

## Features

- **Configurable Vertical Reach** - Set how many additional cells duplicants can reach upward and downward
- **Configurable Horizontal Reach** - Set how many additional cells duplicants can reach left and right
- **Reach Over Chasms** - Allows duplicants to reach across gaps they cannot cross
- **Safe Mode** - Prevents reaching through solid walls and obstacles

## How to Use

1. **Open Mod Options** - Main Menu → Mods → Longer Arms → Options (gear icon)
2. **Configure Vertical Reach** - Set additional cells for up/down reach (0-10)
3. **Configure Horizontal Reach** - Set additional cells for left/right reach (0-10)
4. **Enable Safe Mode** - Recommended to prevent reaching through walls
5. **Restart Game** - Changes require a game restart to take effect

### Tips

- Vanilla allows reaching 3 cells up from floor level (cells 1-3)
- Setting vertical reach to 1 allows reaching cell 4
- Safe Mode caps horizontal reach at 2 to prevent wall clipping issues
- Disable Safe Mode only if you understand the consequences

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\LongerArms\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact**
- **One-Time Modification** - Reach values applied once when game loads
- **No Runtime Overhead** - No continuous calculations or monitoring
- **Efficient Patches** - Only modifies reach calculation when needed

## Future Updates

- Diagonal reach for corner cells
- Linked reach sliders option
- Per-duplicant reach customization

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

Please mention "Longer Arms" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.3.76**: Fix ladder bug, added safe mode
- **1.0.2.64**: Diagonal bug fix
- **1.0.1.56**: Code cleaning
- **1.0.1.0**: Initial release
