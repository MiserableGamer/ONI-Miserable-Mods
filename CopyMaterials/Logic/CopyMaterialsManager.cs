using System;
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

        public static bool DebugMode = true;
        public static float WatcherTimeoutSeconds = 3f;

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

            Log($"Source set: {building?.Def?.PrefabID} with material {material}");
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