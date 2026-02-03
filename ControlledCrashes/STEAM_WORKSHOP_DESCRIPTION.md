[h1]Controlled Crashes[/h1]

Prevents Oxygen Not Included from crashing by catching exceptions in known problematic areas and logging detailed diagnostic information to help identify the root cause.

[b]Important:[/b] This mod is a direct copy of ExceptionCrashHandler by TrashCanHands, who has deleted his entire Workshop for ONI. If he reinstates his Workshop in the future, I will gladly delete this mod. [b]This mod DOES NOT fix anything.[/b] It just prevents certain exceptions from causing a Crash to Desktop - it does not resolve the underlying issue; it catches them gracefully. The method that caused the exception will not continue executing.

[h2]Features[/h2]
[list]
[*][b]Crash Prevention[/b] - Catches exceptions in 14 game systems before they crash the game
[*][b]Detailed Logging[/b] - Timestamps, locations, and crash counts in the game log
[*][b]Stack Trace Analysis[/b] - Helps identify suspect mods or vanilla issues
[*][b]Actionable Recommendations[/b] - Suggestions after repeated crashes of the same entity
[*][b]Crash Tracking[/b] - Per-entity frequency to detect patterns (auto-cleanup at 1000 entries)
[*][b]Targeted Fixes[/b] - Some patches prevent crashes by steering invalid state (e.g. VineBranch null-GO)
[/list]

[h2]How to Use[/h2]
[olist]
[*][b]Install the Mod[/b] - Subscribe on Steam Workshop or place in local mods folder; enable in the Mods menu
[*][b]Play Normally[/b] - When a protected path would crash, the mod catches it and logs a warning instead
[*][b]Check the Log[/b] - Look for [b][ControlledCrashes][/b] in your Player log for what was caught and recommendations
[/olist]

[h3]Tips[/h3]
[list]
[*]Log severity: [b][LOW][/b] cosmetic, [b][MEDIUM][/b] gameplay, [b][HIGH][/b] serious, [b][CRITICAL][/b] severe
[*]After 3–5 crashes of the same entity, the mod suggests a recommendation (e.g. dig up the vine, rebuild the building)
[/list]

[h2]Protected Systems[/h2]
[list]
[*][b]FetchAreaChore[/b] - Invalid priorities and delivery issues
[*][b]Growing plants[/b] - Growth calculation errors
[*][b]Animation system[/b] - Missing animation files
[*][b]Minion AI[/b] - Animation tracking and todo list crashes
[*][b]Critter feeding[/b] - Food pathfinding issues
[*][b]Vine branches[/b] - Cell availability checks and state transition (null GameObject after discovery)
[*][b]Codex recipes[/b] - Outdated mod compatibility
[*][b]UI panels[/b] - Todo sidescreen crashes
[*][b]Underground Conduit mod[/b] - Power terminal and details screen issues
[/list]

[h2]Reading the Logs[/h2]
Look for [b][ControlledCrashes][/b] entries in your Player log with severity levels and recommendations.

[h2]Installation[/h2]
Subscribe on Steam Workshop and enable in the Mods menu. Or download from [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/releases]GitHub[/url] and extract to [code]%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledCrashes\[/code]

[h2]Compatibility[/h2]
[list]
[*][b]Oxygen Not Included[/b] - Build 700386 or later
[*][b]Mod API[/b] - Version 2
[*][b]DLC Support[/b] - Works with base game and all DLC
[*][b]Other Mods[/b] - Works alongside other mods; can help identify which mods are causing crashes
[/list]

[h2]Performance[/h2]
[b]Minimal impact[/b] - Patches only run when the protected code path is hit. No per-frame overhead.

[h2]Support & Issues[/h2]
Need help, found a bug, or have a suggestion? We're here to help!

[h3]Community[/h3]
[list]
[*][b]💬 Discord[/b]: [url=https://discord.com/channels/1452947938304200861/1452947939927392398]Join our Discord server[/url] for discussions, questions, and community support
[*][b]📝 GitHub Discussions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions]Discuss on GitHub[/url] - share ideas, ask questions, or get help with modding
[/list]

[h3]Reporting Issues[/h3]
Found a bug or have a feature request? Please report it on GitHub using our issue templates:
[list]
[*][b]🐛 Bug Reports[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml]Report a Bug[/url] - Use this for crashes, errors, or unexpected behavior
[*][b]💡 Feature Requests[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml]Suggest a Feature[/url] - Have an idea for a new feature or improvement?
[*][b]❓ Questions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml]Ask a Question[/url] - Need help understanding how something works?
[*][b]📝 Other Issues[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml]Other Issue[/url] - Something else that doesn't fit the above categories
[/list]

Please mention "Controlled Crashes" in your issue title or description.

[h2]Mod Collection[/h2]
This mod is part of the [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 200+ Ultimate Mods collection[/url] on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

[h2]Credits[/h2]
[list]
[*]Original concept and implementation: [b]ExceptionCrashHandler[/b] by TrashCanHands (Workshop removed; this is a direct copy)
[*]Built using [url=https://github.com/peterhaneve/ONIMods]PLib[/url] by Peter Han
[*]Uses [url=https://github.com/pardeike/Harmony]Harmony[/url] for runtime patching
[/list]

[h2]Version History[/h2]
[list]
[*][b]1.0.1.0[/b]: VineBranch GoTo null-GO patch (prevents crash when discovering new vine); 14 protected systems
[*][b]1.0.2.0[/b]: Initial release; 13 crash protection patches; crash tracking and diagnostic system
[/list]
