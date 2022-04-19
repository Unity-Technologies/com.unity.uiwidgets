using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Path = Unity.UIWidgets.ui.Path;

namespace Unity.UIWidgets.engine {

    #region Platform: Windows Specific Functionalities
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
public partial class UIWidgetsPanelWrapper {
    Texture _renderTexture;

    public Texture renderTexture {
        get { return _renderTexture; }
    }

    public bool requireColorspaceShader => !useColorspaceGamma;

    void _createRenderTexture(int width, int height, float devicePixelRatio) {
        D.assert(_renderTexture == null);

        _width = width;
        _height = height;
        this.devicePixelRatio = devicePixelRatio;
    }

    void _destroyRenderTexture() {
        D.assert(_renderTexture != null);
        var releaseOK = UIWidgetsPanel_releaseNativeTexture(_ptr);
        D.assert(releaseOK);

        _renderTexture = null;
    }

    void _enableUIWidgetsPanel(string font_settings) {
        D.assert(_renderTexture == null);
        IntPtr native_tex_ptr = UIWidgetsPanel_onEnable(_ptr, _width, _height, devicePixelRatio,
            Application.streamingAssetsPath, font_settings);
        D.assert(native_tex_ptr != IntPtr.Zero);

        _renderTexture =
            Texture2D.CreateExternalTexture(_width, _height, TextureFormat.BGRA32, false, true, native_tex_ptr);
    }

    void _disableUIWidgetsPanel() {
        _renderTexture = null;
    }

    void _resizeUIWidgetsPanel() {
        D.assert(_renderTexture == null);
        IntPtr native_tex_ptr = UIWidgetsPanel_onRenderTexture(_ptr, _width, _height, devicePixelRatio);
        D.assert(native_tex_ptr != IntPtr.Zero);

        _renderTexture =
            Texture2D.CreateExternalTexture(_width, _height, TextureFormat.BGRA32, false, true, native_tex_ptr);
    }

    [DllImport(NativeBindings.dllName)]
    static extern IntPtr UIWidgetsPanel_onEnable(IntPtr ptr,
        int width, int height, float dpi, string streamingAssetsPath, string font_settings);

    [DllImport(NativeBindings.dllName)]
    [return: MarshalAs(UnmanagedType.U1)]
    static extern bool UIWidgetsPanel_releaseNativeTexture(IntPtr ptr);

    [DllImport(NativeBindings.dllName)]
    static extern IntPtr UIWidgetsPanel_onRenderTexture(
        IntPtr ptr, int width, int height, float dpi);
}
#endif
    #endregion
    
    #region Platform: MacOs/iOS Specific Functionalities
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
    public partial class UIWidgetsPanelWrapper {
        Texture _renderTexture;

        public Texture renderTexture {
            get { return _renderTexture; }
        }
        
        //since both Metal and OpenGLCore are supported on Mac/iOS, we use this flag to separate the code paths for
        //each backend respectively
        bool isMetalBackend => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal;

        public bool requireColorspaceShader => isMetalBackend && !useColorspaceGamma;

        void _createRenderTexture(int width, int height, float devicePixelRatio) {
            D.assert(_renderTexture == null);

            if (!isMetalBackend) {
                var colorSpace = useColorspaceGamma ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB;
                var graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.ARGB32, colorSpace);

                var desc = new RenderTextureDescriptor(
                    width, height, graphicsFormat, 0) {
                    useMipMap = false,
                    autoGenerateMips = false,
                };

                var _localRenderTexture = new RenderTexture(desc) {hideFlags = HideFlags.HideAndDontSave};
                _localRenderTexture.Create();
                _renderTexture = _localRenderTexture;
            }

            _width = width;
            _height = height;
            this.devicePixelRatio = devicePixelRatio;
        }

        void _destroyRenderTexture() {
            D.assert(_renderTexture != null);

            if (isMetalBackend) {
                var releaseOK = UIWidgetsPanel_releaseNativeTexture(_ptr);
                D.assert(releaseOK);
            }
            else {
                ObjectUtils.SafeDestroy(_renderTexture);
            }
            _renderTexture = null;
        }

