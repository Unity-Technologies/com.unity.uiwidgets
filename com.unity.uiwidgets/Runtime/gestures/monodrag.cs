using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    enum _DragState {
        ready,
        possible,
        accepted,
    }

    public delegate void GestureDragEndCallback(DragEndDetails details);

    public delegate void GestureDragCancelCallback();

    public abstract class DragGestureRecognizer : OneSequenceGestureRecognizer {
        public DragGestureRecognizer(
            object debugOwner = null,
            PointerDeviceKind? kind = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.down)
            : base(debugOwner: debugOwner, kind: kind) {
            this.dragStartBehavior = dragStartBehavior;
        }

        public DragStartBehavior dragStartBehavior;

        public GestureDragDownCallback onDown;

        public GestureDragStartCallback onStart;

        public GestureDragUpdateCallback onUpdate;

        public GestureDragEndCallback onEnd;

        public GestureDragCancelCallback onCancel;

        public float? minFlingDistance;

        public float? minFlingVelocity;

        public float? maxFlingVelocity;

        _DragState _state = _DragState.ready;
        Offset _initialPosition;
        protected Offset _pendingDragOffset;
        TimeSpan _lastPendingEventTimestamp;

        protected abstract bool _isFlingGesture(VelocityEstimate estimate);
        protected abstract Offset _getDeltaForDetails(Offset delta);
        protected abstract float? _getPrimaryValueFromOffset(Offset value);
        protected abstract bool _hasSufficientPendingDragDeltaToAccept { get; }

        readonly Dictionary<int, VelocityTracker> _velocityTrackers = new Dictionary<int, VelocityTracker>();

        public override void addScrollPointer(PointerScrollEvent evt) {
            startTrackingScrollerPointer(evt.pointer);
            if (_state == _DragState.ready) {
                _state = _DragState.possible;
                _initialPosition = evt.position;
                if (onStart != null) {
                    invokeCallback<object>("onStart", () => {
                        onStart(new DragStartDetails(
                            sourceTimeStamp: evt.timeStamp,
                            globalPosition: _initialPosition
                        ));
                        return null;
                    });
                }
            }
        }

        public override void addAllowedPointer(PointerDownEvent evt) {
            startTrackingPointer(evt.pointer);
            _velocityTrackers[evt.pointer] = new VelocityTracker();
            if (_state == _DragState.ready) {
                _state = _DragState.possible;
                _initialPosition = evt.position;
                _pendingDragOffset = Offset.zero;
                _lastPendingEventTimestamp = evt.timeStamp;
                if (onDown != null) {
                    invokeCallback<object>("onDown",
                        () => {
                            onDown(new DragDownDetails(globalPosition: _initialPosition));
                            return null;
                        });
                }
            }
            else if (_state == _DragState.accepted) {
                resolve(GestureDisposition.accepted);
            }
        }

        protected override void handleEvent(PointerEvent evt) {
            D.assert(_state != _DragState.ready);
            if (evt is PointerScrollEvent) {
                var scrollEvt = (PointerScrollEvent) evt;
                Offset delta = scrollEvt.scrollDelta;
                if (onUpdate != null) {
                    invokeCallback<object>("onUpdate", () => {
                        onUpdate(new DragUpdateDetails(
                            sourceTimeStamp: evt.timeStamp,
                            delta: _getDeltaForDetails(delta),
                            primaryDelta: _getPrimaryValueFromOffset(delta),
                            globalPosition: evt.position,
                            isScroll: true
                        ));
                        return null;
                    });
                }

                stopTrackingScrollerPointer(evt.pointer);
                return;
            }

            if (!evt.synthesized
                && (evt is PointerDownEvent || evt is PointerMoveEvent)) {
                var tracker = _velocityTrackers[evt.pointer];
                D.assert(tracker != null);
                tracker.addPosition(evt.timeStamp, evt.position);
            }

            if (evt is PointerMoveEvent) {
                Offset delta = evt.delta;
                if (_state == _DragState.accepted) {
                    if (onUpdate != null) {
                        invokeCallback<object>("onUpdate", () => {
                            onUpdate(new DragUpdateDetails(
                                sourceTimeStamp: evt.timeStamp,
                                delta: _getDeltaForDetails(delta),
                                primaryDelta: _getPrimaryValueFromOffset(delta),
                                globalPosition: evt.position
                            ));
                            return null;
                        });
                    }
                }
                else {
                    _pendingDragOffset += delta;
                    _lastPendingEventTimestamp = evt.timeStamp;
                    if (_hasSufficientPendingDragDeltaToAccept) {
                        resolve(GestureDisposition.accepted);
                    }
                }
            }

            stopTrackingIfPointerNoLongerDown(evt);
        }

        public override void acceptGesture(int pointer) {
            if (_state != _DragState.accepted) {
                _state = _DragState.accepted;
                Offset delta = _pendingDragOffset;
                var timestamp = _lastPendingEventTimestamp;

                Offset updateDelta = null;
                switch (dragStartBehavior) {
                    case DragStartBehavior.start:
                        _initialPosition = _initialPosition + delta;
                        updateDelta = Offset.zero;
                        break;
                    case DragStartBehavior.down:
                        updateDelta = _getDeltaForDetails(delta);
                        break;
                }

                D.assert(updateDelta != null);

                _pendingDragOffset = Offset.zero;
                _lastPendingEventTimestamp = default(TimeSpan);
                if (onStart != null) {
                    invokeCallback<object>("onStart", () => {
                        onStart(new DragStartDetails(
                            sourceTimeStamp: timestamp,
                            globalPosition: _initialPosition
                        ));
                        return null;
                    });
                }

                if (updateDelta != Offset.zero && onUpdate != null) {
                    invokeCallback<object>("onUpdate", () => {
                        onUpdate(new DragUpdateDetails(
                            sourceTimeStamp: timestamp,
                            delta: updateDelta,
                            primaryDelta: _getPrimaryValueFromOffset(updateDelta),
                            globalPosition: _initialPosition + updateDelta
                        ));
                        return null;
                    });
                }
            }
        }

        public override void rejectGesture(int pointer) {
            stopTrackingPointer(pointer);
        }

        protected override void didStopTrackingLastScrollerPointer(int pointer) {
            _state = _DragState.ready;
            invokeCallback<object>("onEnd", () => {
                    onEnd(new DragEndDetails(
                        velocity: Velocity.zero,
                        primaryVelocity: 0.0f
                    ));
                    return null;
                }, debugReport: () => { return "Pointer scroll end"; }
            );
        }

        protected override void didStopTrackingLastPointer(int pointer) {
            if (_state == _DragState.possible) {
                resolve(GestureDisposition.rejected);
                _state = _DragState.ready;
                if (onCancel != null) {
                    invokeCallback<object>("onCancel", () => {
                        onCancel();
                        return null;
                    });
                }

                return;
            }

            bool wasAccepted = _state == _DragState.accepted;
            _state = _DragState.ready;
            if (wasAccepted && onEnd != null) {
                var tracker = _velocityTrackers[pointer];
                D.assert(tracker != null);

                var estimate = tracker.getVelocityEstimate();
                if (estimate != null && _isFlingGesture(estimate)) {
                    Velocity velocity = new Velocity(pixelsPerSecond: estimate.pixelsPerSecond)
                        .clampMagnitude(minFlingVelocity ?? Constants.kMinFlingVelocity,
                            maxFlingVelocity ?? Constants.kMaxFlingVelocity);
                    invokeCallback<object>("onEnd", () => {
                        onEnd(new DragEndDetails(
                            velocity: velocity,
                            primaryVelocity: _getPrimaryValueFromOffset(velocity.pixelsPerSecond)
                        ));
                        return null;
                    }, debugReport: () =>
                        $"{estimate}; fling at {velocity}.");
                }
                else {
                    invokeCallback<object>("onEnd", () => {
                            onEnd(new DragEndDetails(
                                velocity: Velocity.zero,
                                primaryVelocity: 0.0f
                            ));
                            return null;
                        }, debugReport: () =>
                            estimate == null
                                ? "Could not estimate velocity."
                                : estimate + "; judged to not be a fling."
                    );
                }
            }

            _velocityTrackers.Clear();
        }

        public override void dispose() {
            _velocityTrackers.Clear();
            base.dispose();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<DragStartBehavior>("start behavior", dragStartBehavior));
        }
    }

    public class VerticalDragGestureRecognizer : DragGestureRecognizer {
        public VerticalDragGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null)
            : base(debugOwner: debugOwner, kind: kind) {
        }

        protected override bool _isFlingGesture(VelocityEstimate estimate) {
            float minVelocity = minFlingVelocity ?? Constants.kMinFlingVelocity;
            float minDistance = minFlingDistance ?? Constants.kTouchSlop;
            return Mathf.Abs(estimate.pixelsPerSecond.dy) > minVelocity && Mathf.Abs(estimate.offset.dy) > minDistance;
        }

        protected override bool _hasSufficientPendingDragDeltaToAccept {
            get { return Mathf.Abs(_pendingDragOffset.dy) > Constants.kTouchSlop; }
        }

        protected override Offset _getDeltaForDetails(Offset delta) {
            return new Offset(0.0f, delta.dy);
        }

        protected override float? _getPrimaryValueFromOffset(Offset value) {
            return value.dy;
        }

        public override string debugDescription {
            get { return "vertical drag"; }
        }
    }

    public class HorizontalDragGestureRecognizer : DragGestureRecognizer {
        public HorizontalDragGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null)
            : base(debugOwner: debugOwner, kind: kind) {
        }

        protected override bool _isFlingGesture(VelocityEstimate estimate) {
            float minVelocity = minFlingVelocity ?? Constants.kMinFlingVelocity;
            float minDistance = minFlingDistance ?? Constants.kTouchSlop;
            return Mathf.Abs(estimate.pixelsPerSecond.dx) > minVelocity && Mathf.Abs(estimate.offset.dx) > minDistance;
        }

        protected override bool _hasSufficientPendingDragDeltaToAccept {
            get { return Mathf.Abs(_pendingDragOffset.dx) > Constants.kTouchSlop; }
        }

        protected override Offset _getDeltaForDetails(Offset delta) {
            return new Offset(delta.dx, 0.0f);
        }

        protected override float? _getPrimaryValueFromOffset(Offset value) {
            return value.dx;
        }

        public override string debugDescription {
            get { return "horizontal drag"; }
        }
    }

    public class PanGestureRecognizer : DragGestureRecognizer {
        public PanGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null)
            : base(debugOwner: debugOwner, kind: kind) {
        }

        protected override bool _isFlingGesture(VelocityEstimate estimate) {
            float minVelocity = minFlingVelocity ?? Constants.kMinFlingVelocity;
            float minDistance = minFlingDistance ?? Constants.kTouchSlop;
            return estimate.pixelsPerSecond.distanceSquared > minVelocity * minVelocity
                   && estimate.offset.distanceSquared > minDistance * minDistance;
        }

        protected override bool _hasSufficientPendingDragDeltaToAccept {
            get { return _pendingDragOffset.distance > Constants.kPanSlop; }
        }

        protected override Offset _getDeltaForDetails(Offset delta) {
            return delta;
        }

        protected override float? _getPrimaryValueFromOffset(Offset value) {
            return null;
        }

        public override string debugDescription {
            get { return "pan"; }
        }
    }
}