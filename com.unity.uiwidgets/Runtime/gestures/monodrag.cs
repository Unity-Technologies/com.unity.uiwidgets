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
            this.startTrackingScrollerPointer(evt.pointer);
            if (this._state == _DragState.ready) {
                this._state = _DragState.possible;
                this._initialPosition = new OffsetPair(global: evt.position, local: evt.localPosition);
                if (this.onStart != null) {
                    this.invokeCallback<object>("onStart", () => {
                        this.onStart(new DragStartDetails(
                            sourceTimeStamp: evt.timeStamp,
                            globalPosition: this._initialPosition.global,
                            localPosition: this._initialPosition.local
                        ));
                        return null;
                    });
                }
            }
        }

        public override void addAllowedPointer(PointerDownEvent evt) {
            this.startTrackingPointer(evt.pointer, evt.transform);
            this._velocityTrackers[evt.pointer] = new VelocityTracker();
            if (this._state == _DragState.ready) {
                this._state = _DragState.possible;
                this._initialPosition = new OffsetPair(global: evt.position, local: evt.localPosition);
                this._pendingDragOffset = OffsetPair.zero;
                this._globalDistanceMoved = 0f;
                this._lastPendingEventTimestamp = evt.timeStamp;
                this._lastTransform = evt.transform;
                if (this.onDown != null) {
                    this.invokeCallback<object>("onDown",
                        () => {
                            this.onDown(new DragDownDetails(
                                globalPosition: this._initialPosition.global,
                                localPosition: this._initialPosition.local));
                            return null;
                        });
                }
            }
            else if (this._state == _DragState.accepted) {
                this.resolve(GestureDisposition.accepted);
            }
        }

        protected override void handleEvent(PointerEvent evt) {
            D.assert(this._state != _DragState.ready);
            if (evt is PointerScrollEvent) {
                var scrollEvt = (PointerScrollEvent) evt;
                Offset delta = scrollEvt.scrollDelta;
                if (this.onUpdate != null) {
                    this.invokeCallback<object>("onUpdate", () => {
                        this.onUpdate(new DragUpdateDetails(
                            sourceTimeStamp: evt.timeStamp,
                            delta: this._getDeltaForDetails(delta),
                            primaryDelta: this._getPrimaryValueFromOffset(delta),
                            globalPosition: evt.position,
                            localPosition: evt.localPosition,
                            isScroll: true
                        ));
                        return null;
                    });
                }

                this.stopTrackingScrollerPointer(evt.pointer);
                return;
            }

            if (!evt.synthesized
                && (evt is PointerDownEvent || evt is PointerMoveEvent)) {
                var tracker = this._velocityTrackers[evt.pointer];
                D.assert(tracker != null);
                tracker.addPosition(evt.timeStamp, evt.localPosition);
            }

            if (evt is PointerMoveEvent) {
                if (this._state == _DragState.accepted) {
                    if (this.onUpdate != null) {
                        this.invokeCallback<object>("onUpdate", () => {
                            this.onUpdate(new DragUpdateDetails(
                                sourceTimeStamp: evt.timeStamp,
                                delta: this._getDeltaForDetails(evt.localDelta),
                                primaryDelta: this._getPrimaryValueFromOffset(evt.localDelta),
                                globalPosition: evt.position,
                                localPosition: evt.localPosition
                            ));
                            return null;
                        });
                    }
                }
                else {
                    this._pendingDragOffset += new OffsetPair(local: evt.localDelta, global: evt.delta);
                    this._lastPendingEventTimestamp = evt.timeStamp;
                    this._lastTransform = evt.transform;
                    Offset movedLocally = this._getDeltaForDetails(evt.localDelta);
                    Matrix4 localToGlobalTransform = evt.transform == null ? null : Matrix4.tryInvert(evt.transform);
                    this._globalDistanceMoved += PointerEvent.transformDeltaViaPositions(
                        transform: localToGlobalTransform,
                        untransformedDelta: movedLocally,
                        untransformedEndPosition: evt.localPosition
                        ).distance * (this._getPrimaryValueFromOffset(movedLocally) ?? 1).sign();
                    if (this._hasSufficientGlobalDistanceToAccept) {
                        this.resolve(GestureDisposition.accepted);
                    }
                }
            }

            this.stopTrackingIfPointerNoLongerDown(evt);
        }

        public override void acceptGesture(int pointer) {
            if (this._state != _DragState.accepted) {
                this._state = _DragState.accepted;
                OffsetPair delta = this._pendingDragOffset;
                var timestamp = this._lastPendingEventTimestamp;
                Matrix4 transform = this._lastTransform;

                Offset localUpdateDelta = null;
                switch (this.dragStartBehavior) {
                    case DragStartBehavior.start:
                        this._initialPosition = this._initialPosition + delta;
                        localUpdateDelta = Offset.zero;
                        break;
                    case DragStartBehavior.down:
                        localUpdateDelta = this._getDeltaForDetails(delta.local);
                        break;
                }

                D.assert(localUpdateDelta != null);

                this._pendingDragOffset = OffsetPair.zero;
                this._lastPendingEventTimestamp = default(TimeSpan);
                this._lastTransform = null;
                if (this.onStart != null) {
                    this.invokeCallback<object>("onStart", () => {
                        this.onStart(new DragStartDetails(
                            sourceTimeStamp: timestamp,
                            globalPosition: this._initialPosition.global,
                            localPosition: this._initialPosition.local
                        ));
                        return null;
                    });
                }

                if (localUpdateDelta != Offset.zero && this.onUpdate != null) {
                    Matrix4 localToGlobal = transform != null ? Matrix4.tryInvert(transform) : null;
                    Offset correctedLocalPosition = this._initialPosition.local + localUpdateDelta;
                    Offset globalUpdateDelta = PointerEvent.transformDeltaViaPositions(
                        untransformedEndPosition: correctedLocalPosition,
                        untransformedDelta: localUpdateDelta,
                        transform: localToGlobal
                    );
                    OffsetPair updateDelta = new OffsetPair(local: localUpdateDelta, global: globalUpdateDelta);
                    OffsetPair correctedPosition = this._initialPosition + updateDelta; // Only adds delta for down behaviour
                    this.invokeCallback<object>("onUpdate", () => {
                        this.onUpdate(new DragUpdateDetails(
                            sourceTimeStamp: timestamp,
                            delta: localUpdateDelta,
                            primaryDelta: this._getPrimaryValueFromOffset(localUpdateDelta),
                            globalPosition: this._initialPosition.global,
                            localPosition: this._initialPosition.local
                        ));
                        return null;
                    });
                }
            }
        }

        public override void rejectGesture(int pointer) {
            this.stopTrackingPointer(pointer);
        }

        protected override void didStopTrackingLastScrollerPointer(int pointer) {
            this._state = _DragState.ready;
            this.invokeCallback<object>("onEnd", () => {
                    this.onEnd(new DragEndDetails(
                        velocity: Velocity.zero,
                        primaryVelocity: 0.0f
                    ));
                    return null;
                }, debugReport: () => { return "Pointer scroll end"; }
            );
        }

        protected override void didStopTrackingLastPointer(int pointer) {
            if (this._state == _DragState.possible) {
                this.resolve(GestureDisposition.rejected);
                this._state = _DragState.ready;
                if (this.onCancel != null) {
                    this.invokeCallback<object>("onCancel", () => {
                        this.onCancel();
                        return null;
                    });
                }

                return;
            }

            bool wasAccepted = this._state == _DragState.accepted;
            this._state = _DragState.ready;
            if (wasAccepted && this.onEnd != null) {
                var tracker = this._velocityTrackers[pointer];
                D.assert(tracker != null);

                var estimate = tracker.getVelocityEstimate();
                if (estimate != null && this._isFlingGesture(estimate)) {
                    Velocity velocity = new Velocity(pixelsPerSecond: estimate.pixelsPerSecond)
                        .clampMagnitude(this.minFlingVelocity ?? Constants.kMinFlingVelocity,
                            this.maxFlingVelocity ?? Constants.kMaxFlingVelocity);
                    this.invokeCallback<object>("onEnd", () => {
                        this.onEnd(new DragEndDetails(
                            velocity: velocity,
                            primaryVelocity: this._getPrimaryValueFromOffset(velocity.pixelsPerSecond)
                        ));
                        return null;
                    }, debugReport: () =>
                        $"{estimate}; fling at {velocity}.");
                }
                else {
                    this.invokeCallback<object>("onEnd", () => {
                            this.onEnd(new DragEndDetails(
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

            this._velocityTrackers.Clear();
        }

        public override void dispose() {
            this._velocityTrackers.Clear();
            base.dispose();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<DragStartBehavior>("start behavior", this.dragStartBehavior));
        }
    }

    public class VerticalDragGestureRecognizer : DragGestureRecognizer {
        public VerticalDragGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null)
            : base(debugOwner: debugOwner, kind: kind) {
        }

        protected override bool _isFlingGesture(VelocityEstimate estimate) {
            float minVelocity = this.minFlingVelocity ?? Constants.kMinFlingVelocity;
            float minDistance = this.minFlingDistance ?? Constants.kTouchSlop;
            return Mathf.Abs(estimate.pixelsPerSecond.dy) > minVelocity && Mathf.Abs(estimate.offset.dy) > minDistance;
        }

        protected override bool _hasSufficientGlobalDistanceToAccept {
            get { return Mathf.Abs(this._globalDistanceMoved) > Constants.kTouchSlop; }
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
            float minVelocity = this.minFlingVelocity ?? Constants.kMinFlingVelocity;
            float minDistance = this.minFlingDistance ?? Constants.kTouchSlop;
            return Mathf.Abs(estimate.pixelsPerSecond.dx) > minVelocity && Mathf.Abs(estimate.offset.dx) > minDistance;
        }

        protected override bool _hasSufficientGlobalDistanceToAccept {
            get { return Mathf.Abs(this._globalDistanceMoved) > Constants.kTouchSlop; }
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
            float minVelocity = this.minFlingVelocity ?? Constants.kMinFlingVelocity;
            float minDistance = this.minFlingDistance ?? Constants.kTouchSlop;
            return estimate.pixelsPerSecond.distanceSquared > minVelocity * minVelocity
                   && estimate.offset.distanceSquared > minDistance * minDistance;
        }

        protected override bool _hasSufficientGlobalDistanceToAccept {
            get { return Math.Abs(this._globalDistanceMoved) > Constants.kPanSlop; }
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