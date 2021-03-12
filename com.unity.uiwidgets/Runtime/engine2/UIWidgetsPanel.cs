using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.editor2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Unity.UIWidgets.engine2 {

    public enum UIWidgetsWindowType {
        InvalidPanel = 0,
        GameObjectPanel = 1,
        EditorWindowPanel = 2
    }
    
    public interface IUIWidgetsWindow {
        Offset windowPosToScreenPos(Offset offset);

        void startCoroutine(IEnumerator routing);

        bool isActive();

        void mainEntry();

        void onNewFrameScheduled();

        UIWidgetsWindowType getWindowType();
    }

    public partial class UIWidgetsPanel : RawImage, IUIWidgetsWindow {
        UIWidgetsPanelWrapper _wrapper;

        public float devicePixelRatioOverride;

        public bool hardwareAntiAliasing;

        public UIWidgetsWindowType getWindowType() {
            return UIWidgetsWindowType.GameObjectPanel;
        }
        
        public bool isActive() {
            return IsActive();
        }

        public void startCoroutine(IEnumerator routing) {
            StartCoroutine(routing);
        }

        public void onNewFrameScheduled() {
            
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

        public bool m_ShowDebugLog = false;
        
        public static List<UIWidgetsPanel> panels = new List<UIWidgetsPanel>();
        public static bool ShowDebugLog {
            get => _ShowDebugLog;
            set {
                foreach (var panel in panels) {
                    panel.m_ShowDebugLog = value;
                }

                _ShowDebugLog = value;
            }
        }
        
        static bool _ShowDebugLog = false;
        Texture2D targetTexture = null;

        protected void OnEnable() {
            base.OnEnable();
            var settings = new Dictionary<string, object>();
            if (fonts != null && fonts.Length > 0) {
                settings.Add("fonts", fontsToObject(fonts));
            }

            D.assert(_wrapper == null);
            _wrapper = new UIWidgetsPanelWrapper();
            _wrapper.Initiate(this, _currentWidth, _currentHeight, _currentDevicePixelRatio, settings);
            if (targetTexture == null) {
                targetTexture = new Texture2D(_currentWidth, _currentHeight, TextureFormat.BGRA32, 1, true);
                // GetComponent<Renderer>().material.mainTexture = targetTexture;
                Debug.Log($"{_currentWidth}, {_currentHeight}");
                for (int y = 0; y < _currentHeight; y++) {
                    for (int x = 0; x < _currentWidth; x++) {

                        targetTexture.SetPixel(x, y, Color.green);

                    }
                }

                targetTexture.Apply();
            }
            texture = targetTexture;

            Input_OnEnable();
            
            panels.Add(this);
            _ShowDebugLog = m_ShowDebugLog;
        }

        public void mainEntry() {
            main();
        }

        protected virtual void main() {
        }

        protected override void OnRectTransformDimensionsChange() {
            if (_wrapper != null && _wrapper.didDisplayMetricsChanged(_currentWidth, _currentHeight, _currentDevicePixelRatio)) {
                _wrapper.OnDisplayMetricsChanged(_currentWidth, _currentHeight, _currentDevicePixelRatio);
                texture = _wrapper.renderTexture;
            }
        }

        protected override void OnDisable() {
            D.assert(_wrapper != null);
            _wrapper?.Destroy();
            _wrapper = null;
            texture = null;

            Input_OnDisable();
            base.OnDisable();
            
            panels.Remove(this);
        }

        protected virtual void Update() {
            Graphics.CopyTexture(_wrapper.renderTexture,0,0, 100, 100, 400, 400, targetTexture, 0, 0, 100, 100);
            Input_Update();
        }

        protected virtual void OnGUI() {
            Input_OnGUI();
        }
    }

    public partial class UIWidgetsPanel : IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IDragHandler {
        bool _isEntered;
        Vector2 _lastMousePosition;

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

                    if (Input.mouseScrollDelta.magnitude != 0) {
                        _onScroll();
                    }
                }
            }
        }

        void Input_OnGUI() {
            Event e = Event.current;
            if (e.isKey) {
                _wrapper.OnKeyDown(e);
            }
        }

        void _onMouseMove() {
            var pos = _getPointerPosition(Input.mousePosition);
            _wrapper.OnMouseMove(pos);
        }

        void _onScroll() {
            var pos = _getPointerPosition(Input.mousePosition);
            _wrapper.OnMouseScroll(Input.mouseScrollDelta, pos);
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