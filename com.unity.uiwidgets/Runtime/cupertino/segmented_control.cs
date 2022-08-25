using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoSegmentedControlsUtils {
        public static readonly EdgeInsetsGeometry _kHorizontalItemPadding = EdgeInsets.symmetric(horizontal: 16.0f);
        public const float _kMinSegmentedControlHeight = 28.0f;
        public static readonly TimeSpan _kFadeDuration = TimeSpan.FromMilliseconds(165);
    }

    public class CupertinoSegmentedControl<T> : StatefulWidget
    {
        public CupertinoSegmentedControl(
            Key key = null,
            Dictionary<T, Widget> children = null,
            ValueChanged<T> onValueChanged = null,
            T groupValue = default,
            Color unselectedColor = null,
            Color selectedColor = null,
            Color borderColor = null,
            Color pressedColor = null,
            EdgeInsetsGeometry padding = null
        ) : base(key: key) {
            D.assert(children != null);
            D.assert(children.Count >= 2);
            D.assert(onValueChanged != null);
            D.assert(
                groupValue == null || children.Keys.Any((T child) => child.Equals(groupValue)), () =>
                    "The groupValue must be either null or one of the keys in the children map."
            );
            this.children = children;
            this.onValueChanged = onValueChanged;
            this.groupValue = groupValue;
            this.unselectedColor = unselectedColor;
            this.selectedColor = selectedColor;
            this.borderColor = borderColor;
            this.pressedColor = pressedColor;
            this.padding = padding;
        }

        public readonly Dictionary<T, Widget> children;
        public readonly T groupValue;
        public readonly ValueChanged<T> onValueChanged;
        public readonly Color unselectedColor;
        public readonly Color selectedColor;
        public readonly Color borderColor;
        public readonly Color pressedColor;
        public readonly EdgeInsetsGeometry padding;

        public override State createState() {
            return new _SegmentedControlState<T>();
        }
    }

    class _SegmentedControlState<T> : TickerProviderStateMixin<CupertinoSegmentedControl<T>> {
        
        T __pressedKey = default;
        bool __isNull = true;

        void setPressedKey(T newkey) {
            __pressedKey = newkey;
            __isNull = false;
        }

        void setPressedKeyNull() {
            __pressedKey = default;
            __isNull = true;
        }

        bool isPressedKeyNull() {
            return __isNull;
        }

        T getPressedKey() {
            return __pressedKey;
        }

        readonly List<AnimationController> _selectionControllers = new List<AnimationController>();
        readonly List<ColorTween> _childTweens = new List<ColorTween>();

        ColorTween _forwardBackgroundColorTween;
        ColorTween _reverseBackgroundColorTween;
        ColorTween _textColorTween;

        Color _selectedColor;
        Color _unselectedColor;
        Color _borderColor;
        Color _pressedColor;

        AnimationController createAnimationController() {
            var controller = new AnimationController(
                duration: CupertinoSegmentedControlsUtils._kFadeDuration,
                vsync: this
            );
            controller.addListener(() => {
                setState(() => {
                });
            });
            return controller;
        }

        bool _updateColors() {
            D.assert(mounted, () => "This should only be called after didUpdateDependencies");
            var changed = false;
            var selectedColor = widget.selectedColor ?? CupertinoTheme.of(context).primaryColor;
            if (_selectedColor != selectedColor) {
                changed = true;
                _selectedColor = selectedColor;
            }

            var unselectedColor = widget.unselectedColor ?? CupertinoTheme.of(context).primaryContrastingColor;
            if (_unselectedColor != unselectedColor) {
                changed = true;
                _unselectedColor = unselectedColor;
            }

            var borderColor = widget.borderColor ?? CupertinoTheme.of(context).primaryColor;
            if (_borderColor != borderColor) {
                changed = true;
                _borderColor = borderColor;
            }

            var pressedColor = widget.pressedColor ?? CupertinoTheme.of(context).primaryColor.withOpacity(0.2f);
            if (_pressedColor != pressedColor) {
                changed = true;
                _pressedColor = pressedColor;
            }

            _forwardBackgroundColorTween = new ColorTween(
                begin: _pressedColor,
                end: _selectedColor
            );
            _reverseBackgroundColorTween = new ColorTween(
                begin: _unselectedColor,
                end: _selectedColor
            );
            
            _textColorTween = new ColorTween(
                begin: _selectedColor,
                end: _unselectedColor
            );
            
            return changed;
        }

        void _updateAnimationControllers() {
            D.assert(mounted, () => "This should only be called after didUpdateDependencies");
            foreach (var controller in _selectionControllers) {
                controller.dispose();
            }

            _selectionControllers.Clear();
            _childTweens.Clear();
            foreach (var key in widget.children.Keys) {
                var animationController = createAnimationController();
                if (widget.groupValue.Equals(key)) {
                    _childTweens.Add(_reverseBackgroundColorTween);
                    animationController.setValue(1.0f);
                }
                else {
                    _childTweens.Add(_forwardBackgroundColorTween);
                }

                _selectionControllers.Add(animationController);
            }
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();

            if (_updateColors()) {
                _updateAnimationControllers();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = oldWidget as CupertinoSegmentedControl<T>;
            base.didUpdateWidget(oldWidget);

            if (_updateColors() || _oldWidget.children.Count != widget.children.Count) {
                _updateAnimationControllers();
            }

            if (!_oldWidget.groupValue.Equals(widget.groupValue)) {
                var index = 0;
                foreach (var key in widget.children.Keys) {
                    if (widget.groupValue.Equals(key)) {
                        _childTweens[index] = _forwardBackgroundColorTween;
                        _selectionControllers[index].forward();
                    }
                    else {
                        _childTweens[index] = _reverseBackgroundColorTween;
                        _selectionControllers[index].reverse();
                    }

                    index += 1;
                }
            }
        }

        public override void dispose() {
            foreach (var animationController in _selectionControllers) {
                animationController.dispose();
            }

            base.dispose();
        }


        void _onTapDown(T currentKey) {
            if (isPressedKeyNull() && !currentKey.Equals(widget.groupValue)) {
                setState(() => {
                    setPressedKey(currentKey);
                });
            }
        }

        void _onTapCancel() {
            setState(setPressedKeyNull);
        }

        bool isPressKeyEquals(T currentKey) {
            return isPressedKeyNull() && currentKey == null ||
                   !isPressedKeyNull() && currentKey.Equals(getPressedKey());
        }

        void _onTap(T currentKey) {
            if (!isPressKeyEquals(currentKey))
                return;
            
            if (!currentKey.Equals(widget.groupValue)) {
                widget.onValueChanged(currentKey);
            }

            setPressedKeyNull();
        }

        Color getTextColor(int index, T currentKey) {
            if (_selectionControllers[index].isAnimating)
                return _textColorTween.evaluate(_selectionControllers[index]);
            if (widget.groupValue.Equals(currentKey))
                return _unselectedColor;
            return _selectedColor;
        }

        Color getBackgroundColor(int index, T currentKey) {
            if (_selectionControllers[index].isAnimating)
                return _childTweens[index].evaluate(_selectionControllers[index]);
            if (widget.groupValue.Equals(currentKey))
                return _selectedColor;
            if (isPressKeyEquals(currentKey))
                return _pressedColor;
            return _unselectedColor;
        }

        public override Widget build(BuildContext context) {
            List<Widget> _gestureChildren = new List<Widget>();
            List<Color> _backgroundColors = new List<Color>();
            int index = 0;
            int? selectedIndex = null;
            int? pressedIndex = null;
            foreach (var currentKey in widget.children.Keys) {
                var currentKey2 = currentKey;
                selectedIndex = (widget.groupValue.Equals(currentKey2)) ? index : selectedIndex;
                pressedIndex = (isPressKeyEquals(currentKey2)) ? index : pressedIndex;
                var textStyle = DefaultTextStyle.of(context).style.copyWith(
                    color: getTextColor(index, currentKey2)
                );
                var iconTheme = new IconThemeData(
                    color: getTextColor(index, currentKey2)
                );

                Widget child = new Center(
                    child: widget.children[currentKey2]
                );

                child = new GestureDetector(
                    onTapDown: (TapDownDetails _event) => {
                        _onTapDown(currentKey2);
                    },
                    onTapCancel: _onTapCancel,
                    onTap: () => {
                        _onTap(currentKey2);
                    },
                    child: new IconTheme(
                        data: iconTheme,
                        child: new DefaultTextStyle(
                            style: textStyle,
                            child: child
                        )
                    )
                );

                _backgroundColors.Add(getBackgroundColor(index, currentKey2));
                _gestureChildren.Add(child);
                index += 1;
            }

            Widget box = new _SegmentedControlRenderWidget<T>(
                children: _gestureChildren,
                selectedIndex: selectedIndex,
                pressedIndex: pressedIndex,
                backgroundColors: _backgroundColors,
                borderColor: _borderColor
            );

            return new Padding(
                padding: widget.padding ?? CupertinoSegmentedControlsUtils._kHorizontalItemPadding,
                child: new UnconstrainedBox(
                    constrainedAxis: Axis.horizontal,
                    child: box
                )
            );
        }
    }

    public class _SegmentedControlRenderWidget<T> : MultiChildRenderObjectWidget {
        public _SegmentedControlRenderWidget(
            Key key = null,
            List<Widget> children = null,
            int? selectedIndex = null,
            int? pressedIndex = null,
            List<Color> backgroundColors = null,
            Color borderColor = null
        ) : base(
            key: key,
            children: children ?? new List<Widget>()
        ) {
            this.selectedIndex = selectedIndex;
            this.pressedIndex = pressedIndex;
            this.backgroundColors = backgroundColors;
            this.borderColor = borderColor;
        }

        public readonly int? selectedIndex;
        public readonly int? pressedIndex;
        public readonly List<Color> backgroundColors;
        public readonly Color borderColor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSegmentedControl<T>(
                textDirection: Directionality.of(context),
                selectedIndex: selectedIndex,
                pressedIndex: pressedIndex,
                backgroundColors: backgroundColors,
                borderColor: borderColor
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (_RenderSegmentedControl<T>) renderObject;
            ((_RenderSegmentedControl<T>) renderObject).textDirection = Directionality.of(context);
            ((_RenderSegmentedControl<T>) renderObject).selectedIndex = selectedIndex;
            ((_RenderSegmentedControl<T>) renderObject).pressedIndex = pressedIndex;
            ((_RenderSegmentedControl<T>) renderObject).backgroundColors = backgroundColors;
            ((_RenderSegmentedControl<T>) renderObject).borderColor = borderColor;
        }
    }

    class _SegmentedControlContainerBoxParentData : ContainerBoxParentData<RenderBox> {
        public RRect surroundingRect;
    }

    public delegate RenderBox _NextChild(RenderBox child);

    class _RenderSegmentedControl<T> : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        ContainerBoxParentData<RenderBox>> {
        public _RenderSegmentedControl(
            int? selectedIndex = null,
            int? pressedIndex = null,
            TextDirection? textDirection = null,
            List<Color> backgroundColors = null,
            Color borderColor = null
        ) {
            D.assert(textDirection != null);
            _textDirection = textDirection;
            _selectedIndex = selectedIndex;
            _pressedIndex = pressedIndex;
            _backgroundColors = backgroundColors;
            _borderColor = borderColor;
        }

        public int? selectedIndex {
            get { return _selectedIndex; }
            set {
                if (_selectedIndex == value) {
                    return;
                }

                _selectedIndex = value;
                markNeedsPaint();
            }
        }

        int? _selectedIndex;

        public int? pressedIndex {
            get { return _pressedIndex; }
            set {
                if (_pressedIndex == value) {
                    return;
                }

                _pressedIndex = value;
                markNeedsPaint();
            }
        }

        int? _pressedIndex;

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

        TextDirection? _textDirection;


        public List<Color> backgroundColors {
            get { return _backgroundColors; }
            set {
                if (_backgroundColors == value) {
                    return;
                }

                _backgroundColors = value;
                markNeedsPaint();
            }
        }

        List<Color> _backgroundColors;


        public Color borderColor {
            get { return _borderColor; }
            set {
                if (_borderColor == value) {
                    return;
                }

                _borderColor = value;
                markNeedsPaint();
            }
        }

        Color _borderColor;


        protected internal override float computeMinIntrinsicWidth(float height) {
            RenderBox child = firstChild;
            float minWidth = 0.0f;
            while (child != null) {
                _SegmentedControlContainerBoxParentData childParentData =
                    child.parentData as _SegmentedControlContainerBoxParentData;
                float childWidth = child.getMinIntrinsicWidth(height);
                minWidth = Mathf.Max(minWidth, childWidth);
                child = childParentData.nextSibling;
            }

            return minWidth * childCount;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            RenderBox child = firstChild;
            float maxWidth = 0.0f;
            while (child != null) {
                _SegmentedControlContainerBoxParentData childParentData =
                    child.parentData as _SegmentedControlContainerBoxParentData;
                float childWidth = child.getMaxIntrinsicWidth(height);
                maxWidth = Mathf.Max(maxWidth, childWidth);
                child = childParentData.nextSibling;
            }

            return maxWidth * childCount;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            RenderBox child = firstChild;
            float minHeight = 0.0f;
            while (child != null) {
                _SegmentedControlContainerBoxParentData childParentData =
                    child.parentData as _SegmentedControlContainerBoxParentData;
                float childHeight = child.getMinIntrinsicHeight(width);
                minHeight = Mathf.Max(minHeight, childHeight);
                child = childParentData.nextSibling;
            }

            return minHeight;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            RenderBox child = firstChild;
            float maxHeight = 0.0f;
            while (child != null) {
                _SegmentedControlContainerBoxParentData childParentData =
                    child.parentData as _SegmentedControlContainerBoxParentData;
                float childHeight = child.getMaxIntrinsicHeight(width);
                maxHeight = Mathf.Max(maxHeight, childHeight);
                child = childParentData.nextSibling;
            }

            return maxHeight;
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return defaultComputeDistanceToHighestActualBaseline(baseline);
        }

        public override void setupParentData(RenderObject child) {
            child = (RenderBox) child;
            if (!(child.parentData is _SegmentedControlContainerBoxParentData)) {
                child.parentData = new _SegmentedControlContainerBoxParentData();
            }
        }

        void _layoutRects(_NextChild nextChild, RenderBox leftChild, RenderBox rightChild) {
            RenderBox child = leftChild;
            float start = 0.0f;
            while (child != null) {
                _SegmentedControlContainerBoxParentData childParentData =
                    child.parentData as _SegmentedControlContainerBoxParentData;
                Offset childOffset = new Offset(start, 0.0f);
                childParentData.offset = childOffset;
                Rect childRect = Rect.fromLTWH(start, 0.0f, child.size.width, child.size.height);
                RRect rChildRect = null;
                if (child == leftChild) {
                    rChildRect = RRect.fromRectAndCorners(childRect, topLeft: Radius.circular(3.0f),
                        bottomLeft: Radius.circular(3.0f));
                }
                else if (child == rightChild) {
                    rChildRect = RRect.fromRectAndCorners(childRect, topRight: Radius.circular(3.0f),
                        bottomRight: Radius.circular(3.0f));
                }
                else {
                    rChildRect = RRect.fromRectAndCorners(childRect, topRight: Radius.zero);
                }

                childParentData.surroundingRect = rChildRect;
                start += child.size.width;
                child = nextChild(child);
            }
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            float maxHeight = CupertinoSegmentedControlsUtils._kMinSegmentedControlHeight;

            float childWidth = constraints.minWidth / childCount;
            foreach (RenderBox child in getChildrenAsList()) {
                childWidth = Mathf.Max(childWidth, child.getMaxIntrinsicWidth(float.PositiveInfinity));
            }

            childWidth = Mathf.Min(childWidth, constraints.maxWidth / childCount);

            RenderBox child1 = firstChild;
            while (child1 != null) {
                float boxHeight = child1.getMaxIntrinsicHeight(childWidth);
                maxHeight = Mathf.Max(maxHeight, boxHeight);
                child1 = childAfter(child1);
            }

            constraints.constrainHeight(maxHeight);

            BoxConstraints childConstraints = BoxConstraints.tightFor(
                width: childWidth,
                height: maxHeight
            );

            child1 = firstChild;
            while (child1 != null) {
                child1.layout(childConstraints, parentUsesSize: true);
                child1 = childAfter(child1);
            }

            switch (textDirection) {
                case TextDirection.rtl:
                    _layoutRects(
                        childBefore,
                        lastChild,
                        firstChild
                    );
                    break;
                case TextDirection.ltr:
                    _layoutRects(
                        childAfter,
                        firstChild,
                        lastChild
                    );
                    break;
            }

            size = constraints.constrain(new Size(childWidth * childCount, maxHeight));
        }

        public override void paint(PaintingContext context, Offset offset) {
            RenderBox child = firstChild;
            int index = 0;
            while (child != null) {
                _paintChild(context, offset, child, index);
                child = childAfter(child);
                index += 1;
            }
        }

        void _paintChild(PaintingContext context, Offset offset, RenderBox child, int childIndex) {
            D.assert(child != null);

            _SegmentedControlContainerBoxParentData childParentData =
                child.parentData as _SegmentedControlContainerBoxParentData;

            context.canvas.drawRRect(
                childParentData.surroundingRect.shift(offset),
                new Paint() {
                    color = backgroundColors[childIndex],
                    style = PaintingStyle.fill
                }
            );
            context.canvas.drawRRect(
                childParentData.surroundingRect.shift(offset),
                new Paint() {
                    color = borderColor,
                    strokeWidth = 1.0f,
                    style = PaintingStyle.stroke
                }
            );

            context.paintChild(child, childParentData.offset + offset);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            D.assert(position != null);
            RenderBox child = lastChild;
            while (child != null) {
                _SegmentedControlContainerBoxParentData childParentData =
                    child.parentData as _SegmentedControlContainerBoxParentData;
                if (childParentData.surroundingRect.contains(position)) {
                    Offset center = (Offset.zero & child.size).center;
                    return result.addWithRawTransform(
                        transform: MatrixUtils.forceToPoint(center),
                        position: center,
                        hitTest: (BoxHitTestResult result1, Offset position1) => {
                            D.assert(position1 == center);
                            return child.hitTest(result1, position: center);
                        }
                    );
                }

                child = childParentData.previousSibling;
            }

            return false;
        }
    }
}