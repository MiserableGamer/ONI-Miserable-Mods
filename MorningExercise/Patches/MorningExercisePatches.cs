using Database;
using HarmonyLib;
using Klei.AI;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

using MORNINGEXERCISE = MorningExercise.MorningExerciseStrings.UI.SCHEDULEGROUPS.MORNINGEXERCISE;

namespace MorningExercise
{
    // All Harmony patches for the mod
    public static class MorningExercisePatches
    {
        public const string WARMUP_EFFECT_ID = "MorningExercise_WarmUp";
        public const string BIONIC_WARMUP_EFFECT_ID = "MorningExercise_BionicWarmUp";

        public static Options.MorningExerciseOptions Options { get; set; }
        public static ScheduleBlockType ExerciseBlock { get; set; }
        public static ColorStyleSetting ExerciseColor { get; set; }
        public static ScheduleGroup ExerciseGroup { get; set; }
        public static ChoreType ExerciseChoreType { get; set; }
        public static ChoreType WaitingForExerciseChoreType { get; set; }
        public static Effect WarmUpEffect { get; set; }
        public static Effect BionicWarmUpEffect { get; set; }

        // Register all strings when localization initializes
        [HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
        public static class Localization_Initialize_Patch
        {
            internal static void Postfix()
            {
                LocString.CreateLocStringKeys(typeof(MorningExerciseStrings.DUPLICANTS));
                LocString.CreateLocStringKeys(typeof(MorningExerciseStrings.BUILDINGS.PREFABS));
                LocString.CreateLocStringKeys(typeof(MorningExerciseStrings.UI.SCHEDULEGROUPS));
                
                // Fallback: manually add building strings in case CreateLocStringKeys doesn't map them
                Strings.Add("STRINGS.BUILDINGS.PREFABS.MANUALEXERCISER.NAME", MorningExerciseStrings.BUILDINGS.PREFABS.MANUALEXERCISER.NAME);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.MANUALEXERCISER.DESC", MorningExerciseStrings.BUILDINGS.PREFABS.MANUALEXERCISER.DESC);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.MANUALEXERCISER.EFFECT", MorningExerciseStrings.BUILDINGS.PREFABS.MANUALEXERCISER.EFFECT);
                
                Debug.Log("[MorningExercise] Strings registered in Localization.Initialize");
            }
        }

        // Add custom exercise chore types
        [HarmonyPatch(typeof(ChoreTypes), MethodType.Constructor, typeof(ResourceSet))]
        public static class ChoreTypes_Constructor_Patch
        {
            internal static void Postfix(ChoreTypes __instance)
            {
                // Use reflection to call the private Add method
                var addMethod = typeof(ChoreTypes).GetMethod("Add", 
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(string), typeof(string[]), typeof(string), typeof(string[]), 
                            typeof(string), typeof(string), typeof(string), typeof(bool), typeof(int), typeof(string) },
                    null);

                if (addMethod != null)
                {
                    ExerciseChoreType = (ChoreType)addMethod.Invoke(__instance, new object[]
                    {
                        "MorningExercise",
                        new string[0],
                        "",
                        new string[0],
                        (string)MorningExerciseStrings.DUPLICANTS.CHORES.EXERCISE.NAME,
                        (string)MorningExerciseStrings.DUPLICANTS.CHORES.EXERCISE.STATUS,
                        (string)MorningExerciseStrings.DUPLICANTS.CHORES.EXERCISE.TOOLTIP,
                        false,
                        -1,
                        null
                    });

                    WaitingForExerciseChoreType = (ChoreType)addMethod.Invoke(__instance, new object[]
                    {
                        "WaitingForExercise",
                        new string[0],
                        "",
                        new string[0],
                        (string)MorningExerciseStrings.DUPLICANTS.CHORES.WAITINGFOREXERCISE.NAME,
                        (string)MorningExerciseStrings.DUPLICANTS.CHORES.WAITINGFOREXERCISE.STATUS,
                        (string)MorningExerciseStrings.DUPLICANTS.CHORES.WAITINGFOREXERCISE.TOOLTIP,
                        true,
                        -1,
                        null
                    });

                    Debug.Log("[MorningExercise] Custom chore types registered");
                }
                else
                {
                    Debug.LogError("[MorningExercise] Failed to find ChoreTypes.Add method!");
                }
            }
        }

