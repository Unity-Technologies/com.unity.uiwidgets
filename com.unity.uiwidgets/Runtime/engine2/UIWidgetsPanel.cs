using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.editor2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        public static List<UIWidgetsPanel> panels = new List<UIWidgetsPanel>();

        static bool _ShowDebugLog;

        public float devicePixelRatioOverride;

        public bool hardwareAntiAliasing;

        public TextFont[] fonts;

        public bool m_ShowDebugLog;

        readonly Dictionary<string, TextFont> _internalTextFonts = new Dictionary<string, TextFont>();
        UIWidgetsPanelWrapper _wrapper;

        int _currentWidth {
            get { return Mathf.RoundToInt(rectTransform.rect.width * canvas.scaleFactor); }
        }

        int _currentHeight {
            get { return Mathf.RoundToInt(rectTransform.rect.height * canvas.scaleFactor); }
        }

        float _currentDevicePixelRatio {
            get {
                var currentDpi = Screen.dpi;
                if (currentDpi == 0) {
                    currentDpi = canvas.GetComponent<CanvasScaler>().fallbackScreenDPI;
                }

                return currentDpi / 96;
            }
        }

        public static bool ShowDebugLog {
            get { return _ShowDebugLog; }
            set {
                foreach (var panel in panels) {
                    panel.m_ShowDebugLog = value;
                }

                _ShowDebugLog = value;
            }
        }

        protected virtual void Update() {
            Input_Update();
        }

        protected void OnEnable() {
            base.OnEnable();
            D.assert(_wrapper == null);
            _wrapper = new UIWidgetsPanelWrapper();
            onEnable();
            if (fonts != null && fonts.Length > 0) {
                addFontFromInspector(fonts: fonts);
            }

            _wrapper.Initiate(this, width: _currentWidth, height: _currentHeight, dpr: _currentDevicePixelRatio,
                settings: _internalTextFonts);
            texture = _wrapper.renderTexture;
            _internalTextFonts.Clear();
            Input_OnEnable();
            panels.Add(this);
            _ShowDebugLog = m_ShowDebugLog;
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

        protected virtual void OnGUI() {
            Input_OnGUI();
        }

        protected override void OnRectTransformDimensionsChange() {
            if (_wrapper != null && _wrapper.didDisplayMetricsChanged(width: _currentWidth, height: _currentHeight,
                dpr: _currentDevicePixelRatio)) {
                _wrapper.OnDisplayMetricsChanged(width: _currentWidth, height: _currentHeight,
                    dpr: _currentDevicePixelRatio);
                texture = _wrapper.renderTexture;
            }
        }

        public UIWidgetsWindowType getWindowType() {
            return UIWidgetsWindowType.GameObjectPanel;
        }

        public bool isActive() {
            return IsActive();
        }

        public void startCoroutine(IEnumerator routing) {
            StartCoroutine(routine: routing);
        }

        public void onNewFrameScheduled() {
        }

        public Offset windowPosToScreenPos(Offset offset) {
            Camera camera = null;
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera) {
                camera = canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }

            var pos = new Vector2(x: offset.dx, y: offset.dy);
            pos = pos * _currentDevicePixelRatio / canvas.scaleFactor;
            var rect = rectTransform.rect;
            pos.x += rect.min.x;
            pos.y = rect.max.y - pos.y;
            var worldPos = rectTransform.TransformPoint(new Vector2(x: pos.x, y: pos.y));
            var screenPos = RectTransformUtility.WorldToScreenPoint(cam: camera, worldPoint: worldPos);
            return new Offset(dx: screenPos.x, Screen.height - screenPos.y);
        }

        public void mainEntry() {
            main();
        }

        protected virtual void onEnable() {
        }

        protected void AddFont(string family, List<string> assets, List<int> weights) {
            if (assets.Count != weights.Count) {
                Debug.LogError($"The size of {family}‘s assets should be equal to the weights'.");
                return;
            }

            if (_internalTextFonts.ContainsKey(key: family)) {
                var textFont = _internalTextFonts[key: family];
                var fonts = new Font[assets.Count];
                for (var j = 0; j < assets.Count; j++) {
                    var font = new Font();
                    font.asset = assets[index: j];
                    font.weight = weights[index: j];
                    fonts[j] = font;
                }

                textFont.fonts = fonts;
                _internalTextFonts[key: family] = textFont;
            }
            else {
                var textFont = new TextFont();
                textFont.family = family;
                var fonts = new Font[assets.Count];
                for (var j = 0; j < assets.Count; j++) {
                    var font = new Font();
                    font.asset = assets[index: j];
                    font.weight = weights[index: j];
                    fonts[j] = font;
                }

                textFont.fonts = fonts;
                _internalTextFonts.Add(key: family, value: textFont);
            }
        }

        void addFontFromInspector(TextFont[] fonts) {
            foreach (var font in fonts) {
                if (_internalTextFonts.ContainsKey(key: font.family)) {
                    _internalTextFonts[key: font.family] = font;
                }
                else {
                    _internalTextFonts.Add(key: font.family, value: font);
                }
            }
        }

        protected virtual void main() {
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
    }

    public partial class UIWidgetsPanel : IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IDragHandler {
        bool _isEntered;
        Vector2 _lastMousePosition;

        public void OnDrag(PointerEventData eventData) {
            var pos = _getPointerPosition(position: Input.mousePosition);
            _wrapper.OnDrag(pos: pos, pointerId: eventData.pointerId);
        }

        public void OnPointerDown(PointerEventData eventData) {
            var pos = _getPointerPosition(position: Input.mousePosition);
            _wrapper.OnPointerDown(pos: pos, pointerId: eventData.pointerId);
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

        public void OnPointerUp(PointerEventData eventData) {
            var pos = _getPointerPosition(position: Input.mousePosition);
            _wrapper.OnPointerUp(pos: pos, pointerId: eventData.pointerId);
        }

        Vector2? _getPointerPosition(Vector2 position) {
            var worldCamera = canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect: rectTransform, screenPoint: position, cam: worldCamera, out var localPoint)) {
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
            var e = Event.current;
            if (e.isKey) {
                _wrapper.OnKeyDown(e: e);
            }
        }

        void _onMouseMove() {
            var pos = _getPointerPosition(position: Input.mousePosition);
            _wrapper.OnMouseMove(pos: pos);
        }

        void _onScroll() {
            var pos = _getPointerPosition(position: Input.mousePosition);
            _wrapper.OnMouseScroll(delta: Input.mouseScrollDelta, pos: pos);
        }
    }
}