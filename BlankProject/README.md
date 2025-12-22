# BlankProject - ONI Mod Template

This is a template project for creating Oxygen Not Included mods with a clean, organized structure.

## Version

- **0.1.3**: Current version

## Project Structure

```
BlankProject/
├── BlankProject.cs              # Main mod entry point (UserMod2)
├── Components/                   # Game components (KMonoBehaviour implementations)
│   └── ExampleComponent.cs      # Example component template
├── Buildings/                    # Building configurations (IBuildingConfig implementations)
│   └── ExampleBuildingConfig.cs # Example building template
├── Patches/                      # Harmony patches
│   └── BlankProjectPatches.cs   # All Harmony patches (static class)
├── Options/                      # Mod options (PLib options)
│   └── BlankProjectOptions.cs   # Mod configuration options
├── Strings/                      # Localization strings
│   └── BlankProjectStrings.cs  # Localized strings
└── Properties/                   # Assembly info
    └── AssemblyInfo.cs
```

## Getting Started

1. **Rename the project**: 
   - Rename `BlankProject` to your mod name throughout all files
   - Update namespaces from `BlankProject` to your mod name
   - Update the `.csproj` file with your new project name

2. **Main Entry Point** (`BlankProject.cs`):
   - This is where `OnLoad` is called
   - Initialize PLib and options here
   - Set up any global state

3. **Patches** (`Patches/BlankProjectPatches.cs`):
   - Add all your Harmony patches here
   - Keep it as a static class
   - Store shared state as static properties

4. **Options** (`Options/BlankProjectOptions.cs`):
   - Define your mod's configurable options
   - Uses PLib options system
   - Shared config location enabled by default

5. **Components** (`Components/`):
   - Add your game components here
   - Components that extend `KMonoBehaviour` or other game classes

6. **Buildings** (`Buildings/`):
   - Add your building configurations here
   - Implement `IBuildingConfig` for new buildings

7. **Strings** (`Strings/`):
   - Add localization strings here
   - Register them in `Localization.Initialize` patch

## Example Usage

### Adding a Harmony Patch

In `Patches/BlankProjectPatches.cs`:

```csharp
[HarmonyPatch(typeof(SomeGameClass), nameof(SomeGameClass.SomeMethod))]
public static class SomeGameClass_SomeMethod_Patch
{
    internal static void Postfix()
    {
        // Your patch code here
    }
}
```

### Adding a Mod Option

In `Options/BlankProjectOptions.cs`:

```csharp
[Option("My Option", "Description of my option")]
[Limit(0, 100)]
public int MyOption { get; set; } = 50;
```

### Adding a Component

Create a new file in `Components/`:

```csharp
public class MyComponent : KMonoBehaviour
{
    protected override void OnSpawn()
    {
        base.OnSpawn();
        // Initialize
    }
}
```

## Notes

- The main entry point follows the conventional ONI mod structure
- All patches are in a static class separate from the main mod class
- Files are organized by purpose for better maintainability
- Example files can be deleted or modified as needed

