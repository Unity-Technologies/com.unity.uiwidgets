using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public enum WrapAlignment {
        start,
        end,
        center,
        spaceBetween,
        spaceAround,
        spaceEvenly
    }

    public enum WrapCrossAlignment {
        start,
        end,
        center
    }

    class _RunMetrics {
        public _RunMetrics(float mainAxisExtent, float crossAxisExtent, int childCount) {
            this.mainAxisExtent = mainAxisExtent;
            this.crossAxisExtent = crossAxisExtent;
            this.childCount = childCount;
        }

        public readonly float mainAxisExtent;
        public readonly float crossAxisExtent;
        public readonly int childCount;
    }

    public class WrapParentData : ContainerBoxParentData<RenderBox> {
        public int _runIndex = 0;
    }

    public class RenderWrap : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox, WrapParentData> {
        public RenderWrap(
            List<RenderBox> children = null,
            Axis direction = Axis.horizontal,
            WrapAlignment alignment = WrapAlignment.start,
            float spacing = 0.0f,
            WrapAlignment runAlignment = WrapAlignment.start,
            float runSpacing = 0.0f,
            WrapCrossAlignment crossAxisAlignment = WrapCrossAlignment.start,
            TextDirection? textDirection = null,
            VerticalDirection verticalDirection = VerticalDirection.down
        ) {
            _direction = direction;
            _alignment = alignment;
            _spacing = spacing;
            _runAlignment = runAlignment;
            _runSpacing = runSpacing;
            _crossAxisAlignment = crossAxisAlignment;
            _textDirection = textDirection;
            _verticalDirection = verticalDirection;

            addAll(children);
        }

        Axis _direction;

        public Axis direction {
            get { return _direction; }
            set {
                if (_direction == value) {
                    return;
                }

                _direction = value;
                markNeedsLayout();
            }
        }

        WrapAlignment _alignment;

        public WrapAlignment alignment {
            get { return _alignment; }
            set {
                if (_alignment == value) {
                    return;
                }

                _alignment = value;
                markNeedsLayout();
            }
        }

        float _spacing;

        public float spacing {
            get { return _spacing; }
            set {
                if (_spacing == value) {
                    return;
                }

                _spacing = value;
                markNeedsLayout();
            }
        }

        WrapAlignment _runAlignment;

        public WrapAlignment runAlignment {
            get { return _runAlignment; }
            set {
                if (_runAlignment == value) {
                    return;
                }

                _runAlignment = value;
                markNeedsLayout();
            }
        }

        float _runSpacing;

        public float runSpacing {
            get { return _runSpacing; }
            set {
                if (_runSpacing == value) {
                    return;
                }

                _runSpacing = value;
                markNeedsLayout();
            }
        }

        WrapCrossAlignment _crossAxisAlignment;

        public WrapCrossAlignment crossAxisAlignment {
            get { return _crossAxisAlignment; }
            set {
                if (_crossAxisAlignment == value) {
                    return;
                }

                _crossAxisAlignment = value;
                markNeedsLayout();
            }
        }

        TextDirection? _textDirection;

        public TextDirection? textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection != value) {
                    _textDirection = value;
                    markNeedsLayout();
                }
            }
        }

        VerticalDirection _verticalDirection;

        public VerticalDirection verticalDirection {
            get { return _verticalDirection; }
            set {
                if (_verticalDirection != value) {
                    _verticalDirection = value;
                    markNeedsLayout();
                }
            }
        }

        bool _debugHasNecessaryDirections {
            get {
                if (firstChild != null && lastChild != firstChild) {
                    // i.e. there"s more than one child
                    switch (direction) {
                        case Axis.horizontal:
                            D.assert(textDirection != null,
                                () => $"Horizontal {GetType()} with multiple children has a null textDirection, so the layout order is undefined.");
                            break;
                        case Axis.vertical:
                            break;
                    }
                }

                if (alignment == WrapAlignment.start || alignment == WrapAlignment.end) {
                    switch (direction) {
                        case Axis.horizontal:
                            D.assert(textDirection != null,
                                () => $"Horizontal {GetType()} with alignment {alignment} has a null textDirection, so the alignment cannot be resolved.");
                            break;
                        case Axis.vertical:
                            break;
                    }
                }

                if (runAlignment == WrapAlignment.start || runAlignment == WrapAlignment.end) {
                    switch (direction) {
                        case Axis.horizontal:
                            break;
                        case Axis.vertical:
                            D.assert(textDirection != null,
                                () => $"Vertical {GetType()} with runAlignment {runAlignment} has a null textDirection, so the alignment cannot be resolved.");
                            break;
                    }
                }

                if (crossAxisAlignment == WrapCrossAlignment.start ||
                    crossAxisAlignment == WrapCrossAlignment.end) {
                    switch (direction) {
                        case Axis.horizontal:
                            break;
                        case Axis.vertical:
                            D.assert(textDirection != null,
                                () => $"Vertical {GetType()} with crossAxisAlignment {crossAxisAlignment} has a null textDirection, so the alignment cannot be resolved.");
                            break;
                    }
                }

                return true;
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is WrapParentData)) {
                child.parentData = new WrapParentData();
            }
        }

        float _computeIntrinsicHeightForWidth(float width) {
            D.assert(direction == Axis.horizontal);
            int runCount = 0;
            float height = 0.0f;
            float runWidth = 0.0f;
            float runHeight = 0.0f;
            int childCount = 0;
            RenderBox child = firstChild;
            while (child != null) {
                float childWidth = child.getMaxIntrinsicWidth(float.PositiveInfinity);
                float childHeight = child.getMaxIntrinsicHeight(childWidth);
                if (runWidth + childWidth > width) {
                    height += runHeight;
                    if (runCount > 0) {
                        height += runSpacing;
                    }

                    runCount += 1;
                    runWidth = 0.0f;
                    runHeight = 0.0f;
                    childCount = 0;
                }

                runWidth += childWidth;
                runHeight = Mathf.Max(runHeight, childHeight);
                if (childCount > 0) {
                    runWidth += spacing;
                }

                childCount += 1;
                child = childAfter(child);
            }

            if (childCount > 0) {
                height += runHeight + runSpacing;
            }

            return height;
        }

        float _computeIntrinsicWidthForHeight(float height) {
            D.assert(direction == Axis.vertical);
            int runCount = 0;
            float width = 0.0f;
            float runHeight = 0.0f;
            float runWidth = 0.0f;
            int childCount = 0;
            RenderBox child = firstChild;
            while (child != null) {
                float childHeight = child.getMaxIntrinsicHeight(float.PositiveInfinity);
                float childWidth = child.getMaxIntrinsicWidth(childHeight);
                if (runHeight + childHeight > height) {
                    width += runWidth;
                    if (runCount > 0) {
                        width += runSpacing;
                    }

                    runCount += 1;
                    runHeight = 0.0f;
                    runWidth = 0.0f;
                    childCount = 0;
                }

                runHeight += childHeight;
                runWidth = Mathf.Max(runWidth, childWidth);
                if (childCount > 0) {
                    runHeight += spacing;
                }

                childCount += 1;
                child = childAfter(child);
            }

            if (childCount > 0) {
                width += runWidth + runSpacing;
            }

            return width;
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            switch (direction) {
                case Axis.horizontal:
                    float width = 0.0f;
                    RenderBox child = firstChild;
                    while (child != null) {
                        width = Mathf.Max(width, child.getMinIntrinsicWidth(float.PositiveInfinity));
                        child = childAfter(child);
                    }

                    return width;
                case Axis.vertical:
                    return _computeIntrinsicWidthForHeight(height);
            }

            throw new Exception("Unknown axis: " + direction);
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            switch (direction) {
                case Axis.horizontal:
                    float width = 0.0f;
                    RenderBox child = firstChild;
                    while (child != null) {
                        width += child.getMaxIntrinsicWidth(float.PositiveInfinity);
                        child = childAfter(child);
                    }

                    return width;
                case Axis.vertical:
                    return _computeIntrinsicWidthForHeight(height);
            }

            throw new Exception("Unknown axis: " + direction);
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            switch (direction) {
                case Axis.horizontal:
                    return _computeIntrinsicHeightForWidth(width);
                case Axis.vertical:
                    float height = 0.0f;
                    RenderBox child = firstChild;
                    while (child != null) {
                        height = Mathf.Max(height, child.getMinIntrinsicHeight(float.PositiveInfinity));
                        child = childAfter(child);
                    }

                    return height;
            }

            throw new Exception("Unknown axis: " + direction);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            switch (direction) {
                case Axis.horizontal:
                    return _computeIntrinsicHeightForWidth(width);
                case Axis.vertical:
                    float height = 0.0f;
                    RenderBox child = firstChild;
                    while (child != null) {
                        height += child.getMaxIntrinsicHeight(float.PositiveInfinity);
                        child = childAfter(child);
                    }

                    return height;
            }

            throw new Exception("Unknown axis: " + direction);
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return defaultComputeDistanceToHighestActualBaseline(baseline);
        }

        float _getMainAxisExtent(RenderBox child) {
            switch (direction) {
                case Axis.horizontal:
                    return child.size.width;
                case Axis.vertical:
                    return child.size.height;
            }

            return 0.0f;
        }

        float _getCrossAxisExtent(RenderBox child) {
            switch (direction) {
                case Axis.horizontal:
                    return child.size.height;
                case Axis.vertical:
                    return child.size.width;
            }

            return 0.0f;
        }

        Offset _getOffset(float mainAxisOffset, float crossAxisOffset) {
            switch (direction) {
                case Axis.horizontal:
                    return new Offset(mainAxisOffset, crossAxisOffset);
                case Axis.vertical:
                    return new Offset(crossAxisOffset, mainAxisOffset);
            }

            return Offset.zero;
        }

        float _getChildCrossAxisOffset(bool flipCrossAxis, float runCrossAxisExtent, float childCrossAxisExtent) {
            float freeSpace = runCrossAxisExtent - childCrossAxisExtent;
            switch (crossAxisAlignment) {
                case WrapCrossAlignment.start:
                    return flipCrossAxis ? freeSpace : 0.0f;
                case WrapCrossAlignment.end:
                    return flipCrossAxis ? 0.0f : freeSpace;
                case WrapCrossAlignment.center:
                    return freeSpace / 2.0f;
            }

            return 0.0f;
        }

        bool _hasVisualOverflow = false;

        protected override void performLayout() {
            D.assert(_debugHasNecessaryDirections);
            _hasVisualOverflow = false;
            RenderBox child = firstChild;
            if (child == null) {
                size = constraints.smallest;
                return;
            }

            BoxConstraints childConstraints;
            float mainAxisLimit = 0.0f;
            bool flipMainAxis = false;
            bool flipCrossAxis = false;
            switch (direction) {
                case Axis.horizontal:
                    childConstraints = new BoxConstraints(maxWidth: constraints.maxWidth);
                    mainAxisLimit = constraints.maxWidth;
                    if (textDirection == TextDirection.rtl) {
                        flipMainAxis = true;
                    }

                    if (verticalDirection == VerticalDirection.up) {
                        flipCrossAxis = true;
                    }

                    break;
                case Axis.vertical:
                    childConstraints = new BoxConstraints(maxHeight: constraints.maxHeight);
                    mainAxisLimit = constraints.maxHeight;
                    if (verticalDirection == VerticalDirection.up) {
                        flipMainAxis = true;
                    }

                    if (textDirection == TextDirection.rtl) {
                        flipCrossAxis = true;
                    }

                    break;
                default:
                    throw new Exception("Unknown axis: " + direction);
            }

            float spacing = this.spacing;
            float runSpacing = this.runSpacing;
            List<_RunMetrics> runMetrics = new List<_RunMetrics> { };
            float mainAxisExtent = 0.0f;
            float crossAxisExtent = 0.0f;
            float runMainAxisExtent = 0.0f;
            float runCrossAxisExtent = 0.0f;
            int childCount = 0;
            while (child != null) {
                child.layout(childConstraints, parentUsesSize: true);
                float childMainAxisExtent = _getMainAxisExtent(child);
                float childCrossAxisExtent = _getCrossAxisExtent(child);
                if (childCount > 0 && runMainAxisExtent + spacing + childMainAxisExtent > mainAxisLimit) {
                    mainAxisExtent = Mathf.Max(mainAxisExtent, runMainAxisExtent);
                    crossAxisExtent += runCrossAxisExtent;
                    if (runMetrics.isNotEmpty()) {
                        crossAxisExtent += runSpacing;
                    }

                    runMetrics.Add(new _RunMetrics(runMainAxisExtent, runCrossAxisExtent, childCount));
                    runMainAxisExtent = 0.0f;
                    runCrossAxisExtent = 0.0f;
                    childCount = 0;
                }

                runMainAxisExtent += childMainAxisExtent;
                if (childCount > 0) {
                    runMainAxisExtent += spacing;
                }

                runCrossAxisExtent = Mathf.Max(runCrossAxisExtent, childCrossAxisExtent);
                childCount += 1;
                D.assert(child.parentData is WrapParentData);
                WrapParentData childParentData = child.parentData as WrapParentData;
                childParentData._runIndex = runMetrics.Count;
                child = childParentData.nextSibling;
            }

            if (childCount > 0) {
                mainAxisExtent = Mathf.Max(mainAxisExtent, runMainAxisExtent);
                crossAxisExtent += runCrossAxisExtent;
                if (runMetrics.isNotEmpty()) {
                    crossAxisExtent += runSpacing;
                }

                runMetrics.Add(new _RunMetrics(runMainAxisExtent, runCrossAxisExtent, childCount));
            }

            int runCount = runMetrics.Count;
            D.assert(runCount > 0);

            float containerMainAxisExtent = 0.0f;
            float containerCrossAxisExtent = 0.0f;

            switch (direction) {
                case Axis.horizontal:
                    size = constraints.constrain(new Size(mainAxisExtent, crossAxisExtent));
                    containerMainAxisExtent = size.width;
                    containerCrossAxisExtent = size.height;
                    break;
                case Axis.vertical:
                    size = constraints.constrain(new Size(crossAxisExtent, mainAxisExtent));
                    containerMainAxisExtent = size.height;
                    containerCrossAxisExtent = size.width;
                    break;
            }

            _hasVisualOverflow =
                containerMainAxisExtent < mainAxisExtent || containerCrossAxisExtent < crossAxisExtent;

            float crossAxisFreeSpace = Mathf.Max(0.0f, containerCrossAxisExtent - crossAxisExtent);
            float runLeadingSpace = 0.0f;
            float runBetweenSpace = 0.0f;
            switch (runAlignment) {
                case WrapAlignment.start:
                    break;
                case WrapAlignment.end:
                    runLeadingSpace = crossAxisFreeSpace;
                    break;
                case WrapAlignment.center:
                    runLeadingSpace = crossAxisFreeSpace / 2.0f;
                    break;
                case WrapAlignment.spaceBetween:
                    runBetweenSpace = runCount > 1 ? crossAxisFreeSpace / (runCount - 1) : 0.0f;
                    break;
                case WrapAlignment.spaceAround:
                    runBetweenSpace = crossAxisFreeSpace / runCount;
                    runLeadingSpace = runBetweenSpace / 2.0f;
                    break;
                case WrapAlignment.spaceEvenly:
                    runBetweenSpace = crossAxisFreeSpace / (runCount + 1);
                    runLeadingSpace = runBetweenSpace;
                    break;
            }

            runBetweenSpace += runSpacing;
            float crossAxisOffset = flipCrossAxis ? containerCrossAxisExtent - runLeadingSpace : runLeadingSpace;

            child = firstChild;
            for (int i = 0; i < runCount; ++i) {
                _RunMetrics metrics = runMetrics[i];
                float runMainAxisExtent2 = metrics.mainAxisExtent;
                float runCrossAxisExtent2 = metrics.crossAxisExtent;
                float childCount2 = metrics.childCount;

                float mainAxisFreeSpace = Mathf.Max(0.0f, containerMainAxisExtent - runMainAxisExtent2);
                float childLeadingSpace = 0.0f;
                float childBetweenSpace = 0.0f;

                switch (alignment) {
                    case WrapAlignment.start:
                        break;
                    case WrapAlignment.end:
                        childLeadingSpace = mainAxisFreeSpace;
                        break;
                    case WrapAlignment.center:
                        childLeadingSpace = mainAxisFreeSpace / 2.0f;
                        break;
                    case WrapAlignment.spaceBetween:
                        childBetweenSpace = childCount2 > 1 ? mainAxisFreeSpace / (childCount2 - 1) : 0.0f;
                        break;
                    case WrapAlignment.spaceAround:
                        childBetweenSpace = mainAxisFreeSpace / childCount2;
                        childLeadingSpace = childBetweenSpace / 2.0f;
                        break;
                    case WrapAlignment.spaceEvenly:
                        childBetweenSpace = mainAxisFreeSpace / (childCount2 + 1);
                        childLeadingSpace = childBetweenSpace;
                        break;
                }

                childBetweenSpace += spacing;
                float childMainPosition =
                    flipMainAxis ? containerMainAxisExtent - childLeadingSpace : childLeadingSpace;

                if (flipCrossAxis) {
                    crossAxisOffset -= runCrossAxisExtent2;
                }

                while (child != null) {
                    D.assert(child.parentData is WrapParentData);
                    WrapParentData childParentData = child.parentData as WrapParentData;
                    if (childParentData._runIndex != i) {
                        break;
                    }

                    float childMainAxisExtent = _getMainAxisExtent(child);
                    float childCrossAxisExtent = _getCrossAxisExtent(child);
                    float childCrossAxisOffset =
                        _getChildCrossAxisOffset(flipCrossAxis, runCrossAxisExtent2, childCrossAxisExtent);
                    if (flipMainAxis) {
                        childMainPosition -= childMainAxisExtent;
                    }

                    childParentData.offset = _getOffset(childMainPosition, crossAxisOffset + childCrossAxisOffset);
                    if (flipMainAxis) {
                        childMainPosition -= childBetweenSpace;
                    }
                    else {
                        childMainPosition += childMainAxisExtent + childBetweenSpace;
                    }

                    child = childParentData.nextSibling;
                }

                if (flipCrossAxis) {
                    crossAxisOffset -= runBetweenSpace;
                }
                else {
                    crossAxisOffset += runCrossAxisExtent2 + runBetweenSpace;
                }
            }
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return defaultHitTestChildren(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_hasVisualOverflow) {
                context.pushClipRect(needsCompositing, offset, Offset.zero & size, defaultPaint);
            }
            else {
                defaultPaint(context, offset);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<Axis>("direction", direction));
            properties.add(new EnumProperty<WrapAlignment>("alignment", alignment));
            properties.add(new FloatProperty("spacing", spacing));
            properties.add(new EnumProperty<WrapAlignment>("runAlignment", runAlignment));
            properties.add(new FloatProperty("runSpacing", runSpacing));
            properties.add(new FloatProperty("crossAxisAlignment", runSpacing));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add(new EnumProperty<VerticalDirection>("verticalDirection", verticalDirection,
                defaultValue: VerticalDirection.down));
        }
    }
}