# Morning Exercise

Adds a Morning Exercise schedule block and Manual Exerciser building, allowing Duplicants to work out and gain Athletics or Morale bonuses.

## Features

- **Morning Exercise Schedule Block** - New schedule block type for exercise time
- **Manual Exerciser Building** - Exercise-only building in the Medicine category
- **Warm Up Buff** - Regular Duplicants gain Athletics bonus after exercising
- **Bionic Warm Up Buff** - Bionic Duplicants gain Morale bonus instead
- **Experience Gain** - Optional permanent Athletics attribute experience
- **Separate Dupe/Boop Settings** - Independent configuration for each duplicant type
- **Animation Variants** - Choose between Standard, Variant A, or Variant B animations

## How to Use

1. **Research Medicine I** - Unlocks the Manual Exerciser building
2. **Build Manual Exercisers** - Found in the Medicine category
3. **Add Schedule Blocks** - Add "Morning Exercise" to duplicant schedules
4. **Duplicants Exercise** - They find an exerciser during exercise time
5. **Receive Buffs** - After exercising, dupes get Athletics or Morale bonuses

### Tips

- Regular Duplicants get Athletics bonus (default +3 for 6 minutes)
- Bionic Duplicants get Morale bonus (default +2 for 12 minutes)
- Duplicants can only exercise once per exercise block
- Enable experience gain for permanent Athletics improvements

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\MorningExercise\`
3. Enable in the Mods menu

## ⚠️ WARNING: NOT SAVE SAFE

**If you delete this mod, your save games will NO LONGER LOAD!**
- Before removing this mod, you MUST remove all Morning Exercise schedule blocks from all Duplicants' schedules
- Failure to remove schedule blocks before uninstalling will crash the game when loading saves

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact**
- **Standard Game Mechanics** - Uses existing schedule and chore systems
- **No Polling** - Event-driven, no continuous monitoring
- **Lightweight Buffs** - Standard effect system for buffs

## Future Updates

- Custom animation variant for Manual Exerciser
- Additional exercise building types
- More buff customization options

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

Please mention "Morning Exercise" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.2.127**: Initial release
- **1.0.2.125**: Dupes now go to relax after exercising or if already buffed
- **1.0.1.103**: Code cleaning
