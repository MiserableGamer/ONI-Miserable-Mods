# ControlledMods

Configure and improve behavior from supported ONI mods through a single in-game options menu.

## Features

- In-game settings in Mod Options
- Direct Workshop links for supported mods in Mod Options
- Restart button added to the main menu
- Safe mod detection (patches apply only when target mods are present)
- Saved settings between sessions

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

- **Fix Power Terminal and Logic Terminal crash** - Prevents InvalidCastException when a logic wire is built in the same cell as a Power Terminal or Logic Terminal (optional, on by default)
- **Copy Settings** - Enables the vanilla Copy Settings tool for conduit terminals, senders, and receivers; channel is copied when you paste settings

Compatible with Berkay's Resource Sensor and ResourceSensorFIXED.

### [Free Resource Buildings (castrolol's mod)](https://steamcommunity.com/sharedfiles/filedetails/?id=2839006500)

- Fixes Free Energy Generator wattage slider to control real output
- Adds **Power Sink** (1x1 configurable power consumer, 0-40,000 W)

### [Customize Plants](https://steamcommunity.com/sharedfiles/filedetails/?id=1818145851)

- Applies `max_age` to Vine Branch (ovagro), which vanilla Customize Plants misses

### [Duplicant Room Sensor (Pholith's mod)](https://steamcommunity.com/sharedfiles/filedetails/?id=1921058858)

- Per-sensor **Range Limit** toggle and configurable **Range Input** (1-64)
- Range-limited sensing respects walls/closed doors and stays room-bounded

### [Darkness Not Excluded (Relit)](https://steamcommunity.com/sharedfiles/filedetails/?id=3609476592)

- Helps reduce light bleed through solid tiles in darkness visuals

### [KIN Underground Conduit](https://steamcommunity.com/sharedfiles/filedetails/?id=3347169088)

- Fixes Power Terminal / Logic Terminal InvalidCastException crash
- Enables **Copy Settings** for terminals, senders, and receivers

### [Signs, Tags and Ribbons](https://steamcommunity.com/sharedfiles/filedetails/?id=2883096049)

- Adds **Raw Natural Gas** to the Small Element Tag variant list
- Friendly tooltips on variant buttons (e.g. "Raw Natural Gas", "Brine")

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
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3640737967) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledMods\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386+
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Target Mods** - Does not require target mods to be installed; safely ignored if not present

## Performance

**Minimal Performance Impact**
- **Conditional patches** - Patches apply only when the target mod is detected and the relevant option is enabled
- **No polling** - No per-frame or sim tick work; logic runs only on paste (Copy Settings) or when the game triggers the patched events

## Future Updates

- Conditional patching only
- No continuous polling loops

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

- **1.2.0**: Resource Sensor (Berkay's mod) – sidescreen with Atmosphere/Storage/Conduits scope checkboxes; counting for atmosphere, storage buildings, and conduits with category tag expansion; threshold max raised to 9,999,999 with units stripped; range visualizer clears on deselect; Copy Settings copies scope toggles; ControlledAutomation inversion support
- **1.1.0**: KIN Underground Conduit – Logic Terminal crash fix, Copy Settings for conduit terminals/senders/receivers
- **1.0.0**: Initial release
