using System;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.core;
using Unity.UIWidgets.foundation;
using Stopwatch = Unity.UIWidgets.core.Stopwatch;

namespace Unity.UIWidgets.async {
    public static partial class _stream {
        public delegate void _TimerCallback();
    }

    public abstract class Stream<T> {
        public Stream() {
        }

        // const Stream._internal();

        public static Stream<T> empty() => new _EmptyStream<T>();

        // @Since("2.5")
        public static Stream<T> value(T value) {
            var result = new _AsyncStreamController<T>(null, null, null, null);
            result._add(value);
            result._closeUnchecked();
            return result.stream;
        }

        // @Since("2.5")
        public static Stream<T> error(object error, string stackTrace = null) {
            // ArgumentError.checkNotNull(error, "error");
            var result = new _AsyncStreamController<T>(null, null, null, null);
            result._addError(error, stackTrace ?? AsyncError.defaultStackTrace(error));
            result._closeUnchecked();
            return result.stream;
        }

        public static Stream<T> fromFuture(Future<T> future) {
            // Use the controller's buffering to fill in the value even before
            // the stream has a listener. For a single value, it's not worth it
            // to wait for a listener before doing the `then` on the future.
            _StreamController<T> controller =
                new _SyncStreamController<T>(null, null, null, null);
            future.then((value) => {
                controller._add((T) value);
                controller._closeUnchecked();
            }, onError: (error) => {
                controller._addError(error, null);
                controller._closeUnchecked();
                return FutureOr.nil;
            });
            return controller.stream;
        }

        public static Stream<T> fromFutures(IEnumerable<Future<T>> futures) {
            _StreamController<T> controller =
                new _SyncStreamController<T>(null, null, null, null);
            int count = 0;
            // Declare these as variables holding closures instead of as
            // function declarations.
            // This avoids creating a new closure from the functions for each future.
            var onValue = new Action<object>((object value) => {
                if (!controller.isClosed) {
                    controller._add((T) value);
                    if (--count == 0) controller._closeUnchecked();
                }
            });
            var onError = new Func<Exception, FutureOr>((error) => {
                if (!controller.isClosed) {
                    controller._addError(error, null);
                    if (--count == 0) controller._closeUnchecked();
                }

                return FutureOr.nil;
            });
            // The futures are already running, so start listening to them immediately
            // (instead of waiting for the stream to be listened on).
            // If we wait, we might not catch errors in the futures in time.
            foreach (var future in futures) {
                count++;
                future.then(onValue, onError: onError);
            }

            // Use schedule microtask since controller is sync.
            if (count == 0) async_.scheduleMicrotask(controller.close);
            return controller.stream;
        }

        public static Stream<T> fromIterable(IEnumerable<T> elements) {
            return new _GeneratedStreamImpl<T>(
                () => (_PendingEvents<T>) new _IterablePendingEvents<T>(elements));
        }

        public static Stream<T> periodic(TimeSpan period,
            Func<int, T> computation = null) {
            Timer timer = default;
            int computationCount = 0;
            StreamController<T> controller = null;
            // Counts the time that the Stream was running (and not paused).
            Stopwatch watch = new Stopwatch();

            Action sendEvent = () => {
                watch.reset();
                T data = default;
                if (computation != null) {
                    try {
                        data = computation(computationCount++);
                    }
                    catch (Exception e) {
                        controller.addError(e, e.StackTrace);
                        return;
                    }
                }

                controller.add(data);
            };

            Action startPeriodicTimer = () => {
                D.assert(timer == null);
                timer = Timer.periodic(period, (object timer1) => {
                    sendEvent();
                    return null;
                });
            };

            // the original code new an abstract class
            controller = StreamController<T>.create(
                sync: true,
                onListen: () => {
                    watch.start();
                    startPeriodicTimer();
                },
                onPause: () => {
                    timer.cancel();
                    timer = null;
                    watch.stop();
                },
                onResume: () => {
                    D.assert(timer == null);
                    TimeSpan elapsed = watch.elapsed;
                    watch.start();
                    timer = Timer.create(period - elapsed, () => {
                        timer = null;
                        startPeriodicTimer();
                        sendEvent();
                    });
                },
                onCancel: () => {
                    if (timer != null) timer.cancel();
                    timer = null;
                    return Future._nullFuture;
                });
            return controller.stream;
        }

