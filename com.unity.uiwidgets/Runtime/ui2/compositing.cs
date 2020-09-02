using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.ui2 {
    public class Scene : NativeWrapperDisposable {
        internal Scene(IntPtr ptr) : base(ptr) {
        }

        protected override void DisposePtr(IntPtr ptr) {
            Scene_dispose(ptr);
        }

        public Promise<Image> toImage(int width, int height) {
            if (width <= 0 || height <= 0) {
                throw new Exception("Invalid image dimensions.");
            }

            var completer = new Promise<Image>(true);
            GCHandle completerHandle = GCHandle.Alloc(completer);

            IntPtr error =
                Scene_toImage(_ptr, width, height, _toImageCallback,
                    (IntPtr) completerHandle);
            if (error != null) {
                completerHandle.Free();
                throw new Exception(Marshal.PtrToStringAnsi(error));
            }

            return completer;
        }

        [MonoPInvokeCallback(typeof(Scene_toImageCallback))]
        static void _toImageCallback(IntPtr callbackHandle, IntPtr result) {
            GCHandle completerHandle = (GCHandle) callbackHandle;
            var completer = (Promise<Image>) completerHandle.Target;
            completerHandle.Free();

            if (result == IntPtr.Zero) {
                completer.Reject(new Exception("operation failed"));
            }
            else {
                var image = new Image(result);
                completer.Resolve(image);
            }
        }

        [DllImport(NativeBindings.dllName)]
        static extern void Scene_dispose(IntPtr ptr);

        delegate void Scene_toImageCallback(IntPtr callbackHandle, IntPtr result);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Scene_toImage(IntPtr ptr, int width, int height, Scene_toImageCallback callback,
            IntPtr callbackHandle);
    }

    public abstract class _EngineLayerWrapper : EngineLayer {
        protected _EngineLayerWrapper(IntPtr ptr) : base(ptr) {
        }

        internal List<_EngineLayerWrapper> _debugChildren;

        internal bool _debugWasUsedAsOldLayer = false;

        internal bool _debugCheckNotUsedAsOldLayer() {
            D.assert(
                !_debugWasUsedAsOldLayer,
                () => "Layer $runtimeType was previously used as oldLayer.\n" +
                      "Once a layer is used as oldLayer, it may not be used again. Instead, " +
                      "after calling one of the SceneBuilder.push* methods and passing an oldLayer " +
                      "to it, use the layer returned by the method as oldLayer in subsequent " +
                      "frames.");
            return true;
        }
    }

    public class TransformEngineLayer : _EngineLayerWrapper {
        internal TransformEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class OffsetEngineLayer : _EngineLayerWrapper {
        internal OffsetEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class ClipRectEngineLayer : _EngineLayerWrapper {
        internal ClipRectEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class ClipRRectEngineLayer : _EngineLayerWrapper {
        internal ClipRRectEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class ClipPathEngineLayer : _EngineLayerWrapper {
        internal ClipPathEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class OpacityEngineLayer : _EngineLayerWrapper {
        internal OpacityEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class ColorFilterEngineLayer : _EngineLayerWrapper {
        internal ColorFilterEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class ImageFilterEngineLayer : _EngineLayerWrapper {
        internal ImageFilterEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class BackdropFilterEngineLayer : _EngineLayerWrapper {
        internal BackdropFilterEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class ShaderMaskEngineLayer : _EngineLayerWrapper {
        internal ShaderMaskEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class PhysicalShapeEngineLayer : _EngineLayerWrapper {
        internal PhysicalShapeEngineLayer(IntPtr ptr) : base(ptr) {
        }
    }

    public class SceneBuilder : NativeWrapper {
        public SceneBuilder() : base(SceneBuilder_constructor()) {
        }

        protected override void DisposePtr(IntPtr ptr) {
            SceneBuilder_dispose(ptr);
        }

        readonly Dictionary<EngineLayer, string> _usedLayers = new Dictionary<EngineLayer, string>();

        bool _debugCheckUsedOnce(EngineLayer layer, string usage) {
            D.assert(() => {
                if (layer == null) {
                    return true;
                }

                D.assert(
                    !_usedLayers.ContainsKey(layer),
                    () => $"Layer {layer.GetType()} already used.\n" +
                          $"The layer is already being used as {_usedLayers[layer]} in this scene.\n" +
                          "A layer may only be used once in a given scene.");

                _usedLayers[layer] = usage;
                return true;
            });

            return true;
        }

        bool _debugCheckCanBeUsedAsOldLayer(_EngineLayerWrapper layer, String methodName) {
            D.assert(() => {
                if (layer == null) {
                    return true;
                }

                layer._debugCheckNotUsedAsOldLayer();
                D.assert(_debugCheckUsedOnce(layer, $"oldLayer in {methodName}"));
                layer._debugWasUsedAsOldLayer = true;
                return true;
            });
            return true;
        }

        readonly List<_EngineLayerWrapper> _layerStack = new List<_EngineLayerWrapper>();

        bool _debugPushLayer(_EngineLayerWrapper newLayer) {
            D.assert(() => {
                if (_layerStack.isNotEmpty()) {
                    _EngineLayerWrapper currentLayer = _layerStack.last();
                    currentLayer._debugChildren = currentLayer._debugChildren ?? new List<_EngineLayerWrapper>();
                    currentLayer._debugChildren.Add(newLayer);
                }

                _layerStack.Add(newLayer);
                return true;
            });
            return true;
        }

        public unsafe TransformEngineLayer pushTransform(
            float[] matrix4,
            TransformEngineLayer oldLayer = null
        ) {
            D.assert(PaintingUtils._matrix4IsValid(matrix4));
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushTransform"));
            fixed (float* matrix4Ptr = matrix4) {
                TransformEngineLayer layer = new TransformEngineLayer(SceneBuilder_pushTransform(_ptr, matrix4Ptr));
                D.assert(_debugPushLayer(layer));
                return layer;
            }
        }

        public OffsetEngineLayer pushOffset(
            float dx,
            float dy,
            OffsetEngineLayer oldLayer = null
        ) {
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushOffset"));
            OffsetEngineLayer layer = new OffsetEngineLayer(SceneBuilder_pushOffset(_ptr, dx, dy));
            D.assert(_debugPushLayer(layer));
            return layer;
        }

        public void pop() {
            if (_layerStack.isNotEmpty()) {
                _layerStack.removeLast();
            }

            SceneBuilder_pop(_ptr);
        }

        public Scene build() {
            return new Scene(SceneBuilder_build(_ptr));
        }
        
        public void addRetained(EngineLayer retainedLayer) {
            D.assert(retainedLayer is _EngineLayerWrapper);
            D.assert(() => {
                _EngineLayerWrapper layer = retainedLayer as _EngineLayerWrapper;

                void recursivelyCheckChildrenUsedOnce(_EngineLayerWrapper parentLayer) {
                    _debugCheckUsedOnce(parentLayer, "retained layer");
                    parentLayer._debugCheckNotUsedAsOldLayer();

                    if (parentLayer._debugChildren == null || parentLayer._debugChildren.isEmpty()) {
                        return;
                    }

                    parentLayer._debugChildren.ForEach(recursivelyCheckChildrenUsedOnce);
                }

                recursivelyCheckChildrenUsedOnce(layer);

                return true;
            });

            _EngineLayerWrapper wrapper = retainedLayer as _EngineLayerWrapper;
            SceneBuilder_addRetained(_ptr, wrapper._ptr);
        }
        
        public void addPicture(
            Offset offset,
            Picture picture, 
            bool isComplexHint = false,
            bool willChangeHint = false
        ) {
            int hints = (isComplexHint ? 1 : 0) | (willChangeHint ? 2 : 0);
            SceneBuilder_addPicture(_ptr, offset.dx, offset.dy, picture._ptr, hints);
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_constructor();

        [DllImport(NativeBindings.dllName)]
        static extern void SceneBuilder_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe IntPtr SceneBuilder_pushTransform(IntPtr ptr, float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_pushOffset(IntPtr ptr, float dx, float dy);

        [DllImport(NativeBindings.dllName)]
        static extern void SceneBuilder_pop(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_build(IntPtr ptr);
        
        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_addPicture(IntPtr ptr, float dx, float dy, IntPtr picture, int hints);

        [DllImport(NativeBindings.dllName)]
        static extern void SceneBuilder_addRetained(IntPtr ptr, IntPtr retainedLayer);
    }
}