using System;
using System.Collections.Generic;
using System.Reflection;
using ControlledStorage.NoSweepZone;
using HarmonyLib;
using UnityEngine;

namespace ControlledStorage.Patches
{
    public static class DeliveryControlPatches
    {
        // Set to true to log sweeper/fetch details to Player.log; not in mod options (change here for dev debugging).
        private const bool DeliveryControlDebugLogs = false;

        [ThreadStatic]
        private static bool _sweeperContext;

        // Precondition for deposit: blocks chore at assignment so errand doesn't flash (like NoManualDelivery/Automatable)
        // When consumerState is null (pre-consumer check) or invalid, allow - IsFetchablePickup/FindFetchTarget filter per-consumer
        private static readonly Chore.Precondition DeliveryControlDepositPrecondition = new Chore.Precondition
        {
            id = "ControlledStorage_DeliveryControl",
            description = "",
            fn = (ref Chore.Precondition.Context context, object data) =>
            {
                if (data is not StorageDeliveryControl control) return true;
                var cs = context.consumerState;
                if (cs == null) return true;
                return cs.hasSolidTransferArm ? control.AllowSweeperDeposit : control.AllowDupeDeposit;
            },
            canExecuteOnAnyThread = true
        };

        private static float _lastLoopLogTime;
        private const float LoopLogThrottleSeconds = 5f;

        // Find first pickupable that is not same-bin and passes delivery control checks
        private static Pickupable PickFirstValidTarget(List<Pickupable> pickupables, Storage destination)
        {
            if (pickupables == null || destination == null) return null;
            var destControl = destination.GetComponent<StorageDeliveryControl>();
            if (destControl != null && !destControl.AllowSweeperDeposit) return null;
            foreach (var p in pickupables)
            {
                if (p == null) continue;
                if (p.storage == destination) continue; // skip same-bin
                var srcControl = p.storage?.GetComponent<StorageDeliveryControl>();
                if (srcControl != null && !srcControl.AllowSweeperExtract) continue;
                return p;
            }
            return null;
        }

        private static void Log(string msg)
        {
            if (DeliveryControlDebugLogs)
                Debug.Log("[ControlledStorage.DeliveryControl] " + msg);
        }

        [HarmonyPatch(typeof(Pickupable), "CouldBePickedUpByTransferArm", new Type[] { typeof(int) })]
        public static class Pickupable_CouldBePickedUpByTransferArm_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static void Postfix(Pickupable __instance, ref bool __result)
            {
                if (!__result) return;
                var storage = __instance.storage;
                if (storage == null) return;
                var component = storage.GetComponent<StorageDeliveryControl>();
                if (component != null && !component.AllowSweeperExtract)
                    __result = false;
            }
        }

        // Upstream chokepoint: dupes call CouldBePickedUpByMinion, sweepers call CouldBePickedUpByTransferArm.
        // Blocking here covers all FetchChore/FetchAreaChore paths for dupes without affecting sweepers.
        [HarmonyPatch(typeof(Pickupable), nameof(Pickupable.CouldBePickedUpByMinion), new Type[] { typeof(int) })]
        public static class Pickupable_CouldBePickedUpByMinion_NoSweep_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(Pickupable __instance, ref bool __result)
            {
                if (!__result) return;
                var instance = NoSweepZoneSaveState.Instance;
                if (instance == null) return;

                if (instance.NoSweep.ContainsCell(__instance.cachedCell))
                {
                    __result = false;
                    return;
                }
                if (__instance.transform != null)
                {
                    int posCell = Grid.PosToCell(__instance.transform.position);
                    if (posCell != __instance.cachedCell && Grid.IsValidCell(posCell) && instance.NoSweep.ContainsCell(posCell))
                        __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(FetchManager), "FindFetchTarget", new Type[] { typeof(List<Pickupable>), typeof(Storage), typeof(FetchChore) })]
        public static class FetchManager_FindFetchTarget_Sweeper_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static void Prefix(List<Pickupable> pickupables, Storage destination)
            {
                _sweeperContext = true;
                if (!DeliveryControlDebugLogs || destination == null) return;
                int sameBinCount = 0;
                if (pickupables != null)
                {
                    foreach (var p in pickupables)
                    {
                        if (p != null && p.storage == destination) sameBinCount++;
                    }
                }
                if (sameBinCount > 0)
                {
                    var destBuilding = destination.GetComponent<Building>();
                    string destInfo = destBuilding != null ? $"{destBuilding.Def?.PrefabID} @ cell {Grid.PosToCell(destination.transform.position)}" : "?";
                    Log($"FindFetchTarget: destination={destInfo}, pickupables={pickupables?.Count ?? 0}, SAME-BIN candidates={sameBinCount} (game may pick one of these -> loop)");
                }
            }

