using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledStorage.Patches
{
    /// <summary>
    /// Patches to control duplicant and sweeper deposit/extract for storage.
    /// </summary>
    public static class DeliveryControlPatches
    {
        [ThreadStatic]
        private static bool _sweeperContext;

        /// <summary>
        /// Patch Pickupable.CouldBePickedUpByTransferArm to block sweeper extract from our storage.
        /// Sweeper builds its pickupables list using this - items never get into the list if we return false.
        /// </summary>
        [HarmonyPatch(typeof(Pickupable), nameof(Pickupable.CouldBePickedUpByTransferArm), typeof(int))]
        public static class Pickupable_CouldBePickedUpByTransferArm_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static void Postfix(Pickupable __instance, ref bool __result)
            {
                if (!__result) return;
                var storage = __instance.storage;
                if (storage == null) return;
                var control = storage.GetComponent<StorageDeliveryControl>();
                if (control != null && !control.AllowSweeperExtract)
                    __result = false;
            }
        }

        /// <summary>
        /// Patch FetchManager.FindFetchTarget(List, Storage, FetchChore) - only used by SolidTransferArm.
        /// Reliable sweeper-only hook: deposit (destination) and extract (result.storage) checks.
        /// </summary>
        [HarmonyPatch(typeof(FetchManager), nameof(FetchManager.FindFetchTarget), typeof(System.Collections.Generic.List<Pickupable>), typeof(Storage), typeof(FetchChore))]
        public static class FetchManager_FindFetchTarget_Sweeper_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static void Prefix() => _sweeperContext = true;

            internal static void Postfix(Storage destination, ref Pickupable __result)
            {
                try
                {
                    if (__result == null) return;

                    // Sweeper deposit: block delivering TO our storage when sweeper deposit is disabled
                    if (destination != null)
                    {
                        var destControl = destination.GetComponent<StorageDeliveryControl>();
                        if (destControl != null && !destControl.AllowSweeperDeposit)
                        {
                            __result = null;
                            return;
                        }
                    }

                    // Sweeper extract: block picking FROM our storage when sweeper extract is disabled
                    var sourceStorage = __result?.storage;
                    if (sourceStorage != null)
                    {
                        var sourceControl = sourceStorage.GetComponent<StorageDeliveryControl>();
                        if (sourceControl != null && !sourceControl.AllowSweeperExtract)
                        {
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

        /// <summary>
        /// Patch FetchManager to control dupe deposit and extract.
        /// Deposit: block when destination is our storage and AllowDupeDeposit is false (NoManualDelivery-style).
        /// Extract: block when pickup is in our storage and AllowDupeExtract is false.
        /// </summary>
        [HarmonyPatch(typeof(FetchManager), nameof(FetchManager.IsFetchablePickup))]
        public static class FetchManager_IsFetchablePickup_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static void Postfix(Pickupable pickup, FetchChore chore, Storage destination, ref bool __result)
            {
                if (!__result) return;
                if (_sweeperContext) return; // Sweeper rules handled by FetchManager_FindFetchTarget_Sweeper_Patch

                // No-Sweep Zone: block dupe sweeps from marked cells
                if (ControlledStorageOptions.Instance.EnableNoSweepZones)
                {
                    var noSweep = ControlledStorage.NoSweepZone.NoSweepZoneSaveState.Instance;
                    if (noSweep != null && noSweep.NoSweep.ContainsCell(pickup.cachedCell))
                    {
                        __result = false;
                        return;
                    }
                }

                // Deposit: block fetches TO our storage when dupe deposit is disabled
                if (destination != null)
                {
                    var destControl = destination.GetComponent<StorageDeliveryControl>();
                    if (destControl != null && !destControl.AllowDupeDeposit)
                    {
                        __result = false;
                        return;
                    }
                }

                // Extract: block fetches FROM our storage when dupe extract is disabled
                var sourceStorage = pickup.storage;
                if (sourceStorage != null)
                {
                    var sourceControl = sourceStorage.GetComponent<StorageDeliveryControl>();
                    if (sourceControl != null && !sourceControl.AllowDupeExtract)
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}
