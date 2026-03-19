# Controlled Framerate

Adaptive game speed that automatically adjusts to maintain your target FPS. Run your colony at the fastest speed your PC can handle without dropping frames. Includes a built-in benchmark tool, customisable speed settings per save, and a live speed monitor.

## Features

- **Adaptive Speed Mode** - Dynamically adjusts game speed each frame to maintain your target FPS, slowing down when your PC struggles and speeding up when there's headroom
- **Built-in Benchmark** - Automated benchmark tests speeds from your configured maximum down to 1x, measuring FPS at each step to find the fastest sustainable speed for your colony
- **Per-Save Profiles** - Benchmark results and speed settings are stored per save, so each colony gets its own optimal speeds
- **Custom Speed Buttons** - Overrides the game's three speed buttons (Slow/Medium/Fast) with your benchmarked or manually configured speeds
- **Live Speed Monitor** - Collapsible side panel showing real-time FPS, current speed, mode, target FPS, and Standard/Medium/Max speed controls with inline adjustment buttons
- **Adaptive Mode Persistence** - Remembers whether Adaptive mode was enabled and restores it automatically when you reload a save
- **Mod Compatibility** - Detects other speed-modifying mods and adjusts behaviour to avoid conflicts

## How to Use

1. **Run a Benchmark** - Click the benchmark button in the top bar to open the configuration screen. Set your target FPS, maximum speed to test, step size, and acceptable threshold, then click Start
2. **Review Results** - After the benchmark completes, review the results showing your max sustainable speed, proposed speed button values, and FPS statistics. Accept to apply or discard to keep current settings
3. **Enable Adaptive Mode** - Click the adaptive mode button in the top bar to toggle automatic speed adjustment. The game will continuously adjust speed to stay near your target FPS
4. **Fine-tune Settings** - Use the +/- buttons on the Speed Monitor panel to adjust your target FPS and Standard/Medium/Max speed levels on the fly without re-running the benchmark

### Tips

- Lower your target FPS for higher game speeds — a target of 25 FPS lets the game run faster than a target of 60
- The benchmark tests your colony as it currently is. Re-run the benchmark as your colony grows, since more buildings and duplicants will reduce performance
- Speed settings are saved per colony, so you can have different speeds for a small early-game colony vs a large late-game base
- The Speed Monitor panel can be collapsed by clicking the header to save screen space
- If Adaptive mode seems stuck at a lower speed than expected, check that your FPS is at least 5 above your target — the controller uses a dead zone to prevent oscillation

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3670573352) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledFramerate\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Other Speed Mods** - Automatically detects other speed-modifying mods and disables conflicting features to prevent issues

## Performance

**Minimal Performance Impact**
- **FPS Monitoring** - Uses a lightweight exponential moving average updated once per frame with negligible overhead
- **Adaptive Controller** - Simple per-frame calculation with no allocations, comparable to the game's own speed control logic
- **Benchmark** - Only runs when manually triggered and restores normal speed when complete

## Future Updates

- Configurable dead zone and ramp rate from the options screen
- Multiple benchmark profiles per save for different game states

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

Please mention "Controlled Framerate" in your issue title or description.

## My Workshop & Collections

- [My Workshop](https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140) – All my ONI mods on Steam
- [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) – 235+ tested, compatible mods for Oxygen Not Included
- [The Controlled Series](https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653) – Collection of Controlled mods

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.2.0**: Added more specific speed controls to the Speed Monitor
- **1.1.0**: Reworked the menu buttons; Monitor now available in fixed or adaptive modes
- **1.0.0**: Initial release — adaptive speed control, benchmark tool, per-save profiles, live speed monitor
