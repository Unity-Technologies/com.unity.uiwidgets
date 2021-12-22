using System;
using Unity.UIWidgets.async;

namespace Unity.UIWidgets.async {
    public class CastStream<S, T> : Stream<T> {
        readonly Stream<S> _source;

        public CastStream(Stream<S> _source) {
            this._source = _source;
        }

        public override bool isBroadcast {
            get { return _source.isBroadcast; }
        }

        public override StreamSubscription<T> listen(Action<T> onData, Action<object, string> onError = null,
            Action onDone = null, bool cancelOnError = false) {
            var result = new CastStreamSubscription<S, T>(
                _source.listen(null, onDone: onDone, cancelOnError: cancelOnError));

            result.onData(onData);
            result.onError(onError);
            return result;
        }

        public override Stream<R> cast<R>() => new CastStream<S, R>(_source);
    }


    class CastStreamSubscription<S, T> : StreamSubscription<T> {
        readonly StreamSubscription<S> _source;

        /// Zone where listen was called.
        readonly Zone _zone = Zone.current;

        /// User's data handler. May be null.
        ZoneUnaryCallback _handleData;

        /// Copy of _source's handleError so we can report errors in onData.
        /// May be null.
        ZoneBinaryCallback _handleError;

        public CastStreamSubscription(StreamSubscription<S> _source) {
            this._source = _source;
            _source.onData(_onData);
        }

        public override Future cancel() => _source.cancel();

        public override void onData(Action<T> handleData) {
            _handleData = handleData == null
                ? null
                : _zone.registerUnaryCallback(data => {
                    handleData((T) data);
                    return null;
                });
        }

        public override void onError(Action<object, string> handleError) {
            _source.onError(handleError);
            if (handleError == null) {
                _handleError = null;
            }
            else {
                _handleError = _zone
                    .registerBinaryCallback((a, b) => {
                        handleError(a, (string) b);
                        return null;
                    });
            }
        }

        public override void onDone(Action handleDone) {
            _source.onDone(handleDone);
        }
        void _onData(S data) {
            if (_handleData == null) return;
            T targetData;
            try {
                // siyao: this might go wrong
                targetData = (T) (object) data;
            }
            catch (Exception error) {
                if (_handleError == null) {
                    _zone.handleUncaughtError(error);
                }
                else {
                    _zone.runBinaryGuarded(_handleError, error, error.StackTrace);
                }

                return;
            }

            _zone.runUnaryGuarded(_handleData, targetData);
        }

        public override void pause(Future resumeSignal = null) {
            _source.pause(resumeSignal);
        }

        public override void resume() {
            _source.resume();
        }

        public override bool isPaused {
            get { return _source.isPaused; }
        }

        public override Future<E> asFuture<E>(E futureValue) => _source.asFuture<E>(futureValue);
    }

    class CastStreamTransformer<SS, ST, TS, TT>
        : StreamTransformerBase<TS, TT> {
        public readonly StreamTransformer<SS, ST> _source;

        public CastStreamTransformer(StreamTransformer<SS, ST> _source) {
            this._source = _source;
        }

        public override StreamTransformer<RS, RT> cast<RS, RT>() =>
            new CastStreamTransformer<SS, ST, RS, RT>(_source);

        public override Stream<TT> bind(Stream<TS> stream) =>
            _source.bind(stream.cast<SS>()).cast<TT>();
    }
}