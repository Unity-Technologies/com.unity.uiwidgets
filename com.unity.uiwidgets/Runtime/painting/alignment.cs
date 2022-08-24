using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public abstract class AlignmentGeometry : IEquatable<AlignmentGeometry> {
        public AlignmentGeometry() {
        }

        protected virtual float _x { get; }

        protected virtual float _start { get; }

        protected virtual float _y { get; }

        public virtual AlignmentGeometry add(AlignmentGeometry other) {
            return new _MixedAlignment(
                _x + other._x,
                _start + other._start,
                _y + other._y
            );
        }

        public abstract AlignmentGeometry SelfMinus();

        public static AlignmentGeometry operator -(AlignmentGeometry self) {
            return self.SelfMinus();
        }

        public abstract AlignmentGeometry Multiply(float other);

        public static AlignmentGeometry operator *(AlignmentGeometry self, float other) {
            return self.Multiply(other);
        }

        public abstract AlignmentGeometry Divide(float other);

        public static AlignmentGeometry operator /(AlignmentGeometry self, float other) {
            return self.Divide(other);
        }


        public abstract AlignmentGeometry Remainder(float other);

        public static AlignmentGeometry operator %(AlignmentGeometry self, float other) {
            return self.Remainder(other);
        }

        public static AlignmentGeometry lerp(AlignmentGeometry a, AlignmentGeometry b, float t) {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b * t;
            if (b == null)
                return a * (1.0f - t);
            if (a is Alignment _a && b is Alignment _b)
                return Alignment.lerpAlignment(_a, _b, t);
            if (a is AlignmentDirectional _ad && b is AlignmentDirectional _bd)
                return AlignmentDirectional.lerpAlignmentDirectional(_ad, _bd, t);
            return new _MixedAlignment(
                MathUtils.lerpNullableFloat(a._x, b._x, t),
                MathUtils.lerpNullableFloat(a._start, b._start, t),
                MathUtils.lerpNullableFloat(a._y, b._y, t)
            );
        }

        public abstract Alignment resolve(TextDirection? direction);

        public override string ToString() {
            if (_start == 0.0)
                return Alignment._stringify(_x, _y);
            if (_x == 0.0)
                return AlignmentDirectional._stringify(_start, _y);
            return Alignment._stringify(_x, _y) + " + " + AlignmentDirectional._stringify(_start, 0.0);
        }

        public bool Equals(AlignmentGeometry other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return _x.Equals(other._x) && _start.Equals(other._start) && _y.Equals(other._y);
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

            return Equals((AlignmentGeometry) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _x.GetHashCode();
                hashCode = (hashCode * 397) ^ _start.GetHashCode();
                hashCode = (hashCode * 397) ^ _y.GetHashCode();
                return hashCode;
            }
        }
    }

    public class Alignment : AlignmentGeometry, IEquatable<Alignment> {
        public Alignment(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public readonly float x;

        public readonly float y;

        public static readonly Alignment topLeft = new Alignment(-1.0f, -1.0f);
        public static readonly Alignment topCenter = new Alignment(0, -1.0f);
        public static readonly Alignment topRight = new Alignment(1.0f, -1.0f);
        public static readonly Alignment centerLeft = new Alignment(-1.0f, 0.0f);
        public static readonly Alignment center = new Alignment(0.0f, 0.0f);
        public static readonly Alignment centerRight = new Alignment(1.0f, 0.0f);
        public static readonly Alignment bottomLeft = new Alignment(-1.0f, 1.0f);
        public static readonly Alignment bottomCenter = new Alignment(0.0f, 1.0f);
        public static readonly Alignment bottomRight = new Alignment(1.0f, 1.0f);

        public static Alignment operator -(Alignment a, Alignment b) {
            return new Alignment(a.x - b.x, a.y - b.y);
        }

        public static Alignment operator +(Alignment a, Alignment b) {
            return new Alignment(a.x + b.x, a.y + b.y);
        }

        public override AlignmentGeometry SelfMinus() {
            return new Alignment(-x, -y);
        }

        public override AlignmentGeometry Multiply(float other) {
            return new Alignment(x * other, y * other);
        }

        public override AlignmentGeometry Divide(float other) {
            return new Alignment(x / other, y / other);
        }

        public override AlignmentGeometry Remainder(float other) {
            return new Alignment(x % other, y % other);
        }

        public Offset alongOffset(Offset other) {
            float centerX = other.dx / 2.0f;
            float centerY = other.dy / 2.0f;
            return new Offset(centerX + x * centerX, centerY + y * centerY);
        }

        public Offset alongSize(Size other) {
            float centerX = other.width / 2.0f;
            float centerY = other.height / 2.0f;
            return new Offset(centerX + x * centerX, centerY + y * centerY);
        }

        public Offset withinRect(Rect rect) {
            float halfWidth = rect.width / 2.0f;
            float halfHeight = rect.height / 2.0f;
            return new Offset(
                rect.left + halfWidth + x * halfWidth,
                rect.top + halfHeight + y * halfHeight
            );
        }

        public Rect inscribe(Size size, Rect rect) {
            float halfWidthDelta = (rect.width - size.width) / 2.0f;
            float halfHeightDelta = (rect.height - size.height) / 2.0f;
            return Rect.fromLTWH(
                rect.left + halfWidthDelta + x * halfWidthDelta,
                rect.top + halfHeightDelta + y * halfHeightDelta,
                size.width,
                size.height
            );
        }

        public static Alignment lerpAlignment(Alignment a, Alignment b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return new Alignment(MathUtils.lerpNullableFloat(0.0f, b.x, t), MathUtils.lerpNullableFloat(0.0f, b.y, t));
            }

            if (b == null) {
                return new Alignment(MathUtils.lerpNullableFloat(a.x, 0.0f, t), MathUtils.lerpNullableFloat(a.y, 0.0f, t));
            }

            return new Alignment(MathUtils.lerpNullableFloat(a.x, b.x, t), MathUtils.lerpNullableFloat(a.y, b.y, t));
        }

        public bool Equals(Alignment other) {
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

            return Equals((Alignment) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ y.GetHashCode();
            }
        }

        public static bool operator ==(Alignment a, Alignment b) {
            return Equals(a, b);
        }

        public static bool operator !=(Alignment a, Alignment b) {
            return !(a == b);
        }

        public override string ToString() {
            if (x == -1.0f && y == -1.0f) {
                return "topLeft";
            }

            if (x == 0.0f && y == -1.0f) {
                return "topCenter";
            }

            if (x == 1.0f && y == -1.0f) {
                return "topRight";
            }

            if (x == -1.0f && y == 0.0f) {
                return "centerLeft";
            }

            if (x == 0.0f && y == 0.0f) {
                return "center";
            }

            if (x == 1.0f && y == 0.0f) {
                return "centerRight";
            }

            if (x == -1.0f && y == 1.0f) {
                return "bottomLeft";
            }

            if (x == 0.0f && y == 1.0f) {
                return "bottomCenter";
            }

            if (x == 1.0f && y == 1.0f) {
                return "bottomRight";
            }

            return $"Alignment({x:F1}, {y:F1})";
        }

        public override Alignment resolve(TextDirection? direction) {
            return this;
        }


        internal static string _stringify(float x, float y) {
            if (x == -1.0f && y == -1.0f)
                return "topLeft";
            if (x == 0.0f && y == -1.0f)
                return "topCenter";
            if (x == 1.0f && y == -1.0f)
                return "topRight";
            if (x == -1.0f && y == 0.0f)
                return "centerLeft";
            if (x == 0.0f && y == 0.0f)
                return "center";
            if (x == 1.0f && y == 0.0f)
                return "centerRight";
            if (x == -1.0f && y == 1.0f)
                return "bottomLeft";
            if (x == 0.0f && y == 1.0f)
                return "bottomCenter";
            if (x == 1.0f && y == 1.0f)
                return "bottomRight";
            return $"Alignment({x}, " +
                   $"{y})";
        }
    }

    public class AlignmentDirectional : AlignmentGeometry {
        public AlignmentDirectional(float start, float y) {
            this.start = start;
            this.y = y;
        }

        readonly float _px;

        protected override float _x {
            get => 0;
        }

        readonly float start;

        protected override float _start {
            get => start;
        }

        readonly float y;

        protected override float _y {
            get => y;
        }

        public static readonly AlignmentDirectional topStart = new AlignmentDirectional(-1.0f, -1.0f);

        public static readonly AlignmentDirectional topCenter = new AlignmentDirectional(0.0f, -1.0f);

        public static readonly AlignmentDirectional topEnd = new AlignmentDirectional(1.0f, -1.0f);

        public static readonly AlignmentDirectional centerStart = new AlignmentDirectional(-1.0f, 0.0f);

        public static readonly AlignmentDirectional center = new AlignmentDirectional(0.0f, 0.0f);

        public static readonly AlignmentDirectional centerEnd = new AlignmentDirectional(1.0f, 0.0f);

        public static readonly AlignmentDirectional bottomStart = new AlignmentDirectional(-1.0f, 1.0f);

        public static readonly AlignmentDirectional bottomCenter = new AlignmentDirectional(0.0f, 1.0f);

        public static readonly AlignmentDirectional bottomEnd = new AlignmentDirectional(1.0f, 1.0f);

        public override AlignmentGeometry add(AlignmentGeometry other) {
            if (other is AlignmentDirectional alignmentDirectional)
                return this + alignmentDirectional;
            return base.add(other);
        }

        public static AlignmentDirectional operator -(AlignmentDirectional self, AlignmentDirectional other) {
            return new AlignmentDirectional(self.start - other.start, self.y - other.y);
        }

        public static AlignmentDirectional operator +(AlignmentDirectional self, AlignmentDirectional other) {
            return new AlignmentDirectional(self.start + other.start, self.y + other.y);
        }

        public override AlignmentGeometry SelfMinus() {
            return new AlignmentDirectional(-start, -y);
        }

        public override AlignmentGeometry Multiply(float other) {
            return new AlignmentDirectional(start * other, y * other);
        }

        public override AlignmentGeometry Divide(float other) {
            return new AlignmentDirectional(start / other, y / other);
        }

        public override AlignmentGeometry Remainder(float other) {
            return new AlignmentDirectional(start % other, y % other);
        }

        public static AlignmentDirectional lerpAlignmentDirectional(AlignmentDirectional a, AlignmentDirectional b, float t) {
            if (a == null && b == null)
                return null;
            if (a == null)
                return new AlignmentDirectional(MathUtils.lerpNullableFloat(0.0f, b.start, t),
                    MathUtils.lerpNullableFloat(0.0f, b.y, t));
            if (b == null)
                return new AlignmentDirectional(MathUtils.lerpNullableFloat(a.start, 0.0f, t),
                    MathUtils.lerpNullableFloat(a.y, 0.0f, t));
            return new AlignmentDirectional(MathUtils.lerpNullableFloat(a.start, b.start, t), MathUtils.lerpNullableFloat(a.y, b.y, t));
        }

        public override Alignment resolve(TextDirection? direction) {
            D.assert(direction != null);
            switch (direction) {
                case TextDirection.rtl:
                    return new Alignment(-start, y);
                case TextDirection.ltr:
                    return new Alignment(start, y);
            }

            return null;
        }

        internal static string _stringify(double start, double y) {
            if (start == -1.0f && y == -1.0f)
                return "AlignmentDirectional.topStart";
            if (start == 0.0f && y == -1.0f)
                return "AlignmentDirectional.topCenter";
            if (start == 1.0f && y == -1.0f)
                return "AlignmentDirectional.topEnd";
            if (start == -1.0f && y == 0.0f)
                return "AlignmentDirectional.centerStart";
            if (start == 0.0f && y == 0.0f)
                return "AlignmentDirectional.center";
            if (start == 1.0f && y == 0.0f)
                return "AlignmentDirectional.centerEnd";
            if (start == -1.0f && y == 1.0f)
                return "AlignmentDirectional.bottomStart";
            if (start == 0.0f && y == 1.0f)
                return "AlignmentDirectional.bottomCenter";
            if (start == 1.0f && y == 1.0f)
                return "AlignmentDirectional.bottomEnd";
            return $"AlignmentDirectional({start}, {y})";
        }

        public override string ToString() => _stringify(start, y);
    }


    class _MixedAlignment : AlignmentGeometry {
        internal _MixedAlignment(float _x, float _start, float _y) {
            _px = _x;
            _pstart = _start;
            _py = _y;
        }

        readonly float _px;

        protected override float _x {
            get => _px;
        }

        readonly float _pstart;

        protected override float _start {
            get => _pstart;
        }

        readonly float _py;

        protected override float _y {
            get => _py;
        }

        public override AlignmentGeometry SelfMinus() {
            return new _MixedAlignment(
                -_x,
                -_start,
                -_y
            );
        }

        public override AlignmentGeometry Multiply(float other) {
            return new _MixedAlignment(
                _x * other,
                _start * other,
                _y * other
            );
        }

        public override AlignmentGeometry Divide(float other) {
            return new _MixedAlignment(
                _x / other,
                _start / other,
                _y / other
            );
        }

        public override AlignmentGeometry Remainder(float other) {
            return new _MixedAlignment(
                _x % other,
                _start % other,
                _y % other
            );
        }

        public override Alignment resolve(TextDirection? direction) {
            D.assert(direction != null);
            switch (direction) {
                case TextDirection.rtl:
                    return new Alignment(_x - _start, _y);
                case TextDirection.ltr:
                    return new Alignment(_x + _start, _y);
            }

            return null;
        }
    }

    public class TextAlignVertical {
        public TextAlignVertical(float y) {
            D.assert(y >= -1.0 && y <= 1.0);
            this.y = y;
        }

        public readonly float y;

        public static readonly TextAlignVertical top = new TextAlignVertical(y: -1.0f);

        public static readonly TextAlignVertical center = new TextAlignVertical(y: 0.0f);

        public static readonly TextAlignVertical bottom = new TextAlignVertical(y: 1.0f);

        public override string ToString() {
            return $"{foundation_.objectRuntimeType(this, "TextAlignVertical")}(y: {y})";
        }
    }
}