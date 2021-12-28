using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public class ListBodyParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
    }

    delegate float __ChildSizingFunction(RenderBox child);


    public class RenderListBody : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        ListBodyParentData> {
        public RenderListBody(
            List<RenderBox> children = null,
            AxisDirection axisDirection = AxisDirection.down) {
            _axisDirection = axisDirection;
            addAll(children);
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is ListBodyParentData)) {
                child.parentData = new ListBodyParentData();
            }
        }

        public AxisDirection axisDirection {
            get { return _axisDirection; }
            set {
                if (_axisDirection == value) {
                    return;
                }

                _axisDirection = value;
                markNeedsLayout();
            }
        }

        AxisDirection _axisDirection;

        public Axis? mainAxis {
            get { return AxisUtils.axisDirectionToAxis(axisDirection); }
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            D.assert(() => {
                switch (mainAxis) {
                    case Axis.horizontal:
                        if (!constraints.hasBoundedWidth) {
                            return true;
                        }

                        break;
                    case Axis.vertical:
                        if (!constraints.hasBoundedHeight) {
                            return true;
                        }

                        break;
                }

                throw new UIWidgetsError(new List<DiagnosticsNode>{
                    new ErrorSummary("RenderListBody must have unlimited space along its main axis."),
                    new ErrorDescription(
                        "RenderListBody does not clip or resize its children, so it must be " +
                        "placed in a parent that does not constrain the main axis."
                    ),
                    new ErrorHint(
                        "You probably want to put the RenderListBody inside a " +
                        "RenderViewport with a matching main axis."
                    )
                });
            });

            D.assert(() => {
                switch (mainAxis) {
                    case Axis.horizontal:
                        if (constraints.hasBoundedHeight) {
                            return true;
                        }

                        break;
                    case Axis.vertical:
                        if (constraints.hasBoundedWidth) {
                            return true;
                        }

                        break;
                }

                throw new UIWidgetsError(new List<DiagnosticsNode>{
                    new ErrorSummary("RenderListBody must have a bounded constraint for its cross axis."),
                    new ErrorDescription(
                        "RenderListBody forces its children to expand to fit the RenderListBody's container, " +
                        "so it must be placed in a parent that constrains the cross " +
                        "axis to a finite dimension."
                    ),
                    // TODO(jacobr): this hint is a great candidate to promote to being an
                    // automated quick fix in the future.
                    new ErrorHint(
                        "If you are attempting to nest a RenderListBody with " +
                        "one direction inside one of another direction, you will want to " +
                        "wrap the inner one inside a box that fixes the dimension in that direction, " +
                        "for example, a RenderIntrinsicWidth or RenderIntrinsicHeight object. " +
                        "This is relatively expensive, however." // (that's why we don't do it automatically)
                    )
                });
            });

            float mainAxisExtent = 0.0f;
            RenderBox child = firstChild;

            BoxConstraints innerConstraints;
            float position;

            switch (axisDirection) {
                case AxisDirection.right:
                    innerConstraints = BoxConstraints.tightFor(height: constraints.maxHeight);
                    while (child != null) {
                        child.layout(innerConstraints, parentUsesSize: true);
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        childParentData.offset = new Offset(mainAxisExtent, 0.0f);
                        mainAxisExtent += child.size.width;
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    size = constraints.constrain(new Size(mainAxisExtent,
                        constraints.maxHeight));
                    break;
                case AxisDirection.left:
                    innerConstraints = BoxConstraints.tightFor(height: constraints.maxHeight);
                    while (child != null) {
                        child.layout(innerConstraints, parentUsesSize: true);
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        mainAxisExtent += child.size.width;
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    position = 0.0f;
                    child = firstChild;
                    while (child != null) {
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        position += child.size.width;
                        childParentData.offset = new Offset((mainAxisExtent - position), 0.0f);
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    size = constraints.constrain(new Size(mainAxisExtent,
                        constraints.maxHeight));
                    break;
                case AxisDirection.down:
                    innerConstraints = BoxConstraints.tightFor(width: constraints.maxWidth);
                    while (child != null) {
                        child.layout(innerConstraints, parentUsesSize: true);
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        childParentData.offset = new Offset(0.0f, mainAxisExtent);
                        mainAxisExtent += child.size.height;
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    size = constraints.constrain(new Size(constraints.maxWidth, mainAxisExtent));
                    break;
                case AxisDirection.up:
                    innerConstraints = BoxConstraints.tightFor(width: constraints.maxWidth);
                    while (child != null) {
                        child.layout(innerConstraints, parentUsesSize: true);
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        mainAxisExtent += child.size.height;
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    position = 0.0f;
                    child = firstChild;
                    while (child != null) {
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        position += child.size.height;
                        childParentData.offset = new Offset(0.0f, mainAxisExtent - position);
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    size = constraints.constrain(new Size(constraints.maxWidth, mainAxisExtent));
                    break;
            }

            D.assert(size.isFinite);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection>("axisDirection", axisDirection));
        }

        float _getIntrinsicCrossAxis(__ChildSizingFunction childSize) {
            float extent = 0.0f;
            RenderBox child = firstChild;
            while (child != null) {
                extent = Mathf.Max(extent, childSize(child));
                ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                child = childParentData.nextSibling;
            }

            return extent;
        }

        float _getIntrinsicMainAxis(__ChildSizingFunction childSize) {
            float extent = 0.0f;
            RenderBox child = firstChild;
            while (child != null) {
                extent += childSize(child);
                ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                child = childParentData.nextSibling;
            }

            return extent;
        }


        protected internal override float computeMinIntrinsicWidth(float height) {
            switch (mainAxis) {
                case Axis.horizontal:
                    return _getIntrinsicMainAxis((RenderBox child) => child.getMinIntrinsicWidth(height));
                case Axis.vertical:
                    return _getIntrinsicCrossAxis((RenderBox child) => child.getMinIntrinsicWidth(height));
            }
            D.assert(false);
            return 0.0f;
        }


        protected internal override float computeMaxIntrinsicWidth(float height) {
            switch (mainAxis) {
                case Axis.horizontal:
                    return _getIntrinsicMainAxis((RenderBox child) => child.getMaxIntrinsicWidth(height));
                case Axis.vertical:
                    return _getIntrinsicCrossAxis((RenderBox child) => child.getMaxIntrinsicWidth(height));
            }

            D.assert(false);
            return 0.0f;
        }


        protected internal override float computeMinIntrinsicHeight(float width) {
            switch (mainAxis) {
                case Axis.horizontal:
                    return _getIntrinsicMainAxis((RenderBox child) => child.getMinIntrinsicHeight(width));
                case Axis.vertical:
                    return _getIntrinsicCrossAxis((RenderBox child) => child.getMinIntrinsicHeight(width));
            }

            D.assert(false);
            return 0.0f;
        }


        protected internal override float computeMaxIntrinsicHeight(float width) {
            switch (mainAxis) {
                case Axis.horizontal:
                    return _getIntrinsicMainAxis((RenderBox child) => child.getMaxIntrinsicHeight(width));
                case Axis.vertical:
                    return _getIntrinsicCrossAxis((RenderBox child) => child.getMaxIntrinsicHeight(width));
            }

            D.assert(false);
            return 0.0f;
        }


        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return defaultComputeDistanceToFirstActualBaseline(baseline);
        }


        public override void paint(PaintingContext context, Offset offset) {
            defaultPaint(context, offset);
        }


        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return defaultHitTestChildren(result, position: position);
        }
    }
}