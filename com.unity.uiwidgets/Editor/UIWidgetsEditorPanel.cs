using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Font = Unity.UIWidgets.engine.Font;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.Editor {
    public class UIWidgetsEditorPanel : EditorWindow, IUIWidgetsWindow {
        
        Configurations _configurations;
        bool _ShowDebugLog;
        UIWidgetsPanelWrapper _wrapper;

        /*
         * In order to wait for the Editor to starts properly before showing the UIWidgets panel,
         * which is essential when we are using EditorMode (https://github.com/Unity-Technologies/unity-editor-core/wiki/EditorModes,
         * available for Unity stuff only) to make an UIWidgets panel as an initial window of the Editor,
         * we add this flag so that the UI will wait for one frame (which seems to be enough for Unity Editor to starts properly)
         * before generating the UIWidgets panel
         */
        bool _needWaitToEnable = true;
        
        Material _uiMaterial = null;

        int _currentWidth {
            get { return Mathf.RoundToInt(f: position.size.x); }
        }

        int _currentHeight {
            get { return Mathf.RoundToInt(f: position.size.y); }
        }

        protected float? devicePixelRatioEditorOnlyOverride = null;

        float _currentDevicePixelRatio {
            get {
                return devicePixelRatioEditorOnlyOverride ?? EditorGUIUtility.pixelsPerPoint; }
        }

        protected virtual void OnUpdate() {
            
        }

        void Update() {
            if (_needWaitToEnable) {
                return;
            }
            
            OnUpdate();
            _wrapper.onEditorUpdate();
        }

        IEnumerator DoEnableAfterOneFrame() {
            yield return null;

            _needWaitToEnable = false;
            DoOnEnable();
        }

        void DoOnEnable() {
            D.assert(_wrapper == null);

            //enable listener to MouseMoveEvents by default
            //user can disable it in onEnable() if needed
            wantsMouseMove = true;
            
            _configurations = new Configurations();
            _wrapper = new UIWidgetsPanelWrapper();
            onEnable();
            _wrapper.Initiate(this, width: _currentWidth, height: _currentHeight, dpr: _currentDevicePixelRatio,
                _configurations: _configurations);
            _configurations.Clear();

            if (_wrapper.requireColorspaceShader) {
                _uiMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.uiwidgets/Resources/uiwidgets_ui.mat");
            }
            
            Input_OnEnable();
        }
        
#if UNITY_EDITOR_OSX
        void TryInitializeOpenGLCoreOnMacEditor() {
            var type = SystemInfo.graphicsDeviceType;
            if (type != GraphicsDeviceType.OpenGLCore) {
                return;
            }
            OpenGLCoreUtil.RenderTextureCreateFailureWorkaround();
            OpenGLCoreUtil.Init();
        }
#endif
        
        void OnEnable() {
            _needWaitToEnable = true;
#if UNITY_EDITOR_OSX
            TryInitializeOpenGLCoreOnMacEditor();
#endif
            startCoroutine(DoEnableAfterOneFrame());
        }

        void OnDestroy() {
            D.assert(_wrapper != null);
            _wrapper?.Destroy();
            _wrapper = null;

            _needWaitToEnable = true;
            Input_OnDisable();
        }

        protected virtual void onGUI() {
            
        }
        
        void OnGUI() {
            if (_wrapper != null) {
                if (_wrapper.didDisplayMetricsChanged(width: _currentWidth, height: _currentHeight,
                    dpr: _currentDevicePixelRatio)) {
                    _wrapper.OnDisplayMetricsChanged(width: _currentWidth, height: _currentHeight,
                        dpr: _currentDevicePixelRatio);
                    //don't show the UI when we are still resizing the window
                    return;
                }
                
                if (_wrapper.renderTexture == null) {
                    return;
                }
                
                EditorGUI.DrawPreviewTexture(new Rect(0.0f, 0.0f, width: position.width, height: position.height),
                    image: _wrapper.renderTexture, mat: _uiMaterial);
                Input_OnGUIEvent(evt: Event.current);

                //user customized onGUI logics goes here
                onGUI();
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
            else if (evt.type == EventType.DragUpdated) {
                var pos = _getPointerPosition(position: evt.mousePosition);
                _wrapper.OnDragUpdateInEditor(pos: pos);
            }
            else if (evt.type == EventType.DragPerform || evt.type == EventType.DragExited) {
                var pos = _getPointerPosition(position: evt.mousePosition);
                _wrapper.OnDragReleaseInEditor(pos: pos);
            }
            else if (evt.isKey) {
                _wrapper.OnKeyDown(e: evt);
                Event.current.Use();
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