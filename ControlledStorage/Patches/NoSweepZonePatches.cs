using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using PeterHan.PLib.PatchManager;
using PeterHan.PLib.Core;
using PeterHan.PLib.Actions;
using ControlledStorage.NoSweepZone;

namespace ControlledStorage.Patches
{
    public static class NoSweepZonePatches
    {
        internal static PAction ToolAction { get; private set; }
        internal static PAction OverlayAction { get; private set; }

        [PLibMethod(RunAt.BeforeDbInit)]
        internal static void BeforeDbInit()
        {
            var toolIcon = ICONS.TOOL_ICON_SPRITE;
            var setIcon = ICONS.SET_VISUALIZER_SPRITE;
            var cancelIcon = ICONS.CANCEL_VISUALIZER_SPRITE;
            Assets.Sprites.Add(toolIcon.name, toolIcon);
            Assets.Sprites.Add(setIcon.name, setIcon);
            Assets.Sprites.Add(cancelIcon.name, cancelIcon);

            Strings.Add(NoSweepZone.UI.STRINGS.OVERLAY_NAME.Key, NoSweepZone.UI.STRINGS.OVERLAY_NAME.Value);
            Strings.Add(NoSweepZone.UI.STRINGS.OVERLAY_DESCRIPTION.Key, NoSweepZone.UI.STRINGS.OVERLAY_DESCRIPTION.Value);
        }

        internal static void OnModLoad(Harmony harmony)
        {
            ToolAction = new PActionManager().CreateAction(NoSweepZone.UI.Actions.TOOL_ACTION_KEY, NoSweepZone.UI.Actions.TOOL_ACTION_TITLE, new PKeyBinding(KKeyCode.None));
            OverlayAction = new PActionManager().CreateAction(NoSweepZone.UI.Actions.OVERLAY_ACTION_KEY, NoSweepZone.UI.Actions.OVERLAY_ACTION_TITLE, new PKeyBinding(KKeyCode.None));
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        static class Db_Initialize_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix()
            {
                Strings.Add(NoSweepZone.UI.STRINGS.OVERLAY_NAME.Key, NoSweepZone.UI.STRINGS.OVERLAY_NAME.Value);
                Strings.Add(NoSweepZone.UI.STRINGS.OVERLAY_DESCRIPTION.Key, NoSweepZone.UI.STRINGS.OVERLAY_DESCRIPTION.Value);
            }
        }

        [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
        static class PlayerController_OnPrefabInit_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(PlayerController __instance)
            {
                foreach (var t in __instance.tools)
                    if (t != null && t is NoSweepZoneTool)
                        return;

                var interfaceTools = new List<InterfaceTool>(__instance.tools);
                var toolObj = new GameObject("NoSweepZoneTool");
                toolObj.AddOrGet<NoSweepZoneTool>();
                toolObj.transform.SetParent(__instance.gameObject.transform);
                toolObj.SetActive(true);
                toolObj.SetActive(false);
                interfaceTools.Add(toolObj.GetComponent<InterfaceTool>());
                __instance.tools = interfaceTools.ToArray();
            }
        }

        [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
        static class SaveGame_OnPrefabInit_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(SaveGame __instance)
            {
                __instance.gameObject.AddOrGet<NoSweepZoneSaveState>();
            }
        }

        [HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
        static class ToolMenu_CreateBasicTools_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(ToolMenu __instance)
            {
                foreach (var tc in __instance.basicTools)
                    if (tc?.tools != null)
                        foreach (var ti in tc.tools)
                            if (ti?.toolName == nameof(NoSweepZoneTool))
                                return;

                __instance.basicTools.Add(ToolMenu.CreateToolCollection(
                    NoSweepZone.UI.STRINGS.TOOL_TITLE,
                    NoSweepZone.UI.STRINGS.TOOL_ICON,
                    ToolAction.GetKAction(),
                    nameof(NoSweepZoneTool),
                    NoSweepZone.UI.STRINGS.TOOL_DESCRIPTION,
                    false
                ));
            }
        }

