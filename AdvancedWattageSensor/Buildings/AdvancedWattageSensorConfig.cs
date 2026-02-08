using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace AdvancedWattageSensor.Buildings
{
    public class AdvancedWattageSensorConfig : IBuildingConfig
    {
        public static string ID = "AdvancedWattageSensor";

        // Custom kanim with meter/needle layers for the wattage display
        private static readonly string kanim = "OniPowerSensor_kanim";

        public override BuildingDef CreateBuildingDef()
        {
            var buildingDef = BuildingTemplates.CreateBuildingDef(
                id: ID,
                width: 1,
                height: 1,
                anim: kanim,
                hitpoints: 30,
                construction_time: 30f,
                construction_mass: BUILDINGS.CONSTRUCTION_MASS_KG.TIER0,
                construction_materials: MATERIALS.REFINED_METALS,
                melting_point: 1600f,
                build_location_rule: BuildLocationRule.Anywhere,
                decor: BUILDINGS.DECOR.PENALTY.TIER0,
                noise: NOISE_POLLUTION.NONE,
                temperature_modification_mass_scale: 0.2f);

            buildingDef.Overheatable = false;
            buildingDef.Floodable = false;
            buildingDef.Entombable = false;
            buildingDef.ViewMode = OverlayModes.Logic.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.SceneLayer = Grid.SceneLayer.Building;
            buildingDef.AlwaysOperational = true;

            buildingDef.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.OutputPort(
                    LogicSwitch.PORT_ID,
                    new CellOffset(0, 0),
                    STRINGS.BUILDINGS.PREFABS.LOGICWATTAGESENSOR.LOGIC_PORT,
                    STRINGS.BUILDINGS.PREFABS.LOGICWATTAGESENSOR.LOGIC_PORT_ACTIVE,
                    STRINGS.BUILDINGS.PREFABS.LOGICWATTAGESENSOR.LOGIC_PORT_INACTIVE,
                    true,
                    false)
            };

            SoundEventVolumeCache.instance.AddVolume(kanim, "PowerSwitch_on", NOISE_POLLUTION.NOISY.TIER3);
            SoundEventVolumeCache.instance.AddVolume(kanim, "PowerSwitch_off", NOISE_POLLUTION.NOISY.TIER3);
            GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);

            return buildingDef;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            var sensor = go.AddOrGet<Components.AdvancedWattageSensorComponent>();
            sensor.manuallyControlled = false;
            sensor.activateOnHigherThan = true;
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
        }
    }
}
