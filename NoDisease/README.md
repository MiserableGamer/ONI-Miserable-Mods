# No Disease Mod

A mod for Oxygen Not Included that **completely disables** the disease system - not just immunity, but removes disease mechanics entirely from the game.

> **Note**: This mod is intended as an **interim solution** until [FastTrack](https://steamcommunity.com/sharedfiles/filedetails/?id=1967921388) by Peter Han is updated for the current game version. If you only want the "No Disease" functionality without FastTrack's other features, this standalone mod is for you. This mod will be removed from the workshop if requested by other modders.

## Features

This mod comprehensively disables all disease-related systems:

- **No Germs**: All existing germs are wiped from the world on load
- **No Disease Spread**: Germs cannot spread through air, water, or pipes
- **No Infections**: Duplicants cannot get sick from germs
- **No Disease UI**: Disease overlay and panels are hidden
- **No Disease Tools**: Disinfect tool is removed
- **Hidden Buildings**: Disease-related buildings are deprecated:
  - Doctor Stations (basic and advanced)
  - Disease Sensors (gas, liquid, solid conduits)
  - Apothecary buildings (except when Radiation DLC is enabled - kept for rad pills)

## Why Use This Mod?

- **Performance**: Disease simulation can impact game performance on large colonies
- **Gameplay Preference**: Some players find the disease system tedious rather than fun
- **Simplification**: Focus on other aspects of colony management

## Technical Details

This mod patches over 40 game methods to completely disable:
- Disease containers and emitters
- Germ exposure monitoring
- Auto-disinfection systems
- World generation disease placement
- Disease in conduits and storage

## Credits

This mod is inspired by the disease-disabling functionality from **FastTrack** by **Peter Han**.
- Original source: [FastTrack on GitHub](https://github.com/peterhaneve/ONIMods/tree/main/FastTrack)
- Licensed under MIT License

## Compatibility

- **Supported Content**: All DLCs
- **Minimum Build**: 700386
- **API Version**: 2
- Uses PLib by Peter Han

## Support & Issues

Need help, found a bug, or have a suggestion? We're here to help!

### Community

- **💬 Discord**: [Join our Discord server](https://discord.com/channels/1452947938304200861/1452947939927392398) for discussions, questions, and community support
- **📝 GitHub Discussions**: [Discuss on GitHub](https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions) - share ideas, ask questions, or get help with modding

### Reporting Issues

Found a bug or have a feature request? Please report it on GitHub using our issue templates:

- **🐛 Bug Reports**: [Report a Bug](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml)
- **💡 Feature Requests**: [Suggest a Feature](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml)
- **❓ Questions**: [Ask a Question](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml)

Please mention "No Disease" in your issue title or description.

## Version History

- **0.0.1.1**: Initial release - complete disease system removal
- **1.0.1.13**: Fixed visual bug (red box around duplicants with existing disease effects)
