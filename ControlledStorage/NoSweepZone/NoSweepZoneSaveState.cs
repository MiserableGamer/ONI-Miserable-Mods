using System.Collections;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

namespace ControlledStorage.NoSweepZone
{
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
