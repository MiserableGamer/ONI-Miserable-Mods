using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
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
            if (opts.TintLogicTerminalLight)
                PatchLogicTerminalLightTint(harmony);
        }

        // --- Logic Terminal light tint: child overlay controller with our kanim, tint "output" symbol ---
        private const bool DebugLogicTerminalLight = false;
        private const string OurKanimId = "controlledmods_logic_terminal_kanim";
        private static readonly HashedString OutputSymbolHash = (HashedString)"output";
        private const int StateOff = 0, StateOn = 1, StateRed = 2, StateGreen = 3;
        private static string StateName(int s) => s < 0 ? "?" : (s == StateOff ? "off" : s == StateOn ? "on" : s == StateRed ? "red" : "green");
        private static readonly Color TintUnlit = new Color(0.18f, 0.18f, 0.18f, 1f);
        private static readonly Color TintRed = new Color(1f, 0.25f, 0.25f, 1f);
        private static readonly Color TintGreen = new Color(0.25f, 1f, 0.25f, 1f);
        private static readonly HashedString LogicOperationalPortId = (HashedString)"LogicOperational";
        private static KAnimFile CachedOurKanimFile;
        private static Type CachedLogicTerminalType;
        private static FieldInfo CachedOutputCellField;
        private static ConditionalWeakTable<GameObject, LastLogicTerminalState> LastStateByGo;

        private sealed class LastLogicTerminalState
        {
            public int State = -1;
        }

        private static void PatchLogicTerminalLightTint(Harmony harmony)
        {
            CachedLogicTerminalType = AccessTools.TypeByName("UndergroundConduit.LogicTerminal");
            if (CachedLogicTerminalType == null) return;
            CachedOutputCellField = AccessTools.Field(CachedLogicTerminalType, "OutputCell");
            if (CachedOutputCellField == null) return;

            LastStateByGo = new ConditionalWeakTable<GameObject, LastLogicTerminalState>();

            var configType = AccessTools.TypeByName("UndergroundConduit.Buildings.LogicTerminalConfig");
            if (configType != null)
            {
                var doPost = AccessTools.Method(configType, "DoPostConfigureComplete", new[] { typeof(GameObject) });
                if (doPost != null)
                    harmony.Patch(doPost, postfix: new HarmonyMethod(typeof(UndergroundConduitPatches), nameof(LogicTerminalConfig_DoPostConfigureComplete_Postfix)));
            }

            var logicTerminalOnSpawn = AccessTools.Method(CachedLogicTerminalType, "OnSpawn");
            if (logicTerminalOnSpawn != null)
                harmony.Patch(logicTerminalOnSpawn, postfix: new HarmonyMethod(typeof(UndergroundConduitPatches), nameof(LogicTerminal_OnSpawn_Postfix)));

            var animatorType = AccessTools.TypeByName("UndergroundConduit.Animator");
            if (animatorType != null)
            {
                var simMethod = AccessTools.Method(animatorType, "Sim1000ms", new[] { typeof(float) });
                if (simMethod != null)
                    harmony.Patch(simMethod, postfix: new HarmonyMethod(typeof(UndergroundConduitPatches), nameof(Animator_Sim1000ms_Postfix)));
            }

            ControlledModsMod.Log("Patched Logic Terminal light tint (child overlay + output symbol tint)");
        }

        public static void LogicTerminal_OnSpawn_Postfix(object __instance)
        {
            if (__instance == null) return;
            var comp = __instance as Component;
            if (comp == null || comp.gameObject == null) return;
            var go = comp.gameObject;

            if (CachedOurKanimFile == null)
                CachedOurKanimFile = Assets.GetAnim(OurKanimId);

            if (CachedOurKanimFile != null)
                CreateOutputOverlay(go);

            if (go.GetComponent<LogicTerminalLightController>() == null)
                go.AddComponent<LogicTerminalLightController>();
            UpdateLogicTerminalLight(go);
        }

        private static void CreateOutputOverlay(GameObject parent)
        {
            try
            {
                var parentKbac = parent.GetComponent<KBatchedAnimController>();
                if (parentKbac == null) { ControlledModsMod.LogWarning("[UCLight] No parent KBAC"); return; }

                var meterPrefab = Assets.GetPrefab(MeterConfig.ID);
                if (meterPrefab == null) { ControlledModsMod.LogWarning("[UCLight] MeterConfig prefab not found"); return; }

                var overlayGo = UnityEngine.Object.Instantiate(meterPrefab);
                overlayGo.name = parent.name + ".outputOverlay";
                overlayGo.SetActive(false);
                overlayGo.transform.parent = parent.transform;

                var pos = parent.transform.GetPosition();
                pos.x -= 0.02f;
                pos.y -= 0.02f;
                pos.z -= 0.1f;
                overlayGo.transform.SetPosition(pos);

                overlayGo.GetComponent<KPrefabID>().PrefabTag = new Tag(overlayGo.name);

                var kbac = overlayGo.GetComponent<KBatchedAnimController>();
                kbac.AnimFiles = new KAnimFile[] { CachedOurKanimFile };
                kbac.initialAnim = "on";
                kbac.initialMode = KAnim.PlayMode.Paused;
                kbac.isMovable = true;
                kbac.FlipX = parentKbac.FlipX;
                kbac.FlipY = parentKbac.FlipY;

                var tracker = overlayGo.GetComponent<KBatchedAnimTracker>();
                if (tracker != null)
                    UnityEngine.Object.Destroy(tracker);

                overlayGo.SetActive(true);

                kbac.SetSymbolVisiblity((HashedString)"base", false);
                kbac.SetSymbolVisiblity((HashedString)"place", false);
                kbac.SetSymbolVisiblity((HashedString)"ui", false);

                new KAnimLink(parentKbac, kbac);

                if (DebugLogicTerminalLight)
                    ControlledModsMod.Log($"[UCLight] Created output overlay on {parent.name}");
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"[UCLight] CreateOutputOverlay failed: {ex.Message}");
            }
        }

        public static void LogicTerminalConfig_DoPostConfigureComplete_Postfix(GameObject go)
        {
            if (go == null || !ControlledModsOptions.Instance.TintLogicTerminalLight) return;
            if (go.GetComponent(CachedLogicTerminalType) == null) return;
            go.AddComponent<LogicTerminalLightController>();
        }

        public static void Animator_Sim1000ms_Postfix(object __instance)
        {
            if (__instance == null || CachedLogicTerminalType == null) return;
            var comp = __instance as Component;
            if (comp == null || comp.gameObject == null) return;
            if (comp.gameObject.GetComponent(CachedLogicTerminalType) == null) return;
            UpdateLogicTerminalLight(comp.gameObject);
        }

        internal static void UpdateLogicTerminalLight(GameObject go, int? eventValue = null)
        {
            if (go == null || CachedLogicTerminalType == null || CachedOutputCellField == null || LastStateByGo == null) return;
            var operational = go.GetComponent<Operational>() ?? go.GetComponentInChildren<Operational>();
            var terminal = go.GetComponent(CachedLogicTerminalType);
            var animators = go.GetComponentsInChildren<KBatchedAnimController>(true);
            if (operational == null || terminal == null || animators == null || animators.Length == 0) return;

            var last = LastStateByGo.GetOrCreateValue(go);

            if (DebugLogicTerminalLight)
            {
                string source = eventValue.HasValue ? "Event" : "Poll";
                ControlledModsMod.Log($"[UCLight] Update src={source} ev={eventValue?.ToString() ?? "-"} isActive={operational.IsActive} go={go.name}");
            }

            int desired;
            if (eventValue.HasValue)
            {
                if (!operational.IsActive) return;
                desired = LogicCircuitNetwork.IsBitActive(0, eventValue.Value) ? StateGreen : StateRed;
            }
            else
            {
                if (!operational.IsActive)
                    desired = StateOff;
                else
                {
                    int outputCell = (int)CachedOutputCellField.GetValue(terminal);
                    if (outputCell < 0)
                        desired = StateOn;
                    else
                    {
                        var network = Game.Instance.logicCircuitSystem.GetNetworkForCell(outputCell) as LogicCircuitNetwork;
                        if (network == null)
                            desired = (last.State == StateRed || last.State == StateGreen) ? last.State : StateOn;
                        else
                            desired = LogicCircuitNetwork.IsBitActive(0, network.OutputValue) ? StateGreen : StateRed;
                    }
                }
            }

            if (last.State == desired) return;
            if (DebugLogicTerminalLight)
                ControlledModsMod.Log($"[UCLight] {StateName(last.State)} → {StateName(desired)}");
            last.State = desired;
            Color tint = desired == StateOff ? TintUnlit : desired == StateOn ? Color.white : desired == StateRed ? TintRed : TintGreen;
            foreach (var anim in animators)
            {
                if (anim == null) continue;
                anim.SetSymbolTint(OutputSymbolHash, tint);
            }
        }

        private sealed class LogicTerminalLightController : KMonoBehaviour, ISim1000ms
        {
            private static readonly EventSystem.IntraObjectHandler<LogicTerminalLightController> OnLogicValueChangedDelegate =
                new EventSystem.IntraObjectHandler<LogicTerminalLightController>((c, data) => c.OnLogicValueChanged(data));

            public override void OnSpawn()
            {
                base.OnSpawn();
                Subscribe((int)GameHashes.LogicEvent, OnLogicValueChangedDelegate);
                UpdateLogicTerminalLight(gameObject);
            }

            public void Sim1000ms(float dt)
            {
                UpdateLogicTerminalLight(gameObject);
            }

            public override void OnCleanUp()
            {
                Unsubscribe((int)GameHashes.LogicEvent, OnLogicValueChangedDelegate);
                base.OnCleanUp();
            }

            private void OnLogicValueChanged(object data)
            {
                if (!(data is LogicValueChanged lvc) || lvc.portID != LogicOperationalPortId) return;
                if (DebugLogicTerminalLight)
                    ControlledModsMod.Log($"[UCLight] LogicEvent port={lvc.portID} val={lvc.newValue}");
                UpdateLogicTerminalLight(gameObject, lvc.newValue);
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
