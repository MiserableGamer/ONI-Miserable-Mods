[h1]Copy Materials Tool[/h1]

[b]⚠️ WARNING: This is a testing/work-in-progress mod. There WILL be issues, crashes, and incompatibilities. Use at your own risk. Do not use on save files you are not willing to lose.[/b]

A Quality of Life mod that extends the "Copy Settings" tool to also copy construction materials between buildings of the same type.

[h2]Features[/h2]

[list]
[*][b]Material Copying[/b] - Copy construction materials from one building to others of the same prefab type
[*][b]Conduit Support[/b] - Fully supports wires, logic wires, liquid conduits, and gas conduits (including insulated variants)
[*][b]Connection Preservation[/b] - Maintains conduit connection states during rebuild operations
[*][b]Bridge Support[/b] - Works with standard and extended bridges (compatible with ExtendedBuildingWidth mod)
[*][b]Smart Rebuilding[/b] - Automatically deconstructs and rebuilds buildings with the new material
[*][b]Batch Operations[/b] - Copy materials to multiple buildings by dragging the tool
[/list]

[h2]How to Use[/h2]

[olist]
[*][b]Select Source Building[/b] - Click on a building to use as the source
[*][b]Activate Copy Materials[/b] - In the building's side screen, click the "Copy Materials" button
[*][b]Apply to Targets[/b] - Click or drag over target buildings of the same type
[*][b]Automatic Rebuild[/b] - The mod will:
[list]
[*]Queue deconstruction of target buildings
[*]Place blueprints with the source material
[*]Preserve conduit connections
[/list]
[/olist]

[h3]Tips[/h3]

[list]
[*]Works with all building types: regular buildings, conduits, bridges, and more
[*]If materials already match, no rebuild is needed
[*]For conduits, connections to neighboring conduits are automatically preserved
[*]Extended bridges maintain their width when copying materials
[/list]

[h2]Compatibility[/h2]

[list]
[*][b]ExtendedBuildingWidth Mod[/b] - Fully supported - bridge widths are preserved when copying materials
[*][b]Other Mods[/b] - Compatible with most mods. If you encounter issues, please report them.
[*][b]Oxygen Not Included[/b] - Build 700386 or later
[*][b]Mod API[/b] - Version 2
[/list]

[h2]Performance[/h2]

[b]Minimal Performance Impact[/b]

[list]
[*][b]Idle[/b] - No performance impact when the tool is not in use
[*][b]Active Use[/b] - Lightweight operations only during active tool usage
[*][b]Temporary Components[/b] - All monitoring components self-destruct after completing their tasks
[*][b]Event-Driven[/b] - Uses Harmony patches that only trigger on specific game events
[/list]

The mod is designed to be efficient and should not impact game performance, even in large colonies.

[h2]Known Limitations[/h2]

[list]
[*]Only works with buildings of the same prefab type (e.g., can't copy from a Wire to a Logic Wire)
[*]Requires buildings to be deconstructed and rebuilt (takes time)
[*]Large batch operations may queue many deconstruction tasks
[/list]

[h2]Troubleshooting[/h2]

[h3]Materials Not Copying[/h3]

[list]
[*]Ensure source and target buildings are the same type
[*]Check that the tool is properly activated (button clicked in side screen)
[*]Verify the mod is enabled in the Mods menu
[/list]

[h3]Connections Not Preserved[/h3]

[list]
[*]This should be rare. If connections aren't preserved, try copying one building at a time
[*]Report the issue with details about the building type and connections
[/list]

[h3]Blueprint Appears Too Soon[/h3]

[list]
[*]For bridges, there may be a brief moment where a blueprint appears before deconstruction completes
[*]This is normal and will resolve automatically
[/list]

[h2]Future Updates[/h2]

[list]
[*][b]Settings Preservation[/b] - Automatically preserve priority, facades, and copy group tags when copying materials
[*][b]Blueprint Issues[/b] - Fix blueprint placement timing issues
[*][b]Port Overlap Errors[/b] - Resolve temporary overlapping ports error that can occur with bridges
[/list]

[h2]Support & Issues[/h2]

Found a bug or have a suggestion? Report it on GitHub:

[url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues]Issues[/url]

Please mention "Copy Materials Tool" in your issue title or description.

[h2]Mod Collection[/h2]

This mod is part of the [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 200+ Ultimate Mods collection[/url] on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

[h2]Credits[/h2]

[list]
[*]Built using [url=https://github.com/peterhaneve/ONIMods]PLib[/url] by Peter Han
[*]Uses [url=https://github.com/pardeike/Harmony]Harmony[/url] for runtime patching
[*]Inspired by the game's built-in Copy Settings tool
[/list]

[h2]Version History[/h2]

[list]
[*][b]0.1.8[/b]: Current version (auto-increments on build)
[*]See git history for detailed changelog
[/list]

[b]Note[/b]: This mod is a work in progress. Report bugs and suggestions on [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues]GitHub Issues[/url] or through Steam Workshop comments.

