using HarmonyLib;

namespace ControlledAssignments.Patches
{
    // Makes any building with a HandSanitizer component assignable (vanilla + modded).
    // This covers Wash Basin, Wash Sink, and Hand Sanitizer (Bleach Station).
    [HarmonyPatch(typeof(HandSanitizer), nameof(HandSanitizer.OnSpawn))]
    public static class HandSanitizer_OnSpawn_Patch
    {
        internal static void Postfix(HandSanitizer __instance)
        {
            var ownable = __instance.gameObject.AddOrGet<Ownable>();
            ownable.slotID = AssignmentConstants.SinkSlotId;
            ownable.canBePublic = true;
        }
    }
}
