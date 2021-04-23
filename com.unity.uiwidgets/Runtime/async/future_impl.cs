using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.async {
    using _FutureOnValue = Func<object, FutureOr>;
    using _FutureErrorTest = Func<Exception, bool>;
    using _FutureAction = Func<FutureOr>;

    abstract class _Completer : Completer {
        protected readonly _Future _future = new _Future();
        public override Future future => _future;

        public override void completeError(Exception error) {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            if (!_future._mayComplete) throw new Exception("Future already completed");
            AsyncError replacement = Zone.current.errorCallback(error);
            if (replacement != null) {
                error = async_._nonNullError(replacement.InnerException);
            }

            _completeError(error);
        }

        protected abstract void _completeError(Exception error);

        public override bool isCompleted => !_future._mayComplete;
    }

    class _AsyncCompleter : _Completer {
        public override void complete(FutureOr value = default) {
            if (!_future._mayComplete) throw new Exception("Future already completed");
            _future._asyncComplete(value);
        }

        protected override void _completeError(Exception error) {
            _future._asyncCompleteError(error);
        }
    }

    class _SyncCompleter : _Completer {
        public override void complete(FutureOr value = default) {
            if (!_future._mayComplete) throw new Exception("Future already completed");
            _future._complete(value);
        }

        protected override void _completeError(Exception error) {
            _future._completeError(error);
        }
    }


    class _FutureListener {
        public const int maskValue = 1;
        public const int maskError = 2;
        public const int maskTestError = 4;
        public const int maskWhencomplete = 8;

        public const int stateChain = 0;
        public const int stateThen = maskValue;
        public const int stateThenOnerror = maskValue | maskError;
        public const int stateCatcherror = maskError;
        public const int stateCatcherrorTest = maskError | maskTestError;
        public const int stateWhencomplete = maskWhencomplete;
        public const int maskType = maskValue | maskError | maskTestError | maskWhencomplete;
        public const int stateIsAwait = 16;

        internal _FutureListener _nextListener;

        public readonly _Future result;
        public readonly int state;
        public readonly Delegate callback;
        public readonly Func<Exception, FutureOr> errorCallback;

        _FutureListener(_Future result, Delegate callback, Func<Exception, FutureOr> errorCallback, int state) {
            this.result = result;
            this.state = state;
            this.callback = callback;
            this.errorCallback = errorCallback;
        }

        public static _FutureListener then(
            _Future result, _FutureOnValue onValue, Func<Exception, FutureOr> errorCallback) {
            return new _FutureListener(
                result, onValue, errorCallback,
                (errorCallback == null) ? stateThen : stateThenOnerror
            );
        }

        public static _FutureListener thenAwait(
            _Future result, _FutureOnValue onValue, Func<Exception, FutureOr> errorCallback) {
            return new _FutureListener(
                result, onValue, errorCallback,
                ((errorCallback == null) ? stateThen : stateThenOnerror) | stateIsAwait
            );
        }

        public static _FutureListener catchError(_Future result, Func<Exception, FutureOr> errorCallback,
            _FutureErrorTest callback) {
            return new _FutureListener(
                result, callback, errorCallback,
                (callback == null) ? stateCatcherror : stateCatcherrorTest
            );
        }

        public static _FutureListener whenComplete(_Future result, _FutureAction callback) {
            return new _FutureListener(
                result, callback, null,
                stateWhencomplete
            );
        }

        internal Zone _zone => result._zone;

        public bool handlesValue => (state & maskValue) != 0;
        public bool handlesError => (state & maskError) != 0;
        public bool hasErrorTest => (state & maskType) == stateCatcherrorTest;
        public bool handlesComplete => (state & maskType) == stateWhencomplete;

        public bool isAwait => (state & stateIsAwait) != 0;

        internal _FutureOnValue _onValue {
            get {
                D.assert(handlesValue);
                return (_FutureOnValue) callback;
            }
        }

        internal Func<Exception, FutureOr> _onError => errorCallback;

        internal _FutureErrorTest _errorTest {
            get {
                D.assert(hasErrorTest);
                return (_FutureErrorTest) callback;
            }
        }

        internal _FutureAction _whenCompleteAction {
            get {
                D.assert(handlesComplete);
                return (_FutureAction) callback;
            }
        }

        public bool hasErrorCallback {
            get {
                D.assert(handlesError);
                return _onError != null;
            }
        }

        public FutureOr handleValue(object sourceResult) {
            return (FutureOr) _zone.runUnary(arg => _onValue(arg), sourceResult);
        }

        public bool matchesErrorTest(AsyncError asyncError) {
            if (!hasErrorTest) return true;
            return (bool) _zone.runUnary(arg => _errorTest((Exception) arg), asyncError.InnerException);
        }

        public FutureOr handleError(AsyncError asyncError) {
            D.assert(handlesError && hasErrorCallback);

            var errorCallback = this.errorCallback;
            return (FutureOr) _zone.runUnary(arg => errorCallback((Exception) arg), asyncError.InnerException);
        }

        public FutureOr handleWhenComplete() {
            D.assert(!handlesError);
            return (FutureOr) _zone.run(() => _whenCompleteAction());
        }
    }

    public class _Future : Future {
        internal const int _stateIncomplete = 0;
        internal const int _statePendingComplete = 1;
        internal const int _stateChained = 2;
        internal const int _stateValue = 4;
        internal const int _stateError = 8;

        internal int _state = _stateIncomplete;

        internal readonly Zone _zone;

        internal object _resultOrListeners;

        internal _Future() {
            _zone = Zone.current;
        }

        internal _Future(Zone zone) {
            _zone = zone;
        }

        internal static _Future immediate(FutureOr result) {
            var future = new _Future(Zone.current);
            future._asyncComplete(result);
            return future;
        }

        internal static _Future zoneValue(object value, Zone zone) {
            var future = new _Future(zone);
            future._setValue(value);
            return future;
        }

        internal static _Future immediateError(Exception error) {
            var future = new _Future(Zone.current);
            future._asyncCompleteError(error);
            return future;
        }

        internal static _Future value(object value) {
            return zoneValue(value, Zone.current);
        }

        internal bool _mayComplete => _state == _stateIncomplete;
        internal bool _isPendingComplete => _state == _statePendingComplete;
        internal bool _mayAddListener => _state <= _statePendingComplete;
        internal bool _isChained => _state == _stateChained;
        internal bool _isComplete => _state >= _stateValue;
        internal bool _hasError => _state == _stateError;

        internal void _setChained(_Future source) {
            D.assert(_mayAddListener);
            _state = _stateChained;
            _resultOrListeners = source;
        }

        public override Future then(Func<object, FutureOr> f, Func<Exception, FutureOr> onError = null) {
            Zone currentZone = Zone.current;
            if (!ReferenceEquals(currentZone, async_._rootZone)) {
                f = async_._registerUnaryHandler(f, currentZone);
                if (onError != null) {
                    onError = async_._registerErrorHandler(onError, currentZone);
                }
            }

            _Future result = new _Future();
            _addListener(_FutureListener.then(result, f, onError));
            return result;
        }

        public override Future catchError(Func<Exception, FutureOr> onError, Func<Exception, bool> test = null) {
            _Future result = new _Future();
            if (!ReferenceEquals(result._zone, async_._rootZone)) {
                onError = async_._registerErrorHandler(onError, result._zone);
                if (test != null) {
                    test = async_._registerUnaryHandler(test, result._zone);
                }
            }

            _addListener(_FutureListener.catchError(result, onError, test));
            return result;
        }

        public override Future whenComplete(Func<FutureOr> action) {
            _Future result = new _Future();
            if (!ReferenceEquals(result._zone, async_._rootZone)) {
                action = async_._registerHandler(action, result._zone);
            }

            _addListener(_FutureListener.whenComplete(result, action));
            return result;
        }

        // Stream<T> asStream() => new Stream<T>.fromFuture(this);

        internal void _setPendingComplete() {
            D.assert(_mayComplete);
            _state = _statePendingComplete;
        }

        internal void _clearPendingComplete() {
            D.assert(_isPendingComplete);
            _state = _stateIncomplete;
        }

        internal AsyncError _error {
            get {
                D.assert(_hasError);
                return (AsyncError) _resultOrListeners;
            }
        }

        internal _Future _chainSource {
            get {
                D.assert(_isChained);
                return (_Future) _resultOrListeners;
            }
        }

        internal void _setValue(object value) {
            D.assert(!(value is Future || value is FutureOr));
            D.assert(!_isComplete); // But may have a completion pending.
            _state = _stateValue;
            _resultOrListeners = value;
        }

        internal void _setErrorObject(AsyncError error) {
            D.assert(!_isComplete); // But may have a completion pending.
            _state = _stateError;
            _resultOrListeners = error;
        }

        internal void _setError(Exception error) {
            _setErrorObject(new AsyncError(error));
        }

        internal void _cloneResult(_Future source) {
            D.assert(!_isComplete);
            D.assert(source._isComplete);
            _state = source._state;
            _resultOrListeners = source._resultOrListeners;
        }

        internal void _addListener(_FutureListener listener) {
            D.assert(listener._nextListener == null);
            if (_mayAddListener) {
                listener._nextListener = (_FutureListener) _resultOrListeners;
                _resultOrListeners = listener;
            }
            else {
                if (_isChained) {
                    // Delegate listeners to chained source future.
                    // If the source is complete, instead copy its values and
                    // drop the chaining.
                    _Future source = _chainSource;
                    if (!source._isComplete) {
                        source._addListener(listener);
                        return;
                    }

                    _cloneResult(source);
                }

                D.assert(_isComplete);
                // Handle late listeners asynchronously.
                _zone.scheduleMicrotask(() => {
                    _propagateToListeners(this, listener);
                    return null;
                });
            }
        }

        void _prependListeners(_FutureListener listeners) {
            if (listeners == null) return;
            if (_mayAddListener) {
                _FutureListener existingListeners = (_FutureListener) _resultOrListeners;
                _resultOrListeners = listeners;
                if (existingListeners != null) {
                    _FutureListener cursor = listeners;
                    while (cursor._nextListener != null) {
                        cursor = cursor._nextListener;
                    }

                    cursor._nextListener = existingListeners;
                }
            }
            else {
                if (_isChained) {
                    // Delegate listeners to chained source future.
                    // If the source is complete, instead copy its values and
                    // drop the chaining.
                    _Future source = _chainSource;
                    if (!source._isComplete) {
                        source._prependListeners(listeners);
                        return;
                    }

                    _cloneResult(source);
                }

                D.assert(_isComplete);
                listeners = _reverseListeners(listeners);
                _zone.scheduleMicrotask(() => {
                    _propagateToListeners(this, listeners);
                    return null;
                });
            }
        }

        _FutureListener _removeListeners() {
            // Reverse listeners before returning them, so the resulting list is in
            // subscription order.
            D.assert(!_isComplete);
            _FutureListener current = (_FutureListener) _resultOrListeners;
            _resultOrListeners = null;
            return _reverseListeners(current);
        }

        _FutureListener _reverseListeners(_FutureListener listeners) {
            _FutureListener prev = null;
            _FutureListener current = listeners;
            while (current != null) {
                _FutureListener next = current._nextListener;
                current._nextListener = prev;
                prev = current;
                current = next;
            }

            return prev;
        }

        static void _chainForeignFuture(Future source, _Future target) {
            D.assert(!target._isComplete);
            D.assert(!(source is _Future));

            // Mark the target as chained (and as such half-completed).
            target._setPendingComplete();
            try {
                source.then((value) => {
                        D.assert(target._isPendingComplete);
                        // The "value" may be another future if the foreign future
                        // implementation is mis-behaving,
                        // so use _complete instead of _completeWithValue.
                        target._clearPendingComplete(); // Clear this first, it's set again.
                        target._complete(FutureOr.value(value));
                        return new FutureOr();
                    },
                    onError: (Exception error) => {
                        D.assert(target._isPendingComplete);
                        target._completeError(error);
                        return new FutureOr();
                    });
            }
            catch (Exception e) {
                // This only happens if the `then` call threw synchronously when given
                // valid arguments.
                // That requires a non-conforming implementation of the Future interface,
                // which should, hopefully, never happen.
                async_.scheduleMicrotask(() => {
                    target._completeError(e);
                    return null;
                });
            }
        }

        static void _chainCoreFuture(_Future source, _Future target) {
            D.assert(target._mayAddListener); // Not completed, not already chained.
            while (source._isChained) {
                source = source._chainSource;
            }

            if (source._isComplete) {
                _FutureListener listeners = target._removeListeners();
                target._cloneResult(source);
                _propagateToListeners(target, listeners);
            }
            else {
                _FutureListener listeners = (_FutureListener) target._resultOrListeners;
                target._setChained(source);
                source._prependListeners(listeners);
            }
        }

        internal void _complete(FutureOr value = default) {
            D.assert(!_isComplete);
            if (value.isFuture) {
                if (value.f is _Future coreFuture) {
                    _chainCoreFuture(coreFuture, this);
                }
                else {
                    _chainForeignFuture(value.f, this);
                }
            }
            else {
                _FutureListener listeners = _removeListeners();
                _setValue(value.v);
                _propagateToListeners(this, listeners);
            }
        }

        internal void _completeWithValue(object value) {
            D.assert(!_isComplete);

            _FutureListener listeners = _removeListeners();
            _setValue(value);
            _propagateToListeners(this, listeners);
        }

        internal void _completeError(Exception error) {
            D.assert(!_isComplete);

            _FutureListener listeners = _removeListeners();
            _setError(error);
            _propagateToListeners(this, listeners);
        }

        internal void _asyncComplete(FutureOr value) {
            D.assert(!_isComplete);
            // Two corner cases if the value is a future:
            //   1. the future is already completed and an error.
            //   2. the future is not yet completed but might become an error.
            // The first case means that we must not immediately complete the Future,
            // as our code would immediately start propagating the error without
            // giving the time to install error-handlers.
            // However the second case requires us to deal with the value immediately.
            // Otherwise the value could complete with an error and report an
            // unhandled error, even though we know we are already going to listen to
            // it.

            if (value.isFuture) {
                _chainFuture(value.f);
                return;
            }

            _setPendingComplete();
            _zone.scheduleMicrotask(() => {
                _completeWithValue(value.v);
                return null;
            });
        }

        internal void _chainFuture(Future value) {
            if (value is _Future future) {
                if (future._hasError) {
                    // Delay completion to allow the user to register callbacks.
                    _setPendingComplete();
                    _zone.scheduleMicrotask(() => {
                        _chainCoreFuture(future, this);
                        return null;
                    });
                }
                else {
                    _chainCoreFuture(future, this);
                }

                return;
            }

            // Just listen on the foreign future. This guarantees an async delay.
            _chainForeignFuture(value, this);
        }


        internal void _asyncCompleteError(Exception error) {
            D.assert(!_isComplete);

            _setPendingComplete();
            _zone.scheduleMicrotask(() => {
                _completeError(error);
                return null;
            });
        }


        static void _propagateToListeners(_Future source, _FutureListener listeners) {
            while (true) {
                D.assert(source._isComplete);
                bool hasError = source._hasError;
                if (listeners == null) {
                    if (hasError) {
                        AsyncError asyncError = source._error;
                        source._zone.handleUncaughtError(asyncError);
                    }

                    return;
                }

                // Usually futures only have one listener. If they have several, we
                // call handle them separately in recursive calls, continuing
                // here only when there is only one listener left.
                while (listeners._nextListener != null) {
                    _FutureListener currentListener = listeners;
                    listeners = currentListener._nextListener;
                    currentListener._nextListener = null;
                    _propagateToListeners(source, currentListener);
                }

                _FutureListener listener = listeners;
                var sourceResult = source._resultOrListeners;

                // Do the actual propagation.
                // Set initial state of listenerHasError and listenerValueOrError. These
                // variables are updated with the outcome of potential callbacks.
                // Non-error results, including futures, are stored in
                // listenerValueOrError and listenerHasError is set to false. Errors
                // are stored in listenerValueOrError as an [AsyncError] and
                // listenerHasError is set to true.
                bool listenerHasError = hasError;
                var listenerValueOrError = sourceResult;

                // Only if we either have an error or callbacks, go into this, somewhat
                // expensive, branch. Here we'll enter/leave the zone. Many futures
                // don't have callbacks, so this is a significant optimization.
                if (hasError || listener.handlesValue || listener.handlesComplete) {
                    Zone zone = listener._zone;
                    if (hasError && !source._zone.inSameErrorZone(zone)) {
                        // Don't cross zone boundaries with errors.
                        AsyncError asyncError = source._error;
                        source._zone.handleUncaughtError(asyncError);
                        return;
                    }

                    Zone oldZone = null;
                    if (!ReferenceEquals(Zone.current, zone)) {
                        // Change zone if it's not current.
                        oldZone = Zone._enter(zone);
                    }

                    // These callbacks are abstracted to isolate the try/catch blocks
                    // from the rest of the code to work around a V8 glass jaw.
                    Action handleWhenCompleteCallback = () => {
                        // The whenComplete-handler is not combined with normal value/error
                        // handling. This means at most one handleX method is called per
                        // listener.
                        D.assert(!listener.handlesValue);
                        D.assert(!listener.handlesError);
                        FutureOr completeResult;
                        try {
                            completeResult = listener.handleWhenComplete();
                        }
                        catch (Exception e) {
                            if (hasError && ReferenceEquals(source._error.InnerException, e)) {
                                listenerValueOrError = source._error;
                            }
                            else {
                                listenerValueOrError = new AsyncError(e);
                            }

                            listenerHasError = true;
                            return;
                        }

                        if (completeResult.isFuture) {
                            var completeResultFuture = completeResult.f;
                            if (completeResultFuture is _Future completeResultCoreFuture &&
                                completeResultCoreFuture._isComplete) {
                                if (completeResultCoreFuture._hasError) {
                                    listenerValueOrError = completeResultCoreFuture._error;
                                    listenerHasError = true;
                                }

                                // Otherwise use the existing result of source.
                                return;
                            }

                            // We have to wait for the completeResult future to complete
                            // before knowing if it's an error or we should use the result
                            // of source.
                            var originalSource = source;
                            listenerValueOrError =
                                completeResultFuture.then((_) => FutureOr.future(originalSource));
                            listenerHasError = false;
                        }
                    };

                    Action handleValueCallback = () => {
                        try {
                            listenerValueOrError = listener.handleValue(sourceResult);
                        }
                        catch (Exception e) {
                            listenerValueOrError = new AsyncError(e);
                            listenerHasError = true;
                        }
                    };

                    Action handleError = () => {
                        try {
                            AsyncError asyncError = source._error;
                            if (listener.matchesErrorTest(asyncError) &&
                                listener.hasErrorCallback) {
                                listenerValueOrError = listener.handleError(asyncError);
                                listenerHasError = false;
                            }
                        }
                        catch (Exception e) {
                            if (ReferenceEquals(source._error.InnerException, e)) {
                                listenerValueOrError = source._error;
                            }
                            else {
                                listenerValueOrError = new AsyncError(e);
                            }

                            listenerHasError = true;
                        }
                    };

                    if (listener.handlesComplete) {
                        handleWhenCompleteCallback();
                    }
                    else if (!hasError) {
                        if (listener.handlesValue) {
                            handleValueCallback();
                        }
                    }
                    else {
                        if (listener.handlesError) {
                            handleError();
                        }
                    }

                    // If we changed zone, oldZone will not be null.
                    if (oldZone != null) Zone._leave(oldZone);

                    if (listenerValueOrError is FutureOr futureOr) {
                        listenerValueOrError = futureOr.isFuture ? futureOr.f : futureOr.v;
                    }

                    // If the listener's value is a future we need to chain it. Note that
                    // this can only happen if there is a callback.
                    if (listenerValueOrError is Future chainSource) {
                        // Shortcut if the chain-source is already completed. Just continue
                        // the loop.
                        _Future listenerResult = listener.result;
                        if (chainSource is _Future chainSourceCore) {
                            if (chainSourceCore._isComplete) {
                                listeners = listenerResult._removeListeners();
                                listenerResult._cloneResult(chainSourceCore);
                                source = chainSourceCore;
                                continue;
                            }
                            else {
                                _chainCoreFuture(chainSourceCore, listenerResult);
                            }
                        }
                        else {
                            _chainForeignFuture(chainSource, listenerResult);
                        }

                        return;
                    }
                }

                _Future result = listener.result;
                listeners = result._removeListeners();
                if (!listenerHasError) {
                    result._setValue(listenerValueOrError);
                }
                else {
                    AsyncError asyncError = (AsyncError) listenerValueOrError;
                    result._setErrorObject(asyncError);
                }

                // Prepare for next round.
                source = result;
            }
        }


        public override Future timeout(TimeSpan timeLimit, Func<FutureOr> onTimeout = null) {
            if (_isComplete) return immediate(this);

            _Future result = new _Future();
            Timer timer;
            if (onTimeout == null) {
                timer = Timer.create(timeLimit, () => {
                    result._completeError(
                        new TimeoutException("Future not completed", timeLimit));
                    return null;
                });
            }
            else {
                Zone zone = Zone.current;
                onTimeout = async_._registerHandler(onTimeout, zone);

                timer = Timer.create(timeLimit, () => {
                    try {
                        result._complete((FutureOr) zone.run(() => onTimeout()));
                    }
                    catch (Exception e) {
                        result._completeError(e);
                    }

                    return null;
                });
            }

            then(v => {
                if (timer.isActive) {
                    timer.cancel();
                    result._completeWithValue(v);
                }

                return FutureOr.nil;
            }, onError: e => {
                if (timer.isActive) {
                    timer.cancel();
                    result._completeError(e);
                }

                return FutureOr.nil;
            });
            return result;
        }
    }

    public static partial class async_ {
        internal static Func<object> _registerHandler(Func<object> handler, Zone zone) {
            var callback = zone.registerCallback(() => handler());
            return () => callback();
        }

        internal static Func<FutureOr> _registerHandler(Func<FutureOr> handler, Zone zone) {
            var callback = zone.registerCallback(() => handler());
            return () => (FutureOr) callback();
        }

        internal static Func<object, FutureOr> _registerUnaryHandler(Func<object, FutureOr> handler, Zone zone) {
            var callback = zone.registerUnaryCallback(arg => handler(arg));
            return arg => (FutureOr) callback(arg);
        }

        internal static Func<Exception, bool> _registerUnaryHandler(Func<Exception, bool> handler, Zone zone) {
            var callback = zone.registerUnaryCallback(arg => handler((Exception) arg));
            return arg => (bool) callback(arg);
        }

        internal static Func<Exception, FutureOr> _registerErrorHandler(Func<Exception, FutureOr> errorHandler,
            Zone zone) {
            var callback = zone.registerUnaryCallback(arg => errorHandler((Exception) arg));
            return arg => (FutureOr) callback(arg);
        }
    }
}