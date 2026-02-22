using System;
using System.Reflection;
using HarmonyLib;
using Klei.AI;
using UnityEngine;
using ControlledMods.Components;
using ControlledMods.ModDetection;
using ControlledMods.Options;

namespace ControlledMods.Patches.CustomizablePlants
{
    /// <summary>
    /// When Customize Plants is loaded and the option is enabled, applies its max_age for VineBranch to the prefab
    /// and to each VineBranch instance (oldAge max) so fruit drops per config (e.g. max_age = 1 for instant drop).
    /// </summary>
    public static class VineBranchMaxAgePatches
    {
        public static void ApplyPatches(Harmony harmony)
        {
            if (!CustomizablePlantsDetection.Loaded)
                return;
            if (!ControlledModsOptions.Instance.EnableCustomizablePlantsVineBranchMaxAge)
                return;

            Type plantHelperType = AccessTools.TypeByName("CustomizePlants.PlantHelper");
            if (plantHelperType != null)
            {
                MethodInfo processPlant = AccessTools.Method(plantHelperType, "ProcessPlant", new[] { typeof(GameObject) });
                if (processPlant != null)
                    harmony.Patch(processPlant, postfix: new HarmonyMethod(typeof(PlantHelper_ProcessPlant_Patch), nameof(PlantHelper_ProcessPlant_Patch.Postfix)));
                else
                    ControlledModsMod.LogWarning("CustomizablePlants: ProcessPlant method not found");
            }
            else
                ControlledModsMod.LogWarning("CustomizablePlants: PlantHelper type not found");

            var assetsAddPrefab = AccessTools.Method(typeof(Assets), nameof(Assets.AddPrefab), new[] { typeof(KPrefabID) });
            if (assetsAddPrefab != null)
                harmony.Patch(assetsAddPrefab, postfix: new HarmonyMethod(typeof(Assets_AddPrefab_Patch), nameof(Assets_AddPrefab_Patch.Postfix)));

            Type vineBranchType = typeof(VineBranch);
            Type instanceType = AccessTools.Inner(vineBranchType, "Instance");
            if (instanceType != null)
            {
                ConstructorInfo ctor = AccessTools.Constructor(instanceType, new[] { typeof(IStateMachineTarget), AccessTools.Inner(vineBranchType, "Def") });
                if (ctor != null)
                    harmony.Patch(ctor, postfix: new HarmonyMethod(typeof(VineBranch_Instance_Constructor_Patch), nameof(VineBranch_Instance_Constructor_Patch.Postfix)));
                else
                    ControlledModsMod.LogWarning("VineBranch.Instance constructor not found");
            }
        }

        private static class Assets_AddPrefab_Patch
        {
            public static void Postfix(KPrefabID prefab)
            {
                if (prefab == null || prefab.gameObject == null) return;
                if (prefab.PrefabID() != "VineBranch") return;

                float? maxAge = PlantHelper_ProcessPlant_Patch.GetCustomizablePlantsVineBranchMaxAge();
                if (maxAge == null) return;

                var comp = prefab.gameObject.AddOrGet<VineBranchMaxAgeOverride>();
                comp.MaxAge = maxAge.Value;
                ControlledModsMod.Log($"[VineBranch max_age] Applied max_age={maxAge.Value} to VineBranch prefab (via Assets.AddPrefab).");
            }
        }

        private static class PlantHelper_ProcessPlant_Patch
        {
            public static void Postfix(GameObject plant)
            {
                if (plant == null || plant.PrefabID() != "VineBranch") return;

                float? maxAge = GetCustomizablePlantsVineBranchMaxAge();
                if (maxAge == null)
                {
                    ControlledModsMod.Log("[VineBranch max_age] No Customize Plants setting for VineBranch (id \"VineBranch\", \"Vine Branch\", or \"Ovagro Vine\" with max_age set).");
                    return;
                }

                var comp = plant.AddOrGet<VineBranchMaxAgeOverride>();
                comp.MaxAge = maxAge.Value;
                ControlledModsMod.Log($"[VineBranch max_age] Applied max_age={maxAge.Value} to VineBranch prefab.");
            }

