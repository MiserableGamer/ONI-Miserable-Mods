# GitHub Releases

This repository uses GitHub Actions to automatically create **individual releases per mod** when tags are pushed.

## How to Create a Release

### Create Release via Git Tag

Each mod gets its own release. To create a release for a specific mod:

1. Create and push a tag with the mod name and version:
   ```bash
   # Format: ModName-v1.0.0 or vModName-1.0.0
   git tag BonbonTreeBoost-v1.0.0
   git push origin BonbonTreeBoost-v1.0.0
   ```

2. The GitHub Actions workflow will automatically:
   - Detect the tag push
   - Parse the mod name from the tag
   - Build only that specific mod
   - Create a release with the mod's zip file attached

### Tag Naming Formats

The workflow supports these tag formats:
- `ModName-v1.0.0` (recommended)
- `vModName-1.0.0`
- `ModName-1.0.0`

Examples:
- `BonbonTreeBoost-v1.0.0`
- `CopyMaterials-v2.1.3`
- `vMorningExercise-1.5.0`
- `EmptyStorage-1.0.1`

**Important:** The tag **must** include the mod name. Tags with only a version (like `v1.0.0`) will be rejected.

## Supported Mods

The workflow will build and release these mods:
- **BonbonTreeBoost**
- **CopyMaterials**
- **DebugFogOfWar**
- **EmptyStorage**
- **LongerArms**
- **MorningExercise**
- **ThresholdFixed**

**Excluded mods** (will not create releases):
- BlankProject (template)
- CommonLib (shared library)
- ResourceLimpet (work in progress)

## Release Contents

Each release includes a **single zip file** that contains a folder structure ready for local installation:

**Zip Structure:**
```
ModName-v1.0.0.zip
  └── ModName/
      ├── ModName.dll (or CopyMaterialsTool.dll for CopyMaterials)
      ├── PLib.dll (if required)
      ├── CommonLib.dll (if required, e.g., ThresholdFixed)
      ├── mod.yaml
      ├── mod_info.yaml
      ├── preview.png (if available)
      └── anim/ (if present, with full folder structure)
```

**Installation:**
1. Download the zip file
2. Extract it to: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\`
3. The extracted `ModName/` folder should be at: `...\mods\Local\ModName\`
4. Launch the game and enable the mod

The zip file is created in the project's `release/` folder and contains everything needed to run the mod locally without Steam Workshop.

## Workflow

1. **Push code updates** - Normal commits don't create releases
2. **Create a tag** - Only when you want to release a specific mod
3. **Workflow runs** - Builds and packages that mod
4. **Release created** - GitHub release with the mod zip file

This allows you to:
- Push code updates without creating releases
- Control which mods get released independently
- Release different mods at different times
- Keep development work separate from releases

## Manual Release Process

If you need to create a release manually:

1. Build the mod in Release configuration
2. Create a zip file containing:
   - All files from the mod's folder (excluding source files)
   - The DLL from `bin/Release/`
   - Any required dependencies (PLib.dll, CommonLib.dll)
   - The `anim/` folder if present
3. Go to GitHub Releases page
4. Click "Draft a new release"
5. Create a new tag or select existing
6. Upload the zip file
7. Publish the release

