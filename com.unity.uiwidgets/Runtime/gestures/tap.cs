using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public class TapDownDetails {
        public TapDownDetails(Offset globalPosition = null,
            Offset localPosition = null,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
            this.kind = kind;
            this.device = device;
        }

        public readonly Offset globalPosition;

        public readonly Offset localPosition;

        public readonly PointerDeviceKind? kind;

        public readonly int device;
    }

    public delegate void GestureTapDownCallback(TapDownDetails details);

    public class TapUpDetails {
        public TapUpDetails(Offset globalPosition = null,
            Offset localPosition = null,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
            this.kind = kind;
            this.device = device;
        }

        public readonly Offset globalPosition;
        
        public readonly Offset localPosition;

        public readonly PointerDeviceKind kind;

        public readonly int device;
    }

    public delegate void GestureTapUpCallback(TapUpDetails details);

    public delegate void GestureTapCallback();

    public delegate void GestureTapCancelCallback();


    public abstract class BaseTapGestureRecognizer : PrimaryPointerGestureRecognizer {
        public BaseTapGestureRecognizer(object debugOwner = null) 
        : base(deadline: Constants.kPressTimeout, debugOwner: debugOwner) {
            
        }

        bool _sentTapDown = false;
        bool _wonArenaForPrimaryPointer = false;
        OffsetPair _finalPosition;

        PointerDownEvent _down;
        PointerUpEvent _up;

        protected abstract void handleTapDown(PointerDownEvent down);

        protected abstract void handleTapUp(PointerDownEvent down, PointerUpEvent up);

        protected abstract void handleTapCancel(PointerDownEvent down, PointerCancelEvent cancel, string reason);

        public override void addAllowedPointer(PointerEvent evt) {
            var _evt = (PointerDownEvent) evt;
            D.assert(_evt != null);
            if (state == GestureRecognizerState.ready) {
                _down = _evt;
            }
            base.addAllowedPointer(_evt);
        }

        protected override void handlePrimaryPointer(PointerEvent evt) {
            if (evt is PointerUpEvent) {
                _up = (PointerUpEvent) evt;
                _finalPosition = new OffsetPair(global: evt.position, local: evt.localPosition);

                _checkUp();
            } else if (evt is PointerCancelEvent) {
                resolve(GestureDisposition.rejected);
                if (_sentTapDown) {
                    _checkCancel((PointerCancelEvent) evt, "");
                }

                _reset();
            } else if (evt.buttons != _down?.buttons) {
                resolve(GestureDisposition.rejected);
                stopTrackingPointer(primaryPointer);
            }
        }

        protected override void resolve(GestureDisposition disposition) {
            if (_wonArenaForPrimaryPointer && disposition == GestureDisposition.rejected) {
                D.assert(_sentTapDown);
                _checkCancel(null, "spontaneous");
                _reset();
            }
            
            base.resolve(disposition);
        }

        protected override void didExceedDeadline() {
            _checkDown();
        }

        public override void acceptGesture(int pointer) {
            base.acceptGesture(pointer);

            if (pointer == primaryPointer) {
                _checkDown();
                _wonArenaForPrimaryPointer = true;
                _checkUp();
            }
        }

        public override void rejectGesture(int pointer) {
            base.rejectGesture(pointer);
            if (pointer == primaryPointer) {
                D.assert(state != GestureRecognizerState.possible);
                if (_sentTapDown) {
                    _checkCancel(null, "forced");
                }

                _reset();
            }
        }

        void _checkDown() {
            if (_sentTapDown) {
                return;
            }
            
            handleTapDown(down: _down);
            _sentTapDown = true;
        }

        void _checkUp() {
            if (!_wonArenaForPrimaryPointer || _up == null) {
                return;
            }
            handleTapUp(down: _down, up: _up);
            _reset();
        }

        void _checkCancel(PointerCancelEvent evt, string note) {
            handleTapCancel(down: _down, cancel: evt, reason: note);
        }

        void _reset() {
            _sentTapDown = false;
            _wonArenaForPrimaryPointer = false;
            _up = null;
            _down = null;
        }
        
        public override string debugDescription {
            get { return "base tap"; }
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("wonArenaForPrimaryPointer", value: _wonArenaForPrimaryPointer, ifTrue: "won arena"));
            properties.add(new DiagnosticsProperty<Offset>("finalPosition", _up?.position, defaultValue: null));
            properties.add(new DiagnosticsProperty<Offset>("finalLocalPosition", _up?.position, defaultValue: _up?.position));
            properties.add(new DiagnosticsProperty<int>("button", _down?.buttons?? 0, defaultValue: 0));
            properties.add(new FlagProperty("sentTapDown", value: _sentTapDown, ifTrue: "sent tap down"));
        }
    }

    public class TapGestureRecognizer : BaseTapGestureRecognizer {
        public TapGestureRecognizer(object debugOwner = null) : base(debugOwner: debugOwner) {
        }
        
        public GestureTapDownCallback onTapDown;
        
        public GestureTapUpCallback onTapUp;
        
        public GestureTapCallback onTap;
        
        public GestureTapCancelCallback onTapCancel;
        
        public GestureTapDownCallback onSecondaryTapDown;
        
        public GestureTapUpCallback onSecondaryTapUp;
        
        public GestureTapCancelCallback onSecondaryTapCancel;

        protected override bool isPointerAllowed(PointerDownEvent evt) {
            if (onTapDown == null && 
                onTap == null && 
                onTapUp == null && 
                onTapCancel == null) {
                return false;
            }
            
            return base.isPointerAllowed(evt);
        }

        protected override void handleTapDown(PointerDownEvent down) {
            if (onTapDown != null) {
                TapDownDetails details = new TapDownDetails(
                    globalPosition: down.position,
                    localPosition: down.localPosition,
                    kind: down.kind,
                    device: down.device
                );

                    invokeCallback<object>("onTapDown", () => {
                        onTapDown(details);
                        return null;
                    });
            }
        }

        protected override void handleTapUp(PointerDownEvent down, PointerUpEvent up) {
            TapUpDetails details = new TapUpDetails(
                globalPosition: up.position,
                localPosition: up.localPosition,
                kind: up.kind,
                device: up.device
                );

            if (onTapUp != null) {
                invokeCallback<object>("onTapUp", () => {
                    onTapUp(details);
                    return null;
                });
            }

            if (onTap != null) {
                invokeCallback<object>("onTap", () => {
                    onTap();
                    return null;
                });
            }
        }

        protected override void handleTapCancel(PointerDownEvent down, PointerCancelEvent cancel, string note) {
            if (onTapCancel != null) {
                invokeCallback<object>("onTapCancel", () => {
                    onTapCancel();
                    return null;
                });
            }
        }
        
        public override string debugDescription {
            get { return "tap"; }
        }
    }
}