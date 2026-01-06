using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
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

        // Call this from OnLoad to apply the patch
        public static void ApplyPatch(Harmony harmony)
        {
            try
            {
                ControlledModsMod.Log("=== Starting PDialog patch application ===");
                
                int patchCount = 0;
                
                // Find ALL PDialog types across ALL loaded assemblies
                // Each mod that bundles PLib has its own copy, so we need to patch them all
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        Type pDialogType = null;
                        foreach (var type in assembly.GetTypes())
                        {
                            if (type.FullName == "PeterHan.PLib.UI.PDialog")
                            {
                                pDialogType = type;
                                break;
                            }
                        }
                        
                        if (pDialogType == null) continue;
                        
                        ControlledModsMod.Log($"Found PDialog in assembly: {assembly.GetName().Name}");
                        
                        // Try to patch SetDialogSize
                        var setDialogSizeMethod = AccessTools.Method(pDialogType, "SetDialogSize");
                        if (setDialogSizeMethod != null)
                        {
                            var prefix = new HarmonyMethod(typeof(OptionsDialogPatch), nameof(SetDialogSize_Prefix));
                            harmony.Patch(setDialogSizeMethod, prefix: prefix);
                            ControlledModsMod.Log($"  Patched SetDialogSize in {assembly.GetName().Name}");
                            patchCount++;
                        }
                        else
                        {
                            // Fallback to Build method
                            var buildMethod = AccessTools.Method(pDialogType, "Build");
                            if (buildMethod != null)
                            {
                                var buildPrefix = new HarmonyMethod(typeof(OptionsDialogPatch), nameof(Build_Prefix));
                                harmony.Patch(buildMethod, prefix: buildPrefix);
                                ControlledModsMod.Log($"  Patched Build in {assembly.GetName().Name}");
                                patchCount++;
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Some assemblies can't be scanned - skip them
                    }
                    catch (Exception ex)
                    {
                        ControlledModsMod.Log($"  Error scanning {assembly.GetName().Name}: {ex.Message}");
                    }
                }
                
                ControlledModsMod.Log($"Applied PDialog resize patch to {patchCount} assemblies");
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
