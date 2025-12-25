using System;
using System.Collections.Generic;
using UnityEngine;

namespace CopyMaterials.Logic
{
    public static class CopyMaterialsManager
    {
        private static Building sourceBuilding;
        private static SimHashes sourceMaterial = SimHashes.Vacuum;

        public static PrioritySetting sourcePriority = default;
        public static string sourceFacadeID = null;
        public static Tag sourceCopyGroupTag = Tag.Invalid;
        public static int? sourceBridgeWidth = null; // Store bridge width for ExtendedBuildingWidth support

        public static bool DebugMode = false;
        public static float WatcherTimeoutSeconds = 3f;

        // Store connections by cell location - key is (cell, layer)
        private static Dictionary<(int cell, int layer), UtilityConnections> storedConnections = new Dictionary<(int, int), UtilityConnections>();

        public static void StoreConnections(int cell, int layer, UtilityConnections connections)
        {
            if (connections != (UtilityConnections)0)
            {
                storedConnections[(cell, layer)] = connections;
                Log($"Stored connections {connections} for cell {cell}, layer {layer}");
            }
        }

        public static UtilityConnections GetStoredConnections(int cell, int layer)
        {
            if (storedConnections.TryGetValue((cell, layer), out var connections))
            {
                return connections;
            }
            return (UtilityConnections)0;
        }

        public static void ClearStoredConnections(int cell, int layer)
        {
            storedConnections.Remove((cell, layer));
        }

        public static void SetSource(Building building, SimHashes material)
        {
            sourceBuilding = building;
            sourceMaterial = material;

            var p = building?.GetComponent<Prioritizable>();
            sourcePriority = p != null ? p.GetMasterPriority() : default;

            var facade = building?.GetComponent<BuildingFacade>();
            sourceFacadeID = facade?.CurrentFacade;

            var cbs = building?.GetComponent<CopyBuildingSettings>();
            sourceCopyGroupTag = cbs != null ? cbs.copyGroupTag : Tag.Invalid;

            // Capture bridge width for ExtendedBuildingWidth support
            sourceBridgeWidth = GetBridgeWidth(building);

            Log($"Source set: {building?.Def?.PrefabID} with material {material}, bridge width: {sourceBridgeWidth}");
        }

        public static Building GetSourceBuilding()
        {
            return sourceBuilding;
        }

        public static SimHashes GetSourceMaterial()
        {
            return sourceMaterial;
        }

        public static void ClearSource()  // New: Clear on deactivate
        {
            sourceBuilding = null;
            sourceMaterial = SimHashes.Vacuum;
            sourcePriority = default;
            sourceFacadeID = null;
            sourceCopyGroupTag = Tag.Invalid;
            sourceBridgeWidth = null;
        }

        /// <summary>
        /// Get bridge width from a building (supports ExtendedBuildingWidth mod)
        /// </summary>
        public static int? GetBridgeWidth(Building building)
        {
            if (building == null) return null;

            // Check if this is a bridge
            if (!building.Def.PrefabID.Contains("Bridge")) return null;

            try
            {
                // Try to get width from the building component using reflection
                // ExtendedBuildingWidth likely stores width in a field or property
                var buildingType = building.GetType();
                
                // Try common field names for width
                var widthField = buildingType.GetField("width", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (widthField != null)
                {
                    var widthValue = widthField.GetValue(building);
                    if (widthValue is int intWidth)
                    {
                        Log($"Found bridge width via 'width' field: {intWidth}");
                        return intWidth;
                    }
                }

                // Try property
                var widthProperty = buildingType.GetProperty("Width", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (widthProperty != null)
                {
                    var widthValue = widthProperty.GetValue(building);
                    if (widthValue is int intWidth)
                    {
                        Log($"Found bridge width via 'Width' property: {intWidth}");
                        return intWidth;
                    }
                }

                // Try ExtendedBuildingWidth specific field/property
                widthField = buildingType.GetField("extendedWidth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (widthField != null)
                {
                    var widthValue = widthField.GetValue(building);
                    if (widthValue is int intWidth)
                    {
                        Log($"Found bridge width via 'extendedWidth' field: {intWidth}");
                        return intWidth;
                    }
                }

                // Try to get width from BuildingDef
                var defWidth = building.Def.WidthInCells;
                if (defWidth > 0)
                {
                    Log($"Using default bridge width from BuildingDef: {defWidth}");
                    return defWidth;
                }
            }
            catch (System.Exception e)
            {
                Warn($"Error getting bridge width: {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// Apply bridge width to a building (supports ExtendedBuildingWidth mod)
        /// </summary>
        public static bool ApplyBridgeWidth(GameObject buildingGO, int? width)
        {
            if (buildingGO == null || !width.HasValue) return false;

            var building = buildingGO.GetComponent<Building>();
            if (building == null) return false;

            // Check if this is a bridge
            if (!building.Def.PrefabID.Contains("Bridge")) return false;

            try
            {
                var buildingType = building.GetType();
                int targetWidth = width.Value;

                // Try to set width via field
                var widthField = buildingType.GetField("width", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (widthField != null)
                {
                    widthField.SetValue(building, targetWidth);
                    Log($"Applied bridge width via 'width' field: {targetWidth}");
                    return true;
                }

                // Try property
                var widthProperty = buildingType.GetProperty("Width", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (widthProperty != null && widthProperty.CanWrite)
                {
                    widthProperty.SetValue(building, targetWidth);
                    Log($"Applied bridge width via 'Width' property: {targetWidth}");
                    return true;
                }

                // Try ExtendedBuildingWidth specific field
                widthField = buildingType.GetField("extendedWidth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (widthField != null)
                {
                    widthField.SetValue(building, targetWidth);
                    Log($"Applied bridge width via 'extendedWidth' field: {targetWidth}");
                    return true;
                }

                // Try to call a method to set width
                var setWidthMethod = buildingType.GetMethod("SetWidth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (setWidthMethod != null)
                {
                    setWidthMethod.Invoke(building, new object[] { targetWidth });
                    Log($"Applied bridge width via 'SetWidth' method: {targetWidth}");
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Warn($"Error applying bridge width: {e.Message}");
            }

            return false;
        }

        public static void ClearAllStoredConnections()
        {
            storedConnections.Clear();
        }

        public static void ShowGlobalMessage(string message)
        {
            PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, message, null, Vector3.zero, 1.5f);
        }

        public static bool TrySetPrimaryElement(GameObject go, SimHashes elem)
        {
            if (elem == SimHashes.Vacuum) return false;

            var pe = go.GetComponent<PrimaryElement>();
            if (pe != null)
            {
                pe.ElementID = elem;
                return true;
            }
            return false;
        }

        public static void ApplyPriorityToObject(GameObject go, PrioritySetting priority)
        {
            if (go == null) return;
            var p = go.GetComponent<Prioritizable>();
            if (p != null)
            {
                p.SetMasterPriority(priority);
            }
        }

        public static void Log(string msg)
        {
            if (DebugMode) Debug.Log($"[CopyMaterials] {msg}");
        }

        public static void Warn(string msg)
        {
            Debug.LogWarning($"[CopyMaterials] {msg}");
        }
    }
}