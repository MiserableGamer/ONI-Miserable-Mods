using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using ControlledExtraction.Options;

namespace ControlledExtraction
{
    public class ControlledExtractionMod : KMod.UserMod2
    {
        public static bool EnableDebugLogs = false;
        
        private static bool? ronivansLegacyLoaded = null;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(ControlledExtractionOptions));
            harmony.PatchAll();
        }

        public static void Log(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[ControlledExtraction] {message}");
            }
        }

        // Check if Ronivan's Legacy is ENABLED (not just installed)
        public static bool IsRonivansLegacyLoaded()
        {
            if (!ronivansLegacyLoaded.HasValue)
            {
                ronivansLegacyLoaded = false;
                
                if (Global.Instance?.modManager?.mods != null)
                {
                    foreach (var mod in Global.Instance.modManager.mods)
                    {
                        bool isRonivansLegacy = mod.IsEnabledForActiveDlc() && 
                            (mod.title.ToLower().Contains("ronivan") || 
                             mod.staticID.ToLower().Contains("ronivan"));
                        
                        if (isRonivansLegacy)
                        {
                            ronivansLegacyLoaded = true;
                            Log($"Ronivan's Legacy detected: {mod.title}");
                            break;
                        }
                    }
                }
            }
            return ronivansLegacyLoaded.Value;
        }
    }
}
