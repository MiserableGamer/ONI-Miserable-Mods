# Skills and Stats Progress FIXED

Displays detailed skill XP progress, attribute experience, travel distance, speed info, and radiation stats on the duplicant stats panel. A fixed and updated version of Mantakus's SkillsAndStatsProgress mod, rebuilt for the current game version.

## Features

- **Skill XP Progress** - Shows current XP, XP needed for next skillpoint, percentage progress, and available/total skillpoints
- **Attribute Experience** - Displays per-attribute XP progress with level, experience, max XP, and percentage towards next level
- **Active XP Highlighting** - Attributes currently gaining experience are highlighted with bold formatting
- **Travel Distance Tracking** - Shows distance traveled today and total, broken down by navigation type (floor, ladder, tube, etc.)
- **Speed Display** - Real-time movement speed, position coordinates, and configurable average speed calculation
- **Radiation Info** - Radiation balance, change rate per cycle, recovery, current exposure, and resistance (Spaced Out DLC)
- **Workable Efficiency Pop-ups** - Optional floating text showing work efficiency, attribute levels, and XP gain when duplicants work on buildings
- **Delta Tracking** - Optional XP change tracking over configurable time intervals

## Configuration

All options are configurable via the in-game Mod Options menu (PLib Options). Settings are grouped into:

- **Display** - Control what's shown: max XP, required XP, stat name truncation, tab switching, XP precision
- **Speed & Travel** - Toggle speed display, average speed interval, travel distance tracking
- **Tracking** - Enable delta tracking with configurable snapshot intervals and sample rates
- **Radiation** - Toggle radiation info panel (Spaced Out DLC only)
- **Workable Info** - Configure efficiency pop-ups: visibility, speed, duration, font size, selected-dupe-only mode

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3662450561) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\SkillsAndStatsProgressFIXED\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact**
- **UI Only** - Only runs when the duplicant stats panel is visible
- **Efficient Tracking** - Delta tracking uses configurable sampling intervals to minimise overhead

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

Please mention "SkillsAndStatsProgressFIXED" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Originally created by [Mantakus](https://github.com/Mantakus) as SkillsAndStatsProgress
- Fixed and updated by MiserableGamer for current game versions
- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.1**: Fixed delta crash, and more bugfixes
- **1.0.0**: Initial release - rebuilt from Mantakus's original mod for current game API
