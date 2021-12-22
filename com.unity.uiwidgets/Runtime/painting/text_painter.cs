using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public class PlaceholderDimensions {
        public PlaceholderDimensions(
            Size size,
            PlaceholderAlignment alignment,
            TextBaseline? baseline,
            float baselineOffset
        ) {
            D.assert(size != null);
            this.size = size;
            this.alignment = alignment;
            this.baseline = baseline;
            this.baselineOffset = baselineOffset;
        }

        public readonly Size size;

        public readonly PlaceholderAlignment alignment;

        public readonly float baselineOffset;

        public readonly TextBaseline? baseline;

        public override string ToString() {
            return $"PlaceholderDimensions({size}, {baseline})";
        }
    }

    public enum TextWidthBasis {
        parent,

        longestLine,
    }

    class _CaretMetrics {
        public _CaretMetrics(Offset offset, float? fullHeight) {
            this.offset = offset;
            this.fullHeight = fullHeight;
        }

        public Offset offset;
        public float? fullHeight;
    }

    public class TextPainter {
        InlineSpan _text;
        TextAlign _textAlign;
        TextDirection? _textDirection;
        float _textScaleFactor;
        Paragraph _layoutTemplate;
        Paragraph _paragraph;
        bool _needsLayout = true;
        int? _maxLines;
        string _ellipsis;
        float _lastMinWidth;
        float _lastMaxWidth;
        TextWidthBasis _textWidthBasis;
        TextHeightBehavior _textHeightBehavior;

        public TextPainter(InlineSpan text = null,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            float textScaleFactor = 1.0f,
            int? maxLines = null,
            string ellipsis = null,
            Locale locale = null,
            StrutStyle strutStyle = null,
            TextWidthBasis textWidthBasis = TextWidthBasis.parent,
            TextHeightBehavior textHeightBehavior = null) {
            D.assert(text == null || text.debugAssertIsValid());
            D.assert(maxLines == null || maxLines > 0);
            _text = text;
            _textAlign = textAlign;
            _textDirection = textDirection;
            _textScaleFactor = textScaleFactor;
            _maxLines = maxLines;
            _ellipsis = ellipsis;
            _locale = locale;
            _strutStyle = strutStyle;
            _textWidthBasis = textWidthBasis;
            _textHeightBehavior = textHeightBehavior;
        }

        public void markNeedsLayout() {
            _paragraph = null;
            _needsLayout = true;
            _previousCaretPosition = null;
            _previousCaretPrototype = null;
        }

        public float textScaleFactor {
            get { return _textScaleFactor; }
            set {
                if (_textScaleFactor == value) {
                    return;
                }

                _textScaleFactor = value;
                markNeedsLayout();
                _layoutTemplate = null;
            }
        }

        public string ellipsis {
            get { return _ellipsis; }
            set {
                if (_ellipsis == value) {
                    return;
                }

                _ellipsis = value;
                markNeedsLayout();
            }
        }

        public InlineSpan text {
            get { return _text; }
            set {
                D.assert(value == null || value.debugAssertIsValid());
                if (_text == value) {
                    return;
                }

                if (!Equals(_text == null ? null : _text.style, value == null ? null : value.style)) {
                    _layoutTemplate = null;
                }

                _text = value;
                markNeedsLayout();
            }
        }

        public Size size {
            get {
                Debug.Assert(!_needsLayout);
                return new Size(width, height);
            }
        }

        public TextDirection? textDirection {
            get { return _textDirection; }
            set {
                if (textDirection == value) {
                    return;
                }

                _textDirection = value;
                markNeedsLayout();
                _layoutTemplate = null;
            }
        }

        public TextAlign textAlign {
            get { return _textAlign; }
            set {
                if (_textAlign == value) {
                    return;
                }

                _textAlign = value;
                markNeedsLayout();
            }
        }

        public bool didExceedMaxLines {
            get {
                Debug.Assert(!_needsLayout);
                return _paragraph.didExceedMaxLines();
            }
        }

        public int? maxLines {
            get { return _maxLines; }
            set {
                if (_maxLines == value) {
                    return;
                }

                _maxLines = value;
                markNeedsLayout();
            }
        }

        public StrutStyle strutStyle {
            get { return _strutStyle; }
            set {
                if (_strutStyle == value) {
                    return;
                }

                _strutStyle = value;
                markNeedsLayout();
            }
        }

        StrutStyle _strutStyle;

        public Locale locale {
            get { return _locale; }
            set {
                if (_locale == value)
                    return;
                _locale = value;
                markNeedsLayout();
            }
        }

        Locale _locale;

        public TextWidthBasis textWidthBasis {
            get { return _textWidthBasis; }
            set {
                if (_textWidthBasis == value)
                    return;
                _textWidthBasis = value;
                markNeedsLayout();
            }
        }

        public TextHeightBehavior textHeightBehavior {
            get { return _textHeightBehavior; }
            set {
                if (_textHeightBehavior == value)
                    return;
                _textHeightBehavior = value;
                markNeedsLayout();
            }
        }

        public List<TextBox> inlinePlaceholderBoxes {
            get {
                return _inlinePlaceholderBoxes;
            }
        }

        List<TextBox> _inlinePlaceholderBoxes;

        public List<float> inlinePlaceholderScales {
            get {
                return _inlinePlaceholderScales;
            }
        }

        List<float> _inlinePlaceholderScales;

        public float minIntrinsicWidth {
            get {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.minIntrinsicWidth());
            }
        }

        public void setPlaceholderDimensions(List<PlaceholderDimensions> value) {
            if (value == null || value.isEmpty() || value.equalsList(_placeholderDimensions)) {
                return;
            }

            D.assert(() => {
                int placeholderCount = 0;
                text.visitChildren((InlineSpan span) => {
                    if (span is PlaceholderSpan) {
                        placeholderCount += 1;
                    }

                    return true;
                });
                return placeholderCount == value.Count;
            });
            _placeholderDimensions = value;
            markNeedsLayout();
        }

        List<PlaceholderDimensions> _placeholderDimensions;

        public float maxIntrinsicWidth {
            get {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.maxIntrinsicWidth());
            }
        }

        public float height {
            get {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.height());
            }
        }

        public float width {
            get {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(textWidthBasis == TextWidthBasis.longestLine
                    ? _paragraph.longestLine()
                    : _paragraph.width());
            }
        }

        public float computeDistanceToActualBaseline(TextBaseline baseline) {
            Debug.Assert(!_needsLayout);
            switch (baseline) {
                case TextBaseline.alphabetic:
                    return _paragraph.alphabeticBaseline();
                case TextBaseline.ideographic:
                    return _paragraph.ideographicBaseline();
            }

            return 0.0f;
        }

        public void layout(float minWidth = 0.0f, float maxWidth = float.PositiveInfinity) {
            Debug.Assert(text != null,
                "TextPainter.text must be set to a non-null value before using the TextPainter.");
            Debug.Assert(textDirection != null,
                "TextPainter.textDirection must be set to a non-null value before using the TextPainter.");
            if (!_needsLayout && minWidth == _lastMinWidth && maxWidth == _lastMaxWidth) {
                return;
            }

            _needsLayout = false;
            if (_paragraph == null) {
                var builder = new ParagraphBuilder(_createParagraphStyle());
                _text.build(builder, textScaleFactor: textScaleFactor,
                    dimensions: _placeholderDimensions);
                _inlinePlaceholderScales = builder.placeholderScales;
                _paragraph = builder.build();
            }

            _lastMinWidth = minWidth;
            _lastMaxWidth = maxWidth;
            _paragraph.layout(new ParagraphConstraints(maxWidth));

            if (minWidth != maxWidth) {
                var newWidth = MathUtils.clamp(maxIntrinsicWidth, minWidth, maxWidth);
                if (newWidth != width) {
                    _paragraph.layout(new ParagraphConstraints(newWidth));
                }
            }
            _inlinePlaceholderBoxes = _paragraph.getBoxesForPlaceholders();
        }

        public void paint(Canvas canvas, Offset offset) {
            Debug.Assert(!_needsLayout);
            canvas.drawParagraph(_paragraph, offset);
        }

        public Offset getOffsetForCaret(TextPosition position, Rect caretPrototype) {
            _computeCaretMetrics(position, caretPrototype);
            return _caretMetrics.offset;
        }

        public float? getFullHeightForCaret(TextPosition position, Rect caretPrototype) {
            _computeCaretMetrics(position, caretPrototype);
            return _caretMetrics.fullHeight;
        }

        _CaretMetrics _caretMetrics;

        TextPosition _previousCaretPosition;
        Rect _previousCaretPrototype;

        void _computeCaretMetrics(TextPosition position, Rect caretPrototype) {
            D.assert(!_needsLayout);
            if (position == _previousCaretPosition && caretPrototype == _previousCaretPrototype) {
                return;
            }

            var offset = position.offset;
            Rect rect;
            switch (position.affinity) {
                case TextAffinity.upstream:
                    rect = _getRectFromUpstream(offset, caretPrototype) ??
                           _getRectFromDownStream(offset, caretPrototype);
                    break;
                case TextAffinity.downstream:
                    rect = _getRectFromDownStream(offset, caretPrototype) ??
                           _getRectFromUpstream(offset, caretPrototype);
                    break;
                default:
                    throw new UIWidgetsError("Unknown Position Affinity");
            }

            _caretMetrics = new _CaretMetrics(
                offset: rect != null ? new Offset(rect.left, rect.top) : _emptyOffset,
                fullHeight: rect != null ? (float?) (rect.bottom - rect.top) : null);

            // Cache the caret position. This was forgot in flutter until https://github.com/flutter/flutter/pull/38821
            _previousCaretPosition = position;
            _previousCaretPrototype = caretPrototype;
        }

        public List<TextBox> getBoxesForSelection(
            TextSelection selection,
            BoxHeightStyle boxHeightStyle = BoxHeightStyle.tight,
            BoxWidthStyle boxWidthStyle = BoxWidthStyle.tight) {
            D.assert(!_needsLayout);
            var results = _paragraph.getBoxesForRange(
                selection.start,
                selection.end,
                boxHeightStyle: boxHeightStyle,
                boxWidthStyle: boxWidthStyle);
            return results;
        }

        public TextPosition getPositionForOffset(Offset offset) {
            D.assert(!_needsLayout);
            return _paragraph.getPositionForOffset(offset);
        }

        public TextRange getWordBoundary(TextPosition position) {
            D.assert(!_needsLayout);
            return _paragraph.getWordBoundary(position);
        }

        public TextRange getLineBoundary(TextPosition position) {
            D.assert(!_needsLayout);
            return _paragraph.getLineBoundary(position);
        }
        
        List<ui.LineMetrics> computeLineMetrics() {
            D.assert(!_needsLayout);
            return _paragraph.computeLineMetrics();
        }

        ParagraphStyle _createParagraphStyle(TextDirection defaultTextDirection = TextDirection.ltr) {
            D.assert(textDirection != null,
                () => "TextPainter.textDirection must be set to a non-null value before using the TextPainter.");
            return _text.style?.getParagraphStyle(
                textAlign: textAlign,
                textDirection: textDirection ?? defaultTextDirection,
                textScaleFactor: textScaleFactor,
                maxLines: _maxLines,
                textHeightBehavior: _textHeightBehavior,
                ellipsis: _ellipsis,
                locale: _locale,
                strutStyle: _strutStyle
            ) ?? new ParagraphStyle(
                textAlign: textAlign,
                textDirection: textDirection ?? defaultTextDirection,
                maxLines: maxLines,
                textHeightBehavior: _textHeightBehavior,
                ellipsis: ellipsis,
                locale: locale
            );
        }

        public float preferredLineHeight {
            get {
                if (_layoutTemplate == null) {
                    var builder = new ParagraphBuilder(_createParagraphStyle(TextDirection.ltr)
                    ); // direction doesn't matter, text is just a space
                    if (text != null && text.style != null) {
                        builder.pushStyle(text.style.getTextStyle(textScaleFactor: textScaleFactor));
                    }

                    builder.addText(" ");
                    _layoutTemplate = builder.build();
                    _layoutTemplate.layout(new ParagraphConstraints(float.PositiveInfinity));
                }

                return _layoutTemplate.height();
            }
        }

        float _applyFloatingPointHack(float layoutValue) {
            return Mathf.Ceil(layoutValue);
        }

        const int _zwjUtf16 = 0x200d;

        Rect _getRectFromUpstream(int offset, Rect caretPrototype) {
            string flattenedText = _text.toPlainText(includePlaceholders: false);
            var prevCodeUnit = _text.codeUnitAt(Mathf.Max(0, offset - 1));
            if (prevCodeUnit == null) {
                return null;
            }

            bool needsSearch = _isUtf16Surrogate(prevCodeUnit.Value) || _text.codeUnitAt(offset) == _zwjUtf16 ||
                               _isUnicodeDirectionality(prevCodeUnit);
            int graphemeClusterLength = needsSearch ? 2 : 1;
            List<TextBox> boxes = null;
            while ((boxes == null || boxes.isEmpty()) && flattenedText != null) {
                int prevRuneOffset = offset - graphemeClusterLength;
                boxes = _paragraph.getBoxesForRange(prevRuneOffset, offset, boxHeightStyle: BoxHeightStyle.strut);
                if (boxes.isEmpty()) {
                    if (!needsSearch) {
                        break;
                    }

                    if (prevRuneOffset < -flattenedText.Length) {
                        break;
                    }

                    graphemeClusterLength *= 2;
                    continue;
                }

                TextBox box = boxes[0];
                const int NEWLINE_CODE_UNIT = 10;
                if (prevCodeUnit == NEWLINE_CODE_UNIT) {
                    return Rect.fromLTRB(_emptyOffset.dx, box.bottom,
                        _emptyOffset.dx, box.bottom + box.bottom - box.top);
                }

                float caretEnd = box.end;
                float dx = box.direction == TextDirection.rtl ? caretEnd - caretPrototype.width : caretEnd;
                return Rect.fromLTRB(Mathf.Min(dx, _paragraph.width()), box.top,
                    Mathf.Min(dx, _paragraph.width()), box.bottom);
            }

            return null;
        }

        Rect _getRectFromDownStream(int offset, Rect caretPrototype) {
            string flattenedText = _text.toPlainText(includePlaceholders: false);
            var nextCodeUnit =
                _text.codeUnitAt(Mathf.Min(offset, flattenedText == null ? 0 : flattenedText.Length - 1));
            if (nextCodeUnit == null) {
                return null;
            }

            bool needsSearch = _isUtf16Surrogate(nextCodeUnit.Value) || nextCodeUnit == _zwjUtf16;
            int graphemeClusterLength = needsSearch ? 2 : 1;
            List<TextBox> boxes = null;
            while ((boxes == null || boxes.isEmpty()) && flattenedText != null) {
                int nextRuneOffset = offset + graphemeClusterLength;
                boxes = _paragraph.getBoxesForRange(offset, nextRuneOffset, boxHeightStyle: BoxHeightStyle.strut);
                if (boxes.isEmpty()) {
                    if (!needsSearch) {
                        break;
                    }

                    if (nextRuneOffset >= flattenedText.Length << 1) {
                        break;
                    }

                    graphemeClusterLength *= 2;
                    continue;
                }

                TextBox box = boxes[boxes.Count - 1];
                float caretStart = box.start;
                float dx = box.direction == TextDirection.rtl ? caretStart - caretPrototype.width : caretStart;
                return Rect.fromLTRB(Mathf.Min(dx, _paragraph.width()), box.top,
                    Mathf.Min(dx, _paragraph.width()), box.bottom);
            }

            return null;
        }

        Offset _emptyOffset {
            get {
                D.assert(!_needsLayout);
                switch (textAlign) {
                    case TextAlign.left:
                        return Offset.zero;
                    case TextAlign.right:
                        return new Offset(width, 0.0f);
                    case TextAlign.center:
                        return new Offset(width / 2.0f, 0.0f);
                    case TextAlign.justify:
                        if (textDirection == TextDirection.rtl) {
                            return new Offset(width, 0.0f);
                        }

                        return Offset.zero;
                }

                return null;
            }
        }

        static bool _isUtf16Surrogate(int value) {
            return (value & 0xF800) == 0xD800;
        }

        bool _isUnicodeDirectionality(int? value) {
            return value == 0x200F || value == 0x200E;
        }
        
         public int? getOffsetAfter(int offset) {
            int? nextCodeUnit = _text.codeUnitAt(offset);
            if (nextCodeUnit == null)
                return null;
            // TODO(goderbauer): doesn't handle extended grapheme clusters with more than one Unicode scalar value (https://github.com/flutter/flutter/issues/13404).
            return _isUtf16Surrogate(nextCodeUnit.Value) ? offset + 2 : offset + 1;
        }
         
         public int? getOffsetBefore(int offset) {
             int? prevCodeUnit = _text.codeUnitAt(offset - 1);
             if (prevCodeUnit == null)
                 return null;
             // TODO(goderbauer): doesn't handle extended grapheme clusters with more than one Unicode scalar value (https://github.com/flutter/flutter/issues/13404).
             return _isUtf16Surrogate(prevCodeUnit.Value) ? offset - 2 : offset - 1;
         }
        
    }
}