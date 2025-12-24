using HarmonyLib;
using UnityEngine;
using System.Linq;
using PeterHan.PLib.UI;

namespace ResourceLimpet.Patches
{
    public static class ResourceLimpetPatches
    {
        // Register building strings when localization initializes
        [HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
        public static class Localization_Initialize_Patch
        {
            internal static void Postfix()
            {
                // Register building strings manually
                Strings.Add("STRINGS.BUILDINGS.PREFABS.RESOURCELIMPET.NAME", Buildings.ResourceLimpetConfig.NAME);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.RESOURCELIMPET.DESC", Buildings.ResourceLimpetConfig.DESC);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.RESOURCELIMPET.EFFECT", Buildings.ResourceLimpetConfig.EFFECT);
                
                Strings.Add("STRINGS.BUILDINGS.PREFABS.RIBBONNOTIFIER.NAME", Buildings.RibbonNotifierConfig.NAME);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.RIBBONNOTIFIER.DESC", Buildings.RibbonNotifierConfig.DESC);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.RIBBONNOTIFIER.EFFECT", Buildings.RibbonNotifierConfig.EFFECT);
                
                Debug.Log("[ResourceLimpet] Building strings registered");
            }
        }
        // Register buildings in build menu and ensure anim files are copied
        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            internal static void Postfix()
            {
                // Copy anim files from LogicAlarm to RibbonNotifier after all buildings are loaded
                var ribbonNotifierDef = Assets.GetBuildingDef(Buildings.RibbonNotifierConfig.ID);
                var logicAlarmDef = Assets.GetBuildingDef("LogicAlarm");
                
                if (ribbonNotifierDef != null && logicAlarmDef != null && 
                    logicAlarmDef.AnimFiles != null && logicAlarmDef.AnimFiles.Length > 0)
                {
                    ribbonNotifierDef.AnimFiles = logicAlarmDef.AnimFiles;
                    Debug.Log("[ResourceLimpet] Copied anim files from LogicAlarm to RibbonNotifier");
                }

                // Register Resource Limpet in Automation category
                RegisterBuildingInPlanMenu(Buildings.ResourceLimpetConfig.ID, "automation");

                // Register Ribbon Notifier in Automation category
                RegisterBuildingInPlanMenu(Buildings.RibbonNotifierConfig.ID, "automation");
            }

