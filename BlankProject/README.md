# BlankProject - ONI Mod Template

This is a simple template project for creating Oxygen Not Included mods. It provides a minimal starting point with just the essential files.

## Getting Started

1. **Rename the project**: 
   - Rename `BlankProject` to your mod name throughout all files
   - Update namespaces from `BlankProject` to your mod name
   - Update the `.csproj` file with your new project name
   - Update `mod_info.yaml` and `mod.yaml` with your mod details

2. **Main Entry Point** (`BlankProject.cs`):
   - This is where `OnLoad` is called
   - Initialize PLib here
   - Add your mod initialization code

3. **Add your code**:
   - Create additional `.cs` files as needed in the root directory
   - Organize into subfolders (Components/, Buildings/, Patches/, Options/, etc.) as your mod grows
   - See MorningExercise as an example of a more complex mod structure

## Example: Adding Options

If you need mod options, add PLib.Options:

```csharp
using PeterHan.PLib.Options;

// In OnLoad:
new POptions().RegisterOptions(this, typeof(Options.YourOptionsClass));
```

## Example: Adding Harmony Patches

```csharp
using HarmonyLib;

[HarmonyPatch(typeof(SomeGameClass), nameof(SomeGameClass.SomeMethod))]
public static class SomeGameClass_SomeMethod_Patch
{
    internal static void Postfix()
    {
        // Your patch code here
    }
}
```

## Notes

- This template keeps things simple - just one root `.cs` file to start
- Add folders and files as your mod grows in complexity
- See MorningExercise for an example of a fully-featured mod structure
- This template is not uploaded to GitHub (excluded via .gitignore)

