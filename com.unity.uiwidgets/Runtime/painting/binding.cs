using Unity.UIWidgets.gestures;
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
    }

    public static partial class painting_ {
        public static ImageCache imageCache => PaintingBinding.instance.imageCache;
    }
}