using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace Unity.UIWidgets.editor {
#if UNITY_EDITOR
    public abstract class UIWidgetsEditorWindow : EditorWindow, WindowHost {
        WindowAdapter _windowAdapter;
        
        static readonly List<UIWidgetsEditorWindow> _activeEditorWindows = new List<UIWidgetsEditorWindow>();

        [InitializeOnLoadMethod]
        static void _OnBaseEditorWindowLoaded()
        {
            EditorApplication.quitting += () =>
            {
                foreach (var editorWindow in _activeEditorWindows) {
                    editorWindow.OnDisable();
                }
                
                _activeEditorWindows.Clear();
            };
        }
        
        public UIWidgetsEditorWindow() {
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            
            _activeEditorWindows.Add(this);
        }
        
        void OnDestroy() {
            if (_activeEditorWindows.Contains(this)) {
                _activeEditorWindows.Remove(this);
            }
        }

        protected virtual void OnEnable() {
            if (_windowAdapter == null) {
                _windowAdapter = new EditorWindowAdapter(this);
            }

            _windowAdapter.OnEnable();

            RenderBox rootRenderBox;
            using (_windowAdapter.getScope()) {
                rootRenderBox = createRenderBox();
            }

            if (rootRenderBox != null) {
                _windowAdapter.attachRootRenderBox(rootRenderBox);
                return;
            }

            Widget rootWidget;
            using (_windowAdapter.getScope()) {
                rootWidget = createWidget();
            }

            _windowAdapter.attachRootWidget(rootWidget);
        }

        protected virtual void OnDisable() {
            _windowAdapter.OnDisable();
        }

        protected virtual void OnGUI() {
            _windowAdapter.OnGUI(Event.current);
        }

        protected virtual void Update() {
            _windowAdapter.Update();
        }

        protected virtual RenderBox createRenderBox() {
            return null;
        }

        protected abstract Widget createWidget();

        public Window window {
            get { return _windowAdapter; }
        }
    }

    public class EditorWindowAdapter : WindowAdapter {
        public readonly EditorWindow editorWindow;

        public EditorWindowAdapter(EditorWindow editorWindow) : base(true) {
            this.editorWindow = editorWindow;
        }

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            base.scheduleFrame(regenerateLayerTree);
            editorWindow.Repaint();
        }

        protected override bool hasFocus() {
            return EditorWindow.focusedWindow == editorWindow;
        }

        public override GUIContent titleContent {
            get { return editorWindow.titleContent; }
        }

        protected override float queryDevicePixelRatio() {
            return EditorGUIUtility.pixelsPerPoint;
        }
        
        protected override int queryAntiAliasing() {
            return defaultAntiAliasing;
        }

        protected override Vector2 queryWindowSize() {
            return editorWindow.position.size;
        }

        protected override TimeSpan getTime() {
            return TimeSpan.FromSeconds(EditorApplication.timeSinceStartup);
        }

        float? _lastUpdateTime;

        protected override void updateDeltaTime() {
            if (_lastUpdateTime == null) {
                _lastUpdateTime = (float) EditorApplication.timeSinceStartup;
            }

            deltaTime = (float) EditorApplication.timeSinceStartup - _lastUpdateTime.Value;
            unscaledDeltaTime = deltaTime;
            _lastUpdateTime = (float) EditorApplication.timeSinceStartup;
        }
    }

