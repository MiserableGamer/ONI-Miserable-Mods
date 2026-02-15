using System.Collections.Generic;

namespace SkillsAndStatsProgressFIXED
{
    public static class DataHelper
    {
        private static Dictionary<DataEnum, string> dt = new Dictionary<DataEnum, string>
        {
            { DataEnum.Skillexp, "" },
            { DataEnum.Construction, "Construction" },
            { DataEnum.Digging, "Digging" },
            { DataEnum.Tinkering, "Machinery" },
            { DataEnum.Athletics, "Athletics" },
            { DataEnum.Learning, "Learning" },
            { DataEnum.Cooking, "Cooking" },
            { DataEnum.Creativity, "Art" },
            { DataEnum.Strength, "Strength" },
            { DataEnum.Kindness, "Caring" },
            { DataEnum.Farming, "Botanist" },
            { DataEnum.Ranching, "Ranching" }
        };

        private static Dictionary<string, DataEnum> dt2 = new Dictionary<string, DataEnum>
        {
            { "", DataEnum.Skillexp },
            { "Construction", DataEnum.Construction },
            { "Digging", DataEnum.Digging },
            { "Machinery", DataEnum.Tinkering },
            { "Athletics", DataEnum.Athletics },
            { "Learning", DataEnum.Learning },
            { "Cooking", DataEnum.Cooking },
            { "Art", DataEnum.Creativity },
            { "Strength", DataEnum.Strength },
            { "Caring", DataEnum.Kindness },
            { "Botanist", DataEnum.Farming },
            { "Ranching", DataEnum.Ranching }
        };

        public static string ConvertEnumToString(DataEnum E)
        {
            return dt[E];
        }

        public static DataEnum ConvertStringToEnum(string S)
        {
            return dt2[S];
        }

        public static bool TryConvertStringToEnum(string S, out DataEnum result)
        {
            return dt2.TryGetValue(S, out result);
        }
    }
}
