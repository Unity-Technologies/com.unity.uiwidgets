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
        OffsetPair _initialPosition;
        protected OffsetPair _pendingDragOffset;
        TimeSpan _lastPendingEventTimestamp;
        Matrix4 _lastTransform;
        protected float _globalDistanceMoved;

        protected abstract bool _isFlingGesture(VelocityEstimate estimate);
        protected abstract Offset _getDeltaForDetails(Offset delta);
        protected abstract float? _getPrimaryValueFromOffset(Offset value);
        protected abstract bool _hasSufficientGlobalDistanceToAccept { get; }

        readonly Dictionary<int, VelocityTracker> _velocityTrackers = new Dictionary<int, VelocityTracker>();

        public override void addScrollPointer(PointerScrollEvent evt) {
            startTrackingScrollerPointer(evt.pointer);
            if (_state == _DragState.ready) {
                _state = _DragState.possible;
                _initialPosition = new OffsetPair(global: evt.position, local: evt.localPosition);
                if (onStart != null) {
                    invokeCallback<object>("onStart", () => {
                        onStart(new DragStartDetails(
                            sourceTimeStamp: evt.timeStamp,
                            globalPosition: _initialPosition.global,
                            localPosition: _initialPosition.local
                        ));
                        return null;
                    });
                }
            }
        }

        public override void addAllowedPointer(PointerEvent evt) {
            startTrackingPointer(evt.pointer, evt.transform);
            _velocityTrackers[evt.pointer] = new VelocityTracker();
            if (_state == _DragState.ready) {
                _state = _DragState.possible;
                _initialPosition = new OffsetPair(global: evt.position, local: evt.localPosition);
                _pendingDragOffset = OffsetPair.zero;
                _globalDistanceMoved = 0f;
                _lastPendingEventTimestamp = evt.timeStamp;
                _lastTransform = evt.transform;
                if (onDown != null) {
                    invokeCallback<object>("onDown",
                        () => {
                            onDown(new DragDownDetails(
                                globalPosition: _initialPosition.global,
                                localPosition: _initialPosition.local));
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
                            localPosition: evt.localPosition,
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
                tracker.addPosition(evt.timeStamp, evt.localPosition);
            }

            if (evt is PointerMoveEvent) {
                if (_state == _DragState.accepted) {
                    if (onUpdate != null) {
                        invokeCallback<object>("onUpdate", () => {
                            onUpdate(new DragUpdateDetails(
                                sourceTimeStamp: evt.timeStamp,
                                delta: _getDeltaForDetails(evt.localDelta),
                                primaryDelta: _getPrimaryValueFromOffset(evt.localDelta),
                                globalPosition: evt.position,
                                localPosition: evt.localPosition
                            ));
                            return null;
                        });
                    }
                }
                else {
                    _pendingDragOffset += new OffsetPair(local: evt.localDelta, global: evt.delta);
                    _lastPendingEventTimestamp = evt.timeStamp;
                    _lastTransform = evt.transform;
                    Offset movedLocally = _getDeltaForDetails(evt.localDelta);
                    Matrix4 localToGlobalTransform = evt.transform == null ? null : Matrix4.tryInvert(evt.transform);
                    _globalDistanceMoved += PointerEvent.transformDeltaViaPositions(
                        transform: localToGlobalTransform,
                        untransformedDelta: movedLocally,
                        untransformedEndPosition: evt.localPosition
                        ).distance * (_getPrimaryValueFromOffset(movedLocally) ?? 1).sign();
                    if (_hasSufficientGlobalDistanceToAccept) {
                        resolve(GestureDisposition.accepted);
                    }
                }
            }

            stopTrackingIfPointerNoLongerDown(evt);
        }

        public override void acceptGesture(int pointer) {
            if (_state != _DragState.accepted) {
                _state = _DragState.accepted;
                OffsetPair delta = _pendingDragOffset;
                var timestamp = _lastPendingEventTimestamp;
                Matrix4 transform = _lastTransform;

                Offset localUpdateDelta = null;
                switch (dragStartBehavior) {
                    case DragStartBehavior.start:
                        _initialPosition = _initialPosition + delta;
                        localUpdateDelta = Offset.zero;
                        break;
                    case DragStartBehavior.down:
                        localUpdateDelta = _getDeltaForDetails(delta.local);
                        break;
                }

                D.assert(localUpdateDelta != null);

                _pendingDragOffset = OffsetPair.zero;
                _lastPendingEventTimestamp = default(TimeSpan);
                _lastTransform = null;
                if (onStart != null) {
                    invokeCallback<object>("onStart", () => {
                        onStart(new DragStartDetails(
                            sourceTimeStamp: timestamp,
                            globalPosition: _initialPosition.global,
                            localPosition: _initialPosition.local
                        ));
                        return null;
                    });
                }

                if (localUpdateDelta != Offset.zero && onUpdate != null) {
                    Matrix4 localToGlobal = transform != null ? Matrix4.tryInvert(transform) : null;
                    Offset correctedLocalPosition = _initialPosition.local + localUpdateDelta;
                    Offset globalUpdateDelta = PointerEvent.transformDeltaViaPositions(
                        untransformedEndPosition: correctedLocalPosition,
                        untransformedDelta: localUpdateDelta,
                        transform: localToGlobal
                    );
                    OffsetPair updateDelta = new OffsetPair(local: localUpdateDelta, global: globalUpdateDelta);
                    OffsetPair correctedPosition = _initialPosition + updateDelta; // Only adds delta for down behaviour
                    invokeCallback<object>("onUpdate", () => {
                        onUpdate(new DragUpdateDetails(
                            sourceTimeStamp: timestamp,
                            delta: localUpdateDelta,
                            primaryDelta: _getPrimaryValueFromOffset(localUpdateDelta),
                            globalPosition: _initialPosition.global,
                            localPosition: _initialPosition.local
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

        protected override bool _hasSufficientGlobalDistanceToAccept {
            get { return Mathf.Abs(_globalDistanceMoved) > Constants.kTouchSlop; }
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

        protected override bool _hasSufficientGlobalDistanceToAccept {
            get { return Mathf.Abs(_globalDistanceMoved) > Constants.kTouchSlop; }
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

        protected override bool _hasSufficientGlobalDistanceToAccept {
            get { return Math.Abs(_globalDistanceMoved) > Constants.kPanSlop; }
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