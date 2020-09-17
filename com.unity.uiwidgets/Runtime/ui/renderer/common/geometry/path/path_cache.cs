using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class uiPathCache : PoolObject {
        float _distTol;
        float _tessTol;

        List<uiPathPath> _paths = new List<uiPathPath>();
        List<uiPathPoint> _points = new List<uiPathPoint>();

        float _scale;

        bool _fillConvex;

        //mesh cache
        uiMeshMesh _fillMesh;

        public uiMeshMesh fillMesh {
            get { return _fillMesh; }
        }

        uiMeshMesh _strokeMesh;

        public uiMeshMesh strokeMesh {
            get { return _strokeMesh; }
        }

        float _strokeWidth;
        StrokeCap _lineCap;
        StrokeJoin _lineJoin;
        float _miterLimit;
        float _fringe;

        uiPath.uiPathShapeHint _shapeHint;

        public static uiPathCache create(float scale, uiPath.uiPathShapeHint shapeHint) {
            uiPathCache newPathCache = ObjectPool<uiPathCache>.alloc();
            newPathCache._distTol = 0.01f / scale;
            newPathCache._tessTol = 0.25f / scale;
            newPathCache._scale = scale;
            newPathCache._shapeHint = shapeHint;
            return newPathCache;
        }

        public bool canReuse(float scale) {
            if (_scale != scale) {
                return false;
            }

            return true;
        }

        public bool canSkipAAHairline {
            get { return _shapeHint == uiPath.uiPathShapeHint.Rect; }
        }

        public override void clear() {
            _paths.Clear();
            _points.Clear();
            ObjectPool<uiMeshMesh>.release(_fillMesh);
            _fillMesh = null;

            ObjectPool<uiMeshMesh>.release(_strokeMesh);
            _strokeMesh = null;

            _shapeHint = uiPath.uiPathShapeHint.Other;
        }

        public uiPathCache() {
        }

        public void addPath() {
            _paths.Add(uiPathPath.create(
                first: _points.Count,
                winding: uiPathWinding.counterClockwise
            ));
        }

        public void addPoint(float x, float y, uiPointFlags flags) {
            _addPoint(uiPathPoint.create(x: x, y: y, flags: flags));
        }

        void _addPoint(uiPathPoint point) {
            if (_paths.Count == 0) {
                addPath();
                addPoint(0, 0, uiPointFlags.corner);
            }

            var path = _paths[_paths.Count - 1];
            if (path.count > 0) {
                var pt = _points[_points.Count - 1];
                if (uiPathUtils.ptEquals(pt.x, pt.y, point.x, point.y, _distTol)) {
                    pt.flags |= point.flags;
                    _points[_points.Count - 1] = pt;
                    return;
                }
            }

            _points.Add(point);
            path.count++;
            _paths[_paths.Count - 1] = path;
        }

        public void tessellateBezier(
            float x2, float y2,
            float x3, float y3, float x4, float y4,
            uiPointFlags flags) {
            float x1, y1;
            if (_points.Count == 0) {
                x1 = 0;
                y1 = 0;
            }
            else {
                var pt = _points[_points.Count - 1];
                x1 = pt.x;
                y1 = pt.y;
            }

            if (x1 == x2 && x1 == x3 && x1 == x4 &&
                y1 == y2 && y1 == y3 && y1 == y4) {
                return;
            }

            var points = uiTessellationGenerator.tessellateBezier(x1, y1, x2, y2, x3, y3, x4, y4, _tessTol);
            D.assert(points.Count > 0);
            for (int i = 0; i < points.Count; i++) {
                var point = points[i];
                if (i == points.Count - 1) {
                    _addPoint(uiPathPoint.create(
                        x: point.x + x1,
                        y: point.y + y1,
                        flags: flags
                    ));
                }
                else {
                    _addPoint(uiPathPoint.create(
                        x: point.x + x1,
                        y: point.y + y1
                    ));
                }
            }
        }

        public void closePath() {
            if (_paths.Count == 0) {
                return;
            }

            var path = _paths[_paths.Count - 1];
            path.closed = true;
            _paths[_paths.Count - 1] = path;
        }

        public void pathWinding(uiPathWinding winding) {
            if (_paths.Count == 0) {
                return;
            }

            var path = _paths[_paths.Count - 1];
            path.winding = winding;
            _paths[_paths.Count - 1] = path;
        }

        public void normalize() {
            var points = _points;
            var paths = _paths;
            for (var j = 0; j < paths.Count; j++) {
                var path = paths[j];
                if (path.count <= 1) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;

                var p0 = points[ip0];
                var p1 = points[ip1];
                if (uiPathUtils.ptEquals(p0.x, p0.y, p1.x, p1.y, _distTol)) {
                    path.count--;
                    path.closed = true;
                    paths[j] = path;
                }

                if (path.count > 2) {
                    if (path.winding == uiPathWinding.clockwise) {
                        uiPathUtils.polyReverse(points, path.first, path.count);
                    }
                }
            }
        }

        void _calculateJoins(float w, StrokeJoin lineJoin, float miterLimit) {
            float iw = w > 0.0f ? 1.0f / w : 0.0f;

            var points = _points;
            var paths = _paths;
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 1) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;

                for (var j = 0; j < path.count; j++) {
                    var p0 = points[ip0];
                    var p1 = points[ip1];
                    p0.dx = p1.x - p0.x;
                    p0.dy = p1.y - p0.y;
                    p0.len = uiPathUtils.normalize(ref p0.dx, ref p0.dy);
                    points[ip0] = p0;
                    ip0 = ip1++;
                }

                ip0 = path.first + path.count - 1;
                ip1 = path.first;
                path.convex = true;
                for (var j = 0; j < path.count; j++) {
                    var p0 = points[ip0];
                    var p1 = points[ip1];
                    float dlx0 = p0.dy;
                    float dly0 = -p0.dx;
                    float dlx1 = p1.dy;
                    float dly1 = -p1.dx;

                    // Calculate extrusions
                    p1.dmx = (dlx0 + dlx1) * 0.5f;
                    p1.dmy = (dly0 + dly1) * 0.5f;
                    float dmr2 = p1.dmx * p1.dmx + p1.dmy * p1.dmy;
                    if (dmr2 > 0.000001f) {
                        float scale = 1.0f / dmr2;
                        if (scale > 600.0f) {
                            scale = 600.0f;
                        }

                        p1.dmx *= scale;
                        p1.dmy *= scale;
                    }

                    // Clear flags, but keep the corner.
                    p1.flags &= uiPointFlags.corner;

                    // Keep track of left turns.
                    float cross = p1.dx * p0.dy - p0.dx * p1.dy;
                    
                    if (cross > 0.0f) {
                        p1.flags |= uiPointFlags.left;
                    } else if (cross < 0.0f) {
                        path.convex = false;
                    }

                    // Calculate if we should use bevel or miter for inner join.
                    float limit = Mathf.Max(1.01f, Mathf.Min(p0.len, p1.len) * iw);
                    if (dmr2 * limit * limit < 1.0f) {
                        p1.flags |= uiPointFlags.innerBevel;
                    }

                    // Check to see if the corner needs to be beveled.
                    if ((p1.flags & uiPointFlags.corner) != 0) {
                        if (lineJoin == StrokeJoin.bevel ||
                            lineJoin == StrokeJoin.round || dmr2 * miterLimit * miterLimit < 1.0f) {
                            p1.flags |= uiPointFlags.bevel;
                        }
                    }

                    points[ip0] = p0;
                    points[ip1] = p1;

                    ip0 = ip1++;
                }

                paths[i] = path;
            }
        }

        uiVertexUV _expandStroke(float w, float fringe, StrokeCap lineCap, StrokeJoin lineJoin, float miterLimit) {
            float aa = fringe;
            float u0 = 0.0f, u1 = 1.0f;
            int ncap = 0;
            if (lineCap == StrokeCap.round || lineJoin == StrokeJoin.round) {
                ncap = uiPathUtils.curveDivs(w, Mathf.PI, _tessTol);
            }

            w += aa * 0.5f;

            if (aa == 0.0f) {
                u0 = 0.5f;
                u1 = 0.5f;
            }
            _calculateJoins(w, lineJoin, miterLimit);

            var points = _points;
            var paths = _paths;

            var cvertices = 0;
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 1) {
                    continue;
                }

                cvertices += path.count * 2;
                cvertices += 8;
            }

            var _vertices = ObjectPool<uiList<Vector3>>.alloc();
            _vertices.SetCapacity(cvertices);
            var _uv = ObjectPool<uiList<Vector2>>.alloc();
            _uv.SetCapacity(cvertices);
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 1) {
                    continue;
                }

                path.istroke = _vertices.Count;

                int s, e, ip0, ip1;
                if (path.closed) {
                    ip0 = path.first + path.count - 1;
                    ip1 = path.first;
                    s = 0;
                    e = path.count;
                }
                else {
                    ip0 = path.first;
                    ip1 = path.first + 1;
                    s = 1;
                    e = path.count - 1;
                }

                var p0 = points[ip0];
                var p1 = points[ip1];

                if (!path.closed) {
                    if (lineCap == StrokeCap.butt) {
                        _vertices.buttCapStart(_uv, p0, p0.dx, p0.dy, w, 0.0f, aa, u0, u1);
                    }
                    else if (lineCap == StrokeCap.square) {
                        _vertices.buttCapStart(_uv, p0, p0.dx, p0.dy, w, w, aa, u0, u1);
                    }
                    else {
                        // round
                        _vertices.roundCapStart(_uv, p0, p0.dx, p0.dy, w, ncap, u0, u1);
                    }
                }

                for (var j = s; j < e; j++) {
                    p0 = points[ip0];
                    p1 = points[ip1];

                    if ((p1.flags & (uiPointFlags.bevel | uiPointFlags.innerBevel)) != 0) {
                        if (lineJoin == StrokeJoin.round) {
                            _vertices.roundJoin(_uv, p0, p1, w, w, ncap, u0, u1, aa);
                        }
                        else {
                            _vertices.bevelJoin(_uv, p0, p1, w, w, u0, u1, aa);
                        }
                    }
                    else {
                        _vertices.Add(new Vector2(p1.x + p1.dmx * w, p1.y + p1.dmy * w));
                        _vertices.Add(new Vector2(p1.x - p1.dmx * w, p1.y - p1.dmy * w));
                        _uv.Add(new Vector2(u0, 1));
                        _uv.Add(new Vector2(u1, 1));
                    }

                    ip0 = ip1++;
                }

                if (!path.closed) {
                    p0 = points[ip0];
                    p1 = points[ip1];
                    if (lineCap == StrokeCap.butt) {
                        _vertices.buttCapEnd(_uv, p1, p0.dx, p0.dy, w, 0.0f, aa, u0, u1);
                    }
                    else if (lineCap == StrokeCap.square) {
                        _vertices.buttCapEnd(_uv, p1, p0.dx, p0.dy, w, w, aa, u0, u1);
                    }
                    else {
                        // round
                        _vertices.roundCapEnd(_uv, p1, p0.dx, p0.dy, w, ncap, u0, u1);
                    }
                }
                else {
                    _vertices.Add(_vertices[path.istroke]);
                    _vertices.Add(_vertices[path.istroke + 1]);
                    _uv.Add(new Vector2(u0, 1));
                    _uv.Add(new Vector2(u1, 1));
                }

                path.nstroke = _vertices.Count - path.istroke;
                paths[i] = path;
            }
            D.assert(_uv.Count == _vertices.Count);

            return new uiVertexUV {
                strokeVertices = _vertices,
                strokeUV = _uv,
            };
        }

        uiVertexUV _expandFill(float fringe) {
            float aa = canSkipAAHairline ? 0f : fringe;
            float woff = aa * 0.5f;
            var points = _points;
            var paths = _paths;
            _calculateJoins(fringe, StrokeJoin.miter, 4.0f);

            var cvertices = 0;
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 2) {
                    continue;
                }

                cvertices += path.count;
            }

            _fillConvex = false;
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 2) {
                    continue;
                }

                if (_fillConvex) {
                    // if more than two paths, convex is false.
                    _fillConvex = false;
                    break;
                }

                if (!path.convex) {
                    // if not convex, convex is false.
                    break;
                }

                _fillConvex = true;
            }

            var _vertices = ObjectPool<uiList<Vector3>>.alloc();
            _vertices.SetCapacity(cvertices);
            var _uv = ObjectPool<uiList<Vector2>>.alloc();
            _uv.SetCapacity(cvertices);
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 2) {
                    continue;
                }

                path.ifill = _vertices.Count;
                for (var j = 0; j < path.count; j++) {
                    var p = points[path.first + j];
                    if (aa > 0.0f) {
                        _vertices.Add(new Vector2(p.x + p.dmx * woff, p.y + p.dmy * woff));
                    }
                    else {
                        _vertices.Add(new Vector2(p.x, p.y));
                    }

                    _uv.Add(new Vector2(0.5f, 1.0f));
                }

                path.nfill = _vertices.Count - path.ifill;
                paths[i] = path;
            }

            uiList<Vector3> _strokeVertices = null;
            uiList<Vector2> _strokeUV = null;
            if (aa > 0.0f) {
                _strokeVertices = ObjectPool<uiList<Vector3>>.alloc();
                _strokeUV = ObjectPool<uiList<Vector2>>.alloc();
                cvertices = 0;
                for (var i = 0; i < paths.Count; i++) {
                    var path = paths[i];
                    if (path.count <= 2) {
                        continue;
                    }

                    cvertices += path.count * 2;
                }
                _strokeVertices.SetCapacity(cvertices);
                _strokeUV.SetCapacity(cvertices);

                float lw = _fillConvex ? woff : aa + woff;
                float rw = aa - woff;
                float lu = _fillConvex ? 0.5f : 0.0f;
                float ru = 1.0f;
                
                for (var i = 0; i < paths.Count; i++) {
                    var path = paths[i];
                    if (path.count <= 2) {
                        continue;
                    }

                    path.istroke = _strokeVertices.Count;
                    for (var j = 0; j < path.count; j++) {
                        var p = points[path.first + j];
                        _strokeVertices.Add(new Vector2(p.x + p.dmx * lw, p.y + p.dmy * lw));
                        _strokeUV.Add(new Vector2(lu, 1.0f));
                        _strokeVertices.Add(new Vector2(p.x - p.dmx * rw, p.y - p.dmy * rw));
                        _strokeUV.Add(new Vector2(ru, 1.0f));
                    }

                    path.nstroke = _strokeVertices.Count - path.istroke;
                    paths[i] = path;
                }
            }

            return new uiVertexUV {
                fillVertices = _vertices,
                fillUV = _uv,
                strokeVertices = _strokeVertices,
                strokeUV = _strokeUV,
            };
        }

        public void computeStrokeMesh(float strokeWidth, float fringe, StrokeCap lineCap, StrokeJoin lineJoin, float miterLimit) {
            if (_strokeMesh != null &&
                _fillMesh == null && // Ensure that the cached stroke mesh was not calculated in computeFillMesh
                _strokeWidth == strokeWidth &&
                _fringe == fringe &&
                _lineCap == lineCap &&
                _lineJoin == lineJoin &&
                _miterLimit == miterLimit) {
                return;
            }

            var verticesUV = _expandStroke(strokeWidth, fringe, lineCap, lineJoin, miterLimit);

            var paths = _paths;

            var cindices = 0;
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 1) {
                    continue;
                }

                if (path.nstroke > 0) {
                    D.assert(path.nstroke >= 2);
                    cindices += (path.nstroke - 2) * 3;
                }
            }

            var indices = ObjectPool<uiList<int>>.alloc();
            indices.SetCapacity(cindices);
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 1) {
                    continue;
                }

                if (path.nstroke > 0) {
                    for (var j = 2; j < path.nstroke; j++) {
                        if ((j & 1) == 0) {
                            indices.Add(path.istroke + j - 1);
                            indices.Add(path.istroke + j - 2);
                            indices.Add(path.istroke + j);
                        }
                        else {
                            indices.Add(path.istroke + j - 2);
                            indices.Add(path.istroke + j - 1);
                            indices.Add(path.istroke + j);
                        }
                    }
                }
            }

            D.assert(indices.Count == cindices);

            ObjectPool<uiMeshMesh>.release(_strokeMesh);
            _strokeMesh = uiMeshMesh.create(null, verticesUV.strokeVertices, indices, verticesUV.strokeUV);
            ObjectPool<uiMeshMesh>.release(_fillMesh);
            _fillMesh = null;
            _strokeWidth = strokeWidth;
            _fringe = fringe;
            _lineCap = lineCap;
            _lineJoin = lineJoin;
            _miterLimit = miterLimit;
            return;
        }

        public void computeFillMesh(float fringe, out bool convex) {
            if (_fillMesh != null && (fringe != 0.0f || _strokeMesh != null) && _fringe == fringe) {
                convex = _fillConvex;
                return;
            }

            var verticesUV = _expandFill(fringe);
            convex = _fillConvex;

            var paths = _paths;

            var cindices = 0;
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 2) {
                    continue;
                }

                if (path.nfill > 0) {
                    D.assert(path.nfill >= 2);
                    cindices += (path.nfill - 2) * 3;
                }
            }

            var indices = ObjectPool<uiList<int>>.alloc();
            indices.SetCapacity(cindices);
            for (var i = 0; i < paths.Count; i++) {
                var path = paths[i];
                if (path.count <= 2) {
                    continue;
                }

                if (path.nfill > 0) {
                    for (var j = 2; j < path.nfill; j++) {
                        indices.Add(path.ifill);
                        indices.Add(path.ifill + j);
                        indices.Add(path.ifill + j - 1);
                    }
                }
            }

            D.assert(indices.Count == cindices);

            if (verticesUV.strokeVertices != null) {
                cindices = 0;
                for (var i = 0; i < paths.Count; i++) {
                    var path = paths[i];
                    if (path.count <= 2) {
                        continue;
                    }

                    if (path.nstroke > 0) {
                        D.assert(path.nstroke >= 6);
                        cindices += path.nstroke * 3;
                    }
                }

                var strokeIndices = ObjectPool<uiList<int>>.alloc();
                strokeIndices.SetCapacity(cindices);
                for (var i = 0; i < paths.Count; i++) {
                    var path = paths[i];
                    if (path.count <= 2) {
                        continue;
                    }

                    if (path.nstroke > 0) {
                        strokeIndices.Add(path.istroke + path.nstroke - 1);
                        strokeIndices.Add(path.istroke + path.nstroke - 2);
                        strokeIndices.Add(path.istroke);
                        strokeIndices.Add(path.istroke + path.nstroke - 1);
                        strokeIndices.Add(path.istroke);
                        strokeIndices.Add(path.istroke + 1);
                        for (var j = 2; j < path.nstroke; j++) {
                            if ((j & 1) == 0) {
                                strokeIndices.Add(path.istroke + j - 1);
                                strokeIndices.Add(path.istroke + j - 2);
                                strokeIndices.Add(path.istroke + j);
                            }
                            else {
                                strokeIndices.Add(path.istroke + j - 2);
                                strokeIndices.Add(path.istroke + j - 1);
                                strokeIndices.Add(path.istroke + j);
                            }
                        }
                    }
                }

                D.assert(strokeIndices.Count == cindices);

                ObjectPool<uiMeshMesh>.release(_strokeMesh);
                _strokeMesh = uiMeshMesh.create(null, verticesUV.strokeVertices, strokeIndices, verticesUV.strokeUV);
            }

            var mesh = uiMeshMesh.create(null, verticesUV.fillVertices, indices, verticesUV.fillUV);
            _fillMesh = mesh;
            _fringe = fringe;
        }
    }
}