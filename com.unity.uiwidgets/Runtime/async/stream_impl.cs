using System;
using System.Diagnostics;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

static partial class _stream {
    public delegate void _DataHandler<T>(T value);
    public delegate void _DoneHandler();

    public static void _nullDataHandler<T>(T obj) {}
    
    public static void _nullErrorHandler(Exception error) {
        Zone.current.handleUncaughtError(error);
    }
    
    public static void _nullDoneHandler() {}
}

abstract class _DelayedEvent<T> {
    /** Added as a linked list on the [StreamController]. */
    internal _DelayedEvent<T> next;
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
class _DelayedError : _DelayedEvent<object> {
readonly Exception error;
readonly  string stackTrace;

internal _DelayedError(Exception error, string stackTrace) {
    this.error = error;
    this.stackTrace = stackTrace;
}
public override void perform(_EventDispatch<object> dispatch) {
    dispatch._sendError(error, stackTrace);
}
}

class _DelayedDone : _DelayedEvent<object> {
    internal _DelayedDone(){}

 public override void perform(_EventDispatch<object> dispatch) {
    dispatch._sendDone();
}

_DelayedEvent<object>  next {
    get { return null; }
    set { throw new Exception("No events after a done.");}
}
}
interface  _EventSink<T> {
    void _add(T data);
    void _addError(object error, string stackTrace);
    void _close();
}

interface _EventDispatch<T> {
    void _sendData(T data);
    void _sendError(Object error, string stackTrace);
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

  void _setPendingEvents(_PendingEvents<T> pendingEvents) {
    D.assert(_pending == null);
    if (pendingEvents == null) return;
    _pending = pendingEvents;
    if (!pendingEvents.isEmpty) {
      _state |= _STATE_HAS_PENDING;
      _pending.schedule(this);
    }
  }

  // StreamSubscription interface.

  void onData(Action<T> handleData) {
    handleData ??= _stream._nullDataHandler;
    // TODO(floitsch): the return type should be 'void', and the type
    // should be inferred.
    _onData = d => _zone.registerUnaryCallback(data => {
         handleData((T) data);
         return default;
    });
  }

  // Siyao: c# does not support convert action
  void onError(Action<object, string> handleError) {
    handleError ??= (input1, input2) =>_stream._nullErrorHandler(null);
    
      _onError = (_, __)=> _zone
          .registerBinaryCallback((in1, in2)=> {
               handleError(in1, (string) in2);
               return null;
          });
  
  }

  void onDone(Action handleDone) {
    handleDone ??= _stream._nullDoneHandler;
    _onDone = ()=>_zone.registerCallback(()=> {
         handleDone();
         return null;
    });
  }

  void pause(Future resumeSignal) {
    if (_isCanceled) return;
    bool wasPaused = _isPaused;
    bool wasInputPaused = _isInputPaused;
    // Increment pause count and mark input paused (if it isn't already).
    _state = (_state + _STATE_PAUSE_COUNT) | _STATE_INPUT_PAUSED;
    if (resumeSignal != null) resumeSignal.whenComplete(resume);
    if (!wasPaused && _pending != null) _pending.cancelSchedule();
    if (!wasInputPaused && !_inCallback) _guardCallback(_onPause);
  }

  void resume() {
    if (_isCanceled) return;
    if (_isPaused) {
      _decrementPauseCount();
      if (!_isPaused) {
        if (_hasPending && !_pending.isEmpty) {
          // Input is still paused.
          _pending.schedule(this);
        } else {
          D.assert(_mayResumeInput);
          _state &= ~_STATE_INPUT_PAUSED;
          if (!_inCallback) _guardCallback(_onResume);
        }
      }
    }
  }

  Future cancel() {
    // The user doesn't want to receive any further events. If there is an
    // error or done event pending (waiting for the cancel to be done) discard
    // that event.
    _state &= ~_STATE_WAIT_FOR_CANCEL;
    if (!_isCanceled) {
      _cancel();
    }
    return _cancelFuture ?? Future._nullFuture;
  }

  Future<E> asFuture<E>(E futureValue) {
    _Future result = new _Future();

    // Overwrite the onDone and onError handlers.
    _onDone = ()=> {
      result._complete(FutureOr.value(futureValue));
    };
    _onError = (error,  stackTrace) => {
      Future cancelFuture = cancel();
      if (!Equals(cancelFuture, Future._nullFuture)) {
        cancelFuture.whenComplete(() =>{
          result._completeError(error);
        });
      } else {
        result._completeError(error);
      }
    };

    return result.to<E>();
  }

  // State management.

  internal bool _isInputPaused{get => (_state & _STATE_INPUT_PAUSED) != 0;}
  internal bool _isClosed{get => (_state & _STATE_CLOSED) != 0;}
  internal bool _isCanceled{get => (_state & _STATE_CANCELED) != 0;}
  internal bool _waitsForCancel{get => (_state & _STATE_WAIT_FOR_CANCEL) != 0;}
  internal bool _inCallback{get => (_state & _STATE_IN_CALLBACK) != 0;}
  internal bool _hasPending{get => (_state & _STATE_HAS_PENDING) != 0;}
  internal bool _isPaused{get => _state >= _STATE_PAUSE_COUNT;}
  internal bool _canFire{get => _state < _STATE_IN_CALLBACK;}

