[h1]Controlled Crashes[/h1]

Prevents Oxygen Not Included from crashing by catching exceptions in known problematic areas and logging detailed diagnostic information to help identify the root cause.

[h2]Important Notice[/h2]

[b]This mod is a direct copy of ExceptionCrashHandler by TrashCanHands, who has deleted his entire Workshop for ONI. If he reinstates his Workshop in the future, I will gladly delete this mod.[/b]

[b]This mod DOES NOT fix anything.[/b] It just prevents certain exceptions from causing a Crash to Desktop - it does not resolve anything to do with the exception, it just catches it gracefully. The method that was called that caused the exception will not continue executing.

[h2]Features[/h2]
[list]
[*][b]Crash protection[/b] - Catches exceptions in 14+ game systems (Codex, FetchAreaChore, Growing, animation, Minion AI, etc.) before they CTD
[*][b]Detailed logging[/b] - Timestamps, crash counts, and diagnostic info in output_log.txt
[*][b]Mod identification[/b] - Stack trace analysis to help identify suspect mods
[*][b]Recommendations[/b] - Actionable advice after repeated crashes of the same entity
[*][b]Crash tracking[/b] - Frequency per entity with automatic cleanup at 1000 entries
[/list]

[h3]Protected Systems[/h3]
[list]
[*] FetchAreaChore - Invalid priorities and delivery issues
[*] Growing plants - Growth calculation errors
[*] Animation system - Missing animation files
[*] Minion AI - Animation tracking and todo list crashes
[*] Critter feeding - Food pathfinding issues
[*] Vine branches - Cell availability checks
[*] Codex - Duplicate entries and recipe panel (e.g. outdated mod compatibility)
[*] UI panels - Todo sidescreen crashes
[*] Underground Conduit mod - Power terminals and details screens
[/list]

[h2]How to Use[/h2]
[olist]
[*][b]Subscribe or install[/b] - Add the mod via Steam Workshop or manual install and enable in the Mods menu
[*][b]Play as usual[/b] - When a protected method would crash, the mod catches it and logs instead
[*][b]Check the log[/b] - Look for [b][ControlledCrashes][/b] in output_log.txt for what was caught and recommendations
[/olist]

[h3]Tips[/h3]
[list]
[*] Log severity: [b][LOW][/b] cosmetic, [b][MEDIUM][/b] may affect gameplay, [b][HIGH][/b] serious, [b][CRITICAL][/b] address soon
[*] After 3-5 crashes of the same entity, the mod suggests a fix (e.g. dismantle and rebuild a building)
[*] The game continues running after a caught exception so you can save before fixing the cause
[/list]

[h2]Compatibility[/h2]
[list]
[*][b]Oxygen Not Included[/b] - Build 700386 or later
[*][b]Mod API[/b] - Version 2
[*][b]DLC Support[/b] - Works with base game and all DLC (including Bionic Booster Pack)
[*][b]Other Mods[/b] - Compatible with most mods; helps identify which mods may be causing crashes
[/list]

[h2]Performance[/h2]
[b]Minimal Performance Impact[/b]
[list]
[*] Patches only run when the protected methods are invoked
[*] Crash tracker is in-memory with automatic cleanup at 1000 entries
[/list]

[h2]Future Updates[/h2]
[list]
[*] Additional protected systems as new crash patterns are identified
[*] Refinements to recommendations and log formatting
[/list]

[h2]Support & Issues[/h2]
Need help, found a bug, or have a suggestion? We're here to help!

[h3]Community[/h3]
[list]
[*][b]Discord[/b]: [url=https://discord.com/channels/1452947938304200861/1452947939927392398]Join our Discord server[/url] for discussions, questions, and community support
[*][b]GitHub Discussions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions]Discuss on GitHub[/url] - share ideas, ask questions, or get help with modding
[/list]

[h3]Reporting Issues[/h3]
Found a bug or have a feature request? Please report it on GitHub using our issue templates:
[list]
[*][b]Bug Reports[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml]Report a Bug[/url] - Use this for crashes, errors, or unexpected behavior
[*][b]Feature Requests[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml]Suggest a Feature[/url] - Have an idea for a new feature or improvement?
[*][b]Questions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml]Ask a Question[/url] - Need help understanding how something works?
[*][b]Other Issues[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml]Other Issue[/url] - Something else that doesn't fit the above categories
[/list]

Please mention "ControlledCrashes" in your issue title or description.

[h2]Mod Collection[/h2]
This mod is part of the [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 235+ Ultimate Mods collection[/url] on Steam Workshop, featuring over 235 tested and compatible mods for Oxygen Not Included.

[h2]Credits[/h2]
[list]
[*] Original concept and implementation: [b]ExceptionCrashHandler[/b] by TrashCanHands (Workshop no longer available)
[*] Built using [url=https://github.com/peterhaneve/ONIMods]PLib[/url] by Peter Han
[*] Uses [url=https://github.com/pardeike/Harmony]Harmony[/url] for runtime patching
[/list]

[h2]Version History[/h2]
[list]
[*][b]1.1.0[/b] - Added catch for duplicate codex entries; prevent null-key crash when skipping duplicates
[*][b]1.0.0[/b] - Initial release; 13 crash protection patches; crash tracking and diagnostic system; mod conflict detection
[/list]
