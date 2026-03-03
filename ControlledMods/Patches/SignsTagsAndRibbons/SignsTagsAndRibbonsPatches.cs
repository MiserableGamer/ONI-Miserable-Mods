using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ControlledMods.Options;

namespace ControlledMods.Patches.SignsTagsAndRibbons
{
    /// <summary>Friendly display names for Small Element Tag variant anim names (tooltips).</summary>
    public static class SmallElementTagVariantNames
    {
        /// <summary>Lookup: anim name → tooltip/display name. Add or edit entries here.</summary>
        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "off", "Off" },
            { "art_brackene", "Brackene" },
            { "art_brine_water", "Brine" },
            { "art_carbon_dioxide_gas", "Carbon Dioxide" },
            { "art_carbon_dioxide_liquid", "Liquid Carbon Dioxide" },
            { "art_chlorine_gas", "Chlorine" },
            { "art_chlorine_liquid", "Liquid Chlorine" },
            { "art_crude_oil_liquid", "Crude Oil" },
            { "art_ethanol_liquid", "Ethanol" },
            { "art_fallout_gas", "Fallout" },
            { "art_helium_gas", "Helium" },
            { "art_hydrogen_gas", "Hydrogen" },
            { "art_hydrogen_liquid", "Liquid Hydrogen" },
            { "art_methane_gas", "Natural Gas" },
            { "art_methane_liquid", "Liquid Methane" },
            { "art_naphtha", "Naphtha" },
            { "art_oxygen_gas", "Oxygen" },
            { "art_oxygen_liquid", "Liquid Oxygen" },
            { "art_petrolleum_liquid", "Petroleum" },
            { "art_polluted_oxygen_gas", "Polluted Oxygen" },
            { "art_polluted_water", "Polluted Water" },
            { "art_propane_gas", "Propane" },
            { "art_propane_liquid", "Liquid Propane" },
            { "art_radwaste_liquid", "Nuclear Waste" },
            { "art_salt_water", "Salt Water" },
            { "art_sour_gas", "Sour Gas" },
            { "art_steam_gas", "Steam" },
            { "art_super_coolant", "Super Coolant" },
            { "art_visco_gel", "Visco-Gel" },
            { "art_water", "Water" },
            { "art_raw_methane_gas", "Raw Natural Gas" }
        };

        public static string GetFriendlyName(string animName)
        {
            if (string.IsNullOrEmpty(animName))
                return animName;
            if (Names.TryGetValue(animName, out var name))
                return name;
            // Fallback: strip "art_" prefix and replace underscores with spaces
            var s = animName.StartsWith("art_", StringComparison.OrdinalIgnoreCase) ? animName.Substring(4) : animName;
            return s.Replace("_", " ");
        }
    }
    /// <summary>
    /// Minimal STAR integration: our kanim contains all original variants plus Raw Natural Gas.
    /// Replace the building def's AnimFiles with only our kanim, and append our variant name to the mod's list.
    /// </summary>
    public static class SignsTagsAndRibbonsPatches
    {
        private const string VariantAnimName = "art_raw_methane_gas";
        private const string AdditionalKanimName = "additional_small_element_tags_kanim";

        /// <summary>Cached for tooltip postfix so we don't do reflection per button (avoids delay when opening side screen).</summary>
        private static FieldInfo _cachedButtonsField;

        private static bool ShouldInject()
        {
            return AccessTools.TypeByName("SignsTagsAndRibbons.SmallElementTagConfig") != null
                && ControlledModsOptions.Instance?.EnableSignsTagsAndRibbons == true;
        }

        public static void ApplyPatches(Harmony harmony)
        {
            var configType = AccessTools.TypeByName("SignsTagsAndRibbons.SmallElementTagConfig");
            if (configType == null)
                return;

            var createDef = AccessTools.Method(configType, "CreateBuildingDef", Type.EmptyTypes);
            if (createDef != null)
                harmony.Patch(createDef, postfix: new HarmonyMethod(typeof(SignsTagsAndRibbonsPatches), nameof(CreateBuildingDef_Postfix)));

            var doPost = AccessTools.Method(configType, "DoPostConfigureComplete", new[] { typeof(UnityEngine.GameObject) });
            if (doPost != null)
                harmony.Patch(doPost, postfix: new HarmonyMethod(typeof(SignsTagsAndRibbonsPatches), nameof(DoPostConfigureComplete_Postfix)));

            ControlledModsMod.Log("SignsTagsAndRibbons: patches applied (add Raw Natural Gas variant)");

            // Tooltips on variant buttons: add ToolTip with friendly name from lookup
            var signSideScreenType = AccessTools.TypeByName("SignsTagsAndRibbons.SignSideScreen");
            if (signSideScreenType != null)
            {
                _cachedButtonsField = AccessTools.Field(signSideScreenType, "buttons");
                var addButtonMethod = AccessTools.Method(signSideScreenType, "AddButton");
                if (addButtonMethod != null)
                {
                    harmony.Patch(addButtonMethod,
                        postfix: new HarmonyMethod(typeof(SignsTagsAndRibbonsPatches), nameof(SignSideScreen_AddButton_Postfix)));
                }
            }
        }

        /// <summary>Replace the building def's AnimFiles with only our kanim (contains all original variants + Raw Natural Gas).</summary>
        public static void CreateBuildingDef_Postfix(BuildingDef __result)
        {
            if (__result == null || !ShouldInject())
                return;
            var ourKanim = Assets.GetAnim(AdditionalKanimName);
            if (ourKanim == null)
            {
                ControlledModsMod.LogWarning("SignsTagsAndRibbons: kanim '" + AdditionalKanimName + "' not found.");
                return;
            }
            __result.AnimFiles = new KAnimFile[] { ourKanim };
        }

        /// <summary>Append our variant name to the mod's list. No other changes.</summary>
        public static void DoPostConfigureComplete_Postfix(UnityEngine.GameObject go)
        {
            if (go == null || !ShouldInject())
                return;
            var selectableType = AccessTools.TypeByName("SignsTagsAndRibbons.SelectableSign");
            if (selectableType == null)
                return;
            var selectable = go.GetComponent(selectableType);
            if (selectable == null)
                return;
            var listField = AccessTools.Field(selectableType, "AnimationNames");
            if (listField == null)
                return;
            var list = listField.GetValue(selectable) as List<string>;
            if (list == null)
                return;
            if (!list.Contains(VariantAnimName))
                list.Add(VariantAnimName);
        }

        /// <summary>Add tooltip to variant button with friendly name from SmallElementTagVariantNames lookup. Uses cached reflection to avoid delay when opening side screen.</summary>
        public static void SignSideScreen_AddButton_Postfix(object __instance, string animName)
        {
            if (__instance == null || string.IsNullOrEmpty(animName) || _cachedButtonsField == null)
                return;
            var buttons = _cachedButtonsField.GetValue(__instance) as List<GameObject>;
            if (buttons == null || buttons.Count == 0)
                return;
            var buttonGo = buttons[buttons.Count - 1];
            if (buttonGo == null)
                return;
            var toolTip = buttonGo.GetComponent<ToolTip>();
            if (toolTip == null)
                toolTip = buttonGo.AddComponent<ToolTip>();
            toolTip.SetSimpleTooltip(SmallElementTagVariantNames.GetFriendlyName(animName));
        }
    }
}
