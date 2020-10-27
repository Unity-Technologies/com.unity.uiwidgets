/*
using System;
using System.Text;
using RSG;
using RSG.Promises;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.scheduler {
    public delegate void TickerCallback(TimeSpan elapsed);

    public interface TickerProvider {
        Ticker createTicker(TickerCallback onTick);
    }

    public class Ticker {
        public Ticker(TickerCallback onTick, Func<string> debugLabel = null) {
            D.assert(() => {
                _debugCreationStack = "skipped, use StackTraceUtility.ExtractStackTrace() if you need it"; // StackTraceUtility.ExtractStackTrace();
                return true;
            });
            _onTick = onTick;
            this.debugLabel = debugLabel;
        }

        TickerFutureImpl _future;

        public bool muted {
            get { return _muted; }
            set {
                if (value == _muted) {
                    return;
                }

                _muted = value;
                if (value) {
                    unscheduleTick();
                }
                else if (shouldScheduleTick) {
                    scheduleTick();
                }
            }
        }

        bool _muted = false;

        public bool isTicking {
            get {
                if (_future == null) {
                    return false;
                }

                if (muted) {
                    return false;
                }

                if (SchedulerBinding.instance.framesEnabled) {
                    return true;
                }

                if (SchedulerBinding.instance.schedulerPhase != SchedulerPhase.idle) {
                    return true;
                }

                return false;
            }
        }

        public bool isActive {
            get { return _future != null; }
        }

        TimeSpan? _startTime;

        public TickerFuture start() {
            D.assert(() => {
                if (isActive) {
                    throw new UIWidgetsError(
                        "A ticker that is already active cannot be started again without first stopping it.\n" +
                        "The affected ticker was: " + toString(debugIncludeStack: true));
                }

                return true;
            });

            D.assert(_startTime == null);
            _future = new TickerFutureImpl();
            if (shouldScheduleTick) {
                scheduleTick();
            }

            if (SchedulerBinding.instance.schedulerPhase > SchedulerPhase.idle &&
                SchedulerBinding.instance.schedulerPhase < SchedulerPhase.postFrameCallbacks) {
                _startTime = SchedulerBinding.instance.currentFrameTimeStamp;
            }

            return _future;
        }

        public void stop(bool canceled = false) {
            if (!isActive) {
                return;
            }

            var localFuture = _future;
            _future = null;
            _startTime = null;
            D.assert(!isActive);

            unscheduleTick();
            if (canceled) {
                localFuture._cancel(this);
            }
            else {
                localFuture._complete();
            }
        }

        readonly TickerCallback _onTick;

        int? _animationId;

        protected bool scheduled {
            get { return _animationId != null; }
        }

        protected bool shouldScheduleTick {
            get { return !muted && isActive && !scheduled; }
        }

        void _tick(TimeSpan timeStamp) {
            D.assert(isTicking);
            D.assert(scheduled);
            _animationId = null;

            _startTime = _startTime ?? timeStamp;

            _onTick(timeStamp - _startTime.Value);

            if (shouldScheduleTick) {
                scheduleTick(rescheduling: true);
            }
        }

        protected void scheduleTick(bool rescheduling = false) {
            D.assert(!scheduled);
            D.assert(shouldScheduleTick);
            _animationId = SchedulerBinding.instance.scheduleFrameCallback(_tick, rescheduling: rescheduling);
        }

        protected void unscheduleTick() {
            if (scheduled) {
                SchedulerBinding.instance.cancelFrameCallbackWithId(_animationId.Value);
                _animationId = null;
            }

            D.assert(!shouldScheduleTick);
        }

        public void absorbTicker(Ticker originalTicker) {
            D.assert(!isActive);
            D.assert(_future == null);
            D.assert(_startTime == null);
            D.assert(_animationId == null);
            D.assert((originalTicker._future == null) == (originalTicker._startTime == null),
                () => "Cannot absorb Ticker after it has been disposed.");
            if (originalTicker._future != null) {
                _future = originalTicker._future;
                _startTime = originalTicker._startTime;
                if (shouldScheduleTick) {
                    scheduleTick();
                }

                originalTicker._future = null;
                originalTicker.unscheduleTick();
            }

            originalTicker.dispose();
        }

        public virtual void dispose() {
            if (_future != null) {
                var localFuture = _future;
                _future = null;
                D.assert(!isActive);
                unscheduleTick();
                localFuture._cancel(this);
            }

            D.assert(() => {
                _startTime = default(TimeSpan);
                return true;
            });
        }

        internal readonly Func<string> debugLabel;

        string _debugCreationStack;

        public override string ToString() {
            return toString(debugIncludeStack: false);
        }

        public string toString(bool debugIncludeStack = false) {
            var buffer = new StringBuilder();
            buffer.Append(GetType() + "(");
            D.assert(() => {
                if (debugLabel != null) {
                    buffer.Append(debugLabel());
                }
                return true;
            });
            buffer.Append(')');
            D.assert(() => {
                if (debugIncludeStack) {
                    buffer.AppendLine();
                    buffer.AppendLine("The stack trace when the " + GetType() + " was actually created was:");
                    UIWidgetsError.defaultStackFilter(_debugCreationStack.TrimEnd().Split('\n'))
                        .Each(line => buffer.AppendLine(line));
                }

                return true;
            });
            return buffer.ToString();
        }
    }

    public interface TickerFuture : IPromise {
        void whenCompleteOrCancel(VoidCallback callback);

        IPromise orCancel { get; }
    }

    public class TickerFutureImpl : Promise, TickerFuture {
        public static TickerFuture complete() {
            var result = new TickerFutureImpl();
            result._complete();
            return result;
        }

        Promise _secondaryCompleter;
        bool? _completed;

        internal void _complete() {
            D.assert(_completed == null);
            _completed = true;
            Resolve();
            if (_secondaryCompleter != null) {
                _secondaryCompleter.Resolve();
            }
        }

        internal void _cancel(Ticker ticker) {
            D.assert(_completed == null);
            _completed = false;
            if (_secondaryCompleter != null) {
                _secondaryCompleter.Reject(new TickerCanceled(ticker));
            }
        }

        public void whenCompleteOrCancel(VoidCallback callback) {
            orCancel.Then(() => callback(), ex => callback());
        }

        public IPromise orCancel {
            get {
                if (_secondaryCompleter == null) {
                    _secondaryCompleter = new Promise();
                    if (_completed != null) {
                        if (_completed.Value) {
                            _secondaryCompleter.Resolve();
                        }
                        else {
                            _secondaryCompleter.Reject(new TickerCanceled());
                        }
                    }
                }

                return _secondaryCompleter;
            }
        }
    }

    public class TickerCanceled : Exception {
        public TickerCanceled(Ticker ticker = null) {
            this.ticker = ticker;
        }

        public readonly Ticker ticker;

        public override string ToString() {
            if (ticker != null) {
                return "This ticker was canceled: " + ticker;
            }

            return "The ticker was canceled before the \"orCancel\" property was first used.";
        }
    }
}*/