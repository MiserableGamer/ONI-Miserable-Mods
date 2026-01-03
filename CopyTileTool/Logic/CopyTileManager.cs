using UnityEngine;

namespace CopyTileTool.Logic
{
    public enum ToolState
    {
        Idle,                    // Tool not active
        SelectingDestination,    // First click done, waiting for source selection
        SelectingSource,         // Source selected, ready to drag
        ReadyToDrag              // Both selected, drag to apply
    }

    public static class CopyTileManager
    {
        public static bool DebugMode = false;

        // Current tool state
        public static ToolState CurrentState { get; private set; } = ToolState.Idle;

        // Destination tile info (what we want to build)
        private static BuildingDef destinationDef;
        private static SimHashes destinationMaterial = SimHashes.Vacuum;
        private static PrioritySetting destinationPriority = default;

        // Source tile info (what we're looking for to replace)
        private static BuildingDef sourceDef;
        private static SimHashes sourceMaterial = SimHashes.Vacuum;

        // Set destination (first click - what to build)
        public static void SetDestination(Building building, SimHashes material)
        {
            destinationDef = building?.Def;
            destinationMaterial = material;

            var p = building?.GetComponent<Prioritizable>();
            destinationPriority = p != null ? p.GetMasterPriority() : default;

            CurrentState = ToolState.SelectingSource;

            Log($"Destination set: {destinationDef?.PrefabID} with material {destinationMaterial}");
        }

        // Set source (second click - what to match/replace)
        public static void SetSource(Building building, SimHashes material)
        {
            sourceDef = building?.Def;
            sourceMaterial = material;

            CurrentState = ToolState.ReadyToDrag;

            Log($"Source set: {sourceDef?.PrefabID} with material {sourceMaterial}");
        }

        public static BuildingDef GetDestinationDef() => destinationDef;
        public static SimHashes GetDestinationMaterial() => destinationMaterial;
        public static PrioritySetting GetDestinationPriority() => destinationPriority;

        public static BuildingDef GetSourceDef() => sourceDef;
        public static SimHashes GetSourceMaterial() => sourceMaterial;

        // Check if a tile matches the source criteria
        public static bool MatchesSource(Building building, PrimaryElement pe)
        {
            if (building == null || pe == null) return false;
            if (sourceDef == null) return false;

            bool typeMatches = building.Def.PrefabID == sourceDef.PrefabID;
            bool materialMatches = pe.ElementID == sourceMaterial;

            return typeMatches && materialMatches;
        }

        // Check if a building is a floor tile
        public static bool IsTile(GameObject go)
        {
            if (go == null) return false;

            // Check for FloorTiles tag
            var kpid = go.GetComponent<KPrefabID>();
            if (kpid != null && kpid.HasTag(GameTags.FloorTiles))
                return true;

            // Also check for SimCellOccupier (tiles replace cells)
            if (go.GetComponent<SimCellOccupier>() != null)
                return true;

            return false;
        }

        public static void ClearAll()
        {
            destinationDef = null;
            destinationMaterial = SimHashes.Vacuum;
            destinationPriority = default;
            sourceDef = null;
            sourceMaterial = SimHashes.Vacuum;
            CurrentState = ToolState.Idle;

            Log("CopyTileManager cleared");
        }

        public static void ResetToDestinationSelected()
        {
            // Keep destination, clear source, go back to SelectingSource state
            sourceDef = null;
            sourceMaterial = SimHashes.Vacuum;
            CurrentState = ToolState.SelectingSource;

            Log("Reset to SelectingSource state");
        }

        public static void ShowPopup(string message, Vector3 position)
        {
            PopFXManager.Instance.SpawnFX(
                PopFXManager.Instance.sprite_Plus,
                message,
                null,
                position,
                2f
            );
        }

        public static void Log(string msg)
        {
            if (DebugMode) Debug.Log($"[CopyTileTool] {msg}");
        }

        public static void Warn(string msg)
        {
            Debug.LogWarning($"[CopyTileTool] {msg}");
        }
    }
}

