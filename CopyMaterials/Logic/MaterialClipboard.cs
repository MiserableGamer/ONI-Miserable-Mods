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

            SourcePrefabID = null;
            try
            {
                var bc = source.GetComponent<BuildingComplete>();
                if (bc != null && bc.Def != null)
                    SourcePrefabID = bc.Def.PrefabID;
            }
            catch { SourcePrefabID = null; }

            // For a completed building, PrimaryElement.Element.tag is the material tag.
            // This is what Build/UnderConstruction systems tend to accept.
            Tag matTag = Tag.Invalid;
            try
            {
                var pe = source.GetComponent<PrimaryElement>();
                if (pe != null && pe.Element != null)
                    matTag = pe.Element.tag;
            }
            catch { matTag = Tag.Invalid; }

            if (matTag == Tag.Invalid)
            {
                Clear();
                return;
            }

            // Most buildings are 1-slot. Doors are 1-slot.
            // If you later want multi-slot, we can expand this by reading selected elements from a Constructable.
            CopiedMaterialTags = new[] { matTag };
        }
    }
}