        // Create the exercise schedule block type
        [HarmonyPatch(typeof(ScheduleBlockTypes), MethodType.Constructor, typeof(ResourceSet))]
        public static class ScheduleBlockTypes_Constructor_Patch
        {
            internal static void Postfix(ScheduleBlockTypes __instance)
            {
                var color = ExerciseColor != null ? ExerciseColor.activeColor : Color.green;
                ExerciseBlock = __instance.Add(new ScheduleBlockType(MORNINGEXERCISE.ID, __instance,
                    MORNINGEXERCISE.NAME, MORNINGEXERCISE.DESCRIPTION, color));

                Debug.Log("[MorningExercise] Schedule block type registered");
            }
        }

        // Create the exercise schedule group
        [HarmonyPatch(typeof(ScheduleGroups), MethodType.Constructor, typeof(ResourceSet))]
        public static class ScheduleGroups_Constructor_Patch
        {
            internal static void Postfix(ScheduleGroups __instance)
            {
                if (__instance == null)
                {
                    Debug.LogError("[MorningExercise] ScheduleGroups instance is null!");
                    return;
                }

                if (ExerciseBlock == null)
                {
                    Debug.LogError("[MorningExercise] Exercise block type not defined!");
                    return;
                }

                Color groupColor = ExerciseColor != null ? ExerciseColor.inactiveColor : new Color(0.2f, 0.7f, 0.3f, 1.0f);

                // Create schedule group with only the exercise block (no recreation)
                ExerciseGroup = __instance.Add(
                    MORNINGEXERCISE.ID, 
                    0, 
                    (string)MORNINGEXERCISE.NAME, 
                    (string)MORNINGEXERCISE.DESCRIPTION,
                    groupColor, 
                    (string)MORNINGEXERCISE.NOTIFICATION_TOOLTIP,
                    new List<ScheduleBlockType> { ExerciseBlock });

                Debug.Log("[MorningExercise] Schedule group registered");
            }
        }

