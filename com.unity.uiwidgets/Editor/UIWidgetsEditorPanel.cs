using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.UIWidgets.editor2;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.Editor {
    public class UIWidgetsEditorPanel : EditorWindow, IUIWidgetsWindow {
        UIWidgetsPanelWrapper _wrapper;
       
        public UIWidgetsWindowType getWindowType() {
            return UIWidgetsWindowType.EditorWindowPanel;
        }
        
        public bool isActive() {
            return true;
        }

        public void startCoroutine(IEnumerator routing) {
            this.StartCoroutine(routing);
        }
        
        public void onNewFrameScheduled() {
            Repaint();
        }

        public Offset windowPosToScreenPos(Offset offset) {
            return offset;
        }

        int _currentWidth {
            get { return Mathf.RoundToInt(position.size.x); }
        }

        int _currentHeight {
            get { return Mathf.RoundToInt(position.size.y); }
        }

        float _currentDevicePixelRatio {
            get { return EditorGUIUtility.pixelsPerPoint; }
        }

        void OnDestroy() {
            D.assert(_wrapper != null);
            _wrapper?.Destroy();
            _wrapper = null;
            _internalTextFonts.Clear();
            Input_OnDisable();
        }
        
        protected virtual void onFill() {}

        List<UIWidgetsPanel.TextFont> _internalTextFonts = new List<UIWidgetsPanel.TextFont>();
        protected void AddFont(string family, List<string> assets, List<int> weights) {
            int repeat = 0;
            for(int i = 0; i< _internalTextFonts.Count; i++ ) {
                if (_internalTextFonts[i].family == family)
                    repeat++;
            }
            if (repeat == 0) {
                UIWidgetsPanel.TextFont textFont = new UIWidgetsPanel.TextFont();
                textFont.family = family;
                UIWidgetsPanel.Font[] fonts = new UIWidgetsPanel.Font[assets.Count];
                for (int i = 0; i < assets.Count; i++) {
                    UIWidgetsPanel.Font font = new UIWidgetsPanel.Font();
                    font.asset = assets[i];
                    font.weight = weights[i];
                    fonts[i] = font;
                }
                textFont.fonts = fonts;
                _internalTextFonts.Add(textFont);
            }
        }

        void OnEnable() {
            D.assert(_wrapper == null);
            _wrapper = new UIWidgetsPanelWrapper();
            
            onFill();
            var settings = new Dictionary<string, object>();
            UIWidgetsPanel.TextFont[] textFonts = loadTextFont();
            settings.Add("fonts", fontsToObject(textFonts));
            
            _wrapper.Initiate(this, _currentWidth, _currentHeight, _currentDevicePixelRatio, settings);
            Input_OnEnable();
        }

        void Update() {
            _wrapper.onEditorUpdate();
        }

        void OnGUI()
        {
            if (_wrapper != null) {
                if (_wrapper.didDisplayMetricsChanged(_currentWidth, _currentHeight, _currentDevicePixelRatio)) {
                    _wrapper.OnDisplayMetricsChanged(_currentWidth, _currentHeight, _currentDevicePixelRatio);
                }
                
                GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), _wrapper.renderTexture);
                Input_OnGUIEvent(Event.current);
            }
        }

        Vector2? _getPointerPosition(Vector2 position) {
            return new Vector2(position.x, position.y);
        }
        static object fontsToObject(UIWidgetsPanel.TextFont[] textFont) {
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

        UIWidgetsPanel.TextFont[] loadTextFont() {
            
            UIWidgetsPanel.TextFont[] textFonts = new UIWidgetsPanel.TextFont[_internalTextFonts.Count];
            for (int i = 0; i < _internalTextFonts.Count; i++) {
                textFonts[i] = _internalTextFonts[i];
            }
            return textFonts;
        }


        int _buttonToPointerId(int buttonId) {
            if (buttonId == 0) {
                return -1;
            }
            else if (buttonId == 1) {
                return -2;
            }

            return 0;
        }

        void Input_OnGUIEvent(Event evt) {
            if (evt.type == EventType.MouseDown) {
                var pos = _getPointerPosition(evt.mousePosition);
                _wrapper.OnPointerDown(pos, _buttonToPointerId(evt.button));
            }
            else if (evt.type == EventType.MouseUp || evt.rawType == EventType.MouseUp) {
                var pos = _getPointerPosition(evt.mousePosition);
                _wrapper.OnPointerUp(pos, _buttonToPointerId(evt.button));
            }
            else if (evt.type == EventType.MouseDrag) {
                var pos = _getPointerPosition(evt.mousePosition);
                _wrapper.OnMouseMove(pos);
            }
            else if (evt.type == EventType.MouseMove) {
                var pos = _getPointerPosition(evt.mousePosition);
                _wrapper.OnMouseMove(pos);
            }
            else if (evt.type == EventType.ScrollWheel && evt.delta.magnitude != 0) {
                var delta = evt.delta;
                delta.y /= 3f;
                var pos = _getPointerPosition(evt.mousePosition);
                _wrapper.OnMouseScroll(delta, pos);
            }
            
        }
        
        public void mainEntry() { 
            main();
        }

        protected virtual void main() {
        }

        void Input_OnDisable() {
            
        }

        void Input_OnEnable() {
            
        }
    }
}