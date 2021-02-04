using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NativeBindings = Unity.UIWidgets.ui.NativeBindings;

namespace Unity.UIWidgets.engine2 {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public partial class UIWidgetsPanel {
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

            texture = _renderTexture;
        }

        void _destroyRenderTexture() {
            D.assert(_renderTexture != null);
            texture = null;
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
            texture = null;
        }


        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onEnable(IntPtr ptr,
            IntPtr nativeTexturePtr, int width, int height, float dpi, string streamingAssetsPath,
            string font_settings);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onRenderTexture(
            IntPtr ptr, IntPtr nativeTexturePtr, int width, int height, float dpi);
    }

#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    public partial class UIWidgetsPanel {
        Texture _renderTexture;
    
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
            texture = null;
        }

        void _enableUIWidgetsPanel(string font_settings) {
            D.assert(_renderTexture == null);
            IntPtr native_tex_ptr = UIWidgetsPanel_onEnable(_ptr, _width, _height, _devicePixelRatio,
                Application.streamingAssetsPath, font_settings);
            D.assert(native_tex_ptr != IntPtr.Zero);

            _renderTexture =
                Texture2D.CreateExternalTexture(_width, _height, TextureFormat.BGRA32, false, true, native_tex_ptr);

            texture = _renderTexture;
        }

        void _disableUIWidgetsPanel() {
            _renderTexture = null;
            texture = null;
        }
        
        void _resizeUIWidgetsPanel()
        {
            D.assert(_renderTexture == null);
            
            IntPtr native_tex_ptr = UIWidgetsPanel_onRenderTexture(_ptr, _width, _height, _devicePixelRatio);
            D.assert(native_tex_ptr != IntPtr.Zero);
            
            _renderTexture =
                Texture2D.CreateExternalTexture(_width, _height, TextureFormat.BGRA32, false, true, native_tex_ptr);

            texture = _renderTexture;
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

    public partial class UIWidgetsPanel : RawImage {
        [Serializable]
        public struct Font {
            public string asset;
            public int weight;
        }

        [Serializable]
        public struct TextFont {
            public string family;
            [SerializeField] public Font[] fonts;
        }

        public TextFont[] fonts;

        static object fontsToObject(TextFont[] textFont) {
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

        public float devicePixelRatioOverride = 1;


        public float hardwareAntiAliasing;
        // RectTransform rectTransform {
        //     get { return rectTransform; }
        // }

        // Canvas canvas {
        //     get { return canvas; }
        // }

        // Texture texture {
        //     set { texture = value; }
        // }

        public static UIWidgetsPanel current {
            get { return Window.instance._panel; }
        }

        public Isolate isolate { get; private set; }

        IntPtr _ptr;
        GCHandle _handle;

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

        protected void OnEnable() {
            base.OnEnable();
            D.assert(_renderTexture == null);
            _recreateRenderTexture(_currentWidth, _currentHeight, _currentDevicePixelRatio);

            _handle = GCHandle.Alloc(this);
            _ptr = UIWidgetsPanel_constructor((IntPtr) _handle, UIWidgetsPanel_entrypoint);
            var settings = new Dictionary<string, object>();
            if (fonts != null && fonts.Length > 0) {
                settings.Add("fonts", fontsToObject(fonts));
            }

            _enableUIWidgetsPanel(JSONMessageCodec.instance.toJson(settings));

            Input_OnEnable();
            NativeConsole.OnEnable();
        }

        protected virtual void main() {
        }

        void _entryPoint() {
            try {
                isolate = Isolate.current;
                Window.instance._panel = this;

                main();
            }
            catch (Exception ex) {
                Debug.LogException(new Exception("exception in main", ex));
            }
        }

        protected override void OnRectTransformDimensionsChange() {
            if (_ptr != IntPtr.Zero && _renderTexture) {
                if (_recreateRenderTexture(_currentWidth, _currentHeight, _currentDevicePixelRatio)) {
                    _resizeUIWidgetsPanel();
                }
            }
        }

        protected override void OnDisable() {
            Input_OnDisable();

            UIWidgetsPanel_onDisable(_ptr);
            UIWidgetsPanel_dispose(_ptr);
            _ptr = IntPtr.Zero;

            _handle.Free();
            _handle = default;

            _disableUIWidgetsPanel();

            D.assert(!isolate.isValid);
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

        protected virtual void Update() {
            Input_Update();
        }

        protected virtual void OnGUI() {
            Input_OnGUI();
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


        delegate void UIWidgetsPanel_EntrypointCallback(IntPtr handle);

        [MonoPInvokeCallback(typeof(UIWidgetsPanel_EntrypointCallback))]
        static void UIWidgetsPanel_entrypoint(IntPtr handle) {
            GCHandle gcHandle = (GCHandle) handle;
            UIWidgetsPanel panel = (UIWidgetsPanel) gcHandle.Target;
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
    }

    public partial class UIWidgetsPanel : IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IDragHandler {
        bool _isEntered;
        Vector2 _lastMousePosition;

        void Input_OnEnable() {
        }

        void Input_OnDisable() {
        }

        void Input_Update() {
            if (Input.touchCount == 0 && Input.mousePresent) {
                if (_isEntered) {
                    if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)) {
                        if (_lastMousePosition.x != Input.mousePosition.x ||
                            _lastMousePosition.y != Input.mousePosition.y) {
                            _lastMousePosition = Input.mousePosition;
                            _onMouseMove();
                        }
                    }
                    else {
                        _lastMousePosition = Input.mousePosition;
                    }
                }
            }
        }

        void Input_OnGUI() {
            Event e = Event.current;
            if (e.isKey) {
                UIWidgetsPanel_onKey(_ptr, e.keyCode, e.type == EventType.KeyDown);
                if (e.character != 0 || e.keyCode == KeyCode.Backspace) {
                    PointerEventConverter.KeyEvent.Enqueue(new Event(e));
                    // TODO: add on char
                    // UIWidgetsPanel_onChar(_ptr, e.character);
                }
            }
        }

        Vector2? _getPointerPosition(Vector2 position) {
            Camera worldCamera = canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, position, worldCamera, out var localPoint)) {
                var scaleFactor = canvas.scaleFactor;
                localPoint.x = (localPoint.x - rectTransform.rect.min.x) * scaleFactor;
                localPoint.y = (rectTransform.rect.max.y - localPoint.y) * scaleFactor;
                return localPoint;
            }

            return null;
        }

        void _onMouseMove() {
            var pos = _getPointerPosition(Input.mousePosition);
            if (pos == null) {
                return;
            }

            UIWidgetsPanel_onMouseMove(_ptr, pos.Value.x, pos.Value.y);
        }

        public void OnPointerDown(PointerEventData eventData) {
            var pos = _getPointerPosition(Input.mousePosition);
            if (pos == null) {
                return;
            }

            // mouse event
            if (eventData.pointerId < 0) {
                UIWidgetsPanel_onMouseDown(_ptr, pos.Value.x, pos.Value.y, eventData.pointerId);
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            var pos = _getPointerPosition(Input.mousePosition);
            if (pos == null) {
                return;
            }

            // mouse event
            if (eventData.pointerId < 0) {
                UIWidgetsPanel_onMouseUp(_ptr, pos.Value.x, pos.Value.y, eventData.pointerId);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            D.assert(eventData.pointerId < 0);
            _isEntered = true;
            _lastMousePosition = Input.mousePosition;
        }

        public void OnPointerExit(PointerEventData eventData) {
            D.assert(eventData.pointerId < 0);
            _isEntered = false;
            UIWidgetsPanel_onMouseLeave(_ptr);
        }

        public void OnDrag(PointerEventData eventData) {
            var pos = _getPointerPosition(Input.mousePosition);
            if (pos == null) {
                return;
            }

            // mouse event
            if (eventData.pointerId < 0) {
                UIWidgetsPanel_onMouseMove(_ptr, pos.Value.x, pos.Value.y);
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
    }
}