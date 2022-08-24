using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Path = System.IO.Path;
using RawImage = UnityEngine.UI.RawImage;

namespace Unity.UIWidgets.engine {
    public enum UIWidgetsWindowType {
        InvalidPanel = 0,
        GameObjectPanel = 1,
        EditorWindowPanel = 2
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
        public string absPath;
    }

    public interface IUIWidgetsWindow {
        Offset windowPosToScreenPos(Offset offset);

        void startCoroutine(IEnumerator routing);

        bool isActive();

        void mainEntry();

        void onNewFrameScheduled();

        UIWidgetsWindowType getWindowType();
    }
    public class Configurations {
        private Dictionary<string, TextFont> _textFonts = new Dictionary<string, TextFont>();

        public void Clear() {
            _textFonts.Clear();
        }
        public void AddFont(string family, TextFont font) {
            _textFonts[key: family] = font;
        }
        public object fontsToObject() {
            Dictionary<string, TextFont> settings = _textFonts;
            if (settings == null || settings.Count == 0) {
                return null;
            }

            var result = new object[settings.Count];
            var i = 0;
            foreach (var setting in settings) {
                var font = new Dictionary<string, object>();
                font.Add("family", value: setting.Key);
                var dic = new List<Dictionary<string, object>>();
                for (var j = 0; j < setting.Value.fonts.Length; j++) {
                    var fontDic = new Dictionary<string, object>();
                    var fileExist = false;

                    if (setting.Value.fonts[j].asset.Length > 0) {
                        var assetPath = setting.Value.fonts[j].asset;
                        var assetAbsolutePath = Path.Combine(setting.Value.absPath ?? Application.streamingAssetsPath, assetPath);
#if !UNITY_EDITOR && UNITY_ANDROID
                        if (!AndroidPlatformUtil.FileExists(assetPath)) {
#else
                        if (!File.Exists(assetAbsolutePath)) {
#endif
                            Debug.LogError($"The font asset (family: \"{setting.Key}\", path: \"{assetPath}\") is not found");
                        }
                        else {
                            fileExist = true;
                        }
                        fontDic.Add("asset", value: setting.Value.fonts[j].asset);
                    }

                    if (setting.Value.fonts[j].weight > 0) {
                        fontDic.Add("weight", value: setting.Value.fonts[j].weight);
                    }

                    if (fileExist) {
                        dic.Add(fontDic);
                    }
                }

                font.Add("fonts", value: dic.ToArray());
                result[i] = font;
                i++;
            }

            return result;
        }
    }

    [ExecuteInEditMode]
    public partial class UIWidgetsPanel : RawImage, IUIWidgetsWindow {
        static List<UIWidgetsPanel> panels = new List<UIWidgetsPanel>();

        static public Isolate anyIsolate {
            get {
                if (panels.Count == 0) {
                    return null;
                }

                return panels[0]._wrapper.isolate;
            }
        }

        static void registerPanel(UIWidgetsPanel panel) {
            panels.Add(panel);
        }

        static void unregisterPanel(UIWidgetsPanel panel) {
            panels.Remove(panel);
        }

        public float devicePixelRatioEditorOnlyOverride;
        
        public TextFont[] fonts;

        Configurations _configurations;

        UIWidgetsPanelWrapper _wrapper;

        protected UIWidgetsPanelWrapper wrapper {
            get { return _wrapper; }
        }

        int _currentWidth {
            get { return Mathf.RoundToInt(rectTransform.rect.width * canvas.scaleFactor); }
        }

        int _currentHeight {
            get { return Mathf.RoundToInt(rectTransform.rect.height * canvas.scaleFactor); }
        }

        protected float _currentDevicePixelRatio {
            get {
#if !UNITY_EDITOR
                return _wrapper.displayMetrics.DevicePixelRatioByDefault;
#endif
                if (devicePixelRatioEditorOnlyOverride > 0) {
                    return devicePixelRatioEditorOnlyOverride;
                }
                
                var currentDpi = Screen.dpi;
                if (currentDpi == 0) {
                    currentDpi = canvas.GetComponent<CanvasScaler>().fallbackScreenDPI;
                }

                return currentDpi / 96;
            }
        }

        bool _viewMetricsCallbackRegistered;

        void _handleViewMetricsChanged(string method, List<JSONNode> args) {
            using (Isolate.getScope(anyIsolate)) {
                if (_wrapper == null) {
                    return;
                }
                _wrapper.displayMetrics.onViewMetricsChanged();
                Window.instance.updateSafeArea();
                Window.instance.onMetricsChanged?.Invoke();
            }
        }

        protected virtual void Update() {
            if (!_viewMetricsCallbackRegistered) {
                _viewMetricsCallbackRegistered = true;
                UIWidgetsMessageManager.instance?.AddChannelMessageDelegate("ViewportMetricsChanged",
                    _handleViewMetricsChanged);
            }

#if !UNITY_EDITOR
            CollectGarbageOnDemand();
#endif

            Input_Update();
        }

        #region OnDemandGC
#if !UNITY_EDITOR
        // 8 MB
        const long kCollectAfterAllocating = 8 * 1024 * 1024;
        
