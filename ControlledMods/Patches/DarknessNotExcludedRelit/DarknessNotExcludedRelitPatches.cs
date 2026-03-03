using System;
using HarmonyLib;
using UnityEngine;
using ControlledMods.ModDetection;
using ControlledMods.Options;

namespace ControlledMods.Patches.DarknessNotExcludedRelit
{
    public static class DarknessNotExcludedRelitPatches
    {
        private const int MeshTransmissionPercent = 50;
        private const int AirflowTransmissionPercent = 50;
        private const int PneumaticDoorTransmissionPercent = 50;
        private static int _minImpliedLuxCutoff = 1;

        public static void ApplyPatches(Harmony harmony)
        {
            if (!DarknessNotExcludedRelitDetection.Loaded)
                return;

            var opts = ControlledModsOptions.Instance;
            if (!opts.EnableDarknessImpliedLightOcclusionFix)
                return;

            _minImpliedLuxCutoff = Mathf.Max(0, opts.DarknessMinImpliedLuxCutoff);

            var behaviorType = AccessTools.TypeByName("DarknessNotIncluded.Darkness.Behavior");
            var actualOrImpliedLightLevel = AccessTools.Method(behaviorType, "ActualOrImpliedLightLevel", new[] { typeof(int) });
            if (actualOrImpliedLightLevel != null)
            {
                harmony.Patch(
                    actualOrImpliedLightLevel,
                    prefix: new HarmonyMethod(typeof(DarknessBehavior_ActualOrImpliedLightLevel_Patch), nameof(DarknessBehavior_ActualOrImpliedLightLevel_Patch.Prefix))
                );
                ControlledModsMod.Log("Darkness Not Excluded Relit compatibility patches applied");
            }
            else
            {
                ControlledModsMod.LogWarning("Could not patch DarknessNotIncluded.Darkness.Behavior.ActualOrImpliedLightLevel");
            }
        }

        private static float CellTransmission(int cell)
        {
            if (!Grid.IsValidCell(cell))
                return 0f;

            if (TryGetBuilding(cell, out var building))
            {
                var id = GetPrefabId(building);
                if (id.IndexOf("MeshTile", StringComparison.OrdinalIgnoreCase) >= 0)
                    return MeshTransmissionPercent / 100f;
                if (id.IndexOf("AirflowTile", StringComparison.OrdinalIgnoreCase) >= 0
                    || id.IndexOf("GasPermeableMembrane", StringComparison.OrdinalIgnoreCase) >= 0)
                    return AirflowTransmissionPercent / 100f;
                if (id.IndexOf("PneumaticDoor", StringComparison.OrdinalIgnoreCase) >= 0
                    || id.IndexOf("DoorInternal", StringComparison.OrdinalIgnoreCase) >= 0
                    || string.Equals(id, "Door", StringComparison.OrdinalIgnoreCase))
                    return PneumaticDoorTransmissionPercent / 100f;
            }

            if (Grid.IsSolidCell(cell))
                return 0f;

            if (Grid.HasDoor[cell] && TryGetBuilding(cell, out var doorBuilding))
            {
                var id = GetPrefabId(doorBuilding);
                if (id.IndexOf("PneumaticDoor", StringComparison.OrdinalIgnoreCase) >= 0
                    || id.IndexOf("DoorInternal", StringComparison.OrdinalIgnoreCase) >= 0
                    || string.Equals(id, "Door", StringComparison.OrdinalIgnoreCase))
                    return PneumaticDoorTransmissionPercent / 100f;
                return 0f;
            }

            return 1f;
        }

        private static bool TryGetBuilding(int cell, out KMonoBehaviour building)
        {
            building = null;
            if (!Grid.IsValidCell(cell))
                return false;

            var go = Grid.Objects[cell, (int)ObjectLayer.Building]
                ?? Grid.Objects[cell, (int)ObjectLayer.FoundationTile]
                ?? Grid.Objects[cell, (int)ObjectLayer.PlasticTile]
                ?? Grid.Objects[cell, (int)ObjectLayer.ReplacementTile];
            if (go == null)
                return false;

            building = go.GetComponent<KMonoBehaviour>();
            return building != null;
        }

        private static string GetPrefabId(KMonoBehaviour building)
        {
            if (building == null)
                return string.Empty;

            try
            {
                var id = building.GetComponent<KPrefabID>();
                if (id != null)
                {
                    string tagName = id.PrefabTag.Name;
                    if (!string.IsNullOrEmpty(tagName))
                        return tagName;
                }
            }
            catch
            {
            }

            try
            {
                var def = building.GetComponent<Building>();
                if (def != null && def.Def != null && !string.IsNullOrEmpty(def.Def.PrefabID))
                    return def.Def.PrefabID;
            }
            catch
            {
            }

            return building.name ?? string.Empty;
        }

