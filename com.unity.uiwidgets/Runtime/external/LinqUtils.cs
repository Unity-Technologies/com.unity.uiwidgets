using System.Collections.Generic;

namespace Unity.UIWidgets.external
{
    public static class LinqUtils<T, S>
    {
        public delegate T CreateItem(S item);

        public delegate bool FilterDict(S value);

        public static List<T> SelectList(IEnumerable<S> items, CreateItem createItem)
        {
            if (items == null)
                return null;
            var results = new List<T>();
            foreach (var item in items) results.Add(createItem(item));

            return results;
        }

        public static T[] SelectArray(List<S> items, CreateItem createItem)
        {
            if (items == null)
                return null;

            var itemCount = items.Count;
            var results = new T[itemCount];
            for (var i= 0; i < itemCount; i++)
            {
                results[i] = createItem(items[i]);
            }

            return results;
        }

        public static Dictionary<T, S> SelectDictionary(IEnumerable<S> items, CreateItem createItem)
        {
            if (items == null)
                return null;
            var results = new Dictionary<T, S>();
            foreach (var item in items) results.Add(createItem(item), item);
            return results;
        }

        public static Dictionary<T, S> WhereDictionary(Dictionary<T, S> items, FilterDict filterDict)
        {
            if (items == null)
                return null;
            var results = new Dictionary<T, S>();
            foreach (var item in items)
                if (filterDict(item.Value))
                    results.Add(item.Key, item.Value);
            return results;
        }
    }

    public static class LinqUtils<T>
    {
        public delegate T CreateItem(T item);

        public delegate bool FilterItem(T item);

        public static List<T> SelectList(IEnumerable<T> items, CreateItem createItem)
        {
            if (items == null)
                return null;
            var results = new List<T>();
            foreach (var item in items) results.Add(createItem(item));

            return results;
        }

        public static List<T> WhereList(IEnumerable<T> items, FilterItem filterItem)
        {
            if (items == null)
                return null;
            var results = new List<T>();
            foreach (var item in items)
                if (filterItem(item))
                    results.Add(item);

            return results;
        }
    }
}