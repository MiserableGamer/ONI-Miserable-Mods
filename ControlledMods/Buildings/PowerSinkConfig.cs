using TUNING;
using UnityEngine;

namespace ControlledMods.Buildings
{
    public class PowerSinkConfig : IBuildingConfig
    {
        public const string ID = "ControlledMods_PowerSink";
        public const float DEFAULT_WATTAGE = 2000f;

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                ID,
                1, 1,
                "free_energy_kanim",
                100,
                30f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
                MATERIALS.REFINED_METALS,
                2400f,
                BuildLocationRule.Anywhere,
                BUILDINGS.DECOR.BONUS.TIER5,
                NOISE_POLLUTION.NOISY.TIER4
            );

            def.Overheatable = false;
            def.Floodable = false;
            def.Entombable = false;
            def.RequiresPowerInput = true;
            def.EnergyConsumptionWhenActive = DEFAULT_WATTAGE;
            def.PowerInputOffset = new CellOffset(0, 0);
            def.SelfHeatKilowattsWhenActive = 0f;
            def.ExhaustKilowattsWhenActive = 0f;
            def.ViewMode = OverlayModes.Power.ID;
            def.AudioCategory = "HollowMetal";
            def.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<PowerSink>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>();
        }
    }
}
