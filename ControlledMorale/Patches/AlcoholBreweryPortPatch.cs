using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace ControlledMorale
{
    // Colours the brewery's water port blue (#0063ff) and ethanol port lime-green (#00ff6a),
    // and injects a "Liquid Input: Water/Ethanol" hover tooltip when in the liquid conduit overlay.
    internal static class AlcoholBreweryPortPatch
    {
        static readonly Color WaterConnected      = new Color(0.000f, 0.388f, 1.000f); // #0063ff
        static readonly Color WaterDisconnected   = new Color(0.000f, 0.200f, 0.600f);
        static readonly Color EthanolConnected    = new Color(0.000f, 1.000f, 0.416f); // #00ff6a
        static readonly Color EthanolDisconnected = new Color(0.000f, 0.500f, 0.200f);

        static readonly CellOffset WaterOffset   = new CellOffset(0, 1);
        static readonly CellOffset EthanolOffset = new CellOffset(0, 2);

        // ── Port colouring ───────────────────────────────────────────────────────

        [HarmonyPatch(typeof(BuildingCellVisualizer), "DefinePorts")]
        public static class Patch_DefinePorts
        {
            public static void Postfix(BuildingCellVisualizer __instance)
            {
                var building = Traverse.Create(__instance).Field<Building>("building").Value;
                if (building?.Def?.PrefabID != AlcoholBreweryConfig.ID) return;

                // ports is 'protected List<PortEntry>' — walk backwards to find the last LiquidIn
                // entry, which is the secondary (ethanol) port added by ConduitSecondaryInput.
                var ports = Traverse.Create(__instance).Field("ports").GetValue() as System.Collections.IList;
                if (ports == null) return;

                int firstLiquid = -1, lastLiquid = -1;
                for (int i = 0; i < ports.Count; i++)
                {
                    if (Traverse.Create(ports[i]).Field<EntityCellVisualizer.Ports>("type").Value
                        == EntityCellVisualizer.Ports.LiquidIn)
                    {
                        if (firstLiquid == -1) firstLiquid = i;
                        lastLiquid = i;
                    }
                }

                if (firstLiquid >= 0 && firstLiquid != lastLiquid)
                {
                    Traverse.Create(ports[firstLiquid]).Field("connectedTint").SetValue(WaterConnected);
                    Traverse.Create(ports[firstLiquid]).Field("disconnectedTint").SetValue(WaterDisconnected);
                    Traverse.Create(ports[lastLiquid]).Field("connectedTint").SetValue(EthanolConnected);
                    Traverse.Create(ports[lastLiquid]).Field("disconnectedTint").SetValue(EthanolDisconnected);
                }
            }
        }

        // ── Port hover tooltips ──────────────────────────────────────────────────

        static string        s_tipText;
        static Sprite        s_tipSprite;
        static Color         s_tipColor;
        static TextStyleSetting s_tipStyle;

        [HarmonyPatch(typeof(SelectToolHoverTextCard), nameof(SelectToolHoverTextCard.UpdateHoverElements))]
        public static class Patch_UpdateHoverElements
        {
            public static void Prefix(SelectToolHoverTextCard __instance, List<KSelectable> __hoverObjects)
            {
                s_tipText = null;

                if (OverlayScreen.Instance?.GetMode() != OverlayModes.LiquidConduits.ID) return;

                int mouseCell = Grid.PosToCell(Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos()));
                if (!Grid.IsValidCell(mouseCell)) return;

                if (s_tipStyle == null)
                {
                    var src = __instance.Styles_BodyText.Standard;
                    s_tipStyle = new TextStyleSetting
                    {
                        sdfFont            = src.sdfFont,
                        fontSize           = src.fontSize,
                        textColor          = src.textColor,
                        style              = src.style,
                        enableWordWrapping = src.enableWordWrapping
                    };
                }

                s_tipSprite = BuildingCellVisualizerResources.Instance().liquidInputIcon;

                foreach (EntityCellVisualizer vis in Components.EntityCellVisualizers.Items)
                {
                    if (!(vis is BuildingCellVisualizer bcv)) continue;

                    Building bldg = bcv.building;
                    if (bldg?.Def?.PrefabID != AlcoholBreweryConfig.ID) continue;

                    var   rot      = bldg.GetComponent<Rotatable>();
                    int   baseCell = Grid.PosToCell(bldg.gameObject);

                    int waterCell   = Grid.OffsetCell(baseCell, rot != null ? rot.GetRotatedCellOffset(WaterOffset)   : WaterOffset);
                    int ethanolCell = Grid.OffsetCell(baseCell, rot != null ? rot.GetRotatedCellOffset(EthanolOffset) : EthanolOffset);

                    if (mouseCell == waterCell)
                    {
                        s_tipText  = "Liquid Input: Water";
                        s_tipColor = WaterConnected;
                        break;
                    }
                    if (mouseCell == ethanolCell)
                    {
                        s_tipText  = "Liquid Input: Ethanol";
                        s_tipColor = EthanolConnected;
                        break;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.EndDrawing))]
        public static class Patch_EndDrawing
        {
            public static void Prefix(HoverTextDrawer __instance)
            {
                if (s_tipText == null) return;

                __instance.BeginShadowBar();
                __instance.DrawIcon(s_tipSprite, s_tipColor, 20);
                __instance.DrawText(s_tipText, s_tipStyle);
                __instance.EndShadowBar();

                s_tipText = null;
            }
        }
    }
}
