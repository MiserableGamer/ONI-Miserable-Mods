# Copy Materials Tool

**⚠️ WARNING: This is a testing/work-in-progress mod. There WILL be issues, crashes, and incompatibilities. Use at your own risk. Do not use on save files you are not willing to lose.**

A Quality of Life mod for Oxygen Not Included that extends the "Copy Settings" tool to also copy construction materials between buildings of the same type.

## Features

- **Material Copying**: Copy construction materials from one building to others of the same prefab type
- **Conduit Support**: Fully supports wires, logic wires, liquid conduits, and gas conduits (including insulated variants)
- **Connection Preservation**: Maintains conduit connection states during rebuild operations
- **Bridge Support**: Works with standard and extended bridges (compatible with ExtendedBuildingWidth mod)
- **Smart Rebuilding**: Automatically deconstructs and rebuilds buildings with the new material
- **Batch Operations**: Copy materials to multiple buildings by dragging the tool

## How to Use

1. **Select Source Building**: Click on a building to use as the source
2. **Activate Copy Materials**: In the building's side screen, click the "Copy Materials" button
3. **Apply to Targets**: Click or drag over target buildings of the same type
4. **Automatic Rebuild**: The mod will:
   - Queue deconstruction of target buildings
   - Place blueprints with the source material
   - Preserve conduit connections

### Tips

- Works with all building types: regular buildings, conduits, bridges, and more
- If materials already match, no rebuild is needed
- For conduits, connections to neighboring conduits are automatically preserved
- Extended bridges maintain their width when copying materials

## Installation

### Steam Workshop (Recommended)
1. Subscribe to the mod on the Steam Workshop
2. Launch Oxygen Not Included
3. Enable the mod in the Mods menu

### Manual Installation
1. Download the latest release
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\CopyMaterialsTool\`
3. Ensure the folder contains:
   - `CopyMaterialsTool.dll`
   - `mod_info.yaml`
   - `mod.yaml`
4. Launch the game and enable the mod in the Mods menu

## Requirements

- **Oxygen Not Included**: Build 700386 or later
- **Mod API**: Version 2
- **Dependencies**: 
  - PLib (automatically included)
  - Harmony (automatically included)

## Compatibility

- **ExtendedBuildingWidth Mod**: Fully supported - bridge widths are preserved when copying materials
- **Other Mods**: Compatible with most mods. If you encounter issues, please report them.

## Performance

The mod has **minimal performance impact**:

- **Idle**: No performance impact when the tool is not in use
- **Active Use**: Lightweight operations only during active tool usage
- **Temporary Components**: All monitoring components self-destruct after completing their tasks
- **Event-Driven**: Uses Harmony patches that only trigger on specific game events

The mod is designed to be efficient and should not impact game performance, even in large colonies.

## Building from Source

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.8
- Oxygen Not Included game files (for references)

### Required References
- `0Harmony.dll` (Harmony library)
- `Assembly-CSharp.dll` (from ONI installation)
- `Assembly-CSharp-firstpass.dll` (from ONI installation)
- `PLib.dll` (PLib library)
- `UnityEngine.dll` and related modules (from ONI installation)
- `KMod.dll` (from ONI installation)

### Build Steps
1. Clone or download this repository
2. Open `CopyMaterialsTool.csproj` in Visual Studio
3. Ensure all references point to valid ONI DLLs
4. Build the project (version will auto-increment in `mod_info.yaml`)
5. Copy the output DLL and mod files to your mods directory

## Configuration

### Debug Mode
Debug logging can be toggled by setting `CopyMaterialsManager.DebugMode = true` in code. By default, debug logs are disabled to prevent log spam.

## Known Limitations

- Only works with buildings of the same prefab type (e.g., can't copy from a Wire to a Logic Wire)
- Requires buildings to be deconstructed and rebuilt (takes time)
- Large batch operations may queue many deconstruction tasks

## Troubleshooting

### Materials Not Copying
- Ensure source and target buildings are the same type
- Check that the tool is properly activated (button clicked in side screen)
- Verify the mod is enabled in the Mods menu

### Connections Not Preserved
- This should be rare. If connections aren't preserved, try copying one building at a time
- Report the issue with details about the building type and connections

### Blueprint Appears Too Soon
- For bridges, there may be a brief moment where a blueprint appears before deconstruction completes
- This is normal and will resolve automatically

## Future Updates

- **Settings Preservation**: Automatically preserve priority, facades, and copy group tags when copying materials
- **Blueprint Issues**: Fix blueprint placement timing issues
- **Port Overlap Errors**: Resolve temporary overlapping ports error that can occur with bridges

## License

[Specify your license here]

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching
- Inspired by the game's built-in Copy Settings tool

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

Please mention "Copy Materials Tool" in your issue title or description so we can identify it easily.

## Version History

- **0.1.10**: Test release
