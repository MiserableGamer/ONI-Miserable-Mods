# Empty Storage

Adds an "Empty Storage" button to storage buildings, allowing you to quickly drop all contents either immediately or via duplicant labor.

## Features

- **Empty Storage Button** - Adds a convenient button to all storage buildings (gas, liquid, and solid)
- **Immediate Emptying** - Drop all contents instantly with a single click
- **Duplicant Labor Option** - Create a task for duplicants to empty the storage over time
- **Skill Requirements** - Optionally require duplicants to have appropriate skills
- **Configurable Work Time** - Adjustable work time per 100kg (0.1 to 10 seconds)
- **Bionic Dupe Support** - Bionic dupes with Tidying Booster can empty any storage type

## How to Use

1. **Select Storage Building** - Click on any gas, liquid, or solid storage building
2. **Click Empty Storage** - Find the "Empty Storage" button in the building's info panel
3. **Contents Drop** - If immediate mode is enabled, contents drop instantly
4. **Cancel if Needed** - Click the button again to cancel an in-progress emptying task

### Tips

- Immediate emptying is fastest but doesn't require duplicant labor
- Skill requirements can be disabled in options if you want any dupe to empty storage
- Bionic dupes with Tidying Booster bypass skill requirements
- Work time scales with the mass stored - larger containers take longer

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\EmptyStorage\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact**
- **On-Demand Only** - Button only triggers when clicked
- **Lightweight Tasks** - Emptying tasks use standard game work system
- **No Background Processing** - No continuous monitoring or polling

## Future Updates

- Universal Cancel Tool support for canceling emptying tasks
- Additional configuration options

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

Please mention "Empty Storage" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.2.120**: Fixed bug affecting game priority system (bionic lubricant refill and other chores)
- **1.0.1.91**: Initial release
- **1.0.1.83**: Code cleaning
