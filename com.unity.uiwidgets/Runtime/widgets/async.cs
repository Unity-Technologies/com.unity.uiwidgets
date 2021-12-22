using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public abstract class StreamBuilderBase<T, S> : StatefulWidget {
        public StreamBuilderBase(Key key = null, Stream<T> stream = null) : base(key: key) {
            this.stream = stream;
        }

        public readonly Stream<T> stream;

        public abstract S initial();

        public virtual S afterConnected(S current) => current;

        public abstract S afterData(S current, T data);

        public virtual S afterError(S current, object error) => current;

        public virtual S afterDone(S current) => current;

        public virtual S afterDisconnected(S current) => current;

        public abstract Widget build(BuildContext context, S currentSummary);

        public override State createState() => new _StreamBuilderBaseState<T, S>();
    }

    class _StreamBuilderBaseState<T, S> : State<StreamBuilderBase<T, S>> {
        StreamSubscription<T> _subscription;
        S _summary;

        public override void initState() {
            base.initState();
            _summary = widget.initial();
            _subscribe();
        }

        public override void didUpdateWidget(StatefulWidget statefulWidget) {
            StreamBuilderBase<T, S> oldWidget = statefulWidget as StreamBuilderBase<T, S>;
            if (oldWidget == null) {
                return;
            }

            base.didUpdateWidget(statefulWidget);
            if (oldWidget != null) {
                if (oldWidget.stream != widget.stream) {
                    if (_subscription != null) {
                        _unsubscribe();
                        _summary = widget.afterDisconnected(_summary);
                    }

                    _subscribe();
                }
            }
        }

        public override Widget build(BuildContext context) => widget.build(context, _summary);

        public override void dispose() {
            _unsubscribe();
            base.dispose();
        }

        void _subscribe() {
            if (widget.stream != null) {
                _subscription = widget.stream.listen(
                    (T data) => { setState(() => { _summary = widget.afterData(_summary, data); }); },
                    onError: (object error, string stackTrace) => {
                        setState(() => { _summary = widget.afterError(_summary, error); });
                    },
                    onDone: () => { setState(() => { _summary = widget.afterDone(_summary); }); });
                _summary = widget.afterConnected(_summary);
            }
        }

        void _unsubscribe() {
            if (_subscription != null) {
                _subscription.cancel();
                _subscription = null;
            }
        }
    }

    public enum ConnectionState {
        none,

        waiting,

        active,

        done,
    }

