using System;
using UnityEngine;

namespace Unity.UIWidgets.ui{
    public static class MathUtils {
        const float _valueNearlyZero = 1f / (1 << 12);

        public static bool isConvexPolygon(Offset[] polygonVerts, int polygonSize) {
            if (polygonSize < 3) {
                return false;
            }

            float lastArea = 0;
            float lastPerpDot = 0;

            int prevIndex = polygonSize - 1;
            int currIndex = 0;
            int nextIndex = 1;

            Offset origin = polygonVerts[0];
            Vector2 v0 = (polygonVerts[currIndex] - polygonVerts[prevIndex]).toVector();
            Vector2 v1 = (polygonVerts[nextIndex] - polygonVerts[currIndex]).toVector();
            Vector2 w0 = (polygonVerts[currIndex] - origin).toVector();
            Vector2 w1 = (polygonVerts[nextIndex] - origin).toVector();

            for (int i = 0; i < polygonSize; i++) {
                if (!polygonVerts[i].isFinite) {
                    return false;
                }

                float perpDot = v0.cross(v1);
                if (lastPerpDot * perpDot < 0) {
                    return false;
                }

                if (0 != perpDot) {
                    lastPerpDot = perpDot;
                }

                float quadArea = w0.cross(w1);
                if (quadArea * lastArea < 0) {
                    return false;
                }

                if (0 != quadArea) {
                    lastArea = quadArea;
                }

                prevIndex = currIndex;
                currIndex = nextIndex;
                nextIndex = (currIndex + 1) % polygonSize;
                v0 = v1;
                v1 = (polygonVerts[nextIndex] - polygonVerts[currIndex]).toVector();
                w0 = w1;
                w1 = (polygonVerts[nextIndex] - origin).toVector();
            }

            return true;
        }

        public static float cross(this Vector2 vector1, Vector2 vector2) {
            return Vector3.Cross(new Vector3(vector1.x, vector1.y, 0f), new Vector3(vector2.x, vector2.y, 0f)).z;
        }

        public static bool valueNearlyZero(this float x, float? tolerance = null) {
            tolerance = tolerance ?? _valueNearlyZero;
            return Mathf.Abs(x) <= tolerance;
        }

        public static float clamp(this float value, float min, float max) {
            if (value < min) {
                value = min;
            }
            else if (value > max) {
                value = max;
            }

            return value;
        }

        public static int clamp(this int value, int min, int max) {
            if (value < min) {
                value = min;
            }
            else if (value > max) {
                value = max;
            }

            return value;
        }

        public static int abs(this int value) {
            return Mathf.Abs(value);
        }

        public static float abs(this float value) {
            return Mathf.Abs(value);
        }

        public static int sign(this float value) {
            return value == 0.0f ? 0 : value > 0.0f ? 1 : -1;
        }

        public static bool isInfinite(this float it) {
            return float.IsInfinity(it);
        }

        public static bool isFinite(this float it) {
            return !float.IsInfinity(it);
        }

        public static bool isNaN(this float it) {
            return float.IsNaN(it);
        }

        public static float? lerpNullableFloat(float? a, float? b, float t) {
            if (a == null && b == null) {
                return null;
            }

            a = a ?? b;
            b = b ?? a;
            return a + (b - a) * t;
        }
        
        public static float lerpNullableFloat(float a, float b, float t) {
            return a + (b - a) * t;
        }

        public static int round(this float value) {
            return Mathf.RoundToInt(value);
        }

        public static int floor(this float value) {
            return Mathf.FloorToInt(value);
        }

        public static int ceil(this float value) {
            return Mathf.CeilToInt(value);
        }
    }

    public abstract class OffsetBase : IEquatable<OffsetBase> {
        protected OffsetBase(float _dx, float _dy) {
            this._dx = _dx;
            this._dy = _dy;
        }

        protected readonly float _dx;

        protected readonly float _dy;

        public bool isInfinite {
            get { return float.IsInfinity(_dx) || float.IsInfinity(_dy); }
        }

        public bool isFinite {
            get { return !float.IsInfinity(_dx) && !float.IsInfinity(_dy); }
        }

        public static bool operator <(OffsetBase a, OffsetBase b) {
            return a._dx < b._dx && a._dy < b._dy;
        }

        public static bool operator <=(OffsetBase a, OffsetBase b) {
            return a._dx <= b._dx && a._dy <= b._dy;
        }

        public static bool operator >(OffsetBase a, OffsetBase b) {
            return a._dx > b._dx && a._dy > b._dy;
        }

        public static bool operator >=(OffsetBase a, OffsetBase b) {
            return a._dx >= b._dx && a._dy >= b._dy;
        }

        public bool Equals(OffsetBase other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return _dx.Equals(other._dx) && _dy.Equals(other._dy);
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

            return Equals((OffsetBase) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (_dx.GetHashCode() * 397) ^ _dy.GetHashCode();
            }
        }

