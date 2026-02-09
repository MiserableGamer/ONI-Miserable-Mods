using HarmonyLib;
using System;
using System.Reflection;
using ControlledMods.ModDetection;
using ControlledMods.Options;
using UnityEngine;

namespace ControlledMods.Patches.UndergroundConduit
{
    // Patches for KIN's Underground Conduit mod. Only applied when the mod is detected. Each fix can be toggled in mod options.
    public static class UndergroundConduitPatches
    {
        private const int CopySettingsHash = -905833192;

        public static void ApplyPatches(Harmony harmony)
        {
            if (!UndergroundConduitDetection.Loaded)
                return;

            var opts = ControlledModsOptions.Instance;
            if (opts.FixPowerTerminalLogicWireCrash)
                PatchOperationalChangedGuard(harmony);
            if (opts.EnableCopySettingsForConduits)
            {
                PatchConfigsAddCopyBuildingSettings(harmony);
                PatchKPrefabIDTriggerForChannelCopy(harmony);
            }
        }

        // PowerTerminal and LogicTerminal OnOperationalChanged(object data) do (bool)data. When a logic wire is in the same
        // cell, the same event hash can be triggered with non-bool data, causing InvalidCastException. Skip the original when data is not bool.
        private static void PatchOperationalChangedGuard(Harmony harmony)
        {
            var prefix = new HarmonyMethod(typeof(UndergroundConduitPatches), nameof(OperationalChanged_Guard_Prefix));
            PatchOperationalChangedGuardForType(harmony, "UndergroundConduit.PowerTerminal", "PowerTerminal", prefix);
            PatchOperationalChangedGuardForType(harmony, "UndergroundConduit.LogicTerminal", "LogicTerminal", prefix);
        }

