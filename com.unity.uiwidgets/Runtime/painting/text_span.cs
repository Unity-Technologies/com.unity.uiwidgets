using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;
using UnityEngine.Assertions;

namespace Unity.UIWidgets.painting {
    public class TextSpan : InlineSpan, IEquatable<TextSpan> {
        public delegate bool Visitor(TextSpan span);

        public readonly string text;
        public readonly List<InlineSpan> children;
        public readonly GestureRecognizer recognizer;
        public readonly string semanticsLabel;

        public TextSpan(
            string text = null,
            TextStyle style = null,
            List<InlineSpan> children = null,
            GestureRecognizer recognizer = null,
            string semanticsLabel = null
            ) : base(style: style) {
            this.text = text;
            this.children = children;
            this.recognizer = recognizer;
            this.semanticsLabel = semanticsLabel;
        }

        public override void build(
            ParagraphBuilder builder,
            float textScaleFactor = 1.0f,
            List<PlaceholderDimensions> dimensions = null
            ) {
            D.assert(debugAssertIsValid());
            var hasStyle = style != null;

            if (hasStyle) {
                builder.pushStyle(style.getTextStyle(textScaleFactor: textScaleFactor));
            }
            if (text != null)
                builder.addText(text);
            if (children != null) {
                foreach (InlineSpan child in children) {
                    D.assert(child != null);
                    child.build(
                        builder,
                        textScaleFactor: textScaleFactor,
                        dimensions: dimensions
                    );
                }
            }

            if (hasStyle) {
                builder.pop();
            }
        }

        public override bool visitChildren(InlineSpanVisitor visitor) {
            if (text != null) {
                if (!visitor(this))
                    return false;
            }

            if (children != null) {
                foreach (InlineSpan child in children) {
                    if (!child.visitChildren(visitor))
                        return false;
                }
            }

            return true;
        }

        protected override InlineSpan getSpanForPositionVisitor(TextPosition position, Accumulator offset) {
            if (text == null) {
                return null;
            }

            TextAffinity affinity = position.affinity;
            int targetOffset = position.offset;
            int endOffset = offset.value + text.Length;
            if (offset.value == targetOffset && affinity == TextAffinity.downstream ||
                offset.value < targetOffset && targetOffset < endOffset ||
                endOffset == targetOffset && affinity == TextAffinity.upstream) {
                return this;
            }

            offset.increment(text.Length);
            return null;
        }

        public override void computeToPlainText(
            StringBuilder buffer, 
            bool includeSemanticsLabels = true,
            bool includePlaceholders = true
            ) {
            D.assert(debugAssertIsValid());
            if (semanticsLabel != null && includeSemanticsLabels) {
                buffer.Append(semanticsLabel);
            }
            else if (text != null) {
                buffer.Append(text);
            }

            if (children != null) {
                foreach (InlineSpan child in children) {
                    child.computeToPlainText(buffer,
                        includeSemanticsLabels: includeSemanticsLabels,
                        includePlaceholders: includePlaceholders
                    );
                }
            }
        }

        public override void computeSemanticsInformation(List<InlineSpanSemanticsInformation> collector) {
            D.assert(debugAssertIsValid());
            if (text != null || semanticsLabel != null) {
                collector.Add(new InlineSpanSemanticsInformation(
                    text,
                    semanticsLabel: semanticsLabel,
                    recognizer: recognizer
                ));
            }

            if (children != null) {
                foreach (InlineSpan child in children) {
                    child.computeSemanticsInformation(collector);
                }
            }
        }

        protected override int? codeUnitAtVisitor(int index, Accumulator offset) {
            if (text == null) {
                return null;
            }

            if (index - offset.value < text.Length) {
                return text[index - offset.value];
            }

            offset.increment(text.Length);
            return null;
        }

        public override bool debugAssertIsValid() {
            D.assert(() => {
                if (children != null) {
                    foreach (InlineSpan child in children) {
                        if (child == null) {
                            throw new UIWidgetsError(
                                "A TextSpan object with a non-null child list should not have any nulls in its child list.\n" +
                                "The full text in question was:\n" +
                                toStringDeep(prefixLineOne: "  "));
                        }

                        D.assert(child.debugAssertIsValid());
                    }
                }

                return true;
            });
            return base.debugAssertIsValid();
        }

        public override RenderComparison compareTo(InlineSpan other) {
            if (Equals(this, other))
                return RenderComparison.identical;
            if (other.GetType() != GetType())
                return RenderComparison.layout;
            TextSpan textSpan = other as TextSpan;
            if (textSpan.text != text ||
                children?.Count != textSpan.children?.Count ||
                (style == null) != (textSpan.style == null))
                return RenderComparison.layout;
            RenderComparison result = recognizer == textSpan.recognizer
                ? RenderComparison.identical
                : RenderComparison.metadata;
            if (style != null) {
                RenderComparison candidate = style.compareTo(textSpan.style);
                if (candidate > result)
                    result = candidate;
                if (result == RenderComparison.layout)
                    return result;
            }

            if (children != null) {
                for (int index = 0; index < children.Count; index += 1) {
                    RenderComparison candidate = children[index].compareTo(textSpan.children[index]);
                    if (candidate > result)
                        result = candidate;
                    if (result == RenderComparison.layout)
                        return result;
                }
            }

            return result;
        }

        public override int GetHashCode() {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (text != null ? text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (children != null ? children.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (recognizer != null ? recognizer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (semanticsLabel != null ? semanticsLabel.GetHashCode() : 0);
                return hashCode;
            }
        }
        public static bool operator ==(TextSpan left, TextSpan right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextSpan left, TextSpan right) {
            return !Equals(left, right);
        }

        static bool childEquals(List<InlineSpan> left, List<InlineSpan> right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }

            if (left == null || right == null) {
                return false;
            }

            return left.SequenceEqual(right);
        }

        public override string toStringShort() {
            return foundation_.objectRuntimeType(this, "TextSpan");
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(
                new StringProperty(
                    "text",
                    text,
                    showName: false,
                    defaultValue: null
                )
            );
            if (style == null && text == null && children == null)
                properties.add(DiagnosticsNode.message("(empty)"));

            properties.add(new DiagnosticsProperty<GestureRecognizer>(
                "recognizer", recognizer,
                description: recognizer?.GetType()?.ToString(),
                defaultValue: null
            ));

            if (semanticsLabel != null) {
                properties.add(new StringProperty("semanticsLabel", semanticsLabel));
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            if (children == null) {
                return new List<DiagnosticsNode>();
            }
            return LinqUtils<DiagnosticsNode, InlineSpan>.SelectList(children,((child) => {
                    if (child != null) {
                        return child.toDiagnosticsNode();
                    }
                    else {
                        return DiagnosticsNode.message("<null child>");
                    }
                }));
        }


        public bool Equals(TextSpan other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return base.Equals(other) && text == other.text && Equals(children, other.children) && Equals(recognizer, other.recognizer) && semanticsLabel == other.semanticsLabel;
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

            return Equals((TextSpan) obj);
        }
    }
}