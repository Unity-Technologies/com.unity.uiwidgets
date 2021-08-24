using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Font = Unity.UIWidgets.engine.Font;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.Editor {
    public class UIWidgetsEditorPanel : EditorWindow, IUIWidgetsWindow {
        
        Configurations _configurations;
        bool _ShowDebugLog;
        UIWidgetsPanelWrapper _wrapper;

        int _currentWidth {
            get { return Mathf.RoundToInt(f: position.size.x); }
        }

        int _currentHeight {
            get { return Mathf.RoundToInt(f: position.size.y); }
        }

        float _currentDevicePixelRatio {
            get { return EditorGUIUtility.pixelsPerPoint; }
        }

        void Update() {
            _wrapper.onEditorUpdate();
        }

        void OnEnable() {
            D.assert(_wrapper == null);
            _configurations = new Configurations();
            _wrapper = new UIWidgetsPanelWrapper();
            onEnable();
            _wrapper.Initiate(this, width: _currentWidth, height: _currentHeight, dpr: _currentDevicePixelRatio,
                _configurations: _configurations);
            _configurations.Clear();
            Input_OnEnable();
        }

        void OnDestroy() {
            D.assert(_wrapper != null);
            _wrapper?.Destroy();
            _wrapper = null;
            Input_OnDisable();
        }

        void OnGUI() {
            if (_wrapper != null) {
                if (_wrapper.didDisplayMetricsChanged(width: _currentWidth, height: _currentHeight,
                    dpr: _currentDevicePixelRatio)) {
                    _wrapper.OnDisplayMetricsChanged(width: _currentWidth, height: _currentHeight,
                        dpr: _currentDevicePixelRatio);
                }

                GUI.DrawTexture(new Rect(0.0f, 0.0f, width: position.width, height: position.height),
                    image: _wrapper.renderTexture);
                Input_OnGUIEvent(evt: Event.current);
            }
        }

        public UIWidgetsWindowType getWindowType() {
            return UIWidgetsWindowType.EditorWindowPanel;
        }

        public bool isActive() {
            return true;
        }

        public void startCoroutine(IEnumerator routing) {
            this.StartCoroutine(coroutine: routing);
        }

        public void onNewFrameScheduled() {
            Repaint();
        }

        public Offset windowPosToScreenPos(Offset offset) {
            return offset;
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

            var textFont = new TextFont {family = family};
            var fonts = new Font[assets.Count];
            for (var j = 0; j < assets.Count; j++) {
                var font = new Font {asset = assets[index: j], weight = weights[index: j]};
                fonts[j] = font;
            }

            textFont.fonts = fonts;
            _configurations.AddFont(family,textFont);
        }

        Vector2? _getPointerPosition(Vector2 position) {
            return new Vector2(x: position.x, y: position.y);
        }

        int _buttonToPointerId(int buttonId) {
            if (buttonId == 0) {
                return -1;
            }

            if (buttonId == 1) {
                return -2;
            }

            return 0;
        }

        void Input_OnGUIEvent(Event evt) {
            if (evt.type == EventType.MouseDown) {
                var pos = _getPointerPosition(position: evt.mousePosition);
                _wrapper.OnPointerDown(pos: pos, _buttonToPointerId(buttonId: evt.button));
            }
            else if (evt.type == EventType.MouseUp || evt.rawType == EventType.MouseUp) {
                var pos = _getPointerPosition(position: evt.mousePosition);
                _wrapper.OnPointerUp(pos: pos, _buttonToPointerId(buttonId: evt.button));
            }
            else if (evt.type == EventType.MouseDrag) {
                var pos = _getPointerPosition(position: evt.mousePosition);
                _wrapper.OnMouseMove(pos: pos);
            }
            else if (evt.type == EventType.MouseMove) {
                var pos = _getPointerPosition(position: evt.mousePosition);
                _wrapper.OnMouseMove(pos: pos);
            }
            else if (evt.type == EventType.ScrollWheel && evt.delta.magnitude != 0) {
                var delta = evt.delta;
                delta.y /= 3f;
                var pos = _getPointerPosition(position: evt.mousePosition);
                _wrapper.OnMouseScroll(delta: delta, pos: pos);
            }
            else if (evt.isKey) {
                _wrapper.OnKeyDown(e: evt);
            }
        }

        protected virtual void main() {
        }

        void Input_OnDisable() {
        }

        void Input_OnEnable() {
        }
    }
}