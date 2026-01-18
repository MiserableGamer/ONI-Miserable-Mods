[h1]Controlled Extraction[/h1]

Per-building control over Oil Well extraction rates and output ports for various buildings. Finally, you decide how fast to pump!

[h2]Features[/h2]

[list]
[*] [b]Adjustable Water Input Rate[/b]: Each Oil Well Cap gets a slider to control water consumption from 0.01 kg/s to 100 kg/s
[*] [b]Proportional Scaling[/b]: Oil output and gas pressure buildup all scale with the water input rate
[*] [b]Per-Building Control[/b]: Each Oil Well can have different settings - throttle some, boost others
[*] [b]Copy Settings Support[/b]: Use the game's Copy Settings tool to quickly apply rates across multiple wells
[*] [b]Output Ports[/b]: Add gas, liquid, and solid conveyor ports to various buildings
[/list]

[h2]Extraction Rates[/h2]

[table]
[tr][th]Water Input[/th][th]Oil Output[/th][th]Notes[/th][/tr]
[tr][td]0.01 kg/s[/td][td]0.033 kg/s[/td][td]Minimal - conserve water[/td][/tr]
[tr][td]1 kg/s[/td][td]3.33 kg/s[/td][td]Vanilla default[/td][/tr]
[tr][td]10 kg/s[/td][td]33.33 kg/s[/td][td]10x speed (pipe limit)[/td][/tr]
[tr][td]100 kg/s[/td][td]333.33 kg/s[/td][td]Maximum chaos![/td][/tr]
[/table]

[i]Note: Standard liquid pipes max at 10 kg/s.[/i]

[h2]⚠️ Important: Gas Pressure Warning[/h2]

[b]Everything scales with extraction rate![/b] Higher water input = faster gas pressure buildup.

[table]
[tr][th]Water Rate[/th][th]Gas Buildup[/th][th]Venting Needed[/th][/tr]
[tr][td]1 kg/s[/td][td]Normal[/td][td]Occasional[/td][/tr]
[tr][td]10 kg/s[/td][td]10x faster[/td][td]Very frequent[/td][/tr]
[tr][td]100 kg/s[/td][td]100x faster[/td][td]Constant![/td][/tr]
[/table]

At high extraction rates, duplicants may spend all their time venting pressure!

[b]Solutions:[/b]
[list]
[*] Enable [b]"Add Gas Output Port"[/b] in mod options (requires restart) - built-in automatic venting!
[*] Increase "Max Gas Storage" to reduce venting frequency
[*] Or use [url=https://steamcommunity.com/workshop/filedetails/?id=2058745508]Piped Everything[/url] for more advanced piping
[/list]

[h2]Building Output Ports[/h2]

Add output ports to pipe away gases, liquids, and solids from various buildings!

[h3]Oil Well Cap[/h3]
[list]
[*] Gas Output (Natural Gas)
[*] Liquid Output (Crude Oil)
[/list]

[h3]Oil Refinery[/h3]
[list]
[*] Gas Output (Methane)
[/list]

[h3]Ethanol Distillery[/h3]
[list]
[*] Gas Output (CO2)
[*] Solid Output (Polluted Dirt)
[*] Solid Input (Lumber)
[/list]

[h3]Generators[/h3]
[table]
[tr][th]Building[/th][th]CO2 Port[/th][th]Polluted Water Port[/th][/tr]
[tr][td]Coal Generator[/td][td]✅[/td][td]-[/td][/tr]
[tr][td]Wood Burner[/td][td]✅[/td][td]-[/td][/tr]
[tr][td]Petroleum Generator[/td][td]✅[/td][td]✅[/td][/tr]
[/table]

All ports default to OFF and require game restart when changed.

[h2]Mod Options[/h2]

Configure via [b]Options > Mods > Controlled Extraction[/b]:

[b]Oil Well Cap:[/b]
[list]
[*] Default Water Rate (for new wells)
[*] Minimum/Maximum slider values
[*] Backpressure Threshold (when dupes vent, default 75%)
[*] Max Gas Storage (default 80 kg - increase for high extraction rates!)
[*] Max Oil Storage (default 50 kg - increase if oil backs up)
[*] Add Gas Output Port - automatic venting!
[*] Add Liquid Output Port - direct oil extraction!
[/list]

Each building category has its own options section for enabling output ports.

[h2]Compatibility[/h2]

[list]
[*] ✅ [b]Ronivan's Legacy[/b]: Fully compatible - scales rates regardless of gas element
[*] ✅ [b]Piped Everything[/b]: Works alongside for even more options
[*] ✅ Base Game
[*] ✅ Spaced Out! DLC
[*] ✅ All DLCs and content packs
[/list]

[h2]How to Use[/h2]

1. Build an Oil Well Cap on an Oil Reservoir (as normal)
2. Select the building
3. The slider (previously "Backpressure Release Threshold") is now [b]Water Input Rate[/b]
4. Adjust to your preferred extraction speed (0.01 to 100 kg/s)
5. Use Copy Settings to apply to other wells

[i]Note: The original backpressure slider is replaced. Duplicants will still depressurize at the configured threshold.[/i]

[h2]Version History[/h2]

[b]1.0.1.35[/b]
[list]
[*] Bugfixes
[/list]

[b]1.0.0.22[/b]
[list]
[*] Initial release
[/list]

[h2]Source Code[/h2]

This mod is open source: [url=https://github.com/MiserableGamer/ONI-Miserable-Mods]GitHub Repository[/url]

[h2]Support[/h2]

Found a bug? Have a suggestion? Please report on [url=https://github.com/MiserableGamer/ONI-Miserable-Mods/issues]GitHub Issues[/url] or leave a comment below.

[h2]Other Mods[/h2]

Check out my other quality-of-life mods in the [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156]ONI 200+ Ultimate Mods Collection[/url]!
