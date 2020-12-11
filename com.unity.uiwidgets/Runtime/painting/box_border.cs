using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public enum BoxShape {
        rectangle,
        circle,
    }

    public class Border : ShapeBorder, IEquatable<Border> {
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

        public readonly BorderSide top;
        public readonly BorderSide right;
        public readonly BorderSide bottom;
        public readonly BorderSide left;

        public override EdgeInsets dimensions {
            get {
                return EdgeInsets.fromLTRB(
                    left.width,
                    top.width,
                    right.width,
                    bottom.width);
            }
        }

        public bool isUniform {
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

        public override void paint(Canvas canvas, Rect rect) {
            paint(canvas, rect, BoxShape.rectangle, null);
        }

        public void paint(Canvas canvas, Rect rect,
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

        public override Path getInnerPath(Rect rect) {
            var path = new Path();
            path.addRect(dimensions.deflateRect(rect));
            return path;
        }

        public override Path getOuterPath(Rect rect) {
            var path = new Path();
            path.addRect(rect);
            return path;
        }

        static void _paintUniformBorderWithRadius(Canvas canvas, Rect rect, BorderSide side,
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

        static void _paintUniformBorderWithCircle(Canvas canvas, Rect rect, BorderSide side) {
            D.assert(side.style != BorderStyle.none);
            float width = side.width;
            Paint paint = side.toPaint();
            float radius = (rect.shortestSide - width) / 2.0f;
            canvas.drawCircle(rect.center, radius, paint);
        }

        static void _paintUniformBorderWithRectangle(Canvas canvas, Rect rect, BorderSide side) {
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
}