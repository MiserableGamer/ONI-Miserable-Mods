# Empty Storage Mod

A mod for Oxygen Not Included that adds an "Empty Storage" button to storage buildings, allowing you to quickly drop all contents either immediately or via duplicant labor.

## Features

- **Empty Storage Button**: Adds a convenient "Empty Storage" button to all storage buildings (gas, liquid, and solid storage)
- **Flexible Emptying Options**:
  - **Immediate Emptying**: Drop all contents instantly with a single click
  - **Duplicant Labor**: Create a task for duplicants to empty the storage over time
- **Skill Requirements**: Optionally require duplicants to have the appropriate skills:
  - **Plumbing** skill for gas and liquid storage
  - **Tidy** skill (Improved Strength) for solid storage
- **Configurable Work Time**: 
  - Option to make emptying instant or time-based
  - Adjustable work time per 100kg (0.1 to 10 seconds)
- **Bionic Dupe Support**: Bionic dupes with the Tidying Booster can empty liquid, gas, and solid storage even without the required skills

## Configuration

The mod includes several configurable options accessible through the mod options menu:

1. **Immediate Emptying** (Default: Enabled)
   - When enabled, storage contents are dropped immediately
   - When disabled, a duplicant task is created to empty the storage
   - When enabled, all other options are disabled

2. **Require Skills** (Default: Enabled)
   - When enabled, duplicants need the appropriate skill to empty storage
   - Only applies when Immediate Emptying is disabled
   - Gas/Liquid storage requires Plumbing skill
   - Solid storage requires Tidy skill (Improved Strength)

3. **Use Work Time** (Default: Enabled)
   - When enabled, emptying takes time based on the mass stored
   - When disabled, emptying is instant (but still requires duplicant labor)
   - Only applies when Immediate Emptying is disabled

4. **Work Time per 100kg** (Default: 1.0 seconds)
   - The amount of time (in seconds) it takes to empty 100kg of stored material
   - Range: 0.1 to 10 seconds
   - Only applies when Use Work Time is enabled

## How to Use

1. Select any storage building (gas, liquid, or solid storage)
2. Click the "Empty Storage" button in the building's info panel
3. If Immediate Emptying is enabled, contents will drop instantly
4. If Immediate Emptying is disabled, a duplicant will be assigned to empty the storage
5. You can cancel the emptying task using the "Empty Storage" button again (toggles on/off)

## Bionic Dupe Support

Bionic dupes with the **Tidying Booster** installed can empty liquid, gas, and solid storage even without the required skills. This allows bionic dupes to perform storage emptying tasks that would normally require the Plumbing skill (for gas/liquid storage) or the Tidy skill (for solid storage).

## Compatibility

- **Supported Content**: All DLCs
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

Please mention "Empty Storage" in your issue title or description so we can identify it easily.

## Future Features

- **Universal Cancel Tool Support**: Integration with the game's built-in cancel tool to allow canceling emptying tasks using the standard cancel tool, in addition to the mod's own cancel button.

## Credits

- Developed for Oxygen Not Included
- Uses PLib by Peter Han

## Version History

- **0.1.82**: Initial release

