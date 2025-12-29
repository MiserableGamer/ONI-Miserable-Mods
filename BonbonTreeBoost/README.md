# Bonbon Tree Boost Mod

A mod for Oxygen Not Included that allows you to configure the Bonbon Tree (Space Tree) Nectar production, growth speed, and fertilizer consumption to suit your gameplay needs.

## Features

- **Separate Wild and Domestic Settings**: Configure wild and domesticated trees independently
- **Configurable Nectar Production**: Adjust multipliers for Space Tree sugar water (nectar) production (1.0x to 10.0x)
- **Independent Growth Rate Control**: Control trunk and branch growth rates separately for wild and domesticated trees (0.1x to 10.0x)
- **Fertilizer Consumption Adjustment**: Modify how much Snow fertilizer domesticated trees consume (0.0x to 10.0x)
- **Production Balance Control**: Choose whether wild or domestic trees produce more, or if they produce equally
- **Branch Harvesting Control**: Enable or disable wood harvesting from branches
- **Fully Configurable**: All settings are adjustable through the mod options menu
- **Works with Existing Saves**: Changes apply to existing trees, new trees, and all saves

## Configuration Options

The mod includes configurable options organized into groups, accessible through the mod options menu:

### Wild Trees

1. **Nectar Production Multiplier** (Default: 2.0x)
   - Multiplier for wild Space Tree sugar water production
   - Range: 1.0 to 10.0
   - Higher values = more nectar production

2. **Trunk Growth Rate** (Default: 1.0x)
   - Multiplier for wild Space Tree trunk growth rate
   - Range: 0.1 to 10.0
   - Higher values = trunk grows faster

3. **Branch Growth Rate** (Default: 1.0x)
   - Multiplier for wild Space Tree branch growth rate
   - Range: 0.1 to 10.0
   - Higher values = branches grow faster

### Domesticated Trees

1. **Nectar Production Multiplier** (Default: 2.0x)
   - Multiplier for domesticated Space Tree sugar water production
   - Range: 1.0 to 10.0
   - Higher values = more nectar production

2. **Trunk Growth Rate** (Default: 1.0x)
   - Multiplier for domesticated Space Tree trunk growth rate
   - Range: 0.1 to 10.0
   - Higher values = trunk grows faster

3. **Branch Growth Rate** (Default: 1.0x)
   - Multiplier for domesticated Space Tree branch growth rate
   - Range: 0.1 to 10.0
   - Higher values = branches grow faster

4. **Fertilizer Consumption Rate** (Default: 1.0x)
   - Multiplier for Snow fertilizer consumption (domesticated trees only)
   - Range: 0.0 to 10.0
   - Set to 0.0 to disable fertilizer consumption entirely
   - Higher values = trees consume more fertilizer

### Production Balance

1. **Wild vs Domestic Production Balance** (Default: 3 - Wild Advantage)
   - Controls the production relationship between wild and domesticated trees
   - Options:
     - **1 = Domestic Advantage**: Domestic trees produce more than wild trees
     - **2 = Equal Production**: Wild and domestic trees produce the same rate
     - **3 = Wild Advantage**: Wild trees produce more than domestic trees
   - Combined with the Production Advantage Multiplier to determine the exact ratio

2. **Production Advantage Multiplier** (Default: 2.0x)
   - When balance is set to Domestic Advantage (1) or Wild Advantage (3), this controls how much more one produces than the other
   - Range: 0.5 to 10.0
   - Example: 2.0 means the advantaged type produces 2x more than the other

### Branch Settings

1. **Allow Branch Harvesting** (Default: Enabled)
   - If disabled, branches cannot be harvested for wood
   - Dupes will only harvest nectar from the trunk
   - If enabled, dupes can harvest both nectar and wood from branches

## How It Works

The mod modifies the Space Tree's mechanics by adjusting:
- **Production Duration**: Lower duration means more frequent production cycles (affects nectar production rate)
- **Growth Rates**: Modifies the trunk and branch growth rates independently for wild and domesticated trees
- **Fertilizer Consumption Rate**: Directly modifies the mass consumption rate of Snow fertilizer (domesticated trees only)
- **Production Balance**: Allows you to control whether wild or domesticated trees are more productive, or if they produce equally
- **Branch Harvesting**: Controls whether branches can be harvested for wood or only nectar is collected

Changes are applied when trees are created, affecting both existing and new trees in your save.

## Compatibility

- **Supported Content**: All DLCs (Base Game, Spaced Out!, The Frosty Planet Pack, The Bionic Booster Pack)
- **Minimum Build**: 700386
- **API Version**: 2
- Uses PLib for options management
- **Restart Required**: Game restart is required after changing mod options

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

Please mention "Bonbon Tree Boost" in your issue title or description so we can identify it easily.

## Mod Collection

This mod is part of the [ONI 200+ Ultimate Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3613749156) on Steam Workshop, featuring over 200 tested and compatible mods for Oxygen Not Included.

## Credits

- Developed for Oxygen Not Included
- Uses PLib by Peter Han
- Uses Harmony by pardeike

## Version History

- **1.0.1.4**: Bug fixes and code cleaning
- **1.0.1.11**: Initial release
- **2.0.0.66**: Complete rewrite of the code, new options added
  - Separate configuration options for wild and domesticated trees
  - Independent control over trunk and branch growth rates
  - Production balance system to control wild vs domestic production relationships
  - Production advantage multiplier for fine-tuning balance
  - Branch harvesting control option

