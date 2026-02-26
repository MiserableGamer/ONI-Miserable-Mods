# Controlled Power

Power diode for Oxygen Not Included: one-way power flow between circuits, with upstream logic/load reporting that includes downstream demand.

## Features

- **One-way power flow** — Power moves input -> output only.
- **Transformer-style behavior** — Built on stable vanilla transformer mechanics.
- **Upstream current reporting** — Input-side logic/wire readouts include downstream current load.
- **Upstream potential reporting** — Input-side potential load includes downstream potential demand through diode chains.

## How to Use

1. Research and build the **Power Diode** between two power circuits.
2. Connect source power to the input side and loads to the output side.
3. Use wattage sensors on the upstream circuit to monitor total downstream demand.

### Tips

- Use diodes to prevent backfeed and enforce power direction.
- Chain diodes to segment power networks while keeping upstream load visibility.
- Warning: Do not feed two diode outputs into the same circuit. Upstream power calculations are currently designed for a single diode path and may be incorrect with merged diode outputs.

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3673526385) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledPower\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** — Build 700386 or later
- **Mod API** — Version 2
- **DLC Support** — Works with base game and all DLC (including Bionic Booster Pack)
- **Other Mods** — Compatible with most mods; combined logic option works with any mod's power sensors

## Performance

**Minimal Performance Impact**
- One Harmony patch on CircuitManager when combined logic is used
- Diode uses the same power simulation as vanilla transformers

## Support & Issues

Please mention "Controlled Power" in your issue title or description when reporting on GitHub.

## My Workshop & Collections

- [My Workshop](https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140) – All my ONI mods on Steam
- [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) – 235+ tested, compatible mods for Oxygen Not Included
- [The Controlled Series](https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653) – Collection of Controlled mods

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

### 1.1.0
- Added upstream calculations enable/disable checkbox

### 1.0.0
- Initial release
