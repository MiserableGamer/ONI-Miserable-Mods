using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using MiserableMods.Shared;
using ControlledVisuals.Patches;

namespace ControlledVisuals
{
    public class ControlledVisualsMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            // Initialize PLib
            PUtil.InitLibrary();

            // Initialize debug overlay (only active in DEBUG builds)
            DevDebug.Init();

            // Register strings for options UI
            RegisterStrings();

            // Register mod options
            new POptions().RegisterOptions(this, typeof(ControlledVisualsOptions));

            // Apply manual patches first (before PatchAll)
            // ConduitFlowVisualizer has static initializers that cause issues with attribute-based patching
            ConduitFlowVisualizerPatches.ApplyPatches(harmony);

            // Apply remaining Harmony patches (Game.OnSpawn, Game.DestroyInstances, etc.)
            harmony.PatchAll();
        }

        private static void RegisterStrings()
        {
            // Category
            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CATEGORY_VISUAL", "Visual");

            // Main option
            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.NAME", "Pipe Animation Quality");
            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.TOOLTIP",
                "Controls the visual fidelity of liquid and gas pipe animations.\n\n" +
                "<i>No changes to actual pipe mechanics will occur - this only affects visuals.</i>\n\n" +
                "<b>Performance Impact: <color=#FF8827>Medium</color></b>");

            // Enum values
            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.FULL", "Full");
            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.FULL_TOOLTIP",
                "Pipe animation quality is unchanged from the base game.\nAnimations update every frame.");

            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.REDUCED", "Reduced");
            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.REDUCED_TOOLTIP",
                "Pipe animations update at 10 FPS (every 0.1 seconds).\n" +
                "When zoomed far out, updates reduce to 1 FPS.\n\n" +
                "<i>Recommended for mid-game colonies.</i>");

            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.MINIMAL", "Minimal");
            Strings.Add("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.MINIMAL_TOOLTIP",
                "Pipe animations update at 2 FPS (every 0.5 seconds).\n" +
                "When zoomed far out, updates reduce to 1 FPS.\n\n" +
                "<i>Recommended for large colonies with extensive pipe networks.</i>");
        }
    }
}