        private static float PathTransmission(int fromCell, int toCell)
        {
            if (!Grid.IsValidCell(fromCell) || !Grid.IsValidCell(toCell))
                return 0f;
            if (fromCell == toCell)
                return 1f;

            Grid.CellToXY(fromCell, out int x0, out int y0);
            Grid.CellToXY(toCell, out int x1, out int y1);

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int x = x0;
            int y = y0;
            float transmission = 1f;

            while (!(x == x1 && y == y1))
            {
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }

                int stepCell = Grid.XYToCell(x, y);
                if (!Grid.IsValidCell(stepCell))
                    return 0f;

                transmission *= CellTransmission(stepCell);
                if (transmission <= 0f)
                    return 0f;
            }

            return Mathf.Clamp01(transmission);
        }

        private static void ConsiderLux(ref float maxLux, int targetCell, int sourceCell, float distanceWeight)
        {
            if (!Grid.IsValidCell(sourceCell))
                return;

            int sourceLux = Grid.LightIntensity[sourceCell];
            if (sourceLux <= 0)
                return;

            float transmission = PathTransmission(targetCell, sourceCell);
            if (transmission <= 0f)
                return;

            float impliedLux = sourceLux * Mathf.Clamp01(distanceWeight) * transmission;
            if (impliedLux > maxLux)
                maxLux = impliedLux;
        }

        public static class DarknessBehavior_ActualOrImpliedLightLevel_Patch
        {
            public static bool Prefix(int cell, ref int __result)
            {
                if (!Grid.IsValidCell(cell))
                {
                    __result = 0;
                    return false;
                }

                int cellLux = Grid.LightIntensity[cell];
                if (cellLux > 0)
                {
                    __result = cellLux;
                    return false;
                }

                float nearbyLux = 0f;
                ConsiderLux(ref nearbyLux, cell, Grid.CellAbove(cell), 0.75f);
                ConsiderLux(ref nearbyLux, cell, Grid.CellRight(cell), 0.75f);
                ConsiderLux(ref nearbyLux, cell, Grid.CellBelow(cell), 0.75f);
                ConsiderLux(ref nearbyLux, cell, Grid.CellLeft(cell), 0.75f);
                if (nearbyLux > 0f)
                {
                    __result = Mathf.FloorToInt(nearbyLux);
                    if (__result < _minImpliedLuxCutoff)
                        __result = 0;
                    return false;
                }

                float midLux = 0f;
                ConsiderLux(ref midLux, cell, Grid.CellUpRight(cell), 0.5f);
                ConsiderLux(ref midLux, cell, Grid.CellDownRight(cell), 0.5f);
                ConsiderLux(ref midLux, cell, Grid.CellDownLeft(cell), 0.5f);
                ConsiderLux(ref midLux, cell, Grid.CellUpLeft(cell), 0.5f);
                ConsiderLux(ref midLux, cell, Grid.CellAbove(Grid.CellAbove(cell)), 0.5f);
                ConsiderLux(ref midLux, cell, Grid.CellRight(Grid.CellRight(cell)), 0.5f);
                ConsiderLux(ref midLux, cell, Grid.CellBelow(Grid.CellBelow(cell)), 0.5f);
                ConsiderLux(ref midLux, cell, Grid.CellLeft(Grid.CellLeft(cell)), 0.5f);
                if (midLux > 0f)
                {
                    __result = Mathf.FloorToInt(midLux);
                    if (__result < _minImpliedLuxCutoff)
                        __result = 0;
                    return false;
                }

                float farLux = 0f;
                ConsiderLux(ref farLux, cell, Grid.CellUpRight(Grid.CellAbove(cell)), 0.25f);
                ConsiderLux(ref farLux, cell, Grid.CellUpRight(Grid.CellRight(cell)), 0.25f);
                ConsiderLux(ref farLux, cell, Grid.CellDownRight(Grid.CellRight(cell)), 0.25f);
                ConsiderLux(ref farLux, cell, Grid.CellDownRight(Grid.CellBelow(cell)), 0.25f);
                ConsiderLux(ref farLux, cell, Grid.CellDownLeft(Grid.CellBelow(cell)), 0.25f);
                ConsiderLux(ref farLux, cell, Grid.CellDownLeft(Grid.CellLeft(cell)), 0.25f);
                ConsiderLux(ref farLux, cell, Grid.CellUpLeft(Grid.CellLeft(cell)), 0.25f);
                ConsiderLux(ref farLux, cell, Grid.CellUpLeft(Grid.CellAbove(cell)), 0.25f);

                __result = farLux > 0f ? Mathf.FloorToInt(farLux) : 0;
                if (__result < _minImpliedLuxCutoff)
                    __result = 0;
                return false;
            }
        }
    }
}
