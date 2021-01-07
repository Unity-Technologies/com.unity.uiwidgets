using System.Collections;
using System.Collections.Generic;

namespace Unity.UIWidgets.foundation {
    public class ObserverList<T> : ICollection<T> {
        readonly List<T> _list = new List<T>();
        bool _isDirty = false;
        HashSet<T> _set = null;

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator() {
            return _list.GetEnumerator();
        }

        public void Add(T item) {
            _isDirty = true;
            _list.Add(item);
        }

        public bool Remove(T item) {
            _isDirty = true;
            _set?.Clear();
            return _list.Remove(item);
        }

        public bool Contains(T item) {
            if (_list.Count < 3) {
                return _list.Contains(item);
            }

            if (_isDirty) {
                if (_set == null) {
                    _set = new HashSet<T>(_list);
                }
                else {
                    _set.Clear();
                    _set.UnionWith(_list);
                }
                _isDirty = false;
            }

            return _set.Contains(item);
        }

        public void Clear() {
            _isDirty = true;
            _list.Clear();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _list.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }
    }
}