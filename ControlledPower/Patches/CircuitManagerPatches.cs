using System.Collections.Generic;
using ControlledPower.Components;
using HarmonyLib;
using UnityEngine;

namespace ControlledPower.Patches
{
    // Circuit patches for diode chain load behavior.
    // Includes:
    // - output load caching used by capacity controller
    // - upstream joules augmentation for diode input circuits
    // - deterministic potential override for diode input circuits
    [HarmonyPatch(typeof(CircuitManager))]
    public static class CircuitManagerPatches
    {
        [System.ThreadStatic]
        private static HashSet<ushort> _augmenting;
        [System.ThreadStatic]
        private static bool _bypassLogicLinkAdjustments;

        [HarmonyPatch(nameof(CircuitManager.Sim200msFirst))]
        [HarmonyPrefix]
        public static void Sim200msFirst_Prefix(CircuitManager __instance)
        {
            if (__instance == null || PowerDiodeLogicLink.LinkedDiodes.Count == 0)
                return;

            const float simDt = 0.2f;

            // Build list of links with valid circuit ids for topological sort.
            var links = new List<PowerDiodeLogicLink>();
            foreach (var l in PowerDiodeLogicLink.LinkedDiodes)
            {
                if (l == null || !l.GetCircuitIds(out ushort i, out ushort o))
                    continue;
                if (i == CircuitManager.INVALID_ID || o == CircuitManager.INVALID_ID || i == o)
                    continue;
                links.Add(l);
            }
            if (links.Count == 0)
                return;

            // Topological order is kept for deterministic per-diode updates in chains.
            var order = TopologicalOrder(links);
            foreach (int idx in order)
            {
                var link = links[idx];
                if (!link.GetCircuitIds(out _, out ushort outputId) || outputId == CircuitManager.INVALID_ID)
                    continue;
                var consumer = link.GetComponent<PowerDiodeInputConsumer>();
                if (consumer == null)
                    continue;
                // Keep the virtual consumer neutral; potential is calculated in GetWattsNeededWhenActive postfix.
                consumer.SetWattsNeededWhenActive(0f);
                consumer.EnergySim200ms(simDt);
            }

            var cache = new Dictionary<ushort, float>();
            foreach (var link in PowerDiodeLogicLink.LinkedDiodes)
            {
                if (link == null || !link.GetCircuitIds(out _, out ushort oid))
                    continue;
                if (oid == CircuitManager.INVALID_ID || cache.ContainsKey(oid))
                    continue;
                _bypassLogicLinkAdjustments = true;
                float w;
                try
                {
                    // Internal diode sizing must use raw circuit draw, not checkbox-adjusted display values.
                    w = __instance.GetWattsUsedByCircuit(oid);
                }
                finally
                {
                    _bypassLogicLinkAdjustments = false;
                }
                if (w >= 0f)
                    cache[oid] = w;
            }

            foreach (var link in PowerDiodeLogicLink.LinkedDiodes)
            {
                if (link == null || !link.GetCircuitIds(out _, out ushort outputId))
                    continue;
                if (!cache.TryGetValue(outputId, out float watts))
                    continue;
                var capacity = link.GetComponent<PowerDiodeCapacityController>();
                if (capacity != null)
                    capacity.ApplyOutputCircuitLoad(watts);
            }
        }

        // Topological order for diodes: when link i's input = link j's output, i must be before j.
        // Returns indices into links list. Kahn's algorithm.
        private static List<int> TopologicalOrder(List<PowerDiodeLogicLink> links)
        {
            int n = links.Count;
            var inputIds = new ushort[n];
            var outputIds = new ushort[n];
            for (int i = 0; i < n; i++)
            {
                if (!links[i].GetCircuitIds(out inputIds[i], out outputIds[i]))
                {
                    inputIds[i] = ushort.MaxValue;
                    outputIds[i] = ushort.MaxValue;
                }
            }

            var inDegree = new int[n];
            var outputToIndices = new Dictionary<ushort, List<int>>();
            for (int j = 0; j < n; j++)
            {
                if (outputIds[j] == ushort.MaxValue) continue;
                if (!outputToIndices.TryGetValue(outputIds[j], out var list))
                {
                    list = new List<int>();
                    outputToIndices[outputIds[j]] = list;
                }
                list.Add(j);
            }
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    if (inputIds[i] != ushort.MaxValue && inputIds[i] == outputIds[j])
                        inDegree[j]++;
                }
            }

