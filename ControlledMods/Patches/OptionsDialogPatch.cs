using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;

namespace ControlledMods.Patches
{
    // Patch to increase the size of the PLib options dialog
    // Uses runtime patching because PDialog.SetDialogSize is private
    public static class OptionsDialogPatch
    {
        // Target dialog dimensions (original max is 800x600)
        // We want something larger since we have categories that need more space
        private const float TARGET_WIDTH = 900f;
        private const float TARGET_HEIGHT = 700f;

        // Call this from OnLoad to apply the patch. Only patches our own assembly so we only resize our options dialog.
        public static void ApplyPatch(Harmony harmony)
        {
            try
            {
                var ourAssembly = typeof(ControlledModsMod).Assembly;
                Type pDialogType = null;
                foreach (var type in ourAssembly.GetTypes())
                {
                    if (type.FullName == "PeterHan.PLib.UI.PDialog")
                    {
                        pDialogType = type;
                        break;
                    }
                }

                if (pDialogType == null)
                    return;

                var setDialogSizeMethod = AccessTools.Method(pDialogType, "SetDialogSize");
                if (setDialogSizeMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(OptionsDialogPatch), nameof(SetDialogSize_Prefix));
                    harmony.Patch(setDialogSizeMethod, prefix: prefix);
                }
                else
                {
                    var buildMethod = AccessTools.Method(pDialogType, "Build");
                    if (buildMethod != null)
                    {
                        var buildPrefix = new HarmonyMethod(typeof(OptionsDialogPatch), nameof(Build_Prefix));
                        harmony.Patch(buildMethod, prefix: buildPrefix);
                    }
                }
            }
            catch (Exception e)
            {
                ControlledModsMod.LogWarning($"Failed to apply OptionsDialog patch: {e.Message}\n{e.StackTrace}");
            }
        }

        // Prefix for PDialog.Build() - modifies Size and MaxSize before the dialog is built
        private static void Build_Prefix(object __instance)
        {
            try
            {
                var type = __instance.GetType();
                
                // Check if this is a ModOptions dialog by looking at the Name
                var nameProperty = AccessTools.Property(type, "Name");
                if (nameProperty != null)
                {
                    var name = nameProperty.GetValue(__instance) as string;
                    ControlledModsMod.Log($"PDialog.Build called for: {name}");
                    
                    // Only modify ModOptions dialogs
                    if (name == "ModOptions")
                    {
                        // Modify MaxSize property
                        var maxSizeProperty = AccessTools.Property(type, "MaxSize");
                        if (maxSizeProperty != null)
                        {
                            var currentMax = (Vector2)maxSizeProperty.GetValue(__instance);
                            ControlledModsMod.Log($"Current MaxSize: {currentMax}");
                            
                            maxSizeProperty.SetValue(__instance, new Vector2(TARGET_WIDTH, TARGET_HEIGHT));
                            ControlledModsMod.Log($"Set MaxSize to: {TARGET_WIDTH}x{TARGET_HEIGHT}");
                        }
                        
                        // Modify Size property (minimum size)
                        var sizeProperty = AccessTools.Property(type, "Size");
                        if (sizeProperty != null)
                        {
                            var currentSize = (Vector2)sizeProperty.GetValue(__instance);
                            ControlledModsMod.Log($"Current Size: {currentSize}");
                            
                            // Set minimum size to be larger
                            sizeProperty.SetValue(__instance, new Vector2(TARGET_WIDTH * 0.8f, TARGET_HEIGHT * 0.6f));
                            ControlledModsMod.Log($"Set Size to: {TARGET_WIDTH * 0.8f}x{TARGET_HEIGHT * 0.6f}");
                        }

                    }
                }
            }
            catch (Exception e)
            {
                ControlledModsMod.LogWarning($"Failed to modify dialog size in Build_Prefix: {e.Message}");
            }
        }

        // Prefix for PDialog.SetDialogSize() - intercepts the sizing
        private static void SetDialogSize_Prefix(object __instance, GameObject dialog)
        {
            try
            {
                var type = __instance.GetType();
                
                // Check if this is a ModOptions dialog
                var nameProperty = AccessTools.Property(type, "Name");
                if (nameProperty != null)
                {
                    var name = nameProperty.GetValue(__instance) as string;
                    ControlledModsMod.Log($"PDialog.SetDialogSize called for: {name}");
                    
                    if (name == "ModOptions")
                    {
                        // Modify MaxSize before the sizing calculation runs
                        var maxSizeProperty = AccessTools.Property(type, "MaxSize");
                        if (maxSizeProperty != null)
                        {
                            maxSizeProperty.SetValue(__instance, new Vector2(TARGET_WIDTH, TARGET_HEIGHT));
                            ControlledModsMod.Log($"Set MaxSize to: {TARGET_WIDTH}x{TARGET_HEIGHT} before sizing");
                        }
                        
                        // Also increase minimum size
                        var sizeProperty = AccessTools.Property(type, "Size");
                        if (sizeProperty != null)
                        {
                            sizeProperty.SetValue(__instance, new Vector2(TARGET_WIDTH * 0.8f, TARGET_HEIGHT * 0.6f));
                            ControlledModsMod.Log($"Set Size to: {TARGET_WIDTH * 0.8f}x{TARGET_HEIGHT * 0.6f} before sizing");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ControlledModsMod.LogWarning($"Failed to modify dialog size in SetDialogSize_Prefix: {e.Message}");
            }
        }
    }
}
