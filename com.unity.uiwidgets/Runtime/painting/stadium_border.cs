using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public class StadiumBorder : ShapeBorder, IEquatable<StadiumBorder> {
        public StadiumBorder(BorderSide side = null) {
            this.side = side ?? BorderSide.none;
        }

        public readonly BorderSide side;

        public override EdgeInsetsGeometry dimensions {
            get { return EdgeInsets.all(side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new StadiumBorder(side: side.scale(t));
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is StadiumBorder stadiumBorder) {
                return new StadiumBorder(side: BorderSide.lerp(stadiumBorder.side, side, t));
            }

            if (a is CircleBorder circleBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(circleBorder.side, side, t),
                    circleness: 1.0f - t
                );
            }

            if (a is RoundedRectangleBorder rectBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(rectBorder.side, side, t),
                    borderRadius: rectBorder.borderRadius,
                    rectness: 1.0f - t
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is StadiumBorder stadiumBorder) {
                return new StadiumBorder(side: BorderSide.lerp(side, stadiumBorder.side, t));
            }

            if (b is CircleBorder circleBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(side, circleBorder.side, t),
                    circleness: t
                );
            }

            if (b is RoundedRectangleBorder rectBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(side, rectBorder.side, t),
                    borderRadius: rectBorder.borderRadius,
                    rectness: t
                );
            }

            return base.lerpTo(b, t);
        }

        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null) {
            Radius radius = Radius.circular(rect.shortestSide / 2.0f);
            var path = new Path();
            path.addRRect(RRect.fromRectAndRadius(rect, radius).deflate(side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null) {
            Radius radius = Radius.circular(rect.shortestSide / 2.0f);
            var path = new Path();
            path.addRRect(RRect.fromRectAndRadius(rect, radius));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection = null) {
            switch (side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    Radius radius = Radius.circular(rect.shortestSide / 2.0f);
                    canvas.drawRRect(
                        RRect.fromRectAndRadius(rect, radius).deflate(side.width / 2.0f),
                        side.toPaint()
                    );
                    break;
            }
        }

        public bool Equals(StadiumBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(side, other.side);
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

            return Equals((StadiumBorder) obj);
        }

        public override int GetHashCode() {
            return (side != null ? side.GetHashCode() : 0);
        }

        public static bool operator ==(StadiumBorder left, StadiumBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(StadiumBorder left, StadiumBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{foundation_.objectRuntimeType(this, "StadiumBorder")}({side})";
        }
    }

    class _StadiumToCircleBorder : ShapeBorder, IEquatable<_StadiumToCircleBorder> {
        public _StadiumToCircleBorder(
            BorderSide side = null,
            float circleness = 0.0f
        ) {
            this.side = BorderSide.none;
            this.circleness = circleness;
        }

        public readonly BorderSide side;

        public readonly float circleness;

        public override EdgeInsetsGeometry dimensions {
            get { return EdgeInsets.all(side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new _StadiumToCircleBorder(
                side: side.scale(t),
                circleness: t
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is StadiumBorder stadiumBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(stadiumBorder.side, side, t),
                    circleness: circleness * t
                );
            }

            if (a is CircleBorder circleBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(circleBorder.side, side, t),
                    circleness: circleness + (1.0f - circleness) * (1.0f - t)
                );
            }

            if (a is _StadiumToCircleBorder border) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(border.side, side, t),
                    circleness: MathUtils.lerpNullableFloat(border.circleness, circleness, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is StadiumBorder stadiumBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(side, stadiumBorder.side, t),
                    circleness: circleness * (1.0f - t)
                );
            }

            if (b is CircleBorder circleBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(side, circleBorder.side, t),
                    circleness: circleness + (1.0f - circleness) * t
                );
            }

            if (b is _StadiumToCircleBorder border) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(side, border.side, t),
                    circleness: MathUtils.lerpNullableFloat(circleness, border.circleness, t)
                );
            }

            return base.lerpTo(b, t);
        }

        Rect _adjustRect(Rect rect) {
            if (circleness == 0.0f || rect.width == rect.height) {
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
                    (rect.left + delta),
                    rect.top,
                    (rect.right - delta),
                    rect.bottom
                );
            }
        }

        BorderRadius _adjustBorderRadius(Rect rect) {
            return BorderRadius.circular(rect.shortestSide / 2.0f);
        }

        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addRRect(_adjustBorderRadius(rect).toRRect(_adjustRect(rect)).deflate(side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addRRect(_adjustBorderRadius(rect).toRRect(_adjustRect(rect)));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection = null) {
            switch (side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    float width = side.width;
                    if (width == 0.0f) {
                        canvas.drawRRect(_adjustBorderRadius(rect).toRRect(_adjustRect(rect)),
                            side.toPaint());
                    }
                    else {
                        RRect outer = _adjustBorderRadius(rect).toRRect(_adjustRect(rect));
                        RRect inner = outer.deflate(width);
                        Paint paint = new Paint {
                            color = side.color,
                        };
                        canvas.drawDRRect(outer, inner, paint);
                    }

                    break;
            }
        }

        public bool Equals(_StadiumToCircleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(side, other.side) && circleness.Equals(other.circleness);
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

            return Equals((_StadiumToCircleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((side != null ? side.GetHashCode() : 0) * 397) ^ circleness.GetHashCode();
            }
        }

        public static bool operator ==(_StadiumToCircleBorder left, _StadiumToCircleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(_StadiumToCircleBorder left, _StadiumToCircleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"StadiumBorder({side}, {circleness * 100:F1}% " +
                   "of the way to being a CircleBorder)";
        }
    }

    class _StadiumToRoundedRectangleBorder : ShapeBorder, IEquatable<_StadiumToRoundedRectangleBorder> {
        public _StadiumToRoundedRectangleBorder(
            BorderSide side = null,
            BorderRadius borderRadius = null,
            float rectness = 0.0f
        ) {
            this.side = side ?? BorderSide.none;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
            this.rectness = rectness;
        }

        public readonly BorderSide side;

        public readonly BorderRadius borderRadius;

        public readonly float rectness;

        public override EdgeInsetsGeometry dimensions {
            get { return EdgeInsets.all(side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new _StadiumToRoundedRectangleBorder(
                side: side.scale(t),
                borderRadius: (BorderRadius) (borderRadius * t),
                rectness: t
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is StadiumBorder stadiumBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(stadiumBorder.side, side, t),
                    borderRadius: borderRadius,
                    rectness: rectness * t
                );
            }

            if (a is RoundedRectangleBorder rectBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(rectBorder.side, side, t),
                    borderRadius: borderRadius,
                    rectness: rectness + (1.0f - rectness) * (1.0f - t)
                );
            }

            if (a is _StadiumToRoundedRectangleBorder border) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(border.side, side, t),
                    borderRadius: BorderRadius.lerp(border.borderRadius, borderRadius, t),
                    rectness: MathUtils.lerpNullableFloat(border.rectness, rectness, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is StadiumBorder stadiumBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(side, stadiumBorder.side, t),
                    borderRadius: borderRadius,
                    rectness: rectness * (1.0f - t)
                );
            }

            if (b is RoundedRectangleBorder rectBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(side, rectBorder.side, t),
                    borderRadius: borderRadius,
                    rectness: rectness + (1.0f - rectness) * t
                );
            }

            if (b is _StadiumToRoundedRectangleBorder border) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(side, border.side, t),
                    borderRadius: BorderRadius.lerp(borderRadius, border.borderRadius, t),
                    rectness: MathUtils.lerpNullableFloat(rectness, border.rectness, t)
                );
            }

            return base.lerpTo(b, t);
        }

        BorderRadius _adjustBorderRadius(Rect rect) {
            return BorderRadius.lerp(
                borderRadius,
                BorderRadius.all(Radius.circular(rect.shortestSide / 2.0f)),
                1.0f - rectness
            );
        }

        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addRRect(_adjustBorderRadius(rect).toRRect(rect).deflate(side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addRRect(_adjustBorderRadius(rect).toRRect(rect));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection = null) {
            switch (side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    float width = side.width;
                    if (width == 0.0f) {
                        canvas.drawRRect(_adjustBorderRadius(rect).toRRect(rect), side.toPaint());
                    }
                    else {
                        RRect outer = _adjustBorderRadius(rect).toRRect(rect);
                        RRect inner = outer.deflate(width);
                        Paint paint = new Paint {
                            color = side.color,
                        };
                        canvas.drawDRRect(outer, inner, paint);
                    }

                    break;
            }
        }

        public bool Equals(_StadiumToRoundedRectangleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(side, other.side) && Equals(borderRadius, other.borderRadius) &&
                   rectness.Equals(other.rectness);
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

            return Equals((_StadiumToRoundedRectangleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (side != null ? side.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (borderRadius != null ? borderRadius.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ rectness.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(_StadiumToRoundedRectangleBorder left, _StadiumToRoundedRectangleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(_StadiumToRoundedRectangleBorder left, _StadiumToRoundedRectangleBorder right) {
            return !Equals(left, right);
        }


        public override string ToString() {
            return $"StadiumBorder({side}, {borderRadius}, " +
                   $"{rectness * 100:F1}% of the way to being a " +
                   "RoundedRectangleBorder)";
        }
    }
}