            internal static void Postfix(
                [HarmonyArgument(0)] List<Pickupable> pickupables,
                [HarmonyArgument(1)] Storage destination,
                ref Pickupable __result)
            {
                try
                {
                    if (__result != null && destination != null && __result.storage == destination)
                    {
                        if (Time.unscaledTime - _lastLoopLogTime >= LoopLogThrottleSeconds)
                        {
                            _lastLoopLogTime = Time.unscaledTime;
                            var destBuilding = destination.GetComponent<Building>();
                            var pe = __result.GetComponent<PrimaryElement>();
                            string destInfo = destBuilding != null ? $"{destBuilding.Def?.PrefabID} @ cell {Grid.PosToCell(destination.transform.position)}" : "?";
                            string itemInfo = pe?.Element != null ? pe.Element.tag.Name : (__result?.name ?? "?");
                            Log($"LOOP: sweeper would pick '{itemInfo}' FROM destination and put BACK into same bin {destInfo}");
                        }
                    }
                    if (__result != null)
                    {
                        var sourceStorage = __result.storage;
                        if (destination != null && sourceStorage == destination)
                        {
                            // Game picked same-bin item - redirect to first valid candidate instead of null so sweeper can deliver other items
                            __result = PickFirstValidTarget(pickupables, destination);
                            return;
                        }
                        if (destination != null)
                        {
                            var component = destination.GetComponent<StorageDeliveryControl>();
                            if (component != null && !component.AllowSweeperDeposit)
                            {
                                __result = null;
                                return;
                            }
                        }
                        var pickupable = __result;
                        var storage = pickupable?.storage;
                        if (storage != null)
                        {
                            var component2 = storage.GetComponent<StorageDeliveryControl>();
                            if (component2 != null && !component2.AllowSweeperExtract)
                                __result = null;
                        }
                    }
                }
                finally
                {
                    _sweeperContext = false;
                }
            }
        }

        [HarmonyPatch(typeof(FetchManager), "IsFetchablePickup")]
        public static class FetchManager_IsFetchablePickup_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl
                || ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(Pickupable pickup, FetchChore chore, Storage destination, ref bool __result)
            {
                if (!__result) return;
                if (_sweeperContext) return;
                if (ControlledStorageOptions.Instance.EnableNoSweepZones)
                {
                    var instance = NoSweepZoneSaveState.Instance;
                    if (instance != null)
                    {
                        if (instance.NoSweep.ContainsCell(pickup.cachedCell))
                        {
                            __result = false;
                            return;
                        }
                        if (pickup.transform != null)
                        {
                            int posCell = Grid.PosToCell(pickup.transform.position);
                            if (posCell != pickup.cachedCell && Grid.IsValidCell(posCell) && instance.NoSweep.ContainsCell(posCell))
                            {
                                __result = false;
                                return;
                            }
                        }
                    }
                }
                if (destination != null)
                {
                    var component = destination.GetComponent<StorageDeliveryControl>();
                    if (component != null && !component.AllowDupeDeposit)
                    {
                        __result = false;
                        return;
                    }
                }
                var sourceStorage = pickup.storage;
                if (sourceStorage != null)
                {
                    var component2 = sourceStorage.GetComponent<StorageDeliveryControl>();
                    if (component2 != null && !component2.AllowDupeExtract)
                        __result = false;
                }
            }
        }

        // Sensor paths (ClosestEdibleSensor, ClosestPickupableSensor) use IsFetchablePickup_Exclude
        // which receives KPrefabID, not Pickupable. All callers are dupe-only so no sweeper exemption needed.
        // Must check Pickupable.cachedCell in addition to transform position — they can differ.
        [HarmonyPatch(typeof(FetchManager), nameof(FetchManager.IsFetchablePickup_Exclude), new[] {
            typeof(KPrefabID), typeof(Storage), typeof(float), typeof(HashSet<Tag>), typeof(Tag[]), typeof(Storage)
        })]
        public static class FetchManager_IsFetchablePickup_Exclude_NoSweep_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(KPrefabID pickup_id, Storage source, ref bool __result)
            {
                if (!__result) return;
                var instance = NoSweepZoneSaveState.Instance;
                if (instance == null) return;

                if (pickup_id != null)
                {
                    var pickupable = pickup_id.GetComponent<Pickupable>();
                    if (pickupable != null && instance.NoSweep.ContainsCell(pickupable.cachedCell))
                    {
                        __result = false;
                        return;
                    }
                    if (pickup_id.transform != null)
                    {
                        int posCell = Grid.PosToCell(pickup_id.transform.GetPosition());
                        if (Grid.IsValidCell(posCell) && instance.NoSweep.ContainsCell(posCell))
                        {
                            __result = false;
                            return;
                        }
                    }
                }

                if (source != null && source.transform != null)
                {
                    int sourceCell = Grid.PosToCell(source.transform.GetPosition());
                    if (Grid.IsValidCell(sourceCell) && instance.NoSweep.ContainsCell(sourceCell))
                        __result = false;
                }
            }
        }

        // When an item's cell changes, check if it entered or left a No Sweep Zone.
        // On entry: mark unfetchable (via FetchableMonitor re-eval) + cancel active dupe chores.
        // On exit: mark fetchable again. Sweepers are unaffected throughout.
        [HarmonyPatch(typeof(Pickupable), "OnCellChange")]
        public static class Pickupable_OnCellChange_NoSweep_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(Pickupable __instance)
            {
                var instance = NoSweepZoneSaveState.Instance;
                if (instance == null || !instance.NoSweep.HasCells) return;

                int cell = __instance.cachedCell;
                if (!Grid.IsValidCell(cell)) return;

                bool inZone = instance.NoSweep.ContainsCell(cell);
                NoSweepZoneChoreInvalidation.UpdatePickupableZoneTracking(__instance, inZone);
            }
        }

        // Makes zone items unfetchable in FetchableMonitor, removing them from FetchManager.
        // Dupes use FetchManager to find targets — removing items eliminates chore oscillation.
        // Sweepers call CouldBePickedUpByTransferArm which also calls IsFetchable(),
        // so we skip the zone check when _sweeperContext is set (during SolidTransferArm.AsyncUpdate).
        [HarmonyPatch(typeof(FetchableMonitor.Instance), "IsFetchable", new Type[0])]
        public static class FetchableMonitor_IsFetchable_NoSweep_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(FetchableMonitor.Instance __instance, ref bool __result)
            {
                if (!__result) return;
                if (_sweeperContext) return;
                var zone = NoSweepZoneSaveState.Instance;
                if (zone == null || !zone.NoSweep.HasCells) return;
                var pickupable = __instance.pickupable;
                if (pickupable != null && zone.NoSweep.ContainsCell(pickupable.cachedCell))
                    __result = false;
            }
        }

        // Sets _sweeperContext during SolidTransferArm.AsyncUpdate so that IsFetchable checks
        // (called via CouldBePickedUpByTransferArm in the sweeper's AsyncUpdateVisitor) skip
        // the No Sweep Zone filter. Without this, sweepers cannot see zone items at all.
        [HarmonyPatch(typeof(SolidTransferArm), "AsyncUpdate")]
        public static class SolidTransferArm_AsyncUpdate_SweeperContext_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Prefix() => _sweeperContext = true;

            internal static void Postfix() => _sweeperContext = false;
        }

        // Add deposit precondition at chore creation - chore filtered for dupes/sweepers before assignment, no errand flashing
        // Use TargetMethod to find the FetchChore(..., Storage destination, ...) constructor - signatures vary by game version
        [HarmonyPatch(typeof(FetchChore))]
        public static class FetchChore_Constructor_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static MethodBase TargetMethod()
            {
                foreach (var ctor in typeof(FetchChore).GetConstructors())
                {
                    var ps = ctor.GetParameters();
                    if (ps.Length >= 2 && ps[1].ParameterType == typeof(Storage))
                        return ctor;
                }
                return null;
            }

            internal static void Postfix(FetchChore __instance)
            {
                var dest = __instance.destination;
                if (dest == null) return;
                var control = dest.GetComponent<StorageDeliveryControl>();
                if (control != null)
                    __instance.AddPrecondition(DeliveryControlDepositPrecondition, control);
            }
        }
    }
}
