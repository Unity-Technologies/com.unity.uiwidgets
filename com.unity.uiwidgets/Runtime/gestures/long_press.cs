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
            this.velocity = Velocity.zero;
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
            this.resolve(GestureDisposition.accepted);
            this._longPressAccepted = true;
            base.acceptGesture(this.primaryPointer);
            if (this.onLongPress != null) {
                this.invokeCallback<object>("onLongPress", () => {
                    this.onLongPress();
                    return null;
                });
            }

            if (this.onLongPressStart != null) {
                this.invokeCallback<object>("onLongPressStart",
                    () => {
                        this.onLongPressStart(new LongPressStartDetails(
                            globalPosition: this._longPressOrigin.global,
                            localPosition:this._longPressOrigin.local
                        ));
                        return null;
                    });
            }
        }

        protected override void handlePrimaryPointer(PointerEvent evt) {
            if (evt is PointerUpEvent) {
                if (this._longPressAccepted) {
                    if (this.onLongPressUp != null) {
                        this.invokeCallback<object>("onLongPressUp", () => {
                            this.onLongPressUp();
                            return null;
                        });
                    }

                    if (this.onLongPressEnd != null) {
                        this.invokeCallback<object>("onLongPressEnd", () => {
                            this.onLongPressEnd(new LongPressEndDetails(
                                globalPosition: evt.position,
                                localPosition: evt.localPosition));
                            return null;
                        });
                    }

                    this._longPressAccepted = true;
                }
                else {
                    this.resolve(GestureDisposition.rejected);
                }
            }
            else if (evt is PointerDownEvent || evt is PointerCancelEvent) {
                this._longPressAccepted = false;
                this._longPressOrigin =  OffsetPair.fromEventPosition(evt);
            }
            else if (evt is PointerMoveEvent && this._longPressAccepted && this.onLongPressMoveUpdate != null) {
                this.invokeCallback<object>("onLongPressMoveUpdate", () => {
                    this.onLongPressMoveUpdate(new LongPressMoveUpdateDetails(
                        globalPosition: evt.position,
                        localPosition: evt.localPosition,
                        offsetFromOrigin: evt.position - this._longPressOrigin.global,
                        localOffsetFromOrigin: evt.localPosition - this._longPressOrigin.local
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