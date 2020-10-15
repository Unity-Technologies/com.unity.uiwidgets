using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class PaintingBinding : GestureBinding {
         protected override void initInstances() {
             base.initInstances();
             instance = this;
             _imageCache = createImageCache();
        
             if (shaderWarmUp != null) {
                 shaderWarmUp.execute();
             }
         }

        public new static PaintingBinding instance {
            get { return (PaintingBinding) GestureBinding.instance; }
            set { Window.instance._binding = value; }
        }

        public static ShaderWarmUp shaderWarmUp = new DefaultShaderWarmUp();

        public ImageCache imageCache => _imageCache;
        ImageCache _imageCache;


        protected virtual ImageCache createImageCache() {
            return new ImageCache();
        }
        //
        // public Future<Codec> instantiateImageCodec(byte[] bytes,
        //     int? cacheWidth = null,
        //     int? cacheHeight = null) {
        //     D.assert(cacheWidth == null || cacheWidth > 0);
        //     D.assert(cacheHeight == null || cacheHeight > 0);
        //
        //     Future<object> f = instantiateImageCodec(null).then<object>(c => {
        //         return FutureOr.null_;
        //     }).to<Codec>().asOf<object>();
        //     return ui.instantiateImageCodec(
        //         bytes,
        //         targetWidth: cacheWidth,
        //         targetHeight: cacheHeight
        //     );
        // }
    }

    public static partial class painting_ {
        public static ImageCache imageCache => PaintingBinding.instance.imageCache;
    }
}