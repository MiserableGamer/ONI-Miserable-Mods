using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace MorningExercise
{
    public sealed class MorningExercise : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(Options.MorningExerciseOptions));
            MorningExercisePatches.Options = POptions.ReadSettings<Options.MorningExerciseOptions>() ?? new Options.MorningExerciseOptions();

            // Set up green color scheme for the exercise schedule block
            MorningExercisePatches.ExerciseColor = ScriptableObject.CreateInstance<ColorStyleSetting>();
            MorningExercisePatches.ExerciseColor.activeColor = new Color(0.4f, 0.9f, 0.4f, 1.0f);
            MorningExercisePatches.ExerciseColor.inactiveColor = new Color(0.2f, 0.7f, 0.3f, 1.0f);
            MorningExercisePatches.ExerciseColor.disabledColor = new Color(0.4f, 0.4f, 0.416f, 1.0f);
            MorningExercisePatches.ExerciseColor.disabledActiveColor = new Color(0.6f, 0.588f, 0.625f, 1.0f);
            MorningExercisePatches.ExerciseColor.hoverColor = MorningExercisePatches.ExerciseColor.activeColor;
            MorningExercisePatches.ExerciseColor.disabledhoverColor = new Color(0.48f, 0.46f, 0.5f, 1.0f);

            UnityEngine.Debug.Log("[MorningExercise] Mod loaded successfully");
        }
    }
}

