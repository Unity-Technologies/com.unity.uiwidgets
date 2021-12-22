using System;
using System.Collections.Generic;
using UIWidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public enum FlexFit {
        tight,
        loose,
    }

    public class FlexParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
        public int flex;

        public FlexFit fit;

        public override string ToString() => $"{base.ToString()}; flex={flex}; fit={fit}";
    }

    public enum MainAxisSize {
        min,
        max,
    }

    public enum MainAxisAlignment {
        start,
        end,
        center,
        spaceBetween,
        spaceAround,
        spaceEvenly,
    }

    public enum CrossAxisAlignment {
        start,
        end,
        center,
        stretch,
        baseline,
    }

    public delegate float _ChildSizingFunction(RenderBox child, float extent);

    public class RenderFlex : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        FlexParentData> {
        public RenderFlex(
            List<RenderBox> children = null,
            Axis direction = Axis.horizontal,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            TextDirection textDirection = TextDirection.ltr,
            VerticalDirection verticalDirection = VerticalDirection.down,
            TextBaseline textBaseline = TextBaseline.alphabetic
        ) {
            _direction = direction;
            _mainAxisAlignment = mainAxisAlignment;
            _mainAxisSize = mainAxisSize;
            _crossAxisAlignment = crossAxisAlignment;
            _textDirection = textDirection;
            _verticalDirection = verticalDirection;
            _textBaseline = textBaseline;

            addAll(children);
        }

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

        public Axis _direction;

        public MainAxisSize mainAxisSize {
            get { return _mainAxisSize; }
            set {
                if (_mainAxisSize == value) {
                    return;
                }

                _mainAxisSize = value;
                markNeedsLayout();
            }
        }

        public MainAxisSize _mainAxisSize;

        public MainAxisAlignment mainAxisAlignment {
            get { return _mainAxisAlignment; }
            set {
                if (_mainAxisAlignment == value) {
                    return;
                }

                _mainAxisAlignment = value;
                markNeedsLayout();
            }
        }

        public MainAxisAlignment _mainAxisAlignment;

        public CrossAxisAlignment crossAxisAlignment {
            get { return _crossAxisAlignment; }
            set {
                if (_crossAxisAlignment == value) {
                    return;
                }

                _crossAxisAlignment = value;
                markNeedsLayout();
            }
        }

        public CrossAxisAlignment _crossAxisAlignment;

        public TextDirection? textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value) {
                    return;
                }

                _textDirection = value;
                markNeedsLayout();
            }
        }

        public TextDirection? _textDirection;

        public VerticalDirection? verticalDirection {
            get { return _verticalDirection; }
            set {
                if (_verticalDirection == value) {
                    return;
                }

                _verticalDirection = value;
                markNeedsLayout();
            }
        }

        public VerticalDirection? _verticalDirection;

        public TextBaseline textBaseline {
            get { return _textBaseline; }
            set {
                if (_textBaseline == value) {
                    return;
                }

                _textBaseline = value;
                markNeedsLayout();
            }
        }

        public TextBaseline _textBaseline;

        bool _debugHasNecessaryDirections {
            get  {
                if (firstChild != null && lastChild != firstChild) {
                    // i.e. there's more than one child
                    switch (direction) {
                        case Axis.horizontal:
                            D.assert(textDirection != null, () => $"Horizontal {GetType()} with multiple children has a null textDirection, so the layout order is undefined.");
                            break;
                        case Axis.vertical:
                            D.assert(verticalDirection != null, () => $"Vertical {GetType()} with multiple children has a null verticalDirection, so the layout order is undefined.");
                            break;
                    }
                }
                if (mainAxisAlignment == MainAxisAlignment.start ||
                mainAxisAlignment == MainAxisAlignment.end) {
                    switch (direction) {
                        case Axis.horizontal:
                            D.assert(textDirection != null, () => $"Horizontal {GetType()} with {mainAxisAlignment} has a null textDirection, so the alignment cannot be resolved.");
                            break;
                        case Axis.vertical:
                            D.assert(verticalDirection != null, () => $"Vertical {GetType()} with {mainAxisAlignment} has a null verticalDirection, so the alignment cannot be resolved.");
                            break;
                    }
                }
                if (crossAxisAlignment == CrossAxisAlignment.start ||
                crossAxisAlignment == CrossAxisAlignment.end) {
                    switch (direction) {
                        case Axis.horizontal:
                            D.assert(verticalDirection != null, () => $"Horizontal {GetType()} with {crossAxisAlignment} has a null verticalDirection, so the alignment cannot be resolved.");
                            break;
                        case Axis.vertical:
                            D.assert(textDirection != null, () => $"Vertical {GetType()} with {crossAxisAlignment} has a null textDirection, so the alignment cannot be resolved.");
                            break;
                    }
                }
                return true;
            }
        }
        
        
        
        public float _overflow;
        
        bool _hasOverflow {
            get {
                return _overflow > foundation_.precisionErrorTolerance;
            }
        } 

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is FlexParentData)) {
                child.parentData = new FlexParentData();
            }
        }

        public float _getIntrinsicSize(
            Axis sizingDirection,
            float extent,
            _ChildSizingFunction childSize
        ) {
            if (_direction == sizingDirection) {
                float totalFlex = 0.0f;
                float inflexibleSpace = 0.0f;
                float maxFlexFractionSoFar = 0.0f;

                RenderBox child = firstChild;
                while (child != null) {
                    int flex = _getFlex(child);
                    totalFlex += flex;
                    if (flex > 0) {
                        float flexFraction = childSize(child, extent) / _getFlex(child);
                        maxFlexFractionSoFar = Mathf.Max(maxFlexFractionSoFar, flexFraction);
                    }
                    else {
                        inflexibleSpace += childSize(child, extent);
                    }

                    var childParentData = (FlexParentData) child.parentData;
                    child = childParentData.nextSibling;
                }

                return maxFlexFractionSoFar * totalFlex + inflexibleSpace;
            }
            else {
                float? availableMainSpace = extent;
                int totalFlex = 0;
                float inflexibleSpace = 0.0f;
                float maxCrossSize = 0.0f;
                RenderBox child = firstChild;
                while (child != null) {
                    int flex = _getFlex(child);
                    totalFlex += flex;
                    if (flex == 0) {
                        float mainSize = 0.0f;
                        float crossSize = 0.0f;

                        switch (_direction) {
                            case Axis.horizontal:
                                mainSize = child.getMaxIntrinsicWidth(float.PositiveInfinity);
                                crossSize = childSize(child, mainSize);
                                break;
                            case Axis.vertical:
                                mainSize = child.getMaxIntrinsicHeight(float.PositiveInfinity);
                                crossSize = childSize(child, mainSize);
                                break;
                        }

                        inflexibleSpace += mainSize;
                        maxCrossSize = Mathf.Max(maxCrossSize, crossSize);
                    }

                    var childParentData = (FlexParentData) child.parentData;
                    child = childParentData.nextSibling;
                }

                float spacePerFlex = Mathf.Max(0.0f, ((availableMainSpace - inflexibleSpace) / totalFlex) ?? 0.0f);

                child = firstChild;
                while (child != null) {
                    int flex = _getFlex(child);
                    if (flex > 0) {
                        maxCrossSize = Mathf.Max(maxCrossSize, childSize(child, spacePerFlex * flex));
                    }

                    var childParentData = (FlexParentData) child.parentData;
                    child = childParentData.nextSibling;
                }

                return maxCrossSize;
            }
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            return _getIntrinsicSize(
                sizingDirection: Axis.horizontal,
                extent: height,
                childSize: (RenderBox child, float extent) => child.getMinIntrinsicWidth(extent)
            );
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            return _getIntrinsicSize(
                sizingDirection: Axis.horizontal,
                extent: height,
                childSize: (RenderBox child, float extent) => child.getMaxIntrinsicWidth(extent)
            );
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return _getIntrinsicSize(
                sizingDirection: Axis.vertical,
                extent: width,
                childSize: (RenderBox child, float extent) => child.getMinIntrinsicHeight(extent)
            );
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return _getIntrinsicSize(
                sizingDirection: Axis.vertical,
                extent: width,
                childSize: (RenderBox child, float extent) => child.getMaxIntrinsicHeight(extent)
            );
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            if (_direction == Axis.horizontal) {
                return defaultComputeDistanceToHighestActualBaseline(baseline);
            }

            return defaultComputeDistanceToFirstActualBaseline(baseline);
        }

        public int _getFlex(RenderBox child) {
            var childParentData = (FlexParentData) child.parentData;
            return childParentData.flex;
        }

        public FlexFit _getFit(RenderBox child) {
            var childParentData = (FlexParentData) child.parentData;
            return childParentData.fit;
        }

        public float _getCrossSize(RenderBox child) {
            switch (_direction) {
                case Axis.horizontal:
                    return child.size.height;
                case Axis.vertical:
                    return child.size.width;
            }

            return 0;
        }

        public float _getMainSize(RenderBox child) {
            switch (_direction) {
                case Axis.horizontal:
                    return child.size.width;
                case Axis.vertical:
                    return child.size.height;
            }

            return 0;
        }

        protected override void performLayout() {
            D.assert(_debugHasNecessaryDirections);
            BoxConstraints constraints = this.constraints;
            int totalFlex = 0;
            int totalChildren = 0;
            D.assert(constraints != null);
            float maxMainSize = _direction == Axis.horizontal
                ? constraints.maxWidth
                : constraints.maxHeight;
            bool canFlex = maxMainSize < float.PositiveInfinity;

            float crossSize = 0.0f;
            float allocatedSize = 0.0f;
            RenderBox child = firstChild;
            RenderBox lastFlexChild = null;
            while (child != null) {
                var childParentData = (FlexParentData) child.parentData;
                totalChildren++;
                int flex = _getFlex(child);
                if (flex > 0) {
                    totalFlex += childParentData.flex;
                    lastFlexChild = child;
                }
                else {
                    BoxConstraints innerConstraints = null;
                    if (crossAxisAlignment == CrossAxisAlignment.stretch) {
                        switch (_direction) {
                            case Axis.horizontal:
                                innerConstraints = new BoxConstraints(
                                    minHeight: constraints.maxHeight,
                                    maxHeight: constraints.maxHeight);
                                break;
                            case Axis.vertical:
                                innerConstraints = new BoxConstraints(
                                    minWidth: constraints.maxWidth,
                                    maxWidth: constraints.maxWidth);
                                break;
                        }
                    }
                    else {
                        switch (_direction) {
                            case Axis.horizontal:
                                innerConstraints = new BoxConstraints(
                                    maxHeight: constraints.maxHeight);
                                break;
                            case Axis.vertical:
                                innerConstraints = new BoxConstraints(
                                    maxWidth: constraints.maxWidth);
                                break;
                        }
                    }

                    child.layout(innerConstraints, parentUsesSize: true);
                    allocatedSize += _getMainSize(child);
                    crossSize = Mathf.Max(crossSize, _getCrossSize(child));
                }
                D.assert(child.parentData == childParentData);
                child = childParentData.nextSibling;
            }

            float freeSpace = Mathf.Max(0.0f, (canFlex ? maxMainSize : 0.0f) - allocatedSize);
            float allocatedFlexSpace = 0.0f;
            float maxBaselineDistance = 0.0f;
            if (totalFlex > 0 || crossAxisAlignment == CrossAxisAlignment.baseline) {
                float spacePerFlex = canFlex && totalFlex > 0 ? (freeSpace / totalFlex) : float.NaN;
                child = firstChild;
                float maxSizeAboveBaseline = 0;
                float maxSizeBelowBaseline = 0;
                while (child != null) {
                    int flex = _getFlex(child);
                    if (flex > 0) {
                        float maxChildExtent = canFlex
                            ? (child == lastFlexChild ? (freeSpace - allocatedFlexSpace) : spacePerFlex * flex)
                            : float.PositiveInfinity;
                        float minChildExtent = 0.0f;
                        switch (_getFit(child)) {
                            case FlexFit.tight:
                                minChildExtent = maxChildExtent;
                                break;
                            case FlexFit.loose:
                                minChildExtent = 0.0f;
                                break;
                        }

                        BoxConstraints innerConstraints = null;
                        if (crossAxisAlignment == CrossAxisAlignment.stretch) {
                            switch (_direction) {
                                case Axis.horizontal:
                                    innerConstraints = new BoxConstraints(
                                        minWidth: minChildExtent,
                                        maxWidth: maxChildExtent,
                                        minHeight: constraints.maxHeight,
                                        maxHeight: constraints.maxHeight);
                                    break;
                                case Axis.vertical:
                                    innerConstraints = new BoxConstraints(
                                        minWidth: constraints.maxWidth,
                                        maxWidth: constraints.maxWidth,
                                        minHeight: minChildExtent,
                                        maxHeight: maxChildExtent);
                                    break;
                            }
                        }
                        else {
                            switch (_direction) {
                                case Axis.horizontal:
                                    innerConstraints = new BoxConstraints(
                                        minWidth: minChildExtent,
                                        maxWidth: maxChildExtent,
                                        maxHeight: constraints.maxHeight);
                                    break;
                                case Axis.vertical:
                                    innerConstraints = new BoxConstraints(
                                        maxWidth: constraints.maxWidth,
                                        minHeight: minChildExtent,
                                        maxHeight: maxChildExtent);
                                    break;
                            }
                        }

                        child.layout(innerConstraints, parentUsesSize: true);
                        float childSize = _getMainSize(child);
                        allocatedSize += childSize;
                        allocatedFlexSpace += maxChildExtent;
                        crossSize = Mathf.Max(crossSize, _getCrossSize(child));
                    }

                    if (crossAxisAlignment == CrossAxisAlignment.baseline) {
                        float? distance = child.getDistanceToBaseline(textBaseline, onlyReal: true);
                        if (distance != null) {
                            maxBaselineDistance = Mathf.Max(maxBaselineDistance, distance.Value);
                            maxSizeAboveBaseline = Mathf.Max(distance.Value, maxSizeAboveBaseline);
                            maxSizeBelowBaseline = Mathf.Max(child.size.height - distance.Value, maxSizeBelowBaseline);
                            crossSize = maxSizeAboveBaseline + maxSizeBelowBaseline;
                        }
                    }

                    var childParentData = (FlexParentData) child.parentData;
                    child = childParentData.nextSibling;
                }
            }

            float idealSize = canFlex && mainAxisSize == MainAxisSize.max ? maxMainSize : allocatedSize;
            float actualSize = 0.0f;
            float actualSizeDelta = 0.0f;
            switch (_direction) {
                case Axis.horizontal:
                    size = constraints.constrain(new Size(idealSize, crossSize));
                    actualSize = size.width;
                    crossSize = size.height;
                    break;
                case Axis.vertical:
                    size = constraints.constrain(new Size(crossSize, idealSize));
                    actualSize = size.height;
                    crossSize = size.width;
                    break;
            }

            actualSizeDelta = actualSize - allocatedSize;
            _overflow = Mathf.Max(0.0f, -actualSizeDelta);

            float remainingSpace = Mathf.Max(0.0f, actualSizeDelta);
            float leadingSpace = 0.0f;
            float betweenSpace = 0.0f;
            bool flipMainAxis = !_startIsTopLeft(direction, textDirection, verticalDirection);
            switch (_mainAxisAlignment) {
                case MainAxisAlignment.start:
                    leadingSpace = 0.0f;
                    betweenSpace = 0.0f;
                    break;
                case MainAxisAlignment.end:
                    leadingSpace = remainingSpace;
                    betweenSpace = 0.0f;
                    break;
                case MainAxisAlignment.center:
                    leadingSpace = remainingSpace / 2.0f;
                    betweenSpace = 0.0f;
                    break;
                case MainAxisAlignment.spaceBetween:
                    leadingSpace = 0.0f;
                    betweenSpace = totalChildren > 1 ? remainingSpace / (totalChildren - 1) : 0.0f;
                    break;
                case MainAxisAlignment.spaceAround:
                    betweenSpace = totalChildren > 0 ? remainingSpace / totalChildren : 0.0f;
                    leadingSpace = betweenSpace / 2.0f;
                    break;
                case MainAxisAlignment.spaceEvenly:
                    betweenSpace = totalChildren > 0 ? remainingSpace / (totalChildren + 1) : 0.0f;
                    leadingSpace = betweenSpace;
                    break;
            }

            // Position elements
            float childMainPosition = flipMainAxis ? actualSize - leadingSpace : leadingSpace;
            child = firstChild;
            while (child != null) {
                var childParentData = (FlexParentData) child.parentData;
                float childCrossPosition = 0.0f;
                switch (_crossAxisAlignment) {
                    case CrossAxisAlignment.start:
                    case CrossAxisAlignment.end:
                        childCrossPosition =
                            _startIsTopLeft(
                                AxisUtils.flipAxis(direction), textDirection, verticalDirection)
                            == (_crossAxisAlignment == CrossAxisAlignment.start)
                                ? 0.0f
                                : crossSize - _getCrossSize(child);
                        break;
                    case CrossAxisAlignment.center:
                        childCrossPosition = crossSize / 2.0f - _getCrossSize(child) / 2.0f;
                        break;
                    case CrossAxisAlignment.stretch:
                        childCrossPosition = 0.0f;
                        break;
                    case CrossAxisAlignment.baseline:
                        childCrossPosition = 0.0f;
                        if (_direction == Axis.horizontal) {
                            float? distance = child.getDistanceToBaseline(textBaseline, onlyReal: true);
                            if (distance != null) {
                                childCrossPosition = maxBaselineDistance - distance.Value;
                            }
                        }

                        break;
                }

                if (flipMainAxis) {
                    childMainPosition -= _getMainSize(child);
                }

                switch (_direction) {
                    case Axis.horizontal:
                        childParentData.offset = new Offset(childMainPosition, childCrossPosition);
                        break;
                    case Axis.vertical:
                        childParentData.offset = new Offset(childCrossPosition, childMainPosition);
                        break;
                }

                if (flipMainAxis) {
                    childMainPosition -= betweenSpace;
                }
                else {
                    childMainPosition += _getMainSize(child) + betweenSpace;
                }

                child = childParentData.nextSibling;
            }
        }

        static bool _startIsTopLeft(Axis direction, TextDirection? textDirection,
            VerticalDirection? verticalDirection) {
            switch (direction) {
                case Axis.horizontal:
                    switch (textDirection) {
                        case TextDirection.ltr:
                            return true;
                        case TextDirection.rtl:
                            return false;
                    }

                    break;
                case Axis.vertical:
                    switch (verticalDirection) {
                        case VerticalDirection.down:
                            return true;
                        case VerticalDirection.up:
                            return false;
                    }

                    break;
            }

            return true;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (!_hasOverflow) {
                defaultPaint(context, offset);
                return;
            }

            if (size.isEmpty) {
                return;
            }

            context.pushClipRect(needsCompositing, offset, Offset.zero & size, defaultPaint);

            D.assert(() => {
                List<DiagnosticsNode> debugOverflowHints = new List<DiagnosticsNode>{
                new ErrorDescription(
                    $"The overflowing {GetType()} has an orientation of {_direction}."
                ),
                new ErrorDescription(
                    $"The edge of the {GetType()} that is overflowing has been marked " +
                    "in the rendering with a yellow and black striped pattern. This is " +
                    "usually caused by the contents being too big for the {GetType()}."
                ),
                new ErrorHint(
                    "Consider applying a flex factor (e.g. using an Expanded widget) to " +
                    $"force the children of the {GetType()} to fit within the available " +
                    "space instead of being sized to their natural size."
                ),
                new ErrorHint(
                    "This is considered an error condition because it indicates that there " +
                    "is content that cannot be seen. If the content is legitimately bigger " +
                    "than the available space, consider clipping it with a ClipRect widget " +
                    "before putting it in the flex, or using a scrollable container rather " +
                    "than a Flex, like a ListView."
                )};
                
                Rect overflowChildRect;
                switch (_direction) {
                    case Axis.horizontal:
                        overflowChildRect = Rect.fromLTWH(0.0f, 0.0f, size.width + _overflow, 0.0f);
                        break;
                    case Axis.vertical:
                        overflowChildRect = Rect.fromLTWH(0.0f, 0.0f, 0.0f, size.height + _overflow);
                        break;
                    default:
                        throw new Exception("Unknown direction: " + _direction);
                }

                DebugOverflowIndicatorMixin.paintOverflowIndicator(this, context, offset, Offset.zero & size,
                    overflowChildRect, overflowHints: debugOverflowHints);
                return true;
            });
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return defaultHitTestChildren(result, position: position);
        }
        
        public override Rect describeApproximatePaintClip(RenderObject child) => _hasOverflow ? Offset.zero & size : null;
        
        
        public override string toStringShort() {
            string header = base.toStringShort();
            if (_hasOverflow)
                header += " OVERFLOWING";
            return header;
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<Axis>("direction", direction));
            properties.add(new EnumProperty<MainAxisAlignment>("mainAxisAlignment", mainAxisAlignment));
            properties.add(new EnumProperty<MainAxisSize>("mainAxisSize", mainAxisSize));
            properties.add(new EnumProperty<CrossAxisAlignment>("crossAxisAlignment", crossAxisAlignment));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add(new EnumProperty<VerticalDirection?>("verticalDirection", verticalDirection, defaultValue: null));
            properties.add(new EnumProperty<TextBaseline>("textBaseline", textBaseline, defaultValue: null));
        }
    }
}