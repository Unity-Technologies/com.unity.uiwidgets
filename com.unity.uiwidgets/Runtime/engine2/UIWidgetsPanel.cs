using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.editor2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NativeBindings = Unity.UIWidgets.ui.NativeBindings;

namespace Unity.UIWidgets.engine2 {

    public interface IUIWidgetsWindow {
        Offset windowPosToScreenPos(Offset offset);

        Coroutine startCoroutine(IEnumerator routing);

        bool isActive();

        void mainEntry();
    }
    
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public partial class UIWidgetsPanel : IUIWidgetsWindow{
        UIWidgetsPanelWrapper _wrapper;
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
            var settings = new Dictionary<string, object>();
            if (fonts != null && fonts.Length > 0) {
                settings.Add("fonts", fontsToObject(fonts));
            }
            
            D.assert(_wrapper == null);
            _wrapper = new UIWidgetsPanelWrapper();
            _wrapper.Initiate(this, _currentWidth, _currentHeight, _currentDevicePixelRatio, settings);
            texture = _wrapper.renderTexture;
        }

        public void mainEntry() {
            main();
        }

        protected virtual void main() {
        }

        protected override void OnRectTransformDimensionsChange() {
            if (_wrapper != null) {
                _wrapper.OnRectTransformDimensionsChange(_currentWidth, _currentHeight, _currentDevicePixelRatio);
                texture = _wrapper.renderTexture;
            }
        }
        
        protected override void OnDisable() {
            D.assert(_wrapper != null);
            _wrapper?.Destroy();
            _wrapper = null;
            texture = null;
            base.OnDisable();
        }

        protected virtual void Update() {
            Input_Update();
        }

        protected virtual void OnGUI() {
            Input_OnGUI();
        }
    }

    public partial class UIWidgetsPanel : IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IDragHandler {

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

        public bool isActive() {
            return IsActive();
        }

        public Coroutine startCoroutine(IEnumerator routing) {
            return StartCoroutine(routing);
        }
        
        public Offset windowPosToScreenPos(Offset offset) {
            Camera camera = null;
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera) {
                camera = canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }

            var pos = new Vector2(offset.dx, offset.dy);
            pos = pos * _currentDevicePixelRatio / canvas.scaleFactor;
            var rect = rectTransform.rect;
            pos.x += rect.min.x;
            pos.y = rect.max.y - pos.y;
            var worldPos = rectTransform.TransformPoint(new Vector2(pos.x, pos.y));
            var screenPos = RectTransformUtility.WorldToScreenPoint(camera, worldPos);
            return new Offset(screenPos.x, Screen.height - screenPos.y);
        }
                
        bool _isEntered;
        Vector2 _lastMousePosition;

        public void Input_Update() {
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

        public void Input_OnGUI() {
            Event e = Event.current;
            if (e.isKey) {
                _wrapper.OnKeyDown(e);
            }
        }

        void _onMouseMove() {
            var pos = _getPointerPosition(Input.mousePosition);
            _wrapper.OnMouseMove(pos);
        }

        public void OnPointerDown(PointerEventData eventData) {
            var pos = _getPointerPosition(Input.mousePosition);
            _wrapper.OnPointerDown(pos, eventData.pointerId);
        }

        public void OnPointerUp(PointerEventData eventData) {
            var pos = _getPointerPosition(Input.mousePosition);
            _wrapper.OnPointerUp(pos, eventData.pointerId);
        }
        
        public void OnPointerEnter(PointerEventData eventData) {
            D.assert(eventData.pointerId < 0);
            _isEntered = true;
            _lastMousePosition = Input.mousePosition;
        }

        public void OnPointerExit(PointerEventData eventData) {
            D.assert(eventData.pointerId < 0);
            _isEntered = false;
            _wrapper.OnPointerLeave();
        }

        public void OnDrag(PointerEventData eventData) {
            var pos = _getPointerPosition(Input.mousePosition);
            _wrapper.OnDrag(pos, eventData.pointerId);
        }
    }
}