# Resource Limpet

Attaches to storage buildings and outputs ribbon cable signals for low/high resource levels.

## Features

- **Storage Attachment** - Attaches to gas, liquid, or solid storage buildings
- **Ribbon Cable Output** - Uses two channels for low/high signals
- **Low Level Signal** - Sends signal on one channel when storage hits low level
- **High Level Signal** - Sends signal on another channel when storage hits high level
- **Compact Design** - 1x1 size building

## How to Use

1. **Build the Resource Limpet** - Place next to a storage building
2. **Connect Ribbon Cable** - Connect to your automation network
3. **Configure Thresholds** - Set low and high level thresholds
4. **Receive Signals** - Automation signals trigger at configured levels

### Tips

- Based on the Automated Notifier with ribbon cable support
- Different notifications depending on which channel is activated
- Great for managing storage levels with automation
- Works with all storage building types

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ResourceLimpet\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact**
- **Event-Driven** - Only updates when storage levels change
- **Efficient Checks** - Lightweight threshold comparisons
- **No Polling** - Uses game's built-in storage monitoring

## Future Updates

- Additional threshold options
- More customization for signal behavior
- Support for multiple storage types

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

Please mention "Resource Limpet" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **0.0.0.18**: Initial release
