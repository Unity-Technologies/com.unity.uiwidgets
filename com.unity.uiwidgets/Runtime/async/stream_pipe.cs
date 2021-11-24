using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.async {
    public static partial class _stream {
        /** Runs user code and takes actions depending on success or failure. */
        internal static void _runUserCode<T>(
            Func<T> userCode, Action<T> onSuccess, Action<Exception> onError) {
            try {
                onSuccess(userCode());
            }
            catch (Exception e) {
                AsyncError replacement = Zone.current.errorCallback(e);
                if (replacement == null) {
                    onError(e);
                }
                else {
                    var error = async_._nonNullError(replacement);
                    onError(error);
                }
            }
        }

        internal static void _cancelAndErrorWithReplacement<T>(StreamSubscription<T> subscription,
            _Future future, Exception error) {
            AsyncError replacement = Zone.current.errorCallback(error);
            if (replacement != null) {
                error = (Exception) _async._nonNullError(replacement);
            }

            _cancelAndError(subscription, future, error);
        }

        internal delegate void _ErrorCallback(Exception error);


        internal static _ErrorCallback _cancelAndErrorClosure<T>(
            StreamSubscription<T> subscription, _Future future) {
            return (error) => { _cancelAndError(subscription, future, error); };
        }

        internal static void _cancelAndValue<T>(StreamSubscription<T> subscription, _Future future, object value) {
            var cancelFuture = subscription.cancel();
            if (cancelFuture != null && !Equals(cancelFuture, Future._nullFuture)) {
                cancelFuture.whenComplete(() => future._complete(FutureOr.value(value)));
            }
            else {
                future._complete(FutureOr.value(value));
            }
        }

        static void _cancelAndError<T>(StreamSubscription<T> subscription, _Future future, Exception error
        ) {
            var cancelFuture = subscription.cancel();
            if (cancelFuture != null && !Equals(cancelFuture, Future._nullFuture)) {
                cancelFuture.whenComplete(() => future._completeError(error));
            }
            else {
                future._completeError(error);
            }
        }

        internal static void _cancelAndValue<T>(StreamSubscription<T> subscription, _Future future, FutureOr value) {
            var cancelFuture = subscription.cancel();
            if (cancelFuture != null && !Equals(cancelFuture, Future._nullFuture)) {
                cancelFuture.whenComplete(() => future._complete(value));
            }
            else {
                future._complete(value);
            }
        }


        internal delegate bool _Predicate<T>(T value);

//
        internal static void _addErrorWithReplacement<T>(_EventSink<T> sink, Exception error, string stackTrace) {
            AsyncError replacement = Zone.current.errorCallback(error);
            if (replacement != null) {
                error = async_._nonNullError(replacement);
                stackTrace = replacement.StackTrace;
            }

            sink._addError(error, stackTrace);
        }

        internal delegate T _Transformation<S, T>(S value);

        internal delegate bool _ErrorTest(Exception error);

        internal delegate bool _Equality<T>(T a, T b);
    }

    abstract class _ForwardingStream<S, T> : Stream<T> {
        internal readonly Stream<S> _source;

        internal _ForwardingStream(Stream<S> _source) {
            this._source = _source;
        }

        public override bool isBroadcast {
            get { return _source.isBroadcast; }
        }

        public override StreamSubscription<T> listen(Action<T> onData, Action<object, string> onError = null,
            Action onDone = null, bool cancelOnError = false) {
            cancelOnError = Equals(true, cancelOnError);
            return _createSubscription(onData, onError, onDone, cancelOnError);
        }

        internal virtual StreamSubscription<T> _createSubscription(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError) {
            return new _ForwardingStreamSubscription<S, T>(
                this, onData, onError, onDone, cancelOnError);
        }

        // Override the following methods in subclasses to change the behavior.

        internal virtual void _handleData(S data, _EventSink<T> sink) {
            sink._add((T) (object) data);
        }

        internal virtual void _handleError(object error, _EventSink<T> sink) {
            string stackTrace = error is Exception ? ((Exception) error).StackTrace : "";
            sink._addError(error, stackTrace);
        }

        internal virtual void _handleDone(_EventSink<T> sink) {
            sink._close();
        }
    }

//
// /**
//  * Abstract superclass for subscriptions that forward to other subscriptions.
//  */
    class _ForwardingStreamSubscription<S, T>
        : _BufferingStreamSubscription<T> {
        readonly _ForwardingStream<S, T> _stream;

        StreamSubscription<S> _subscription;

        internal _ForwardingStreamSubscription(_ForwardingStream<S, T> _stream,
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError
        )
            : base(onData, onError, onDone, cancelOnError) {
            this._stream = _stream;
            _subscription = _stream._source
                .listen(_handleData, onError: _handleError, onDone: _handleDone);
        }

        // _StreamSink interface.
        // Transformers sending more than one event have no way to know if the stream
        // is canceled or closed after the first, so we just ignore remaining events.

        public override void _add(T data) {
            if (_isClosed) return;
            base._add(data);
        }

        public override void _addError(object error, string stackTrace) {
            if (_isClosed) return;
            base._addError(error, stackTrace);
        }

        // StreamSubscription callbacks.

        protected override void _onPause() {
            if (_subscription == null) return;
            _subscription.pause();
        }

        protected override void _onResume() {
            if (_subscription == null) return;
            _subscription.resume();
        }

        protected override Future _onCancel() {
            if (_subscription != null) {
                StreamSubscription<S> subscription = _subscription;
                _subscription = null;
                return subscription.cancel();
            }

            return null;
        }

        // Methods used as listener on source subscription.

        void _handleData(S data) {
            _stream._handleData(data, this);
        }

        void _handleError(object error, string stackTrace) {
            _stream._handleError((Exception) error, this);
        }

        void _handleDone() {
            _stream._handleDone(this);
        }
    }

//
// // -------------------------------------------------------------------
// // Stream transformers used by the default Stream implementation.
// // -------------------------------------------------------------------
//
//
    class _WhereStream<T> : _ForwardingStream<T, T> {
        readonly _stream._Predicate<T> _test;

        internal _WhereStream(Stream<T> source, Func<T, bool> test) : base(source) {
            _test = d => test(d);
        }

        internal override void _handleData(T inputEvent, _EventSink<T> sink) {
            bool satisfies;
            try {
                satisfies = _test(inputEvent);
            }
            catch (Exception e) {
                _stream._addErrorWithReplacement(sink, e, e.StackTrace);
                return;
            }

            if (satisfies) {
                sink._add(inputEvent);
            }
        }
    }

//
//
// /**
//  * A stream pipe that converts data events before passing them on.
//  */
    class _MapStream<S, T> : _ForwardingStream<S, T> {
        readonly _stream._Transformation<S, T> _transform;

        internal _MapStream(Stream<S> source, Func<S, T> transform) : base(source) {
            _transform = d => transform(d);
        }

        internal override void _handleData(S inputEvent, _EventSink<T> sink) {
            T outputEvent;
            try {
                outputEvent = _transform(inputEvent);
            }
            catch (Exception e) {
                _stream._addErrorWithReplacement(sink, e, e.StackTrace);
                return;
            }

            sink._add(outputEvent);
        }
    }

//
// /**
//  * A stream pipe that converts data events before passing them on.
//  */
    class _ExpandStream<S, T> : _ForwardingStream<S, T> {
        readonly _stream._Transformation<S, IEnumerable<T>> _expand;

        internal _ExpandStream(Stream<S> source, _stream._Transformation<S, IEnumerable<T>> expand) : base(source) {
            _expand = expand;
        }

        internal override void _handleData(S inputEvent, _EventSink<T> sink) {
            try {
                foreach (T value in _expand(inputEvent)) {
                    sink._add(value);
                }
            }
            catch (Exception e) {
                // If either _expand or iterating the generated iterator throws,
                // we abort the iteration.
                _stream._addErrorWithReplacement(sink, e, e.StackTrace);
            }
        }
    }

//
//
// /**
//  * A stream pipe that converts or disposes error events
//  * before passing them on.
//  */
    class _HandleErrorStream<T> : _ForwardingStream<T, T> {
        readonly ZoneBinaryCallback _transform;
        readonly _stream._ErrorTest _test;

        internal _HandleErrorStream(Stream<T> source, ZoneBinaryCallback onError, _stream._ErrorTest test) :
            base(source) {
            _transform = onError;
            _test = test;
        }


        internal override void _handleError(object error, _EventSink<T> sink) {
            bool matches = true;
            if (_test != null) {
                try {
                    matches = _test((Exception) error);
                }
                catch (Exception e) {
                    _stream._addErrorWithReplacement(sink, e, e.StackTrace);
                    return;
                }
            }

            string stackTrace = error is Exception ? ((Exception) error).StackTrace : "";
            if (matches) {
                try {
                    _async._invokeErrorHandler(_transform, error, stackTrace);
                }
                catch (Exception e) {
                    if (Equals(e, error)) {
                        sink._addError(error, stackTrace);
                    }
                    else {
                        _stream._addErrorWithReplacement(sink, e, e.StackTrace);
                    }

                    return;
                }
            }
            else {
                sink._addError(error, stackTrace);
            }
        }
    }

//
    class _TakeStream<T> : _ForwardingStream<T, T> {
        readonly int _count;

        internal _TakeStream(Stream<T> source, int count) : base(source) {
            _count = count;
            // This test is done early to avoid handling an async error
            // in the _handleData method.
            // ArgumentError.checkNotNull(count, "count");
        }

        internal override StreamSubscription<T> _createSubscription(Action<T> onData, Action<object, string> onError,
            Action onDone, bool cancelOnError) {
            if (_count == 0) {
                _source.listen(null).cancel();
                return new _DoneStreamSubscription<T>(() => onDone());
            }

            return new _StateStreamSubscription<T>(
                this, onData, onError, onDone, cancelOnError, _count);
        }

        internal override void _handleData(T inputEvent, _EventSink<T> sink) {
            _StateStreamSubscription<T> subscription = (_StateStreamSubscription<T>) sink;
            int count = subscription._count;
            if (count > 0) {
                sink._add(inputEvent);
                count -= 1;
                subscription._count = count;
                if (count == 0) {
                    // Closing also unsubscribes all subscribers, which unsubscribes
                    // this from source.
                    sink._close();
                }
            }
        }
    }

//
// /**
//  * A [_ForwardingStreamSubscription] with one extra state field.
//  *
//  * Use by several different classes, storing an integer, bool or general.
//  */
    class _StateStreamSubscription<T> : _ForwardingStreamSubscription<T, T> {
        // Raw state field. Typed access provided by getters and setters below.
        // siyao: this is used as bool and int, if it was used at the same time, everything would be fxxked up.
        object _sharedState;

        internal _StateStreamSubscription(
            _ForwardingStream<T, T> stream,
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError, object _sharedState
        )
            : base(stream, onData, onError, onDone, cancelOnError) {
            this._sharedState = _sharedState;
        }

        internal bool _flag {
            get => (bool) _sharedState;
            set => _sharedState = value;
        }

        internal int _count {
            get => (int) _sharedState;
            set => _sharedState = value;
        }

        internal object _value {
            get => _sharedState;
            set => _sharedState = value;
        }
    }

    class _TakeWhileStream<T> : _ForwardingStream<T, T> {
        readonly _stream._Predicate<T> _test;

        internal _TakeWhileStream(Stream<T> source, _stream._Predicate<T> test)
            : base(source) {
            _test = test;
        }


        internal override void _handleData(T inputEvent, _EventSink<T> sink) {
            bool satisfies;
            try {
                satisfies = _test(inputEvent);
            }
            catch (Exception e) {
                _stream._addErrorWithReplacement(sink, e, e.StackTrace);
                // The test didn't say true. Didn't say false either, but we stop anyway.
                sink._close();
                return;
            }

            if (satisfies) {
                sink._add(inputEvent);
            }
            else {
                sink._close();
            }
        }
    }

//
    class _SkipStream<T> : _ForwardingStream<T, T> {
        readonly int _count;

        internal _SkipStream(Stream<T> source, int count)
            : base(source) {
            _count = count;
            // This test is done early to avoid handling an async error
            // in the _handleData method.
            // ArgumentError.checkNotNull(count, "count");
            // RangeError.checkNotNegative(count, "count");
        }

        internal override StreamSubscription<T> _createSubscription(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError) {
            return new _StateStreamSubscription<T>(
                this, onData, onError, onDone, cancelOnError, _count);
        }

        internal void _handleDone(T inputEvent, _EventSink<T> sink) {
            _StateStreamSubscription<T> subscription = (_StateStreamSubscription<T>) sink;
            int count = subscription._count;
            if (count > 0) {
                subscription._count = count - 1;
                return;
            }

            sink._add(inputEvent);
        }
    }


    class _SkipWhileStream<T> : _ForwardingStream<T, T> {
        readonly _stream._Predicate<T> _test;

        internal _SkipWhileStream(Stream<T> source, _stream._Predicate<T> test) : base(source) {
            _test = test;
        }

        internal override StreamSubscription<T> _createSubscription(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError
        ) {
            return new _StateStreamSubscription<T>(
                this, onData, onError, onDone, cancelOnError, false);
        }

        internal override void _handleData(T inputEvent, _EventSink<T> sink) {
            _StateStreamSubscription<T> subscription = (_StateStreamSubscription<T>) sink;
            bool hasFailed = subscription._flag;
            if (hasFailed) {
                sink._add(inputEvent);
                return;
            }

            bool satisfies;
            try {
                satisfies = _test(inputEvent);
            }
            catch (Exception e) {
                _stream._addErrorWithReplacement(sink, e, e.StackTrace);
                // A failure to return a boolean is considered "not matching".
                subscription._flag = true;
                return;
            }

            if (!satisfies) {
                subscription._flag = true;
                sink._add(inputEvent);
            }
        }
    }


    class _DistinctStream<T> : _ForwardingStream<T, T> {
        static readonly object _SENTINEL = new object();

        readonly _stream._Equality<T> _equals;

        internal _DistinctStream(Stream<T> source, _stream._Equality<T> equals) : base(source) {
            _equals = equals;
        }

        internal override StreamSubscription<T> _createSubscription(Action<T> onData, Action<object, string> onError,
            Action onDone, bool cancelOnError) {
            return new _StateStreamSubscription<T>(
                this, onData, onError, onDone, cancelOnError, _SENTINEL);
        }

        internal override void _handleData(T inputEvent, _EventSink<T> sink) {
            _StateStreamSubscription<T> subscription = (_StateStreamSubscription<T>) sink;
            var previous = subscription._value;
            if (Equals(previous, _SENTINEL)) {
                // First event.
                subscription._value = inputEvent;
                sink._add(inputEvent);
            }
            else {
                T previousEvent = (T) previous;
                bool isEqual;
                try {
                    if (_equals == null) {
                        isEqual = Equals(previousEvent, inputEvent);
                    }
                    else {
                        isEqual = _equals(previousEvent, inputEvent);
                    }
                }
                catch (Exception e) {
                    _stream._addErrorWithReplacement(sink, e, e.StackTrace);
                    return;
                }

                if (!isEqual) {
                    sink._add(inputEvent);
                    subscription._value = inputEvent;
                }
            }
        }
    }
}