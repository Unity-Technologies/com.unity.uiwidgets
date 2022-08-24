using UIWidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public abstract class RenderShiftedBox : RenderObjectWithChildMixinRenderBox<RenderBox> {
        protected RenderShiftedBox(RenderBox child) {
            this.child = child;
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

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            float? result;

            if (child != null) {
                D.assert(!debugNeedsLayout);

                result = child.getDistanceToActualBaseline(baseline);
                var childParentData = (BoxParentData) child.parentData;
                if (result != null) {
                    result += childParentData.offset.dy;
                }
            }
            else {
                result = base.computeDistanceToActualBaseline(baseline);
            }

            return result;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                var childParentData = (BoxParentData) child.parentData;
                context.paintChild(child, childParentData.offset + offset);
            }
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            if (child != null) {
                var childParentData = (BoxParentData) child.parentData;
                return result.addWithPaintOffset(
                    offset: childParentData.offset,
                    position: position,
                    hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                        D.assert(transformed == position - childParentData.offset);
                        return child.hitTest(resultIn, position: transformed);
                    }
                );
            }

            return false;
        }
    }

    public class RenderPadding : RenderShiftedBox {
        public RenderPadding(
            EdgeInsetsGeometry padding = null,
            TextDirection? textDirection = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(padding != null);
            D.assert(padding.isNonNegative);
            _textDirection = textDirection;
            _padding = padding;
        }
        EdgeInsets _resolvedPadding;
        
        void _resolve() {
            if (_resolvedPadding != null)
                return;
            _resolvedPadding = padding.resolve(textDirection);
            D.assert(_resolvedPadding.isNonNegative);
        }

        void _markNeedResolution() {
            _resolvedPadding = null;
            markNeedsLayout();
        }
        public EdgeInsetsGeometry padding {
            get { return _padding; }
            set {
                D.assert(value != null);
                D.assert(value.isNonNegative);

                if (_padding == value) {
                    return;
                }

                _padding = value;
                markNeedsLayout();
            }
        }
        EdgeInsetsGeometry _padding;

        public TextDirection? textDirection {
            get { return _textDirection; }
            set {if (_textDirection == value)
                    return;
                _textDirection = value;
                _markNeedResolution(); }
        }
        TextDirection? _textDirection;
       

        protected internal override float computeMinIntrinsicWidth(float height) {
            _resolve();
            float totalHorizontalPadding = _resolvedPadding.left + _resolvedPadding.right;
            float totalVerticalPadding = _resolvedPadding.top + _resolvedPadding.bottom;
            if (child != null) // next line relies on double.infinity absorption
                return child.getMinIntrinsicWidth(Mathf.Max(0.0f, height - totalVerticalPadding)) + totalHorizontalPadding;
            return totalHorizontalPadding;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            _resolve();
            float totalHorizontalPadding = _resolvedPadding.left + _resolvedPadding.right;
            float totalVerticalPadding = _resolvedPadding.top + _resolvedPadding.bottom;
            if (child != null) // next line relies on double.infinity absorption
                return child.getMaxIntrinsicWidth(Mathf.Max(0.0f, height - totalVerticalPadding)) + totalHorizontalPadding;
            return totalHorizontalPadding;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            _resolve();
            float totalHorizontalPadding = _resolvedPadding.left + _resolvedPadding.right;
            float totalVerticalPadding = _resolvedPadding.top + _resolvedPadding.bottom;
            if (child != null) // next line relies on double.infinity absorption
                return child.getMinIntrinsicHeight(Mathf.Max(0.0f, width - totalHorizontalPadding)) + totalVerticalPadding;
            return totalVerticalPadding;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            _resolve();
            float totalHorizontalPadding = _resolvedPadding.left + _resolvedPadding.right;
            float totalVerticalPadding = _resolvedPadding.top + _resolvedPadding.bottom;
            if (child != null) // next line relies on double.infinity absorption
                return child.getMaxIntrinsicHeight(Mathf.Max(0.0f, width - totalHorizontalPadding)) + totalVerticalPadding;
            return totalVerticalPadding;
        }

        protected override void performLayout() {
             BoxConstraints constraints = this.constraints;
            _resolve();
            D.assert(_resolvedPadding != null);
            if (child == null) {
                size = constraints.constrain(new Size(
                    _resolvedPadding.left + _resolvedPadding.right,
                    _resolvedPadding.top + _resolvedPadding.bottom
                ));
                return;
            }
             BoxConstraints innerConstraints = constraints.deflate(_resolvedPadding);
            child.layout(innerConstraints, parentUsesSize: true);
             BoxParentData childParentData = child.parentData as BoxParentData;
            childParentData.offset = new Offset(_resolvedPadding.left, _resolvedPadding.top);
            size = constraints.constrain(new Size(
                _resolvedPadding.left + child.size.width + _resolvedPadding.right,
                _resolvedPadding.top + child.size.height + _resolvedPadding.bottom
            ));
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            base.debugPaintSize(context, offset);
            D.assert(()=> {
                Rect outerRect = offset & size;
                D.debugPaintPadding(context.canvas, outerRect, child != null ? _resolvedPadding.deflateRect(outerRect) : null);
                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding));
            properties.add(new EnumProperty<TextDirection>("textDirection", (TextDirection)textDirection, defaultValue: null));
        }
    }

    public abstract class RenderAligningShiftedBox : RenderShiftedBox {
        protected RenderAligningShiftedBox(
            AlignmentGeometry  alignment = null,
            TextDirection? textDirection = null,
            RenderBox child = null
        ) : base(child) {
            _alignment = alignment ?? Alignment.center;
            _textDirection = textDirection;
        }
        
        Alignment _resolvedAlignment;

        void _resolve() {
            if (_resolvedAlignment != null)
                return;
            _resolvedAlignment = alignment.resolve(textDirection);
        }

        void _markNeedResolution() {
            _resolvedAlignment = null;
            markNeedsLayout();
        }

        public AlignmentGeometry alignment {
            get { return _alignment; }
            set {
                D.assert(value != null);
                if (_alignment == value) {
                    return;
                }

                _alignment = value;
                _markNeedResolution();
            }
        }

        AlignmentGeometry _alignment;

        public TextDirection? textDirection {
            get { return _textDirection;}
            set {
                if (_textDirection == value)
                    return;
                _textDirection = value;
                _markNeedResolution();
            }
        }
        TextDirection? _textDirection;
        
        protected void alignChild() {
            _resolve();
            D.assert(child != null);
            D.assert(!child.debugNeedsLayout);
            D.assert(child.hasSize);
            D.assert(hasSize);
            D.assert(_resolvedAlignment != null);
            var childParentData = (BoxParentData) child.parentData;
            childParentData.offset = _resolvedAlignment.alongOffset(size - child.size);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new EnumProperty<TextDirection>("textDirection", (TextDirection)textDirection, defaultValue: null));
        
        }
    }

    public class RenderPositionedBox : RenderAligningShiftedBox {
        public RenderPositionedBox(
            RenderBox child = null,
            float? widthFactor = null,
            float? heightFactor = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null
        ) : base(child: child, alignment: alignment ?? Alignment.center, textDirection: textDirection){
            D.assert(widthFactor == null || widthFactor >= 0.0);
            D.assert(heightFactor == null || heightFactor >= 0.0);
            _widthFactor = widthFactor;
            _heightFactor = heightFactor;
        }

        public float? widthFactor {
            get { return _widthFactor; }
            set {
                D.assert(value == null || value >= 0.0);
                if (_widthFactor == value) {
                    return;
                }

                _widthFactor = value;
                markNeedsLayout();
            }
        }

        float? _widthFactor;

        public float? heightFactor {
            get { return _heightFactor; }
            set {
                D.assert(value == null || value >= 0.0);
                if (_heightFactor == value) {
                    return;
                }

                _heightFactor = value;
                markNeedsLayout();
            }
        }

        float? _heightFactor;

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            bool shrinkWrapWidth = _widthFactor != null || float.IsPositiveInfinity(constraints.maxWidth);
            bool shrinkWrapHeight = _heightFactor != null || float.IsPositiveInfinity(constraints.maxHeight);

            if (child != null) {
                child.layout(constraints.loosen(), parentUsesSize: true);
                size = constraints.constrain(new Size(
                    shrinkWrapWidth ? child.size.width * (_widthFactor ?? 1.0f) : float.PositiveInfinity,
                    shrinkWrapHeight ? child.size.height * (_heightFactor ?? 1.0f) : float.PositiveInfinity));
                alignChild();
            }
            else {
                size = constraints.constrain(new Size(
                    shrinkWrapWidth ? 0.0f : float.PositiveInfinity,
                    shrinkWrapHeight ? 0.0f : float.PositiveInfinity));
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            base.debugPaintSize(context, offset);
            D.assert(() => {
                Paint paint;
                if (child != null && !child.size.isEmpty) {
                    Path path;
                    paint = new Paint {
                        style = PaintingStyle.stroke,
                        strokeWidth = 1.0f,
                        color = new ui.Color(0xFFFFFF00)
                    };

                    BoxParentData childParentData = (BoxParentData) child.parentData;
                    if (childParentData.offset.dy > 0) {
                        float headSize = Mathf.Min(childParentData.offset.dy * 0.2f, 10.0f);

                        float x = offset.dx + size.width / 2.0f;
                        float y = offset.dy;
                        path = new Path();
                        path.moveTo(x, y);
                        path.lineTo(x += 0.0f, y += childParentData.offset.dy - headSize);
                        path.lineTo(x += headSize, y += 0.0f);
                        path.lineTo(x += -headSize, y += headSize);
                        path.lineTo(x += -headSize, y += -headSize);
                        path.lineTo(x += headSize, y += 0.0f);

                        x = offset.dx + size.width / 2.0f;
                        y = offset.dy + size.height;
                        path.moveTo(x, y);
                        path.lineTo(x += 0.0f, y += -childParentData.offset.dy + headSize);
                        path.lineTo(x += headSize, y += 0.0f);
                        path.lineTo(x += -headSize, y += -headSize);
                        path.lineTo(x += -headSize, y += headSize);
                        path.lineTo(x += headSize, y += 0.0f);
                        context.canvas.drawPath(path, paint);
                    }

                    if (childParentData.offset.dx > 0.0) {
                        float headSize = Mathf.Min(childParentData.offset.dx * 0.2f, 10.0f);
                        float x = offset.dx;
                        float y = offset.dy + size.height / 2.0f;
                        path = new Path();
                        path.moveTo(x, y);
                        path.lineTo(x += childParentData.offset.dx - headSize, y += 0.0f);
                        path.lineTo(x += 0.0f, y += headSize);
                        path.lineTo(x += headSize, y += -headSize);
                        path.lineTo(x += -headSize, y += -headSize);
                        path.lineTo(x += 0.0f, y += headSize);

                        path.moveTo(x = offset.dx + size.width, y = offset.dy + size.height / 2.0f);
                        path.lineTo(x += -childParentData.offset.dx + headSize, y += 0.0f);
                        path.lineTo(x += 0.0f, y += headSize);
                        path.lineTo(x += -headSize, y += -headSize);
                        path.lineTo(x += headSize, y += -headSize);
                        path.lineTo(x += 0.0f, y += headSize);
                        path.close();
                        context.canvas.drawPath(path, paint);
                    }
                }
                else {
                    paint = new Paint {
                        color = new ui.Color(0x90909090),
                    };
                    context.canvas.drawRect(offset & size, paint);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("widthFactor", _widthFactor, ifNull: "expand"));
            properties.add(new FloatProperty("heightFactor", _heightFactor, ifNull: "expand"));
        }
    }

    public class RenderConstrainedOverflowBox : RenderAligningShiftedBox {
        public RenderConstrainedOverflowBox(
            RenderBox child = null,
            float? minWidth = null,
            float? maxWidth = null,
            float? minHeight = null,
            float? maxHeight = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null
        ) : base(child: child, alignment: alignment ?? Alignment.center, textDirection: textDirection){
            _minWidth = minWidth;
            _maxWidth = maxWidth;
            _minHeight = minHeight;
            _maxHeight = maxHeight;
        }

        public float? minWidth {
            get { return _minWidth; }
            set {
                if (_minWidth == value) {
                    return;
                }

                _minWidth = value;
                markNeedsLayout();
            }
        }

        public float? _minWidth;

        public float? maxWidth {
            get { return _maxWidth; }
            set {
                if (_maxWidth == value) {
                    return;
                }

                _maxWidth = value;
                markNeedsLayout();
            }
        }

        public float? _maxWidth;

        public float? minHeight {
            get { return _minHeight; }
            set {
                if (_minHeight == value) {
                    return;
                }

                _minHeight = value;
                markNeedsLayout();
            }
        }

        public float? _minHeight;

        public float? maxHeight {
            get { return _maxHeight; }
            set {
                if (_maxHeight == value) {
                    return;
                }

                _maxHeight = value;
                markNeedsLayout();
            }
        }

        public float? _maxHeight;

        public BoxConstraints _getInnerConstraints(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: _minWidth ?? constraints.minWidth,
                maxWidth: _maxWidth ?? constraints.maxWidth,
                minHeight: _minHeight ?? constraints.minHeight,
                maxHeight: _maxHeight ?? constraints.maxHeight
            );
        }

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override void performResize() {
            size = constraints.biggest;
        }

        protected override void performLayout() {
            if (child != null) {
                child.layout(_getInnerConstraints(constraints), parentUsesSize: true);
                alignChild();
            }
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("minWidth", minWidth, ifNull: "use parent minWidth constraint"));
            properties.add(new FloatProperty("maxWidth", maxWidth, ifNull: "use parent maxWidth constraint"));
            properties.add(new FloatProperty("minHeight", minHeight, ifNull: "use parent minHeight constraint"));
            properties.add(new FloatProperty("maxHeight", maxHeight, ifNull: "use parent maxHeight constraint"));
        }
    }

    public class RenderUnconstrainedBox : RenderAligningShiftedBox {//, DebugOverflowIndicatorMixin
        public RenderUnconstrainedBox(
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null,
            Axis? constrainedAxis = null,
            RenderBox child = null
        ) : base(alignment , textDirection ,child) {
            D.assert(alignment != null);
            _constrainedAxis = constrainedAxis;
        }

        public Axis? constrainedAxis {
            get { return _constrainedAxis; }
            set {
                if (_constrainedAxis == value) {
                    return;
                }

                _constrainedAxis = value;
                markNeedsLayout();
            }
        }
        public Axis? _constrainedAxis;

        public Rect _overflowContainerRect = Rect.zero;
        public Rect _overflowChildRect = Rect.zero;
        public bool _isOverflowing = false;

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            if (child != null) {
                BoxConstraints childConstraints = null;
                if (constrainedAxis != null) {
                    switch (constrainedAxis) {
                        case Axis.horizontal:
                            childConstraints = new BoxConstraints(
                                maxWidth: constraints.maxWidth,
                                minWidth: constraints.minWidth);
                            break;
                        case Axis.vertical:
                            childConstraints = new BoxConstraints(
                                maxHeight: constraints.maxHeight,
                                minHeight: constraints.minHeight);
                            break;
                    }
                }
                else {
                    childConstraints = new BoxConstraints();
                }

                child.layout(childConstraints, parentUsesSize: true);
                size = constraints.constrain(child.size);
                alignChild();
                var childParentData = (BoxParentData) child.parentData;
                _overflowContainerRect = Offset.zero & size;
                _overflowChildRect = childParentData.offset & child.size;
            }
            else {
                size = constraints.smallest;
                _overflowContainerRect = Rect.zero;
                _overflowChildRect = Rect.zero;
            }

            _isOverflowing = RelativeRect.fromRect(
                _overflowContainerRect, _overflowChildRect).hasInsets;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child == null || size.isEmpty) {
                return;
            }

            if (!_isOverflowing) {
                base.paint(context, offset);
                return;
            }

            context.pushClipRect(needsCompositing, offset, Offset.zero & size, base.paint);
            D.assert(() => {
                DebugOverflowIndicatorMixin.paintOverflowIndicator(this, context, offset, _overflowContainerRect,
                    _overflowChildRect);
                return true;
            });
        }
        public override Rect describeApproximatePaintClip(RenderObject child) {
            return _isOverflowing ? Offset.zero & size : null;
        }

        public override string toStringShort() {
            string header = base.toStringShort();
            if (_isOverflowing)
                header += "OVERFLOWING";
            return header;
        }
    }

    public class RenderSizedOverflowBox : RenderAligningShiftedBox {
        public RenderSizedOverflowBox(
            RenderBox child = null,
            Size requestedSize = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null
        ) : base(child: child, alignment: alignment ?? Alignment.center, textDirection: textDirection) {
            D.assert(requestedSize != null);
            _requestedSize = requestedSize;
        }

        public Size requestedSize {
            get { return _requestedSize; }
            set {
                if (_requestedSize == value) {
                    return;
                }

                _requestedSize = value;
                markNeedsLayout();
            }
        }

        public Size _requestedSize;

        protected internal override float computeMinIntrinsicWidth(float height) {
            return _requestedSize.width;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            return _requestedSize.width;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return _requestedSize.height;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return _requestedSize.height;
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            if (child != null) {
                return child.getDistanceToActualBaseline(baseline);
            }

            return base.computeDistanceToActualBaseline(baseline);
        }

        protected override void performLayout() {
            size = constraints.constrain(_requestedSize);
            if (child != null) {
                child.layout(constraints, parentUsesSize: true);
                alignChild();
            }
        }
    }

    public class RenderFractionallySizedOverflowBox : RenderAligningShiftedBox {
        public RenderFractionallySizedOverflowBox(
            RenderBox child = null,
            float? widthFactor = null,
            float? heightFactor = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null
        ) : base(child: child, alignment: alignment ?? Alignment.center, textDirection: textDirection){
            _widthFactor = widthFactor;
            _heightFactor = heightFactor;
            D.assert(_widthFactor == null || _widthFactor >= 0.0);
            D.assert(_heightFactor == null || _heightFactor >= 0.0);
        }

        public float? widthFactor {
            get { return _widthFactor; }
            set {
                D.assert(value == null || value >= 0.0f);
                if (_widthFactor == value) {
                    return;
                }

                _widthFactor = value;
                markNeedsLayout();
            }
        }

        public float? _widthFactor;

        public float? heightFactor {
            get { return _heightFactor; }
            set {
                D.assert(value == null || value >= 0.0);
                if (_heightFactor == value) {
                    return;
                }

                _heightFactor = value;
                markNeedsLayout();
            }
        }

        public float? _heightFactor;

        public BoxConstraints _getInnerConstraints(BoxConstraints constraints) {
            float minWidth = constraints.minWidth;
            float maxWidth = constraints.maxWidth;
            if (_widthFactor != null) {
                float width = maxWidth * _widthFactor.Value;
                minWidth = width;
                maxWidth = width;
            }

            float minHeight = constraints.minHeight;
            float maxHeight = constraints.maxHeight;
            if (_heightFactor != null) {
                float height = maxHeight * _heightFactor.Value;
                minHeight = height;
                maxHeight = height;
            }

            return new BoxConstraints(
                minWidth: minWidth,
                maxWidth: maxWidth,
                minHeight: minHeight,
                maxHeight: maxHeight
            );
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            float result;
            if (child == null) {
                result = base.computeMinIntrinsicWidth(height);
            }
            else {
                result = child.getMinIntrinsicWidth(height * (_heightFactor ?? 1.0f));
            }

            return result / (_widthFactor ?? 1.0f);
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            float result;
            if (child == null) {
                result = base.computeMaxIntrinsicWidth(height);
            }
            else {
                result = child.getMaxIntrinsicWidth(height * (_heightFactor ?? 1.0f));
            }

            return result / (_widthFactor ?? 1.0f);
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            float result;
            if (child == null) {
                result = base.computeMinIntrinsicHeight(width);
            }
            else {
                result = child.getMinIntrinsicHeight(width * (_widthFactor ?? 1.0f));
            }

            return result / (_heightFactor ?? 1.0f);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float result;
            if (child == null) {
                result = base.computeMaxIntrinsicHeight(width);
            }
            else {
                result = child.getMaxIntrinsicHeight(width * (_widthFactor ?? 1.0f));
            }

            return result / (_heightFactor ?? 1.0f);
        }

        protected override void performLayout() {
            if (child != null) {
                child.layout(_getInnerConstraints(constraints), parentUsesSize: true);
                size = constraints.constrain(child.size);
                alignChild();
            }
            else {
                size = constraints.constrain(
                    _getInnerConstraints(constraints).constrain(Size.zero));
            }
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("widthFactor", _widthFactor, ifNull: "pass-through"));
            properties.add(new FloatProperty("heightFactor", _heightFactor, ifNull: "pass-through"));
        }
    }

    public abstract class SingleChildLayoutDelegate {
        public SingleChildLayoutDelegate(Listenable _relayout = null) {
            this._relayout = _relayout;
        }

        public readonly Listenable _relayout;

        public virtual Size getSize(BoxConstraints constraints) {
            return constraints.biggest;
        }

        public virtual BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints;
        }

        public virtual Offset getPositionForChild(Size size, Size childSize) {
            return Offset.zero;
        }

        public abstract bool shouldRelayout(SingleChildLayoutDelegate oldDelegate);
    }

    public class RenderCustomSingleChildLayoutBox : RenderShiftedBox {
        public RenderCustomSingleChildLayoutBox(
            RenderBox child = null,
            SingleChildLayoutDelegate layoutDelegate = null
            ) : base(child) {
            D.assert(layoutDelegate != null);
            _delegate = layoutDelegate;
        }

        public SingleChildLayoutDelegate layoutDelegate {
            get { return _delegate; }
            set {
                var newDelegate = value;
                D.assert(newDelegate != null);
                if (_delegate == newDelegate) {
                    return;
                }

                SingleChildLayoutDelegate oldDelegate = _delegate;
                if (newDelegate.GetType() != oldDelegate.GetType() || newDelegate.shouldRelayout(oldDelegate)) {
                    markNeedsLayout();
                }

                _delegate = newDelegate;
                if (attached) {
                    oldDelegate?._relayout?.removeListener(markNeedsLayout);
                    newDelegate?._relayout?.addListener(markNeedsLayout);
                }
            }
        }

        SingleChildLayoutDelegate _delegate;

        public override void attach(object owner) {
            base.attach(owner);
            _delegate?._relayout?.addListener(markNeedsLayout);
        }

        public override void detach() {
            _delegate?._relayout?.removeListener(markNeedsLayout);
            base.detach();
        }

        Size _getSize(BoxConstraints constraints) {
            return constraints.constrain(_delegate.getSize(constraints));
        }


        protected internal override float computeMinIntrinsicWidth(float height) {
            float width = _getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            float width = _getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            float height = _getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float height = _getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected override void performLayout() {
            size = _getSize(constraints);
            if (child != null) {
                BoxConstraints childConstraints = layoutDelegate.getConstraintsForChild(constraints);
                D.assert(childConstraints.debugAssertIsValid(isAppliedConstraint: true));
                child.layout(childConstraints, parentUsesSize: !childConstraints.isTight);
                BoxParentData childParentData = (BoxParentData) child.parentData;
                childParentData.offset = layoutDelegate.getPositionForChild(size,
                    childConstraints.isTight ? childConstraints.smallest : child.size);
            }
        }
    }

    public class RenderBaseline : RenderShiftedBox {
        public RenderBaseline(
            RenderBox child = null,
            float baseline = 0.0f,
            TextBaseline baselineType = TextBaseline.alphabetic
        ) : base(child) {
            _baseline = baseline;
            _baselineType = baselineType;
        }

        public float baseline {
            get { return _baseline; }
            set {
                if (_baseline == value) {
                    return;
                }
                _baseline = value;
                markNeedsLayout();
            }
        }

        public float _baseline;


        public TextBaseline baselineType {
            get { return _baselineType; }
            set {
                if (_baselineType == value) {
                    return;
                }

                _baselineType = value;
                markNeedsLayout();
            }
        }

        public TextBaseline _baselineType;

        protected override void performLayout() {
            if (child != null) {
                BoxConstraints constraints = this.constraints;
                child.layout(constraints.loosen(), parentUsesSize: true);
                float? childBaseline = child.getDistanceToBaseline(baselineType);
                float actualBaseline = baseline;
                float top = actualBaseline - (childBaseline ?? 0.0f);
                var childParentData = (BoxParentData) child.parentData;
                childParentData.offset = new Offset(0.0f, top);
                Size childSize = child.size;
                size = constraints.constrain(new Size(childSize.width, top + childSize.height));
            }
            else {
                performResize();
            }
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("baseline", baseline));
            properties.add(new EnumProperty<TextBaseline>("baselineType", baselineType));
        }
    }
}