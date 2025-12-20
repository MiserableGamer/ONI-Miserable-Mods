# Longer Arms

Extends duplicant reach distance, allowing them to reach over chasms and other areas that would normally be out of reach.

## Features

✅ **Configurable Vertical Reach** - Set how many additional cells duplicants can reach upward and downward

✅ **Configurable Horizontal Reach** - Set how many additional cells duplicants can reach left and right (allows reaching over chasms)

✅ **Reach Over Chasms** - Allows duplicants to reach across gaps and chasms that they cannot cross

✅ **Easy Configuration** - Simple options menu with separate sliders for vertical and horizontal reach

## Configuration

- **Vertical Reach (cells)** (Default: 1)
  - Number of additional cells beyond vanilla reach that duplicants can reach vertically (up/down)
  - Vanilla allows reaching 3 cells up (cells 1-3 from floor at 0)
  - Setting this to 1 allows reaching cell 4
  - Range: 0 to 10 cells
  - Set to 0 to disable vertical reach extension (vanilla behavior)

- **Horizontal Reach (cells)** (Default: 1)
  - Number of additional cells beyond vanilla reach that duplicants can reach horizontally (left/right)
  - Allows reaching over chasms and gaps that duplicants cannot cross
  - Range: 0 to 10 cells
  - Set to 0 to disable horizontal reach extension (vanilla behavior)

## Performance

⚡ **Minimal Performance Impact**

- ✅ **One-time initialization**: Runs once during game startup
- ✅ **No runtime overhead**: No ongoing processing during gameplay
- ✅ **Efficient implementation**: Single batch operation for path generation
- ✅ **No frame rate impact**: Uses the same pathfinding system as vanilla

The mod simply extends the game's existing reach tables, so there is no noticeable performance difference compared to vanilla gameplay.

## Compatibility

- ✅ All DLCs supported
- ✅ Minimum Build: 700386
- ✅ API Version: 2

## Support & Issues

Found a bug or have a feature request? Please report it on GitHub:

🔗 **Issues**: https://github.com/MiserableGamer/ONI-Miserable-Mods/issues

Please mention "Longer Arms" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Developed for Oxygen Not Included
- Uses PLib by Peter Han
- Uses Harmony by pardeike

## Future Functionality

The following features are planned for future releases:

🔮 **Diagonal Reach** - Allow duplicants to reach cells that are diagonally positioned (both above/below and to the side). This would enable reaching corner cells that are currently unreachable even with extended vertical reach. This feature was partially implemented but removed for v1.0 to ensure stability and proper wall collision detection.

🔮 **Linked Reach Sliders** - A checkbox option to link the vertical and horizontal reach sliders, so that adjusting one automatically updates the other to match. The last value you set would be applied to both sliders when the link is enabled.

## Version History

- **0.1.54**: Current version
  - Separate vertical and horizontal reach sliders
  - Vertical reach: extends upward and downward reach
  - Horizontal reach: allows reaching over chasms and gaps
  - Both features fully working and configurable (0-10 cells each)
  - Proper path validation to prevent reaching through solid walls

