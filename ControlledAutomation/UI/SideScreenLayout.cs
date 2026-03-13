using UnityEngine;

namespace ControlledAutomation.UI
{
    /// <summary>
    /// Shared layout constants for ControlledAutomation sidescreens.
    /// Change these in one place to adjust checkbox styling across all sidescreens.
    /// </summary>
    public static class SideScreenLayout
    {
        /// <summary>Margin for sidescreen panels (left, right, top, bottom).</summary>
        public static readonly RectOffset PanelMargin = new RectOffset(8, 4, 4, 4);

        /// <summary>Margin for checkbox rows when embedded in a larger panel (e.g. TemperatureRange).</summary>
        public static readonly RectOffset CheckboxRowMargin = new RectOffset(8, 0, 4, 0);

        /// <summary>Size of the checkbox square to match game UI (e.g. "Rebuild when broken").</summary>
        public static readonly Vector2 CheckboxSize = new Vector2(26f, 26f);

        /// <summary>FlexSize to prevent checkbox stretching; keeps it left-aligned.</summary>
        public static readonly Vector2 CheckboxFlexSize = Vector2.zero;

        /// <summary>Text alignment for checkbox labels.</summary>
        public static readonly TextAnchor CheckboxTextAlignment = TextAnchor.MiddleLeft;

        /// <summary>Spacing between elements in checkbox panels.</summary>
        public const int CheckboxPanelSpacing = 4;
    }
}
