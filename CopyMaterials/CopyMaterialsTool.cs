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
            Debug.Log("[CopyMaterialsTool] Loaded");
        }
    }
}
