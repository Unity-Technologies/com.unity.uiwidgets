using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class uiPath : PoolObject {
        const float _KAPPA90 = 0.5522847493f;

        uiList<float> _commands;
        float _commandx;
        float _commandy;
        float _minX, _minY;
        float _maxX, _maxY;

        uiPathCache _cache;

        public uint pathKey = 0;
        public bool needCache = false;
        
        bool _isNaiveRRect = false;
        public bool isNaiveRRect => _isNaiveRRect;
        
        uiPathShapeHint _shapeHint = uiPathShapeHint.Other;
        public uiPathShapeHint shapeHint => _shapeHint;
        
        float _rRectCorner;
        public float rRectCorner => _rRectCorner;
        
        void _updateRRectFlag(bool isNaiveRRect, uiPathShapeHint shapeHint = uiPathShapeHint.Other, float corner = 0) {
            if (_commands.Count > 0 && !_isNaiveRRect) {
                return;
            }
            _isNaiveRRect = isNaiveRRect && _hasOnlyMoveTos();
            _shapeHint = shapeHint;
            _rRectCorner = corner;
        }
        
        bool _hasOnlyMoveTos() {
            var i = 0;
            while (i < _commands.Count) {
                var cmd = (PathCommand) _commands[i];
                switch (cmd) {
                    case PathCommand.moveTo: 
                        i += 3;
                        break;
                    case PathCommand.lineTo:
                        return false;
                    case PathCommand.bezierTo:
                        return false;
                    case PathCommand.close:
                        i++;
                        break;
                    case PathCommand.winding:
                        i += 2;
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        public static uiPath create(int capacity = 128) {
            uiPath newPath = ObjectPool<uiPath>.alloc();
            newPath._reset();
            return newPath;
        }

        public uiPath() {
        }

        public override void clear() {
            ObjectPool<uiList<float>>.release(_commands);
            ObjectPool<uiPathCache>.release(_cache);
            _cache = null;
            _commands = null;

            needCache = false;
            pathKey = 0;
            _isNaiveRRect = false;
            _shapeHint = uiPathShapeHint.Other;
            _rRectCorner = 0;
        }

        void _reset() {
            _commands = ObjectPool<uiList<float>>.alloc();
            _commandx = 0;
            _commandy = 0;
            _minX = float.MaxValue;
            _minY = float.MaxValue;
            _maxX = float.MinValue;
            _maxY = float.MinValue;
            ObjectPool<uiPathCache>.release(_cache);
            _cache = null;
            _isNaiveRRect = false;
        }

        internal uiPathCache flatten(float scale) {
            scale = Mathf.Round(scale * 2.0f) / 2.0f; // round to 0.5f

            if (this._cache != null && this._cache.canReuse(scale)) {
                return this._cache;
            }

            var _cache = uiPathCache.create(scale, _shapeHint);

            var i = 0;
            while (i < _commands.Count) {
                var cmd = (uiPathCommand) _commands[i];
                switch (cmd) {
                    case uiPathCommand.moveTo:
                        _cache.addPath();
                        _cache.addPoint(_commands[i + 1], _commands[i + 2], uiPointFlags.corner);
                        i += 3;
                        break;
                    case uiPathCommand.lineTo:
                        _cache.addPoint(_commands[i + 1], _commands[i + 2], uiPointFlags.corner);
                        i += 3;
                        break;
                    case uiPathCommand.bezierTo:
                        _cache.tessellateBezier(
                            _commands[i + 1], _commands[i + 2],
                            _commands[i + 3], _commands[i + 4],
                            _commands[i + 5], _commands[i + 6], uiPointFlags.corner);
                        i += 7;
                        break;
                    case uiPathCommand.close:
                        _cache.closePath();
                        i++;
                        break;
                    case uiPathCommand.winding:
                        _cache.pathWinding((uiPathWinding) _commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        D.assert(false, () => "unknown cmd: " + cmd);
                        break;
                }
            }

            _cache.normalize();
            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = _cache;
            return _cache;
        }

        void _expandBounds(float x, float y) {
            if (x < _minX) {
                _minX = x;
            }

            if (y < _minY) {
                _minY = y;
            }

            if (x > _maxX) {
                _maxX = x;
            }

            if (y > _maxY) {
                _maxY = y;
            }
        }
        
        public uiRect getBounds() {
            if (_minX >= _maxX || _minY >= _maxY) {
                return uiRectHelper.zero;
            }

            return uiRectHelper.fromLTRB(_minX, _minY, _maxX, _maxY);
        }

        public uiRect getBoundsWithMargin(float margin) {
            if (_minX - margin >= _maxX + margin || _minY - margin >= _maxY + margin) {
                return uiRectHelper.zero;
            }

            return uiRectHelper.fromLTRB(_minX - margin, _minY - margin, _maxX + margin, _maxY + margin);
        }

        void _appendMoveTo(float x, float y) {
            _commands.Add((float) uiPathCommand.moveTo);
            _commands.Add(x);
            _commands.Add(y);

            _commandx = x;
            _commandy = y;

            ObjectPool<uiPathCache>.release(_cache);
            _cache = null;
        }

        void _appendLineTo(float x, float y) {
            _expandBounds(_commandx, _commandy);
            _expandBounds(x, y);

            _commands.Add((float) uiPathCommand.lineTo);
            _commands.Add(x);
            _commands.Add(y);

            _commandx = x;
            _commandy = y;

            ObjectPool<uiPathCache>.release(_cache);
            _cache = null;
        }

        void _appendBezierTo(float x1, float y1, float x2, float y2, float x3, float y3) {
            _expandBounds(_commandx, _commandy);
            _expandBounds(x1, y1);
            _expandBounds(x2, y2);
            _expandBounds(x3, y3);

            _commands.Add((float) uiPathCommand.bezierTo);
            _commands.Add(x1);
            _commands.Add(y1);
            _commands.Add(x2);
            _commands.Add(y2);
            _commands.Add(x3);
            _commands.Add(y3);

            _commandx = x3;
            _commandy = y3;

            ObjectPool<uiPathCache>.release(_cache);
            _cache = null;
        }

        void _appendClose() {
            _commands.Add((float) uiPathCommand.close);

            ObjectPool<uiPathCache>.release(_cache);
            _cache = null;
        }

        void _appendWinding(float winding) {
            _commands.Add((float) uiPathCommand.winding);
            _commands.Add(winding);

            ObjectPool<uiPathCache>.release(_cache);
            _cache = null;
        }

        public void addRect(uiRect rect) {
            _updateRRectFlag(true, uiPathShapeHint.Rect);
            _appendMoveTo(rect.left, rect.top);
            _appendLineTo(rect.left, rect.bottom);
            _appendLineTo(rect.right, rect.bottom);
            _appendLineTo(rect.right, rect.top);
            _appendClose();
        }

        public void addRect(Rect rect) {
            _updateRRectFlag(true, uiPathShapeHint.Rect);
            _appendMoveTo(rect.left, rect.top);
            _appendLineTo(rect.left, rect.bottom);
            _appendLineTo(rect.right, rect.bottom);
            _appendLineTo(rect.right, rect.top);
            _appendClose();
        }

        public void addRRect(RRect rrect) {
            _updateRRectFlag(rrect.isNaiveRRect(), uiPathShapeHint.NaiveRRect, rrect.blRadiusX);
            float w = rrect.width;
            float h = rrect.height;
            float halfw = Mathf.Abs(w) * 0.5f;
            float halfh = Mathf.Abs(h) * 0.5f;
            float signW = Mathf.Sign(w);
            float signH = Mathf.Sign(h);

            float rxBL = Mathf.Min(rrect.blRadiusX, halfw) * signW;
            float ryBL = Mathf.Min(rrect.blRadiusY, halfh) * signH;
            float rxBR = Mathf.Min(rrect.brRadiusX, halfw) * signW;
            float ryBR = Mathf.Min(rrect.brRadiusY, halfh) * signH;
            float rxTR = Mathf.Min(rrect.trRadiusX, halfw) * signW;
            float ryTR = Mathf.Min(rrect.trRadiusY, halfh) * signH;
            float rxTL = Mathf.Min(rrect.tlRadiusX, halfw) * signW;
            float ryTL = Mathf.Min(rrect.tlRadiusY, halfh) * signH;
            float x = rrect.left;
            float y = rrect.top;

            _appendMoveTo(x, y + ryTL);
            _appendLineTo(x, y + h - ryBL);
            _appendBezierTo(x, y + h - ryBL * (1 - _KAPPA90),
                x + rxBL * (1 - _KAPPA90), y + h, x + rxBL, y + h);
            _appendLineTo(x + w - rxBR, y + h);
            _appendBezierTo(x + w - rxBR * (1 - _KAPPA90), y + h,
                x + w, y + h - ryBR * (1 - _KAPPA90), x + w, y + h - ryBR);
            _appendLineTo(x + w, y + ryTR);
            _appendBezierTo(x + w, y + ryTR * (1 - _KAPPA90),
                x + w - rxTR * (1 - _KAPPA90), y, x + w - rxTR, y);
            _appendLineTo(x + rxTL, y);
            _appendBezierTo(x + rxTL * (1 - _KAPPA90), y,
                x, y + ryTL * (1 - _KAPPA90), x, y + ryTL);
            _appendClose();
        }

        public void moveTo(float x, float y) {
            _appendMoveTo(x, y);
        }

        public void lineTo(float x, float y) {
            _updateRRectFlag(false);
            _appendLineTo(x, y);
        }

        public void winding(PathWinding dir) {
            _appendWinding((float) dir);
        }

        public void addEllipse(float cx, float cy, float rx, float ry) {
            _updateRRectFlag(rx == ry, uiPathShapeHint.Circle, rx);
            _appendMoveTo(cx - rx, cy);
            _appendBezierTo(cx - rx, cy + ry * _KAPPA90,
                cx - rx * _KAPPA90, cy + ry, cx, cy + ry);
            _appendBezierTo(cx + rx * _KAPPA90, cy + ry,
                cx + rx, cy + ry * _KAPPA90, cx + rx, cy);
            _appendBezierTo(cx + rx, cy - ry * _KAPPA90,
                cx + rx * _KAPPA90, cy - ry, cx, cy - ry);
            _appendBezierTo(cx - rx * _KAPPA90, cy - ry,
                cx - rx, cy - ry * _KAPPA90, cx - rx, cy);
            _appendClose();
        }

        public void addCircle(float cx, float cy, float r) {
            addEllipse(cx, cy, r, r);
        }

        public void arcTo(Rect rect, float startAngle, float sweepAngle, bool forceMoveTo = true) {
            _updateRRectFlag(false);
            var mat = Matrix3.makeScale(rect.width / 2, rect.height / 2);
            var center = rect.center;
            mat.postTranslate(center.dx, center.dy);

            _addArcCommands(0, 0, 1, startAngle, startAngle + sweepAngle,
                sweepAngle >= 0 ? PathWinding.clockwise : PathWinding.counterClockwise, forceMoveTo, mat);
        }

        public void close() {
            _appendClose();
        }

        void _addArcCommands(
            float cx, float cy, float r, float a0, float a1,
            PathWinding dir, bool forceMoveTo, Matrix3 transform = null) {
            // Clamp angles
            float da = a1 - a0;
            if (dir == PathWinding.clockwise) {
                if (Mathf.Abs(da) >= Mathf.PI * 2) {
                    da = Mathf.PI * 2;
                }
                else {
                    while (da < 0.0f) {
                        da += Mathf.PI * 2;
                    }

                    if (da <= 1e-5) {
                        return;
                    }
                }
            }
            else {
                if (Mathf.Abs(da) >= Mathf.PI * 2) {
                    da = -Mathf.PI * 2;
                }
                else {
                    while (da > 0.0f) {
                        da -= Mathf.PI * 2;
                    }

                    if (da >= -1e-5) {
                        return;
                    }
                }
            }

            // Split arc into max 90 degree segments.
            int ndivs = Mathf.Max(1, Mathf.Min((int) (Mathf.Abs(da) / (Mathf.PI * 0.5f) + 0.5f), 5));
            float hda = (da / ndivs) / 2.0f;
            float kappa = Mathf.Abs(4.0f / 3.0f * (1.0f - Mathf.Cos(hda)) / Mathf.Sin(hda));

            if (dir == PathWinding.counterClockwise) {
                kappa = -kappa;
            }

            PathCommand move = (forceMoveTo || _commands.Count == 0) ? PathCommand.moveTo : PathCommand.lineTo;
            float px = 0, py = 0, ptanx = 0, ptany = 0;

            for (int i = 0; i <= ndivs; i++) {
                float a = a0 + da * (i / (float) ndivs);
                float dx = Mathf.Cos(a);
                float dy = Mathf.Sin(a);
                float x = cx + dx * r;
                float y = cy + dy * r;
                float tanx = -dy * r * kappa;
                float tany = dx * r * kappa;

                if (i == 0) {
                    float x1 = x, y1 = y;
                    if (transform != null) {
                        transform.mapXY(x1, y1, out x1, out y1);
                    }

                    if (move == PathCommand.moveTo) {
                        _appendMoveTo(x1, y1);
                    }
                    else {
                        _appendLineTo(x1, y1);
                    }
                }
                else {
                    float c1x = px + ptanx;
                    float c1y = py + ptany;
                    float c2x = x - tanx;
                    float c2y = y - tany;
                    float x1 = x;
                    float y1 = y;
                    if (transform != null) {
                        transform.mapXY(c1x, c1y, out c1x, out c1y);
                        transform.mapXY(c2x, c2y, out c2x, out c2y);
                        transform.mapXY(x1, y1, out x1, out y1);
                    }

                    _appendBezierTo(c1x, c1y, c2x, c2y, x1, y1);
                }

                px = x;
                py = y;
                ptanx = tanx;
                ptany = tany;
            }
        }

        public static uiPath fromPath(Path path) {
            D.assert(path != null);

            uiPath uipath;
            bool exists = uiPathCacheManager.tryGetUiPath(path.pathKey, out uipath);
            if (exists) {
                return uipath;
            }
            
            uipath._updateRRectFlag(path.isNaiveRRect, (uiPathShapeHint)path.shapeHint, path.rRectCorner);
            
            var i = 0;
            var _commands = path.commands;
            while (i < _commands.Count) {
                var cmd = (uiPathCommand) _commands[i];
                switch (cmd) {
                    case uiPathCommand.moveTo: {
                        float x = _commands[i + 1];
                        float y = _commands[i + 2];
                        uipath._appendMoveTo(x, y);
                    }
                        i += 3;
                        break;
                    case uiPathCommand.lineTo: {
                        float x = _commands[i + 1];
                        float y = _commands[i + 2];

                        uipath._appendLineTo(x, y);
                    }
                        i += 3;
                        break;
                    case uiPathCommand.bezierTo: {
                        float c1x = _commands[i + 1];
                        float c1y = _commands[i + 2];
                        float c2x = _commands[i + 3];
                        float c2y = _commands[i + 4];
                        float x1 = _commands[i + 5];
                        float y1 = _commands[i + 6];

                        uipath._appendBezierTo(c1x, c1y, c2x, c2y, x1, y1);
                    }
                        i += 7;
                        break;
                    case uiPathCommand.close:
                        uipath._appendClose();
                        i++;
                        break;
                    case uiPathCommand.winding:
                        uipath._appendWinding(_commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        D.assert(false, () => "unknown cmd: " + cmd);
                        break;
                }
            }

            return uipath;
        }
    }
}