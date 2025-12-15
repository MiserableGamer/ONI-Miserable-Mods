using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class MaterialClipboard
    {
        internal static Tag[] CopiedMaterialTags { get; private set; }
        internal static string SourcePrefabID { get; private set; }

        internal static bool HasData =>
            CopiedMaterialTags != null &&
            CopiedMaterialTags.Length > 0 &&
            !string.IsNullOrEmpty(SourcePrefabID);

        internal static void Clear()
        {
            CopiedMaterialTags = null;
            SourcePrefabID = null;
        }

        internal static void SetFromSource(GameObject source)
        {
            if (source == null)
            {
                Clear();
                return;
            }

            // PrefabID for "exact building type only"
            SourcePrefabID = null;
            try
            {
                var bc = source.GetComponent<BuildingComplete>();
                if (bc != null && bc.Def != null)
                    SourcePrefabID = bc.Def.PrefabID;
            }
            catch { SourcePrefabID = null; }

            // Materials: use your proven element-id path, then convert to element tags.
            int[] elementIds = BuildingMaterialUtil.TryGetConstructionElementIds(source);
            if (elementIds == null || elementIds.Length == 0)
            {
                Clear();
                return;
            }

            Tag[] tags = new Tag[elementIds.Length];
            for (int i = 0; i < elementIds.Length; i++)
            {
                try
                {
                    // ONI construction element IDs are typically SimHashes. ElementLoader can map them.
                    var el = ElementLoader.FindElementByHash((SimHashes)elementIds[i]);
                    tags[i] = el != null ? el.tag : Tag.Invalid;
                }
                catch
                {
                    tags[i] = Tag.Invalid;
                }
            }

            // Validate
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == Tag.Invalid)
                {
                    Clear();
                    return;
                }
            }

            CopiedMaterialTags = tags;
        }
    }
}
