using TUNING;
using UnityEngine;
using ControlledPower.Components;

namespace ControlledPower.Buildings
{
    // Power Diode based on vanilla transformer behavior.
    // 2x1 footprint, input at (0,0), output at (1,0).
    public class PowerDiodeConfig : IBuildingConfig
    {
        public const string ID = "PowerDiode";

        public override BuildingDef CreateBuildingDef()
        {
            int width = 2;
            int height = 1;
            string anim = "power_diode_kanim";
            int hitpoints = 30;
            float construction_time = 30f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
            string[] materials = MATERIALS.RAW_METALS;
            float melting_point = 800f;
            BuildLocationRule buildLocationRule = BuildLocationRule.Anywhere;
            EffectorValues tier2 = NOISE_POLLUTION.NOISY.TIER5;

            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                ID, width, height, anim, hitpoints, construction_time,
                tier, materials, melting_point, buildLocationRule,
                BUILDINGS.DECOR.PENALTY.TIER1, tier2, 0.2f);

            def.RequiresPowerInput = true;
            def.RequiresPowerOutput = true;
            def.PowerInputOffset = new CellOffset(0, 0);   // left (anchor)
            def.PowerOutputOffset = new CellOffset(1, 0);  // right
            def.ElectricalArrowOffset = new CellOffset(1, 0);
            def.SceneLayer = Grid.SceneLayer.LogicGatesFront;
            def.ObjectLayer = ObjectLayer.LogicGate;
            def.ViewMode = OverlayModes.Power.ID;
            def.AudioCategory = "Metal";
            def.ExhaustKilowattsWhenActive = 0f;
            def.SelfHeatKilowattsWhenActive = 1f;
            def.Entombable = true;
            def.GeneratorWattageRating = 4000f;
            def.GeneratorBaseCapacity = 4000f;
            def.PermittedRotations = PermittedRotations.R360;
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding, false);
            go.AddComponent<RequireInputs>();
            BuildingDef def = go.GetComponent<Building>().Def;
            Battery battery = go.AddOrGet<Battery>();
            battery.powerSortOrder = 1000;
            battery.capacity = def.GeneratorWattageRating;
            battery.chargeWattage = def.GeneratorWattageRating;
            go.AddComponent<PowerTransformer>().powerDistributionOrder = 9;
            go.AddComponent<PowerDiodeCapacityController>();
            go.AddComponent<PowerDiodeLogicLink>();
            go.AddComponent<PowerDiodeInputConsumer>();
            go.AddOrGet<CopyBuildingSettings>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go.GetComponent<EnergyConsumer>());
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
