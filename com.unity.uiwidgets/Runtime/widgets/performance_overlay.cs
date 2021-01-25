using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public class PerformanceOverlay : LeafRenderObjectWidget {
        public PerformanceOverlay(
            Key key = null,
            int optionsMask = 0,
            int rasterizerThreshold = 0,
            bool checkerboardRasterCacheImages = false,
            bool checkerboardOffscreenLayers = false
            
        ) : base(key: key) {
            this.optionsMask = optionsMask;
            this.rasterizerThreshold = rasterizerThreshold;
            this.checkerboardOffscreenLayers = checkerboardOffscreenLayers;
            this.checkerboardRasterCacheImages = checkerboardRasterCacheImages;
        }

        public readonly int optionsMask;
        public readonly int rasterizerThreshold;
        public readonly bool checkerboardRasterCacheImages;
        public readonly bool checkerboardOffscreenLayers;
        
        

        public static PerformanceOverlay allEnabled(
            Key key = null,
            int rasterizerThreshold = 0,
            bool checkerboardRasterCacheImages = false,
            bool checkerboardOffscreenLayers = false
        ) {
            
            return new PerformanceOverlay(
                optionsMask :
                    1 <<  (int) PerformanceOverlayOption.displayRasterizerStatistics |
                    1 <<  (int) PerformanceOverlayOption.visualizeRasterizerStatistics |
                    1 <<  (int) PerformanceOverlayOption.displayEngineStatistics |
                    1 <<  (int) PerformanceOverlayOption.visualizeEngineStatistics
            );
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPerformanceOverlay(
                optionsMask: optionsMask,
                rasterizerThreshold: rasterizerThreshold,
                checkerboardRasterCacheImages: checkerboardRasterCacheImages,
                checkerboardOffscreenLayers: checkerboardOffscreenLayers
                );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderPerformanceOverlay _renderObject = (RenderPerformanceOverlay) renderObject;
            _renderObject.optionsMask = optionsMask;
             _renderObject.rasterizerThreshold = rasterizerThreshold;
        }
    }
}