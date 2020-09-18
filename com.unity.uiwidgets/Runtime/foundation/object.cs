namespace Unity.UIWidgets.foundation {
    public static partial class foundation_ {
        public static string objectRuntimeType(object @object, string optimizedValue) {
            D.assert(() => {
                optimizedValue = @object.GetType().ToString();
                return true;
            });
            return optimizedValue;
        }
    }
}