using System;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.scheduler {
    public delegate void TickerCallback(TimeSpan elapsed);

    public interface TickerProvider {
        Ticker createTicker(TickerCallback onTick);
    }

    public class Ticker : IDisposable {
        public Ticker(TickerCallback onTick, string debugLabel = null) {
            D.assert(() => {
                _debugCreationStack = StackTraceUtility.ExtractStackTrace();
                return true;
            });
            _onTick = onTick;
            this.debugLabel = debugLabel;
        }

        TickerFuture _future;

        public bool muted {
            get => _muted;
            set {
                if (value == _muted)
                    return;

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
                if (_future == null)
                    return false;
                if (muted)
                    return false;
                if (SchedulerBinding.instance.framesEnabled)
                    return true;
                if (SchedulerBinding.instance.schedulerPhase != SchedulerPhase.idle)
                    return true;
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
                        new List<DiagnosticsNode>() {
                            new ErrorSummary("A ticker was started twice."),
                            new ErrorDescription(
                                "A ticker that is already active cannot be started again without first stopping it."),
                            describeForError("The affected ticker was: ")
                        });
                }
                return true;
            });

            D.assert(_startTime == null);
            _future = new TickerFuture();
            if (shouldScheduleTick) {
                scheduleTick();
            }

            if (SchedulerBinding.instance.schedulerPhase > SchedulerPhase.idle &&
                SchedulerBinding.instance.schedulerPhase < SchedulerPhase.postFrameCallbacks) {
                _startTime = SchedulerBinding.instance.currentFrameTimeStamp;
            }

            return _future;
        }

        public DiagnosticsNode describeForError(string name) {
            return new DiagnosticsProperty<Ticker>(name, this, description: toString(debugIncludeStack: true));
        }

        public void stop(bool canceled = false) {
            if (!isActive)
                return;

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
            get => _animationId != null;
        }

        protected bool shouldScheduleTick => !muted && isActive && !scheduled;

        void _tick(TimeSpan timeStamp) {
            D.assert(isTicking);
            D.assert(scheduled);
            _animationId = null;

            _startTime = _startTime ?? timeStamp;
            _onTick(timeStamp - _startTime.Value);

            if (shouldScheduleTick)
                scheduleTick(rescheduling: true);
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

            originalTicker.Dispose();
        }

        public virtual void Dispose() {
            if (_future != null) {
                var localFuture = _future;
                _future = null;
                D.assert(!isActive);
                unscheduleTick();
                localFuture._cancel(this);
            }

            D.assert(() => {
                _startTime = TimeSpan.Zero;
                return true;
            });
        }

        public readonly string debugLabel;
        internal string _debugCreationStack;

        public override string ToString() {
            return toString(debugIncludeStack: false);
        }

        public string toString(bool debugIncludeStack = false) {
            var buffer = new StringBuilder();
            buffer.Append(GetType() + "(");
            D.assert(() => {
                buffer.Append(debugLabel ?? "");
                return true;
            });
            buffer.Append(')');
            D.assert(() => {
                if (debugIncludeStack) {
                    buffer.AppendLine();
                    buffer.AppendLine("The stack trace when the " + GetType() + " was actually created was: ");
                    foreach (var line in UIWidgetsError.defaultStackFilter(
                        _debugCreationStack.TrimEnd().Split('\n'))) {
                        buffer.AppendLine(line);
                    }
                }

                return true;
            });
            return buffer.ToString();
        }
    }

    public class TickerFuture : Future {
        internal TickerFuture() {
        }

        public static TickerFuture complete() {
            var result = new TickerFuture();
            result._complete();
            return result;
        }

        readonly Completer _primaryCompleter = Completer.create();
        Completer _secondaryCompleter;
        bool? _completed; // null means unresolved, true means complete, false means canceled

        internal void _complete() {
            D.assert(_completed == null);
            _completed = true;
            _primaryCompleter.complete();
            _secondaryCompleter?.complete();
        }

        internal void _cancel(Ticker ticker) {
            D.assert(_completed == null);
            _completed = false;
            _secondaryCompleter?.completeError(new TickerCanceled(ticker));
        }

        public void whenCompleteOrCancel(VoidCallback callback) {
            orCancel.then((value) => {
                callback();
                return FutureOr.nil;
            }, ex => {
                callback();
                return FutureOr.nil;
            });
        }

        public Future orCancel {
            get {
                if (_secondaryCompleter == null) {
                    _secondaryCompleter = Completer.create();
                    if (_completed != null) {
                        if (_completed.Value) {
                            _secondaryCompleter.complete();
                        }
                        else {
                            _secondaryCompleter.completeError(new TickerCanceled());
                        }
                    }
                }

                return _secondaryCompleter.future;
            }
        }

        // public override Stream asStream() {
        //     return _primaryCompleter.future.asStream();
        // }

        public override Future catchError(Func<Exception, FutureOr> onError, Func<Exception, bool> test = null) {
            return _primaryCompleter.future.catchError(onError, test: test);
        }

        public override Future then(Func<object, FutureOr> onValue, Func<Exception, FutureOr> onError = null) {
            return _primaryCompleter.future.then(onValue, onError: onError);
        }

        public override Future timeout(TimeSpan timeLimit, Func<FutureOr> onTimeout = null) {
            return _primaryCompleter.future.timeout(timeLimit, onTimeout: onTimeout);
        }

        public override Future whenComplete(Func<FutureOr> action) {
            return _primaryCompleter.future.whenComplete(action);
        }

        public override string ToString() =>
            $"{foundation_.describeIdentity(this)}({(_completed == null ? "active" : (_completed.Value ? "complete" : "canceled"))})";
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
}