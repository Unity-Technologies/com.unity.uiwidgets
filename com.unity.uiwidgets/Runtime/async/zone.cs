using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.async {
    public delegate object ZoneCallback();

    public delegate object ZoneUnaryCallback(object arg);

    public delegate object ZoneBinaryCallback(object arg1, object arg2);


    public delegate void HandleUncaughtErrorHandler(Zone self, ZoneDelegate parent, Zone zone, Exception error);

    public delegate object RunHandler(
        Zone self, ZoneDelegate parent, Zone zone, ZoneCallback f);

    public delegate object RunUnaryHandler(
        Zone self, ZoneDelegate parent, Zone zone, ZoneUnaryCallback f, object arg);

    public delegate object RunBinaryHandler(Zone self, ZoneDelegate parent,
        Zone zone, ZoneBinaryCallback f, object arg1, object arg2);


    public delegate ZoneCallback RegisterCallbackHandler(
        Zone self, ZoneDelegate parent, Zone zone, ZoneCallback f);

    public delegate ZoneUnaryCallback RegisterUnaryCallbackHandler(
        Zone self, ZoneDelegate parent, Zone zone, ZoneUnaryCallback f);

    public delegate ZoneBinaryCallback RegisterBinaryCallbackHandler(Zone self,
        ZoneDelegate parent, Zone zone, ZoneBinaryCallback f);


    public delegate AsyncError ErrorCallbackHandler(Zone self, ZoneDelegate parent,
        Zone zone, Exception error);

    public delegate void ScheduleMicrotaskHandler(
        Zone self, ZoneDelegate parent, Zone zone, ZoneCallback f);

    public delegate Timer CreateTimerHandler(
        Zone self, ZoneDelegate parent, Zone zone, TimeSpan duration, ZoneCallback f);

    public delegate Timer CreatePeriodicTimerHandler(Zone self, ZoneDelegate parent,
        Zone zone, TimeSpan period, ZoneUnaryCallback f);

    public delegate void PrintHandler(
        Zone self, ZoneDelegate parent, Zone zone, string line);

    public delegate Zone ForkHandler(Zone self, ZoneDelegate parent, Zone zone,
        ZoneSpecification specification, Hashtable zoneValues);

    public class AsyncError : Exception {
        public AsyncError() {
        }

        protected AsyncError(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public AsyncError(string message) : base(message) {
        }

        public AsyncError(string message, Exception innerException) : base(message, innerException) {
        }

        public AsyncError(Exception innerException) : base(null, innerException) {
        }

        public static string defaultStackTrace(object error) {
            if (error is Exception ex) {
                return ex.StackTrace;
            }

            return "";
        }
    }

    struct _ZoneFunction<T> where T : Delegate {
        public readonly _Zone zone;
        public readonly T function;

        internal _ZoneFunction(_Zone zone, T function) {
            this.zone = zone;
            this.function = function;
        }
    }

    struct _RunNullaryZoneFunction {
        public readonly _Zone zone;
        public readonly RunHandler function;

        internal _RunNullaryZoneFunction(_Zone zone, RunHandler function) {
            this.zone = zone;
            this.function = function;
        }
    }

    struct _RunUnaryZoneFunction {
        public readonly _Zone zone;
        public readonly RunUnaryHandler function;

        internal _RunUnaryZoneFunction(_Zone zone, RunUnaryHandler function) {
            this.zone = zone;
            this.function = function;
        }
    }

    struct _RunBinaryZoneFunction {
        public readonly _Zone zone;
        public readonly RunBinaryHandler function;

        internal _RunBinaryZoneFunction(_Zone zone, RunBinaryHandler function) {
            this.zone = zone;
            this.function = function;
        }
    }

    struct _RegisterNullaryZoneFunction {
        public readonly _Zone zone;
        public readonly RegisterCallbackHandler function;

        internal _RegisterNullaryZoneFunction(_Zone zone, RegisterCallbackHandler function) {
            this.zone = zone;
            this.function = function;
        }
    }

    struct _RegisterUnaryZoneFunction {
        public readonly _Zone zone;
        public readonly RegisterUnaryCallbackHandler function;

        internal _RegisterUnaryZoneFunction(_Zone zone, RegisterUnaryCallbackHandler function) {
            this.zone = zone;
            this.function = function;
        }
    }

    struct _RegisterBinaryZoneFunction {
        public readonly _Zone zone;
        public readonly RegisterBinaryCallbackHandler function;

        internal _RegisterBinaryZoneFunction(_Zone zone, RegisterBinaryCallbackHandler function) {
            this.zone = zone;
            this.function = function;
        }
    }

    public class ZoneSpecification {
        public ZoneSpecification(
            HandleUncaughtErrorHandler handleUncaughtError = null,
            RunHandler run = null,
            RunUnaryHandler runUnary = null,
            RunBinaryHandler runBinary = null,
            RegisterCallbackHandler registerCallback = null,
            RegisterUnaryCallbackHandler registerUnaryCallback = null,
            RegisterBinaryCallbackHandler registerBinaryCallback = null,
            ErrorCallbackHandler errorCallback = null,
            ScheduleMicrotaskHandler scheduleMicrotask = null,
            CreateTimerHandler createTimer = null,
            CreatePeriodicTimerHandler createPeriodicTimer = null,
            PrintHandler print = null,
            ForkHandler fork = null) {
            this.handleUncaughtError = handleUncaughtError;
            this.run = run;
            this.runUnary = runUnary;
            this.runBinary = runBinary;
            this.registerCallback = registerCallback;
            this.registerUnaryCallback = registerUnaryCallback;
            this.registerBinaryCallback = registerBinaryCallback;
            this.errorCallback = errorCallback;
            this.scheduleMicrotask = scheduleMicrotask;
            this.createTimer = createTimer;
            this.createPeriodicTimer = createPeriodicTimer;
            this.print = print;
            this.fork = fork;
        }

        public static ZoneSpecification from(
            ZoneSpecification other,
            HandleUncaughtErrorHandler handleUncaughtError = null,
            RunHandler run = null,
            RunUnaryHandler runUnary = null,
            RunBinaryHandler runBinary = null,
            RegisterCallbackHandler registerCallback = null,
            RegisterUnaryCallbackHandler registerUnaryCallback = null,
            RegisterBinaryCallbackHandler registerBinaryCallback = null,
            ErrorCallbackHandler errorCallback = null,
            ScheduleMicrotaskHandler scheduleMicrotask = null,
            CreateTimerHandler createTimer = null,
            CreatePeriodicTimerHandler createPeriodicTimer = null,
            PrintHandler print = null,
            ForkHandler fork = null) {
            return new ZoneSpecification(
                handleUncaughtError: handleUncaughtError ?? other.handleUncaughtError,
                run: run ?? other.run,
                runUnary: runUnary ?? other.runUnary,
                runBinary: runBinary ?? other.runBinary,
                registerCallback: registerCallback ?? other.registerCallback,
                registerUnaryCallback:
                registerUnaryCallback ?? other.registerUnaryCallback,
                registerBinaryCallback:
                registerBinaryCallback ?? other.registerBinaryCallback,
                errorCallback: errorCallback ?? other.errorCallback,
                scheduleMicrotask: scheduleMicrotask ?? other.scheduleMicrotask,
                createTimer: createTimer ?? other.createTimer,
                createPeriodicTimer: createPeriodicTimer ?? other.createPeriodicTimer,
                print: print ?? other.print,
                fork: fork ?? other.fork
            );
        }

        public HandleUncaughtErrorHandler handleUncaughtError { get; }
        public RunHandler run { get; }
        public RunUnaryHandler runUnary { get; }
        public RunBinaryHandler runBinary { get; }
        public RegisterCallbackHandler registerCallback { get; }
        public RegisterUnaryCallbackHandler registerUnaryCallback { get; }
        public RegisterBinaryCallbackHandler registerBinaryCallback { get; }
        public ErrorCallbackHandler errorCallback { get; }
        public ScheduleMicrotaskHandler scheduleMicrotask { get; }
        public CreateTimerHandler createTimer { get; }
        public CreatePeriodicTimerHandler createPeriodicTimer { get; }
        public PrintHandler print { get; }
        public ForkHandler fork { get; }
    }


    public interface ZoneDelegate {
        void handleUncaughtError(Zone zone, Exception error);

        object run(Zone zone, ZoneCallback f);
        object runUnary(Zone zone, ZoneUnaryCallback f, object arg);
        object runBinary(Zone zone, ZoneBinaryCallback f, object arg1, object arg2);

        ZoneCallback registerCallback(Zone zone, ZoneCallback f);
        ZoneUnaryCallback registerUnaryCallback(Zone zone, ZoneUnaryCallback f);
        ZoneBinaryCallback registerBinaryCallback(Zone zone, ZoneBinaryCallback f);

        AsyncError errorCallback(Zone zone, Exception error);
        void scheduleMicrotask(Zone zone, ZoneCallback f);
        Timer createTimer(Zone zone, TimeSpan duration, ZoneCallback f);
        Timer createPeriodicTimer(Zone zone, TimeSpan period, ZoneUnaryCallback f);
        void print(Zone zone, string line);
        Zone fork(Zone zone, ZoneSpecification specification, Hashtable zoneValues);
    }

    public abstract class Zone {
        protected Zone() {
        }

        public static readonly Zone root = async_._rootZone;

        internal static Zone _current = async_._rootZone;
        public static Zone current => _current;

        public abstract void handleUncaughtError(Exception error);
        public abstract Zone parent { get; }
        public abstract Zone errorZone { get; }
        public abstract bool inSameErrorZone(Zone otherZone);
        public abstract Zone fork(ZoneSpecification specification = null, Hashtable zoneValues = null);

        public abstract object run(ZoneCallback action);
        public abstract object runUnary(ZoneUnaryCallback action, object argument);
        public abstract object runBinary(ZoneBinaryCallback action, object argument1, object argument2);
        public abstract object runGuarded(ZoneCallback action);
        public abstract object runUnaryGuarded(ZoneUnaryCallback action, object argument);
        public abstract object runBinaryGuarded(ZoneBinaryCallback action, object argument1, object argument2);

        public abstract ZoneCallback registerCallback(ZoneCallback callback);
        public abstract ZoneUnaryCallback registerUnaryCallback(ZoneUnaryCallback callback);
        public abstract ZoneBinaryCallback registerBinaryCallback(ZoneBinaryCallback fcallback);

        public abstract ZoneCallback bindCallback(ZoneCallback callback);
        public abstract ZoneUnaryCallback bindUnaryCallback(ZoneUnaryCallback callback);
        public abstract ZoneBinaryCallback bindBinaryCallback(ZoneBinaryCallback callback);
        public abstract ZoneCallback bindCallbackGuarded(ZoneCallback callback);
        public abstract ZoneUnaryCallback bindUnaryCallbackGuarded(ZoneUnaryCallback callback);
        public abstract ZoneBinaryCallback bindBinaryCallbackGuarded(ZoneBinaryCallback callback);

        public abstract AsyncError errorCallback(Exception error);
        public abstract void scheduleMicrotask(ZoneCallback callback);
        public abstract Timer createTimer(TimeSpan duration, ZoneCallback callback);
        public abstract Timer createPeriodicTimer(TimeSpan period, ZoneUnaryCallback callback);
        public abstract void print(string line);

        internal static Zone _enter(Zone zone) {
            D.assert(zone != null);
            D.assert(!ReferenceEquals(zone, _current));
            Zone previous = _current;
            _current = zone;
            return previous;
        }

        internal static void _leave(Zone previous) {
            D.assert(previous != null);
            _current = previous;
        }

        public abstract object this[object key] { get; }
    }

    public static partial class async_ {
        internal static ZoneDelegate _parentDelegate(_Zone zone) {
            if (zone.parent == null) return null;
            return zone._parent._delegate;
        }
    }

    class _ZoneDelegate : ZoneDelegate {
        readonly _Zone _delegationTarget;

        internal _ZoneDelegate(_Zone delegationTarget) {
            _delegationTarget = delegationTarget;
        }

        public void handleUncaughtError(Zone zone, Exception error) {
            var implementation = _delegationTarget._handleUncaughtError;
            _Zone implZone = implementation.zone;
            HandleUncaughtErrorHandler handler = implementation.function;
            handler(implZone, async_._parentDelegate(implZone), zone, error);
        }

        public object run(Zone zone, ZoneCallback f) {
            var implementation = _delegationTarget._run;
            _Zone implZone = implementation.zone;
            RunHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, f);
        }

        public object runUnary(Zone zone, ZoneUnaryCallback f, object arg) {
            var implementation = _delegationTarget._runUnary;
            _Zone implZone = implementation.zone;
            RunUnaryHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, f, arg);
        }

        public object runBinary(Zone zone, ZoneBinaryCallback f, object arg1, object arg2) {
            var implementation = _delegationTarget._runBinary;
            _Zone implZone = implementation.zone;
            RunBinaryHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, f, arg1, arg2);
        }

        public ZoneCallback registerCallback(Zone zone, ZoneCallback f) {
            var implementation = _delegationTarget._registerCallback;
            _Zone implZone = implementation.zone;
            RegisterCallbackHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, f);
        }

        public ZoneUnaryCallback registerUnaryCallback(Zone zone, ZoneUnaryCallback f) {
            var implementation = _delegationTarget._registerUnaryCallback;
            _Zone implZone = implementation.zone;
            RegisterUnaryCallbackHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, f);
        }

        public ZoneBinaryCallback registerBinaryCallback(Zone zone, ZoneBinaryCallback f) {
            var implementation = _delegationTarget._registerBinaryCallback;
            _Zone implZone = implementation.zone;
            RegisterBinaryCallbackHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, f);
        }

        public AsyncError errorCallback(Zone zone, Exception error) {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            var implementation = _delegationTarget._errorCallback;
            _Zone implZone = implementation.zone;
            if (ReferenceEquals(implZone, async_._rootZone)) return null;
            ErrorCallbackHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, error);
        }

        public void scheduleMicrotask(Zone zone, ZoneCallback f) {
            var implementation = _delegationTarget._scheduleMicrotask;
            _Zone implZone = implementation.zone;
            ScheduleMicrotaskHandler handler = implementation.function;
            handler(implZone, async_._parentDelegate(implZone), zone, f);
        }

        public Timer createTimer(Zone zone, TimeSpan duration, ZoneCallback f) {
            var implementation = _delegationTarget._createTimer;
            _Zone implZone = implementation.zone;
            CreateTimerHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, duration, f);
        }

        public Timer createPeriodicTimer(Zone zone, TimeSpan period, ZoneUnaryCallback f) {
            var implementation = _delegationTarget._createPeriodicTimer;
            _Zone implZone = implementation.zone;
            CreatePeriodicTimerHandler handler = implementation.function;
            return handler(implZone, async_._parentDelegate(implZone), zone, period, f);
        }

        public void print(Zone zone, string line) {
            var implementation = _delegationTarget._print;
            _Zone implZone = implementation.zone;
            PrintHandler handler = implementation.function;
            handler(implZone, async_._parentDelegate(implZone), zone, line);
        }

        public Zone fork(Zone zone, ZoneSpecification specification, Hashtable zoneValues) {
            var implementation = _delegationTarget._fork;
            _Zone implZone = implementation.zone;
            ForkHandler handler = implementation.function;
            return handler(
                implZone, async_._parentDelegate(implZone), zone, specification, zoneValues);
        }
    }

    abstract class _Zone : Zone {
        protected _Zone() {
        }

        internal abstract _RunNullaryZoneFunction _run { get; }
        internal abstract _RunUnaryZoneFunction _runUnary { get; }
        internal abstract _RunBinaryZoneFunction _runBinary { get; }
        internal abstract _RegisterNullaryZoneFunction _registerCallback { get; }
        internal abstract _RegisterUnaryZoneFunction _registerUnaryCallback { get; }
        internal abstract _RegisterBinaryZoneFunction _registerBinaryCallback { get; }
        internal abstract _ZoneFunction<ErrorCallbackHandler> _errorCallback { get; }
        internal abstract _ZoneFunction<ScheduleMicrotaskHandler> _scheduleMicrotask { get; }
        internal abstract _ZoneFunction<CreateTimerHandler> _createTimer { get; }
        internal abstract _ZoneFunction<CreatePeriodicTimerHandler> _createPeriodicTimer { get; }
        internal abstract _ZoneFunction<PrintHandler> _print { get; }
        internal abstract _ZoneFunction<ForkHandler> _fork { get; }
        internal abstract _ZoneFunction<HandleUncaughtErrorHandler> _handleUncaughtError { get; }
        internal abstract ZoneDelegate _delegate { get; }
        internal abstract Hashtable _map { get; }
        internal abstract _Zone _parent { get; }
        public override Zone parent => _parent;

        public override bool inSameErrorZone(Zone otherZone) {
            return ReferenceEquals(this, otherZone) ||
                   ReferenceEquals(errorZone, otherZone.errorZone);
        }
    }

    class _CustomZone : _Zone {
        internal override _RunNullaryZoneFunction _run { get; }
        internal override _RunUnaryZoneFunction _runUnary { get; }
        internal override _RunBinaryZoneFunction _runBinary { get; }
        internal override _RegisterNullaryZoneFunction _registerCallback { get; }
        internal override _RegisterUnaryZoneFunction _registerUnaryCallback { get; }
        internal override _RegisterBinaryZoneFunction _registerBinaryCallback { get; }
        internal override _ZoneFunction<ErrorCallbackHandler> _errorCallback { get; }
        internal override _ZoneFunction<ScheduleMicrotaskHandler> _scheduleMicrotask { get; }
        internal override _ZoneFunction<CreateTimerHandler> _createTimer { get; }
        internal override _ZoneFunction<CreatePeriodicTimerHandler> _createPeriodicTimer { get; }
        internal override _ZoneFunction<PrintHandler> _print { get; }
        internal override _ZoneFunction<ForkHandler> _fork { get; }
        internal override _ZoneFunction<HandleUncaughtErrorHandler> _handleUncaughtError { get; }

        ZoneDelegate _delegateCache;
        internal override _Zone _parent { get; }

        internal override Hashtable _map { get; }

        internal override ZoneDelegate _delegate {
            get {
                if (_delegateCache != null) return _delegateCache;
                _delegateCache = new _ZoneDelegate(this);
                return _delegateCache;
            }
        }

        internal _CustomZone(_Zone parent, ZoneSpecification specification, Hashtable map) {
            _parent = parent;
            _map = map;

            _run = (specification.run != null)
                ? new _RunNullaryZoneFunction(this, specification.run)
                : parent._run;
            _runUnary = (specification.runUnary != null)
                ? new _RunUnaryZoneFunction(this, specification.runUnary)
                : parent._runUnary;
            _runBinary = (specification.runBinary != null)
                ? new _RunBinaryZoneFunction(this, specification.runBinary)
                : parent._runBinary;
            _registerCallback = (specification.registerCallback != null)
                ? new _RegisterNullaryZoneFunction(this, specification.registerCallback)
                : parent._registerCallback;
            _registerUnaryCallback = (specification.registerUnaryCallback != null)
                ? new _RegisterUnaryZoneFunction(
                    this, specification.registerUnaryCallback)
                : parent._registerUnaryCallback;
            _registerBinaryCallback = (specification.registerBinaryCallback != null)
                ? new _RegisterBinaryZoneFunction(
                    this, specification.registerBinaryCallback)
                : parent._registerBinaryCallback;
            _errorCallback = (specification.errorCallback != null)
                ? new _ZoneFunction<ErrorCallbackHandler>(
                    this, specification.errorCallback)
                : parent._errorCallback;
            _scheduleMicrotask = (specification.scheduleMicrotask != null)
                ? new _ZoneFunction<ScheduleMicrotaskHandler>(
                    this, specification.scheduleMicrotask)
                : parent._scheduleMicrotask;
            _createTimer = (specification.createTimer != null)
                ? new _ZoneFunction<CreateTimerHandler>(this, specification.createTimer)
                : parent._createTimer;
            _createPeriodicTimer = (specification.createPeriodicTimer != null)
                ? new _ZoneFunction<CreatePeriodicTimerHandler>(
                    this, specification.createPeriodicTimer)
                : parent._createPeriodicTimer;
            _print = (specification.print != null)
                ? new _ZoneFunction<PrintHandler>(this, specification.print)
                : parent._print;
            _fork = (specification.fork != null)
                ? new _ZoneFunction<ForkHandler>(this, specification.fork)
                : parent._fork;
            _handleUncaughtError = (specification.handleUncaughtError != null)
                ? new _ZoneFunction<HandleUncaughtErrorHandler>(
                    this, specification.handleUncaughtError)
                : parent._handleUncaughtError;
        }

        public override Zone errorZone => _handleUncaughtError.zone;

        public override object runGuarded(ZoneCallback f) {
            try {
                return run(f);
            }
            catch (Exception e) {
                handleUncaughtError(e);
                return null;
            }
        }

        public override object runUnaryGuarded(ZoneUnaryCallback f, object arg) {
            try {
                return runUnary(f, arg);
            }
            catch (Exception e) {
                handleUncaughtError(e);
                return null;
            }
        }

        public override object runBinaryGuarded(ZoneBinaryCallback f, object arg1, object arg2) {
            try {
                return runBinary(f, arg1, arg2);
            }
            catch (Exception e) {
                handleUncaughtError(e);
                return null;
            }
        }

        public override ZoneCallback bindCallback(ZoneCallback f) {
            var registered = registerCallback(f);
            return () => run(registered);
        }

        public override ZoneUnaryCallback bindUnaryCallback(ZoneUnaryCallback f) {
            var registered = registerUnaryCallback(f);
            return (arg) => runUnary(registered, arg);
        }

        public override ZoneBinaryCallback bindBinaryCallback(ZoneBinaryCallback f) {
            var registered = registerBinaryCallback(f);
            return (arg1, arg2) => runBinary(registered, arg1, arg2);
        }

        public override ZoneCallback bindCallbackGuarded(ZoneCallback f) {
            var registered = registerCallback(f);
            return () => runGuarded(registered);
        }

        public override ZoneUnaryCallback bindUnaryCallbackGuarded(ZoneUnaryCallback f) {
            var registered = registerUnaryCallback(f);
            return (arg) => runUnaryGuarded(registered, arg);
        }

        public override ZoneBinaryCallback bindBinaryCallbackGuarded(ZoneBinaryCallback f) {
            var registered = registerBinaryCallback(f);
            return (arg1, arg2) => runBinaryGuarded(registered, arg1, arg2);
        }

        public override object this[object key] {
            get {
                var result = _map[key];
                if (result != null || _map.ContainsKey(key)) return result;
                // If we are not the root zone, look up in the parent zone.
                if (parent != null) {
                    // We do not optimize for repeatedly looking up a key which isn't
                    // there. That would require storing the key and keeping it alive.
                    // Copying the key/value from the parent does not keep any new values
                    // alive.
                    var value = parent[key];
                    if (value != null) {
                        _map[key] = value;
                    }

                    return value;
                }

                D.assert(this == async_._rootZone);
                return null;
            }
        }

        public override void handleUncaughtError(Exception error) {
            var implementation = _handleUncaughtError;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            HandleUncaughtErrorHandler handler = implementation.function;
            handler(implementation.zone, parentDelegate, this, error);
        }

        public override Zone fork(ZoneSpecification specification = null, Hashtable zoneValues = null) {
            var implementation = _fork;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            ForkHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, specification, zoneValues);
        }

        public override object run(ZoneCallback f) {
            var implementation = _run;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            RunHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, f);
        }

        public override object runUnary(ZoneUnaryCallback f, object arg) {
            var implementation = _runUnary;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            RunUnaryHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, f, arg);
        }

        public override object runBinary(ZoneBinaryCallback f, object arg1, object arg2) {
            var implementation = _runBinary;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            RunBinaryHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, f, arg1, arg2);
        }

        public override ZoneCallback registerCallback(ZoneCallback callback) {
            var implementation = _registerCallback;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            RegisterCallbackHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, callback);
        }

        public override ZoneUnaryCallback registerUnaryCallback(ZoneUnaryCallback callback) {
            var implementation = _registerUnaryCallback;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            RegisterUnaryCallbackHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, callback);
        }

        public override ZoneBinaryCallback registerBinaryCallback(ZoneBinaryCallback callback) {
            var implementation = _registerBinaryCallback;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            RegisterBinaryCallbackHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, callback);
        }

        public override AsyncError errorCallback(Exception error) {
            if (error == null)
                throw new ArgumentNullException(nameof(error));
            var implementation = _errorCallback;
            _Zone implementationZone = implementation.zone;
            if (ReferenceEquals(implementationZone, async_._rootZone)) return null;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementationZone);
            ErrorCallbackHandler handler = implementation.function;
            return handler(implementationZone, parentDelegate, this, error);
        }

        public override void scheduleMicrotask(ZoneCallback f) {
            var implementation = _scheduleMicrotask;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            ScheduleMicrotaskHandler handler = implementation.function;
            handler(implementation.zone, parentDelegate, this, f);
        }

        public override Timer createTimer(TimeSpan duration, ZoneCallback f) {
            var implementation = _createTimer;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            CreateTimerHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, duration, f);
        }

        public override Timer createPeriodicTimer(TimeSpan duration, ZoneUnaryCallback f) {
            var implementation = _createPeriodicTimer;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            CreatePeriodicTimerHandler handler = implementation.function;
            return handler(implementation.zone, parentDelegate, this, duration, f);
        }

        public override void print(string line) {
            var implementation = _print;
            ZoneDelegate parentDelegate = async_._parentDelegate(implementation.zone);
            PrintHandler handler = implementation.function;
            handler(implementation.zone, parentDelegate, this, line);
        }
    }

    public static partial class async_ {
        public static void _rootHandleUncaughtError(
            Zone self, ZoneDelegate parent, Zone zone, Exception error) {
            if (error == null)
                error = new ArgumentNullException(nameof(error));

            _schedulePriorityAsyncCallback(() => {
                _rethrow(error);
                return null;
            });
        }

        internal static void _rethrow(Exception ex) {
            ExceptionDispatchInfo.Capture(ex).Throw();
        }

        internal static object _rootRun(Zone self, ZoneDelegate parent, Zone zone, ZoneCallback f) {
            if (Zone._current == zone) return f();

            Zone old = Zone._enter(zone);
            try {
                return f();
            }
            finally {
                Zone._leave(old);
            }
        }

        internal static object _rootRunUnary(
            Zone self, ZoneDelegate parent, Zone zone, ZoneUnaryCallback f, object arg) {
            if (Zone._current == zone) return f(arg);

            Zone old = Zone._enter(zone);
            try {
                return f(arg);
            }
            finally {
                Zone._leave(old);
            }
        }

        internal static object _rootRunBinary(Zone self, ZoneDelegate parent, Zone zone,
            ZoneBinaryCallback f, object arg1, object arg2) {
            if (Zone._current == zone) return f(arg1, arg2);

            Zone old = Zone._enter(zone);
            try {
                return f(arg1, arg2);
            }
            finally {
                Zone._leave(old);
            }
        }

        internal static ZoneCallback _rootRegisterCallback(
            Zone self, ZoneDelegate parent, Zone zone, ZoneCallback f) {
            return f;
        }

        internal static ZoneUnaryCallback _rootRegisterUnaryCallback(
            Zone self, ZoneDelegate parent, Zone zone, ZoneUnaryCallback f) {
            return f;
        }

        internal static ZoneBinaryCallback _rootRegisterBinaryCallback(
            Zone self, ZoneDelegate parent, Zone zone, ZoneBinaryCallback f) {
            return f;
        }

        internal static AsyncError _rootErrorCallback(Zone self, ZoneDelegate parent, Zone zone, Exception error) =>
            null;

        internal static void _rootScheduleMicrotask(
            Zone self, ZoneDelegate parent, Zone zone, ZoneCallback f) {
            if (!ReferenceEquals(_rootZone, zone)) {
                bool hasErrorHandler = !_rootZone.inSameErrorZone(zone);
                if (hasErrorHandler) {
                    f = zone.bindCallbackGuarded(f);
                }
                else {
                    f = zone.bindCallback(f);
                }
            }

            _scheduleAsyncCallback(f);
        }

        internal static Timer _rootCreateTimer(Zone self, ZoneDelegate parent, Zone zone,
            TimeSpan duration, ZoneCallback callback) {
            if (!ReferenceEquals(_rootZone, zone)) {
                callback = zone.bindCallback(callback);
            }

            return Timer._createTimer(duration, callback);
        }

        internal static Timer _rootCreatePeriodicTimer(Zone self, ZoneDelegate parent, Zone zone,
            TimeSpan duration, ZoneUnaryCallback callback) {
            if (!ReferenceEquals(_rootZone, zone)) {
                callback = zone.bindUnaryCallback(callback);
            }

            return Timer._createPeriodicTimer(duration, callback);
        }

        internal static void _rootPrint(Zone self, ZoneDelegate parent, Zone zone, string line) {
            Debug.Log(line);
        }

        internal static Zone _rootFork(Zone self, ZoneDelegate parent, Zone zone,
            ZoneSpecification specification, Hashtable zoneValues) {
            if (specification == null) {
                specification = new ZoneSpecification();
            }

            Hashtable valueMap;
            if (zoneValues == null) {
                valueMap = ((_Zone) zone)._map;
            }
            else {
                valueMap = new Hashtable(zoneValues);
            }

            return new _CustomZone((_Zone) zone, specification, valueMap);
        }
    }

    class _RootZone : _Zone {
        internal override _RunNullaryZoneFunction _run =>
            new _RunNullaryZoneFunction(async_._rootZone, async_._rootRun);

        internal override _RunUnaryZoneFunction _runUnary =>
            new _RunUnaryZoneFunction(async_._rootZone, async_._rootRunUnary);

        internal override _RunBinaryZoneFunction _runBinary =>
            new _RunBinaryZoneFunction(async_._rootZone, async_._rootRunBinary);

        internal override _RegisterNullaryZoneFunction _registerCallback =>
            new _RegisterNullaryZoneFunction(async_._rootZone, async_._rootRegisterCallback);

        internal override _RegisterUnaryZoneFunction _registerUnaryCallback =>
            new _RegisterUnaryZoneFunction(async_._rootZone, async_._rootRegisterUnaryCallback);

        internal override _RegisterBinaryZoneFunction _registerBinaryCallback =>
            new _RegisterBinaryZoneFunction(async_._rootZone, async_._rootRegisterBinaryCallback);

        internal override _ZoneFunction<ErrorCallbackHandler> _errorCallback =>
            new _ZoneFunction<ErrorCallbackHandler>(async_._rootZone, async_._rootErrorCallback);

        internal override _ZoneFunction<ScheduleMicrotaskHandler> _scheduleMicrotask =>
            new _ZoneFunction<ScheduleMicrotaskHandler>(async_._rootZone, async_._rootScheduleMicrotask);

        internal override _ZoneFunction<CreateTimerHandler> _createTimer =>
            new _ZoneFunction<CreateTimerHandler>(async_._rootZone, async_._rootCreateTimer);

        internal override _ZoneFunction<CreatePeriodicTimerHandler> _createPeriodicTimer =>
            new _ZoneFunction<CreatePeriodicTimerHandler>(async_._rootZone, async_._rootCreatePeriodicTimer);

        internal override _ZoneFunction<PrintHandler> _print =>
            new _ZoneFunction<PrintHandler>(async_._rootZone, async_._rootPrint);

        internal override _ZoneFunction<ForkHandler> _fork =>
            new _ZoneFunction<ForkHandler>(async_._rootZone, async_._rootFork);

        internal override _ZoneFunction<HandleUncaughtErrorHandler> _handleUncaughtError =>
            new _ZoneFunction<HandleUncaughtErrorHandler>(
                async_._rootZone, async_._rootHandleUncaughtError);

        internal override _Zone _parent => null;

        internal override Hashtable _map => _rootMap;
        static readonly Hashtable _rootMap = new Hashtable();

        static ZoneDelegate _rootDelegate;

        internal override ZoneDelegate _delegate {
            get {
                if (_rootDelegate != null) return _rootDelegate;
                return _rootDelegate = new _ZoneDelegate(this);
            }
        }

        public override Zone errorZone => this;

        public override object runGuarded(ZoneCallback f) {
            try {
                if (ReferenceEquals(async_._rootZone, _current)) {
                    return f();
                }

                return async_._rootRun(null, null, this, f);
            }
            catch (Exception e) {
                handleUncaughtError(e);
                return null;
            }
        }

        public override object runUnaryGuarded(ZoneUnaryCallback f, object arg) {
            try {
                if (ReferenceEquals(async_._rootZone, _current)) {
                    return f(arg);
                }

                return async_._rootRunUnary(null, null, this, f, arg);
            }
            catch (Exception e) {
                handleUncaughtError(e);
                return null;
            }
        }

        public override object runBinaryGuarded(ZoneBinaryCallback f, object arg1, object arg2) {
            try {
                if (ReferenceEquals(async_._rootZone, _current)) {
                    return f(arg1, arg2);
                }

                return async_._rootRunBinary(null, null, this, f, arg1, arg2);
            }
            catch (Exception e) {
                handleUncaughtError(e);
                return null;
            }
        }

        public override ZoneCallback bindCallback(ZoneCallback f) {
            return () => run(f);
        }

        public override ZoneUnaryCallback bindUnaryCallback(ZoneUnaryCallback f) {
            return (arg) => runUnary(f, arg);
        }

        public override ZoneBinaryCallback bindBinaryCallback(ZoneBinaryCallback f) {
            return (arg1, arg2) => runBinary(f, arg1, arg2);
        }

        public override ZoneCallback bindCallbackGuarded(ZoneCallback f) {
            return () => runGuarded(f);
        }

        public override ZoneUnaryCallback bindUnaryCallbackGuarded(ZoneUnaryCallback f) {
            return (arg) => runUnaryGuarded(f, arg);
        }

        public override ZoneBinaryCallback bindBinaryCallbackGuarded(ZoneBinaryCallback f) {
            return (arg1, arg2) => runBinaryGuarded(f, arg1, arg2);
        }

        public override object this[object key] => null;

        public override void handleUncaughtError(Exception error) {
            async_._rootHandleUncaughtError(null, null, this, error);
        }

        public override Zone fork(ZoneSpecification specification = null, Hashtable zoneValues = null) {
            return async_._rootFork(null, null, this, specification, zoneValues);
        }

        public override object run(ZoneCallback f) {
            if (ReferenceEquals(_current, async_._rootZone)) return f();
            return async_._rootRun(null, null, this, f);
        }

        public override object runUnary(ZoneUnaryCallback f, object arg) {
            if (ReferenceEquals(_current, async_._rootZone)) return f(arg);
            return async_._rootRunUnary(null, null, this, f, arg);
        }

        public override object runBinary(ZoneBinaryCallback f, object arg1, object arg2) {
            if (ReferenceEquals(_current, async_._rootZone)) return f(arg1, arg2);
            return async_._rootRunBinary(null, null, this, f, arg1, arg2);
        }

        public override ZoneCallback registerCallback(ZoneCallback f) => f;

        public override ZoneUnaryCallback registerUnaryCallback(ZoneUnaryCallback f) => f;

        public override ZoneBinaryCallback registerBinaryCallback(ZoneBinaryCallback f) => f;

        public override AsyncError errorCallback(Exception error) => null;

        public override void scheduleMicrotask(ZoneCallback f) {
            async_._rootScheduleMicrotask(null, null, this, f);
        }

        public override Timer createTimer(TimeSpan duration, ZoneCallback f) {
            return Timer._createTimer(duration, f);
        }

        public override Timer createPeriodicTimer(TimeSpan duration, ZoneUnaryCallback f) {
            return Timer._createPeriodicTimer(duration, f);
        }

        public override void print(string line) {
            Debug.Log(line);
        }
    }

    public static partial class async_ {
        internal static readonly _Zone _rootZone = new _RootZone();

        public static R runZoned<R>(Func<R> body,
            Hashtable zoneValues = null, ZoneSpecification zoneSpecification = null) {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            return _runZoned(body, zoneValues, zoneSpecification);
        }

        public static R runZonedGuarded<R>(Func<R> body, Action<Exception> onError,
            Hashtable zoneValues = null, ZoneSpecification zoneSpecification = null) {
            if (body == null)
                throw new ArgumentNullException(nameof(body));
            if (onError == null)
                throw new ArgumentNullException(nameof(onError));

            HandleUncaughtErrorHandler errorHandler =
                (Zone self, ZoneDelegate parent, Zone zone, Exception error) => {
                    try {
                        self.parent.runUnary((e) => {
                            onError((Exception) e);
                            return null;
                        }, error);
                    }
                    catch (Exception e) {
                        parent.handleUncaughtError(zone, e);
                    }
                };
            if (zoneSpecification == null) {
                zoneSpecification =
                    new ZoneSpecification(handleUncaughtError: errorHandler);
            }
            else {
                zoneSpecification = ZoneSpecification.from(zoneSpecification,
                    handleUncaughtError: errorHandler);
            }

            try {
                return _runZoned(body, zoneValues, zoneSpecification);
            }
            catch (Exception error) {
                onError(error);
            }

            return default;
        }

        internal static R _runZoned<R>(Func<R> body, Hashtable zoneValues, ZoneSpecification specification) =>
            (R) Zone.current
                .fork(specification: specification, zoneValues: zoneValues)
                .run(() => body());
    }
}