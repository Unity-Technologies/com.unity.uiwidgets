using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public class MultiChildLayoutParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
        public object id;

        public override string ToString() {
            return $"{base.ToString()}; id={id}";
        }
    }

    public abstract class MultiChildLayoutDelegate {
        protected MultiChildLayoutDelegate(Listenable relayout = null) {
            _relayout = relayout;
        }
        public readonly Listenable _relayout;
        Dictionary<object, RenderBox> _idToChild;
        HashSet<RenderBox> _debugChildrenNeedingLayout;

        public bool hasChild(object childId) {
            return _idToChild.getOrDefault(childId) != null;
        }

        public Size layoutChild(object childId, BoxConstraints constraints) {
            RenderBox child = _idToChild[childId];
            D.assert(() => {
                if (child == null) {
                    throw new UIWidgetsError(
                        $"The {this} custom multichild layout delegate tried to lay out a non-existent child.\n" +
                        $"There is no child with the id \"{childId}\"."
                    );
                }

                if (!_debugChildrenNeedingLayout.Remove(child)) {
                    throw new UIWidgetsError(
                        $"The $this custom multichild layout delegate tried to lay out the child with id \"{childId}\" more than once.\n" +
                        "Each child must be laid out exactly once."
                    );
                }

                try {
                    D.assert(constraints.debugAssertIsValid(isAppliedConstraint: true));
                }
                catch (AssertionError exception) {
                    throw new UIWidgetsError(new List<DiagnosticsNode> {
                        new ErrorSummary(
                            $"The $this custom multichild layout delegate provided invalid box constraints for the child with id {childId}."),
                        new DiagnosticsProperty<AssertionError>("Exception", exception, showName: false),
                        new ErrorDescription(
                            "The minimum width and height must be greater than or equal to zero.\n" +
                            "The maximum width must be greater than or equal to the minimum width.\n" +
                            "The maximum height must be greater than or equal to the minimum height."
                        )
                    });
                }

                return true;
            });
            child.layout(constraints, parentUsesSize: true);
            return child.size;
        }

        public void positionChild(object childId, Offset offset) {
            RenderBox child = _idToChild[childId];
            D.assert(() => {
                if (child == null) {
                    throw new UIWidgetsError(
                        $"The {this} custom multichild layout delegate tried to position out a non-existent child:\n" +
                        $"There is no child with the id \"{childId}\"."
                    );
                }

                if (offset == null) {
                    throw new UIWidgetsError(
                        $"The {this} custom multichild layout delegate provided a null position for the child with id \"{childId}\"."
                    );
                }

                return true;
            });
            MultiChildLayoutParentData childParentData = (MultiChildLayoutParentData) child.parentData;
            childParentData.offset = offset;
        }

        DiagnosticsNode _debugDescribeChild(RenderBox child) {
            MultiChildLayoutParentData childParentData = (MultiChildLayoutParentData) child.parentData;
            return new DiagnosticsProperty<RenderBox>($"{childParentData.id}", child);
        }


        internal void _callPerformLayout(Size size, RenderBox firstChild) {
            Dictionary<object, RenderBox> previousIdToChild = _idToChild;

            HashSet<RenderBox> debugPreviousChildrenNeedingLayout = null;
            D.assert(() => {
                debugPreviousChildrenNeedingLayout = _debugChildrenNeedingLayout;
                _debugChildrenNeedingLayout = new HashSet<RenderBox>();
                return true;
            });

            try {
                _idToChild = new Dictionary<object, RenderBox>();
                RenderBox child = firstChild;
                while (child != null) {
                    MultiChildLayoutParentData childParentData = (MultiChildLayoutParentData) child.parentData;
                    D.assert(() => {
                        if (childParentData.id == null) {
                            throw new UIWidgetsError(new List<DiagnosticsNode>{
                                new ErrorSummary("Every child of a RenderCustomMultiChildLayoutBox must have an ID in its parent data."),
                                child.describeForError("The following child has no ID")
                            });
                        }

                        return true;
                    });
                    _idToChild[childParentData.id] = child;
                    D.assert(() => {
                        _debugChildrenNeedingLayout.Add(child);
                        return true;
                    });
                    child = childParentData.nextSibling;
                }

                performLayout(size);
                D.assert(() => {
                    List<DiagnosticsNode> renderBoxes = new List<DiagnosticsNode>();
                    foreach (var renderBox in _debugChildrenNeedingLayout) {
                        renderBoxes.Add(_debugDescribeChild(renderBox));
                    }
                    if (_debugChildrenNeedingLayout.isNotEmpty()) {
                        throw new UIWidgetsError(new List<DiagnosticsNode>{
                            new ErrorSummary("Each child must be laid out exactly once."),
                            new DiagnosticsBlock(
                                name: "The $this custom multichild layout delegate forgot " +
                                      "to lay out the following child(ren)",
                                properties: renderBoxes,
                                style: DiagnosticsTreeStyle.whitespace
                            )
                        });
                    }

                    return true;
                });
            }
            finally {
                _idToChild = previousIdToChild;
                D.assert(() => {
                    _debugChildrenNeedingLayout = debugPreviousChildrenNeedingLayout;
                    return true;
                });
            }
        }

        public virtual Size getSize(BoxConstraints constraints) {
            return constraints.biggest;
        }


        public abstract void performLayout(Size size);


        public abstract bool shouldRelayout(MultiChildLayoutDelegate oldDelegate);

        public override string ToString() {
            return $"{GetType()}";
        }
    }

    public class RenderCustomMultiChildLayoutBox : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<
        RenderBox
        , MultiChildLayoutParentData> {
        public RenderCustomMultiChildLayoutBox(
            List<RenderBox> children = null,
            MultiChildLayoutDelegate layoutDelegate = null
        ) {
            D.assert(layoutDelegate != null);
            _delegate = layoutDelegate;
            addAll(children);
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is MultiChildLayoutParentData)) {
                child.parentData = new MultiChildLayoutParentData();
            }
        }

        public MultiChildLayoutDelegate layoutDelegate {
            get { return _delegate; }
            set {
                D.assert(value != null);
                if (_delegate == value) {
                    return;
                }
                MultiChildLayoutDelegate oldDelegate = _delegate;
                if (value.GetType() != oldDelegate.GetType() || value.shouldRelayout(oldDelegate)) {
                    markNeedsLayout();
                }
                _delegate = value;
                if (attached) {
                    oldDelegate?._relayout?.removeListener(markNeedsLayout);
                    value?._relayout?.addListener(markNeedsLayout);
                }
            }
        }

        MultiChildLayoutDelegate _delegate;
        
        public override void attach(object owner) {
            base.attach(owner);
            _delegate?._relayout?.addListener(markNeedsLayout);
        }

        public override void detach() {
            _delegate?._relayout?.removeListener(markNeedsLayout);
            base.detach();
        }

        Size _getSize(BoxConstraints constraints) {
            D.assert(constraints.debugAssertIsValid());
            return constraints.constrain(_delegate.getSize(constraints));
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            float width = _getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            float width = _getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            float height = _getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float height = _getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected override void performLayout() {
            size = _getSize(constraints);
            layoutDelegate._callPerformLayout(size, firstChild);
        }

        public override void paint(PaintingContext context, Offset offset) {
            defaultPaint(context, offset);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return defaultHitTestChildren(result, position: position);
        }
    }
}