using HarmonyLib;
using Klei.AI;

namespace SkillsAndStatsProgressFIXED
{
    [HarmonyPatch(typeof(Workable), "StartWork")]
    internal class WorkableStartPatches
    {
        public static string GetBool(bool b)
        {
            return b ? "*" : "-";
        }

        public static void Postfix(Workable __instance)
        {
            if (__instance.worker == null)
                return;

            float num = (__instance.GetEfficiencyMultiplier(__instance.worker) - 1f) * 100f;
            bool value = __instance.lightEfficiencyBonus;
            string text = string.Concat(new string[]
            {
                DisplayHelper.ShrinkTo(__instance.GetType().Name, 12),
                ":\nEff:",
                (num < 0f) ? "" : "+",
                num.ToString("F0"),
                "% ",
                GetBool(__instance.currentlyLit),
                "/",
                GetBool(value),
                "\n"
            });

            Attribute workAttribute = __instance.GetWorkAttribute();
            string AtId = null;
            if (__instance.GetType() == typeof(Pickupable))
            {
                AtId = Db.Get().Attributes.Athletics.Id;
            }
            else if (workAttribute != null)
            {
                AtId = workAttribute.Id;
            }

            AttributeLevels attributeLevels = null;
            MinionResume minionResume = (__instance.worker != null) ? __instance.worker.GetComponent<MinionResume>() : null;
            if (minionResume == null)
                return;

            if (AtId != null)
            {
                text = text + Db.Get().Attributes.Get(AtId).Name + ":";
                attributeLevels = __instance.worker.GetComponent<AttributeLevels>();
                if (attributeLevels != null)
                {
                    AttributeInstance attributeInstance = ModifiersExtensions.GetAttributes(minionResume).AttributeTable.Find((AttributeInstance ins) => ins.Id == AtId);
                    if (attributeInstance != null)
                    {
                        AttributeLevel attributeLevel = attributeLevels.GetAttributeLevel(AtId);
                        text = text + ((attributeLevel != null) ? attributeLevel.level.ToString() : "??") + "/" + attributeInstance.GetTotalValue().ToString();
                    }
                }
            }

            WorkInfo workInfo;
            if (!WorkInfo.WInfo.TryGetValue(minionResume, out workInfo))
            {
                workInfo = new WorkInfo();
            }
            workInfo.Time = GameClock.Instance.GetTime();
            workInfo.StartExp = ((attributeLevels == null) ? 0f : ((attributeLevels.GetAttributeLevel(AtId) != null) ? attributeLevels.GetAttributeLevel(AtId).experience : 0f));
            workInfo.Wrk = __instance;
            WorkInfo.WInfo[minionResume] = workInfo;

            if (!Config.Cfg.WorkableShowOnlyResultReport)
            {
                DisplayHelper.ShowText(text, __instance.worker.gameObject, Config.Cfg.WorkableInfoReport1Color, Config.Cfg.WorkableInfoReport1Speed, Config.Cfg.WorkableInfoReport1Time);
            }
        }
    }
}