            private static void RegisterBuildingInPlanMenu(string buildingId, string categoryName)
            {
                var buildingDef = Assets.GetBuildingDef(buildingId);
                if (buildingDef == null)
                {
                    Debug.LogWarning($"[ResourceLimpet] Building def not found: {buildingId}");
                    return;
                }

                // Find the category in PLANORDER
                bool foundCategory = false;
                for (int i = 0; i < TUNING.BUILDINGS.PLANORDER.Count; i++)
                {
                    var planInfo = TUNING.BUILDINGS.PLANORDER[i];
                    string hashCategoryName = HashCache.Get().Get(planInfo.category);

                    if (hashCategoryName == categoryName)
                    {
                        foundCategory = true;
                        if (planInfo.buildingAndSubcategoryData == null)
                        {
                            planInfo.buildingAndSubcategoryData = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>();
                        }

                        // Use same subcategory as other buildings in this category
                        string subcategory = "uncategorized";
                        if (planInfo.buildingAndSubcategoryData.Count > 0)
                        {
                            subcategory = planInfo.buildingAndSubcategoryData[0].Value;
                        }

                        bool alreadyAdded = planInfo.buildingAndSubcategoryData.Any(
                            kvp => kvp.Key == buildingId);

                        if (!alreadyAdded)
                        {
                            planInfo.buildingAndSubcategoryData.Add(
                                new System.Collections.Generic.KeyValuePair<string, string>(
                                    buildingId,
                                    subcategory));

                            TUNING.BUILDINGS.PLANORDER[i] = planInfo;
                            Debug.Log($"[ResourceLimpet] {buildingId} added to {categoryName} category in PLANORDER");
                        }
                        break;
                    }
                }

                if (!foundCategory)
                {
                    Debug.LogWarning($"[ResourceLimpet] Could not find {categoryName} category in PLANORDER!");
                }
            }
        }

        // Add buildings to tech tree (try common automation tech names)
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Tech_Patch
        {
            internal static void Postfix()
            {
                // Try common automation tech names
                string[] techNames = { "LogicControl", "LogicBasic", "BasicAutomation", "Logic", "Automation" };
                Tech automationTech = null;

                foreach (string techName in techNames)
                {
                    try
                    {
                        automationTech = Db.Get().Techs.Get(techName);
                        if (automationTech != null)
                        {
                            Debug.Log($"[ResourceLimpet] Found automation tech: {techName}");
                            break;
                        }
                    }
                    catch
                    {
                        // Tech doesn't exist, try next one
                        continue;
                    }
                }

                if (automationTech != null)
                {
                    if (!automationTech.unlockedItemIDs.Contains(Buildings.ResourceLimpetConfig.ID))
                    {
                        automationTech.unlockedItemIDs.Add(Buildings.ResourceLimpetConfig.ID);
                        Debug.Log("[ResourceLimpet] Resource Limpet added to automation tech");
                    }

                    if (!automationTech.unlockedItemIDs.Contains(Buildings.RibbonNotifierConfig.ID))
                    {
                        automationTech.unlockedItemIDs.Add(Buildings.RibbonNotifierConfig.ID);
                        Debug.Log("[ResourceLimpet] Ribbon Notifier added to automation tech");
                    }
                }
                else
                {
                    // If no tech found, buildings will still be available but not in tech tree
                    // This is fine - they can be unlocked via other means or just be available from start
                    Debug.LogWarning("[ResourceLimpet] Could not find automation tech! Buildings will be available but not in tech tree.");
                }
            }
        }

        // Copy anim files from LogicAlarm to RibbonNotifier - ensure they're available when needed
        [HarmonyPatch(typeof(BuildingLoader), "Add2DComponents")]
        public static class BuildingLoader_Add2DComponents_Patch
        {
            internal static void Prefix(BuildingDef def, GameObject go, string initialAnimState, bool no_collider, int layer)
            {
                // Only fix RibbonNotifier if anim files are missing
                if (def != null && def.PrefabID == Buildings.RibbonNotifierConfig.ID)
                {
                    // Always try to get anim files from LogicAlarm (game's standard building)
                    var logicAlarmDef = Assets.GetBuildingDef("LogicAlarm");
                    if (logicAlarmDef != null && logicAlarmDef.AnimFiles != null && logicAlarmDef.AnimFiles.Length > 0)
                    {
                        def.AnimFiles = logicAlarmDef.AnimFiles;
                    }
                    else if (def.AnimFiles == null || def.AnimFiles.Length == 0)
                    {
                        // If LogicAlarm anim files aren't available, log warning
                        Debug.LogWarning("[ResourceLimpet] LogicAlarm anim files not available for RibbonNotifier!");
                    }
                }
            }
        }

        // Copy KBatchedAnimController settings from LogicAlarm after prefab is created (similar to MorningExercise)
        [HarmonyPatch(typeof(Assets), "CreatePrefabs")]
        public static class Assets_CreatePrefabs_RibbonNotifier_Patch
        {
            internal static void Postfix()
            {
                var ribbonNotifierPrefab = Assets.GetPrefab(Buildings.RibbonNotifierConfig.ID);

                if (ribbonNotifierPrefab == null)
                {
                    return; // Not an error, might not be loaded yet
                }

                // Copy KBatchedAnimController settings from LogicAlarm prefab
                var logicAlarmPrefab = Assets.GetPrefab("LogicAlarm");
                if (logicAlarmPrefab != null)
                {
                    var alarmKbac = logicAlarmPrefab.GetComponent<KBatchedAnimController>();
                    var notifierKbac = ribbonNotifierPrefab.GetComponent<KBatchedAnimController>();

                    if (alarmKbac != null && notifierKbac != null)
                    {
                        notifierKbac.fgLayer = alarmKbac.fgLayer;
                        notifierKbac.initialAnim = alarmKbac.initialAnim;
                        notifierKbac.initialMode = alarmKbac.initialMode;
                        Debug.Log("[ResourceLimpet] Copied KBatchedAnimController settings from LogicAlarm");
                    }
                }
            }
        }

        // Register custom sidescreen for RibbonNotifier using PLib
        [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
        public static class DetailsScreen_OnPrefabInit_Patch
        {
            internal static void Postfix()
            {
                // Register our custom sidescreen using PLib's UI system
                PUIUtils.AddSideScreenContent<ResourceLimpet.UI.RibbonNotifierSidescreen>("RibbonNotifierSidescreen", null);
                Debug.Log("[ResourceLimpet] Registered RibbonNotifier sidescreen");
            }
        }
    }
}

