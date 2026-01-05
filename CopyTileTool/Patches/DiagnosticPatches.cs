using HarmonyLib;
using UnityEngine;
using Rendering;
using CopyTileTool.Logic;

// Diagnostic patches to trace tile replacement flow
// These can be disabled by setting EnableDiagnostics = false

namespace CopyTileTool.Patches
{
    public static class DiagnosticPatches
    {
        // Set to false to disable all diagnostic logging
        public static bool EnableDiagnostics = true;

        private static void Log(string message)
        {
            if (EnableDiagnostics)
            {
                CopyTileManager.Log($"[DIAG] {message}");
            }
        }
    }

    // Log when Constructable.OnCompleteWork runs (tile construction finishes)
    [HarmonyPatch(typeof(Constructable), "OnCompleteWork")]
    public static class Constructable_OnCompleteWork_Patch
    {
        public static void Prefix(Constructable __instance)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;

            var building = __instance.GetComponent<Building>();
            if (building == null || !building.Def.IsTilePiece) return;

            int cell = Grid.PosToCell(__instance.transform.GetLocalPosition());
            var isReplacement = __instance.IsReplacementTile;
            
            CopyTileManager.Log($"[DIAG] Constructable.OnCompleteWork START: cell={cell}, def={building.Def.PrefabID}, IsReplacementTile={isReplacement}");

            if (isReplacement)
            {
                var replacementCandidate = building.Def.GetReplacementCandidate(cell);
                if (replacementCandidate != null)
                {
                    var oldBuilding = replacementCandidate.GetComponent<Building>();
                    var oldPE = replacementCandidate.GetComponent<PrimaryElement>();
                    CopyTileManager.Log($"[DIAG] ReplacementCandidate found: def={oldBuilding?.Def?.PrefabID}, element={oldPE?.ElementID}");
                    
                    var simCellOccupier = replacementCandidate.GetComponent<SimCellOccupier>();
                    CopyTileManager.Log($"[DIAG] SimCellOccupier present: {simCellOccupier != null}");
                }
                else
                {
                    CopyTileManager.Log($"[DIAG] WARNING: No ReplacementCandidate found for replacement tile!");
                }
            }
        }

        public static void Postfix(Constructable __instance)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;

            var building = __instance.GetComponent<Building>();
            if (building == null || !building.Def.IsTilePiece) return;

