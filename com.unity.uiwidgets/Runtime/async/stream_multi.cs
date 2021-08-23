using System;

namespace Unity.UIWidgets.async {
    
    /**
     *  Stream.multi is not supported by flutter 1.17.5 yet, but it might be useful for developers. To address this issue, we put all the necessary codes for this feature
     *  in this single file.
     *
     * [TODO] remove this code when we eventually upgrade UIWidgets to above 2.0
     */
    public class StreamMultiUtils<T>
    {
        public static Stream<T> multi(Action<MultiStreamController<T>> onListen, bool isBroadcast = false) {
            return new _MultiStream<T>(onListen, isBroadcast);
        }
    }
    
    public interface MultiStreamController<T> : IStreamController<T> {
        void addSync(T value);

        void addErrorSync(object error, string trackStack);

        void closeSync();
    }
    
    class _MultiStream<T> : Stream<T> {
        public override bool isBroadcast {
            get {
                return _isBroadcast;
            }
        }

        bool _isBroadcast;

        /// The callback called for each listen.
        public readonly Action<MultiStreamController<T>> _onListen;

        public _MultiStream(Action<MultiStreamController<T>> _onListen, bool isBroadcast) {
            _isBroadcast = isBroadcast;
            this._onListen = _onListen;
        }

        public override StreamSubscription<T> listen(Action<T> onData, Action<object, string> onError = null,
            Action onDone = null, bool cancelOnError = false) {
            var controller = new _MultiStreamController<T>();
            controller.onListen = () => {
                _onListen(controller);
            };
            return controller._subscribe(
                onData, onError, onDone, cancelOnError);
        }
    }

    class _MultiStreamController<T> : _AsyncStreamController<T>, MultiStreamController<T> {
        public _MultiStreamController() : base(null, null, null, null)
        {
        }

        public void addSync(T value) {
            if (!_mayAddEvent) throw _badEventState();
            if (hasListener) _subscription._add(value);
        }

        public void addErrorSync(object error, string trackStack) {
            if (!_mayAddEvent) throw _badEventState();
            if (hasListener) {
                _subscription._addError(error, trackStack ?? "");
            }
        }

        public void closeSync() {
            if (isClosed) return;
            if (!_mayAddEvent) throw _badEventState();
            _state |= _StreamController<T>._STATE_CLOSED;
            if (hasListener) _subscription._close();
        }

        public override Stream<T> stream {
            get {
                throw new Exception("Not available");
            }
        }
    }
}