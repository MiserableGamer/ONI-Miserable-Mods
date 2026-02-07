using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace ControlledStorage.NoSweepZone
{
    /// <summary>
    /// Cancels active fetch chores whose target pickupable is in the No Sweep Zone,
    /// and manages FetchableMonitor re-evaluation so zone items are removed from FetchManager
    /// (prevents dupe chore oscillation while sweepers remain unaffected).
    /// </summary>
    internal static class NoSweepZoneChoreInvalidation
    {
        // Tracks Pickupables currently known to be in a No Sweep Zone.
        // Used by OnCellChange to detect zone transitions and trigger FetchableMonitor re-evaluation.
        private static readonly HashSet<Pickupable> _inZonePickupables = new HashSet<Pickupable>();

        internal static bool IsInNoSweepZone(Pickupable pickupable) => _inZonePickupables.Contains(pickupable);

        internal static void ClearTracking() => _inZonePickupables.Clear();

        /// <summary>
        /// Called from OnCellChange when a Pickupable enters or leaves a No Sweep Zone.
        /// Updates tracking, triggers FetchableMonitor re-evaluation via TagsChanged,
        /// and cancels active dupe chores if the item entered the zone.
        /// </summary>
        internal static void UpdatePickupableZoneTracking(Pickupable pickupable, bool nowInZone)
        {
            if (pickupable == null) return;
            bool wasInZone = _inZonePickupables.Contains(pickupable);
            if (nowInZone == wasInZone) return;

            if (nowInZone)
            {
                _inZonePickupables.Add(pickupable);
                CancelDupeChoresForPickupable(pickupable);
            }
            else
            {
                _inZonePickupables.Remove(pickupable);
            }
            pickupable.Trigger((int)GameHashes.TagsChanged, null);
        }

        /// <summary>
        /// After zone cells are drawn or erased, scan the affected rectangle for Pickupables
        /// and update their zone tracking + trigger FetchableMonitor re-evaluation.
        /// </summary>
        internal static void RefreshFetchabilityInArea(int x0, int y0, int x1, int y1)
        {
            if (NoSweepZoneSaveState.Instance == null) return;
            var zone = NoSweepZoneSaveState.Instance.NoSweep;
            int width = x1 - x0 + 1;
            int height = y1 - y0 + 1;
            if (width <= 0 || height <= 0) return;

            GameScenePartitioner.Instance.ReadonlyVisitEntries<object>(
                x0, y0, width, height,
                GameScenePartitioner.Instance.pickupablesLayer,
                _refreshVisitor, null);
        }

        private static readonly System.Func<object, object, Util.IterationInstruction> _refreshVisitor = (obj, _) =>
        {
            if (obj is Pickupable pickup && pickup != null)
            {
                var zone = NoSweepZoneSaveState.Instance;
                if (zone != null)
                {
                    bool inZone = zone.NoSweep.ContainsCell(pickup.cachedCell);
                    UpdatePickupableZoneTracking(pickup, inZone);
                }
            }
            return Util.IterationInstruction.Continue;
        };

        internal static void InvalidateFetchChoresInZone()
        {
            if (NoSweepZoneSaveState.Instance == null) return;
            var zone = NoSweepZoneSaveState.Instance.NoSweep;

            // Dupes
            foreach (var minion in Components.LiveMinionIdentities.Items)
            {
                if (minion == null) continue;
                var driver = minion.GetComponent<ChoreDriver>();
                if (driver == null || !driver.HasChore()) continue;
                TryFailChoreIfTargetInZone(driver.GetCurrentChore(), zone);
            }

            // Sweepers
            var arms = Object.FindObjectsOfType<SolidTransferArm>();
            if (arms != null)
            {
                foreach (var arm in arms)
                {
                    if (arm == null) continue;
                    var driver = arm.GetComponent<ChoreDriver>();
                    if (driver == null || !driver.HasChore()) continue;
                    TryFailChoreIfTargetInZone(driver.GetCurrentChore(), zone);
                }
            }
        }

        internal static void CancelDupeChoresForPickupable(Pickupable target)
        {
            if (target == null) return;
            foreach (var minion in Components.LiveMinionIdentities.Items)
            {
                if (minion == null) continue;
                var driver = minion.GetComponent<ChoreDriver>();
                if (driver == null || !driver.HasChore()) continue;
                TryFailChoreIfTargetMatches(driver.GetCurrentChore(), target);
            }
        }

        private static void TryFailChoreIfTargetMatches(Chore chore, Pickupable target)
        {
            var pickup = GetChorePickupTarget(chore);
            if (pickup != null && pickup == target)
                chore.Fail("No Sweep Zone");
        }

        private static void TryFailChoreIfTargetInZone(Chore chore, NoSweepZoneSaveState.NoSweepState zone)
        {
            var pickup = GetChorePickupTarget(chore);
            if (pickup == null) return;

            int cachedCell = pickup.cachedCell;
            int posCell = Grid.PosToCell(pickup.transform.GetPosition());
            bool inZone = (Grid.IsValidCell(cachedCell) && zone.ContainsCell(cachedCell))
                || (Grid.IsValidCell(posCell) && zone.ContainsCell(posCell));
            if (!inZone) return;

            chore.Fail("No Sweep Zone");
        }

        private static Pickupable GetChorePickupTarget(Chore chore)
        {
            if (chore == null) return null;

            if (chore is FetchAreaChore fac)
            {
                if (!fac.IsFetching) return null;
                var targetGo = fac.GetFetchTarget;
                return targetGo != null ? targetGo.GetComponent<Pickupable>() : null;
            }
            if (chore is EatChore eatChore)
            {
                var smi = Traverse.Create(eatChore).Field("smi").GetValue<EatChore.StatesInstance>();
                if (smi?.sm == null) return null;
                var edibleGo = smi.sm.ediblesource.Get(smi);
                return edibleGo != null ? edibleGo.GetComponent<Pickupable>() : null;
            }
            if (chore is BingeEatChore bingeChore)
            {
                var smi = Traverse.Create(bingeChore).Field("smi").GetValue<BingeEatChore.StatesInstance>();
                if (smi?.sm == null) return null;
                var edibleGo = smi.sm.ediblesource.Get(smi);
                return edibleGo != null ? edibleGo.GetComponent<Pickupable>() : null;
            }
            return null;
        }
    }
    [SerializationConfig(MemberSerialization.OptIn)]
    internal class NoSweepZoneSaveState : KMonoBehaviour
    {
        [Serialize]
        public NoSweepState NoSweep = new NoSweepState();

        internal static NoSweepZoneSaveState Instance { get; private set; }

        internal NoSweepZoneSaveState()
        {
            Instance = this;
            NoSweepZoneOverlay.SetupOverlay();
        }

        protected override void OnCleanUp()
        {
            NoSweepZoneChoreInvalidation.ClearTracking();
            Instance = null;
            base.OnCleanUp();
        }

        [SerializationConfig(MemberSerialization.OptIn)]
        internal sealed class NoSweepState : IEnumerable<int>
        {
            [Serialize]
            private List<int> _cellList = new List<int>();

            private HashSet<int> _cellSet = new HashSet<int>();
            private readonly object _rebuildLock = new object();

            // Called from worker threads (SimDebugView overlay) — must be thread-safe
            internal bool ContainsCell(int cell)
            {
                RebuildSetIfNeeded();
                var snapshot = _cellSet;
                return snapshot != null && snapshot.Contains(cell);
            }

            internal void AddCell(int cell)
            {
                _cellList ??= new List<int>();
                _cellSet ??= new HashSet<int>();
                if (_cellSet.Add(cell))
                    _cellList.Add(cell);
            }

            internal void RemoveCell(int cell)
            {
                _cellSet?.Remove(cell);
                _cellList?.Remove(cell);
            }

            internal void Clear()
            {
                _cellSet?.Clear();
                _cellList?.Clear();
            }

            // Thread-safe rebuild: constructs a new HashSet atomically instead of
            // clearing and re-adding to a shared instance (which caused
            // IndexOutOfRangeException when multiple worker threads entered simultaneously)
            private void RebuildSetIfNeeded()
            {
                if (_cellList != null && (_cellSet == null || _cellSet.Count != _cellList.Count))
                {
                    lock (_rebuildLock)
                    {
                        if (_cellList != null && (_cellSet == null || _cellSet.Count != _cellList.Count))
                        {
                            _cellSet = new HashSet<int>(_cellList);
                        }
                    }
                }
            }

            public IEnumerator<int> GetEnumerator()
            {
                RebuildSetIfNeeded();
                var snapshot = _cellSet;
                return (snapshot ?? new HashSet<int>()).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
