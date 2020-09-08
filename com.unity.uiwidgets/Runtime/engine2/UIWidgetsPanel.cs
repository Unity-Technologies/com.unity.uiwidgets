using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui2;
using UnityEngine;
using UnityEngine.UI;
using NativeBindings = Unity.UIWidgets.ui2.NativeBindings;

namespace Unity.UIWidgets.engine2 {
    public class UIWidgetsPanel : RawImage {
        public IntPtr isolate { get; private set; }

        IntPtr _ptr;
        GCHandle _handle;

        RenderTexture _renderTexture;
        int _width;
        int _height;
        float _devicePixelRatio;

        int _currentWidth {
            get { return Mathf.RoundToInt(rectTransform.rect.width * canvas.scaleFactor); }
        }

        int _currentHeight {
            get { return Mathf.RoundToInt(rectTransform.rect.height * canvas.scaleFactor); }
        }

        float _currentDevicePixelRatio {
            get {
                float currentDpi = Screen.dpi;
                if (currentDpi == 0) {
                    currentDpi = canvas.GetComponent<CanvasScaler>().fallbackScreenDPI;
                }

                return currentDpi / 96;
            }
        }

        protected override void OnEnable() {
            base.OnEnable();

            _recreateRenderTexture(_currentWidth, _currentHeight, _currentDevicePixelRatio);

            _handle = GCHandle.Alloc(this);

            _ptr = UIWidgetsPanel_constructor((IntPtr) _handle, UIWidgetsPanel_entrypoint);
            UIWidgetsPanel_onEnable(_ptr, _renderTexture.GetNativeTexturePtr(),
                _width, _height, _devicePixelRatio);
        }

        protected virtual void main() {
            Debug.Log(Debug.isDebugBuild);
        }

        public void entryPoint() {
            try {
                isolate = Isolate.current;
                main();
            }
            catch (Exception ex) {
                Debug.LogException(new Exception("exception in main", ex));
            }
        }

        protected override void OnRectTransformDimensionsChange() {
            if (_ptr != IntPtr.Zero) {
                if (_recreateRenderTexture(_currentWidth, _currentHeight, _currentDevicePixelRatio)) {
                    UIWidgetsPanel_onRenderTexture(_ptr,
                        _renderTexture.GetNativeTexturePtr(),
                        _width, _height, _devicePixelRatio);
                }
            }
        }

        protected override void OnDisable() {
            UIWidgetsPanel_onDisable(_ptr);
            UIWidgetsPanel_dispose(_ptr);
            _ptr = IntPtr.Zero;

            _handle.Free();
            _handle = default;

            base.OnDisable();
        }

        bool _recreateRenderTexture(int width, int height, float devicePixelRatio) {
            if (_renderTexture != null && _width == width && _height == height &&
                _devicePixelRatio == devicePixelRatio) {
                return false;
            }

            if (_renderTexture) {
                _destroyRenderTexture();
            }

            _createRenderTexture(width, height, devicePixelRatio);
            return true;
        }

        void _createRenderTexture(int width, int height, float devicePixelRatio) {
            D.assert(_renderTexture == null);

            var desc = new RenderTextureDescriptor(
                width, height, RenderTextureFormat.ARGB32, 0) {
                useMipMap = false,
                autoGenerateMips = false,
            };

            _renderTexture = new RenderTexture(desc) {hideFlags = HideFlags.HideAndDontSave};
            _renderTexture.Create();

            _width = width;
            _height = height;
            _devicePixelRatio = devicePixelRatio;

            texture = _renderTexture;
        }

        void _destroyRenderTexture() {
            D.assert(_renderTexture != null);
            texture = null;
            ObjectUtils.SafeDestroy(_renderTexture);
            _renderTexture = null;
        }

        delegate void UIWidgetsPanel_EntrypointCallback(IntPtr handle);

        [MonoPInvokeCallback(typeof(UIWidgetsPanel_EntrypointCallback))]
        static void UIWidgetsPanel_entrypoint(IntPtr handle) {
            GCHandle gcHandle = (GCHandle) handle;
            UIWidgetsPanel panel = (UIWidgetsPanel) gcHandle.Target;
            panel.entryPoint();
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr UIWidgetsPanel_constructor(IntPtr handle,
            UIWidgetsPanel_EntrypointCallback entrypointCallback);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onEnable(IntPtr ptr,
            IntPtr nativeTexturePtr, int width, int height, float dpi);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onDisable(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onRenderTexture(
            IntPtr ptr, IntPtr nativeTexturePtr, int width, int height, float dpi);
    }
}