# Controlled Buildings

Adjusts building footprints and placement rules to fix visual conflicts with decorative elements like ceiling trim.

## Features

- **Juicer Footprint Fix** - Reduces the Juicer's placement footprint from 3x4 to 3x3, so it fits in 4-high rooms with ceiling trim along the top row. The building retains its full visual appearance.

- **Transit Tubes & Backwall** - Transit tubes always draw in front of drywall/backwall, and drywall can be built in the same cell as tubes. Tube segments can be built in or behind tiles. Tighter bends and connection rules are allowed.

## How to Use

1. **Install the mod** - Subscribe on Steam Workshop or install manually
2. **Play normally** - The Juicer will now fit in rooms with ceiling trim without removing 3 tiles of trim

### How It Works

The Juicer's visual animation is 4 cells tall, but the actual collision/placement footprint is reduced to 3 cells high. The top row of the visual extends into the cell above, but that cell is free for ceiling trim or other decorative buildings.

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3670726296) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledBuildings\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact**
- **One-time patch** - Runs once during game load, no per-frame overhead

## Future Updates

- Additional building footprint adjustments based on community feedback
- Configurable options for which buildings to adjust

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

Please mention "Controlled Buildings" in your issue title or description.

## My Workshop & Collections

- [My Workshop](https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140) – All my ONI mods on Steam
- [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) – 235+ tested, compatible mods for Oxygen Not Included
- [The Controlled Series](https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653) – Collection of Controlled mods

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.2**: Fixed atmosphere gas appearing behind drywall
- **1.0.1**: Fixed Arbor Tree layering
- **1.0.0**: Initial release

