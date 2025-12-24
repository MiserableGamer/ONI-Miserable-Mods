using STRINGS;
using TUNING;
using UnityEngine;

namespace ResourceLimpet.Buildings
{
    public class RibbonNotifierConfig : IBuildingConfig
    {
        public const string ID = "RibbonNotifier";

        public static readonly LocString NAME = (LocString)"Ribbon Notifier";
        public static readonly LocString DESC = (LocString)"A notification device that receives ribbon cable signals and displays different notifications based on the activated channel.";
        public static readonly LocString EFFECT = (LocString)"Receives ribbon cable input and displays notifications depending on which channel is activated.";

        public override BuildingDef CreateBuildingDef()
        {
            // Create building def using standard Klei LogicAlarm anim
            // Use the same settings as LogicAlarm (Automated Notifier)
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(
                id: ID,
                width: 1,
                height: 1,
                anim: "logic_alarm_kanim", // Standard Klei anim for Automated Notifier
                hitpoints: 30,
                construction_time: 10f,
                construction_mass: TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1,
                construction_materials: MATERIALS.REFINED_METALS,
                melting_point: 1600f,
                build_location_rule: BuildLocationRule.Anywhere,
                decor: DECOR.NONE,
                noise: NOISE_POLLUTION.NONE);

            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.Entombable = false;
            buildingDef.AudioCategory = "Metal";
            buildingDef.AudioSize = "small";
            buildingDef.BaseTimeUntilRepair = -1f;
            buildingDef.DefaultAnimState = "off";
            buildingDef.ObjectLayer = ObjectLayer.LogicGate;
            buildingDef.SceneLayer = Grid.SceneLayer.LogicGatesFront;
            buildingDef.PermittedRotations = PermittedRotations.R360;

            // Ribbon cable input port instead of single wire
            buildingDef.LogicInputPorts = new System.Collections.Generic.List<LogicPorts.Port>
            {
                LogicPorts.Port.RibbonInputPort(RibbonNotifierComponent.INPUT_PORT_ID, new CellOffset(0, 0),
                    ResourceLimpetStrings.INPUT_PORT_DESC,
                    ResourceLimpetStrings.INPUT_PORT_ACTIVE,
                    ResourceLimpetStrings.INPUT_PORT_INACTIVE,
                    true, false)
            };

            GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            // Remove the original LogicAlarm component if it exists
            var oldAlarm = go.GetComponent<LogicAlarm>();
            if (oldAlarm != null)
            {
                UnityEngine.Object.DestroyImmediate(oldAlarm);
            }

            // Add our ribbon notifier component
            go.AddOrGet<RibbonNotifierComponent>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RemoveLoopingSounds(go);
        }
    }
}

