# ControlledMods

Configure and improve behavior from supported ONI mods through a single in-game options menu.

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
- **Counting** - Atmosphere (cell element + pickupables), any building with a Storage component, and gas/liquid/solid conduits, based on the scope checkboxes; category tags in the element filter are expanded to discovered resources
- **Threshold** - Max raised to 9,999,999; units stripped from the threshold display (no "kg" in textbox or tooltips); input character limit raised to 8
- **Range visualizer** - Clears when the building is deselected (same behavior as switching to Room mode)
- **Copy Settings** - Copies the Atmosphere / Storage / Conduits scope toggles
- **Inversion** - When ControlledAutomation is loaded with inversion enabled, the invert checkbox appears on the Resource Sensor

Compatible with Berkay's Resource Sensor and ResourceSensorFIXED.

### KIN Underground Conduit

- Adds **Atmosphere / Storage / Conduits** scope controls
- Improves counting (includes tile storage and conduit contents)
- Raises threshold max to **9,999,999** and removes hardcoded unit text
- Clears range visualizer correctly on deselect
- Supports **Copy Settings** and ControlledAutomation inversion

Compatible with Berkay's Resource Sensor and ResourceSensorFIXED.

### [Free Resource Buildings (castrolol's mod)](https://steamcommunity.com/sharedfiles/filedetails/?id=2839006500)

- Fixes Free Energy Generator wattage slider to control real output
- Adds **Power Sink** (1x1 configurable power consumer, 0-40,000 W)

### [Customize Plants](https://steamcommunity.com/sharedfiles/filedetails/?id=1818145851)

- Applies `max_age` to Vine Branch (ovagro), which vanilla Customize Plants misses

### [Duplicant Room Sensor (Pholith's mod)](https://steamcommunity.com/sharedfiles/filedetails/?id=1921058858)

- Per-sensor **Range Limit** toggle and configurable **Range Input** (1-64)
- Range-limited sensing respects walls/closed doors and stays room-bounded
- Compatible with Peter Han's ShowRange visualization
- Supports **Copy Settings** for range toggle/value

### [KIN Underground Conduit](https://steamcommunity.com/sharedfiles/filedetails/?id=3347169088)

- Fixes Power Terminal / Logic Terminal InvalidCastException crash
- Enables **Copy Settings** for terminals, senders, and receivers

## How to Use

1. Subscribe and enable ControlledMods
2. Enable any target mods you want to control
3. Open **Mod Options -> ControlledMods**
4. Configure settings
5. Restart game

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3640737967) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledMods\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386+
- **Mod API** - Version 2
- **DLC** - Base game and all DLC
- **Target Mods** - Optional (safe if not installed)

## Performance

- Conditional patching only
- No continuous polling loops

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

## My Workshop & Collections

- [My Workshop](https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140) – All my ONI mods on Steam
- [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) – 235+ tested, compatible mods for Oxygen Not Included
- [The Controlled Series](https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653) – Collection of Controlled mods

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.5.0**: Free Resource Buildings – Free Energy Generator wattage slider now controls actual power output; Power Sink building added (configurable power consumer for testing)
- **1.4.0**: Resource Sensor – Storage counting now includes tile-based storage (e.g. Storage Tiles) in both Distance and Room modes
- **1.3.0**: Restart button on the main menu (above Quit) for quick restarts
- **1.2.0**: Resource Sensor (Berkay's mod) – sidescreen with Atmosphere/Storage/Conduits scope checkboxes; counting for atmosphere, storage buildings, and conduits with category tag expansion; threshold max raised to 9,999,999 with units stripped; range visualizer clears on deselect; Copy Settings copies scope toggles; ControlledAutomation inversion support
- **1.1.0**: KIN Underground Conduit – Logic Terminal crash fix, Copy Settings for conduit terminals/senders/receivers
- **1.0.0**: Initial release
