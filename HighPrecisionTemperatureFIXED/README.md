# High Precision Temperature FIXED

Displays temperatures with 4 decimal places instead of the default 1 decimal place. A fixed and updated version of the original High Precision Temperature mod.

## Features

- **High Precision Display** - Shows temperature values like 25.1234°C instead of 25.1°C
- **All Units Supported** - Works with Celsius, Fahrenheit, and Kelvin
- **Robust Patching** - Uses a prefix patch approach that should survive future game updates

## Why This Mod?

The vanilla game restricts temperature display to 1 decimal place for most values (only showing 4 decimals when temperature is below 0.1). This can make precise thermal management difficult.

The original "High Precision Temperature" mod stopped working after game updates changed the internal method structure. This version:

1. **Targets the correct method** - Patches `AppendFormattedTemperature` where the actual formatting happens
2. **Uses a Prefix patch** - More reliable than IL transpilation for surviving updates
3. **Maintains compatibility** - Works with all temperature displays in the game

## Use Cases

- **Thermal Management** - See exact temperatures for precise heat exchange calculations
- **Debugging** - Identify small temperature changes that would otherwise be hidden
- **Mod Development** - Useful for testing thermal mechanics in custom buildings

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\HighPrecisionTemperatureFIXED\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact** - The patch simply changes the format string used for temperature display. No additional calculations or processing required.

## Technical Details

The vanilla game uses this logic in `GameUtil.AppendFormattedTemperature`:

```csharp
if (Mathf.Abs(temp) < 0.1f)
    builder.AppendFormat("{0:##0.####}", temp);  // 4 decimals
else
    builder.AppendFormat("{0:##0.#}", temp);     // 1 decimal
```

This mod replaces that with:

```csharp
builder.AppendFormat("{0:##0.####}", temp);      // Always 4 decimals
```

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

Please mention "HighPrecisionTemperatureFIXED" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- **Original Concept** - Based on the original "High Precision Temperature" mod
- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.0.0**: Initial release - Fixed version using Prefix patch approach
