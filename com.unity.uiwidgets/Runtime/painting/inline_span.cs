using System;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class Accumulator {
        public Accumulator(int _value = 0) {
            this._value = _value;
        }

        public int value {
            get { return _value; }
        }

        int _value;

        public void increment(int addend) {
            D.assert(addend >= 0);
            _value += addend;
        }
    }

    public delegate bool InlineSpanVisitor(InlineSpan span);

    public delegate bool TextSpanVisitor(TextSpan span);

    public class InlineSpanSemanticsInformation : IEquatable<InlineSpanSemanticsInformation> {
        public InlineSpanSemanticsInformation(
            string text,
            bool? isPlaceholder = false,
            string semanticsLabel = null,
            GestureRecognizer recognizer = null
        ) {
            D.assert(text != null);
            D.assert(isPlaceholder != null);
            D.assert(isPlaceholder == false || (text == "\uFFFC" && semanticsLabel == null && recognizer == null));
            requiresOwnNode = isPlaceholder.Value || recognizer != null;
        }

        public static readonly InlineSpanSemanticsInformation placeholder =
            new InlineSpanSemanticsInformation("\uFFFC", isPlaceholder: true);

        public readonly string text;

        public readonly string semanticsLabel;

        public readonly GestureRecognizer recognizer;

        public readonly bool isPlaceholder;

        public readonly bool requiresOwnNode;


        public override string ToString() =>
            $"{foundation_.objectRuntimeType(this, "InlineSpanSemanticsInformation")}" +
            "text: " + text + " , semanticsLabel: "+ semanticsLabel + " , recognizer: " + recognizer;

        public bool Equals(InlineSpanSemanticsInformation other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return text == other.text && semanticsLabel == other.semanticsLabel &&
                   Equals(recognizer, other.recognizer) && isPlaceholder == other.isPlaceholder;
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

            return Equals((InlineSpanSemanticsInformation) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (text != null ? text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (semanticsLabel != null ? semanticsLabel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (recognizer != null ? recognizer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isPlaceholder.GetHashCode();
                return hashCode;
            }
        }
    }

    public abstract class InlineSpan : DiagnosticableTree, IEquatable<InlineSpan> {
        public InlineSpan(
            TextStyle style = null
        ) {
            this.style = style;
        }

        public readonly TextStyle style;


        public abstract void build(ParagraphBuilder builder,
            float textScaleFactor = 1, List<PlaceholderDimensions> dimensions = null
        );

        public abstract bool visitChildren(InlineSpanVisitor visitor);

        public virtual InlineSpan getSpanForPosition(TextPosition position) {
            D.assert(debugAssertIsValid());
            Accumulator offset = new Accumulator();
            InlineSpan result = null;
            visitChildren((InlineSpan span) => {
                result = span.getSpanForPositionVisitor(position, offset);
                return result == null;
            });
            return result;
        }

        protected abstract InlineSpan getSpanForPositionVisitor(TextPosition position, Accumulator offset);

        public virtual string toPlainText(
            bool includeSemanticsLabels = true,
            bool includePlaceholders = true) {
            StringBuilder buffer = new StringBuilder();
            computeToPlainText(buffer, includeSemanticsLabels: includeSemanticsLabels,
                includePlaceholders: includePlaceholders);
            return buffer.ToString();
        }

        List<InlineSpanSemanticsInformation> getSemanticsInformation() {
            List<InlineSpanSemanticsInformation> collector = new List<InlineSpanSemanticsInformation>();

            computeSemanticsInformation(collector);
            return collector;
        }

        public abstract void computeSemanticsInformation(List<InlineSpanSemanticsInformation> collector);

        public abstract void computeToPlainText(
            StringBuilder buffer,
            bool includeSemanticsLabels = true,
            bool includePlaceholders = true);

        public int? codeUnitAt(int index) {
            if (index < 0)
                return null;
            Accumulator offset = new Accumulator();
            int? result = null;
            visitChildren((InlineSpan span) => {
                result = span.codeUnitAtVisitor(index, offset);
                return result == null;
            });
            return result;
        }

        protected abstract int? codeUnitAtVisitor(int index, Accumulator offset);

        public virtual bool debugAssertIsValid() => true;
        public abstract RenderComparison compareTo(InlineSpan other);

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.whitespace;
            if (style != null) {
                style.debugFillProperties(properties);
            }
        }

        public bool Equals(InlineSpan other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(style, other.style);
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

            return Equals((InlineSpan) obj);
        }

        public override int GetHashCode() {
            return (style != null ? style.GetHashCode() : 0);
        }

        public static bool operator ==(InlineSpan left, InlineSpan right) {
            return Equals(left, right);
        }

        public static bool operator !=(InlineSpan left, InlineSpan right) {
            return !Equals(left, right);
        }
    }
}