using HarmonyLib;

namespace ControlledAssignments.Patches
{
    // Makes any building with a Shower component assignable (vanilla + modded).
    [HarmonyPatch(typeof(Shower), nameof(Shower.OnSpawn))]
    public static class Shower_OnSpawn_Patch
    {
        internal static void Postfix(Shower __instance)
        {
            var ownable = __instance.gameObject.AddOrGet<Ownable>();
            ownable.slotID = AssignmentConstants.ShowerSlotId;
            ownable.canBePublic = true;
        }
    }
}
