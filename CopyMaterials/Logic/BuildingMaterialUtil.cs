using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class BuildingMaterialUtil
    {
        internal static int[] TryGetConstructionElementIds(GameObject go)
        {
            if (go == null) return null;

            // Under construction -> Constructable selected elements
            Component constructable = go.GetComponent("Constructable");
            if (constructable != null)
            {
                int[] ids = TryReadIntElementList(constructable, "selectedElements")
                         ?? TryReadIntElementList(constructable, "elements");
                if (ids != null && ids.Length > 0)
                    return ids;
            }

            // Fallback: primary element (single-material buildings)
            var primary = go.GetComponent<PrimaryElement>();
            if (primary != null)
                return new[] { (int)primary.ElementID };

            return null;
        }

        private static int[] TryReadIntElementList(Component comp, string fieldName)
        {
            try
            {
                var tr = Traverse.Create(comp);
                object val = tr.Field(fieldName).GetValue();
                if (val == null) return null;

                var arr = val as Array;
                if (arr != null)
                {
                    var result = new List<int>(arr.Length);
                    foreach (var x in arr)
                    {
                        int id;
                        if (TryConvertElementId(x, out id))
                            result.Add(id);
                    }
                    return result.ToArray();
                }

                var list = val as IList;
                if (list != null)
                {
                    var result = new List<int>(list.Count);
                    foreach (var x in list)
                    {
                        int id;
                        if (TryConvertElementId(x, out id))
                            result.Add(id);
                    }
                    return result.ToArray();
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private static bool TryConvertElementId(object x, out int id)
        {
            id = 0;
            if (x == null) return false;

            if (x is int) { id = (int)x; return true; }

            try
            {
                var t = x.GetType();
                var f = t.GetField("id");
                if (f != null)
                {
                    id = Convert.ToInt32(f.GetValue(x));
                    return true;
                }
                var p = t.GetProperty("id");
                if (p != null)
                {
                    id = Convert.ToInt32(p.GetValue(x, null));
                    return true;
                }
            }
            catch { }

            return false;
        }
    }
}
