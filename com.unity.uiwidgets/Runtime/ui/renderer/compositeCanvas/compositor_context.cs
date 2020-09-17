using System;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class CompositorContext {
        public class ScopedFrame : IDisposable {
            readonly CompositorContext _context;
            readonly Canvas _canvas;
            readonly bool _instrumentationEnabled;

            public ScopedFrame(CompositorContext context, Canvas canvas, bool instrumentationEnabled) {
                _context = context;
                _canvas = canvas;
                _instrumentationEnabled = instrumentationEnabled;
                _context._beginFrame(this, _instrumentationEnabled);
            }

            public CompositorContext context() {
                return _context;
            }

            public Canvas canvas() {
                return _canvas;
            }

            public bool raster(LayerTree layerTree, bool ignoreRasterCache) {
                layerTree.preroll(this, ignoreRasterCache);
                layerTree.paint(this, ignoreRasterCache);
                return true;
            }

            public void Dispose() {
                _context._endFrame(this, _instrumentationEnabled);
            }
        }

        readonly RasterCache _rasterCache;
        readonly Stopwatch _frameTime;

        public CompositorContext() {
            _rasterCache = new RasterCache();
            _frameTime = new Stopwatch();
        }

        public ScopedFrame acquireFrame(Canvas canvas, bool instrumentationEnabled) {
            return new ScopedFrame(this, canvas, instrumentationEnabled);
        }

        public void onGrContextCreated(Surface surface) {
            _rasterCache.clear();
            _rasterCache.meshPool = surface.getMeshPool();
        }

        public void onGrContextDestroyed() {
            _rasterCache.clear();
        }

        public RasterCache rasterCache() {
            return _rasterCache;
        }

        public Stopwatch frameTime() {
            return _frameTime;
        }

        void _beginFrame(ScopedFrame frame, bool enableInstrumentation) {
            if (enableInstrumentation) {
                _frameTime.start();
            }
        }

        void _endFrame(ScopedFrame frame, bool enableInstrumentation) {
            _rasterCache.sweepAfterFrame();
            if (enableInstrumentation) {
                _frameTime.stop();
            }
        }
    }
}