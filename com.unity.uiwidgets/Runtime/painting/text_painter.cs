﻿using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    class _CaretMetrics {
        public _CaretMetrics(Offset offset, float? fullHeight) {
            this.offset = offset;
            this.fullHeight = fullHeight;
        }

        public Offset offset;
        public float? fullHeight;
    }

    public class TextPainter {
        TextSpan _text;
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

        public TextPainter(TextSpan text = null,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            float textScaleFactor = 1.0f,
            int? maxLines = null,
            string ellipsis = "",
            StrutStyle strutStyle = null) {
            _text = text;
            _textAlign = textAlign;
            _textDirection = textDirection;
            _textScaleFactor = textScaleFactor;
            _maxLines = maxLines;
            _ellipsis = ellipsis;
            _strutStyle = strutStyle;
        }

        public float textScaleFactor {
            get { return _textScaleFactor; }
            set {
                if (_textScaleFactor == value) {
                    return;
                }

                _textScaleFactor = value;
                _layoutTemplate = null;
                _needsLayout = true;
                Paragraph.release(ref _paragraph);
            }
        }

        public string ellipsis {
            get { return _ellipsis; }
            set {
                if (_ellipsis == value) {
                    return;
                }

                _ellipsis = value;
                Paragraph.release(ref _paragraph);
                _needsLayout = true;
            }
        }

        public TextSpan text {
            get { return _text; }
            set {
                if ((_text == null && value == null) || (_text != null && text.Equals(value))) {
                    return;
                }

                if (!Equals(_text == null ? null : _text.style, value == null ? null : value.style)) {
                    _layoutTemplate = null;
                }

                _text = value;
                Paragraph.release(ref _paragraph);
                _needsLayout = true;
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
                Paragraph.release(ref _paragraph);
                _layoutTemplate = null;
                _needsLayout = true;
            }
        }

        public TextAlign textAlign {
            get { return _textAlign; }
            set {
                if (_textAlign == value) {
                    return;
                }

                _textAlign = value;
                Paragraph.release(ref _paragraph);
                _needsLayout = true;
            }
        }

        public bool didExceedMaxLines {
            get {
                Debug.Assert(!_needsLayout);
                return _paragraph.didExceedMaxLines;
            }
        }

        public int? maxLines {
            get { return _maxLines; }
            set {
                if (_maxLines == value) {
                    return;
                }

                _maxLines = value;
                Paragraph.release(ref _paragraph);
                _needsLayout = true;
            }
        }

        public StrutStyle strutStyle {
            get { return _strutStyle; }
            set {
                if (_strutStyle == value) {
                    return;
                }

                _strutStyle = value;
                _paragraph = null;
                _needsLayout = true;
            }
        }

        StrutStyle _strutStyle;

        public float minIntrinsicWidth {
            get {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.minIntrinsicWidth);
            }
        }

        public float maxIntrinsicWidth {
            get {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.maxIntrinsicWidth);
            }
        }

        public float height {
            get {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.height);
            }
        }

        public float width {
            get {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.width);
            }
        }

        public float computeDistanceToActualBaseline(TextBaseline baseline) {
            Debug.Assert(!_needsLayout);
            switch (baseline) {
                case TextBaseline.alphabetic:
                    return _paragraph.alphabeticBaseline;
                case TextBaseline.ideographic:
                    return _paragraph.ideographicBaseline;
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
                _text.build(builder, textScaleFactor);
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
        }

        public void paint(Canvas canvas, Offset offset) {
            Debug.Assert(!_needsLayout);
            _paragraph.paint(canvas, offset);
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

        public Paragraph.LineRange getLineRange(int lineNumber) {
            D.assert(!_needsLayout);
            return _paragraph.getLineRange(lineNumber);
        }

        public Paragraph.LineRange getLineRange(TextPosition textPosition) {
            return getLineRange(getLineIndex(textPosition));
        }

        public List<TextBox> getBoxesForSelection(TextSelection selection) {
            D.assert(!_needsLayout);
            var results = _paragraph.getRectsForRange(selection.start, selection.end);
            return results;
        }

        public TextPosition getPositionForOffset(Offset offset) {
            D.assert(!_needsLayout);
            var result = _paragraph.getGlyphPositionAtCoordinate(offset.dx, offset.dy);
            return new TextPosition(result.position, result.affinity);
        }

        public TextRange getWordBoundary(TextPosition position) {
            D.assert(!_needsLayout);
            var range = _paragraph.getWordBoundary(position.offset);
            return new TextRange(range.start, range.end);
        }

        public TextPosition getPositionVerticalMove(TextPosition position, int move) {
            D.assert(!_needsLayout);
            var offset = getOffsetForCaret(position, Rect.zero);
            var lineIndex = Mathf.Min(Mathf.Max(_paragraph.getLine(position) + move, 0),
                _paragraph.getLineCount() - 1);
            var targetLineStart = _paragraph.getLineRange(lineIndex).start;
            var newLineOffset = getOffsetForCaret(new TextPosition(targetLineStart), Rect.zero);
            return getPositionForOffset(new Offset(offset.dx, newLineOffset.dy));
        }

        public int getLineIndex(TextPosition position) {
            D.assert(!_needsLayout);
            return _paragraph.getLine(position);
        }

        public int getLineCount() {
            D.assert(!_needsLayout);
            return _paragraph.getLineCount();
        }

        public TextPosition getWordRight(TextPosition position) {
            D.assert(!_needsLayout);
            var offset = position.offset;
            while (true) {
                var range = _paragraph.getWordBoundary(offset);
                if (range.end == range.start) {
                    break;
                }

                if (!char.IsWhiteSpace((char) (text.codeUnitAt(range.start) ?? 0))) {
                    return new TextPosition(range.end);
                }

                offset = range.end;
            }

            return new TextPosition(offset, position.affinity);
        }

        public TextPosition getWordLeft(TextPosition position) {
            D.assert(!_needsLayout);
            var offset = Mathf.Max(position.offset - 1, 0);
            while (true) {
                var range = _paragraph.getWordBoundary(offset);
                if (!char.IsWhiteSpace((char) (text.codeUnitAt(range.start) ?? 0))) {
                    return new TextPosition(range.start);
                }

                offset = Mathf.Max(range.start - 1, 0);
                if (offset == 0) {
                    break;
                }
            }

            return new TextPosition(offset, position.affinity);
        }

        ParagraphStyle _createParagraphStyle(TextDirection defaultTextDirection = TextDirection.ltr) {
            if (_text.style == null) {
                return new ParagraphStyle(
                    textAlign: textAlign,
                    textDirection: textDirection ?? defaultTextDirection,
                    maxLines: maxLines,
                    ellipsis: ellipsis,
                    strutStyle: _strutStyle
                );
            }

            return _text.style.getParagraphStyle(textAlign, textDirection ?? defaultTextDirection,
                ellipsis, maxLines, textScaleFactor);
        }

        public float preferredLineHeight {
            get {
                if (_layoutTemplate == null) {
                    var builder = new ParagraphBuilder(_createParagraphStyle(TextDirection.ltr)
                    ); // direction doesn't matter, text is just a space
                    if (text != null && text.style != null) {
                        builder.pushStyle(text.style, textScaleFactor);
                    }

                    builder.addText(" ");
                    _layoutTemplate = builder.build();
                    _layoutTemplate.layout(new ParagraphConstraints(float.PositiveInfinity));
                }

                return _layoutTemplate.height;
            }
        }

        float _applyFloatingPointHack(float layoutValue) {
            return Mathf.Ceil(layoutValue);
        }

        const int _zwjUtf16 = 0x200d;

        Rect _getRectFromUpstream(int offset, Rect caretPrototype) {
            string flattenedText = _text.toPlainText();
            var prevCodeUnit = _text.codeUnitAt(Mathf.Max(0, offset - 1));
            if (prevCodeUnit == null) {
                return null;
            }

            bool needsSearch = _isUtf16Surrogate(prevCodeUnit.Value) || _text.codeUnitAt(offset) == _zwjUtf16;
            int graphemeClusterLength = needsSearch ? 2 : 1;
            List<TextBox> boxes = null;
            while ((boxes == null || boxes.isEmpty()) && flattenedText != null) {
                int prevRuneOffset = offset - graphemeClusterLength;
                boxes = _paragraph.getRectsForRange(prevRuneOffset, offset);
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
                return Rect.fromLTRB(Mathf.Min(dx, width), box.top,
                    Mathf.Min(dx, width), box.bottom);
            }

            return null;
        }

        Rect _getRectFromDownStream(int offset, Rect caretPrototype) {
            string flattenedText = _text.toPlainText();
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
                boxes = _paragraph.getRectsForRange(offset, nextRuneOffset);
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
                return Rect.fromLTRB(Mathf.Min(dx, width), box.top,
                    Mathf.Min(dx, width), box.bottom);
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
    }
}