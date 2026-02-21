using HarmonyLib;
using UnityEngine;

namespace ControlledSublimation.Patches
{
	// Prevents CTD when the same building is added to the Codex multiple times.
	// Can happen when a building is in the plan screen more than once (e.g. Oxysconce).
	[HarmonyPatch(typeof(CodexCache), nameof(CodexCache.AddEntry))]
	public static class CodexCache_AddEntry_Patch
	{
		public static bool Prefix(string id)
		{
			string formatted = CodexCache.FormatLinkID(id);
			if (CodexCache.entries.ContainsKey(formatted))
			{
				return false;
			}
			return true;
		}
	}

	// When AddEntry is skipped (duplicate), the caller can still add the entry with an unset id.
	// Return null so the caller skips adding when we blocked the duplicate.
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
