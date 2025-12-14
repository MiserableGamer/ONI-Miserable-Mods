using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CopyMaterialsTool
{
    internal static class CopyMaterialsButtonInjector
    {
        // Runtime debug toggle (safe; not const)
        internal static bool DEBUG_LOGS = true;

        internal static void TryInject(object detailsScreenObj)
        {
            DetailsScreen detailsScreen = detailsScreenObj as DetailsScreen;
            if (detailsScreen == null)
                return;

            try
            {
                KButton[] buttons = detailsScreen.GetComponentsInChildren<KButton>(true);
                KButton copySettingsBtn = null;

                for (int i = 0; i < buttons.Length; i++)
                {
                    KButton b = buttons[i];
                    if (b == null) continue;
                    if (!b.gameObject.activeInHierarchy) continue;

                    if (IsCopySettingsButton(b))
                    {
                        copySettingsBtn = b;
                        break;
                    }
                }

                if (copySettingsBtn == null)
                {
                    LogDebug("Copy Settings button not found yet.");
                    return;
                }

                Transform parent = copySettingsBtn.transform.parent;
                if (parent == null)
                    return;

                // Prevent duplicates
                if (parent.Find("CopyMaterialsButton") != null)
                {
                    LogDebug("Copy Materials already injected.");
                    return;
                }

                // Clone
                GameObject clone = UnityEngine.Object.Instantiate(copySettingsBtn.gameObject, parent);
                clone.name = "CopyMaterialsButton";
                clone.SetActive(true);

                clone.transform.SetSiblingIndex(copySettingsBtn.transform.GetSiblingIndex() + 1);

                RectTransform rtClone = clone.transform as RectTransform;
                if (rtClone != null)
                    rtClone.localScale = Vector3.one;

                // ---- Layout safety ----
                LayoutElement srcLE = copySettingsBtn.GetComponent<LayoutElement>();
                LayoutElement dstLE = clone.GetComponent<LayoutElement>() ?? clone.AddComponent<LayoutElement>();

                if (srcLE != null)
                {
                    dstLE.minWidth = srcLE.minWidth;
                    dstLE.minHeight = srcLE.minHeight;
                    dstLE.preferredWidth = srcLE.preferredWidth;
                    dstLE.preferredHeight = srcLE.preferredHeight;
                    dstLE.flexibleWidth = srcLE.flexibleWidth;
                    dstLE.flexibleHeight = srcLE.flexibleHeight;
                }
                else
                {
                    dstLE.preferredWidth = 24f;
                    dstLE.preferredHeight = 24f;
                    dstLE.flexibleWidth = 0f;
                    dstLE.flexibleHeight = 0f;
                }
                dstLE.ignoreLayout = false;

                // ---- Force graphics visible & raycastable ----
                Graphic[] graphics = clone.GetComponentsInChildren<Graphic>(true);
                for (int i = 0; i < graphics.Length; i++)
                {
                    Graphic g = graphics[i];
                    if (g == null) continue;

                    g.enabled = true;
                    g.raycastTarget = true;

                    Color c = g.color;
                    c.a = 1f;
                    g.color = c;
                }

                // ---- Label ----
                LocText label = clone.GetComponentInChildren<LocText>(true);
                if (label != null)
                    label.text = "Copy Materials";

                // ---- Tooltip ----
                ToolTip tip = clone.GetComponent<ToolTip>();
                if (tip != null)
                    tip.toolTip = "Copy construction materials; drag to apply";

                // ---- Button wiring ----
                KButton btn = clone.GetComponent<KButton>();
                if (btn == null)
                {
                    Debug.LogWarning("[CopyMaterialsTool] Clone has no KButton component.");
                    return;
                }

                // This is the ONLY enable flag guaranteed to exist
                btn.enabled = true;

                btn.onClick -= ActivateCopyMaterialsTool;
                btn.onClick += ActivateCopyMaterialsTool;

                // ---- Force layout rebuild ----
                Transform rebuild = parent;
                for (int i = 0; i < 6 && rebuild != null; i++)
                {
                    RectTransform rt = rebuild as RectTransform;
                    if (rt != null)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

                    rebuild = rebuild.parent;
                }

                LogDebug("Copy Materials button injected and wired.");
            }
            catch (Exception e)
            {
                Debug.LogError("[CopyMaterialsTool] Injection failed: " + e);
            }
        }

        private static void ActivateCopyMaterialsTool()
        {
            LogDebug("Copy Materials button clicked.");
            ToolActivator.DEBUG_LOGS = DEBUG_LOGS;

            CopyMaterialsTool tool = CopyMaterialsToolSingleton.GetOrCreate();
            ToolActivator.Activate(tool);
        }

        private static bool IsCopySettingsButton(KButton b)
        {
            if (b == null) return false;

            string n = b.gameObject != null ? (b.gameObject.name ?? "") : "";
            if (n.IndexOf("copy", StringComparison.OrdinalIgnoreCase) >= 0 &&
                n.IndexOf("setting", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            LocText lt = b.GetComponentInChildren<LocText>(true);
            if (lt != null && !string.IsNullOrEmpty(lt.text))
            {
                string t = lt.text.ToLowerInvariant();
                if (t.Contains("copy") && t.Contains("setting"))
                    return true;
            }

            return false;
        }

        private static void LogDebug(string msg)
        {
            if (DEBUG_LOGS)
                Debug.Log("[CopyMaterialsTool] " + msg);
        }
    }

    /// <summary>
    /// Hook all likely DetailsScreen update points to avoid first-click timing issues.
    /// </summary>
    [HarmonyPatch]
    internal static class DetailsScreen_AnyUpdate_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            Type t = typeof(DetailsScreen);
            MethodInfo[] methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (m == null) continue;

                string name = m.Name;
                if (name == "set_target" || name == "Refresh" || name == "OnSelectObject" || name == "OnRefreshData")
                    yield return m;
            }
        }

        static void Postfix(object __instance)
        {
            CopyMaterialsButtonInjector.TryInject(__instance);
        }
    }
}
