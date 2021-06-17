using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class ViewConfiguration {
        public ViewConfiguration(
            Size size = null,
            float devicePixelRatio = 1.0f
        ) {
            this.size = size ?? Size.zero;
            this.devicePixelRatio = devicePixelRatio;
        }

        public readonly Size size;

        public readonly float devicePixelRatio;

        public Matrix4 toMatrix() {
            return Matrix4.diagonal3Values(devicePixelRatio, devicePixelRatio, 1);
        }

        public override string ToString() {
            return $"{size} at {devicePixelRatio}x";
        }
    }

    public class RenderView : RenderObjectWithChildMixinRenderObject<RenderBox> {
        public RenderView(
            RenderBox child = null,
            ViewConfiguration configuration = null,
            ui.Window window = null) {
            D.assert(configuration != null);

            this.child = child;
            _configuration = configuration;
            _window = window;
        }

        public Size size {
            get { return _size; }
        }

        Size _size = Size.zero;

        public ViewConfiguration configuration {
            get { return _configuration; }
            set {
                D.assert(value != null);
                if (value == _configuration) {
                    return;
                }

                _configuration = value;
                replaceRootLayer((OffsetLayer) _updateMatricesAndCreateNewRootLayer());
                D.assert(_rootTransform != null);
                markNeedsLayout();
            }
        }

        ViewConfiguration _configuration;
        ui.Window _window;

        public void prepareInitialFrame() {
            D.assert(owner != null);
            D.assert(_rootTransform == null);
            scheduleInitialLayout();
            scheduleInitialPaint((OffsetLayer)_updateMatricesAndCreateNewRootLayer());
            D.assert(_rootTransform != null);
        }
        
        Matrix4 _rootTransform;

        public TransformLayer _updateMatricesAndCreateNewRootLayer() {
            _rootTransform = configuration.toMatrix();
            TransformLayer rootLayer = new TransformLayer(transform: _rootTransform);
            rootLayer.attach(this);
            return rootLayer;
        }

        protected override void debugAssertDoesMeetConstraints() {
            D.assert(false);
        }

        protected override void performResize() {
            D.assert(false);
        }

        protected override void performLayout() {
            D.assert(_rootTransform != null);
            _size = configuration.size;
            D.assert(_size.isFinite);

            if (child != null) {
                child.layout(BoxConstraints.tight(_size));
            }
        }

        public bool hitTest(HitTestResult result, Offset position = null) {
            if (child != null) {
                child.hitTest(new BoxHitTestResult(result), position: position);
            }

            result.add(new HitTestEntry(this));
            return true;
        }

        public IEnumerable<MouseTrackerAnnotation> hitTestMouseTrackers(Offset position) {
            return layer.findAllAnnotations<MouseTrackerAnnotation>(
                position * configuration.devicePixelRatio
            ).annotations;
        }

        public override bool isRepaintBoundary {
            get { return true; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                context.paintChild(child, offset);
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            transform.multiply(_rootTransform);
            base.applyPaintTransform(child, transform);
        }

        public void compositeFrame() {
            var builder = new SceneBuilder();
            using (var scene = layer.buildScene(builder)) {
                Window.instance.render(scene);
            }

            D.assert(() => {
                if (D.debugRepaintRainbowEnabled || D.debugRepaintTextRainbowEnabled) {
                    D.debugCurrentRepaintColor =
                        D.debugCurrentRepaintColor.withHue((D.debugCurrentRepaintColor.hue + 2.0f) % 360.0f);
                }

                return true;
            });
        }

        public override Rect paintBounds {
            get { return Offset.zero & (size * configuration.devicePixelRatio); }
        }

        public override Rect semanticBounds {
            get {
                D.assert(_rootTransform != null);
                return MatrixUtils.transformRect(_rootTransform, Offset.zero & size);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            D.assert(() => {
                properties.add(DiagnosticsNode.message("debug mode enabled"));
                return true;
            });
            properties.add(new DiagnosticsProperty<Size>("window size", Window.instance.physicalSize,
                tooltip: "in physical pixels"));
            properties.add(new FloatProperty("device pixel ratio", Window.instance.devicePixelRatio,
                tooltip: "physical pixels per logical pixel"));
            properties.add(new DiagnosticsProperty<ViewConfiguration>("configuration", configuration,
                tooltip: "in logical pixels"));
        }
    }
}