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
            try { prefabId = bc.Def.PrefabID; } catch { prefabId = null; }
            if (string.IsNullOrEmpty(prefabId))
                return ApplyResult.NotRebuildable;

            int cell = Grid.PosToCell(target.transform.position);
            if (!Grid.IsValidCell(cell))
                return ApplyResult.NotRebuildable;

            if (!TryQueueDeconstruct(target))
                return ApplyResult.NotRebuildable;

            PendingRebuildManager.Queue(
                target.GetInstanceID(),
                new PendingRebuildManager.Pending
                {
                    prefabId = prefabId,
                    cell = cell,
                    materialTags = (Tag[])copiedMaterialTags.Clone(),
                    orientationEnum = TryGetOrientationEnum(target)
                }
            );

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
                    try { mi0.Invoke(decon, null); return true; } catch { }
                }

                MethodInfo mi1 = AccessTools.Method(t, methodNames[i], new[] { typeof(bool) });
                if (mi1 != null)
                {
                    try { mi1.Invoke(decon, new object[] { true }); return true; } catch { }
                }
            }

            return false;
        }

        // Called from PendingRebuildManager on cleanup
        internal static bool TryPlaceReplacementPlan(string prefabId, int cell, Tag[] materialTags, object orientationEnum, bool debug)
        {
            DEBUG_LOGS = debug;

            if (string.IsNullOrEmpty(prefabId) || !Grid.IsValidCell(cell) || materialTags == null || materialTags.Length == 0)
                return false;

            BuildingDef def = TryGetBuildingDef(prefabId);
            if (def == null)
            {
                Log("Could not resolve BuildingDef for " + prefabId);
                return false;
            }

            int beforeSig = GetCellSignature(cell);

            // ✅ Correct subsystem: BuildQueueManager (does not depend on active UI tools)
            bool ok = TryPlaceViaBuildQueueManager(def, prefabId, cell, materialTags, orientationEnum);
            if (ok && CellNowHasPlanOrConstructable(cell, beforeSig))
            {
                Log("Placed via BuildQueueManager");
                return true;
            }

            Log("No compatible BuildQueueManager placer found / no plan detected");
            return false;
        }

        private static bool TryPlaceViaBuildQueueManager(BuildingDef def, string prefabId, int cell, Tag[] materialTags, object orientationEnum)
        {
            Type bqmType = AccessTools.TypeByName("BuildQueueManager");
            if (bqmType == null)
            {
                Log("BuildQueueManager type not found");
                return false;
            }

            object inst = null;
            PropertyInfo pInst = AccessTools.Property(bqmType, "Instance");
            if (pInst != null) inst = pInst.GetValue(null, null);

            if (inst == null)
            {
                FieldInfo fInst = AccessTools.Field(bqmType, "Instance");
                if (fInst != null) inst = fInst.GetValue(null);
            }

            if (inst == null)
            {
                Log("BuildQueueManager.Instance not found");
                return false;
            }

            MethodInfo[] methods = bqmType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (m == null) continue;
                if (m.IsSpecialName) continue;

                ParameterInfo[] ps = m.GetParameters();
                if (ps == null || ps.Length == 0) continue;

                // Only consider methods that can possibly enqueue a build:
                // must accept either BuildingDef or prefabId AND a cell-ish arg.
                if (!HasParam(ps, typeof(BuildingDef)) && !HasParam(ps, typeof(string)))
                    continue;
                if (!HasParam(ps, typeof(int)) && !HasParam(ps, typeof(Vector3)))
                    continue;

                object[] args = BuildArgs(ps, def, prefabId, cell, materialTags, orientationEnum);
                if (args == null) continue;

                try
                {
                    object ret = m.Invoke(inst, args);
                    if (m.ReturnType == typeof(bool) && ret is bool && !(bool)ret)
                        continue;

                    // If it didn't throw, assume it queued something; outer verification will confirm.
                    return true;
                }
                catch
                {
                    // try next
                }
            }

            return false;
        }

        private static bool HasParam(ParameterInfo[] ps, Type t)
        {
            for (int i = 0; i < ps.Length; i++)
                if (ps[i].ParameterType == t)
                    return true;
            return false;
        }

        private static object[] BuildArgs(ParameterInfo[] ps, BuildingDef def, string prefabId, int cell, Tag[] materialTags, object orientationEnum)
        {
            object[] args = new object[ps.Length];
            bool usedCellInt = false;

            for (int i = 0; i < ps.Length; i++)
            {
                Type pt = ps[i].ParameterType;

                if (pt == typeof(BuildingDef))
                    args[i] = def;
                else if (pt == typeof(string))
                    args[i] = prefabId;
                else if (pt == typeof(int))
                    args[i] = (!usedCellInt) ? (usedCellInt = true, cell).Item2 : 0;
                else if (pt == typeof(Vector3))
                    args[i] = Grid.CellToPos(cell);
                else if (pt == typeof(Tag[]))
                    args[i] = (Tag[])materialTags.Clone();
                else if (pt.IsEnum && orientationEnum != null && pt == orientationEnum.GetType())
                    args[i] = orientationEnum;
                else if (pt == typeof(bool))
                    args[i] = true;
                else
                {
                    if (pt.IsValueType) return null;
                    args[i] = null;
                }
            }

            return args;
        }

        private static int GetCellSignature(int cell)
        {
            try
            {
                int sig = 17;
                foreach (ObjectLayer layer in Enum.GetValues(typeof(ObjectLayer)))
                {
                    GameObject go = Grid.Objects[cell, (int)layer];
                    int id = go != null ? go.GetInstanceID() : 0;
                    sig = (sig * 31) ^ id;
                }
                return sig;
            }
            catch { return 0; }
        }

        private static bool CellNowHasPlanOrConstructable(int cell, int beforeSig)
        {
            try
            {
                int afterSig = GetCellSignature(cell);
                bool changed = afterSig != beforeSig;

                foreach (ObjectLayer layer in Enum.GetValues(typeof(ObjectLayer)))
                {
                    GameObject go = Grid.Objects[cell, (int)layer];
                    if (go == null) continue;

                    if (go.GetComponent("Constructable") != null) return true;
                    if (go.GetComponent("BuildingUnderConstruction") != null) return true;

                    string n = go.name;
                    if (!string.IsNullOrEmpty(n))
                    {
                        string l = n.ToLowerInvariant();
                        if (l.Contains("construction") || l.Contains("underconstruction") || l.Contains("plan"))
                            return true;
                    }
                }

                return changed;
            }
            catch { return false; }
        }

        private static BuildingDef TryGetBuildingDef(string prefabId)
        {
            try
            {
                MethodInfo mi = AccessTools.Method(typeof(Assets), "GetBuildingDef", new[] { typeof(string) });
                if (mi != null)
                    return mi.Invoke(null, new object[] { prefabId }) as BuildingDef;
            }
            catch { }

            try
            {
                Type bcmType = AccessTools.TypeByName("BuildingConfigManager");
                if (bcmType != null)
                {
                    object inst = null;
                    PropertyInfo p = AccessTools.Property(bcmType, "Instance");
                    if (p != null) inst = p.GetValue(null, null);
                    if (inst == null)
                    {
                        FieldInfo f = AccessTools.Field(bcmType, "Instance");
                        if (f != null) inst = f.GetValue(null);
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

                Type t = rot.GetType();

                PropertyInfo p = AccessTools.Property(t, "Orientation");
                if (p != null && p.CanRead)
                    return p.GetValue(rot, null);

                FieldInfo f = AccessTools.Field(t, "orientation");
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
}
