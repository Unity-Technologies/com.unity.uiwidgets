using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.async {
    static partial class _stream {
        internal delegate void _DataHandler<T>(T value);

        internal delegate void _DoneHandler();

        internal static void _nullDataHandler<T>(T obj) {
        }

        internal static void _nullErrorHandler(Exception error) {
            Zone.current.handleUncaughtError(error);
        }

        internal static void _nullDoneHandler() {
        }

        internal delegate _PendingEvents<T> _EventGenerator<T>();

        internal delegate void _BroadcastCallback<T>(StreamSubscription<T> subscription);
    }

    abstract class _StreamImpl<T> : Stream<T> {
        // ------------------------------------------------------------------
        // Stream interface.

        public override StreamSubscription<T> listen(
            Action<T> onData, Action<object, string> onError = null, Action onDone = null, bool cancelOnError = false) {
            // void onData(T data),
            // {Function onError, void onDone(), bool cancelOnError}) {
            cancelOnError = Equals(true, cancelOnError);
            StreamSubscription<T> subscription =
                _createSubscription(onData, onError, onDone, cancelOnError);
            _onListen(subscription);
            return subscription;
        }

        // -------------------------------------------------------------------
        /** Create a subscription object. Called by [subcribe]. */
        internal virtual StreamSubscription<T> _createSubscription(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError) {
            return new _BufferingStreamSubscription<T>(
                onData, onError, onDone, cancelOnError);
        }

        /** Hook called when the subscription has been created. */
        void _onListen(StreamSubscription<T> subscription) {
        }
    }

    class _GeneratedStreamImpl<T> : _StreamImpl<T> {
        readonly _stream._EventGenerator<T> _pending;
        bool _isUsed = false;

        internal _GeneratedStreamImpl(_stream._EventGenerator<T> _pending) {
            this._pending = _pending;
        }

        internal override StreamSubscription<T> _createSubscription(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError) {
            if (_isUsed) throw new Exception("Stream has already been listened to.");
            _isUsed = true;
            var result = new _BufferingStreamSubscription<T>(
                onData, onError, onDone, cancelOnError);
            result._setPendingEvents(_pending());
            return result;
        }
    }

    class _IterablePendingEvents<T> : _PendingEvents<T> {
        IEnumerator<T> _iterator;

        internal _IterablePendingEvents(IEnumerable<T> data) {
            _iterator = data.GetEnumerator();
        }

        public override bool isEmpty {
            get { return _iterator == null; }
        }

        public override void handleNext(_EventDispatch<T> dispatch) {
            if (_iterator == null) {
                throw new Exception("No events pending.");
            }

            bool? hasMore = null;
            try {
                hasMore = _iterator.MoveNext();
                if (hasMore ?? false) {
                    dispatch._sendData(_iterator.Current);
                }
                else {
                    _iterator = null;
                    dispatch._sendDone();
                }
            }
            catch (Exception e) {
                if (hasMore == null) {
                    // Threw in .moveNext().
                    // Ensure that we send a done afterwards.
                    _iterator = Enumerable.Empty<T>().GetEnumerator(); // new EmptyIterator<Null>();
                    dispatch._sendError(e, e.StackTrace);
                }
                else {
                    // Threw in .current.
                    dispatch._sendError(e, e.StackTrace);
                }
            }
        }

        public override void clear() {
            if (isScheduled) cancelSchedule();
            _iterator = null;
        }
    }


    abstract class _DelayedEvent<T> {
        /** Added as a linked list on the [StreamController]. */
        internal virtual _DelayedEvent<T> next { get; set; }

        /** Execute the delayed event on the [StreamController]. */
        public abstract void perform(_EventDispatch<T> dispatch);
    }

    class _DelayedData<T> : _DelayedEvent<T> {
        readonly T value;

        internal _DelayedData(T value) {
            this.value = value;
        }

        public override void perform(_EventDispatch<T> dispatch) {
            dispatch._sendData(value);
        }
    }

    /** A delayed error event. */
    class _DelayedError<T> : _DelayedEvent<T> {
        readonly Exception error;
        readonly string stackTrace;

        internal _DelayedError(Exception error, string stackTrace) {
            this.error = error;
            this.stackTrace = stackTrace;
        }

        public override void perform(_EventDispatch<T> dispatch) {
            dispatch._sendError(error, stackTrace);
        }
    }

    class _DelayedDone<T> : _DelayedEvent<T> {
        internal _DelayedDone() {
        }

        public override void perform(_EventDispatch<T> dispatch) {
            dispatch._sendDone();
        }

        internal override _DelayedEvent<T> next {
            get { return null; }
            set { throw new Exception("No events after a done."); }
        }
    }

    interface _EventSink<T> {
        void _add(T data);
        void _addError(object error, string stackTrace);
        void _close();
    }

    interface _EventDispatch<T> {
        void _sendData(T data);
        void _sendError(object error, string stackTrace);
        void _sendDone();
    }

    class _BufferingStreamSubscription<T>
        : StreamSubscription<T>, _EventSink<T>, _EventDispatch<T> {
        /** The `cancelOnError` flag from the `listen` call. */
        const int _STATE_CANCEL_ON_ERROR = 1;

        const int _STATE_CLOSED = 2;
        const int _STATE_INPUT_PAUSED = 4;
        const int _STATE_CANCELED = 8;
        const int _STATE_WAIT_FOR_CANCEL = 16;
        const int _STATE_IN_CALLBACK = 32;
        const int _STATE_HAS_PENDING = 64;
        const int _STATE_PAUSE_COUNT = 128;

        //@pagma("vm:entry-point")
        _stream._DataHandler<T> _onData;
        Action<Exception, string> _onError;
        _stream._DoneHandler _onDone;
        readonly Zone _zone = Zone.current;

        /** Bit vector based on state-constants above. */
        int _state;

        // TODO(floitsch): reuse another field
        /** The future [_onCancel] may return. */
        Future _cancelFuture;

        _PendingEvents<T> _pending;

        internal _BufferingStreamSubscription(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError) {
            _state = (cancelOnError ? _STATE_CANCEL_ON_ERROR : 0);
            this.onData(onData);
            this.onError(onError);
            this.onDone(onDone);
        }

        internal void _setPendingEvents(_PendingEvents<T> pendingEvents) {
            D.assert(_pending == null);
            if (pendingEvents == null) return;
            _pending = pendingEvents;
            if (!pendingEvents.isEmpty) {
                _state |= _STATE_HAS_PENDING;
                _pending.schedule(this);
            }
        }

        // StreamSubscription interface.

        public override void onData(Action<T> handleData) {
            handleData = handleData ?? _stream._nullDataHandler;
            // TODO(floitsch): the return type should be 'void', and the type
            // should be inferred.
            _onData = d => {
                _zone.registerUnaryCallback(data => 
                {
                    handleData((T) data);
                    return default;
                }).Invoke(d);
            };
        }

        // Siyao: c# does not support convert action
        public override void onError(Action<object, string> handleError) {
            handleError = handleError ?? ((input1, input2) => _stream._nullErrorHandler(null));

            _onError = (arg1, arg2) => _zone
                .registerBinaryCallback((in1, in2) => {
                    handleError(in1, (string) in2);
                    return null;
                }).Invoke(arg1, arg2);
        }

        public override void onDone(Action handleDone) {
            handleDone = handleDone ?? _stream._nullDoneHandler;
            _onDone = () => _zone.registerCallback(() => {
                handleDone();
                return null;
            }).Invoke();
        }

        public override void pause(Future resumeSignal) {
            if (_isCanceled) return;
            bool wasPaused = _isPaused;
            bool wasInputPaused = _isInputPaused;
            // Increment pause count and mark input paused (if it isn't already).
            _state = (_state + _STATE_PAUSE_COUNT) | _STATE_INPUT_PAUSED;
            if (resumeSignal != null) resumeSignal.whenComplete(resume);
            if (!wasPaused && _pending != null) _pending.cancelSchedule();
            if (!wasInputPaused && !_inCallback) _guardCallback(_onPause);
        }

        public override void resume() {
            if (_isCanceled) return;
            if (_isPaused) {
                _decrementPauseCount();
                if (!_isPaused) {
                    if (_hasPending && !_pending.isEmpty) {
                        // Input is still paused.
                        _pending.schedule(this);
                    }
                    else {
                        D.assert(_mayResumeInput);
                        _state &= ~_STATE_INPUT_PAUSED;
                        if (!_inCallback) _guardCallback(_onResume);
                    }
                }
            }
        }

        public override Future cancel() {
            // The user doesn't want to receive any further events. If there is an
            // error or done event pending (waiting for the cancel to be done) discard
            // that event.
            _state &= ~_STATE_WAIT_FOR_CANCEL;
            if (!_isCanceled) {
                _cancel();
            }

            return _cancelFuture ?? Future._nullFuture;
        }

        public override Future<E> asFuture<E>(E futureValue) {
            _Future result = new _Future();

            // Overwrite the onDone and onError handlers.
            _onDone = () => { result._complete(FutureOr.value(futureValue)); };
            _onError = (error, stackTrace) => {
                Future cancelFuture = cancel();
                if (!Equals(cancelFuture, Future._nullFuture)) {
                    cancelFuture.whenComplete(() => { result._completeError(error); });
                }
                else {
                    result._completeError(error);
                }
            };

            return result.to<E>();
        }

        // State management.

        internal bool _isInputPaused {
            get => (_state & _STATE_INPUT_PAUSED) != 0;
        }

        internal bool _isClosed {
            get => (_state & _STATE_CLOSED) != 0;
        }

        internal bool _isCanceled {
            get => (_state & _STATE_CANCELED) != 0;
        }

        internal bool _waitsForCancel {
            get => (_state & _STATE_WAIT_FOR_CANCEL) != 0;
        }

        internal bool _inCallback {
            get => (_state & _STATE_IN_CALLBACK) != 0;
        }

        internal bool _hasPending {
            get => (_state & _STATE_HAS_PENDING) != 0;
        }

        internal bool _isPaused {
            get => _state >= _STATE_PAUSE_COUNT;
        }

        internal bool _canFire {
            get => _state < _STATE_IN_CALLBACK;
        }

        internal bool _mayResumeInput {
            get =>
                !_isPaused && (_pending == null || _pending.isEmpty);
        }

        internal bool _cancelOnError {
            get => (_state & _STATE_CANCEL_ON_ERROR) != 0;
        }

        public override bool isPaused {
            get => _isPaused;
        }

        void _cancel() {
            _state |= _STATE_CANCELED;
            if (_hasPending) {
                _pending.cancelSchedule();
            }

            if (!_inCallback) _pending = null;
            _cancelFuture = _onCancel();
        }

        void _decrementPauseCount() {
            D.assert(_isPaused);
            _state -= _STATE_PAUSE_COUNT;
        }

        // _EventSink interface.

        public virtual void _add(T data) {
            D.assert(!_isClosed);
            if (_isCanceled) return;
            if (_canFire) {
                _sendData(data);
            }
            else {
                _addPending(new _DelayedData<T>(data));
            }
        }

        public virtual void _addError(object error, string stackTrace) {
            if (_isCanceled) return;
            if (_canFire) {
                _sendError(error, stackTrace); // Reports cancel after sending.
            }
            else {
                _addPending(new _DelayedError<T>((Exception) error, stackTrace));
            }
        }

        public virtual void _close() {
            D.assert(!_isClosed);
            if (_isCanceled) return;
            _state |= _STATE_CLOSED;
            if (_canFire) {
                _sendDone();
            }
            else {
                _addPending(new _DelayedDone<T>());
            }
        }

        // Hooks called when the input is paused, unpaused or canceled.
        // These must not throw. If overwritten to call user code, include suitable
        // try/catch wrapping and send any errors to
        // [_Zone.current.handleUncaughtError].
        protected virtual void _onPause() {
            D.assert(_isInputPaused);
        }

        protected virtual void _onResume() {
            D.assert(!_isInputPaused);
        }

        protected virtual Future _onCancel() {
            D.assert(_isCanceled);
            return null;
        }

        // Handle pending events.

        internal void _addPending(_DelayedEvent<T> evt) {
            _StreamImplEvents<T> pending = _pending as _StreamImplEvents<T>;
            if (_pending == null) {
                pending = (_StreamImplEvents<T>) (_pending = new _StreamImplEvents<T>());
            }

            pending.add(evt);
            if (!_hasPending) {
                _state |= _STATE_HAS_PENDING;
                if (!_isPaused) {
                    _pending.schedule(this);
                }
            }
        }

        public virtual void _sendData(T data) {
            D.assert(!_isCanceled);
            D.assert(!_isPaused);
            D.assert(!_inCallback);
            bool wasInputPaused = _isInputPaused;
            _state |= _STATE_IN_CALLBACK;
            _zone.runUnaryGuarded(data1 => {
                _onData((T) data1);
                return null;
            }, data);
            _state &= ~_STATE_IN_CALLBACK;
            _checkState(wasInputPaused);
        }

        public virtual void _sendError(object error, string stackTrace) {
            D.assert(!_isCanceled);
            D.assert(!_isPaused);
            D.assert(!_inCallback);
            bool wasInputPaused = _isInputPaused;

            void sendError() {
                // If the subscription has been canceled while waiting for the cancel
                // future to finish we must not report the error.
                if (_isCanceled && !_waitsForCancel) return;
                _state |= _STATE_IN_CALLBACK;
                // TODO(floitsch): this dynamic should be 'void'.
                var onError = _onError;
                if (onError != null) {
                    _zone.runBinaryGuarded((error1, stack) => {
                        onError((Exception) error1, (string) stack);
                        return null;
                    }, error, stackTrace);
                }
                else {
                    // Siyao: c# could not cast Action
                    D.assert(_onError != null);
                    // _zone.runUnaryGuarded(error => _onError, error);
                }

                _state &= ~_STATE_IN_CALLBACK;
            }

            if (_cancelOnError) {
                _state |= _STATE_WAIT_FOR_CANCEL;
                _cancel();
                if (_cancelFuture != null &&
                    !Equals(_cancelFuture, Future._nullFuture)) {
                    _cancelFuture.whenComplete(sendError);
                }
                else {
                    sendError();
                }
            }
            else {
                sendError();
                // Only check state if not cancelOnError.
                _checkState(wasInputPaused);
            }
        }

        public void _sendDone() {
            D.assert(!_isCanceled);
            D.assert(!_isPaused);
            D.assert(!_inCallback);

            void sendDone() {
                // If the subscription has been canceled while waiting for the cancel
                // future to finish we must not report the done event.
                if (!_waitsForCancel) return;
                _state |= (_STATE_CANCELED | _STATE_CLOSED | _STATE_IN_CALLBACK);
                _zone.runGuarded(() => {
                    _onDone();
                    return null;
                });
                _state &= ~_STATE_IN_CALLBACK;
            }

            _cancel();
            _state |= _STATE_WAIT_FOR_CANCEL;
            if (_cancelFuture != null &&
                !Equals(_cancelFuture, Future._nullFuture)) {
                _cancelFuture.whenComplete(sendDone);
            }
            else {
                sendDone();
            }
        }

        internal void _guardCallback(Action callback) {
            D.assert(!_inCallback);
            bool wasInputPaused = _isInputPaused;
            _state |= _STATE_IN_CALLBACK;
            callback();
            _state &= ~_STATE_IN_CALLBACK;
            _checkState(wasInputPaused);
        }

        void _checkState(bool wasInputPaused) {
            D.assert(!_inCallback);
            if (_hasPending && _pending.isEmpty) {
                _state &= ~_STATE_HAS_PENDING;
                if (_isInputPaused && _mayResumeInput) {
                    _state &= ~_STATE_INPUT_PAUSED;
                }
            }

            // If the state changes during a callback, we immediately
            // make a new state-change callback. Loop until the state didn't change.
            while (true) {
                if (_isCanceled) {
                    _pending = null;
                    return;
                }

                bool isInputPaused = _isInputPaused;
                if (wasInputPaused == isInputPaused) break;
                _state ^= _STATE_IN_CALLBACK;
                if (isInputPaused) {
                    _onPause();
                }
                else {
                    _onResume();
                }

                _state &= ~_STATE_IN_CALLBACK;
                wasInputPaused = isInputPaused;
            }

            if (_hasPending && !_isPaused) {
                _pending.schedule(this);
            }
        }
    }

    abstract class _PendingEvents<T> {
        // No async event has been scheduled.
        const int _STATE_UNSCHEDULED = 0;

        // An async event has been scheduled to run a function.
        const int _STATE_SCHEDULED = 1;

        // An async event has been scheduled, but it will do nothing when it runs.
        // Async events can't be preempted.
        const int _STATE_CANCELED = 3;

        /**
   * State of being scheduled.
   *
   * Set to [_STATE_SCHEDULED] when pending events are scheduled for
   * async dispatch. Since we can't cancel a [scheduleMicrotask] call, if
   * scheduling is "canceled", the _state is simply set to [_STATE_CANCELED]
   * which will make the async code do nothing except resetting [_state].
   *
   * If events are scheduled while the state is [_STATE_CANCELED], it is
   * merely switched back to [_STATE_SCHEDULED], but no new call to
   * [scheduleMicrotask] is performed.
   */
        int _state = _STATE_UNSCHEDULED;

        public virtual bool isEmpty { get; }

        public bool isScheduled {
            get => _state == _STATE_SCHEDULED;
        }

        public bool _eventScheduled {
            get => _state >= _STATE_SCHEDULED;
        }

        /**
   * Schedule an event to run later.
   *
   * If called more than once, it should be called with the same dispatch as
   * argument each time. It may reuse an earlier argument in some cases.
   */
        public void schedule(_EventDispatch<T> dispatch) {
            if (isScheduled) return;
            D.assert(!isEmpty);
            if (_eventScheduled) {
                D.assert(_state == _STATE_CANCELED);
                _state = _STATE_SCHEDULED;
                return;
            }

            async_.scheduleMicrotask(() => {
                int oldState = _state;
                _state = _STATE_UNSCHEDULED;
                if (oldState == _STATE_CANCELED) return null;
                handleNext(dispatch);
                return null;
            });
            _state = _STATE_SCHEDULED;
        }

        public void cancelSchedule() {
            if (isScheduled) _state = _STATE_CANCELED;
        }

        public abstract void handleNext(_EventDispatch<T> dispatch);

        /** Throw away any pending events and cancel scheduled events. */
        public abstract void clear();
    }

    class _StreamImplEvents<T> : _PendingEvents<T> {
        /// Single linked list of [_DelayedEvent] objects.
        _DelayedEvent<T> firstPendingEvent;

        /// Last element in the list of pending events. New events are added after it.
        _DelayedEvent<T> lastPendingEvent;

        public override bool isEmpty {
            get { return lastPendingEvent == null; }
        }

        internal void add(_DelayedEvent<T> evt) {
            if (lastPendingEvent == null) {
                firstPendingEvent = lastPendingEvent = evt;
            }
            else {
                lastPendingEvent = lastPendingEvent.next = evt;
            }
        }

        public override void handleNext(_EventDispatch<T> dispatch) {
            D.assert(!isScheduled);
            _DelayedEvent<T> evt = firstPendingEvent;
            firstPendingEvent = evt.next;
            if (firstPendingEvent == null) {
                lastPendingEvent = null;
            }

            evt.perform((_EventDispatch<T>) dispatch);
        }

        public override void clear() {
            if (isScheduled) cancelSchedule();
            firstPendingEvent = lastPendingEvent = null;
        }
    }

    class _DoneStreamSubscription<T> : StreamSubscription<T> {
        internal const int _DONE_SENT = 1;
        internal const int _SCHEDULED = 2;
        internal const int _PAUSED = 4;

        readonly Zone _zone;
        int _state = 0;
        _stream._DoneHandler _onDone;

        internal _DoneStreamSubscription(_stream._DoneHandler _onDone) {
            _zone = Zone.current;
            this._onDone = _onDone;
            _schedule();
        }

        bool _isSent {
            get => (_state & _DONE_SENT) != 0;
        }

        bool _isScheduled {
            get => (_state & _SCHEDULED) != 0;
        }

        public override bool isPaused {
            get => _state >= _PAUSED;
        }

        void _schedule() {
            if (_isScheduled) return;
            _zone.scheduleMicrotask(() => {
                _sendDone();
                return null;
            });
            _state |= _SCHEDULED;
        }

        public override void onData(Action<T> handleData) {
        }

        public override void onError(Action<object, string> action) {
        }

        public override void onDone(Action handleDone) {
            _onDone = () => handleDone();
        }

        public override void pause(Future resumeSignal = null) {
            _state += _PAUSED;
            if (resumeSignal != null) resumeSignal.whenComplete(resume);
        }

        public override void resume() {
            if (isPaused) {
                _state -= _PAUSED;
                if (!isPaused && !_isSent) {
                    _schedule();
                }
            }
        }

        public override Future cancel() => Future._nullFuture;

        public override Future<E> asFuture<E>(E futureValue) {
            _Future result = new _Future();
            _onDone = () => { result._completeWithValue(futureValue); };
            return result.to<E>();
        }

        void _sendDone() {
            _state &= ~_SCHEDULED;
            if (isPaused) return;
            _state |= _DONE_SENT;
            if (_onDone != null) _zone.runGuarded(() => _onDone);
        }
    }


    class _AsBroadcastStream<T> : Stream<T> {
        readonly Stream<T> _source;
        readonly _stream._BroadcastCallback<T> _onListenHandler;
        readonly _stream._BroadcastCallback<T> _onCancelHandler;
        readonly Zone _zone;

        _AsBroadcastStreamController<T> _controller;
        StreamSubscription<T> _subscription;

        internal _AsBroadcastStream(
                Stream<T> _source,
                Action<StreamSubscription<T>> onListenHandler,
                Action<StreamSubscription<T>> onCancelHandler)
            // TODO(floitsch): the return type should be void and should be
            // inferred.
        {
            this._source = _source;
            _onListenHandler = a => Zone.current
                .registerUnaryCallback(
                    b => {
                        onListenHandler?.Invoke((StreamSubscription<T>) b);
                        return default;
                    }
                )(a);
            _onCancelHandler = d => Zone.current
                .registerUnaryCallback(
                    c => {
                        onCancelHandler?.Invoke((StreamSubscription<T>) c);
                        return default;
                    })(d);
            _zone = Zone.current;
            _controller = new _AsBroadcastStreamController<T>(_onListen, _onCancel);
        }

        public override bool isBroadcast {
            get { return true; }
        }


        public override StreamSubscription<T> listen(Action<T> onData, Action<object, string> onError = null,
            Action onDone = null, bool cancelOnError = false) {
            if (_controller == null || _controller.isClosed) {
                // Return a dummy subscription backed by nothing, since
                // it will only ever send one done event.
                return new _DoneStreamSubscription<T>(() => onDone());
            }

            _subscription = _subscription ?? _source.listen(_controller.add,
                onError: _controller.addError, onDone: () => _controller.close());
            cancelOnError = Equals(true, cancelOnError);
            return _controller._subscribe(onData, onError, onDone, cancelOnError);
        }

        void _onCancel() {
            bool shutdown = (_controller == null) || _controller.isClosed;
            if (_onCancelHandler != null) {
                _zone.runUnary(
                    a => {
                        _onCancelHandler((StreamSubscription<T>) a);
                        return default;
                    }, new _BroadcastSubscriptionWrapper<T>(this));
            }

            if (shutdown) {
                if (_subscription != null) {
                    _subscription.cancel();
                    _subscription = null;
                }
            }
        }

        void _onListen() {
            if (_onListenHandler != null) {
                _zone.runUnary(
                    a => {
                        _onListenHandler((StreamSubscription<T>) a);
                        return default;
                    }, new _BroadcastSubscriptionWrapper<T>(this));
            }
        }

        // Methods called from _BroadcastSubscriptionWrapper.
        internal void _cancelSubscription() {
            if (_subscription == null) return;
            // Called by [_controller] when it has no subscribers left.
            StreamSubscription<T> subscription = _subscription;
            _subscription = null;
            _controller = null; // Marks the stream as no longer listenable.
            subscription.cancel();
        }

        internal void _pauseSubscription(Future resumeSignal) {
            if (_subscription == null) return;
            _subscription.pause(resumeSignal);
        }

        internal void _resumeSubscription() {
            if (_subscription == null) return;
            _subscription.resume();
        }

        internal bool _isSubscriptionPaused {
            get {
                if (_subscription == null) return false;
                return _subscription.isPaused;
            }
        }
    }

    class _BroadcastSubscriptionWrapper<T> : StreamSubscription<T> {
        readonly _AsBroadcastStream<T> _stream;

        internal _BroadcastSubscriptionWrapper(_AsBroadcastStream<T> _stream) {
            this._stream = _stream;
        }

        public override void onData(Action<T> handleData) {
            throw new Exception(
                "Cannot change handlers of asBroadcastStream source subscription.");
        }

        public override void onError(Action<object, string> action) {
            throw new Exception(
                "Cannot change handlers of asBroadcastStream source subscription.");
        }

        public override void onDone(Action handleDone) {
            throw new Exception(
                "Cannot change handlers of asBroadcastStream source subscription.");
        }

        public override void pause(Future resumeSignal = null) {
            _stream._pauseSubscription(resumeSignal);
        }

        public override void resume() {
            _stream._resumeSubscription();
        }

        public override Future cancel() {
            _stream._cancelSubscription();
            return Future._nullFuture;
        }

        public override bool isPaused {
            get { return _stream._isSubscriptionPaused; }
        }

        public override Future<E> asFuture<E>(E futureValue) {
            throw new Exception(
                "Cannot change handlers of asBroadcastStream source subscription.");
        }
    }


    internal class _StreamIterator<T> : StreamIterator<T> {
        StreamSubscription<T> _subscription;

        //@pragma("vm:entry-point")
        object _stateData;

        bool _isPaused = false;

        internal _StreamIterator(Stream<T> stream) {
            if (stream != null) {
                _stateData = stream;
            }
            else {
                throw new ArgumentException("not null", "stream");
            }

            // _stateData = stream ?? (throw ArgumentError.notNull("stream"));
        }

        object current {
            get {
                if (_subscription != null && _isPaused) {
                    return _stateData;
                }

                return default;
            }
        }

        public override Future<bool> moveNext() {
            if (_subscription != null) {
                if (_isPaused) {
                    var future = new _Future();
                    _stateData = future;
                    _isPaused = false;
                    _subscription.resume();
                    return future.to<bool>();
                }

                throw new Exception("Already waiting for next.");
            }

            return _initializeOrDone();
        }

        Future<bool> _initializeOrDone() {
            D.assert(_subscription == null);
            var stateData = _stateData;
            if (stateData != null) {
                Stream<T> stream = (Stream<T>) stateData;
                _subscription = stream.listen(_onData,
                    onError: _onError, onDone: _onDone, cancelOnError: true);
                var future = new _Future();
                _stateData = future;
                return future.to<bool>();
            }

            return Future._falseFuture.to<bool>();
        }

        public override Future cancel() {
            StreamSubscription<T> subscription = _subscription;
            object stateData = _stateData;
            _stateData = null;
            if (subscription != null) {
                _subscription = null;
                if (!_isPaused) {
                    _Future future = (_Future) stateData;
                    future._asyncComplete(false);
                }

                return subscription.cancel();
            }

            return Future._nullFuture;
        }

        void _onData(T data) {
            D.assert(_subscription != null && !_isPaused);
            _Future moveNextFuture = (_Future) _stateData;
            _stateData = data;
            _isPaused = true;
            moveNextFuture._complete(true);
            if (_subscription != null && _isPaused) _subscription.pause();
        }

        void _onError(object error, string stackTrace) {
            D.assert(_subscription != null && !_isPaused);
            _Future moveNextFuture = (_Future) _stateData;
            _subscription = null;
            _stateData = null;
            moveNextFuture._completeError((Exception) error);
        }

        void _onDone() {
            D.assert(_subscription != null && !_isPaused);
            _Future moveNextFuture = (_Future) _stateData;
            _subscription = null;
            _stateData = null;
            moveNextFuture._complete(false);
        }
    }

    class _EmptyStream<T> : Stream<T> {
        internal _EmptyStream() : base() {
        }

        public override bool isBroadcast {
            get { return true; }
        }

        public override StreamSubscription<T> listen(Action<T> onData, Action<object, string> onError = null,
            Action onDone = null, bool cancelOnError = false) {
            return new _DoneStreamSubscription<T>(() => onDone());
        }
    }
}