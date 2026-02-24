# Controlled Visuals

Fixes a long-standing vanilla bug where items on conveyor rails sometimes render in front of drywall and other buildings. Conveyor contents now always draw behind walls as intended.

## Features

- **Conveyor Items Behind Drywall** - Items on rails consistently render behind drywall, tiles, and other buildings
- **No Gameplay Impact** - Only fixes rendering order; conveyor mechanics are unchanged
- **Works Everywhere** - Loaders, rails, and bridges; moving and stationary items

## How to Use

1. **Enable the Mod** - Enable Controlled Visuals in your mod list
2. **No Configuration** - The fix is always active; conveyor items will render correctly behind walls

### Tips

- No configuration needed—the fix is always active
- Works with all solid conveyor buildings (loaders, bridges, rails)

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledVisuals\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact**
- **Per-Tick Layer Check** - Only corrects render layer for items that need it
- **No Gameplay Logic** - Purely a visual fix; no simulation changes

## Future Updates

- Additional visual fixes may be added in future updates

## Support & Issues

Need help, found a bug, or have a suggestion? We're here to help!

### Community

- **Discord**: [Join our Discord server](https://discord.com/channels/1452947938304200861/1452947939927392398) for discussions, questions, and community support
- **GitHub Discussions**: [Discuss on GitHub](https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions) - share ideas, ask questions, or get help with modding

### Reporting Issues

Found a bug or have a feature request? Please report it on GitHub using our issue templates:

- **Bug Reports**: [Report a Bug](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml) - Use this for crashes, errors, or unexpected behavior
- **Feature Requests**: [Suggest a Feature](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml) - Have an idea for a new feature or improvement?
- **Questions**: [Ask a Question](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml) - Need help understanding how something works?
- **Other Issues**: [Other Issue](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml) - Something else that doesn't fit the above categories

Please mention "ControlledVisuals" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 235 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.0**: Conveyor items behind drywall fix