#endif

    public interface WindowHost {
        Window window { get; }
    }

    public abstract class WindowAdapter : Window {
        static readonly List<WindowAdapter> _windowAdapters = new List<WindowAdapter>();

        public WindowAdapter(bool inEditorWindow = false) {
            this.inEditorWindow = inEditorWindow;
        }

        public static List<WindowAdapter> windowAdapters {
            get { return _windowAdapters; }
        }

        public WidgetInspectorService widgetInspectorService {
            get {
                D.assert(_binding != null);
                return _binding.widgetInspectorService;
            }
        }

        internal WidgetsBinding _binding;
        float _lastWindowWidth;
        float _lastWindowHeight;

        bool _viewMetricsChanged;

        readonly MicrotaskQueue _microtaskQueue = new MicrotaskQueue();
        readonly TimerProvider _timerProvider = new TimerProvider();
        readonly Rasterizer _rasterizer = new Rasterizer();
        readonly ScrollInput _scrollInput = new ScrollInput();

        bool _regenerateLayerTree;
        Surface _surface;

        bool _alive;

        public bool alive {
            get { return _alive; }
        }

        protected virtual TimeSpan getTime() {
            return TimeSpan.FromSeconds(Time.time);
        }

        protected float deltaTime;
        protected float unscaledDeltaTime;

        void updatePhysicalSize() {
            var size = queryWindowSize();
            _physicalSize = new Size(
                size.x * _devicePixelRatio,
                size.y * _devicePixelRatio);
        }


        protected virtual void updateDeltaTime() {
            deltaTime = Time.unscaledDeltaTime;
            unscaledDeltaTime = Time.deltaTime;
        }

        protected virtual void updateSafeArea() {
        }

        public void onViewMetricsChanged() {
            _viewMetricsChanged = true;
        }

        protected abstract bool hasFocus();

        public void OnEnable() {
            _devicePixelRatio = queryDevicePixelRatio();
            _antiAliasing = queryAntiAliasing();
            updatePhysicalSize();
            updateSafeArea();
            D.assert(_surface == null);
            _surface = createSurface();

            _rasterizer.setup(_surface);
            _windowAdapters.Add(this);
            _alive = true;
        }

        public void OnDisable() {
            using (getScope()) {
                _binding.detachRootWidget();
            }

            _windowAdapters.Remove(this);
            _alive = false;

            _rasterizer.teardown();

            D.assert(_surface != null);
            _surface.Dispose();
            _surface = null;
        }

        readonly protected bool inEditorWindow;

        public override IDisposable getScope() {
            WindowAdapter oldInstance = (WindowAdapter) _instance;
            _instance = this;

            if (_binding == null) {
                _binding = new WidgetsBinding(inEditorWindow);
            }

            SchedulerBinding._instance = _binding;

            return new WindowDisposable(this, oldInstance);
        }

        class WindowDisposable : IDisposable {
            readonly WindowAdapter _window;
            readonly WindowAdapter _oldWindow;

            public WindowDisposable(WindowAdapter window, WindowAdapter oldWindow) {
                _window = window;
                _oldWindow = oldWindow;
            }

            public void Dispose() {
                D.assert(_instance == _window);
                _instance = _oldWindow;

                D.assert(SchedulerBinding._instance == _window._binding);
                SchedulerBinding._instance = _oldWindow?._binding;
            }
        }

        public void postPointerEvents(List<PointerData> data) {
            withBinding(() => { onPointerEvent(new PointerDataPacket(data)); });
        }

        public void postPointerEvent(PointerData data) {
            postPointerEvents(new List<PointerData>() {data});
        }

        public void withBinding(Action fn) {
            using (getScope()) {
                fn();
            }
        }

        public T withBindingFunc<T>(Func<T> fn) {
            using (getScope()) {
                return fn();
            }
        }

        protected bool displayMetricsChanged() {
            if (_devicePixelRatio != queryDevicePixelRatio()) {
                return true;
            }
            
            if (_antiAliasing != queryAntiAliasing()) {
                return true;
            }

            var size = queryWindowSize();
            if (_lastWindowWidth != size.x
                || _lastWindowHeight != size.y) {
                return true;
            }

            if (_viewMetricsChanged) {
                return true;
            }

            return false;
        }

        public virtual void OnGUI(Event evt = null) {
            evt = evt ?? Event.current;
            using (getScope()) {
                if (displayMetricsChanged()) {
                    _devicePixelRatio = queryDevicePixelRatio();
                    _antiAliasing = queryAntiAliasing();

                    var size = queryWindowSize();
                    _lastWindowWidth = size.x;
                    _lastWindowHeight = size.y;
                    _physicalSize = new Size(
                        _lastWindowWidth * _devicePixelRatio,
                        _lastWindowHeight * _devicePixelRatio);

                    updateSafeArea();
                    _viewMetricsChanged = false;
                    if (onMetricsChanged != null) {
                        onMetricsChanged();
                    }
                }

                _doOnGUI(evt);
            }
        }

        public virtual GUIContent titleContent {
            get { return null; }
        }

        protected abstract float queryDevicePixelRatio();
        protected abstract int queryAntiAliasing();
        protected abstract Vector2 queryWindowSize();

        protected virtual Surface createSurface() {
            return new WindowSurfaceImpl();
        }

        void _beginFrame() {
            if (onBeginFrame != null) {
                onBeginFrame(getTime());
            }

            flushMicrotasks();

            if (onDrawFrame != null) {
                onDrawFrame();
            }
        }

        void _doOnGUI(Event evt) {
            if (evt.type == EventType.Repaint) {
                if (_regenerateLayerTree) {
                    _regenerateLayerTree = false;
                    _beginFrame();
                }
                else {
                    _rasterizer.drawLastLayerTree();
                }

                return;
            }

            if (onPointerEvent != null) {
                PointerData pointerData = null;

                if (evt.type == EventType.MouseDown) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.down,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * _devicePixelRatio,
                        physicalY: evt.mousePosition.y * _devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseUp || evt.rawType == EventType.MouseUp) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.up,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * _devicePixelRatio,
                        physicalY: evt.mousePosition.y * _devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseDrag) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.move,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * _devicePixelRatio,
                        physicalY: evt.mousePosition.y * _devicePixelRatio
                    );
                }
                else if (evt.type == EventType.MouseMove) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.hover,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * _devicePixelRatio,
                        physicalY: evt.mousePosition.y * _devicePixelRatio
                    );
                }
                else if (evt.type == EventType.ScrollWheel) {
                    onScroll(-evt.delta.x * _devicePixelRatio,
                        -evt.delta.y * _devicePixelRatio,
                        evt.mousePosition.x * _devicePixelRatio,
                        evt.mousePosition.y * _devicePixelRatio,
                        InputUtils.getScrollButtonKey()
                    );
                }
                else if (evt.type == EventType.DragUpdated) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.dragFromEditorMove,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * _devicePixelRatio,
                        physicalY: evt.mousePosition.y * _devicePixelRatio
                    );
                }
                else if (evt.type == EventType.DragPerform) {
                    pointerData = new PointerData(
                        timeStamp: Timer.timespanSinceStartup,
                        change: PointerChange.dragFromEditorRelease,
                        kind: PointerDeviceKind.mouse,
                        device: InputUtils.getMouseButtonKey(evt.button),
                        physicalX: evt.mousePosition.x * _devicePixelRatio,
                        physicalY: evt.mousePosition.y * _devicePixelRatio
                    );
                }

                if (pointerData != null) {
                    onPointerEvent(new PointerDataPacket(new List<PointerData> {
                        pointerData
                    }));
                }
            }

            RawKeyboard.instance._handleKeyEvent(Event.current);
            TextInput.OnGUI();
        }

        public void onScroll(float deltaX, float deltaY, float posX, float posY, int buttonId) {
            _scrollInput.onScroll(deltaX,
                deltaY,
                posX,
                posY,
                buttonId
            );
        }

        void _updateScrollInput(float deltaTime) {
            var deltaScroll = _scrollInput.getScrollDelta(deltaTime);

            if (deltaScroll == Vector2.zero) {
                return;
            }

            PointerData pointerData = new ScrollData(
                timeStamp: Timer.timespanSinceStartup,
                change: PointerChange.scroll,
                kind: PointerDeviceKind.mouse,
                device: _scrollInput.getDeviceId(),
                physicalX: _scrollInput.getPointerPosX(),
                physicalY: _scrollInput.getPointerPosY(),
                scrollX: deltaScroll.x,
                scrollY: deltaScroll.y
            );

            onPointerEvent(new PointerDataPacket(new List<PointerData> {
                pointerData
            }));
        }

        public void Update() {
            if (_physicalSize == null || _physicalSize.isEmpty) {
                updatePhysicalSize();
            }

            updateDeltaTime();
            updateFPS(unscaledDeltaTime);

            Timer.update();

            bool hasFocus = this.hasFocus();
            using (getScope()) {
                WidgetsBinding.instance.focusManager.focusNone(!hasFocus);
                _updateScrollInput(deltaTime);
                TextInput.Update();
                _timerProvider.update(flushMicrotasks);
                flushMicrotasks();
            }
        }

        static readonly TimeSpan _coolDownDelay = new TimeSpan(0, 0, 0, 0, 200);
        static Timer frameCoolDownTimer;

        public override void scheduleFrame(bool regenerateLayerTree = true) {
            if (regenerateLayerTree) {
                _regenerateLayerTree = true;
            }

            onFrameRateSpeedUp();
            frameCoolDownTimer?.cancel();
            frameCoolDownTimer = instance.run(
                _coolDownDelay,
                () => {
                    onFrameRateCoolDown();
                    frameCoolDownTimer = null;
                });
        }

        public override void render(Scene scene) {
            var layerTree = scene.takeLayerTree();
            if (layerTree == null) {
                return;
            }

            if (_physicalSize.isEmpty) {
                return;
            }

            layerTree.frameSize = _physicalSize;
            layerTree.devicePixelRatio = _devicePixelRatio;
            layerTree.antiAliasing = _antiAliasing;
            _rasterizer.draw(layerTree);
        }

        public override void scheduleMicrotask(Action callback) {
            _microtaskQueue.scheduleMicrotask(callback);
        }

        public override void flushMicrotasks() {
            _microtaskQueue.flushMicrotasks();
        }

        public override Timer run(TimeSpan duration, Action callback, bool periodic = false) {
            return periodic
                ? _timerProvider.periodic(duration, callback)
                : _timerProvider.run(duration, callback);
        }

        public override Timer runInMain(Action callback) {
            return _timerProvider.runInMain(callback);
        }

        public void attachRootRenderBox(RenderBox root) {
            using (getScope()) {
                _binding.renderView.child = root;
            }
        }

        public void attachRootWidget(Widget root) {
            using (getScope()) {
                _binding.attachRootWidget(root);
            }
        }

        public void attachRootWidget(Func<Widget> root) {
            using (getScope()) {
                _binding.attachRootWidget(root());
            }
        }

        internal void _forceRepaint() {
            using (getScope()) {
                RenderObjectVisitor visitor = null;
                visitor = (child) => {
                    child.markNeedsPaint();
                    child.visitChildren(visitor);
                };
                _binding.renderView?.visitChildren(visitor);
            }
        }
    }
}