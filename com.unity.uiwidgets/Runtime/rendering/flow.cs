using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public interface FlowPaintingContext {
        Size size { get; }

        int childCount { get; }

        public Size getChildSize(int i);

        public void paintChild(int i, Matrix4 transform = null, float opacity = 1.0f);
    }

    public abstract class FlowDelegate {
        public FlowDelegate(Listenable repaint = null) {
            _repaint = repaint;
        }

        public readonly Listenable _repaint;

        public virtual Size getSize(BoxConstraints constraints) => constraints.biggest;

        public virtual BoxConstraints getConstraintsForChild(int i, BoxConstraints constraints) => constraints;

        public abstract void paintChildren(FlowPaintingContext context);

        public virtual bool shouldRelayout(FlowDelegate oldDelegate) => false;

        public abstract bool shouldRepaint(FlowDelegate oldDelegate);

        public override string ToString() => $"{this}, 'FlowDelegate'";
    }

    class FlowParentData : ContainerBoxParentData<RenderBox> {
        public Matrix4 _transform;
    }

    class RenderFlow : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox, FlowParentData>
        , FlowPaintingContext {
        public RenderFlow(
            List<RenderBox> children = null,
            FlowDelegate del = null) {
            D.assert(del != null);
            _delegate = del;
            addAll(children);
        }

        public override void setupParentData(RenderObject child) {
            ParentData childParentData = child.parentData;
            if (childParentData is FlowParentData flowParentData) {
                flowParentData._transform = null;
            }
            else {
                child.parentData = new FlowParentData();
            }
        }

        public FlowDelegate del {
            get { return _delegate; }
            set {
                D.assert(value != null);
                if (_delegate == value)
                    return;

                FlowDelegate oldDelegate = _delegate;
                _delegate = value;

                if (value.GetType() != oldDelegate.GetType() || value.shouldRelayout(oldDelegate))
                    markNeedsLayout();
                else if (value.shouldRepaint(oldDelegate))
                    markNeedsPaint();

                if (attached) {
                    oldDelegate._repaint?.removeListener(markNeedsPaint);
                    value._repaint?.addListener(markNeedsPaint);
                }
            }
        }

        public FlowDelegate _delegate;

        public override void attach(object owner) {
            base.attach(owner);
            _delegate._repaint?.addListener(markNeedsPaint);
        }

        public override void detach() {
            _delegate._repaint?.removeListener(markNeedsPaint);
            base.detach();
        }

        Size _getSize(BoxConstraints constraints) {
            D.assert(constraints.debugAssertIsValid());
            return constraints.constrain(_delegate.getSize(constraints));
        }

        public override bool isRepaintBoundary => true;


        protected internal override float computeMinIntrinsicWidth(float height) {
            float width = _getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite())
                return width;
            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            float width = _getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite())
                return width;
            return 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            float height = _getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite())
                return height;
            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float height = _getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite())
                return height;
            return 0.0f;
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            size = _getSize(constraints);
            int i = 0;
            _randomAccessChildren.Clear();
            RenderBox child = firstChild;
            while (child != null) {
                _randomAccessChildren.Add(child);
                BoxConstraints innerConstraints = _delegate.getConstraintsForChild(i, constraints);
                child.layout(innerConstraints, parentUsesSize: true);
                FlowParentData childParentData = child.parentData as FlowParentData;
                childParentData.offset = Offset.zero;
                child = childParentData.nextSibling;
                i += 1;
            }
        }

        // Updated during layout. Only valid if layout is not dirty.
        List<RenderBox> _randomAccessChildren = new List<RenderBox>();

        // Updated during paint.
        List<int> _lastPaintOrder = new List<int>();

        // Only valid during paint.
        PaintingContext _paintingContext;
        Offset _paintingOffset;

        public Size getChildSize(int i) {
            if (i < 0 || i >= _randomAccessChildren.Count)
                return null;
            return _randomAccessChildren[i].size;
        }

        public void paintChild(int i, Matrix4 transform = null, float opacity = 1.0f) {
            transform ??= Matrix4.identity();
            RenderBox child = _randomAccessChildren[i];
            FlowParentData childParentData = child.parentData as FlowParentData;
            D.assert(() => {
                if (childParentData._transform != null) {
                    throw new UIWidgetsError(
                        "Cannot call paintChild twice for the same child.\n" +
                        "The flow delegate of type ${_delegate.runtimeType} attempted to " +
                        "paint child $i multiple times, which is not permitted."
                    );
                }

                return true;
            });
            _lastPaintOrder.Add(i);
            childParentData._transform = transform;

            if (opacity == 0.0)
                return;

            void painter(PaintingContext context, Offset offset) {
                context.paintChild(child, offset);
            }

            if (opacity == 1.0f) {
                _paintingContext.pushTransform(needsCompositing, _paintingOffset, transform, painter);
            }
            else {
                _paintingContext.pushOpacity(_paintingOffset, ui.Color.getAlphaFromOpacity(opacity),
                    (PaintingContext context, Offset offset) => {
                        context.pushTransform(needsCompositing, offset, transform, painter);
                    });
            }
        }

        void _paintWithDelegate(PaintingContext context, Offset offset) {
            _lastPaintOrder.Clear();
            _paintingContext = context;
            _paintingOffset = offset;
            foreach (RenderBox child in _randomAccessChildren) {
                FlowParentData childParentData = child.parentData as FlowParentData;
                childParentData._transform = null;
            }

            try {
                _delegate.paintChildren(this);
            }
            finally {
                _paintingContext = null;
                _paintingOffset = null;
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            context.pushClipRect(needsCompositing, offset, Offset.zero & size, _paintWithDelegate);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            List<RenderBox> children = getChildrenAsList();
            for (int i = _lastPaintOrder.Count - 1; i >= 0; --i) {
                int childIndex = _lastPaintOrder[i];
                if (childIndex >= children.Count)
                    continue;
                RenderBox child = children[childIndex];
                FlowParentData childParentData = child.parentData as FlowParentData;
                Matrix4 transform = childParentData._transform;
                if (transform == null)
                    continue;

                bool absorbed = result.addWithPaintTransform(
                    transform: transform,
                    position: position,
                    hitTest: (BoxHitTestResult result, Offset position) => {
                        return child.hitTest(result, position: position);
                    }
                );
                if (absorbed)
                    return true;
            }

            return false;
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            FlowParentData childParentData = child.parentData as FlowParentData;
            if (childParentData._transform != null)
                transform.multiply(childParentData._transform);
            base.applyPaintTransform(child, transform);
        }
    }
}