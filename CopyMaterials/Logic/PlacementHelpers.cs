using System;
using System.Collections.Generic;
using UnityEngine;

namespace CopyMaterials.Logic
{
    public static class PlacementHelpers
    {
        public static IList<Tag> BuildSelectedElementsFromMaterial(SimHashes material)
        {
            IList<Tag> selectedElements = new List<Tag>();
            try
            {
                Tag tag = default(Tag);
                bool tagCreated = false;

                try
                {
                    tag = material.CreateTag();
                    tagCreated = true;
                }
                catch
                {
                    try
                    {
                        var elem = ElementLoader.FindElementByHash(material);
                        if (elem != null)
                        {
                            tag = elem.tag;
                            tagCreated = true;
                        }
                    }
                    catch { }
                }

                if (!tagCreated)
                {
                    try
                    {
                        var name = material.ToString();
                        tag = TagManager.Create(name);
                        tagCreated = true;
                    }
                    catch { }
                }

                if (tagCreated) selectedElements.Add(tag);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CopyMaterials] Failed to create Tag for material {material}: {e}");
            }

            return selectedElements;
        }
    }
}