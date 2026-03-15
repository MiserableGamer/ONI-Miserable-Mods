using System;
using System.Reflection;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace ControlledCrashes
{
    public sealed class ControlledCrashesMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();

            harmony.PatchAll();
            TryPatch3GuBsElectrobankCharger(harmony);
        }

        // 3GuBsVisualFixesNTweaks adds a Postfix to ElectrobankCharger.UpdateMeter that calls
        // UpdateSymbolSwap, which crashes when Storage.items[0] is null or components are missing.
        // We prefix their UpdateSymbolSwap to skip when state is unsafe.
        private static void TryPatch3GuBsElectrobankCharger(Harmony harmony)
        {
            try
            {
                const string typeName = "_3GuBsVisualFixesNTweaks.Patches.ElectrobankCharger_Patches+ElectrobankCharger_Instance_UpdateMeter_Patch";
                Type targetType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    targetType = asm.GetType(typeName);
                    if (targetType != null) break;
                }
                if (targetType == null) return;

                var method = targetType.GetMethod("UpdateSymbolSwap",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    null, new[] { typeof(ElectrobankCharger.Instance) }, null);
                if (method == null) return;

                var prefix = typeof(Patches.ThreeGuBsElectrobankCharger_SkipUnsafeUpdateSymbolSwap).GetMethod("Prefix");
                harmony.Patch(method, prefix: new HarmonyMethod(prefix));
            }
            catch
            {
                // 3GuBs not loaded or different version; our Finalizer still catches crashes
            }
        }
    }
}
