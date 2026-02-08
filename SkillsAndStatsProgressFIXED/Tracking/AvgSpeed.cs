using System.Collections.Generic;

namespace SkillsAndStatsProgressFIXED
{
    public class AvgSpeed
    {
        public static Navigator LastNavigator;
        public static LinkedList<AvgSpeedInfo> L = new LinkedList<AvgSpeedInfo>();
        public static Stack<LinkedListNode<AvgSpeedInfo>> Pool = new Stack<LinkedListNode<AvgSpeedInfo>>();

        public static string GetAvgSpeed(Navigator Nav)
        {
            if (AvgSpeed.LastNavigator != Nav)
            {
                while (L.Last != null)
                {
                    LinkedListNode<AvgSpeedInfo> last = L.Last;
                    L.RemoveLast();
                    RecycleNode(last);
                }
            }
            LastNavigator = Nav;
            int totalDistance = GetTotalDistance(Nav);
            float num;
            int num2;
            float andSet = GetAndSet(totalDistance, out num, out num2);
            string str = Config.Cfg.DebugInfo ? string.Format("List:{0}, Pool: {1}.\n", L.Count, Pool.Count) : "";
            return str + string.Format("Avg.Speed:<b>{0:f3}</b> tile/s Dist:<b>{1}</b> Last <b>{2:f0}</b> s.", andSet, num2, num);
        }

        private static float GetAndSet(int totalDistance, out float TimeInterval, out int D)
        {
            float time = GameClock.Instance.GetTime();
            if (L.First == null || L.First.Value.Time != time)
            {
                L.AddFirst(GetNode(new AvgSpeedInfo(time, totalDistance)));
            }
            float num = 0f;
            int num2 = 0;
            LinkedListNode<AvgSpeedInfo> linkedListNode = L.First;
            float time2 = linkedListNode.Value.Time;
            int distance = linkedListNode.Value.Distance;
            while (linkedListNode != null)
            {
                if (time2 - linkedListNode.Value.Time > Config.Cfg.AvgSpeedInterval + 1f)
                {
                    RemoveFrom(linkedListNode);
                    break;
                }
                num = time2 - linkedListNode.Value.Time;
                num2 = distance - linkedListNode.Value.Distance;
                linkedListNode = linkedListNode.Next;
            }
            TimeInterval = num;
            D = num2;
            return (num == 0f) ? 0f : ((float)num2 / num);
        }

        private static void RemoveFrom(LinkedListNode<AvgSpeedInfo> i)
        {
            LinkedListNode<AvgSpeedInfo> linkedListNode;
            for (linkedListNode = L.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
            {
                if (linkedListNode == i)
                    break;
            }
            if (linkedListNode == null)
            {
                Debug.Log("SkillsAndStatsProgressFIXED: Can not find LinkListNode in RemoveFrom!");
                return;
            }

            bool flag3 = false;
            while (!flag3)
            {
                LinkedListNode<AvgSpeedInfo> last = L.Last;
                if (last == linkedListNode)
                    flag3 = true;
                L.RemoveLast();
                RecycleNode(last);
            }
        }

        private static void RecycleNode(LinkedListNode<AvgSpeedInfo> R)
        {
            Pool.Push(R);
        }

        private static LinkedListNode<AvgSpeedInfo> GetNode(AvgSpeedInfo avgSpeedInfo)
        {
            if (Pool.Count > 0)
            {
                LinkedListNode<AvgSpeedInfo> linkedListNode = Pool.Pop();
                linkedListNode.Value = avgSpeedInfo;
                return linkedListNode;
            }
            return new LinkedListNode<AvgSpeedInfo>(avgSpeedInfo);
        }

        private static int GetTotalDistance(Navigator Nav)
        {
            int num = 0;
            foreach (KeyValuePair<NavType, int> keyValuePair in Nav.distanceTravelledByNavType)
            {
                num += keyValuePair.Value;
            }
            return num;
        }
    }
}
