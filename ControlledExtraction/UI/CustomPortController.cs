using System.Collections.Generic;
using UnityEngine;

namespace ControlledExtraction.UI
{
    // Manages all custom ports on a building
    [SkipSaveFileSerialization]
    public class CustomPortController : KMonoBehaviour
    {
        private List<CustomPortDisplay> ports = new List<CustomPortDisplay>();
        private HashedString lastMode = OverlayModes.None.ID;

        public void AddPort(PortDisplayInfo info)
        {
            var display = gameObject.AddComponent<CustomPortDisplay>();
            display.Setup(info);
            ports.Add(display);
        }

        public void DrawPorts(BuildingCellVisualizer visualizer, HashedString mode)
        {
            bool isNewMode = mode != lastMode;

            if (isNewMode)
            {
                // Hide ports from previous overlay
                foreach (var port in ports)
                {
                    if (port.MatchesOverlay(lastMode))
                    {
                        port.DisableIcon();
                    }
                }
                lastMode = mode;
            }

            // Draw ports for current overlay
            foreach (var port in ports)
            {
                if (port.MatchesOverlay(mode))
                {
                    port.Draw(visualizer, isNewMode);
                }
            }
        }

        // Static helper to add a port to any GameObject
        public static void AddPortToBuilding(GameObject go, PortDisplayInfo info)
        {
            // Ensure building has cell visualizer
            go.AddOrGet<BuildingCellVisualizer>();

            // Add controller and port
            var controller = go.AddOrGet<CustomPortController>();
            controller.AddPort(info);
        }
    }
}
