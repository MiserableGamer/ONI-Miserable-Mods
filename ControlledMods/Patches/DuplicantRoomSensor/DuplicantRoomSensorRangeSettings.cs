using KSerialization;
using System.Collections.Generic;
using UnityEngine;

namespace ControlledMods.Patches.DuplicantRoomSensor
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class DuplicantRoomSensorRangeSettings : KMonoBehaviour, ISaveLoadable
    {
        public const int MinRange = 1;
        public const int MaxRange = 64;

        [Serialize]
        public bool EnableRangeLimit;

        [Serialize]
        public int RangeCells = 5;

        private static readonly EventSystem.IntraObjectHandler<DuplicantRoomSensorRangeSettings> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<DuplicantRoomSensorRangeSettings>((cmp, data) => cmp.OnCopySettings(data));

        [System.NonSerialized]
        public int CachedOriginCell = Grid.InvalidCell;

        [System.NonSerialized]
        public int CachedRange = -1;

        [System.NonSerialized]
        public CavityInfo CachedCavity;

        [System.NonSerialized]
        public int CachedMinX = int.MinValue;

        [System.NonSerialized]
        public int CachedMaxX = int.MinValue;

        [System.NonSerialized]
        public int CachedMinY = int.MinValue;

        [System.NonSerialized]
        public int CachedMaxY = int.MinValue;

        [System.NonSerialized]
        public readonly HashSet<int> CachedReachableCells = new HashSet<int>();

        [System.NonSerialized]
        public float LastReachableRebuildTime = -9999f;

        [System.NonSerialized]
        public bool LastShowRangeEnabled = false;

        [System.NonSerialized]
        public int LastShowRangeRange = int.MinValue;

        [System.NonSerialized]
        public int LastShowRangeOriginCell = Grid.InvalidCell;

        [System.NonSerialized]
        public int LastShowRangeReachableCount = -1;

        [System.NonSerialized]
        public int LastShowRangeReachableXor = int.MinValue;

        [System.NonSerialized]
        public long LastShowRangeReachableSum = long.MinValue;

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        public int GetClampedRange()
        {
            if (RangeCells < MinRange)
                return MinRange;
            if (RangeCells > MaxRange)
                return MaxRange;
            return RangeCells;
        }

        private void OnCopySettings(object data)
        {
            var srcGo = data as GameObject;
            if (srcGo == null)
                return;

            var src = srcGo.GetComponent<DuplicantRoomSensorRangeSettings>();
            if (src == null)
                return;

            EnableRangeLimit = src.EnableRangeLimit;
            RangeCells = src.GetClampedRange();

            // Force runtime caches/visualizer state to refresh on the copied target.
            CachedOriginCell = Grid.InvalidCell;
            CachedRange = -1;
            CachedCavity = null;
            CachedMinX = int.MinValue;
            CachedMaxX = int.MinValue;
            CachedMinY = int.MinValue;
            CachedMaxY = int.MinValue;
            CachedReachableCells.Clear();
            LastReachableRebuildTime = -9999f;
            LastShowRangeEnabled = false;
            LastShowRangeRange = int.MinValue;
            LastShowRangeOriginCell = Grid.InvalidCell;
            LastShowRangeReachableCount = -1;
            LastShowRangeReachableXor = int.MinValue;
            LastShowRangeReachableSum = long.MinValue;
        }
    }
}
