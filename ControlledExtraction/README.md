# Controlled Extraction

Per-building control over Oil Well extraction rates, output ports for various buildings, and Ice Kettle enhancements including meltable type selection.

## Features

- **Adjustable Water Input Rate** - Each Oil Well Cap gets a slider to control water consumption from 0.01 kg/s to 100 kg/s
- **Proportional Scaling** - Oil output and gas pressure buildup all scale with the water input rate
- **Per-Building Control** - Each Oil Well can have different settings - throttle some, boost others
- **Copy Settings Support** - Use the game's Copy Settings tool to quickly apply rates across multiple wells
- **Output Ports** - Add gas, liquid, and solid conveyor ports to various buildings
- **Ice Kettle Enhancements** - CO2 gas output, liquid output, configurable meltable types with per-building multi-select, and smart storage management

## How to Use

### Oil Well Cap
1. **Build an Oil Well Cap** on an Oil Reservoir (as normal)
2. **Select the Building** - The slider is now "Water Input Rate"
3. **Adjust Extraction Speed** - Set from 0.01 to 100 kg/s
4. **Copy Settings** - Use the game's Copy Settings tool to apply to other wells

### Ice Kettle
1. **Enable Features** in mod options (Configure Meltables button, CO2/Liquid ports)
2. **Build Ice Kettles** as normal
3. **Select Meltable Types** - Use the per-building sidescreen to choose which elements each kettle accepts. Types are organised into collapsible Vanilla/DLC and Modded groups, with group-level checkboxes for quick selection
4. **Connect Pipes** - Optionally connect gas/liquid pipes to the output ports

### Tips

- Higher extraction rates mean faster gas pressure buildup - enable the Gas Output Port for automatic venting
- Standard liquid pipes max at 10 kg/s
- Ice Kettle meltable types are configured globally in mod options (with Select All / Select None buttons), then per-building via the sidescreen
- Modded elements from other mods (e.g., Ronivan's Legacy) are auto-detected and available via the "Enable modded meltables" toggle
- When multiple meltable types are selected, the kettle shares its storage space proportionally across all types and automatically melts whichever has the most material first
- Deselect all types on a kettle to pause it - no deliveries will be made and the kettle will sit idle until a type is re-enabled

## Extraction Rates

| Water Input | Oil Output | Notes |
|-------------|------------|-------|
| 0.01 kg/s | 0.033 kg/s | Minimal - conserve water |
| 1 kg/s | 3.33 kg/s | Vanilla default |
| 10 kg/s | 33.33 kg/s | 10x speed (pipe limit) |
| 100 kg/s | 333.33 kg/s | Maximum chaos! |

## Building Output Ports

### Oil Well Cap
- Gas Output (Natural Gas) at (1, 1)
- Liquid Output (Crude Oil) at (2, 1)

### Oil Refinery
- Gas Output (Methane) at (-1, 3)

### Ethanol Distillery
- Gas Output (CO2) at (2, 2)
- Solid Output (Polluted Dirt) at (0, 0)
- Solid Input (Lumber) at (2, 0)

### Ice Kettle
- Gas Output (CO2) at (1, 1)
- Liquid Output (any melted liquid) at (1, 0)
- Smart storage management - shares capacity across all selected meltable types
- Can be paused by deselecting all types in the sidescreen

### Generators

| Building | CO2 Port | Polluted Water Port |
|----------|----------|---------------------|
| Coal Generator | (1, 1) | - |
| Wood Burner | (0, 1) | - |
| Petroleum Generator | (0, 1) | (1, 1) |
| Natural Gas Generator | - (vanilla) | (1, 1) |

*Note: Natural Gas Generator already has a CO2 gas output at (2, 2) in vanilla.*

All ports default to OFF and require game restart when changed.

## Mod Options

Configure via **Options > Mods > Controlled Extraction**:

### Oil Well Cap
- Default Water Rate, Min/Max slider values
- Backpressure Threshold (default: 75%)
- Max Gas/Oil Storage
- Gas and Liquid Output Ports

### Ice Kettle
- CO2 Gas Output Port
- Liquid Output Port
- Configure Meltables (opens dialog for vanilla/DLC per-element toggles, modded meltables toggle, and Select All / Select None buttons)

### Building Output Ports
Each building category (Oil Refinery, Ethanol Distillery, Coal Generator, Wood Burner, Petroleum Generator, Natural Gas Generator) has its own options section for enabling output ports.

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledExtraction\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Ronivan's Legacy** - Fully compatible - scales rates regardless of gas element, auto-detects modded meltable elements
- **Piped Everything** - Works alongside for even more options
- **Conveyor capacity mods** - Auto-detects increased capacity

## Performance

**Minimal Performance Impact**
- **Event-Driven** - No polling or continuous monitoring; patches only fire on building config/spawn
- **Lightweight Controllers** - Output controllers use standard conduit tick updates
- **Lazy Discovery** - Ice Kettle meltable elements are discovered once on first use, not every frame

## Future Updates

- Additional building support
- More Ice Kettle customization options

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

Please mention "Controlled Extraction" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.0**: Initial release
- **1.0.1**: Bugfixes
- **1.0.2**: Bugfix ethanol distillery ports not working
- **1.0.3**: Added more building output ports
- **1.1.0**: Ice Kettle enhancements - CO2 output, liquid output, configurable meltable types with per-building multi-select and modded element support
