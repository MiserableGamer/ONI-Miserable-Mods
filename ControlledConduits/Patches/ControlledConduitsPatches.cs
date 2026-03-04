using ControlledConduits.Buildings;
using ControlledConduits.UI;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace ControlledConduits.Patches
{
    public static class ControlledConduitsPatches
    {
        private static bool buildingRegistered;
        private static bool sideScreenRegistered;

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            private static void Prefix()
            {
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.NAME",
                    CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.NAME);
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.DESC",
                    CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.DESC);
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.EFFECT",
                    CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.EFFECT);
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT",
                    CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT);
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT_ACTIVE",
                    CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT_ACTIVE);
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT_INACTIVE",
                    CONTROLLEDCONDUITS.BUILDINGS.PREFABS.GASCONDUITVOLUMESENSOR.LOGIC_PORT_INACTIVE);

                Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.NAME", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.NAME);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.DESC", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.DESC);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.EFFECT", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.EFFECT);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT_ACTIVE", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT_ACTIVE);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT_INACTIVE", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.LIQUIDCONDUITVOLUMESENSOR.LOGIC_PORT_INACTIVE);

                Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.NAME", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.NAME);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.DESC", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.DESC);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.EFFECT", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.EFFECT);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.LOGIC_PORT", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.LOGIC_PORT);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.LOGIC_PORT_ACTIVE", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.LOGIC_PORT_ACTIVE);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.LOGIC_PORT_INACTIVE", CONTROLLEDCONDUITS.BUILDINGS.PREFABS.SOLIDCONDUITVOLUMESENSOR.LOGIC_PORT_INACTIVE);

                // IThresholdSwitch sidescreen strings (so "Current Packet mass:" and section title resolve)
                Strings.Add("STRINGS.CONTROLLEDCONDUITS.THRESHOLD_SIDESCREEN_TITLE", "Packet mass");
                Strings.Add("STRINGS.CONTROLLEDCONDUITS.THRESHOLD_PACKET_MASS", "Packet mass");
                Strings.Add("STRINGS.CONTROLLEDCONDUITS.THRESHOLD_ABOVE_TOOLTIP", CONTROLLEDCONDUITS.THRESHOLD_ABOVE_TOOLTIP);
                Strings.Add("STRINGS.CONTROLLEDCONDUITS.THRESHOLD_BELOW_TOOLTIP", CONTROLLEDCONDUITS.THRESHOLD_BELOW_TOOLTIP);
                Strings.Add("STRINGS.CONTROLLEDCONDUITS.IGNORE_EMPTY_TITLE", "Ignore Empty (below only)");
                Strings.Add("STRINGS.CONTROLLEDCONDUITS.IGNORE_EMPTY_LABEL", "Ignore Empty (below only)");
                Strings.Add("STRINGS.CONTROLLEDCONDUITS.VOLUME_SENSOR_OPTIONS_TITLE", "Options");

                // Strings only in Prefix; plan screen in Postfix so we're added after registration and cleanup.
            }

            // After all buildings are registered; game adds _kanim to anim names so we request gas_volume_sensor_kanim (files are gas_volume_sensor_build.bytes etc).
            private static void Postfix()
            {
                if (!buildingRegistered)
                {
                    ModUtil.AddBuildingToPlanScreen("HVAC", GasConduitVolumeSensorConfig.ID, "sensors", GasConduitTemperatureSensorConfig.ID);
                    ModUtil.AddBuildingToPlanScreen("Plumbing", LiquidConduitVolumeSensorConfig.ID, "sensors", LiquidConduitTemperatureSensorConfig.ID);
                    ModUtil.AddBuildingToPlanScreen("Conveyance", SolidConduitVolumeSensorConfig.ID, "sensors", SolidConduitTemperatureSensorConfig.ID);
                    buildingRegistered = true;
                }

                if (Assets.BuildingDefs == null)
                    return;
                ApplyVolumeSensorAnim(GasConduitVolumeSensorConfig.ID, "gas_volume_sensor_kanim");
                ApplyVolumeSensorAnim(LiquidConduitVolumeSensorConfig.ID, "liquid_volume_sensor_kanim");
                ApplyVolumeSensorAnim(SolidConduitVolumeSensorConfig.ID, "solid_volume_sensor_kanim");
            }

            private static void ApplyVolumeSensorAnim(string buildingId, string kanimName)
            {
                var def = Assets.GetBuildingDef(buildingId);
                if (def == null)
                    return;
                var customAnim = Assets.GetAnim(kanimName);
                if (customAnim == null)
                    return;
                def.AnimFiles = new KAnimFile[] { customAnim };
                if (def.BuildingComplete != null)
                {
                    var kbac = def.BuildingComplete.GetComponent<KBatchedAnimController>();
                    if (kbac != null)
                        kbac.AnimFiles = new KAnimFile[] { customAnim };
                }
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            private static void Postfix()
            {
                Db.Get().Techs.Get("HVAC").AddUnlockedItemIDs(new[] { GasConduitVolumeSensorConfig.ID });
                Db.Get().Techs.Get("LiquidTemperature").AddUnlockedItemIDs(new[] { LiquidConduitVolumeSensorConfig.ID });
                Db.Get().Techs.Get("SolidManagement").AddUnlockedItemIDs(new[] { SolidConduitVolumeSensorConfig.ID });
            }
        }

        [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
        public static class DetailsScreen_OnPrefabInit_Patch
        {
            private static void Postfix(DetailsScreen __instance)
            {
                if (!sideScreenRegistered)
                {
                    SideScreenHelper.AddSideScreen<ConduitVolumeSensorSideScreen>("ConduitVolumeSensorIgnoreEmpty", __instance);
                    sideScreenRegistered = true;
                }
            }
        }

        // Allow 4 decimal places (0.0001 kg = 0.1 g) when editing the volume sensor threshold.
        [HarmonyPatch(typeof(ThresholdSwitchSideScreen), nameof(ThresholdSwitchSideScreen.SetTarget))]
        public static class ThresholdSwitchSideScreen_SetTarget_Patch
        {
            private static void Postfix(ThresholdSwitchSideScreen __instance, GameObject new_target)
            {
                var numberInput = Traverse.Create(__instance).Field("numberInput").GetValue<KNumberInputField>();
                if (numberInput == null)
                    return;
                bool isVolumeSensor = new_target != null && new_target.GetComponent<Components.ConduitVolumeSensor>() != null;
                numberInput.decimalPlaces = isVolumeSensor ? 4 : 1;
            }
        }

        // "Below" should mean strictly below threshold: empty conduit + Below/0 must be false (not true).
        // When Ignore Empty is on: empty conduit must stay inactive (do not send Green).
        [HarmonyPatch(typeof(ConduitThresholdSensor), nameof(ConduitThresholdSensor.ConduitUpdate), typeof(float))]
        public static class ConduitThresholdSensor_ConduitUpdate_Patch
        {
            private static void Postfix(ConduitThresholdSensor __instance)
            {
                if (!(__instance is Components.ConduitVolumeSensor volumeSensor))
                    return;
                if (__instance.ActivateAboveThreshold)
                    return;
                // Ignore Empty: when conduit is empty, keep output OFF.
                if (volumeSensor.ignoreEmpty && __instance.CurrentValue == 0f && __instance.IsSwitchedOn)
                {
                    __instance.Toggle();
                    return;
                }
                if (__instance.CurrentValue < __instance.Threshold)
                    return;
                if (!__instance.IsSwitchedOn)
                    return;
                __instance.Toggle();
            }
        }

        [HarmonyPatch(typeof(ConduitThresholdSensor), "OnCopySettings")]
        public static class ConduitThresholdSensor_OnCopySettings_Patch
        {
            private static void Postfix(ConduitThresholdSensor __instance, object data)
            {
                if (!(__instance is Components.ConduitVolumeSensor dest) || !(data is GameObject go))
                    return;
                var src = go.GetComponent<Components.ConduitVolumeSensor>();
                if (src != null)
                    dest.ignoreEmpty = src.ignoreEmpty;
            }
        }
    }
}
