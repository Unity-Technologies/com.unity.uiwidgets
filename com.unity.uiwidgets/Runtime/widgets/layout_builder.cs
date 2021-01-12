using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate Widget LayoutWidgetBuilder(BuildContext context, BoxConstraints constraints);
    public delegate Widget ConstraintBuilder(BuildContext context, Constraints constraints);
    public abstract class ConstrainedLayoutBuilder<ConstraintType> : RenderObjectWidget where ConstraintType : Constraints {

        
        public ConstrainedLayoutBuilder(
            Key key = null,
            ConstraintBuilder builder = null
        ) : base(key: key) {
            D.assert(builder != null);
            this.builder = builder;
        }

        public override Element createElement() {
            return new _LayoutBuilderElement<ConstraintType>(this);
        }

        public readonly ConstraintBuilder builder;

    }


    public class _LayoutBuilderElement<ConstraintType> :  RenderObjectElement
        where ConstraintType : Constraints {
        public _LayoutBuilderElement(ConstrainedLayoutBuilder<ConstraintType> widget) 
            : base(widget) {
        }

        public new ConstrainedLayoutBuilder<ConstraintType> widget {
            get {
                return base.widget as ConstrainedLayoutBuilder<ConstraintType>;
            }
        }
        public new RenderConstrainedLayoutBuilderMixinRenderObject<ConstraintType, RenderObject> renderObject {
            get { return base.renderObject as RenderConstrainedLayoutBuilderMixinRenderObject<ConstraintType, RenderObject>;}
        }
        Element _child;

        public override void visitChildren(ElementVisitor visitor) {
            if (_child != null)
                visitor(_child);
        }

        internal override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
            base.forgetChild(child);
        }
        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot); // Creates the renderObject.
            renderObject.updateCallback(_layout);
        }

        public override void update(Widget newWidget) {
            newWidget = (ConstrainedLayoutBuilder<ConstraintType>) newWidget;
            D.assert(widget != newWidget);
            base.update(newWidget);
            D.assert(widget == newWidget);
            renderObject.updateCallback(_layout);
            renderObject.markNeedsLayout();
        }

        protected override void performRebuild() {
            renderObject.markNeedsLayout();
            base.performRebuild(); // Calls widget.updateRenderObject (a no-op in this case).
        }

        public override void unmount() {
            renderObject.updateCallback(null);
            base.unmount();
        }

        
        public void _layout(ConstraintType constraints) {
            owner.buildScope(this, ()=> {
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
            RenderObjectWithChildMixin<RenderObject> renderObject = this.renderObject;
            D.assert(slot == null);
            D.assert(renderObject.debugValidateChild(child));
            renderObject.child = child;
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            RenderConstrainedLayoutBuilderMixinRenderObject<ConstraintType, RenderObject> renderObject = this.renderObject;
            D.assert(renderObject.child == child);
            renderObject.child = null;
            D.assert(renderObject == this.renderObject);
        }

    }

    public interface RenderConstrainedLayoutBuilder<ConstraintType,ChildType>
        where ConstraintType : Constraints 
        where ChildType : RenderObject
    {

        LayoutCallback<ConstraintType> _callback { get; set; }

        void updateCallback(LayoutCallback<ConstraintType> value);

        void layoutAndBuildChild();
    }

    public class LayoutBuilder : ConstrainedLayoutBuilder<BoxConstraints> {
        public LayoutBuilder(
            Key key = null,
            ConstraintBuilder builder = null
            ) : base(key: key, builder: builder) {
            D.assert(builder != null);
        }

        public static LayoutBuilder Create(
            LayoutWidgetBuilder builder,
            Key key = null
            ) {
            ConstraintBuilder _builder = (context, constraints) => {
                return builder(context, (BoxConstraints)constraints);
            };
            return new LayoutBuilder(key,_builder);
        }

        public new LayoutWidgetBuilder builder {
            get {
                LayoutWidgetBuilder  _builder = (context, constraints) => {
                    return base.builder(context, (BoxConstraints) constraints);
                };
                return _builder;
            }
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderLayoutBuilder();
        }
    }
    
    public class _RenderLayoutBuilder : RenderConstrainedLayoutBuilderMixinRenderBox<BoxConstraints, RenderBox> {
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

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return child?.hitTest(result, position: position) ?? false;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                context.paintChild(child, offset);
            }
        }
    }
}