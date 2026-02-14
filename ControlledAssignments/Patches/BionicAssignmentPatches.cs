using System;
using HarmonyLib;

namespace ControlledAssignments.Patches
{
    /// <summary>
    /// When an Ownable spawns on a bionic-only building that we made assignable,
    /// add a precondition so only bionic dupes can be assigned.
    /// Vanilla bionic buildings that already have Ownable (e.g. Gunk Extractor) handle
    /// this themselves; this patch covers buildings where our mod adds the Ownable.
    /// </summary>
    [HarmonyPatch(typeof(Ownable), nameof(Ownable.OnSpawn))]
    public static class Ownable_OnSpawn_Patch
    {
        internal static void Postfix(Ownable __instance)
        {
            if (__instance.slotID != AssignmentConstants.LubricationStationSlotId)
                return;

            __instance.AddAssignPrecondition(OnlyBionicsPrecondition);
        }

        private static bool OnlyBionicsPrecondition(MinionAssignablesProxy proxy)
        {
            return proxy.GetMinionModel() == BionicMinionConfig.MODEL;
        }
    }
}
