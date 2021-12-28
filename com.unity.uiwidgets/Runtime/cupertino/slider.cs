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
    class SliderUtils {
        public const float _kPadding = 8.0f;
        public static readonly Color _kTrackColor = new Color(0xFFB5B5B5);
        public const float _kSliderHeight = 2.0f * (CupertinoThumbPainter.radius + _kPadding);
        public const float _kSliderWidth = 176.0f; // Matches Material Design slider.
        public static readonly TimeSpan _kDiscreteTransitionDuration = TimeSpan.FromMilliseconds(500);
        public const float _kAdjustmentUnit = 0.1f; // Matches iOS implementation of material slider.
    }

    public class CupertinoSlider : StatefulWidget {
        public CupertinoSlider(
            Key key = null,
            float? value = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            float min = 0.0f,
            float max = 1.0f,
            int? divisions = null,
            Color activeColor = null,
            Color thumbColor = null
        ) : base(key: key) {
            D.assert(value != null);
            D.assert(onChanged != null);
            D.assert(value >= min && value <= max);
            D.assert(divisions == null || divisions > 0);
           
            this.value = value.Value;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.min = min;
            this.max = max;
            this.divisions = divisions;
            this.activeColor = activeColor;
            this.thumbColor = thumbColor ?? CupertinoColors.white;
        }

        public readonly float value;

        public readonly ValueChanged<float> onChanged;

        public readonly ValueChanged<float> onChangeStart;

        public readonly ValueChanged<float> onChangeEnd;

        public readonly float min;

        public readonly float max;

        public readonly int? divisions;

        public readonly Color activeColor;

        public readonly Color thumbColor;

        public override State createState() {
            return new _CupertinoSliderState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("value", value));
            properties.add(new FloatProperty("min", min));
            properties.add(new FloatProperty("max", max));
        }
    }

    class _CupertinoSliderState : TickerProviderStateMixin<CupertinoSlider> {
        void _handleChanged(float value) {
            D.assert(widget.onChanged != null);
            float lerpValue = MathUtils.lerpNullableFloat(widget.min, widget.max, value);
            if (lerpValue != widget.value) {
                widget.onChanged(lerpValue);
            }
        }

        void _handleDragStart(float value) {
            D.assert(widget.onChangeStart != null);
            widget.onChangeStart(MathUtils.lerpNullableFloat(widget.min, widget.max, value));
        }

        void _handleDragEnd(float value) {
            D.assert(widget.onChangeEnd != null);
            widget.onChangeEnd(MathUtils.lerpNullableFloat(widget.min, widget.max, value));
        }

        public override Widget build(BuildContext context) {
            return new _CupertinoSliderRenderObjectWidget(
                value: (widget.value - widget.min) / (widget.max - widget.min),
                divisions: widget.divisions,
                activeColor: CupertinoDynamicColor.resolve(
                    widget.activeColor ?? CupertinoTheme.of(context).primaryColor,
                    context
                ),
                thumbColor: widget.thumbColor,
                onChanged: widget.onChanged != null ? (ValueChanged<float>) _handleChanged : null,
                onChangeStart: widget.onChangeStart != null ? (ValueChanged<float>) _handleDragStart : null,
                onChangeEnd: widget.onChangeEnd != null ? (ValueChanged<float>) _handleDragEnd : null,
                vsync: this
            );
        }
    }

    class _CupertinoSliderRenderObjectWidget : LeafRenderObjectWidget {
        public _CupertinoSliderRenderObjectWidget(
            Key key = null,
            float? value = null,
            int? divisions = null,
            Color activeColor = null,
            Color thumbColor = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            TickerProvider vsync = null
        ) : base(key: key) {
            this.value = value;
            this.divisions = divisions;
            this.activeColor = activeColor;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.vsync = vsync;
            this.thumbColor = thumbColor;
        }

        public readonly float? value;
        public readonly int? divisions;
        public readonly Color activeColor;
        public readonly Color thumbColor;
        public readonly ValueChanged<float> onChanged;
        public readonly ValueChanged<float> onChangeStart;
        public readonly ValueChanged<float> onChangeEnd;
        public readonly TickerProvider vsync;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoSlider(
                value: value ?? 0.0f,
                divisions: divisions,
                activeColor: activeColor,
                thumbColor: CupertinoDynamicColor.resolve(thumbColor, context),
                trackColor: CupertinoDynamicColor.resolve(CupertinoColors.systemFill, context),
                onChanged: onChanged,
                onChangeStart: onChangeStart,
                onChangeEnd: onChangeEnd,
                vsync: vsync,
                textDirection: Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderCupertinoSlider renderObject = _renderObject as _RenderCupertinoSlider;
            renderObject.value = value ?? 0.0f;
            renderObject.divisions = divisions;
            renderObject.activeColor = activeColor;
            renderObject.thumbColor = CupertinoDynamicColor.resolve(thumbColor, context);
            renderObject.trackColor = CupertinoDynamicColor.resolve(CupertinoColors.systemFill, context);
            renderObject.onChanged = onChanged;
            renderObject.onChangeStart = onChangeStart;
            renderObject.onChangeEnd = onChangeEnd;
            renderObject.textDirection = Directionality.of(context);
        }
    }

    class _RenderCupertinoSlider : RenderConstrainedBox {
        public _RenderCupertinoSlider(
            float value,
            int? divisions = null,
            Color activeColor = null,
            Color thumbColor = null,
            Color trackColor = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            TickerProvider vsync = null,
            TextDirection? textDirection = null
        ) : base(additionalConstraints: BoxConstraints.tightFor(width: SliderUtils._kSliderWidth,
            height: SliderUtils._kSliderHeight)) {
            D.assert(value >= 0.0f && value <= 1.0f);
            _value = value;
            _divisions = divisions;
            _activeColor = activeColor;
            _thumbColor = thumbColor;
            _trackColor = trackColor;
            _onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            _textDirection = textDirection;
            _drag = new HorizontalDragGestureRecognizer();
            _drag.onStart = _handleDragStart;
            _drag.onUpdate = _handleDragUpdate;
            _drag.onEnd = _handleDragEnd;
            _position = new AnimationController(
                value: value,
                duration: SliderUtils._kDiscreteTransitionDuration,
                vsync: vsync
            );
            _position.addListener(markNeedsPaint);
        }

        public float value {
            get { return _value; }
            set {
                D.assert(value >= 0.0f && value <= 1.0f);
                if (value == _value) {
                    return;
                }

                _value = value;
                if (divisions != null) {
                    _position.animateTo(value, curve: Curves.fastOutSlowIn);
                }
                else {
                    _position.setValue(value);
                }
            }
        }

        float _value;

        public int? divisions {
            get { return _divisions; }
            set {
                if (value == _divisions) {
                    return;
                }

                _divisions = value;
                markNeedsPaint();
            }
        }

        int? _divisions;

        public Color activeColor {
            get { return _activeColor; }
            set {
                if (value == _activeColor) {
                    return;
                }

                _activeColor = value;
                markNeedsPaint();
            }
        }

        Color _activeColor;

        public Color thumbColor {
            get {
                return _thumbColor;
            }
            set {
                if (value == _thumbColor)
                    return;
                _thumbColor = value;
                markNeedsPaint();
            }
        }
        Color _thumbColor;


        public Color trackColor {
            get {
                return _trackColor;
            }
            set {
                if (value == _trackColor)
                    return;
                _trackColor = value;
                markNeedsPaint();
            }
        }
        Color _trackColor;
       

        public ValueChanged<float> onChanged {
            get { return _onChanged; }
            set {
                if (value == _onChanged) {
                    return;
                }

                _onChanged = value;
            }
        }

        ValueChanged<float> _onChanged;

        public ValueChanged<float> onChangeStart;
        public ValueChanged<float> onChangeEnd;

        public TextDirection? textDirection {
            get {
                return _textDirection;
            }
            set {
                D.assert(value != null);
                if (_textDirection == value)
                    return;
                _textDirection = value;
                markNeedsPaint();
            }
        }
        TextDirection? _textDirection;
        



        AnimationController _position;

        HorizontalDragGestureRecognizer _drag;
        float _currentDragValue = 0.0f;

        float _discretizedCurrentDragValue {
            get {
                float dragValue = _currentDragValue.clamp(0.0f, 1.0f);
                if (divisions != null) {
                    dragValue = Mathf.Round(dragValue * divisions.Value) / divisions.Value;
                }

                return dragValue;
            }
        }

        public float _trackLeft {
            get { return SliderUtils._kPadding; }
        }

        public float _trackRight {
            get { return size.width - SliderUtils._kPadding; }
        }

        float _thumbCenter {
            get {
                float visualPosition = 0.0f;
                switch (textDirection) {
                    case TextDirection.rtl:
                        visualPosition = 1.0f - _value;
                        break;
                    case TextDirection.ltr:
                        visualPosition = _value;
                        break;
                }
                return MathUtils.lerpNullableFloat(_trackLeft + CupertinoThumbPainter.radius, _trackRight - CupertinoThumbPainter.radius, visualPosition);
            }
        }

        public bool isInteractive {
            get { return onChanged != null; }
        }

        void _handleDragStart(DragStartDetails details) {
            _startInteraction(details.globalPosition);
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (isInteractive) {
                float extent = Mathf.Max(SliderUtils._kPadding,
                    size.width - 2.0f * (SliderUtils._kPadding + CupertinoThumbPainter.radius));
                float? valueDelta = details.primaryDelta / extent;
                switch (textDirection) {
                    case TextDirection.rtl:
                        _currentDragValue -= valueDelta ?? 0.0f;
                        break;
                    case TextDirection.ltr:
                        _currentDragValue += valueDelta ?? 0.0f;
                        break;
                }
                onChanged(_discretizedCurrentDragValue);
               
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            _endInteraction();
        }

        void _startInteraction(Offset globalPosition) {
            if (isInteractive) {
                if (onChangeStart != null) {
                    onChangeStart(_discretizedCurrentDragValue);
                }

                _currentDragValue = _value;
                onChanged(_discretizedCurrentDragValue);
            }
        }

        void _endInteraction() {
            if (onChangeEnd != null) {
                onChangeEnd(_discretizedCurrentDragValue);
            }

            _currentDragValue = 0.0f;
        }

        protected override bool hitTestSelf(Offset position) {
            return (position.dx - _thumbCenter).abs() < CupertinoThumbPainter.radius + SliderUtils._kPadding;
        }

        public override void handleEvent(PointerEvent e, HitTestEntry entry) {
            D.assert(debugHandleEvent(e, entry));
            if (e is PointerDownEvent pointerDownEvent && isInteractive) {
                _drag.addPointer(pointerDownEvent);
            }
        }

        CupertinoThumbPainter _thumbPainter = new CupertinoThumbPainter();

        public override void paint(PaintingContext context, Offset offset) {
            float visualPosition = 0.0f;
            Color leftColor = null ;
            Color rightColor = null;
            switch (textDirection) {
                case TextDirection.rtl:
                    visualPosition = 1.0f - _position.value;
                    leftColor = _activeColor;
                    rightColor = trackColor;
                    break;
                case TextDirection.ltr:
                    visualPosition = _position.value;
                    leftColor = trackColor;
                    rightColor = _activeColor;
                    break;
            }
           

            float trackCenter = offset.dy + size.height / 2.0f;
            float trackLeft = offset.dx + _trackLeft;
            float trackTop = trackCenter - 1.0f;
            float trackBottom = trackCenter + 1.0f;
            float trackRight = offset.dx + _trackRight;
            float trackActive = offset.dx + _thumbCenter;

            Canvas canvas = context.canvas;

            if (visualPosition > 0.0f) {
                Paint paint = new Paint();
                paint.color = rightColor;
                canvas.drawRRect(RRect.fromLTRBXY(trackLeft, trackTop, trackActive, trackBottom, 1.0f, 1.0f), paint);
            }

            if (visualPosition < 1.0f) {
                Paint paint = new Paint();
                paint.color = leftColor;
                canvas.drawRRect(RRect.fromLTRBXY(trackActive, trackTop, trackRight, trackBottom, 1.0f, 1.0f), paint);
            }

            Offset thumbCenter = new Offset(trackActive, trackCenter);
            new CupertinoThumbPainter(color: thumbColor).paint(canvas, Rect.fromCircle(center: thumbCenter, radius: CupertinoThumbPainter.radius));
        }
    }
}