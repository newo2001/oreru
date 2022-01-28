using System.Collections;
using System.Collections.Generic;

namespace Oreru.Collections {
    public class SortedList<T> : ICollection<T>, IReadOnlyList<T> {
        private readonly List<T> _list;

        private readonly IComparer<T> _comparer;
        private int _count;
        private int _count1;
        
        public SortedList(IComparer<T> comparer) {
            _comparer = _comparer;
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item) {
            var index = ~_list.BinarySearch(item, _comparer);
        }

        public void Clear() => _list.Clear();

        public bool Contains(T item) {
            throw new System.NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new System.NotImplementedException();
        }

        public bool Remove(T item) {
            throw new System.NotImplementedException();
        }

        int ICollection<T>.Count => _count;

        public bool IsReadOnly { get; }

        int IReadOnlyCollection<T>.Count => _count1;

        public T this[int index] => throw new System.NotImplementedException();
    }
}