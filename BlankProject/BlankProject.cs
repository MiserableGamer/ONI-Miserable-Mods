using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using UnityEngine;

namespace BlankProject
{
    /// <summary>
    /// Main entry point for the BlankProject mod template.
    /// This is a simple template - rename BlankProject to your mod name throughout all files.
    /// </summary>
    public sealed class BlankProject : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            // Initialize PLib
            PUtil.InitLibrary();

            UnityEngine.Debug.Log("[BlankProject] Mod loaded successfully");
        }
    }
}

