using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.editor2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextFont = Unity.UIWidgets.engine2.UIWidgetsPanel.TextFont;
using Font = Unity.UIWidgets.engine2.UIWidgetsPanel.Font;

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
        
        protected virtual void onEnable() {}

        Dictionary<string,TextFont> _internalTextFonts = new Dictionary<string, TextFont>();
        protected void AddFont(string family, List<string> assets, List<int> weights) {
            if (assets.Count != weights.Count) {
                UnityEngine.Debug.LogError($"The size of {family}‘s assets should be equal to the weights'.");
                return;
            }
            if (!_internalTextFonts.ContainsKey(family)) {
                TextFont textFont = new TextFont();
                textFont.family = family;
                Font[] fonts = new Font[assets.Count];
                for (int j = 0; j < assets.Count; j++) {
                    Font font = new Font();
                    font.asset = assets[j];
                    font.weight = weights[j];
                    fonts[j] = font;
                }
                textFont.fonts = fonts;
                _internalTextFonts.Add(family,textFont);
            }
        }

        protected void OnEnable() {
            base.OnEnable();
            D.assert(_wrapper == null);
            _wrapper = new UIWidgetsPanelWrapper();
            
            if (fonts != null && fonts.Length > 0) {
                foreach (var font in fonts) {
                    if(!_internalTextFonts.ContainsKey(font.family))
                        _internalTextFonts.Add(font.family,font);
                }
                onEnable();
            }
            else {
                onEnable();
            }
            _wrapper.Initiate(this, _currentWidth, _currentHeight, _currentDevicePixelRatio, _internalTextFonts);
            texture = _wrapper.renderTexture;
            _internalTextFonts.Clear();
            Input_OnEnable();
            panels.Add(this);
            _ShowDebugLog = m_ShowDebugLog;
        }
        TextFont[] loadTextFont() {
            TextFont[] textFonts = new TextFont[_internalTextFonts.Count];
            List<TextFont> fontValues = _internalTextFonts.Values.ToList();
            for (int i = 0; i < fontValues.Count; i++) {
                textFonts[i] = fontValues[i];
            }
            return textFonts;
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
            _internalTextFonts.Clear();
            Input_OnDisable();
            base.OnDisable();
            panels.Remove(this);
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