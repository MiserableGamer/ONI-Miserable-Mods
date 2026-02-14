using HarmonyLib;

namespace ControlledAssignments.Patches
{
    // Hook into every building registration (vanilla + modded).
    // After each building config is fully processed, check if the BuildingComplete
    // template has a Shower or HandSanitizer component and add Ownable if so.
    // Adding to the template ensures Ownable gets the full lifecycle on placed buildings.
    [HarmonyPatch(typeof(BuildingConfigManager), nameof(BuildingConfigManager.RegisterBuilding))]
    public static class BuildingConfigManager_RegisterBuilding_Patch
    {
        internal static void Postfix(BuildingConfigManager __instance, IBuildingConfig config)
        {
            if (!__instance.configTable.TryGetValue(config, out var def)) return;

            var go = def.BuildingComplete;
            if (go == null) return;

            if (go.GetComponent<Shower>() != null)
            {
                var ownable = go.AddOrGet<Ownable>();
                ownable.slotID = AssignmentConstants.ShowerSlotId;
                ownable.canBePublic = true;
                go.AddTag(GameTags.NotRoomAssignable);
            }
            else if (go.GetComponent<HandSanitizer>() != null)
            {
                var ownable = go.AddOrGet<Ownable>();
                ownable.slotID = AssignmentConstants.SinkSlotId;
                ownable.canBePublic = true;
                go.AddTag(GameTags.NotRoomAssignable);
            }
            else if (go.GetComponent<OilChangerWorkableUse>() != null)
            {
                var ownable = go.AddOrGet<Ownable>();
                ownable.slotID = AssignmentConstants.LubricationStationSlotId;
                ownable.canBePublic = true;
                // Bionic buildings don't match standard room constraints, so without
                // this tag the UI defaults to room assignment instead of the dupe list.
                go.AddTag(GameTags.NotRoomAssignable);
            }
        }
    }
}
