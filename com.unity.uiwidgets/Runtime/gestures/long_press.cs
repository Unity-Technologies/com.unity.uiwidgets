using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void GestureLongPressCallback();

    public delegate void GestureLongPressUpCallback();

    public delegate void GestureLongPressStartCallback(LongPressStartDetails details);

    public delegate void GestureLongPressMoveUpdateCallback(LongPressMoveUpdateDetails details);

    public delegate void GestureLongPressEndCallback(LongPressEndDetails details);

    public class LongPressStartDetails {
        public LongPressStartDetails(
            Offset globalPosition = null,
            Offset localPosition = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
        }

        public readonly Offset globalPosition;
        public readonly Offset localPosition;
    }

    public class LongPressMoveUpdateDetails {
        public LongPressMoveUpdateDetails(
            Offset globalPosition = null,
            Offset localPosition = null,
            Offset offsetFromOrigin = null,
            Offset localOffsetFromOrigin = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
            this.offsetFromOrigin = offsetFromOrigin ?? Offset.zero;
            this.localOffsetFromOrigin = localOffsetFromOrigin ?? this.offsetFromOrigin;
        }

        public readonly Offset globalPosition;
        public readonly Offset localPosition;

        public readonly Offset offsetFromOrigin;
        public readonly Offset localOffsetFromOrigin;
    }

    public class LongPressEndDetails {
        public LongPressEndDetails(
            Offset globalPosition = null,
            Offset localPosition = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
            velocity = Velocity.zero;
        }

        public readonly Offset globalPosition;
        public readonly Offset localPosition;
        public readonly Velocity velocity;
    }


    public class LongPressGestureRecognizer : PrimaryPointerGestureRecognizer {
        public LongPressGestureRecognizer(
            TimeSpan? duration = null,
            float? postAcceptSlopTolerance = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null) : base(
            deadline: duration ?? Constants.kLongPressTimeout,
            postAcceptSlopTolerance: postAcceptSlopTolerance,
            kind: kind,
            debugOwner: debugOwner) { }

        bool _longPressAccepted = false;

        OffsetPair _longPressOrigin;
        
        int _initialButtons;

        public GestureLongPressCallback onLongPress;

        public GestureLongPressStartCallback onLongPressStart;

        public GestureLongPressMoveUpdateCallback onLongPressMoveUpdate;

        public GestureLongPressUpCallback onLongPressUp;

        public GestureLongPressEndCallback onLongPressEnd;

        VelocityTracker _velocityTracker;
        
        protected override void didExceedDeadline() {
            resolve(GestureDisposition.accepted);
            _longPressAccepted = true;
            base.acceptGesture(primaryPointer);
            if (onLongPress != null) {
                invokeCallback<object>("onLongPress", () => {
                    onLongPress();
                    return null;
                });
            }

            if (onLongPressStart != null) {
                invokeCallback<object>("onLongPressStart",
                    () => {
                        onLongPressStart(new LongPressStartDetails(
                            globalPosition: _longPressOrigin.global,
                            localPosition:_longPressOrigin.local
                        ));
                        return null;
                    });
            }
        }

        protected override void handlePrimaryPointer(PointerEvent evt) {
            if (evt is PointerUpEvent) {
                if (_longPressAccepted) {
                    if (onLongPressUp != null) {
                        invokeCallback<object>("onLongPressUp", () => {
                            onLongPressUp();
                            return null;
                        });
                    }

                    if (onLongPressEnd != null) {
                        invokeCallback<object>("onLongPressEnd", () => {
                            onLongPressEnd(new LongPressEndDetails(
                                globalPosition: evt.position,
                                localPosition: evt.localPosition));
                            return null;
                        });
                    }

                    _longPressAccepted = true;
                }
                else {
                    resolve(GestureDisposition.rejected);
                }
            }
            else if (evt is PointerDownEvent || evt is PointerCancelEvent) {
                _longPressAccepted = false;
                _longPressOrigin =  OffsetPair.fromEventPosition(evt);
            }
            else if (evt is PointerMoveEvent && _longPressAccepted && onLongPressMoveUpdate != null) {
                invokeCallback<object>("onLongPressMoveUpdate", () => {
                    onLongPressMoveUpdate(new LongPressMoveUpdateDetails(
                        globalPosition: evt.position,
                        localPosition: evt.localPosition,
                        offsetFromOrigin: evt.position - _longPressOrigin.global,
                        localOffsetFromOrigin: evt.localPosition - _longPressOrigin.local
                    ));
                    return null;
                });
            }
        }

        public override void acceptGesture(int pointer) {
        }

        public override string debugDescription {
            get { return "long press"; }
        }
    }
}