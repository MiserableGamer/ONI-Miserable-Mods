using UnityEngine;
using PeterHan.PLib.UI;

namespace ControlledStorage.NoSweepZone
{
    public static class ICONS
    {
        private static Sprite tool_icon;
        public static Sprite TOOL_ICON_SPRITE
        {
            get { if (tool_icon == null) loadIcons(); return tool_icon; }
            set { tool_icon = value; }
        }

        private static Sprite setVisualizer;
        public static Sprite SET_VISUALIZER_SPRITE
        {
            get { if (setVisualizer == null) loadIcons(); return setVisualizer; }
            set { setVisualizer = value; }
        }

        private static Sprite cancelVisualizer;
        public static Sprite CANCEL_VISUALIZER_SPRITE
        {
            get { if (cancelVisualizer == null) loadIcons(); return cancelVisualizer; }
            set { cancelVisualizer = value; }
        }

        private static void loadIcons()
        {
            TOOL_ICON_SPRITE = PUIUtils.LoadSprite("ControlledStorage.images.tool_icon.png");
            TOOL_ICON_SPRITE.name = UI.STRINGS.TOOL_ICON;
            SET_VISUALIZER_SPRITE = PUIUtils.LoadSprite("ControlledStorage.images.visualizer_set.png");
            SET_VISUALIZER_SPRITE.name = UI.STRINGS.SET_VISUALIZER_ICON;
            CANCEL_VISUALIZER_SPRITE = PUIUtils.LoadSprite("ControlledStorage.images.visualizer_cancel.png");
            CANCEL_VISUALIZER_SPRITE.name = UI.STRINGS.CANCEL_VISUALIZER_ICON;
        }
    }
}
