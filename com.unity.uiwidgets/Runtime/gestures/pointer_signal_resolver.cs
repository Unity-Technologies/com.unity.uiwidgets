using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.gestures {
    public delegate void PointerSignalResolvedCallback(PointerSignalEvent evt);

    public class PointerSignalResolver {
        PointerSignalResolvedCallback _firstRegisteredCallback;

        PointerSignalEvent _currentEvent;

        public void register(PointerSignalEvent evt, PointerSignalResolvedCallback callback) {
            D.assert(evt != null);
            D.assert(callback != null);
            D.assert(_currentEvent == null || _currentEvent == evt);
            if (_firstRegisteredCallback != null) {
                return;
            }

            _currentEvent = evt;
            _firstRegisteredCallback = callback;
        }

        public void resolve(PointerSignalEvent evt) {
            if (_firstRegisteredCallback == null) {
                D.assert(_currentEvent == null);
                return;
            }

            D.assert((_currentEvent.original ?? _currentEvent) == evt);
            try {
                _firstRegisteredCallback(_currentEvent);
            }
            catch (Exception exception) {
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "gesture library",
                        context: "while resolving a PointerSignalEvent",
                        informationCollector: information => {
                            information.AppendLine("Event: ");
                            information.AppendFormat(" {0}", evt);
                        }
                    )
                );
            }

            _firstRegisteredCallback = null;
            _currentEvent = null;
        }
    }
}