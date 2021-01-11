using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    
    public class DisposableBuildContext<T> where T : State{
        /// Creates an object that provides access to a [BuildContext] without leaking
        /// a [State].
        ///
        /// Creators must call [dispose] when the [State] is disposed.
        ///
        /// The [State] must not be null, and [State.mounted] must be true.
        public DisposableBuildContext(T state) {
            D.assert(_state != null);
            D.assert(_state.mounted, () => "A DisposableBuildContext was given a BuildContext for an Element that is not mounted.");
        }

        T _state;

        /// Provides safe access to the build context.
        ///
        /// If [dispose] has been called, will return null.
        ///
        /// Otherwise, asserts the [_state] is still mounted and returns its context.
        public BuildContext context {
            get {
                D.assert(_debugValidate());
                if (_state == null) {
                    return null;
                }
                return _state.context;
            }
        }

        /// Called from asserts or tests to determine whether this object is in a
        /// valid state.
        ///
        /// Always returns true, but will assert if [dispose] has not been called
        /// but the state this is tracking is unmounted.
        public bool _debugValidate() {
            D.assert(
                _state == null || _state.mounted,() =>
                "A DisposableBuildContext tried to access the BuildContext of a disposed " +
            "State object. This can happen when the creator of this " +
            "DisposableBuildContext fails to call dispose when it is disposed."
                );
            return true;
        }
        
        /// Marks the [BuildContext] as disposed.
        ///
        /// Creators of this object must call [dispose] when their [Element] is
        /// unmounted, i.e. when [State.dispose] is called.
        void dispose() {
            _state = null;
        }
    }
}