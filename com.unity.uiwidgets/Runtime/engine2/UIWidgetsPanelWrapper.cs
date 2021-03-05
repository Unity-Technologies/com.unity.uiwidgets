using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;
using TextFont = Unity.UIWidgets.engine2.UIWidgetsPanel.TextFont;
using Font = Unity.UIWidgets.engine2.UIWidgetsPanel.Font;

namespace Unity.UIWidgets.editor2 {
    
#region Platform: Windows Specific Functionalities

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public partial class UIWidgetsPanelWrapper {
        RenderTexture _renderTexture;
    
    
        public RenderTexture renderTexture {
            get { return _renderTexture; }
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


#region  Platform: MacOs Specific Functionalities

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

public partial class UIWidgetsPanelWrapper {
    Texture _renderTexture;
    
    public Texture renderTexture {
        get { return _renderTexture; }
    }

    void _createRenderTexture(int width, int height, float devicePixelRatio) {
        D.assert(_renderTexture == null);

        _width = width;
        _height = height;
        _devicePixelRatio = devicePixelRatio;
    }

    void _destroyRenderTexture() {
        D.assert(_renderTexture != null);
        var releaseOK = UIWidgetsPanel_releaseNativeTexture(_ptr);
        D.assert(releaseOK);

        _renderTexture = null;
    }

    void _enableUIWidgetsPanel(string font_settings) {
        D.assert(_renderTexture == null);
        IntPtr native_tex_ptr = UIWidgetsPanel_onEnable(_ptr, _width, _height, _devicePixelRatio,
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

        IntPtr native_tex_ptr = UIWidgetsPanel_onRenderTexture(_ptr, _width, _height, _devicePixelRatio);
        D.assert(native_tex_ptr != IntPtr.Zero);

        _renderTexture =
            Texture2D.CreateExternalTexture(_width, _height, TextureFormat.BGRA32, false, true, native_tex_ptr);
    }

    [DllImport(NativeBindings.dllName)]
    static extern IntPtr UIWidgetsPanel_onEnable(IntPtr ptr,
        int width, int height, float dpi, string streamingAssetsPath, string font_settings);

    [DllImport(NativeBindings.dllName)]
    static extern bool UIWidgetsPanel_releaseNativeTexture(IntPtr ptr);

    [DllImport(NativeBindings.dllName)]
    static extern IntPtr UIWidgetsPanel_onRenderTexture(
        IntPtr ptr, int width, int height, float dpi);
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
    float _devicePixelRatio;

    IUIWidgetsWindow _host;

    public IUIWidgetsWindow window {
        get { return _host; }
    }

    public Isolate isolate { get; private set; }

    public float devicePixelRatio {
        get { return _devicePixelRatio; }
    }

    public void Initiate(IUIWidgetsWindow host, int width, int height, float dpr,
        Dictionary<string, TextFont> settings) {
        
        D.assert(_renderTexture == null);
        _recreateRenderTexture(width, height, dpr);

        _handle = GCHandle.Alloc(this);
        _ptr = UIWidgetsPanel_constructor((IntPtr) _handle, (int) host.getWindowType(), UIWidgetsPanel_entrypoint);
        _host = host;

        TextFont[] textFonts = loadTextFont(settings);
        var fontsetting = new Dictionary<string, object>();
        fontsetting.Add("fonts", fontsToObject(textFonts));
        _enableUIWidgetsPanel(JSONMessageCodec.instance.toJson(fontsetting));
        NativeConsole.OnEnable();
    }
    public TextFont[] loadTextFont(Dictionary<string,TextFont> TextFontsList) {
        
        TextFont[] textFonts = new TextFont[TextFontsList.Count];
        List<TextFont> fontValues = TextFontsList.Values.ToList();
        for (int i = 0; i < fontValues.Count; i++) {
            textFonts[i] = fontValues[i];
        }
        return textFonts;
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

    public bool didDisplayMetricsChanged(int width, int height, float dpr) {
        return width != _width || height != _height || dpr != _devicePixelRatio;
    }

    public void OnDisplayMetricsChanged(int width, int height, float dpr) {
        if (_ptr != IntPtr.Zero && _renderTexture) {
            if (_recreateRenderTexture(width, height, dpr)) {
                _resizeUIWidgetsPanel();
            }
        }
    }
    public object fontsToObject(UIWidgetsPanel.TextFont[] textFont) {
            if (textFont == null || textFont.Length == 0) {
                return null;
            }
            var result = new object[textFont.Length];
            for (int i = 0; i < textFont.Length; i++) {
                var font = new Dictionary<string, object>();
                font.Add("family", textFont[i].family);
                var dic = new Dictionary<string, object>[textFont[i].fonts.Length];
                for (int j = 0; j < textFont[i].fonts.Length; j++) {
                    dic[j] = new Dictionary<string, object>();
                    if (textFont[i].fonts[j].asset.Length > 0) {
                        dic[j].Add("asset", textFont[i].fonts[j].asset);
                    }

                    if (textFont[i].fonts[j].weight > 0) {
                        dic[j].Add("weight", textFont[i].fonts[j].weight);
                    }
                }
                font.Add("fonts", dic);
                result[i] = font;
            }
            return result;
        }

    public void Destroy() {
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

    public void onEditorUpdate() {
        UIWidgetsPanel_onEditorUpdate(_ptr);
    }

    delegate void UIWidgetsPanel_EntrypointCallback(IntPtr handle);

    [MonoPInvokeCallback(typeof(UIWidgetsPanel_EntrypointCallback))]
    static void UIWidgetsPanel_entrypoint(IntPtr handle) {
        GCHandle gcHandle = (GCHandle) handle;
        UIWidgetsPanelWrapper panel = (UIWidgetsPanelWrapper) gcHandle.Target;
        panel._entryPoint();
    }

    [DllImport(NativeBindings.dllName)]
    static extern IntPtr UIWidgetsPanel_constructor(IntPtr handle, int windowType,
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

#if UNITY_EDITOR
    [DllImport(NativeBindings.dllName)]
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

        UIWidgetsPanel_onMouseMove(_ptr, pos.Value.x, pos.Value.y);
    }

    public void OnMouseScroll(Vector2 delta, Vector2? pos) {
        if (pos == null) {
            return;
        }
        UIWidgetsPanel_onScroll(_ptr, delta.x, delta.y, pos.Value.x, pos.Value.y);
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
    
    [DllImport(NativeBindings.dllName)]
    static extern void UIWidgetsPanel_onScroll(IntPtr ptr, float x, float y, float px, float py);
}

#endregion

}