  internal bool _mayResumeInput {
      get =>
          !_isPaused && (_pending == null || _pending.isEmpty);
  }

  internal bool _cancelOnError{get => (_state & _STATE_CANCEL_ON_ERROR) != 0;}

  internal bool isPaused{get => _isPaused;}

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

  public void _add(T data) {
    D.assert(!_isClosed);
    if (_isCanceled) return;
    if (_canFire) {
      _sendData(data);
    } else {
      _addPending(new _DelayedData<object>(data));
    }
  }

  public void _addError(object error, string stackTrace) {
    if (_isCanceled) return;
    if (_canFire) {
      _sendError(error, stackTrace); // Reports cancel after sending.
    } else {
      _addPending(new _DelayedError((Exception)error, stackTrace));
    }
  }

  public void _close() {
    D.assert(!_isClosed);
    if (_isCanceled) return;
    _state |= _STATE_CLOSED;
    if (_canFire) {
      _sendDone();
    } else {
      _addPending(new _DelayedDone());
    }
  }

  // Hooks called when the input is paused, unpaused or canceled.
  // These must not throw. If overwritten to call user code, include suitable
  // try/catch wrapping and send any errors to
  // [_Zone.current.handleUncaughtError].
  void _onPause() {
    D.assert(_isInputPaused);
  }

  void _onResume() {
    D.assert(!_isInputPaused);
  }

  Future _onCancel() {
    D.assert(_isCanceled);
    return null;
  }

  // Handle pending events.

  void _addPending(_DelayedEvent<object> evt) {
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

  public void _sendData(T data) {
    D.assert(!_isCanceled);
    D.assert(!_isPaused);
    D.assert(!_inCallback);
    bool wasInputPaused = _isInputPaused;
    _state |= _STATE_IN_CALLBACK;
    _zone.runUnaryGuarded(data=> {
         _onData((T) data);
         return null;
    }, data);
    _state &= ~_STATE_IN_CALLBACK;
    _checkState(wasInputPaused);
  }

  public void _sendError(object error, string stackTrace) {
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
        _zone.runBinaryGuarded((error, stack)=> {
             onError((Exception) error, (string) stack);
             return null;
        }, error, stackTrace);
      } else {
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
      } else {
        sendError();
      }
    } else {
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
      _zone.runGuarded(()=> {
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
    } else {
      sendDone();
    }
  }

  void _guardCallback(Action callback) {
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
      } else {
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

    public bool  isEmpty{get;}

    public bool  isScheduled{get => _state == _STATE_SCHEDULED;}
    public bool  _eventScheduled{get => _state >= _STATE_SCHEDULED;}

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
    _DelayedEvent<object> firstPendingEvent;

    /// Last element in the list of pending events. New events are added after it.
    _DelayedEvent<object> lastPendingEvent;

    bool  isEmpty {
        get { return lastPendingEvent == null; }
    }

    internal void add(_DelayedEvent<object> evt) {
        if (lastPendingEvent == null) {
            firstPendingEvent = lastPendingEvent = evt;
        } else {
            lastPendingEvent = lastPendingEvent.next = evt;
        }
    }

    public override void handleNext(_EventDispatch<T> dispatch) {
        D.assert(!isScheduled);
        _DelayedEvent<object> evt = firstPendingEvent;
        firstPendingEvent = evt.next;
        if (firstPendingEvent == null) {
            lastPendingEvent = null;
        }
        evt.perform((_EventDispatch<object>) dispatch);
    }

    public override void clear() {
        if (isScheduled) cancelSchedule();
        firstPendingEvent = lastPendingEvent = null;
    }
}

internal  class _StreamIterator<T> : StreamIterator<T> {
  // The stream iterator is always in one of four states.
  // The value of the [_stateData] field depends on the state.
  //
  // When `_subscription == null` and `_stateData != null`:
  // The stream iterator has been created, but [moveNext] has not been called
  // yet. The [_stateData] field contains the stream to listen to on the first
  // call to [moveNext] and [current] returns `null`.
  //
  // When `_subscription != null` and `!_isPaused`:
  // The user has called [moveNext] and the iterator is waiting for the next
  // event. The [_stateData] field contains the [_Future] returned by the
  // [_moveNext] call and [current] returns `null.`
  //
  // When `_subscription != null` and `_isPaused`:
  // The most recent call to [moveNext] has completed with a `true` value
  // and [current] provides the value of the data event.
  // The [_stateData] field contains the [current] value.
  //
  // When `_subscription == null` and `_stateData == null`:
  // The stream has completed or been canceled using [cancel].
  // The stream completes on either a done event or an error event.
  // The last call to [moveNext] has completed with `false` and [current]
  // returns `null`.

  StreamSubscription<object> _subscription;

  //@pragma("vm:entry-point")
  object _stateData;

  bool _isPaused = false;

  internal _StreamIterator(Stream<T> stream) {
      if (stream != null) {
          _stateData = stream;
      }
      else {
          throw new ArgumentException("not null","stream");
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
    StreamSubscription<object> subscription = _subscription;
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
