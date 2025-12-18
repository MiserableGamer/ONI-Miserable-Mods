using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using RomenH.Common;

namespace RomenH.Thresholds
{
	public class Mod : UserMod2
	{
		internal static string ModFolder
		{ get; private set; }

	public override void OnLoad(Harmony harmony)
	{
		ModCommon.Init("Threshold Walls", harmony);
		PUtil.InitLibrary();

		var options = new POptions();
		options.RegisterOptions(this, typeof(ModSettings));

		ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		// Force initialization of static LocString fields in config classes
		// This ensures they're registered before buildings are created
		_ = CautionThresholdWallConfig.Name;
		_ = CautionThresholdWallConfig.Desc;
		_ = CautionThresholdWallConfig.Effect;
		_ = LogicThresholdWallConfig.Name;
		_ = LogicThresholdWallConfig.Desc;
		_ = LogicThresholdWallConfig.Effect;
		_ = MetalThresholdWallConfig.Name;
		_ = MetalThresholdWallConfig.Desc;
		_ = MetalThresholdWallConfig.Effect;
		_ = PlasticThresholdWallConfig.Name;
		_ = PlasticThresholdWallConfig.Desc;
		_ = PlasticThresholdWallConfig.Effect;
		_ = ThresholdWallConfig.Name;
		_ = ThresholdWallConfig.Desc;
		_ = ThresholdWallConfig.Effect;

		// Re-register strings now that static fields are initialized
		StringUtils.RegisterAllLocStrings();

		base.OnLoad(harmony);
	}

		public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<KMod.Mod> mods)
		{
			if (Type.GetType("DrywallHidesPipes.DrywallPatch, DrywallHidesPipes", false, false) != null)
			{
				ModSettings.Instance.HidePipesAndWires = true;
			}

			base.OnAllModsLoaded(harmony, mods);
		}
	}
}
