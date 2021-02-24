using System.Collections;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.editor2 {
#if UNITY_EDITOR
    
    class MyApp : StatelessWidget
    {
        public override Widget build(BuildContext context)
        {
            return new WidgetsApp(
                home: new Container(color: Colors.blue),
                pageRouteBuilder: (settings, builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                    )
            );
        }
    }

    public class UIWidgetsEditorPanel : EditorWindow, IUIWidgetsWindow {
        UIWidgetsPanelWrapper _wrapper;
        
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

            Input_OnDisable();
            
            Debug.Log("destroy");
        }

        void OnEnable() {
            D.assert(_wrapper == null);
            _wrapper = new UIWidgetsPanelWrapper();
            _wrapper.Initiate(this, _currentWidth, _currentHeight, _currentDevicePixelRatio, new Dictionary<string, object>());
            
            Input_OnEnable();
            
            Debug.Log("enabled");
        }

        void OnGUI()
        {
            if (_wrapper != null) {
                if (_wrapper.didDisplayMetricsChanged(_currentWidth, _currentHeight, _currentDevicePixelRatio)) {
                    _wrapper.OnDisplayMetricsChanged(_currentWidth, _currentHeight, _currentDevicePixelRatio);
                }
                
                GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), _wrapper.renderTexture);

                Input_OnGUIEvent(Event.current);
                
                Repaint();
            }
        }

        Vector2? _getPointerPosition(Vector2 position) {
            return new Vector2(position.x * _currentDevicePixelRatio, position.y * _currentDevicePixelRatio);
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
#endif

}