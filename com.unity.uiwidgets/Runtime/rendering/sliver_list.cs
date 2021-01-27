using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.rendering {
    public class RenderSliverList : RenderSliverMultiBoxAdaptor {
        public RenderSliverList(
            RenderSliverBoxChildManager childManager = null
        ) : base(childManager: childManager) {
        }

        protected override void performLayout() {
            childManager.didStartLayout();
            childManager.setDidUnderflow(false);

            float scrollOffset = constraints.scrollOffset + constraints.cacheOrigin;
            D.assert(scrollOffset >= 0.0);
            float remainingExtent = constraints.remainingCacheExtent;
            D.assert(remainingExtent >= 0.0);
            float targetEndScrollOffset = scrollOffset + remainingExtent;
            BoxConstraints childConstraints = constraints.asBoxConstraints();
            int leadingGarbage = 0;
            int trailingGarbage = 0;
            bool reachedEnd = false;

            if (firstChild == null) {
                if (!addInitialChild()) {
                    geometry = SliverGeometry.zero;
                    childManager.didFinishLayout();
                    return;
                }
            }

            RenderBox leadingChildWithLayout = null, trailingChildWithLayout = null;

            RenderBox earliestUsefulChild = firstChild;
            for (float earliestScrollOffset = childScrollOffset(earliestUsefulChild) ?? 0.0f;
                earliestScrollOffset > scrollOffset;
                earliestScrollOffset = childScrollOffset(earliestUsefulChild)?? 0.0f) {
                earliestUsefulChild = insertAndLayoutLeadingChild(childConstraints, parentUsesSize: true);

                if (earliestUsefulChild == null) {
                    var childParentData = (SliverMultiBoxAdaptorParentData) firstChild.parentData;
                    childParentData.layoutOffset = 0.0f;

                    if (scrollOffset == 0.0) {
                        earliestUsefulChild = firstChild;
                        leadingChildWithLayout = earliestUsefulChild;
                        trailingChildWithLayout = trailingChildWithLayout ?? earliestUsefulChild;
                        break;
                    }
                    else {
                        geometry = new SliverGeometry(
                            scrollOffsetCorrection: -scrollOffset
                        );
                        return;
                    }
                }
                else {
                    float firstChildScrollOffset = earliestScrollOffset - paintExtentOf(firstChild);
                    if (firstChildScrollOffset < -foundation_.precisionErrorTolerance) {
                        float correction = 0.0f;
                        while (earliestUsefulChild != null) {
                            D.assert(firstChild == earliestUsefulChild);
                            correction += paintExtentOf(firstChild);
                            earliestUsefulChild =
                                insertAndLayoutLeadingChild(childConstraints, parentUsesSize: true);
                        }

                        geometry = new SliverGeometry(
                            scrollOffsetCorrection: correction - earliestScrollOffset
                        );
                        var childParentData = (SliverMultiBoxAdaptorParentData) firstChild.parentData;
                        childParentData.layoutOffset = 0.0f;
                        return;
                    }
                    else {
                        var childParentData = (SliverMultiBoxAdaptorParentData) earliestUsefulChild.parentData;
                        childParentData.layoutOffset = firstChildScrollOffset;
                        D.assert(earliestUsefulChild == firstChild);
                        leadingChildWithLayout = earliestUsefulChild;
                        trailingChildWithLayout = trailingChildWithLayout ?? earliestUsefulChild;
                    }
                }
            }

            D.assert(earliestUsefulChild == firstChild);
            D.assert(childScrollOffset(earliestUsefulChild) <= scrollOffset);

            if (leadingChildWithLayout == null) {
                earliestUsefulChild.layout(childConstraints, parentUsesSize: true);
                leadingChildWithLayout = earliestUsefulChild;
                trailingChildWithLayout = earliestUsefulChild;
            }

            bool inLayoutRange = true;
            RenderBox child = earliestUsefulChild;
            int index = indexOf(child);
            float endScrollOffset = childScrollOffset(child) + paintExtentOf(child) ?? 0.0f;

            Func<bool> advance = () => {
                D.assert(child != null);
                if (child == trailingChildWithLayout) {
                    inLayoutRange = false;
                }

                child = childAfter(child);
                if (child == null) {
                    inLayoutRange = false;
                }

                index += 1;
                if (!inLayoutRange) {
                    if (child == null || indexOf(child) != index) {
                        child = insertAndLayoutChild(childConstraints,
                            after: trailingChildWithLayout,
                            parentUsesSize: true
                        );
                        if (child == null) {
                            return false;
                        }
                    }
                    else {
                        child.layout(childConstraints, parentUsesSize: true);
                    }

                    trailingChildWithLayout = child;
                }

                D.assert(child != null);
                var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                childParentData.layoutOffset = endScrollOffset;
                D.assert(childParentData.index == index);
                endScrollOffset = childScrollOffset(child) + paintExtentOf(child) ?? 0.0f ;
                return true;
            };

            while (endScrollOffset < scrollOffset) {
                leadingGarbage += 1;
                if (!advance()) {
                    D.assert(leadingGarbage == childCount);
                    D.assert(child == null);

                    collectGarbage(leadingGarbage - 1, 0);
                    D.assert(firstChild == lastChild);
                    float extent = childScrollOffset(lastChild) + paintExtentOf(lastChild) ?? 0.0f;
                    geometry = new SliverGeometry(
                        scrollExtent: extent,
                        paintExtent: 0.0f,
                        maxPaintExtent: extent
                    );
                    return;
                }
            }

            while (endScrollOffset < targetEndScrollOffset) {
                if (!advance()) {
                    reachedEnd = true;
                    break;
                }
            }

            if (child != null) {
                child = childAfter(child);
                while (child != null) {
                    trailingGarbage += 1;
                    child = childAfter(child);
                }
            }

            collectGarbage(leadingGarbage, trailingGarbage);

            D.assert(debugAssertChildListIsNonEmptyAndContiguous());

            float? estimatedMaxScrollOffset;
            if (reachedEnd) {
                estimatedMaxScrollOffset = endScrollOffset;
            }
            else {
                estimatedMaxScrollOffset = childManager.estimateMaxScrollOffset(
                    constraints,
                    firstIndex: indexOf(firstChild),
                    lastIndex: indexOf(lastChild),
                    leadingScrollOffset: childScrollOffset(firstChild) ?? 0.0f,
                    trailingScrollOffset: endScrollOffset
                );

                D.assert(estimatedMaxScrollOffset >= endScrollOffset - childScrollOffset(firstChild));
            }

            float paintExtent = calculatePaintOffset(
                constraints,
                from: childScrollOffset(firstChild) ?? 0.0f,
                to: endScrollOffset
            );
            float cacheExtent = calculateCacheOffset(
                constraints,
                from: childScrollOffset(firstChild) ?? 0.0f,
                to: endScrollOffset
            );
            float targetEndScrollOffsetForPaint =
                constraints.scrollOffset + constraints.remainingPaintExtent;
            geometry = new SliverGeometry(
                scrollExtent: estimatedMaxScrollOffset.Value,
                paintExtent: paintExtent,
                cacheExtent: cacheExtent,
                maxPaintExtent: estimatedMaxScrollOffset.Value,
                hasVisualOverflow: endScrollOffset > targetEndScrollOffsetForPaint ||
                                   constraints.scrollOffset > 0.0
            );

            if (estimatedMaxScrollOffset == endScrollOffset) {
                childManager.setDidUnderflow(true);
            }

            childManager.didFinishLayout();
        }
    }
}