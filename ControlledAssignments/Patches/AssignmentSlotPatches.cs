using HarmonyLib;

namespace ControlledAssignments.Patches
{
    // Register custom OwnableSlots so dupes can be assigned to showers and sinks.
    // MinionAssignablesProxy.ConfigureAssignableSlots iterates all Db.Get().AssignableSlots
    // and creates OwnableSlotInstances automatically, so registering here is all we need.
    [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
    public static class Db_Initialize_Patch
    {
        internal static void Postfix(Db __instance)
        {
            __instance.AssignableSlots.Add(new OwnableSlot(AssignmentConstants.ShowerSlotId, AssignmentConstants.ShowerSlotName));
            __instance.AssignableSlots.Add(new OwnableSlot(AssignmentConstants.SinkSlotId, AssignmentConstants.SinkSlotName));
        }
    }
}