            int cell = Grid.PosToCell(__instance.transform.GetLocalPosition());
            CopyTileManager.Log($"[DIAG] Constructable.OnCompleteWork END: cell={cell}");
        }
    }

    // Log when BuildingDef.Build creates a completed tile
    [HarmonyPatch(typeof(BuildingDef), "Build", typeof(int), typeof(Orientation), typeof(Storage), typeof(System.Collections.Generic.IList<Tag>), typeof(float), typeof(bool), typeof(float))]
    public static class BuildingDef_Build_Patch
    {
        public static void Prefix(BuildingDef __instance, int cell)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;
            if (!__instance.IsTilePiece) return;
            if (!Grid.IsValidCell(cell)) return;

            try
            {
                CopyTileManager.Log($"[DIAG] BuildingDef.Build START: cell={cell}, def={__instance.PrefabID}, TileLayer={__instance.TileLayer}");
                
                // Check what's currently in Grid.Objects for this cell
                if (__instance.TileLayer != ObjectLayer.NumLayers)
                {
                    var existingTile = Grid.Objects[cell, (int)__instance.TileLayer];
                    CopyTileManager.Log($"[DIAG] Current Grid.Objects[{cell}, TileLayer]: {(existingTile != null ? existingTile.name : "null")}");
                }
                
                if (__instance.ReplacementLayer != ObjectLayer.NumLayers)
                {
                    var existingReplacement = Grid.Objects[cell, (int)__instance.ReplacementLayer];
                    CopyTileManager.Log($"[DIAG] Current Grid.Objects[{cell}, ReplacementLayer]: {(existingReplacement != null ? existingReplacement.name : "null")}");
                }
            }
            catch { } // Silently skip on error
        }

        public static void Postfix(BuildingDef __instance, int cell, GameObject __result)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;
            if (!__instance.IsTilePiece) return;
            if (!Grid.IsValidCell(cell)) return;

            try
            {
                CopyTileManager.Log($"[DIAG] BuildingDef.Build END: cell={cell}, result={(__result != null ? __result.name : "null")}");
                
                // Check Grid.Objects after build
                if (__instance.TileLayer != ObjectLayer.NumLayers)
                {
                    var tileAfter = Grid.Objects[cell, (int)__instance.TileLayer];
                    CopyTileManager.Log($"[DIAG] After build Grid.Objects[{cell}, TileLayer]: {(tileAfter != null ? tileAfter.name : "null")}");
                }
            }
            catch { } // Silently skip on error
        }
    }

    // Log when TileVisualizer.RefreshCell is called
    [HarmonyPatch(typeof(TileVisualizer), "RefreshCell", typeof(int), typeof(ObjectLayer), typeof(ObjectLayer))]
    public static class TileVisualizer_RefreshCell_Patch
    {
        public static void Prefix(int cell, ObjectLayer tile_layer, ObjectLayer replacement_layer)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;

            // Safety check - NumLayers is used as "none" and would cause IndexOutOfRange
            if (tile_layer == ObjectLayer.NumLayers && replacement_layer == ObjectLayer.NumLayers) return;
            if (!Grid.IsValidCell(cell)) return;

            string tileObjName = "N/A";
            string replObjName = "N/A";

            try
            {
                if (tile_layer != ObjectLayer.NumLayers)
                {
                    var tileObj = Grid.Objects[cell, (int)tile_layer];
                    tileObjName = tileObj != null ? tileObj.name : "null";
                }
                if (replacement_layer != ObjectLayer.NumLayers)
                {
                    var replObj = Grid.Objects[cell, (int)replacement_layer];
                    replObjName = replObj != null ? replObj.name : "null";
                }
            }
            catch { return; } // Silently skip if any error

            CopyTileManager.Log($"[DIAG] TileVisualizer.RefreshCell: cell={cell}, tile_layer={tile_layer}, replacement_layer={replacement_layer}");
            CopyTileManager.Log($"[DIAG]   TileLayer object: {tileObjName}");
            CopyTileManager.Log($"[DIAG]   ReplacementLayer object: {replObjName}");
        }
    }

    // Log when SimCellOccupier.DestroySelf is called
    [HarmonyPatch(typeof(SimCellOccupier), "DestroySelf")]
    public static class SimCellOccupier_DestroySelf_Patch
    {
        public static void Prefix(SimCellOccupier __instance, System.Action onComplete)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;

            var building = __instance.GetComponent<Building>();
            var pe = __instance.GetComponent<PrimaryElement>();
            int cell = Grid.PosToCell(__instance.transform.GetPosition());
            
            CopyTileManager.Log($"[DIAG] SimCellOccupier.DestroySelf START: cell={cell}, def={building?.Def?.PrefabID}, element={pe?.ElementID}, hasCallback={onComplete != null}");
        }

        public static void Postfix(SimCellOccupier __instance)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;

            int cell = Grid.PosToCell(__instance.transform.GetPosition());
            CopyTileManager.Log($"[DIAG] SimCellOccupier.DestroySelf END: cell={cell}");
        }
    }

    // Log when BlockTileRenderer.Rebuild is called
    [HarmonyPatch(typeof(BlockTileRenderer), "Rebuild")]
    public static class BlockTileRenderer_Rebuild_Patch
    {
        public static void Prefix(BlockTileRenderer __instance, ObjectLayer layer, int cell)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;

            CopyTileManager.Log($"[DIAG] BlockTileRenderer.Rebuild: layer={layer}, cell={cell}");
        }
    }

    // Log when BlockTileRenderer.AddBlock is called
    [HarmonyPatch(typeof(BlockTileRenderer), "AddBlock")]
    public static class BlockTileRenderer_AddBlock_Patch
    {
        public static void Prefix(int renderLayer, BuildingDef def, bool isReplacement, SimHashes element, int cell)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;

            CopyTileManager.Log($"[DIAG] BlockTileRenderer.AddBlock: cell={cell}, def={def?.PrefabID}, isReplacement={isReplacement}, element={element}");
        }
    }

    // Log when BlockTileRenderer.RemoveBlock is called
    [HarmonyPatch(typeof(BlockTileRenderer), "RemoveBlock")]
    public static class BlockTileRenderer_RemoveBlock_Patch
    {
        public static void Prefix(BuildingDef def, bool isReplacement, SimHashes element, int cell)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;

            CopyTileManager.Log($"[DIAG] BlockTileRenderer.RemoveBlock: cell={cell}, def={def?.PrefabID}, isReplacement={isReplacement}, element={element}");
        }
    }

    // Fix tile render cleanup - explicitly remove with correct element
    // This works around a game bug where RemoveBlock uses the wrong element during replacement
    //
    // NOTE: This fixes ghosts but TrueTiles texture may not apply on first replacement of CopyTileTool tiles.
    // TrueTiles works on: CopyTileTool creation, 2nd+ replacement, and after save/reload.
    //
    [HarmonyPatch(typeof(BuildingComplete), "OnCleanUp")]
    public static class BuildingComplete_OnCleanUp_Patch
    {
        public static void Prefix(BuildingComplete __instance)
        {
            try
            {
                var building = __instance.GetComponent<Building>();
                if (building == null || !building.Def.IsTilePiece) return;
                if (!building.Def.isKAnimTile) return;

                int cell = Grid.PosToCell(__instance.transform.GetPosition());
                if (!Grid.IsValidCell(cell)) return;

                var pe = __instance.GetComponent<PrimaryElement>();
                if (pe == null) return;

                SimHashes actualElement = pe.ElementID;

                if (DiagnosticPatches.EnableDiagnostics)
                {
                    var decon = __instance.GetComponent<Deconstructable>();
                    CopyTileManager.Log($"[DIAG] BuildingComplete.OnCleanUp: cell={cell}, def={building.Def.PrefabID}, PrimaryElement={actualElement}");
                    if (decon != null && decon.constructionElements != null)
                    {
                        var elementsStr = string.Join(", ", decon.constructionElements);
                        CopyTileManager.Log($"[DIAG]   Deconstructable.constructionElements: [{elementsStr}]");
                    }
                }

                // FIX: Explicitly remove the tile from render cache with the CORRECT element
                // The game's cleanup uses the wrong element during replacement, causing ghost tiles
                World.Instance.blockTileRenderer.RemoveBlock(building.Def, false, actualElement, cell);
                
                if (DiagnosticPatches.EnableDiagnostics)
                {
                    CopyTileManager.Log($"[DIAG]   FIX APPLIED: Called RemoveBlock with correct element {actualElement}");
                }
            }
            catch (System.Exception e)
            {
                if (DiagnosticPatches.EnableDiagnostics)
                {
                    CopyTileManager.Log($"[DIAG] Error in OnCleanUp fix: {e.Message}");
                }
            }
        }
    }

    // Log when a Constructable finishes and sets up the Deconstructable
    [HarmonyPatch(typeof(BuildingDef), "Build", typeof(int), typeof(Orientation), typeof(Storage), typeof(System.Collections.Generic.IList<Tag>), typeof(float), typeof(bool), typeof(float))]
    public static class BuildingDef_Build_ConstructionElements_Patch
    {
        public static void Postfix(BuildingDef __instance, int cell, System.Collections.Generic.IList<Tag> selected_elements, GameObject __result)
        {
            if (!DiagnosticPatches.EnableDiagnostics) return;
            if (!__instance.IsTilePiece) return;
            if (__result == null) return;

            try
            {
                var decon = __result.GetComponent<Deconstructable>();
                if (decon != null && decon.constructionElements != null)
                {
                    var elementsStr = string.Join(", ", decon.constructionElements);
                    var selectedStr = selected_elements != null ? string.Join(", ", selected_elements) : "null";
                    CopyTileManager.Log($"[DIAG] After Build - Deconstructable.constructionElements: [{elementsStr}], selectedElements was: [{selectedStr}]");
                }
            }
            catch { }
        }
    }
}

