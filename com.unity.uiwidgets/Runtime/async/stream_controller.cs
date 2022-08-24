using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.async {
    static partial class _stream {
        public delegate void ControllerCallback();

        public delegate Future ControllerCancelCallback();

        public delegate void _NotificationHandler();


        public static void _runGuarded(_NotificationHandler notificationHandler) {
            if (notificationHandler == null) return;
            try {
                notificationHandler();
            }
            catch (Exception e) {
                Zone.current.handleUncaughtError(e);
            }
        }
    }


    public interface IStreamController<T> {
        Stream<T> stream { get; }
        
        _stream.ControllerCallback onListen { get; set; }

        // void  onListen(void onListenHandler());

        _stream.ControllerCallback onPause { get; set; }

        // void set onPause(void onPauseHandler());

        _stream.ControllerCallback onResume { get; set; }

        // void set onResume(void onResumeHandler());

        _stream.ControllerCancelCallback onCancel { get; set; }

        // void set onCancel(onCancelHandler());

        StreamSink<T> sink { get; }

       bool isClosed { get; }

        bool isPaused { get; }

        /** Whether there is a subscriber on the [Stream]. */
        bool hasListener { get; }

        // public abstract void add(T evt);
        //
        // public abstract void addError(object error, string stackTrace);

        Future close();

        Future addStream(Stream<T> source, bool? cancelOnError = false);
        
        void add(T evt);
        void addError(object error, string stackTrace);

        Future done { get; }
    } 
    
    public abstract class StreamController<T> : StreamSink<T>, IStreamController<T> {
        /** The stream that this controller is controlling. */
        public virtual Stream<T> stream { get; }

        public static StreamController<T> create(
            _stream.ControllerCallback onListen = null,
            _stream.ControllerCallback onPause = null,
            _stream.ControllerCallback onResume = null,
            _stream.ControllerCancelCallback onCancel = null,
            //  Action onListen = null,
            //  Action onPause = null,
            //  Action onResume = null,
            // Action onCancel = null,
            bool sync = false) {
            return sync
                ? (StreamController<T>) new _SyncStreamController<T>(onListen, onPause, onResume, onCancel)
                : new _AsyncStreamController<T>(onListen, onPause, onResume, onCancel);
        }

        public static StreamController<T> broadcast(
            Action onListen = null, Action onCancel = null, bool sync = false) {
            return sync
                ? (StreamController<T>) new _SyncBroadcastStreamController<T>(() => onListen?.Invoke(), onCancel)
                : new _AsyncBroadcastStreamController<T>(() => onListen?.Invoke(), () => {
                    onCancel?.Invoke();
                    return Future._nullFuture;
                });
        }

        public virtual _stream.ControllerCallback onListen { get; set; }

        // void  onListen(void onListenHandler());

        public virtual _stream.ControllerCallback onPause { get; set; }

        // void set onPause(void onPauseHandler());

        public virtual _stream.ControllerCallback onResume { get; set; }

        // void set onResume(void onResumeHandler());

        public virtual _stream.ControllerCancelCallback onCancel { get; set; }

        // void set onCancel(onCancelHandler());

        public virtual StreamSink<T> sink { get; }

        public virtual bool isClosed { get; }

        public virtual bool isPaused { get; }

        /** Whether there is a subscriber on the [Stream]. */
        public virtual bool hasListener { get; }

        // public abstract void add(T evt);
        //
        // public abstract void addError(object error, string stackTrace);

        public abstract override Future close();

        public abstract Future addStream(Stream<T> source, bool? cancelOnError = false);
    }

    public interface SynchronousStreamController<T> {
        //: StreamController<T> {
        // public abstract void add(T data);

        // public abstract void addError(object error, string stackTrace);

        // public abstract Future close();
    }

    interface _StreamControllerLifecycle<T> {
        StreamSubscription<T> _subscribe(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError);

        void _recordPause(StreamSubscription<T> subscription);
        void _recordResume(StreamSubscription<T> subscription);
        Future _recordCancel(StreamSubscription<T> subscription);
    }

//
// // Base type for implementations of stream controllers.
    abstract class _StreamControllerBase<T>
        :
            StreamController<T>,
            _StreamControllerLifecycle<T>,
            _EventSink<T>,
            _EventDispatch<T> {
        public abstract StreamSubscription<T> _subscribe(Action<T> onData, Action<object, string> onError,
            Action onDone, bool cancelOnError);


        public virtual void _recordPause(StreamSubscription<T> subscription) {
        }

        public virtual void _recordResume(StreamSubscription<T> subscription) {
        }

        public virtual Future _recordCancel(StreamSubscription<T> subscription) => null;

        public abstract void _add(T data);

        public abstract void _addError(object error, string stackTrace);

        public abstract void _close();

        public abstract void _sendData(T data);

        public abstract void _sendError(object error, string stackTrace);

        public abstract void _sendDone();
    }

    abstract class _StreamController<T> : _StreamControllerBase<T> {
        /** The controller is in its initial state with no subscription. */
        internal const int _STATE_INITIAL = 0;

        internal const int _STATE_SUBSCRIBED = 1;

        /** The subscription is canceled. */
        internal const int _STATE_CANCELED = 2;

        /** Mask for the subscription state. */
        internal const int _STATE_SUBSCRIPTION_MASK = 3;

        // The following state relate to the controller, not the subscription.
        // If closed, adding more events is not allowed.
        // If executing an [addStream], new events are not allowed either, but will
        // be added by the stream.

        internal const int _STATE_CLOSED = 4;
        internal const int _STATE_ADDSTREAM = 8;

        // @pragma("vm:entry-point")
        object _varData;

        /** Current state of the controller. */
        // @pragma("vm:entry-point")
        protected int _state = _STATE_INITIAL;

        // TODO(lrn): Could this be stored in the varData field too, if it's not
        // accessed until the call to "close"? Then we need to special case if it's
        // accessed earlier, or if close is called before subscribing.
        _Future _doneFuture;

        public override _stream.ControllerCallback onListen { get; set; }
        public override _stream.ControllerCallback onPause  { get; set; }
        public override _stream.ControllerCallback onResume  { get; set; }
        public override _stream.ControllerCancelCallback onCancel  { get; set; }

        internal _StreamController(_stream.ControllerCallback onListen, _stream.ControllerCallback onPause,
            _stream.ControllerCallback onResume, _stream.ControllerCancelCallback onCancel) {
            this.onListen = onListen;
            this.onPause = onPause;
            this.onResume = onResume;
            this.onCancel = onCancel;
        }

        // Return a new stream every time. The streams are equal, but not identical.
        public override Stream<T> stream {
            get => new _ControllerStream<T>(this);
        }

        public override StreamSink<T> sink {
            get => new _StreamSinkWrapper<T>(this);
        }

        bool _isCanceled {
            get => (_state & _STATE_CANCELED) != 0;
        }

        /** Whether there is an active listener. */
        public override bool hasListener {
            get => (_state & _STATE_SUBSCRIBED) != 0;
        }

        /** Whether there has not been a listener yet. */
        bool _isInitialState {
            get =>
                (_state & _STATE_SUBSCRIPTION_MASK) == _STATE_INITIAL;
        }

        public override bool isClosed {
            get => (_state & _STATE_CLOSED) != 0;
        }

        public override bool isPaused {
            get =>
                hasListener ? _subscription._isInputPaused : !_isCanceled;
        }

        bool _isAddingStream {
            get => (_state & _STATE_ADDSTREAM) != 0;
        }

        /** New events may not be added after close, or during addStream. */
        internal bool _mayAddEvent {
            get => (_state < _STATE_CLOSED);
        }

        // Returns the pending events.
        // Pending events are events added before a subscription exists.
        // They are added to the subscription when it is created.
        // Pending events, if any, are kept in the _varData field until the
        // stream is listened to.
        // While adding a stream, pending events are moved into the
        // state object to allow the state object to use the _varData field.
        _PendingEvents<T> _pendingEvents {
            get {
                D.assert(_isInitialState);
                if (!_isAddingStream) {
                    return (_PendingEvents<T>) _varData;
                }

                _StreamControllerAddStreamState<T> state = (_StreamControllerAddStreamState<T>) _varData;
                return (_PendingEvents<T>) state.varData;
            }
        }

        // Returns the pending events, and creates the object if necessary.
        _StreamImplEvents<T> _ensurePendingEvents() {
            D.assert(_isInitialState);
            if (!_isAddingStream) {
                _varData = _varData ?? new _StreamImplEvents<T>();
                return (_StreamImplEvents<T>) _varData;
            }

            _StreamControllerAddStreamState<T> state = (_StreamControllerAddStreamState<T>) _varData;
            if (state.varData == null) state.varData = new _StreamImplEvents<T>();
            return (_StreamImplEvents<T>) state.varData;
        }

        // Get the current subscription.
        // If we are adding a stream, the subscription is moved into the state
        // object to allow the state object to use the _varData field.
        protected _ControllerSubscription<T> _subscription {
            get {
                D.assert(hasListener);
                if (_isAddingStream) {
                    _StreamControllerAddStreamState<T> addState = (_StreamControllerAddStreamState<T>) _varData;
                    return (_ControllerSubscription<T>) addState.varData;
                }

                return (_ControllerSubscription<T>) _varData;
            }
        }

        protected Exception _badEventState() {
            if (isClosed) {
                return new Exception("Cannot add event after closing");
            }

            D.assert(_isAddingStream);
            return new Exception("Cannot add event while adding a stream");
        }

        // StreamSink interface.
        public override Future addStream(Stream<T> source, bool? cancelOnError = false) {
            if (!_mayAddEvent) throw _badEventState();
            if (_isCanceled) return _Future.immediate(FutureOr.nil);
            _StreamControllerAddStreamState<T> addState =
                new _StreamControllerAddStreamState<T>(
                    this, _varData, source, cancelOnError ?? false);
            _varData = addState;
            _state |= _STATE_ADDSTREAM;
            return addState.addStreamFuture;
        }

        public override Future done {
            get { return _ensureDoneFuture(); }
        }

        Future _ensureDoneFuture() {
            _doneFuture = _doneFuture ?? (_isCanceled ? Future._nullFuture : new _Future());
            return _doneFuture;
        }

        public override void add(T value) {
            if (!_mayAddEvent) throw _badEventState();
            _add(value);
        }

        public override void addError(object error, string stackTrace) {
            // ArgumentError.checkNotNull(error, "error");
            if (!_mayAddEvent) throw _badEventState();
            error = _async._nonNullError(error);
            AsyncError replacement = Zone.current.errorCallback((Exception) error);
            if (replacement != null) {
                error = _async._nonNullError(replacement);
                // stackTrace = replacement.stackTrace;
            }

            stackTrace = stackTrace ?? AsyncError.defaultStackTrace(error);
            _addError(error, stackTrace);
        }

        public override Future close() {
            if (isClosed) {
                return _ensureDoneFuture();
            }

            if (!_mayAddEvent) throw _badEventState();
            _closeUnchecked();
            return _ensureDoneFuture();
        }

        internal void _closeUnchecked() {
            _state |= _STATE_CLOSED;
            if (hasListener) {
                _sendDone();
            }
            else if (_isInitialState) {
                _ensurePendingEvents().add(new _DelayedDone<T>());
            }
        }

        // EventSink interface. Used by the [addStream] events.

        // Add data event, used both by the [addStream] events and by [add].
        public override void _add(T value) {
            if (hasListener) {
                _sendData(value);
            }
            else if (_isInitialState) {
                _ensurePendingEvents().add(new _DelayedData<T>(value));
            }
        }

        public override void _addError(object error, string stackTrace) {
            if (hasListener) {
                _sendError(error, stackTrace);
            }
            else if (_isInitialState) {
                _ensurePendingEvents().add(new _DelayedError<T>((Exception) error, stackTrace));
            }
        }

        public override void _close() {
            // End of addStream stream.
            D.assert(_isAddingStream);
            _StreamControllerAddStreamState<T> addState = (_StreamControllerAddStreamState<T>) _varData;
            _varData = addState.varData;
            _state &= ~_STATE_ADDSTREAM;
            addState.complete();
        }

        // _StreamControllerLifeCycle interface

        public override StreamSubscription<T> _subscribe(
            Action<T> onData,
            Action<object, string> onError,
            Action onDone, bool cancelOnError) {
            if (!_isInitialState) {
                throw new Exception("Stream has already been listened to.");
            }

            _ControllerSubscription<T> subscription = new _ControllerSubscription<T>(
                this, onData, onError, onDone, cancelOnError);

            _PendingEvents<T> pendingEvents = _pendingEvents;
            _state |= _STATE_SUBSCRIBED;
            if (_isAddingStream) {
                _StreamControllerAddStreamState<T> addState = (_StreamControllerAddStreamState<T>) _varData;
                addState.varData = subscription;
                addState.resume();
            }
            else {
                _varData = subscription;
            }

            subscription._setPendingEvents(pendingEvents);
            subscription._guardCallback(() => { _stream._runGuarded(() => onListen?.Invoke()); });

            return subscription;
        }

        public override Future _recordCancel(StreamSubscription<T> subscription) {
            // When we cancel, we first cancel any stream being added,
            // Then we call `onCancel`, and finally the _doneFuture is completed.
            // If either of addStream's cancel or `onCancel` returns a future,
            // we wait for it before continuing.
            // Any error during this process ends up in the returned future.
            // If more errors happen, we act as if it happens inside nested try/finallys
            // or whenComplete calls, and only the last error ends up in the
            // returned future.
            Future result = null;
            if (_isAddingStream) {
                _StreamControllerAddStreamState<T> addState = (_StreamControllerAddStreamState<T>) _varData;
                result = addState.cancel();
            }

            _varData = null;
            _state =
                (_state & ~(_STATE_SUBSCRIBED | _STATE_ADDSTREAM)) | _STATE_CANCELED;

            if (onCancel != null) {
                if (result == null) {
                    // Only introduce a future if one is needed.
                    // If _onCancel returns null, no future is needed.
                    try {
                        result = onCancel();
                    }
                    catch (Exception e) {
                        // Return the error in the returned future.
                        // Complete it asynchronously, so there is time for a listener
                        // to handle the error.
                        var f = new _Future();
                        f._asyncCompleteError(e);
                        result = f;
                    }
                }
                else {
                    // Simpler case when we already know that we will return a future.
                    result = result.whenComplete(() => onCancel());
                }
            }

            void complete() {
                if (_doneFuture != null && _doneFuture._mayComplete) {
                    _doneFuture._asyncComplete(FutureOr.nil);
                }
            }

            if (result != null) {
                result = result.whenComplete(complete);
            }
            else {
                complete();
            }

            return result;
        }

        public override void _recordPause(StreamSubscription<T> subscription) {
            if (_isAddingStream) {
                _StreamControllerAddStreamState<T> addState = (_StreamControllerAddStreamState<T>) _varData;
                addState.pause();
            }

            _stream._runGuarded(() => onPause?.Invoke());
        }

        public override void _recordResume(StreamSubscription<T> subscription) {
            if (_isAddingStream) {
                _StreamControllerAddStreamState<T> addState = (_StreamControllerAddStreamState<T>) _varData;
                addState.resume();
            }

            _stream._runGuarded(() => onResume?.Invoke());
        }
    }

//
    abstract class _SyncStreamControllerDispatch<T>
        : _StreamController<T>, SynchronousStreamController<T> {

        public override void _sendData(T data) {
            _subscription._add(data);
        }

        public override void _sendError(object error, string stackTrace) {
            _subscription._addError(error, stackTrace);
        }

        public override void _sendDone() {
            _subscription._close();
        }

        protected _SyncStreamControllerDispatch(_stream.ControllerCallback onListen, _stream.ControllerCallback onPause,
            _stream.ControllerCallback onResume, _stream.ControllerCancelCallback onCancel) : base(onListen, onPause,
            onResume, onCancel) {
        }
    }

    abstract class _AsyncStreamControllerDispatch<T>
        : _StreamController<T> {
        public override void _sendData(T data) {
            _subscription._addPending(new _DelayedData<T>(data));
        }

        public override void _sendError(object error, string stackTrace) {
            _subscription._addPending(new _DelayedError<T>((Exception) error, stackTrace));
        }

        public override void _sendDone() {
            _subscription._addPending(new _DelayedDone<T>());
        }

        protected _AsyncStreamControllerDispatch(_stream.ControllerCallback onListen,
            _stream.ControllerCallback onPause, _stream.ControllerCallback onResume,
            _stream.ControllerCancelCallback onCancel) : base(onListen, onPause, onResume, onCancel) {
        }
    }

// TODO(lrn): Use common superclass for callback-controllers when VM supports
// constructors in mixin superclasses.

    class _AsyncStreamController<T> : _AsyncStreamControllerDispatch<T> {
        // public override void close() {
        //     throw new NotImplementedException();
        // }
        public _AsyncStreamController(_stream.ControllerCallback onListen, _stream.ControllerCallback onPause,
            _stream.ControllerCallback onResume, _stream.ControllerCancelCallback onCancel) : base(onListen, onPause,
            onResume, onCancel) {
        }
    }

    class _SyncStreamController<T> : _SyncStreamControllerDispatch<T> {
        public _SyncStreamController(_stream.ControllerCallback onListen, _stream.ControllerCallback onPause,
            _stream.ControllerCallback onResume, _stream.ControllerCancelCallback onCancel) : base(onListen, onPause,
            onResume, onCancel) {
        }
    }


    class _ControllerStream<T> : _StreamImpl<T>, IEquatable<_ControllerStream<T>> {
        _StreamControllerLifecycle<T> _controller;

        internal _ControllerStream(_StreamControllerLifecycle<T> _controller) {
            this._controller = _controller;
        }

        internal override StreamSubscription<T> _createSubscription(
            Action<T> onData, Action<object, string> onError, Action onDone, bool cancelOnError) =>
            _controller._subscribe(onData, onError, onDone, cancelOnError);

        // Override == and hashCode so that new streams returned by the same
        // controller are considered equal. The controller returns a new stream
        // each time it's queried, but doesn't have to cache the result.

        // int  hashCode {
        //     get { return _controller.GetHashCode() ^ 0x35323532; }
        // }

        // bool operator ==(object other) {
        //   if (identical(this, other)) return true;
        //   return other is _ControllerStream &&
        //       identical(other._controller, this._controller);
        // }

        public bool Equals(_ControllerStream<T> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(_controller, other._controller);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((_ControllerStream<T>) obj);
        }

        public override int GetHashCode() {
            return _controller.GetHashCode() ^ 0x35323532;
        }
    }

    class _ControllerSubscription<T> : _BufferingStreamSubscription<T> {
        internal readonly _StreamControllerLifecycle<T> _controller;

        internal _ControllerSubscription(
            _StreamControllerLifecycle<T> _controller,
            Action<T> onData,
            Action<object, string> onError,
            Action onDone, bool cancelOnError
        )
            : base(onData, onError, onDone, cancelOnError) {
            this._controller = _controller;
        }

        protected override Future _onCancel() {
            return _controller._recordCancel(this);
        }

        protected override void _onPause() {
            _controller._recordPause(this);
        }

        protected override void _onResume() {
            _controller._recordResume(this);
        }
    }

    /** A class that exposes only the [StreamSink] interface of an object. */
    class _StreamSinkWrapper<T> : StreamSink<T> {
        readonly StreamController<T> _target;

        internal _StreamSinkWrapper(StreamController<T> _target) {
            this._target = _target;
        }

        public override void add(T data) {
            _target.add(data);
        }

        public override void addError(object error, string stackTrace) {
            _target.addError(error, stackTrace);
        }

        public override Future close() => _target.close();

        public override Future addStream(Stream<T> source) => _target.addStream(source);

        public override Future done {
            get { return _target.done; }
        }
    }

    class _AddStreamState<T> {
        // [_Future] returned by call to addStream.
        internal readonly _Future addStreamFuture;

        // Subscription on stream argument to addStream.
        internal readonly StreamSubscription<T> addSubscription;

        internal _AddStreamState(
            _EventSink<T> controller, Stream<T> source, bool cancelOnError) {
            addStreamFuture = new _Future();
            addSubscription = source.listen(controller._add,
                onError: cancelOnError
                    ? makeErrorHandler(controller)
                    : controller._addError,
                onDone: controller._close,
                cancelOnError: cancelOnError);
        }

        public static Action<object, string> makeErrorHandler(_EventSink<T> controller) {
            return (object e, string s) => {
                controller._addError(e, s);
                controller._close();
            };
        }

        public void pause() {
            addSubscription.pause();
        }

        public void resume() {
            addSubscription.resume();
        }

        public Future cancel() {
            var cancel = addSubscription.cancel();
            if (cancel == null) {
                addStreamFuture._asyncComplete(FutureOr.nil);
                return null;
            }

            return cancel.whenComplete(() => { addStreamFuture._asyncComplete(FutureOr.nil); });
        }

        public void complete() {
            addStreamFuture._asyncComplete(FutureOr.nil);
        }
    }

    class _StreamControllerAddStreamState<T> : _AddStreamState<T> {
        // The subscription or pending data of a _StreamController.
        // Stored here because we reuse the `_varData` field  in the _StreamController
        // to store this state object.
        public object varData;

        internal _StreamControllerAddStreamState(_StreamController<T> controller, object varData,
            Stream<T> source, bool cancelOnError)
            : base(controller, source, cancelOnError) {
            if (controller.isPaused) {
                addSubscription.pause();
            }
        }
    }
}