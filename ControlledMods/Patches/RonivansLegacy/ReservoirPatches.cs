using HarmonyLib;
using ControlledMods.ModDetection;
using ControlledMods.Options;
using UnityEngine;

namespace ControlledMods.Patches.RonivansLegacy
{
    // Patches for Ronivan's Legacy reservoir buildings
    // These patches only apply if Ronivan's Legacy is detected
    public static class ReservoirPatches
    {
        // Called from main mod to apply patches if Ronivan's Legacy is loaded
        public static void ApplyPatches(Harmony harmony)
        {
            if (!ModDetector.RonivansLegacyLoaded)
            {
                ControlledModsMod.Log("Ronivan's Legacy not detected - skipping reservoir patches");
                return;
            }

            ControlledModsMod.Log("Ronivan's Legacy detected - applying reservoir patches");

            // Medium reservoirs (multi-port, use PortConduitConsumer)
            PatchBuildingConfig(harmony, 
                "RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.CustomReservoirs.MedGasReservoirConfig",
                nameof(MedGasReservoir_Postfix));
            PatchBuildingConfig(harmony, 
                "RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.CustomReservoirs.MedLiquidReservoirConfig",
                nameof(MedLiquidReservoir_Postfix));

            // Small reservoirs (standard conduit, use ConduitConsumer)
            // These are defined in the "Default" config, inherited by Normal and Inverted variants
            PatchBuildingConfig(harmony, 
                "RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.CustomReservoirs.SmallGasReservoirDefaultConfig",
                nameof(SmallGasReservoir_Postfix));
            PatchBuildingConfig(harmony, 
                "RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.CustomReservoirs.SmallLiquidReservoirDefaultConfig",
                nameof(SmallLiquidReservoir_Postfix));

            // Wall tanks (standard conduit, use ConduitConsumer)
            PatchBuildingConfig(harmony, 
                "RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.CustomReservoirs.SmallGasReservoirWallConfig",
                nameof(WallGasTank_Postfix));
            PatchBuildingConfig(harmony, 
                "RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.CustomReservoirs.SmallLiquidReservoirWallConfig",
                nameof(WallLiquidTank_Postfix));
        }

        private static void PatchBuildingConfig(Harmony harmony, string typeName, string postfixMethodName)
        {
            var configType = AccessTools.TypeByName(typeName);
            if (configType == null)
            {
                ControlledModsMod.Log($"  {typeName}: NOT FOUND");
                return;
            }

            var originalMethod = AccessTools.Method(configType, "ConfigureBuildingTemplate");
            if (originalMethod == null)
            {
                ControlledModsMod.Log($"  {typeName}: ConfigureBuildingTemplate not found");
                return;
            }

            var postfix = new HarmonyMethod(typeof(ReservoirPatches), postfixMethodName);
            harmony.Patch(originalMethod, postfix: postfix);
            ControlledModsMod.Log($"  Patched: {configType.Name}");
        }

        // ========== Medium Reservoirs (multi-port, use PortConduitConsumer) ==========

        private static void MedGasReservoir_Postfix(GameObject go)
        {
            try
            {
                float newCapacity = ControlledModsOptions.Instance.MedGasReservoirCapacity;
                UpdateStorageCapacity(go, newCapacity, "MedGasReservoir", usePortConduitConsumer: true);
            }
            catch (System.Exception e)
            {
                ControlledModsMod.LogWarning($"Error in MedGasReservoir postfix: {e.Message}");
            }
        }

        private static void MedLiquidReservoir_Postfix(GameObject go)
        {
            try
            {
                float newCapacity = ControlledModsOptions.Instance.MedLiquidReservoirCapacity;
                UpdateStorageCapacity(go, newCapacity, "MedLiquidReservoir", usePortConduitConsumer: true);
            }
            catch (System.Exception e)
            {
                ControlledModsMod.LogWarning($"Error in MedLiquidReservoir postfix: {e.Message}");
            }
        }

        // ========== Small Reservoirs (standard conduit, use ConduitConsumer) ==========

        private static void SmallGasReservoir_Postfix(GameObject go)
        {
            try
            {
                float newCapacity = ControlledModsOptions.Instance.SmallGasReservoirCapacity;
                UpdateStorageCapacity(go, newCapacity, "SmallGasReservoir", usePortConduitConsumer: false);
            }
            catch (System.Exception e)
            {
                ControlledModsMod.LogWarning($"Error in SmallGasReservoir postfix: {e.Message}");
            }
        }

        private static void SmallLiquidReservoir_Postfix(GameObject go)
        {
            try
            {
                float newCapacity = ControlledModsOptions.Instance.SmallLiquidReservoirCapacity;
                UpdateStorageCapacity(go, newCapacity, "SmallLiquidReservoir", usePortConduitConsumer: false);
            }
            catch (System.Exception e)
            {
                ControlledModsMod.LogWarning($"Error in SmallLiquidReservoir postfix: {e.Message}");
            }
        }

        // ========== Wall Tanks (standard conduit, use ConduitConsumer) ==========

        private static void WallGasTank_Postfix(GameObject go)
        {
            try
            {
                float newCapacity = ControlledModsOptions.Instance.WallGasTankCapacity;
                UpdateStorageCapacity(go, newCapacity, "WallGasTank", usePortConduitConsumer: false);
            }
            catch (System.Exception e)
            {
                ControlledModsMod.LogWarning($"Error in WallGasTank postfix: {e.Message}");
            }
        }

        private static void WallLiquidTank_Postfix(GameObject go)
        {
            try
            {
                float newCapacity = ControlledModsOptions.Instance.WallLiquidTankCapacity;
                UpdateStorageCapacity(go, newCapacity, "WallLiquidTank", usePortConduitConsumer: false);
            }
            catch (System.Exception e)
            {
                ControlledModsMod.LogWarning($"Error in WallLiquidTank postfix: {e.Message}");
            }
        }

        // ========== Shared Helper ==========

        private static void UpdateStorageCapacity(GameObject go, float newCapacity, string buildingName, bool usePortConduitConsumer)
        {
            var storage = go.GetComponent<Storage>();
            if (storage == null)
            {
                ControlledModsMod.Log($"{buildingName}: WARNING - No Storage component found!");
                return;
            }

            float originalCapacity = storage.capacityKg;
            storage.capacityKg = newCapacity;
            ControlledModsMod.Log($"{buildingName}: Storage {originalCapacity} -> {newCapacity} kg");

            if (usePortConduitConsumer)
            {
                // Medium reservoirs use PortConduitConsumer (multi-port buildings)
                int count = 0;
                foreach (var comp in go.GetComponents<Component>())
                {
                    if (comp.GetType().Name == "PortConduitConsumer")
                    {
                        var capacityField = AccessTools.Field(comp.GetType(), "capacityKG");
                        if (capacityField != null)
                        {
                            capacityField.SetValue(comp, newCapacity);
                            count++;
                        }
                    }
                }
                ControlledModsMod.Log($"  Updated {count} PortConduitConsumer(s)");
            }
            else
            {
                // Small reservoirs use standard ConduitConsumer
                var consumer = go.GetComponent<ConduitConsumer>();
                if (consumer != null)
                {
                    consumer.capacityKG = newCapacity;
                    ControlledModsMod.Log($"  Updated ConduitConsumer");
                }
            }
        }
    }
}
