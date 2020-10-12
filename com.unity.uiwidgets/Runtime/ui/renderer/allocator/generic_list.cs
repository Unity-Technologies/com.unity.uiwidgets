using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.uiOld{
    public class uiList<T> : PoolObject {
        List<T> list;

        public override void setup() {
            base.setup();
            list = list ?? new List<T>(128);
        }

        public uiList() {
        }

        public List<T> data {
            get { return list; }
        }

        public void Add(T item) {
            list.Add(item);
        }

        public void AddRange(IList<T> items) {
            list.AddRange(items);
        }

        public void Clear() {
            list.Clear();
        }

        public override void clear() {
            //clear the list immediately to avoid potential memory leak
            //otherwise, we may clear it in Setup() for lazy update
            list.Clear();
        }

        public int Count {
            get { return list.Count; }
        }

        public void SetCapacity(int capacity) {
            list.Capacity = Math.Max(capacity, list.Capacity);
        }

        public T this[int index] {
            get { return list[index]; }
            set { list[index] = value; }
        }
    }
}