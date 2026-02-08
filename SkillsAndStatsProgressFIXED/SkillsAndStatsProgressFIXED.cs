using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

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

            if (!Init)
            {
                Init = true;
                Debug.Log("SkillsAndStatsProgressFIXED: OnLoad - Version:" + Config.Cfg.Ver.ToString() + string.Format(" @Build: {0})", typeof(SkillsAndStatsProgressFIXEDMod).Assembly.GetName().Version));
            }

            harmony.PatchAll();
        }

        private static bool Init;
    }
}