        public static Stream<T> eventTransformed(
            Stream<T> source, _async._SinkMapper<T, T> mapSink) {
            return new _BoundSinkStream<T, T>(source, mapSink);
        }

        static Stream<U> castFrom<S, U>(Stream<S> source) =>
            new CastStream<S, U>(source);

        public virtual bool isBroadcast {
            get { return false; }
        }

        public virtual Stream<T> asBroadcastStream(
            Action<StreamSubscription<T>> onListen = null,
            Action<StreamSubscription<T>> onCancel = null) {
            return new _AsBroadcastStream<T>(this, onListen, onCancel);
        }

        public abstract StreamSubscription<T> listen(
            Action<T> onData, Action<object, string> onError = null, Action onDone = null, bool cancelOnError = false);

        public Stream<T> where(Func<T, bool> test) {
            return new _WhereStream<T>(this, test);
        }

        public Stream<S> map<S>(Func<T, S> convert) {
            return new _MapStream<T, S>(this, convert);
        }

        public Stream<E> asyncMap<E>(Func<T, FutureOr> convert) {
            _StreamControllerBase<E> controller = null;
            StreamSubscription<T> subscription = null;

            void onListen() {
                var add = new Action<E>(controller.add);
                D.assert(controller is _StreamController<E> ||
                         controller is _BroadcastStreamController<E>);
                var addError = new Action<object, string>(controller._addError);
                subscription = listen((T evt) => {
                    FutureOr newValue;
                    try {
                        newValue = convert(evt);
                    }
                    catch (Exception e) {
                        controller.addError(e, e.StackTrace);
                        return;
                    }

                    if (newValue.f is Future<E> newFuture) {
                        // siyao: this if different from dart
                        subscription.pause();
                        newFuture
                            .then(d => add((E) d), onError: (e) => {
                                addError(e, e.StackTrace);
                                return FutureOr.nil;
                            })
                            .whenComplete(subscription.resume);
                    }
                    else {
                        // Siyao: This works as if this is csharpt
                        controller.add((E) newValue.v);
                    }
                }, onError: addError, onDone: () => controller.close());
            }

            if (isBroadcast) {
                controller = (_StreamControllerBase<E>) StreamController<E>.broadcast(
                    onListen: () => onListen(),
                    onCancel: () => { subscription.cancel(); },
                    sync: true);
            }
            else {
                controller = (_StreamControllerBase<E>) StreamController<E>.create(
                    onListen: onListen,
                    onPause: () => { subscription.pause(); },
                    onResume: () => { subscription.resume(); },
                    onCancel: () => subscription.cancel(),
                    sync: true);
            }

            return controller.stream;
        }

        Stream<E> asyncExpand<E>(Func<T, Stream<E>> convert) {
            _StreamControllerBase<E> controller = null;
            StreamSubscription<T> subscription = null;

            void onListen() {
                D.assert(controller is _StreamController<E> ||
                         controller is _BroadcastStreamController<E>);
                subscription = listen((T evt) => {
                        Stream<E> newStream;
                        try {
                            newStream = convert(evt);
                        }
                        catch (Exception e) {
                            controller.addError(e, e.StackTrace);
                            return;
                        }

                        if (newStream != null) {
                            subscription.pause();
                            controller.addStream(newStream).whenComplete(subscription.resume);
                        }
                    },
                    onError: controller._addError, // Avoid Zone error replacement.
                    onDone: () => controller.close());
            }

            if (isBroadcast) {
                controller = (_StreamControllerBase<E>) StreamController<E>.broadcast(
                    onListen: () => onListen(),
                    onCancel: () => { subscription.cancel(); },
                    sync: true);
            }
            else {
                controller = (_StreamControllerBase<E>) StreamController<E>.create(
                    onListen: () => onListen(),
                    onPause: () => { subscription.pause(); },
                    onResume: () => { subscription.resume(); },
                    onCancel: () => subscription.cancel(),
                    sync: true);
            }

            return controller.stream;
        }

