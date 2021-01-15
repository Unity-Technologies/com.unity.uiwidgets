using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    
    public class DisposableBuildContext<T> where T : State{

        public DisposableBuildContext(T _state) {
            D.assert(_state != null);
            D.assert(_state.mounted, () => "A DisposableBuildContext was given a BuildContext for an Element that is not mounted.");
            this._state = _state;
        }

        T _state;
        
        public BuildContext context {
            get {
                D.assert(_debugValidate());
                if (_state == null) {
                    return null;
                }
                return _state.context;
            }
        }
        
        public bool _debugValidate() {
            D.assert(
                _state == null || _state.mounted,() =>
                "A DisposableBuildContext tried to access the BuildContext of a disposed " +
            "State object. This can happen when the creator of this " +
            "DisposableBuildContext fails to call dispose when it is disposed."
                );
            return true;
        }
        
        public void dispose() {
            _state = null;
        }
    }
}