# Morning Exercise Mod

A mod for Oxygen Not Included that adds a Morning Exercise schedule block, allowing Duplicants to work out on Manual Exercisers and gain Athletics or Morale bonuses.

## Features

- **Morning Exercise Schedule Block**: Adds a new schedule block type that Duplicants can use for exercise time
- **Manual Exerciser Building**: New exercise-only building (separate from Manual Generators) available in the Medicine category
- **Warm Up Buff**: Regular Duplicants gain an Athletics bonus after exercising
- **Bionic Warm Up Buff**: Bionic Duplicants gain a Morale bonus instead of Athletics
- **Waiting Chore**: If no exercise equipment is available, Duplicants enter a waiting state instead of going idle
- **Configurable Options**: Customize exercise duration, buff duration, and bonus amounts
- **Separate Settings for Dupes and Boops**: Independent configuration for standard Duplicants ("Dupes") and Bionic Duplicants ("Boops")
- **Enable/Disable Exercise**: Toggle exercise on or off for each duplicant type independently
- **Experience Gain**: Optional permanent Athletics attribute experience gain (like Manual Generator)
- **Animation Variant Selection**: Choose between Standard, Variant A, or Variant B Manual Generator animations

## How It Works

1. Research **Medicine I** to unlock the Manual Exerciser building
2. Build **Manual Exercisers** in your base (found in the Medicine build category)
3. Add **Morning Exercise** blocks to your Duplicants' schedules
4. During exercise time, Duplicants will find an available Manual Exerciser
5. They exercise for the configured duration (default: 20 seconds)
6. After completing exercise, they receive the appropriate buff:
   - **Regular Duplicants ("Dupes")**: Warm Up buff (+3 Athletics by default)
   - **Bionic Duplicants ("Boops")**: Bionic Warm Up buff (+2 Morale by default)
7. The buff lasts for the configured duration:
   - **Dupes**: Default 6 minutes / 360 seconds (configurable)
   - **Boops**: Default 12 minutes / 720 seconds (configurable, longer due to their extended cycles)
8. If experience gain is enabled, Duplicants also gain permanent Athletics attribute experience (same as Manual Generator)
9. Duplicants can only exercise once per exercise block (prevents spam)

## Configuration

The mod includes configurable options accessible through the mod options menu:

### Dupes (Standard Duplicants)

- **Enable Exercise** (Default: Enabled)
  - Toggle to allow or prevent standard Duplicants from exercising
  - When disabled, standard Duplicants will not create exercise chores

- **Exercise Duration (seconds)** (Default: 20)
  - How long Duplicants exercise on the Manual Exerciser
  - Range: 5 to 120 seconds
  - Shorter durations allow more frequent buffs but less exercise time

- **Buff Duration (seconds)** (Default: 360)
  - How long the Warm Up buff lasts for standard Duplicants after exercising
  - Range: 60 to 999 seconds
  - Default: 6 minutes / 360 seconds

- **Athletics Bonus** (Default: 3)
  - The Athletics bonus granted by the Warm Up buff for regular Duplicants
  - Range: 1 to 10
  - Higher values make Duplicants move faster and work more efficiently

### Boops (Bionic Duplicants)

- **Enable Exercise** (Default: Enabled)
  - Toggle to allow or prevent Bionic Duplicants from exercising
  - When disabled, Bionic Duplicants will not create exercise chores

- **Buff Duration (seconds)** (Default: 720)
  - How long the Warm Up buff lasts for Bionic Duplicants after exercising
  - Range: 60 to 999 seconds
  - Default: 12 minutes / 720 seconds (longer due to extended bionic cycles)

- **Morale Bonus** (Default: 2)
  - The Morale bonus granted by the Warm Up buff for Bionic Duplicants
  - Range: 1 to 10
  - Higher values provide better stress management for bionic Duplicants

### Experience Settings

- **Enable Experience Gain** (Default: Enabled)
  - Whether exercising grants permanent Athletics attribute experience (like Manual Generator)
  - When enabled, Duplicants gain experience that accumulates in the background
  - Experience contributes to Athletics attribute level-ups

- **Experience Multiplier** (Default: 1)
  - Multiplier for Athletics experience gained per second of exercise
  - Range: 0 to 10
  - 1 = same rate as Manual Generator (1 experience per second)
  - Higher values grant experience faster

### Visual Settings

- **Animation Variant** (Default: Variant A)
  - Which animation variant to use for the Manual Exerciser building
  - Options: Standard, Variant A, or Variant B
  - Changes require a game restart to take effect

## Building Details

**Manual Exerciser**
- **Category**: Medicine
- **Research**: Medicine I
- **Size**: 2x2 tiles
- **Material**: All Metals
- **Function**: Exercise-only building (does not generate power)
- **Animation**: Uses Manual Generator animation (selectable variant: Standard, Variant A, or Variant B)

## Buff Details

**Warm Up** (Regular Duplicants / "Dupes")
- Grants Athletics bonus (configurable, default +3)
- Duration: Configurable (default 360 seconds / 6 minutes)
- Applied after completing exercise
- Only one exercise per schedule block
- Can be disabled via mod options

**Bionic Warm Up** (Bionic Duplicants / "Boops")
- Grants Morale bonus (configurable, default +2)
- Duration: Configurable (default 720 seconds / 12 minutes, longer due to extended cycles)
- Applied after completing exercise
- Only one exercise per schedule block
- Can be disabled via mod options

## Performance

This mod has **minimal performance impact**:

- **Efficient Equipment Lookup**: Uses static registry for fast Manual Exerciser lookups
- **Cached Checks**: Equipment availability checks are cached to avoid repeated lookups
- **ISim200ms Updates**: Monitor component runs 5 times per second (not every frame)
- **No Pathfinding Overhead**: Uses existing game chore system for pathfinding
- **One-Time Initialization**: Schedule blocks, effects, and chore types registered once at startup

The mod leverages existing game systems (chores, schedule blocks, effects) and adds minimal overhead during gameplay.

## Compatibility

- **Supported Content**: All DLCs (Base Game, Spaced Out!, The Frosty Planet Pack, The Bionic Booster Pack)
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

Please mention "Morning Exercise" in your issue title or description so we can identify it easily.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Developed for Oxygen Not Included
- Uses PLib by Peter Han
- Uses Harmony by pardeike

## Future Functionality

The following features are planned for future releases:

- **Custom Animation Variant**: A dedicated custom animation for the Manual Exerciser building, providing a unique visual appearance distinct from the Manual Generator. This would include custom sprites and animations specifically designed for the exercise machine, making it visually distinct while maintaining the same functionality.

## Version History

- **0.1.101**: Initial release
