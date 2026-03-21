# Controlled Morale

Beverage production and morale tuning for **Oxygen Not Included**: brew three mod liquids (**beer**, **wine**, and **spirits**) at the **Alcohol Brewery**, pipe them out, and serve them from the **Water Cooler** for Quality of Life and Athletics effects.

## Features

- **Alcohol Brewery** — Powered 3×3 **Refining** building: automatic production (no dupe operation), dual liquid inputs (**water** + **ethanol**), manual or conveyor delivery of **sucrose** and crop-specific ingredients, and **piped liquid output** for your drinks.
- **Three beverages** — **Beer** (sleet wheat), **wine** (bristle berry / ovagro fig when that DLC is active), and **spirits** (nosh bean seeds); each recipe has its own inputs and craft time.
- **Output handling** — Finished liquid stays in the building until a dupe **picks it up** (bottler-style interaction). If you **queue a different recipe** while old product is still inside, that product is **ejected** so the new batch can run (otherwise output would block the machine).
- **Water Cooler integration** — Extra drink options on the cooler; dupes get timed **morale (Quality of Life)** buffs with an **Athletics** tradeoff, tuned per drink tier.
- **Side screen** — Recipe ingredient tooltips account for **what is actually in the building’s storage** (pipes and manual delivery), not only colony inventory.

## How to Use

1. Unlock **Food Repurposing** (same tech as related food refining).
2. Build the **Alcohol Brewery** from the **Refining** menu (grouped near the ethanol chain).
3. Connect **water** and **ethanol** pipes, supply **sucrose** and the crop for your chosen recipe, and run **output** piping to storage or a **Water Cooler**.
4. Queue recipes on the building; when a batch finishes, have a dupe **pick up** the bottle from the brewery (or change recipe to eject leftover product to the floor).
5. On the **Water Cooler**, select the matching beverage type once the liquid is available.

### Tips

- Every recipe uses **ethanol** — plan production accordingly.
- **Athletics** penalties matter for digging and hauling; use scheduling or job priorities if needed.
- **DLC:** Wine uses **ovagro fig** when **The Frosty Planet Pack** content is available; otherwise **prickle fruit** (bristle berry).

## Installation

### Steam Workshop

Subscribe on the Steam Workshop and enable the mod in **Mods**.

### Manual

1. Download the [latest release](https://github.com/MiserableGamer/ONI-Miserable-Mods/releases).
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\local\ControlledMorale\`
3. Enable in the **Mods** menu.

## Compatibility

| | |
|---|---|
| **Game** | Oxygen Not Included — build **652719** or later (see `mod_info.yaml`) |
| **API** | Mod API **2** |
| **Content** | `supportedContent: ALL` — base game and DLC where vanilla ingredients exist |

## Performance

**Low impact** — Harmony patches at startup (database, localization, buildings, fabricator UI, Water Cooler); runtime behavior matches normal powered fabricators and piping.

## Support & Issues

Mention **Controlled Morale** in the title when reporting on [GitHub Issues](https://github.com/MiserableGamer/ONI-Miserable-Mods/issues).

## My Workshop & Collections

- [My Workshop](https://steamcommunity.com/id/miserablegamer/myworkshopfiles/?appid=457140)
- [ONI 235+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156)
- [The Controlled Series](https://steamcommunity.com/sharedfiles/filedetails/?id=3672308653)

## Credits

- [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han  
- [Harmony](https://github.com/pardeike/Harmony) for runtime patching  

## Version History

### 1.0.1

- Release crash fix

### 1.0.0

- Initial release

