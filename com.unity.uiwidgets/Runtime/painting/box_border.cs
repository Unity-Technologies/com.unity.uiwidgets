using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public enum BoxShape {
        rectangle,
        circle,
    }


    public abstract class BoxBorder : ShapeBorder {
        public BoxBorder() {
            isUniform = false;
        }

        public BorderSide top { get; set; }

        public BorderSide bottom { get; set; }

        public virtual bool isUniform { get; }


        public override ShapeBorder add(ShapeBorder other, bool reversed = false) => null;

        public static BoxBorder lerp(BoxBorder a, BoxBorder b, float t) {
            if ((a is Border || a == null) && (b is Border || b == null))
                return Border.lerp((Border) a, (Border) b, t);
            if ((a is BorderDirectional || a == null) && (b is BorderDirectional || b == null))
                return BorderDirectional.lerp(a as BorderDirectional, b as BorderDirectional, t);
            if (b is Border && a is BorderDirectional) {
                BoxBorder c = b;
                b = a;
                a = c;
                t = 1.0f - t;
            }

            if (a is Border border && b is BorderDirectional borderDirectional) {
                if (borderDirectional.start == BorderSide.none && borderDirectional.end == BorderSide.none) {
                    return new Border(
                        top: BorderSide.lerp(border.top, borderDirectional.top, t),
                        right: BorderSide.lerp(border.right, BorderSide.none, t),
                        bottom: BorderSide.lerp(border.bottom, borderDirectional.bottom, t),
                        left: BorderSide.lerp(border.left, BorderSide.none, t)
                    );
                }

                if (border.left == BorderSide.none && border.right == BorderSide.none) {
                    return new BorderDirectional(
                        top: BorderSide.lerp(border.top, borderDirectional.top, t),
                        start: BorderSide.lerp(BorderSide.none, borderDirectional.start, t),
                        end: BorderSide.lerp(BorderSide.none, borderDirectional.end, t),
                        bottom: BorderSide.lerp(border.bottom, borderDirectional.bottom, t)
                    );
                }

                if (t < 0.5f) {
                    return new Border(
                        top: BorderSide.lerp(border.top, borderDirectional.top, t),
                        right: BorderSide.lerp(border.right, BorderSide.none, t * 2.0f),
                        bottom: BorderSide.lerp(border.bottom, borderDirectional.bottom, t),
                        left: BorderSide.lerp(border.left, BorderSide.none, t * 2.0f)
                    );
                }

                return new BorderDirectional(
                    top: BorderSide.lerp(border.top, borderDirectional.top, t),
                    start: BorderSide.lerp(BorderSide.none, borderDirectional.start, (t - 0.5f) * 2.0f),
                    end: BorderSide.lerp(BorderSide.none, borderDirectional.end, (t - 0.5f) * 2.0f),
                    bottom: BorderSide.lerp(border.bottom, borderDirectional.bottom, t)
                );
            }

            throw new UIWidgetsError(new List<DiagnosticsNode>() {
                new ErrorSummary("BoxBorder.lerp can only interpolate Border and BorderDirectional classes."),
                new ErrorDescription(
                    "BoxBorder.lerp() was called with two objects of type ${a.runtimeType} and ${b.runtimeType}:\n" +
                    "  $a\n" +
                    "  $b\n" +
                    "However, only Border and BorderDirectional classes are supported by this method."
                ),
                new ErrorHint("For a more general interpolation method, consider using ShapeBorder.lerp instead.")
            });
        }

        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null) {
            D.assert(textDirection != null,
                () => "The textDirection argument to $runtimeType.getInnerPath must not be null.");
            var result = new Path();
            result.addRect(dimensions.resolve(textDirection).deflateRect(rect));
            return result;
        }

        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null) {
            D.assert(textDirection != null,
                () => "The textDirection argument to $runtimeType.getOuterPath must not be null.");
            var result = new Path();
            result.addRect(rect);
            return result;
        }

        public virtual void paint(
            Canvas canvas,
            Rect rect,
            TextDirection? textDirection = null,
            BoxShape shape = BoxShape.rectangle,
            BorderRadius borderRadus = null
        ) {
            paint(canvas, rect, textDirection);
        }

        internal virtual void _paintUniformBorderWithRadius(Canvas canvas, Rect rect, BorderSide side,
            BorderRadius borderRadius) {
            D.assert(side.style != BorderStyle.none);
            Paint paint = new Paint();
            paint.color = side.color;
            RRect outer = borderRadius.toRRect(rect);
            float width = side.width;
            if (width == 0.0f) {
                paint.style = PaintingStyle.stroke;
                paint.strokeWidth = 0.0f;
                canvas.drawRRect(outer, paint);
            }
            else {
                RRect inner = outer.deflate(width);
                canvas.drawDRRect(outer, inner, paint);
            }
        }

        internal virtual void _paintUniformBorderWithCircle(Canvas canvas, Rect rect, BorderSide side) {
            D.assert(side.style != BorderStyle.none);
            float width = side.width;
            Paint paint = side.toPaint();
            float radius = (rect.shortestSide - width) / 2.0f;
            canvas.drawCircle(rect.center, radius, paint);
        }

        internal virtual void _paintUniformBorderWithRectangle(Canvas canvas, Rect rect, BorderSide side) {
            D.assert(side.style != BorderStyle.none);
            float width = side.width;
            Paint paint = side.toPaint();
            canvas.drawRect(rect.deflate(width / 2.0f), paint);
        }
    }


    public class Border : BoxBorder, IEquatable<Border> {
        public Border(
            BorderSide top = null,
            BorderSide right = null,
            BorderSide bottom = null,
            BorderSide left = null
        ) {
            this.top = top ?? BorderSide.none;
            this.right = right ?? BorderSide.none;
            this.bottom = bottom ?? BorderSide.none;
            this.left = left ?? BorderSide.none;
        }

        public static Border fromBorderSide(BorderSide side) {
            D.assert(side != null);
            return new Border(top: side, right: side, bottom: side, left: side);
        }

        public static Border symmetric(
            BorderSide vertical = null,
            BorderSide horizontal = null
        ) {
            vertical = vertical ?? BorderSide.none;
            horizontal = horizontal ?? BorderSide.none;
            return new Border(top: vertical, left: horizontal, right: horizontal, bottom: vertical);
        }

        public static Border all(
            Color color = null,
            float width = 1.0f,
            BorderStyle style = BorderStyle.solid
        ) {
            BorderSide side = new BorderSide(color: color, width: width, style: style);
            return fromBorderSide(side);
        }

        public static Border merge(Border a, Border b) {
            D.assert(a != null);
            D.assert(b != null);
            D.assert(BorderSide.canMerge(a.top, b.top));
            D.assert(BorderSide.canMerge(a.right, b.right));
            D.assert(BorderSide.canMerge(a.bottom, b.bottom));
            D.assert(BorderSide.canMerge(a.left, b.left));

            return new Border(
                top: BorderSide.merge(a.top, b.top),
                right: BorderSide.merge(a.right, b.right),
                bottom: BorderSide.merge(a.bottom, b.bottom),
                left: BorderSide.merge(a.left, b.left)
            );
        }
        
        public readonly BorderSide right;
        public readonly BorderSide left;

        public override EdgeInsetsGeometry dimensions {
            get {
                return EdgeInsets.fromLTRB(
                    left.width,
                    top.width,
                    right.width,
                    bottom.width);
            }
        }

        public override bool isUniform {
            get { return isSameColor && isSameWidth && isSameStyle; }
        }

        public bool isSameColor {
            get {
                Color topColor = top.color;
                return right.color == topColor
                       && bottom.color == topColor
                       && left.color == topColor;
            }
        }

        public bool isSameWidth {
            get {
                var topWidth = top.width;
                return right.width == topWidth
                       && bottom.width == topWidth
                       && left.width == topWidth;
            }
        }

        public bool isSameStyle {
            get {
                var topStyle = top.style;
                return right.style == topStyle
                       && bottom.style == topStyle
                       && left.style == topStyle;
            }
        }

        public override ShapeBorder add(ShapeBorder other, bool reversed = false) {
            if (!(other is Border border)) {
                return null;
            }

            if (BorderSide.canMerge(top, border.top) &&
                BorderSide.canMerge(right, border.right) &&
                BorderSide.canMerge(bottom, border.bottom) &&
                BorderSide.canMerge(left, border.left)) {
                return merge(this, border);
            }

            return null;
        }

        public override ShapeBorder scale(float t) {
            return new Border(
                top: top.scale(t),
                right: right.scale(t),
                bottom: bottom.scale(t),
                left: left.scale(t)
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is Border border) {
                return lerp(border, this, t);
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is Border border) {
                return lerp(this, border, t);
            }

            return base.lerpTo(b, t);
        }

        public static Border lerp(Border a, Border b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (Border) b.scale(t);
            }

            if (b == null) {
                return (Border) a.scale(1.0f - t);
            }

            return new Border(
                top: BorderSide.lerp(a.top, b.top, t),
                right: BorderSide.lerp(a.right, b.right, t),
                bottom: BorderSide.lerp(a.bottom, b.bottom, t),
                left: BorderSide.lerp(a.left, b.left, t)
            );
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection) {
            paint(canvas, rect, textDirection);
        }

        public override void paint(Canvas canvas, Rect rect,
            TextDirection? textDirection = null,
            BoxShape shape = BoxShape.rectangle,
            BorderRadius borderRadius = null) {
            if (isUniform) {
                switch (top.style) {
                    case BorderStyle.none:
                        return;
                    case BorderStyle.solid:
                        switch (shape) {
                            case BoxShape.circle:
                                D.assert(borderRadius == null,
                                    () => "A borderRadius can only be given for rectangular boxes.");
                                _paintUniformBorderWithCircle(canvas, rect, top);
                                break;
                            case BoxShape.rectangle:
                                if (borderRadius != null) {
                                    _paintUniformBorderWithRadius(canvas, rect, top, borderRadius);
                                }
                                else {
                                    _paintUniformBorderWithRectangle(canvas, rect, top);
                                }

                                break;
                        }

                        return;
                }
            }

            D.assert(borderRadius == null, () => "A borderRadius can only be given for uniform borders.");
            D.assert(shape == BoxShape.rectangle, () => "A border can only be drawn as a circle if it is uniform.");

            BorderUtils.paintBorder(canvas, rect,
                top: top, right: right, bottom: bottom, left: left);
        }

        internal override void _paintUniformBorderWithRadius(Canvas canvas, Rect rect, BorderSide side,
            BorderRadius borderRadius) {
            D.assert(side.style != BorderStyle.none);
            Paint paint = new Paint {
                color = side.color,
            };

            RRect outer = borderRadius.toRRect(rect);
            float width = side.width;
            if (width == 0.0) {
                paint.style = PaintingStyle.stroke;
                paint.strokeWidth = 0.0f;
                canvas.drawRRect(outer, paint);
            }
            else {
                RRect inner = outer.deflate(width);
                canvas.drawDRRect(outer, inner, paint);
            }
        }

        internal override void _paintUniformBorderWithCircle(Canvas canvas, Rect rect, BorderSide side) {
            D.assert(side.style != BorderStyle.none);
            float width = side.width;
            Paint paint = side.toPaint();
            float radius = (rect.shortestSide - width) / 2.0f;
            canvas.drawCircle(rect.center, radius, paint);
        }

        internal override void _paintUniformBorderWithRectangle(Canvas canvas, Rect rect, BorderSide side) {
            D.assert(side.style != BorderStyle.none);
            float width = side.width;
            Paint paint = side.toPaint();
            canvas.drawRect(rect.deflate(width / 2.0f), paint);
        }

        public bool Equals(Border other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(top, other.top)
                   && Equals(right, other.right)
                   && Equals(bottom, other.bottom)
                   && Equals(left, other.left);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((Border) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (top != null ? top.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (right != null ? right.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (bottom != null ? bottom.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (left != null ? left.GetHashCode() : 0);
                return hashCode;
            }
        }


        public override string ToString() {
            if (isUniform) {
                return $"{GetType()}.all({top})";
            }

            List<string> arguments = new List<string>();
            if (top != BorderSide.none) {
                arguments.Add($"top: {top}");
            }

            if (right != BorderSide.none) {
                arguments.Add($"right: {right}");
            }

            if (bottom != BorderSide.none) {
                arguments.Add($"bottom: {bottom}");
            }

            if (left != BorderSide.none) {
                arguments.Add($"left: {left}");
            }

            return $"{GetType()}({string.Join(", ", arguments)})";
        }
    }

    public class BorderDirectional : BoxBorder, IEquatable<BorderDirectional> {
        public BorderDirectional(
            BorderSide top,
            BorderSide start,
            BorderSide end,
            BorderSide bottom
        ) {
            if (top == null) {
                this.top = BorderSide.none;
            }

            if (start == null) {
                this.start = BorderSide.none;
            }

            if (end == null) {
                this.end = BorderSide.none;
            }

            if (bottom == null) {
                this.bottom = BorderSide.none;
            }

            this.top = top;
            this.start = start;
            this.end = end;
            this.bottom = bottom;
        }

        static BorderDirectional merge(BorderDirectional a, BorderDirectional b) {
            D.assert(a != null);
            D.assert(b != null);
            D.assert(BorderSide.canMerge(a.top, b.top));
            D.assert(BorderSide.canMerge(a.start, b.start));
            D.assert(BorderSide.canMerge(a.end, b.end));
            D.assert(BorderSide.canMerge(a.bottom, b.bottom));
            return new BorderDirectional(
                top: BorderSide.merge(a.top, b.top),
                start: BorderSide.merge(a.start, b.start),
                end: BorderSide.merge(a.end, b.end),
                bottom: BorderSide.merge(a.bottom, b.bottom)
            );
        }
        
        public readonly BorderSide start;
        public readonly BorderSide end;

        public override EdgeInsetsGeometry dimensions {
            get => EdgeInsetsDirectional.fromSTEB(start.width, top.width, end.width, bottom.width);
        }

        public override bool isUniform {
            get {
                Color topColor = top.color;
                if (start.color != topColor ||
                    end.color != topColor ||
                    bottom.color != topColor)
                    return false;
                float topWidth = top.width;
                if (start.width != topWidth ||
                    end.width != topWidth ||
                    bottom.width != topWidth)
                    return false;
                BorderStyle topStyle = top.style;
                if (start.style != topStyle ||
                    end.style != topStyle ||
                    bottom.style != topStyle)
                    return false;
                return true;
            }
        }

        public override ShapeBorder add(ShapeBorder other, bool reversed = false) {
            if (other is BorderDirectional otherBorderDirectional) {
                BorderDirectional typedOther = otherBorderDirectional;
                if (BorderSide.canMerge(top, typedOther.top) &&
                    BorderSide.canMerge(start, typedOther.start) &&
                    BorderSide.canMerge(end, typedOther.end) &&
                    BorderSide.canMerge(bottom, typedOther.bottom)) {
                    return BorderDirectional.merge(this, typedOther);
                }

                return null;
            }

            if (other is Border otherBorder) {
                Border typedOther = otherBorder;
                if (!BorderSide.canMerge(typedOther.top, top) ||
                    !BorderSide.canMerge(typedOther.bottom, bottom))
                    return null;
                if (start != BorderSide.none ||
                    end != BorderSide.none) {
                    if (typedOther.left != BorderSide.none ||
                        typedOther.right != BorderSide.none)
                        return null;
                    D.assert(typedOther.left == BorderSide.none);
                    D.assert(typedOther.right == BorderSide.none);
                    return new BorderDirectional(
                        top: BorderSide.merge(typedOther.top, top),
                        start: start,
                        end: end,
                        bottom: BorderSide.merge(typedOther.bottom, bottom)
                    );
                }

                D.assert(start == BorderSide.none);
                D.assert(end == BorderSide.none);
                return new Border(
                    top: BorderSide.merge(typedOther.top, top),
                    right: typedOther.right,
                    bottom: BorderSide.merge(typedOther.bottom, bottom),
                    left: typedOther.left
                );
            }

            return null;
        }

        public override ShapeBorder scale(float t) {
            return new BorderDirectional(
                top: top.scale(t),
                start: start.scale(t),
                end: end.scale(t),
                bottom: bottom.scale(t)
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is BorderDirectional borderDirectional)
                return BorderDirectional.lerp(borderDirectional, this, t);
            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is BorderDirectional borderDirectional)
                return BorderDirectional.lerp(this, borderDirectional, t);
            return base.lerpTo(b, t);
        }

        public static BorderDirectional lerp(BorderDirectional a, BorderDirectional b, float t) {
            if (a == null && b == null)
                return null;
            if (a == null)
                return (BorderDirectional) b.scale(t);
            if (b == null)
                return (BorderDirectional) a.scale(1.0f - t);
            return new BorderDirectional(
                top: BorderSide.lerp(a.top, b.top, t),
                end: BorderSide.lerp(a.end, b.end, t),
                bottom: BorderSide.lerp(a.bottom, b.bottom, t),
                start: BorderSide.lerp(a.start, b.start, t)
            );
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection) {
            paint(canvas, rect, textDirection);
        }

        public override void paint(
            Canvas canvas,
            Rect rect,
            TextDirection? textDirection = null,
            BoxShape shape = BoxShape.rectangle,
            BorderRadius borderRadius = null
        ) {
            if (isUniform) {
                switch (top.style) {
                    case BorderStyle.none:
                        return;
                    case BorderStyle.solid:
                        switch (shape) {
                            case BoxShape.circle:
                                D.assert(borderRadius == null,
                                    () => "A borderRadius can only be given for rectangular boxes.");
                                _paintUniformBorderWithCircle(canvas, rect, top);
                                break;
                            case BoxShape.rectangle:
                                if (borderRadius != null) {
                                    _paintUniformBorderWithRadius(canvas, rect, top, borderRadius);
                                    return;
                                }

                                _paintUniformBorderWithRectangle(canvas, rect, top);
                                break;
                        }

                        return;
                }
            }

            D.assert(borderRadius == null, () => "A borderRadius can only be given for uniform borders.");
            D.assert(shape == BoxShape.rectangle, () => "A border can only be drawn as a circle if it is uniform.");
            BorderSide left = null, right = null;

            D.assert(textDirection != null,
                () => "Non-uniform BorderDirectional objects require a TextDirection when painting.");
            switch (textDirection) {
                case TextDirection.rtl:
                    left = end;
                    right = start;
                    break;
                case TextDirection.ltr:
                    left = start;
                    right = end;
                    break;
            }

            BorderUtils.paintBorder(canvas, rect, top: top, left: left, bottom: bottom, right: right);
        }


        public override string ToString() {
            List<string> arguments = new List<string>();
            if
                (top != BorderSide.none) {
                arguments.Add("top: $top");
            }

            if (start != BorderSide.none) {
                arguments.Add("start: $start");
            }

            if (end != BorderSide.none) {
                arguments.Add("end: $end");
            }

            if (bottom != BorderSide.none) {
                arguments.Add("bottom: $bottom");
            }

            return $"{foundation_.objectRuntimeType(this, "BorderDirectional")}({String.Join(", ", arguments)}";
        }

        public bool Equals(BorderDirectional other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(start, other.start) && Equals(end, other.end) && Equals(top, other.top) &&
                   Equals(bottom, other.bottom);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((BorderDirectional) obj);
        }

        public static bool operator ==(BorderDirectional left, object right) {
            if (left is null) {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(BorderDirectional left, object right) {
            if (left is null) {
                return !(right is null);
            }

            return !left.Equals(right);
        }


        public override int GetHashCode() {
            unchecked {
                var hashCode = (start != null ? start.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (end != null ? end.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (top != null ? top.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (bottom != null ? bottom.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}