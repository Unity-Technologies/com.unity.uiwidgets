using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;
using UnityEngine.Assertions;

namespace Unity.UIWidgets.painting {
    public class TextSpan : InlineSpan, IEquatable<TextSpan> {
        public delegate bool Visitor(TextSpan span);

        public readonly TextStyle style;
        public readonly string text;
        public readonly List<TextSpan> children;
        public readonly GestureRecognizer recognizer;
        public readonly HoverRecognizer hoverRecognizer;
        public readonly string semanticsLabel;

        public TextSpan(
            string text = "",
            TextStyle style = null,
            List<TextSpan> children = null,
            GestureRecognizer recognizer = null,
            HoverRecognizer hoverRecognizer = null,
            string semanticsLabel = null) : base(style) {
            this.text = text;
            this.style = style;
            this.children = children;
            this.recognizer = recognizer;
            this.hoverRecognizer = hoverRecognizer;
            this.semanticsLabel = semanticsLabel;
        }

        public override void build(ParagraphBuilder builder, float textScaleFactor = 1.0f,
            List<PlaceholderDimensions> dimensions = null) {
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

        public override void computeToPlainText(StringBuilder buffer, bool includeSemanticsLabels = true,
            bool includePlaceholders = true) {
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

        public bool hasHoverRecognizer {
            get {
                bool need = false;
                visitTextSpan((text) => {
                    if (text.hoverRecognizer != null) {
                        need = true;
                        return false;
                    }

                    return true;
                });
                return need;
            }
        }

        bool visitTextSpan(Visitor visitor) {
            if (!string.IsNullOrEmpty(text)) {
                if (!visitor.Invoke(this)) {
                    return false;
                }
            }

            if (children != null) {
                foreach (var child in children) {
                    if (!child.visitTextSpan(visitor)) {
                        return false;
                    }
                }
            }

            return true;
        }

        public TextSpan getSpanForPosition(TextPosition position) {
            D.assert(debugAssertIsValid());
            var offset = 0;
            var targetOffset = position.offset;
            var affinity = position.affinity;
            TextSpan result = null;
            visitTextSpan((span) => {
                var endOffset = offset + span.text.Length;
                if ((targetOffset == offset && affinity == TextAffinity.downstream) ||
                    (targetOffset > offset && targetOffset < endOffset) ||
                    (targetOffset == endOffset && affinity == TextAffinity.upstream)) {
                    result = span;
                    return false;
                }

                offset = endOffset;
                return true;
            });
            return result;
        }

        public int? codeUnitAt(int index) {
            if (index < 0) {
                return null;
            }

            var offset = 0;
            int? result = null;
            visitTextSpan(span => {
                if (index - offset < span.text.Length) {
                    result = span.text[index - offset];
                    return false;
                }

                offset += span.text.Length;
                return true;
            });
            return result;
        }

        bool debugAssertIsValid() {
            D.assert(() => {
                if (!visitTextSpan(span => {
                    if (span.children != null) {
                        foreach (TextSpan child in span.children) {
                            if (child == null) {
                                return false;
                            }
                        }
                    }

                    return true;
                })) {
                    throw new UIWidgetsError(
                        "A TextSpan object with a non-null child list should not have any nulls in its child list.\n" +
                        "The full text in question was:\n" +
                        toStringDeep(prefixLineOne: "  "));
                }

                return true;
            });
            return true;
        }

        public override RenderComparison compareTo(InlineSpan other) {
            if (Equals(this, other))
                return RenderComparison.identical;
            if (other.GetType()!= GetType())
                return RenderComparison.layout;
            TextSpan textSpan = other as TextSpan;
            if (textSpan.text != text ||
                children?.Count != textSpan.children?.Count ||
                (style == null) != (textSpan.style == null))
                return RenderComparison.layout;
            RenderComparison result = recognizer == textSpan.recognizer ?
                RenderComparison.identical :
                RenderComparison.metadata;
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

            return Equals((TextSpan) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (style != null ? style.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (text != null ? text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (recognizer != null ? recognizer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (childHash());
                return hashCode;
            }
        }

        public bool Equals(TextSpan other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(style, other.style) && string.Equals(text, other.text) &&
                   childEquals(children, other.children) && recognizer == other.recognizer;
        }

        public static bool operator ==(TextSpan left, TextSpan right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextSpan left, TextSpan right) {
            return !Equals(left, right);
        }

        int childHash() {
            unchecked {
                var hashCode = 0;
                if (children != null) {
                    foreach (var child in children) {
                        hashCode = (hashCode * 397) ^ (child != null ? child.GetHashCode() : 0);
                    }
                }

                return hashCode;
            }
        }

        static bool childEquals(List<TextSpan> left, List<TextSpan> right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }

            if (left == null || right == null) {
                return false;
            }

            return left.SequenceEqual(right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.whitespace;
            // Properties on style are added as if they were properties directly on
            // this TextSpan.
            if (style != null) {
                style.debugFillProperties(properties);
            }

            properties.add(new DiagnosticsProperty<GestureRecognizer>(
                "recognizer", recognizer,
                description: recognizer == null ? "" : recognizer.GetType().FullName,
                defaultValue: foundation_.kNullDefaultValue
            ));

            properties.add(new StringProperty("text", text, showName: false,
                defaultValue: foundation_.kNullDefaultValue));
            if (style == null && text == null && children == null) {
                properties.add(DiagnosticsNode.message("(empty)"));
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            if (children == null) {
                return new List<DiagnosticsNode>();
            }

            return children.Select((child) => {
                if (child != null) {
                    return child.toDiagnosticsNode();
                }
                else {
                    return DiagnosticsNode.message("<null child>");
                }
            }).ToList();
        }
    }
}