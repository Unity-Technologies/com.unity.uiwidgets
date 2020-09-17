using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public abstract class RenderSliverFixedExtentBoxAdaptor : RenderSliverMultiBoxAdaptor {
        protected RenderSliverFixedExtentBoxAdaptor(
            RenderSliverBoxChildManager childManager = null
        ) : base(childManager: childManager) {
        }

        public abstract float itemExtent { get; set; }

        protected virtual float indexToLayoutOffset(float itemExtent, int index) {
            return itemExtent * index;
        }

        protected virtual int getMinChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return itemExtent > 0.0 ? Mathf.Max(0, (int) (scrollOffset / itemExtent)) : 0;
        }

        protected virtual int getMaxChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return itemExtent > 0.0 ? Mathf.Max(0, (int) Mathf.Ceil(scrollOffset / itemExtent) - 1) : 0;
        }

        protected virtual float estimateMaxScrollOffset(SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            float leadingScrollOffset = 0.0f,
            float trailingScrollOffset = 0.0f
        ) {
            return childManager.estimateMaxScrollOffset(
                constraints,
                firstIndex: firstIndex,
                lastIndex: lastIndex,
                leadingScrollOffset: leadingScrollOffset,
                trailingScrollOffset: trailingScrollOffset
            );
        }

        protected float computeMaxScrollOffset(SliverConstraints constraints, float itemExtent) {
            return childManager.childCount.Value * itemExtent;
        }

        protected override void performLayout() {
            childManager.didStartLayout();
            childManager.setDidUnderflow(false);

            float itemExtent = this.itemExtent;

            float scrollOffset = constraints.scrollOffset + constraints.cacheOrigin;
            D.assert(scrollOffset >= 0.0);
            float remainingExtent = constraints.remainingCacheExtent;
            D.assert(remainingExtent >= 0.0);
            float targetEndScrollOffset = scrollOffset + remainingExtent;

            BoxConstraints childConstraints = constraints.asBoxConstraints(
                minExtent: itemExtent,
                maxExtent: itemExtent
            );

            int firstIndex = getMinChildIndexForScrollOffset(scrollOffset, itemExtent);
            int? targetLastIndex = targetEndScrollOffset.isFinite()
                ? getMaxChildIndexForScrollOffset(targetEndScrollOffset, itemExtent)
                : (int?) null;

            if (firstChild != null) {
                int oldFirstIndex = indexOf(firstChild);
                int oldLastIndex = indexOf(lastChild);
                int leadingGarbage = (firstIndex - oldFirstIndex).clamp(0, childCount);
                int trailingGarbage =
                    targetLastIndex == null ? 0 : (oldLastIndex - targetLastIndex.Value).clamp(0, childCount);
                collectGarbage(leadingGarbage, trailingGarbage);
            }
            else {
                collectGarbage(0, 0);
            }

            if (firstChild == null) {
                if (!addInitialChild(index: firstIndex,
                    layoutOffset: indexToLayoutOffset(itemExtent, firstIndex))) {
                    float max = computeMaxScrollOffset(constraints, itemExtent);
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
                RenderBox child = insertAndLayoutLeadingChild(childConstraints);
                if (child == null) {
                    geometry = new SliverGeometry(scrollOffsetCorrection: index * itemExtent);
                    return;
                }

                var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                childParentData.layoutOffset = indexToLayoutOffset(itemExtent, index);
                D.assert(childParentData.index == index);
                trailingChildWithLayout = trailingChildWithLayout ?? child;
            }

            if (trailingChildWithLayout == null) {
                firstChild.layout(childConstraints);
                var childParentData = (SliverMultiBoxAdaptorParentData) firstChild.parentData;
                childParentData.layoutOffset = indexToLayoutOffset(itemExtent, firstIndex);
                trailingChildWithLayout = firstChild;
            }

            while (targetLastIndex == null || indexOf(trailingChildWithLayout) < targetLastIndex) {
                RenderBox child = childAfter(trailingChildWithLayout);
                if (child == null) {
                    child = insertAndLayoutChild(childConstraints, after: trailingChildWithLayout);
                    if (child == null) {
                        break;
                    }
                }
                else {
                    child.layout(childConstraints);
                }

                trailingChildWithLayout = child;
                D.assert(child != null);
                var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                childParentData.layoutOffset = indexToLayoutOffset(itemExtent, childParentData.index);
            }

            int lastIndex = indexOf(lastChild);
            float leadingScrollOffset = indexToLayoutOffset(itemExtent, firstIndex);
            float trailingScrollOffset = indexToLayoutOffset(itemExtent, lastIndex + 1);

            D.assert(firstIndex == 0 || childScrollOffset(firstChild) <= scrollOffset);
            D.assert(debugAssertChildListIsNonEmptyAndContiguous());
            D.assert(indexOf(firstChild) == firstIndex);
            D.assert(targetLastIndex == null || lastIndex <= targetLastIndex);


            float estimatedMaxScrollOffset = estimateMaxScrollOffset(
                constraints,
                firstIndex: firstIndex,
                lastIndex: lastIndex,
                leadingScrollOffset: leadingScrollOffset,
                trailingScrollOffset: trailingScrollOffset
            );

            float paintExtent = calculatePaintOffset(
                constraints,
                from: leadingScrollOffset,
                to: trailingScrollOffset
            );

            float cacheExtent = calculateCacheOffset(
                constraints,
                from: leadingScrollOffset,
                to: trailingScrollOffset
            );

            float targetEndScrollOffsetForPaint =
                constraints.scrollOffset + constraints.remainingPaintExtent;
            int? targetLastIndexForPaint = targetEndScrollOffsetForPaint.isFinite()
                ? getMaxChildIndexForScrollOffset(targetEndScrollOffsetForPaint, itemExtent)
                : (int?) null;
            geometry = new SliverGeometry(
                scrollExtent: estimatedMaxScrollOffset,
                paintExtent: paintExtent,
                cacheExtent: cacheExtent,
                maxPaintExtent: estimatedMaxScrollOffset,
                hasVisualOverflow: (targetLastIndexForPaint != null && lastIndex >= targetLastIndexForPaint)
                                   || constraints.scrollOffset > 0.0
            );

            if (estimatedMaxScrollOffset == trailingScrollOffset) {
                childManager.setDidUnderflow(true);
            }

            childManager.didFinishLayout();
        }
    }

    public class RenderSliverFixedExtentList : RenderSliverFixedExtentBoxAdaptor {
        public RenderSliverFixedExtentList(
            RenderSliverBoxChildManager childManager = null,
            float itemExtent = 0.0f
        ) : base(childManager: childManager) {
            _itemExtent = itemExtent;
        }

        public override float itemExtent {
            get { return _itemExtent; }
            set {
                if (_itemExtent == value) {
                    return;
                }

                _itemExtent = value;
                markNeedsLayout();
            }
        }

        float _itemExtent;
    }
}