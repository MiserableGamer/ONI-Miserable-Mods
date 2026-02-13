using System;
using System.Collections.Generic;
using HarmonyLib;

namespace ControlledMods.ResourceSensor
{
    /// <summary>
    /// Static helper for the liquids/gases fix: add element (liquid/gas) mass for a cell.
    /// Same logic as ResourceSensorFIXED LogicResourceSensor.CountCell element block.
    /// Used by Harmony postfix on ResourceSensor.LogicResourceSensor.CountCell.
    /// </summary>
    public static class LogicResourceSensorHelper
    {
        public static float AddElementMassForCell(int cell, object treeFilterable)
        {
            if (treeFilterable == null) return 0f;
            try
            {
                var tagsProp = AccessTools.Property(treeFilterable.GetType(), "AcceptedTags");
                var tags = tagsProp?.GetValue(treeFilterable) as ICollection<Tag>;
                if (tags == null || tags.Count == 0) return 0f;

                float totalMass = 0f;
                if (Grid.IsValidCell(cell))
                {
                    Element cellElement = Grid.Element[cell];
                    if (cellElement != null && cellElement.id != SimHashes.Vacuum)
                    {
                        foreach (var tag in tags)
                        {
                            Element filterElement = ElementLoader.GetElement(tag);
                            if (filterElement != null && filterElement.id == cellElement.id)
                            {
                                totalMass += Grid.Mass[cell];
                                break;
                            }
                        }
                    }
                }
                return totalMass;
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"ResourceSensor AddElementMassForCell: {ex.Message}");
                return 0f;
            }
        }
    }
}