            /// <summary>Reads max_age for VineBranch from Customize Plants state. StateManager is a static field.</summary>
            public static float? GetCustomizablePlantsVineBranchMaxAge()
            {
                try
                {
                    Type stateType = AccessTools.TypeByName("CustomizePlants.CustomizePlantsState");
                    if (stateType == null) return null;

                    FieldInfo stateManagerField = stateType.GetField("StateManager", BindingFlags.Public | BindingFlags.Static);
                    if (stateManagerField == null) return null;

                    object stateManager = stateManagerField.GetValue(null);
                    if (stateManager == null) return null;

                    PropertyInfo stateProp = stateManager.GetType().GetProperty("State");
                    if (stateProp == null) return null;

                    object state = stateProp.GetValue(stateManager);
                    if (state == null) return null;

                    PropertyInfo plantSettingsProp = state.GetType().GetProperty("PlantSettings");
                    if (plantSettingsProp == null) return null;

                    object plantSettings = plantSettingsProp.GetValue(state);
                    if (plantSettings == null) return null;

                    if (plantSettings is System.Collections.IEnumerable list)
                    {
                        foreach (object item in list)
                        {
                            if (item == null) continue;
                            string id = GetPlantDataId(item);
                            if (string.IsNullOrEmpty(id)) continue;
                            if (id == "VineBranch" || id == "Vine Branch" || id == "Ovagro Vine")
                            {
                                float? maxAgeVal = GetPlantDataMaxAge(item);
                                if (maxAgeVal.HasValue)
                                    return maxAgeVal.Value;
                                return null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ControlledModsMod.LogWarning($"Customize Plants VineBranch max_age lookup failed: {ex.Message}");
                }

                return null;
            }

            private static string GetPlantDataId(object item)
            {
                if (item == null) return null;
                Type t = item.GetType();
                FieldInfo f = t.GetField("id", BindingFlags.Public | BindingFlags.Instance);
                if (f?.GetValue(item) is string s) return s;
                PropertyInfo p = t.GetProperty("id", BindingFlags.Public | BindingFlags.Instance);
                if (p?.GetValue(item) is string s2) return s2;
                p = t.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                return p?.GetValue(item) as string;
            }

            private static float? GetPlantDataMaxAge(object item)
            {
                if (item == null) return null;
                Type t = item.GetType();
                object v = t.GetField("max_age", BindingFlags.Public | BindingFlags.Instance)?.GetValue(item)
                    ?? t.GetProperty("max_age", BindingFlags.Public | BindingFlags.Instance)?.GetValue(item)
                    ?? t.GetProperty("Max_age", BindingFlags.Public | BindingFlags.Instance)?.GetValue(item)
                    ?? t.GetProperty("MaxAge", BindingFlags.Public | BindingFlags.Instance)?.GetValue(item);
                if (v is float fl) return fl;
                if (v is double d) return (float)d;
                if (v is int i) return (float)i;
                return null;
            }
        }

        private static class VineBranch_Instance_Constructor_Patch
        {
            public static void Postfix(object __instance)
            {
                if (__instance == null) return;

                GameObject go = Traverse.Create(__instance).Property("gameObject").GetValue<GameObject>();
                if (go == null) return;

                var overrideComp = go.GetComponent<VineBranchMaxAgeOverride>();
                if (overrideComp == null) return;

                object oldAgeObj = Traverse.Create(__instance).Field("oldAge").GetValue();
                if (oldAgeObj is not AmountInstance oldAge) return;

                float maxAge = overrideComp.MaxAge;
                oldAge.maxAttribute.ClearModifiers();
                if (maxAge > 0f)
                    oldAge.maxAttribute.Add(new AttributeModifier(Db.Get().Amounts.OldAge.maxAttribute.Id, maxAge, null, false, false, true));
                else
                    oldAge.maxAttribute.Add(new AttributeModifier(Db.Get().Amounts.OldAge.maxAttribute.Id, 1e7f, null, false, false, true));

                ControlledModsMod.Log($"[VineBranch max_age] Applied oldAge max={maxAge} to vine branch instance.");
            }
        }
    }
}
