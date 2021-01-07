using System;
using System.Collections.Generic;
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
        readonly _SystemFontsNotifier _systemFonts = new _SystemFontsNotifier();
        public _SystemFontsNotifier  systemFonts {
            get { return _systemFonts; }
        }

        protected virtual ImageCache createImageCache() {
            return new ImageCache();
        }
    }

    public static partial class painting_ {
        public static ImageCache imageCache => PaintingBinding.instance.imageCache;
    }
    
    public class _SystemFontsNotifier : Listenable {
        HashSet<VoidCallback> _systemFontsCallbacks = new HashSet<VoidCallback>();

        void notifyListeners () {
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
}