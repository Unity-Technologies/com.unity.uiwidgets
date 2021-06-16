using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Scene : NativeWrapperDisposable {
        internal Scene(IntPtr ptr) : base(ptr) {
        }

        public override void DisposePtr(IntPtr ptr) {
            Scene_dispose(ptr);
        }

        public Future<Image> toImage(int width, int height) {
            if (width <= 0 || height <= 0) {
                throw new Exception("Invalid image dimensions.");
            }

            return ui_._futurize(
                (_Callback<Image> callback) => {
                    GCHandle callbackHandle = GCHandle.Alloc(callback);
                    IntPtr error =
                        Scene_toImage(_ptr, width, height, _toImageCallback,
                            (IntPtr) callbackHandle);

                    if (error != IntPtr.Zero) {
                        callbackHandle.Free();
                        return Marshal.PtrToStringAnsi(error);
                    }

                    return null;
                });
        }

        [MonoPInvokeCallback(typeof(Scene_toImageCallback))]
        static void _toImageCallback(IntPtr callbackHandle, IntPtr result) {
            GCHandle handle = (GCHandle) callbackHandle;
            var callback = (_Callback<Image>) handle.Target;
            handle.Free();

            if (!Isolate.checkExists()) {
                return;
            }

            try {
                callback(result == IntPtr.Zero ? null : new Image(result));
            }
            catch (Exception ex) {
                Debug.LogException(ex);
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

    public class SceneBuilder : NativeWrapperCPtrDisposable {
        public SceneBuilder() : base(SceneBuilder_constructor()) {
        }

        public override void DisposeCPtrImpl(IntPtr ptr) {
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

        bool _debugCheckCanBeUsedAsOldLayer(_EngineLayerWrapper layer, string methodName) {
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

        public ClipRectEngineLayer pushClipRect(
            Rect rect,
            Clip clipBehavior = Clip.antiAlias,
            ClipRectEngineLayer oldLayer = null) {
            D.assert(clipBehavior != Clip.none);
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushClipRect"));
            ClipRectEngineLayer layer = new ClipRectEngineLayer(SceneBuilder_pushClipRect(_ptr, rect.left, rect.right, rect.top, rect.bottom, (int)clipBehavior));
            D.assert(_debugPushLayer(layer));
            return layer;
        }

        public unsafe ClipRRectEngineLayer pushClipRRect(
            RRect rrect,
            Clip clipBehavior = Clip.antiAlias,
            ClipRRectEngineLayer oldLayer = null) {
            D.assert(clipBehavior != Clip.none);
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushClipRRect"));
            fixed (float* rrectPtr = rrect._value32) {
                ClipRRectEngineLayer layer =
                    new ClipRRectEngineLayer(SceneBuilder_pushClipRRect(_ptr, rrectPtr, (int) clipBehavior));
                D.assert(_debugPushLayer(layer));
                return layer;
            }
        }

        public ClipPathEngineLayer pushClipPath(
            Path path,
            Clip clipBehavior = Clip.antiAlias,
            ClipPathEngineLayer oldLayer = null) {
            D.assert(clipBehavior != Clip.none);
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushClipPath"));
            ClipPathEngineLayer layer = new ClipPathEngineLayer(SceneBuilder_pushClipPath(_ptr, path._ptr, (int)clipBehavior));
            D.assert(_debugPushLayer(layer));
            return layer;
        }

        public OpacityEngineLayer pushOpacity(
            int alpha,
            Offset offset = null,
            OpacityEngineLayer oldLayer = null) {
            offset = offset ?? Offset.zero;
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushOpacity"));
            OpacityEngineLayer layer = new OpacityEngineLayer(SceneBuilder_pushOpacity(_ptr, alpha, offset.dx, offset.dy));
            D.assert(_debugPushLayer(layer));
            return layer;
        }

        public ColorFilterEngineLayer pushColorFilter(ColorFilter colorFilter, ColorFilterEngineLayer oldLayer = null) {
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushColorFilter"));
            ColorFilterEngineLayer layer = new ColorFilterEngineLayer(SceneBuilder_pushColorFilter(_ptr, colorFilter._toNativeColorFilter()._ptr));
            return layer;
        }

        public ImageFilterEngineLayer pushImageFilter(ImageFilter imageFilter, ImageFilterEngineLayer oldLayer = null) {
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushImageFilter"));
            ImageFilterEngineLayer layer = new ImageFilterEngineLayer(SceneBuilder_pushImageFilter(_ptr, imageFilter._toNativeImageFilter()._ptr));
            return layer;
        }
        
        public BackdropFilterEngineLayer pushBackdropFilter(
            ImageFilter filter,
            BackdropFilterEngineLayer oldLayer = null) {
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushBackdropFilter"));
            BackdropFilterEngineLayer layer = new BackdropFilterEngineLayer(SceneBuilder_pushBackdropFilter(_ptr, filter._toNativeImageFilter()._ptr));
            D.assert(_debugPushLayer(layer));
            return layer;
        }
        public ShaderMaskEngineLayer pushShaderMask(
            Shader shader,
            Rect maskRect,
            BlendMode blendMode, 
            ShaderMaskEngineLayer oldLayer = null
        ) {
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "pushShaderMask")); 
            ShaderMaskEngineLayer layer = new ShaderMaskEngineLayer(SceneBuilder_pushShaderMask(
                _ptr,
                shader._ptr,
                maskRect.left,
                maskRect.right,
                maskRect.top,
                maskRect.bottom,
                (int)blendMode
            ));
            D.assert(_debugPushLayer(layer));
            return layer;
        }

        public PhysicalShapeEngineLayer pushPhysicalShape(
            Path path,
            float elevation,
            Color color,
            Color shadowColor,
            Clip clipBehavior = Clip.none,
            PhysicalShapeEngineLayer oldLayer = null) {
            D.assert(_debugCheckCanBeUsedAsOldLayer(oldLayer, "PhysicalShapeEngineLayer"));
            PhysicalShapeEngineLayer layer = new PhysicalShapeEngineLayer(
                SceneBuilder_pushPhysicalShape(_ptr, 
                    path._ptr, 
                    elevation, 
                    (int)color.value, 
                    (int)(shadowColor?.value ?? 0xFF000000), 
                    (int)clipBehavior));
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

        public void addPerformanceOverlay(int enabledOptions, Rect bounds) {
            SceneBuilder_addPerformanceOverlay(enabledOptions, bounds.left, bounds.right, bounds.top, bounds.bottom);
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

        public void addTexture(
            int textureId,
            Offset offset = null,
            float width = 0.0f,
            float height = 0.0f,
            bool freeze = false
        ) {
            offset = offset ?? Offset.zero;
            SceneBuilder_addTexture(_ptr, offset.dx, offset.dy, width, height, textureId, freeze);
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
        static extern IntPtr SceneBuilder_pushClipRect(IntPtr ptr, float left, float right, float top, float bottom,
            int clipBehavior);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe IntPtr SceneBuilder_pushClipRRect(IntPtr ptr, float* rrect, int clipBehavior);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_pushClipPath(IntPtr ptr, IntPtr path, int clipBehavior);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_pushOpacity(IntPtr ptr, int alpha, float dx, float dy);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_pushColorFilter(IntPtr ptr, IntPtr filter);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_pushImageFilter(IntPtr ptr, IntPtr filter);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_pushBackdropFilter(IntPtr ptr, IntPtr filter);

        [DllImport(NativeBindings.dllName)]
        static extern void SceneBuilder_addPerformanceOverlay(int enabledOptions, float left, float right, float top,
            float bottom);
        
        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_pushShaderMask(
            IntPtr ptr,
            IntPtr shader,
            float maskRectLeft, 
            float maskRectRight,
            float maskRectTop, 
            float maskRectBottom,
            int blendMod);
        
        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_pushPhysicalShape(IntPtr ptr, IntPtr path, float evelation, int color,
            int shadowColor, int clipBehavior);

        [DllImport(NativeBindings.dllName)]
        static extern void SceneBuilder_pop(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_build(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr SceneBuilder_addPicture(IntPtr ptr, float dx, float dy, IntPtr picture, int hints);

        [DllImport(NativeBindings.dllName)]
        static extern void SceneBuilder_addTexture(IntPtr ptr, float dx, float dy, float width, float height,
            int textureId, bool freeze);

        [DllImport(NativeBindings.dllName)]
        static extern void SceneBuilder_addRetained(IntPtr ptr, IntPtr retainedLayer);
    }
}