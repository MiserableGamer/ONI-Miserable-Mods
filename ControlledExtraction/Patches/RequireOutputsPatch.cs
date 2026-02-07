using HarmonyLib;

namespace ControlledExtraction.Patches
{
    // Suppresses vanilla "no output conduit connected" status item and operational
    // flag for buildings with our fallback emitter. RequireOutputs.UpdateConnectionState
    // is called from OnSpawn and from ScenePartitioner callbacks - both paths set the
    // operational flag AND toggle a StatusItem (the red icon). We override both so the
    // building operates normally and shows no warning when conduit is disconnected.
    [HarmonyPatch(typeof(RequireOutputs), "UpdateConnectionState")]
    public static class RequireOutputs_UpdateConnectionState_Patch
    {
        public static void Postfix(RequireOutputs __instance)
        {
            if (__instance.GetComponent<Components.PrimaryOutputFallbackEmitter>() == null)
                return;

            // Force RequireOutputs to think it's connected
            __instance.connected = true;
            __instance.previouslyConnected = true;
            __instance.operational.SetFlag(RequireOutputs.outputConnectedFlag, true);

            // Clear the "NeedGasOut" / "NeedLiquidOut" / "NeedSolidOut" status item
            StatusItem statusItem = null;
            switch (__instance.conduitType)
            {
                case ConduitType.Gas:
                    statusItem = Db.Get().BuildingStatusItems.NeedGasOut;
                    break;
                case ConduitType.Liquid:
                    statusItem = Db.Get().BuildingStatusItems.NeedLiquidOut;
                    break;
                case ConduitType.Solid:
                    statusItem = Db.Get().BuildingStatusItems.NeedSolidOut;
                    break;
            }

            if (statusItem != null)
            {
                __instance.hasPipeGuid = __instance.selectable.ToggleStatusItem(
                    statusItem, __instance.hasPipeGuid, false, __instance);
            }
        }
    }
}
