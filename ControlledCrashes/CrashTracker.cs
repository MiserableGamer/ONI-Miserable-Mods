using System;
using System.Collections.Generic;
using UnityEngine;

namespace ControlledCrashes
{
    public static class CrashTracker
    {
        public static int IncrementCrash(string entityKey)
        {
            if (crashCounts.Count >= 1000 && !crashCounts.ContainsKey(entityKey))
            {
                crashCounts.Clear();
                Debug.LogWarning("[ControlledCrashes] Crash tracker cleared - reached 1000 entries");
            }

            if (!crashCounts.ContainsKey(entityKey))
            {
                crashCounts[entityKey] = 0;
            }

            crashCounts[entityKey]++;
            return crashCounts[entityKey];
        }

        public static string GetEntityInfo(GameObject obj)
        {
            if (obj == null)
            {
                return "null";
            }

            string name = obj.name;
            int instanceID = obj.GetInstanceID();
            KPrefabID component = obj.GetComponent<KPrefabID>();

            if (component != null)
            {
                return string.Format("{0} (ID: {1}, Unity: {2})", name, component.InstanceID, instanceID);
            }
            else
            {
                return string.Format("{0} (Unity ID: {1})", name, instanceID);
            }
        }

        public static string GetCellInfo(int cell)
        {
            if (!Grid.IsValidCell(cell))
            {
                return string.Format("Invalid Cell: {0}", cell);
            }

            int x;
            int y;
            Grid.CellToXY(cell, out x, out y);
            return string.Format("Cell {0} (X:{1}, Y:{2})", cell, x, y);
        }

        public static string GetTimestamp()
        {
            return System.DateTime.Now.ToString("HH:mm:ss");
        }

        private static Dictionary<string, int> crashCounts = new Dictionary<string, int>();
        private const int MAX_CRASH_ENTRIES = 1000;
    }
}
