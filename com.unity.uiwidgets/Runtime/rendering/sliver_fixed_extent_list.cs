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
            if (itemExtent > 0.0) {
                float actual = scrollOffset / itemExtent;
                int round = actual.round();
                if ((actual - round).abs() < foundation_.precisionErrorTolerance) {
                    return round;
                }
                return actual.floor();
            }
            return 0;
        }

        protected virtual int getMaxChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return itemExtent > 0.0 ? Mathf.Max(0, (int) Mathf.Ceil(scrollOffset / itemExtent) - 1) : 0;
        }

        protected virtual float estimateMaxScrollOffset(
            SliverConstraints constraints,
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

        int _calculateLeadingGarbage(int firstIndex) {
            RenderBox walker = firstChild;
            int leadingGarbage = 0;
            while(walker != null && indexOf(walker) < firstIndex){
                leadingGarbage += 1;
                walker = childAfter(walker);
            }
            return leadingGarbage;
        }

        int _calculateTrailingGarbage(int targetLastIndex) {
            RenderBox walker = lastChild;
            int trailingGarbage = 0;
            while(walker != null && indexOf(walker) > targetLastIndex){
                trailingGarbage += 1;
                walker = childBefore(walker);
            }
            return trailingGarbage;
        }


        protected override void performLayout() {
            SliverConstraints constraints = this.constraints;
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
                int leadingGarbage = _calculateLeadingGarbage(firstIndex);
                int trailingGarbage = targetLastIndex == null ? 0: _calculateTrailingGarbage(targetLastIndex.Value);
                collectGarbage(leadingGarbage, trailingGarbage);
            }
            else {
                collectGarbage(0, 0);
            }

            if (firstChild == null) {
                if (!addInitialChild(index: firstIndex,
                    layoutOffset: indexToLayoutOffset(itemExtent, firstIndex))) {
                    float max;

                    if (childManager.childCount != null) {
                        max = computeMaxScrollOffset(constraints, itemExtent);
                    }
                    else if (firstIndex <= 0) {
                        max = 0.0f;
                    }
                    else {
                        // We will have to find it manually.
                        int possibleFirstIndex = firstIndex - 1;
                        while (
                            possibleFirstIndex > 0 &&
                            !addInitialChild(
                                index: possibleFirstIndex,
                                layoutOffset: indexToLayoutOffset(itemExtent, possibleFirstIndex)
                            )
                        ) {
                            possibleFirstIndex -= 1;
                        }

                        max = possibleFirstIndex * itemExtent;
                    }

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

            float estimatedMaxScrollOffset = float.PositiveInfinity;
            for (int index = indexOf(trailingChildWithLayout) + 1;
                targetLastIndex == null || index <= targetLastIndex;
                ++index) {
                RenderBox child = childAfter(trailingChildWithLayout);
                if (child == null || indexOf(child) != index) {
                    child = insertAndLayoutChild(childConstraints, after: trailingChildWithLayout);
                    if (child == null) {
                        estimatedMaxScrollOffset = index * itemExtent;
                        break;
                    }
                }
                else {
                    child.layout(childConstraints);
                }

                trailingChildWithLayout = child;
                var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                D.assert(childParentData.index == index);
                childParentData.layoutOffset = indexToLayoutOffset(itemExtent, childParentData.index);
            }

            int lastIndex = indexOf(lastChild);
            float leadingScrollOffset = indexToLayoutOffset(itemExtent, firstIndex);
            float trailingScrollOffset = indexToLayoutOffset(itemExtent, lastIndex + 1);

            D.assert(firstIndex == 0 ||
                     childScrollOffset(firstChild) - scrollOffset <= foundation_.precisionErrorTolerance);
            D.assert(debugAssertChildListIsNonEmptyAndContiguous());
            D.assert(indexOf(firstChild) == firstIndex);
            D.assert(targetLastIndex == null || lastIndex <= targetLastIndex);


            estimatedMaxScrollOffset = Mathf.Min(
                estimatedMaxScrollOffset,
                estimateMaxScrollOffset(
                    constraints,
                    firstIndex: firstIndex,
                    lastIndex: lastIndex,
                    leadingScrollOffset: leadingScrollOffset,
                    trailingScrollOffset: trailingScrollOffset
                )
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