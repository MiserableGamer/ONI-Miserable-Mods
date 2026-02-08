using HarmonyLib;
using Klei.AI;

namespace SkillsAndStatsProgressFIXED
{
    [HarmonyPatch(typeof(StandardWorker), "Work")]
    internal class WorkerWorkPatches
    {
        public static void Postfix(StandardWorker __instance, WorkerBase.WorkResult __result)
        {
            if (__result == WorkerBase.WorkResult.InProgress || __instance == null)
                return;

            MinionResume component = __instance.GetComponent<MinionResume>();
            if (component == null)
                return;

            WorkInfo workInfo;
            if (!WorkInfo.WInfo.TryGetValue(component, out workInfo))
                return;
            if (workInfo == null)
                return;

            string text = __result.ToString() + ":\n" + DisplayHelper.ShrinkTo(workInfo.Wrk.GetType().Name, 12) + "\nTime:";
            text = text + (GameClock.Instance.GetTime() - workInfo.Time).ToString("F2") + "s\n";

            Attribute workAttribute = workInfo.Wrk.GetWorkAttribute();
            string text2 = null;
            if (workInfo.Wrk.GetType() == typeof(Pickupable))
            {
                text2 = Db.Get().Attributes.Athletics.Id;
            }
            else if (workAttribute != null)
            {
                text2 = workAttribute.Id;
            }

            if (text2 != null)
            {
                AttributeLevels component2 = __instance.GetComponent<AttributeLevels>();
                if (component2 != null)
                {
                    AttributeLevel attributeLevel = component2.GetAttributeLevel(text2);
                    if (attributeLevel != null)
                    {
                        float num = attributeLevel.experience;
                        text = text + DisplayHelper.ShrinkTo(Db.Get().Attributes.Get(text2).ProfessionName, 3) + ":";
                        if (num >= workInfo.StartExp)
                        {
                            num -= workInfo.StartExp;
                            text = text + (num / attributeLevel.GetExperienceForNextLevel() * 100f).ToString("F3") + "%";
                        }
                        else
                        {
                            text += "Lvl UP";
                        }
                    }
                }
            }

            DisplayHelper.ShowText(text, __instance.gameObject, Config.Cfg.GetReport2Color(), Config.Cfg.WorkableInfoReport2Speed, Config.Cfg.WorkableInfoReport2Time);
        }
    }
}
