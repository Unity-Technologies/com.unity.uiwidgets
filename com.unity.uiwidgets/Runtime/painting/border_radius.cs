using System;
using System.Text;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class BorderRadius : IEquatable<BorderRadius> {
        BorderRadius(
            Radius topLeft,
            Radius topRight,
            Radius bottomRight,
            Radius bottomLeft) {
            this.topLeft = topLeft ?? Radius.zero;
            this.topRight = topRight ?? Radius.zero;
            this.bottomRight = bottomRight ?? Radius.zero;
            this.bottomLeft = bottomLeft ?? Radius.zero;
        }

        public static BorderRadius all(Radius radius) {
            return only(radius, radius, radius, radius);
        }

        public static BorderRadius all(float radius) {
            return only(radius, radius, radius, radius);
        }

        public static BorderRadius circular(float radius) {
            return all(Radius.circular(radius));
        }

        public static BorderRadius vertical(Radius top = null, Radius bottom = null) {
            return only(top, top, bottom, bottom);
        }

        public static BorderRadius vertical(float? top = null, float? bottom = null) {
            return only(top, top, bottom, bottom);
        }

        public static BorderRadius horizontal(Radius left = null, Radius right = null) {
            return only(left, right, right, left);
        }

        public static BorderRadius horizontal(float? left = null, float? right = null) {
            return only(left, right, right, left);
        }

        public static BorderRadius only(
            Radius topLeft = null, Radius topRight = null,
            Radius bottomRight = null, Radius bottomLeft = null) {
            return new BorderRadius(topLeft, topRight, bottomRight, bottomLeft);
        }

        public static BorderRadius only(
            float? topLeft = null, float? topRight = null,
            float? bottomRight = null, float? bottomLeft = null) {
            var tlRadius = topLeft != null ? Radius.circular(topLeft.Value) : null;
            var trRadius = topRight != null ? Radius.circular(topRight.Value) : null;
            var brRadius = bottomRight != null ? Radius.circular(bottomRight.Value) : null;
            var blRadius = bottomLeft != null ? Radius.circular(bottomLeft.Value) : null;

            return new BorderRadius(tlRadius, trRadius, brRadius, blRadius);
        }

        public static readonly BorderRadius zero = all(Radius.zero);

        public readonly Radius topLeft;
        public readonly Radius topRight;
        public readonly Radius bottomRight;
        public readonly Radius bottomLeft;

        public RRect toRRect(Rect rect) {
            return RRect.fromRectAndCorners(
                rect,
                topLeft: topLeft,
                topRight: topRight,
                bottomRight: bottomRight,
                bottomLeft: bottomLeft
            );
        }

        public static BorderRadius operator -(BorderRadius it, BorderRadius other) {
            return only(
                topLeft: it.topLeft - other.topLeft,
                topRight: it.topRight - other.topRight,
                bottomLeft: it.bottomLeft - other.bottomLeft,
                bottomRight: it.bottomRight - other.bottomRight
            );
        }

        public static BorderRadius operator +(BorderRadius it, BorderRadius other) {
            return only(
                topLeft: it.topLeft + other.topLeft,
                topRight: it.topRight + other.topRight,
                bottomLeft: it.bottomLeft + other.bottomLeft,
                bottomRight: it.bottomRight + other.bottomRight
            );
        }

        public static BorderRadius operator -(BorderRadius it) {
            return only(
                topLeft: -it.topLeft,
                topRight: -it.topRight,
                bottomLeft: -it.bottomLeft,
                bottomRight: -it.bottomRight
            );
        }

        public static BorderRadius operator *(BorderRadius it, float other) {
            return only(
                topLeft: it.topLeft * other,
                topRight: it.topRight * other,
                bottomLeft: it.bottomLeft * other,
                bottomRight: it.bottomRight * other
            );
        }

        public static BorderRadius operator /(BorderRadius it, float other) {
            return only(
                topLeft: it.topLeft / other,
                topRight: it.topRight / other,
                bottomLeft: it.bottomLeft / other,
                bottomRight: it.bottomRight / other
            );
        }

        public static BorderRadius operator %(BorderRadius it, float other) {
            return only(
                topLeft: it.topLeft % other,
                topRight: it.topRight % other,
                bottomLeft: it.bottomLeft % other,
                bottomRight: it.bottomRight % other
            );
        }

        public static BorderRadius lerp(BorderRadius a, BorderRadius b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0f - t);
            }

            return only(
                topLeft: Radius.lerp(a.topLeft, b.topLeft, t),
                topRight: Radius.lerp(a.topRight, b.topRight, t),
                bottomLeft: Radius.lerp(a.bottomLeft, b.bottomLeft, t),
                bottomRight: Radius.lerp(a.bottomRight, b.bottomRight, t)
            );
        }

        public bool Equals(BorderRadius other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return topLeft.Equals(other.topLeft)
                   && topRight.Equals(other.topRight)
                   && bottomRight.Equals(other.bottomRight)
                   && bottomLeft.Equals(other.bottomLeft);
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

            return Equals((BorderRadius) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = topLeft.GetHashCode();
                hashCode = (hashCode * 397) ^ topRight.GetHashCode();
                hashCode = (hashCode * 397) ^ bottomRight.GetHashCode();
                hashCode = (hashCode * 397) ^ bottomLeft.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BorderRadius a, BorderRadius b) {
            return Equals(a, b);
        }

        public static bool operator !=(BorderRadius a, BorderRadius b) {
            return !Equals(a, b);
        }

        public override string ToString() {
            string visual = null;
            if (topLeft == topRight &&
                topRight == bottomLeft &&
                bottomLeft == bottomRight) {
                if (topLeft != Radius.zero) {
                    if (topLeft.x == topLeft.y) {
                        visual = $"BorderRadius.circular({topLeft.x:F1})";
                    }
                    else {
                        visual = $"BorderRadius.all({topLeft})";
                    }
                }
            }
            else {
                var result = new StringBuilder();
                result.Append("BorderRadius.only(");
                bool comma = false;
                if (topLeft != Radius.zero) {
                    result.Append($"topLeft: {topLeft}");
                    comma = true;
                }

                if (topRight != Radius.zero) {
                    if (comma) {
                        result.Append(", ");
                    }

                    result.Append($"topRight: {topRight}");
                    comma = true;
                }

                if (bottomLeft != Radius.zero) {
                    if (comma) {
                        result.Append(", ");
                    }

                    result.Append($"bottomLeft: {bottomLeft}");
                    comma = true;
                }

                if (bottomRight != Radius.zero) {
                    if (comma) {
                        result.Append(", ");
                    }

                    result.Append($"bottomRight: {bottomRight}");
                }

                result.Append(")");
                visual = result.ToString();
            }

            if (visual != null) {
                return visual;
            }

            return "BorderRadius.zero";
        }
    }
}