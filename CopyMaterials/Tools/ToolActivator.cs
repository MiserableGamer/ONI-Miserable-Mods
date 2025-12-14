using HarmonyLib;
using System;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class ToolActivator
    {
        internal static void Activate(InterfaceTool tool)
        {
            if (tool == null) return;

            var pc = PlayerController.Instance;
            if (pc == null) return;

            var pcType = pc.GetType();

            var mi =
                AccessTools.Method(pcType, "ActivateTool", new[] { typeof(InterfaceTool) }) ??
                AccessTools.Method(pcType, "SetTool", new[] { typeof(InterfaceTool) }) ??
                AccessTools.Method(pcType, "SetActiveTool", new[] { typeof(InterfaceTool) }) ??
                AccessTools.Method(pcType, "SwitchTool", new[] { typeof(InterfaceTool) });

            if (mi != null)
            {
                mi.Invoke(pc, new object[] { tool });
                return;
            }

            // last resort: try direct tool activation
            var direct =
                AccessTools.Method(tool.GetType(), "ActivateTool") ??
                AccessTools.Method(tool.GetType(), "OnActivateTool");
            if (direct != null)
                direct.Invoke(tool, null);
        }
    }
}
