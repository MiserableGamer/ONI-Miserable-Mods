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
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3658365488) and enable in the Mods menu.

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

### 1.0.0.0
- Initial release
- 13 crash protection patches implemented
- Crash tracking and diagnostic system
- Mod conflict detection