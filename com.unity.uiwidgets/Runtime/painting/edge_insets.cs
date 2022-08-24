using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public abstract class EdgeInsetsGeometry {
        internal virtual float _bottom { get; }
        internal virtual float _end { get; }
        internal virtual float _left { get; }
        internal virtual float _right { get; }
        internal virtual float _start { get; }
        internal virtual float _top { get; }


        public static EdgeInsetsGeometry infinityEdgeInsetsGeometry = _MixedEdgeInsets.fromLRSETB(
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity
        );

        /// Whether every dimension is non-negative.
        public virtual bool isNonNegative {
            get {
                return _left >= 0.0
                       && _right >= 0.0
                       && _start >= 0.0
                       && _end >= 0.0
                       && _top >= 0.0
                       && _bottom >= 0.0;
            }
        }


        public virtual float horizontal {
            get { return _left + _right + _start + _end; }
        }

        public virtual float vertical {
            get { return _top + _bottom; }
        }


        float? along(Axis axis) {
            switch (axis) {
                case Axis.horizontal:
                    return horizontal;
                case Axis.vertical:
                    return vertical;
            }
            return null;
        }


        Size collapsedSize {
            get { return new Size(horizontal, vertical); }
        }


        EdgeInsetsGeometry flipped {
            get { return _MixedEdgeInsets.fromLRSETB(_right, _left, _end, _start, _bottom, _top); }
        }


        Size inflateSize(Size size) {
            return new Size(size.width + horizontal, size.height + vertical);
        }

        Size deflateSize(Size size) {
            return new Size(size.width - horizontal, size.height - vertical);
        }

        public virtual EdgeInsetsGeometry clamp(EdgeInsetsGeometry min, EdgeInsetsGeometry max) {
            return _MixedEdgeInsets.fromLRSETB(
                _left.clamp(min._left, max._left),
                _right.clamp(min._right, max._right),
                _start.clamp(min._start, max._start),
                _end.clamp(min._end, max._end),
                _top.clamp(min._top, max._top),
                _bottom.clamp(min._bottom, max._bottom)
            );
        }

        public virtual EdgeInsetsGeometry add(EdgeInsetsGeometry other) {
            return _MixedEdgeInsets.fromLRSETB(
                _left + other._left,
                _right + other._right,
                _start + other._start,
                _end + other._end,
                _top + other._top,
                _bottom + other._bottom
            );
        }

        public virtual EdgeInsetsGeometry subtract(EdgeInsetsGeometry other) {
            return _MixedEdgeInsets.fromLRSETB(
                _left - other._left,
                _right - other._right,
                _start - other._start,
                _end - other._end,
                _top - other._top,
                _bottom - other._bottom
            );
        }

        public virtual EdgeInsetsGeometry multiply(float b) {
            return _MixedEdgeInsets.fromLRSETB(
                _left * b,
                _right * b,
                _start * b,
                _end * b,
                _top * b,
                _bottom * b
            );
        }

        public virtual EdgeInsetsGeometry divide(float b) {
            return _MixedEdgeInsets.fromLRSETB(
                _left / b,
                _right / b,
                _start / b,
                _end / b,
                _top / b,
                _bottom / b
            );
        }

        public virtual EdgeInsetsGeometry remainder(float b) {
            return _MixedEdgeInsets.fromLRSETB(
                _left % b,
                _right % b,
                _start % b,
                _end % b,
                _top % b,
                _bottom % b
            );
        }

        public static EdgeInsetsGeometry operator -(EdgeInsetsGeometry a) {
            return a.multiply(-1.0f);
        }

        public static EdgeInsetsGeometry operator +(EdgeInsetsGeometry a, EdgeInsetsGeometry b) {
            return a.add(b);
        }

        public static EdgeInsetsGeometry operator -(EdgeInsetsGeometry a, EdgeInsetsGeometry b) {
            return a.subtract(b);
        }

        public static EdgeInsetsGeometry operator *(EdgeInsetsGeometry a, float other) {
            return a.multiply(other);
        }

        public static EdgeInsetsGeometry operator /(EdgeInsetsGeometry a, float other) {
            return a.divide(other);
        }

        public static EdgeInsetsGeometry operator %(EdgeInsetsGeometry a, float other) {
            return a.remainder(other);
        }

        public static EdgeInsetsGeometry lerp(EdgeInsetsGeometry a, EdgeInsetsGeometry b, float t) {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b * t;
            if (b == null)
                return a * (1.0f - t);
            if (a is EdgeInsets && b is EdgeInsets)
                return EdgeInsets.lerp((EdgeInsets) a, (EdgeInsets) b, t);
            if (a is EdgeInsetsDirectional && b is EdgeInsetsDirectional)
                return EdgeInsetsDirectional.lerp((EdgeInsetsDirectional)a, (EdgeInsetsDirectional)b, t);

            return _MixedEdgeInsets.fromLRSETB(
                MathUtils.lerpNullableFloat(a._left, b._left, t),
                MathUtils.lerpNullableFloat(a._right, b._right, t),
                MathUtils.lerpNullableFloat(a._start, b._start, t),
                MathUtils.lerpNullableFloat(a._end, b._end, t),
                MathUtils.lerpNullableFloat(a._top, b._top, t),
                MathUtils.lerpNullableFloat(a._bottom, b._bottom, t)
            );
        }


        public abstract EdgeInsets resolve(TextDirection? direction);

        public bool Equals(EdgeInsetsGeometry other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return _left.Equals(other._left)
                   && _right.Equals(other._right)
                   && _top.Equals(other._top)
                   && _bottom.Equals(other._bottom)
                   && _start.Equals(other._top)
                   && _end.Equals(other._bottom);
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

            return Equals((EdgeInsetsGeometry) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _left.GetHashCode();
                hashCode = (hashCode * 397) ^ _right.GetHashCode();
                hashCode = (hashCode * 397) ^ _top.GetHashCode();
                hashCode = (hashCode * 397) ^ _bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ _start.GetHashCode();
                hashCode = (hashCode * 397) ^ _end.GetHashCode();
                return hashCode;
            }
        }


        public override string ToString() {
            if (_start == 0.0 && _end == 0.0) {
                if (_left == 0.0 && _right == 0.0 && _top == 0.0 && _bottom == 0.0)
                    return "EdgeInsets.zero";
                if (_left == _right && _right == _top && _top == _bottom)
                    return $"EdgeInsets.all({_left: F1})";
                return $"EdgeInsets({_left: F1}, " +
                       $"{_top: F1}, " +
                       $"{_right: F1}, " +
                       $"{_bottom: F1})";
            }

            if (_left == 0.0 && _right == 0.0) {
                return $"EdgeInsetsDirectional({_start: F1}, " +
                       $"{_top: F1}, " +
                       $"{_end: F1}, " +
                       $"{_bottom: F1})";
            }

            return $"EdgeInsets({_left: F1}, " +
                   $"{_top: F1}, " +
                   $"{_right: F1}, " +
                   $"{_bottom: F1})" +
                   " + " + $"EdgeInsetsDirectional({_start: F1}, " +
                   "0.0, " + $"{_end: F1}, " + "0.0)";
        }


        public static bool operator ==(EdgeInsetsGeometry a, EdgeInsetsGeometry b) {
            return Equals(a, b);
        }

        public static bool operator !=(EdgeInsetsGeometry a, EdgeInsetsGeometry b) {
            return !(a == b);
        }
    }

    public class EdgeInsetsDirectional : EdgeInsetsGeometry {
        public EdgeInsetsDirectional(float start, float top, float end, float bottom) {
            this.start = start;
            this.end = end;
            this.top = top;
            this.bottom = bottom;
        }

        public readonly float start;

        internal override float _start {
            get { return start; }
        }

        public readonly float end;

        internal override float _end {
            get { return end; }
        }

        public readonly float top;

        internal override float _top {
            get { return top; }
        }

        public readonly float bottom;

        internal override float _bottom {
            get { return bottom; }
        }

        internal override float _left {
            get { return 0f; }
        }

        internal override float _right {
            get { return 0f; }
        }


        public static EdgeInsetsDirectional fromSTEB(float start, float top, float end, float bottom) {
            return new EdgeInsetsDirectional(start, top, end, bottom);
        }
        
        public static EdgeInsetsDirectional only( float start = 0.0f, float top = 0.0f,float end = 0.0f,float bottom = 0.0f) {
            return new EdgeInsetsDirectional(start,top,end,bottom);
        }

        static EdgeInsetsDirectional zero = only();

        public override bool isNonNegative {
            get { return start >= 0.0 && top >= 0.0 && end >= 0.0 && bottom >= 0.0; }
        }

        public EdgeInsetsDirectional flipped {
            get { return fromSTEB(end, bottom, start, top); }
        }

        public override EdgeInsetsGeometry add(EdgeInsetsGeometry other) {
            return fromSTEB(
                start + other._start,
                end + other._end,
                top + other._top,
                bottom + other._bottom
            );
        }

        public override EdgeInsetsGeometry subtract(EdgeInsetsGeometry other) {
            return fromSTEB(
                start - other._start,
                end - other._end,
                top - other._top,
                bottom - other._bottom
            );
        }

        public override EdgeInsetsGeometry multiply(float b){
            return fromSTEB(
                start * b,
                end * b,
                top * b,
                bottom * b
            );
        }

        public override EdgeInsetsGeometry divide(float b){
            return fromSTEB(
                start / b,
                end / b,
                top / b,
                bottom / b
            );
        }

        public override EdgeInsetsGeometry remainder(float b){
            return fromSTEB(
                start % b,
                end % b,
                top % b,
                bottom % b
            );
        }

        public static EdgeInsetsDirectional operator -(EdgeInsetsDirectional a) {
            return a.multiply(-1.0f) as EdgeInsetsDirectional;
        }

        public static EdgeInsetsDirectional operator +(EdgeInsetsDirectional a, EdgeInsetsDirectional b) {
            return a.add(b) as EdgeInsetsDirectional;
        }

        public static EdgeInsetsDirectional operator -(EdgeInsetsDirectional a, EdgeInsetsDirectional b) {
            return a.subtract(b) as EdgeInsetsDirectional;
        }

        public static EdgeInsetsDirectional operator *(EdgeInsetsDirectional a, float b) {
            return a.multiply(b) as EdgeInsetsDirectional;
        }

        public static EdgeInsetsDirectional operator /(EdgeInsetsDirectional a, float b) {
            return a.divide(b) as EdgeInsetsDirectional;
        }

        public static EdgeInsetsDirectional operator %(EdgeInsetsDirectional a, float b) {
            return a.remainder(b) as EdgeInsetsDirectional;
        }

        public static EdgeInsetsDirectional lerp(EdgeInsetsDirectional a, EdgeInsetsDirectional b, float t) {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b * t;
            if (b == null)
                return a * (1.0f - t);
            return fromSTEB(
                MathUtils.lerpNullableFloat(a.start, b.start, t),
                MathUtils.lerpNullableFloat(a.top, b.top, t),
                MathUtils.lerpNullableFloat(a.end, b.end, t),
                MathUtils.lerpNullableFloat(a.bottom, b.bottom, t)
            );
        }

        public override EdgeInsets resolve(TextDirection? direction) {
            D.assert(direction != null);
            switch (direction) {
                case TextDirection.rtl:
                    return EdgeInsets.fromLTRB(end, top, start, bottom);
                case TextDirection.ltr:
                    return EdgeInsets.fromLTRB(start, top, end, bottom);
            }

            return null;
        }
    }

    public class _MixedEdgeInsets : EdgeInsetsGeometry {
        public _MixedEdgeInsets(float _left, float _right, float _start, float _end, float _top, float _bottom) {
            this._left = _left;
            this._right = _right;
            this._start = _start;
            this._end = _end;
            this._top = _top;
            this._bottom = _bottom;
        }

        public static _MixedEdgeInsets fromLRSETB(float _left, float _right, float _start, float _end, float _top,
            float _bottom) {
            return new _MixedEdgeInsets(_left, _right, _start, _end, _top, _bottom);
        }

        internal override float _left { get; }

        internal override float _right { get; }

        internal override float _start { get; }

        internal override float _end { get; }

        internal override float _top { get; }

        internal override float _bottom { get; }


        public override bool isNonNegative {
            get {
                return _left >= 0.0
                       && _right >= 0.0
                       && _start >= 0.0
                       && _end >= 0.0
                       && _top >= 0.0
                       && _bottom >= 0.0;
            }
        }

        public override EdgeInsetsGeometry add(EdgeInsetsGeometry other) {
            return fromLRSETB(
                _left + other._left,
                _right + other._right,
                _start + other._start,
                _end + other._end,
                _top + other._top,
                _bottom + other._bottom
            );
        }

        public override EdgeInsetsGeometry subtract(EdgeInsetsGeometry other) {
            return fromLRSETB(
                _left - other._left,
                _right - other._right,
                _start - other._start,
                _end - other._end,
                _top - other._top,
                _bottom - other._bottom
            );
        }

        public override EdgeInsetsGeometry multiply(float b){
            return fromLRSETB(
                _left * b,
                _right * b,
                _start * b,
                _end * b,
                _top * b,
                _bottom * b
            );
        }

        public override EdgeInsetsGeometry divide(float b){
            return fromLRSETB(
                _left / b,
                _right / b,
                _start / b,
                _end / b,
                _top / b,
                _bottom / b
            );
        }

        public override EdgeInsetsGeometry remainder(float b){
            return fromLRSETB(
                _left % b,
                _right % b,
                _start % b,
                _end % b,
                _top % b,
                _bottom % b
            );
        }

        public static _MixedEdgeInsets operator -(_MixedEdgeInsets a) {
            return a.multiply(-1.0f) as _MixedEdgeInsets;
        }

        public static _MixedEdgeInsets operator +(_MixedEdgeInsets a, _MixedEdgeInsets b) {
            return a.add(b) as _MixedEdgeInsets;
        }

        public static _MixedEdgeInsets operator -(_MixedEdgeInsets a, _MixedEdgeInsets b) {
            return a.subtract(b) as _MixedEdgeInsets;
        }

        public static _MixedEdgeInsets operator *(_MixedEdgeInsets a, float b) {
            return a.multiply(b) as _MixedEdgeInsets;
        }

        public static _MixedEdgeInsets operator /(_MixedEdgeInsets a, float b) {
            return a.divide(b) as _MixedEdgeInsets;
        }

        public static _MixedEdgeInsets operator %(_MixedEdgeInsets a, float b) {
            return a.remainder(b) as _MixedEdgeInsets;
        }

        public override EdgeInsets resolve(TextDirection? direction) {
            D.assert(direction != null);
            switch (direction) {
                case TextDirection.rtl:
                    return EdgeInsets.fromLTRB(_end + _left, _top, _start + _right, _bottom);
                case TextDirection.ltr:
                    return EdgeInsets.fromLTRB(_start + _left, _top, _end + _right, _bottom);
            }

            return null;
        }
    }


    public class EdgeInsets : EdgeInsetsGeometry {
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

        public static EdgeInsets infinityEdgeInsets = fromLTRB(
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity
        );

        public override bool isNonNegative {
            get {
                return left >= 0.0
                       && right >= 0.0
                       && top >= 0.0
                       && bottom >= 0.0;
            }
        }

        public override float horizontal {
            get { return left + right; }
        }

        public override float vertical {
            get { return top + bottom; }
        }

        public float along(Axis? axis) {
            D.assert(axis != null);
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

        public override EdgeInsetsGeometry clamp(EdgeInsetsGeometry min, EdgeInsetsGeometry max) {
            return fromLTRB(
                left.clamp(min._left, max._left),
                top.clamp(min._top, max._top),
                right.clamp(min._right, max._right),
                bottom.clamp(min._bottom, max._bottom)
            );
        }

        public override EdgeInsetsGeometry add(EdgeInsetsGeometry other) {
            return fromLTRB(
                left + other._left,
                top + other._top,
                right + other._right,
                bottom + other._bottom
            );
        }

        public override EdgeInsetsGeometry subtract(EdgeInsetsGeometry other) {
            return fromLTRB(
                left - other._left,
                top - other._top,
                right - other._right,
                bottom - other._bottom
            );
        }

        public override EdgeInsetsGeometry multiply(float b){
            return fromLTRB(
                left * b,
                top * b,
                right * b,
                bottom * b
            );
        }

        public override EdgeInsetsGeometry divide(float b){
            return fromLTRB(
                left / b,
                top / b,
                right / b,
                bottom / b
            );
        }

        public override EdgeInsetsGeometry remainder(float b){
            return fromLTRB(
                left % b,
                top % b,
                right % b,
                bottom % b
            );
        }

        public static EdgeInsets operator -(EdgeInsets a) {
            return a.multiply(-1.0f) as EdgeInsets;
        }

        public static EdgeInsets operator +(EdgeInsets a, EdgeInsets b) {
            return a.add(b) as EdgeInsets;
        }

        public static EdgeInsets operator -(EdgeInsets a, EdgeInsets b) {
            return a.subtract(b) as EdgeInsets;
        }

        public static EdgeInsets operator *(EdgeInsets a, float b) {
            return a.multiply(b) as EdgeInsets;
        }

        public static EdgeInsets operator /(EdgeInsets a, float b) {
            return a.divide(b) as EdgeInsets;
        }

        public static EdgeInsets operator %(EdgeInsets a, float b) {
            return a.remainder(b) as EdgeInsets;
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
                MathUtils.lerpNullableFloat(a.left, b.left, t),
                MathUtils.lerpNullableFloat(a.top, b.top, t),
                MathUtils.lerpNullableFloat(a.right, b.right, t),
                MathUtils.lerpNullableFloat(a.bottom, b.bottom, t)
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

        public override EdgeInsets resolve(TextDirection? direction) {
            return this;
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