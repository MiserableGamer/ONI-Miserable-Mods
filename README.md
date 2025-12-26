# ONI Miserable Mods

A collection of quality-of-life and enhancement mods for **Oxygen Not Included** by Klei Entertainment.

## 📦 Available Mods

### 🏃 Morning Exercise
Adds a Morning Exercise schedule block and Manual Exerciser building, allowing Duplicants to work out and gain Athletics or Morale bonuses.

[📖 Read More](MorningExercise/README.md) | [🔗 Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3627507304)

### 📋 Copy Materials Tool
Extends the "Copy Settings" tool to also copy construction materials between buildings of the same type, with full support for conduits and bridges.

[📖 Read More](CopyMaterials/README.md) | [🔗 Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3627507304)

### 📦 Empty Storage
Adds an "Empty Storage" button to storage buildings, allowing you to quickly drop all contents either immediately or via duplicant labor.

[📖 Read More](EmptyStorage/README.md) | [🔗 Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3627507304)

### 🦾 Longer Arms
Increases the reach distance for Duplicants, allowing them to interact with buildings and objects from further away.

[📖 Read More](LongerArms/README.md) | [🔗 Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3627507304)

### 🔍 Debug Fog of War
A debug tool that allows you to reveal the entire map, useful for testing and exploring.

[📖 Read More](DebugFogOfWar/README.md) | [🔗 Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3627507304)

### 🌳 Bonbon Tree Boost
Enhances the Bonbon Tree with configurable production boosts and other improvements.

[📖 Read More](BonbonTreeBoost/README.md) | [🔗 Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3627507304)

### 🧱 Threshold Fixed
Fixes threshold walls to properly detect creature confinement, allowing them to function as intended.

[📖 Read More](ThresholdFixed/README.md) | [🔗 Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3627507304)

## 🚀 Quick Start

### Installation via Steam Workshop (Recommended)

1. Browse to the mod's Steam Workshop page
2. Click "Subscribe"
3. Launch Oxygen Not Included
4. Enable the mod in the Mods menu

### Manual Installation

1. Download the latest release from the mod's GitHub page
2. Extract to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\[ModName]\`
3. Ensure the folder contains the required files (`.dll`, `mod_info.yaml`, `mod.yaml`)
4. Launch the game and enable the mod in the Mods menu

## 📋 Requirements

- **Oxygen Not Included**: Build 700386 or later
- **Mod API**: Version 2
- **Dependencies**: Automatically included (PLib, Harmony)

All mods support:
- Base Game
- Spaced Out! DLC
- The Frosty Planet Pack DLC
- The Bionic Booster Pack DLC

## 🤝 Support & Community

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

When reporting issues, please mention the specific mod name in your issue title or description.

## 🎮 Mod Collection

All mods are part of the [**ONI 200+ Ultimate Mods collection**](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## 🛠️ Building from Source

### Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.8
- Oxygen Not Included game files (for references)

### Required References

- `0Harmony.dll` (Harmony library)
- `Assembly-CSharp.dll` (from ONI installation)
- `Assembly-CSharp-firstpass.dll` (from ONI installation)
- `PLib.dll` (PLib library)
- `UnityEngine.dll` and related modules (from ONI installation)
- `KMod.dll` (from ONI installation)

### Build Steps

1. Clone or download this repository
2. Open the solution file (`ONI Miserable Mods.slnx`) in Visual Studio
3. Ensure all references point to valid ONI DLLs
4. Build the project (version will auto-increment in `mod_info.yaml`)
5. Copy the output DLL and mod files to your mods directory

## 📄 License

See [LICENSE.txt](LICENSE.txt) for license information.

## 🙏 Credits

- **PLib**: [Peter Han](https://github.com/peterhaneve/ONIMods) - Essential modding library
- **Harmony**: [pardeike](https://github.com/pardeike/Harmony) - Runtime patching library
- **Klei Entertainment**: For creating Oxygen Not Included and providing modding support

## 📝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## ⚠️ Disclaimer

These mods are provided as-is. While we strive to maintain compatibility and stability, use at your own risk. Always back up your save files before using mods.

---

**Enjoy your enhanced Oxygen Not Included experience!** 🎉

