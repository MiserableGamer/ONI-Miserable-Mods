using HarmonyLib;
using KMod;
using UnityEngine;

namespace CopyMaterialsTool
{
    public sealed class Mod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            harmony.PatchAll();

            var asm = typeof(Mod).Assembly;
            int count = 0;
            foreach (var t in asm.GetTypes())
            {
                if (t.FullName != null && t.FullName.Contains("UIDump"))
                {
                    Debug.Log("[CopyMaterialsTool] Found type in assembly: " + t.FullName);
                    count++;
                }
            }
            Debug.Log("[CopyMaterialsTool] UIDump types found: " + count);

            Debug.Log("[CopyMaterialsTool] Loaded");
            HarmonyLib.Harmony.DEBUG = true;
            Debug.Log("[CopyMaterialsTool] Harmony DEBUG enabled");
        }
    }
}
