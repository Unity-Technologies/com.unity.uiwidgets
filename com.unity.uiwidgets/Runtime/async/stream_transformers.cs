using System;

namespace Unity.UIWidgets.async {
    class _EventSinkWrapper<T> : EventSink<T> {
        _EventSink<T> _sink;

        internal _EventSinkWrapper(_EventSink<T> _sink) {
            this._sink = _sink;
        }

        public override void add(T data) {
            _sink._add(data);
        }

        public override void addError(object error, string stackTrace) {
            _sink._addError(error, stackTrace ?? AsyncError.defaultStackTrace(error));
        }

        public override Future close() {
            _sink._close();
            return Future._nullFuture;
        }
    }

    class _SinkTransformerStreamSubscription<S, T>
        : _BufferingStreamSubscription<T> {
        /// The transformer's input sink.
        EventSink<S> _transformerSink;

        /// The subscription to the input stream.
        StreamSubscription<S> _subscription;

        internal _SinkTransformerStreamSubscription(Stream<S> source, _async._SinkMapper<S, T> mapper,
                Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError)
            // We set the adapter's target only when the user is allowed to send data.
            : base(onData, onError, onDone, cancelOnError) {
            _EventSinkWrapper<T> eventSink = new _EventSinkWrapper<T>(this);
            _transformerSink = mapper(eventSink);
            _subscription =
                source.listen(_handleData, onError: _handleError, onDone: _handleDone);
        }

        /** Whether this subscription is still subscribed to its source. */
        bool _isSubscribed {
            get { return _subscription != null; }
        }

        // _EventSink interface.

        public override void _add(T data) {
            if (_isClosed) {
                throw new Exception("Stream is already closed");
            }

            base._add(data);
        }

        public override void _addError(object error, string stackTrace) {
            if (_isClosed) {
                throw new Exception("Stream is already closed");
            }

            base._addError(error, stackTrace);
        }

        public override void _close() {
            if (_isClosed) {
                throw new Exception("Stream is already closed");
            }

            base._close();
        }

        // _BufferingStreamSubscription hooks.

        protected override void _onPause() {
            if (_isSubscribed) _subscription.pause();
        }

        protected override void _onResume() {
            if (_isSubscribed) _subscription.resume();
        }

        protected override Future _onCancel() {
            if (_isSubscribed) {
                StreamSubscription<S> subscription = _subscription;
                _subscription = null;
                return subscription.cancel();
            }

            return null;
        }

        void _handleData(S data) {
            try {
                _transformerSink.add(data);
            }
            catch (Exception e) {
                _addError(e, e.StackTrace);
            }
        }

        void _handleError(object error, string stackTrace) {
            try {
                _transformerSink.addError(error, stackTrace);
            }
            catch (Exception e) {
                if (Equals(e, error)) {
                    _addError(error, stackTrace);
                }
                else {
                    _addError(e, e.StackTrace);
                }
            }
        }

        void _handleDone() {
            try {
                _subscription = null;
                _transformerSink.close();
            }
            catch (Exception e) {
                _addError(e, e.StackTrace);
            }
        }
    }

    class _StreamSinkTransformer<S, T> : StreamTransformerBase<S, T> {
        readonly _async._SinkMapper<S, T> _sinkMapper;

        public _StreamSinkTransformer(_async._SinkMapper<S, T> _sinkMapper) {
            this._sinkMapper = _sinkMapper;
        }

        public override Stream<T> bind(Stream<S> stream) =>
            new _BoundSinkStream<S, T>(stream, _sinkMapper);
    }

    class _BoundSinkStream<S, T> : Stream<T> {
        readonly _async._SinkMapper<S, T> _sinkMapper;
        readonly Stream<S> _stream;

        public override bool isBroadcast {
            get { return _stream.isBroadcast; }
        }

        internal _BoundSinkStream(Stream<S> _stream, _async._SinkMapper<S, T> _sinkMapper) {
            this._stream = _stream;
            this._sinkMapper = _sinkMapper;
        }

        public override StreamSubscription<T> listen(Action<T> onData,
            Action<object, string> onError = null, Action onDone = null, bool cancelOnError = default) {
            StreamSubscription<T> subscription =
                new _SinkTransformerStreamSubscription<S, T>(
                    _stream, _sinkMapper, onData, onError, onDone, cancelOnError);
            return subscription;
        }
    }

    static partial class _stream {
        public delegate void _TransformDataHandler<S, T>(S data, EventSink<T> sink);

