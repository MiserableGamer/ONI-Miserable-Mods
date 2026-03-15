using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    /// <summary>
    /// Prevents NullReferenceException in Storage.ConsumeIgnoringDisease when Storage.items
    /// contains null references. This can occur when construction completes (Constructable.FinishConstruction)
    /// and the game tries to consume building materials - a rare edge case where an item was destroyed
    /// elsewhere but not removed from the items list.
    ///
    /// The crash occurs when:
    /// 1. Constructable.FinishConstruction calls storage.ConsumeAllIgnoringDisease()
    /// 2. The method iterates over items and calls ConsumeIgnoringDisease(items[i])
    /// 3. items[i] is null - GetComponent on null throws
    /// </summary>
    [HarmonyPatch(typeof(Storage), nameof(Storage.ConsumeAllIgnoringDisease), new Type[] { typeof(Tag) })]
    public class Storage_ConsumeAllIgnoringDisease_Patch
    {
        public static void Prefix(Storage __instance)
        {
            if (__instance?.items == null)
                return;

            int removed = __instance.items.RemoveAll(x => x == null);
            if (removed > 0)
            {
                string entityKey = "Storage_ConsumeAllIgnoringDisease_NullItems";
                int count = CrashTracker.IncrementCrash(entityKey);
                string storageInfo = CrashTracker.GetEntityInfo(__instance?.gameObject);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] Storage.ConsumeAllIgnoringDisease: removed " + removed + " null item(s) from storage");
                Debug.LogWarning("  Storage: " + storageInfo);
                Debug.LogWarning(string.Format("  Occurrence Count: {0} time(s)", count));
                Debug.LogWarning("  Note: Prevents crash when construction completes with stale null references in storage");
            }
        }
    }
}
