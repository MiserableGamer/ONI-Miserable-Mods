using System.Collections.Generic;
using UnityEngine;

namespace ControlledStorage.NoSweepZone
{
    public class NoSweepZoneOverlay : OverlayModes.Mode
    {
        public static readonly HashedString ID = new HashedString("NOSWEEPZONES");

        private static readonly List<LegendEntry> legendEntries = new List<LegendEntry>
        {
            new LegendEntry("No-Sweep Zone", "", NoSweepZoneCommonProps.NO_SWEEP_COLOR)
        };

        private static readonly int cameraLayerMask = LayerMask.GetMask("MaskedOverlay", "MaskedOverlayBG");
        private static readonly int selectionMask = LayerMask.GetMask("MaskedOverlay");

        public NoSweepZoneOverlay()
        {
        }

        public override Dictionary<string, ToolParameterMenu.ToggleState> CreateDefaultFilters()
        {
            return new Dictionary<string, ToolParameterMenu.ToggleState>
            {
                { "NoSweepZone", ToolParameterMenu.ToggleState.On }
            };
        }

        internal static void SetupOverlay()
        {
            // No overlay filters needed for single-state overlay
        }

        internal static Color GetColor(SimDebugView _, int cell)
        {
            if (NoSweepZoneSaveState.Instance == null) return Color.black;
            return NoSweepZoneSaveState.Instance.NoSweep.ContainsCell(cell)
                ? NoSweepZoneCommonProps.NO_SWEEP_COLOR
                : Color.black;
        }

        public override void Disable()
        {
            CameraController.Instance.ToggleColouredOverlayView(false);
            Camera.main.cullingMask &= ~cameraLayerMask;
            SelectTool.Instance.ClearLayerMask();
            base.Disable();
        }

        public override void Enable()
        {
            base.Enable();
            CameraController.Instance.ToggleColouredOverlayView(true);
            Camera.main.cullingMask |= cameraLayerMask;
            SelectTool.Instance.SetLayerMask(selectionMask);
        }

        public override List<LegendEntry> GetCustomLegendData() => legendEntries;

        public override HashedString ViewMode() => ID;

        public override string GetSoundName() => "Temperature";
    }
}