        private static void PatchOperationalChangedGuardForType(Harmony harmony, string typeName, string displayName, HarmonyMethod prefix)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type == null) return;
            var method = AccessTools.Method(type, "OnOperationalChanged", new[] { typeof(object) });
            if (method == null) return;
            harmony.Patch(method, prefix: prefix);
            ControlledModsMod.Log($"Patched {displayName}.OnOperationalChanged (guard against non-bool data)");
        }

        // Return true to run original, false to skip. Skip when data is not bool to avoid InvalidCastException.
        public static bool OperationalChanged_Guard_Prefix(object data)
        {
            return data is bool;
        }

        // --- Copy Settings: when the game triggers CopySettings on the destination building, copy channel from source (no Grid lookup) ---
        // Trigger is declared on KMonoBehaviour; Harmony requires patching the declared method, not the override on KPrefabID.
        private static void PatchKPrefabIDTriggerForChannelCopy(Harmony harmony)
        {
            var kmonoType = AccessTools.TypeByName("KMonoBehaviour");
            if (kmonoType == null) return;
            var triggerMethod = AccessTools.Method(kmonoType, "Trigger", new[] { typeof(int), typeof(object) });
            if (triggerMethod == null) return;
            harmony.Patch(triggerMethod, postfix: new HarmonyMethod(typeof(UndergroundConduitPatches), nameof(KPrefabID_Trigger_Postfix_CopyChannel)));
        }

        public static void KPrefabID_Trigger_Postfix_CopyChannel(int hash, object data, object __instance)
        {
            if (hash != CopySettingsHash || !(data is GameObject source) || source == null) return;
            var comp = __instance as Component;
            if (comp == null) return;
            GameObject dest = comp.gameObject;
            if (dest == null || dest == source) return;
            CopyChannelFromSourceToDestination(source, dest);
        }

        // Get the Channel property without ambiguity (types like PowerTerminal have "new Channel" that shadows base).
        private static PropertyInfo GetChannelProperty(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            for (var t = type; t != null; t = t.BaseType)
            {
                var prop = t.GetProperty("Channel", flags);
                if (prop != null)
                    return prop;
            }
            return null;
        }

        // Replicate what KIN's working types do (RadboltSender, StorageSender, HeatConduitTerminal): pass the Channel object to SetChannel, not the name.
        private static void CopyChannelFromSourceToDestination(GameObject source, GameObject dest)
        {
            var components = dest.GetComponentsInChildren<Component>(true);
            foreach (var destComp in components)
            {
                if (destComp == null) continue;
                var t = destComp.GetType();
                var channelProp = GetChannelProperty(t);
                if (channelProp == null) continue;
                var sourceComp = source.GetComponent(t);
                if (sourceComp == null) continue;
                var channel = channelProp.GetValue(sourceComp);
                if (channel == null) continue;
                var channelType = channel.GetType();
                var setChannelWithObject = AccessTools.Method(t, "SetChannel", new[] { channelType });
                if (setChannelWithObject == null) continue;
                try
                {
                    setChannelWithObject.Invoke(destComp, new object[] { channel });
                }
                catch (Exception ex)
                {
                    ControlledModsMod.LogWarning($"Copy channel failed for {t.Name}: {ex.Message}");
                }
            }
        }

        // --- Copy Settings: add CopyBuildingSettings to prefabs so the Copy Settings button appears ---
        private static void PatchConfigsAddCopyBuildingSettings(Harmony harmony)
        {
            var kinAssembly = AccessTools.TypeByName("UndergroundConduit.PowerTerminal")?.Assembly;
            if (kinAssembly == null)
                return;

            var configTypeNames = new[]
            {
                "UndergroundConduit.Buildings.PowerTerminalConfig",
                "UndergroundConduit.Buildings.LiquidReceiverConfig",
                "UndergroundConduit.Buildings.LiquidSenderConfig",
                "UndergroundConduit.Buildings.GasReceiverConfig",
                "UndergroundConduit.Buildings.GasSenderConfig",
                "UndergroundConduit.Buildings.LogicTerminalConfig",
                "UndergroundConduit.Buildings.SolidReceiverConfig",
                "UndergroundConduit.Buildings.SolidSenderConfig",
                "UndergroundConduit.Buildings.RadboltSenderConfig",
                "UndergroundConduit.Buildings.RadboltReceiverConfig",
                "UndergroundConduit.Buildings.StorageSenderConfig",
                "UndergroundConduit.Buildings.StorageReceiverConfig"
            };

            var copySettingsType = AccessTools.TypeByName("CopyBuildingSettings");
            var addOrGetMethod = GetAddOrGetCopyBuildingSettingsMethod(copySettingsType);
            if (copySettingsType == null || addOrGetMethod == null)
            {
                ControlledModsMod.LogWarning("CopyBuildingSettings or AddOrGet not found - skipping Copy Settings config patches");
                return;
            }

            foreach (var typeName in configTypeNames)
            {
                var configType = kinAssembly.GetType(typeName);
                if (configType == null)
                    continue;
                var method = AccessTools.Method(configType, "DoPostConfigureComplete", new[] { typeof(GameObject) });
                if (method == null)
                    continue;
                var postfix = new HarmonyMethod(typeof(UndergroundConduitPatches), nameof(Config_DoPostConfigureComplete_Postfix));
                harmony.Patch(method, postfix: postfix);
            }
            ControlledModsMod.Log("Patched KIN conduit configs to add CopyBuildingSettings");
        }

        private static MethodInfo GetAddOrGetCopyBuildingSettingsMethod(Type copyBuildingSettingsType)
        {
            if (copyBuildingSettingsType == null) return null;
            var extType = AccessTools.TypeByName("EntityTemplateExtensions");
            if (extType == null) return null;
            foreach (var m in extType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (m.Name != "AddOrGet" || !m.IsGenericMethod || m.GetParameters().Length != 1) continue;
                var ga = m.GetGenericArguments();
                if (ga.Length == 1)
                {
                    var generic = m.MakeGenericMethod(copyBuildingSettingsType);
                    if (generic.GetParameters()[0].ParameterType == typeof(GameObject))
                        return generic;
                }
            }
            return null;
        }

        public static void Config_DoPostConfigureComplete_Postfix(GameObject go)
        {
            if (go == null) return;
            var copyType = AccessTools.TypeByName("CopyBuildingSettings");
            var addOrGet = GetAddOrGetCopyBuildingSettingsMethod(copyType);
            addOrGet?.Invoke(null, new object[] { go });
        }

    }
}
