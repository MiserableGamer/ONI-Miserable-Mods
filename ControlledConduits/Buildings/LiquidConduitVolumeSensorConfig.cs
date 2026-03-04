using System;
using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

namespace ControlledConduits.Buildings
{
    public class LiquidConduitVolumeSensorConfig : ConduitSensorConfig
    {
        public override ConduitType ConduitType => ConduitType.Liquid;

        public static string ID = "LiquidConduitVolumeSensor";

        private const string Anim = "liquid_temperature_sensor_kanim";

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
                        CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT,
                        CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT_ACTIVE,
                        CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT_INACTIVE,
                        true,
                        false)
                });
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
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
