using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BlankProject
{
    /// <summary>
    /// Main entry point for the BlankProject mod.
    /// </summary>
    public sealed class BlankProject : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            // Initialize PLib and options
            PUtil.InitLibrary();

            UnityEngine.Debug.Log("[BlankProject] Mod loaded successfully");
        }
    }
}