        long lastFrameMemory = 0;
        long nextCollectAt = 0;

        void TryEnableOnDemandGC() 
        {
            if (UIWidgetsGlobalConfiguration.EnableIncrementalGC)
            {
                GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            }
        }

        void TryDisableOnDemandGC() 
        {
            if (UIWidgetsGlobalConfiguration.EnableIncrementalGC)
            {
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            }
        }

        void CollectGarbageOnDemand() 
        {
            if (!UIWidgetsGlobalConfiguration.EnableIncrementalGC)
            {
                return;
            }
            
            var mem = Profiler.GetMonoUsedSizeLong();
            if (mem < lastFrameMemory)
            {
                // GC happened.
                nextCollectAt = mem + kCollectAfterAllocating;
            }
            else if (mem >= nextCollectAt)
            {
                // Trigger incremental GC
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                GarbageCollector.CollectIncremental(1000);
                lastFrameMemory = mem + kCollectAfterAllocating;
                GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            }

            lastFrameMemory = mem;
        }
#endif
        #endregion

        //TODO: this API seems to be more potentially destructive than EnableUIWidgetsWrapperNextFrame, we should try to replace it with the latter
        IEnumerator ReEnableUIWidgetsNextFrame() {
            yield return null;
            enabled = true;
        }
        
        IEnumerator EnableUIWidgetsWrapperNextFrame() {
            yield return null;
            EnableUIWidgetsWrapper();
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        bool AndroidInitialized = true;

        bool IsAndroidInitialized() {
            if (AndroidInitialized) {
                enabled = false;
                AndroidInitialized = false;
                AndroidPlatformUtil.Init();
                startCoroutine(ReEnableUIWidgetsNextFrame());
                return false;
            }
            return true;
        }
#endif
#if UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        void InitializeOpenGL() {
            OpenGLCoreUtil.RenderTextureCreateFailureWorkaround();
            OpenGLCoreUtil.Init();
            startCoroutine(EnableUIWidgetsWrapperNextFrame());
        }
#endif
        
        static bool UIWidgetsDisabled;

        void DisableUIWidgets() {
            Debug.Log("Please change graphic api for UIWidgets.\n" +
                      "Direct3D11 for Windows\n" +
                      "Vulkan for Android\n");
            UIWidgetsDisabled = true;
            enabled = false;
        }

        protected override void OnEnable() {
            if (UIWidgetsDisabled) {
                enabled = false;
                return;
            }

            var type = SystemInfo.graphicsDeviceType;
#if !UNITY_EDITOR && UNITY_ANDROID
            if(type != GraphicsDeviceType.OpenGLES2 && type != GraphicsDeviceType.OpenGLES3){
                DisableUIWidgets();
                return;
            }
            if (!IsAndroidInitialized()) {return ;}
#endif
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (type == GraphicsDeviceType.OpenGLCore) {
                InitializeOpenGL();
                base.OnEnable();
                return;
            }
#endif
#if UNITY_IOS
            if (type == GraphicsDeviceType.OpenGLES2 || type == GraphicsDeviceType.OpenGLES3) {
                InitializeOpenGL();
                base.OnEnable();
                return;
            }
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (type != GraphicsDeviceType.Direct3D11) {
                DisableUIWidgets();
                return;
            }
#endif
            // If user duplicates UIWidgets panel in scene, canvas could be null during OnEnable, which results in error. Skip to avoid error.
            // More explanation: during duplication, editor wakes and enables behaviors in certain order. GameObject behaviors are enabled before canvas.
            if (canvas == null) {
                startCoroutine(EnableUIWidgetsWrapperNextFrame());
                base.OnEnable();
                return;
            }
#if !UNITY_EDITOR && UNITY_IOS
            //the hook API cannot be automatically called on IOS, so we need try hook it here
            Hooks.tryHook();
#endif

#if !UNITY_EDITOR
            TryEnableOnDemandGC();
            Application.lowMemory += () => {
                TryDisableOnDemandGC();
                GC.Collect();
                TryEnableOnDemandGC();
            };
#endif

            base.OnEnable();
            EnableUIWidgetsWrapper();
        }

        void EnableUIWidgetsWrapper() {
            D.assert(_wrapper == null);
            _configurations = new Configurations();
            _wrapper = new UIWidgetsPanelWrapper();
            onEnable();
            if (fonts != null && fonts.Length > 0) {
                foreach (var font in fonts) {
                    AddFont(family: font.family, font: font);
                }
            }
            _wrapper.Initiate(this, width: _currentWidth, height: _currentHeight, dpr: _currentDevicePixelRatio,
                _configurations: _configurations);
            _configurations.Clear();

            if (_wrapper.requireColorspaceShader) {
                material = Resources.Load<Material>("uiwidgets_runtime");
            }
            else {
                material = null;
            }
            texture = _wrapper.renderTexture;
            Input_OnEnable();
            registerPanel(this);
        }

