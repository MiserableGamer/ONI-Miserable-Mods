# Controlled Storage

A unified storage control mod for Oxygen Not Included that combines and enhances multiple storage-related features.

**⚠ No Sweep Zones is currently not compatible with [Better Dupe Assignment](https://steamcommunity.com/sharedfiles/filedetails/?id=3657612749) — BDA makes the No Sweep Zone apply to Sweepers too when it is supposed to apply only to Dupes.**

## Features

- **Empty Storage** - Add "Empty Storage" button to storage buildings. Creates a dupe errand to drop all stored items. Optional skill requirement and instant emptying option.
- **Controlled Filtering** - Customize which item categories appear in storage filters (Clothing, Eggs, Sublimating items). Choose whether items are "standard" or "non-standard".
- **Capacity Control** - Increase character limit in capacity input fields. Support for larger storage values (up to 9,999,999 kg).
- **Delivery Control** - Control dupe and auto-sweeper deposit and extract permissions per storage building. Copy checkbox settings to other buildings of the same type via the Copy Delivery Settings button.
- **No-Sweep Zones** - Mark areas where Duplicants will not sweep items from. Toolbar icon and overlay to define zones.

## How to Use

### Empty Storage
1. **Select a storage building** (Storage Bin, Smart Storage Bin, Fridge, etc.)
2. **Click "Empty Storage"** in the building's side panel
3. A dupe will be assigned to drop all stored items (or instantly if configured)

### Delivery Control
1. **Select a storage or loader building** with Delivery Control enabled
2. **Toggle permissions** for dupe deposit, dupe extract, sweeper deposit, and sweeper extract
3. **Copy settings** to other buildings of the same type using the Copy Delivery Settings button

### No-Sweep Zones
1. **Open the toolbar** and select the No-Sweep Zone tool
2. **Paint zones** on the map where dupes should not sweep from
3. Use the **overlay** to visualize active zones

### Tips

- Use Delivery Control to prevent sweeper loops by disabling sweeper extract on bins that sweepers deposit into
- No-Sweep Zones are great for keeping natural resources in place (e.g. Oxylite near your oxygen supply)
- Capacity Control lets you type values beyond 999,999 kg for modded high-capacity storage

## Mod Options

Configure via **Options > Mods > Controlled Storage**:

### Empty Storage
- **Immediate Emptying** - Drop contents instantly or create a dupe errand (default: instant)
- **Require Skills** - Dupes need Tidy skill to empty solid storage (when not immediate)
- **Use Work Time** - Emptying takes time based on mass stored (when not immediate)
- **Work Time per 100kg** - Configurable from 0.1 to 10 seconds

### Storage Filtering
- **Clothing is Non-Standard** - Move Clothing to Non-Standard filter section
- **Critter Eggs are Non-Standard** - Move Eggs to Non-Standard filter section
- **Sublimating Items are Non-Standard** - Move Bleach Stone, Oxylite, etc. to Non-Standard section

### Capacity Control
- **Additional Input Characters** - Extra characters beyond vanilla's 6-character limit (default: 2, giving 8 total)

### Delivery Control
- **Enable for Storage Bins** - Add delivery control to Storage Bins and Smart Storage Bins
- **Enable for Fridges** - Add delivery control to Refrigerators and Ration Boxes
- **Enable for Loaders** - Add delivery control to conveyor loaders (Solid Conduit Inbox)

### No-Sweep Zones
- **Enable No-Sweep Zones** - Adds the No-Sweep Zone tool to the toolbar

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3658128284) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\ControlledStorage\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC (including Bionic Booster Pack)
- **Other Mods** - Compatible with most mods

## Performance

**Minimal Performance Impact**
- **Event-Driven** - No polling or continuous monitoring; patches only fire on relevant game events
- **Lightweight Overlays** - No-Sweep Zone overlay uses standard game overlay system

## Future Updates

- Additional building type support for Delivery Control
- Enhanced No-Sweep Zone visualization options

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

Please mention "Controlled Storage" in your issue title or description.

## My Workshop & Collections

- [My Workshop](https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140) – All my ONI mods on Steam
- [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) – 235+ tested, compatible mods for Oxygen Not Included
- [The Controlled Series](https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653) – Collection of Controlled mods

## Credits

- **Rubacava** - No-Sweep Zones feature inspired by [Sweep Zones](https://steamcommunity.com/sharedfiles/filedetails/?id=2897886291)
- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.0**: Initial release
- **1.0.1**: Bugfix for Copy errors
- **1.0.2**:Bug fix for sweeper loop
