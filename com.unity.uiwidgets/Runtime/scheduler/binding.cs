using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using developer;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.Rendering;
using FrameTiming = Unity.UIWidgets.ui.FrameTiming;

namespace Unity.UIWidgets.scheduler {
    public static partial class scheduler_ {
        static float _timeDilation = 1.0f;

        public static float timeDilation {
            get { return _timeDilation; }
            set {
                D.assert(value > 0.0f);
                if (_timeDilation == value) {
                    return;
                }

                SchedulerBinding.instance?.resetEpoch();
                _timeDilation = value;
            }
        }
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
        public readonly TaskCallback<T> task;
        public Completer completer;

        internal _TaskEntry(TaskCallback<T> task, int priority) {
            this.task = task;
            this.priority = priority;

            D.assert(() => {
                debugStack = StackTraceUtility.ExtractStackTrace();
                return true;
            });
            completer = Completer.create();
        }

        public int priority { get; }
        public string debugStack { get; private set; }

        public void run() {
            if (!foundation_.kReleaseMode) {
                completer.complete(FutureOr.value(task()));
            }
            else {
                completer.complete(FutureOr.value(task()));
            }
        }

        public int CompareTo(_TaskEntry other) {
            return -priority.CompareTo(value: other.priority);
        }
    }

    class _FrameCallbackEntry {
        public static string debugCurrentCallbackStack;

        public readonly FrameCallback callback;
        public string debugStack;

