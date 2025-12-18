# Copy Materials Tool

**⚠️ WARNING: This is a testing/work-in-progress mod. There WILL be issues, crashes, and incompatibilities. Use at your own risk. Do not use on save files you are not willing to lose.**

**🐛 Found a bug or have a suggestion? Report it on [GitHub Issues](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues)**

A Quality of Life mod that extends the "Copy Settings" tool to also copy construction materials between buildings of the same type.

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

## Requirements

- **Oxygen Not Included**: Build 700386 or later
- **Mod API**: Version 2
- **Dependencies**: Automatically included (PLib, Harmony)

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

## Credits

- Built using [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han
- Uses [Harmony](https://github.com/pardeike/Harmony) for runtime patching
- Inspired by the game's built-in Copy Settings tool

---

**Note**: This mod is a work in progress. Report bugs and suggestions on [GitHub Issues](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues) or through Steam Workshop comments.

