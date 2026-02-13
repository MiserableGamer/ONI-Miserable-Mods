using System.Collections.Generic;
using System.Linq;
using TUNING;

namespace ControlledMods.ResourceSensor
{
    /// <summary>
    /// Static helper for the liquids/gases fix: same storage filter list as ResourceSensorFIXED.
    /// Used by Harmony postfix on ResourceSensor.LogicResourceSensorConfig.ConfigureBuildingTemplate.
    /// </summary>
    public static class LogicResourceSensorConfigHelper
    {
        public static List<Tag> GetStorageFilterList()
        {
            return TUNING.STORAGEFILTERS.FOOD
                .Concat(TUNING.STORAGEFILTERS.NOT_EDIBLE_SOLIDS)
                .Concat(TUNING.STORAGEFILTERS.LIQUIDS)
                .Concat(TUNING.STORAGEFILTERS.GASES)
                .ToList();
        }
    }
}
