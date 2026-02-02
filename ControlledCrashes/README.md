# Controlled Crashes

Prevents Oxygen Not Included from crashing by catching exceptions in known problematic areas and logging detailed diagnostic information to help identify the root cause.

## Important Notice

**This mod is a direct copy of ExceptionCrashHandler by TrashCanHands, who has deleted his entire Workshop for ONI. If he reinstates his Workshop in the future, I will gladly delete this mod.**

**This mod DOES NOT fix anything.** It just prevents certain exceptions from causing a Crash to Desktop - it does not resolve anything to do with the exception, it just catches it gracefully. The method that was called that caused the exception will not continue executing.

## Description

This mod catches exceptions that would normally crash the game and instead logs detailed warnings with:
- What crashed and where
- How many times it has crashed
- Diagnostic information to help identify the cause
- Recommendations for fixing recurring issues

The game continues running after a caught exception, preventing save corruption and allowing you to finish what you're doing before addressing the issue.

## Features

- Catches crashes in 13 different game systems
- Detailed crash logging with timestamps and counts
- Identifies suspect mods causing issues via stack trace analysis
- Provides actionable recommendations after repeated crashes
- Tracks crash frequency per entity to detect patterns
- Automatic cleanup when crash tracker reaches 1000 entries

## Protected Systems

- **FetchAreaChore** - Invalid priorities and delivery issues
- **Growing plants** - Growth calculation errors
- **Animation system** - Missing animation files
- **Minion AI** - Animation tracking and todo list crashes
- **Critter feeding** - Food pathfinding issues
- **Vine branches** - Cell availability checks
- **Codex recipes** - Outdated mod compatibility
- **UI panels** - Todo sidescreen crashes
- **Underground Conduit mod** - Power terminal and details screen issues

## Installation

Subscribe on Steam Workshop or manually place in the local mods folder.

## How It Works

When an exception occurs in a protected method:
1. The mod catches it using Harmony Finalizer patches
2. Logs detailed diagnostic information to the game log
3. Increments a crash counter for that specific entity
4. Provides recommendations after 3-5 crashes of the same thing
5. Returns null to suppress the exception (game continues)

## Reading the Logs

Look for `[ControlledCrashes]` entries in your output_log.txt with severity levels:
- **[LOW]** - Minor issues, usually cosmetic
- **[MEDIUM]** - Moderate issues that may affect gameplay
- **[HIGH]** - Serious issues that should be addressed
- **[CRITICAL]** - Severe issues requiring immediate attention

## Compatibility

- Tested with game build: 700386+
- Compatible with: ALL content (Base game, Spaced Out!, Frosty Planet Pack)
- Special handling for Underground Conduit mod crashes
- Detects and reports suspect mods via stack trace analysis

## Changelog

### 1.0.0.0
- Initial release
- 13 crash protection patches implemented
- Crash tracking and diagnostic system
- Mod conflict detection