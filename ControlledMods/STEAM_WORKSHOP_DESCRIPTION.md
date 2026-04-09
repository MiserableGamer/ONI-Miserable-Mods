[h1]ControlledMods[/h1]

Compatible with: Resource Sensor, Free Resource Buildings, Customize Plants, Duplicant Room Sensor, Darkness Not Excluded (Relit), Signs Tags and Ribbons, KIN Underground Conduit.

Configure and improve behavior from supported ONI mods through one in-game options menu.

[h2]Features[/h2]
[list]
[*]Direct Workshop links for supported mods in Mod Options
[*]Restart button in the main menu and pause menu
[*]Safe mod detection (patches apply only when target mods are present)
[/list]

[h2]Supported Mods[/h2]

[h3]Resource Sensor[/h3]
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2911545239]Resource Sensor[/url]
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
[/list]

[h3]Darkness Not Excluded (Relit)[/h3]
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3609476592]Darkness Not Excluded (Relit)[/url]
[list]
[*]Helps reduce light bleed through solid tiles in darkness visuals
[/list]

[h3]KIN Underground Conduit[/h3]
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3347169088]KIN Underground Conduit[/url]
[list]
[*]Fixes Power Terminal / Logic Terminal InvalidCastException crash
[*]Enables Copy Settings for terminals, senders, and receivers
[/list]

[h3]Signs, Tags and Ribbons[/h3]
Steam Workshop: [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2883096049][Vanilla + DLC] Signs, Tags and Ribbons[/url]
[list]
[*]Adds new variants
[*]Friendly tooltips on variant buttons
[/list]

[h2]How to Use[/h2]
[olist]
[*][b]Subscribe/Install[/b] - Enable the mod in the Mods menu
[*][b]Enable Target Mods[/b] - Enable the mods you want to control
[*][b]Open Mods[/b] - ControlledMods -> Mod Options -> enable required patches
[/olist]

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
[*][b]1.14.1[/b]: Bugfix for PLib merge
[*][b]1.14.0[/b]: Added support for Delivery Temperature Limit [fixed]; prevents SideScreen crashes
[*][b]1.13.1[/b]: Added a crash handler and filter for channels
[*][b]1.12.1[/b]: Fixed Resource Sensor bug; added pause menu restart with auto resume save file facility
[*][b]1.11.0[/b]: Performance improvements
[*][b]1.10.2[/b]: More fixes for Duplicant Room Sensor
[*][b]1.10.1[/b]: Added more Small Element Tags to Signs Tags and Ribbons
[*][b]1.10.0[/b]: Added support for Signs Tags and Ribbons
[*][b]1.9.0[/b]: Added support for Darkness Not Excluded (relit)
[*][b]1.8.1[/b]: Fixed Line of Sight ranging of Duplicant Room Sensor
[*][b]1.8.0[/b]: Pholith's Duplicant Room Sensor support added
[*][b]1.7.0[/b]: Added VineBranch max_age compatibility for Customizable Plants mod
[*][b]1.6.0[/b]: Added colour visualisations to Logic Terminals for logic state
[*][b]1.5.1[/b]: Fixed kanim loading bug
[*][b]1.5.0[/b]: Free Resource Buildings support added – PowerBox output slider now functions; Power Sink building added
[*][b]1.4.0[/b]: Resource Sensor – Storage counting now includes tile-based storage
[*][b]1.3.0[/b]: Restart button on the main menu for quick restarts
[*][b]1.2.0[/b]: Resource Sensor support added
[*][b]1.1.0[/b]: KIN Underground Conduit support added
[*][b]1.0.0[/b]: Initial release
[/list]
