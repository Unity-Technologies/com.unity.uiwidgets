using System;

namespace Unity.UIWidgets.async {
    public static partial class _async {
        public static object _nonNullError(object error) => error ?? new NullReferenceException();
    }
}