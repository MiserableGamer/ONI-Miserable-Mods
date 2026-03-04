using System;
using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

namespace ControlledConduits.Buildings
{
    public class GasConduitVolumeSensorConfig : ConduitSensorConfig
    {
        public override ConduitType ConduitType => ConduitType.Gas;

        public static string ID = "GasConduitVolumeSensor";

        // Fallback to vanilla anim so building always loads; Db.Initialize patch swaps to "gas_volume_sensor_kanim" when the mod anim is registered.
        private const string Anim = "gas_temperature_sensor_kanim";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef result = base.CreateBuildingDef(
                ID,
                Anim,
                TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0,
                MATERIALS.REFINED_METALS,
                new List<LogicPorts.Port>
                {
                    LogicPorts.Port.OutputPort(
                        LogicSwitch.PORT_ID,
                        new CellOffset(0, 0),
                        CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT,
                        CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT_ACTIVE,
                        CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT_INACTIVE,
                        true,
                        false)
                });
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);
            return result;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            base.DoPostConfigureComplete(go);
            var sensor = go.AddComponent<Components.ConduitVolumeSensor>();
            sensor.conduitType = ConduitType;
            sensor.Threshold = 0f;
            sensor.ActivateAboveThreshold = true;
            sensor.manuallyControlled = false;
            sensor.rangeMin = 0f;
            sensor.defaultState = false;
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
        }
    }
}
