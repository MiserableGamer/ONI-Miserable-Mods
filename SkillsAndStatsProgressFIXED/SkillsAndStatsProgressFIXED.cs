using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace SkillsAndStatsProgressFIXED
{
    public sealed class SkillsAndStatsProgressFIXEDMod : UserMod2
    {
        public static SkillsAndStatsProgressFIXEDMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            ConfigMigrationHelper.Migrate("SkillsAndStatsProgressFIXED.dll", "SkillsAndStatsProgressFIXED");
            new POptions().RegisterOptions(this, typeof(Config));

            Debug.Log("SkillsAndStatsProgressFIXED: OnLoad - " +
                string.Format("@Build: {0}", typeof(SkillsAndStatsProgressFIXEDMod).Assembly.GetName().Version));

            harmony.PatchAll();
        }
    }
}
