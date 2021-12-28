using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    
    public abstract class RenderSliverEdgeInsetsPadding : RenderObjectWithChildMixinRenderSliver<RenderSliver> {

        protected virtual EdgeInsets resolvedPadding { get; }
        
        float?  beforePadding {
            get {
                D.assert(constraints != null);
                D.assert(resolvedPadding != null);
                switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection, constraints.growthDirection)) {
                    case AxisDirection.up:
                        return resolvedPadding.bottom;
                    case AxisDirection.right:
                        return resolvedPadding.left;
                    case AxisDirection.down:
                        return resolvedPadding.top;
                    case AxisDirection.left:
                        return resolvedPadding.right;
                }
                return null;
            }

        }
        float?  afterPadding { 
            get { 
                D.assert(constraints != null);
                D.assert(resolvedPadding != null);
                switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection, constraints.growthDirection)) {
                    case AxisDirection.up:
                        return resolvedPadding.top;
                    case AxisDirection.right:
                        return resolvedPadding.right;
                    case AxisDirection.down:
                        return resolvedPadding.bottom;
                    case AxisDirection.left:
                        return resolvedPadding.left;
                }
                return null;
            }
        }
        float?  mainAxisPadding { 
            get { 
                D.assert(constraints != null);
                D.assert(constraints.axis != null);
                D.assert(resolvedPadding != null);
                return resolvedPadding.along(constraints.axis);
            }
        }

        float? crossAxisPadding {
            get { 
                D.assert(constraints != null);
                D.assert(constraints.axis != null);
                D.assert(resolvedPadding != null);
                switch (constraints.axis) {
                    case Axis.horizontal:
                        return resolvedPadding.vertical;
                    case Axis.vertical:
                        return resolvedPadding.horizontal;
                }
                return null;
            }
        }
        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverPhysicalParentData)) 
                child.parentData = new SliverPhysicalParentData();
        }
        protected override void performLayout() {
            SliverConstraints constraints = this.constraints; 
            D.assert(resolvedPadding != null);
            float? beforePadding = this.beforePadding;
            float? afterPadding = this.afterPadding;
            float? mainAxisPadding = this.mainAxisPadding;
            float? crossAxisPadding = this.crossAxisPadding; 
            if (child == null) { 
                geometry = new SliverGeometry(
                    scrollExtent: mainAxisPadding ?? 0.0f, 
                    paintExtent: Mathf.Min(mainAxisPadding ?? 0.0f, constraints.remainingPaintExtent), 
                    maxPaintExtent: mainAxisPadding ?? 0.0f
                    ); 
                return; 
            } 
            child.layout(
                constraints.copyWith(
                    scrollOffset: Mathf.Max(0.0f, constraints.scrollOffset - beforePadding ?? 0.0f), 
                    cacheOrigin: Mathf.Min(0.0f, constraints.cacheOrigin + beforePadding ?? 0.0f), 
                    overlap: 0.0f, 
                    remainingPaintExtent: constraints.remainingPaintExtent - calculatePaintOffset(constraints, from: 0.0f, to: beforePadding ?? 0.0f), 
                    remainingCacheExtent: constraints.remainingCacheExtent - calculateCacheOffset(constraints, from: 0.0f, to: beforePadding ?? 0.0f), 
                    crossAxisExtent: Mathf.Max(0.0f, constraints.crossAxisExtent - crossAxisPadding ?? 0.0f), 
                    precedingScrollExtent: beforePadding ?? 0.0f + constraints.precedingScrollExtent
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
                to: beforePadding ?? 0.0f
                ); 
            float afterPaddingPaintExtent = calculatePaintOffset(
                constraints, 
                from: beforePadding ?? 0.0f + childLayoutGeometry.scrollExtent, 
                to: mainAxisPadding ?? 0.0f + childLayoutGeometry.scrollExtent
                ); 
            float mainAxisPaddingPaintExtent = beforePaddingPaintExtent + afterPaddingPaintExtent; 
            float beforePaddingCacheExtent = calculateCacheOffset(
                constraints, 
                from: 0.0f, 
                to: beforePadding ?? 0.0f
                ); 
            float afterPaddingCacheExtent = calculateCacheOffset(
                constraints, 
                from: beforePadding ?? 0.0f  + childLayoutGeometry.scrollExtent, 
                to: mainAxisPadding ?? 0.0f + childLayoutGeometry.scrollExtent
                ); 
            float mainAxisPaddingCacheExtent = afterPaddingCacheExtent + beforePaddingCacheExtent; 
            float paintExtent = Mathf.Min(
                beforePaddingPaintExtent + Mathf.Max(childLayoutGeometry.paintExtent, childLayoutGeometry.layoutExtent + afterPaddingPaintExtent), 
                constraints.remainingPaintExtent
                ); 
            geometry = new SliverGeometry(
                scrollExtent: (mainAxisPadding ?? 0.0f) + childLayoutGeometry.scrollExtent, 
                paintExtent: paintExtent, 
                layoutExtent: Mathf.Min(mainAxisPaddingPaintExtent + childLayoutGeometry.layoutExtent, paintExtent), 
                cacheExtent: Mathf.Min(mainAxisPaddingCacheExtent + childLayoutGeometry.cacheExtent, constraints.remainingCacheExtent), 
                maxPaintExtent: (mainAxisPadding ?? 0.0f) + childLayoutGeometry.maxPaintExtent, 
                hitTestExtent: Mathf.Max(
                    mainAxisPaddingPaintExtent + childLayoutGeometry.paintExtent, 
                    beforePaddingPaintExtent + childLayoutGeometry.hitTestExtent
                    ), 
                hasVisualOverflow: childLayoutGeometry.hasVisualOverflow
                );
            SliverPhysicalParentData childParentData = child.parentData as SliverPhysicalParentData;
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection, constraints.growthDirection)) { 
                case AxisDirection.up: 
                    childParentData.paintOffset = new Offset(resolvedPadding.left, calculatePaintOffset(constraints, from: resolvedPadding.bottom + childLayoutGeometry.scrollExtent, to: resolvedPadding.bottom + childLayoutGeometry.scrollExtent + resolvedPadding.top)); 
                    break; 
                case AxisDirection.right:
                    childParentData.paintOffset = new Offset(calculatePaintOffset(constraints, from: 0.0f, to: resolvedPadding.left), resolvedPadding.top);
                    break; 
                case AxisDirection.down:
                    childParentData.paintOffset = new Offset(resolvedPadding.left, calculatePaintOffset(constraints, from: 0.0f, to: resolvedPadding.top));
                    break; 
                case AxisDirection.left:
                    childParentData.paintOffset = new Offset(calculatePaintOffset(constraints, from: resolvedPadding.right + childLayoutGeometry.scrollExtent, to: resolvedPadding.right + childLayoutGeometry.scrollExtent + resolvedPadding.left), resolvedPadding.top);
                    break;
            }
            D.assert(childParentData.paintOffset != null);
            D.assert(beforePadding == this.beforePadding);
            D.assert(afterPadding == this.afterPadding);
            D.assert(mainAxisPadding == this.mainAxisPadding);
            D.assert(crossAxisPadding == this.crossAxisPadding);
        }
        protected override bool hitTestChildren(SliverHitTestResult result,  float mainAxisPosition = 0.0f,  float crossAxisPosition  = 0.0f) { 
            if (child != null && child.geometry.hitTestExtent > 0.0) { 
                SliverPhysicalParentData childParentData = child.parentData as SliverPhysicalParentData; 
                result.addWithAxisOffset(
                    mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition,
                    mainAxisOffset: childMainAxisPosition(child) ?? 0.0f,
                    crossAxisOffset: (float)childCrossAxisPosition(child),
                    paintOffset: childParentData.paintOffset,
                    hitTest: child.hitTest
                );
            }
            return false;
        }
        public override float? childMainAxisPosition(RenderObject child) {
            child = (RenderSliver) child;
            D.assert(child != null);
            D.assert(child == this.child);
            return calculatePaintOffset(constraints, from: 0.0f, to: beforePadding ?? 0.0f);
        }
        public override float? childCrossAxisPosition(RenderObject child) {
            child = (RenderSliver) child;
            D.assert(child != null);
            D.assert(child == this.child);
            D.assert(constraints != null);
            D.assert(resolvedPadding != null);
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection, constraints.growthDirection)) {
              case AxisDirection.up:
              case AxisDirection.down:
                return resolvedPadding.left;
              case AxisDirection.left:
              case AxisDirection.right:
                return resolvedPadding.top;
            }
            return null;
        }
        public override float? childScrollOffset(RenderObject child) {
            D.assert(child.parent == this);
            return beforePadding;
        } 
        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(child == this.child);
             SliverPhysicalParentData childParentData = child.parentData as SliverPhysicalParentData;
            childParentData.applyPaintTransform(transform);
        }
        public override void paint(PaintingContext context, Offset offset) {
            if (child != null && child.geometry.visible) {
               SliverPhysicalParentData childParentData = child.parentData as SliverPhysicalParentData;
              context.paintChild(child, offset + childParentData.paintOffset);
            }
        }
        public override void debugPaint(PaintingContext context, Offset offset) {
            base.debugPaint(context, offset); 
            D.assert(()=> { 
                if (D.debugPaintSizeEnabled) { 
                    Size parentSize = getAbsoluteSize();
                    Rect outerRect = offset & parentSize;
                    Size childSize;
                    Rect innerRect = null;
                    if (child != null) {
                        childSize = child.getAbsoluteSize(); 
                        SliverPhysicalParentData childParentData = child.parentData as SliverPhysicalParentData; 
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

       
    }

    public class RenderSliverPadding : RenderSliverEdgeInsetsPadding {
        public RenderSliverPadding(
            EdgeInsetsGeometry padding = null,
            TextDirection? textDirection = null,
            RenderSliver child = null
        ) {
            D.assert(padding != null);
            D.assert(padding.isNonNegative);

            _padding = padding;
            _textDirection = textDirection ?? TextDirection.ltr;
            this.child = child;
        }

        protected override EdgeInsets resolvedPadding {
            get { return  _resolvedPadding;}
        }
        EdgeInsets _resolvedPadding;
        
        void _resolve() {
            if (resolvedPadding != null)
                return;
            _resolvedPadding = padding.resolve(textDirection);
            D.assert(resolvedPadding.isNonNegative);
        }

        void _markNeedsResolution() {
            _resolvedPadding = null;
            markNeedsLayout();
        }
        public EdgeInsetsGeometry padding {
            get { return _padding; }
            set {
                D.assert(value != null);
                D.assert(padding.isNonNegative);
                if (_padding == value) {
                    return;
                }

                _padding = value;
                markNeedsLayout();
            }
        }

        EdgeInsetsGeometry _padding;

        public TextDirection textDirection {
            get { return _textDirection;}
            set {
                if (_textDirection == value)
                    return;
                _textDirection = value;
                _markNeedsResolution();
            }
        }
        TextDirection _textDirection;

        protected override void performLayout() {
            _resolve();
            base.performLayout();
        }

       
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding));
            properties.add(new EnumProperty<TextDirection>("textDirection", textDirection, defaultValue: null));
        }

        
    }
}