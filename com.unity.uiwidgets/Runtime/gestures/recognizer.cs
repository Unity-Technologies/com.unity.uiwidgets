using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public delegate T RecognizerCallback<T>();

    public delegate void RecognizerVoidCallvack();

    public enum DragStartBehavior {
        down,
        start
    }

    public abstract class GestureRecognizer : DiagnosticableTree, GestureArenaMember {
        protected GestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null) {
            this.debugOwner = debugOwner;
            _kindFilter = kind;
        }

        public readonly object debugOwner;

        readonly PointerDeviceKind? _kindFilter;

        readonly Dictionary<int, PointerDeviceKind> _pointerToKind = new Dictionary<int, PointerDeviceKind>{};

        public void addPointer(PointerDownEvent evt) {
            _pointerToKind[evt.pointer] = evt.kind;
            if (isPointerAllowed(evt)) {
                addAllowedPointer(evt);
            }
            else {
                handleNonAllowedPointer(evt);
            }
        }

        public abstract void addAllowedPointer(PointerEvent evt);

        protected virtual void handleNonAllowedPointer(PointerDownEvent evt) {
        }

        protected virtual bool isPointerAllowed(PointerDownEvent evt) {
            return _kindFilter == null || _kindFilter == evt.kind;
        }

        protected virtual PointerDeviceKind getKindForPointer(int pointer) {
            D.assert(_pointerToKind.ContainsKey(pointer));
            return _pointerToKind[pointer];
        }

        public virtual void addScrollPointer(PointerScrollEvent evt) {
        }

        public virtual void dispose() {
        }

        public abstract string debugDescription { get; }

        protected T invokeCallback<T>(string name, RecognizerCallback<T> callback, Func<string> debugReport = null) {
            D.assert(callback != null);

            T result = default(T);
            try {
                D.assert(() => {
                    if (D.debugPrintRecognizerCallbacksTrace) {
                        var report = debugReport != null ? debugReport() : null;
                        // The 19 in the line below is the width of the prefix used by
                        // _debugLogDiagnostic in arena.dart.
                        var prefix = D.debugPrintGestureArenaDiagnostics ? new string(' ', 19) + "❙ " : "";
                        Debug.LogFormat("{0}this calling {1} callback.{2}",
                            prefix, name, report.isNotEmpty() ? " " + report : "");
                    }

                    return true;
                });

                result = callback();
            }
            catch (Exception ex) {
                IEnumerable<DiagnosticsNode> infoCollector() {
                    yield return new StringProperty("Handler", name);
                    yield return new DiagnosticsProperty<GestureRecognizer>("Recognizer", this, style: DiagnosticsTreeStyle.errorProperty);
                }
                
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "gesture",
                    context: new ErrorDescription("while handling a gesture"),
                    informationCollector: infoCollector
                ));
            }

            return result;
        }
        protected void invokeCallback(string name, RecognizerVoidCallvack callback, Func<string> debugReport = null) {
            D.assert(callback != null);
            try {
                D.assert(() => {
                    if (D.debugPrintRecognizerCallbacksTrace) {
                        var report = debugReport != null ? debugReport() : null;
                        var prefix = D.debugPrintGestureArenaDiagnostics ? new string(' ', 19) + "❙ " : "";
                        Debug.LogFormat("{0}this calling {1} callback.{2}",
                            prefix, name, report.isNotEmpty() ? " " + report : "");
                    }

                    return true;
                });

                callback();
            }
            catch (Exception ex) {
                IEnumerable<DiagnosticsNode> infoCollector() {
                    yield return new StringProperty("Handler", name);
                    yield return new DiagnosticsProperty<GestureRecognizer>("Recognizer", this, style: DiagnosticsTreeStyle.errorProperty);
                }
                
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "gesture",
                    context: new ErrorDescription("while handling a gesture"),
                    informationCollector: infoCollector
                ));
            }

            
        }


        public abstract void acceptGesture(int pointer);
        public abstract void rejectGesture(int pointer);

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("debugOwner", debugOwner,
                defaultValue: foundation_.kNullDefaultValue));
        }
    }

    public abstract class OneSequenceGestureRecognizer : GestureRecognizer {
        protected OneSequenceGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null) : base(
            debugOwner, kind) {
        }

        readonly Dictionary<int, GestureArenaEntry> _entries = new Dictionary<int, GestureArenaEntry>();

        readonly HashSet<int> _trackedPointers = new HashSet<int>();

        protected abstract void handleEvent(PointerEvent evt);

        public override void acceptGesture(int pointer) {
        }

        public override void rejectGesture(int pointer) {
        }

        protected abstract void didStopTrackingLastPointer(int pointer);

        protected virtual void didStopTrackingLastScrollerPointer(int pointer) {
        }

        protected virtual void resolve(GestureDisposition disposition) {
            var localEntries = new List<GestureArenaEntry>(_entries.Values);
            _entries.Clear();
            foreach (GestureArenaEntry entry in localEntries) {
                entry.resolve(disposition);
            }
        }

        protected virtual void resolvePointer(int pointer, GestureDisposition disposition) {
            GestureArenaEntry entry = _entries[pointer];
            if (entry != null) {
                entry.resolve(disposition);
                _entries.Remove(pointer);
            }
        }

        public override void dispose() {
            resolve(GestureDisposition.rejected);
            foreach (int pointer in _trackedPointers) {
                GestureBinding.instance.pointerRouter.removeRoute(pointer, handleEvent);
            }

            _trackedPointers.Clear();
            D.assert(_entries.isEmpty());
            base.dispose();
        }

        public GestureArenaTeam team {
            get { return _team; }
            set {
                D.assert(value != null);
                D.assert(_entries.isEmpty());
                D.assert(_trackedPointers.isEmpty());
                D.assert(_team == null);
                _team = value;
            }
        }

        GestureArenaTeam _team;

        GestureArenaEntry _addPointerToArena(int pointer) {
            if (_team != null) {
                return _team.add(pointer, this);
            }

            return GestureBinding.instance.gestureArena.add(pointer, this);
        }

        protected void startTrackingScrollerPointer(int pointer) {
            GestureBinding.instance.pointerRouter.addRoute(pointer, handleEvent);
        }

        protected void stopTrackingScrollerPointer(int pointer) {
            if (_trackedPointers.isEmpty()) {
                didStopTrackingLastScrollerPointer(pointer);
            }
        }

        protected void startTrackingPointer(int pointer, Matrix4 transform = null) {
            GestureBinding.instance.pointerRouter.addRoute(pointer, handleEvent, transform);
            _trackedPointers.Add(pointer);
            D.assert(!_entries.ContainsKey(pointer));
            _entries[pointer] = _addPointerToArena(pointer);
        }

        protected void stopTrackingPointer(int pointer) {
            if (_trackedPointers.Contains(pointer)) {
                GestureBinding.instance.pointerRouter.removeRoute(pointer, handleEvent);
                _trackedPointers.Remove(pointer);
                if (_trackedPointers.isEmpty()) {
                    didStopTrackingLastPointer(pointer);
                }
            }
        }

        protected void stopTrackingIfPointerNoLongerDown(PointerEvent evt) {
            if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                stopTrackingPointer(evt.pointer);
            }
        }
    }

    public enum GestureRecognizerState {
        ready,
        possible,
        accepted,
        defunct,
    }

    public abstract class PrimaryPointerGestureRecognizer : OneSequenceGestureRecognizer {
        protected PrimaryPointerGestureRecognizer(
            TimeSpan? deadline = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null,
            float? preAcceptSlopTolerance = Constants.kTouchSlop,
            float? postAcceptSlopTolerance = Constants.kTouchSlop
        ) : base(debugOwner: debugOwner, kind: kind) {
            D.assert(preAcceptSlopTolerance == null || preAcceptSlopTolerance >= 0,
                () => "The preAcceptSlopTolerance must be positive or null");

            D.assert(postAcceptSlopTolerance == null || postAcceptSlopTolerance >= 0,
                () => "The postAcceptSlopTolerance must be positive or null");

            this.deadline = deadline;
            this.preAcceptSlopTolerance = preAcceptSlopTolerance;
            this.postAcceptSlopTolerance = postAcceptSlopTolerance;
        }

        public readonly TimeSpan? deadline;

        public readonly float? preAcceptSlopTolerance;

        public readonly float? postAcceptSlopTolerance;

        public GestureRecognizerState state = GestureRecognizerState.ready;

        public int primaryPointer;

        public OffsetPair initialPosition;

        Timer _timer;

        public override void addAllowedPointer(PointerEvent evt) {
            var _evt = (PointerDownEvent) evt;
            D.assert(_evt != null);
            startTrackingPointer(_evt.pointer, _evt.transform);
            if (state == GestureRecognizerState.ready) {
                state = GestureRecognizerState.possible;
                primaryPointer = _evt.pointer;
                initialPosition = new OffsetPair(local: _evt.localPosition, global: _evt.position);
                if (deadline != null) {
                    _timer = Timer.create(deadline.Value, () => didExceedDeadlineWithEvent(_evt));
                }
            }
        }

        protected override void handleEvent(PointerEvent evt) {
            D.assert(state != GestureRecognizerState.ready);

            if (evt.pointer == primaryPointer) {
                bool isPreAcceptSlopPastTolerance = state == GestureRecognizerState.possible &&
                                                    preAcceptSlopTolerance != null &&
                                                    _getGlobalDistance(evt) > preAcceptSlopTolerance;
                bool isPostAcceptSlopPastTolerance = state == GestureRecognizerState.accepted &&
                                                     postAcceptSlopTolerance != null &&
                                                     _getGlobalDistance(evt) > postAcceptSlopTolerance;

                if (evt is PointerMoveEvent && (isPreAcceptSlopPastTolerance || isPostAcceptSlopPastTolerance)) {
                    resolve(GestureDisposition.rejected);
                    stopTrackingPointer(primaryPointer);
                }
                else {
                    handlePrimaryPointer(evt);
                }
            }

            stopTrackingIfPointerNoLongerDown(evt);
        }

        protected abstract void handlePrimaryPointer(PointerEvent evt);

        protected virtual void didExceedDeadline() {
            D.assert(deadline == null);
        }

        protected virtual void didExceedDeadlineWithEvent(PointerDownEvent evt) {
            didExceedDeadline();
        }

        public override void acceptGesture(int pointer) {
            if (pointer == primaryPointer && state == GestureRecognizerState.possible) {
                _stopTimer();
                state = GestureRecognizerState.accepted;
            }
        }

        public override void rejectGesture(int pointer) {
            if (pointer == primaryPointer && (state == GestureRecognizerState.possible ||
                                                   state == GestureRecognizerState.accepted)) {
                _stopTimer();
                state = GestureRecognizerState.defunct;
            }
        }

        protected override void didStopTrackingLastPointer(int pointer) {
            _stopTimer();
            state = GestureRecognizerState.ready;
        }

        public override void dispose() {
            _stopTimer();
            base.dispose();
        }

        void _stopTimer() {
            if (_timer != null) {
                _timer.cancel();
                _timer = null;
            }
        }

        float _getGlobalDistance(PointerEvent evt) {
            Offset offset = evt.position - initialPosition.global;
            return offset.distance;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<GestureRecognizerState>("state", state));
        }
    }

    public class OffsetPair {
        /// Creates a [OffsetPair] combining a [local] and [global] [Offset].
        public OffsetPair(
            Offset local = null,
            Offset global = null
        ) {
            this.local = local;
            this.global = global;
        }

        /// Creates a [OffsetPair] from [PointerEvent.localPosition] and
        /// [PointerEvent.position].
        public static OffsetPair fromEventPosition(PointerEvent evt) {
            return new OffsetPair(local: evt.localPosition, global: evt.position);
        }

        /// Creates a [OffsetPair] from [PointerEvent.localDelta] and
        /// [PointerEvent.delta].
        public static OffsetPair fromEventDelta(PointerEvent evt) {
            return new OffsetPair(local: evt.localDelta, global: evt.delta);
        }

        /// A [OffsetPair] where both [Offset]s are [Offset.zero].
        public readonly static OffsetPair zero = new OffsetPair(local: Offset.zero, global: Offset.zero);

        /// The [Offset] in the local coordinate space.
        public readonly Offset local;

        /// The [Offset] in the global coordinate space after conversion to logical
        /// pixels.
        public readonly Offset global;

        /// Adds the `other.global` to [global] and `other.local` to [local].
        public static OffsetPair operator +(OffsetPair current, OffsetPair other) {
            return new OffsetPair(
                local: current.local + other.local,
                global: current.global + other.global
            );
        }

        /// Subtracts the `other.global` from [global] and `other.local` from [local].
        public static OffsetPair operator -(OffsetPair current, OffsetPair other) {
            return new OffsetPair(
                local: current.local - other.local,
                global: current.global - other.global
            );
        }

        public override string ToString() {
            return $"runtimeType(local: ${local}, global: ${global})";
        }
    }
}