        void _enableUIWidgetsPanel(string font_settings) {
            if (isMetalBackend) {
                D.assert(_renderTexture == null);
                var native_tex_ptr = UIWidgetsPanel_onEnable(_ptr, IntPtr.Zero, _width, _height, devicePixelRatio,
                    Application.streamingAssetsPath, font_settings);
                D.assert(native_tex_ptr != IntPtr.Zero);
                _renderTexture =
                    Texture2D.CreateExternalTexture(_width, _height, TextureFormat.BGRA32, false, true, native_tex_ptr);
            }
            else {
                D.assert(_renderTexture != null && _renderTexture.GetNativeTexturePtr() != IntPtr.Zero);
                UIWidgetsPanel_onEnable(_ptr, _renderTexture.GetNativeTexturePtr(),
                    _width, _height, devicePixelRatio, Application.streamingAssetsPath, font_settings);
            }
        }

        void _resizeUIWidgetsPanel() {
            if (isMetalBackend) {
                D.assert(_renderTexture == null);
                var native_tex_ptr =
                    UIWidgetsPanel_onRenderTexture(_ptr, IntPtr.Zero, _width, _height, devicePixelRatio);
                D.assert(native_tex_ptr != IntPtr.Zero);
                _renderTexture =
                    Texture2D.CreateExternalTexture(_width, _height, TextureFormat.BGRA32, false, true, native_tex_ptr);
            }
            else {
                D.assert(_renderTexture != null && _renderTexture.GetNativeTexturePtr() != IntPtr.Zero);
                UIWidgetsPanel_onRenderTexture(_ptr, _renderTexture.GetNativeTexturePtr(), _width, _height, devicePixelRatio);
            }
        }

