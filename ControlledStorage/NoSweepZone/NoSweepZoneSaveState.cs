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

            internal bool ContainsCell(int cell)
            {
                RebuildSetIfNeeded();
                return _cellSet != null && _cellSet.Contains(cell);
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

            private void RebuildSetIfNeeded()
            {
                if (_cellSet == null) _cellSet = new HashSet<int>();
                if (_cellList != null && _cellSet.Count != _cellList.Count)
                {
                    _cellSet.Clear();
                    foreach (var c in _cellList) _cellSet.Add(c);
                }
            }

            public IEnumerator<int> GetEnumerator()
            {
                RebuildSetIfNeeded();
                return (_cellSet ?? new HashSet<int>()).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