        public static bool operator ==(OffsetBase left, OffsetBase right) {
            return Equals(left, right);
        }

        public static bool operator !=(OffsetBase left, OffsetBase right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{GetType()}({_dx:F1}, {_dy:F1})";
        }
    }

    public class Offset : OffsetBase, IEquatable<Offset> {
        public Offset(float dx, float dy) : base(dx, dy) {
        }

        public float dx {
            get { return _dx; }
        }

        public float dy {
            get { return _dy; }
        }

        public float distance {
            get { return Mathf.Sqrt(_dx * _dx + _dy * _dy); }
        }

        public float distanceSquared {
            get { return _dx * _dx + _dy * _dy; }
        }

        public static readonly Offset zero = new Offset(0.0f, 0.0f);
        public static readonly Offset infinite = new Offset(float.PositiveInfinity, float.PositiveInfinity);

        public Offset scale(float scaleX, float? scaleY = null) {
            scaleY = scaleY ?? scaleX;
            return new Offset(dx * scaleX, dy * scaleY.Value);
        }

        public Offset translate(float translateX, float translateY) {
            return new Offset(dx + translateX, dy + translateY);
        }

        public static Offset operator -(Offset a) {
            return new Offset(-a.dx, -a.dy);
        }

        public static Offset operator -(Offset a, Offset b) {
            if (a == null) {
                return new Offset(-b.dx, -b.dy);
            
            }
            else if (b == null) {
                return new Offset(-a.dx, -a.dy);
            }
            return new Offset(a.dx - b.dx, a.dy - b.dy);
        }

        public static Offset operator +(Offset a, Offset b) {
            if (a == null) {
                return b;
            }
            else if (b == null) {
                return a;
            }

            return new Offset(a.dx + b.dx, a.dy + b.dy);
        }

        public static Offset operator *(Offset a, float operand) {
            return new Offset(a.dx * operand, a.dy * operand);
        }

        public static Offset operator /(Offset a, float operand) {
            return new Offset(a.dx / operand, a.dy / operand);
        }

        public static Rect operator &(Offset a, Size other) {
            return Rect.fromLTWH(a.dx, a.dy, other.width, other.height);
        }

        public static Offset lerp(Offset a, Offset b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0f - t);
            }

