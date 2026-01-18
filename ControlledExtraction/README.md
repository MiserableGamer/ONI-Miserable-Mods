# Controlled Extraction

Per-building control over Oil Well extraction rates and output ports for various buildings. Finally, you decide how fast to pump!

## Features

- **Adjustable Water Input Rate**: Each Oil Well Cap gets a slider to control water consumption from 0.01 kg/s to 100 kg/s
- **Proportional Scaling**: Oil output and gas pressure buildup all scale with the water input rate
- **Per-Building Control**: Each Oil Well can have different settings - throttle some, boost others
- **Copy Settings Support**: Use the game's Copy Settings tool to quickly apply rates across multiple wells
- **Output Ports**: Add gas, liquid, and solid conveyor ports to various buildings

## Extraction Rates

| Water Input | Oil Output | Notes |
|-------------|------------|-------|
| 0.01 kg/s | 0.033 kg/s | Minimal - conserve water |
| 1 kg/s | 3.33 kg/s | Vanilla default |
| 10 kg/s | 33.33 kg/s | 10x speed (pipe limit) |
| 100 kg/s | 333.33 kg/s | Maximum chaos! |

*Note: Standard liquid pipes max at 10 kg/s.*

## ⚠️ Important: Gas Pressure Warning

**Everything scales with extraction rate!** Higher water input = faster gas pressure buildup.

| Water Rate | Gas Buildup | Venting Needed |
|------------|-------------|----------------|
| 1 kg/s | Normal | Occasional |
| 10 kg/s | 10x faster | Very frequent |
| 100 kg/s | 100x faster | Constant! |

At high extraction rates, duplicants may spend all their time venting pressure!

**Solutions:**
1. Enable **"Add Gas Output Port"** in mod options (requires restart) - built-in automatic venting!
2. Increase "Max Gas Storage" to reduce venting frequency
3. Or use [Piped Everything](https://steamcommunity.com/workshop/filedetails/?id=2058745508) for more advanced piping

## Building Output Ports

Add output ports to pipe away gases, liquids, and solids from various buildings!

### Oil Well Cap
- Gas Output (Natural Gas) at (1, 1)
- Liquid Output (Crude Oil) at (2, 1)

### Oil Refinery
- Gas Output (Methane) at (-1, 3)

### Ethanol Distillery
- Gas Output (CO2) at (2, 2)
- Solid Output (Polluted Dirt) at (0, 0)
- Solid Input (Lumber) at (2, 0)

### Generators

| Building | CO2 Port | Polluted Water Port |
|----------|----------|---------------------|
| Coal Generator | ✅ (1, 1) | - |
| Wood Burner | ✅ (0, 1) | - |
| Petroleum Generator | ✅ (0, 1) | ✅ (1, 1) |
| Natural Gas Generator | - (vanilla) | ✅ (1, 1) |

*Note: Natural Gas Generator already has a CO2 gas output at (2, 2) in vanilla.*

All ports default to OFF and require game restart when changed.

## Mod Options

Configure via **Options > Mods > Controlled Extraction**:

### Oil Well Cap
- **Default Water Rate**: Initial slider value for new wells (default: 1 kg/s)
- **Minimum/Maximum Water Rate**: Slider limits
- **Backpressure Threshold**: When dupes vent (default: 75%)
- **Max Gas Storage**: Default 80 kg - increase for high extraction rates!
- **Max Oil Storage**: Default 50 kg - increase if oil backs up
- **Add Gas Output Port**: Automatic venting! (requires restart)
- **Add Liquid Output Port**: Direct oil extraction! (requires restart)

Each building category has its own options section for enabling output ports.

### Solid Conduit Capacity

Solid conveyor outputs auto-detect conduit capacity from the game. If you use mods that increase conveyor capacity (e.g., 50 kg instead of 20 kg), this is automatically detected. Falls back to vanilla 20 kg if detection fails.

## Compatibility

- ✅ **Ronivan's Legacy**: Fully compatible - scales rates regardless of gas element
- ✅ **Piped Everything**: Works alongside for even more options
- ✅ **Conveyor capacity mods**: Auto-detects increased capacity
- ✅ Base Game
- ✅ Spaced Out! DLC
- ✅ All DLCs and content packs

## How to Use

1. Build an Oil Well Cap on an Oil Reservoir (as normal)
2. Select the building
3. The slider (previously "Backpressure Release Threshold") is now **Water Input Rate**
4. Adjust to your preferred extraction speed (0.01 to 100 kg/s)
5. Use Copy Settings to apply to other wells

*Note: The original backpressure slider is replaced. Duplicants will still depressurize at the configured threshold.*

## Installation

### Steam Workshop (Recommended)
Subscribe on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=YOUR_WORKSHOP_ID) and enable in the Mods menu.

### Manual Installation
1. Download the latest release
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledExtraction\`
3. Enable in the game's Mods menu

## Requirements

- Oxygen Not Included: Build 700386 or later
- Mod API Version: 2

## Version History

### 1.0.1.35 (Current)
- Bugfixes

### 1.0.0.1
- Initial release

## Credits

- **PLib**: [Peter Han](https://github.com/peterhaneve/ONIMods) - Essential modding library
- **Harmony**: [pardeike](https://github.com/pardeike/Harmony) - Runtime patching library

## Source Code

This mod is open source: [GitHub Repository](https://github.com/MiserableGamer/ONI-Miserable-Mods)

## Support

Found a bug? Have a suggestion? Please report on [GitHub Issues](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues).

## Other Mods

Check out my other quality-of-life mods in the [ONI 200+ Ultimate Mods Collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156)!

## License

See LICENSE.txt for license information.
