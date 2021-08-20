using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.async {
    class _BroadcastStream<T> : _ControllerStream<T> {
        internal _BroadcastStream(_StreamControllerLifecycle<T> controller)
            : base(controller) {
        }

        public override bool isBroadcast {
            get { return true; }
        }
    }

    class _BroadcastSubscription<T> : _ControllerSubscription<T> {
        const int _STATE_EVENT_ID = 1;
        internal const int _STATE_FIRING = 2;

        const int _STATE_REMOVE_AFTER_FIRING = 4;

        // TODO(lrn): Use the _state field on _ControllerSubscription to
        // also store this state. Requires that the subscription implementation
        // does not assume that it's use of the state integer is the only use.
        internal int _eventState = 0; // Initialized to help dart2js type inference.

        internal _BroadcastSubscription<T> _next;
        internal _BroadcastSubscription<T> _previous;

        internal _BroadcastSubscription(_StreamControllerLifecycle<T> controller,
            Action<T> onData,
            Action<object, string> onError,
            Action onDone, bool cancelOnError
        )
            : base(controller, onData, onError, onDone, cancelOnError) {
            _next = _previous = this;
        }

        internal bool _expectsEvent(int eventId) => (_eventState & _STATE_EVENT_ID) == eventId;

        internal void _toggleEventId() {
            _eventState ^= _STATE_EVENT_ID;
        }

        internal bool _isFiring {
            get { return (_eventState & _STATE_FIRING) != 0; }
        }

        internal void _setRemoveAfterFiring() {
            D.assert(_isFiring);
            _eventState |= _STATE_REMOVE_AFTER_FIRING;
        }

        internal bool _removeAfterFiring {
            get { return (_eventState & _STATE_REMOVE_AFTER_FIRING) != 0; }
        }

        // The controller._recordPause doesn't do anything for a broadcast controller,
        // so we don't bother calling it.
        protected override void _onPause() {
        }

        // The controller._recordResume doesn't do anything for a broadcast
        // controller, so we don't bother calling it.
        protected override void _onResume() {
        }

        // _onCancel is inherited.
    }

    abstract class _BroadcastStreamController<T>
        : _StreamControllerBase<T> {
        const int _STATE_INITIAL = 0;
        const int _STATE_EVENT_ID = 1;
        internal const int _STATE_FIRING = 2;
        protected const int _STATE_CLOSED = 4;
        const int _STATE_ADDSTREAM = 8;

        public override _stream.ControllerCallback onListen { get; set; }
        public override _stream.ControllerCancelCallback onCancel { get; set; }

        // State of the controller.
        internal int _state;

        // Double-linked list of active listeners.
        internal _BroadcastSubscription<T> _firstSubscription;
        _BroadcastSubscription<T> _lastSubscription;

        // Extra state used during an [addStream] call.
        _AddStreamState<T> _addStreamState;

        internal _Future _doneFuture;

        internal _BroadcastStreamController(_stream.ControllerCallback onListen,
            _stream.ControllerCancelCallback onCancel) {
            this.onListen = onListen;
            this.onCancel = onCancel;
            _state = _STATE_INITIAL;
        }

        public override _stream.ControllerCallback onPause {
            get {
                throw new Exception(
                    "Broadcast stream controllers do not support pause callbacks");
            }
            set {
                throw new Exception(
                    "Broadcast stream controllers do not support pause callbacks");
            }
        }

        public override _stream.ControllerCallback onResume {
            get {
                throw new Exception(
                    "Broadcast stream controllers do not support pause callbacks");
            }
            set {
                throw new Exception(
                    "Broadcast stream controllers do not support pause callbacks");
            }
        }
        // StreamController interface.

        public override Stream<T> stream {
            get { return new _BroadcastStream<T>(this); }
        }

        public override StreamSink<T> sink {
            get { return new _StreamSinkWrapper<T>(this); }
        }

        public override bool isClosed {
            get { return (_state & _STATE_CLOSED) != 0; }
        }

        /**
   * A broadcast controller is never paused.
   *
   * Each receiving stream may be paused individually, and they handle their
   * own buffering.
   */
        public override bool isPaused {
            get => false;
        }

        /** Whether there are currently one or more subscribers. */
        public override bool hasListener {
            get => !_isEmpty;
        }

        /**
   * Test whether the stream has exactly one listener.
   *
   * Assumes that the stream has a listener (not [_isEmpty]).
   */
        internal bool _hasOneListener {
            get {
                D.assert(!_isEmpty);
                return Equals(_firstSubscription, _lastSubscription);
            }
        }

        /** Whether an event is being fired (sent to some, but not all, listeners). */
        internal virtual bool _isFiring {
            get => (_state & _STATE_FIRING) != 0;
        }

        internal bool _isAddingStream {
            get => (_state & _STATE_ADDSTREAM) != 0;
        }

        internal virtual bool _mayAddEvent {
            get => (_state < _STATE_CLOSED);
        }

        _Future _ensureDoneFuture() {
            if (_doneFuture != null) return _doneFuture;
            return _doneFuture = new _Future();
        }

        // Linked list helpers

        internal virtual bool _isEmpty {
            get { return _firstSubscription == null; }
        }

        /** Adds subscription to linked list of active listeners. */
        void _addListener(_BroadcastSubscription<T> subscription) {
            D.assert(Equals(subscription._next, subscription));
            subscription._eventState = (_state & _STATE_EVENT_ID);
            // Insert in linked list as last subscription.
            _BroadcastSubscription<T> oldLast = _lastSubscription;
            _lastSubscription = subscription;
            subscription._next = null;
            subscription._previous = oldLast;
            if (oldLast == null) {
                _firstSubscription = subscription;
            }
            else {
                oldLast._next = subscription;
            }
        }

        void _removeListener(_BroadcastSubscription<T> subscription) {
            D.assert(Equals(subscription._controller, this));
            D.assert(!Equals(subscription._next, subscription));
            _BroadcastSubscription<T> previous = subscription._previous;
            _BroadcastSubscription<T> next = subscription._next;
            if (previous == null) {
                // This was the first subscription.
                _firstSubscription = next;
            }
            else {
                previous._next = next;
            }

            if (next == null) {
                // This was the last subscription.
                _lastSubscription = previous;
            }
            else {
                next._previous = previous;
            }

            subscription._next = subscription._previous = subscription;
        }

        // _StreamControllerLifecycle interface.

        public override StreamSubscription<T> _subscribe(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError) {
            if (isClosed) {
                onDone = onDone ?? _stream._nullDoneHandler;
                return new _DoneStreamSubscription<T>(() => onDone());
            }

            StreamSubscription<T> subscription = new _BroadcastSubscription<T>(
                this, onData, onError, onDone, cancelOnError);
            _addListener((_BroadcastSubscription<T>) subscription);
            if (Equals(_firstSubscription, _lastSubscription)) {
                // Only one listener, so it must be the first listener.
                _stream._runGuarded(() => onListen());
            }

            return subscription;
        }

        public override Future _recordCancel(StreamSubscription<T> sub) {
            _BroadcastSubscription<T> subscription = (_BroadcastSubscription<T>) sub;
            // If already removed by the stream, don't remove it again.
            if (Equals(subscription._next, subscription)) return null;
            if (subscription._isFiring) {
                subscription._setRemoveAfterFiring();
            }
            else {
                _removeListener(subscription);
                // If we are currently firing an event, the empty-check is performed at
                // the end of the listener loop instead of here.
                if (!_isFiring && _isEmpty) {
                    _callOnCancel();
                }
            }

            return null;
        }

        public override void _recordPause(StreamSubscription<T> subscription) {
        }

        public override void _recordResume(StreamSubscription<T> subscription) {
        }

        // EventSink interface.

        internal virtual Exception _addEventError() {
            if (isClosed) {
                return new Exception("Cannot add new events after calling close");
            }

            D.assert(_isAddingStream);
            return new Exception("Cannot add new events while doing an addStream");
        }

        public override void add(T data) {
            if (!_mayAddEvent) throw _addEventError();
            _sendData(data);
        }

        public override void addError(object error, string stackTrace) {
            // ArgumentError.checkNotNull(error, "error");
            if (!_mayAddEvent) throw _addEventError();
            AsyncError replacement = Zone.current.errorCallback((Exception) error);
            if (replacement != null) {
                error = _async._nonNullError(replacement);
                stackTrace = replacement.StackTrace;
            }

            stackTrace = stackTrace ?? AsyncError.defaultStackTrace(error);
            _sendError(error, stackTrace);
        }

        public override Future close() {
            if (isClosed) {
                D.assert(_doneFuture != null);
                return _doneFuture;
            }

            if (!_mayAddEvent) throw _addEventError();
            _state |= _STATE_CLOSED;
            Future doneFuture = _ensureDoneFuture();
            _sendDone();
            return doneFuture;
        }

        public override Future done {
            get { return _ensureDoneFuture(); }
        }

        public override Future addStream(Stream<T> stream, bool? cancelOnError = null) {
            if (!_mayAddEvent) throw _addEventError();
            _state |= _STATE_ADDSTREAM;
            _addStreamState = new _AddStreamState<T>(this, stream, cancelOnError ?? false);
            return _addStreamState.addStreamFuture;
        }

        // _EventSink interface, called from AddStreamState.
        public override void _add(T data) {
            _sendData(data);
        }

        public override void _addError(object error, string stackTrace) {
            _sendError(error, stackTrace);
        }

        public override void _close() {
            D.assert(_isAddingStream);
            _AddStreamState<T> addState = _addStreamState;
            _addStreamState = null;
            _state &= ~_STATE_ADDSTREAM;
            addState.complete();
        }

        // Event handling.
        internal void _forEachListener(Action<_BufferingStreamSubscription<T>> action) {
            if (_isFiring) {
                throw new Exception(
                    "Cannot fire new event. Controller is already firing an event");
            }

            if (_isEmpty) return;

            // Get event id of this event.
            int id = (_state & _STATE_EVENT_ID);
            // Start firing (set the _STATE_FIRING bit). We don't do [onCancel]
            // callbacks while firing, and we prevent reentrancy of this function.
            //
            // Set [_state]'s event id to the next event's id.
            // Any listeners added while firing this event will expect the next event,
            // not this one, and won't get notified.
            _state ^= _STATE_EVENT_ID | _STATE_FIRING;
            _BroadcastSubscription<T> subscription = _firstSubscription;
            while (subscription != null) {
                if (subscription._expectsEvent(id)) {
                    subscription._eventState |= _BroadcastSubscription<T>._STATE_FIRING;
                    action(subscription);
                    subscription._toggleEventId();
                    _BroadcastSubscription<T> next = subscription._next;
                    if (subscription._removeAfterFiring) {
                        _removeListener(subscription);
                    }

                    subscription._eventState &= ~_BroadcastSubscription<T>._STATE_FIRING;
                    subscription = next;
                }
                else {
                    subscription = subscription._next;
                }
            }

            _state &= ~_STATE_FIRING;

            if (_isEmpty) {
                _callOnCancel();
            }
        }

        internal virtual void _callOnCancel() {
            D.assert(_isEmpty);
            if (isClosed && _doneFuture._mayComplete) {
                // When closed, _doneFuture is not null.
                _doneFuture._asyncComplete(FutureOr.nil);
            }

            _stream._runGuarded(() => onCancel());
        }
    }

    class _SyncBroadcastStreamController<T> : _BroadcastStreamController<T>
        , SynchronousStreamController<T> {
        internal _SyncBroadcastStreamController(
            _stream.ControllerCallback onListen, Action onCancel)
            : base(onListen, () => {
                onCancel();
                return Future._nullFuture;
            }) {
        }

        // EventDispatch interface.

        internal override bool _mayAddEvent {
            get { return base._mayAddEvent && !_isFiring; }
        }

        internal override Exception _addEventError() {
            if (_isFiring) {
                return new Exception(
                    "Cannot fire new event. Controller is already firing an event");
            }

            return base._addEventError();
        }

        public override void _sendData(T data) {
            if (_isEmpty) return;
            if (_hasOneListener) {
                _state |= _BroadcastStreamController<T>._STATE_FIRING;
                _BroadcastSubscription<T> subscription = _firstSubscription;
                subscription._add(data);
                _state &= ~_BroadcastStreamController<T>._STATE_FIRING;
                if (_isEmpty) {
                    _callOnCancel();
                }

                return;
            }

            _forEachListener((_BufferingStreamSubscription<T> subscription) => { subscription._add(data); });
        }

        public override void _sendError(object error, string stackTrace) {
            if (_isEmpty) return;
            _forEachListener((_BufferingStreamSubscription<T> subscription) => {
                subscription._addError(error, stackTrace);
            });
        }

        public override void _sendDone() {
            if (!_isEmpty) {
                _forEachListener((_BufferingStreamSubscription<T> subscription) => { subscription._close(); });
            }
            else {
                D.assert(_doneFuture != null);
                D.assert(_doneFuture._mayComplete);
                _doneFuture._asyncComplete(FutureOr.nil);
            }
        }
    }

//
    class _AsyncBroadcastStreamController<T> : _BroadcastStreamController<T> {
        internal _AsyncBroadcastStreamController(_stream.ControllerCallback onListen,
            _stream.ControllerCancelCallback onCancel)
            : base(onListen, onCancel) {
        }

        // EventDispatch interface.

        public override void _sendData(T data) {
            for (_BroadcastSubscription<T> subscription = _firstSubscription;
                subscription != null;
                subscription = subscription._next) {
                subscription._addPending(new _DelayedData<T>(data));
            }
        }

        public override void _sendError(object error, string stackTrace) {
            for (_BroadcastSubscription<T> subscription = _firstSubscription;
                subscription != null;
                subscription = subscription._next) {
                subscription._addPending(new _DelayedError<T>((Exception) error, stackTrace));
            }
        }

        public override void _sendDone() {
            if (!_isEmpty) {
                for (_BroadcastSubscription<T> subscription = _firstSubscription;
                    subscription != null;
                    subscription = subscription._next) {
                    subscription._addPending(new _DelayedDone<T>());
                }
            }
            else {
                D.assert(_doneFuture != null);
                D.assert(_doneFuture._mayComplete);
                _doneFuture._asyncComplete(FutureOr.nil);
            }
        }
    }

//
// /**
//  * Stream controller that is used by [Stream.asBroadcastStream].
//  *
//  * This stream controller allows incoming events while it is firing
//  * other events. This is handled by delaying the events until the
//  * current event is done firing, and then fire the pending events.
//  *
//  * This class extends [_SyncBroadcastStreamController]. Events of
//  * an "asBroadcastStream" stream are always initiated by events
//  * on another stream, and it is fine to forward them synchronously.
//  */
    class _AsBroadcastStreamController<T> : _SyncBroadcastStreamController<T>
        , _EventDispatch<T> {
        _StreamImplEvents<T> _pending;

        internal _AsBroadcastStreamController(Action onListen, Action onCancel)
            : base(() => onListen(), onCancel) {
        }

        bool _hasPending {
            get { return _pending != null && !_pending.isEmpty; }
        }

        void _addPendingEvent(_DelayedEvent<T> evt) {
            _pending = _pending ?? new _StreamImplEvents<T>();
            _pending.add(evt);
        }

        public override void add(T data) {
            if (!isClosed && _isFiring) {
                _addPendingEvent(new _DelayedData<T>(data));
                return;
            }

            base.add(data);
            while (_hasPending) {
                _pending.handleNext(this);
            }
        }

        public override void addError(object error, string stackTrace) {
            // ArgumentError.checkNotNull(error, "error");
            stackTrace = stackTrace ?? AsyncError.defaultStackTrace(error);
            if (!isClosed && _isFiring) {
                _addPendingEvent(new _DelayedError<T>((Exception) error, stackTrace));
                return;
            }

            if (!_mayAddEvent) throw _addEventError();
            _sendError(error, stackTrace);
            while (_hasPending) {
                _pending.handleNext(this);
            }
        }

        public override Future close() {
            if (!isClosed && _isFiring) {
                _addPendingEvent(new _DelayedDone<T>());
                _state |= _BroadcastStreamController<T>._STATE_CLOSED;
                return base.done;
            }

            Future result = base.close();
            D.assert(!_hasPending);
            return result;
        }

        internal override void _callOnCancel() {
            if (_hasPending) {
                _pending.clear();
                _pending = null;
            }

            base._callOnCancel();
        }
    }
}