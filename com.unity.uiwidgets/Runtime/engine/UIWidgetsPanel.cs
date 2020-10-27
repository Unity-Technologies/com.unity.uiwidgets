/*using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RawImage = UnityEngine.UI.RawImage;
using Rect = UnityEngine.Rect;
using Texture = UnityEngine.Texture;

namespace Unity.UIWidgets.engine {
    public class UIWidgetWindowAdapter : WindowAdapter {
        readonly UIWidgetsPanel _uiWidgetsPanel;
        bool _needsPaint;


        protected override void updateSafeArea() {
            _padding = _uiWidgetsPanel.viewPadding;
            _viewInsets = _uiWidgetsPanel.viewInsets;
        }

        protected override bool hasFocus() {
            return EventSystem.current != null &&
                   EventSystem.current.currentSelectedGameObject == _uiWidgetsPanel.gameObject;
        }

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            base.scheduleFrame(regenerateLayerTree);
            _needsPaint = true;
        }

        public UIWidgetWindowAdapter(UIWidgetsPanel uiWidgetsPanel) {
            _uiWidgetsPanel = uiWidgetsPanel;
        }


        public override void OnGUI(Event evt) {
            if (displayMetricsChanged()) {
                _needsPaint = true;
            }

            if (evt.type == EventType.Repaint) {
                if (!_needsPaint) {
                    return;
                }

                _needsPaint = false;
            }

            base.OnGUI(evt);
        }

        protected override Surface createSurface() {
            return new WindowSurfaceImpl(_uiWidgetsPanel.applyRenderTexture);
        }

        public override GUIContent titleContent {
            get { return new GUIContent(_uiWidgetsPanel.gameObject.name); }
        }

        protected override float queryDevicePixelRatio() {
            return _uiWidgetsPanel.devicePixelRatio;
        }
        
        protected override int queryAntiAliasing() {
            return _uiWidgetsPanel.antiAliasing;
        }

        protected override Vector2 queryWindowSize() {
            var rect = _uiWidgetsPanel.rectTransform.rect;
            // Here we use ReferenceEquals instead of "==" due to
            // https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // In short, "==" is overloaded for UnityEngine.Object and will bring performance issues
            if (!ReferenceEquals(_uiWidgetsPanel.canvas, null)) {
                var size = new Vector2(rect.width, rect.height) *
                           _uiWidgetsPanel.canvas.scaleFactor / _uiWidgetsPanel.devicePixelRatio;
                size.x = Mathf.Round(size.x);
                size.y = Mathf.Round(size.y);
                return size;
            }

            return new Vector2(0, 0);
        }

        public Offset windowPosToScreenPos(Offset windowPos) {
            Camera camera = null;
            var canvas = _uiWidgetsPanel.canvas;
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera) {
                camera = canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }

            var pos = new Vector2(windowPos.dx, windowPos.dy);
            pos = pos * queryDevicePixelRatio() / _uiWidgetsPanel.canvas.scaleFactor;
            var rectTransform = _uiWidgetsPanel.rectTransform;
            var rect = rectTransform.rect;
            pos.x += rect.min.x;
            pos.y = rect.max.y - pos.y;
            var worldPos = rectTransform.TransformPoint(new Vector2(pos.x, pos.y));
            var screenPos = RectTransformUtility.WorldToScreenPoint(camera, worldPos);
            return new Offset(screenPos.x, Screen.height - screenPos.y);
        }
    }

    [RequireComponent(typeof(RectTransform))]
    public class UIWidgetsPanel : RawImage, IPointerDownHandler, IPointerUpHandler, IDragHandler,
        IPointerEnterHandler, IPointerExitHandler, WindowHost {
        static Event _repaintEvent;

        [Tooltip("set to zero if you want to use the default device pixel ratio of the target platforms; otherwise the " +
                 "device pixel ratio will be forced to the given value on all devices.")]
        [SerializeField] protected float devicePixelRatioOverride;
        
        [Tooltip("set to true will enable the hardware anti-alias feature, which will improve the appearance of the UI greatly but " +
                 "making it much slower. Enable it only when seriously required.")]
        [SerializeField] protected bool hardwareAntiAliasing = false;
        WindowAdapter _windowAdapter;
        Texture _texture;
        Vector2 _lastMouseMove;

        readonly HashSet<int> _enteredPointers = new HashSet<int>();

        bool _viewMetricsCallbackRegistered;

        bool _mouseEntered {
            get { return !_enteredPointers.isEmpty(); }
        }

        DisplayMetrics _displayMetrics;

        const int mouseButtonNum = 3;

        void _handleViewMetricsChanged(string method, List<JSONNode> args) {
            _windowAdapter?.onViewMetricsChanged();
            _displayMetrics?.onViewMetricsChanged();
        }

        protected virtual void InitWindowAdapter() {
            D.assert(_windowAdapter == null);
            _windowAdapter = new UIWidgetWindowAdapter(this);

            _windowAdapter.OnEnable();
        }

        protected override void OnEnable() {
            base.OnEnable();

            //Disable the default touch -> mouse event conversion on mobile devices
            Input.simulateMouseWithTouches = false;

            _displayMetrics = DisplayMetricsProvider.provider();
            _displayMetrics.OnEnable();

            _enteredPointers.Clear();

            if (_repaintEvent == null) {
                _repaintEvent = new Event {type = EventType.Repaint};
            }

            InitWindowAdapter();

            Widget root;
            using (_windowAdapter.getScope()) {
                root = createWidget();
            }

            _windowAdapter.attachRootWidget(root);
            _lastMouseMove = Input.mousePosition;
        }

        public float devicePixelRatio {
            get {
                return devicePixelRatioOverride > 0
                    ? devicePixelRatioOverride
                    : _displayMetrics.devicePixelRatio;
            }
        }
        
        public int antiAliasing {
            get { return hardwareAntiAliasing ? Window.defaultAntiAliasing : 0; }
        }

        public WindowPadding viewPadding {
            get { return _displayMetrics.viewPadding; }
        }

        public WindowPadding viewInsets {
            get { return _displayMetrics.viewInsets; }
        }

        protected override void OnDisable() {
            D.assert(_windowAdapter != null);
            _windowAdapter.OnDisable();
            _windowAdapter = null;
            base.OnDisable();
        }

        protected virtual Widget createWidget() {
            return null;
        }

        public void recreateWidget() {
            Widget root;
            using (_windowAdapter.getScope()) {
                root = createWidget();
            }

            _windowAdapter.attachRootWidget(root);
        }

        internal void applyRenderTexture(Rect screenRect, Texture texture, Material mat) {
            this.texture = texture;
            material = mat;
        }

        protected virtual void Update() {
            _displayMetrics.Update();
            UIWidgetsMessageManager.ensureUIWidgetsMessageManagerIfNeeded();

#if UNITY_ANDROID
            if (Input.GetKeyDown(KeyCode.Escape)) {
                this._windowAdapter.withBinding(() => { WidgetsBinding.instance.handlePopRoute(); });
            }
#endif

            if (!_viewMetricsCallbackRegistered) {
                _viewMetricsCallbackRegistered = true;
                UIWidgetsMessageManager.instance?.AddChannelMessageDelegate("ViewportMatricsChanged",
                    _handleViewMetricsChanged);
            }

            if (_mouseEntered) {
                if (_lastMouseMove.x != Input.mousePosition.x || _lastMouseMove.y != Input.mousePosition.y) {
                    handleMouseMovement();
                }
            }

            _lastMouseMove = Input.mousePosition;

            if (_mouseEntered) {
                handleMouseScroll();
            }

            D.assert(_windowAdapter != null);
            _windowAdapter.Update();
            _windowAdapter.OnGUI(_repaintEvent);
        }

        void OnGUI() {
            _displayMetrics.OnGUI();
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) {
                _windowAdapter.OnGUI(Event.current);
            }
        }

        void handleMouseMovement() {
            var pos = getPointPosition(Input.mousePosition);
            if (pos == null) {
                return;
            }
            _windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: PointerDeviceKind.mouse,
                device: getMouseButtonDown(),
                physicalX: pos.Value.x,
                physicalY: pos.Value.y
            ));
        }

        void handleMouseScroll() {
            if (Input.mouseScrollDelta.y != 0 || Input.mouseScrollDelta.x != 0) {
                var scaleFactor = canvas.scaleFactor;
                var pos = getPointPosition(Input.mousePosition);
                if (pos == null) {
                    return;
                }
                _windowAdapter.onScroll(Input.mouseScrollDelta.x * scaleFactor,
                    Input.mouseScrollDelta.y * scaleFactor,
                    pos.Value.x,
                    pos.Value.y,
                    InputUtils.getScrollButtonKey());
            }
        }

        int getMouseButtonDown() {
            //default mouse button key = left mouse button
            var defaultKey = 0;
            for (int key = 0; key < mouseButtonNum; key++) {
                if (Input.GetMouseButton(key)) {
                    defaultKey = key;
                    break;
                }
            }

            return InputUtils.getMouseButtonKey(defaultKey);
        }

        public void OnPointerDown(PointerEventData eventData) {
            var position = getPointPosition(eventData);
            if (position == null) {
                return;
            }
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
            _windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.down,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: InputUtils.getPointerDeviceKey(eventData),
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        public void OnPointerUp(PointerEventData eventData) {
            var position = getPointPosition(eventData);
            if (position == null) {
                return;
            }
            _windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.up,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: InputUtils.getPointerDeviceKey(eventData),
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        Camera getActiveCamera() {
            //refer to: https://zhuanlan.zhihu.com/p/37127981
            Camera eventCamera = null;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) {
                eventCamera = canvas.GetComponent<GraphicRaycaster>().eventCamera;
            }
            return eventCamera;
        }

        Vector2? getPointPosition(PointerEventData eventData) {
            Camera camera = getActiveCamera();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                camera, out localPoint);
            var scaleFactor = canvas.scaleFactor;
            localPoint.x = (localPoint.x - rectTransform.rect.min.x) * scaleFactor;
            localPoint.y = (rectTransform.rect.max.y - localPoint.y) * scaleFactor;
            return localPoint;
        }

        Vector2? getPointPosition(Vector2 position) {
            Vector2 localPoint;
            Camera eventCamera = getActiveCamera();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position,
                eventCamera, out localPoint);
            var scaleFactor = canvas.scaleFactor;
            localPoint.x = (localPoint.x - rectTransform.rect.min.x) * scaleFactor;
            localPoint.y = (rectTransform.rect.max.y - localPoint.y) * scaleFactor;
            return localPoint;
        }

        public void OnDrag(PointerEventData eventData) {
            var position = getPointPosition(eventData);
            if (position == null) {
                return;
            }
            _windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.move,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: InputUtils.getPointerDeviceKey(eventData),
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        public void OnPointerEnter(PointerEventData eventData) {
            var position = getPointPosition(eventData);
            if (position == null) {
                return;
            }
            var pointerKey = InputUtils.getPointerDeviceKey(eventData);
            _enteredPointers.Add(pointerKey);

            _lastMouseMove = eventData.position;
            _windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: pointerKey,
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        public void OnPointerExit(PointerEventData eventData) {
            var position = getPointPosition(eventData);
            if (position == null) {
                return;
            }
            var pointerKey = InputUtils.getPointerDeviceKey(eventData);
            _enteredPointers.Remove(pointerKey);
            _windowAdapter.postPointerEvent(new PointerData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.hover,
                kind: InputUtils.getPointerDeviceKind(eventData),
                device: pointerKey,
                physicalX: position.Value.x,
                physicalY: position.Value.y
            ));
        }

        public Window window {
            get { return _windowAdapter; }
        }
    }
}*/