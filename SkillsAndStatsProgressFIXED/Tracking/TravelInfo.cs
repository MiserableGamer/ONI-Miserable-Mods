using System.Collections.Generic;

namespace SkillsAndStatsProgressFIXED
{
    public class TravelInfo
    {
        public static Dictionary<MinionIdentity, TravelInfo> TD = new Dictionary<MinionIdentity, TravelInfo>();
        public int Today = -1;
        public Dictionary<NavType, int> TodayTravelInfo = new Dictionary<NavType, int>();
        public static Dictionary<NavType, int> Temp = new Dictionary<NavType, int>();

        public static string[] NavTypeName = new string[]
        {
            "Floor",
            "Left wall",
            "Right wall",
            "Ceiling",
            "Ladder",
            "Hover",
            "Swim",
            "Pole",
            "Tube",
            "Solid",
            ""
        };

        public static void UpdateTodayTravelInfo(MinionIdentity M)
        {
            if (!Config.Cfg.ShowTravelPath)
                return;

            TravelInfo travelInfo;
            if (!TD.TryGetValue(M, out travelInfo))
            {
                travelInfo = new TravelInfo();
                TD[M] = travelInfo;
            }
            if (GameClock.Instance.GetCycle() != travelInfo.Today)
            {
                travelInfo.Today = GameClock.Instance.GetCycle();
                travelInfo.TodayTravelInfo.Clear();
                Navigator component = M.GetComponent<Navigator>();
                if (component != null)
                {
                    foreach (KeyValuePair<NavType, int> keyValuePair in component.distanceTravelledByNavType)
                    {
                        travelInfo.TodayTravelInfo.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
            }
        }

        public static void PrintDictionary(Dictionary<NavType, int> Nav)
        {
            if (Nav == null)
            {
                Debug.Log("Dictionary is null.");
                return;
            }
            Debug.Log(string.Format("Dictionary count: {0}.", Nav.Count));
            foreach (KeyValuePair<NavType, int> keyValuePair in Nav)
            {
                Debug.Log(string.Format("Pairs: {0}, {1}.", keyValuePair.Key, keyValuePair.Value));
            }
        }

        public static TravelInfo GetTodayTravelInfo(MinionIdentity M)
        {
            TravelInfo travelInfo;
            if (TD.TryGetValue(M, out travelInfo))
                return travelInfo;
            return null;
        }

        public static void GetTodayTravelInfo(Dictionary<NavType, int> Source, MinionIdentity M, Dictionary<NavType, int> Dest)
        {
            Dest.Clear();
            TravelInfo todayTravelInfo = GetTodayTravelInfo(M);
            if (Dest == null || todayTravelInfo == null || todayTravelInfo.Today != GameClock.Instance.GetCycle())
                return;

            foreach (KeyValuePair<NavType, int> keyValuePair in Source)
            {
                int num;
                todayTravelInfo.TodayTravelInfo.TryGetValue(keyValuePair.Key, out num);
                num = keyValuePair.Value - num;
                Dest[keyValuePair.Key] = num;
            }
        }

        internal static void ShowTravelInfo(DetailsPanelDrawer a, Dictionary<NavType, int> tempTI, string v)
        {
            int num = 0;
            foreach (KeyValuePair<NavType, int> keyValuePair in tempTI)
            {
                num += keyValuePair.Value;
            }
            a.NewLabel(string.Format(v + " (<b>{0,8:N3}</b> Km)", (float)num / 1000f));
            foreach (KeyValuePair<NavType, int> keyValuePair2 in tempTI)
            {
                if (keyValuePair2.Value != 0)
                {
                    a.NewLabel(string.Format("{0,15}: <b>{1,8:N3}</b> Km.", NavTypeName[(int)keyValuePair2.Key].ToString(), (float)keyValuePair2.Value / 1000f));
                }
            }
        }

        internal static void GetTotal(Dictionary<NavType, int> tempTI, bool GetTotalTraveledInfo = false)
        {
            if (tempTI == null)
                return;

            tempTI.Clear();
            foreach (object obj in Components.MinionIdentities)
            {
                MinionIdentity minionIdentity = (MinionIdentity)obj;
                Navigator component = minionIdentity.GetComponent<Navigator>();
                if (component != null)
                {
                    if (GetTotalTraveledInfo)
                    {
                        foreach (KeyValuePair<NavType, int> keyValuePair in component.distanceTravelledByNavType)
                        {
                            int num;
                            if (tempTI.TryGetValue(keyValuePair.Key, out num))
                                tempTI[keyValuePair.Key] = num + keyValuePair.Value;
                            else
                                tempTI[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }
                    else
                    {
                        Temp.Clear();
                        UpdateTodayTravelInfo(minionIdentity);
                        GetTodayTravelInfo(component.distanceTravelledByNavType, minionIdentity, Temp);
                        TravelInfo travelInfo;
                        if (TD.TryGetValue(minionIdentity, out travelInfo))
                        {
                            Dictionary<NavType, int> todayTravelInfo = travelInfo.TodayTravelInfo;
                            foreach (KeyValuePair<NavType, int> keyValuePair2 in todayTravelInfo)
                            {
                                int num2;
                                if (component.distanceTravelledByNavType.TryGetValue(keyValuePair2.Key, out num2))
                                {
                                    num2 -= keyValuePair2.Value;
                                    int num3;
                                    if (tempTI.TryGetValue(keyValuePair2.Key, out num3))
                                        tempTI[keyValuePair2.Key] = num3 + num2;
                                    else
                                        tempTI[keyValuePair2.Key] = num2;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
