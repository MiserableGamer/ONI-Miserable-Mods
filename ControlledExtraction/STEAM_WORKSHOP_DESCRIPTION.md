[h1]Controlled Extraction[/h1]

Per-building control over Oil Well extraction rates, output ports for various buildings, and Ice Kettle enhancements including meltable type selection.

[h2]Features[/h2]
[list]
[*][b]Adjustable Water Input Rate[/b] - Each Oil Well Cap gets a slider to control water consumption from 0.01 kg/s to 100 kg/s
[*][b]Proportional Scaling[/b] - Oil output and gas pressure buildup all scale with the water input rate
[*][b]Per-Building Control[/b] - Each Oil Well can have different settings - throttle some, boost others
[*][b]Copy Settings Support[/b] - Use the game's Copy Settings tool to quickly apply rates across multiple wells
[*][b]Output Ports[/b] - Add gas, liquid, and solid conveyor ports to various buildings
[*][b]Ice Kettle Enhancements[/b] - CO2 gas output, liquid output, configurable meltable types with per-building multi-select, and smart storage management
[/list]

[h2]How to Use[/h2]

[h3]Oil Well Cap[/h3]
[olist]
[*][b]Build an Oil Well Cap[/b] on an Oil Reservoir (as normal)
[*][b]Select the Building[/b] - The slider is now "Water Input Rate"
[*][b]Adjust Extraction Speed[/b] - Set from 0.01 to 100 kg/s
[*][b]Copy Settings[/b] - Use the game's Copy Settings tool to apply to other wells
[/olist]

[h3]Ice Kettle[/h3]
[olist]
[*][b]Enable Features[/b] in mod options (Configure Meltables button, CO2/Liquid ports)
[*][b]Build Ice Kettles[/b] as normal
[*][b]Select Meltable Types[/b] - Use the per-building sidescreen to choose which elements each kettle accepts. Types are organised into collapsible Vanilla/DLC and Modded groups, with group-level checkboxes for quick selection
[*][b]Connect Pipes[/b] - Optionally connect gas/liquid pipes to the output ports
[/olist]

[h3]Tips[/h3]
[list]
[*]Higher extraction rates mean faster gas pressure buildup - enable the Gas Output Port for automatic venting
[*]Standard liquid pipes max at 10 kg/s
[*]Ice Kettle meltable types are configured globally in mod options (with Select All / Select None buttons), then per-building via the sidescreen
[*]Modded elements from other mods (e.g., Ronivan's Legacy) are auto-detected and available via the "Enable modded meltables" toggle
[*]When multiple meltable types are selected, the kettle shares its storage space proportionally across all types and automatically melts whichever has the most material first
[*]Deselect all types on a kettle to pause it - no deliveries will be made and the kettle will sit idle until a type is re-enabled
[/list]

[h2]Extraction Rates[/h2]
[table]
[tr][th]Water Input[/th][th]Oil Output[/th][th]Notes[/th][/tr]
[tr][td]0.01 kg/s[/td][td]0.033 kg/s[/td][td]Minimal - conserve water[/td][/tr]
[tr][td]1 kg/s[/td][td]3.33 kg/s[/td][td]Vanilla default[/td][/tr]
[tr][td]10 kg/s[/td][td]33.33 kg/s[/td][td]10x speed (pipe limit)[/td][/tr]
[tr][td]100 kg/s[/td][td]333.33 kg/s[/td][td]Maximum chaos![/td][/tr]
[/table]

[h2]Building Output Ports[/h2]

Add output ports to pipe away gases, liquids, and solids from various buildings!

[h3]Oil Well Cap[/h3]
[list]
[*]Gas Output (Natural Gas)
[*]Liquid Output (Crude Oil)
[/list]

[h3]Oil Refinery[/h3]
[list]
[*]Gas Output (Methane)
[/list]

[h3]Ethanol Distillery[/h3]
[list]
[*]Gas Output (CO2)
[*]Solid Output (Polluted Dirt)
[*]Solid Input (Lumber)
[/list]

[h3]Ice Kettle[/h3]
[list]
[*]Gas Output (CO2)
[*]Liquid Output (any melted liquid)
[*]Configurable meltable types - per-building multi-select sidescreen with collapsible groups
[*]Smart storage management - shares capacity across all selected meltable types
[*]Can be paused by deselecting all types in the sidescreen
[*]Auto-detects modded elements from other mods
[/list]

[h3]Generators[/h3]
[table]
[tr][th]Building[/th][th]CO2 Port[/th][th]Polluted Water Port[/th][/tr]
[tr][td]Coal Generator[/td][td]Yes[/td][td]-[/td][/tr]
[tr][td]Wood Burner[/td][td]Yes[/td][td]-[/td][/tr]
[tr][td]Petroleum Generator[/td][td]Yes[/td][td]Yes[/td][/tr]
[tr][td]Natural Gas Generator[/td][td]- (vanilla)[/td][td]Yes[/td][/tr]
[/table]

All ports default to OFF and require game restart when changed.

[h2]Mod Options[/h2]

Configure via [b]Options > Mods > Controlled Extraction[/b]:

[h3]Oil Well Cap[/h3]
[list]
[*]Default Water Rate, Min/Max slider values
[*]Backpressure Threshold (default 75%)
[*]Max Gas/Oil Storage
[*]Gas and Liquid Output Ports
[/list]

[h3]Ice Kettle[/h3]
[list]
[*]CO2 Gas Output Port
[*]Liquid Output Port
[*]Configure Meltables (opens dialog for vanilla/DLC per-element toggles, modded meltables toggle, and Select All / Select None buttons)
[/list]

Each building category has its own options section for enabling output ports.

[h2]Compatibility[/h2]
[list]
[*][b]Oxygen Not Included[/b] - Build 700386 or later
[*][b]Mod API[/b] - Version 2
[*][b]DLC Support[/b] - Works with base game and all DLC (including Bionic Booster Pack)
[*][b]Ronivan's Legacy[/b] - Fully compatible - scales rates regardless of gas element, auto-detects modded meltable elements
[*][b]Piped Everything[/b] - Works alongside for even more options
[*][b]Conveyor capacity mods[/b] - Auto-detects increased capacity
[/list]

[h2]Performance[/h2]
[b]Minimal Performance Impact[/b]
[list]
[*][b]Event-Driven[/b] - No polling or continuous monitoring; patches only fire on building config/spawn
[*][b]Lightweight Controllers[/b] - Output controllers use standard conduit tick updates
[*][b]Lazy Discovery[/b] - Ice Kettle meltable elements are discovered once on first use, not every frame
[/list]

[h2]Future Updates[/h2]
[list]
[*]Additional building support
[*]More Ice Kettle customization options
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

Please mention "Controlled Extraction" in your issue title or description.

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
[*][b]1.1.0[/b]: Ice Kettle enhancements - CO2 output, liquid output, configurable meltable types with per-building multi-select and modded element support
[*][b]1.0.3[/b]: Added more building output ports
[*][b]1.0.2[/b]: Bugfix for Ethanol Distillery ports not working
[*][b]1.0.1[/b]: Bugfixes
[*][b]1.0.0[/b]: Initial release
[/list]
