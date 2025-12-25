// CopyMaterials/Logic/Extensions/PlacementHelpers.cs
using System.Collections.Generic;

namespace CopyMaterials.Logic
{
    public static class PlacementHelpers
    {
        public static IList<Tag> BuildSelectedElementsFromMaterial(SimHashes material)
        {
            var list = new List<Tag>();
            var elem = ElementLoader.FindElementByHash(material);
            if (elem != null) list.Add(elem.tag);
            return list;
        }
    }
}
