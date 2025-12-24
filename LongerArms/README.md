# Longer Arms Mod

A mod for Oxygen Not Included that extends duplicant reach distance, allowing them to reach over chasms and other areas that would normally be out of reach.

## Features

- **Configurable Vertical Reach**: Set how many additional cells duplicants can reach upward and downward
- **Configurable Horizontal Reach**: Set how many additional cells duplicants can reach left and right (allows reaching over chasms)
- **Reach Over Chasms**: Allows duplicants to reach across gaps and chasms that they cannot cross
- **Easy Configuration**: Simple options menu with separate sliders for vertical and horizontal reach

## Configuration

The mod includes configurable options accessible through the mod options menu:

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

This mod has **minimal performance impact**:

- **One-time initialization cost**: The mod runs once during game startup when it patches the reach tables
- **No runtime overhead**: After initialization, the mod does not perform any ongoing processing
- **Efficient implementation**: Paths are generated and added to the game's offset tables in a single batch operation
- **No frame rate impact**: The mod does not affect game performance during gameplay

The mod simply extends the game's existing reach tables, so duplicants use the same pathfinding system as vanilla - just with additional reachable cells. There is no noticeable performance difference compared to vanilla gameplay.

## Compatibility

- **Supported Content**: All DLCs
- **Minimum Build**: 700386
- **API Version**: 2
- Uses PLib for options management

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

Please mention "Longer Arms" in your issue title or description so we can identify it easily.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Developed for Oxygen Not Included
- Uses PLib by Peter Han
- Uses Harmony by pardeike

## Future Functionality

The following features are planned for future releases:

- **Diagonal Reach**: Allow duplicants to reach cells that are diagonally positioned (both above/below and to the side). This would enable reaching corner cells that are currently unreachable even with extended vertical reach. This feature was partially implemented but removed for v1.0 to ensure stability and proper wall collision detection.

- **Linked Reach Sliders**: A checkbox option to link the vertical and horizontal reach sliders, so that adjusting one automatically updates the other to match. The last value you set would be applied to both sliders when the link is enabled.

## Version History

- **1.0.1.60**: Initial release
- **1.0.1.56**: Code cleaning

