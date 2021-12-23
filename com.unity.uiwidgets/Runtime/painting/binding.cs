using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class PaintingBinding : ServicesBinding {
        protected override void initInstances() {
            base.initInstances();
            instance = this;
            _imageCache = createImageCache();

            if (shaderWarmUp != null) {
                shaderWarmUp.execute();
            }
        }
        
        public new static PaintingBinding instance {
            get { return (PaintingBinding) ServicesBinding.instance; }
            set { Window.instance._binding = value; }
        }

        public static ShaderWarmUp shaderWarmUp = new DefaultShaderWarmUp();

        public ImageCache imageCache => _imageCache;
        ImageCache _imageCache;
        
        public Listenable  systemFonts {
            get { return _systemFonts; }
        }
        
        readonly _SystemFontsNotifier _systemFonts = new _SystemFontsNotifier();
       
        

        protected virtual ImageCache createImageCache() {
            return new ImageCache();
        }

        public Future<ui.Codec> instantiateImageCodec(byte[] bytes,
            int? cacheWidth = null,
            int? cacheHeight = null
        ) {
            D.assert(cacheWidth == null || cacheWidth > 0);
            D.assert(cacheHeight == null || cacheHeight > 0);
            return ui_.instantiateImageCodec(
                bytes,
                targetWidth: cacheWidth,
                targetHeight: cacheHeight
            );
        }
    }

    internal class _SystemFontsNotifier : Listenable {
        readonly HashSet<VoidCallback> _systemFontsCallbacks = new HashSet<VoidCallback>();

        void notifyListeners() {
            foreach (VoidCallback callback in _systemFontsCallbacks) {
                callback();
            }
        }

        public void addListener(VoidCallback listener) {
            _systemFontsCallbacks.Add(listener);
        }

        public void removeListener(VoidCallback listener) {
            _systemFontsCallbacks.Remove(listener);
        }
    }


    public static partial class painting_ {
        public static ImageCache imageCache => PaintingBinding.instance.imageCache;
    }
}