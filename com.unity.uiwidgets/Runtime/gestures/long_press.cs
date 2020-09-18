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
            Offset globalPosition = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
        }

        public readonly Offset globalPosition;
    }

    public class LongPressMoveUpdateDetails {
        public LongPressMoveUpdateDetails(
            Offset globalPosition = null,
            Offset offsetFromOrigin = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.offsetFromOrigin = offsetFromOrigin ?? Offset.zero;
        }

        public readonly Offset globalPosition;

        public readonly Offset offsetFromOrigin;
    }

    public class LongPressEndDetails {
        public LongPressEndDetails(
            Offset globalPosition = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
        }

        public readonly Offset globalPosition;
    }


    public class LongPressGestureRecognizer : PrimaryPointerGestureRecognizer {
        public LongPressGestureRecognizer(
            float? postAcceptSlopTolerance = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null) : base(
            deadline: Constants.kLongPressTimeout,
            postAcceptSlopTolerance: postAcceptSlopTolerance,
            kind: kind,
            debugOwner: debugOwner) { }

        bool _longPressAccepted = false;

        Offset _longPressOrigin;

        public GestureLongPressCallback onLongPress;

        public GestureLongPressStartCallback onLongPressStart;

        public GestureLongPressMoveUpdateCallback onLongPressMoveUpdate;

        public GestureLongPressUpCallback onLongPressUp;

        public GestureLongPressEndCallback onLongPressEnd;

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
                        onLongPressStart(new LongPressStartDetails(globalPosition: _longPressOrigin));
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
                            onLongPressEnd(new LongPressEndDetails(globalPosition: evt.position));
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
                _longPressOrigin = evt.position;
            }
            else if (evt is PointerMoveEvent && _longPressAccepted && onLongPressMoveUpdate != null) {
                invokeCallback<object>("onLongPressMoveUpdate", () => {
                    onLongPressMoveUpdate(new LongPressMoveUpdateDetails(
                        globalPosition: evt.position,
                        offsetFromOrigin: evt.position - _longPressOrigin
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