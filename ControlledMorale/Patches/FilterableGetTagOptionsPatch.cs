using System.Collections.Generic;
using HarmonyLib;

namespace ControlledMorale
{
    /// <summary>
    /// Vanilla <see cref="Filterable.GetTagOptions"/> uses <see cref="GameTagExtensions.Create(SimHashes)"/>,
    /// which is <c>TagManager.Create(id.ToString())</c>. Custom YAML elements use hash values that are not
    /// named in the <see cref="SimHashes"/> enum, so <c>ToString()</c> is numeric and does not match
    /// <see cref="Element.tag"/> (created from the element id string). Filter UIs then show wrong labels /
    /// icons and <see cref="ElementLoader.GetElement(Tag)"/> fails for the selected row.
    /// </summary>
    [HarmonyPatch(typeof(Filterable), nameof(Filterable.GetTagOptions))]
    public static class FilterableGetTagOptionsPatch
    {
        public static void Postfix(Filterable __instance, ref Dictionary<Tag, HashSet<Tag>> __result)
        {
            if (__instance.filterElementState != Filterable.ElementState.Liquid
                && __instance.filterElementState != Filterable.ElementState.Gas)
            {
                return;
            }

            bool wantLiquid = __instance.filterElementState == Filterable.ElementState.Liquid;

            foreach (Element element in ElementLoader.elements)
            {
                if (element == null || element.disabled)
                    continue;
                if (wantLiquid && !element.IsLiquid)
                    continue;
                if (!wantLiquid && !element.IsGas)
                    continue;

                Tag wrong = GameTagExtensions.Create(element.id);
                Tag correct = element.tag;
                if (wrong == correct)
                    continue;

                Tag category = element.GetMaterialCategoryTag();
                if (!__result.TryGetValue(category, out HashSet<Tag> tags))
                    continue;
                if (tags.Remove(wrong))
                    tags.Add(correct);
            }
        }
    }
}
