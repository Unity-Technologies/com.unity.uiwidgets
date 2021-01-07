namespace Unity.UIWidgets.foundation {
    public static partial class foundation_ {
        public static string objectRuntimeType(object obj, string optimizedValue) {
            D.assert(() => {
                optimizedValue = obj.GetType().ToString();
                return true;
            });
            return optimizedValue;
        }
    }
}