        Stream<T> handleError(ZoneBinaryCallback onError, _stream._ErrorTest test = null) {
            return new _HandleErrorStream<T>(this, onError, test);
        }

        Stream<S> expand<S>(_stream._Transformation<T, IEnumerable<S>> convert) {
            return new _ExpandStream<T, S>(this, convert);
        }

        Future pipe(StreamConsumer<T> streamConsumer) {
            return streamConsumer.addStream(this).then((_) => streamConsumer.close(), (_) => FutureOr.nil);
        }

        public Stream<S> transform<S>(StreamTransformer<T, S> streamTransformer) {
            return streamTransformer.bind(this);
        }

        Future<T> reduce(Func<T, T, T> combine) {
            _Future result = new _Future();
            bool seenFirst = false;
            T value = default;
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T element) => {
                    if (seenFirst) {
                        _stream._runUserCode(() => combine(value, element), (T newValue) => { value = newValue; },
                            onError: (e) => _stream._cancelAndErrorClosure(subscription, result)(e));
                    }
                    else {
                        value = element;
                        seenFirst = true;
                    }
                },
                onError: (e, s) => result._completeError((Exception) e),
                onDone: () => {
                    if (!seenFirst) {
                        try {
                            // Throw and recatch, instead of just doing
                            //  _completeWithErrorCallback, e, theError, StackTrace.current),
                            // to ensure that the stackTrace is set on the error.
                            throw new Exception("IterableElementError.noElement()");
                        }
                        catch (Exception e) {
                            async_._completeWithErrorCallback(result, e);
                        }
                    }
                    else {
                        // TODO: need check
                        result._complete(FutureOr.value(value));
                    }
                },
                cancelOnError: true);
            return result.to<T>();
        }

        Future<S> fold<S>(S initialValue, Func<S, T, S> combine) {
            _Future result = new _Future();
            S value = initialValue;
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T element) => {
                    _stream._runUserCode(() => combine(value, element), (S newValue) => { value = newValue; },
                        e => _stream._cancelAndErrorClosure(subscription, result)(e));
                },
                onError: (e, s) => result._completeError((Exception) e),
                onDone: () => { result._complete(FutureOr.value(value)); },
                cancelOnError: true);
            return result.to<S>();
        }

        Future<string> join(string separator = "") {
            _Future result = new _Future();
            StringBuilder buffer = new StringBuilder();
            StreamSubscription<T> subscription = null;
            bool first = true;
            subscription = listen(
                (T element) => {
                    if (!first) {
                        buffer.Append(separator);
                    }

                    first = false;
                    try {
                        buffer.Append(element);
                    }
                    catch (Exception e) {
                        _stream._cancelAndErrorWithReplacement(subscription, result, e);
                    }
                },
                onError: (e, _) => result._completeError((Exception) e),
                onDone: () => { result._complete(buffer.ToString()); },
                cancelOnError: true);
            return result.to<string>();
        }

        Future<bool> contains(object needle) {
            _Future future = new _Future();
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T element) => {
                    _stream._runUserCode(() => (Equals(element, needle)), (bool isMatch) => {
                        if (isMatch) {
                            _stream._cancelAndValue(subscription, future, true);
                        }
                    }, (e) => _stream._cancelAndErrorClosure(subscription, future)(e));
                },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => { future._complete(false); },
                cancelOnError: true);
            return future.to<bool>();
        }

        public Future forEach(Action<T> action) {
            _Future future = new _Future();
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T element) => {
                    // TODO(floitsch): the type should be 'void' and inferred.
                    _stream._runUserCode<object>(() => {
                            action(element);
                            return default;
                        }, (_) => { },
                        (e) => _stream._cancelAndErrorClosure(subscription, future)(e));
                },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => { future._complete(FutureOr.nil); },
                cancelOnError: true);
            return future;
        }

        Future<bool> every(Func<T, bool> test) {
            _Future future = new _Future();
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T element) => {
                    _stream._runUserCode(() => test(element), (bool isMatch) => {
                        if (!isMatch) {
                            _stream._cancelAndValue(subscription, future, false);
                        }
                    }, ex => _stream._cancelAndErrorClosure(subscription, future)(ex));
                },
                onError: (ex, s) => future._completeError((Exception) ex),
                onDone: () => { future._complete(true); },
                cancelOnError: true);
            return future.to<bool>();
        }

        Future<bool> any(Func<T, bool> test) {
            _Future future = new _Future();
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T element) => {
                    _stream._runUserCode(() => test(element), (bool isMatch) => {
                        if (isMatch) {
                            _stream._cancelAndValue(subscription, future, true);
                        }
                    }, (e) => _stream._cancelAndErrorClosure(subscription, future)(e));
                },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => { future._complete(false); },
                cancelOnError: true);
            return future.to<bool>();
        }

        Future<int> length {
            get {
                _Future future = new _Future();
                int count = 0;
                listen(
                    (_) => { count++; },
                    onError: (e, _) => future._completeError((Exception) e),
                    onDone: () => { future._complete(count); },
                    cancelOnError: true);
                return future.to<int>();
            }
        }

        Future<bool> isEmpty {
            get {
                _Future future = new _Future();
                StreamSubscription<T> subscription = null;
                subscription = listen(
                    (_) => { _stream._cancelAndValue(subscription, future, false); },
                    onError: (e, _) => future._completeError((Exception) e),
                    onDone: () => { future._complete(true); },
                    cancelOnError: true);
                return future.to<bool>();
            }
        }

        public virtual Stream<R> cast<R>() => Stream<T>.castFrom<T, R>(this);

        public Future<List<T>> toList() {
            List<T> result = new List<T>();
            _Future future = new _Future();
            listen(
                (T data) => { result.Add(data); },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => { future._complete(FutureOr.value(result)); },
                cancelOnError: true);
            return future.to<List<T>>();
        }

        public Future<HashSet<T>> toSet() {
            HashSet<T> result = new HashSet<T>();
            _Future future = new _Future();
            listen(
                (T data) => { result.Add(data); },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => { future._complete(FutureOr.value(result)); },
                cancelOnError: true);
            return future.to<HashSet<T>>();
        }

        Future<E> drain<E>(E futureValue) =>
            listen(null, cancelOnError: true).asFuture<E>(futureValue);

        public Stream<T> take(int count) {
            return new _TakeStream<T>(this, count);
        }

        Stream<T> takeWhile(Func<T, bool> test) {
            return new _TakeWhileStream<T>(this, d => test(d));
        }

        Stream<T> skip(int count) {
            return new _SkipStream<T>(this, count);
        }

        Stream<T> skipWhile(Func<T, bool> test) {
            return new _SkipWhileStream<T>(this, d => test(d));
        }

        public Stream<T> distinct(Func<T, T, bool> equals) {
            return new _DistinctStream<T>(this, (d1, d2) => equals(d1, d2));
        }

        Future<T> first {
            get {
                _Future future = new _Future();
                StreamSubscription<T> subscription = null;
                subscription = listen(
                    (T value) => { _stream._cancelAndValue(subscription, future, value); },
                    onError: (e, _) => future._completeError((Exception) e),
                    onDone: () => {
                        try {
                            throw new Exception("IterableElementError.noElement()");
                        }
                        catch (Exception e) {
                            async_._completeWithErrorCallback(future, e);
                        }
                    },
                    cancelOnError: true);
                return future.to<T>();
            }
        }

        Future<T> last {
            get {
                _Future future = new _Future();
                T result = default;
                bool foundResult = false;
                listen(
                    (T value) => {
                        foundResult = true;
                        result = value;
                    },
                    onError: (e, _) => future._completeError((Exception) e),
                    onDone: () => {
                        if (foundResult) {
                            future._complete(FutureOr.value(result));
                            return;
                        }

                        try {
                            throw new Exception("IterableElementError.noElement()");
                        }
                        catch (Exception e) {
                            async_._completeWithErrorCallback(future, e);
                        }
                    },
                    cancelOnError: true);
                return future.to<T>();
            }
        }

        Future<T> single {
            get {
                _Future future = new _Future();
                T result = default;
                bool foundResult = false;
                StreamSubscription<T> subscription = null;
                subscription = listen(
                    (T value) => {
                        if (foundResult) {
                            // This is the second element we get.
                            try {
                                throw new Exception("IterableElementError.tooMany()");
                            }
                            catch (Exception e) {
                                _stream._cancelAndErrorWithReplacement(subscription, future, e);
                            }

                            return;
                        }

                        foundResult = true;
                        result = value;
                    },
                    onError: (e, _) => future._completeError((Exception) e),
                    onDone: () => {
                        if (foundResult) {
                            future._complete(FutureOr.value(result));
                            return;
                        }

                        try {
                            throw new Exception("IterableElementError.noElement()");
                        }
                        catch (Exception e) {
                            async_._completeWithErrorCallback(future, e);
                        }
                    },
                    cancelOnError: true);
                return future.to<T>();
            }
        }

        Future<T> firstWhere(Func<T, bool> test, Func<T> orElse = null) {
            _Future future = new _Future();
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T value) => {
                    _stream._runUserCode(() => test(value), (bool isMatch) => {
                        if (isMatch) {
                            _stream._cancelAndValue(subscription, future, value);
                        }
                    }, (e) => _stream._cancelAndErrorClosure(subscription, future)(e));
                },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => {
                    if (orElse != null) {
                        _stream._runUserCode(orElse, v => future._complete(FutureOr.value(v)), future._completeError);
                        return;
                    }

                    try {
                        throw new Exception("IterableElementError.noElement()");
                    }
                    catch (Exception e) {
                        async_._completeWithErrorCallback(future, e);
                    }
                },
                cancelOnError: true);
            return future.to<T>();
        }

        Future<T> lastWhere(Func<T, bool> test, Func<T> orElse = null) {
            _Future future = new _Future();
            T result = default;
            bool foundResult = false;
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T value) => {
                    _stream._runUserCode(() => true == test(value), (bool isMatch) => {
                        if (isMatch) {
                            foundResult = true;
                            result = value;
                        }
                    }, (e) => _stream._cancelAndErrorClosure(subscription, future)(e));
                },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => {
                    if (foundResult) {
                        future._complete(FutureOr.value(result));
                        return;
                    }

                    if (orElse != null) {
                        _stream._runUserCode(orElse, v => future._complete(FutureOr.value(v)), future._completeError);
                        return;
                    }

                    try {
                        throw new Exception("IterableElementError.noElement()");
                    }
                    catch (Exception e) {
                        async_._completeWithErrorCallback(future, e);
                    }
                },
                cancelOnError: true);
            return future.to<T>();
        }

        Future<T> singleWhere(Func<T, bool> test, Func<T> orElse = null) {
            _Future future = new _Future();
            T result = default;
            bool foundResult = false;
            StreamSubscription<T> subscription = null;
            subscription = listen(
                (T value) => {
                    _stream._runUserCode(() => true == test(value), (bool isMatch) => {
                        if (isMatch) {
                            if (foundResult) {
                                try {
                                    throw new Exception("IterableElementError.tooMany()");
                                }
                                catch (Exception e) {
                                    _stream._cancelAndErrorWithReplacement(subscription, future, e);
                                }

                                return;
                            }

                            foundResult = true;
                            result = value;
                        }
                    }, (e) => _stream._cancelAndErrorClosure(subscription, future)(e));
                },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => {
                    if (foundResult) {
                        future._complete(FutureOr.value(result));
                        return;
                    }

                    try {
                        if (orElse != null) {
                            _stream._runUserCode(orElse, v => future._complete(FutureOr.value(v)),
                                future._completeError);
                            return;
                        }

                        throw new Exception("IterableElementError.noElement()");
                    }
                    catch (Exception e) {
                        async_._completeWithErrorCallback(future, e);
                    }
                },
                cancelOnError: true);
            return future.to<T>();
        }

        Future<T> elementAt(int index) {
            // ArgumentError.checkNotNull(index, "index");
            // RangeError.checkNotNegative(index, "index");
            _Future future = new _Future();
            StreamSubscription<T> subscription = null;
            int elementIndex = 0;
            subscription = listen(
                (T value) => {
                    if (index == elementIndex) {
                        _stream._cancelAndValue(subscription, future, value);
                        return;
                    }

                    elementIndex += 1;
                },
                onError: (e, _) => future._completeError((Exception) e),
                onDone: () => {
                    future._completeError(
                        new Exception($"exception {index} null, {elementIndex}")
                        // new RangeError.index(index, this, "index", null, elementIndex)
                    );
                },
                cancelOnError: true);
            return future.to<T>();
        }

        public Stream<T> timeout(TimeSpan timeLimit, Action<EventSink<T>> onTimeout) {
            _StreamControllerBase<T> controller = null;
            // The following variables are set on listen.
            StreamSubscription<T> subscription = null;
            Timer timer = null;
            Zone zone = null;
            _stream._TimerCallback timeout = null;

            Action<T> onData = (T evt) => {
                timer.cancel();
                timer = zone.createTimer(timeLimit, () => {
                    timeout();
                    return default;
                });
                // It might close the stream and cancel timer, so create recuring Timer
                // before calling into add();
                // issue: https://github.com/dart-lang/sdk/issues/37565
                controller.add(evt);
            };

            Action<object, string> onError = (object error, string stack) => {
                timer.cancel();
                D.assert(controller is _StreamController<T> ||
                         controller is _BroadcastStreamController<T>);
                Exception e = error as Exception;
                controller._addError(e, e.StackTrace); // Avoid Zone error replacement.
                timer = zone.createTimer(timeLimit, () => {
                    timeout();
                    return default;
                });
            };

            Action onDone = () => {
                timer.cancel();
                controller.close();
            };

            Action onListen = () => {
                // This is the onListen callback for of controller.
                // It runs in the same zone that the subscription was created in.
                // Use that zone for creating timers and running the onTimeout
                // callback.
                zone = Zone.current;
                if (onTimeout == null) {
                    timeout = () => {
                        controller.addError(
                            new TimeoutException("No stream event", timeLimit), null);
                    };
                }
                else {
                    // TODO(floitsch): the return type should be 'void', and the type
                    // should be inferred.
                    var registeredOnTimeout =
                        zone.registerUnaryCallback((o) => {
                            onTimeout((EventSink<T>) o);
                            return default;
                        });
                    var wrapper = new _ControllerEventSinkWrapper<T>(null);
                    timeout = () => {
                        wrapper._sink = controller; // Only valid during call.
                        zone.runUnaryGuarded(registeredOnTimeout, wrapper);
                        wrapper._sink = null;
                    };
                }

                subscription = listen(onData, onError: onError, onDone: onDone);
                timer = zone.createTimer(timeLimit, () => {
                    timeout();
                    return default;
                });
            };

            Future onCancel() {
                timer.cancel();
                Future result = subscription.cancel();
                subscription = null;
                return result;
            }

            controller = isBroadcast
                ? (_StreamControllerBase<T>) new _SyncBroadcastStreamController<T>(() => onListen(), () => onCancel())
                : new _SyncStreamController<T>(() => onListen(), () => {
                    // Don't null the timer, onCancel may call cancel again.
                    timer.cancel();
                    subscription.pause();
                }, () => {
                    subscription.resume();
                    timer = zone.createTimer(timeLimit, () => {
                        timeout();
                        return default;
                    });
                }, onCancel);
            return controller.stream;
        }
    }

    public abstract class StreamSubscription<T> {
        public abstract Future cancel();

        public abstract void onData(Action<T> handleData);

        public abstract void onError(Action<object, string> action);

        public abstract void onDone(Action handleDone);

        public abstract void pause(Future resumeSignal = null);

        public abstract void resume();

        public virtual bool isPaused { get; }

        public abstract Future<E> asFuture<E>(E futureValue);
    }

    public abstract class EventSink<T> : Sink<T> {
        // public abstract void add(T evt);

        public abstract void addError(object error, string stackTrace);

        // void close();
    }

