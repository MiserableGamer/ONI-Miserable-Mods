[h1]Controlled Visuals[/h1]

Fixes a long-standing vanilla bug where items on conveyor rails sometimes render in front of drywall and other buildings. Conveyor contents now always draw behind walls as intended.

[h2]Features[/h2]
[list]
[*][b]Conveyor Items Behind Drywall[/b] - Items on rails consistently render behind drywall, tiles, and other buildings
[*][b]Optional Clean Edges Conversion[/b] - Ports and modernizes legacy CleanEdges behavior to clean up neutronium map edges
[*][b]One-Shot Reconvert[/b] - Optional "Reconvert on Next Save Load" toggle reapplies Clean Edges once, then auto-resets
[*][b]No Gameplay Impact (Conveyor Fix)[/b] - Conveyor rendering fix is visual-only; rail mechanics are unchanged
[*][b]Works Everywhere[/b] - Loaders, rails, and bridges; moving and stationary items
[*][b]Multi-temp displays[/b] - Shows K / F / C, with your Options chosen preference as the primary, and the remaining two in brackets
[/list]

[h2]How to Use[/h2]
[olist]
[*][b]Enable the Mod[/b] - Enable Controlled Visuals in your mod list
[*][b]Conveyor Fix[/b] - This visual fix is always active
[*][b]Clean Edges (Optional)[/b] - Enable "Enable Clean Edges" in Mod Options
[*][b]Tune Settings (Optional)[/b] - Set border size and abyssalite mass before conversion
[*][b]Reconvert (Optional)[/b] - Enable "Reconvert on Next Save Load" to apply new border settings once
[/olist]

[h3]Tips[/h3]
[list]
[*]Conveyor render-layer fix is always active
[*]Clean Edges is disabled by default and only runs when enabled
[*]Reconvert on Next Save Load is one-shot and resets to off after successful reconvert
[*]Works with all solid conveyor buildings (loaders, bridges, rails)
[/list]

[h2]Compatibility[/h2]
[list]
[*][b]Oxygen Not Included[/b] - Build 700386 or later
[*][b]Mod API[/b] - Version 2
[*][b]DLC Support[/b] - Works with base game and all DLC
[*][b]Other Mods[/b] - Compatible with most mods
[/list]

[h2]Performance[/h2]
[b]Minimal Performance Impact[/b]
[list]
[*][b]Per-Tick Layer Check[/b] - Only corrects render layer for items that need it
[*][b]No Gameplay Logic[/b] - Purely a visual fix; no simulation changes
[/list]

[h2]Future Updates[/h2]
[list]
[*]Additional visual fixes may be added in future updates
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

Please mention "ControlledVisuals" in your issue title or description.

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
[*]Clean Edges functionality is based on the original mod by [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2311196403]ParallaxMurderer[/url]
[/list]

[h2]Version History[/h2]
[list]
[*][b]1.2.0[/b]: Added multi-temp display
[*][b]1.1.0[/b]: Added Clean Edges functionality
[*][b]1.0.0[/b]: Initial release - Conveyor items behind drywall fix
[/list]
