using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public delegate Widget LayoutWidgetBuilder(BuildContext context, BoxConstraints constraints);
    public abstract class ConstrainedLayoutBuilder<ConstraintType> : RenderObjectWidget where ConstraintType : Constraints {
        public delegate Widget ConstraintBuilder(BuildContext context, ConstraintType constraints);

        
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

        public virtual ConstraintBuilder builder { get; }
    }


    public class _LayoutBuilderElement<ConstraintType> :  RenderObjectElement where ConstraintType : Constraints {
        public _LayoutBuilderElement(ConstrainedLayoutBuilder<ConstraintType> widget) 
            : base(widget) {
        }

        public new ConstrainedLayoutBuilder<ConstraintType> widget {
            get {
                return (ConstrainedLayoutBuilder<ConstraintType>)base.widget ;
            }
        }
        public RenderConstrainedLayoutBuilder<ConstraintType> renderObject_builder {
            get {
                return base.renderObject as RenderConstrainedLayoutBuilder<ConstraintType> ;
            }
        }

        public RenderObjectWithChildMixin renderObject_childMxin {
            get { return base.renderObject as RenderObjectWithChildMixin; }
        }
        
        Element _child;

        public override void visitChildren(ElementVisitor visitor) {
            if (_child != null)
                visitor(_child);
        }

        public override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
            base.forgetChild(child);
        }
        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot); // Creates the renderObject.
            renderObject_builder.updateCallback(_layout);
        }

        public override void update(Widget newWidget) {
            newWidget = (ConstrainedLayoutBuilder<ConstraintType>) newWidget;
            D.assert(widget != newWidget);
            base.update(newWidget);
            D.assert(widget == newWidget);
            renderObject_builder.updateCallback(_layout);
            renderObject.markNeedsLayout();
        }

        protected override void performRebuild() {
            renderObject.markNeedsLayout();
            base.performRebuild(); // Calls widget.updateRenderObject (a no-op in this case).
        }

        public override void unmount() {
            renderObject_builder.updateCallback(null);
            base.unmount();
        }

        
        public void _layout(ConstraintType constraints) {
            owner.buildScope(this, ()=> {
                
                Widget built = null;
                if (widget.builder != null) {
                    try {
                        built = widget.builder(this, constraints);
                        WidgetsD.debugWidgetBuilderValue(widget, built);
                    } catch (Exception e) {
                        _debugDoingBuild = false;
                        IEnumerable<DiagnosticsNode> informationCollector() {
                            yield return new DiagnosticsDebugCreator(new DebugCreator(this));
                        }
                        built = ErrorWidget.builder(WidgetsD._debugReportException("building " + this, e, informationCollector));
                    
                    }
                }
                try {
                    _child = updateChild(_child, built, null);
                    D.assert(_child != null);
                } catch (Exception e) {
                    _debugDoingBuild = false;

                    IEnumerable<DiagnosticsNode> informationCollector() {
                        yield return new DiagnosticsDebugCreator(new DebugCreator(this));
                    }
                    built = ErrorWidget.builder(WidgetsD._debugReportException("building " + this, e, informationCollector));
                }

            });
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            RenderObjectWithChildMixin renderObject = renderObject_childMxin;
            D.assert(slot == null);
            D.assert(renderObject.debugValidateChild(child));
            renderObject.child = child;
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            RenderObjectWithChildMixin renderObject = renderObject_childMxin;
            D.assert(renderObject.child == child);
            renderObject.child = null;
            D.assert(renderObject == this.renderObject);
        }

    }

    public interface RenderConstrainedLayoutBuilder<ConstraintType>
        where ConstraintType : Constraints
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

        protected internal override float computeMinIntrinsicWidth(float height) {
            D.assert(_debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            D.assert(_debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            D.assert(_debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            D.assert(_debugThrowIfNotCheckingIntrinsics());
            return 0.0f;
        }

        protected override void performLayout() {
            D.assert(_callback != null);
            invokeLayoutCallback(_callback);
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