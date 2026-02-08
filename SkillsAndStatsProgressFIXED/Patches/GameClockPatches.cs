using HarmonyLib;
using UnityEngine;

namespace SkillsAndStatsProgressFIXED
{
    [HarmonyPatch(typeof(GameClock), "Render1000ms")]
    public static class GameClockPatches
    {
        public static float d;
        public static int Time;

        public static void Postfix(float dt)
        {
            if (!Config.Cfg.EnableComplexFeature)
                return;
            if (SpeedControlScreen.Instance.IsPaused)
                return;

            d += dt * UnityEngine.Time.timeScale;
            if (d >= (float)Config.Cfg.GetEveryXSecond)
            {
                Time++;
                d -= (float)Config.Cfg.GetEveryXSecond;
                foreach (object obj in Components.LiveMinionIdentities)
                {
                    MinionIdentity minionIdentity = (MinionIdentity)obj;
                    SimpleRecord simpleRecord;
                    if (MinionManager.LastUpdChange.TryGetValue(minionIdentity, out simpleRecord))
                    {
                        simpleRecord.ClearValue();
                    }
                    MinionManager.AddData(minionIdentity, new SimpleRecord(minionIdentity), Time);
                    TravelInfo.UpdateTodayTravelInfo(minionIdentity);
                }
                int num = Config.Cfg.IntervalSecond / Config.Cfg.GetEveryXSecond;
                int time = Time - num;
                MinionManager.RemoveDataOlderThen(time);
            }
        }
    }
}
