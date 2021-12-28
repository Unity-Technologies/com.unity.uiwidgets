using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate Drag GestureMultiDragStartCallback(Offset position);

    public abstract class MultiDragPointerState {
        public MultiDragPointerState(
            Offset initialPosition = null) {
            D.assert(initialPosition != null);
            this.initialPosition = initialPosition;
        }

        public readonly Offset initialPosition;

        readonly VelocityTracker _velocityTracker = new VelocityTracker();

        Drag _client;

        public Offset pendingDelta {
            get { return _pendingDelta; }
        }

        public Offset _pendingDelta = Offset.zero;

        TimeSpan? _lastPendingEventTimestamp;

        GestureArenaEntry _arenaEntry;

        public void _setArenaEntry(GestureArenaEntry entry) {
            D.assert(_arenaEntry == null);
            D.assert(pendingDelta != null);
            D.assert(_client == null);
            _arenaEntry = entry;
        }

        protected void resolve(GestureDisposition disposition) {
            _arenaEntry.resolve(disposition);
        }

        public void _move(PointerMoveEvent pEvent) {
            D.assert(_arenaEntry != null);
            if (!pEvent.synthesized) {
                _velocityTracker.addPosition(pEvent.timeStamp, pEvent.position);
            }

            if (_client != null) {
                D.assert(pendingDelta == null);
                _client.update(new DragUpdateDetails(
                    sourceTimeStamp: pEvent.timeStamp,
                    delta: pEvent.delta,
                    globalPosition: pEvent.position
                ));
            }
            else {
                D.assert(pendingDelta != null);
                _pendingDelta += pEvent.delta;
                _lastPendingEventTimestamp = pEvent.timeStamp;
                checkForResolutionAfterMove();
            }
        }

        public virtual void checkForResolutionAfterMove() {
        }

        public abstract void accepted(GestureMultiDragStartCallback starter);

        public void rejected() {
            D.assert(_arenaEntry != null);
            D.assert(_client == null);
            D.assert(pendingDelta != null);
            _pendingDelta = null;
            _lastPendingEventTimestamp = null;
            _arenaEntry = null;
        }

        public void _startDrag(Drag client) {
            D.assert(_arenaEntry != null);
            D.assert(_client == null);
            D.assert(client != null);
            D.assert(pendingDelta != null);

            _client = client;
            DragUpdateDetails details = new DragUpdateDetails(
                sourceTimeStamp: _lastPendingEventTimestamp ?? TimeSpan.Zero,
                pendingDelta,
                globalPosition: initialPosition
            );

            _pendingDelta = null;
            _lastPendingEventTimestamp = null;
            _client.update(details);
        }

        public void _up() {
            D.assert(_arenaEntry != null);
            if (_client != null) {
                D.assert(pendingDelta == null);
                DragEndDetails details = new DragEndDetails(velocity: _velocityTracker.getVelocity());
                Drag client = _client;
                _client = null;
                client.end(details);
            }
            else {
                D.assert(pendingDelta != null);
                _pendingDelta = null;
                _lastPendingEventTimestamp = null;
            }
        }

        public void _cancel() {
            D.assert(_arenaEntry != null);
            if (_client != null) {
                D.assert(pendingDelta == null);
                Drag client = _client;
                _client = null;
                client.cancel();
            }
            else {
                D.assert(pendingDelta != null);
                _pendingDelta = null;
                _lastPendingEventTimestamp = null;
            }
        }

        public virtual void dispose() {
            _arenaEntry?.resolve(GestureDisposition.rejected);
            _arenaEntry = null;
            D.assert(() => {
                _pendingDelta = null;
                return true;
            });
        }
    }


    public abstract class MultiDragGestureRecognizer<T> : GestureRecognizer where T : MultiDragPointerState {
        protected MultiDragGestureRecognizer(
            object debugOwner, PointerDeviceKind? kind = null) : base(debugOwner: debugOwner, kind: kind) {
        }

        public GestureMultiDragStartCallback onStart;

        Dictionary<int, T> _pointers = new Dictionary<int, T>();

        public override void addAllowedPointer(PointerEvent pEvent) {
            var _pEvent = (PointerDownEvent) pEvent;
            D.assert(_pEvent != null);
            D.assert(_pointers != null);
            D.assert(_pEvent.position != null);
            D.assert(!_pointers.ContainsKey(_pEvent.pointer));

            T state = createNewPointerState(_pEvent);
            _pointers[_pEvent.pointer] = state;
            GestureBinding.instance.pointerRouter.addRoute(_pEvent.pointer, _handleEvent);
            state._setArenaEntry(GestureBinding.instance.gestureArena.add(_pEvent.pointer, this));
        }

        public abstract T createNewPointerState(PointerDownEvent pEvent);

        void _handleEvent(PointerEvent pEvent) {
            D.assert(_pointers != null);
            D.assert(pEvent.timeStamp != null);
            D.assert(pEvent.position != null);
            D.assert(_pointers.ContainsKey(pEvent.pointer));

            T state = _pointers[pEvent.pointer];
            if (pEvent is PointerMoveEvent) {
                state._move((PointerMoveEvent) pEvent);
            }
            else if (pEvent is PointerUpEvent) {
                D.assert(pEvent.delta == Offset.zero);
                state._up();
                _removeState(pEvent.pointer);
            }
            else if (pEvent is PointerCancelEvent) {
                D.assert(pEvent.delta == Offset.zero);
                state._cancel();
                _removeState(pEvent.pointer);
            }
            else if (!(pEvent is PointerDownEvent)) {
                D.assert(false);
            }
        }

        public override void acceptGesture(int pointer) {
            D.assert(_pointers != null);
            T state = _pointers[pointer];
            if (state == null) {
                return;
            }

            state.accepted((Offset initialPosition) => _startDrag(initialPosition, pointer));
        }

        Drag _startDrag(Offset initialPosition, int pointer) {
            D.assert(_pointers != null);
            T state = _pointers[pointer];
            D.assert(state != null);
            D.assert(state._pendingDelta != null);
            Drag drag = null;
            if (onStart != null) {
                drag = invokeCallback("onStart", () => onStart(initialPosition));
            }

            if (drag != null) {
                state._startDrag(drag);
            }
            else {
                _removeState(pointer);
            }

            return drag;
        }

        public override void rejectGesture(int pointer) {
            D.assert(_pointers != null);
            if (_pointers.ContainsKey(pointer)) {
                T state = _pointers[pointer];
                D.assert(state != null);
                state.rejected();
                _removeState(pointer);
            }
        }

        void _removeState(int pointer) {
            if (_pointers == null) {
                return;
            }

            D.assert(_pointers.ContainsKey(pointer));
            GestureBinding.instance.pointerRouter.removeRoute(pointer, _handleEvent);
            var pointerData = _pointers[pointer];
            _pointers.Remove(pointer);
            pointerData.dispose();
        }


        public override void dispose() {
            foreach (var key in _pointers.Keys.ToList()) {
                _removeState(key);
            }
            D.assert(_pointers.isEmpty);
            _pointers = null;
            base.dispose();
        }
    }


    public class _ImmediatePointerState : MultiDragPointerState {
        public _ImmediatePointerState(Offset initialPosition) : base(initialPosition) {
        }

        public override void checkForResolutionAfterMove() {
            D.assert(pendingDelta != null);
            if (pendingDelta.distance > Constants.kTouchSlop) {
                resolve(GestureDisposition.accepted);
            }
        }

        public override void accepted(GestureMultiDragStartCallback starter) {
            starter(initialPosition);
        }
    }


    public class ImmediateMultiDragGestureRecognizer : MultiDragGestureRecognizer<_ImmediatePointerState> {
        public ImmediateMultiDragGestureRecognizer(object debugOwner, PointerDeviceKind? kind = null) : base(
            debugOwner: debugOwner, kind: kind) {
        }

        public override _ImmediatePointerState createNewPointerState(PointerDownEvent pEvent) {
            return new _ImmediatePointerState(pEvent.position);
        }

        public override string debugDescription {
            get { return "multidrag"; }
        }
    }

    public class _HorizontalPointerState : MultiDragPointerState {
        public _HorizontalPointerState(Offset initialPosition) : base(initialPosition) {
        }

        public override void checkForResolutionAfterMove() {
            D.assert(pendingDelta != null);
            if (pendingDelta.dx.abs() > Constants.kTouchSlop) {
                resolve(GestureDisposition.accepted);
            }
        }

        public override void accepted(GestureMultiDragStartCallback starter) {
            starter(initialPosition);
        }
    }

    public class HorizontalMultiDragGestureRecognizer : MultiDragGestureRecognizer<_HorizontalPointerState> {
        public HorizontalMultiDragGestureRecognizer(object debugOwner, PointerDeviceKind? kind = null) : base(
            debugOwner: debugOwner, kind: kind) {
        }

        public override _HorizontalPointerState createNewPointerState(PointerDownEvent pEvent) {
            return new _HorizontalPointerState(pEvent.position);
        }

        public override string debugDescription {
            get { return "horizontal multidrag"; }
        }
    }


    public class _VerticalPointerState : MultiDragPointerState {
        public _VerticalPointerState(Offset initialPosition) : base(initialPosition) {
        }

        public override void checkForResolutionAfterMove() {
            D.assert(pendingDelta != null);
            if (pendingDelta.dy.abs() > Constants.kTouchSlop) {
                resolve(GestureDisposition.accepted);
            }
        }

        public override void accepted(GestureMultiDragStartCallback starter) {
            starter(initialPosition);
        }
    }


    public class VerticalMultiDragGestureRecognizer : MultiDragGestureRecognizer<_VerticalPointerState> {
        public VerticalMultiDragGestureRecognizer(object debugOwner, PointerDeviceKind? kind = null) : base(
            debugOwner: debugOwner, kind: kind) {
        }

        public override _VerticalPointerState createNewPointerState(PointerDownEvent pEvent) {
            return new _VerticalPointerState(pEvent.position);
        }

        public override string debugDescription {
            get { return "vertical multidrag"; }
        }
    }

    public class _DelayedPointerState : MultiDragPointerState {
        public _DelayedPointerState(
            Offset initialPosition = null,
            TimeSpan? delay = null)
            : base(initialPosition) {
            D.assert(delay != null);
            _timer = Timer.create(delay ?? Constants.kLongPressTimeout, _delayPassed);
        }

        Timer _timer;
        GestureMultiDragStartCallback _starter;

        void _delayPassed() {
            D.assert(_timer != null);
            D.assert(pendingDelta != null);
            D.assert(pendingDelta.distance <= Constants.kTouchSlop);
            _timer = null;
            if (_starter != null) {
                _starter(initialPosition);
                _starter = null;
            }
            else {
                resolve(GestureDisposition.accepted);
            }

            D.assert(_starter == null);
        }

        void _ensureTimerStopped() {
            _timer?.cancel();
            _timer = null;
        }

        public override void accepted(GestureMultiDragStartCallback starter) {
            D.assert(_starter == null);
            if (_timer == null) {
                starter(initialPosition);
            }
            else {
                _starter = starter;
            }
        }

        public override void checkForResolutionAfterMove() {
            if (_timer == null) {
                D.assert(_starter != null);
                return;
            }

            D.assert(pendingDelta != null);
            if (pendingDelta.distance > Constants.kTouchSlop) {
                resolve(GestureDisposition.rejected);
                _ensureTimerStopped();
            }
        }

        public override void dispose() {
            _ensureTimerStopped();
            base.dispose();
        }
    }


    public class DelayedMultiDragGestureRecognizer : MultiDragGestureRecognizer<_DelayedPointerState> {
        public DelayedMultiDragGestureRecognizer(
            TimeSpan? delay = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null) : base(debugOwner: debugOwner, kind: kind) {
            if (delay == null) {
                delay = Constants.kLongPressTimeout;
            }

            this.delay = delay;
        }

        readonly TimeSpan? delay;

        public override _DelayedPointerState createNewPointerState(PointerDownEvent pEvent) {
            return new _DelayedPointerState(pEvent.position, delay);
        }

        public override string debugDescription {
            get { return "long multidrag"; }
        }
    }
}