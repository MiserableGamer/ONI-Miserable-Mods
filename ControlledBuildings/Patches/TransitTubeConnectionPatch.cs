using HarmonyLib;

namespace ControlledBuildings.Patches
{
    // Allow any transit tube connection: tighter bends, through tiles, adjacent bridges.
    // Replicates Unrestricted Transit Tubes (asquared31415) so that mod is not required.
    [HarmonyPatch(typeof(UtilityNetworkTubesManager), nameof(UtilityNetworkTubesManager.CanAddConnection))]
    public static class UtilityNetworkTubesManager_CanAddConnection_Patch
    {
        public static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }
}
