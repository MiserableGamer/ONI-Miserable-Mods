# Controlled Sublimation

Complete control over element sublimation and off-gassing, with separate settings for debris and storage.

## Features

- **Per-Element Control** - Configure each element independently
- **Debris vs Storage** - Separate toggles for debris on ground vs items in containers
- **Rate Multiplier** - Adjust sublimation speed from 0% to 1000%
- **Oxysconce Compatible** - Bleach Stone Sconce always works regardless of settings
- **DLC Safe** - Works with or without DLC installed

## Supported Elements

| Element | Emits | Notes |
|---------|-------|-------|
| Bleach Stone | Chlorine | Base game |
| Oxylite | Oxygen | Base game |
| Polluted Dirt | Polluted Oxygen | Base game |
| Slime | Polluted Oxygen | Base game |
| Polluted Water | Polluted Oxygen | Base game (bottles) |
| Polluted Mud | Polluted Oxygen | Spaced Out DLC |

## How to Use

1. **Open Mod Options** - Main Menu → Mods → Controlled Sublimation → Options (gear icon)
2. **Configure Elements** - For each element, set:
   - **As Debris** - Sublimate when lying on ground
   - **In Storage** - Sublimate when in containers
   - **Rate Multiplier** - Speed adjustment (0.0 - 10.0)
3. **Restart Game** - Changes require a game restart to take effect

### Tips

- Set rate to 0.0 to completely disable sublimation for an element
- Oxysconce (Bleach Stone Sconce) always emits chlorine regardless of settings
- DLC elements are safely ignored if the DLC is not installed
- Use storage settings to prevent off-gassing in bins while allowing debris to emit

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledSublimation\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Compatible with most mods, no known conflicts

## Performance

**Minimal Performance Impact**
- **Configuration Only** - Settings applied at game start
- **Lightweight Patches** - Only modifies sublimation rate calculations
- **No Polling** - Event-driven, not continuous monitoring

## Future Updates

- Additional element support as requested
- Per-save configuration options
- More granular rate control

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

Please mention "Controlled Sublimation" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.1.1**: Initial release with per-element debris/storage control
