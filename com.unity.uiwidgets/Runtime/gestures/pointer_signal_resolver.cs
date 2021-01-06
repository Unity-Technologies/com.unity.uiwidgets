using System;
using System.Collections.Generic;
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
                IEnumerable<DiagnosticsNode> infoCollector() {
                    yield return new DiagnosticsProperty<PointerSignalEvent>("Event", evt, style: DiagnosticsTreeStyle.errorProperty);
                }
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "gesture library",
                        context: new ErrorDescription("while resolving a PointerSignalEvent"),
                        informationCollector: infoCollector
                    )
                );
            }

            _firstRegisteredCallback = null;
            _currentEvent = null;
        }
    }
}