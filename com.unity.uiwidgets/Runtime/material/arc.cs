using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    static class ArcUtils {
        public const float _kOnAxisDelta = 2.0f;

        public static readonly List<_Diagonal> _allDiagonals = new List<_Diagonal> {
            new _Diagonal(_CornerId.topLeft, _CornerId.bottomRight),
            new _Diagonal(_CornerId.bottomRight, _CornerId.topLeft),
            new _Diagonal(_CornerId.topRight, _CornerId.bottomLeft),
            new _Diagonal(_CornerId.bottomLeft, _CornerId.topRight)
        };

        public delegate float _KeyFunc<T>(T input);


        public static T _maxBy<T>(List<T> input, _KeyFunc<T> keyFunc) {
            T maxValue = default(T);
            float? maxKey = null;
            foreach (T value in input) {
                float key = keyFunc(value);
                if (maxKey == null || key > maxKey) {
                    maxValue = value;
                    maxKey = key;
                }
            }

            return maxValue;
        }
    }

    public class MaterialPointArcTween : Tween<Offset> {
        public MaterialPointArcTween(
            Offset begin = null,
            Offset end = null) : base(begin: begin, end: end) {
        }

        bool _dirty = true;

        void _initialze() {
            D.assert(begin != null);
            D.assert(end != null);

            Offset delta = end - begin;
            float deltaX = delta.dx.abs();
            float deltaY = delta.dy.abs();
            float distanceFromAtoB = delta.distance;
            Offset c = new Offset(end.dx, begin.dy);

            float sweepAngle() {
                return 2.0f * Mathf.Asin(distanceFromAtoB / (2.0f * _radius));
            }

            if (deltaX > ArcUtils._kOnAxisDelta && deltaY > ArcUtils._kOnAxisDelta) {
                if (deltaX < deltaY) {
                    _radius = distanceFromAtoB * distanceFromAtoB / (c - begin).distance / 2.0f;
                    _center = new Offset(end.dx + _radius * (begin.dx - end.dx).sign(),
                        end.dy);
                    if (begin.dx < end.dx) {
                        _beginAngle = sweepAngle() * (begin.dy - end.dy).sign();
                        _endAngle = 0.0f;
                    }
                    else {
                        _beginAngle = (Mathf.PI + sweepAngle() * (end.dy - begin.dy).sign());
                        _endAngle = Mathf.PI;
                    }
                }
                else {
                    _radius = distanceFromAtoB * distanceFromAtoB / (c - end).distance / 2.0f;
                    _center = new Offset(begin.dx,
                        begin.dy + (end.dy - begin.dy).sign() * _radius);
                    if (begin.dy < end.dy) {
                        _beginAngle = -Mathf.PI / 2.0f;
                        _endAngle = _beginAngle + sweepAngle() * (end.dx - begin.dx).sign();
                    }
                    else {
                        _beginAngle = Mathf.PI / 2.0f;
                        _endAngle = _beginAngle + sweepAngle() * (begin.dx - end.dx).sign();
                    }
                }

                D.assert(_beginAngle != null);
                D.assert(_endAngle != null);
            }
            else {
                _beginAngle = null;
                _endAngle = null;
            }

            _dirty = false;
        }

        public Offset center {
            get {
                if (begin == null || end == null) {
                    return null;
                }

                if (_dirty) {
                    _initialze();
                }

                return _center;
            }
        }

        Offset _center;

        public float? radius {
            get {
                if (begin == null || end == null) {
                    return null;
                }

                if (_dirty) {
                    _initialze();
                }

                return _radius;
            }
        }

        float _radius;

        public float? beginAngle {
            get {
                if (begin == null || end == null) {
                    return null;
                }

                if (_dirty) {
                    _initialze();
                }

                return _beginAngle;
            }
        }

        float? _beginAngle;

        public float? endAngle {
            get {
                if (begin == null || end == null) {
                    return null;
                }

                if (_dirty) {
                    _initialze();
                }

                return _endAngle;
            }
        }

        float? _endAngle;

        public override Offset begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    _dirty = true;
                }
            }
        }

        public override Offset end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    _dirty = true;
                }
            }
        }

        public override Offset lerp(float t) {
            if (_dirty) {
                _initialze();
            }

            if (t == 0.0) {
                return begin;
            }

            if (t == 1.0) {
                return end;
            }

            if (_beginAngle == null || _endAngle == null) {
                return Offset.lerp(begin, end, t);
            }

            float angle = MathUtils.lerpNullableFloat(_beginAngle, _endAngle, t) ?? 0.0f;
            float x = Mathf.Cos(angle) * _radius;
            float y = Mathf.Sin(angle) * _radius;
            return _center + new Offset(x, y);
        }

        public override string ToString() {
            return GetType() + "(" + begin + "->" + end + "); center=" + center +
                   ", radius=" + radius + ", beginAngle=" + beginAngle + ", endAngle=" + endAngle;
        }
    }

    public enum _CornerId {
        topLeft,
        topRight,
        bottomLeft,
        bottomRight
    }

    public class _Diagonal {
        public _Diagonal(
            _CornerId beginId,
            _CornerId endId) {
            this.beginId = beginId;
            this.endId = endId;
        }

        public readonly _CornerId beginId;

        public readonly _CornerId endId;
    }

    public class MaterialRectArcTween : RectTween {
        public MaterialRectArcTween(
            Rect begin = null,
            Rect end = null) : base(begin: begin, end: end) {
        }

        bool _dirty = true;

        void _initialize() {
            D.assert(begin != null);
            D.assert(end != null);
            Offset centersVector = end.center - begin.center;
            _Diagonal diagonal = ArcUtils._maxBy(ArcUtils._allDiagonals,
                (_Diagonal d) => _diagonalSupport(centersVector, d));
            _beginArc = new MaterialPointArcTween(
                begin: _cornerFor(begin, diagonal.beginId),
                end: _cornerFor(end, diagonal.beginId));
            _endArc = new MaterialPointArcTween(
                begin: _cornerFor(begin, diagonal.endId),
                end: _cornerFor(end, diagonal.endId));
            _dirty = false;
        }

        float _diagonalSupport(Offset centersVector, _Diagonal diagonal) {
            Offset delta = _cornerFor(begin, diagonal.endId) - _cornerFor(begin, diagonal.beginId);
            float length = delta.distance;
            return centersVector.dx * delta.dx / length + centersVector.dy * delta.dy / length;
        }

        Offset _cornerFor(Rect rect, _CornerId id) {
            switch (id) {
                case _CornerId.topLeft: return rect.topLeft;
                case _CornerId.topRight: return rect.topRight;
                case _CornerId.bottomLeft: return rect.bottomLeft;
                case _CornerId.bottomRight: return rect.bottomRight;
            }

            return Offset.zero;
        }

        public MaterialPointArcTween beginArc {
            get {
                if (begin == null) {
                    return null;
                }

                if (_dirty) {
                    _initialize();
                }

                return _beginArc;
            }
        }

        MaterialPointArcTween _beginArc;

        public MaterialPointArcTween endArc {
            get {
                if (end == null) {
                    return null;
                }

                if (_dirty) {
                    _initialize();
                }

                return _endArc;
            }
        }

        MaterialPointArcTween _endArc;

        public override Rect begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    _dirty = true;
                }
            }
        }

        public override Rect end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    _dirty = true;
                }
            }
        }

        public override Rect lerp(float t) {
            if (_dirty) {
                _initialize();
            }

            if (t == 0.0) {
                return begin;
            }

            if (t == 1.0) {
                return end;
            }

            return Rect.fromPoints(_beginArc.lerp(t), _endArc.lerp(t));
        }

        public override string ToString() {
            return GetType() + "(" + begin + "->" + end + ")";
        }
    }

    public class MaterialRectCenterArcTween : RectTween {
        public MaterialRectCenterArcTween(
            Rect begin = null,
            Rect end = null) : base(begin: begin, end: end) {
        }

        bool _dirty = true;

        void _initialize() {
            D.assert(begin != null);
            D.assert(end != null);
            _centerArc = new MaterialPointArcTween(
                begin: begin.center,
                end: end.center);
            _dirty = false;
        }

        public MaterialPointArcTween centerArc {
            get {
                if (begin == null || end == null) {
                    return null;
                }

                if (_dirty) {
                    _initialize();
                }

                return _centerArc;
            }
        }

        MaterialPointArcTween _centerArc;


        public override Rect begin {
            get { return base.begin; }
            set {
                if (value != base.begin) {
                    base.begin = value;
                    _dirty = true;
                }
            }
        }

        public override Rect end {
            get { return base.end; }
            set {
                if (value != base.end) {
                    base.end = value;
                    _dirty = true;
                }
            }
        }

        public override Rect lerp(float t) {
            if (_dirty) {
                _initialize();
            }

            if (t == 0.0) {
                return begin;
            }

            if (t == 1.0) {
                return end;
            }

            Offset center = _centerArc.lerp(t);
            float width = MathUtils.lerpFloat(begin.width, end.width, t);
            float height = MathUtils.lerpFloat(begin.height, end.height, t);
            return Rect.fromLTWH(
                (center.dx - width / 2.0f),
                (center.dy - height / 2.0f),
                width,
                height);
        }

        public override string ToString() {
            return GetType() + "(" + begin + "->" + end + "); centerArc=" + centerArc;
        }
    }
}