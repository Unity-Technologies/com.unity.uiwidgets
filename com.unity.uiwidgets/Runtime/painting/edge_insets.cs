using System;
using JetBrains.Annotations;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public abstract class EdgeInsetsGeometry {
        internal float _bottom { get; }
        internal float _end { get; }
        internal float _left { get; }
        internal float _right { get; }
        internal float _start { get; }
        internal float _top { get; }


        public static EdgeInsetsGeometry infinity = _MixedEdgeInsets.fromLRSETB(
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


        public float horizontal {
            get { return _left + _right + _start + _end; }
        }

        public float vertical {
            get { return _top + _bottom; }
        }


        float? along(Axis axis) {
            D.assert(axis != null);
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

        protected EdgeInsetsGeometry subtract(EdgeInsetsGeometry other) {
            return _MixedEdgeInsets.fromLRSETB(
                _left - other._left,
                _right - other._right,
                _start - other._start,
                _end - other._end,
                _top - other._top,
                _bottom - other._bottom
            );
        }

        protected EdgeInsetsGeometry add(EdgeInsetsGeometry other) {
            return _MixedEdgeInsets.fromLRSETB(
                _left + other._left,
                _right + other._right,
                _start + other._start,
                _end + other._end,
                _top + other._top,
                _bottom + other._bottom
            );
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

        public static EdgeInsetsGeometry operator -(EdgeInsetsGeometry a, EdgeInsetsGeometry b) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left - b._left,
                a._right - b._right,
                a._start - b._start,
                a._end - b._end,
                a._top - b._top,
                a._bottom - b._bottom
            );
        }

        public static EdgeInsetsGeometry operator +(EdgeInsetsGeometry a, EdgeInsetsGeometry b) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left + b._left,
                a._right + b._right,
                a._start + b._start,
                a._end + b._end,
                a._top + b._top,
                a._bottom + b._bottom
            );
        }

        public static EdgeInsetsGeometry operator -(EdgeInsetsGeometry a) {
            return _MixedEdgeInsets.fromLRSETB(
                -a._left,
                -a._right,
                -a._start,
                -a._end,
                -a._top,
                -a._bottom
            );
        }

        public static EdgeInsetsGeometry operator *(EdgeInsetsGeometry a, float other) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left * other,
                a._right * other,
                a._start * other,
                a._end * other,
                a._top * other,
                a._bottom * other
            );
        }

        public static EdgeInsetsGeometry operator /(EdgeInsetsGeometry a, float other) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left / other,
                a._right / other,
                a._start / other,
                a._end / other,
                a._top / other,
                a._bottom / other
            );
        }

        public static EdgeInsetsGeometry operator %(EdgeInsetsGeometry a, float other) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left % other,
                a._right % other,
                a._start % other,
                a._end % other,
                a._top % other,
                a._bottom % other
            );
        }


        public static EdgeInsetsGeometry lerp(EdgeInsetsGeometry a, EdgeInsetsGeometry b, float t) {
            D.assert(t != null);
            if (a == null && b == null)
                return null;
            if (a == null)
                return b * t;
            if (b == null)
                return a * (1.0f - t);
            if (a is EdgeInsets && b is EdgeInsets)
                return EdgeInsets.lerp((EdgeInsets) a, (EdgeInsets) b, t);
            if (a is EdgeInsetsDirectional && b is EdgeInsetsDirectional)
                return EdgeInsetsDirectional.lerp(a, b, t);

            return _MixedEdgeInsets.fromLRSETB(
                Mathf.Lerp(a._left, b._left, t),
                Mathf.Lerp(a._right, b._right, t),
                Mathf.Lerp(a._start, b._start, t),
                Mathf.Lerp(a._end, b._end, t),
                Mathf.Lerp(a._top, b._top, t),
                Mathf.Lerp(a._bottom, b._bottom, t)
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

            return Equals((EdgeInsets) obj);
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

        float _start {
            get { return start; }
        }

        public readonly float end;

        float _end {
            get { return end; }
        }

        public readonly float top;

        float _top {
            get { return top; }
        }

        public readonly float bottom;

        float _bottom {
            get { return bottom; }
        }

        float _left {
            get { return 0f; }
        }

        float _right {
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

        public EdgeInsetsGeometry subtract(EdgeInsetsDirectional other) {
            if (other is EdgeInsetsDirectional)
                return this - other;
            return base.subtract(other);
        }

        public EdgeInsetsGeometry add(EdgeInsetsDirectional other) {
            if (other is EdgeInsetsDirectional)
                return this + other;
            return base.add(other);
        }

        public static EdgeInsetsDirectional operator -(EdgeInsetsDirectional a, EdgeInsetsDirectional b) {
            return fromSTEB(
                a.start - b.start,
                a.top - b.top,
                a.end - b.end,
                a.bottom - b.bottom
            );
        }

        public static EdgeInsetsDirectional operator +(EdgeInsetsDirectional a, EdgeInsetsDirectional b) {
            return fromSTEB(
                a.start + b.start,
                a.top + b.top,
                a.end + b.end,
                a.bottom + b.bottom
            );
        }

        public static EdgeInsetsDirectional operator -(EdgeInsetsDirectional a) {
            return fromSTEB(
                -a.start,
                -a.top,
                -a.end,
                -a.bottom
            );
        }

        public static EdgeInsetsDirectional operator *(EdgeInsetsDirectional a, float b) {
            return fromSTEB(
                a.start * b,
                a.top * b,
                a.end * b,
                a.bottom * b
            );
        }

        public static EdgeInsetsDirectional operator /(EdgeInsetsDirectional a, float b) {
            return fromSTEB(
                a.start / b,
                a.top / b,
                a.end / b,
                a.bottom / b
            );
        }

        public static EdgeInsetsDirectional operator %(EdgeInsetsDirectional a, float b) {
            return fromSTEB(
                a.start % b,
                a.top % b,
                a.end % b,
                a.bottom % b
            );
        }


        public static EdgeInsetsDirectional lerp(EdgeInsetsDirectional a, EdgeInsetsDirectional b, float t) {
            D.assert(t != null);
            if (a == null && b == null)
                return null;
            if (a == null)
                return b * t;
            if (b == null)
                return a * (1.0f - t);
            return fromSTEB(
                Mathf.Lerp(a.start, b.start, t),
                Mathf.Lerp(a.top, b.top, t),
                Mathf.Lerp(a.end, b.end, t),
                Mathf.Lerp(a.bottom, b.bottom, t)
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

        public readonly float _left;

        public readonly float _right;

        public readonly float _start;

        public readonly float _end;

        public readonly float _top;

        public readonly float _bottom;


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


        public static _MixedEdgeInsets operator -(_MixedEdgeInsets a, _MixedEdgeInsets b) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left - b._left,
                a._right - b._right,
                a._start - b._start,
                a._end - b._end,
                a._top - b._top,
                a._bottom - b._bottom
            );
        }

        public static _MixedEdgeInsets operator +(_MixedEdgeInsets a, _MixedEdgeInsets b) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left + b._left,
                a._right + b._right,
                a._start + b._start,
                a._end + b._end,
                a._top + b._top,
                a._bottom + b._bottom
            );
        }

        public static _MixedEdgeInsets operator -(_MixedEdgeInsets a) {
            return _MixedEdgeInsets.fromLRSETB(
                -a._left,
                -a._right,
                -a._start,
                -a._end,
                -a._top,
                -a._bottom
            );
        }

        public static _MixedEdgeInsets operator *(_MixedEdgeInsets a, float other) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left * other,
                a._right * other,
                a._start * other,
                a._end * other,
                a._top * other,
                a._bottom * other
            );
        }

        public static _MixedEdgeInsets operator /(_MixedEdgeInsets a, float other) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left / other,
                a._right / other,
                a._start / other,
                a._end / other,
                a._top / other,
                a._bottom / other
            );
        }

        public static _MixedEdgeInsets operator %(_MixedEdgeInsets a, float other) {
            return _MixedEdgeInsets.fromLRSETB(
                a._left % other,
                a._right % other,
                a._start % other,
                a._end % other,
                a._top % other,
                a._bottom % other
            );
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

        public static EdgeInsets infinity = fromLTRB(
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity
        );

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

        public EdgeInsets add(EdgeInsetsGeometry other) {
            return fromLTRB(
                left + other._left,
                top + other._top,
                right + other._right,
                bottom + other._bottom
            );
        }

        public override EdgeInsetsGeometry clamp(EdgeInsetsGeometry min, EdgeInsetsGeometry max) {
            return fromLTRB(
                left.clamp(min._left, max._left),
                top.clamp(min._top, max._top),
                right.clamp(min._right, max._right),
                bottom.clamp(min._bottom, max._bottom)
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
                Mathf.Lerp(a.left, b.left, t),
                Mathf.Lerp(a.top, b.top, t),
                Mathf.Lerp(a.right, b.right, t),
                Mathf.Lerp(a.bottom, b.bottom, t)
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