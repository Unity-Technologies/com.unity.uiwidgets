using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class EdgeInsets : IEquatable<EdgeInsets> {
        EdgeInsets(float left, float top, float right, float bottom) {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public readonly float left;
        public readonly float right;
        public readonly float top;
        public readonly float bottom;

        public bool isNonNegative {
            get {
                return left >= 0.0
                       && right >= 0.0
                       && top >= 0.0
                       && bottom >= 0.0;
            }
        }

        public float horizontal {
            get { return left + right; }
        }

        public float vertical {
            get { return top + bottom; }
        }

        public float along(Axis axis) {
            switch (axis) {
                case Axis.horizontal:
                    return horizontal;
                case Axis.vertical:
                    return vertical;
            }

            throw new Exception("unknown axis");
        }

        public Size collapsedSize {
            get { return new Size(horizontal, vertical); }
        }

        public EdgeInsets flipped {
            get { return fromLTRB(right, bottom, left, top); }
        }

        public Size inflateSize(Size size) {
            return new Size(size.width + horizontal, size.height + vertical);
        }

        public Size deflateSize(Size size) {
            return new Size(size.width - horizontal, size.height - vertical);
        }

        public static EdgeInsets fromLTRB(float left, float top, float right, float bottom) {
            return new EdgeInsets(left, top, right, bottom);
        }

        public static EdgeInsets all(float value) {
            return new EdgeInsets(value, value, value, value);
        }

        public static EdgeInsets only(float left = 0.0f, float top = 0.0f, float right = 0.0f, float bottom = 0.0f) {
            return new EdgeInsets(left, top, right, bottom);
        }

        public static EdgeInsets symmetric(float vertical = 0.0f, float horizontal = 0.0f) {
            return new EdgeInsets(horizontal, vertical, horizontal, vertical);
        }

        public static EdgeInsets fromWindowPadding(WindowPadding padding, float devicePixelRatio) {
            return new EdgeInsets(
                left: padding.left / devicePixelRatio,
                top: padding.top / devicePixelRatio,
                right: padding.right / devicePixelRatio,
                bottom: padding.bottom / devicePixelRatio
            );
        }

        public static readonly EdgeInsets zero = only();

        public Offset topLeft {
            get { return new Offset(left, top); }
        }

        public Offset topRight {
            get { return new Offset(-right, top); }
        }

        public Offset bottomLeft {
            get { return new Offset(left, -bottom); }
        }

        public Offset bottomRight {
            get { return new Offset(-right, -bottom); }
        }

        public Rect inflateRect(Rect rect) {
            return Rect.fromLTRB(
                rect.left - left, rect.top - top,
                rect.right + right, rect.bottom + bottom);
        }

        public Rect deflateRect(Rect rect) {
            return Rect.fromLTRB(
                rect.left + left, rect.top + top,
                rect.right - right, rect.bottom - bottom);
        }

        public EdgeInsets subtract(EdgeInsets other) {
            return fromLTRB(
                left - other.left,
                top - other.top,
                right - other.right,
                bottom - other.bottom
            );
        }

        public EdgeInsets add(EdgeInsets other) {
            return fromLTRB(
                left + other.left,
                top + other.top,
                right + other.right,
                bottom + other.bottom
            );
        }

        public static EdgeInsets operator -(EdgeInsets a, EdgeInsets b) {
            return fromLTRB(
                a.left - b.left,
                a.top - b.top,
                a.right - b.right,
                a.bottom - b.bottom
            );
        }

        public static EdgeInsets operator +(EdgeInsets a, EdgeInsets b) {
            return fromLTRB(
                a.left + b.left,
                a.top + b.top,
                a.right + b.right,
                a.bottom + b.bottom
            );
        }

        public static EdgeInsets operator -(EdgeInsets a) {
            return fromLTRB(
                -a.left,
                -a.top,
                -a.right,
                -a.bottom
            );
        }

        public static EdgeInsets operator *(EdgeInsets a, float b) {
            return fromLTRB(
                a.left * b,
                a.top * b,
                a.right * b,
                a.bottom * b
            );
        }

        public static EdgeInsets operator /(EdgeInsets a, float b) {
            return fromLTRB(
                a.left / b,
                a.top / b,
                a.right / b,
                a.bottom / b
            );
        }

        public static EdgeInsets operator %(EdgeInsets a, float b) {
            return fromLTRB(
                a.left % b,
                a.top % b,
                a.right % b,
                a.bottom % b
            );
        }

        public static EdgeInsets lerp(EdgeInsets a, EdgeInsets b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0f - t);
            }

            return fromLTRB(
                MathUtils.lerpFloat(a.left, b.left, t),
                MathUtils.lerpFloat(a.top, b.top, t),
                MathUtils.lerpFloat(a.right, b.right, t),
                MathUtils.lerpFloat(a.bottom, b.bottom, t)
            );
        }

        public EdgeInsets copyWith(
            float? left = null,
            float? top = null,
            float? right = null,
            float? bottom = null
        ) {
            return only(
                left: left ?? this.left,
                top: top ?? this.top,
                right: right ?? this.right,
                bottom: bottom ?? this.bottom
            );
        }

        public bool Equals(EdgeInsets other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return left.Equals(other.left)
                   && right.Equals(other.right)
                   && top.Equals(other.top)
                   && bottom.Equals(other.bottom);
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

            return Equals((EdgeInsets) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = left.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ top.GetHashCode();
                hashCode = (hashCode * 397) ^ bottom.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(EdgeInsets a, EdgeInsets b) {
            return Equals(a, b);
        }

        public static bool operator !=(EdgeInsets a, EdgeInsets b) {
            return !(a == b);
        }

        public override string ToString() {
            if (left == 0.0 && right == 0.0 && top == 0.0 && bottom == 0.0) {
                return "EdgeInsets.zero";
            }

            if (left == right && right == top && top == bottom) {
                return $"EdgeInsets.all({left:F1})";
            }

            return $"EdgeInsets({left:F1}, " +
                   $"{top:F1}, " +
                   $"{right:F1}, " +
                   $"{bottom:F1})";
        }
    }
}