        void _disableUIWidgetsPanel() {
            _renderTexture = null;
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr UIWidgetsPanel_onEnable(IntPtr ptr,
            IntPtr nativeTexturePtr, int width, int height, float dpi, string streamingAssetsPath,
            string font_settings);
        
        [DllImport(NativeBindings.dllName)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool UIWidgetsPanel_releaseNativeTexture(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr UIWidgetsPanel_onRenderTexture(
            IntPtr ptr, IntPtr nativeTexturePtr, int width, int height, float dpi);
    }
#endif
#endregion

    #region Platform: Android Runtime Specific Functionalities

#if (!UNITY_EDITOR && UNITY_ANDROID )
    public partial class UIWidgetsPanelWrapper {
        RenderTexture _renderTexture;
    
    
        public RenderTexture renderTexture {
            get { return _renderTexture; }
        }

        public bool requireColorspaceShader => false;

        void _createRenderTexture(int width, int height, float devicePixelRatio) {
            D.assert(_renderTexture == null);

            var colorSpace = useColorspaceGamma ? RenderTextureReadWrite.Linear
                    : RenderTextureReadWrite.sRGB;
            var graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.ARGB32, colorSpace);

            var desc = new RenderTextureDescriptor(
                width, height, graphicsFormat, 0) {
                useMipMap = false,
                autoGenerateMips = false,
            };

            _renderTexture = new RenderTexture(desc) {hideFlags = HideFlags.HideAndDontSave};
            _renderTexture.Create();

            _width = width;
            _height = height;
            this.devicePixelRatio = devicePixelRatio;
        }

        void _destroyRenderTexture() {
            D.assert(_renderTexture != null);
            ObjectUtils.SafeDestroy(_renderTexture);
            _renderTexture = null;
        }

        void _enableUIWidgetsPanel(string font_settings) {
            UIWidgetsPanel_onEnable(_ptr, _renderTexture.GetNativeTexturePtr(),
                _width, _height, devicePixelRatio, Application.temporaryCachePath, font_settings);
        }

        void _resizeUIWidgetsPanel() {
            UIWidgetsPanel_onRenderTexture(_ptr,
                _renderTexture.GetNativeTexturePtr(),
                _width, _height, devicePixelRatio);
        }

        void _disableUIWidgetsPanel() {
            _renderTexture = null;
        }

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onEnable(IntPtr ptr,
            IntPtr nativeTexturePtr, int width, int height, float dpi, string streamingAssetsPath,
            string font_settings);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onRenderTexture(
            IntPtr ptr, IntPtr nativeTexturePtr, int width, int height, float dpi);
    }
#endif

#endregion

    #region Window Common Properties and Functions

    public partial class UIWidgetsPanelWrapper {
        public static UIWidgetsPanelWrapper current {
            get { return Window.instance._panel; }
        }

        IntPtr _ptr;
        GCHandle _handle;

        int _width;
        int _height;

        public IUIWidgetsWindow window { get; private set; }

        public Isolate isolate { get; private set; }

        public float devicePixelRatio { get; private set; }
        
        public readonly DisplayMetrics displayMetrics = new DisplayMetrics();

        bool useColorspaceGamma = QualitySettings.activeColorSpace == ColorSpace.Gamma;

        public void Initiate(IUIWidgetsWindow host, int width, int height, float dpr, Configurations _configurations) {
            D.assert(renderTexture == null);
            _recreateRenderTexture(width: width, height: height, devicePixelRatio: dpr);

            _handle = GCHandle.Alloc(this);
            _ptr = UIWidgetsPanel_constructor((IntPtr) _handle, (int) host.getWindowType(),
                entrypointCallback: UIWidgetsPanel_entrypoint);
            window = host;

            var fontsetting = new Dictionary<string, object>();
            fontsetting.Add("fonts", _configurations.fontsToObject());
            _enableUIWidgetsPanel(JSONMessageCodec.instance.toJson(message: fontsetting));
            NativeConsole.OnEnable();

        }

        public void _entryPoint() {
            try {
                isolate = Isolate.current;
                Window.instance._panel = this;
                window.mainEntry();
            }
            catch (Exception ex) {
                Debug.LogException(new Exception("exception in main", innerException: ex));
            }
        }

        public bool didDisplayMetricsChanged(int width, int height, float dpr) {
            return width != _width || height != _height || dpr != devicePixelRatio;
        }

        public void OnDisplayMetricsChanged(int width, int height, float dpr) {
            if (_ptr != IntPtr.Zero && renderTexture) {
                if (_recreateRenderTexture(width: width, height: height, devicePixelRatio: dpr)) {
                    _resizeUIWidgetsPanel();
                }
            }
        }
        public void Destroy() {
            if (_ptr != IntPtr.Zero) {
                ReleaseExternalTextures();
                
                UIWidgetsPanel_onDisable(ptr: _ptr);
                UIWidgetsPanel_dispose(ptr: _ptr);
                _ptr = IntPtr.Zero;
            }

            _handle.Free();
            _handle = default;
            
            _disableUIWidgetsPanel();
            D.assert(result: !isolate.isValid);
        }

        bool _recreateRenderTexture(int width, int height, float devicePixelRatio) {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            
            if (renderTexture != null && _width == width && _height == height &&
                this.devicePixelRatio == devicePixelRatio) {
                return false;
            }

            if (renderTexture) {
                _destroyRenderTexture();
            }

            _createRenderTexture(width: width, height: height, devicePixelRatio: devicePixelRatio);
            return true;
        }

        public int registerTexture(IntPtr externalTexturePtr) {
            return UIWidgetsPanel_registerTexture(ptr: _ptr, externalTexturePtr);
        }

        public void unregisterTexture(int textureId) {
            UIWidgetsPanel_unregisterTexture(ptr: _ptr, textureId: textureId);
        }

        public void markNewFrameAvailable(int textureId) {
            UIWidgetsPanel_markNewFrameAvailable(ptr: _ptr, textureId: textureId);
        }

        public void onEditorUpdate() {
            UIWidgetsPanel_onEditorUpdate(ptr: _ptr);
        }

        delegate void UIWidgetsPanel_EntrypointCallback(IntPtr handle);

        [MonoPInvokeCallback(typeof(UIWidgetsPanel_EntrypointCallback))]
        static void UIWidgetsPanel_entrypoint(IntPtr handle) {
            var gcHandle = (GCHandle) handle;
            var panel = (UIWidgetsPanelWrapper) gcHandle.Target;
            panel._entryPoint();
        }

        [DllImport(dllName: NativeBindings.dllName)]
        static extern IntPtr UIWidgetsPanel_constructor(IntPtr handle, int windowType,
            UIWidgetsPanel_EntrypointCallback entrypointCallback);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_dispose(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onDisable(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern int UIWidgetsPanel_registerTexture(IntPtr ptr, IntPtr nativeTexturePtr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_unregisterTexture(IntPtr ptr, int textureId);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_markNewFrameAvailable(IntPtr ptr, int textureId);

#if UNITY_EDITOR
        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onEditorUpdate(IntPtr ptr);
#else
    static void UIWidgetsPanel_onEditorUpdate(IntPtr ptr) { throw new NotImplementedException(); }
#endif
    }

    #endregion


    #region Input Events Handles

    public partial class UIWidgetsPanelWrapper {
        public void OnMouseMove(Vector2? pos) {
            if (pos == null) {
                return;
            }
            
            UIWidgetsPanel_onMouseMove(ptr: _ptr, x: pos.Value.x, y: pos.Value.y, -1);
        }

        public void OnMouseScroll(Vector2 delta, Vector2? pos) {
            if (pos == null) {
                return;
            }

            UIWidgetsPanel_onScroll(ptr: _ptr, x: delta.x, y: delta.y, px: pos.Value.x, py: pos.Value.y);
        }

        public void OnPointerDown(Vector2? pos, int pointerId) {
            if (pos == null) {
                return;
            }

            // mouse event
            if (pointerId < 0) {
                UIWidgetsPanel_onMouseDown(ptr: _ptr, x: pos.Value.x, y: pos.Value.y, button: pointerId);
            }
        }

        public void OnPointerUp(Vector2? pos, int pointerId) {
            if (pos == null) {
                return;
            }

            // mouse event
            if (pointerId < 0) {
                UIWidgetsPanel_onMouseUp(ptr: _ptr, x: pos.Value.x, y: pos.Value.y, button: pointerId);
            }
        }

        public void OnDrag(Vector2? pos, int pointerId) {
            if (pos == null) {
                return;
            }

            // mouse event
            if (pointerId < 0) {
                UIWidgetsPanel_onMouseMove(ptr: _ptr, x: pos.Value.x, y: pos.Value.y, pointerId);
            }
        }

        public void OnPointerLeave() {
            UIWidgetsPanel_onMouseLeave(ptr: _ptr);
        }
        
        public void OnDragUpdateInEditor(Vector2? pos) {
            if (pos == null) {
                return;
            }
            
            UIWidgetsPanel_onDragUpdateInEditor(ptr: _ptr, x: pos.Value.x, y: pos.Value.y);
        }
        
        public void OnDragReleaseInEditor(Vector2? pos) {
            if (pos == null) {
                return;
            }
            
            UIWidgetsPanel_onDragReleaseInEditor(ptr: _ptr, x: pos.Value.x, y: pos.Value.y);
        }

        public void OnKeyDown(Event e) {
            int modifier = 0;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            modifier |= e.shift ? (1 << (int) FunctionKey.shift) : 0;
            modifier |= e.alt ? (1 << (int) FunctionKey.alt) : 0;
            modifier |= e.command ? (1 << (int) FunctionKey.command) : 0;
            modifier |= e.control ? (1 << (int) FunctionKey.control) : 0;
#endif
            UIWidgetsPanel_onKey(ptr: _ptr, keyCode: e.keyCode, e.type == EventType.KeyDown, modifier);
            if (e.character != 0 || e.keyCode == KeyCode.Backspace) {
                PointerEventConverter.KeyEvent.Enqueue(new Event(other: e));
                // TODO: add on char
                // UIWidgetsPanel_onChar(_ptr, e.character);
            }
        }

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onChar(IntPtr ptr, char c);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onKey(IntPtr ptr, KeyCode keyCode, bool isKeyDown, int modifier);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onMouseDown(IntPtr ptr, float x, float y, int button);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onMouseUp(IntPtr ptr, float x, float y, int button);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onMouseMove(IntPtr ptr, float x, float y, int button);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onMouseLeave(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onScroll(IntPtr ptr, float x, float y, float px, float py);
#if UNITY_EDITOR
        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onDragUpdateInEditor(IntPtr ptr, float x ,float y);
        
        [DllImport(dllName: NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onDragReleaseInEditor(IntPtr ptr, float x ,float y);
#else
        static void UIWidgetsPanel_onDragUpdateInEditor(IntPtr ptr, float x, float y) { throw new NotImplementedException(); }
        
        static void UIWidgetsPanel_onDragReleaseInEditor(IntPtr ptr, float x ,float y) { throw new NotImplementedException(); }
#endif
    }

    #endregion
}