using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public class RenderSliverFillViewport : RenderSliverFixedExtentBoxAdaptor {
        public RenderSliverFillViewport(
            RenderSliverBoxChildManager childManager = null,
            float viewportFraction = 1.0f
        ) :
            base(childManager: childManager) {
            D.assert(viewportFraction > 0.0);
            _viewportFraction = viewportFraction;
        }

        public override float itemExtent {
            get { return constraints.viewportMainAxisExtent * viewportFraction; }
            set { }
        }

        float _viewportFraction;

        public float viewportFraction {
            get { return _viewportFraction; }
            set {
                if (_viewportFraction == value) {
                    return;
                }

                _viewportFraction = value;
                markNeedsLayout();
            }
        }


        float _padding {
            get { return (1.0f - viewportFraction) * constraints.viewportMainAxisExtent * 0.5f; }
        }

        protected override float indexToLayoutOffset(float itemExtent, int index) {
            return _padding + base.indexToLayoutOffset(itemExtent, index);
        }

        protected override int getMinChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return base.getMinChildIndexForScrollOffset(Mathf.Max(scrollOffset - _padding, 0.0f), itemExtent);
        }

        protected override int getMaxChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return base.getMaxChildIndexForScrollOffset(Mathf.Max(scrollOffset - _padding, 0.0f), itemExtent);
        }

        protected override float estimateMaxScrollOffset(SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            float leadingScrollOffset = 0.0f,
            float trailingScrollOffset = 0.0f
        ) {
            float padding = _padding;
            return childManager.estimateMaxScrollOffset(
                       constraints,
                       firstIndex: firstIndex,
                       lastIndex: lastIndex,
                       leadingScrollOffset: leadingScrollOffset - padding,
                       trailingScrollOffset: trailingScrollOffset - padding
                   ) + padding + padding;
        }
    }

    public class RenderSliverFillRemaining : RenderSliverSingleBoxAdapter {
        public RenderSliverFillRemaining(
            RenderBox child = null
        ) : base(child: child) {
        }

        protected override void performLayout() {
            float extent = constraints.remainingPaintExtent - Mathf.Min(constraints.overlap, 0.0f);
            if (child != null) {
                child.layout(constraints.asBoxConstraints(minExtent: extent, maxExtent: extent),
                    parentUsesSize: true);
            }

            float paintedChildSize = calculatePaintOffset(constraints, from: 0.0f, to: extent);
            D.assert(paintedChildSize.isFinite());
            D.assert(paintedChildSize >= 0.0);
            geometry = new SliverGeometry(
                scrollExtent: constraints.viewportMainAxisExtent,
                paintExtent: paintedChildSize,
                maxPaintExtent: paintedChildSize,
                hasVisualOverflow: extent > constraints.remainingPaintExtent ||
                                   constraints.scrollOffset > 0.0
            );
            if (child != null) {
                setChildParentData(child, constraints, geometry);
            }
        }
    }
}