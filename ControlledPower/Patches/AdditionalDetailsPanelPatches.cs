using ControlledPower.Components;
using HarmonyLib;
using UnityEngine;

namespace ControlledPower.Patches
{
    // Adds debug rows to Circuit Overview for diode output current and potential.
    [HarmonyPatch(typeof(AdditionalDetailsPanel), "RefreshEnergyOverviewPanel", new[] { typeof(CollapsibleDetailContentPanel), typeof(GameObject) })]
    public static class AdditionalDetailsPanelPatches
    {
        [HarmonyPostfix]
        public static void RefreshEnergyOverviewPanel_Postfix(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
        {
            if (targetPanel == null || targetEntity == null)
                return;

            var controller = targetEntity.GetComponent<PowerDiodeCapacityController>();
            var link = targetEntity.GetComponent<PowerDiodeLogicLink>();
            if (controller == null || link == null)
                return;

            if (!link.GetCircuitIds(out _, out ushort outputId))
                return;

            // Output Current Load (stored value used for this diode's draw)
            float currentW = controller.StoredOutputCircuitLoadW;
            targetPanel.SetLabel("powerDiodeOutputCurrentLoad", $"Output Current Load: {GameUtil.GetFormattedWattage(currentW, GameUtil.WattageFormatterUnit.Automatic, true)}",
                "Cumulative current load (W) on this diode's output circuit, from Sim200msFirst cache.");

            // Output Potential Load (potential wattage on output circuit)
            float potentialW = Game.Instance?.circuitManager != null ? Game.Instance.circuitManager.GetWattsNeededWhenActive(outputId) : 0f;
            if (potentialW >= 0f)
                targetPanel.SetLabel("powerDiodeOutputPotentialLoad", $"Output Potential Load: {GameUtil.GetFormattedWattage(potentialW, GameUtil.WattageFormatterUnit.Automatic, true)}",
                    "Potential load (W) on this diode's output circuit if all consumers were active.");

            targetPanel.Commit();
        }
    }
}
