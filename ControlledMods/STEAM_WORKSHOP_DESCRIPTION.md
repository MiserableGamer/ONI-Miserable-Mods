[h1]ControlledMods[/h1]

Customize and override settings from other mods via an in-game options menu. Take control of building capacities, limits, and other configurable values without editing config files manually.

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
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3347169088]KIN Underground Conduit[/url]
[list]
[*][b]Fix Power Terminal and Logic Terminal crash[/b] - Prevents InvalidCastException when a logic wire is built in the same cell as a Power Terminal or Logic Terminal (optional, on by default)
[*][b]Copy Settings[/b] - Enables the vanilla Copy Settings tool for conduit terminals, senders, and receivers; channel is copied when you paste settings
[/list]

[i]More mods will be added in future updates![/i]

[h2]How to Use[/h2]
[olist]
[*][b]Subscribe/Install[/b] - Enable the mod in the Mods menu
[*][b]Enable Target Mods[/b] - Enable the mod(s) you want to customize (e.g., KIN Underground Conduit)
[*][b]Open Options[/b] - Click "Mod Options" from the main menu and find "ControlledMods"
[*][b]Configure[/b] - Adjust settings as desired
[*][b]Restart[/b] - Restart the game for changes to take effect
[/olist]

[h3]Tips[/h3]
[list]
[*]Settings are grouped by target mod - expand each section to see available options
[*]Hover over options for detailed descriptions and default values
[*]Changes require a game restart to apply
[/list]

[h2]Compatibility[/h2]
[list]
[*][b]Oxygen Not Included[/b] - Build 700386 or later
[*][b]Mod API[/b] - Version 2
[*][b]DLC Support[/b] - Works with base game and all DLC (including Bionic Booster Pack)
[*][b]Target Mods[/b] - Does not require target mods to be installed; safely ignored if not present
[/list]

[h2]Performance[/h2]
[b]Minimal Performance Impact[/b]
[list]
[*][b]Conditional patches[/b] - Patches apply only when the target mod is detected and the relevant option is enabled
[*][b]No polling[/b] - No per-frame or sim tick work; logic runs only on paste (Copy Settings) or when the game triggers the patched events
[/list]

[h2]Future Updates[/h2]
[list]
[*]Support for additional mods (suggest your favorites!)
[*]More building customization options
[*]Additional configurable values per mod
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
