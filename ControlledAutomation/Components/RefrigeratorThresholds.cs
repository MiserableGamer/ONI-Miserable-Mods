using HarmonyLib;
using System;
using System.Reflection;

namespace ControlledAutomation.Components
{
    /// <summary>
    /// Thresholds component for Refrigerator.
    /// Also supports the Freezer mod which copy-pastes the Refrigerator class.
    /// </summary>
    public class RefrigeratorThresholds : ThresholdsBase
    {
        private delegate void UpdateLogicCircuitDelegate(Refrigerator refrigerator);
        private static readonly UpdateLogicCircuitDelegate updateMethod
            = AccessTools.MethodDelegate<UpdateLogicCircuitDelegate>(
                AccessTools.Method(typeof(Refrigerator), "UpdateLogicCircuit"));

#pragma warning disable CS0649
        [MyCmpGet]
        private Refrigerator refrigerator;
#pragma warning restore CS0649

        protected override void UpdateLogicCircuit()
        {
            if (refrigerator != null)
            {
                updateMethod(refrigerator);
                return;
            }

            // Support for Freezer mod which copy-pastes the Refrigerator class
            Type freezerType = Type.GetType("Psyko.Freezer.Freezer, Freezer");
            if (freezerType != null)
            {
                MethodInfo info = AccessTools.Method(freezerType, "UpdateLogicCircuit");
                var freezer = GetComponent(freezerType);
                if (freezer != null && info != null)
                    info.Invoke(freezer, null);
            }
        }
    }
}
