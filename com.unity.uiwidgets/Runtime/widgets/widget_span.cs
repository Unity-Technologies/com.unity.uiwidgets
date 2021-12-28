using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    class WidgetSpan : PlaceholderSpan, IEquatable<WidgetSpan> {
        public WidgetSpan(
            Widget child ,
            TextBaseline? baseline = null,
            TextStyle style = null,
            PlaceholderAlignment alignment = PlaceholderAlignment.bottom
        ) : base(
            alignment: alignment,
            baseline: baseline,
            style: style
        ) {
            D.assert(child != null);
            D.assert(baseline != null ||!(
                (alignment == PlaceholderAlignment.aboveBaseline) ||
                (alignment == PlaceholderAlignment.belowBaseline) ||
                (alignment == PlaceholderAlignment.baseline)
            ));
            this.child = child;
        }

        public readonly Widget child;

        public override void build(
            ParagraphBuilder builder,
            float textScaleFactor = 1.0f,
            List<PlaceholderDimensions> dimensions = null) {
            D.assert(DebugAssertIsValid());
            D.assert(dimensions != null);
            bool hasStyle = style != null;
            if (hasStyle) {
                builder.pushStyle(style.getTextStyle(textScaleFactor: textScaleFactor));
            }

            D.assert(builder.placeholderCount < dimensions.Count);
            PlaceholderDimensions currentDimensions = dimensions[builder.placeholderCount];
            builder.addPlaceholder(
                currentDimensions.size.width,
                currentDimensions.size.height,
                alignment,
                scale: textScaleFactor,
                baseline: currentDimensions.baseline,
                baselineOffset: currentDimensions.baselineOffset
            );
            if (hasStyle) {
                builder.pop();
            }
        }


        public override bool visitChildren(InlineSpanVisitor visitor) {
            return visitor(this);
        }


        protected override InlineSpan getSpanForPositionVisitor(TextPosition position, Accumulator offset) {
            if (position.offset == offset.value) {
                return this;
            }

            offset.increment(1);
            return null;
        }

        protected override int? codeUnitAtVisitor(int index, Accumulator offset) {
            return null;
        }

        public override RenderComparison compareTo(InlineSpan other) {
            if (this == other)
                return RenderComparison.identical;
            if (other.GetType() != GetType())
                return RenderComparison.layout;
            if ((style == null) != (other.style == null))
                return RenderComparison.layout;
            WidgetSpan typedOther = other as WidgetSpan;
            if (!child.Equals(typedOther.child) || alignment != typedOther.alignment) {
                return RenderComparison.layout;
            }

            RenderComparison result = RenderComparison.identical;
            if (style != null) {
                RenderComparison candidate = style.compareTo(other.style);
                if ((int) candidate > (int) result)
                    result = candidate;
                if (result == RenderComparison.layout)
                    return result;
            }

            return result;
        }

        public override int GetHashCode() {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (child != null ? child.GetHashCode() : 0);
            }
        }

        public bool DebugAssertIsValid() {
            return true;
        }

        public bool Equals(WidgetSpan other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return base.Equals(other) && Equals(child, other.child);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((WidgetSpan) obj);
        }
        public static bool operator ==(WidgetSpan left, WidgetSpan right) {
            return Equals(left, right);
        }

        public static bool operator !=(WidgetSpan left, WidgetSpan right) {
            return !Equals(left, right);
        }
        
        public override InlineSpan getSpanForPosition(TextPosition position) {
            D.assert(debugAssertIsValid());
            return null;
        }
        
        public override bool debugAssertIsValid() {
            return true;
        }
    }
} 