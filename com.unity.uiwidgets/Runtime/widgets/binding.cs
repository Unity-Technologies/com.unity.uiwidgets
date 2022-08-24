using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public interface WidgetsBindingObserver {
        void didChangeMetrics();

        void didChangeTextScaleFactor();

        void didChangePlatformBrightness();

        void didChangeLocales(List<Locale> locale);

        Future<bool> didPopRoute();

        Future<bool> didPushRoute(string route);

        void didChangeAccessibilityFeatures();
    }

    public static partial class ui_ {
        public static void Each<T>(this IEnumerable<T> source, Action<T> fn) {
            foreach (var item in source) {
                fn.Invoke(item);
            }
        }

        public static void Each<T>(this IEnumerable<T> source, Action<T, int> fn) {
            int index = 0;

            foreach (T item in source) {
                fn.Invoke(item, index);
                index++;
            }
        }

        /// <summary>
        /// Convert a variable length argument list of items to an enumerable.
        /// </summary>
        public static IEnumerable<T> FromItems<T>(params T[] items) {
            foreach (var item in items) {
                yield return item;
            }
        }

        public static void runApp(Widget app) {
            var instance = UiWidgetsBinding.ensureInitialized();
            instance.scheduleAttachRootWidget(app);
            instance.scheduleWarmUpFrame();
        }
    }

    public class WidgetsBinding : RendererBinding {
        public new static WidgetsBinding instance {
            get { return (WidgetsBinding) RendererBinding.instance; }
            set { RendererBinding.instance = value; }
        }

        protected override void initInstances() {
            base.initInstances();
            instance = this;

            D.assert(() => {
                // _debugAddStackFilters();
                return true;
            });

            _buildOwner = new BuildOwner();
            buildOwner.onBuildScheduled = _handleBuildScheduled;
            window.onLocaleChanged += handleLocaleChanged;
            widgetInspectorService = new WidgetInspectorService();

            // window.onAccessibilityFeaturesChanged = handleAccessibilityFeaturesChanged;
            // SystemChannels.navigation.setMethodCallHandler(_handleNavigationInvocation);
            // FlutterErrorDetails.propertiesTransformers.add(transformDebugCreator);
        }

        public BuildOwner buildOwner {
            get { return _buildOwner; }
        }

        BuildOwner _buildOwner;

        public FocusManager focusManager {
            get { return _buildOwner.focusManager; }
        }

        readonly List<WidgetsBindingObserver> _observers = new List<WidgetsBindingObserver>();

        public void addObserver(WidgetsBindingObserver observer) {
            _observers.Add(observer);
        }

        public bool removeObserver(WidgetsBindingObserver observer) {
            return _observers.Remove(observer);
        }

        public void handlePopRoute() {
            var idx = -1;

            void _handlePopRouteSub(bool result) {
                if (!result) {
                    idx++;
                    if (idx >= _observers.Count) {
                        Application.Quit();
                        return;
                    }

                    _observers[idx].didPopRoute().then_(_handlePopRouteSub);
                }
            }

            _handlePopRouteSub(false);
        }

        public WidgetInspectorService widgetInspectorService;

        protected override void handleMetricsChanged() {
            base.handleMetricsChanged();
            foreach (WidgetsBindingObserver observer in _observers) {
                observer.didChangeMetrics();
            }
        }

        protected override void handleTextScaleFactorChanged() {
            base.handleTextScaleFactorChanged();
            foreach (WidgetsBindingObserver observer in _observers) {
                observer.didChangeTextScaleFactor();
            }
        }

        protected override void handlePlatformBrightnessChanged() {
            base.handlePlatformBrightnessChanged();
            foreach (WidgetsBindingObserver observer in _observers) {
                observer.didChangePlatformBrightness();
            }
        }

        protected virtual void handleLocaleChanged() {
            dispatchLocalesChanged(Window.instance.locales);
        }

        protected virtual void dispatchLocalesChanged(List<Locale> locales) {
            foreach (WidgetsBindingObserver observer in _observers) {
                observer.didChangeLocales(locales);
            }
        }

        void _handleBuildScheduled() {
            D.assert(() => {
                if (debugBuildingDirtyElements) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary("Build scheduled during frame."),
                        new ErrorDescription(
                            "While the widget tree was being built, laid out, and painted, " +
                            "a new frame was scheduled to rebuild the widget tree."
                        ),
                        new ErrorHint(
                            "This might be because setState() was called from a layout or " +
                            "paint callback. " +
                            "If a change is needed to the widget tree, it should be applied " +
                            "as the tree is being built. Scheduling a change for the subsequent " +
                            "frame instead results in an interface that lags behind by one frame. " +
                            "If this was done to make your build dependent on a size measured at " +
                            "layout time, consider using a LayoutBuilder, CustomSingleChildLayout, " +
                            "or CustomMultiChildLayout. If, on the other hand, the one frame delay " +
                            "is the desired effect, for example because this is an " +
                            "animation, consider scheduling the frame in a post-frame callback " +
                            "using SchedulerBinding.addPostFrameCallback or " +
                            "using an AnimationController to trigger the animation."
                        )
                    });
                }

                return true;
            });

            ensureVisualUpdate();
        }

        protected bool debugBuildingDirtyElements = false;

        protected override void drawFrame() {
            D.assert(!debugBuildingDirtyElements);
            D.assert(() => {
                debugBuildingDirtyElements = true;
                return true;
            });
            try {
                if (renderViewElement != null) {
                    buildOwner.buildScope(renderViewElement);
                }

                base.drawFrame();
                buildOwner.finalizeTree();
            }
            finally {
                D.assert(() => {
                    debugBuildingDirtyElements = false;
                    return true;
                });
            }
        }
        
        public RenderObjectToWidgetElement<RenderBox> renderViewElement {
            get { return _renderViewElement; }
        }

        RenderObjectToWidgetElement<RenderBox> _renderViewElement;

        public void detachRootWidget() {
            if (_renderViewElement == null) {
                return;
            }

            //The former widget tree must be layout first before its destruction
            drawFrame();
            attachRootWidget(null);
            buildOwner.buildScope(_renderViewElement);
            buildOwner.finalizeTree();

            pipelineOwner.rootNode = null;
            _renderViewElement.deactivate();
            _renderViewElement.unmount();
            _renderViewElement = null;
        }

        public void scheduleAttachRootWidget(Widget rootWidget) {
            Timer.run(() => {
                attachRootWidget(rootWidget);
                return null;
            });
        }

        public void attachRootWidget(Widget rootWidget) {
            _renderViewElement = new RenderObjectToWidgetAdapter<RenderBox>(
                container: renderView,
                debugShortDescription: "[root]",
                child: rootWidget
            ).attachToRenderTree(buildOwner, _renderViewElement);
        }
        
        bool isRootWidgetAttached {
            get { return _renderViewElement != null; }
        }
    }

    public class RenderObjectToWidgetAdapter<T> : RenderObjectWidget where T : RenderObject {
        public RenderObjectToWidgetAdapter(
            Widget child = null,
            RenderObjectWithChildMixin<T> container = null,
            string debugShortDescription = null
        ) : base(
            new GlobalObjectKey<State>(container)) {
            this.child = child;
            this.container = container;
            this.debugShortDescription = debugShortDescription;
        }

        public readonly Widget child;

        public readonly RenderObjectWithChildMixin<T> container;

        public readonly string debugShortDescription;

        public override Element createElement() {
            return new RenderObjectToWidgetElement<T>(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return (RenderObject) container;
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
        }

        public RenderObjectToWidgetElement<T> attachToRenderTree(BuildOwner owner,
            RenderObjectToWidgetElement<T> element) {
            if (element == null) {
                owner.lockState(() => {
                    element = (RenderObjectToWidgetElement<T>) createElement();
                    D.assert(element != null);
                    element.assignOwner(owner);
                });
                owner.buildScope(element, () => { element.mount(null, null); });
                SchedulerBinding.instance.ensureVisualUpdate();
            }
            else {
                element._newWidget = this;
                element.markNeedsBuild();
            }

            return element;
        }

        public override string toStringShort() {
            return debugShortDescription ?? base.toStringShort();
        }
    }

    public class RenderObjectToWidgetElement<T> : RootRenderObjectElement where T : RenderObject {
        public RenderObjectToWidgetElement(RenderObjectToWidgetAdapter<T> widget) : base(widget) {
        }

        public new RenderObjectToWidgetAdapter<T> widget {
            get { return (RenderObjectToWidgetAdapter<T>) base.widget; }
        }

        Element _child;

        static readonly object _rootChildSlot = new object();

        public override void visitChildren(ElementVisitor visitor) {
            if (_child != null) {
                visitor(_child);
            }
        }

        public override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
            base.forgetChild(child);
        }

        public override void mount(Element parent, object newSlot) {
            D.assert(parent == null);
            base.mount(parent, newSlot);
            _rebuild();
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            _rebuild();
        }

        internal Widget _newWidget;

        protected override void performRebuild() {
            if (_newWidget != null) {
                Widget newWidget = _newWidget;
                _newWidget = null;
                update((RenderObjectToWidgetAdapter<T>)newWidget);
            }

            base.performRebuild();
            D.assert(_newWidget == null);
        }

        void _rebuild() {
            try {
                _child = updateChild(_child, widget.child,
                    _rootChildSlot);
                // allow 
            }
            catch (Exception ex) {
                var details = new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "widgets library",
                    context: new ErrorDescription("attaching to the render tree")
                );
                UIWidgetsError.reportError(details);

                Widget error = ErrorWidget.builder(details);
                _child = updateChild(null, error, _rootChildSlot);
            }
        }

        public new RenderObjectWithChildMixin<T> renderObject {
            get { return (RenderObjectWithChildMixin<T>) base.renderObject; }
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert(slot == _rootChildSlot);
            D.assert(renderObject.debugValidateChild(child));
            renderObject.child = (T) child;
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(renderObject.child == child);
            renderObject.child = null;
        }
    }

    public class UiWidgetsBinding : WidgetsBinding {
        // todo 
        public static WidgetsBinding ensureInitialized() {
            if (WidgetsBinding.instance == null) {
                return new UiWidgetsBinding();
            }
            
            return WidgetsBinding.instance;
        }
    }
}