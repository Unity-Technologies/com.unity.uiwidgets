namespace Unity.UIWidgets.async {
    public partial class _async {
        internal static object _invokeErrorHandler(
            ZoneBinaryCallback errorHandler, object error, string stackTrace) {
                // Dynamic invocation because we don't know the actual type of the
                // first argument or the error object, but we should successfully call
                // the handler if they match up.
                // TODO(lrn): Should we? Why not the same below for the unary case?
            return errorHandler(error, stackTrace);
        }
    }
}
