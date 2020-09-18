using System;
using System.Collections.Generic;
using System.Text;
using RSG.Promises;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Debug = UnityEngine.Debug;
using FrameCallback = Unity.UIWidgets.ui.FrameCallback;

namespace Unity.UIWidgets.scheduler {
    class _FrameCallbackEntry {
        internal _FrameCallbackEntry(FrameCallback callback, bool rescheduling = false) {
            this.callback = callback;

            D.assert(() => {
                if (rescheduling) {
                    D.assert(() => {
                        if (debugCurrentCallbackStack == null) {
                            throw new UIWidgetsError(
                                "scheduleFrameCallback called with rescheduling true, but no callback is in scope.\n" +
                                "The \"rescheduling\" argument should only be set to true if the " +
                                "callback is being reregistered from within the callback itself, " +
                                "and only then if the callback itself is entirely synchronous. " +
                                "If this is the initial registration of the callback, or if the " +
                                "callback is asynchronous, then do not use the \"rescheduling\" " +
                                "argument.");
                        }

                        return true;
                    });
                    debugStack = debugCurrentCallbackStack;
                } else {
                    debugStack = "skipped, use StackTraceUtility.ExtractStackTrace() if you need it"; // StackTraceUtility.ExtractStackTrace();
                }

                return true;
            });
        }

        public readonly FrameCallback callback;

        internal static string debugCurrentCallbackStack;
        internal string debugStack;
    }

    public enum SchedulerPhase {
        idle,
        transientCallbacks,
        midFrameMicrotasks,
        persistentCallbacks,
        postFrameCallbacks,
    }

    public class SchedulerBinding {
        public static SchedulerBinding instance {
            get {
                D.assert(_instance != null,
                    () => "Binding.instance is null. " +
                    "This usually happens when there is a callback from outside of UIWidgets. " +
                    "Try to use \"using (WindowProvider.of(BuildContext).getScope()) { ... }\" to wrap your code.");
                return _instance;
            }

            set {
                if (value == null) {
                    D.assert(_instance != null, () => "Binding.instance is already cleared.");
                    _instance = null;
                } else {
                    D.assert(_instance == null, () => "Binding.instance is already assigned.");
                    _instance = value;
                }
            }
        }

        internal static SchedulerBinding _instance;

        public SchedulerBinding() {
            Window.instance.onBeginFrame += _handleBeginFrame;
            Window.instance.onDrawFrame += _handleDrawFrame;
        }

        public float timeDilation {
            get { return _timeDilation; }
            set {
                if (_timeDilation == value) {
                    return;
                }

                resetEpoch();
                _timeDilation = value;
            }
        }

        float _timeDilation = 1.0f;

        int _nextFrameCallbackId = 0;
        Dictionary<int, _FrameCallbackEntry> _transientCallbacks = new Dictionary<int, _FrameCallbackEntry>();
        readonly HashSet<int> _removedIds = new HashSet<int>();

        public int transientCallbackCount {
            get { return _transientCallbacks.Count; }
        }

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

        public bool hasScheduledFrame {
            get { return _hasScheduledFrame; }
        }

        bool _hasScheduledFrame = false;

        public SchedulerPhase schedulerPhase {
            get { return _schedulerPhase; }
        }

        SchedulerPhase _schedulerPhase = SchedulerPhase.idle;

        public bool framesEnabled {
            get { return _framesEnabled; }
            set {
                if (_framesEnabled == value) {
                    return;
                }

                _framesEnabled = value;
                if (value) {
                    scheduleFrame();
                }
            }
        }

        bool _framesEnabled = true; // todo: set it to false when app switched to background

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
            if (_hasScheduledFrame || !_framesEnabled) {
                return;
            }

            D.assert(() => {
                if (scheduler_.debugPrintScheduleFrameStacks) {
                    Debug.LogFormat("scheduleFrame() called. Current phase is {0}.", schedulerPhase);
                }

                return true;
            });

            Window.instance.scheduleFrame();
            _hasScheduledFrame = true;
        }

        public void scheduleForcedFrame() {
            if (_hasScheduledFrame) {
                return;
            }

            D.assert(() => {
                if (scheduler_.debugPrintScheduleFrameStacks) {
                    Debug.LogFormat("scheduleForcedFrame() called. Current phase is {0}.", schedulerPhase);
                }

                return true;
            });

            Window.instance.scheduleFrame();
            _hasScheduledFrame = true;
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
            return new TimeSpan((long) (rawDurationSinceEpoch.Ticks / timeDilation) + _epochStart.Ticks);
        }

        public TimeSpan currentFrameTimeStamp {
            get { return _currentFrameTimeStamp.Value; }
        }