        [HarmonyPatch(typeof(OverlayLegend), "OnSpawn")]
        static class OverlayLegend_OnSpawn_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Prefix(OverlayLegend __instance)
            {
                var instance = Traverse.Create(__instance);
                if (instance.Field("overlayInfoList").FieldExists() && instance.Field("overlayInfoList").GetValue<List<OverlayLegend.OverlayInfo>>() != null)
                {
                    var info = new OverlayLegend.OverlayInfo
                    {
                        name = NoSweepZone.UI.STRINGS.OVERLAY_NAME.Key,
                        mode = NoSweepZoneOverlay.ID,
                        infoUnits = new List<OverlayLegend.OverlayInfoUnit>(),
                        isProgrammaticallyPopulated = true
                    };
                    instance.Field("overlayInfoList").GetValue<List<OverlayLegend.OverlayInfo>>().Add(info);
                }
            }
        }

        [HarmonyPatch(typeof(OverlayMenu), "InitializeToggles")]
        static class OverlayMenu_InitializeToggles_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(List<KIconToggleMenu.ToggleInfo> ___overlayToggleInfos)
            {
                var constructor = AccessTools.Constructor(
                    AccessTools.Inner(typeof(OverlayMenu), "OverlayToggleInfo"),
                    new[] { typeof(string), typeof(string), typeof(HashedString), typeof(string), typeof(Action), typeof(string), typeof(string) }
                );
                var obj = constructor.Invoke(new object[] {
                    NoSweepZone.UI.STRINGS.OVERLAY_NAME.Value,
                    NoSweepZone.UI.STRINGS.OVERLAY_ICON,
                    NoSweepZoneOverlay.ID,
                    "",
                    OverlayAction.GetKAction(),
                    "",
                    NoSweepZone.UI.STRINGS.OVERLAY_NAME.Value
                });
                ___overlayToggleInfos.Add((KIconToggleMenu.ToggleInfo)obj);
            }
        }

        [HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
        static class OverlayScreen_RegisterModes_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            private delegate void RegisterModeDelegate(OverlayScreen instance, OverlayModes.Mode mode);
            private static readonly RegisterModeDelegate RegisterMode = (RegisterModeDelegate)typeof(OverlayScreen)
                .GetMethod("RegisterMode", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.CreateDelegate(typeof(RegisterModeDelegate));

            internal static void Postfix(OverlayScreen __instance)
            {
                RegisterMode?.Invoke(__instance, new NoSweepZoneOverlay());
            }
        }

        [HarmonyPatch(typeof(StatusItem), "GetStatusItemOverlayBySimViewMode")]
        static class StatusItem_GetStatusItemOverlayBySimViewMode_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Prefix(Dictionary<HashedString, StatusItem.StatusItemOverlays> ___overlayBitfieldMap)
            {
                if (!___overlayBitfieldMap.ContainsKey(NoSweepZoneOverlay.ID))
                    ___overlayBitfieldMap.Add(NoSweepZoneOverlay.ID, StatusItem.StatusItemOverlays.None);
            }
        }

        [HarmonyPatch(typeof(SimDebugView), "OnPrefabInit")]
        static class SimDebugView_OnPrefabInit_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix(IDictionary<HashedString, System.Func<SimDebugView, int, Color>> ___getColourFuncs)
            {
                if (!___getColourFuncs.ContainsKey(NoSweepZoneOverlay.ID))
                    ___getColourFuncs.Add(NoSweepZoneOverlay.ID, NoSweepZoneOverlay.GetColor);
            }
        }

        [HarmonyPatch(typeof(ToolMenu), "OnPrefabInit")]
        static class ToolMenu_OnPrefabInit_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix()
            {
                NoSweepZoneToolMenu.CreateInstance();
            }
        }

        [HarmonyPatch(typeof(Game), "DestroyInstances")]
        static class Game_DestroyInstances_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableNoSweepZones;

            internal static void Postfix()
            {
                NoSweepZoneToolMenu.DestroyInstance();
            }
        }
    }
}
