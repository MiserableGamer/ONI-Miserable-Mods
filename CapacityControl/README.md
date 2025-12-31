# Capacity Control Mod

A mod for Oxygen Not Included that increases the storage capacity text input character limit beyond the vanilla 6 characters.

## Problem

The vanilla game limits the storage capacity control text input to 6 characters. This means you can only manually type values up to 999,999 kg. When using mods that increase storage capacity beyond this amount, you can use the slider to set higher values, but you cannot manually type specific values larger than 6 digits.

## Solution

This mod allows you to configure how many additional characters to add beyond the vanilla limit. By default, it adds 2 extra characters (for 8 total), allowing values up to 9,999,999 kg.

## Features

- **Configurable Character Limit**: Choose how many extra characters to add (1-10) via the mod options menu
- **Works with All Storage**: Applies to any building that uses the capacity control side screen
- **Compatible with Storage Mods**: Designed to work alongside mods that increase storage capacity

## Configuration

Access the mod options through the game's Mod Options menu:

- **Additional Characters** (Default: 2): Number of additional characters beyond the vanilla 6 character limit
  - Range: 1 to 10
  - Examples:
    - 1 = 7 characters total (up to 999,999.9 kg)
    - 2 = 8 characters total (up to 9,999,999 kg)
    - 4 = 10 characters total (up to 999,999,999 kg)

**Note:** Changes require a game restart to take effect.

## Technical Details

This mod patches `CapacityControlSideScreen.OnSpawn` to modify the `TMP_InputField` character limit after the screen is initialized. Configuration is stored in the shared PLib config location.

## Compatibility

- Works with all DLC (base game, Spaced Out!, etc.)
- Compatible with mods that increase storage capacity
- No known conflicts with other mods

## Version History

- **1.0.1.1**: Initial release with configurable character limit