//@immutable
    public class AsyncSnapshot<T> : IEquatable<AsyncSnapshot<T>> {
        AsyncSnapshot(ConnectionState connectionState, object data, object error) {
            D.assert(!(data != null && error != null));
            this.connectionState = connectionState;
            this.data = (T) data;
            this.error = error;
        }

        public static AsyncSnapshot<object> nothing() {
            return new AsyncSnapshot<object>(ConnectionState.none, null, null);
        }

        public static AsyncSnapshot<T> withData(ConnectionState state, T data) {
            return new AsyncSnapshot<T>(state, data, null);
        }

        public static AsyncSnapshot<T> withError(ConnectionState state, object error) {
            return new AsyncSnapshot<T>(state, null, error);
        }

        public readonly ConnectionState connectionState;

        public readonly T data;

        public T requireData {
            get {
                if (hasData)
                    return data;
                if (hasError)
                    //TODO: not sure if cast works
                    throw (Exception) error;
                throw new Exception("Snapshot has neither data nor error");
            }
        }

        public readonly object error;

        public AsyncSnapshot<T> inState(ConnectionState state) {
            return new AsyncSnapshot<T>(state, data, error);
        }

        public bool hasData {
            get => data != null;
        }

        public bool hasError {
            get => error != null;
        }

        public override string ToString() =>
            $"{foundation_.objectRuntimeType(this, "AsyncSnapshot")}({connectionState}, {data}, {error})";

        public static bool operator ==(AsyncSnapshot<T> left, AsyncSnapshot<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(AsyncSnapshot<T> left, AsyncSnapshot<T> right) {
            return !Equals(left, right);
        }

        public bool Equals(AsyncSnapshot<T> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return connectionState == other.connectionState && EqualityComparer<T>.Default.Equals(data, other.data) &&
                   Equals(error, other.error);
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

            return Equals((AsyncSnapshot<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) connectionState;
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(data);
                hashCode = (hashCode * 397) ^ (error != null ? error.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public static partial class _async {
        public delegate Widget AsyncWidgetBuilder<T>(BuildContext context, AsyncSnapshot<T> snapshot);
    }

// TODO(ianh): remove unreachable code above once https://github.com/dart-lang/linter/issues/1139 is fixed
    public class StreamBuilder<T> : StreamBuilderBase<T, AsyncSnapshot<T>> {
        public StreamBuilder(
            _async.AsyncWidgetBuilder<T> builder,
            Key key = null,
            T initialData = default,
            Stream<T> stream = null
        ) : base(key: key, stream: stream) {
            D.assert(builder != null);
            this.builder = builder;
            this.initialData = initialData;
        }

        public readonly _async.AsyncWidgetBuilder<T> builder;

        public readonly T initialData;


        public override
            AsyncSnapshot<T> initial() => AsyncSnapshot<T>.withData(ConnectionState.none, initialData);


        public override
            AsyncSnapshot<T> afterConnected(AsyncSnapshot<T> current) => current.inState(ConnectionState.waiting);

        public override
            AsyncSnapshot<T> afterData(AsyncSnapshot<T> current, T data) {
            return AsyncSnapshot<T>.withData(ConnectionState.active, data);
        }

        public override
            AsyncSnapshot<T> afterError(AsyncSnapshot<T> current, object error) {
            return AsyncSnapshot<T>.withError(ConnectionState.active, error);
        }

        public override
            AsyncSnapshot<T> afterDone(AsyncSnapshot<T> current) => current.inState(ConnectionState.done);

        public override
            AsyncSnapshot<T> afterDisconnected(AsyncSnapshot<T> current) => current.inState(ConnectionState.none);

        public override
            Widget build(BuildContext context, AsyncSnapshot<T> currentSummary) => builder(context, currentSummary);
    }

// TODO(ianh): remove unreachable code above once https://github.com/dart-lang/linter/issues/1141 is fixed
    public class FutureBuilder<T> : StatefulWidget {
        public FutureBuilder(
            _async.AsyncWidgetBuilder<T> builder,
            Key key = null,
            Future<T> future = null,
            T initialData = default
        ) :
            base(key: key) {
            D.assert(builder != null);
            this.builder = builder;
            this.future = future;
            this.initialData = initialData;
        }

        public readonly Future<T> future;

        public readonly _async.AsyncWidgetBuilder<T> builder;

        public readonly T initialData;

        public override
            State createState() => new _FutureBuilderState<T>();
    }

    class _FutureBuilderState<T> : State<FutureBuilder<T>> {
        object _activeCallbackIdentity;
        AsyncSnapshot<T> _snapshot;

        public override
            void initState() {
            base.initState();
            _snapshot = AsyncSnapshot<T>.withData(ConnectionState.none, widget.initialData);
            _subscribe();
        }

        public override
            void didUpdateWidget(StatefulWidget statefulWidget) {
            var oldWidget = statefulWidget as FutureBuilder<T>;
            if (oldWidget == null) {
                return;
            }

            base.didUpdateWidget(oldWidget);
            if (oldWidget.future != widget.future) {
                if (_activeCallbackIdentity != null) {
                    _unsubscribe();
                    _snapshot = _snapshot.inState(ConnectionState.none);
                }

                _subscribe();
            }
        }

        public override
            Widget build(BuildContext context) => widget.builder(context, _snapshot);

        public override
            void dispose() {
            _unsubscribe();
            base.dispose();
        }

        void _subscribe() {
            if (widget.future != null) {
                object callbackIdentity = new object();
                _activeCallbackIdentity = callbackIdentity;
                widget.future.then((object dataIn) => {
                    var data = (T) dataIn;
                    if (_activeCallbackIdentity == callbackIdentity) {
                        setState(() => { _snapshot = AsyncSnapshot<T>.withData(ConnectionState.done, data); });
                    }
                }, onError: (Exception error) => {
                    if (_activeCallbackIdentity == callbackIdentity) {
                        setState(() => { _snapshot = AsyncSnapshot<T>.withError(ConnectionState.done, error); });
                    }

                    return FutureOr.nil;
                });
                _snapshot = _snapshot.inState(ConnectionState.waiting);
            }
        }

        void _unsubscribe() {
            _activeCallbackIdentity = null;
        }
    }
}