            return new Offset(MathUtils.lerpNullableFloat(a.dx, b.dx, t), MathUtils.lerpNullableFloat(a.dy, b.dy, t));
        }

        public bool Equals(Offset other) {
            return base.Equals(other);
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

            return Equals((Offset) obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator ==(Offset left, Offset right) {
            return Equals(left, right);
        }

        public static bool operator !=(Offset left, Offset right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Offset({_dx:F1}, {_dy:F1})";
        }
    }

    public class Size : OffsetBase, IEquatable<Size> {
        public Size(float width, float height) : base(width, height) {
        }

        public static Size copy(Size source) {
            return new Size(source.width, source.height);
        }

        public static Size square(float dimension) {
            return new Size(dimension, dimension);
        }

        public static Size fromWidth(float width) {
            return new Size(width, float.PositiveInfinity);
        }

        public static Size fromHeight(float height) {
            return new Size(float.PositiveInfinity, height);
        }

        public static Size fromRadius(float radius) {
            return new Size(radius * 2, radius * 2);
        }

        public float width {
            get { return _dx; }
        }

        public float height {
            get { return _dy; }
        }

        public static readonly Size zero = new Size(0.0f, 0.0f);

        public static readonly Size infinite = new Size(float.PositiveInfinity, float.PositiveInfinity);

        public bool isEmpty {
            get { return width <= 0.0 || height <= 0.0; }
        }

        public static Size operator -(Size a, Offset b) {
            return new Size(a.width - b.dx, a.height - b.dy);
        }

        public static Size operator +(Size a, Offset b) {
            return new Size(a.width + b.dx, a.height + b.dy);
        }

        public static Offset operator -(Size a, Size b) {
            return new Offset(a.width - b.width, a.height - b.height);
        }

        public static Size operator *(Size a, float operand) {
            return new Size(a.width * operand, a.height * operand);
        }

        public static Size operator /(Size a, float operand) {
            return new Size(a.width / operand, a.height / operand);
        }

        public float shortestSide {
            get { return Mathf.Min(width.abs(), height.abs()); }
        }

        public float longestSide {
            get { return Mathf.Max(width.abs(), height.abs()); }
        }

        public Offset topLeft(Offset origin) {
            return origin;
        }

        public Offset topCenter(Offset origin) {
            return new Offset(origin.dx + width / 2.0f, origin.dy);
        }

        public Offset topRight(Offset origin) {
            return new Offset(origin.dx + width, origin.dy);
        }

        public Offset centerLeft(Offset origin) {
            return new Offset(origin.dx, origin.dy + height / 2.0f);
        }

        public Offset center(Offset origin) {
            return new Offset(origin.dx + width / 2.0f, origin.dy + height / 2.0f);
        }

        public Offset centerRight(Offset origin) {
            return new Offset(origin.dx + width, origin.dy + height / 2.0f);
        }

        public Offset bottomLeft(Offset origin) {
            return new Offset(origin.dx, origin.dy + height);
        }

        public Offset bottomCenter(Offset origin) {
            return new Offset(origin.dx + width / 2.0f, origin.dy + height);
        }

        public Offset bottomRight(Offset origin) {
            return new Offset(origin.dx + width, origin.dy + height);
        }

        public bool contains(Offset offset) {
            return offset.dx >= 0.0 && offset.dx < width && offset.dy >= 0.0 && offset.dy < height;
        }

        public Size flipped {
            get { return new Size(height, width); }
        }

        public static Size lerp(Size a, Size b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0f - t);
            }

            return new Size(MathUtils.lerpNullableFloat(a.width, b.width, t), MathUtils.lerpNullableFloat(a.height, b.height, t));
        }

        public bool Equals(Size other) {
            return base.Equals(other);
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

            return Equals((Size) obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator ==(Size left, Size right) {
            return Equals(left, right);
        }

        public static bool operator !=(Size left, Size right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Size({_dx:F1}, {_dy:F1})";
        }
    }

    public class Rect : IEquatable<Rect> {
        Rect(float left, float top, float right, float bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static Rect fromLTRB(float left, float top, float right, float bottom) {
            return new Rect(left, top, right, bottom);
        }

        public static Rect fromLTWH(float left, float top, float width, float height) {
            return new Rect(left, top, left + width, top + height);
        }

        public static Rect fromCircle(Offset center, float radius) {
            return new Rect(center.dx - radius, center.dy - radius, center.dx + radius, center.dy + radius);
        }
        public static Rect fromCenter( Offset center, float width, float height ) {
            return new Rect(
                center.dx - width / 2,
                center.dy - height / 2,
                center.dx + width / 2,
                center.dy + height / 2
            );
        }

        public static Rect fromPoints(Offset a, Offset b) {
            return new Rect(
                Mathf.Min(a.dx, b.dx),
                Mathf.Min(a.dy, b.dy),
                Mathf.Max(a.dx, b.dx),
                Mathf.Max(a.dy, b.dy)
            );
        }

        internal float[] _value32 => new[] {left, top, right, bottom};

        public readonly float left;
        public readonly float top;
        public readonly float right;
        public readonly float bottom;

        public float width {
            get { return right - left; }
        }

        public float height {
            get { return bottom - top; }
        }

        public Size size {
            get { return new Size(width, height); }
        }

        public bool hasNaN => left.isNaN() || top.isNaN() || right.isNaN() || bottom.isNaN();

        public float area {
            get { return width * height; }
        }

        public float margin {
            get { return width + height; }
        }

        public static readonly Rect zero = new Rect(0, 0, 0, 0);

        public static readonly Rect one = new Rect(0, 0, 1, 1);

        public static readonly Rect infinity = new Rect(float.NegativeInfinity, float.NegativeInfinity,
            float.PositiveInfinity, float.PositiveInfinity);

        public const float _giantScalar = 1.0E+9f;

        public static readonly Rect largest =
            fromLTRB(-_giantScalar, -_giantScalar, _giantScalar, _giantScalar);

        public bool isInfinite {
            get {
                return float.IsInfinity(left)
                       || float.IsInfinity(top)
                       || float.IsInfinity(right)
                       || float.IsInfinity(bottom);
            }
        }

        public bool isFinite {
            get { return !isInfinite; }
        }

        public bool isEmpty {
            get { return left >= right || top >= bottom; }
        }

        public Rect shift(Offset offset) {
            return fromLTRB(left + offset.dx, top + offset.dy, right + offset.dx,
                bottom + offset.dy);
        }

        public Rect translate(float translateX, float translateY) {
            return fromLTRB(left + translateX, top + translateY, right + translateX,
                bottom + translateY);
        }

        public Rect scale(float scaleX, float? scaleY = null) {
            scaleY = scaleY ?? scaleX;
            return fromLTRB(
                left * scaleX, top * scaleY.Value,
                right * scaleX, bottom * scaleY.Value);
        }

        public Rect outset(float dx, float dy) {
            return new Rect(left - dx, top - dy, right + dx, bottom + dy);
        }

        public Offset[] toQuad() {
            Offset[] dst = new Offset[4];
            dst[0] = new Offset(left, top);
            dst[1] = new Offset(right, top);
            dst[2] = new Offset(right, bottom);
            dst[3] = new Offset(left, bottom);
            return dst;
        }


        public Rect inflate(float delta) {
            return fromLTRB(left - delta, top - delta, right + delta, bottom + delta);
        }

        public Rect deflate(float delta) {
            return inflate(-delta);
        }

        public Rect intersect(Rect other) {
            return fromLTRB(
                Mathf.Max(left, other.left),
                Mathf.Max(top, other.top),
                Mathf.Min(right, other.right),
                Mathf.Min(bottom, other.bottom)
            );
        }

        public Rect expandToInclude(Rect other) {
            if (isEmpty) {
                return other;
            }

            if (other == null || other.isEmpty) {
                return this;
            }

            return fromLTRB(
                Mathf.Min(left, other.left),
                Mathf.Min(top, other.top),
                Mathf.Max(right, other.right),
                Mathf.Max(bottom, other.bottom)
            );
        }

        public bool overlaps(Rect other) {
            if (right <= other.left || other.right <= left) {
                return false;
            }

            if (bottom <= other.top || other.bottom <= top) {
                return false;
            }

            return true;
        }

        public float shortestSide {
            get { return Mathf.Min(Mathf.Abs(width), Mathf.Abs(height)); }
        }

        public float longestSide {
            get { return Mathf.Max(Mathf.Abs(width), Mathf.Abs(height)); }
        }

        public Offset topLeft {
            get { return new Offset(left, top); }
        }

        public Offset topCenter {
            get { return new Offset(left + width / 2.0f, top); }
        }

        public Offset topRight {
            get { return new Offset(right, top); }
        }

        public Offset centerLeft {
            get { return new Offset(left, top + height / 2.0f); }
        }

        public Offset center {
            get { return new Offset(left + width / 2.0f, top + height / 2.0f); }
        }

        public Offset centerRight {
            get { return new Offset(right, bottom); }
        }

        public Offset bottomLeft {
            get { return new Offset(left, bottom); }
        }

        public Offset bottomCenter {
            get { return new Offset(left + width / 2.0f, bottom); }
        }

        public Offset bottomRight {
            get { return new Offset(right, bottom); }
        }

        public bool contains(Offset offset) {
            return offset.dx >= left && offset.dx < right && offset.dy >= top && offset.dy < bottom;
        }

        public bool containsInclusive(Offset offset) {
            return offset.dx >= left && offset.dx <= right && offset.dy >= top &&
                   offset.dy <= bottom;
        }

        public bool contains(Rect rect) {
            return contains(rect.topLeft) && contains(rect.bottomRight);
        }

        public Rect round() {
            return fromLTRB(
                Mathf.Round(left), Mathf.Round(top),
                Mathf.Round(right), Mathf.Round(bottom));
        }

        public Rect roundOut() {
            return fromLTRB(
                Mathf.Floor(left), Mathf.Floor(top),
                Mathf.Ceil(right), Mathf.Ceil(bottom));
        }

        public Rect roundOutScale(float scale) {
            return fromLTRB(
                Mathf.Floor(left * scale),
                Mathf.Floor(top * scale),
                Mathf.Ceil(right * scale),
                Mathf.Ceil(bottom * scale));
        }

        public Rect withDevicePixelRatio(float devicePixelRatio) {
            return fromLTRB(
                Mathf.Floor(left * devicePixelRatio) / devicePixelRatio,
                Mathf.Floor(top * devicePixelRatio) / devicePixelRatio,
                Mathf.Ceil(right * devicePixelRatio) / devicePixelRatio,
                Mathf.Ceil(bottom * devicePixelRatio) / devicePixelRatio);
        }

        public Rect roundIn() {
            return fromLTRB(
                Mathf.Ceil(left), Mathf.Ceil(top),
                Mathf.Floor(right), Mathf.Floor(bottom));
        }

        public Rect normalize() {
            if (left <= right && top <= bottom) {
                return this;
            }

            return fromLTRB(
                Mathf.Min(left, right),
                Mathf.Min(top, bottom),
                Mathf.Max(left, right),
                Mathf.Max(top, bottom)
            );
        }

        public static Rect lerp(Rect a, Rect b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return fromLTRB(b.left * t, b.top * t, b.right * t, b.bottom * t);
            }

            if (b == null) {
                float k = 1.0f - t;
                return fromLTRB(a.left * k, a.top * k, a.right * k, a.bottom * k);
            }

            return fromLTRB(
                MathUtils.lerpNullableFloat(a.left, b.left, t),
                MathUtils.lerpNullableFloat(a.top, b.top, t),
                MathUtils.lerpNullableFloat(a.right, b.right, t),
                MathUtils.lerpNullableFloat(a.bottom, b.bottom, t)
            );
        }

        public bool Equals(Rect other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return left.Equals(other.left) && top.Equals(other.top) && right.Equals(other.right) &&
                   bottom.Equals(other.bottom);
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

            return Equals((Rect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = left.GetHashCode();
                hashCode = (hashCode * 397) ^ top.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ bottom.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Rect left, Rect right) {
            return Equals(left, right);
        }

        public static bool operator !=(Rect left, Rect right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return "Rect.fromLTRB(" + left.ToString("0.0") + ", " + top.ToString("0.0") + ", " +
                   right.ToString("0.0") + ", " + bottom.ToString("0.0") + ")";
        }
    }

    public class Radius : IEquatable<Radius> {
        Radius(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public static Radius circular(float radius) {
            return elliptical(radius, radius);
        }

        public static Radius elliptical(float x, float y) {
            return new Radius(x, y);
        }

        public readonly float x;
        public readonly float y;

        public static readonly Radius zero = circular(0.0f);

        public static Radius operator -(Radius a) {
            return elliptical(-a.x, -a.y);
        }

        public static Radius operator -(Radius a, Radius b) {
            return elliptical(a.x - b.x, a.y - b.y);
        }

        public static Radius operator -(Radius a, float b) {
            return elliptical(a.x - b, a.y - b);
        }

        public static Radius operator +(Radius a, Radius b) {
            return elliptical(a.x + b.x, a.y + b.y);
        }

        public static Radius operator +(Radius a, float b) {
            return elliptical(a.x + b, a.y + b);
        }

        public static Radius operator *(Radius a, Radius b) {
            return elliptical(a.x * b.x, a.y * b.y);
        }

        public static Radius operator *(Radius a, float b) {
            return elliptical(a.x * b, a.y * b);
        }

        public static Radius operator /(Radius a, Radius b) {
            return elliptical(a.x / b.x, a.y / b.y);
        }

        public static Radius operator /(Radius a, float b) {
            return elliptical(a.x / b, a.y / b);
        }

        public static Radius operator %(Radius a, Radius b) {
            return elliptical(a.x % b.x, a.y % b.y);
        }

        public static Radius operator %(Radius a, float b) {
            return elliptical(a.x % b, a.y % b);
        }

        public static Radius lerp(Radius a, Radius b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return elliptical(b.x * t, b.y * t);
            }

            if (b == null) {
                float k = 1.0f - t;
                return elliptical(a.x * k, a.y * k);
            }

            return elliptical(
                MathUtils.lerpNullableFloat(a.x, b.x, t),
                MathUtils.lerpNullableFloat(a.y, b.y, t)
            );
        }

        public bool Equals(Radius other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return x.Equals(other.x) && y.Equals(other.y);
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

            return Equals((Radius) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ y.GetHashCode();
            }
        }

        public static bool operator ==(Radius left, Radius right) {
            return Equals(left, right);
        }

        public static bool operator !=(Radius left, Radius right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return x == y
                ? $"Radius.circular({x:F1})"
                : $"Radius.elliptical({x:F1}, ${y:F1})";
        }
    }

    public class RRect : IEquatable<RRect> {
        RRect(float left, float top, float right, float bottom,
            Radius tlRadius = null, Radius trRadius = null, Radius brRadius = null, Radius blRadius = null) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.tlRadius = tlRadius ?? Radius.zero;
            this.trRadius = trRadius ?? Radius.zero;
            this.brRadius = brRadius ?? Radius.zero;
            this.blRadius = blRadius ?? Radius.zero;
        }

        RRect(float left, float top, float right, float bottom,
            float? tlRadius = null, float? trRadius = null, float? brRadius = null, float? blRadius = null) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.tlRadius = tlRadius != null ? Radius.circular(tlRadius.Value) : Radius.zero;
            this.trRadius = trRadius != null ? Radius.circular(trRadius.Value) : Radius.zero;
            this.brRadius = brRadius != null ? Radius.circular(brRadius.Value) : Radius.zero;
            this.blRadius = blRadius != null ? Radius.circular(blRadius.Value) : Radius.zero;
        }


        public static RRect fromLTRBXY(
            float left, float top, float right, float bottom,
            float radiusX, float radiusY) {
            var radius = Radius.elliptical(radiusX, radiusY);
            return new RRect(left, top, right, bottom,
                radius, radius, radius, radius);
        }


        public static RRect fromLTRBR(
            float left, float top, float right, float bottom, Radius radius) {
            return new RRect(left, top, right, bottom,
                radius, radius, radius, radius);
        }

        public static RRect fromLTRBR(
            float left, float top, float right, float bottom, float radius) {
            var r = Radius.circular(radius);
            return new RRect(left, top, right, bottom,
                r, r, r, r);
        }

        public static RRect fromRectXY(Rect rect, float radiusX, float radiusY) {
            var radius = Radius.elliptical(radiusX, radiusY);
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                radius, radius, radius, radius);
        }

        public static RRect fromRect(Rect rect) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom, (Radius) null);
        }

        public static RRect fromRectAndRadius(Rect rect, Radius radius) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                radius, radius, radius, radius);
        }

        public static RRect fromRectAndRadius(Rect rect, float radius) {
            var r = Radius.circular(radius);
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                r, r, r, r);
        }

        public static RRect fromLTRBAndCorners(
            float left, float top, float right, float bottom,
            Radius topLeft = null, Radius topRight = null, Radius bottomRight = null, Radius bottomLeft = null) {
            return new RRect(left, top, right, bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public static RRect fromLTRBAndCorners(
            float left, float top, float right, float bottom,
            float? topLeft = null, float? topRight = null, float? bottomRight = null, float? bottomLeft = null) {
            return new RRect(left, top, right, bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public static RRect fromRectAndCorners(
            Rect rect,
            Radius topLeft = null, Radius topRight = null, Radius bottomRight = null, Radius bottomLeft = null) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public static RRect fromRectAndCorners(
            Rect rect,
            float? topLeft = null, float? topRight = null, float? bottomRight = null, float? bottomLeft = null) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        internal float[] _value32 => new[] {
            left,
            top,
            right,
            bottom,
            tlRadiusX,
            tlRadiusY,
            trRadiusX,
            trRadiusY,
            brRadiusX,
            brRadiusY,
            blRadiusX,
            blRadiusY,
        };

        public readonly float left;
        public readonly float top;
        public readonly float right;
        public readonly float bottom;

        public readonly Radius tlRadius;
        public readonly Radius trRadius;
        public readonly Radius brRadius;
        public readonly Radius blRadius;

        public float tlRadiusX {
            get { return tlRadius.x; }
        }

        public float tlRadiusY {
            get { return tlRadius.y; }
        }

        public float trRadiusX {
            get { return trRadius.x; }
        }

        public float trRadiusY {
            get { return trRadius.y; }
        }

        public float blRadiusX {
            get { return blRadius.x; }
        }

        public float blRadiusY {
            get { return blRadius.y; }
        }

        public float brRadiusX {
            get { return brRadius.x; }
        }

        public float brRadiusY {
            get { return brRadius.y; }
        }

        public static readonly RRect zero = new RRect(0, 0, 0, 0, (Radius) null);

        public RRect shift(Offset offset) {
            return fromLTRBAndCorners(
                left + offset.dx,
                top + offset.dy,
                right + offset.dx,
                bottom + offset.dy,
                tlRadius,
                trRadius,
                brRadius,
                blRadius
            );
        }

        public RRect inflate(float delta) {
            return fromLTRBAndCorners(
                left - delta,
                top - delta,
                right + delta,
                bottom + delta,
                tlRadius + delta,
                trRadius + delta,
                brRadius + delta,
                blRadius + delta
            );
        }

        public RRect deflate(float delta) {
            return inflate(-delta);
        }

        public float width {
            get { return right - left; }
        }

        public float height {
            get { return bottom - top; }
        }

        public Rect outerRect {
            get { return Rect.fromLTRB(left, top, right, bottom); }
        }

        public Rect safeInnerRect {
            get {
                const float kInsetFactor = 0.29289321881f; // 1-cos(pi/4)

                float leftRadius = Mathf.Max(blRadiusX, tlRadiusX);
                float topRadius = Mathf.Max(tlRadiusY, trRadiusY);
                float rightRadius = Mathf.Max(trRadiusX, brRadiusX);
                float bottomRadius = Mathf.Max(brRadiusY, blRadiusY);

                return Rect.fromLTRB(
                    left + leftRadius * kInsetFactor,
                    top + topRadius * kInsetFactor,
                    right - rightRadius * kInsetFactor,
                    bottom - bottomRadius * kInsetFactor
                );
            }
        }

        public Rect middleRect {
            get {
                float leftRadius = Mathf.Max(blRadiusX, tlRadiusX);
                float topRadius = Mathf.Max(tlRadiusY, trRadiusY);
                float rightRadius = Mathf.Max(trRadiusX, brRadiusX);
                float bottomRadius = Mathf.Max(brRadiusY, blRadiusY);

                return Rect.fromLTRB(
                    left + leftRadius,
                    top + topRadius,
                    right - rightRadius,
                    bottom - bottomRadius
                );
            }
        }

        public Rect wideMiddleRect {
            get {
                float topRadius = Mathf.Max(tlRadiusY, trRadiusY);
                float bottomRadius = Mathf.Max(brRadiusY, blRadiusY);

                return Rect.fromLTRB(
                    left,
                    top + topRadius,
                    right,
                    bottom - bottomRadius
                );
            }
        }

        public Rect tallMiddleRect {
            get {
                float leftRadius = Mathf.Max(blRadiusX, tlRadiusX);
                float rightRadius = Mathf.Max(trRadiusX, brRadiusX);

                return Rect.fromLTRB(
                    left + leftRadius,
                    top,
                    right - rightRadius,
                    bottom
                );
            }
        }

        public bool isEmpty {
            get { return left >= right || top >= bottom; }
        }

        public bool isFinite {
            get {
                return left.isFinite()
                       && top.isFinite()
                       && right.isFinite()
                       && bottom.isFinite();
            }
        }

        public bool isInfinite {
            get { return !isFinite; }
        }

        public bool isRect {
            get {
                return tlRadius == Radius.zero &&
                       trRadius == Radius.zero &&
                       blRadius == Radius.zero &&
                       brRadius == Radius.zero;
            }
        }

        public bool isStadium {
            get {
                return tlRadius == trRadius
                       && trRadius == brRadius
                       && brRadius == blRadius
                       && (width <= 2.0 * tlRadiusX || height <= 2.0 * tlRadiusY);
            }
        }

        public bool isEllipse {
            get {
                return tlRadius == trRadius
                       && trRadius == brRadius
                       && brRadius == blRadius
                       && (width <= 2.0 * tlRadiusX && height <= 2.0 * tlRadiusY);
            }
        }

        public bool isCircle {
            get { return width == height && isEllipse; }
        }

        public float shortestSide {
            get { return Mathf.Min(width.abs(), height.abs()); }
        }

        public float longestSide {
            get { return Mathf.Max(width.abs(), height.abs()); }
        }

        public bool hasNaN => left.isNaN() || top.isNaN() || right.isNaN() || bottom.isNaN() ||
                              trRadiusX.isNaN() || trRadiusY.isNaN() || tlRadiusX.isNaN() || tlRadiusY.isNaN() ||
                              brRadiusX.isNaN() || brRadiusY.isNaN() || blRadiusX.isNaN() || blRadiusY.isNaN();


        public Offset center {
            get { return new Offset(left + width / 2.0f, top + height / 2.0f); }
        }

        float _getMin(float min, float radius1, float radius2, float limit) {
            float sum = radius1 + radius2;
            if (sum > limit && sum != 0.0) {
                return Mathf.Min(min, limit / sum);
            }

            return min;
        }

        RRect _scaled;
        
        public RRect scaleRadii() {
            float scale = 1.0f;
            scale = _getMin(scale, blRadiusY, tlRadiusY, height);
            scale = _getMin(scale, tlRadiusX, trRadiusX, width);
            scale = _getMin(scale, trRadiusY, brRadiusY, height);
            scale = _getMin(scale, brRadiusX, blRadiusX, width);

            if (scale < 1.0) {
                return fromLTRBAndCorners(
                    top: top,
                    left: left,
                    right: right,
                    bottom: bottom,
                    topLeft: tlRadius * scale,
                    topRight: trRadius * scale,
                    bottomLeft: blRadius * scale,
                    bottomRight: brRadius * scale
                );
            }

            return fromLTRBAndCorners(
                top: top,
                left: left,
                right: right,
                bottom: bottom,
                topLeft: tlRadius,
                topRight: trRadius,
                bottomLeft: blRadius,
                bottomRight: brRadius
            );
        }

        internal void _scaleRadii() {
            if (_scaled == null) {
                float scale = 1.0f;

                scale = _getMin(scale, blRadiusY, tlRadiusY, height);
                scale = _getMin(scale, tlRadiusX, trRadiusX, width);
                scale = _getMin(scale, trRadiusY, brRadiusY, height);
                scale = _getMin(scale, brRadiusX, blRadiusX, width);

                if (scale < 1.0) {
                    _scaled = fromLTRBAndCorners(
                        left: left, top: top, right: right, bottom: bottom,
                        topLeft: tlRadius * scale, topRight: trRadius * scale,
                        bottomRight: brRadius * scale, bottomLeft: blRadius * scale);
                }
                else {
                    _scaled = this;
                }
            }
        }

        public bool contains(Offset point) {
            if (point.dx < left || point.dx >= right || point.dy < top || point.dy >= bottom) {
                return false;
            }

            _scaleRadii();

            float x;
            float y;
            float radiusX;
            float radiusY;

            if (point.dx < left + _scaled.tlRadiusX &&
                point.dy < top + _scaled.tlRadiusY) {
                x = point.dx - left - _scaled.tlRadiusX;
                y = point.dy - top - _scaled.tlRadiusY;
                radiusX = _scaled.tlRadiusX;
                radiusY = _scaled.tlRadiusY;
            }
            else if (point.dx > right - _scaled.trRadiusX &&
                     point.dy < top + _scaled.trRadiusY) {
                x = point.dx - right + _scaled.trRadiusX;
                y = point.dy - top - _scaled.trRadiusY;
                radiusX = _scaled.trRadiusX;
                radiusY = _scaled.trRadiusY;
            }
            else if (point.dx > right - _scaled.brRadiusX &&
                     point.dy > bottom - _scaled.brRadiusY) {
                x = point.dx - right + _scaled.brRadiusX;
                y = point.dy - bottom + _scaled.brRadiusY;
                radiusX = _scaled.brRadiusX;
                radiusY = _scaled.brRadiusY;
            }
            else if (point.dx < left + _scaled.blRadiusX &&
                     point.dy > bottom - _scaled.blRadiusY) {
                x = point.dx - left - _scaled.blRadiusX;
                y = point.dy - bottom + _scaled.blRadiusY;
                radiusX = _scaled.blRadiusX;
                radiusY = _scaled.blRadiusY;
            }
            else {
                return true;
            }

            x = x / radiusX;
            y = y / radiusY;
            if (x * x + y * y > 1.0) {
                return false;
            }

            return true;
        }

        public static RRect lerp(RRect a, RRect b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return fromLTRBAndCorners(
                    b.left * t,
                    b.top * t,
                    b.right * t,
                    b.bottom * t,
                    b.tlRadius * t,
                    b.trRadius * t,
                    b.brRadius * t,
                    b.blRadius * t
                );
            }

            if (b == null) {
                float k = 1.0f - t;
                return fromLTRBAndCorners(
                    a.left * k,
                    a.top * k,
                    a.right * k,
                    a.bottom * k,
                    a.tlRadius * k,
                    a.trRadius * k,
                    a.brRadius * k,
                    a.blRadius * k);
            }

            return fromLTRBAndCorners(
                MathUtils.lerpNullableFloat(a.left, b.left, t),
                MathUtils.lerpNullableFloat(a.top, b.top, t),
                MathUtils.lerpNullableFloat(a.right, b.right, t),
                MathUtils.lerpNullableFloat(a.bottom, b.bottom, t),
                Radius.lerp(a.tlRadius, b.tlRadius, t),
                Radius.lerp(a.trRadius, b.trRadius, t),
                Radius.lerp(a.brRadius, b.brRadius, t),
                Radius.lerp(a.blRadius, b.blRadius, t));
        }

        public bool contains(Rect rect) {
            if (!outerRect.contains(rect)) {
                return false;
            }

            if (isRect) {
                return true;
            }

            return contains(rect.topLeft) &&
                   contains(rect.topRight) &&
                   contains(rect.bottomRight) &&
                   contains(rect.bottomLeft);
        }

        public bool Equals(RRect other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return left.Equals(other.left)
                   && top.Equals(other.top)
                   && right.Equals(other.right)
                   && bottom.Equals(other.bottom)
                   && tlRadius.Equals(other.tlRadius)
                   && trRadius.Equals(other.trRadius)
                   && brRadius.Equals(other.brRadius)
                   && blRadius.Equals(other.blRadius);
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

            return Equals((RRect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = left.GetHashCode();
                hashCode = (hashCode * 397) ^ top.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ tlRadius.GetHashCode();
                hashCode = (hashCode * 397) ^ trRadius.GetHashCode();
                hashCode = (hashCode * 397) ^ brRadius.GetHashCode();
                hashCode = (hashCode * 397) ^ blRadius.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RRect a, RRect b) {
            return Equals(a, b);
        }

        public static bool operator !=(RRect a, RRect b) {
            return !(a == b);
        }

        public override string ToString() {
            string rect = $"{left:F1)}, " +
                          $"{top:F1}, " +
                          $"{right:F1}, " +
                          $"{bottom:F1}";

            if (tlRadius == trRadius &&
                trRadius == brRadius &&
                brRadius == blRadius) {
                if (tlRadius.x == tlRadius.y) {
                    return $"RRect.fromLTRBR({rect}, {tlRadius.x:F1})";
                }

                return $"RRect.fromLTRBXY($rect, {tlRadius.x:F1}, {tlRadius.y:F1})";
            }

            return "RRect.fromLTRBAndCorners(" +
                   $"{rect}, " +
                   $"topLeft: {tlRadius}, " +
                   $"topRight: {trRadius}, " +
                   $"bottomRight: {brRadius}, " +
                   $"bottomLeft: {blRadius}" +
                   ")";
        }
    }

    public class RSTransform {
        RSTransform(float scos, float ssin, float tx, float ty) {
            _value[0] = scos;
            _value[1] = ssin;
            _value[2] = tx;
            _value[3] = ty;
        }

        public static RSTransform fromComponents(
            float rotation,
            float scale,
            float anchorX,
            float anchorY,
            float translateX,
            float translateY
        ) {
            float scos = Mathf.Cos(rotation) * scale;
            float ssin = Mathf.Sin(rotation) * scale;
            float tx = translateX + -scos * anchorX + ssin * anchorY;
            float ty = translateY + -ssin * anchorX - scos * anchorY;
            return new RSTransform(scos, ssin, tx, ty);
        }

        float[] _value = new float[4];

        public float scos => _value[0];

        public float ssin => _value[1];

        public float tx => _value[2];

        public float ty => _value[3];
    }
}