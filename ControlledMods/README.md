# ControlledMods

Customize and override settings from other mods via an in-game options menu. Take control of building capacities, limits, and other configurable values without editing config files manually.

## Features

- **In-Game Configuration** - All settings accessible from the main menu Mod Options
- **Restart Button** - Adds a Restart button to the main menu (above Quit) for quick game restarts after changing mods or settings
- **Smart Detection** - Options only apply if the target mod is detected and enabled
- **No Dependencies** - Works independently; target mods are optional, not required
- **Persistent Settings** - Configuration saved between game sessions

## Supported Mods

### Resource Sensor (Berkay's mod)

When the Resource Sensor mod is detected and the option is enabled:

- **Sidescreen** - Three scope checkboxes: **Atmosphere**, **Storage**, **Conduits** (each can be toggled independently); "Include Storage Buildings" row hidden; Global mode row hidden
- **Counting** - Atmosphere (cell element + pickupables), any building with a Storage component (including tile-based storage like Storage Tiles), and gas/liquid/solid conduits, based on the scope checkboxes; category tags in the element filter are expanded to discovered resources
- **Threshold** - Max raised to 9,999,999; units stripped from the threshold display (no "kg" in textbox or tooltips); input character limit raised to 8
- **Range visualizer** - Clears when the building is deselected (same behavior as switching to Room mode)
- **Copy Settings** - Copies the Atmosphere / Storage / Conduits scope toggles
- **Inversion** - When ControlledAutomation is loaded with inversion enabled, the invert checkbox appears on the Resource Sensor

Compatible with Berkay's Resource Sensor and ResourceSensorFIXED.

### Free Resource Buildings (castrolol's mod)

When the Free Resource Buildings mod is detected and the option is enabled:

- **Free Energy Generator wattage slider fix** - The sidescreen wattage slider now actually controls power output (the original mod's slider changes a value that is never read for generation)
- **Power Sink building** - Adds a Power Sink to the Power build menu (reverse of the Power Box): a 1×1 building that consumes power at a configurable rate via a sidescreen slider (0–40,000 W). Useful for testing power systems. Uses a red-tinted Power Box animation to distinguish it visually

### KIN Underground Conduit

- **Fix Power Terminal and Logic Terminal crash** - Prevents InvalidCastException when a logic wire is built in the same cell as a Power Terminal or Logic Terminal (optional, on by default)
- **Copy Settings** - Enables the vanilla Copy Settings tool for conduit terminals, senders, and receivers; channel is copied when you paste settings

*More mods will be added in future updates!*

## How to Use

1. **Subscribe/Install** - Enable the mod in the Mods menu
2. **Enable Target Mods** - Enable the mod(s) you want to customize (e.g., KIN Underground Conduit)
3. **Open Options** - Click "Mod Options" from the main menu and find "ControlledMods"
4. **Configure** - Adjust settings as desired
5. **Restart** - Restart the game for changes to take effect

### Tips

- Settings are grouped by target mod - expand each section to see available options
- Hover over options for detailed descriptions and default values
- Changes require a game restart to apply
- New buildings will use the new capacity; existing buildings may need to be rebuilt

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledMods\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Target Mods** - Does not require target mods to be installed; safely ignored if not present

## Performance

**Minimal Performance Impact**
- **Conditional patches** - Patches apply only when the target mod is detected and the relevant option is enabled
- **No polling** - No per-frame or sim tick work; logic runs only on paste (Copy Settings) or when the game triggers the patched events

## Future Updates

- Support for additional mods (suggest your favorites!)
- More building customization options
- Additional configurable values per mod

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

Please mention "ControlledMods" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 235 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.6.0**: Added colour visualisations to Logic Terminals for logic state
- **1.5.1**: Fixed kanim loading bug
- **1.5.0**: Free Resource Buildings – Free Energy Generator wattage slider now controls actual power output; Power Sink building added (configurable power consumer for testing)
- **1.4.0**: Resource Sensor – Storage counting now includes tile-based storage (e.g. Storage Tiles) in both Distance and Room modes
- **1.3.0**: Restart button on the main menu (above Quit) for quick restarts
- **1.2.0**: Resource Sensor (Berkay's mod) – sidescreen with Atmosphere/Storage/Conduits scope checkboxes; counting for atmosphere, storage buildings, and conduits with category tag expansion; threshold max raised to 9,999,999 with units stripped; range visualizer clears on deselect; Copy Settings copies scope toggles; ControlledAutomation inversion support
- **1.1.0**: KIN Underground Conduit – Logic Terminal crash fix, Copy Settings for conduit terminals/senders/receivers
- **1.0.0**: Initial release
