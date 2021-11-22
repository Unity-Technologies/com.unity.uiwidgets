using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unity.UIWidgets.external {
    public class SplayTree<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey> {
        SplayTreeNode root;
        int count;
        int version = 0;

        public void Add(TKey key, TValue value) {
            Set(key, value, throwOnExisting: true);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            Set(item.Key, item.Value, throwOnExisting: true);
        }
        
        public void AddAll(IEnumerable<TKey> list) {
            foreach (var key in list) {
                Add(new KeyValuePair<TKey, TValue>(key, default));
            }
        }

        void Set(TKey key, TValue value, bool throwOnExisting) {
            if (count == 0) {
                version++;
                root = new SplayTreeNode(key, value);
                count = 1;
                return;
            }

            Splay(key);

            var c = key.CompareTo(root.Key);
            if (c == 0) {
                if (throwOnExisting) {
                    throw new ArgumentException("An item with the same key already exists in the tree.");
                }

                version++;
                root.Value = value;
                return;
            }

            var n = new SplayTreeNode(key, value);
            if (c < 0) {
                n.LeftChild = root.LeftChild;
                n.RightChild = root;
                root.LeftChild = null;
            }
            else {
                n.RightChild = root.RightChild;
                n.LeftChild = root;
                root.RightChild = null;
            }

            root = n;
            count++;
            Splay(key);
            version++;
        }

        public void Clear() {
            root = null;
            count = 0;
            version++;
        }

        public bool ContainsKey(TKey key) {
            if (count == 0) {
                return false;
            }

            Splay(key);

            return key.CompareTo(root.Key) == 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            if (count == 0) {
                return false;
            }

            Splay(item.Key);

            return item.Key.CompareTo(root.Key) == 0 &&
                   (ReferenceEquals(root.Value, item.Value) ||
                    (!ReferenceEquals(item.Value, null) && item.Value.Equals(root.Value)));
        }

        public KeyValuePair<TKey, TValue>? First() {
            SplayTreeNode t = root;
            if (t == null) {
                return null;
            }

            while (t.LeftChild != null) {
                t = t.LeftChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        public KeyValuePair<TKey, TValue> FirstOrDefault() {
            SplayTreeNode t = root;
            if (t == null) {
                return new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
            }

            while (t.LeftChild != null) {
                t = t.LeftChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }
        
        public TKey lastKeyBefore(TKey key) {
            if (key == null) throw new Exception("should input null");
            if (root == null) throw new Exception("root is null");
            int comp = Splay(key);
            if (comp > 0) return root.Key;
            SplayTreeNode node = root.LeftChild;
            if (node == null) throw new Exception("does not exist");
            while (node.RightChild != null) {
                node = node.RightChild;
            }
            return node.Key;
        }
        
        public TKey firstKey() {
            if (root == null) return default; // TODO: this is suppose to be null
            var first = First().Value;
            return first.Key;
        }
        
        public TKey lastKey() {
            if (root == null) return default; // TODO: this is suppose to be null
            var last = Last().Value;
            return last.Key;
        }
        
        public TKey firstKeyAfter(TKey key) {
            if (key == null) throw new Exception("should input null");
            if (root == null) throw new Exception("root is null");
            int comp = Splay(key);
            if (comp < 0) return root.Key;
            SplayTreeNode node = root.LeftChild;
            if (node == null)  throw new Exception("does not exist");
            while (node.LeftChild != null) {
                node = node.LeftChild;
            }
            return node.Key;
        }

        public KeyValuePair<TKey, TValue>? Last() {
            SplayTreeNode t = root;
            if (t == null) {
                return null;
            }

            while (t.RightChild != null) {
                t = t.RightChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        public KeyValuePair<TKey, TValue> LastOrDefault() {
            SplayTreeNode t = root;
            if (t == null) {
                return new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
            }

            while (t.RightChild != null) {
                t = t.RightChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        int Splay(TKey key) {
            SplayTreeNode l, r, t, y, header;
            l = r = header = new SplayTreeNode(default(TKey), default(TValue));
            t = root;
            int c;
            while (true) {
                c = key.CompareTo(t.Key);
                if (c < 0) {
                    if (t.LeftChild == null) {
                        break;
                    }

                    if (key.CompareTo(t.LeftChild.Key) < 0) {
                        y = t.LeftChild;
                        t.LeftChild = y.RightChild;
                        y.RightChild = t;
                        t = y;
                        if (t.LeftChild == null) {
                            break;
                        }
                    }

                    r.LeftChild = t;
                    r = t;
                    t = t.LeftChild;
                }
                else if (c > 0) {
                    if (t.RightChild == null) {
                        break;
                    }

                    if (key.CompareTo(t.RightChild.Key) > 0) {
                        y = t.RightChild;
                        t.RightChild = y.LeftChild;
                        y.LeftChild = t;
                        t = y;
                        if (t.RightChild == null) {
                            break;
                        }
                    }

                    l.RightChild = t;
                    l = t;
                    t = t.RightChild;
                }
                else {
                    break;
                }
            }

            l.RightChild = t.LeftChild;
            r.LeftChild = t.RightChild;
            t.LeftChild = header.RightChild;
            t.RightChild = header.LeftChild;
            root = t;
            return c;
        }

        public bool Remove(TKey key) {
            if (count == 0) {
                return false;
            }

            Splay(key);

            if (key.CompareTo(root.Key) != 0) {
                return false;
            }

            if (root.LeftChild == null) {
                root = root.RightChild;
            }
            else {
                var swap = root.RightChild;
                root = root.LeftChild;
                Splay(key);
                root.RightChild = swap;
            }

            version++;
            count--;
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            if (count == 0) {
                value = default(TValue);
                return false;
            }

            Splay(key);
            if (key.CompareTo(root.Key) != 0) {
                value = default(TValue);
                return false;
            }

            value = root.Value;
            return true;
        }

        public TValue this[TKey key] {
            get {
                if (count == 0) {
                    throw new KeyNotFoundException("The key was not found in the tree.");
                }

                Splay(key);
                if (key.CompareTo(root.Key) != 0) {
                    throw new KeyNotFoundException("The key was not found in the tree.");
                }

                return root.Value;
            }

            set { Set(key, value, throwOnExisting: false); }
        }

        public int Count {
            get { return count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (count == 0) {
                return false;
            }

            Splay(item.Key);

            if (item.Key.CompareTo(root.Key) == 0 && (ReferenceEquals(root.Value, item.Value) ||
                                                           (!ReferenceEquals(item.Value, null) &&
                                                            item.Value.Equals(root.Value)))) {
                return false;
            }

            if (root.LeftChild == null) {
                root = root.RightChild;
            }
            else {
                var swap = root.RightChild;
                root = root.LeftChild;
                Splay(item.Key);
                root.RightChild = swap;
            }

            version++;
            count--;
            return true;
        }

        public void Trim(int depth) {
            if (depth < 0) {
                throw new ArgumentOutOfRangeException("depth", "The trim depth must not be negative.");
            }

            if (count == 0) {
                return;
            }

            if (depth == 0) {
                Clear();
            }
            else {
                var prevCount = count;
                count = Trim(root, depth - 1);
                if (prevCount != count) {
                    version++;
                }
            }
        }

        int Trim(SplayTreeNode node, int depth) {
            if (depth == 0) {
                node.LeftChild = null;
                node.RightChild = null;
                return 1;
            }
            else {
                int count = 1;

                if (node.LeftChild != null) {
                    count += Trim(node.LeftChild, depth - 1);
                }

                if (node.RightChild != null) {
                    count += Trim(node.RightChild, depth - 1);
                }

                return count;
            }
        }

        public ICollection<TKey> Keys {
            get { return new TiedList<TKey>(this, version, AsList(node => node.Key)); }
        }

        public ICollection<TValue> Values {
            get { return new TiedList<TValue>(this, version, AsList(node => node.Value)); }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            AsList(node => new KeyValuePair<TKey, TValue>(node.Key, node.Value)).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return new TiedList<KeyValuePair<TKey, TValue>>(this, version,
                AsList(node => new KeyValuePair<TKey, TValue>(node.Key, node.Value))).GetEnumerator();
        }

        IList<TEnumerator> AsList<TEnumerator>(Func<SplayTreeNode, TEnumerator> selector) {
            if (root == null) {
                return new TEnumerator[0];
            }

            var result = new List<TEnumerator>(count);
            PopulateList(root, result, selector);
            return result;
        }

        void PopulateList<TEnumerator>(SplayTreeNode node, List<TEnumerator> list,
            Func<SplayTreeNode, TEnumerator> selector) {
            if (node.LeftChild != null) {
                PopulateList(node.LeftChild, list, selector);
            }

            list.Add(selector(node));
            if (node.RightChild != null) {
                PopulateList(node.RightChild, list, selector);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        sealed class SplayTreeNode {
            public readonly TKey Key;

            public TValue Value;
            public SplayTreeNode LeftChild;
            public SplayTreeNode RightChild;

            public SplayTreeNode(TKey key, TValue value) {
                Key = key;
                Value = value;
            }
        }

        sealed class TiedList<T> : IList<T> {
            readonly SplayTree<TKey, TValue> tree;
            readonly int version;
            readonly IList<T> backingList;

            public TiedList(SplayTree<TKey, TValue> tree, int version, IList<T> backingList) {
                if (tree == null) {
                    throw new ArgumentNullException("tree");
                }

                if (backingList == null) {
                    throw new ArgumentNullException("backingList");
                }

                this.tree = tree;
                this.version = version;
                this.backingList = backingList;
            }

            public int IndexOf(T item) {
                if (tree.version != version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                return backingList.IndexOf(item);
            }

            public void Insert(int index, T item) {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index) {
                throw new NotSupportedException();
            }

            public T this[int index] {
                get {
                    if (tree.version != version) {
                        throw new InvalidOperationException("The collection has been modified.");
                    }

                    return backingList[index];
                }
                set { throw new NotSupportedException(); }
            }

            public void Add(T item) {
                throw new NotSupportedException();
            }

            public void Clear() {
                throw new NotSupportedException();
            }

            public bool Contains(T item) {
                if (tree.version != version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                return backingList.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex) {
                if (tree.version != version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                backingList.CopyTo(array, arrayIndex);
            }

            public int Count {
                get { return tree.count; }
            }

            public bool IsReadOnly {
                get { return true; }
            }

            public bool Remove(T item) {
                throw new NotSupportedException();
            }

            public IEnumerator<T> GetEnumerator() {
                if (tree.version != version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                foreach (var item in backingList) {
                    yield return item;
                    if (tree.version != version) {
                        throw new InvalidOperationException("The collection has been modified.");
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }
    }
}