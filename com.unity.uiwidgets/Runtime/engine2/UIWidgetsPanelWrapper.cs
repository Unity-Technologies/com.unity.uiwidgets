using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.editor2 {

    public delegate void UIWidgetsMainFunction();
    
    public class UIWidgetsPanelWrapper {
        
        RenderTexture _renderTexture;
        
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
        }

        void _destroyRenderTexture() {
            D.assert(_renderTexture != null);
            ObjectUtils.SafeDestroy(_renderTexture);
            _renderTexture = null;
        }

        void _enableUIWidgetsPanel(string font_settings) {
            UIWidgetsPanel_onEnable(_ptr, _renderTexture.GetNativeTexturePtr(),
                _width, _height, _devicePixelRatio, Application.streamingAssetsPath, font_settings);
        }

        void _resizeUIWidgetsPanel() {
            UIWidgetsPanel_onRenderTexture(_ptr,
                _renderTexture.GetNativeTexturePtr(),
                _width, _height, _devicePixelRatio);
        }

        void _disableUIWidgetsPanel() {
            _renderTexture = null;
        }
        
        public static UIWidgetsPanelWrapper current {
            get { return Window.instance._panel; }
        }

        public IUIWidgetsWindow window => _host;

        public Isolate isolate { get; private set; }

        IntPtr _ptr;
        GCHandle _handle;

        int _width;
        int _height;
        float _devicePixelRatio;

        public RenderTexture renderTexture => _renderTexture;
        
        public float devicePixelRatio => _devicePixelRatio;

        IUIWidgetsWindow _host;

        public void Initiate(IUIWidgetsWindow host, int width, int height, float dpr, Dictionary<string, object> settings) {
            D.assert(_renderTexture == null);
            _recreateRenderTexture(width, height, dpr);

            _handle = GCHandle.Alloc(this);
            _ptr = UIWidgetsPanel_constructor((IntPtr) _handle, UIWidgetsPanel_entrypoint);

            _host = host;

            _enableUIWidgetsPanel(JSONMessageCodec.instance.toJson(settings));
            
            Input_OnEnable();
            NativeConsole.OnEnable();
        }
        
        public void _entryPoint() {
            try {
                isolate = Isolate.current;
                Window.instance._panel = this;

                _host.mainEntry();
            }
            catch (Exception ex) {
                Debug.LogException(new Exception("exception in main", ex));
            }
        }
        
        public void OnRectTransformDimensionsChange(int width, int height, float dpr) {
            if (_ptr != IntPtr.Zero && _renderTexture) {
                if (_recreateRenderTexture(width, height, dpr)) {
                    _resizeUIWidgetsPanel();
                }
            }
        }
        
        public void Destroy() {
            Input_OnDisable();

            UIWidgetsPanel_onDisable(_ptr);
            UIWidgetsPanel_dispose(_ptr);
            _ptr = IntPtr.Zero;

            _handle.Free();
            _handle = default;

            _disableUIWidgetsPanel();

            D.assert(!isolate.isValid);
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
        
        public int registerTexture(Texture texture) {
            return UIWidgetsPanel_registerTexture(_ptr, texture.GetNativeTexturePtr());
        }

        public void unregisterTexture(int textureId) {
            UIWidgetsPanel_unregisterTexture(_ptr, textureId);
        }

        public void markNewFrameAvailable(int textureId) {
            UIWidgetsPanel_markNewFrameAvailable(_ptr, textureId);
        }
        
        void Input_OnEnable() {
        }

        void Input_OnDisable() {
        }
        
        public void OnMouseMove(Vector2? pos) {
            if (pos == null) {
                return;
            }
            UIWidgetsPanel_onMouseMove(_ptr, pos.Value.x, pos.Value.y);
        }

        public void OnPointerDown(Vector2? pos, int pointerId) {
            if (pos == null) {
                return;
            }

            // mouse event
            if (pointerId < 0) {
                UIWidgetsPanel_onMouseDown(_ptr, pos.Value.x, pos.Value.y, pointerId);
            }
        }

        public void OnPointerUp(Vector2? pos, int pointerId) {
            if (pos == null) {
                return;
            }

            // mouse event
            if (pointerId < 0) {
                UIWidgetsPanel_onMouseUp(_ptr, pos.Value.x, pos.Value.y, pointerId);
            }
        }

        public void OnDrag(Vector2? pos, int pointerId) {
            if (pos == null) {
                return;
            }

            // mouse event
            if (pointerId < 0) {
                UIWidgetsPanel_onMouseMove(_ptr, pos.Value.x, pos.Value.y);
            }
        }

        public void OnPointerLeave() {
            UIWidgetsPanel_onMouseLeave(_ptr);
        }

        public void OnKeyDown(Event e) {
            UIWidgetsPanel_onKey(_ptr, e.keyCode, e.type == EventType.KeyDown);
            if (e.character != 0 || e.keyCode == KeyCode.Backspace) {
                PointerEventConverter.KeyEvent.Enqueue(new Event(e));
                // TODO: add on char
                // UIWidgetsPanel_onChar(_ptr, e.character);
            }
        }
        
        
        delegate void UIWidgetsPanel_EntrypointCallback(IntPtr handle);

        [MonoPInvokeCallback(typeof(UIWidgetsPanel_EntrypointCallback))]
        static void UIWidgetsPanel_entrypoint(IntPtr handle) {
            GCHandle gcHandle = (GCHandle) handle;
            UIWidgetsPanelWrapper panel = (UIWidgetsPanelWrapper) gcHandle.Target;
            panel._entryPoint();
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr UIWidgetsPanel_constructor(IntPtr handle,
            UIWidgetsPanel_EntrypointCallback entrypointCallback);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onDisable(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern int UIWidgetsPanel_registerTexture(IntPtr ptr, IntPtr nativeTexturePtr);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_unregisterTexture(IntPtr ptr, int textureId);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_markNewFrameAvailable(IntPtr ptr, int textureId);
        
        
        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onEnable(IntPtr ptr,
            IntPtr nativeTexturePtr, int width, int height, float dpi, string streamingAssetsPath,
            string font_settings);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onRenderTexture(
            IntPtr ptr, IntPtr nativeTexturePtr, int width, int height, float dpi);
        
        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onChar(IntPtr ptr, char c);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onKey(IntPtr ptr, KeyCode keyCode, bool isKeyDown);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onMouseDown(IntPtr ptr, float x, float y, int button);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onMouseUp(IntPtr ptr, float x, float y, int button);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onMouseMove(IntPtr ptr, float x, float y);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onMouseLeave(IntPtr ptr);
    }
    
}