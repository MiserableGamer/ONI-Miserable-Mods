using System.Collections.Generic;
using HarmonyLib;
using ControlledVisuals.Options;
using ControlledVisuals.State;
using PeterHan.PLib.Options;
using UnityEngine;
using CVOptions = ControlledVisuals.Options.ControlledVisualsOptions;

namespace ControlledVisuals.Patches
{
    [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.OnPrefabInit))]
    public static class SaveGame_CleanEdgesState_Patch
    {
        public static bool Prepare() => CVOptions.Instance.EnableCleanEdges;

        public static void Postfix(SaveGame __instance)
        {
            __instance.gameObject.AddOrGet<CleanEdgesSaveState>();
        }
    }

    [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.OnSpawn))]
    public static class SaveGame_CleanEdgesConvert_Patch
    {
        private const float NeutroniumBorderMassKg = 20000f;
        private const int CurrentConversionVersion = 1;
        private static readonly Dictionary<int, float> RowTemps = new Dictionary<int, float>(64);

        public static bool Prepare() => CVOptions.Instance.EnableCleanEdges;

        public static void Postfix()
        {
            var options = CVOptions.Instance;
            bool forceReconvert = options.ReconvertOnNextSaveLoad;
            int configuredBorderSize = options.CleanEdgesBorderSize;
            int configuredAbyssaliteMass = options.CleanEdgesAbyssaliteMassKg;
            var state = SaveGame.Instance?.gameObject?.AddOrGet<CleanEdgesSaveState>();
            if (!forceReconvert && state != null && state.CleanEdgesConverted)
            {
                Debug.Log("[ControlledVisuals] CleanEdges: save already converted, skipping.");
                return;
            }

            if (forceReconvert)
                Debug.Log("[ControlledVisuals] CleanEdges: forced reconvert requested for this save load.");

            Debug.Log($"[ControlledVisuals] CleanEdges: options in effect -> EnableCleanEdges={options.EnableCleanEdges}, ReconvertOnNextSaveLoad={forceReconvert}, BorderSize={configuredBorderSize}, AbyssaliteMassKg={configuredAbyssaliteMass}");

            if (!TryConvertMapEdges(out string message))
            {
                Debug.LogWarning("[ControlledVisuals] CleanEdges: conversion skipped. " + message);
                return;
            }

            if (state != null)
            {
                state.CleanEdgesConverted = true;
                state.ConversionVersion = CurrentConversionVersion;
            }

            if (forceReconvert)
            {
                options.ReconvertOnNextSaveLoad = false;
                try
                {
                    POptions.WriteSettings<CVOptions>(options);
                    Debug.Log("[ControlledVisuals] CleanEdges: one-shot reconvert flag reset to false.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[ControlledVisuals] CleanEdges: conversion succeeded but failed to persist one-shot flag reset: " + ex.Message);
                }
            }

            Debug.Log("[ControlledVisuals] CleanEdges: conversion completed (" + (forceReconvert ? "forced reconvert" : "normal conversion") + "). " + message);
        }

        private static bool TryConvertMapEdges(out string message)
        {
            message = string.Empty;

            var options = CVOptions.Instance;
            int borderSize = Mathf.Clamp(options.CleanEdgesBorderSize, 1, 20);
            float abyssaliteMass = Mathf.Clamp(options.CleanEdgesAbyssaliteMassKg, 1, 20000);
            int width = Grid.WidthInCells;
            int height = Grid.HeightInCells;

            if (width <= 0 || height <= 0 || Grid.Element == null || Grid.Element.Length == 0)
            {
                message = "Grid is not initialized.";
                return false;
            }

            var abyssalite = ElementLoader.FindElementByHash(SimHashes.Katairite);
            var neutronium = ElementLoader.FindElementByHash(SimHashes.Unobtanium);
            if (abyssalite == null || neutronium == null)
            {
                message = "Required elements were not found.";
                return false;
            }

            borderSize = Mathf.Clamp(borderSize, 1, Mathf.Max(1, width / 2));
            RowTemps.Clear();

            var neutroniumCells = new List<int>(1024);
            int firstSpaceNeutroniumRow = height;
            int convertedToAbyssalite = 0;
            int convertedToNeutronium = 0;
            int topBorderRestored = 0;
            int leftBorderRestored = 0;
            int rightBorderRestored = 0;

            for (int cell = 0; cell < Grid.Element.Length; cell++)
            {
                if (!IsNeutronium(cell))
                    continue;

                neutroniumCells.Add(cell);
                if (IsSpace(cell))
                    firstSpaceNeutroniumRow = Mathf.Min(firstSpaceNeutroniumRow, Grid.CellRow(cell));
            }

            if (neutroniumCells.Count == 0)
            {
                message = "No neutronium cells found.";
                return false;
            }

            var protectedGeyserCells = CollectProtectedGeyserCells();

            int interiorRight = width - borderSize - 1;
            foreach (int cell in neutroniumCells)
            {
                if (protectedGeyserCells.Contains(cell))
                    continue;

                int col = Grid.CellColumn(cell);
                int row = Grid.CellRow(cell);
                bool insideHorizontalInterior = col >= borderSize && col <= interiorRight;
                bool belowSpaceBoundary = row >= firstSpaceNeutroniumRow;

                if ((insideHorizontalInterior || belowSpaceBoundary) && row >= borderSize)
                {
                    if (ReplaceCell(cell, abyssalite.id, abyssaliteMass))
                        convertedToAbyssalite++;
                }
            }

            for (int row = 0; row < borderSize; row++)
            {
                int rowStart = row * width;
                for (int col = 0; col < width; col++)
                {
                    if (ReplaceCell(rowStart + col, neutronium.id, NeutroniumBorderMassKg))
                    {
                        convertedToNeutronium++;
                        topBorderRestored++;
                    }
                }
            }

            for (int row = 0; row < height; row++)
            {
                int rowStart = row * width;
                // Restore every exposed left shell segment and paint inward.
                for (int col = 0; col < width; col++)
                {
                    int cell = rowStart + col;
                    if (!IsMaterialCell(cell))
                        continue;

                    bool isExposedLeftEdge = col == 0;
                    if (!isExposedLeftEdge)
                    {
                        int outsideCell = rowStart + col - 1;
                        isExposedLeftEdge = !IsMaterialCell(outsideCell) || IsSpace(outsideCell);
                    }

                    if (!isExposedLeftEdge)
                        continue;

                    for (int offset = 0; offset < borderSize; offset++)
                    {
                        int targetCol = col + offset;
                        if (targetCol >= width)
                            break;

                        int targetCell = rowStart + targetCol;
                        if (!IsMaterialCell(targetCell))
                            break;

                        if (ReplaceCell(targetCell, neutronium.id, NeutroniumBorderMassKg))
                        {
                            convertedToNeutronium++;
                            leftBorderRestored++;
                        }
                    }
                }

                // Restore every exposed right shell segment and paint inward.
                for (int col = width - 1; col >= 0; col--)
                {
                    int cell = rowStart + col;
                    if (!IsMaterialCell(cell))
                        continue;

                    bool isExposedRightEdge = col == width - 1;
                    if (!isExposedRightEdge)
                    {
                        int outsideCell = rowStart + col + 1;
                        isExposedRightEdge = !IsMaterialCell(outsideCell) || IsSpace(outsideCell);
                    }

                    if (!isExposedRightEdge)
                        continue;

                    for (int offset = 0; offset < borderSize; offset++)
                    {
                        int targetCol = col - offset;
                        if (targetCol < 0)
                            break;

                        int targetCell = rowStart + targetCol;
                        if (!IsMaterialCell(targetCell))
                            break;

                        if (ReplaceCell(targetCell, neutronium.id, NeutroniumBorderMassKg))
                        {
                            convertedToNeutronium++;
                            rightBorderRestored++;
                        }
                    }
                }
            }

            message = $"Converted {convertedToAbyssalite} cells to abyssalite, restored {convertedToNeutronium} border cells to neutronium (top={topBorderRestored}, left={leftBorderRestored}, right={rightBorderRestored}), protected {protectedGeyserCells.Count} geyser-adjacent cells.";
            return true;
        }

        private static bool IsNeutronium(int cell)
        {
            return Grid.IsValidCell(cell) && Grid.Element[cell].id == SimHashes.Unobtanium;
        }

        private static HashSet<int> CollectProtectedGeyserCells()
        {
            var protectedCells = new HashSet<int>();
            var clusterManager = ClusterManager.Instance;
            if (clusterManager == null)
                return protectedCells;

            foreach (var world in clusterManager.WorldContainers)
            {
                if (world == null)
                    continue;

                foreach (var geyser in Components.Geysers.GetItems(world.id))
                {
                    if (geyser == null)
                        continue;

                    int centerCell = Grid.PosToCell(geyser.transform.position);
                    if (!Grid.IsValidCell(centerCell))
                        continue;

                    for (int y = -1; y <= 2; y++)
                    {
                        for (int x = -2; x <= 2; x++)
                        {
                            int cell = Grid.OffsetCell(centerCell, x, y);
                            if (Grid.IsValidCell(cell) && Grid.Element[cell].id == SimHashes.Unobtanium)
                                protectedCells.Add(cell);
                        }
                    }
                }
            }

            return protectedCells;
        }

        private static bool IsSpace(int cell)
        {
            if (!Grid.IsValidCell(cell))
                return false;

            return Grid.IsCellBiomeSpaceBiome(cell);
        }

        private static bool IsMaterialCell(int cell)
        {
            if (!Grid.IsValidCell(cell))
                return false;

            return Grid.Element[cell].id != SimHashes.Vacuum;
        }

        private static float AverageRowTemp(int cell)
        {
            int row = Grid.CellRow(cell);
            if (RowTemps.TryGetValue(row, out float cached))
                return cached;

            int start = row * Grid.WidthInCells;
            int end = start + Grid.WidthInCells;
            float total = 0f;
            int count = 0;

            for (int i = start; i < end; i++)
            {
                AccumulateTemp(i, ref total, ref count);
                AccumulateTemp(Grid.CellAbove(i), ref total, ref count);
                AccumulateTemp(Grid.CellBelow(i), ref total, ref count);
            }

            float average = count > 0 ? total / count : 293.15f;
            RowTemps[row] = average;
            return average;
        }

        private static void AccumulateTemp(int cell, ref float total, ref int count)
        {
            if (!Grid.IsValidCell(cell))
                return;

            float temp = Grid.Temperature[cell];
            if (float.IsNaN(temp) || float.IsInfinity(temp))
                return;

            total += temp;
            count++;
        }

        private static bool ReplaceCell(int cell, SimHashes targetElement, float massKg)
        {
            if (!Grid.IsValidCell(cell))
                return false;

            bool changed = Grid.Element[cell].id != targetElement;
            SimMessages.ReplaceAndDisplaceElement(cell, targetElement, null, massKg, AverageRowTemp(cell), byte.MaxValue, 0, -1);
            return changed;
        }
    }
}
