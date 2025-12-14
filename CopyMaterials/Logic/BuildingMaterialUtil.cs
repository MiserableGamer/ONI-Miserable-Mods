using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class BuildingMaterialUtil
    {
        internal static int[] TryGetConstructionElementIds(GameObject go)
        {
            if (go == null) return null;

            // 1) Completed buildings: try Reconstructable.selectedElements first
            Component recon = go.GetComponent("Reconstructable") ?? go.GetComponent("ReconstructableBuilding");
            if (recon != null)
            {
                int[] ids = TryReadElementIdsFromUnknownContainer(recon, "selectedElements")
                        ?? TryReadElementIdsFromUnknownContainer(recon, "SelectedElements")
                        ?? TryReadElementIdsFromUnknownContainer(recon, "elements")
                        ?? TryReadElementIdsFromUnknownContainer(recon, "Elements");

                if (ids != null && ids.Length > 0)
                    return ids;
            }

            // 2) Under construction: Constructable selected elements
            Component constructable = go.GetComponent("Constructable");
            if (constructable != null)
            {
                int[] ids = TryReadElementIdsFromUnknownContainer(constructable, "selectedElements")
                         ?? TryReadElementIdsFromUnknownContainer(constructable, "elements");
                if (ids != null && ids.Length > 0)
                    return ids;
            }

            // 3) Fallback: primary element (single-material buildings)
            var primary = go.GetComponent<PrimaryElement>();
            if (primary != null)
                return new[] { (int)primary.ElementID };

            return null;
        }

        private static int[] TryReadElementIdsFromUnknownContainer(Component comp, string fieldName)
        {
            try
            {
                var tr = Traverse.Create(comp);
                object val = tr.Field(fieldName).GetValue();
                if (val == null) return null;

                // int[]
                int[] iarr = val as int[];
                if (iarr != null && iarr.Length > 0)
                    return (int[])iarr.Clone();

                // enum[] (SimHashes[])
                Array arr = val as Array;
                if (arr != null && arr.Length > 0)
                {
                    var result = new List<int>(arr.Length);
                    for (int i = 0; i < arr.Length; i++)
                    {
                        object x = arr.GetValue(i);
                        if (x == null) continue;

                        if (x is int)
                            result.Add((int)x);
                        else if (x.GetType().IsEnum)
                            result.Add(Convert.ToInt32(x));
                    }
                    return result.Count > 0 ? result.ToArray() : null;
                }

                // IList (List<Tag>, List<SimHashes>, etc)
                IList list = val as IList;
                if (list != null && list.Count > 0)
                {
                    var result = new List<int>(list.Count);
                    for (int i = 0; i < list.Count; i++)
                    {
                        object x = list[i];
                        if (x == null) continue;

                        if (x is int)
                            result.Add((int)x);
                        else if (x is Tag)
                        {
                            // Tag -> element -> SimHashes int
                            Element element = ElementLoader.GetElement((Tag)x);
                            if (element != null) result.Add((int)element.id);
                        }
                        else if (x.GetType().IsEnum)
                            result.Add(Convert.ToInt32(x));
                    }
                    return result.Count > 0 ? result.ToArray() : null;
                }
            }
            catch
            {
                // ignore
            }
            return null;
        }
    }
}
