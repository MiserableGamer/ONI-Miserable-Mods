using System.Linq;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class MaterialClipboard
    {
        internal static int[] CopiedElementIds;

        internal static bool HasData
        {
            get { return CopiedElementIds != null && CopiedElementIds.Length > 0; }
        }

        internal static void Set(int[] elementIds)
        {
            CopiedElementIds = elementIds != null ? elementIds.ToArray() : null;
            Debug.Log("[CopyMaterialsTool] Copied elements: " + (CopiedElementIds == null ? 0 : CopiedElementIds.Length));
        }
    }
}