// /** [Stream] wrapper that only exposes the [Stream] interface. */
    public class StreamView<T> : Stream<T> {
        readonly Stream<T> _stream;

        public StreamView(Stream<T> stream) : base() {
            _stream = stream;
        }

        public override bool isBroadcast {
            get { return _stream.isBroadcast; }
        }

        public override Stream<T> asBroadcastStream(Action<StreamSubscription<T>> onListen = null,
            Action<StreamSubscription<T>> onCancel = null)
            =>
                _stream.asBroadcastStream(onListen: onListen, onCancel: onCancel);

        public override StreamSubscription<T> listen(Action<T> onData, Action<object, string> onError = null,
            Action onDone = null, bool cancelOnError = false) {
            return _stream.listen(onData,
                onError: onError, onDone: onDone, cancelOnError: cancelOnError);
        }
    }

    public interface StreamConsumer<S> {
        Future addStream(Stream<S> stream);

        Future close();
    }

    public abstract class StreamSink<S> : EventSink<S>, StreamConsumer<S> {
        // Future close();

        public virtual Future done { get; }

        public virtual Future addStream(Stream<S> stream) {
            throw new System.NotImplementedException();
        }

        // public Future closeConsumer() {
        //     throw new System.NotImplementedException();
        // }
    }

    public abstract class StreamTransformer<S, T> {
        // c# does not support change constructor
        public static StreamTransformer<U, V> create<U, V>(_async._SubscriptionTransformer<U, V> onListen)
            {
            return new _StreamSubscriptionTransformer<U, V>(onListen);
        }


        public static StreamTransformer<S, T> fromHandlers(
            _stream._TransformDataHandler<S, T> handleData = null,
            _stream._TransformErrorHandler<T> handleError = null,
            _stream._TransformDoneHandler<T> handleDone = null) {
            return new _StreamHandlerTransformer<S, T>(handleData, handleError, handleDone);
        }

        // @Since("2.1")
        public static StreamTransformer<S, T> fromBind(Func<Stream<S>, Stream<T>> bind) {
            return new _StreamBindTransformer<S, T>(bind);
        }

        public static StreamTransformer<TS, TT> castFrom<SS, ST, TS, TT>(
            StreamTransformer<SS, ST> source) {
            return new CastStreamTransformer<SS, ST, TS, TT>(source);
        }

        public abstract Stream<T> bind(Stream<S> stream);

        public abstract StreamTransformer<RS, RT> cast<RS, RT>();
    }

    public abstract class StreamTransformerBase<S, T> : StreamTransformer<S, T> {
        public StreamTransformerBase() {
        }

        public override StreamTransformer<RS, RT> cast<RS, RT>() =>
            StreamTransformer<RS, RT>.castFrom<S, T, RS, RT>(this);
    }

    public abstract class StreamIterator<T> {
        /** Create a [StreamIterator] on [stream]. */
        public static StreamIterator<T> Create(Stream<T> stream)
            // TODO(lrn): use redirecting factory constructor when type
            // arguments are supported.
            =>
                new _StreamIterator<T>(stream);

        public abstract Future<bool> moveNext();

        T current { get; }

        public abstract Future cancel();
    }

    internal class _ControllerEventSinkWrapper<T> : EventSink<T> {
        internal EventSink<T> _sink;

        internal _ControllerEventSinkWrapper(EventSink<T> _sink) {
            this._sink = _sink;
        }

        public override void add(T data) {
            _sink.add(data);
        }

        public override void addError(object error, string stackTrace) {
            _sink.addError(error, stackTrace);
        }

        public override Future close() {
            _sink.close();
            return Future._nullFuture;
        }
    }
}