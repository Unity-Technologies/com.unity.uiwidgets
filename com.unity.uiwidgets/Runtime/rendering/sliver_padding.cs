using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class RenderSliverPadding : RenderObjectWithChildMixinRenderSliver<RenderSliver> {
        public RenderSliverPadding(
            EdgeInsets padding = null,
            RenderSliver child = null
        ) {
            D.assert(padding != null);
            D.assert(padding.isNonNegative);

            _padding = padding;
            this.child = child;
        }

        public EdgeInsets padding {
            get { return _padding; }
            set {
                D.assert(value != null);
                D.assert(value.isNonNegative);
                if (_padding == value) {
                    return;
                }

                _padding = value;
                markNeedsLayout();
            }
        }

        EdgeInsets _padding;

        public float beforePadding {
            get {
                D.assert(constraints != null);

                switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                    constraints.axisDirection, constraints.growthDirection)) {
                    case AxisDirection.up:
                        return _padding.bottom;
                    case AxisDirection.right:
                        return _padding.left;
                    case AxisDirection.down:
                        return _padding.top;
                    case AxisDirection.left:
                        return _padding.right;
                }

                return 0.0f;
            }
        }

        public float afterPadding {
            get {
                D.assert(constraints != null);

                switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                    constraints.axisDirection, constraints.growthDirection)) {
                    case AxisDirection.up:
                        return _padding.top;
                    case AxisDirection.right:
                        return _padding.right;
                    case AxisDirection.down:
                        return _padding.bottom;
                    case AxisDirection.left:
                        return _padding.left;
                }

                return 0.0f;
            }
        }

        public float mainAxisPadding {
            get {
                D.assert(constraints != null);

                return _padding.along(constraints.axis);
            }
        }

        public float crossAxisPadding {
            get {
                D.assert(constraints != null);

                switch (constraints.axis) {
                    case Axis.horizontal:
                        return _padding.vertical;
                    case Axis.vertical:
                        return _padding.horizontal;
                }

                D.assert(false);
                return 0.0f;
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverPhysicalParentData)) {
                child.parentData = new SliverPhysicalParentData();
            }
        }

        protected override void performLayout() {
            float beforePadding = this.beforePadding;
            float afterPadding = this.afterPadding;
            float mainAxisPadding = this.mainAxisPadding;
            float crossAxisPadding = this.crossAxisPadding;
            if (child == null) {
                geometry = new SliverGeometry(
                    scrollExtent: mainAxisPadding,
                    paintExtent: Mathf.Min(mainAxisPadding, constraints.remainingPaintExtent),
                    maxPaintExtent: mainAxisPadding
                );
                return;
            }

            child.layout(
                constraints.copyWith(
                    scrollOffset: Mathf.Max(0.0f, constraints.scrollOffset - beforePadding),
                    cacheOrigin: Mathf.Min(0.0f, constraints.cacheOrigin + beforePadding),
                    overlap: 0.0f,
                    remainingPaintExtent: constraints.remainingPaintExtent -
                                          calculatePaintOffset(constraints, from: 0.0f, to: beforePadding),
                    remainingCacheExtent: constraints.remainingCacheExtent -
                                          calculateCacheOffset(constraints, from: 0.0f, to: beforePadding),
                    crossAxisExtent: Mathf.Max(0.0f, constraints.crossAxisExtent - crossAxisPadding)
                ),
                parentUsesSize: true
            );

            SliverGeometry childLayoutGeometry = child.geometry;
            if (childLayoutGeometry.scrollOffsetCorrection != null) {
                geometry = new SliverGeometry(
                    scrollOffsetCorrection: childLayoutGeometry.scrollOffsetCorrection
                );
                return;
            }

            float beforePaddingPaintExtent = calculatePaintOffset(
                constraints,
                from: 0.0f,
                to: beforePadding
            );

            float afterPaddingPaintExtent = calculatePaintOffset(
                constraints,
                from: beforePadding + childLayoutGeometry.scrollExtent,
                to: mainAxisPadding + childLayoutGeometry.scrollExtent
            );

            float mainAxisPaddingPaintExtent = beforePaddingPaintExtent + afterPaddingPaintExtent;
            float beforePaddingCacheExtent = calculateCacheOffset(
                constraints,
                from: 0.0f,
                to: beforePadding
            );
            float afterPaddingCacheExtent = calculateCacheOffset(
                constraints,
                from: beforePadding + childLayoutGeometry.scrollExtent,
                to: mainAxisPadding + childLayoutGeometry.scrollExtent
            );

            float mainAxisPaddingCacheExtent = afterPaddingCacheExtent + beforePaddingCacheExtent;
            float paintExtent = Mathf.Min(
                beforePaddingPaintExtent + Mathf.Max(childLayoutGeometry.paintExtent,
                    childLayoutGeometry.layoutExtent + afterPaddingPaintExtent),
                constraints.remainingPaintExtent
            );

            geometry = new SliverGeometry(
                scrollExtent: mainAxisPadding + childLayoutGeometry.scrollExtent,
                paintExtent: paintExtent,
                layoutExtent: Mathf.Min(mainAxisPaddingPaintExtent + childLayoutGeometry.layoutExtent,
                    paintExtent),
                cacheExtent: Mathf.Min(mainAxisPaddingCacheExtent + childLayoutGeometry.cacheExtent,
                    constraints.remainingCacheExtent),
                maxPaintExtent: mainAxisPadding + childLayoutGeometry.maxPaintExtent,
                hitTestExtent: Mathf.Max(
                    mainAxisPaddingPaintExtent + childLayoutGeometry.paintExtent,
                    beforePaddingPaintExtent + childLayoutGeometry.hitTestExtent
                ),
                hasVisualOverflow: childLayoutGeometry.hasVisualOverflow
            );

            var childParentData = (SliverPhysicalParentData) child.parentData;
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection,
                constraints.growthDirection)) {
                case AxisDirection.up:
                    childParentData.paintOffset = new Offset(_padding.left,
                        calculatePaintOffset(constraints,
                            from: _padding.bottom + childLayoutGeometry.scrollExtent,
                            to: _padding.bottom + childLayoutGeometry.scrollExtent + _padding.top));
                    break;
                case AxisDirection.right:
                    childParentData.paintOffset =
                        new Offset(calculatePaintOffset(constraints, from: 0.0f, to: _padding.left),
                            _padding.top);
                    break;
                case AxisDirection.down:
                    childParentData.paintOffset = new Offset(_padding.left,
                        calculatePaintOffset(constraints, from: 0.0f, to: _padding.top));
                    break;
                case AxisDirection.left:
                    childParentData.paintOffset = new Offset(
                        calculatePaintOffset(constraints,
                            from: _padding.right + childLayoutGeometry.scrollExtent,
                            to: _padding.right + childLayoutGeometry.scrollExtent + _padding.left),
                        _padding.top);
                    break;
            }

            D.assert(childParentData.paintOffset != null);
            D.assert(beforePadding == this.beforePadding);
            D.assert(afterPadding == this.afterPadding);
            D.assert(mainAxisPadding == this.mainAxisPadding);
            D.assert(crossAxisPadding == this.crossAxisPadding);
        }

        protected override bool hitTestChildren(SliverHitTestResult result, float mainAxisPosition = 0.0f,
            float crossAxisPosition = 0.0f) {
            if (child != null && child.geometry.hitTestExtent > 0.0) {
                SliverPhysicalParentData childParentData = child.parentData as SliverPhysicalParentData;
                result.addWithAxisOffset(
                    mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition,
                    mainAxisOffset: childMainAxisPosition(child),
                    crossAxisOffset: childCrossAxisPosition(child),
                    paintOffset: childParentData.paintOffset,
                    hitTest: child.hitTest
                );
            }

            return false;
        }

        public override float childMainAxisPosition(RenderObject child) {
            D.assert(child != null);
            D.assert(child == this.child);

            return calculatePaintOffset(constraints, from: 0.0f, to: beforePadding);
        }

        public override float childCrossAxisPosition(RenderObject child) {
            D.assert(child != null);
            D.assert(child == this.child);
            D.assert(constraints != null);

            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                constraints.axisDirection, constraints.growthDirection)) {
                case AxisDirection.up:
                case AxisDirection.down:
                    return _padding.left;
                case AxisDirection.left:
                case AxisDirection.right:
                    return _padding.top;
            }

            return 0.0f;
        }

        public override float childScrollOffset(RenderObject child) {
            D.assert(child.parent == this);

            return beforePadding;
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(child == this.child);

            var childParentData = (SliverPhysicalParentData) child.parentData;
            childParentData.applyPaintTransform(transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null && child.geometry.visible) {
                var childParentData = (SliverPhysicalParentData) child.parentData;
                context.paintChild(child, offset + childParentData.paintOffset);
            }
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            base.debugPaint(context, offset);
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    Size parentSize = getAbsoluteSizeRelativeToOrigin();
                    Rect outerRect = offset & parentSize;
                    Size childSize = null;
                    Rect innerRect = null;
                    if (child != null) {
                        childSize = child.getAbsoluteSizeRelativeToOrigin();
                        var childParentData = (SliverPhysicalParentData) child.parentData;
                        innerRect = (offset + childParentData.paintOffset) & childSize;
                        D.assert(innerRect.top >= outerRect.top);
                        D.assert(innerRect.left >= outerRect.left);
                        D.assert(innerRect.right <= outerRect.right);
                        D.assert(innerRect.bottom <= outerRect.bottom);
                    }

                    D.debugPaintPadding(context.canvas, outerRect, innerRect);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", padding));
        }
    }
}