        internal _FrameCallbackEntry(FrameCallback callback, bool rescheduling = false) {
            this.callback = callback;

            D.assert(() => {
                if (rescheduling) {
                    D.assert(() => {
                        if (debugCurrentCallbackStack == null) {
                            throw new UIWidgetsError(
                                new List<DiagnosticsNode> {
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
    }

    public enum SchedulerPhase {
        idle,
        transientCallbacks,
        midFrameMicrotasks,
        persistentCallbacks,
        postFrameCallbacks
    }

    public class SchedulerBinding : PaintingBinding {
        readonly List<FrameCallback> _persistentCallbacks = new List<FrameCallback>();

        readonly List<FrameCallback> _postFrameCallbacks = new List<FrameCallback>();
        readonly HashSet<int> _removedIds = new HashSet<int>();

        readonly PriorityQueue<_TaskEntry> _taskQueue = new PriorityQueue<_TaskEntry>();

        readonly List<TimingsCallback> _timingsCallbacks = new List<TimingsCallback>();

        TimeSpan? _currentFrameTimeStamp;
        string _debugBanner;

        int _debugFrameNumber;
        TimeSpan _epochStart = TimeSpan.Zero;

        TimeSpan? _firstRawTimeStampInEpoch;

        bool _hasRequestedAnEventLoopCallback;
        bool _ignoreNextEngineDrawFrame;

        int _nextFrameCallbackId;

        Completer _nextFrameCompleter;
        Dictionary<int, _FrameCallbackEntry> _transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();

        bool _warmUpFrame;


        public SchedulingStrategy schedulingStrategy = scheduler_.defaultSchedulingStrategy;
        
        public new static SchedulerBinding instance {
            get { return (SchedulerBinding) Window.instance._binding; }
            private set { Window.instance._binding = value; }
        }

        public AppLifecycleState? lifecycleState { get; private set; }

        public int transientCallbackCount {
            get { return _transientCallbacks.Count; }
        }

        public Future endOfFrame {
            get {
                if (_nextFrameCompleter == null) {
                    if (schedulerPhase == SchedulerPhase.idle) {
                        scheduleFrame();
                    }

                    _nextFrameCompleter = Completer.create();
                    addPostFrameCallback(timeStamp => {
                        _nextFrameCompleter.complete();
                        _nextFrameCompleter = null;
                    });
                }

                return _nextFrameCompleter.future;
            }
        }

        public bool hasScheduledFrame { get; private set; }

        public SchedulerPhase schedulerPhase { get; private set; } = SchedulerPhase.idle;

        public bool framesEnabled { get; private set; } = true;

        public TimeSpan currentFrameTimeStamp {
            get {
                D.assert(_currentFrameTimeStamp != null);
                return _currentFrameTimeStamp.Value;
            }
        }

        public TimeSpan currentSystemFrameTimeStamp { get; private set; } = TimeSpan.Zero;

        protected override void initInstances() {
            base.initInstances();
            instance = this;

            //SystemChannels.lifecycle.setMessageHandler(_handleLifecycleMessage);
            readInitialLifecycleStateFromNativeWindow();

            if (!foundation_.kReleaseMode) {
                var frameNumber = 0;
                addTimingsCallback(timings => {
                    foreach (var frameTiming in timings) {
                        frameNumber += 1;
                        _profileFramePostEvent(frameNumber: frameNumber, frameTiming: frameTiming);
                    }
                });
            }
        }

        public void addTimingsCallback(TimingsCallback callback) {
            _timingsCallbacks.Add(item: callback);
            if (_timingsCallbacks.Count == 1) {
                D.assert(window.onReportTimings == null);
                window.onReportTimings = _executeTimingsCallbacks;
            }

            D.assert(window.onReportTimings == _executeTimingsCallbacks);
        }

        public void removeTimingsCallback(TimingsCallback callback) {
            D.assert(_timingsCallbacks.Contains(item: callback));
            _timingsCallbacks.Remove(item: callback);
            if (_timingsCallbacks.isEmpty()) {
                window.onReportTimings = null;
            }
        }

        void _executeTimingsCallbacks(List<FrameTiming> timings) {
            var clonedCallbacks =
                new List<TimingsCallback>(collection: _timingsCallbacks);
            foreach (var callback in clonedCallbacks) {
                try {
                    if (_timingsCallbacks.Contains(item: callback)) {
                        callback(timings: timings);
                    }
                }
                catch (Exception ex) {
                    InformationCollector collector = null;
                    D.assert(() => {
                        IEnumerable<DiagnosticsNode> infoCollect() {
                            yield return new DiagnosticsProperty<TimingsCallback>(
                                "The TimingsCallback that gets executed was",
                                value: callback,
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

        protected void readInitialLifecycleStateFromNativeWindow() {
            if (lifecycleState == null) {
                handleAppLifecycleStateChanged(_parseAppLifecycleMessage(message: window.initialLifecycleState));
            }
        }

        protected virtual void handleAppLifecycleStateChanged(AppLifecycleState state) {
            lifecycleState = state;
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

        public Future scheduleTask<T>(
            TaskCallback<T> task,
            Priority priority) {
            var isFirstTask = _taskQueue.isEmpty;
            var entry = new _TaskEntry<T>(
                task: task,
                priority: priority.value
            );
            _taskQueue.enqueue(item: entry);
            if (isFirstTask && !locked) {
                _ensureEventLoopCallback();
            }

            return entry.completer.future;
        }


        protected override void unlocked() {
            base.unlocked();
            if (_taskQueue.isNotEmpty) {
                _ensureEventLoopCallback();
            }
        }

        void _ensureEventLoopCallback() {
            D.assert(result: !locked);
            D.assert(_taskQueue.count != 0);
            if (_hasRequestedAnEventLoopCallback) {
                return;
            }

            _hasRequestedAnEventLoopCallback = true;
            Timer.run(callback: _runTasks);
        }

        object _runTasks() {
            _hasRequestedAnEventLoopCallback = false;
            if (handleEventLoopCallback()) {
                _ensureEventLoopCallback(); // runs next task when there's time
            }

            return null;
        }

        bool handleEventLoopCallback() {
            if (_taskQueue.isEmpty || locked) {
                return false;
            }

            var entry = _taskQueue.first;
            if (schedulingStrategy(priority: entry.priority, this)) {
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
                        yield return DiagnosticsNode.message(
                            "\nThis exception was thrown in the context of a scheduler callback. " +
                            "When the scheduler callback was _registered_ (as opposed to when the " +
                            "exception was thrown), this was the stack: " + callbackStack);
                    }

                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        "scheduler library",
                        new ErrorDescription("during a task callback"),
                        informationCollector: callbackStack == null
                            ? (InformationCollector) null
                            : infoCollector
                    ));
                }

                return _taskQueue.isNotEmpty;
            }

            return false;
        }

        public int scheduleFrameCallback(FrameCallback callback, bool rescheduling = false) {
            scheduleFrame();
            _nextFrameCallbackId += 1;
            _transientCallbacks[key: _nextFrameCallbackId] =
                new _FrameCallbackEntry(callback: callback, rescheduling: rescheduling);
            return _nextFrameCallbackId;
        }

        public void cancelFrameCallbackWithId(int id) {
            D.assert(id > 0);
            _transientCallbacks.Remove(key: id);
            _removedIds.Add(item: id);
        }

        public void addPersistentFrameCallback(FrameCallback callback) {
            _persistentCallbacks.Add(item: callback);
        }

        public void addPostFrameCallback(FrameCallback callback) {
            _postFrameCallbacks.Add(item: callback);
        }

        void _setFramesEnabledState(bool enabled) {
            if (framesEnabled == enabled) {
                return;
            }

            framesEnabled = enabled;
            if (enabled) {
                scheduleFrame();
            }
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
        
        static readonly TimeSpan _coolDownDelay = new TimeSpan(0, 0, 0, 0, 200);
        
#pragma warning disable CS0414
        static Timer frameCoolDownTimer = null;
#pragma warning restore CS0414
        
        public void scheduleFrame() {
            if (hasScheduledFrame || !framesEnabled) {
                return;
            }

            D.assert(() => {
                if (scheduler_.debugPrintScheduleFrameStacks) {
                    Debug.LogFormat("scheduleFrame() called. Current phase is {0}.", schedulerPhase);
                }

                return true;
            });

            ensureFrameCallbacksRegistered();
            Window.instance.scheduleFrame();
            hasScheduledFrame = true;

#if !UNITY_EDITOR
            adjustFrameRate();
#endif
        }

#if !UNITY_EDITOR
        void adjustFrameRate() {
            if (!UIWidgetsGlobalConfiguration.EnableAutoAdjustFramerate) {
                return;
            }
            
            onFrameRateSpeedUp();
            frameCoolDownTimer?.cancel();
            frameCoolDownTimer = Timer.create(_coolDownDelay,
                () => {
                    onFrameRateCoolDown();
                    frameCoolDownTimer = null;
                }
            );
        }
        
        const int defaultMaxRenderFrameInterval = 200;
        const int defaultMinRenderFrameInterval = 1;
        
        void onFrameRateSpeedUp() {
            OnDemandRendering.renderFrameInterval = defaultMinRenderFrameInterval;
        }

        void onFrameRateCoolDown() {
            OnDemandRendering.renderFrameInterval = defaultMaxRenderFrameInterval;
        }
#endif

        public void scheduleForcedFrame() {
            if (!framesEnabled) {
                return;
            }

            if (hasScheduledFrame) {
                return;
            }

            D.assert(() => {
                if (scheduler_.debugPrintScheduleFrameStacks) {
                    Debug.LogFormat("scheduleForcedFrame() called. Current phase is {0}.", schedulerPhase);
                }

                return true;
            });

            ensureFrameCallbacksRegistered();
            Window.instance.scheduleFrame();
            hasScheduledFrame = true;

#if !UNITY_EDITOR
            adjustFrameRate();
#endif
        }

        public void scheduleWarmUpFrame() {
            if (_warmUpFrame || schedulerPhase != SchedulerPhase.idle) {
                return;
            }

            _warmUpFrame = true;
            Timeline.startSync("Warm-up frame");

            var hadScheduledFrame = hasScheduledFrame;
            // We use timers here to ensure that microtasks flush in between.
            Timer.run(() => {
                D.assert(result: _warmUpFrame);
                handleBeginFrame(null);
                return null;
            });
            Timer.run(() => {
                D.assert(result: _warmUpFrame);
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
                if (hadScheduledFrame) {
                    scheduleFrame();
                }

                return null;
            });

            // Lock events so touch events etc don't insert themselves until the
            // scheduled frame has finished.
            lockEvents(() => endOfFrame.then(v => {
                Timeline.finishSync();
                return FutureOr.nil;
            }));
        }

        public void resetEpoch() {
            _epochStart = _adjustForEpoch(rawTimeStamp: currentSystemFrameTimeStamp);
            _firstRawTimeStampInEpoch = null;
        }

        TimeSpan _adjustForEpoch(TimeSpan rawTimeStamp) {
            var rawDurationSinceEpoch = _firstRawTimeStampInEpoch == null
                ? TimeSpan.Zero
                : rawTimeStamp - _firstRawTimeStampInEpoch.Value;
            return new TimeSpan((long) (rawDurationSinceEpoch.Ticks / scheduler_.timeDilation) +
                                _epochStart.Ticks);
        }

        void _handleBeginFrame(TimeSpan rawTimeStamp) {
            if (_warmUpFrame) {
                D.assert(result: !_ignoreNextEngineDrawFrame);
                _ignoreNextEngineDrawFrame = true;
                return;
            }

            handleBeginFrame(rawTimeStamp: rawTimeStamp);
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
            _currentFrameTimeStamp = _adjustForEpoch(rawTimeStamp ?? currentSystemFrameTimeStamp);

            if (rawTimeStamp != null) {
                currentSystemFrameTimeStamp = rawTimeStamp.Value;
            }

            D.assert(() => {
                _debugFrameNumber += 1;

                if (scheduler_.debugPrintBeginFrameBanner || scheduler_.debugPrintEndFrameBanner) {
                    var frameTimeStampDescription = new StringBuilder();
                    if (rawTimeStamp != null) {
                        _debugDescribeTimeStamp(timeStamp: _currentFrameTimeStamp.Value,
                            buffer: frameTimeStampDescription);
                    }
                    else {
                        frameTimeStampDescription.Append("(warm-up frame)");
                    }

                    _debugBanner =
                        $"▄▄▄▄▄▄▄▄ Frame {_debugFrameNumber.ToString().PadRight(7)}   ${frameTimeStampDescription.ToString().PadLeft(18)} ▄▄▄▄▄▄▄▄";
                    if (scheduler_.debugPrintBeginFrameBanner) {
                        Debug.Log(message: _debugBanner);
                    }
                }

                return true;
            });

            D.assert(schedulerPhase == SchedulerPhase.idle);
            hasScheduledFrame = false;

            try {
                Timeline.startSync("Animate");
                schedulerPhase = SchedulerPhase.transientCallbacks;
                var callbacks = _transientCallbacks;
                _transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();
                foreach (var entry in callbacks) {
                    if (!_removedIds.Contains(item: entry.Key)) {
                        _invokeFrameCallback(
                            callback: entry.Value.callback, timeStamp: _currentFrameTimeStamp.Value,
                            callbackStack: entry.Value.debugStack);
                    }
                }

                _removedIds.Clear();
            }
            finally {
                schedulerPhase = SchedulerPhase.midFrameMicrotasks;
            }
        }

        public void handleDrawFrame() {
            
            D.assert(schedulerPhase == SchedulerPhase.midFrameMicrotasks);
            Timeline.finishSync();
            try {
                schedulerPhase = SchedulerPhase.persistentCallbacks;
                foreach (var callback in _persistentCallbacks) {
                    _invokeFrameCallback(callback: callback, timeStamp: _currentFrameTimeStamp.Value);
                }

                schedulerPhase = SchedulerPhase.postFrameCallbacks;
                var localPostFrameCallbacks = new List<FrameCallback>(collection: _postFrameCallbacks);
                _postFrameCallbacks.Clear();
                foreach (var callback in localPostFrameCallbacks) {
                    _invokeFrameCallback(callback: callback, timeStamp: _currentFrameTimeStamp.Value);
                }
            }
            finally {
                schedulerPhase = SchedulerPhase.idle;
                D.assert(() => {
                    if (scheduler_.debugPrintEndFrameBanner) {
                        Debug.Log(new string('▀', count: _debugBanner.Length));
                    }

                    _debugBanner = null;
                    return true;
                });
                _currentFrameTimeStamp = null;
            }
        }

        void _profileFramePostEvent(int frameNumber, FrameTiming frameTiming) {
            developer_.postEvent("Flutter.Frame", new Hashtable {
                {"number", frameNumber},
                {"startTime", frameTiming.timestampInMicroseconds(phase: FramePhase.buildStart)},
                {"elapsed", (int) (frameTiming.totalSpan.TotalMilliseconds * 1000)},
                {"build", (int) (frameTiming.buildDuration.TotalMilliseconds * 1000)},
                {"raster", (int) (frameTiming.rasterDuration.TotalMilliseconds * 1000)}
            });
        }


        static void _debugDescribeTimeStamp(TimeSpan timeStamp, StringBuilder buffer) {
            if (timeStamp.TotalDays > 0) {
                buffer.AppendFormat("{0}d ", arg0: timeStamp.Days);
            }

            if (timeStamp.TotalHours > 0) {
                buffer.AppendFormat("{0}h ", arg0: timeStamp.Hours);
            }

            if (timeStamp.TotalMinutes > 0) {
                buffer.AppendFormat("{0}m ", arg0: timeStamp.Minutes);
            }

            if (timeStamp.TotalSeconds > 0) {
                buffer.AppendFormat("{0}s ", arg0: timeStamp.Seconds);
            }

            buffer.AppendFormat("{0}", arg0: timeStamp.Milliseconds);

            var microseconds = (int) (timeStamp.Ticks % 10000 / 10);
            if (microseconds > 0) {
                buffer.AppendFormat(".{0}", microseconds.ToString().PadLeft(3, '0'));
            }

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
                callback(timeStamp: timeStamp);
            }
            catch (Exception ex) {
                IEnumerable<DiagnosticsNode> infoCollector() {
                    yield return DiagnosticsNode.message(
                        "\nThis exception was thrown in the context of a scheduler callback. " +
                        "When the scheduler callback was _registered_ (as opposed to when the " +
                        "exception was thrown), this was the stack:");
                    var builder = new StringBuilder();
                    foreach (var line in UIWidgetsError.defaultStackFilter(
                        callbackStack.TrimEnd().Split('\n'))) {
                        builder.AppendLine(value: line);
                    }

                    yield return DiagnosticsNode.message(builder.ToString());
                }

                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: ex,
                    "scheduler library",
                    new ErrorDescription("during a scheduler callback"),
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
            if (scheduler.transientCallbackCount > 0) {
                return priority >= Priority.animation.value;
            }

            return true;
        }
    }
}