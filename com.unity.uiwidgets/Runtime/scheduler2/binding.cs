using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using developer;
using Unity.UIWidgets.async;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using FrameTiming = Unity.UIWidgets.ui.FrameTiming;
using Timer = Unity.UIWidgets.async2.Timer;

namespace Unity.UIWidgets.scheduler2 {
    public static partial class scheduler_ {
        public static float timeDilation {
            get { return _timeDilation; }
            set {
                D.assert(value > 0.0f);
                if (_timeDilation == value)
                    return;

                SchedulerBinding.instance?.resetEpoch();
                _timeDilation = value;
            }
        }

        static float _timeDilation = 1.0f;
    }

    public delegate void FrameCallback(TimeSpan timeStamp);

    public delegate T TaskCallback<out T>();

    public delegate bool SchedulingStrategy(int priority = 0, SchedulerBinding scheduler = null);

    interface _TaskEntry : IComparable<_TaskEntry> {
        int priority { get; }
        string debugStack { get; }
        void run();
    }

    class _TaskEntry<T> : _TaskEntry {
        internal _TaskEntry(TaskCallback<T> task, int priority) {
            this.task = task;
            this.priority = priority;

            D.assert(() => {
                debugStack = StackTraceUtility.ExtractStackTrace();
                return true;
            });
            completer = Completer.create();
        }

        public readonly TaskCallback<T> task;
        public int priority { get; }
        public string debugStack { get; private set; }
        public Completer completer;

        public void run() {
            if (!foundation_.kReleaseMode) {
                completer.complete(FutureOr.value(task()));
            }
            else {
                completer.complete(FutureOr.value(task()));
            }
        }

        public int CompareTo(_TaskEntry other) {
            return -priority.CompareTo(other.priority);
        }
    }

    class _FrameCallbackEntry {
        internal _FrameCallbackEntry(FrameCallback callback, bool rescheduling = false) {
            this.callback = callback;

            D.assert(() => {
                if (rescheduling) {
                    D.assert(() => {
                        if (debugCurrentCallbackStack == null) {
                            throw new UIWidgetsError(
                                new List<DiagnosticsNode>() {
                                    new ErrorSummary(
                                        "scheduleFrameCallback called with rescheduling true, but no callback is in scope."),
                                    new ErrorDescription(
                                        "The \"rescheduling\" argument should only be set to true if the " +
                                        "callback is being reregistered from within the callback itself, " +
                                        "and only then if the callback itself is entirely synchronous."),
                                    new ErrorHint("If this is the initial registration of the callback, or if the " +
                                                  "callback is asynchronous, then do not use the \"rescheduling\" " +
                                                  "argument.")
                                });
                        }

                        return true;
                    });
                    debugStack = debugCurrentCallbackStack;
                }
                else {
                    debugStack = StackTraceUtility.ExtractStackTrace();
                }

                return true;
            });
        }

        public readonly FrameCallback callback;

        public static string debugCurrentCallbackStack;
        public string debugStack;
    }

    public enum SchedulerPhase {
        idle,
        transientCallbacks,
        midFrameMicrotasks,
        persistentCallbacks,
        postFrameCallbacks,
    }

    public class SchedulerBinding : PaintingBinding {
        protected override void initInstances() {
            base.initInstances();
            instance = this;

            //SystemChannels.lifecycle.setMessageHandler(_handleLifecycleMessage);
            readInitialLifecycleStateFromNativeWindow();

            if (!foundation_.kReleaseMode) {
                int frameNumber = 0;
                addTimingsCallback((List<FrameTiming> timings) => {
                    foreach (FrameTiming frameTiming in timings) {
                        frameNumber += 1;
                        _profileFramePostEvent(frameNumber, frameTiming);
                    }
                });
            }
        }

        readonly List<TimingsCallback> _timingsCallbacks = new List<TimingsCallback>();

        public void addTimingsCallback(TimingsCallback callback) {
            _timingsCallbacks.Add(callback);
            if (_timingsCallbacks.Count == 1) {
                D.assert(window.onReportTimings == null);
                window.onReportTimings = _executeTimingsCallbacks;
            }

            D.assert(window.onReportTimings == _executeTimingsCallbacks);
        }

        public void removeTimingsCallback(TimingsCallback callback) {
            D.assert(_timingsCallbacks.Contains(callback));
            _timingsCallbacks.Remove(callback);
            if (_timingsCallbacks.isEmpty()) {
                window.onReportTimings = null;
            }
        }

