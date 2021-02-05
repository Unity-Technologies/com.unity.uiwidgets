using System;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public class RoundedRectangleBorder : ShapeBorder, IEquatable<RoundedRectangleBorder> {
        public RoundedRectangleBorder(
            BorderSide side = null,
            BorderRadius borderRadius = null
        ) {
            this.side = side ?? BorderSide.none;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
        }

        public readonly BorderSide side;

        public readonly BorderRadius borderRadius;


        public override EdgeInsetsGeometry dimensions {
            get { return EdgeInsets.all(side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new RoundedRectangleBorder(
                side: side.scale(t),
                borderRadius: (BorderRadius) (borderRadius * t)
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is RoundedRectangleBorder border) {
                return new RoundedRectangleBorder(
                    side: BorderSide.lerp(border.side, side, t),
                    borderRadius: BorderRadius.lerp(border.borderRadius, borderRadius, t)
                );
            }

            if (a is CircleBorder circleBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(circleBorder.side, side, t),
                    borderRadius: borderRadius,
                    circleness: 1.0f - t
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is RoundedRectangleBorder border) {
                return new RoundedRectangleBorder(
                    side: BorderSide.lerp(side, border.side, t),
                    borderRadius: BorderRadius.lerp(borderRadius, border.borderRadius, t)
                );
            }

            if (b is CircleBorder circleBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(side, circleBorder.side, t),
                    borderRadius: borderRadius,
                    circleness: t
                );
            }

            return base.lerpTo(b, t);
        }

        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addRRect(borderRadius.resolve(textDirection).toRRect(rect).deflate(side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addRRect(borderRadius.resolve(textDirection).toRRect(rect));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection = null) {
            switch (side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    float width = side.width;
                    if (width == 0.0) {
                        canvas.drawRRect(borderRadius.resolve(textDirection).toRRect(rect), side.toPaint());
                    }
                    else {
                        RRect outer = borderRadius.resolve(textDirection).toRRect(rect);
                        RRect inner = outer.deflate(width);
                        Paint paint = new Paint {
                            color = side.color,
                        };
                        canvas.drawDRRect(outer, inner, paint);
                    }

                    break;
            }
        }

        public bool Equals(RoundedRectangleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(side, other.side) && Equals(borderRadius, other.borderRadius);
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

            return Equals((RoundedRectangleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((side != null ? side.GetHashCode() : 0) * 397) ^
                       (borderRadius != null ? borderRadius.GetHashCode() : 0);
            }
        }

        public static bool operator ==(RoundedRectangleBorder left, RoundedRectangleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(RoundedRectangleBorder left, RoundedRectangleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{GetType()}({side}, {borderRadius})";
        }
    }

    class _RoundedRectangleToCircleBorder : ShapeBorder, IEquatable<_RoundedRectangleToCircleBorder> {
        public _RoundedRectangleToCircleBorder(
            BorderSide side = null,
            BorderRadius borderRadius = null,
            float circleness = 0.0f
        ) {
            this.side = side ?? BorderSide.none;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
            this.circleness = circleness;
        }

        public readonly BorderSide side;

        public readonly BorderRadius borderRadius;

        public readonly float circleness;

        public override EdgeInsetsGeometry dimensions {
            get { return EdgeInsets.all(side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new _RoundedRectangleToCircleBorder(
                side: side.scale(t),
                borderRadius: (BorderRadius) (borderRadius * t),
                circleness: t
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is RoundedRectangleBorder rectBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(rectBorder.side, side, t),
                    borderRadius: BorderRadius.lerp(rectBorder.borderRadius, borderRadius, t),
                    circleness: circleness * t
                );
            }

            if (a is CircleBorder circleBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(circleBorder.side, side, t),
                    borderRadius: borderRadius,
                    circleness: circleness + (1.0f - circleness) * (1.0f - t)
                );
            }

            if (a is _RoundedRectangleToCircleBorder border) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(border.side, side, t),
                    borderRadius: BorderRadius.lerp(border.borderRadius, borderRadius, t),
                    circleness: MathUtils.lerpNullableFloat(border.circleness, circleness, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is RoundedRectangleBorder rectBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(side, rectBorder.side, t),
                    borderRadius: BorderRadius.lerp(borderRadius, rectBorder.borderRadius, t),
                    circleness: circleness * (1.0f - t)
                );
            }

            if (b is CircleBorder circleBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(side, circleBorder.side, t),
                    borderRadius: borderRadius,
                    circleness: circleness + (1.0f - circleness) * t
                );
            }

            if (b is _RoundedRectangleToCircleBorder border) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(side, border.side, t),
                    borderRadius: BorderRadius.lerp(borderRadius, border.borderRadius, t),
                    circleness: MathUtils.lerpNullableFloat(circleness, border.circleness, t)
                );
            }

            return base.lerpTo(b, t);
        }

        Rect _adjustRect(Rect rect) {
            if (circleness == 0.0 || rect.width == rect.height) {
                return rect;
            }

            if (rect.width < rect.height) {
                float delta = circleness * (rect.height - rect.width) / 2.0f;
                return Rect.fromLTRB(
                    rect.left,
                    rect.top + delta,
                    rect.right,
                    rect.bottom - delta
                );
            }
            else {
                float delta = circleness * (rect.width - rect.height) / 2.0f;
                return Rect.fromLTRB(
                    rect.left + delta,
                    rect.top,
                    rect.right - delta,
                    rect.bottom
                );
            }
        }

        BorderRadius _adjustBorderRadius(Rect rect, TextDirection? textDirection) {
            BorderRadius resolvedRadius = borderRadius.resolve(textDirection);
            if (circleness == 0.0f) {
                return resolvedRadius;
            }

            return BorderRadius.lerp(resolvedRadius, BorderRadius.circular(rect.shortestSide / 2.0f), circleness);
        }

        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addRRect(_adjustBorderRadius(rect, textDirection).toRRect(_adjustRect(rect)).deflate(side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addRRect(_adjustBorderRadius(rect, textDirection).toRRect(_adjustRect(rect)));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection = null) {
            switch (side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    float width = side.width;
                    if (width == 0.0) {
                        canvas.drawRRect(_adjustBorderRadius(rect, textDirection).toRRect(_adjustRect(rect)),
                            side.toPaint());
                    }
                    else {
                        RRect outer = _adjustBorderRadius(rect, textDirection).toRRect(_adjustRect(rect));
                        RRect inner = outer.deflate(width);
                        Paint paint = new Paint {
                            color = side.color,
                        };
                        canvas.drawDRRect(outer, inner, paint);
                    }

                    break;
            }
        }

        public bool Equals(_RoundedRectangleToCircleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(side, other.side) && Equals(borderRadius, other.borderRadius) &&
                   circleness.Equals(other.circleness);
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

            return Equals((_RoundedRectangleToCircleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (side != null ? side.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (borderRadius != null ? borderRadius.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ circleness.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(_RoundedRectangleToCircleBorder left, _RoundedRectangleToCircleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(_RoundedRectangleToCircleBorder left, _RoundedRectangleToCircleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"RoundedRectangleBorder({side}, {borderRadius}, " +
                   $"{circleness * 100:F1}% of the way to being a CircleBorder)";
        }
    }
}