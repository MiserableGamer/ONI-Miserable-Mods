using System.Collections.Generic;

namespace ControlledStorage.NoSweepZone
{
    public sealed class NoSweepZoneHoverCard : HoverTextConfiguration
    {
        public NoSweepZoneHoverCard()
        {
            ToolName = UI.STRINGS.TOOL_HOVER_CARD_TITLE;
        }

        public override void UpdateHoverElements(List<KSelectable> hoveredObjects)
        {
            var screenInstance = HoverTextScreen.Instance;
            var drawer = screenInstance.BeginDrawing();
            drawer.BeginShadowBar();

            DrawTitle(screenInstance, drawer);
            drawer.NewLine();

            drawer.DrawIcon(screenInstance.GetSprite("icon_mouse_left"), 20);
            drawer.DrawText("Drag", Styles_Instruction.Standard);
            drawer.AddIndent(8);

            drawer.DrawIcon(screenInstance.GetSprite("icon_mouse_right"), 20);
            drawer.DrawText("Back", Styles_Instruction.Standard);

            drawer.EndShadowBar();
            drawer.EndDrawing();
        }
    }
}
