using STRINGS;
using TUNING;
using UnityEngine;

namespace ResourceLimpet.Buildings
{
    public class ResourceLimpetConfig : IBuildingConfig
    {
        public const string ID = "ResourceLimpet";

        public static readonly LocString NAME = (LocString)"Resource Limpet";
        public static readonly LocString DESC = (LocString)"A monitoring device that attaches to storage buildings and outputs ribbon cable signals for low and high resource levels.";
        public static readonly LocString EFFECT = (LocString)"Monitors attached storage and sends signals on ribbon cable channels when storage reaches low or high thresholds.";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(
                id: ID,
                width: 1,
                height: 1,
                anim: "resource_limpet_kanim", // Will be replaced with user's artwork
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

            // Ribbon cable output port - using two channels
            buildingDef.LogicOutputPorts = new System.Collections.Generic.List<LogicPorts.Port>
            {
                LogicPorts.Port.RibbonOutputPort(ResourceLimpetComponent.OUTPUT_PORT_ID, new CellOffset(0, 0),
                    ResourceLimpetStrings.OUTPUT_PORT_DESC,
                    ResourceLimpetStrings.OUTPUT_PORT_ACTIVE,
                    ResourceLimpetStrings.OUTPUT_PORT_INACTIVE)
            };

            GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            go.AddOrGet<ResourceLimpetComponent>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RemoveLoopingSounds(go);
        }
    }
}

