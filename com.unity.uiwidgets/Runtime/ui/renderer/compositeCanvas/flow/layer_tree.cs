using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class LayerTree {
        Layer _rootLayer;

        public Layer rootLayer {
            get { return _rootLayer; }
            set { _rootLayer = value; }
        }

        Size _frameSize;

        public Size frameSize {
            get { return _frameSize; }
            set { _frameSize = value; }
        }

        float _devicePixelRatio;

        public float devicePixelRatio {
            get { return _devicePixelRatio; }
            set { _devicePixelRatio = value; }
        }
        
        int _antiAliasing;

        public int antiAliasing {
            get { return _antiAliasing; }
            set { _antiAliasing = value; }
        }

        static readonly Matrix3 _identityMatrix = Matrix3.I();

        public void preroll(CompositorContext.ScopedFrame frame, bool ignoreRasterCache = false) {
            var prerollContext = new PrerollContext {
                rasterCache = ignoreRasterCache ? null : frame.context().rasterCache(),
                devicePixelRatio = frame.canvas().getDevicePixelRatio(),
                antiAliasing = antiAliasing,
                cullRect = Rect.largest,
                frameTime = frame.context().frameTime()
            };

            _rootLayer.preroll(prerollContext, _identityMatrix);
        }

        public void paint(CompositorContext.ScopedFrame frame, bool ignoreRasterCache = false) {
            var paintContext = new PaintContext {
                canvas = frame.canvas(),
                rasterCache = ignoreRasterCache ? null : frame.context().rasterCache(),
                frameTime = frame.context().frameTime()
            };

            if (_rootLayer.needsPainting) {
                _rootLayer.paint(paintContext);
            }
        }
    }
}