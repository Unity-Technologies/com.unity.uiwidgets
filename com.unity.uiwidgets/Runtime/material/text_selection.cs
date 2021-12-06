using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.material {
    public static class MaterialUtils {
        public static readonly TextSelectionControls materialTextSelectionControls =
            new _MaterialTextSelectionControls();

        internal const float _kHandleSize = 22.0f;

        internal const float _kToolbarScreenPadding = 8.0f;

        internal const float _kToolbarHeight = 44.0f;

        internal const float _kToolbarContentDistanceBelow = _kHandleSize - 2.0f;

        internal const float _kToolbarContentDistance = 8.0f;
    }

    public class _TextSelectionToolbar : StatefulWidget {
        public _TextSelectionToolbar(Key key = null, VoidCallback handleCut = null,
            VoidCallback handleCopy = null, VoidCallback handlePaste = null, VoidCallback handleSelectAll = null,
            bool isAbove = false) : base(key: key) {
            this.handleCut = handleCut;
            this.handleCopy = handleCopy;
            this.handlePaste = handlePaste;
            this.handleSelectAll = handleSelectAll;
            this.isAbove = isAbove;
        }

        public readonly VoidCallback handleCut;
        public readonly VoidCallback handleCopy;
        public readonly VoidCallback handlePaste;
        public readonly VoidCallback handleSelectAll;
        public readonly bool isAbove;

        public override State createState() {
            return new _TextSelectionToolbarState();
        }
    }

    public class _TextSelectionToolbarState : TickerProviderStateMixin<_TextSelectionToolbar> {
        bool _overflowOpen = false;

        UniqueKey _containerKey = new UniqueKey();

        FlatButton _getItem(VoidCallback onPressed, string label) {
            D.assert(onPressed != null);
            return new FlatButton(
                child: new Text(label),
                onPressed: onPressed
            );
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = (_TextSelectionToolbar) oldWidget;
            if (((widget.handleCut == null) != (_oldWidget.handleCut == null))
                || ((widget.handleCopy == null) != (_oldWidget.handleCopy == null))
                || ((widget.handlePaste == null) != (_oldWidget.handlePaste == null))
                || ((widget.handleSelectAll == null) != (_oldWidget.handleSelectAll == null))) {
                _containerKey = new UniqueKey();
                _overflowOpen = false;
            }

            base.didUpdateWidget(oldWidget);
        }

        public override Widget build(BuildContext context) {
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            List<Widget> items = new List<Widget>();

            if (widget.handleCut != null) {
                items.Add(_getItem(widget.handleCut, localizations.cutButtonLabel));
            }

            if (widget.handleCopy != null) {
                items.Add(_getItem(widget.handleCopy, localizations.copyButtonLabel));
            }

            if (widget.handlePaste != null) {
                items.Add(_getItem(widget.handlePaste, localizations.pasteButtonLabel));
            }

            if (widget.handleSelectAll != null) {
                items.Add(_getItem(widget.handleSelectAll, localizations.selectAllButtonLabel));
            }

            if (items.isEmpty()) {
                return new Container(width: 0.0f, height: 0.0f);
            }

            items.Insert(0, new IconButton(
                    icon: new Icon(_overflowOpen ? Icons.arrow_back : Icons.more_vert),
                    onPressed: () => { setState(() => { _overflowOpen = !_overflowOpen; }); },
                    tooltip: _overflowOpen
                        ? localizations.backButtonTooltip
                        : localizations.moreButtonTooltip
                )
            );

            return new _TextSelectionToolbarContainer(
                key: _containerKey,
                overflowOpen: _overflowOpen,
                child: new AnimatedSize(
                    vsync: this,
                    duration: new TimeSpan(0, 0, 0, 0, 140),
                    child: new Material(
                        elevation: 1.0f,
                        child: new _TextSelectionToolbarItems(
                            isAbove: widget.isAbove,
                            overflowOpen: _overflowOpen,
                            children: items)
                    )
                )
            );
        }
    }

    class _TextSelectionToolbarContainer : SingleChildRenderObjectWidget {
        public _TextSelectionToolbarContainer(
            Key key = null,
            Widget child = null,
            bool overflowOpen = false
        ) : base(key: key, child: child) {
            D.assert(child != null);
            this.overflowOpen = overflowOpen;
        }

        public readonly bool overflowOpen;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _TextSelectionToolbarContainerRenderBox(overflowOpen: overflowOpen);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var _renderObject = (_TextSelectionToolbarContainerRenderBox) renderObject;
            _renderObject.overflowOpen = overflowOpen;
        }
    }

    class _TextSelectionToolbarContainerRenderBox : RenderProxyBox {
        public _TextSelectionToolbarContainerRenderBox(
            bool overflowOpen = false
        ) : base() {
            _overflowOpen = overflowOpen;
        }

        float? _closedWidth;

        public bool overflowOpen {
            get { return _overflowOpen; }
            set {
                if (value == overflowOpen) {
                    return;
                }

                _overflowOpen = value;
                markNeedsLayout();
            }
        }

        bool _overflowOpen;

        protected override void performLayout() {
            child.layout(constraints.loosen(), parentUsesSize: true);

            if (!overflowOpen && _closedWidth == null) {
                _closedWidth = child.size.width;
            }

            size = constraints.constrain(new Size(
                (_closedWidth == null || child.size.width > _closedWidth) ? child.size.width : _closedWidth.Value,
                child.size.height
            ));

            _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;
            childParentData.offset = new Offset(
                size.width - child.size.width,
                0.0f
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;
            context.paintChild(child, childParentData.offset + offset);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;
            return result.addWithPaintOffset(
                offset: childParentData.offset,
                position: position,
                hitTest: (BoxHitTestResult boxResult, Offset boxTransformed) => {
                    D.assert(boxTransformed == position - childParentData.offset);
                    return child.hitTest(boxResult, position: boxTransformed);
                }
            );
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is _ToolbarParentData)) {
                child.parentData = new _ToolbarParentData();
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;
            transform.translate(childParentData.offset.dx, childParentData.offset.dy);
            base.applyPaintTransform(child, transform);
        }
    }

    class _TextSelectionToolbarItems : MultiChildRenderObjectWidget {
        public _TextSelectionToolbarItems(
            Key key = null,
            bool isAbove = false,
            bool overflowOpen = false,
            List<Widget> children = null) : base(key: key, children: children) {
            D.assert(children != null);
            this.isAbove = isAbove;
            this.overflowOpen = overflowOpen;
        }

        public readonly bool isAbove;
        public readonly bool overflowOpen;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _TextSelectionToolbarItemsRenderBox(
                isAbove: isAbove,
                overflowOpen: overflowOpen
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var _renderObject = (_TextSelectionToolbarItemsRenderBox) renderObject;
            _renderObject.isAbove = isAbove;
            _renderObject.overflowOpen = overflowOpen;
        }

        public override Element createElement() {
            return new _TextSelectionToolbarItemsElement(this);
        }
    }

    class _ToolbarParentData : ContainerBoxParentData<RenderBox> {
        public bool shouldPaint;

        public override string ToString() {
            return $"{base.ToString()}; shouldPaint={shouldPaint}";
        }
    }

    class _TextSelectionToolbarItemsElement : MultiChildRenderObjectElement {
        public _TextSelectionToolbarItemsElement(
            MultiChildRenderObjectWidget widget) :
            base(widget: widget) {
        }

        static bool _shouldPaint(Element child) {
            return (child.renderObject.parentData as _ToolbarParentData).shouldPaint;
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            foreach (var child in LinqUtils<Element>.WhereList(children,(_shouldPaint))) {
                visitor(child);
            }
        }
    }

    class _TextSelectionToolbarItemsRenderBox : ContainerRenderObjectMixinRenderBox<RenderBox, _ToolbarParentData> {
        public _TextSelectionToolbarItemsRenderBox(
            bool isAbove = false,
            bool overflowOpen = false
        ) : base() {
            _isAbove = isAbove;
            _overflowOpen = overflowOpen;
        }

        int _lastIndexThatFits = -1;

        public bool isAbove {
            get { return _isAbove; }
            set {
                if (value == isAbove) {
                    return;
                }

                _isAbove = value;
                markNeedsLayout();
            }
        }

        bool _isAbove;

        public bool overflowOpen {
            get { return _overflowOpen; }
            set {
                if (value == overflowOpen) {
                    return;
                }

                _overflowOpen = value;
                markNeedsLayout();
            }
        }

        bool _overflowOpen;

        void _layoutChildren() {
            BoxConstraints sizedConstraints = _overflowOpen
                ? constraints
                : BoxConstraints.loose(new Size(
                    constraints.maxWidth,
                    MaterialUtils._kToolbarHeight
                ));

            int i = -1;
            float width = 0.0f;
            visitChildren((RenderObject renderObjectChild) => {
                i++;

                if (_lastIndexThatFits != -1 && !overflowOpen) {
                    return;
                }

                RenderBox child = renderObjectChild as RenderBox;
                child.layout(sizedConstraints.loosen(), parentUsesSize: true);
                width += child.size.width;

                if (width > sizedConstraints.maxWidth && _lastIndexThatFits == -1) {
                    _lastIndexThatFits = i - 1;
                }
            });

            RenderBox navButton = firstChild;
            if (_lastIndexThatFits != -1
                && _lastIndexThatFits == childCount - 2
                && width - navButton.size.width <= sizedConstraints.maxWidth) {
                _lastIndexThatFits = -1;
            }
        }

        bool _shouldPaintChild(RenderObject renderObjectChild, int index) {
            if (renderObjectChild == firstChild) {
                return _lastIndexThatFits != -1;
            }

            if (_lastIndexThatFits == -1) {
                return true;
            }

            return (index > _lastIndexThatFits) == overflowOpen;
        }

        void _placeChildren() {
            int i = -1;
            Size nextSize = new Size(0.0f, 0.0f);
            float fitWidth = 0.0f;
            RenderBox navButton = firstChild;
            float overflowHeight = overflowOpen && !isAbove ? navButton.size.height : 0.0f;
            visitChildren((RenderObject renderObjectChild) => {
                i++;

                RenderBox child = renderObjectChild as RenderBox;
                _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;

                if (renderObjectChild == navButton) {
                    return;
                }

                if (!_shouldPaintChild(renderObjectChild, i)) {
                    childParentData.shouldPaint = false;
                    return;
                }

                childParentData.shouldPaint = true;

                if (!overflowOpen) {
                    childParentData.offset = new Offset(fitWidth, 0.0f);
                    fitWidth += child.size.width;
                    nextSize = new Size(
                        fitWidth,
                        Mathf.Max(child.size.height, nextSize.height)
                    );
                }
                else {
                    childParentData.offset = new Offset(0.0f, overflowHeight);
                    overflowHeight += child.size.height;
                    nextSize = new Size(
                        Mathf.Max(child.size.width, nextSize.width),
                        overflowHeight
                    );
                }
            });

            _ToolbarParentData navButtonParentData = navButton.parentData as _ToolbarParentData;
            if (_shouldPaintChild(firstChild, 0)) {
                navButtonParentData.shouldPaint = true;
                if (overflowOpen) {
                    navButtonParentData.offset = isAbove
                        ? new Offset(0.0f, overflowHeight)
                        : Offset.zero;
                    nextSize = new Size(
                        nextSize.width,
                        isAbove ? nextSize.height + navButton.size.height : nextSize.height
                    );
                }
                else {
                    navButtonParentData.offset = new Offset(fitWidth, 0.0f);
                    nextSize = new Size(nextSize.width + navButton.size.width, nextSize.height);
                }
            }
            else {
                navButtonParentData.shouldPaint = false;
            }

            size = nextSize;
        }

        protected override void performLayout() {
            _lastIndexThatFits = -1;
            if (firstChild == null) {
                performResize();
                return;
            }

            _layoutChildren();
            _placeChildren();
        }

        public override void paint(PaintingContext context, Offset offset) {
            visitChildren((RenderObject renderObjectChild) => {
                RenderBox child = renderObjectChild as RenderBox;
                _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;
                if (!childParentData.shouldPaint) {
                    return;
                }

                context.paintChild(child, childParentData.offset + offset);
            });
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is _ToolbarParentData)) {
                child.parentData = new _ToolbarParentData();
            }
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            RenderBox child = lastChild;
            while (child != null) {
                _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;

                if (!childParentData.shouldPaint) {
                    child = childParentData.previousSibling;
                    continue;
                }

                bool isHit = result.addWithPaintOffset(
                    offset: childParentData.offset,
                    position: position,
                    hitTest: (BoxHitTestResult boxResult, Offset boxTransformed) => {
                        D.assert(boxTransformed == position - childParentData.offset);
                        return child.hitTest(boxResult, position: boxTransformed);
                    }
                );
                if (isHit) {
                    return true;
                }

                child = childParentData.previousSibling;
            }

            return false;
        }
    }

    class _TextSelectionToolbarLayout : SingleChildLayoutDelegate {
        internal _TextSelectionToolbarLayout(Offset anchor, float upperBounds, bool fitsAbove) {
            this.anchor = anchor;
            this.upperBounds = upperBounds;
            this.fitsAbove = fitsAbove;
        }

        public readonly Offset anchor;
        public readonly float upperBounds;
        public readonly bool fitsAbove;

        static float _centerOn(float position, float width, float min, float max) {
            if (position - width / 2.0f < min) {
                return min;
            }

            if (position + width / 2.0f > max) {
                return max - width;
            }

            return position - width / 2.0f;
        }

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints.loosen();
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            return new Offset(
                _centerOn(
                    anchor.dx,
                    childSize.width,
                    MaterialUtils._kToolbarScreenPadding,
                    size.width - MaterialUtils._kToolbarScreenPadding
                ),
                fitsAbove
                    ? Mathf.Max(upperBounds, anchor.dy - childSize.height)
                    : anchor.dy
            );
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            return anchor != ((_TextSelectionToolbarLayout) oldDelegate).anchor;
        }
    }

    class _TextSelectionHandlePainter : AbstractCustomPainter {
        internal _TextSelectionHandlePainter(Color color) {
            this.color = color;
        }

        public readonly Color color;

        public override void paint(Canvas canvas, Size size) {
            Paint paint = new Paint();
            paint.color = color;
            float radius = size.width / 2.0f;
            Rect circle = Rect.fromCircle(center: new Offset(radius, radius), radius: radius);
            Rect point = Rect.fromLTWH(0.0f, 0.0f, radius, radius);
            Path path = new Path();
            path.addOval(circle);
            path.addRect(point);
            canvas.drawPath(path, paint);
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            return color != ((_TextSelectionHandlePainter) oldPainter).color;
        }
    }

    class _MaterialTextSelectionControls : TextSelectionControls {
        public static readonly TextSelectionControls materialTextSelectionControls =
            new _MaterialTextSelectionControls();

        public override Size getHandleSize(float textLineHeight) {
            return new Size(MaterialUtils._kHandleSize,
                MaterialUtils._kHandleSize);
        }

        public override Widget buildToolbar(BuildContext context, Rect globalEditableRegion, float textLineHeight,
            Offset selectionMidpoint, List<TextSelectionPoint> endpoints, TextSelectionDelegate selectionDelegate) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            D.assert(material_.debugCheckHasMaterialLocalizations(context));

            TextSelectionPoint startTextSelectionPoint = endpoints[0];
            TextSelectionPoint endTextSelectionPoint = endpoints.Count > 1
                ? endpoints[1]
                : endpoints[0];
            const float closedToolbarHeightNeeded = MaterialUtils._kToolbarScreenPadding
                                                    + MaterialUtils._kToolbarHeight
                                                    + MaterialUtils._kToolbarContentDistance;
            float paddingTop = MediaQuery.of(context).padding.top;
            float availableHeight = globalEditableRegion.top
                                    + startTextSelectionPoint.point.dy
                                    - textLineHeight
                                    - paddingTop;
            bool fitsAbove = closedToolbarHeightNeeded <= availableHeight;
            Offset anchor = new Offset(
                globalEditableRegion.left + selectionMidpoint.dx,
                fitsAbove
                    ? globalEditableRegion.top + startTextSelectionPoint.point.dy - textLineHeight -
                      MaterialUtils._kToolbarContentDistance
                    : globalEditableRegion.top + endTextSelectionPoint.point.dy +
                      MaterialUtils._kToolbarContentDistanceBelow
            );

            return new Stack(
                children: new List<Widget>() {
                    new CustomSingleChildLayout(
                        layoutDelegate: new _TextSelectionToolbarLayout(
                            anchor,
                            MaterialUtils._kToolbarScreenPadding + paddingTop,
                            fitsAbove
                        ),
                        child: new _TextSelectionToolbar(
                            handleCut: canCut(selectionDelegate)
                                ? () => handleCut(selectionDelegate)
                                : (VoidCallback) null,
                            handleCopy: canCopy(selectionDelegate)
                                ? () => handleCopy(selectionDelegate)
                                : (VoidCallback) null,
                            handlePaste: canPaste(selectionDelegate)
                                ? () => handlePaste(selectionDelegate)
                                : (VoidCallback) null,
                            handleSelectAll: canSelectAll(selectionDelegate)
                                ? () => handleSelectAll(selectionDelegate)
                                : (VoidCallback) null,
                            isAbove: fitsAbove
                        )
                    ),
                }
            );
        }

        public override Widget buildHandle(BuildContext context, TextSelectionHandleType type, float textLineHeight) {
            Widget handle = new SizedBox(
                width: MaterialUtils._kHandleSize,
                height: MaterialUtils._kHandleSize,
                child: new CustomPaint(
                    painter: new _TextSelectionHandlePainter(
                        color: Theme.of(context).textSelectionHandleColor
                    )
                )
            );

            switch (type) {
                case TextSelectionHandleType.left: // points up-right
                    return new Transform(
                        transform: Matrix4.rotationZ(Mathf.PI / 2),
                        child: handle
                    );
                case TextSelectionHandleType.right: // points up-left
                    return handle;
                case TextSelectionHandleType.collapsed: // points up
                    return new Transform(
                        transform: Matrix4.rotationZ(Mathf.PI / 4),
                        child: handle
                    );
            }

            return null;
        }

        public override Offset getHandleAnchor(TextSelectionHandleType type, float textLineHeight) {
           return Offset.zero;
        }

        new bool canSelectAll(TextSelectionDelegate selectionDelegate) {
            TextEditingValue value = selectionDelegate.textEditingValue;
            return selectionDelegate.selectAllEnabled &&
                   value.text.isNotEmpty() &&
                   !(value.selection.start == 0 && value.selection.end == value.text.Length);
        }
    }
}