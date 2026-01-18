using UnityEngine;

namespace ControlledExtraction.UI
{
    // Configuration for a custom port display
    public class PortDisplayInfo
    {
        public ConduitType ConduitType { get; }
        public CellOffset Offset { get; }
        public bool IsInput { get; }
        public SimHashes Element { get; }
        public Color Color { get; }

        public PortDisplayInfo(ConduitType type, CellOffset offset, bool isInput, SimHashes element, Color? color = null)
        {
            ConduitType = type;
            Offset = offset;
            IsInput = isInput;
            Element = element;
            
            // Default colors based on element type if not specified
            if (color.HasValue)
            {
                Color = color.Value;
            }
            else
            {
                Color = GetDefaultColor(element, type);
            }
        }

        private static Color GetDefaultColor(SimHashes element, ConduitType type)
        {
            // Try to get element color
            var elementObj = ElementLoader.FindElementByHash(element);
            if (elementObj != null && elementObj.substance != null)
            {
                return elementObj.substance.colour;
            }
            return GetConduitTypeColor(type);
        }

        private static Color GetConduitTypeColor(ConduitType type)
        {
            if (type == ConduitType.Gas)
                return new Color(0.5f, 0.8f, 1f);      // Light blue
            else if (type == ConduitType.Liquid)
                return new Color(0.3f, 0.8f, 0.5f);    // Green
            else if (type == ConduitType.Solid)
                return new Color(0.8f, 0.6f, 0.3f);    // Orange/brown
            else
                return Color.white;
        }
    }
}
