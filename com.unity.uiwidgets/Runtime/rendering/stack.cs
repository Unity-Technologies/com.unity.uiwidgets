using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class RelativeRect : IEquatable<RelativeRect> {
        RelativeRect(float left, float top, float right, float bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public readonly float left;
        public readonly float top;
        public readonly float right;
        public readonly float bottom;

        public static RelativeRect fromLTRB(float left, float top, float right, float bottom) {
            return new RelativeRect(left, top, right, bottom);
        }

        public static RelativeRect fromSize(Rect rect, Size container) {
            return new RelativeRect(
                rect.left,
                rect.top,
                container.width - rect.right,
                container.height - rect.bottom);
        }

        public static RelativeRect fromRect(Rect rect, Rect container) {
            return fromLTRB(
                rect.left - container.left,
                rect.top - container.top,
                container.right - rect.right,
                container.bottom - rect.bottom
            );
        }

        public static readonly RelativeRect fill = fromLTRB(0.0f, 0.0f, 0.0f, 0.0f);

        public bool hasInsets {
            get { return left > 0.0 || top > 0.0 || right > 0.0 || bottom > 0.0; }
        }

        public RelativeRect shift(Offset offset) {
            return fromLTRB(
                left + offset.dx,
                top + offset.dy,
                right - offset.dx,
                bottom - offset.dy);
        }

        public RelativeRect inflate(float delta) {
            return fromLTRB(
                left - delta,
                top - delta,
                right - delta,
                bottom - delta);
        }

        public RelativeRect deflate(float delta) {
            return inflate(-delta);
        }

        public RelativeRect intersect(RelativeRect other) {
            return fromLTRB(
                Mathf.Max(left, other.left),
                Mathf.Max(top, other.top),
                Mathf.Max(right, other.right),
                Mathf.Max(bottom, other.bottom)
            );
        }

        public Rect toRect(Rect container) {
            return Rect.fromLTRB(
                left + container.left,
                top + container.top,
                container.right - right,
                container.bottom - bottom);
        }

        public Rect toSize(Size container) {
            return Rect.fromLTRB(
                left,
                top,
                container.width - right,
                container.height - bottom);
        }

        public static RelativeRect lerp(RelativeRect a, RelativeRect b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return fromLTRB(b.left * t, b.top * t, b.right * t, b.bottom * t);
            }

            if (b == null) {
                float k = 1.0f - t;
                return fromLTRB(b.left * k, b.top * k, b.right * k, b.bottom * k);
            }

            return fromLTRB(
                MathUtils.lerpFloat(a.left, b.left, t),
                MathUtils.lerpFloat(a.top, b.top, t),
                MathUtils.lerpFloat(a.right, b.right, t),
                MathUtils.lerpFloat(a.bottom, b.bottom, t)
            );
        }

        public bool Equals(RelativeRect other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return left.Equals(other.left)
                   && top.Equals(other.top)
                   && right.Equals(other.right)
                   && bottom.Equals(other.bottom);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((RelativeRect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = left.GetHashCode();
                hashCode = (hashCode * 397) ^ top.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ bottom.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RelativeRect a, RelativeRect b) {
            return Equals(a, b);
        }

        public static bool operator !=(RelativeRect a, RelativeRect b) {
            return !(a == b);
        }
    }

    public class StackParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
        public float? top;
        public float? right;
        public float? bottom;
        public float? left;
        public float? width;
        public float? height;

        public bool isPositioned {
            get {
                return top != null || right != null || bottom != null || left != null ||
                       width != null || height != null;
            }
        }

        RelativeRect rect {
            get {
                return RelativeRect.fromLTRB(left ?? 0.0f, top ?? 0.0f, right ?? 0.0f,
                    bottom ?? 0.0f);
            }
            set {
                top = value.top;
                right = value.right;
                bottom = value.bottom;
                left = value.left;
            }
        }
    }

    public enum StackFit {
        loose,
        expand,
        passthrough,
    }

    public enum Overflow {
        visible,
        clip,
    }

    public class RenderStack : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        StackParentData> {
        public RenderStack(
            StackFit? fit,
            Overflow? overflow,
            List<RenderBox> children = null,
            Alignment alignment = null) {
            _alignment = alignment ?? Alignment.topLeft;
            _fit = fit ?? StackFit.loose;
            _overflow = overflow ?? Overflow.clip;
            addAll(children);
        }

        bool _hasVisualOverflow = false;

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is StackParentData)) {
                child.parentData = new StackParentData();
            }
        }

        Alignment _alignment;

        public Alignment alignment {
            get { return _alignment; }
            set {
                if (_alignment == value) {
                    return;
                }

                _alignment = value;
                markNeedsLayout();
            }
        }

        StackFit _fit;

        public StackFit fit {
            get { return _fit; }
            set {
                if (_fit == value) {
                    return;
                }

                _fit = value;
                markNeedsLayout();
            }
        }

        Overflow _overflow;

        public Overflow overflow {
            get { return _overflow; }
            set {
                if (_overflow == value) {
                    return;
                }

                _overflow = value;
                markNeedsPaint();
            }
        }

        public static float getIntrinsicDimension(RenderBox firstChild, mainChildSizeGetter getter) {
            float extent = 0;
            RenderBox child = firstChild;
            while (child != null) {
                StackParentData childParentData = child.parentData as StackParentData;
                if (!childParentData.isPositioned)
                    extent = Math.Max(extent, getter(child));
                D.assert(child.parentData == childParentData);
                child = childParentData.nextSibling;
            }

            return extent;
        }

        public delegate float mainChildSizeGetter(RenderBox child);

        float _getIntrinsicDimension(mainChildSizeGetter getter) {
            float extent = 0.0f;
            RenderBox child = firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;
                if (!childParentData.isPositioned) {
                    extent = Mathf.Max(extent, getter(child));
                }

                D.assert(child.parentData == childParentData);
                if (childParentData != null) {
                    child = childParentData.nextSibling;
                }
            }

            return extent;
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            return _getIntrinsicDimension((RenderBox child) => child.getMinIntrinsicWidth(height));
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            return _getIntrinsicDimension((RenderBox child) => child.getMaxIntrinsicWidth(height));
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return _getIntrinsicDimension((RenderBox child) => child.getMinIntrinsicHeight(width));
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return _getIntrinsicDimension((RenderBox child) => child.getMaxIntrinsicHeight(width));
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return defaultComputeDistanceToHighestActualBaseline(baseline);
        }

        public static bool layoutPositionedChild(RenderBox child, StackParentData childParentData, Size size,
            Alignment alignment) {
            D.assert(childParentData.isPositioned);
            D.assert(child.parentData == childParentData);

            bool hasVisualOverflow = false;
            BoxConstraints childConstraints = new BoxConstraints();

            if (childParentData.left != null && childParentData.right != null)
                childConstraints =
                    childConstraints.tighten(width: size.width - childParentData.right - childParentData.left);
            else if (childParentData.width != null)
                childConstraints = childConstraints.tighten(width: childParentData.width);

            if (childParentData.top != null && childParentData.bottom != null)
                childConstraints =
                    childConstraints.tighten(height: size.height - childParentData.bottom - childParentData.top);
            else if (childParentData.height != null)
                childConstraints = childConstraints.tighten(height: childParentData.height);

            child.layout(childConstraints, parentUsesSize: true);

            float? x;
            if (childParentData.left != null) {
                x = childParentData.left;
            }
            else if (childParentData.right != null) {
                x = size.width - childParentData.right - child.size.width;
            }
            else {
                x = alignment.alongOffset(size - child.size as Offset).dx;
            }

            if (x < 0.0 || x + child.size.width > size.width)
                hasVisualOverflow = true;

            float? y;
            if (childParentData.top != null) {
                y = childParentData.top;
            }
            else if (childParentData.bottom != null) {
                y = size.height - childParentData.bottom - child.size.height;
            }
            else {
                y = alignment.alongOffset(size - child.size as Offset).dy;
            }

            if (y < 0.0 || y + child.size.height > size.height)
                hasVisualOverflow = true;

            childParentData.offset = new Offset(x ?? 0, y ?? 0);

            return hasVisualOverflow;
        }

        protected override void performLayout() {
            _hasVisualOverflow = false;
            bool hasNonPositionedChildren = false;
            if (childCount == 0) {
                size = constraints.biggest;
                return;
            }

            float width = constraints.minWidth;
            float height = constraints.minHeight;

            BoxConstraints nonPositionedConstraints = null;
            switch (fit) {
                case StackFit.loose:
                    nonPositionedConstraints = constraints.loosen();
                    break;
                case StackFit.expand:
                    nonPositionedConstraints = BoxConstraints.tight(constraints.biggest);
                    break;
                case StackFit.passthrough:
                    nonPositionedConstraints = constraints;
                    break;
            }


            RenderBox child = firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;

                if (!childParentData.isPositioned) {
                    hasNonPositionedChildren = true;

                    child.layout(nonPositionedConstraints, parentUsesSize: true);

                    Size childSize = child.size;
                    width = Mathf.Max(width, childSize.width);
                    height = Mathf.Max(height, childSize.height);
                }

                child = childParentData.nextSibling;
            }

            if (hasNonPositionedChildren) {
                size = new Size(width, height);
                D.assert(size.width == constraints.constrainWidth(width));
                D.assert(size.height == constraints.constrainHeight(height));
            }
            else {
                size = constraints.biggest;
            }

            child = firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;

                if (!childParentData.isPositioned) {
                    childParentData.offset = _alignment.alongOffset(size - child.size);
                }
                else {
                    BoxConstraints childConstraints = new BoxConstraints();

                    if (childParentData.left != null && childParentData.right != null) {
                        childConstraints =
                            childConstraints.tighten(
                                width: size.width - childParentData.right - childParentData.left);
                    }
                    else if (childParentData.width != null) {
                        childConstraints = childConstraints.tighten(width: childParentData.width);
                    }

                    if (childParentData.top != null && childParentData.bottom != null) {
                        childConstraints =
                            childConstraints.tighten(
                                height: size.height - childParentData.bottom - childParentData.top);
                    }
                    else if (childParentData.height != null) {
                        childConstraints = childConstraints.tighten(height: childParentData.height);
                    }

                    child.layout(childConstraints, parentUsesSize: true);

                    float x;
                    if (childParentData.left != null) {
                        x = childParentData.left.Value;
                    }
                    else if (childParentData.right != null) {
                        x = size.width - childParentData.right.Value - child.size.width;
                    }
                    else {
                        x = _alignment.alongOffset(size - child.size).dx;
                    }

                    if (x < 0.0 || x + child.size.width > size.width) {
                        _hasVisualOverflow = true;
                    }

                    float y;
                    if (childParentData.top != null) {
                        y = childParentData.top.Value;
                    }
                    else if (childParentData.bottom != null) {
                        y = size.height - childParentData.bottom.Value - child.size.height;
                    }
                    else {
                        y = _alignment.alongOffset(size - child.size).dy;
                    }

                    if (y < 0.0 || y + child.size.height > size.height) {
                        _hasVisualOverflow = true;
                    }

                    childParentData.offset = new Offset(x, y);
                }

                D.assert(child.parentData == childParentData);
                child = childParentData.nextSibling;
            }
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return defaultHitTestChildren(result, position: position);
        }

        public virtual void paintStack(PaintingContext context, Offset offset) {
            defaultPaint(context, offset);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_overflow == Overflow.clip && _hasVisualOverflow) {
                context.pushClipRect(needsCompositing, offset, Offset.zero & size, paintStack);
            }
            else {
                paintStack(context, offset);
            }
        }

        public override Rect describeApproximatePaintClip(RenderObject childRaw) {
            return _hasVisualOverflow ? Offset.zero & size : null;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Alignment>("alignment", alignment));
            properties.add(new EnumProperty<StackFit>("fit", fit));
            properties.add(new EnumProperty<Overflow>("overflow", overflow));
        }
    }

    class RenderIndexedStack : RenderStack {
        public RenderIndexedStack(
            List<RenderBox> children = null,
            Alignment alignment = null,
            int? index = 0
        ) : base(fit: null, overflow: null, children: children, alignment: alignment ?? Alignment.topLeft) {
            _index = index;
        }

        public int? index {
            get { return _index; }
            set {
                if (_index != value) {
                    _index = value;
                    markNeedsLayout();
                }
            }
        }

        int? _index;

        RenderBox _childAtIndex() {
            D.assert(index != null);
            RenderBox child = firstChild;
            int i = 0;
            while (child != null && i < index) {
                StackParentData childParentData = (StackParentData) child.parentData;
                child = childParentData.nextSibling;
                i += 1;
            }

            D.assert(i == index);
            D.assert(child != null);
            return child;
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position) {
            if (firstChild == null || index == null) {
                return false;
            }

            D.assert(position != null);
            RenderBox child = _childAtIndex();
            StackParentData childParentData = (StackParentData) child.parentData;
            return result.addWithPaintOffset(
                offset: childParentData.offset,
                position: position,
                hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                    D.assert(transformed == position - childParentData.offset);
                    return child.hitTest(resultIn, position: transformed);
                }
            );
        }

        public override void paintStack(PaintingContext context, Offset offset) {
            if (firstChild == null || index == null) {
                return;
            }

            RenderBox child = _childAtIndex();
            StackParentData childParentData = (StackParentData) child.parentData;
            context.paintChild(child, childParentData.offset + offset);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("index", index));
        }
    }
}