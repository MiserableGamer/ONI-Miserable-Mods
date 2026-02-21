using STRINGS;
using TUNING;
using UnityEngine;

namespace ControlledSublimation.Buildings
{
	/// <summary>
	/// Bleach Stone Sconce (Oxysconce) — clone of the vanilla Oxylite Sconce concept:
	/// 1×1 building that stores Bleach Stone and lets it sublimate to Chlorine.
	/// Use custom anim "bleach_stone_sconce_kanim" (or replace with your anim name).
	/// </summary>
	public class BleachStoneSconceConfig : IBuildingConfig
	{
		public const string ID = "Bleach Stone Sconce";

		// Placeholder anim; replace with your custom kanim when ready (e.g. bleach_stone_sconce_kanim).
		public const string AnimName = "bleachstone_sconce_kanim";

		public const float CapacityKg = 240f;
		public const float RefillMass = 96f; // Refill when below this (match vanilla Oxysconce)

		public override BuildingDef CreateBuildingDef()
		{
			const int width = 1;
			const int height = 1;
			string anim = AnimName;
			int hitpoints = 10;
			float construction_time = 3f;
			float[] tier = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0;
			string[] materials = MATERIALS.ALL_METALS;
			float melting_point = 800f;
			BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
			EffectorValues decor = TUNING.BUILDINGS.DECOR.BONUS.TIER0;
			EffectorValues noise = NOISE_POLLUTION.NOISY.TIER0;

			BuildingDef def = BuildingTemplates.CreateBuildingDef(
				ID,
				width,
				height,
				anim,
				hitpoints,
				construction_time,
				tier,
				materials,
				melting_point,
				build_location_rule,
				decor,
				noise,
				0.2f
			);

			def.RequiresPowerInput = false;
			def.ExhaustKilowattsWhenActive = 0f;
			def.SelfHeatKilowattsWhenActive = 0f;
			def.ViewMode = OverlayModes.Oxygen.ID;
			def.AudioCategory = "HollowMetal";
			def.Breakable = true;
			return def;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			Prioritizable.AddRef(go);
			Storage storage = go.AddOrGet<Storage>();
			storage.capacityKg = CapacityKg;
			storage.showInUI = true;
			storage.showCapacityStatusItem = true;
			storage.showCapacityAsMainStatus = true;
			ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
			manualDeliveryKG.SetStorage(storage);
			manualDeliveryKG.RequestedItemTag = SimHashes.BleachStone.CreateTag();
			manualDeliveryKG.capacity = CapacityKg;
			manualDeliveryKG.refillMass = RefillMass;
			manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;
			go.AddOrGet<StorageMeter>();
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			BuildingTemplates.DoPostConfigure(go);
		}
	}
}
