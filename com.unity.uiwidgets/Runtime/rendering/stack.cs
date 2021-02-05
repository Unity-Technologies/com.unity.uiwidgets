using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
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
                MathUtils.lerpNullableFloat(a.left, b.left, t),
                MathUtils.lerpNullableFloat(a.top, b.top, t),
                MathUtils.lerpNullableFloat(a.right, b.right, t),
                MathUtils.lerpNullableFloat(a.bottom, b.bottom, t)
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
        
        public override string ToString() {
            return
                $"RelativeRect.fromLTRB({left:F1}, {top :F1}, {right :F1}, {bottom :F1})";
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
        public override string ToString() {
            List<string> values = new List<string>();
                if (top != null) values.Add($"top={top:F1}");
            if (right != null) values.Add($"right={right:F1}");
            if (bottom != null) values.Add($"bottom=${bottom:F1}");
            if (left != null) values.Add($"left=${left:F1}");
            if (width != null) values.Add($"width=${width:F1}");
            if (height != null) values.Add($"height=${height:F1}");
            if (values.isEmpty())
                values.Add("not positioned");
            values.Add(base.ToString());
            return string.Join("; ", values);
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
            List<RenderBox> children = null,
            AlignmentGeometry alignment =  null,
            TextDirection textDirection = TextDirection.ltr,
            StackFit fit = StackFit.loose,
            Overflow overflow = Overflow.clip
            ) {
            _textDirection = textDirection;
            _alignment = alignment ?? AlignmentDirectional.topStart;
            _fit = fit;
            _overflow = overflow;
            addAll(children);
        }

        bool _hasVisualOverflow = false;

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is StackParentData)) {
                child.parentData = new StackParentData();
            }
        }
        void _resolve() {
            if (_resolvedAlignment != null)
                return;
            _resolvedAlignment = alignment.resolve(textDirection);
        }
        
        Alignment _resolvedAlignment;

        void _markNeedResolution() {
            _resolvedAlignment = null;
            markNeedsLayout();
        }
        
        public TextDirection?  textDirection {
            get {
                return _textDirection;
            }
            set {
                if (_textDirection == value)
                    return;
                _textDirection = value;
                _markNeedResolution();

            }
        }

        TextDirection? _textDirection;
        
        
        
        AlignmentGeometry _alignment;

        public AlignmentGeometry alignment {
            get { return _alignment; }
            set {
                D.assert(value != null);
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
                D.assert(childParentData != null);
                if (!childParentData.isPositioned)
                    extent = Math.Max(extent, getter(child));
                D.assert(child.parentData == childParentData);
                child = childParentData.nextSibling;
            }

            return extent;
        }

        public delegate float mainChildSizeGetter(RenderBox child);

        protected internal override float computeMinIntrinsicWidth(float height) {
            return getIntrinsicDimension(firstChild,(RenderBox child) => child.getMinIntrinsicWidth(height));
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            return getIntrinsicDimension(firstChild,(RenderBox child) => child.getMaxIntrinsicWidth(height));
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return getIntrinsicDimension(firstChild,(RenderBox child) => child.getMinIntrinsicHeight(width));
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return getIntrinsicDimension(firstChild,(RenderBox child) => child.getMaxIntrinsicHeight(width));
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
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
            BoxConstraints constraints = this.constraints;
            _resolve();
            D.assert(_resolvedAlignment != null);
            _hasVisualOverflow = false;
            bool hasNonPositionedChildren = false;
            if (childCount == 0) {
              size = constraints.biggest;
              D.assert(size.isFinite);
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
            D.assert(nonPositionedConstraints != null);

            RenderBox child = firstChild;
            while (child != null) {
              StackParentData childParentData = child.parentData as StackParentData;
              D.assert(childParentData != null);
              if (!childParentData.isPositioned) {
                hasNonPositionedChildren = true;

                child.layout(nonPositionedConstraints, parentUsesSize: true);

                Size childSize = child.size;
                width = Mathf.Max(width, childSize.width);
                height =  Mathf.Max(height, childSize.height);
              }

              child = childParentData.nextSibling;
            }

            if (hasNonPositionedChildren) {
              size = new Size(width, height);
              D.assert(size.width == constraints.constrainWidth(width));
              D.assert(size.height == constraints.constrainHeight(height));
            } else {
              size = constraints.biggest;
            }

            D.assert(size.isFinite);

            child = firstChild;
            while (child != null) {
              StackParentData childParentData = child.parentData as StackParentData;

              if (!childParentData.isPositioned) {
                childParentData.offset = _resolvedAlignment.alongOffset(size - child.size as Offset);
              } else {
                _hasVisualOverflow = layoutPositionedChild(child, childParentData, size, _resolvedAlignment) || _hasVisualOverflow;
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
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new EnumProperty<TextDirection>("textDirection", textDirection?? TextDirection.ltr));
            properties.add(new EnumProperty<StackFit>("fit", fit));
            properties.add(new EnumProperty<Overflow>("overflow", overflow));
        }
    }

    class RenderIndexedStack : RenderStack {
        public RenderIndexedStack(
            List<RenderBox> children = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null,
            int index = 0
        ) :  base(
            children: children,
            alignment: alignment ?? AlignmentDirectional.topStart,
            textDirection: textDirection?? TextDirection.ltr) {
            _index = index;
        }

        public int index {
            get { return _index; }
            set {
                if (_index != value) {
                    _index = value;
                    markNeedsLayout();
                }
            }
        }

        int _index;

        RenderBox _childAtIndex() {
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

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            if (firstChild == null) {
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
            if (firstChild == null) {
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