        protected override void OnDisable() {
            if (_wrapper != null) {
                unregisterPanel(this);
                _wrapper.Destroy();
            }

            _wrapper = null;
            texture = null;
            Input_OnDisable();
#if !UNITY_EDITOR
            TryDisableOnDemandGC();
#endif
            base.OnDisable();
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

        public virtual void mainEntry() {
            main();
        }

        protected virtual void onEnable() {
        }

        protected void AddFont(string family, TextFont font) {
            _configurations.AddFont(family, font);
        }

        protected void AddFont(string family, List<string> assets, List<int> weights, string absPath = null) {
            if (assets.Count != weights.Count) {
                Debug.LogError($"The size of {family}‘s assets should be equal to the weights'.");
                return;
            }

            var textFont = new TextFont {family = family, absPath = absPath};
            var fonts = new Font[assets.Count];
            for (var j = 0; j < assets.Count; j++) {
                var font = new Font {asset = assets[index: j], weight = weights[index: j]};
                fonts[j] = font;
            }

            textFont.fonts = fonts;
            AddFont(family: family, font: textFont);
        }

        protected virtual void main() {
        }
    }

    enum UIWidgetsInputMode 
    {
        Mouse,
        Touch
    }

    public partial class UIWidgetsPanel : IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IDragHandler {
        bool _isEntered;
        Vector2 _lastMousePosition;

        UIWidgetsInputMode _inputMode;

        static UIWidgetsPanel _currentActivePanel = null;

        void _convertPointerData(PointerEventData evt, out Vector2? position, out int pointerId) {
            position = _inputMode == UIWidgetsInputMode.Mouse
                ? _getPointerPosition(Input.mousePosition)
                : _getPointerPosition(evt);
            pointerId = _inputMode == UIWidgetsInputMode.Mouse ? evt.pointerId : (-1 - evt.pointerId);
        }

        Vector2? _getPointerPosition(Vector2 position) {
            var worldCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect: rectTransform, screenPoint: position, cam: worldCamera, out var localPoint)) {
                var scaleFactor = canvas.scaleFactor;
                localPoint.x = (localPoint.x - rectTransform.rect.min.x) * scaleFactor;
                localPoint.y = (rectTransform.rect.max.y - localPoint.y) * scaleFactor;
                return localPoint;
            }

            return null;
        }

        Vector2? _getPointerPosition(PointerEventData eventData) {
            //refer to: https://zhuanlan.zhihu.com/p/37127981
            Camera eventCamera = null;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) {
                eventCamera = canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                eventCamera, out localPoint);
            var scaleFactor = canvas.scaleFactor;
            localPoint.x = (localPoint.x - rectTransform.rect.min.x) * scaleFactor;
            localPoint.y = (rectTransform.rect.max.y - localPoint.y) * scaleFactor;
            return localPoint;
        }

        void Input_OnEnable() {
            _inputMode = Input.mousePresent ? UIWidgetsInputMode.Mouse : UIWidgetsInputMode.Touch;
            Focus();
        }

        void Input_OnDisable() {
            UnFocus();
        }

        void Input_Update() {
            //we only process hover events for desktop applications
            if (_inputMode == UIWidgetsInputMode.Mouse) {
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

#if UNITY_ANDROID && !UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape)) {
                using (Isolate.getScope(anyIsolate)) {
                    WidgetsBinding.instance.handlePopRoute();
                }
            }
#endif
        }

        private bool isActivePanel => _currentActivePanel == this;

        public void Focus() {
            _currentActivePanel = this;
        }

        public void UnFocus() {
            if (_currentActivePanel == this) {
                _currentActivePanel = null;
            }
        }

        void Input_OnGUI() {
            if (!isActivePanel) {
                return;
            }
            
            var e = Event.current;
            if (e.isKey) {
                _wrapper.OnKeyDown(e: e);
            }
        }

        void _onMouseMove() {
            if (_inputMode != UIWidgetsInputMode.Mouse) {
                return;
            }
            var pos = _getPointerPosition(Input.mousePosition);
            _wrapper.OnMouseMove(pos);
        }

        void _onScroll() {
            if (_inputMode != UIWidgetsInputMode.Mouse) {
                return;
            }
            var pos = _getPointerPosition(Input.mousePosition);
            _wrapper.OnMouseScroll(Input.mouseScrollDelta, pos);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (_inputMode != UIWidgetsInputMode.Mouse) {
                return;
            }
            D.assert(eventData.pointerId < 0);
            _isEntered = true;
            _lastMousePosition = Input.mousePosition;
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (_inputMode != UIWidgetsInputMode.Mouse) {
                return;
            }
            D.assert(eventData.pointerId < 0);
            _isEntered = false;
            _wrapper.OnPointerLeave();
        }

        public void OnPointerDown(PointerEventData eventData) {
            Focus();
            _convertPointerData(eventData, out var pos, out var pointerId);
            _wrapper.OnPointerDown(pos, pointerId);
        }

        public void OnPointerUp(PointerEventData eventData) {
            _convertPointerData(eventData, out var pos, out var pointerId);
            _wrapper.OnPointerUp(pos, pointerId);
        }

        public void OnDrag(PointerEventData eventData) {
            Focus();
            _convertPointerData(eventData, out var pos, out var pointerId);
            _wrapper.OnDrag(pos, pointerId);
        }
    }
}