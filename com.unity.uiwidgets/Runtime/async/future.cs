using System;
using System.Collections.Generic;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.async {
    public struct FutureOr {
        public object v;
        public Future f;

        public bool isFuture => f != null;

        public static FutureOr value(object value) {
            return new FutureOr {v = value};
        }

        public static FutureOr future(Future future) {
            return new FutureOr {f = future};
        }

        public static readonly FutureOr nil = value(null);

        public static implicit operator FutureOr(Future f) => future(f);

        public static implicit operator FutureOr(bool v) => value(v);

        public static implicit operator FutureOr(int v) => value(v);

        public static implicit operator FutureOr(long v) => value(v);

        public static implicit operator FutureOr(float v) => value(v);

        public static implicit operator FutureOr(double v) => value(v);

        public static implicit operator FutureOr(string v) => value(v);

        public static implicit operator FutureOr(byte[] v) => value(v);
        
        public static implicit operator FutureOr(RoutePopDisposition v) => value(v);
        public static implicit operator FutureOr(Dictionary<Type, object> v) => value(v);
    }

    public abstract class Future {
        internal static readonly _Future _nullFuture = _Future.zoneValue(null, Zone.root);

        internal static readonly _Future _falseFuture = _Future.zoneValue(false, Zone.root);

        public static Future create(Func<FutureOr> computation) {
            _Future result = new _Future();
            Timer.run(() => {
                try {
                    result._complete(computation());
                }
                catch (Exception e) {
                    async_._completeWithErrorCallback(result, e);
                }

                return null;
            });
            return result;
        }

        public static Future microtask(Func<FutureOr> computation) {
            _Future result = new _Future();
            async_.scheduleMicrotask(() => {
                try {
                    result._complete(computation());
                }
                catch (Exception e) {
                    async_._completeWithErrorCallback(result, e);
                }

                return null;
            });
            return result;
        }

        public static Future sync(Func<FutureOr> computation) {
            try {
                var result = computation();
                if (result.isFuture) {
                    return result.f;
                }
                else {
                    return _Future.value(result);
                }
            }
            catch (Exception error) {
                var future = new _Future();
                AsyncError replacement = Zone.current.errorCallback(error);
                if (replacement != null) {
                    future._asyncCompleteError(async_._nonNullError(replacement.InnerException));
                }
                else {
                    future._asyncCompleteError(error);
                }

                return future;
            }
        }

        public static Future value(FutureOr value = default) {
            return _Future.immediate(value);
        }

        public static Future error(Exception error) {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            if (!ReferenceEquals(Zone.current, async_._rootZone)) {
                AsyncError replacement = Zone.current.errorCallback(error);
                if (replacement != null) {
                    error = async_._nonNullError(replacement.InnerException);
                }
            }

            return _Future.immediateError(error);
        }

        public static Future delayed(TimeSpan duration, Func<FutureOr> computation = null) {
            _Future result = new _Future();
            Timer.create(duration, () => {
                if (computation == null) {
                    result._complete();
                }
                else {
                    try {
                        result._complete(computation());
                    }
                    catch (Exception e) {
                        async_._completeWithErrorCallback(result, e);
                    }
                }

                return null;
            });
            return result;
        }

        public static Future wait<T>(IEnumerable<Future> futures, bool eagerError = false, Action<T> cleanUp = null) {
            _Future result = new _Future();
            List<T> values = null; // Collects the values. Set to null on error.
            int remaining = 0; // How many futures are we waiting for.
            Exception error = null; // The first error from a future.

            Func<Exception, FutureOr> handleError = (Exception theError) => {
                remaining--;
                if (values != null) {
                    if (cleanUp != null) {
                        foreach (var value in values) {
                            if (value != null) {
                                // Ensure errors from cleanUp are uncaught.
                                sync(() => {
                                    cleanUp(value);
                                    return FutureOr.nil;
                                });
                            }
                        }
                    }

                    values = null;
                    if (remaining == 0 || eagerError) {
                        result._completeError(theError);
                    }
                    else {
                        error = theError;
                    }
                }
                else if (remaining == 0 && !eagerError) {
                    result._completeError(error);
                }

                return FutureOr.nil;
            };

            try {
                // As each future completes, put its value into the corresponding
                // position in the list of values.
                foreach (var future in futures) {
                    int pos = remaining;
                    future.then((object value) => {
                        remaining--;
                        if (values != null) {
                            values[pos] = (T)value;
                            if (remaining == 0) {
                                result._completeWithValue(values);
                            }
                        }
                        else {
                            if (cleanUp != null && value != null) {
                                // Ensure errors from cleanUp are uncaught.
                                sync(() => {
                                    cleanUp((T) value);
                                    return FutureOr.nil;
                                });
                            }

                            if (remaining == 0 && !eagerError) {
                                result._completeError(error);
                            }
                        }

                        return FutureOr.nil;
                    }, onError: handleError);
                    // Increment the 'remaining' after the call to 'then'.
                    // If that call throws, we don't expect any future callback from
                    // the future, and we also don't increment remaining.
                    remaining++;
                }

                if (remaining == 0) {
                    return value(FutureOr.value(new List<T>()));
                }

                values = new List<T>(new T[remaining]);
            }
            catch (Exception e) {
                // The error must have been thrown while iterating over the futures
                // list, or while installing a callback handler on the future.
                if (remaining == 0 || eagerError) {
                    // Throw a new Future.error.
                    // Don't just call `result._completeError` since that would propagate
                    // the error too eagerly, not giving the callers time to install
                    // error handlers.
                    // Also, don't use `_asyncCompleteError` since that one doesn't give
                    // zones the chance to intercept the error.
                    return Future.error(e);
                }
                else {
                    // Don't allocate a list for values, thus indicating that there was an
                    // error.
                    // Set error to the caught exception.
                    error = e;
                }
            }

            return result;
        }

        public static Future any(IEnumerable<Future> futures) {
            var completer = Completer.sync();
            Func<object, FutureOr> onValue = (object value) => {
                if (!completer.isCompleted) completer.complete(FutureOr.value(value));
                return FutureOr.nil;
            };

            Func<Exception, FutureOr> onError = (Exception error) => {
                if (!completer.isCompleted) completer.completeError(error);
                return FutureOr.nil;
            };

            foreach (var future in futures) {
                future.then(onValue, onError: onError);
            }

            return completer.future;
        }

        public static Future forEach<T>(IEnumerable<T> elements, Func<T, FutureOr> action) {
            var iterator = elements.GetEnumerator();
            return doWhile(() => {
                if (!iterator.MoveNext()) return false;

                var result = action(iterator.Current);
                if (result.isFuture) return FutureOr.future(result.f.then(_kTrue));
                return true;
            });
        }

        static readonly Func<object, FutureOr> _kTrue = (_) => true;

        public static Future doWhile(Func<FutureOr> action) {
            _Future doneSignal = new _Future();
            ZoneUnaryCallback nextIteration = null;
            // Bind this callback explicitly so that each iteration isn't bound in the
            // context of all the previous iterations' callbacks.
            // This avoids, e.g., deeply nested stack traces from the stack trace
            // package.
            nextIteration = Zone.current.bindUnaryCallbackGuarded((object keepGoingObj) => {
                bool keepGoing = (bool) keepGoingObj;
                while (keepGoing) {
                    FutureOr result;
                    try {
                        result = action();
                    }
                    catch (Exception error) {
                        // Cannot use _completeWithErrorCallback because it completes
                        // the future synchronously.
                        async_._asyncCompleteWithErrorCallback(doneSignal, error);
                        return null;
                    }

                    if (result.isFuture) {
                        result.f.then((value) => {
                            nextIteration((bool) value);
                            return FutureOr.nil;
                        }, onError: error => {
                            doneSignal._completeError(error);
                            return FutureOr.nil;
                        });
                        return null;
                    }

                    keepGoing = (bool) result.v;
                }

                doneSignal._complete();
                return null;
            });

            nextIteration(true);
            return doneSignal;
        }

        public abstract Future then(Func<object, FutureOr> onValue, Func<Exception, FutureOr> onError = null);

        public Future then(Action<object> onValue, Func<Exception, FutureOr> onError = null) {
            return then(v => {
                onValue(v);
                return FutureOr.nil;
            }, onError);
        }

        public abstract Future catchError(Func<Exception, FutureOr> onError, Func<Exception, bool> test = null);

        public Future catchError(Action<Exception> onError, Func<Exception, bool> test = null) {
            return catchError(e => {
                onError(e);
                return FutureOr.nil;
            }, test);
        }

        public abstract Future whenComplete(Func<FutureOr> action);

        public Future whenComplete(Action action) {
            return whenComplete(() => {
                action();
                return FutureOr.nil;
            });
        }

        // public abstract Stream asStream();

        public abstract Future timeout(TimeSpan timeLimit, Func<FutureOr> onTimeout = null);

        public Future<T> to<T>() {
            if (this is Future<T>) {
                return (Future<T>) this;
            }

            return new Future<T>(this);
        }
    }

    public class Future<T> : Future {
        readonly Future _future;

        public Future(Future future) {
            _future = future;
        }

        public override Future then(Func<object, FutureOr> onValue, Func<Exception, FutureOr> onError = null) {
            return _future.then(onValue, onError);
        }

        public Future then_(Func<T, FutureOr> onValue, Func<Exception, FutureOr> onError = null) {
            return _future.then(obj => onValue((T) obj), onError);
        }

        public Future<R> then_<R>(Func<T, FutureOr> onValue, Func<Exception, FutureOr> onError = null) {
            return _future.then(obj => onValue((T) obj), onError).to<R>();
        }

        public Future then_(Action<T> onValue, Func<Exception, FutureOr> onError = null) {
            return _future.then(obj => onValue((T) obj), onError);
        }

        public override Future catchError(Func<Exception, FutureOr> onError, Func<Exception, bool> test = null) {
            return _future.catchError(onError, test);
        }

        public override Future whenComplete(Func<FutureOr> action) {
            return _future.whenComplete(action);
        }

        public override Future timeout(TimeSpan timeLimit, Func<FutureOr> onTimeout = null) {
            return _future.timeout(timeLimit, onTimeout);
        }
    }

    public class TimeoutException : Exception {
        public readonly TimeSpan? duration;

        public TimeoutException(string message, TimeSpan? duration = null) : base(message) {
            this.duration = duration;
        }

        public override string ToString() {
            string result = "TimeoutException";
            if (duration != null) result = $"TimeoutException after {duration}";
            if (Message != null) result = $"result: {Message}";
            return result;
        }
    }

    public abstract class Completer {
        public static Completer create() => new _AsyncCompleter();

        public static Completer sync() => new _SyncCompleter();

        public abstract Future future { get; }

        public abstract void complete(FutureOr value = default);

        public abstract void completeError(Exception error);
        public abstract bool isCompleted { get; }
    }

    public static partial class async_ {
        internal static void _completeWithErrorCallback(_Future result, Exception error) {
            AsyncError replacement = Zone.current.errorCallback(error);
            if (replacement != null) {
                error = _nonNullError(replacement.InnerException);
            }

            result._completeError(error);
        }

        internal static void _asyncCompleteWithErrorCallback(_Future result, Exception error) {
            AsyncError replacement = Zone.current.errorCallback(error);
            if (replacement != null) {
                error = _nonNullError(replacement.InnerException);
            }

            result._asyncCompleteError(error);
        }

        internal static Exception _nonNullError(Exception error) =>
            error ?? new Exception("Throw of null.");
    }
}