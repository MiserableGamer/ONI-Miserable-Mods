using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace ControlledAutomation.Buildings
{
    public class TemperatureRangeSensorConfig : IBuildingConfig
    {
        public const string ID = "TemperatureRangeSensor";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                ID, 1, 1,
                "switchthermal_kanim",
                30, 30f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER0,
                MATERIALS.REFINED_METALS,
                1600f,
                BuildLocationRule.Anywhere,
                BUILDINGS.DECOR.PENALTY.TIER0,
                NOISE_POLLUTION.NONE,
                0.2f
            );

            def.Overheatable = false;
            def.Floodable = false;
            def.Entombable = false;
            def.ViewMode = OverlayModes.Logic.ID;
            def.AudioCategory = "Metal";
            def.SceneLayer = Grid.SceneLayer.Building;
            def.AlwaysOperational = true;

            def.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.OutputPort(
                    LogicSwitch.PORT_ID,
                    new CellOffset(0, 0),
                    STRINGS.CONTROLLEDAUTOMATION.BUILDINGS.PREFABS.TEMPERATURERANGESENSOR.LOGIC_PORT,
                    STRINGS.CONTROLLEDAUTOMATION.BUILDINGS.PREFABS.TEMPERATURERANGESENSOR.LOGIC_PORT_ACTIVE,
                    STRINGS.CONTROLLEDAUTOMATION.BUILDINGS.PREFABS.TEMPERATURERANGESENSOR.LOGIC_PORT_INACTIVE,
                    true, false
                )
            };

            SoundEventVolumeCache.instance.AddVolume("switchthermal_kanim", "PowerSwitch_on", NOISE_POLLUTION.NOISY.TIER3);
            SoundEventVolumeCache.instance.AddVolume("switchthermal_kanim", "PowerSwitch_off", NOISE_POLLUTION.NOISY.TIER3);
            GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);

            return def;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            var sensor = go.AddOrGet<Components.TemperatureRangeSensor>();
            sensor.manuallyControlled = false;
            sensor.minTemp = 0f;
            sensor.maxTemp = 1273.15f;
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
        }
    }
}
