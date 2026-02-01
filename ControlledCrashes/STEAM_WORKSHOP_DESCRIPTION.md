[h1]Controlled Crashes[/h1]

Prevents Oxygen Not Included from crashing by catching exceptions and logging detailed diagnostic information to help you identify and fix the root cause.

[h2]Important Notice[/h2]

[b]This mod is a direct copy of ExceptionCrashHandler by TrashCanHands, who has deleted his entire Workshop for ONI. If he reinstates his Workshop in the future, I will gladly delete this mod.[/b]

[b]This mod DOES NOT fix anything.[/b] It just prevents certain exceptions from causing a Crash to Desktop - it does not resolve anything to do with the exception, it just catches it gracefully. The method that was called that caused the exception will not continue executing.

[h2]Features[/h2]
[list]
[*] Catches crashes in 13 different game systems before they can crash the game
[*] Detailed crash logging with timestamps, locations, and crash counts
[*] Identifies suspect mods causing issues via stack trace analysis
[*] Provides actionable recommendations after repeated crashes
[*] Tracks crash frequency per entity to detect patterns
[*] Special handling for known mod conflicts (Underground Conduit, etc.)
[/list]

[h2]How It Works[/h2]

When an exception would normally crash your game, this mod:
[olist]
[*] Catches the exception using Harmony Finalizer patches
[*] Logs detailed diagnostic information to output_log.txt
[*] Tracks how many times that specific thing has crashed
[*] Provides recommendations after 3-5 crashes of the same entity
[*] Lets the game continue running (no crash!)
[/olist]

[h2]Protected Systems[/h2]

[list]
[*] FetchAreaChore - Invalid priorities and delivery crashes
[*] Growing plants - Growth calculation errors
[*] Animation system - Missing animation files
[*] Minion AI - Animation tracking and todo list
[*] Critter feeding - Food pathfinding issues
[*] Vine branches - Cell availability checks
[*] Codex recipes - Outdated mod compatibility
[*] UI panels - Todo sidescreen crashes
[*] Underground Conduit mod - Power terminals and details screens
[/list]

[h2]Reading the Logs[/h2]

Look for [b][ControlledCrashes][/b] entries in your output_log.txt with severity levels:
[list]
[*] [b][LOW][/b] - Minor issues, usually cosmetic
[*] [b][MEDIUM][/b] - Moderate issues that may affect gameplay
[*] [b][HIGH][/b] - Serious issues that should be addressed
[*] [b][CRITICAL][/b] - Severe issues requiring immediate attention
[/list]

The mod will tell you:
[list]
[*] What crashed and where (coordinates, building name, minion name)
[*] How many times it has crashed
[*] What might be causing it
[*] What you should do to fix it
[/list]

[h2]Example Log Output[/h2]

[code]
[ControlledCrashes] [15:42:33] [HIGH] Invalid FetchAreaChore priority detected:
  Priority: 0 (INVALID - fixing to 5)
  Chore Type: FarmFetch
  Target: Farm Tile (ID: 12345, Unity: 67890)
  Location: Cell 2145 (X:15, Y:42)
  Crash Count: 3 time(s)
  RECOMMENDATION: Farm tile at Cell 2145 has priority issues. Dismantle and rebuild the farm tile.
[/code]

[h2]Compatibility[/h2]

Compatible with Base game, Spaced Out!, and Frosty Planet Pack. Works alongside other mods and can help identify which mods are causing crashes.

[h2]Issues & Feedback[/h2]

Please report any bugs or suggestions in the comments or on GitHub.

[h2]Credits[/h2]

This is a direct copy of ExceptionCrashHandler by TrashCanHands. All credit for the original concept and implementation goes to them.
