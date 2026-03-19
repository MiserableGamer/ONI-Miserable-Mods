[h1]Controlled Framerate[/h1]

Adaptive game speed that automatically adjusts to maintain your target FPS. Run your colony at the fastest speed your PC can handle without dropping frames. Includes a built-in benchmark tool, customisable speed settings per save, and a live speed monitor.

[h2]Features[/h2]
[list]
[*] [b]Adaptive Speed Mode[/b] - Dynamically adjusts game speed each frame to maintain your target FPS, slowing down when your PC struggles and speeding up when there's headroom
[*] [b]Built-in Benchmark[/b] - Automated benchmark tests speeds from your configured maximum down to 1x, measuring FPS at each step to find the fastest sustainable speed for your colony
[*] [b]Per-Save Profiles[/b] - Benchmark results and speed settings are stored per save, so each colony gets its own optimal speeds
[*] [b]Custom Speed Buttons[/b] - Overrides the game's three speed buttons (Slow/Medium/Fast) with your benchmarked or manually configured speeds
[*] [b]Live Speed Monitor[/b] - Collapsible side panel showing real-time FPS, current speed, mode, target FPS, and Standard/Medium/Max speed controls with inline adjustment buttons
[*] [b]Adaptive Mode Persistence[/b] - Remembers whether Adaptive mode was enabled and restores it automatically when you reload a save
[*] [b]Mod Compatibility[/b] - Detects other speed-modifying mods and adjusts behaviour to avoid conflicts
[/list]

[h2]How to Use[/h2]
[olist]
[*] [b]Run a Benchmark[/b] - Click the benchmark button in the top bar to open the configuration screen. Set your target FPS, maximum speed to test, step size, and acceptable threshold, then click Start
[*] [b]Review Results[/b] - After the benchmark completes, review the results showing your max sustainable speed, proposed speed button values, and FPS statistics. Accept to apply or discard to keep current settings
[*] [b]Enable Adaptive Mode[/b] - Click the adaptive mode button in the top bar to toggle automatic speed adjustment. The game will continuously adjust speed to stay near your target FPS
[*] [b]Fine-tune Settings[/b] - Use the +/- buttons on the Speed Monitor panel to adjust your target FPS and Standard/Medium/Max speed levels on the fly without re-running the benchmark
[/olist]

[h3]Tips[/h3]
[list]
[*] Lower your target FPS for higher game speeds — a target of 25 FPS lets the game run faster than a target of 60
[*] The benchmark tests your colony as it currently is. Re-run the benchmark as your colony grows, since more buildings and duplicants will reduce performance
[*] Speed settings are saved per colony, so you can have different speeds for a small early-game colony vs a large late-game base
[*] The Speed Monitor panel can be collapsed by clicking the header to save screen space
[*] If Adaptive mode seems stuck at a lower speed than expected, check that your FPS is at least 5 above your target — the controller uses a dead zone to prevent oscillation
[/list]

[h2]How the Benchmark Works[/h2]

The benchmark automates what you'd normally do by trial and error:
[olist]
[*] Starts at your configured maximum speed (e.g. 10x)
[*] Lets the game settle at that speed, then measures FPS for several seconds
[*] If FPS is below your target, steps down to the next lower speed
[*] Repeats until it finds a speed where FPS meets your target (within your configured threshold)
[*] Proposes Slow/Medium/Fast button speeds based on the result
[/olist]

A live chart shows the progress as it tests each speed, with FPS bars coloured green (pass) or red (fail) relative to your target.

[h2]How Adaptive Mode Works[/h2]

When Adaptive mode is active, the mod adjusts [b]Time.timeScale[/b] every frame:
[list]
[*] If FPS drops below your target, speed decreases to let your PC catch up
[*] If FPS is well above your target (5+ FPS headroom), speed gradually increases
[*] If FPS is near your target, speed holds steady to avoid oscillation
[*] Speed never exceeds the ceiling set for the current speed button
[/list]

[h2]Compatibility[/h2]
[list]
[*] [b]Oxygen Not Included[/b] - Build 700386 or later
[*] [b]Mod API[/b] - Version 2
[*] [b]DLC Support[/b] - Works with base game and all DLC (including Bionic Booster Pack)
[*] [b]Other Speed Mods[/b] - Automatically detects other speed-modifying mods and disables conflicting features to prevent issues
[/list]

[h2]Performance[/h2]
[b]Minimal Performance Impact[/b]
[list]
[*] [b]FPS Monitoring[/b] - Uses a lightweight exponential moving average updated once per frame with negligible overhead
[*] [b]Adaptive Controller[/b] - Simple per-frame calculation with no allocations, comparable to the game's own speed control logic
[*] [b]Benchmark[/b] - Only runs when manually triggered and restores normal speed when complete
[/list]

[h2]Future Updates[/h2]
[list]
[*] Configurable dead zone and ramp rate from the options screen
[*] Multiple benchmark profiles per save for different game states
[/list]

[h2]Support & Issues[/h2]
Need help, found a bug, or have a suggestion? We're here to help!

[h3]Community[/h3]
[list]
[*] [b]Discord[/b]: [url=https://discord.com/channels/1452947938304200861/1452947939927392398]Join our Discord server[/url] for discussions, questions, and community support
[*] [b]GitHub Discussions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions]Discuss on GitHub[/url] - share ideas, ask questions, or get help with modding
[/list]

[h3]Reporting Issues[/h3]
Found a bug or have a feature request? Please report it on GitHub using our issue templates:
[list]
[*] [b]Bug Reports[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml]Report a Bug[/url] - Use this for crashes, errors, or unexpected behavior
[*] [b]Feature Requests[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml]Suggest a Feature[/url] - Have an idea for a new feature or improvement?
[*] [b]Questions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml]Ask a Question[/url] - Need help understanding how something works?
[*] [b]Other Issues[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml]Other Issue[/url] - Something else that doesn't fit the above categories
[/list]

Please mention "Controlled Framerate" in your issue title or description.

[h2]My Workshop & Collections[/h2]
[list]
[*][url=https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140]My Workshop[/url] – All my ONI mods on Steam
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 235+ Ultimate Mods collection[/url] – 235+ tested, compatible mods for Oxygen Not Included
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653]The Controlled Series[/url] – Collection of Controlled mods
[/list]

[h2]Credits[/h2]
[list]
[*] Built using [url=https://github.com/peterhaneve/ONIMods]PLib[/url] by Peter Han
[*] Uses [url=https://github.com/pardeike/Harmony]Harmony[/url] for runtime patching
[/list]

[h2]Version History[/h2]
[list]
[*] [b]1.2.0[/b]: Added more specific speed controls to the Speed Monitor
[*] [b]1.1.0[/b]: Reworked the menu buttons; Monitor now available in fixed or adaptive modes
[*] [b]1.0.0[/b]: Initial release — adaptive speed control, benchmark tool, per-save profiles, live speed monitor
[/list]
