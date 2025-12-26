[h1]Longer Arms[/h1]
Extends duplicant reach distance, allowing them to reach over chasms and other areas that would normally be out of reach.

[h2]Features[/h2]
[list]
[*][b]Configurable Vertical Reach[/b] - Set how many additional cells duplicants can reach upward and downward
[*][b]Configurable Horizontal Reach[/b] - Set how many additional cells duplicants can reach left and right (allows reaching over chasms)
[*][b]Reach Over Chasms[/b] - Allows duplicants to reach across gaps and chasms that they cannot cross
[*][b]Easy Configuration[/b] - Simple options menu with separate sliders for vertical and horizontal reach
[/list]

[h2]Configuration[/h2]
[list]
[*][b]Vertical Reach (cells)[/b] (Default: 1)
[list]
[*]Number of additional cells beyond vanilla reach that duplicants can reach vertically (up/down)
[*]Vanilla allows reaching 3 cells up (cells 1-3 from floor at 0)
[*]Setting this to 1 allows reaching cell 4
[*]Range: 0 to 10 cells
[*]Set to 0 to disable vertical reach extension (vanilla behavior)
[/list]
[*][b]Horizontal Reach (cells)[/b] (Default: 1)
[list]
[*]Number of additional cells beyond vanilla reach that duplicants can reach horizontally (left/right)
[*]Allows reaching over chasms and gaps that duplicants cannot cross
[*]Range: 0 to 10 cells
[*]Set to 0 to disable horizontal reach extension (vanilla behavior)
[/list]
[*][b]Safe Mode (Prevent Reach-Through-Walls)[/b] (Default: true)
[list]
[*][b]Important[/b]: Due to the way the game calculates reach paths, extending horizontal reach beyond a certain amount can create an issue where duplicants can reach through obstructed tiles (solid walls/rocks).
[*]When [b]enabled[/b] (recommended): Horizontal reach is automatically capped at 2 additional cells to prevent reaching through solid tiles. This ensures safe behavior.
[*]When [b]disabled[/b]: You can use the full horizontal reach value (up to 10), but this will allow duplicants to reach through walls and other solid obstacles. Use this option only if you understand the consequences and want maximum reach despite potential issues.
[/list]
[/list]

[h2]Performance[/h2]
[b]Minimal Performance Impact[/b]
[list]

The mod simply extends the game's existing reach tables, so there is no noticeable performance difference compared to vanilla gameplay.

[h2]Compatibility[/h2]
[list]
[*]All DLCs supported
[*]Minimum Build: 700386
[*]API Version: 2
[/list]

[h2]Support & Issues[/h2]
Need help, found a bug, or have a suggestion? We're here to help!

[h3]Community[/h3]
[list]
[*][b]💬 Discord[/b]: [url=https://discord.com/channels/1452947938304200861/1452947939927392398]Join our Discord server[/url] for discussions, questions, and community support
[*][b]📝 GitHub Discussions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions]Discuss on GitHub[/url] - share ideas, ask questions, or get help with modding
[/list]

[h3]Reporting Issues[/h3]
Found a bug or have a feature request? Please report it on GitHub using our issue templates:
[list]
[*][b]🐛 Bug Reports[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml]Report a Bug[/url] - Use this for crashes, errors, or unexpected behavior
[*][b]💡 Feature Requests[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml]Suggest a Feature[/url] - Have an idea for a new feature or improvement?
[*][b]❓ Questions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml]Ask a Question[/url] - Need help understanding how something works?
[*][b]📝 Other Issues[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml]Other Issue[/url] - Something else that doesn't fit the above categories
[/list]

Please mention "Longer Arms" in your issue title or description.

[h2]Mod Collection[/h2]
This mod is part of the [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 200+ Ultimate Mods collection[/url] on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

[h2]Credits[/h2]
[list]
[*]Developed for Oxygen Not Included
[*]Uses PLib by Peter Han
[*]Uses Harmony by pardeike
[/list]

[h2]Future Functionality[/h2]
The following features are planned for future releases:
[list]
[*][b]Diagonal Reach[/b] - Allow duplicants to reach cells that are diagonally positioned (both above/below and to the side). This would enable reaching corner cells that are currently unreachable even with extended vertical reach. This feature was partially implemented but removed for v1.0 to ensure stability and proper wall collision detection.
[*][b]Linked Reach Sliders[/b] - A checkbox option to link the vertical and horizontal reach sliders, so that adjusting one automatically updates the other to match. The last value you set would be applied to both sliders when the link is enabled.
[/list]

[h2]Version History[/h2]
[list]
[*][b]1.0.1.0[/b]: Initial release
[*][b]1.0.1.56[/b]: Code cleaning
[*][b]1.0.2.64[/b]: Diagonal bug fix
[*][b]1.0.3.76[/b]: Fix ladder bug, and added safe mode
[/list]
