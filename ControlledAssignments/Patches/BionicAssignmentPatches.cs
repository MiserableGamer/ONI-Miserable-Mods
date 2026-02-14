using HarmonyLib;

namespace ControlledAssignments.Patches
{
    /// <summary>
    /// For bionic-only buildings that our mod makes assignable (e.g. Lubrication Station),
    /// restrict assignment to bionic duplicants only.
    /// This patches CanAssignTo directly so the check happens at evaluation time,
    /// working reliably for both new and existing (save-loaded) buildings.
    /// </summary>
    [HarmonyPatch(typeof(Assignable), nameof(Assignable.CanAssignTo))]
    public static class Assignable_CanAssignTo_Patch
    {
        internal static void Postfix(Assignable __instance, IAssignableIdentity identity, ref bool __result)
        {
            if (!__result) return;

            if (__instance.slotID != AssignmentConstants.LubricationStationSlotId) return;

            if (identity is MinionAssignablesProxy proxy &&
                proxy.GetMinionModel() != BionicMinionConfig.MODEL)
            {
                __result = false;
            }
        }
    }
}
