using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class ToolActivator
    {
        internal static bool DEBUG_LOGS = false;

        internal static void Activate(InterfaceTool tool)
        {
            if (tool == null)
                return;

            try
            {
                // 1) Preferred: PlayerController.Instance.ActivateTool(tool, ...)
                object pc = AccessTools.Property(typeof(PlayerController), "Instance") != null
                    ? AccessTools.Property(typeof(PlayerController), "Instance").GetValue(null, null)
                    : null;

                if (pc == null)
                {
                    // Some builds use a field instead of property
                    var instField = AccessTools.Field(typeof(PlayerController), "Instance");
                    if (instField != null) pc = instField.GetValue(null);
                }

                if (pc != null)
                {
                    Type pct = pc.GetType();

                    // ActivateTool(InterfaceTool)
                    MethodInfo m1 = AccessTools.Method(pct, "ActivateTool", new[] { typeof(InterfaceTool) });
                    if (m1 != null)
                    {
                        m1.Invoke(pc, new object[] { tool });
                        LogDebug("Activated via PlayerController.ActivateTool(InterfaceTool)");
                        return;
                    }

                    // ActivateTool(InterfaceTool, bool)
                    MethodInfo m2 = AccessTools.Method(pct, "ActivateTool", new[] { typeof(InterfaceTool), typeof(bool) });
                    if (m2 != null)
                    {
                        m2.Invoke(pc, new object[] { tool, true });
                        LogDebug("Activated via PlayerController.ActivateTool(InterfaceTool,bool)");
                        return;
                    }

                    // ActivateTool(InterfaceTool, ToolParameterMenu) etc. (rare)
                    MethodInfo[] candidates = pct.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    for (int i = 0; i < candidates.Length; i++)
                    {
                        MethodInfo mi = candidates[i];
                        if (mi == null || mi.Name != "ActivateTool") continue;

                        ParameterInfo[] ps = mi.GetParameters();
                        if (ps == null || ps.Length == 0) continue;

                        // First arg must accept InterfaceTool
                        if (!ps[0].ParameterType.IsAssignableFrom(typeof(InterfaceTool)) &&
                            !typeof(InterfaceTool).IsAssignableFrom(ps[0].ParameterType))
                            continue;

                        object[] args = new object[ps.Length];
                        args[0] = tool;
                        for (int a = 1; a < args.Length; a++)
                        {
                            // best-effort defaults
                            if (ps[a].ParameterType == typeof(bool))
                                args[a] = true;
                            else
                                args[a] = null;
                        }

                        mi.Invoke(pc, args);
                        LogDebug("Activated via PlayerController.ActivateTool(...) reflection overload");
                        return;
                    }
                }

                // 2) Fallback: ToolMenu.Instance / ActivateTool if present
                var toolMenuType = AccessTools.TypeByName("ToolMenu");
                if (toolMenuType != null)
                {
                    object tm = null;

                    var instProp = AccessTools.Property(toolMenuType, "Instance");
                    if (instProp != null) tm = instProp.GetValue(null, null);

                    if (tm == null)
                    {
                        var instField = AccessTools.Field(toolMenuType, "Instance");
                        if (instField != null) tm = instField.GetValue(null);
                    }

                    if (tm != null)
                    {
                        MethodInfo mt = AccessTools.Method(tm.GetType(), "ActivateTool", new[] { typeof(InterfaceTool) });
                        if (mt != null)
                        {
                            mt.Invoke(tm, new object[] { tool });
                            LogDebug("Activated via ToolMenu.ActivateTool(InterfaceTool)");
                            return;
                        }
                    }
                }

                Debug.LogWarning("[CopyMaterialsTool] Could not activate tool (no known activation path worked).");
            }
            catch (Exception e)
            {
                Debug.LogError("[CopyMaterialsTool] Tool activation failed: " + e);
            }
        }

        private static void LogDebug(string msg)
        {
            if (DEBUG_LOGS)
                Debug.Log("[CopyMaterialsTool] " + msg);
        }
    }
}
