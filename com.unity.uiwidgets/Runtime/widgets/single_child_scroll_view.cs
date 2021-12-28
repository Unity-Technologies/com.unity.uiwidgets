using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class SingleChildScrollView : StatelessWidget {
        public SingleChildScrollView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            EdgeInsetsGeometry padding = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            ScrollController controller = null,
            Widget child = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(!(controller != null && primary == true),
                () =>
                    "Primary ScrollViews obtain their ScrollController via inheritance from a PrimaryScrollController widget. " +
                    "You cannot both set primary to true and pass an explicit controller.");
            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.padding = padding;
            this.primary = primary ?? controller == null && scrollDirection == Axis.vertical;
            this.physics = physics;
            this.controller = controller;
            this.child = child;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly Axis scrollDirection;

        public readonly bool reverse;

        public readonly EdgeInsetsGeometry padding;

        public readonly ScrollController controller;

        public readonly bool primary;

        public readonly ScrollPhysics physics;

        public readonly Widget child;

        public readonly DragStartBehavior dragStartBehavior;

        AxisDirection _getDirection(BuildContext context) {
            return AxisDirectionUtils.getAxisDirectionFromAxisReverseAndDirectionality(context, scrollDirection,
                reverse) ?? AxisDirection.down;
        }

        public override Widget build(BuildContext context) {
            AxisDirection axisDirection = _getDirection(context);
            Widget contents = child;
            if (padding != null) {
                contents = new Padding(
                    padding: padding,
                    child: contents);
            }

            ScrollController scrollController = primary
                ? PrimaryScrollController.of(context)
                : controller;

            Scrollable scrollable = new Scrollable(
                dragStartBehavior: dragStartBehavior,
                axisDirection: axisDirection,
                controller: scrollController,
                physics: physics,
                viewportBuilder: (BuildContext subContext, ViewportOffset offset) => {
                    return new _SingleChildViewport(
                        axisDirection: axisDirection,
                        offset: offset,
                        child: contents);
                }
            );

            if (primary && scrollController != null) {
                return PrimaryScrollController.none(child: scrollable);
            }

            return scrollable;
        }
    }


    class _SingleChildViewport : SingleChildRenderObjectWidget {
        public _SingleChildViewport(
            Key key = null,
            AxisDirection axisDirection = AxisDirection.down,
            ViewportOffset offset = null,
            Widget child = null
        ) : base(key: key, child: child) {
            this.axisDirection = axisDirection;
            this.offset = offset;
        }

        public readonly AxisDirection axisDirection;

        public readonly ViewportOffset offset;


        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSingleChildViewport(
                axisDirection: axisDirection,
                offset: offset
            );
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderSingleChildViewport _renderObject = (_RenderSingleChildViewport) renderObject;
            _renderObject.axisDirection = axisDirection;
            _renderObject.offset = offset;
        }
    }


    class _RenderSingleChildViewport : RenderObjectWithChildMixinRenderBox<RenderBox>, RenderAbstractViewport {
        public _RenderSingleChildViewport(
            AxisDirection axisDirection = AxisDirection.down,
            ViewportOffset offset = null,
            float cacheExtent = RenderViewportUtils.defaultCacheExtent,
            RenderBox child = null) {
            D.assert(offset != null);
            _axisDirection = axisDirection;
            _offset = offset;
            _cacheExtent = cacheExtent;
            this.child = child;
        }

        public new RenderObject parent {
            get { return (RenderObject) base.parent; }
        }

        public AxisDirection axisDirection {
            get { return _axisDirection; }
            set {
                if (value == _axisDirection) {
                    return;
                }

                _axisDirection = value;
                markNeedsLayout();
            }
        }

        AxisDirection _axisDirection;

        public Axis? axis {
            get { return AxisUtils.axisDirectionToAxis(axisDirection); }
        }

        public ViewportOffset offset {
            get { return _offset; }
            set {
                D.assert(value != null);
                if (value == _offset) {
                    return;
                }

                if (attached) {
                    _offset.removeListener(_hasScrolled);
                }

                _offset = value;
                if (attached) {
                    _offset.addListener(_hasScrolled);
                }

                markNeedsLayout();
            }
        }

        ViewportOffset _offset;

        public float cacheExtent {
            get { return _cacheExtent; }
            set {
                if (value == _cacheExtent) {
                    return;
                }

                _cacheExtent = value;
                markNeedsLayout();
            }
        }

        float _cacheExtent;

        void _hasScrolled() {
            markNeedsPaint();
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is ParentData)) {
                child.parentData = new ParentData();
            }
        }

        public override void attach(object owner) {
            base.attach(owner);
            _offset.addListener(_hasScrolled);
        }

        public override void detach() {
            _offset.removeListener(_hasScrolled);
            base.detach();
        }

        public override bool isRepaintBoundary {
            get { return true; }
        }


        float _viewportExtent {
            get {
                D.assert(hasSize);
                switch (axis) {
                    case Axis.horizontal:
                        return size.width;
                    case Axis.vertical:
                        return size.height;
                }

                D.assert(false);
                return 0.0f;
            }
        }

        float _minScrollExtent {
            get {
                D.assert(hasSize);
                return 0.0f;
            }
        }

        float _maxScrollExtent {
            get {
                D.assert(hasSize);
                if (child == null) {
                    return 0.0f;
                }

                switch (axis) {
                    case Axis.horizontal:
                        return Mathf.Max(0.0f, child.size.width - size.width);
                    case Axis.vertical:
                        return Mathf.Max(0.0f, child.size.height - size.height);
                }

                D.assert(false);
                return 0.0f;
            }
        }

        BoxConstraints _getInnerConstraints(BoxConstraints constraints) {
            switch (axis) {
                case Axis.horizontal:
                    return constraints.heightConstraints();
                case Axis.vertical:
                    return constraints.widthConstraints();
            }

            return null;
        }


        protected internal override float computeMinIntrinsicWidth(float height) {
            if (child != null) {
                return child.getMinIntrinsicWidth(height);
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            if (child != null) {
                return child.getMaxIntrinsicWidth(height);
            }

            return 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            if (child != null) {
                return child.getMinIntrinsicHeight(width);
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (child != null) {
                return child.getMaxIntrinsicHeight(width);
            }

            return 0.0f;
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            if (child == null) {
                size = constraints.smallest;
            }
            else {
                child.layout(_getInnerConstraints(constraints), parentUsesSize: true);
                size = constraints.constrain(child.size);
            }

            offset.applyViewportDimension(_viewportExtent);
            offset.applyContentDimensions(_minScrollExtent, _maxScrollExtent);
        }

        Offset _paintOffset {
            get { return _paintOffsetForPosition(offset.pixels); }
        }

        Offset _paintOffsetForPosition(float position) {
            switch (axisDirection) {
                case AxisDirection.up:
                    return new Offset(0.0f, position - child.size.height + size.height);
                case AxisDirection.down:
                    return new Offset(0.0f, -position);
                case AxisDirection.left:
                    return new Offset(position - child.size.width + size.width, 0.0f);
                case AxisDirection.right:
                    return new Offset(-position, 0.0f);
            }

            return null;
        }

        bool _shouldClipAtPaintOffset(Offset paintOffset) {
            D.assert(child != null);
            return paintOffset < Offset.zero ||
                   !(Offset.zero & size).contains((paintOffset & child.size).bottomRight);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                Offset paintOffset = _paintOffset;

                void paintContents(PaintingContext subContext, Offset SubOffset) {
                    subContext.paintChild(child, SubOffset + paintOffset);
                }

                if (_shouldClipAtPaintOffset(paintOffset)) {
                    context.pushClipRect(needsCompositing, offset, Offset.zero & size, paintContents);
                }
                else {
                    paintContents(context, offset);
                }
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            Offset paintOffset = _paintOffset;
            transform.translate(paintOffset.dx, paintOffset.dy);
        }

        public override Rect describeApproximatePaintClip(RenderObject child) {
            if (child != null && _shouldClipAtPaintOffset(_paintOffset)) {
                return Offset.zero & size;
            }

            return null;
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            if (child != null) {
                return result.addWithPaintOffset(
                    offset: _paintOffset,
                    position: position,
                    hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                        D.assert(transformed == position + (-_paintOffset));
                        return child.hitTest(result, position: transformed);
                    }
                );
            }

            return false;
        }


        public RevealedOffset getOffsetToReveal(RenderObject target, float alignment, Rect rect = null) {
            rect = rect ?? target.paintBounds;
            if (!(target is RenderBox)) {
                return new RevealedOffset(offset: offset.pixels, rect: rect);
            }

            RenderBox targetBox = (RenderBox) target;
            Matrix4 transform = targetBox.getTransformTo(child);
            Rect bounds = MatrixUtils.transformRect(transform, rect);
            Size contentSize = child.size;

            float leadingScrollOffset = 0.0f;
            float targetMainAxisExtent = 0.0f;
            float mainAxisExtent = 0.0f;

            switch (axisDirection) {
                case AxisDirection.up:
                    mainAxisExtent = size.height;
                    leadingScrollOffset = contentSize.height - bounds.bottom;
                    targetMainAxisExtent = bounds.height;
                    break;
                case AxisDirection.right:
                    mainAxisExtent = size.width;
                    leadingScrollOffset = bounds.left;
                    targetMainAxisExtent = bounds.width;
                    break;
                case AxisDirection.down:
                    mainAxisExtent = size.height;
                    leadingScrollOffset = bounds.top;
                    targetMainAxisExtent = bounds.height;
                    break;
                case AxisDirection.left:
                    mainAxisExtent = size.width;
                    leadingScrollOffset = contentSize.width - bounds.right;
                    targetMainAxisExtent = bounds.width;
                    break;
            }

            float targetOffset = leadingScrollOffset - (mainAxisExtent - targetMainAxisExtent) * alignment;
            Rect targetRect = bounds.shift(_paintOffsetForPosition(targetOffset));
            return new RevealedOffset(offset: targetOffset, rect: targetRect);
        }

        public override void showOnScreen(
            RenderObject descendant = null,
            Rect rect = null,
            TimeSpan? duration = null,
            Curve curve = null
        ) {
            if (!offset.allowImplicitScrolling) {
                base.showOnScreen(
                    descendant: descendant,
                    rect: rect,
                    duration: duration,
                    curve: curve
                );
            }

            Rect newRect = RenderViewport.showInViewport(
                descendant: descendant,
                viewport: this,
                offset: offset,
                rect: rect,
                duration: duration,
                curve: curve);

            base.showOnScreen(
                rect: newRect,
                duration: duration,
                curve: curve);
        }
    }
}