        // Create the Warm Up effects and status items
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            internal static void Postfix(Db __instance)
            {
                Options = POptions.ReadSettings<Options.MorningExerciseOptions>() ?? new Options.MorningExerciseOptions();

                // Create Warm Up effect for regular dupes (Athletics bonus)
                WarmUpEffect = new Effect(
                    WARMUP_EFFECT_ID,
                    MorningExerciseStrings.DUPLICANTS.MODIFIERS.WARMUP.NAME,
                    MorningExerciseStrings.DUPLICANTS.MODIFIERS.WARMUP.DESCRIPTION,
                    Options.BuffDuration,
                    true,
                    true,
                    false,
                    null,
                    -1f,
                    0f,
                    null,
                    ""
                );

                // Add Athletics bonus modifier
                WarmUpEffect.Add(new AttributeModifier(
                    __instance.Attributes.Athletics.Id,
                    Options.AthleticsBonus,
                    MorningExerciseStrings.DUPLICANTS.MODIFIERS.WARMUP.NAME,
                    false,
                    false
                ));

                __instance.effects.Add(WarmUpEffect);

                // Create Bionic Warm Up effect (Morale bonus for bionics)
                BionicWarmUpEffect = new Effect(
                    BIONIC_WARMUP_EFFECT_ID,
                    MorningExerciseStrings.DUPLICANTS.MODIFIERS.BIONICWARMUP.NAME,
                    MorningExerciseStrings.DUPLICANTS.MODIFIERS.BIONICWARMUP.DESCRIPTION,
                    Options.BionicBuffDuration,
                    true,
                    true,
                    false,
                    null,
                    -1f,
                    0f,
                    null,
                    ""
                );

                // Add Morale bonus modifier for bionics
                BionicWarmUpEffect.Add(new AttributeModifier(
                    __instance.Attributes.QualityOfLife.Id,
                    Options.BionicMoraleBonus,
                    MorningExerciseStrings.DUPLICANTS.MODIFIERS.BIONICWARMUP.NAME,
                    false,
                    false
                ));

                __instance.effects.Add(BionicWarmUpEffect);

                Debug.Log("[MorningExercise] Effects registered - Regular: +" + Options.AthleticsBonus + 
                    " Athletics (" + Options.BuffDuration + "s), Bionic: +" + Options.BionicMoraleBonus + 
                    " Morale (" + Options.BionicBuffDuration + "s), Exercise time: " + Options.ExerciseDuration + "s");
            }
        }

        // Add Manual Exerciser to Medicine I tech tree
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_ManualExerciser_Tech_Patch
        {
            internal static void Postfix()
            {
                var medicineTech = Db.Get().Techs.Get("MedicineI");
                if (medicineTech != null)
                {
                    if (!medicineTech.unlockedItemIDs.Contains(ManualExerciserConfig.ID))
                    {
                        medicineTech.unlockedItemIDs.Add(ManualExerciserConfig.ID);
                        Debug.Log("[MorningExercise] Manual Exerciser added to Medicine I tech");
                    }
                    else
                    {
                        Debug.Log("[MorningExercise] Manual Exerciser already in Medicine I tech");
                    }
                }
                else
                {
                    Debug.LogWarning("[MorningExercise] Could not find Medicine I tech!");
                    var allTechs = Db.Get().Techs.resources;
                    Debug.LogWarning($"[MorningExercise] Available techs: {string.Join(", ", allTechs.Select(t => t.Id))}");
                }
            }
        }

        // Add Manual Exerciser to Medicine category in build menu
        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_ManualExerciser_Patch
        {
            internal static void Postfix()
            {
                var buildingDef = Assets.GetBuildingDef(ManualExerciserConfig.ID);
                if (buildingDef == null)
                {
                    Debug.LogWarning("[MorningExercise] Building def not found during LoadGeneratedBuildings!");
                    return;
                }
                
                Debug.Log($"[MorningExercise] Adding Manual Exerciser to plan screen - Def: {buildingDef.PrefabID}");
                
                // Find Medicine category in PLANORDER
                bool foundMedicine = false;
                for (int i = 0; i < TUNING.BUILDINGS.PLANORDER.Count; i++)
                {
                    var planInfo = TUNING.BUILDINGS.PLANORDER[i];
                    string categoryName = HashCache.Get().Get(planInfo.category);
                    Debug.Log($"[MorningExercise] Checking PLANORDER category: {categoryName}");
                    
                    if (categoryName == "medical")
                    {
                        foundMedicine = true;
                        if (planInfo.buildingAndSubcategoryData == null)
                        {
                            planInfo.buildingAndSubcategoryData = new List<KeyValuePair<string, string>>();
                        }
                        
                        // Use same subcategory as other buildings in this category
                        string subcategory = "uncategorized";
                        if (planInfo.buildingAndSubcategoryData.Count > 0)
                        {
                            subcategory = planInfo.buildingAndSubcategoryData[0].Value;
                        }
                        
                        bool alreadyAdded = planInfo.buildingAndSubcategoryData.Any(
                            kvp => kvp.Key == ManualExerciserConfig.ID);
                        
                        if (!alreadyAdded)
                        {
                            planInfo.buildingAndSubcategoryData.Add(
                                new KeyValuePair<string, string>(
                                    ManualExerciserConfig.ID, 
                                    subcategory));
                            
                            TUNING.BUILDINGS.PLANORDER[i] = planInfo;
                            
                            Debug.Log($"[MorningExercise] Manual Exerciser added to Medicine category in PLANORDER (subcategory: {subcategory})");
                        }
                        else
                        {
                            Debug.Log("[MorningExercise] Manual Exerciser already in Medicine category");
                        }
                        break;
                    }
                }
                
                if (!foundMedicine)
                {
                    Debug.LogWarning("[MorningExercise] Could not find Medicine category in PLANORDER!");
                    Debug.LogWarning($"[MorningExercise] Available categories: {string.Join(", ", TUNING.BUILDINGS.PLANORDER.Select(pi => HashCache.Get().Get(pi.category)))}");
                }
            }
        }

        // Copy animation setup from ManualGenerator after prefab is created
        [HarmonyPatch(typeof(Assets), "CreatePrefabs")]
        public static class Assets_CreatePrefabs_ManualExerciser_Patch
        {
            internal static void Postfix()
            {
                var exerciserPrefab = Assets.GetPrefab(ManualExerciserConfig.ID);
                var buildingDef = Assets.GetBuildingDef(ManualExerciserConfig.ID);
                
                if (exerciserPrefab == null)
                {
                    Debug.LogError("[MorningExercise] Could not find Manual Exerciser prefab after creation!");
                    return;
                }
                
                if (buildingDef == null)
                {
                    Debug.LogError("[MorningExercise] Manual Exerciser prefab exists but building def is null!");
                    return;
                }
                
                // Copy animation setup from ManualGenerator
                var manualGeneratorPrefab = Assets.GetPrefab("ManualGenerator");
                if (manualGeneratorPrefab != null)
                {
                    var generatorKbac = manualGeneratorPrefab.GetComponent<KBatchedAnimController>();
                    var exerciserKbac = exerciserPrefab.GetComponent<KBatchedAnimController>();
                    
                    if (generatorKbac != null && exerciserKbac != null)
                    {
                        exerciserKbac.fgLayer = generatorKbac.fgLayer;
                        exerciserKbac.initialAnim = generatorKbac.initialAnim;
                        exerciserKbac.initialMode = generatorKbac.initialMode;
                        Debug.Log("[MorningExercise] Copied KBatchedAnimController settings from ManualGenerator");
                    }
                }
                
                Debug.Log($"[MorningExercise] Manual Exerciser prefab verified - Prefab: {exerciserPrefab.name}, Def: {buildingDef.PrefabID}");
            }
        }

        // Add ExerciseMonitor to all dupes
        [HarmonyPatch(typeof(BaseMinionConfig), nameof(BaseMinionConfig.BaseMinion))]
        public static class BaseMinionConfig_BaseMinion_Patch
        {
            internal static void Postfix(GameObject __result)
            {
                if (__result != null)
                {
                    __result.AddOrGet<ExerciseMonitor>();
                }
            }
        }

        // Add exercise button to schedule screen
        [HarmonyPatch(typeof(ScheduleScreenEntry), "Setup")]
        public static class ScheduleScreenEntry_Setup_Patch
        {
            private static FieldInfo paintButtonBathtimeField;
            private static MethodInfo configPaintButtonMethod;
            private static MethodInfo refreshPaintButtonsMethod;

            internal static void Postfix(ScheduleScreenEntry __instance)
            {
                if (ExerciseBlock == null || ExerciseGroup == null) return;

                // Cache reflection fields for performance
                if (paintButtonBathtimeField == null)
                {
                    paintButtonBathtimeField = typeof(ScheduleScreenEntry).GetField("PaintButtonBathtime", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (configPaintButtonMethod == null)
                {
                    configPaintButtonMethod = typeof(ScheduleScreenEntry).GetMethod("ConfigPaintButton",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (refreshPaintButtonsMethod == null)
                {
                    refreshPaintButtonsMethod = typeof(ScheduleScreenEntry).GetMethod("RefreshPaintButtons",
                        BindingFlags.Public | BindingFlags.Instance);
                }

                // Clone the bathtime button and configure it for exercise
                var bathtime = paintButtonBathtimeField?.GetValue(__instance) as GameObject;
                if (bathtime != null)
                {
                    var button = Util.KInstantiateUI(bathtime, bathtime.transform.parent.gameObject);
                    if (button.TryGetComponent(out MultiToggle toggle))
                    {
                        ref var ads = ref toggle.states[0].additional_display_settings[0];
                        ads.color = ExerciseColor.inactiveColor;
                        ads.color_on_hover = ExerciseColor.hoverColor;
                        toggle.states[1].additional_display_settings[0].color = ExerciseColor.inactiveColor;
                    }
                    button.name = MORNINGEXERCISE.ID;
                    
                    var sprite = Def.GetUISprite(Assets.GetPrefab("ManualGenerator")).first;
                    configPaintButtonMethod?.Invoke(__instance, new object[] { button, ExerciseGroup, sprite });
                    refreshPaintButtonsMethod?.Invoke(__instance, null);
                }
            }
        }

        // Patch ScheduleBlock.get_allowed_types to handle case where ScheduleGroup doesn't exist yet
        [HarmonyPatch(typeof(ScheduleBlock), "get_allowed_types")]
        public static class ScheduleBlock_get_allowed_types_Patch
        {
            internal static void Finalizer(Exception __exception, ScheduleBlock __instance, ref List<ScheduleBlockType> __result)
            {
                // If an exception occurred and it's about MorningExercise ScheduleGroup not being found,
                // return empty list instead of throwing - this happens during initialization
                if (__exception != null && __exception.Message != null && __exception.Message.Contains("MorningExercise"))
                {
                    // Return empty list - will be populated later when ScheduleGroup is fully registered
                    __result = new List<ScheduleBlockType>();
                    // Suppress the exception by returning normally (don't rethrow)
                    return;
                }
                // If it's not about MorningExercise or no exception, let the exception propagate
            }
        }

        // Patch ChorePreconditions constructor to modify IsScheduledTime to allow Relax during Exercise block when buffed
        [HarmonyPatch(typeof(ChorePreconditions), MethodType.Constructor)]
        public static class ChorePreconditions_Constructor_Patch
        {
            internal static void Postfix(ChorePreconditions __instance)
            {
                // Store the original delegate function
                var originalFn = __instance.IsScheduledTime.fn;
                
                // Replace with our modified version
                __instance.IsScheduledTime.fn = delegate(ref Chore.Precondition.Context context, object data)
                {
                    // First run the original check
                    bool originalResult = originalFn(ref context, data);
                    
                    // If original check passed, we're done
                    if (originalResult) return true;
                    
                    // Only modify for Relax chores checking Recreation schedule block
                    if (ExerciseBlock == null) return false;
                    if (context.chore == null || context.chore.choreType != Db.Get().ChoreTypes.Relax) return false;
                    
                    ScheduleBlockType requestedType = data as ScheduleBlockType;
                    if (requestedType != Db.Get().ScheduleBlockTypes.Recreation) return false;
                    
                    // Check if dupe is in Exercise block time
                    var scheduleBlock = context.consumerState?.scheduleBlock;
                    if (scheduleBlock == null || !scheduleBlock.IsAllowed(ExerciseBlock)) return false;
                    
                    // Check if dupe has the warm-up buff
                    var dupe = context.consumerState?.gameObject;
                    if (dupe == null) return false;
                    
                    var effects = dupe.GetComponent<Effects>();
                    if (effects == null) return false;
                    
                    bool hasBuff = effects.HasEffect(WARMUP_EFFECT_ID) || effects.HasEffect(BIONIC_WARMUP_EFFECT_ID);
                    if (!hasBuff) return false;
                    
                    // Allow Relax chore during Exercise block when they have the buff
                    return true;
                };
            }
        }

    }
}