            var queue = new Queue<int>();
            for (int j = 0; j < n; j++)
                if (inDegree[j] == 0) queue.Enqueue(j);
            var result = new List<int>(n);
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                result.Add(i);
                ushort inp = inputIds[i];
                if (inp == ushort.MaxValue) continue;
                if (outputToIndices.TryGetValue(inp, out var dependents))
                {
                    foreach (int j in dependents)
                    {
                        inDegree[j]--;
                        if (inDegree[j] == 0) queue.Enqueue(j);
                    }
                }
            }
            if (result.Count != n)
            {
                for (int j = 0; j < n; j++)
                    if (!result.Contains(j)) result.Add(j);
            }
            return result;
        }

        private static float GetBaseConsumerPotential(CircuitManager circuitManager, ushort circuitId)
        {
            if (circuitManager == null || circuitId == CircuitManager.INVALID_ID)
                return 0f;
            var consumers = circuitManager.GetConsumersOnCircuit(circuitId);
            if (consumers == null)
                return 0f;
            float total = 0f;
            foreach (var c in consumers)
            {
                if (c == null || c is PowerDiodeInputConsumer)
                    continue;
                float w = c.WattsNeededWhenActive;
                if (w > 0f)
                    total += w;
            }
            return total;
        }

        private static float GetBaseConsumerCurrent(CircuitManager circuitManager, ushort circuitId)
        {
            if (circuitManager == null || circuitId == CircuitManager.INVALID_ID)
                return 0f;
            var consumers = circuitManager.GetConsumersOnCircuit(circuitId);
            if (consumers == null)
                return 0f;
            float total = 0f;
            foreach (var c in consumers)
            {
                if (c == null || c is PowerDiodeInputConsumer)
                    continue;
                float w = c.WattsUsed;
                if (w > 0f)
                    total += w;
            }
            return total;
        }

        private static float GetBranchPotential(
            CircuitManager circuitManager,
            ushort circuitId,
            Dictionary<ushort, float> memo,
            HashSet<ushort> visiting)
        {
            if (circuitManager == null || circuitId == CircuitManager.INVALID_ID)
                return 0f;
            if (memo.TryGetValue(circuitId, out float cached))
                return cached;
            if (!visiting.Add(circuitId))
                return 0f; // cycle guard

            float total = GetBaseConsumerPotential(circuitManager, circuitId);
            foreach (var link in PowerDiodeLogicLink.LinkedDiodes)
            {
                if (link == null || !link.GetCircuitIds(out ushort inputId, out ushort outputId))
                    continue;
                if (!link.IsLogicLinkEnabled)
                    continue;
                if (inputId != circuitId || inputId == outputId)
                    continue;
                total += GetBranchPotential(circuitManager, outputId, memo, visiting);
            }

            visiting.Remove(circuitId);
            memo[circuitId] = total;
            return total;
        }

        [HarmonyPatch(nameof(CircuitManager.GetJoulesAvailableOnCircuit))]
        [HarmonyPostfix]
        public static void GetJoulesAvailableOnCircuit_Postfix(CircuitManager __instance, ushort circuitID, ref float __result)
        {
            if (__instance == null || circuitID == CircuitManager.INVALID_ID)
                return;

            if (_augmenting == null)
                _augmenting = new HashSet<ushort>();
            if (_augmenting.Contains(circuitID))
                return;
            _augmenting.Add(circuitID);
            try
            {
                var addedIds = new HashSet<ushort>();
                float add = 0f;
                foreach (var link in PowerDiodeLogicLink.LinkedDiodes)
                {
                    if (link == null || !link.GetCircuitIds(out ushort inputId, out ushort outputId))
                        continue;
                    if (!link.IsLogicLinkEnabled)
                        continue;
                    if (inputId == outputId)
                        continue;
                    // Only add when we're the INPUT (add output's chain). Do not add when output — avoids double-count.
                    if (circuitID == inputId && outputId != CircuitManager.INVALID_ID && addedIds.Add(outputId) && !_augmenting.Contains(outputId))
                        add += __instance.GetJoulesAvailableOnCircuit(outputId);
                }
                __result += add;
            }
            finally
            {
                _augmenting.Remove(circuitID);
            }
        }

        [HarmonyPatch(nameof(CircuitManager.GetWattsNeededWhenActive))]
        [HarmonyPostfix]
        public static void GetWattsNeededWhenActive_Postfix(CircuitManager __instance, ushort originCircuitId, ref float __result)
        {
            if (__instance == null || originCircuitId == CircuitManager.INVALID_ID || __result < 0f)
                return;

            // Enabled diodes: deterministic cumulative branch potential
            // (local non-diode consumers + downstream enabled diode branches).
            bool hasDiodeInput = false;
            bool hasDisabledDiodeInput = false;
            float downstream = 0f;
            var memo = new Dictionary<ushort, float>();
            var visiting = new HashSet<ushort>();

            foreach (var link in PowerDiodeLogicLink.LinkedDiodes)
            {
                if (link == null || !link.GetCircuitIds(out ushort inputId, out ushort outputId))
                    continue;
                if (inputId != originCircuitId || inputId == outputId || outputId == CircuitManager.INVALID_ID)
                    continue;

                if (!link.IsLogicLinkEnabled)
                {
                    hasDisabledDiodeInput = true;
                    continue;
                }

                hasDiodeInput = true;
                downstream += GetBranchPotential(__instance, outputId, memo, visiting);
            }

            if (hasDiodeInput)
            {
                float localEnabled = GetBaseConsumerPotential(__instance, originCircuitId);
                __result = Mathf.Max(0f, localEnabled + downstream);
                return;
            }

            if (hasDisabledDiodeInput)
            {
                // Link OFF means local circuit-only potential reporting.
                __result = Mathf.Max(0f, GetBaseConsumerPotential(__instance, originCircuitId));
            }
        }

        [HarmonyPatch(nameof(CircuitManager.GetWattsUsedByCircuit))]
        [HarmonyPostfix]
        public static void GetWattsUsedByCircuit_Postfix(CircuitManager __instance, ushort circuitID, ref float __result)
        {
            if (_bypassLogicLinkAdjustments)
                return;
            if (__instance == null || circuitID == CircuitManager.INVALID_ID || __result < 0f)
                return;

            // Link OFF means local circuit-only current reporting on diode input circuits.
            bool hasDisabledDiodeInput = false;
            bool hasEnabledDiodeInput = false;
            foreach (var link in PowerDiodeLogicLink.LinkedDiodes)
            {
                if (link == null || !link.GetCircuitIds(out ushort inputId, out ushort outputId))
                    continue;
                if (inputId != circuitID || inputId == outputId || outputId == CircuitManager.INVALID_ID)
                    continue;
                if (link.IsLogicLinkEnabled)
                    hasEnabledDiodeInput = true;
                else
                    hasDisabledDiodeInput = true;
            }

            if (!hasEnabledDiodeInput && hasDisabledDiodeInput)
                __result = Mathf.Max(0f, GetBaseConsumerCurrent(__instance, circuitID));
        }
    }
}
