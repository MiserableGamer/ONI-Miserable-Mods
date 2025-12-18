using HarmonyLib;
using KMod;
using System;
using System.Reflection;
using UnityEngine;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using CopyMaterials.Logic;

namespace CopyMaterials
{
    public class CopyMaterialsMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            // Initialise PLib
            PUtil.InitLibrary();
            CopyMaterialsStrings.Register();

            // Apply Harmony patches if needed
            harmony.PatchAll();

            // Try to manually patch conduit types if they exist
            TryPatchConduitTypes(harmony);
        }

        private void TryPatchConduitTypes(Harmony harmony)
        {
            // Patch all conduit types
            var conduitTypes = new[] { "LiquidConduit", "GasConduit", "InsulatedLiquidConduit", "InsulatedGasConduit" };
            
            foreach (var typeName in conduitTypes)
            {
                CopyMaterialsManager.Log($"Trying to find and patch {typeName}");
                var conduitType = Patches.ConduitOnSpawnPatch.FindTypeInAssemblies(typeName);
                if (conduitType != null)
                {
                    CopyMaterialsManager.Log($"Found {typeName} type in assembly {conduitType.Assembly.GetName().Name}");
                    var onSpawnMethod = conduitType.GetMethod("OnSpawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (onSpawnMethod != null)
                    {
                        CopyMaterialsManager.Log($"Found OnSpawn method for {typeName}, attempting to patch");
                        try
                        {
                            // Create a dynamic postfix method
                            var helperType = typeof(ConduitPatchHelpers);
                            var postfixMethodName = typeName.Replace("Insulated", "").Replace("Conduit", "ConduitPostfix");
                            var postfixMethod = helperType.GetMethod(postfixMethodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                            
                            if (postfixMethod == null)
                            {
                                // Use generic postfix if specific one doesn't exist
                                postfixMethod = helperType.GetMethod("GenericConduitPostfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                            }
                            
                            if (postfixMethod != null)
                            {
                                harmony.Patch(onSpawnMethod, postfix: new HarmonyMethod(postfixMethod) { priority = 1 });
                                CopyMaterialsManager.Log($"Successfully patched {typeName}.OnSpawn");
                            }
                            else
                            {
                                CopyMaterialsManager.Warn($"Could not find postfix method for {typeName}");
                            }
                        }
                        catch (Exception e)
                        {
                            CopyMaterialsManager.Warn($"Failed to patch {typeName}.OnSpawn: {e.Message}\n{e.StackTrace}");
                        }
                    }
                    else
                    {
                        CopyMaterialsManager.Warn($"{typeName} type found but OnSpawn method not found");
                    }
                }
                else
                {
                    CopyMaterialsManager.Warn($"{typeName} type not found");
                }
            }
        }

        private static class ConduitPatchHelpers
        {
            static void LiquidConduitPostfix(KMonoBehaviour __instance)
            {
                Patches.ConduitOnSpawnPatch.OnSpawnPostfix(__instance, "LiquidConduit");
            }

            static void GasConduitPostfix(KMonoBehaviour __instance)
            {
                Patches.ConduitOnSpawnPatch.OnSpawnPostfix(__instance, "GasConduit");
            }

            static void GenericConduitPostfix(KMonoBehaviour __instance)
            {
                // Try to determine the type name from the instance
                string typeName = __instance.GetType().Name;
                Patches.ConduitOnSpawnPatch.OnSpawnPostfix(__instance, typeName);
            }
        }
    }
}