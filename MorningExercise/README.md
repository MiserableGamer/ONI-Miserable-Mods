# Morning Exercise Mod

Adds a Morning Exercise schedule block and Manual Exerciser building, allowing Duplicants to work out and gain Athletics or Morale bonuses.

## Features

- **Morning Exercise Schedule Block** - New schedule block type for exercise time
- **Manual Exerciser Building** - Exercise-only building in the Medicine category (requires Medicine I research)
- **Warm Up Buff** - Regular Duplicants gain Athletics bonus after exercising (configurable, default +3)
- **Bionic Warm Up Buff** - Bionic Duplicants gain Morale bonus instead of Athletics (configurable, default +2)
- **Waiting Chore** - Duplicants wait for equipment instead of going idle when no exercisers are available
- **Configurable Options** - Customize exercise duration, buff duration, and bonus amounts
- **Separate Settings for Dupes and Boops** - Independent configuration for standard Duplicants ("Dupes") and Bionic Duplicants ("Boops")
- **Enable/Disable Exercise** - Toggle exercise on or off for each duplicant type independently
- **Experience Gain** - Optional permanent Athletics attribute experience gain (like Manual Generator)
- **Animation Variant Selection** - Choose between Standard, Variant A, or Variant B Manual Generator animations

## How It Works

1. Research **Medicine I** to unlock the Manual Exerciser
2. Build **Manual Exercisers** in your base (Medicine category)
3. Add **Morning Exercise** blocks to your Duplicants' schedules
4. During exercise time, Duplicants find an available Manual Exerciser
5. They exercise for the configured duration (default: 20 seconds)
6. After completing exercise, they receive the appropriate buff:
   - **Regular Duplicants ("Dupes")**: Warm Up buff (+3 Athletics by default, 6 minutes)
   - **Bionic Duplicants ("Boops")**: Bionic Warm Up buff (+2 Morale by default, 12 minutes)
7. If experience gain is enabled, Duplicants also gain permanent Athletics attribute experience (same as Manual Generator)
8. Duplicants can only exercise once per exercise block

## Configuration

Options are organized into sections: **Dupes**, **Boops**, **Experience Settings**, and **Visual Settings**.

### Dupes (Standard Duplicants)

- **Enable Exercise** (Default: Enabled) - Toggle to allow/prevent standard Duplicants from exercising
- **Exercise Duration (seconds)** (Default: 20)
  - Range: 5 to 120 seconds
- **Buff Duration (seconds)** (Default: 360)
  - Range: 60 to 999 seconds
  - Default: 6 minutes
- **Athletics Bonus** (Default: 3)
  - Range: 1 to 10

### Boops (Bionic Duplicants)

- **Enable Exercise** (Default: Enabled) - Toggle to allow/prevent Bionic Duplicants from exercising
- **Buff Duration (seconds)** (Default: 720)
  - Range: 60 to 999 seconds
  - Default: 12 minutes (longer due to extended bionic cycles)
- **Morale Bonus** (Default: 2)
  - Range: 1 to 10

### Experience Settings

- **Enable Experience Gain** (Default: Enabled)
  - Grants permanent Athletics attribute experience (like Manual Generator)
- **Experience Multiplier** (Default: 1)
  - Range: 0 to 10
  - 1 = same rate as Manual Generator (1 exp/second)

### Visual Settings

- **Animation Variant** (Default: Variant A)
  - Options: Standard, Variant A, or Variant B

## Building Details

**Manual Exerciser**
- Category: Medicine
- Research: Medicine I
- Size: 2x2 tiles
- Material: All Metals
- Function: Exercise-only (does not generate power)
- Animation: Selectable variant (Standard, Variant A, or Variant B)

## Performance

**Minimal Performance Impact**

## ⚠️ IMPORTANT WARNING ⚠️

**This mod is NOT game save safe!**

- **If you delete this mod, your save games will NO LONGER LOAD**
- **Before removing this mod, you MUST remove all Morning Exercise schedule blocks from all Duplicants' schedules**
- **Failure to remove the schedule blocks before uninstalling will result in a crash when trying to load the save file**

## Compatibility

- All DLCs supported (Base Game, Spaced Out!, The Frosty Planet Pack, The Bionic Booster Pack)
- Minimum Build: 700386
- API Version: 2

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

Please mention "Morning Exercise" in your issue title or description.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Developed for Oxygen Not Included
- Uses PLib by Peter Han
- Uses Harmony by pardeike

## Future Functionality

The following features are planned for future releases:

**Custom Animation Variant** - A dedicated custom animation for the Manual Exerciser building, providing a unique visual appearance distinct from the Manual Generator. This would include custom sprites and animations specifically designed for the exercise machine, making it visually distinct while maintaining the same functionality.

## Version History

- **1.0.2.127**: Initial release
- **1.0.1.103**: Code cleaning
- **1.0.2.125**: Dupes will now go to relax after exercising, or if they already are buffed, rather than just to idle
