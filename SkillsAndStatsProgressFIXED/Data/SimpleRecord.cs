using System;
using System.Text;
using Klei.AI;

namespace SkillsAndStatsProgressFIXED
{
    public class SimpleRecord
    {
        private int[] data = new int[12];

        public static SimpleRecord Empty = new SimpleRecord();

        public SimpleRecord()
        {
        }

        public SimpleRecord(int SkillExp, int Constr, int Digging, int Tinkering, int Athletics, int Learning, int Cooking, int Creativity, int Strength, int Kindness, int Farming, int Ranching)
        {
            this.SetValue(SkillExp, Constr, Digging, Tinkering, Athletics, Learning, Cooking, Creativity, Strength, Kindness, Farming, Ranching);
        }

        public SimpleRecord(MinionIdentity M)
        {
            this.SetValue(M);
        }

        public void SetValue(MinionIdentity m)
        {
            MinionResume component = m.GetComponent<MinionResume>();
            int num = (int)MinionResume.CalculatePreviousExperienceBar(component.TotalSkillPointsGained);
            int num2 = (int)MinionResume.CalculateNextExperienceBar(component.TotalSkillPointsGained);
            int value = (int)component.TotalExperienceGained - num;
            this[DataEnum.Skillexp] = value;
            AttributeLevels component2 = m.GetComponent<AttributeLevels>();
            foreach (object obj in Enum.GetValues(typeof(DataEnum)))
            {
                DataEnum dataEnum = (DataEnum)obj;
                if (dataEnum == DataEnum.Skillexp)
                    continue;
                this[dataEnum] = (int)component2.GetAttributeLevel(DataHelper.ConvertEnumToString(dataEnum)).experience;
            }
        }

        public void SetValue(int skillExp, int constr, int digging, int tinkering, int athletics, int learning, int cooking, int creativity, int strength, int kindness, int farming, int ranching)
        {
            this[DataEnum.Skillexp] = skillExp;
            this[DataEnum.Construction] = constr;
            this[DataEnum.Digging] = digging;
            this[DataEnum.Tinkering] = tinkering;
            this[DataEnum.Athletics] = athletics;
            this[DataEnum.Learning] = learning;
            this[DataEnum.Cooking] = cooking;
            this[DataEnum.Creativity] = creativity;
            this[DataEnum.Strength] = strength;
            this[DataEnum.Kindness] = kindness;
            this[DataEnum.Farming] = farming;
            this[DataEnum.Ranching] = ranching;
        }

        public void ClearValue()
        {
            for (int i = 0; i < this.data.Length; i++)
            {
                this.data[i] = 0;
            }
        }

        public int this[DataEnum D]
        {
            get { return this.data[(int)D]; }
            set { this.data[(int)D] = value; }
        }

        public int this[string S]
        {
            get { return this[DataHelper.ConvertStringToEnum(S)]; }
            set { this[DataHelper.ConvertStringToEnum(S)] = value; }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("SkillExp: {0}, ", this[DataEnum.Skillexp]));
            foreach (object obj in Enum.GetValues(typeof(DataEnum)))
            {
                DataEnum dataEnum = (DataEnum)obj;
                if (dataEnum == DataEnum.Skillexp)
                    continue;
                stringBuilder.Append(DataHelper.ConvertEnumToString(dataEnum) + " " + this[dataEnum].ToString() + " ");
            }
            return stringBuilder.ToString();
        }
    }
}
