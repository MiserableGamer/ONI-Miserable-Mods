using HarmonyLib;
using UnityEngine;

namespace ControlledStorage.Patches
{
    // Adds Delivery Control to KIN's Underground Conduit Storage Sender when that mod is loaded and the option is enabled.
    public static class KINUndergroundConduitPatches
    {
        public static void ApplyPatches(Harmony harmony)
        {
            if (!ModDetection.UndergroundConduitDetection.Loaded)
                return;
            if (!ControlledStorageOptions.Instance.EnableDeliveryControlKINStorageSender)
                return;

            var configType = AccessTools.TypeByName("UndergroundConduit.Buildings.StorageSenderConfig");
            if (configType == null)
                return;

            var method = AccessTools.Method(configType, "DoPostConfigureComplete", new[] { typeof(GameObject) });
            if (method == null)
                return;

            harmony.Patch(method, postfix: new HarmonyMethod(typeof(KINUndergroundConduitPatches), nameof(StorageSenderConfig_DoPostConfigureComplete_Postfix)));
        }

        private static void StorageSenderConfig_DoPostConfigureComplete_Postfix(GameObject go)
        {
            if (go == null) return;
            if (!ControlledStorageOptions.Instance.EnableDeliveryControlKINStorageSender) return;
            go.AddOrGet<ControlledStorage.StorageDeliveryControl>();
        }
    }
}
