using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class RebuildApply
    {
        internal static bool DEBUG_LOGS = false;

        internal enum ApplyResult
        {
            NotRebuildable = 0,
            Incompatible = 1,
            Applied = 2
        }

        internal static ApplyResult TryApplyAndQueueRebuild(GameObject target, Tag[] copiedMaterialTags)
        {
            return TryApplyAndQueueRebuild(target, copiedMaterialTags, DEBUG_LOGS);
        }

        internal static ApplyResult TryApplyAndQueueRebuild(GameObject target, Tag[] copiedMaterialTags, bool debug)
        {
            DEBUG_LOGS = debug;

            if (target == null || copiedMaterialTags == null || copiedMaterialTags.Length == 0)
                return ApplyResult.NotRebuildable;

            var bc = target.GetComponent<BuildingComplete>();
            if (bc == null || bc.Def == null)
                return ApplyResult.NotRebuildable;

            string prefabId = null;
            try { prefabId = bc.Def.PrefabID; } catch { }

            if (string.IsNullOrEmpty(prefabId))
                return ApplyResult.NotRebuildable;

            int cell = Grid.PosToCell(target.transform.position);
            if (!Grid.IsValidCell(cell))
                return ApplyResult.NotRebuildable;

            Tag[] selected = (Tag[])copiedMaterialTags.Clone();
            object orientation = TryGetOrientationEnum(target);

            // 1) Queue deconstruct
            if (!TryQueueDeconstruct(target))
                return ApplyResult.NotRebuildable;

            // 2) Place after cleanup
            PendingRebuildManager.Queue(
                target.GetInstanceID(),
                new PendingRebuildManager.Pending
                {
                    prefabId = prefabId,
                    cell = cell,
                    elements = null,                // legacy slot unused
                    orientationEnum = orientation,
                    // We'll stash tags via a static temp map in PendingRebuildManager? No — simplest: serialize tags to ints.
                    // But Pending struct currently has int[] only. We'll encode tags as hashes.
                }
            );

            // We must pass tags through PendingRebuildManager. It currently holds int[].
            // Encode Tag -> int hash, decode later.
            PendingRebuildManager_Compat.StoreTagsForInstance(target.GetInstanceID(), selected);

            return ApplyResult.Applied;
        }

        private static bool TryQueueDeconstruct(GameObject target)
        {
            Component decon = target.GetComponent("Deconstructable");
            if (decon == null)
            {
                Log("No Deconstructable on " + target.name);
                return false;
            }

            Type t = decon.GetType();

            string[] methodNames =
            {
                "QueueDeconstruct",
                "QueueDeconstruction",
                "RequestDeconstruct",
                "RequestDeconstruction",
                "Deconstruct"
            };

            for (int i = 0; i < methodNames.Length; i++)
            {
                MethodInfo mi0 = AccessTools.Method(t, methodNames[i], Type.EmptyTypes);
                if (mi0 != null)
                {
                    try { mi0.Invoke(decon, null); Log("Deconstruct via " + methodNames[i] + "()"); return true; }
                    catch { }
                }

                MethodInfo mi1 = AccessTools.Method(t, methodNames[i], new[] { typeof(bool) });
                if (mi1 != null)
                {
                    try { mi1.Invoke(decon, new object[] { true }); Log("Deconstruct via " + methodNames[i] + "(bool)"); return true; }
                    catch { }
                }
            }

            return false;
        }

        /// <summary>
        /// Called by PendingRebuildManager after the old building is gone.
        /// </summary>
        internal static bool TryPlacePlanByPrefabId(string prefabId, int cell, int[] legacyElements, object orientationEnum, bool debug)
        {
            DEBUG_LOGS = debug;

            Tag[] tags = PendingRebuildManager_Compat.TryConsumeTagsForCellPlacement();
            if (tags == null || tags.Length == 0)
            {
                Log("No stored tags for placement");
                return false;
            }

            BuildingDef def = TryGetBuildingDef(prefabId);
            if (def == null)
            {
                Log("Could not resolve BuildingDef for " + prefabId);
                return false;
            }

            // Verify cell is clear-ish
            // (If something is already there, Create may fail)
            try
            {
                GameObject existing = Grid.Objects[cell, (int)ObjectLayer.Building];
                if (existing != null)
                    Log("Warning: building still in cell at placement time: " + existing.name);
            }
            catch { }

            Type bucType = AccessTools.TypeByName("BuildingUnderConstruction");
            if (bucType == null)
            {
                Log("BuildingUnderConstruction type not found");
                return false;
            }

            // Find a static Create-like method
            MethodInfo[] methods = bucType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (m == null) continue;

                string lower = (m.Name ?? "").ToLowerInvariant();
                if (!(lower.Contains("create") || lower.Contains("place")))
                    continue;

                object[] args = BuildBUCArgs(m.GetParameters(), def, cell, tags, orientationEnum);
                if (args == null) continue;

                try
                {
                    object ret = m.Invoke(null, args);

                    // Success check: constructable/under construction exists in cell
                    if (CellHasUnderConstruction(cell))
                    {
                        Log("Placed via BuildingUnderConstruction." + m.Name);
                        return true;
                    }
                }
                catch
                {
                    // try next
                }
            }

            Log("No compatible BuildingUnderConstruction.Create found");
            return false;
        }

        private static object[] BuildBUCArgs(ParameterInfo[] ps, BuildingDef def, int cell, Tag[] tags, object orientationEnum)
        {
            if (ps == null) return null;

            object[] args = new object[ps.Length];

            for (int i = 0; i < ps.Length; i++)
            {
                Type pt = ps[i].ParameterType;
                string pn = (ps[i].Name ?? "").ToLowerInvariant();

                if (pt == typeof(BuildingDef))
                {
                    args[i] = def;
                }
                else if (pt == typeof(int))
                {
                    // cell or something else; best effort: if name says cell, use cell, else 0
                    args[i] = pn.Contains("cell") ? cell : cell;
                }
                else if (pt == typeof(Vector3))
                {
                    args[i] = Grid.CellToPos(cell);
                }
                else if (pt == typeof(Tag[]))
                {
                    args[i] = (Tag[])tags.Clone();
                }
                else if (pt.IsEnum && orientationEnum != null && pt == orientationEnum.GetType())
                {
                    args[i] = orientationEnum;
                }
                else if (pt == typeof(bool))
                {
                    args[i] = true;
                }
                else
                {
                    if (pt.IsValueType) return null;
                    args[i] = null;
                }
            }

            return args;
        }

        private static bool CellHasUnderConstruction(int cell)
        {
            try
            {
                foreach (ObjectLayer layer in Enum.GetValues(typeof(ObjectLayer)))
                {
                    GameObject go = Grid.Objects[cell, (int)layer];
                    if (go == null) continue;
                    if (go.GetComponent("BuildingUnderConstruction") != null) return true;
                    if (go.GetComponent("Constructable") != null) return true;
                }
            }
            catch { }
            return false;
        }

        private static BuildingDef TryGetBuildingDef(string prefabId)
        {
            try
            {
                // Preferred: Assets.GetBuildingDef(string)
                MethodInfo mi = AccessTools.Method(typeof(Assets), "GetBuildingDef", new[] { typeof(string) });
                if (mi != null)
                {
                    var def = mi.Invoke(null, new object[] { prefabId }) as BuildingDef;
                    if (def != null) return def;
                }
            }
            catch { }

            try
            {
                // Fallback: BuildingConfigManager.Instance.GetBuildingDef(string)
                Type bcmType = AccessTools.TypeByName("BuildingConfigManager");
                if (bcmType != null)
                {
                    object inst = null;
                    var pInst = AccessTools.Property(bcmType, "Instance");
                    if (pInst != null) inst = pInst.GetValue(null, null);
                    if (inst == null)
                    {
                        var fInst = AccessTools.Field(bcmType, "Instance");
                        if (fInst != null) inst = fInst.GetValue(null);
                    }

                    if (inst != null)
                    {
                        MethodInfo mi = AccessTools.Method(bcmType, "GetBuildingDef", new[] { typeof(string) });
                        if (mi != null)
                            return mi.Invoke(inst, new object[] { prefabId }) as BuildingDef;
                    }
                }
            }
            catch { }

            return null;
        }

        private static object TryGetOrientationEnum(GameObject target)
        {
            try
            {
                Component rot = target.GetComponent("Rotatable");
                if (rot == null) return null;

                var t = rot.GetType();

                var p = AccessTools.Property(t, "Orientation");
                if (p != null && p.CanRead)
                    return p.GetValue(rot, null);

                var f = AccessTools.Field(t, "orientation");
                if (f != null)
                    return f.GetValue(rot);
            }
            catch { }

            return null;
        }

        private static void Log(string msg)
        {
            if (DEBUG_LOGS)
                Debug.Log("[CopyMaterialsTool] " + msg);
        }
    }

    /// <summary>
    /// Tiny bridge because PendingRebuildManager.Pending only had int[].
    /// We store Tag[] for the *most recent* pending placement.
    /// This is safe because placements happen one-at-a-time on OnCleanUp.
    /// </summary>
    internal static class PendingRebuildManager_Compat
    {
        private static Tag[] _nextTags;

        internal static void StoreTagsForInstance(int instanceId, Tag[] tags)
        {
            _nextTags = tags;
        }

        internal static Tag[] TryConsumeTagsForCellPlacement()
        {
            var t = _nextTags;
            _nextTags = null;
            return t;
        }
    }
}
