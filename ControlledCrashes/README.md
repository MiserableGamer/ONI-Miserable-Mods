# Controlled Crashes

Prevents Oxygen Not Included from crashing by catching exceptions in known problematic areas and logging detailed diagnostic information to help identify the root cause.

## Important Notice

**This mod is a direct copy of ExceptionCrashHandler by TrashCanHands, who has deleted his entire Workshop for ONI. If he reinstates his Workshop in the future, I will gladly delete this mod.**

**This mod DOES NOT fix anything.** It just prevents certain exceptions from causing a Crash to Desktop - it does not resolve anything to do with the exception, it just catches it gracefully. The method that was called that caused the exception will not continue executing.

## Features

- **Crash protection** - Catches exceptions in 14+ game systems (Codex, FetchAreaChore, Growing, animation, Minion AI, etc.) before they CTD
- **Detailed logging** - Timestamps, crash counts, and diagnostic info in output_log.txt
- **Mod identification** - Stack trace analysis to help identify suspect mods
- **Recommendations** - Actionable advice after repeated crashes of the same entity
- **Crash tracking** - Frequency per entity with automatic cleanup at 1000 entries

### Protected Systems

- **FetchAreaChore** - Invalid priorities and delivery issues
- **Growing plants** - Growth calculation errors
- **Animation system** - Missing animation files
- **Minion AI** - Animation tracking and todo list crashes
- **Critter feeding** - Food pathfinding issues
- **Vine branches** - Cell availability checks
- **Codex** - Duplicate entries and recipe panel (e.g. outdated mod compatibility)
- **UI panels** - Todo sidescreen crashes
- **Underground Conduit mod** - Power terminal and details screen issues

## How to Use

1. **Subscribe or install** - Add the mod via Steam Workshop or manual install and enable in the Mods menu
2. **Play as usual** - When a protected method would crash, the mod catches it and logs instead
3. **Check the log** - Look for `[ControlledCrashes]` in `output_log.txt` for what was caught and recommendations

### Tips

- Log severity: **[LOW]** cosmetic, **[MEDIUM]** may affect gameplay, **[HIGH]** serious, **[CRITICAL]** address soon
- After 3–5 crashes of the same entity, the mod suggests a fix (e.g. dismantle and rebuild a building)
- The game continues running after a caught exception so you can save before fixing the cause

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3658365488) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledCrashes\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Other Mods** - Compatible with most mods; helps identify which mods may be causing crashes

## Performance

**Minimal Performance Impact**
- Patches only run when the protected methods are invoked
- Crash tracker is in-memory with automatic cleanup at 1000 entries

## Future Updates

- Additional protected systems as new crash patterns are identified
- Refinements to recommendations and log formatting

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

Please mention "ControlledCrashes" in your issue title or description.

## My Workshop & Collections

- [My Workshop](https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140) – All my ONI mods on Steam
- [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) – 235+ tested, compatible mods for Oxygen Not Included
- [The Controlled Series](https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653) – Collection of Controlled mods

## Credits

- Original concept and implementation: **ExceptionCrashHandler** by TrashCanHands (Workshop no longer available)
- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.2.2** - Code revision for incorrect mod blaming
- **1.2.1** - Prevent crash when construction completes with null items in storage
- **1.2.0** - Added more catches
- **1.1.0** - Added catch for duplicate codex entries; prevent null-key crash when skipping duplicates
- **1.0.0** - Initial release; 13 crash protection patches; crash tracking and diagnostic system; mod conflict detection
