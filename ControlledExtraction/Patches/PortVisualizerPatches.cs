// Port visualization is handled automatically by the game through ISecondaryOutput interface.
// ConduitSecondaryOutput already implements ISecondaryOutput, so ports are visualized automatically.
// This file is kept as a placeholder in case custom visualization is needed in the future.

namespace ControlledExtraction.Patches
{
    // No patches needed - game handles port visualization through BuildingCellVisualizer.DefinePorts()
    // which automatically detects ISecondaryInput and ISecondaryOutput components.
}
