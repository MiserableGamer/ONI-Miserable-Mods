using UnityEngine;

namespace ControlledCrashes.Patches
{
    // Prefix for 3GuBs UpdateSymbolSwap: skip when state is unsafe (null item, missing components).
    // Prevents NullReferenceException instead of catching in Finalizer.
    public static class ThreeGuBsElectrobankCharger_SkipUnsafeUpdateSymbolSwap
    {
        public static bool Prefix(ElectrobankCharger.Instance smi)
        {
            if (smi == null) return false;
            var storage = smi.Storage;
            if (storage?.items == null || storage.items.Count == 0) return false;
            var firstItem = storage.items[0];
            if (firstItem == null) return false;
            var kbac = smi.GetComponent<KBatchedAnimController>();
            if (kbac == null) return false;
            var soc = kbac.GetComponent<SymbolOverrideController>();
            if (soc == null) return false;
            var itemKbac = firstItem.GetComponent<KBatchedAnimController>();
            if (itemKbac == null || itemKbac.AnimFiles == null || itemKbac.AnimFiles.Length == 0) return false;
            var data = itemKbac.AnimFiles[0].GetData();
            if (data?.build?.symbols == null || data.build.symbols.Length == 0) return false;
            return true;
        }
    }
}
