using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ControlledStorage.NoSweepZone
{
    public class NoSweepZoneTool : DragTool
    {
        private NoSweepZoneMode selectedMode = NoSweepZoneMode.Set;
        private SpriteRenderer toolRenderer;

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();

            visualizer = new GameObject("NoSweepZoneVisualizer");
            visualizer.SetActive(false);

            var offsetObject = new GameObject();
            var spriteRenderer = offsetObject.AddComponent<SpriteRenderer>();
            toolRenderer = spriteRenderer;
            spriteRenderer.color = NoSweepZoneCommonProps.TOOL_COLOR;
            spriteRenderer.sprite = ICONS.SET_VISUALIZER_SPRITE;

            offsetObject.transform.SetParent(visualizer.transform);
            offsetObject.transform.localPosition = new Vector3(0, Grid.HalfCellSizeInMeters);
            var sprite = spriteRenderer.sprite;
            offsetObject.transform.localScale = new Vector3(
                Grid.CellSizeInMeters / (sprite.texture.width / sprite.pixelsPerUnit),
                Grid.CellSizeInMeters / (sprite.texture.height / sprite.pixelsPerUnit)
            );

            offsetObject.SetLayerRecursively(LayerMask.NameToLayer("Overlay"));
            visualizer.transform.SetParent(transform);

            var areaVisualizerField = AccessTools.Field(typeof(DragTool), "areaVisualizer");
            var areaVisualizerSpriteRendererField = AccessTools.Field(typeof(DragTool), "areaVisualizerSpriteRenderer");

            var aV = Util.KInstantiate((GameObject)AccessTools.Field(typeof(DeconstructTool), "areaVisualizer").GetValue(DeconstructTool.Instance));
            aV.SetActive(false);
            aV.name = "NoSweepZoneAreaVisualizer";

            var aVSpriteRenderer = aV.GetComponent<SpriteRenderer>();
            areaVisualizerSpriteRendererField.SetValue(this, aVSpriteRenderer);
            aV.transform.SetParent(transform);
            aVSpriteRenderer.color = NoSweepZoneCommonProps.TOOL_COLOR;
            aVSpriteRenderer.material.color = NoSweepZoneCommonProps.TOOL_COLOR;
            areaVisualizerField.SetValue(this, aV);

            gameObject.AddComponent<NoSweepZoneHoverCard>();
        }

        public override void OnActivateTool()
        {
            base.OnActivateTool();
            SetMode(Mode.Box);
            OverlayScreen.Instance.ToggleOverlay(newMode: NoSweepZoneOverlay.ID);

            var menu = NoSweepZoneToolMenu.Instance;
            if (menu != null && !menu.HasOptions)
                menu.PopulateMenu();
            menu?.ShowMenu();
            if (menu != null)
                menu.OnSettingChanged += OnToolSettingChange;
        }

        public override void OnDeactivateTool(InterfaceTool new_tool)
        {
            base.OnDeactivateTool(new_tool);
            OverlayScreen.Instance.ToggleOverlay(newMode: OverlayModes.None.ID);

            var menu = NoSweepZoneToolMenu.Instance;
            if (menu != null)
            {
                menu.OnSettingChanged -= OnToolSettingChange;
                menu.HideMenu();
            }
        }

        private void OnToolSettingChange(NoSweepZoneMode toolMode)
        {
            selectedMode = toolMode;
            toolRenderer.sprite = selectedMode == NoSweepZoneMode.Set
                ? ICONS.SET_VISUALIZER_SPRITE
                : ICONS.CANCEL_VISUALIZER_SPRITE;
        }

        public override void OnDragComplete(Vector3 cursorDown, Vector3 cursorUp)
        {
            base.OnDragComplete(cursorDown, cursorUp);

            if (!hasFocus || NoSweepZoneSaveState.Instance == null) return;

            Grid.PosToXY(cursorDown, out int x0, out int y0);
            Grid.PosToXY(cursorUp, out int x1, out int y1);

            if (x0 > x1) Util.Swap(ref x0, ref x1);
            if (y0 > y1) Util.Swap(ref y0, ref y1);

            var state = NoSweepZoneSaveState.Instance.NoSweep;

            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    int cell = Grid.XYToCell(x, y);
                    // Don't use Grid.IsVisible — it excludes fog/unexplored cells, so items there would never be in the zone and would get picked up.
                    if (Grid.IsValidCell(cell) && Grid.Element[cell].id != SimHashes.Unobtanium)
                    {
                        if (selectedMode == NoSweepZoneMode.Set)
                            state.AddCell(cell);
                        else
                            state.RemoveCell(cell);
                    }
                }
            }

            if (selectedMode == NoSweepZoneMode.Set)
                NoSweepZoneChoreInvalidation.InvalidateFetchChoresInZone();

            NoSweepZoneChoreInvalidation.RefreshFetchabilityInArea(x0, y0, x1, y1);
        }
    }
}
