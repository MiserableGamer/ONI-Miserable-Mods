# Controlled Visuals

A performance optimization mod for Oxygen Not Included that allows you to reduce the visual quality of pipe animations to improve frame rates in large colonies.

## Features

- **Configurable Pipe Animation Quality** - Choose between Full, Reduced, or Minimal quality for liquid and gas pipe flow animations
- **Smart Zoom Detection** - Automatically reduces animation updates further when zoomed out (since you can't see the detail anyway)
- **No Gameplay Impact** - Only affects visual rendering, not actual pipe mechanics

## How to Use

1. Enable the mod in your mod list
2. Access the mod options (gear icon in the mod list)
3. Select your preferred **Pipe Animation Quality**:
   - **Full**: Unchanged from base game (every frame)
   - **Reduced**: Updates at 10 FPS (every 0.1 seconds)
   - **Minimal**: Updates at 2 FPS (every 0.5 seconds)
4. Restart the game for changes to take effect

### Tips

- Start with **Reduced** quality - most players won't notice the difference
- Use **Minimal** for very large colonies with extensive pipe networks
- The mod automatically reduces updates to 1 FPS when zoomed far out regardless of setting

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledVisuals\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Compatible with most mods

## Performance Impact

**Medium Performance Improvement**

The performance benefit depends on your colony size and pipe network complexity:
- Small colonies (< 50 dupes): Minimal improvement
- Medium colonies (50-100 dupes): Noticeable improvement
- Large colonies (100+ dupes with extensive piping): Significant improvement

The mod works by throttling how often the pipe flow mesh is recalculated and redrawn, which can be CPU-intensive in colonies with thousands of pipe segments.

## Technical Details

- Throttles `ConduitFlowVisualizer.Render` updates based on configured quality
- Redraws the last calculated mesh between updates instead of recalculating
- Automatically detects zoom level and reduces updates further when zoomed out

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

Please mention "ControlledVisuals" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Inspired by [Fast Track](https://github.com/peterhaneve/ONIMods) by Peter Han
- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.0**: Initial release - Configurable pipe animation quality
