using System;
using System.Collections.Generic;

namespace SkillsAndStatsProgressFIXED
{
    public static class MinionManager
    {
        public static Dictionary<MinionIdentity, SimpleRecord> LastValue = new Dictionary<MinionIdentity, SimpleRecord>();
        public static Dictionary<MinionIdentity, SimpleRecord> Change = new Dictionary<MinionIdentity, SimpleRecord>();
        public static Dictionary<MinionIdentity, SimpleRecord> LastUpdChange = new Dictionary<MinionIdentity, SimpleRecord>();
        public static LinkedList<ComplexRecord> L = new LinkedList<ComplexRecord>();

        public static void AddData(MinionIdentity M, SimpleRecord S, int Time)
        {
            SimpleRecord simpleRecord = new SimpleRecord();
            SimpleRecord simpleRecord2;
            if (LastValue.TryGetValue(M, out simpleRecord2))
            {
                SimpleRecord simpleRecord3 = Change[M];
                SimpleRecord simpleRecord4 = LastUpdChange[M];
                foreach (object obj in Enum.GetValues(typeof(DataEnum)))
                {
                    DataEnum dataEnum = (DataEnum)obj;
                    if (simpleRecord2[dataEnum] != S[dataEnum])
                    {
                        int num = S[dataEnum] - simpleRecord2[dataEnum];
                        if (num < 0)
                            num = 0;
                        simpleRecord3[dataEnum] += num;
                        simpleRecord4[dataEnum] += num;
                        simpleRecord[dataEnum] = num;
                        simpleRecord2[dataEnum] = S[dataEnum];
                    }
                }
                ComplexRecord value = ComplexRecord.Create(simpleRecord, M, Time);
                L.AddFirst(value);
            }
            else
            {
                LastValue.Add(M, S);
                Change.Add(M, new SimpleRecord());
                LastUpdChange.Add(M, new SimpleRecord());
            }
        }

        public static SimpleRecord GetLastAttribSum(MinionIdentity M)
        {
            foreach (ComplexRecord complexRecord in L)
            {
                if (M == complexRecord.Minion)
                    return complexRecord.Delta;
            }
            return SimpleRecord.Empty;
        }

        public static void RemoveDataOlderThen(int Time)
        {
            while (L.Last != null && L.Last.Value.Time <= Time)
            {
                ComplexRecord value = L.Last.Value;
                L.RemoveLast();

                SimpleRecord simpleRecord;
                SimpleRecord simpleRecord2;
                if (Change.TryGetValue(value.Minion, out simpleRecord) &&
                    LastUpdChange.TryGetValue(value.Minion, out simpleRecord2))
                {
                    foreach (object obj in Enum.GetValues(typeof(DataEnum)))
                    {
                        DataEnum dataEnum = (DataEnum)obj;
                        simpleRecord[dataEnum] -= value.Delta[dataEnum];
                        simpleRecord2[dataEnum] -= value.Delta[dataEnum];
                    }
                }
                ComplexRecord.Recycle(value);
            }
        }
    }
}
