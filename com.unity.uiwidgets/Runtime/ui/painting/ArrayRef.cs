using System;

namespace Unity.UIWidgets.uiOld{
    public class ArrayRef<T> {
        public T[] array;

        public int length;

        public ArrayRef() {
            array = Array.Empty<T>();
            length = 0;
        }

        public void add(T item) {
            if (length == array.Length) {
                int newCapacity = array.Length == 0 ? 4 : array.Length * 2;
                Array.Resize(ref array, newCapacity);
            }

            array[length++] = item;
        }
    }
}
