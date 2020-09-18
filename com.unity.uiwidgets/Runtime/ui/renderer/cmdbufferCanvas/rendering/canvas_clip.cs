using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class ClipElement : PoolObject {
        public int saveCount;
        public uiMeshMesh mesh;
        public bool convex;
        public bool isRect;
        public uiRect? rect { get; private set; }

        uint _genId;
        bool _isIntersectionOfRects;
        uiRect _bound;
        uiMatrix3? _invMat;

        public ClipElement() {
        }

        public override void clear() {
            ObjectPool<uiMeshMesh>.release(mesh);
            saveCount = 0;
            mesh = null;
            convex = false;
            isRect = false;
            _genId = 0;
            _isIntersectionOfRects = false;
            _invMat = null;
        }

        public static ClipElement create(int saveCount, uiPath uiPath, uiMatrix3 matrix, float scale) {
            ClipElement newElement = ObjectPool<ClipElement>.alloc();

            newElement.saveCount = saveCount;

            var pathCache = uiPath.flatten(scale);
            pathCache.computeFillMesh(0.0f, out newElement.convex);
            var fillMesh = pathCache.fillMesh;
            newElement.mesh = fillMesh.transform(matrix);

            var vertices = newElement.mesh.vertices;
            if (newElement.convex && vertices.Count == 4 && matrix.rectStaysRect() &&
                (Mathf.Abs(vertices[0].x - vertices[1].x) < 1e-6 && Mathf.Abs(vertices[1].y - vertices[2].y) < 1e-6 &&
                 Mathf.Abs(vertices[2].x - vertices[3].x) < 1e-6 && Mathf.Abs(vertices[3].y - vertices[0].y) < 1e-6 ||
                 Mathf.Abs(vertices[0].y - vertices[1].y) < 1e-6 && Mathf.Abs(vertices[1].x - vertices[2].x) < 1e-6 &&
                 Mathf.Abs(vertices[2].y - vertices[3].y) < 1e-6 && Mathf.Abs(vertices[3].x - vertices[0].x) < 1e-6)) {
                newElement.isRect = true;
                newElement.rect = newElement.mesh.bounds;
            }
            else {
                newElement.isRect = false;
                newElement.rect = null;
            }

            return newElement;
        }

        public void setRect(uiRect rect) {
            D.assert(ClipStack.invalidGenID != _genId);
            D.assert(isRect && uiRectHelper.contains(this.rect.Value, rect));
            this.rect = rect;
        }

        public void setEmpty() {
            _genId = ClipStack.emptyGenID;
            _isIntersectionOfRects = false;
            _bound = uiRectHelper.zero;
        }

        public void updateBoundAndGenID(ClipElement prior) {
            _genId = ClipStack.getNextGenID();
            _isIntersectionOfRects = false;

            if (isRect) {
                _bound = rect.Value;
                if (prior == null || prior.isIntersectionOfRects()) {
                    _isIntersectionOfRects = true;
                }
            }
            else {
                _bound = mesh.bounds;
            }

            if (prior != null) {
                _bound = uiRectHelper.intersect(_bound, prior.getBound());
            }

            if (_bound.isEmpty) {
                setEmpty();
            }
        }

        public bool isEmpty() {
            D.assert(ClipStack.invalidGenID != _genId);
            return getGenID() == ClipStack.emptyGenID;
        }

        public uiRect getBound() {
            D.assert(ClipStack.invalidGenID != _genId);
            return _bound;
        }

        public bool isIntersectionOfRects() {
            D.assert(ClipStack.invalidGenID != _genId);
            return _isIntersectionOfRects;
        }

        public uint getGenID() {
            D.assert(ClipStack.invalidGenID != _genId);
            return _genId;
        }

        bool _convexContains(uiRect rect) {
            if (mesh.vertices.Count <= 2) {
                return false;
            }

            for (var i = 0; i < mesh.vertices.Count; i++) {
                var p1 = mesh.vertices[i];
                var p0 = mesh.vertices[i == mesh.vertices.Count - 1 ? 0 : i + 1];

                var v = p1 - p0;
                if (v.x == 0.0 && v.y == 0.0) {
                    continue;
                }

                float yL = v.y * (rect.left - p0.x);
                float xT = v.x * (rect.top - p0.y);
                float yR = v.y * (rect.right - p0.x);
                float xB = v.x * (rect.bottom - p0.y);

                if ((xT < yL) || (xT < yR) || (xB < yL) || (xB < yR)) {
                    return false;
                }
            }

            return true;
        }

        public bool contains(uiRect rect) {
            if (isRect) {
                return uiRectHelper.contains(this.rect.Value, rect);
            }

            if (convex) {
                if (mesh.matrix != null && !mesh.matrix.Value.isIdentity()) {
                    if (_invMat == null) {
                        _invMat = mesh.matrix.Value.invert();
                    }

                    rect = _invMat.Value.mapRect(rect);
                }

                return _convexContains(rect);
            }

            return false;
        }
    }

    class ClipStack : PoolObject {
        static uint _genId = wideOpenGenID;

        public static uint getNextGenID() {
            return ++_genId;
        }

        public const uint invalidGenID = 0;

        public const uint emptyGenID = 1;

        public const uint wideOpenGenID = 2;

        public readonly List<ClipElement> stack = new List<ClipElement>(32);

        ClipElement _lastElement;
        uiRect _bound;
        int _saveCount;

        public static ClipStack create() {
            return ObjectPool<ClipStack>.alloc();
        }

        public override void clear() {
            _saveCount = 0;
            _lastElement = null;
            foreach (var clipelement in stack) {
                ObjectPool<ClipElement>.release(clipelement);
            }

            stack.Clear();
        }

        public void save() {
            _saveCount++;
        }

        public void restore() {
            _saveCount--;
            _restoreTo(_saveCount);
        }

        void _restoreTo(int saveCount) {
            while (_lastElement != null) {
                if (_lastElement.saveCount <= saveCount) {
                    break;
                }

                var lastelement = stack[stack.Count - 1];
                ObjectPool<ClipElement>.release(lastelement);

                stack.RemoveAt(stack.Count - 1);
                _lastElement = stack.Count == 0 ? null : stack[stack.Count - 1];
            }
        }

        public void clipPath(uiPath uiPath, uiMatrix3 matrix, float scale) {
            var element = ClipElement.create(_saveCount, uiPath, matrix, scale);
            _pushElement(element);
        }

        void _pushElement(ClipElement element) {
            ClipElement prior = _lastElement;
            if (prior != null) {
                if (prior.isEmpty()) {
                    ObjectPool<ClipElement>.release(element);
                    return;
                }

                if (prior.saveCount == _saveCount) {
                    // can not update prior if it's cross save count.
                    if (prior.isRect && element.isRect) {
                        var isectRect = uiRectHelper.intersect(prior.rect.Value, element.rect.Value);
                        if (isectRect.isEmpty) {
                            prior.setEmpty();
                            ObjectPool<ClipElement>.release(element);
                            return;
                        }

                        prior.setRect(isectRect);
                        var priorprior = stack.Count > 1 ? stack[stack.Count - 2] : null;
                        prior.updateBoundAndGenID(priorprior);
                        ObjectPool<ClipElement>.release(element);
                        return;
                    }

                    if (!uiRectHelper.overlaps(prior.getBound(), element.getBound())) {
                        prior.setEmpty();
                        ObjectPool<ClipElement>.release(element);
                        return;
                    }
                }
            }

            stack.Add(element);
            _lastElement = element;
            element.updateBoundAndGenID(prior);
        }

        public void getBounds(out uiRect? bound, out bool isIntersectionOfRects) {
            if (_lastElement == null) {
                bound = null;
                isIntersectionOfRects = false;
                return;
            }

            var element = _lastElement;
            bound = element.getBound();
            isIntersectionOfRects = element.isIntersectionOfRects();
        }
    }

    class ReducedClip : PoolObject {
        public uiRect? scissor;
        public List<ClipElement> maskElements = new List<ClipElement>();
        ClipElement _lastElement;

        public bool isEmpty() {
            return scissor != null && scissor.Value.isEmpty;
        }

        public ReducedClip() {
        }

        public override void clear() {
            scissor = null;
            maskElements.Clear();
            _lastElement = null;
        }

        public uint maskGenID() {
            var element = _lastElement;
            if (element == null) {
                return ClipStack.wideOpenGenID;
            }

            return element.getGenID();
        }

        public static ReducedClip create(ClipStack stack, uiRect layerBounds, uiRect queryBounds) {
            ReducedClip clip = ObjectPool<ReducedClip>.alloc();
            uiRect? stackBounds;
            bool iior;
            stack.getBounds(out stackBounds, out iior);

            if (stackBounds == null) {
                clip.scissor = layerBounds;
                return clip;
            }

            stackBounds = uiRectHelper.intersect(layerBounds, stackBounds.Value);
            if (iior) {
                clip.scissor = stackBounds;
                return clip;
            }

            queryBounds = uiRectHelper.intersect(stackBounds.Value, queryBounds);
            if (queryBounds.isEmpty) {
                clip.scissor = uiRectHelper.zero;
                return clip;
            }

            clip.scissor = queryBounds;
            clip._walkStack(stack, clip.scissor.Value);
            return clip;
        }

        void _walkStack(ClipStack stack, uiRect queryBounds) {
            foreach (var element in stack.stack) {
                if (element.isRect) {
                    continue;
                }

                if (element.contains(queryBounds)) {
                    continue;
                }

                maskElements.Add(element);
                _lastElement = element;
            }
        }
    }
}