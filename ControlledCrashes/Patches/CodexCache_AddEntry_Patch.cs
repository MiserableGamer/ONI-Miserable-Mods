using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    // Prevents CTD when the same building is added to the Codex multiple times (e.g. SPACECOOLER).
    // Vanilla logs an error then does entries.Add(id, entry), which throws for duplicate keys.
    // We skip the original when the id is already present.
    [HarmonyPatch(typeof(CodexCache), nameof(CodexCache.AddEntry))]
    public static class CodexCache_AddEntry_Patch
    {
        public static bool Prefix(string id)
        {
            string formatted = CodexCache.FormatLinkID(id);
            if (CodexCache.entries.ContainsKey(formatted))
            {
                Debug.LogWarning("[ControlledCrashes] Skipping duplicate Codex entry '" + id +
                    "'. Already registered. Likely a mod added this building to the plan screen twice (e.g. in multiple categories).");
                return false;
            }
            return true;
        }
    }

    // When AddEntry is skipped (duplicate), entry.id is never set. The caller then does
    // dictionary.Add(codexEntry.id, codexEntry) and throws ArgumentNullException on null key.
    // Return null so the caller's "if (codexEntry != null)" skips the Add.
    [HarmonyPatch(typeof(CodexEntryGenerator), "GenerateSingleBuildingEntry")]
    public static class CodexEntryGenerator_GenerateSingleBuildingEntry_Patch
    {
        public static void Postfix(ref CodexEntry __result)
        {
            if (__result != null && string.IsNullOrEmpty(__result.id))
            {
                __result = null;
            }
        }
    }
}
