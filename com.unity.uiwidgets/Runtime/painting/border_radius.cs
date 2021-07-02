using System;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public abstract class BorderRadiusGeometry : IEquatable<BorderRadiusGeometry> {
        public BorderRadiusGeometry() {
        }

        protected virtual Radius _topLeft { get; }
        protected virtual Radius _topRight { get; }
        protected virtual Radius _bottomLeft { get; }
        protected virtual Radius _bottomRight { get; }
        protected virtual Radius _topStart { get; }
        protected virtual Radius _topEnd { get; }
        protected virtual Radius _bottomStart { get; }
        protected virtual Radius _bottomEnd { get; }

        public virtual BorderRadiusGeometry subtract(BorderRadiusGeometry other) {
            return new _MixedBorderRadius(
                _topLeft - other._topLeft,
                _topRight - other._topRight,
                _bottomLeft - other._bottomLeft,
                _bottomRight - other._bottomRight,
                _topStart - other._topStart,
                _topEnd - other._topEnd,
                _bottomStart - other._bottomStart,
                _bottomEnd - other._bottomEnd
            );
        }

        public virtual BorderRadiusGeometry add(BorderRadiusGeometry other) {
            return new _MixedBorderRadius(
                _topLeft + other._topLeft,
                _topRight + other._topRight,
                _bottomLeft + other._bottomLeft,
                _bottomRight + other._bottomRight,
                _topStart + other._topStart,
                _topEnd + other._topEnd,
                _bottomStart + other._bottomStart,
                _bottomEnd + other._bottomEnd
            );
        }

        public abstract BorderRadiusGeometry SelfMinus();

        public static BorderRadiusGeometry operator -(BorderRadiusGeometry other) {
            return other.SelfMinus();
        }

        public abstract BorderRadiusGeometry Multiply(float other);

        public static BorderRadiusGeometry operator *(BorderRadiusGeometry self, float other) {
            return self.Multiply(other);
        }

        public abstract BorderRadiusGeometry Divide(float other);

        public static BorderRadiusGeometry operator /(BorderRadiusGeometry self, float other) {
            return self.Divide(other);
        }

        public abstract BorderRadiusGeometry Remainder(float other);

        public static BorderRadiusGeometry operator %(BorderRadiusGeometry self, float other) {
            return self.Remainder(other);
        }

        public static BorderRadiusGeometry lerp(BorderRadiusGeometry a, BorderRadiusGeometry b, float t) {
            if (a == null && b == null)
                return null;
            a = a ?? BorderRadius.zero;
            b = b ?? BorderRadius.zero;
            return a.add((b.subtract(a)) * t);
        }

        public abstract BorderRadius resolve(TextDirection? textDirection);

        public override string ToString() {
            string visual = null, logical = null;
            if (_topLeft == _topRight &&
                _topRight == _bottomLeft &&
                _bottomLeft == _bottomRight) {
                if (_topLeft != Radius.zero) {
                    if (_topLeft.x == _topLeft.y) {
                        visual = $"BorderRadius.circular({_topLeft.x})";
                    }
                    else {
                        visual = $"BorderRadius.all({_topLeft})";
                    }
                }
            }
            else {
                StringBuilder result = new StringBuilder();
                result.Append("BorderRadius.only(");
                bool comma = false;
                if (_topLeft != Radius.zero) {
                    result.Append($"topLeft: {_topLeft}");
                    comma = true;
                }

                if (_topRight != Radius.zero) {
                    if (comma)
                        result.Append(", ");
                    result.Append($"topRight: {_topRight}");
                    comma = true;
                }

                if (_bottomLeft != Radius.zero) {
                    if (comma)
                        result.Append(", ");
                    result.Append($"bottomLeft: {_bottomLeft}");
                    comma = true;
                }

                if (_bottomRight != Radius.zero) {
                    if (comma)
                        result.Append(", ");
                    result.Append($"bottomRight: {_bottomRight}");
                }

                result.Append(")");
                visual = result.ToString();
            }

            if (_topStart == _topEnd &&
                _topEnd == _bottomEnd &&
                _bottomEnd == _bottomStart) {
                if (_topStart != Radius.zero) {
                    if (_topStart.x == _topStart.y) {
                        logical = $"BorderRadiusDirectional.circular({_topStart.x})";
                    }
                    else {
                        logical = $"BorderRadiusDirectional.all({_topStart})";
                    }
                }
            }
            else {
                StringBuilder result = new StringBuilder();
                result.Append("BorderRadiusDirectional.only(");
                bool comma = false;
                if (_topStart != Radius.zero) {
                    result.Append($"topStart: {_topStart}");
                    comma = true;
                }

                if (_topEnd != Radius.zero) {
                    if (comma)
                        result.Append(", ");
                    result.Append($"topEnd: {_topEnd}");
                    comma = true;
                }

                if (_bottomStart != Radius.zero) {
                    if (comma)
                        result.Append(", ");
                    result.Append($"bottomStart: {_bottomStart}");
                    comma = true;
                }

                if (_bottomEnd != Radius.zero) {
                    if (comma)
                        result.Append(", ");
                    result.Append($"bottomEnd: {_bottomEnd}");
                }

                result.Append(")");
                logical = result.ToString();
            }

            if (visual != null && logical != null)
                return $"{visual} + {logical}";
            if (visual != null)
                return visual;
            if (logical != null)
                return logical;
            return "BorderRadius.zero";
        }

        public static bool operator ==(BorderRadiusGeometry left, BorderRadiusGeometry right) {
            return Equals(left, right);
        }

        public static bool operator !=(BorderRadiusGeometry left, BorderRadiusGeometry right) {
            return !Equals(left, right);
        }

        public bool Equals(BorderRadiusGeometry other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(_topLeft, other._topLeft) && Equals(_topRight, other._topRight) &&
                   Equals(_bottomLeft, other._bottomLeft) && Equals(_bottomRight, other._bottomRight) &&
                   Equals(_topStart, other._topStart) && Equals(_topEnd, other._topEnd) &&
                   Equals(_bottomStart, other._bottomStart) && Equals(_bottomEnd, other._bottomEnd);
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

            return Equals((BorderRadiusGeometry) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (_topLeft != null ? _topLeft.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_topRight != null ? _topRight.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_bottomLeft != null ? _bottomLeft.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_bottomRight != null ? _bottomRight.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_topStart != null ? _topStart.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_topEnd != null ? _topEnd.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_bottomStart != null ? _bottomStart.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_bottomEnd != null ? _bottomEnd.GetHashCode() : 0);
                return hashCode;
            }
        }
    }


    public class BorderRadius : BorderRadiusGeometry, IEquatable<BorderRadius> {
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


        protected override Radius _topLeft {
            get => topLeft;
        }

        public readonly Radius topRight;

        protected override Radius _topRight {
            get => topRight;
        }

        public readonly Radius bottomLeft;

        protected override Radius _bottomLeft {
            get => bottomLeft;
        }

        public readonly Radius bottomRight;

        protected override Radius _bottomRight {
            get => bottomRight;
        }

        protected override Radius _topStart {
            get => Radius.zero;
        }

        protected override Radius _topEnd {
            get => Radius.zero;
        }

        protected override Radius _bottomStart {
            get => Radius.zero;
        }

        protected override Radius _bottomEnd {
            get => Radius.zero;
        }

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

        public override BorderRadiusGeometry SelfMinus() {
            return only(
                topLeft: -topLeft,
                topRight: -topRight,
                bottomLeft: -bottomLeft,
                bottomRight: -bottomRight
            );
        }

        public override BorderRadiusGeometry Multiply(float other) {
            return only(
                topLeft: topLeft * other,
                topRight: topRight * other,
                bottomLeft: bottomLeft * other,
                bottomRight: bottomRight * other
            );
        }

        public override BorderRadiusGeometry Divide(float other) {
            return only(
                topLeft: topLeft / other,
                topRight: topRight / other,
                bottomLeft: bottomLeft / other,
                bottomRight: bottomRight / other
            );
        }

        public override BorderRadiusGeometry Remainder(float other) {
            return only(
                topLeft: topLeft % other,
                topRight: topRight % other,
                bottomLeft: bottomLeft % other,
                bottomRight: bottomRight % other
            );
        }

        public static BorderRadius lerp(BorderRadius a, BorderRadius b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (BorderRadius) (b * t);
            }

            if (b == null) {
                return (BorderRadius) (a * (1.0f - t));
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

            return Equals(topLeft, topLeft)
                   && Equals(topRight, other.topRight)
                   && Equals(bottomRight, other.bottomRight)
                   && Equals(bottomLeft, other.bottomLeft);
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

        public override BorderRadius resolve(TextDirection? textDirection) => this;
    }


    public class BorderRadiusDirectional : BorderRadiusGeometry {
        BorderRadiusDirectional(
            Radius topStart,
            Radius topEnd,
            Radius bottomStart,
            Radius bottomEnd) {
            this.topStart = topStart ?? Radius.zero;
            this.topEnd = topEnd ?? Radius.zero;
            this.bottomStart = bottomStart ?? Radius.zero;
            this.bottomEnd = bottomEnd ?? Radius.zero;
        }

        public static BorderRadiusDirectional all(Radius radius) {
            return only(
                topStart: radius,
                topEnd: radius,
                bottomStart: radius,
                bottomEnd: radius
            );
        }

        public static BorderRadiusDirectional circular(float radius) {
            return all(
                Radius.circular(radius)
            );
        }

        public static BorderRadiusDirectional vertical(
            Radius top = null,
            Radius bottom = null
        ) {
            top = top ?? Radius.zero;
            bottom = bottom ?? Radius.zero;
            return only(
                topStart: top,
                topEnd: top,
                bottomStart: bottom,
                bottomEnd: bottom
            );
        }

        public static BorderRadiusDirectional horizontal(
            Radius start = null,
            Radius end = null
        ) {
            start = start ?? Radius.zero;
            end = end ?? Radius.zero;
            return only(
                topStart: start,
                topEnd: end,
                bottomStart: start,
                bottomEnd: end
            );
        }

        public static BorderRadiusDirectional only(
            Radius topStart,
            Radius topEnd,
            Radius bottomStart,
            Radius bottomEnd) {
            return new BorderRadiusDirectional(topStart, topEnd, bottomStart, bottomEnd);
        }

        public static readonly BorderRadiusDirectional zero = BorderRadiusDirectional.all(Radius.zero);

        public readonly Radius topStart;


        protected override Radius _topStart {
            get => topStart;
        }

        public readonly Radius topEnd;


        protected override Radius _topEnd {
            get => topEnd;
        }

        public readonly Radius bottomStart;


        protected override Radius _bottomStart {
            get => bottomStart;
        }

        public readonly Radius bottomEnd;


        protected override Radius _bottomEnd {
            get => bottomEnd;
        }


        protected override Radius _topLeft {
            get => Radius.zero;
        }


        protected override Radius _topRight {
            get => Radius.zero;
        }


        protected override Radius _bottomLeft {
            get => Radius.zero;
        }


        protected override Radius _bottomRight {
            get => Radius.zero;
        }

        public override BorderRadiusGeometry subtract(BorderRadiusGeometry other) {
            if (other is BorderRadiusDirectional borderRadiusDirectional)
                return this - borderRadiusDirectional;
            return base.subtract(other);
        }

        public override BorderRadiusGeometry add(BorderRadiusGeometry other) {
            if (other is BorderRadiusDirectional)
                return this + (BorderRadiusDirectional) other;
            return base.add(other);
        }

        public static BorderRadiusDirectional operator -(BorderRadiusDirectional self, BorderRadiusDirectional other) {
            return BorderRadiusDirectional.only(
                topStart: self.topStart - other.topStart,
                topEnd: self.topEnd - other.topEnd,
                bottomStart: self.bottomStart - other.bottomStart,
                bottomEnd: self.bottomEnd - other.bottomEnd
            );
        }

        public static BorderRadiusDirectional operator +(BorderRadiusDirectional other, BorderRadiusDirectional right) {
            return BorderRadiusDirectional.only(
                topStart: other.topStart + right.topStart,
                topEnd: other.topEnd + right.topEnd,
                bottomStart: other.bottomStart + right.bottomStart,
                bottomEnd: other.bottomEnd + right.bottomEnd
            );
        }

        public override BorderRadiusGeometry SelfMinus() {
            return BorderRadiusDirectional.only(
                topStart: -topStart,
                topEnd: -topEnd,
                bottomStart: -bottomStart,
                bottomEnd: -bottomEnd
            );
        }

        public override BorderRadiusGeometry Multiply(float other) {
            return BorderRadiusDirectional.only(
                topStart: topStart * other,
                topEnd: topEnd * other,
                bottomStart: bottomStart * other,
                bottomEnd: bottomEnd * other
            );
        }

        public override BorderRadiusGeometry Divide(float other) {
            return BorderRadiusDirectional.only(
                topStart: topStart / other,
                topEnd: topEnd / other,
                bottomStart: bottomStart / other,
                bottomEnd: bottomEnd / other
            );
        }

        public override BorderRadiusGeometry Remainder(float other) {
            return BorderRadiusDirectional.only(
                topStart: topStart % other,
                topEnd: topEnd % other,
                bottomStart: bottomStart % other,
                bottomEnd: bottomEnd % other
            );
        }

        public static BorderRadiusDirectional lerp(BorderRadiusDirectional a, BorderRadiusDirectional b, float t) {
            if (a == null && b == null)
                return null;
            if (a == null)
                return (BorderRadiusDirectional) (b * t);
            if (b == null)
                return (BorderRadiusDirectional) (a * (1.0f - t));
            return BorderRadiusDirectional.only(
                topStart: Radius.lerp(a.topStart, b.topStart, t),
                topEnd: Radius.lerp(a.topEnd, b.topEnd, t),
                bottomStart: Radius.lerp(a.bottomStart, b.bottomStart, t),
                bottomEnd: Radius.lerp(a.bottomEnd, b.bottomEnd, t)
            );
        }

        public override BorderRadius resolve(TextDirection? direction) {
            switch (direction) {
                case TextDirection.rtl:
                    return BorderRadius.only(
                        topLeft: topEnd,
                        topRight: topStart,
                        bottomLeft: bottomEnd,
                        bottomRight: bottomStart
                    );
                case TextDirection.ltr:
                    return BorderRadius.only(
                        topLeft: topStart,
                        topRight: topEnd,
                        bottomLeft: bottomStart,
                        bottomRight: bottomEnd
                    );
            }

            return null;
        }
    }


    class _MixedBorderRadius : BorderRadiusGeometry {
        internal _MixedBorderRadius(
            Radius _topLeft,
            Radius _topRight,
            Radius _bottomLeft,
            Radius _bottomRight,
            Radius _topStart,
            Radius _topEnd,
            Radius _bottomStart,
            Radius _bottomEnd) {
            this._topLeft = _topLeft;
            this._topRight = _topRight;
            this._bottomLeft = _bottomLeft;
            this._bottomRight = _bottomRight;
            this._topStart = _topStart;
            this._topEnd = _topEnd;
            this._bottomStart = _bottomStart;
            this._bottomEnd = _bottomEnd;
        }

        protected override Radius _topLeft { get; }

        protected override Radius _topRight { get; }

        protected override Radius _bottomLeft { get; }

        protected override Radius _bottomRight { get; }

        protected override Radius _topStart { get; }

        protected override Radius _topEnd { get; }

        protected override Radius _bottomStart { get; }

        protected override Radius _bottomEnd { get; }

        public override BorderRadiusGeometry SelfMinus() {
            return new _MixedBorderRadius(
                -_topLeft,
                -_topRight,
                -_bottomLeft,
                -_bottomRight,
                -_topStart,
                -_topEnd,
                -_bottomStart,
                -_bottomEnd
            );
        }

        public override BorderRadiusGeometry Multiply(float other) {
            return new _MixedBorderRadius(
                _topLeft * other,
                _topRight * other,
                _bottomLeft * other,
                _bottomRight * other,
                _topStart * other,
                _topEnd * other,
                _bottomStart * other,
                _bottomEnd * other
            );
        }

        public override BorderRadiusGeometry Divide(float other) {
            return new _MixedBorderRadius(
                _topLeft / other,
                _topRight / other,
                _bottomLeft / other,
                _bottomRight / other,
                _topStart / other,
                _topEnd / other,
                _bottomStart / other,
                _bottomEnd / other
            );
        }

        public override BorderRadiusGeometry Remainder(float other) {
            return new _MixedBorderRadius(
                _topLeft % other,
                _topRight % other,
                _bottomLeft % other,
                _bottomRight % other,
                _topStart % other,
                _topEnd % other,
                _bottomStart % other,
                _bottomEnd % other
            );
        }

        public override BorderRadius resolve(TextDirection? direction) {
            switch (direction) {
                case TextDirection.rtl:
                    return BorderRadius.only(
                        topLeft: _topLeft + _topEnd,
                        topRight: _topRight + _topStart,
                        bottomLeft: _bottomLeft + _bottomEnd,
                        bottomRight: _bottomRight + _bottomStart
                    );
                case TextDirection.ltr:
                    return BorderRadius.only(
                        topLeft: _topLeft + _topStart,
                        topRight: _topRight + _topEnd,
                        bottomLeft: _bottomLeft + _bottomStart,
                        bottomRight: _bottomRight + _bottomEnd
                    );
            }

            return null;
        }
    }
}