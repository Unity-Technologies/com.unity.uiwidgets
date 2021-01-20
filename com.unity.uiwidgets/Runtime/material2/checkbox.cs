using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    class CheckboxUtils {
        public const float _kEdgeSize = Checkbox.width;
        public static readonly Radius _kEdgeRadius = Radius.circular(1.0f);
        public const float _kStrokeWidth = 2.0f;
    }

    public class Checkbox : StatefulWidget {
        public Checkbox(
            Key key = null,
            bool? value = false,
            bool tristate = false,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color checkColor = null,
            MaterialTapTargetSize? materialTapTargetSize = null
        ) : base(key: key) {
            D.assert(tristate || value != null);
            this.value = value;
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.checkColor = checkColor;
            this.tristate = tristate;
            this.materialTapTargetSize = materialTapTargetSize;
        }

        public readonly bool? value;

        public readonly ValueChanged<bool?> onChanged;

        public readonly Color activeColor;

        public readonly Color checkColor;

        public readonly bool tristate;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        public const float width = 18.0f;

        public override State createState() {
            return new _CheckboxState();
        }
    }

    class _CheckboxState : TickerProviderStateMixin<Checkbox> {
        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            ThemeData themeData = Theme.of(context);
            Size size;
            switch (widget.materialTapTargetSize ?? themeData.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    size = new Size(2 * Constants.kRadialReactionRadius + 8.0f,
                        2 * Constants.kRadialReactionRadius + 8.0f);
                    break;
                case MaterialTapTargetSize.shrinkWrap:
                    size = new Size(2 * Constants.kRadialReactionRadius, 2 * Constants.kRadialReactionRadius);
                    break;
                default:
                    throw new Exception("Unknown target size: " + widget.materialTapTargetSize);
            }

            BoxConstraints additionalConstraints = BoxConstraints.tight(size);
            return new _CheckboxRenderObjectWidget(
                value: widget.value,
                tristate: widget.tristate,
                activeColor: widget.activeColor ?? themeData.toggleableActiveColor,
                checkColor: widget.checkColor ?? new Color(0xFFFFFFFF),
                inactiveColor: widget.onChanged != null
                    ? themeData.unselectedWidgetColor
                    : themeData.disabledColor,
                onChanged: widget.onChanged,
                additionalConstraints: additionalConstraints,
                vsync: this
            );
        }
    }

    class _CheckboxRenderObjectWidget : LeafRenderObjectWidget {
        public _CheckboxRenderObjectWidget(
            Key key = null,
            bool? value = null,
            bool tristate = false,
            Color activeColor = null,
            Color checkColor = null,
            Color inactiveColor = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null,
            BoxConstraints additionalConstraints = null
        ) : base(key: key) {
            D.assert(tristate || value != null);
            D.assert(activeColor != null);
            D.assert(inactiveColor != null);
            D.assert(vsync != null);
            this.value = value;
            this.tristate = tristate;
            this.activeColor = activeColor;
            this.checkColor = checkColor;
            this.inactiveColor = inactiveColor;
            this.onChanged = onChanged;
            this.vsync = vsync;
            this.additionalConstraints = additionalConstraints;
        }

        public readonly bool? value;
        public readonly bool tristate;
        public readonly Color activeColor;
        public readonly Color checkColor;
        public readonly Color inactiveColor;
        public readonly ValueChanged<bool?> onChanged;
        public readonly TickerProvider vsync;
        public readonly BoxConstraints additionalConstraints;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCheckbox(
                value: value,
                tristate: tristate,
                activeColor: activeColor,
                checkColor: checkColor,
                inactiveColor: inactiveColor,
                onChanged: onChanged,
                vsync: vsync,
                additionalConstraints: additionalConstraints
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderCheckbox renderObject = _renderObject as _RenderCheckbox;
            renderObject.value = value;
            renderObject.tristate = tristate;
            renderObject.activeColor = activeColor;
            renderObject.checkColor = checkColor;
            renderObject.inactiveColor = inactiveColor;
            renderObject.onChanged = onChanged;
            renderObject.additionalConstraints = additionalConstraints;
            renderObject.vsync = vsync;
        }
    }


    class _RenderCheckbox : RenderToggleable {
        public _RenderCheckbox(
            bool? value = null,
            bool tristate = false,
            Color activeColor = null,
            Color checkColor = null,
            Color inactiveColor = null,
            BoxConstraints additionalConstraints = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null
        ) : base(
                value: value,
                tristate: tristate,
                activeColor: activeColor,
                inactiveColor: inactiveColor,
                onChanged: onChanged,
                additionalConstraints: additionalConstraints,
                vsync: vsync
            ) {
            _oldValue = value;
            this.checkColor = checkColor;
        }

        bool? _oldValue;
        public Color checkColor;

        public override bool? value {
            set {
                if (value == this.value) {
                    return;
                }

                _oldValue = this.value;
                base.value = value;
            }
        }

        RRect _outerRectAt(Offset origin, float t) {
            float inset = 1.0f - (t - 0.5f).abs() * 2.0f;
            float size = CheckboxUtils._kEdgeSize - inset * CheckboxUtils._kStrokeWidth;
            Rect rect = Rect.fromLTWH(origin.dx + inset, origin.dy + inset, size, size);
            return RRect.fromRectAndRadius(rect, CheckboxUtils._kEdgeRadius);
        }

        Color _colorAt(float t) {
            return onChanged == null
                ? inactiveColor
                : (t >= 0.25f ? activeColor : Color.lerp(inactiveColor, activeColor, t * 4.0f));
        }

        void _initStrokePaint(Paint paint) {
            paint.color = checkColor;
            paint.style = PaintingStyle.stroke;
            paint.strokeWidth = CheckboxUtils._kStrokeWidth;
        }

        void _drawBorder(Canvas canvas, RRect outer, float t, Paint paint) {
            D.assert(t >= 0.0f && t <= 0.5f);
            float size = outer.width;
            RRect inner = outer.deflate(Mathf.Min(size / 2.0f, CheckboxUtils._kStrokeWidth + size * t));
            canvas.drawDRRect(outer, inner, paint);
        }

        void _drawCheck(Canvas canvas, Offset origin, float t, Paint paint) {
            D.assert(t >= 0.0f && t <= 1.0f);
            Path path = new Path();
            Offset start = new Offset(CheckboxUtils._kEdgeSize * 0.15f, CheckboxUtils._kEdgeSize * 0.45f);
            Offset mid = new Offset(CheckboxUtils._kEdgeSize * 0.4f, CheckboxUtils._kEdgeSize * 0.7f);
            Offset end = new Offset(CheckboxUtils._kEdgeSize * 0.85f, CheckboxUtils._kEdgeSize * 0.25f);
            if (t < 0.5f) {
                float strokeT = t * 2.0f;
                Offset drawMid = Offset.lerp(start, mid, strokeT);
                path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
                path.lineTo(origin.dx + drawMid.dx, origin.dy + drawMid.dy);
            }
            else {
                float strokeT = (t - 0.5f) * 2.0f;
                Offset drawEnd = Offset.lerp(mid, end, strokeT);
                path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
                path.lineTo(origin.dx + mid.dx, origin.dy + mid.dy);
                path.lineTo(origin.dx + drawEnd.dx, origin.dy + drawEnd.dy);
            }

            canvas.drawPath(path, paint);
        }

        void _drawDash(Canvas canvas, Offset origin, float t, Paint paint) {
            D.assert(t >= 0.0f && t <= 1.0f);
            Offset start = new Offset(CheckboxUtils._kEdgeSize * 0.2f, CheckboxUtils._kEdgeSize * 0.5f);
            Offset mid = new Offset(CheckboxUtils._kEdgeSize * 0.5f, CheckboxUtils._kEdgeSize * 0.5f);
            Offset end = new Offset(CheckboxUtils._kEdgeSize * 0.8f, CheckboxUtils._kEdgeSize * 0.5f);
            Offset drawStart = Offset.lerp(start, mid, 1.0f - t);
            Offset drawEnd = Offset.lerp(mid, end, t);
            canvas.drawLine(origin + drawStart, origin + drawEnd, paint);
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;
            paintRadialReaction(canvas, offset, size.center(Offset.zero));

            Offset origin = offset + (size / 2.0f - Size.square(CheckboxUtils._kEdgeSize) / 2.0f);
            AnimationStatus status = position.status;
            float tNormalized = status == AnimationStatus.forward || status == AnimationStatus.completed
                ? position.value
                : 1.0f - position.value;

            if (_oldValue == false || value == false) {
                float t = value == false ? 1.0f - tNormalized : tNormalized;
                RRect outer = _outerRectAt(origin, t);
                Paint paint = new Paint();
                paint.color = _colorAt(t);

                if (t <= 0.5f) {
                    _drawBorder(canvas, outer, t, paint);
                }
                else {
                    canvas.drawRRect(outer, paint);

                    _initStrokePaint(paint);
                    float tShrink = (t - 0.5f) * 2.0f;
                    if (_oldValue == null || value == null) {
                        _drawDash(canvas, origin, tShrink, paint);
                    }
                    else {
                        _drawCheck(canvas, origin, tShrink, paint);
                    }
                }
            }
            else {
                // Two cases: null to true, true to null
                RRect outer = _outerRectAt(origin, 1.0f);
                Paint paint = new Paint();
                paint.color = _colorAt(1.0f);
                canvas.drawRRect(outer, paint);

                _initStrokePaint(paint);
                if (tNormalized <= 0.5f) {
                    float tShrink = 1.0f - tNormalized * 2.0f;
                    if (_oldValue == true) {
                        _drawCheck(canvas, origin, tShrink, paint);
                    }
                    else {
                        _drawDash(canvas, origin, tShrink, paint);
                    }
                }
                else {
                    float tExpand = (tNormalized - 0.5f) * 2.0f;
                    if (value == true) {
                        _drawCheck(canvas, origin, tExpand, paint);
                    }
                    else {
                        _drawDash(canvas, origin, tExpand, paint);
                    }
                }
            }
        }
    }
}