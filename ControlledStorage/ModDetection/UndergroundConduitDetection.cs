using HarmonyLib;
using System;

namespace ControlledStorage.ModDetection
{
    // Detection for KIN's Underground Conduit mod. Used to optionally add Delivery Control to its Storage Sender.
    public static class UndergroundConduitDetection
    {
        public const string DisplayName = "KIN Underground Conduit";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            try
            {
                var type = AccessTools.TypeByName("UndergroundConduit.Mod");
                Loaded = type != null;
            }
            catch
            {
                Loaded = false;
            }
        }
    }
}
