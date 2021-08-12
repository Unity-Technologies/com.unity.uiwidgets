namespace Unity.UIWidgets.core {
    public abstract class Sink<T> {
        public abstract void add(T data);

        public abstract void close();
    }
}