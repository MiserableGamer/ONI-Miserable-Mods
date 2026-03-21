using System.Collections.Generic;
using Database;
using STRINGS;
using TUNING;
using UnityEngine;

namespace ControlledMorale
{
    public class AlcoholBreweryConfig : IBuildingConfig
    {
        public const string ID = "AlcoholBrewery";

        private static readonly ConduitPortInfo EthanolPort = new ConduitPortInfo(
            ConduitType.Liquid,
            new CellOffset(0, 2));

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 3;
            int height = 3;
            string anim = "alcohol_brewery_kanim";
            int hitpoints = 30;
            float construction_time = 60f;
            float[] tier = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] allMetals = MATERIALS.ALL_METALS;
            float melting_point = 1600f;
            BuildLocationRule locationRule = BuildLocationRule.OnFloor;
            EffectorValues noise = NOISE_POLLUTION.NOISY.TIER1;
            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                id,
                width,
                height,
                anim,
                hitpoints,
                construction_time,
                tier,
                allMetals,
                melting_point,
                locationRule,
                TUNING.BUILDINGS.DECOR.NONE,
                noise,
                0.2f);
            def.RequiresPowerInput = true;
            def.EnergyConsumptionWhenActive = 360f;
            def.SelfHeatKilowattsWhenActive = 2f;
            def.ExhaustKilowattsWhenActive = 0.5f;
            def.ViewMode = OverlayModes.LiquidConduits.ID;
            def.AudioCategory = "HollowMetal";
            def.Floodable = true;
            def.Overheatable = true;
            def.InputConduitType = ConduitType.Liquid;
            def.UtilityInputOffset = new CellOffset(0, 1);
            def.PowerInputOffset = new CellOffset(1, 0);
            // Left 1 and down 1 from bottom-center (1,0) → (0,0); stays clear of water on (0,1)
            def.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
            def.PermittedRotations = PermittedRotations.FlipH;
            def.AddSearchTerms(SEARCH_TERMS.MORALE);
            def.AddSearchTerms(SEARCH_TERMS.WATER);
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            AlcoholBrewery brewery = go.AddOrGet<AlcoholBrewery>();
            brewery.choreType = Db.Get().ChoreTypes.Fabricate;
            brewery.fetchChoreTypeIdHash = Db.Get().ChoreTypes.FabricateFetch.IdHash;
            brewery.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
            brewery.duplicantOperated = false;
            brewery.showProgressBar = true;
            brewery.heatedTemperature = 318.15f;
            go.AddOrGet<FabricatorIngredientStatusManager>();
            go.AddOrGet<CopyBuildingSettings>();
            ComplexFabricatorWorkable workable = go.AddOrGet<ComplexFabricatorWorkable>();
            workable.overrideAnims = new KAnimFile[]
            {
                Assets.GetAnim("anim_interacts_cookstation_kanim")
            };
            BuildingTemplates.CreateComplexFabricatorStorage(go, brewery);
            brewery.inStorage.SetDefaultStoredItemModifiers(StandardStorage);
            brewery.buildStorage.SetDefaultStoredItemModifiers(StandardStorage);
            brewery.outStorage.SetDefaultStoredItemModifiers(OutputStorage);
            brewery.outStorage.capacityKg = 500f;
            brewery.outStorage.allowItemRemoval = false;
            brewery.storeProduced = true;
            brewery.keepExcessLiquids = true;
            brewery.outputOffset = new Vector3(0.8f, 0.5f, 0f);

            ConduitConsumer waterConsumer = go.AddOrGet<ConduitConsumer>();
            waterConsumer.conduitType = ConduitType.Liquid;
            waterConsumer.storage = brewery.inStorage;
            waterConsumer.capacityTag = ElementLoader.FindElementByHash(SimHashes.Water).tag;
            waterConsumer.capacityKG = 80f;
            waterConsumer.alwaysConsume = true;
            waterConsumer.forceAlwaysSatisfied = true;
            waterConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;

            ConduitConsumer ethanolConsumer = go.AddComponent<ConduitConsumer>();
            ethanolConsumer.conduitType = ConduitType.Liquid;
            ethanolConsumer.storage = brewery.inStorage;
            ethanolConsumer.capacityTag = ElementLoader.FindElementByHash(SimHashes.Ethanol).tag;
            ethanolConsumer.capacityKG = 80f;
            ethanolConsumer.alwaysConsume = true;
            ethanolConsumer.forceAlwaysSatisfied = true;
            ethanolConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;
            ethanolConsumer.useSecondaryInput = true;

            AttachSecondaryPort(go);

            ManualDeliveryKG manualWater = go.AddComponent<ManualDeliveryKG>();
            manualWater.SetStorage(brewery.inStorage);
            manualWater.RequestedItemTag = ElementLoader.FindElementByHash(SimHashes.Water).tag;
            manualWater.capacity = 80f;
            manualWater.refillMass = 20f;
            manualWater.MinimumMass = 5f;
            manualWater.choreTypeIDHash = Db.Get().ChoreTypes.MachineFetch.IdHash;

            ManualDeliveryKG manualEthanol = go.AddComponent<ManualDeliveryKG>();
            manualEthanol.SetStorage(brewery.inStorage);
            manualEthanol.RequestedItemTag = ElementLoader.FindElementByHash(SimHashes.Ethanol).tag;
            manualEthanol.capacity = 80f;
            manualEthanol.refillMass = 20f;
            manualEthanol.MinimumMass = 3f;
            manualEthanol.choreTypeIDHash = Db.Get().ChoreTypes.MachineFetch.IdHash;

            AlcoholBreweryPickupWorkable pickupWorkable = go.AddOrGet<AlcoholBreweryPickupWorkable>();
            pickupWorkable.storage = brewery.outStorage;

            Prioritizable.AddRef(go);
            go.AddOrGetDef<RocketUsageRestriction.Def>();
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            base.DoPostConfigurePreview(def, go);
            AttachSecondaryPort(go);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            base.DoPostConfigureUnderConstruction(go);
            AttachSecondaryPort(go);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            AttachSecondaryPort(go);
            RequireInputs reqIn = go.GetComponent<RequireInputs>();
            reqIn.SetRequirements(true, false);
            reqIn.requireConduitHasMass = false;
            SymbolOverrideControllerUtil.AddToPrefab(go);
            go.AddOrGet<LogicOperationalController>();
            var vanillaFabSm = go.GetComponent<ComplexFabricatorSM>();
            if (vanillaFabSm != null)
                Object.Destroy(vanillaFabSm);
        }

        private static void AttachSecondaryPort(GameObject go)
        {
            if (go.GetComponent<ConduitSecondaryInput>() != null)
                return;
            go.AddComponent<ConduitSecondaryInput>().portInfo = EthanolPort;
        }

        private static readonly List<Storage.StoredItemModifier> StandardStorage = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Preserve
        };

        private static readonly List<Storage.StoredItemModifier> OutputStorage = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Preserve,
            Storage.StoredItemModifier.Insulate
        };
    }
}
