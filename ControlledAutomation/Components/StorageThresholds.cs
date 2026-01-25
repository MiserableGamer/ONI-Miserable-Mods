using HarmonyLib;

namespace ControlledAutomation.Components
{
    public class StorageThresholds : ThresholdsBase
    {
        private delegate void UpdateLogicAndActiveStateDelegate(StorageLockerSmart locker);
        private static readonly UpdateLogicAndActiveStateDelegate updateMethod =
            AccessTools.MethodDelegate<UpdateLogicAndActiveStateDelegate>(
                AccessTools.Method(typeof(StorageLockerSmart), "UpdateLogicAndActiveState"));

#pragma warning disable CS0649
        [MyCmpGet] private StorageLockerSmart storageLockerSmart;
#pragma warning restore CS0649

        protected override void UpdateLogicCircuit()
        {
            if (storageLockerSmart != null)
                updateMethod(storageLockerSmart);
        }
    }
}
