# File Organization Instructions

The project structure has been updated to organize files into folders. Please move the following files:

## Files to Move:

### Components/ (Game Components)
- `ExerciseMonitor.cs` → `Components/ExerciseMonitor.cs`
- `ExerciseWorkable.cs` → `Components/ExerciseWorkable.cs`

### Buildings/ (Building Configurations)
- `ManualExerciserConfig.cs` → `Buildings/ManualExerciserConfig.cs`

### Patches/ (Harmony Patches)
- `MorningExercisePatches.cs` → `Patches/MorningExercisePatches.cs`

### Options/ (Mod Options)
- `MorningExerciseOptions.cs` → `Options/MorningExerciseOptions.cs`

### Strings/ (Localization)
- `MorningExerciseStrings.cs` → `Strings/MorningExerciseStrings.cs`

## How to Move:

**In Visual Studio:**
1. Right-click each file in Solution Explorer
2. Select "Cut"
3. Right-click the destination folder
4. Select "Paste"

Visual Studio will automatically update the `.csproj` file with the new paths.

**Or manually:**
1. Create the folders: `Components`, `Buildings`, `Patches`, `Options`, `Strings`
2. Move the files using Windows Explorer
3. The `.csproj` file has already been updated with the new paths

## Folder Structure:

```
MorningExercise/
├── Components/          # Game components (ExerciseMonitor, ExerciseWorkable)
├── Buildings/          # Building configurations (ManualExerciserConfig)
├── Patches/            # Harmony patches (MorningExercisePatches)
├── Options/            # Mod options (MorningExerciseOptions)
├── Strings/            # Localization strings (MorningExerciseStrings)
├── Properties/         # Assembly info (already organized)
└── anim/               # Animation assets (already organized)
```

