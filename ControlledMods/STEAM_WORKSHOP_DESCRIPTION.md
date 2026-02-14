[h1]ControlledMods[/h1]

Configure and improve behavior from supported ONI mods through one in-game options menu.

[h2]Features[/h2]
[list]
[*][b]In-Game Configuration[/b] - All settings accessible from the main menu Mod Options
[*][b]Restart Button[/b] - Adds a Restart button to the main menu (above Quit) for quick game restarts after changing mods or settings
[*][b]Smart Detection[/b] - Options only apply if the target mod is detected and enabled
[*][b]No Dependencies[/b] - Works independently; target mods are optional, not required
[*][b]Persistent Settings[/b] - Configuration saved between game sessions
[/list]

[h2]Supported Mods[/h2]

[h3]Resource Sensor[/h3]
When the Resource Sensor mod is detected and the option is enabled:
[list]
[*][b]Sidescreen[/b] - Three scope checkboxes: Atmosphere, Storage, Conduits (each can be toggled independently); "Include Storage Buildings" row hidden; Global mode row hidden
[*][b]Counting[/b] - Atmosphere (cell element + pickupables), any building with a Storage component, and gas/liquid/solid conduits, based on the scope checkboxes; category tags in the element filter are expanded to discovered resources
[*][b]Threshold[/b] - Max raised to 9,999,999; units stripped from the threshold display (no "kg" in textbox or tooltips); input character limit raised to 8
[*][b]Range visualizer[/b] - Clears when the building is deselected (same behavior as switching to Room mode)
[*][b]Copy Settings[/b] - Copies the Atmosphere / Storage / Conduits scope toggles
[*][b]Inversion[/b] - When ControlledAutomation is loaded with inversion enabled, the invert checkbox appears on the Resource Sensor
[/list]

[h3]KIN Underground Conduit[/h3]
[list]
[*]Adds Atmosphere / Storage / Conduits scope controls
[*]Improves counting (including tile storage and conduit contents)
[*]Raises threshold max to 9,999,999 and removes hardcoded unit text
[*]Clears range visualizer correctly on deselect
[*]Supports Copy Settings and ControlledAutomation inversion
[/list]
Compatible with Berkay's Resource Sensor and ResourceSensorFIXED.

[h3]Free Resource Buildings[/h3]
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2839006500]Free Resource Buildings[/url]
[list]
[*]Fixes Free Energy Generator wattage slider to control real output
[*]Adds Power Sink (1x1 configurable power consumer, 0-40,000 W)
[/list]

[h3]Customize Plants[/h3]
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=1818145851]Customize Plants[/url]
[list]
[*]Applies max_age to Vine Branch (ovagro), which vanilla Customize Plants misses
[/list]

[h3]Duplicant Room Sensor[/h3]
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=1921058858]Duplicant Room Sensor[/url]
[list]
[*]Per-sensor Range Limit toggle and Range Input (1-64)
[*]Range-limited sensing respects walls/closed doors and stays room-bounded
[*]Compatible with Peter Han's ShowRange visualization
[*]Supports Copy Settings for range toggle/value
[/list]

[h3]KIN Underground Conduit[/h3]
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3347169088]KIN Underground Conduit[/url]
[list]
[*]Fixes Power Terminal / Logic Terminal InvalidCastException crash
[*]Enables Copy Settings for terminals, senders, and receivers
[/list]

[h2]How to Use[/h2]
[olist]
[*][b]Subscribe/Install[/b] - Enable the mod in the Mods menu
[*][b]Enable Target Mods[/b] - Enable the mods you want to control
[*][b]Open Options[/b] - Mod Options -> ControlledMods
[*][b]Configure[/b] - Adjust settings
[*][b]Restart[/b] - Restart game
[/olist]

[h3]Tips[/h3]
[list]
[*]Settings are grouped by target mod - expand each section to see available options
[*]Hover over options for detailed descriptions and default values
[*]Changes require a game restart to apply
[/list]

[h2]Compatibility[/h2]
[list]
[*][b]Oxygen Not Included[/b] - Build 700386+
[*][b]Mod API[/b] - Version 2
[*][b]DLC[/b] - Base game and all DLC
[*][b]Target Mods[/b] - Optional (safe if not installed)
[/list]

[h2]Performance[/h2]
[list]
[*]Conditional patching only
[*]No continuous polling loops
[/list]

[h2]Support & Issues[/h2]
Need help, found a bug, or have a suggestion? We're here to help!

[h3]Community[/h3]
[list]
[*][b]Discord[/b]: [url=https://discord.com/channels/1452947938304200861/1452947939927392398]Join our Discord server[/url] for discussions, questions, and community support
[*][b]GitHub Discussions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/discussions]Discuss on GitHub[/url] - share ideas, ask questions, or get help with modding
[/list]

[h3]Reporting Issues[/h3]
Found a bug or have a feature request? Please report it on GitHub using our issue templates:
[list]
[*][b]Bug Reports[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=bug_report.yml]Report a Bug[/url] - Use this for crashes, errors, or unexpected behavior
[*][b]Feature Requests[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=feature_request.yml]Suggest a Feature[/url] - Have an idea for a new feature or improvement?
[*][b]Questions[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=question.yml]Ask a Question[/url] - Need help understanding how something works?
[*][b]Other Issues[/b]: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues/new?template=other.yml]Other Issue[/url] - Something else that doesn't fit the above categories
[/list]

Please mention "ControlledMods" in your issue title or description.

[h2]My Workshop & Collections[/h2]
[list]
[*][url=https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140]My Workshop[/url] – All my ONI mods on Steam
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 235+ Ultimate Mods collection[/url] – 235+ tested, compatible mods for Oxygen Not Included
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653]The Controlled Series[/url] – Collection of Controlled mods
[/list]

[h2]Credits[/h2]
[list]
[*]Built using [url=https://github.com/peterhaneve/ONIMods]PLib[/url] by Peter Han
[*]Uses [url=https://github.com/pardeike/Harmony]Harmony[/url] for runtime patching
[/list]

[h2]Version History[/h2]
[list]
[*][b]1.3.0[/b]: Restart button on the main menu (above Quit) for quick restarts
[*][b]1.2.0[/b]: Resource Sensor – sidescreen with Atmosphere/Storage/Conduits scope checkboxes; counting for atmosphere, storage buildings, and conduits with category tag expansion; threshold max raised to 9,999,999 with units stripped; range visualizer clears on deselect; Copy Settings copies scope toggles; ControlledAutomation inversion support
[*][b]1.1.0[/b]: KIN Underground Conduit – Logic Terminal crash fix, Copy Settings for conduit terminals/senders/receivers
[*][b]1.0.0[/b]: Initial release
[/list]
