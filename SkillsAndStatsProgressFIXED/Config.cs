using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace SkillsAndStatsProgressFIXED
{
    public class Config
    {
        public int Ver { get; set; } = 1;
        public bool EnabledFirstFeature { get; set; } = true;
        public bool EnableComplexFeature { get; set; } = false;
        public bool EnableAdditionalInfo { get; set; } = false;
        public int IntervalSecond { get; set; } = 600;
        public int GetEveryXSecond { get; set; } = 5;
        public int ShrinkStatNameToXchar { get; set; } = 0;
        public bool ShowMaxExpForSkill { get; set; } = true;
        public bool ShowMaxExpForStats { get; set; } = true;
        public bool AlterSortOrder { get; set; } = false;
        public bool ShowTravelPath { get; set; } = true;
        public bool ShowActualSpeed { get; set; } = true;
        public float AvgSpeedInterval { get; set; } = 30f;
        public bool DebugInfo { get; set; } = false;
        public bool ShowRequiredXp { get; set; } = true;
        public bool ShowWorkableInfo { get; set; } = false;
        public bool ShowWorkableOnlyForSelectedDuplicant { get; set; } = true;
        public Color WorkableInfoReport1Color { get; set; } = Color.green;
        public Color WorkableInfoReport2Color { get; set; } = Color.cyan;
        public bool WorkableShowOnlyResultReport { get; set; } = false;
        public float WorkableInfoReport1Speed { get; set; } = 0.1f;
        public float WorkableInfoReport2Speed { get; set; } = 0.1f;
        public float WorkableInfoReport1Time { get; set; } = 10f;
        public float WorkableInfoReport2Time { get; set; } = 10f;
        public float WorkableReportFontSize { get; set; } = 30f;
        public bool DontMessWithMyMods { get; set; } = false;
        public bool ShowRadiationInfo { get; set; } = true;

        [NonSerialized]
        private static Config cfg;

        public static Config Cfg
        {
            get
            {
                if (cfg != null)
                    return cfg;

                if (!File.Exists(DataHelper.GetConfigFileName()))
                {
                    Debug.Log("Config file not found (" + DataHelper.GetConfigFileName() + "). Creating new one with default values.");
                    cfg = new Config();
                    try
                    {
                        using (StreamWriter streamWriter = new StreamWriter(DataHelper.GetConfigFileName()))
                        {
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
                            xmlSerializer.Serialize(streamWriter, cfg);
                            streamWriter.Flush();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Something goes wrong, can not save config (" + DataHelper.GetConfigFileName() + "), Exception: " + ex.ToString());
                    }
                    return cfg;
                }

                try
                {
                    using (FileStream fileStream = File.OpenRead(DataHelper.GetConfigFileName()))
                    {
                        XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(Config));
                        cfg = (xmlSerializer2.Deserialize(fileStream) as Config);
                    }
                }
                catch (Exception ex2)
                {
                    Debug.Log("Something goes wrong, can not load config (" + DataHelper.GetConfigFileName() + "), Exception: " + ex2.ToString());
                    cfg = new Config();
                }
                return cfg;
            }
        }
    }
}
