# Copy Materials (ONI Mod)

A QoL tool that mirrors "Copy Settings" but applies construction materials:
- Click a source building → press "Copy Materials" in side screen.
- Click or drag over target buildings of the same type.
- The game queues deconstruction, then places blueprints with the source material.

## Build

- Requires .NET Framework (3.5/4.x depending on your ONI mod setup) and Harmony.
- Reference ONI assemblies (from the game install):
  - Assembly-CSharp.dll
  - UnityEngine.dll (+ modules as needed)
  - Klei DLLs (e.g., Klei.dll, KMod.dll)
- Compile into `CopyMaterials.dll`.

## Install

- Create a folder: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\CopyMaterials\`
- Place:
  - `mod_info.json`
  - `CopyMaterials.dll`
  - `Localization\en.json`
- Launch the game and enable the mod.

## Notes & TODO

- Material override path varies by ONI build; `BuildTool.Place` + a `MaterialOverride` may need adapting if your build doesn't expose this helper. If unavailable, patch the build placer or intercept `Game.Instance` build queue to inject `SelectedElements` for the blueprint.
- The selection tool uses a simple cell sweep; optimize if needed.
- Consider batching deconstruct/rebuild operations and providing an undo.
- Add filtering UI (e.g., only apply to locked/open doors, exclude certain states).