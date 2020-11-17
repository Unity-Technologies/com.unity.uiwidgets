using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.cupertino {
    class CupertinoSwitchUtils {
        public const float _kTrackWidth = 51.0f;
        public const float _kTrackHeight = 31.0f;
        public const float _kTrackRadius = _kTrackHeight / 2.0f;
        public const float _kTrackInnerStart = _kTrackHeight / 2.0f;
        public const float _kTrackInnerEnd = _kTrackWidth - _kTrackInnerStart;
        public const float _kTrackInnerLength = _kTrackInnerEnd - _kTrackInnerStart;
        public const float _kSwitchWidth = 59.0f;
        public const float _kSwitchHeight = 39.0f;
        public const float _kCupertinoSwitchDisabledOpacity = 0.5f;
        public static readonly Color _kTrackColor = CupertinoColors.lightBackgroundGray;
        public static readonly TimeSpan _kReactionDuration = new TimeSpan(0, 0, 0, 0, 300);
        public static readonly TimeSpan _kToggleDuration = new TimeSpan(0, 0, 0, 0, 200);
    }

    public class CupertinoSwitch : StatefulWidget {
        public CupertinoSwitch(
            bool value,
            ValueChanged<bool> onChanged,
            Key key = null,
            Color activeColor = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            this.value = value;
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly bool value;

        public readonly ValueChanged<bool> onChanged;

        public readonly Color activeColor;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _CupertinoSwitchState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("value", value: value, ifTrue: "on", ifFalse: "off", showName: true));
            properties.add(new ObjectFlagProperty<ValueChanged<bool>>("onChanged", onChanged, ifNull: "disabled"));
        }
    }

    class _CupertinoSwitchState : TickerProviderStateMixin<CupertinoSwitch> {
        public override Widget build(BuildContext context) {
            return new Opacity(
                opacity: widget.onChanged == null ? CupertinoSwitchUtils._kCupertinoSwitchDisabledOpacity : 1.0f,
                child: new _CupertinoSwitchRenderObjectWidget(
                    value: widget.value,
                    activeColor: widget.activeColor ?? CupertinoColors.activeGreen,
                    onChanged: widget.onChanged,
                    vsync: this,
                    dragStartBehavior: widget.dragStartBehavior
                )
            );
        }
    }

    class _CupertinoSwitchRenderObjectWidget : LeafRenderObjectWidget {
        public _CupertinoSwitchRenderObjectWidget(
            Key key = null,
            bool value = false,
            Color activeColor = null,
            ValueChanged<bool> onChanged = null,
            TickerProvider vsync = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            this.value = value;
            this.activeColor = activeColor;
            this.onChanged = onChanged;
            this.vsync = vsync;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly bool value;
        public readonly Color activeColor;
        public readonly ValueChanged<bool> onChanged;
        public readonly TickerProvider vsync;
        public readonly DragStartBehavior dragStartBehavior;
        
        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoSwitch(
                value: value,
                activeColor: activeColor,
                onChanged: onChanged,
                textDirection: Directionality.of(context),
                vsync: vsync,
                dragStartBehavior: dragStartBehavior
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var _renderObject = renderObject as _RenderCupertinoSwitch;
            _renderObject.value = value;
            _renderObject.activeColor = activeColor;
            _renderObject.onChanged = onChanged;
            _renderObject.textDirection = Directionality.of(context);
            _renderObject.vsync = vsync;
            _renderObject.dragStartBehavior = dragStartBehavior;
        }
    }


    class _RenderCupertinoSwitch : RenderConstrainedBox {
        public _RenderCupertinoSwitch(
            bool value,
            Color activeColor,
            TextDirection textDirection,
            TickerProvider vsync,
            ValueChanged<bool> onChanged = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(additionalConstraints: BoxConstraints.tightFor(
            width: CupertinoSwitchUtils._kSwitchWidth,
            height: CupertinoSwitchUtils._kSwitchHeight)
        ) {
            D.assert(activeColor != null);
            D.assert(vsync != null);
            _value = value;
            _activeColor = activeColor;
            _onChanged = onChanged;
            _textDirection = textDirection;
            _vsync = vsync;

            _tap = new TapGestureRecognizer() {
                onTapDown = _handleTapDown,
                onTap = _handleTap,
                onTapUp = _handleTapUp,
                onTapCancel = _handleTapCancel,
            };

            _drag = new HorizontalDragGestureRecognizer() {
                onStart = _handleDragStart,
                onUpdate = _handleDragUpdate,
                onEnd = _handleDragEnd,
                dragStartBehavior = dragStartBehavior
            };

            _positionController = new AnimationController(
                duration: CupertinoSwitchUtils._kToggleDuration,
                value: value ? 1.0f : 0.0f,
                vsync: vsync
            );
            _position = new CurvedAnimation(
                parent: _positionController,
                curve: Curves.linear
            );
            _position.addListener(markNeedsPaint);
            _position.addStatusListener(_handlePositionStateChanged);

            _reactionController = new AnimationController(
                duration: CupertinoSwitchUtils._kReactionDuration,
                vsync: vsync
            );
            _reaction = new CurvedAnimation(
                parent: _reactionController,
                curve: Curves.ease
            );
            _reaction.addListener(markNeedsPaint);
        }

        AnimationController _positionController;
        CurvedAnimation _position;
        AnimationController _reactionController;
        Animation<float> _reaction;

        public bool value {
            get { return _value; }
            set {
                if (value == _value) {
                    return;
                }

                _value = value;
                // this.markNeedsSemanticsUpdate();
                _position.curve = Curves.ease;
                _position.reverseCurve = Curves.ease.flipped;
                if (value) {
                    _positionController.forward();
                }
                else {
                    _positionController.reverse();
                }
            }
        }

        bool _value;

        public TickerProvider vsync {
            get { return _vsync; }
            set {
                D.assert(value != null);
                if (value == _vsync) {
                    return;
                }

                _vsync = value;
                _positionController.resync(vsync);
                _reactionController.resync(vsync);
            }
        }

        TickerProvider _vsync;

        public Color activeColor {
            get { return _activeColor; }
            set {
                D.assert(value != null);
                if (value == _activeColor) {
                    return;
                }

                _activeColor = value;
                markNeedsPaint();
            }
        }

        Color _activeColor;

        public ValueChanged<bool> onChanged {
            get { return _onChanged; }
            set {
                if (value == _onChanged) {
                    return;
                }

                bool wasInteractive = isInteractive;
                _onChanged = value;
                if (wasInteractive != isInteractive) {
                    markNeedsPaint();
                }
            }
        }

        ValueChanged<bool> _onChanged;

        public TextDirection textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value) {
                    return;
                }

                _textDirection = value;
                markNeedsPaint();
            }
        }

        TextDirection _textDirection;


        public DragStartBehavior dragStartBehavior {
            get { return _drag.dragStartBehavior; }
            set {
                if (_drag.dragStartBehavior == value) {
                    return;
                }

                _drag.dragStartBehavior = value;
            }
        }

        public bool isInteractive {
            get { return onChanged != null; }
        }

        TapGestureRecognizer _tap;
        HorizontalDragGestureRecognizer _drag;

        public override void attach(object _owner) {
            base.attach(_owner);
            if (value) {
                _positionController.forward();
            }
            else {
                _positionController.reverse();
            }

            if (isInteractive) {
                switch (_reactionController.status) {
                    case AnimationStatus.forward:
                        _reactionController.forward();
                        break;
                    case AnimationStatus.reverse:
                        _reactionController.reverse();
                        break;
                    case AnimationStatus.dismissed:
                    case AnimationStatus.completed:
                        break;
                }
            }
        }

        public override void detach() {
            _positionController.stop();
            _reactionController.stop();
            base.detach();
        }

        void _handlePositionStateChanged(AnimationStatus status) {
            if (isInteractive) {
                if (status == AnimationStatus.completed && !_value) {
                    onChanged(true);
                }
                else if (status == AnimationStatus.dismissed && _value) {
                    onChanged(false);
                }
            }
        }

        void _handleTapDown(TapDownDetails details) {
            if (isInteractive) {
                _reactionController.forward();
            }
        }

        void _handleTap() {
            if (isInteractive) {
                onChanged(!_value);
                _emitVibration();
            }
        }

        void _handleTapUp(TapUpDetails details) {
            if (isInteractive) {
                _reactionController.reverse();
            }
        }

        void _handleTapCancel() {
            if (isInteractive) {
                _reactionController.reverse();
            }
        }

        void _handleDragStart(DragStartDetails details) {
            if (isInteractive) {
                _reactionController.forward();
                _emitVibration();
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (isInteractive) {
                _position.curve = null;
                _position.reverseCurve = null;
                float delta = details.primaryDelta / CupertinoSwitchUtils._kTrackInnerLength ?? 0f;

                _positionController.setValue(_positionController.value + delta);

                // switch (this.textDirection) {
                //     case TextDirection.rtl:
                //         this._positionController.setValue(this._positionController.value - delta);
                //         break;
                //     case TextDirection.ltr:
                //         this._positionController.setValue(this._positionController.value + delta);
                //         break;
                // }
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            if (_position.value >= 0.5) {
                _positionController.forward();
            }
            else {
                _positionController.reverse();
            }

            _reactionController.reverse();
        }

        void _emitVibration() {
            // switch (Platform defaultTargetPlatform) {
            //     case TargetPlatform.iOS:
            //         HapticFeedback.lightImpact();
            //         break;
            //     case TargetPlatform.fuchsia:
            //     case TargetPlatform.android:
            //         break;
            // }
            return;
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && isInteractive) {
                _drag.addPointer(evt as PointerDownEvent);
                _tap.addPointer(evt as PointerDownEvent);
            }
        }

        // public override void describeSemanticsConfiguration(SemanticsConfiguration config) {
        //     base.describeSemanticsConfiguration(config);
        //
        //     if (isInteractive)
        //         config.onTap = _handleTap;
        //
        //     config.isEnabled = isInteractive;
        //     config.isToggled = _value;
        // }

        public readonly CupertinoThumbPainter _thumbPainter = new CupertinoThumbPainter();

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;

            float currentValue = _position.value;
            float currentReactionValue = _reaction.value;

            float visualPosition = 0f;
            switch (textDirection) {
                case TextDirection.rtl:
                    visualPosition = 1.0f - currentValue;
                    break;
                case TextDirection.ltr:
                    visualPosition = currentValue;
                    break;
            }

            Color trackColor = _value ? activeColor : CupertinoSwitchUtils._kTrackColor;
            float borderThickness =
                1.5f + (CupertinoSwitchUtils._kTrackRadius - 1.5f) * Mathf.Max(currentReactionValue, currentValue);

            Paint paint = new Paint();
            paint.color = trackColor;

            Rect trackRect = Rect.fromLTWH(
                offset.dx + (size.width - CupertinoSwitchUtils._kTrackWidth) / 2.0f,
                offset.dy + (size.height - CupertinoSwitchUtils._kTrackHeight) / 2.0f,
                CupertinoSwitchUtils._kTrackWidth,
                CupertinoSwitchUtils._kTrackHeight
            );
            RRect outerRRect = RRect.fromRectAndRadius(trackRect, Radius.circular(CupertinoSwitchUtils
                ._kTrackRadius));
            RRect innerRRect = RRect.fromRectAndRadius(trackRect.deflate(borderThickness), Radius.circular
                (CupertinoSwitchUtils._kTrackRadius));
            canvas.drawDRRect(outerRRect, innerRRect, paint);

            float currentThumbExtension = CupertinoThumbPainter.extension * currentReactionValue;
            float thumbLeft = MathUtils.lerpFloat(
                trackRect.left + CupertinoSwitchUtils._kTrackInnerStart - CupertinoThumbPainter.radius,
                trackRect.left + CupertinoSwitchUtils._kTrackInnerEnd - CupertinoThumbPainter.radius -
                currentThumbExtension,
                visualPosition
            );
            float thumbRight = MathUtils.lerpFloat(
                trackRect.left + CupertinoSwitchUtils._kTrackInnerStart + CupertinoThumbPainter.radius +
                currentThumbExtension,
                trackRect.left + CupertinoSwitchUtils._kTrackInnerEnd + CupertinoThumbPainter.radius,
                visualPosition
            );
            float thumbCenterY = offset.dy + size.height / 2.0f;

            _thumbPainter.paint(canvas, Rect.fromLTRB(
                thumbLeft,
                thumbCenterY - CupertinoThumbPainter.radius,
                thumbRight,
                thumbCenterY + CupertinoThumbPainter.radius
            ));
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(
                new FlagProperty("value", value: value, ifTrue: "checked", ifFalse: "unchecked", showName: true));
            description.add(new FlagProperty("isInteractive", value: isInteractive, ifTrue: "enabled",
                ifFalse: "disabled",
                showName: true, defaultValue: true));
        }
    }
}