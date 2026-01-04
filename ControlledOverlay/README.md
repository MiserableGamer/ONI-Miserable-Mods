# Controlled Overlay

Adds a text input field to the Temperature Overlay's relative temperature slider, allowing you to type exact temperature values instead of struggling with the slider.

## Features

- **Exact Temperature Input** - Type the exact temperature you want to focus on
- **Unit Aware** - Respects your temperature unit preference (°C, °F, K)
- **Replaces Slider Display** - The center temperature display becomes an editable input field
- **Instant Feedback** - Slider snaps to your entered value immediately
- **No Delete Required** - Just start typing to replace the value

## How to Use

1. **Open Temperature Overlay** - Press F3 or click the temperature button
2. **Select Relative Mode** - Choose "Relative Temperature" mode
3. **Click Input Field** - Click on the temperature display (center of slider)
4. **Type Value** - Just start typing your target temperature
5. **Press Enter** - The slider will snap to that temperature

### Tips

- No need to delete the existing value - just start typing
- Works with any temperature unit you have configured
- Great for monitoring specific temperature ranges for crops, critters, or equipment
- The overlay will show temperatures relative to your entered value

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledOverlay\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Does not conflict with other overlay mods or CapacityControl

## Performance

**Minimal Performance Impact**
- **Idle** - No performance impact when not using the temperature overlay
- **Active Use** - Lightweight UI updates only when typing
- **Event-Driven** - Only processes input when the overlay is active

## Future Updates

- Additional overlay controls (range adjustment, min/max settings)
- Custom temperature presets
- Keyboard shortcuts for quick temperature changes

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

Please mention "Controlled Overlay" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.1.1**: Initial release - adds temperature input field to relative temperature overlay
