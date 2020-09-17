using System;
using Unity.UIWidgets.flow;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.editor {
    public class Rasterizer {
        Surface _surface;
        CompositorContext _compositorContext;
        LayerTree _lastLayerTree;
        Action _nextFrameCallback;

        public Rasterizer() {
            _compositorContext = new CompositorContext();
        }

        public void setup(Surface surface) {
            _surface = surface;
            _compositorContext.onGrContextCreated(_surface);
        }

        public void teardown() {
            _compositorContext.onGrContextDestroyed();
            _surface = null;
            _lastLayerTree = null;
        }

        public LayerTree getLastLayerTree() {
            return _lastLayerTree;
        }

        public void drawLastLayerTree() {
            if (_lastLayerTree == null || _surface == null) {
                return;
            }

            _drawToSurface(_lastLayerTree);
        }

        public void draw(LayerTree layerTree) {
            _doDraw(layerTree);
        }

        public void setNextFrameCallback(Action callback) {
            _nextFrameCallback = callback;
        }

        public CompositorContext getCompositorContext() {
            return _compositorContext;
        }

        void _doDraw(LayerTree layerTree) {
            if (layerTree == null || _surface == null) {
                return;
            }

            if (_drawToSurface(layerTree)) {
                _lastLayerTree = layerTree;
            }
        }

        bool _drawToSurface(LayerTree layerTree) {
            D.assert(_surface != null);

            var frame = _surface.acquireFrame(
                layerTree.frameSize, layerTree.devicePixelRatio, layerTree.antiAliasing);
            if (frame == null) {
                return false;
            }

            var canvas = frame.getCanvas();

            using (var compositorFrame = _compositorContext.acquireFrame(canvas, true)) {
                if (compositorFrame != null && compositorFrame.raster(layerTree, false)) {
                    frame.submit();
                    _fireNextFrameCallbackIfPresent();
                    return true;
                }
                return false;
            }
        }

        void _fireNextFrameCallbackIfPresent() {
            if (_nextFrameCallback == null) {
                return;
            }

            var callback = _nextFrameCallback;
            _nextFrameCallback = null;
            callback();
        }
    }
}