        /// Error-handler coming from [StreamTransformer.fromHandlers].
        public delegate void _TransformErrorHandler<T>(
            object error, string stackTrace, EventSink<T> sink);

        /// Done-handler coming from [StreamTransformer.fromHandlers].
        public delegate void _TransformDoneHandler<T>(EventSink<T> sink);
    }

    class _HandlerEventSink<S, T> : EventSink<S> {
        readonly _stream._TransformDataHandler<S, T> _handleData;
        readonly _stream._TransformErrorHandler<T> _handleError;
        readonly _stream._TransformDoneHandler<T> _handleDone;

        /// The output sink where the handlers should send their data into.
        EventSink<T> _sink;

        internal _HandlerEventSink(
            _stream._TransformDataHandler<S, T> _handleData, _stream._TransformErrorHandler<T> _handleError,
            _stream._TransformDoneHandler<T> _handleDone, EventSink<T> _sink) {
            this._handleData = _handleData;
            this._handleError = _handleError;
            this._handleDone = _handleDone;
            this._sink = _sink;
            if (_sink == null) {
                throw new Exception("The provided sink must not be null.");
            }
        }

        bool _isClosed {
            get { return _sink == null; }
        }

        public override void add(S data) {
            if (_isClosed) {
                throw new Exception("Sink is closed");
            }

            if (_handleData != null) {
                _handleData(data, _sink);
            }
            else {
                _sink.add((T)((object)data));
            }
        }

        public override void addError(object error, string stackTrace) {
            // ArgumentError.checkNotNull(error, "error");
            if (_isClosed) {
                throw new Exception("Sink is closed");
            }

            if (_handleError != null) {
                stackTrace =  stackTrace ?? AsyncError.defaultStackTrace(error);
                _handleError(error, stackTrace, _sink);
            }
            else {
                _sink.addError(error, stackTrace);
            }
        }

        public override Future close() {
            if (_isClosed) return Future._nullFuture;
            var sink = _sink;
            _sink = null;
            if (_handleDone != null) {
                _handleDone(sink);
            }
            else {
                sink.close();
            }
            return Future._nullFuture;
        }
    }

    class _StreamHandlerTransformer<S, T> : _StreamSinkTransformer<S, T> {
        internal _StreamHandlerTransformer(
            _stream._TransformDataHandler<S, T> handleData = null,
            _stream._TransformErrorHandler<T> handleError = null,
            _stream._TransformDoneHandler<T> handleDone = null)
            : base((EventSink<T> outputSink) => {
                return new _HandlerEventSink<S, T>(
                    handleData, handleError, handleDone, outputSink);
            }) {
        }

        public override Stream<T> bind(Stream<S> stream) {
            return base.bind(stream);
        }
    }

    class _StreamBindTransformer<S, T> : StreamTransformerBase<S, T> {
        readonly Func<Stream<S>, Stream<T>> _bind;

        internal _StreamBindTransformer(Func<Stream<S>, Stream<T>> _bind) {
            this._bind = _bind;
        }

        public override Stream<T> bind(Stream<S> stream) => _bind(stream);
    }

    public partial class _async {
        public delegate EventSink<S> _SinkMapper<S, T>(EventSink<T> output);

        public delegate StreamSubscription<T> _SubscriptionTransformer<S, T>(Stream<S> stream, bool cancelOnError);
    }

    class _StreamSubscriptionTransformer<S, T> : StreamTransformerBase<S, T> {
        readonly _async._SubscriptionTransformer<S, T> _onListen;

        internal _StreamSubscriptionTransformer(_async._SubscriptionTransformer<S, T> _onListen) {
            this._onListen = _onListen;
        }

        public override Stream<T> bind(Stream<S> stream) =>
            new _BoundSubscriptionStream<S, T>(stream, _onListen);
    }

    class _BoundSubscriptionStream<S, T> : Stream<T> {
        internal _BoundSubscriptionStream(Stream<S> _stream, _async._SubscriptionTransformer<S, T> _onListen) {
            this._stream = _stream;
            this._onListen = _onListen;
        }

        readonly _async._SubscriptionTransformer<S, T> _onListen;
        readonly Stream<S> _stream;

        public override bool isBroadcast {
            get { return _stream.isBroadcast; }
        }

        public override StreamSubscription<T> listen(Action<T> onData,
            Action<object, string> onError = null, Action onDone = null, bool cancelOnError = false) {
            //cancelOnError = cancelOnError;
            StreamSubscription<T> result = _onListen(_stream, cancelOnError);
            result.onData(onData);
            result.onError(onError);
            result.onDone(onDone);
            return result;
        }
    }
}