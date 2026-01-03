# Copy Tile Tool

Replace tiles in bulk by selecting a destination type, then a source type to replace, then drag-selecting an area.

## Features

- **Tile Type Replacement** - Change tiles from one type to another (e.g., Standard Tile → Mesh Tile)
- **Exact Matching** - Only replaces tiles with the same type AND material as your source
- **Bulk Replacement** - Drag to select multiple tiles at once
- **Safe Replacement** - Queues deconstruction and reconstruction through normal game systems
- **All Tile Types** - Works with Standard, Mesh, Airflow, Insulated, Metal, Plastic, Bunker, Carpet, Window, and all DLC tiles

## How to Use

1. **Select Destination Tile** - Click on a tile that represents what you want tiles to become
2. **Click "Copy Tile" Button** - In the tile's side screen, click the "Copy Tile" button
3. **Select Source Tile** - Click on a tile of a DIFFERENT type that you want to replace
4. **Drag to Apply** - Drag over the area to queue replacements for all matching tiles

### Tips

- This tool is for changing tile **types** (e.g., Standard Tile → Mesh Tile)
- For changing tile **materials** only (e.g., Sandstone Tile → Granite Tile), use the [Copy Materials Tool](https://steamcommunity.com/sharedfiles/filedetails/?id=3626072188) instead ([GitHub](https://github.com/MiserableGamer/ONI-Miserable-Mods/tree/master/CopyMaterialsTool))
- Only tiles that exactly match the destination (both type AND material) will be replaced
- If the source and destination are the same tile type, the tool will reject the selection

## Installation

### Steam Workshop (Recommended)
Subscribe on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=XXXXXXXXX) and enable in the Mods menu.

### Manual Installation
1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases)
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\CopyTileTool\`
3. Enable in the Mods menu

## Compatibility

- **Oxygen Not Included** - Build 700386 or later
- **Mod API** - Version 2
- **DLC Support** - Works with base game and all DLC

## Performance

**Minimal Performance Impact**


## Future Updates

- Visual feedback improvements during drag selection
- Additional tile type support as new content is released

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
- **📝 Other Issues**: [Other Issue](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml) - Something else that doesn't fit the above categories

Please mention "Copy Tile Tool" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching

## Version History

- **1.0.0.0**: Initial release
