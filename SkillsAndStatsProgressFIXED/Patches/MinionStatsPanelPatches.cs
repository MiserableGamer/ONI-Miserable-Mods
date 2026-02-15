using System.Collections.Generic;
using HarmonyLib;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace SkillsAndStatsProgressFIXED
{
    // Replaces MinionPersonalityPanel.RefreshAttributesPanel to show detailed XP progress
    [HarmonyPatch(typeof(MinionPersonalityPanel), "RefreshAttributesPanel")]
    public static class MinionStatsPanelPatches
    {
        // Format string for XP values — F0 for clean display, or full precision if opted in
        private static string XpFormat => Config.Cfg.HighPrecisionXP ? "F3" : "F0";

        private static string FormatXp(float value)
        {
            return value.ToString(XpFormat);
        }

        private static Dictionary<string, float> OldValue = new Dictionary<string, float>();
        public static Dictionary<NavType, int> TempTI = new Dictionary<NavType, int>();
        public static Dictionary<NavType, int> x = new Dictionary<NavType, int>();
        public static float OldTime = 0f;
        public static float OldRad = 0f;
        public static string LastChange;

        // RefreshAttributesPanel is private static, so Harmony gives us the parameters directly
        public static bool Prefix(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
        {
            if (!targetEntity.GetComponent<MinionIdentity>())
            {
                targetPanel.SetActive(false);
                return false;
            }

            MinionResume component2 = targetEntity.GetComponent<MinionResume>();
            MinionIdentity component3 = targetEntity.GetComponent<MinionIdentity>();
            int num = (int)MinionResume.CalculatePreviousExperienceBar(component2.TotalSkillPointsGained);
            int num2 = (int)MinionResume.CalculateNextExperienceBar(component2.TotalSkillPointsGained);
            int num3 = (int)component2.TotalExperienceGained - num;
            int num4 = num2 - num;
            float num5 = 100f * (float)num3 / (float)num4;
            string text = string.Format("Exp: <b>{0}</b>{1} <b>{2:F2}%</b> SP:{3}/{4}  ", new object[]
            {
                num3,
                Config.Cfg.ShowMaxExpForSkill ? ("/" + num4.ToString()) : "",
                num5,
                component2.AvailableSkillpoints,
                component2.TotalSkillPointsGained
            });

            string text2 = "";
            if (Config.Cfg.EnableComplexFeature && component3 != null)
            {
                SimpleRecord simpleRecord;
                if (MinionManager.LastUpdChange.TryGetValue(component3, out simpleRecord))
                {
                    int tempi = simpleRecord[DataEnum.Skillexp];
                    text2 = Print(tempi);
                }
                SimpleRecord simpleRecord2;
                if (MinionManager.Change.TryGetValue(component3, out simpleRecord2))
                {
                    text2 = "  D: (" + text2 + simpleRecord2[DataEnum.Skillexp].ToString() + ")";
                }
            }

            targetPanel.SetLabel("skillXpProgress", text + text2,
                string.Format("Skillpoint experience and available/max skillpoints." + UI.HORIZONTAL_BR_RULE + "Exp needed: <b>{0}</b>", num4 - num3));

            if (Config.Cfg.EnableComplexFeature && Config.Cfg.EnableAdditionalInfo)
            {
                string debugText = string.Format("T: {0}/{1}  N1: {2} N2: {3}", new object[]
                {
                    GameClockPatches.Time,
                    GameClockPatches.d,
                    MinionManager.L.Count,
                    MinionManager.Change.Count
                });
                targetPanel.SetLabel("complexDebugInfo", debugText,
                    "T: Current and subcurrent time. \n\n N1: Linked list count.\n N2: Dictionary count.");
            }

            if (Config.Cfg.ShowRadiationInfo && DlcManager.IsExpansion1Active())
            {
                targetPanel.SetLabel("radiationInfo", GetRadiationInfo(component2, component3), "Radiation details");
            }

            AttributeLevels component4 = targetEntity.GetComponent<AttributeLevels>();
            List<AttributeInstance> list = new List<AttributeInstance>(targetEntity.GetAttributes().AttributeTable);
            List<AttributeInstance> list2 = list.FindAll((AttributeInstance a) => a.Attribute.ShowInUI == Klei.AI.Attribute.Display.Skill);
            AttributeInstance spaceNavAttr = list.Find((AttributeInstance a) => a.Id == Db.Get().Attributes.SpaceNavigation.Id);
            if (spaceNavAttr != null)
            {
                list2.Add(spaceNavAttr);
            }

            if (list2.Count > 0)
            {
                foreach (AttributeInstance attributeInstance2 in list2)
                {
                    text = attributeInstance2.Id;
                    string s = text;
                    AttributeLevel attributeLevel = component4.GetAttributeLevel(text);
                    bool flag9 = false;
                    string str;
                    if (attributeLevel != null)
                    {
                        float experience = attributeLevel.experience;
                        float num6;
                        if (OldValue.TryGetValue(text, out num6))
                        {
                            if (num6 != experience)
                            {
                                flag9 = true;
                                if (!SpeedControlScreen.Instance.IsPaused)
                                {
                                    OldValue[text] = experience;
                                }
                            }
                        }
                        else
                        {
                            flag9 = true;
                            OldValue.Add(text, experience);
                        }
                        flag9 = flag9 && Config.Cfg.EnabledFirstFeature;
                    if (!flag9)
                    {
                        text = string.Format("/{0} <b>{1}</b>{2} <b>{3:F2}</b>%", new object[]
                        {
                            attributeInstance2.GetFormattedValue(),
                                FormatXp(Config.Cfg.ShowRequiredXp ? (attributeLevel.GetExperienceForNextLevel() - attributeLevel.experience) : attributeLevel.experience),
                                Config.Cfg.ShowMaxExpForStats ? ("/" + FormatXp(attributeLevel.GetExperienceForNextLevel())) : "",
                                attributeLevel.GetPercentComplete() * 100f
                            });
                            str = string.Format("Exp needed: <b>{0}</b>", FormatXp(attributeLevel.GetExperienceForNextLevel() - attributeLevel.experience));
                        }
                    else
                    {
                        text = string.Format("/{0} {1}{2} {3:F2}%", new object[]
                        {
                            attributeInstance2.GetFormattedValue(),
                                FormatXp(Config.Cfg.ShowRequiredXp ? (attributeLevel.GetExperienceForNextLevel() - attributeLevel.experience) : attributeLevel.experience),
                                Config.Cfg.ShowMaxExpForStats ? ("/" + FormatXp(attributeLevel.GetExperienceForNextLevel())) : "",
                                attributeLevel.GetPercentComplete() * 100f
                            });
                            str = string.Format("Exp needed: {0}", FormatXp(attributeLevel.GetExperienceForNextLevel() - attributeLevel.experience));
                        }
                    }
                    else
                    {
                        str = (text = "");
                    }

                    string text3;
                    if (Config.Cfg.ShrinkStatNameToXchar > 0)
                    {
                        text3 = attributeInstance2.Name.Substring(0,
                            (attributeInstance2.Name.Length > Config.Cfg.ShrinkStatNameToXchar)
                                ? Config.Cfg.ShrinkStatNameToXchar
                                : attributeInstance2.Name.Length);
                    }
                    else
                    {
                        text3 = attributeInstance2.Name;
                    }

                    text2 = "";
                    DataEnum trackedEnum;
                    if (Config.Cfg.EnableComplexFeature && component3 != null && DataHelper.TryConvertStringToEnum(s, out trackedEnum))
                    {
                        string text4 = MinionManager.GetLastAttribSum(component3)[trackedEnum].ToString();
                        SimpleRecord simpleRecord3;
                        string text5;
                        if (MinionManager.LastUpdChange.TryGetValue(component3, out simpleRecord3))
                        {
                            int tempi2 = simpleRecord3[trackedEnum];
                            text5 = Print(tempi2);
                        }
                        else
                        {
                            text5 = "0";
                        }
                        SimpleRecord simpleRecord4;
                        if (MinionManager.Change.TryGetValue(component3, out simpleRecord4))
                        {
                            text2 = string.Concat(new string[]
                            {
                                "  (",
                                text4,
                                "/",
                                text5,
                                simpleRecord4[trackedEnum].ToString(),
                                ")"
                            });
                        }
                    }

                    string labelText;
                    if (!flag9)
                    {
                        labelText = string.Format("  {0} {1} {2} <b>{3}</b>", new object[]
                        {
                            text3,
                            attributeLevel != null ? attributeLevel.GetLevel().ToString() : "",
                            text,
                            text2
                        });
                    }
                    else
                    {
                        labelText = string.Format("=><b>{0} {1} {2} {3}</b>", new object[]
                        {
                            text3,
                            attributeLevel != null ? attributeLevel.GetLevel().ToString() : "",
                            text,
                            text2
                        });
                    }

                    targetPanel.SetLabel("attr_" + attributeInstance2.Id, labelText,
                        attributeInstance2.GetAttributeValueTooltip() + UI.HORIZONTAL_BR_RULE + str);
                }
            }

            if (Config.Cfg.ShowActualSpeed)
            {
                Navigator component5 = component3.GetComponent<Navigator>();
                if (component5 != null)
                {
                    text2 = "Speed: <b>";
                    if (component5.transitionDriver != null && component5.transitionDriver.GetTransition != null)
                    {
                        Vector3 position = component3.GetComponent<Transform>().position;
                        int posX = 0;
                        int posY = 0;
                        Grid.PosToXY(position, out posX, out posY);
                        text2 = text2 + (((double)component5.transitionDriver.GetTransition.speed != 1.0 && component5.IsMoving())
                            ? string.Format("{0:f3} ", component5.transitionDriver.GetTransition.speed)
                            : "0.000")
                            + string.Format("</b> x:<b>{0}</b> y:<b> {1}</b> Cell: <b>{2}</b>", posX, posY, Grid.PosToCell(component5.gameObject));
                        if (Config.Cfg.AvgSpeedInterval >= 0f)
                        {
                            text2 += "\n" + AvgSpeed.GetAvgSpeed(component5);
                        }
                    }
                    targetPanel.SetLabel("speedInfo", text2, "Speed info.");
                }
            }

            if (Config.Cfg.ShowTravelPath)
            {
                Navigator component6 = component3.GetComponent<Navigator>();
                if (component6 != null)
                {
                    TravelInfo.GetTodayTravelInfo(component6.distanceTravelledByNavType, component3, TempTI);
                    SetTravelLabel(targetPanel, TempTI, "travelToday", "Distance traveled today: ");
                    SetTravelLabel(targetPanel, component6.distanceTravelledByNavType, "travelTotal", "Total traveled distance: ");
                    TravelInfo.GetTotal(x, false);
                    SetTravelLabel(targetPanel, x, "travelAllToday", "Total Distance today: ");
                    TravelInfo.GetTotal(x, true);
                    SetTravelLabel(targetPanel, x, "travelAllTotal", "Total Distance: ");
                }
            }

            targetPanel.Commit();
            return false;
        }

        // Converts TravelInfo.ShowTravelInfo from DetailsPanelDrawer to CollapsibleDetailContentPanel
        private static void SetTravelLabel(CollapsibleDetailContentPanel panel, Dictionary<NavType, int> travelData, string idPrefix, string header)
        {
            int total = 0;
            foreach (KeyValuePair<NavType, int> kvp in travelData)
            {
                total += kvp.Value;
            }
            string mainText = string.Format(header + " (<b>{0,8:N3}</b> Km)", (float)total / 1000f);
            string details = "";
            foreach (KeyValuePair<NavType, int> kvp2 in travelData)
            {
                if (kvp2.Value != 0)
                {
                    details += string.Format("\n{0}: <b>{1:N3}</b> Km", TravelInfo.NavTypeName[(int)kvp2.Key], (float)kvp2.Value / 1000f);
                }
            }
            panel.SetLabel(idPrefix, mainText, details);
        }

        private static string GetRadiationInfo(MinionResume mr, MinionIdentity m)
        {
            Amounts amounts = ModifiersExtensions.GetAmounts(mr.gameObject);
            float value = amounts.GetValue(Db.Get().Amounts.RadiationBalance.Id);
            int num = Grid.PosToCell(mr.gameObject);
            float num2;
            if (Grid.IsValidCell(num))
                num2 = Grid.Radiation[num];
            else
                num2 = 0f;

            float totalValue = Db.Get().Attributes.RadiationRecovery.Lookup(mr).GetTotalValue();
            float totalValue2 = Db.Get().Attributes.RadiationResistance.Lookup(mr).GetTotalValue();
            float time = GameClock.Instance.GetTime();
            float num3 = time - OldTime;

            string text;
            if (OldTime != 0f && num3 != 0f)
            {
                text = ((value - OldRad) / num3 * 600f).ToString("F2");
            }
            else if (LastChange != null)
            {
                text = LastChange;
            }
            else
            {
                text = "??";
            }

            LastChange = text;
            OldRad = value;
            OldTime = time;

            return string.Concat(new string[]
            {
                "Rad: <b>", value.ToString("F0"),
                "</b>  Ch:<b>", text,
                "</b>/cycle Rec: <b>", (totalValue * 600f).ToString("F0"),
                "</b>  Cur.Exp.: <b>", num2.ToString("F0"),
                "</b>/<b>", (num2 * (1f - totalValue2)).ToString("F0"),
                "</b> Res:<b>", (totalValue2 * 100f).ToString("F0"),
                " </b>%"
            });
        }

        private static string Print(int tempi)
        {
            if (tempi == 0)
                return "0/";
            char c = tempi < 0 ? '-' : '+';
            return c.ToString() + tempi.ToString() + "/";
        }
    }
}