        void _executeTimingsCallbacks(List<FrameTiming> timings) {
            List<TimingsCallback> clonedCallbacks =
                new List<TimingsCallback>(_timingsCallbacks);
            foreach (TimingsCallback callback in clonedCallbacks) {
                try {
                    if (_timingsCallbacks.Contains(callback)) {
                        callback(timings);
                    }
                }
                catch (Exception ex) {
                    InformationCollector collector = null;
                    D.assert(() => {
                        IEnumerable<DiagnosticsNode> infoCollect() {
                            yield return new DiagnosticsProperty<TimingsCallback>(
                                "The TimingsCallback that gets executed was",
                                callback,
                                style: DiagnosticsTreeStyle.errorProperty);
                        }
                        collector = infoCollect;
                        return true;
                    });

                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: ex,
                        context: new ErrorDescription("while executing callbacks for FrameTiming"),
                        informationCollector: collector
                    ));
                }
            }
        }

        public static SchedulerBinding instance {
            get { return (SchedulerBinding) Window.instance._binding; }
            private set { Window.instance._binding = value; }
        }

        public AppLifecycleState? lifecycleState => _lifecycleState;
        AppLifecycleState? _lifecycleState;

        protected void readInitialLifecycleStateFromNativeWindow() {
            if (_lifecycleState == null) {
                handleAppLifecycleStateChanged(_parseAppLifecycleMessage(window.initialLifecycleState));
            }
        }

        protected virtual void handleAppLifecycleStateChanged(AppLifecycleState state) {
            _lifecycleState = state;
            switch (state) {
                case AppLifecycleState.resumed:
                case AppLifecycleState.inactive:
                    _setFramesEnabledState(true);
                    break;
                case AppLifecycleState.paused:
                case AppLifecycleState.detached:
                    _setFramesEnabledState(false);
                    break;
            }
        }

        static AppLifecycleState _parseAppLifecycleMessage(string message) {
            switch (message) {
                case "AppLifecycleState.paused":
                    return AppLifecycleState.paused;
                case "AppLifecycleState.resumed":
                    return AppLifecycleState.resumed;
                case "AppLifecycleState.inactive":
                    return AppLifecycleState.inactive;
                case "AppLifecycleState.detached":
                    return AppLifecycleState.detached;
            }

            throw new Exception("unknown AppLifecycleState: " + message);
        }


        public SchedulingStrategy schedulingStrategy = scheduler_.defaultSchedulingStrategy;

        readonly PriorityQueue<_TaskEntry> _taskQueue = new PriorityQueue<_TaskEntry>();

        public Future scheduleTask<T>(
            TaskCallback<T> task,
            Priority priority) {
            bool isFirstTask = _taskQueue.isEmpty;
            _TaskEntry<T> entry = new _TaskEntry<T>(
                task,
                priority.value
            );
            _taskQueue.enqueue(entry);
            if (isFirstTask && !locked)
                _ensureEventLoopCallback();
            return entry.completer.future;
        }
        

        protected override void unlocked() {
            base.unlocked();
            if (_taskQueue.isNotEmpty)
                _ensureEventLoopCallback();
        }

        bool _hasRequestedAnEventLoopCallback = false;

        void _ensureEventLoopCallback() {
            D.assert(!locked);
            D.assert(_taskQueue.count != 0);
            if (_hasRequestedAnEventLoopCallback)
                return;
            _hasRequestedAnEventLoopCallback = true;
            Timer.run(_runTasks);
        }

        object _runTasks() {
            _hasRequestedAnEventLoopCallback = false;
            if (handleEventLoopCallback())
                _ensureEventLoopCallback(); // runs next task when there's time
            return null;
        }

        bool handleEventLoopCallback() {
            if (_taskQueue.isEmpty || locked)
                return false;

            _TaskEntry entry = _taskQueue.first;
            if (schedulingStrategy(priority: entry.priority, scheduler: this)) {
                try {
                    _taskQueue.removeFirst();
                    entry.run();
                }
                catch (Exception exception) {
                    string callbackStack = null;
                    D.assert(() => {
                        callbackStack = entry.debugStack;
                        return true;
                    });

                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return DiagnosticsNode.message("\nThis exception was thrown in the context of a scheduler callback. " +
                                                             "When the scheduler callback was _registered_ (as opposed to when the " +
                                                             "exception was thrown), this was the stack: " + callbackStack);
                    }
                    
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "scheduler library",
                        context: new ErrorDescription("during a task callback"),
                        informationCollector: callbackStack == null
                            ? (InformationCollector) null
                            : infoCollector
                    ));
                }

                return _taskQueue.isNotEmpty;
            }

            return false;
        }

        int _nextFrameCallbackId = 0;
        Dictionary<int, _FrameCallbackEntry> _transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();
        readonly HashSet<int> _removedIds = new HashSet<int>();

        public int transientCallbackCount => _transientCallbacks.Count;

        public int scheduleFrameCallback(FrameCallback callback, bool rescheduling = false) {
            scheduleFrame();
            _nextFrameCallbackId += 1;
            _transientCallbacks[_nextFrameCallbackId] =
                new _FrameCallbackEntry(callback, rescheduling: rescheduling);
            return _nextFrameCallbackId;
        }

        public void cancelFrameCallbackWithId(int id) {
            D.assert(id > 0);
            _transientCallbacks.Remove(id);
            _removedIds.Add(id);
        }

        readonly List<FrameCallback> _persistentCallbacks = new List<FrameCallback>();

        public void addPersistentFrameCallback(FrameCallback callback) {
            _persistentCallbacks.Add(callback);
        }

        readonly List<FrameCallback> _postFrameCallbacks = new List<FrameCallback>();

        public void addPostFrameCallback(FrameCallback callback) {
            _postFrameCallbacks.Add(callback);
        }

        Completer _nextFrameCompleter;

        Future endOfFrame {
            get {
                if (_nextFrameCompleter == null) {
                    if (schedulerPhase == SchedulerPhase.idle)
                        scheduleFrame();
                    _nextFrameCompleter = Completer.create();
                    addPostFrameCallback((timeStamp) => {
                        _nextFrameCompleter.complete();
                        _nextFrameCompleter = null;
                    });
                }

                return _nextFrameCompleter.future;
            }
        }

        public bool hasScheduledFrame => _hasScheduledFrame;
        bool _hasScheduledFrame = false;

        public SchedulerPhase schedulerPhase => _schedulerPhase;
        SchedulerPhase _schedulerPhase = SchedulerPhase.idle;

        public bool framesEnabled => _framesEnabled;
        bool _framesEnabled = true;

        void _setFramesEnabledState(bool enabled) {
            if (_framesEnabled == enabled)
                return;
            _framesEnabled = enabled;
            if (enabled)
                scheduleFrame();
        }

        protected void ensureFrameCallbacksRegistered() {
            window.onBeginFrame = window.onBeginFrame ?? _handleBeginFrame;
            window.onDrawFrame = window.onDrawFrame ?? _handleDrawFrame;
        }

        public void ensureVisualUpdate() {
            switch (schedulerPhase) {
                case SchedulerPhase.idle:
                case SchedulerPhase.postFrameCallbacks:
                    scheduleFrame();
                    return;
                case SchedulerPhase.transientCallbacks:
                case SchedulerPhase.midFrameMicrotasks:
                case SchedulerPhase.persistentCallbacks:
                    return;
            }
        }

        public void scheduleFrame() {
            if (_hasScheduledFrame || !framesEnabled)
                return;

            D.assert(() => {
                if (scheduler_.debugPrintScheduleFrameStacks) {
                    Debug.LogFormat("scheduleFrame() called. Current phase is {0}.", schedulerPhase);
                }

                return true;
            });

            ensureFrameCallbacksRegistered();
            Window.instance.scheduleFrame();
            _hasScheduledFrame = true;
        }

        public void scheduleForcedFrame() {
            if (!framesEnabled)
                return;

            if (_hasScheduledFrame)
                return;

            D.assert(() => {
                if (scheduler_.debugPrintScheduleFrameStacks) {
                    Debug.LogFormat("scheduleForcedFrame() called. Current phase is {0}.", schedulerPhase);
                }

                return true;
            });

            ensureFrameCallbacksRegistered();
            Window.instance.scheduleFrame();
            _hasScheduledFrame = true;
        }

        bool _warmUpFrame = false;

        public void scheduleWarmUpFrame() {
            if (_warmUpFrame || schedulerPhase != SchedulerPhase.idle)
                return;

            _warmUpFrame = true;
            Timeline.startSync("Warm-up frame");

            bool hadScheduledFrame = _hasScheduledFrame;
            // We use timers here to ensure that microtasks flush in between.
            Timer.run(() => {
                D.assert(_warmUpFrame);
                handleBeginFrame(null);
                return null;
            });
            Timer.run(() => {
                D.assert(_warmUpFrame);
                handleDrawFrame();
                // We call resetEpoch after this frame so that, in the hot reload case,
                // the very next frame pretends to have occurred immediately after this
                // warm-up frame. The warm-up frame's timestamp will typically be far in
                // the past (the time of the last real frame), so if we didn't reset the
                // epoch we would see a sudden jump from the old time in the warm-up frame
                // to the new time in the "real" frame. The biggest problem with this is
                // that implicit animations end up being triggered at the old time and
                // then skipping every frame and finishing in the new time.
                resetEpoch();
                _warmUpFrame = false;
                if (hadScheduledFrame)
                    scheduleFrame();
                return null;
            });

            // Lock events so touch events etc don't insert themselves until the
            // scheduled frame has finished.
            lockEvents(() => endOfFrame.then(v => {
                Timeline.finishSync();
                return FutureOr.nil;
            }));
        }

        TimeSpan? _firstRawTimeStampInEpoch;
        TimeSpan _epochStart = TimeSpan.Zero;
        TimeSpan _lastRawTimeStamp = TimeSpan.Zero;

        public void resetEpoch() {
            _epochStart = _adjustForEpoch(_lastRawTimeStamp);
            _firstRawTimeStampInEpoch = null;
        }

        TimeSpan _adjustForEpoch(TimeSpan rawTimeStamp) {
            var rawDurationSinceEpoch = _firstRawTimeStampInEpoch == null
                ? TimeSpan.Zero
                : rawTimeStamp - _firstRawTimeStampInEpoch.Value;
            return new TimeSpan((long) (rawDurationSinceEpoch.Ticks / scheduler_.timeDilation) +
                                _epochStart.Ticks);
        }

        public TimeSpan currentFrameTimeStamp {
            get {
                D.assert(_currentFrameTimeStamp != null);
                return _currentFrameTimeStamp.Value;
            }
        }

        TimeSpan? _currentFrameTimeStamp;

        public TimeSpan currentSystemFrameTimeStamp => _lastRawTimeStamp;

        int _debugFrameNumber = 0;
        string _debugBanner;
        bool _ignoreNextEngineDrawFrame = false;

        void _handleBeginFrame(TimeSpan rawTimeStamp) {
            if (_warmUpFrame) {
                D.assert(!_ignoreNextEngineDrawFrame);
                _ignoreNextEngineDrawFrame = true;
                return;
            }

            handleBeginFrame(rawTimeStamp);
        }

        void _handleDrawFrame() {
            if (_ignoreNextEngineDrawFrame) {
                _ignoreNextEngineDrawFrame = false;
                return;
            }

            handleDrawFrame();
        }

        public void handleBeginFrame(TimeSpan? rawTimeStamp) {
            Timeline.startSync("Frame");

            _firstRawTimeStampInEpoch = _firstRawTimeStampInEpoch ?? rawTimeStamp;
            _currentFrameTimeStamp = _adjustForEpoch(rawTimeStamp ?? _lastRawTimeStamp);

            if (rawTimeStamp != null)
                _lastRawTimeStamp = rawTimeStamp.Value;

            D.assert(() => {
                _debugFrameNumber += 1;

                if (scheduler_.debugPrintBeginFrameBanner || scheduler_.debugPrintEndFrameBanner) {
                    StringBuilder frameTimeStampDescription = new StringBuilder();
                    if (rawTimeStamp != null) {
                        _debugDescribeTimeStamp(_currentFrameTimeStamp.Value, frameTimeStampDescription);
                    }
                    else {
                        frameTimeStampDescription.Append("(warm-up frame)");
                    }

                    _debugBanner =
                        $"▄▄▄▄▄▄▄▄ Frame {_debugFrameNumber.ToString().PadRight(7)}   ${frameTimeStampDescription.ToString().PadLeft(18)} ▄▄▄▄▄▄▄▄";
                    if (scheduler_.debugPrintBeginFrameBanner)
                        Debug.Log(_debugBanner);
                }

                return true;
            });

            D.assert(_schedulerPhase == SchedulerPhase.idle);
            _hasScheduledFrame = false;

            try {
                Timeline.startSync("Animate");
                _schedulerPhase = SchedulerPhase.transientCallbacks;
                var callbacks = _transientCallbacks;
                _transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();
                foreach (var entry in callbacks) {
                    if (!_removedIds.Contains(entry.Key)) {
                        _invokeFrameCallback(
                            entry.Value.callback, _currentFrameTimeStamp.Value, entry.Value.debugStack);
                    }
                }

                _removedIds.Clear();
            }
            finally {
                _schedulerPhase = SchedulerPhase.midFrameMicrotasks;
            }
        }

        public void handleDrawFrame() {
            D.assert(_schedulerPhase == SchedulerPhase.midFrameMicrotasks);
            Timeline.finishSync();

            try {
                _schedulerPhase = SchedulerPhase.persistentCallbacks;
                foreach (FrameCallback callback in _persistentCallbacks)
                    _invokeFrameCallback(callback, _currentFrameTimeStamp.Value);

                _schedulerPhase = SchedulerPhase.postFrameCallbacks;
                var localPostFrameCallbacks = new List<FrameCallback>(_postFrameCallbacks);
                _postFrameCallbacks.Clear();
                foreach (FrameCallback callback in localPostFrameCallbacks)
                    _invokeFrameCallback(callback, _currentFrameTimeStamp.Value);
            }
            finally {
                _schedulerPhase = SchedulerPhase.idle;
                D.assert(() => {
                    if (scheduler_.debugPrintEndFrameBanner)
                        Debug.Log(new string('▀', _debugBanner.Length));
                    _debugBanner = null;
                    return true;
                });
                _currentFrameTimeStamp = null;
            }
        }

        void _profileFramePostEvent(int frameNumber, FrameTiming frameTiming) {
            developer_.postEvent("Flutter.Frame", new Hashtable {
                {"number", frameNumber},
                {"startTime", frameTiming.timestampInMicroseconds(FramePhase.buildStart)},
                {"elapsed", (int) (frameTiming.totalSpan.TotalMilliseconds * 1000)},
                {"build", (int) (frameTiming.buildDuration.TotalMilliseconds * 1000)},
                {"raster", (int) (frameTiming.rasterDuration.TotalMilliseconds * 1000)},
            });
        }


        static void _debugDescribeTimeStamp(TimeSpan timeStamp, StringBuilder buffer) {
            if (timeStamp.TotalDays > 0)
                buffer.AppendFormat("{0}d ", timeStamp.Days);
            if (timeStamp.TotalHours > 0)
                buffer.AppendFormat("{0}h ", timeStamp.Hours);
            if (timeStamp.TotalMinutes > 0)
                buffer.AppendFormat("{0}m ", timeStamp.Minutes);
            if (timeStamp.TotalSeconds > 0)
                buffer.AppendFormat("{0}s ", timeStamp.Seconds);
            buffer.AppendFormat("{0}", timeStamp.Milliseconds);

            int microseconds = (int) (timeStamp.Ticks % 10000 / 10);
            if (microseconds > 0)
                buffer.AppendFormat(".{0}", microseconds.ToString().PadLeft(3, '0'));
            buffer.Append("ms");
        }

        void _invokeFrameCallback(FrameCallback callback, TimeSpan timeStamp, string callbackStack = null) {
            D.assert(callback != null);
            D.assert(_FrameCallbackEntry.debugCurrentCallbackStack == null);
            D.assert(() => {
                _FrameCallbackEntry.debugCurrentCallbackStack = callbackStack;
                return true;
            });

            try {
                callback(timeStamp);
            }
            catch (Exception ex) {
                IEnumerable<DiagnosticsNode> infoCollector() {
                    yield return DiagnosticsNode.message("\nThis exception was thrown in the context of a scheduler callback. " +
                                                         "When the scheduler callback was _registered_ (as opposed to when the " +
                                                         "exception was thrown), this was the stack:");
                    StringBuilder builder = new StringBuilder();
                    foreach (var line in UIWidgetsError.defaultStackFilter(
                        callbackStack.TrimEnd().Split('\n'))) {
                        builder.AppendLine(line);
                    }

                    yield return DiagnosticsNode.message(builder.ToString());
                }
                
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "scheduler library",
                    context: new ErrorDescription("during a scheduler callback"),
                    informationCollector: callbackStack == null
                        ? (InformationCollector) null
                        : infoCollector
                ));
            }

            D.assert(() => {
                _FrameCallbackEntry.debugCurrentCallbackStack = null;
                return true;
            });
        }
    }

    public static partial class scheduler_ {
        public static bool defaultSchedulingStrategy(int priority, SchedulerBinding scheduler) {
            if (scheduler.transientCallbackCount > 0)
                return priority >= Priority.animation.value;
            return true;
        }
    }
}