        TimeSpan? _currentFrameTimeStamp;

        int _profileFrameNumber = 0;
        string _debugBanner;

        void _handleBeginFrame(TimeSpan rawTimeStamp) {
            handleBeginFrame(rawTimeStamp);
        }

        void _handleDrawFrame() {
            handleDrawFrame();
        }

        public void handleBeginFrame(TimeSpan? rawTimeStamp) {
            _firstRawTimeStampInEpoch = _firstRawTimeStampInEpoch ?? rawTimeStamp;
            _currentFrameTimeStamp = _adjustForEpoch(rawTimeStamp ?? _lastRawTimeStamp);

            if (rawTimeStamp != null) {
                _lastRawTimeStamp = rawTimeStamp.Value;
            }

            D.assert(() => {
                _profileFrameNumber += 1;
                return true;
            });

            D.assert(() => {
                if (scheduler_.debugPrintBeginFrameBanner || scheduler_.debugPrintEndFrameBanner) {
                    var frameTimeStampDescription = new StringBuilder();
                    if (rawTimeStamp != null) {
                        _debugDescribeTimeStamp(
                            _currentFrameTimeStamp.Value, frameTimeStampDescription);
                    } else {
                        frameTimeStampDescription.Append("(warm-up frame)");
                    }

                    _debugBanner =
                        $"▄▄▄▄▄▄▄▄ Frame {_profileFrameNumber.ToString().PadRight(7)}   {frameTimeStampDescription.ToString().PadLeft(18)} ▄▄▄▄▄▄▄▄";
                    if (scheduler_.debugPrintBeginFrameBanner) {
                        Debug.Log(_debugBanner);
                    }
                }

                return true;
            });

            D.assert(_schedulerPhase == SchedulerPhase.idle);
            _hasScheduledFrame = false;

            try {
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
            } finally {
                _schedulerPhase = SchedulerPhase.midFrameMicrotasks;
            }
        }


        public void handleDrawFrame() {
            D.assert(_schedulerPhase == SchedulerPhase.midFrameMicrotasks);

            try {
                _schedulerPhase = SchedulerPhase.persistentCallbacks;
                foreach (FrameCallback callback in _persistentCallbacks) {
                    _invokeFrameCallback(callback, _currentFrameTimeStamp.Value);
                }

                _schedulerPhase = SchedulerPhase.postFrameCallbacks;
                var localPostFrameCallbacks = new List<FrameCallback>(_postFrameCallbacks);
                _postFrameCallbacks.Clear();
                foreach (FrameCallback callback in localPostFrameCallbacks) {
                    _invokeFrameCallback(callback, _currentFrameTimeStamp.Value);
                }
            } finally {
                _schedulerPhase = SchedulerPhase.idle;
                D.assert(() => {
                    if (scheduler_.debugPrintEndFrameBanner) {
                        Debug.Log(new string('▀', _debugBanner.Length));
                    }

                    _debugBanner = null;
                    return true;
                });
                _currentFrameTimeStamp = null;
            }
        }

        static void _debugDescribeTimeStamp(TimeSpan timeStamp, StringBuilder buffer) {
            if (timeStamp.TotalDays > 0) {
                buffer.AppendFormat("{0}d ", timeStamp.Days);
            }

            if (timeStamp.TotalHours > 0) {
                buffer.AppendFormat("{0}h ", timeStamp.Hours);
            }

            if (timeStamp.TotalMinutes > 0) {
                buffer.AppendFormat("{0}m ", timeStamp.Minutes);
            }

            if (timeStamp.TotalSeconds > 0) {
                buffer.AppendFormat("{0}s ", timeStamp.Seconds);
            }

            buffer.AppendFormat("{0}", timeStamp.Milliseconds);

            int microseconds = (int) (timeStamp.Ticks % 10000 / 10);
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
                callback(timeStamp);
            } catch (Exception ex) {
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "scheduler library",
                    context: "during a scheduler callback",
                    informationCollector: callbackStack == null
                        ? (InformationCollector) null
                        : information => {
                            information.AppendLine(
                                "\nThis exception was thrown in the context of a scheduler callback. " +
                                "When the scheduler callback was _registered_ (as opposed to when the " +
                                "exception was thrown), this was the stack:"
                            );
                            UIWidgetsError.defaultStackFilter(callbackStack.TrimEnd().Split('\n'))
                                .Each((line) => information.AppendLine(line));
                        }
                ));
            }

            D.assert(() => {
                _FrameCallbackEntry.debugCurrentCallbackStack = null;
                return true;
            });
        }
    }
}
