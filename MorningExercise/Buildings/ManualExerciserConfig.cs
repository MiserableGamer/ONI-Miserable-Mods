using STRINGS;
using TUNING;
using UnityEngine;
using PeterHan.PLib.Options;

namespace MorningExercise
{
    // Building config for the Manual Exerciser - exercise-only building
    public class ManualExerciserConfig : IBuildingConfig
    {
        public const string ID = "ManualExerciser";

        public static readonly LocString NAME = MorningExerciseStrings.BUILDINGS.PREFABS.MANUALEXERCISER.NAME;
        public static readonly LocString DESC = MorningExerciseStrings.BUILDINGS.PREFABS.MANUALEXERCISER.DESC;
        public static readonly LocString EFFECT = MorningExerciseStrings.BUILDINGS.PREFABS.MANUALEXERCISER.EFFECT;

        private static string GetAnimationName()
        {
            var options = POptions.ReadSettings<Options.MorningExerciseOptions>() ?? new Options.MorningExerciseOptions();
            switch (options.AnimationVariant)
            {
                case Options.AnimationVariant.Standard:
                    return "generatormanual_kanim";
                case Options.AnimationVariant.VariantA:
                    return "generatormanual_a_kanim";
                case Options.AnimationVariant.VariantB:
                    return "generatormanual_b_kanim";
                default:
                    return "generatormanual_a_kanim";
            }
        }

        public override BuildingDef CreateBuildingDef()
        {
            // Get animation name from options
            string animName = GetAnimationName();
            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                ID,
                2,
                2,
                animName,
                30,
                30f,
                TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3,
                MATERIALS.ALL_METALS,
                1600f,
                BuildLocationRule.OnFloor,
                TUNING.BUILDINGS.DECOR.NONE,
                NOISE_POLLUTION.NOISY.TIER3,
                0.2f
            );

            def.ViewMode = OverlayModes.None.ID;
            def.AudioCategory = "Metal";
            def.Breakable = true;
            def.ForegroundLayer = Grid.SceneLayer.BuildingFront;
            def.SelfHeatKilowattsWhenActive = 0f;
            def.AddSearchTerms(SEARCH_TERMS.MEDICINE);
            def.DefaultAnimState = "off";

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            // Add components needed for exercise
            go.AddOrGet<Operational>();
            go.AddOrGet<ExerciseWorkable>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            var buildingComplete = go.AddOrGet<BuildingComplete>();
            buildingComplete.isManuallyOperated = true;
            
            go.AddOrGet<LoopingSounds>();
            Prioritizable.AddRef(go);
            
            var kbac = go.AddOrGet<KBatchedAnimController>();
            kbac.fgLayer = Grid.SceneLayer.BuildingFront;
            kbac.initialAnim = "off";
            kbac.initialMode = KAnim.PlayMode.Once;
        }
    }
}

