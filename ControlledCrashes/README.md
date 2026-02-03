# Controlled Crashes

Prevents Oxygen Not Included from crashing by catching exceptions in known problematic areas and logging detailed diagnostic information to help identify the root cause.

**Important:** This mod is a direct copy of ExceptionCrashHandler by TrashCanHands, who has deleted his entire Workshop for ONI. If he reinstates his Workshop in the future, I will gladly delete this mod. **This mod DOES NOT fix anything.** It just prevents certain exceptions from causing a Crash to Desktop - it does not resolve the underlying issue; it catches them gracefully. The method that caused the exception will not continue executing.

## Features

- **Crash Prevention** - Catches exceptions in 14 game systems before they crash the game
- **Detailed Logging** - Timestamps, locations, and crash counts in the game log
- **Stack Trace Analysis** - Helps identify suspect mods or vanilla issues
- **Actionable Recommendations** - Suggestions after repeated crashes of the same entity
- **Crash Tracking** - Per-entity frequency to detect patterns (auto-cleanup at 1000 entries)
- **Targeted Fixes** - Some patches prevent crashes by steering invalid state (e.g. VineBranch null-GO)

## How to Use

1. **Install the Mod** - Subscribe on Steam Workshop or place in local mods folder; enable in the Mods menu
2. **Play Normally** - When a protected path would crash, the mod catches it and logs a warning instead
3. **Check the Log** - Look for `[ControlledCrashes]` in your Player log for what was caught and recommendations

### Tips

- Log severity: **[LOW]** cosmetic, **[MEDIUM]** gameplay, **[HIGH]** serious, **[CRITICAL]** severe
- After 3–5 crashes of the same entity, the mod suggests a recommendation (e.g. dig up the vine, rebuild the building)

## Protected Systems

- **FetchAreaChore** - Invalid priorities and delivery issues
- **Growing plants** - Growth calculation errors
- **Animation system** - Missing animation files
- **Minion AI** - Animation tracking and todo list crashes
- **Critter feeding** - Food pathfinding issues
- **Vine branches** - Cell availability checks and state transition (null GameObject after discovery)
- **Codex recipes** - Outdated mod compatibility
- **UI panels** - Todo sidescreen crashes
- **Underground Conduit mod** - Power terminal and details screen issues

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledCrashes\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Works alongside other mods; can help identify which mods are causing crashes

## Performance

**Minimal impact** - Patches only run when the protected code path is hit (e.g. vine transition, chore delivery). No per-frame overhead.

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

Please mention "Controlled Crashes" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Original concept and implementation: **ExceptionCrashHandler** by TrashCanHands (Workshop removed; this is a direct copy)
- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.1.0**: VineBranch GoTo null-GO patch (prevents crash when discovering new vine); 14 protected systems
- **1.0.0.0**: Initial release; 13 crash protection patches; crash tracking and diagnostic system
