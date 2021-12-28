using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
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
        public static readonly TimeSpan _kReactionDuration = TimeSpan.FromMilliseconds(300);
        public static readonly TimeSpan _kToggleDuration = TimeSpan.FromMilliseconds(200);
    }

    public class CupertinoSwitch : StatefulWidget {
        public CupertinoSwitch(
            bool value,
            ValueChanged<bool> onChanged,
            Key key = null,
            Color activeColor = null,
            Color trackColor = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            this.value = value;
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.trackColor = trackColor;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly bool value;

        public readonly ValueChanged<bool> onChanged;

        public readonly Color activeColor;
        
        public readonly Color trackColor;

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
        public TapGestureRecognizer _tap;
        public HorizontalDragGestureRecognizer _drag;

        AnimationController _positionController;
        public CurvedAnimation position;

        AnimationController _reactionController;
        public Animation<float> _reaction;

        bool isInteractive {
            get {
                return  widget.onChanged != null;
            }
        }
        bool needsPositionAnimation = false;
        
        public override void initState() {
            base.initState();

            _tap = new TapGestureRecognizer();
            _tap.onTapDown = _handleTapDown;
            _tap.onTapUp = _handleTapUp;
            _tap.onTap = _handleTap;
            _tap.onTapCancel = _handleTapCancel;
            _drag = new HorizontalDragGestureRecognizer();
            _drag.onStart = _handleDragStart;
            _drag.onUpdate = _handleDragUpdate;
            _drag.onEnd = _handleDragEnd;
            _drag.dragStartBehavior = widget.dragStartBehavior;

            _positionController = new AnimationController(
                duration: CupertinoSwitchUtils._kToggleDuration,
                value: widget.value ? 1.0f : 0.0f,
                vsync: this
            );
            position = new CurvedAnimation(
                parent: _positionController,
                curve: Curves.linear
            );
            _reactionController = new AnimationController(
                duration: CupertinoSwitchUtils._kReactionDuration,
                vsync: this
            );
            _reaction = new CurvedAnimation(
                parent: _reactionController,
                curve: Curves.ease
            );
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CupertinoSwitch) oldWidget;
            base.didUpdateWidget(oldWidget);
            _drag.dragStartBehavior = widget.dragStartBehavior;

            if (needsPositionAnimation || ((CupertinoSwitch) oldWidget).value != widget.value)
                _resumePositionAnimation(isLinear: needsPositionAnimation);
        }
        void _resumePositionAnimation( bool isLinear = true ) {
            needsPositionAnimation = false;
            position.curve = isLinear ? null : Curves.ease;
            position.reverseCurve = isLinear ? null : Curves.ease.flipped;
            if (widget.value)
                _positionController.forward();
            else
                _positionController.reverse();
        }

        void _handleTapDown(TapDownDetails details) {
            if (isInteractive)
                needsPositionAnimation = false;
            _reactionController.forward();
        }

        void _handleTap() {
            if (isInteractive) {
                widget.onChanged(!widget.value);
            }
        }

        void _handleTapUp(TapUpDetails details) {
            if (isInteractive) {
                needsPositionAnimation = false;
                _reactionController.reverse();
            }
        }

        void _handleTapCancel() {
            if (isInteractive)
                _reactionController.reverse();
        }

        void _handleDragStart(DragStartDetails details) {
            if (isInteractive) {
                needsPositionAnimation = false;
                _reactionController.forward();
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (isInteractive) {
                position.curve = null;
                    position.reverseCurve = null;
                float? delta = details.primaryDelta / CupertinoSwitchUtils._kTrackInnerLength;
                switch (Directionality.of(context)) {
                    case TextDirection.rtl:
                        _positionController.setValue( _positionController.value - delta ?? 0.0f);
                        break;
                    case TextDirection.ltr:
                        _positionController.setValue( _positionController.value + delta ?? 0.0f);
                        break;
                }
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            setState(()=> { needsPositionAnimation = true; });
            if (position.value >= 0.5 != widget.value)
                widget.onChanged(!widget.value);
            _reactionController.reverse();
        }
        public override Widget build(BuildContext context) {
            if (needsPositionAnimation)
                _resumePositionAnimation();
            return new Opacity(
                opacity: widget.onChanged == null ? CupertinoSwitchUtils._kCupertinoSwitchDisabledOpacity : 1.0f,
                child: new _CupertinoSwitchRenderObjectWidget(
                    value: widget.value,
                    activeColor: CupertinoDynamicColor.resolve(
                        widget.activeColor ?? CupertinoColors.systemGreen,
                        context
                    ),
                    trackColor: CupertinoDynamicColor.resolve(widget.trackColor ?? CupertinoColors.secondarySystemFill, context),
                    onChanged: widget.onChanged,
                    textDirection: Directionality.of(context),
                    state: this
                )
            );
        }
        public override void dispose() {
            _tap.dispose();
            _drag.dispose();

            _positionController.dispose();
            _reactionController.dispose();
            base.dispose();
        }
    }

    class _CupertinoSwitchRenderObjectWidget : LeafRenderObjectWidget {
        public _CupertinoSwitchRenderObjectWidget(
            Key key = null,
            bool value = false,
            Color activeColor = null,
            Color trackColor = null,
            ValueChanged<bool> onChanged = null,
            TextDirection? textDirection = null,
            _CupertinoSwitchState state = null
        ) : base(key: key) {
            this.value = value;
            this.activeColor = activeColor;
            this.trackColor = trackColor;
            this.onChanged = onChanged;
            this.state = state;
            this.textDirection = textDirection;

        }

        public readonly bool value;
        public readonly Color activeColor;
        public readonly Color trackColor;
        public readonly ValueChanged<bool> onChanged;
        public readonly _CupertinoSwitchState state;
        public readonly TextDirection? textDirection;
        
        
        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoSwitch(
                value: value,
                activeColor: activeColor,
                trackColor: trackColor,
                onChanged: onChanged,
                textDirection: textDirection,
                state: state
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var _renderObject = renderObject as _RenderCupertinoSwitch;
            _renderObject.value = value;
            _renderObject.activeColor = activeColor;
            _renderObject.trackColor = trackColor;
            _renderObject.onChanged = onChanged;
            _renderObject.textDirection = textDirection;
        }
    }


    class _RenderCupertinoSwitch : RenderConstrainedBox {
        public _RenderCupertinoSwitch(
            bool value,
            Color activeColor,
            Color trackColor = null,
            TextDirection? textDirection = null,
            ValueChanged<bool> onChanged = null,
            _CupertinoSwitchState state = null
        ) : base(additionalConstraints: BoxConstraints.tightFor(
            width: CupertinoSwitchUtils._kSwitchWidth,
            height: CupertinoSwitchUtils._kSwitchHeight)
        ) {
            D.assert(state != null);
            _value = value;
            _activeColor = activeColor;
            _trackColor = trackColor;
            _onChanged = onChanged;
            _textDirection = textDirection;
            _state = state;
            state.position.addListener(markNeedsPaint);
            state._reaction.addListener(markNeedsPaint);


        }

        AnimationController _positionController;
        CurvedAnimation _position;
        AnimationController _reactionController;
        Animation<float> _reaction;
        public readonly _CupertinoSwitchState _state;

        public bool value {
            get { return _value; }
            set {
                if (value == _value)
                    return;
                _value = value;
            }
        }

        bool _value;


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

        public Color trackColor {
            get { return _trackColor; }
            set {
                D.assert(value != null);
                if (value == _trackColor)
                    return;
                _trackColor = value;
                markNeedsPaint();
            }
        }

        Color _trackColor;

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

        public TextDirection? textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value) {
                    return;
                }

                _textDirection = value;
                markNeedsPaint();
            }
        }

        TextDirection? _textDirection;

        public bool isInteractive {
            get { return onChanged != null; }
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && isInteractive) {
                _state._drag.addPointer((PointerDownEvent) evt);
                _state._tap.addPointer((PointerDownEvent) evt);
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;
            float currentValue = _state.position.value;
            float currentReactionValue = _state._reaction.value;

            float visualPosition = 0.0f;
            switch (textDirection) {
                case TextDirection.rtl:
                    visualPosition = 1.0f - currentValue;
                    break;
                case TextDirection.ltr:
                    visualPosition = currentValue;
                    break;
            }

            Paint paint = new Paint() {color = Color.lerp(trackColor, activeColor, currentValue)};
            Rect trackRect = Rect.fromLTWH(
                offset.dx + (size.width - CupertinoSwitchUtils._kTrackWidth) / 2.0f,
                offset.dy + (size.height - CupertinoSwitchUtils._kTrackHeight) / 2.0f,
                CupertinoSwitchUtils._kTrackWidth,
                CupertinoSwitchUtils._kTrackHeight
            );
            RRect trackRRect = RRect.fromRectAndRadius(trackRect, Radius.circular(CupertinoSwitchUtils._kTrackRadius));
            canvas.drawRRect(trackRRect, paint);

            float currentThumbExtension = CupertinoThumbPainter.extension * currentReactionValue;
            float thumbLeft = MathUtils.lerpNullableFloat(
                trackRect.left + CupertinoSwitchUtils._kTrackInnerStart - CupertinoThumbPainter.radius,
                trackRect.left + CupertinoSwitchUtils._kTrackInnerEnd - CupertinoThumbPainter.radius -
                currentThumbExtension,
                visualPosition
            );
            float thumbRight = MathUtils.lerpNullableFloat(
                trackRect.left + CupertinoSwitchUtils._kTrackInnerStart + CupertinoThumbPainter.radius +
                currentThumbExtension,
                trackRect.left + CupertinoSwitchUtils._kTrackInnerEnd + CupertinoThumbPainter.radius,
                visualPosition
            );
            float thumbCenterY = offset.dy + size.height / 2.0f;
            Rect thumbBounds = Rect.fromLTRB(
                thumbLeft,
                thumbCenterY - CupertinoThumbPainter.radius,
                thumbRight,
                thumbCenterY + CupertinoThumbPainter.radius
            );

            context.pushClipRRect(needsCompositing, Offset.zero, thumbBounds, trackRRect,
                (PaintingContext innerContext, Offset offset1) => {
                    CupertinoThumbPainter.switchThumb().paint(innerContext.canvas, thumbBounds);
                });
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