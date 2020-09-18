using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate Widget LayoutWidgetBuilder(BuildContext context, BoxConstraints constraints);

    public class LayoutBuilder : RenderObjectWidget {
        public LayoutBuilder(
            Key key = null,
            LayoutWidgetBuilder builder = null) : base(key: key) {
            D.assert(builder != null);
            this.builder = builder;
        }

        public readonly LayoutWidgetBuilder builder;

        public override Element createElement() {
            return new _LayoutBuilderElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderLayoutBuilder();
        }
    }

    class _LayoutBuilderElement : RenderObjectElement {
        public _LayoutBuilderElement(
            LayoutBuilder widget) : base(widget) {
        }

        new LayoutBuilder widget {
            get { return (LayoutBuilder) base.widget; }
        }

        new _RenderLayoutBuilder renderObject {
            get { return (_RenderLayoutBuilder) base.renderObject; }
        }

        Element _child;

        public override void visitChildren(ElementVisitor visitor) {
            if (_child != null) {
                visitor(_child);
            }
        }

        protected override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            renderObject.callback = _layout;
        }

        public override void update(Widget newWidget) {
            D.assert(widget != newWidget);
            base.update(newWidget);
            D.assert(widget == newWidget);
            renderObject.callback = _layout;
            renderObject.markNeedsLayout();
        }

        protected override void performRebuild() {
            renderObject.markNeedsLayout();
            base.performRebuild();
        }

        public override void unmount() {
            renderObject.callback = null;
            base.unmount();
        }

        void _layout(BoxConstraints constraints) {
            owner.buildScope(this, () => {
                Widget built = null;
                if (widget.builder != null) {
                    built = widget.builder(this, constraints);
                    WidgetsD.debugWidgetBuilderValue(widget, built);
                }

                _child = updateChild(_child, built, null);
                D.assert(_child != null);
            });
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            _RenderLayoutBuilder renderObject = this.renderObject;
            D.assert(slot == null);
            D.assert(renderObject.debugValidateChild(child));
            renderObject.child = (RenderBox) child;
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            _RenderLayoutBuilder renderObject = this.renderObject;
            D.assert(renderObject.child == child);
            renderObject.child = null;
            D.assert(renderObject == this.renderObject);
        }
    }


    public class _RenderLayoutBuilder : RenderObjectWithChildMixinRenderBox<RenderBox> {
        public _RenderLayoutBuilder(
            LayoutCallback<BoxConstraints> callback = null) {
            _callback = callback;
        }

        public LayoutCallback<BoxConstraints> callback {
            get { return _callback; }
            set {
                if (value == _callback) {
                    return;
                }

                _callback = value;
                markNeedsLayout();
            }
        }

        LayoutCallback<BoxConstraints> _callback;

        bool _debugThrowIfNotCheckingIntrinsics() {
            D.assert(() => {
                if (!debugCheckingIntrinsics) {
                    throw new UIWidgetsError(
                        "LayoutBuilder does not support returning intrinsic dimensions.\n" +
                        "Calculating the intrinsic dimensions would require running the layout " +
                        "callback speculatively, which might mutate the live render object tree."
                    );
                }

                return true;
            });
            return true;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            D.assert(_debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            D.assert(_debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            D.assert(_debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            D.assert(_debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected override void performLayout() {
            D.assert(callback != null);
            invokeLayoutCallback(callback);
            if (child != null) {
                child.layout(constraints, parentUsesSize: true);
                size = constraints.constrain(child.size);
            }
            else {
                size = constraints.biggest;
            }
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            return child?.hitTest(result, position: position) ?? false;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                context.paintChild(child, offset);
            }
        }
    }
}