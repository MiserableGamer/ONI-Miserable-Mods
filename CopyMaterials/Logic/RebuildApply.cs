using HarmonyLib;
using System;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class RebuildApply
    {
        internal static bool TryApplyAndRebuild(GameObject target, int[] elementIds)
        {
            if (target == null || elementIds == null || elementIds.Length == 0)
                return false;

            // Try common rebuild components by name (varies by build)
            foreach (string compName in new[] { "Rebuildable", "Reconstructable", "ReconstructableBuilding", "ChangeBuildingMaterial" })
            {
                Component comp = target.GetComponent(compName);
                if (comp == null) continue;

                // Try common method names
                if (TryInvokeIntArray(comp, "RequestRebuild", elementIds)) return true;
                if (TryInvokeIntArray(comp, "Rebuild", elementIds)) return true;
                if (TryInvokeIntArray(comp, "ChangeMaterial", elementIds)) return true;
                if (TryInvokeNoArgs(comp, "RequestRebuild")) return true;
                if (TryInvokeNoArgs(comp, "Rebuild")) return true;
            }

            // If nothing found, we can’t apply yet
            return false;
        }

        private static bool TryInvokeNoArgs(Component comp, string method)
        {
            try
            {
                var mi = AccessTools.Method(comp.GetType(), method, Type.EmptyTypes);
                if (mi == null) return false;
                mi.Invoke(comp, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryInvokeIntArray(Component comp, string method, int[] elementIds)
        {
            try
            {
                var mi = AccessTools.Method(comp.GetType(), method, new[] { typeof(int[]) });
                if (mi == null) return false;
                mi.Invoke(comp, new object[] { elementIds });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
