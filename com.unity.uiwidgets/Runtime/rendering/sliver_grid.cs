using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace com.unity.uiwidgets.Runtime.rendering {
    public class SliverGridGeometry {
        public SliverGridGeometry(
            float? scrollOffset = null,
            float? crossAxisOffset = null,
            float? mainAxisExtent = null,
            float? crossAxisExtent = null
        ) {
            this.scrollOffset = scrollOffset;
            this.crossAxisOffset = crossAxisOffset;
            this.mainAxisExtent = mainAxisExtent;
            this.crossAxisExtent = crossAxisExtent;
        }

        public readonly float? scrollOffset;

        public readonly float? crossAxisOffset;

        public readonly float? mainAxisExtent;

        public readonly float? crossAxisExtent;

        public float? trailingScrollOffset {
            get { return scrollOffset + mainAxisExtent; }
        }

        public BoxConstraints getBoxConstraints(SliverConstraints constraints) {
            return constraints.asBoxConstraints(
                minExtent: mainAxisExtent ?? 0.0f,
                maxExtent: mainAxisExtent ?? 0.0f,
                crossAxisExtent: crossAxisExtent ?? 0.0f
            );
        }

        public override string ToString() {
            List<string> properties = new List<string>();
            properties.Add("scrollOffset: $scrollOffset");
            properties.Add("crossAxisOffset: $crossAxisOffset");
            properties.Add("mainAxisExtent: $mainAxisExtent");
            properties.Add("crossAxisExtent: $crossAxisExtent");
            return $"SliverGridGeometry({string.Join(", ",properties)})";
        }
    }

    public abstract class SliverGridLayout {
        public SliverGridLayout() {
        }

        public abstract int getMinChildIndexForScrollOffset(float scrollOffset);

        public abstract int getMaxChildIndexForScrollOffset(float scrollOffset);

        public abstract SliverGridGeometry getGeometryForChildIndex(int index);

        public abstract float computeMaxScrollOffset(int childCount);
    }

    public class SliverGridRegularTileLayout : SliverGridLayout {
        public SliverGridRegularTileLayout(
            int? crossAxisCount = null,
            float? mainAxisStride = null,
            float? crossAxisStride = null,
            float? childMainAxisExtent = null,
            float? childCrossAxisExtent = null,
            bool? reverseCrossAxis = null
        ) {
            D.assert(crossAxisCount > 0);
            D.assert(mainAxisStride >= 0);
            D.assert(crossAxisStride >= 0);
            D.assert(childMainAxisExtent >= 0);
            D.assert(childCrossAxisExtent >= 0);
            D.assert(reverseCrossAxis != null);
            this.crossAxisCount = crossAxisCount;
            this.mainAxisStride = mainAxisStride;
            this.crossAxisStride = crossAxisStride;
            this.childMainAxisExtent = childMainAxisExtent;
            this.childCrossAxisExtent = childCrossAxisExtent;
            this.reverseCrossAxis = reverseCrossAxis;
        }

        public readonly int? crossAxisCount;

        public readonly float? mainAxisStride;

        public readonly float? crossAxisStride;

        public readonly float? childMainAxisExtent;

        public readonly float? childCrossAxisExtent;

        public readonly bool? reverseCrossAxis;

        public override int getMinChildIndexForScrollOffset(float scrollOffset) {
            return (mainAxisStride > 0.0f
                       ? crossAxisCount * ((int) (scrollOffset / mainAxisStride))
                       : 0) ?? 0;
        }

        public override int getMaxChildIndexForScrollOffset(float scrollOffset) {
            if (mainAxisStride > 0.0f) {
                int? mainAxisCount = (scrollOffset / mainAxisStride)?.ceil();
                return Mathf.Max(0, (crossAxisCount * mainAxisCount - 1) ?? 0);
            }

            return 0;
        }

        float _getOffsetFromStartInCrossAxis(float crossAxisStart) {
            if (reverseCrossAxis == true) {
                return (crossAxisCount * crossAxisStride - crossAxisStart - childCrossAxisExtent
                        - (crossAxisStride - childCrossAxisExtent)) ??
                       0.0f;
            }

            return crossAxisStart;
        }

        public override SliverGridGeometry getGeometryForChildIndex(int index) {
            float? crossAxisStart = (index % crossAxisCount) * crossAxisStride;
            return new SliverGridGeometry(
                scrollOffset: (index / crossAxisCount) * mainAxisStride,
                crossAxisOffset:
                _getOffsetFromStartInCrossAxis(crossAxisStart ?? 0.0f),
                mainAxisExtent:
                childMainAxisExtent,
                crossAxisExtent:
                childCrossAxisExtent
            );
        }

        public override float computeMaxScrollOffset(int childCount) {
            int? mainAxisCount = ((childCount - 1) / crossAxisCount) + 1;
            float? mainAxisSpacing = mainAxisStride - childMainAxisExtent;
            return (mainAxisStride * mainAxisCount - mainAxisSpacing) ?? 0.0f;
        }
    }

    public abstract class SliverGridDelegate {

        protected SliverGridDelegate() { }

        public abstract SliverGridLayout getLayout(SliverConstraints constraints);

        public abstract bool shouldRelayout(SliverGridDelegate oldDelegate);
    }

    public class SliverGridDelegateWithFixedCrossAxisCount : SliverGridDelegate {
        public SliverGridDelegateWithFixedCrossAxisCount(
            int crossAxisCount,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f
        ) {
            D.assert(crossAxisCount > 0);
            D.assert(mainAxisSpacing >= 0);
            D.assert(crossAxisSpacing >= 0);
            D.assert(childAspectRatio > 0);
            this.crossAxisCount = crossAxisCount;
            this.mainAxisSpacing = mainAxisSpacing;
            this.crossAxisSpacing = crossAxisSpacing;
            this.childAspectRatio = childAspectRatio;
        }

        public readonly int crossAxisCount;

        public readonly float mainAxisSpacing;

        public readonly float crossAxisSpacing;

        public readonly float childAspectRatio;

        bool _debugAssertIsValid() {
            D.assert(crossAxisCount > 0);
            D.assert(mainAxisSpacing >= 0.0f);
            D.assert(crossAxisSpacing >= 0.0f);
            D.assert(childAspectRatio > 0.0f);
            return true;
        }

        public override SliverGridLayout getLayout(SliverConstraints constraints) {
            D.assert(_debugAssertIsValid());
            float usableCrossAxisExtent =
                constraints.crossAxisExtent - crossAxisSpacing * (crossAxisCount - 1);
            float childCrossAxisExtent = usableCrossAxisExtent / crossAxisCount;
            float childMainAxisExtent = childCrossAxisExtent / childAspectRatio;
            return new SliverGridRegularTileLayout(
                crossAxisCount: crossAxisCount,
                mainAxisStride: childMainAxisExtent + mainAxisSpacing,
                crossAxisStride: childCrossAxisExtent + crossAxisSpacing,
                childMainAxisExtent: childMainAxisExtent,
                childCrossAxisExtent: childCrossAxisExtent,
                reverseCrossAxis: AxisUtils.axisDirectionIsReversed(constraints.crossAxisDirection)
            );
        }

        public override bool shouldRelayout(SliverGridDelegate _oldDelegate) {
            SliverGridDelegateWithFixedCrossAxisCount oldDelegate =
                _oldDelegate as SliverGridDelegateWithFixedCrossAxisCount;
            return oldDelegate.crossAxisCount != crossAxisCount
                   || oldDelegate.mainAxisSpacing != mainAxisSpacing
                   || oldDelegate.crossAxisSpacing != crossAxisSpacing
                   || oldDelegate.childAspectRatio != childAspectRatio;
        }
    }

    public class SliverGridDelegateWithMaxCrossAxisExtent : SliverGridDelegate {
        public SliverGridDelegateWithMaxCrossAxisExtent(
            float maxCrossAxisExtent,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f
        ) {
            D.assert(maxCrossAxisExtent >= 0);
            D.assert(mainAxisSpacing >= 0);
            D.assert(crossAxisSpacing >= 0);
            D.assert(childAspectRatio > 0);
            this.maxCrossAxisExtent = maxCrossAxisExtent;
            this.mainAxisSpacing = mainAxisSpacing;
            this.crossAxisSpacing = crossAxisSpacing;
            this.childAspectRatio = childAspectRatio;
        }

        public readonly float maxCrossAxisExtent;

        public readonly float mainAxisSpacing;

        public readonly float crossAxisSpacing;

        public readonly float childAspectRatio;

        bool _debugAssertIsValid() {
            D.assert(maxCrossAxisExtent > 0.0f);
            D.assert(mainAxisSpacing >= 0.0f);
            D.assert(crossAxisSpacing >= 0.0f);
            D.assert(childAspectRatio > 0.0f);
            return true;
        }

        public override SliverGridLayout getLayout(SliverConstraints constraints) {
            D.assert(_debugAssertIsValid());
            int crossAxisCount =
                (constraints.crossAxisExtent / (maxCrossAxisExtent + crossAxisSpacing)).ceil();
            float usableCrossAxisExtent = constraints.crossAxisExtent - crossAxisSpacing * (crossAxisCount - 1);
            float childCrossAxisExtent = usableCrossAxisExtent / crossAxisCount;
            float childMainAxisExtent = childCrossAxisExtent / childAspectRatio;
            return new SliverGridRegularTileLayout(
                crossAxisCount: crossAxisCount,
                mainAxisStride: childMainAxisExtent + mainAxisSpacing,
                crossAxisStride: childCrossAxisExtent + crossAxisSpacing,
                childMainAxisExtent: childMainAxisExtent,
                childCrossAxisExtent: childCrossAxisExtent,
                reverseCrossAxis: AxisUtils.axisDirectionIsReversed(constraints.crossAxisDirection)
            );
        }

        public override bool shouldRelayout(SliverGridDelegate _oldDelegate) {
            SliverGridDelegateWithMaxCrossAxisExtent oldDelegate =
                _oldDelegate as SliverGridDelegateWithMaxCrossAxisExtent;
            return oldDelegate.maxCrossAxisExtent != maxCrossAxisExtent
                   || oldDelegate.mainAxisSpacing != mainAxisSpacing
                   || oldDelegate.crossAxisSpacing != crossAxisSpacing
                   || oldDelegate.childAspectRatio != childAspectRatio;
        }
    }

    public class SliverGridParentData : SliverMultiBoxAdaptorParentData {
        public float crossAxisOffset;

        public override string ToString() {
            return $"crossAxisOffset={crossAxisOffset}; {base.ToString()}";
        }
    }

    public class RenderSliverGrid : RenderSliverMultiBoxAdaptor {
        public RenderSliverGrid(
            RenderSliverBoxChildManager childManager,
            SliverGridDelegate gridDelegate
        ) : base(childManager: childManager) {
            D.assert(gridDelegate != null);
            _gridDelegate = gridDelegate;
        }


        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverGridParentData)) {
                child.parentData = new SliverGridParentData();
            }
        }

        public SliverGridDelegate gridDelegate {
            get { return _gridDelegate; }
            set {
                D.assert(value != null);
                if (_gridDelegate == value) {
                    return;
                }

                if (value.GetType() != _gridDelegate.GetType() ||
                    value.shouldRelayout(_gridDelegate)) {
                    markNeedsLayout();
                }

                _gridDelegate = value;
            }
        }

        SliverGridDelegate _gridDelegate;

        public override float? childCrossAxisPosition(RenderObject child) {
            SliverGridParentData childParentData = (SliverGridParentData) child.parentData;
            return childParentData.crossAxisOffset;
        }

        protected override void performLayout() {
            childManager.didStartLayout();
            childManager.setDidUnderflow(false);

            float scrollOffset = constraints.scrollOffset + constraints.cacheOrigin;
            D.assert(scrollOffset >= 0.0f);
            float remainingExtent = constraints.remainingCacheExtent;
            D.assert(remainingExtent >= 0.0f);
            float targetEndScrollOffset = scrollOffset + remainingExtent;

            SliverGridLayout layout = _gridDelegate.getLayout(constraints);

            int firstIndex = layout.getMinChildIndexForScrollOffset(scrollOffset);
            int? targetLastIndex = targetEndScrollOffset.isFinite()
                ? (int?) layout.getMaxChildIndexForScrollOffset(targetEndScrollOffset)
                : null;

            if (firstChild != null) {
                int oldFirstIndex = indexOf(firstChild);
                int oldLastIndex = indexOf(lastChild);
                int leadingGarbage = (firstIndex - oldFirstIndex).clamp(0, childCount);
                int trailingGarbage = targetLastIndex == null
                    ? 0
                    : ((oldLastIndex - targetLastIndex) ?? 0).clamp(0, childCount);
                collectGarbage(leadingGarbage, trailingGarbage);
            }
            else {
                collectGarbage(0, 0);
            }

            SliverGridGeometry firstChildGridGeometry = layout.getGeometryForChildIndex(firstIndex);
            float? leadingScrollOffset = firstChildGridGeometry.scrollOffset;
            float? trailingScrollOffset = firstChildGridGeometry.trailingScrollOffset;

            if (firstChild == null) {
                if (!addInitialChild(index: firstIndex,
                    layoutOffset: firstChildGridGeometry.scrollOffset ?? 0.0f)) {
                    float max = layout.computeMaxScrollOffset(childManager.childCount ?? 0);
                    geometry = new SliverGeometry(
                        scrollExtent: max,
                        maxPaintExtent: max
                    );
                    childManager.didFinishLayout();
                    return;
                }
            }

            RenderBox trailingChildWithLayout = null;

            for (int index = indexOf(firstChild) - 1; index >= firstIndex; --index) {
                SliverGridGeometry gridGeometry = layout.getGeometryForChildIndex(index);
                RenderBox child = insertAndLayoutLeadingChild(
                    gridGeometry.getBoxConstraints(constraints)
                );
                SliverGridParentData childParentData = child.parentData as SliverGridParentData;
                childParentData.layoutOffset = gridGeometry.scrollOffset ?? 0.0f;
                childParentData.crossAxisOffset = gridGeometry.crossAxisOffset ?? 0.0f;
                D.assert(childParentData.index == index);
                trailingChildWithLayout = trailingChildWithLayout ?? child;
                trailingScrollOffset =
                    Mathf.Max(trailingScrollOffset ?? 0.0f, gridGeometry.trailingScrollOffset ?? 0.0f);
            }

            if (trailingChildWithLayout == null) {
                firstChild.layout(firstChildGridGeometry.getBoxConstraints(constraints));
                SliverGridParentData childParentData = firstChild.parentData as SliverGridParentData;
                childParentData.layoutOffset = firstChildGridGeometry.scrollOffset ?? 0.0f;
                childParentData.crossAxisOffset = firstChildGridGeometry.crossAxisOffset ?? 0.0f;
                trailingChildWithLayout = firstChild;
            }

            for (int index = indexOf(trailingChildWithLayout) + 1;
                targetLastIndex == null || index <= targetLastIndex;
                ++index) {
                SliverGridGeometry gridGeometry = layout.getGeometryForChildIndex(index);
                BoxConstraints childConstraints = gridGeometry.getBoxConstraints(constraints);
                RenderBox child = childAfter(trailingChildWithLayout);
                if (child == null || indexOf(child) != index) {
                    child = insertAndLayoutChild(childConstraints, after: trailingChildWithLayout);
                    if (child == null) {
                        break;
                    }
                }
                else {
                    child.layout(childConstraints);
                }

                trailingChildWithLayout = child;
                SliverGridParentData childParentData = child.parentData as SliverGridParentData;
                childParentData.layoutOffset = gridGeometry.scrollOffset ?? 0.0f;
                childParentData.crossAxisOffset = gridGeometry.crossAxisOffset ?? 0.0f;
                D.assert(childParentData.index == index);
                trailingScrollOffset =
                    Mathf.Max(trailingScrollOffset ?? 0.0f, gridGeometry.trailingScrollOffset ?? 0.0f);
            }

            int lastIndex = indexOf(lastChild);

            D.assert(childScrollOffset(firstChild) <= scrollOffset);
            D.assert(debugAssertChildListIsNonEmptyAndContiguous());
            D.assert(indexOf(firstChild) == firstIndex);
            D.assert(targetLastIndex == null || lastIndex <= targetLastIndex);

            float estimatedTotalExtent = childManager.estimateMaxScrollOffset(
                constraints,
                firstIndex: firstIndex,
                lastIndex: lastIndex,
                leadingScrollOffset: leadingScrollOffset ?? 0.0f,
                trailingScrollOffset: trailingScrollOffset ?? 0.0f
            );

            float paintExtent = calculatePaintOffset(
                constraints,
                from: leadingScrollOffset ?? 0.0f,
                to: trailingScrollOffset ?? 0.0f
            );
            float cacheExtent = calculateCacheOffset(
                constraints,
                from: leadingScrollOffset ?? 0.0f,
                to: trailingScrollOffset ?? 0.0f
            );

            geometry = new SliverGeometry(
                scrollExtent: estimatedTotalExtent,
                paintExtent: paintExtent,
                maxPaintExtent: estimatedTotalExtent,
                cacheExtent: cacheExtent,
                hasVisualOverflow: true
            );

            if (estimatedTotalExtent == trailingScrollOffset) {
                childManager.setDidUnderflow(true);
            }

            childManager.didFinishLayout();
        }
    }
}