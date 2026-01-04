using HarmonyLib;

namespace ControlledOverlay.Patches
{
	// Attaches the TemperatureOverlaySettings component to the SaveGame GameObject
	// This ensures our settings are serialized and deserialized with the save file
	[HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
	public static class SaveGame_OnPrefabInit_Patch
	{
		public static void Postfix(SaveGame __instance)
		{
			// AddOrGet ensures we don't add duplicates and handles both new saves and loaded saves
			__instance.gameObject.AddOrGet<TemperatureOverlaySettings>();
		}
	}
}





