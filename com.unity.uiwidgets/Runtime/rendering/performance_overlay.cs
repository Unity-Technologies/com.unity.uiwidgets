using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public enum PerformanceOverlayOption {
        displayRasterizerStatistics,
        visualizeRasterizerStatistics,
        displayEngineStatistics,
        visualizeEngineStatistics,
    }


    public class RenderPerformanceOverlay : RenderBox {
        public RenderPerformanceOverlay(
            int optionsMask = 0,
            int rasterizerThreshold = 0,
            bool checkerboardRasterCacheImages = false,
            bool checkerboardOffscreenLayers = false
        ) {
            _optionsMask = optionsMask;
            _rasterizerThreshold = rasterizerThreshold;
            _checkerboardRasterCacheImages = checkerboardRasterCacheImages;
            _checkerboardOffscreenLayers = checkerboardOffscreenLayers;
        }

        public int optionsMask {
            get { return _optionsMask; }
            set {
                if (value == _optionsMask) {
                    return;
                }

                _optionsMask = value;
                markNeedsPaint();
            }
        }

        int _optionsMask;

        public int rasterizerThreshold {
            get {
                return _rasterizerThreshold;
            }
            set {
                if (value == _rasterizerThreshold)
                    return;
                _rasterizerThreshold = value;
                markNeedsPaint();
            }
        }
        int _rasterizerThreshold;

        public bool checkerboardRasterCacheImages {
            get {
                return _checkerboardRasterCacheImages;
            }
            set {
                if (value == _checkerboardRasterCacheImages)
                    return;
                _checkerboardRasterCacheImages = value;
                markNeedsPaint();
            }
        }
        bool _checkerboardRasterCacheImages;

        public bool checkerboardOffscreenLayers {
            get { return _checkerboardOffscreenLayers; }
            set {
                if (value == _checkerboardOffscreenLayers)
                    return;
                _checkerboardOffscreenLayers = value;
                markNeedsPaint();
            }
        }
        bool _checkerboardOffscreenLayers;
        
        

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            return 0.0f;
        }

        float _intrinsicHeight {
            get {
                const float kDefaultGraphHeight = 80.0f;
                
                float result = 0.0f;
                if (
                    ((optionsMask | (1 << (int) PerformanceOverlayOption.displayRasterizerStatistics)) > 0) ||
                    ((optionsMask | (1 << (int) PerformanceOverlayOption.visualizeRasterizerStatistics)) > 0)
                        )
                    result += kDefaultGraphHeight;
                if (((optionsMask | (1 << (int) PerformanceOverlayOption.displayEngineStatistics)) > 0) ||
                    ((optionsMask | (1 << (int) PerformanceOverlayOption.visualizeEngineStatistics)) > 0))
                    result += kDefaultGraphHeight;
                return result;
            }
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return _intrinsicHeight;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return _intrinsicHeight;
        }

        protected override void performResize() {
            size = constraints.constrain(new Size(float.PositiveInfinity, _intrinsicHeight));
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(needsCompositing);
            context.addLayer(new PerformanceOverlayLayer(
                overlayRect: Rect.fromLTWH(offset.dx, offset.dy, size.width, size.height),
                optionsMask: optionsMask,
                rasterizerThreshold: rasterizerThreshold,
                checkerboardRasterCacheImages: checkerboardRasterCacheImages,
                checkerboardOffscreenLayers: checkerboardOffscreenLayers
            ));
        }
    }
}