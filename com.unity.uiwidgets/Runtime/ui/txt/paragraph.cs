using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.external;
using UnityEngine;

namespace Unity.UIWidgets.uiOld{
    struct CodeUnitRun {
        public readonly int lineNumber;
        public readonly TextDirection direction;
        public readonly Range<int> codeUnits;
        public Range<float> xPos;
        public readonly int start;
        public readonly int count;

        public CodeUnitRun(Range<int> cu, Range<float> xPos, int line,
            TextDirection direction, int start, int count) {
            lineNumber = line;
            codeUnits = cu;
            this.xPos = xPos;
            this.direction = direction;
            this.start = start;
            this.count = count;
        }

        public GlyphPosition get(int i, GlyphPosition[] glyphPositions) {
            D.assert(i < count);
            return glyphPositions[start + i];
        }
    }


    struct FontMetrics {
        public readonly float ascent;
        public readonly float leading;
        public readonly float descent;
        public readonly float? underlineThickness;
        public readonly float? underlinePosition;
        public readonly float? strikeoutPosition;
        public readonly float? fxHeight;

        static FontMetrics _previousFontMetrics;
        static Font _previousFont;
        static int _previousFontSize;

        public FontMetrics(float ascent, float descent,
            float? underlineThickness = null, float? underlinePosition = null, float? strikeoutPosition = null,
            float? fxHeight = null) {
            this.ascent = ascent;
            this.descent = descent;
            this.underlineThickness = underlineThickness;
            this.underlinePosition = underlinePosition;
            this.strikeoutPosition = strikeoutPosition;
            this.fxHeight = fxHeight;
            leading = 0.0f;
        }

        public static FontMetrics fromFont(Font font, int fontSize) {
            if (fontSize == _previousFontSize && ReferenceEquals(font, _previousFont)) {
                return _previousFontMetrics;
            }

            var ascent = -font.ascent * fontSize / font.fontSize;
            var descent = (font.lineHeight - font.ascent) * fontSize / font.fontSize;
            font.RequestCharactersInTextureSafe("x", fontSize, UnityEngine.FontStyle.Normal);
            font.getGlyphInfo('x', out var glyphInfo, fontSize, UnityEngine.FontStyle.Normal);
            float fxHeight = glyphInfo.glyphHeight;
            _previousFontMetrics = new FontMetrics(ascent, descent, fxHeight: fxHeight);
            _previousFontSize = fontSize;
            _previousFont = font;

            return _previousFontMetrics;
        }
    }

    struct PositionWithAffinity {
        public readonly int position;
        public readonly TextAffinity affinity;

        public PositionWithAffinity(int p, TextAffinity a) {
            position = p;
            affinity = a;
        }
    }

    struct GlyphPosition {
        public Range<float> xPos;
        public readonly int codeUnit;

        public GlyphPosition(float start, float advance, int codeUnit) {
            xPos = new Range<float>(start, start + advance);
            this.codeUnit = codeUnit;
        }

        public void shiftSelf(float shift) {
            xPos = new Range<float>(xPos.start + shift, xPos.end + shift);
        }
    }

    struct Range<T> : IEquatable<Range<T>> {
        public Range(T start, T end) {
            this.start = start;
            this.end = end;
        }

        public bool Equals(Range<T> other) {
            return EqualityComparer<T>.Default.Equals(start, other.start) &&
                   EqualityComparer<T>.Default.Equals(end, other.end);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is Range<T> && Equals((Range<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(start) * 397) ^
                       EqualityComparer<T>.Default.GetHashCode(end);
            }
        }

        public static bool operator ==(Range<T> left, Range<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(Range<T> left, Range<T> right) {
            return !left.Equals(right);
        }

        public readonly T start, end;
    }

    struct GlyphLine {
        public readonly int start;
        public readonly int count;

        public GlyphLine(int start, int count) {
            this.start = start;
            this.count = count;
        }

        public GlyphPosition get(int i, GlyphPosition[] glyphPositions) {
            return glyphPositions[start + i];
        }

        public GlyphPosition last(GlyphPosition[] glyphPositions) {
            return glyphPositions[start + count - 1];
        }
    }


    public class Paragraph {
        public struct LineRange {
            public LineRange(int start, int end, int endExcludingWhitespace, int endIncludingNewLine, bool hardBreak) {
                this.start = start;
                this.end = end;
                this.endExcludingWhitespace = endExcludingWhitespace;
                this.endIncludingNewLine = endIncludingNewLine;
                this.hardBreak = hardBreak;
            }

            public readonly int start;
            public readonly int end;
            public readonly int endExcludingWhitespace;
            public readonly int endIncludingNewLine;
            public readonly bool hardBreak;
        }

        bool _needsLayout = true;

        string _text;
        string _ellipsizedText;
        int _ellipsizedLength;

        StyledRuns _runs;

        ParagraphStyle _paragraphStyle;
        List<LineRange> _lineRanges = new List<LineRange>();

        List<float> _lineWidths = new List<float>();

        GlyphLine[] _glyphLines;
        GlyphPosition[] _glyphPositions;
        PaintRecord[] _paintRecords;
        CodeUnitRun[] _codeUnitRuns;
        float[] _lineHeights;
        float[] _textBlobPositionXs;
        float _maxIntrinsicWidth;
        float _minIntrinsicWidth;
        float _alphabeticBaseline;
        float _ideographicBaseline;
        int _lineCount;
        int _paintRecordsCount;
        int _codeUnitRunsCount;
        int _lineRangeCount;
        int _lineWidthCount;
        bool _didExceedMaxLines;
        TabStops _tabStops = new TabStops();

        static float[] _advancesBuffer;
        static float[] _positionsBuffer;
        static Range<int>[] _wordsBuffer;

        // private float _characterWidth;

        float _width;

        const float kFloatDecorationSpacing = 3.0f;

        public float height {
            get {
                if (_lineHeights == null) {
                    return 0;
                }

                return _lineHeights[getLineCount() - 1];
            }
        }

        public float minIntrinsicWidth {
            get { return _minIntrinsicWidth; }
        }

        public float maxIntrinsicWidth {
            get { return _maxIntrinsicWidth; }
        }

        public float width {
            get { return _width; }
        }


        public float alphabeticBaseline {
            get { return _alphabeticBaseline; }
        }

        public float ideographicBaseline {
            get { return _ideographicBaseline; }
        }

        public bool didExceedMaxLines {
            get { return _didExceedMaxLines; }
        }

        static List<Paragraph> _paragraphPool = new List<Paragraph>();

        public static Paragraph create() {
            if (_paragraphPool.isEmpty()) {
                return new Paragraph();
            }

            Paragraph ret = _paragraphPool.last();
            _paragraphPool.RemoveAt(_paragraphPool.Count - 1);
            return ret;
        }

        public static void release(ref Paragraph paragraph) {
            if (paragraph != null) {
                paragraph.clear();
                _paragraphPool.Add(paragraph);
                paragraph = null;
            }
        }

        public void clear() {
            _needsLayout = true;
            _maxIntrinsicWidth = default;
            _minIntrinsicWidth = default;
            _alphabeticBaseline = default;
            _ideographicBaseline = default;
            _lineCount = default;
            _paintRecordsCount = default;
            _codeUnitRunsCount = default;
            _lineRangeCount = default;
            _lineWidthCount = default;
            _didExceedMaxLines = default;
            _width = default;
            _text = default;
            _ellipsizedText = default;
            _ellipsizedLength = default;
            _runs = null;
            _paragraphStyle = null;
        }

        Paint _textPaint = new Paint {
            filterMode = FilterMode.Bilinear
        };

        Paint _defaultPaint = new Paint {
            filterMode = FilterMode.Bilinear
        };

        public void paint(Canvas canvas, Offset offset) {
            for (int i = 0; i < _paintRecordsCount; i++) {
                var paintRecord = _paintRecords[i];
                paintBackground(canvas, paintRecord, offset);
            }

            for (int i = 0; i < _paintRecordsCount; i++) {
                var paintRecord = _paintRecords[i];
                
                if (paintRecord.style.foreground != null) {
                    _textPaint = paintRecord.style.foreground;
                }
                else {
                    _textPaint = _defaultPaint;
                    _textPaint.color = paintRecord.style.color;
                }

                canvas.drawTextBlob(paintRecord.text, paintRecord.shiftedOffset(offset), _textPaint);
                
                paintDecorations(canvas, paintRecord, offset);
            }
        }

        public void layout(ParagraphConstraints constraints) {
            if (!_needsLayout && _width == constraints.width) {
                return;
            }

            _tabStops.setFont(
                FontManager.instance.getOrCreate(
                    _paragraphStyle.fontFamily ?? TextStyle.kDefaultFontFamily,
                    _paragraphStyle.fontWeight ?? TextStyle.kDefaultFontWeight,
                    _paragraphStyle.fontStyle ?? TextStyle.kDefaultfontStyle).font,
                (int) (_paragraphStyle.fontSize ?? TextStyle.kDefaultFontSize));

            _needsLayout = false;
            _width = Mathf.Floor(constraints.width);

            int lineStyleRunsCount = _computeLineBreak();

            if (_glyphLines == null || _glyphLines.Length < _lineRangeCount) {
                _glyphLines = new GlyphLine[LayoutUtils.minPowerOfTwo(_lineRangeCount)];
            }

            if (_lineHeights == null || _lineHeights.Length < _lineRangeCount) {
                _lineHeights = new float[LayoutUtils.minPowerOfTwo(_lineRangeCount)];
            }

            if (_paintRecords == null || _paintRecords.Length < lineStyleRunsCount) {
                _paintRecords = new PaintRecord[LayoutUtils.minPowerOfTwo(lineStyleRunsCount)];
            }

            _paintRecordsCount = 0;

            if (_codeUnitRuns == null || _codeUnitRuns.Length < lineStyleRunsCount) {
                _codeUnitRuns = new CodeUnitRun[LayoutUtils.minPowerOfTwo(lineStyleRunsCount)];
            }

            _codeUnitRunsCount = 0;

            int styleMaxLines = _paragraphStyle.maxLines ?? int.MaxValue;
            _didExceedMaxLines = _lineRangeCount > styleMaxLines;

            var lineLimit = Mathf.Min(styleMaxLines, _lineRangeCount);
            int styleRunIndex = 0;
            float yOffset = 0;
            float preMaxDescent = 0;
            float maxWordWidth = 0;

            TextBlobBuilder builder = new TextBlobBuilder();
            int ellipsizedLength = _text.Length + (_paragraphStyle.ellipsis?.Length ?? 0);

            // All text blobs share a single position buffer, which is big enough taking ellipsis into consideration
            if (_textBlobPositionXs == null || _textBlobPositionXs.Length < ellipsizedLength) {
                _textBlobPositionXs = new float[LayoutUtils.minPowerOfTwo(ellipsizedLength)];
            }

            builder.setPositionXs(_textBlobPositionXs);
            // this._glyphLines and this._codeUnitRuns will refer to this array for glyph positions
            if (_glyphPositions == null || _glyphPositions.Length < ellipsizedLength) {
                _glyphPositions = new GlyphPosition[LayoutUtils.minPowerOfTwo(ellipsizedLength)];
            }

            // Pointer to the _glyphPositions array, to keep track of where the next glyph is stored
            int pGlyphPositions = 0;

            // Compute max(NumberOfWords(line) for line in lines), to determine the size of word buffers
            int maxWordCount = _computeMaxWordCount();

            if (_wordsBuffer == null || _wordsBuffer.Length < maxWordCount) {
                _wordsBuffer = new Range<int>[maxWordCount < 4 ? 4 : maxWordCount];
            }

            // Iterate through line ranges
            for (int lineNumber = 0; lineNumber < lineLimit; ++lineNumber) {
                var lineRange = _lineRanges[lineNumber];
                int wordIndex = 0;
                float runXOffset = 0;
                float justifyXOffset = 0;

                // Break the line into words if justification should be applied.
                bool justifyLine = _paragraphStyle.textAlign == TextAlign.justify &&
                                   lineNumber != lineLimit - 1 && !lineRange.hardBreak &&
                                   // Do not apply justify if ellipsis should be added, or the ellipsis may be pushed
                                   // out of the border.
                                   // This is still not taken care of in the flutter engine.
                                   !(_paragraphStyle.ellipsized() && _paragraphStyle.maxLines == null);

                int wordCount = _findWords(lineRange.start, lineRange.end, _wordsBuffer);
                float wordGapWidth = !(justifyLine && wordCount > 1)
                    ? 0
                    : (_width - _lineWidths[lineNumber]) / (wordCount - 1);

                // Count the number of style runs, and compute the character number of the longest run by the way
                int lineStyleRunCount = _countLineStyleRuns(lineRange, styleRunIndex, out int maxTextCount);

                string ellipsis = _paragraphStyle.ellipsis;
                bool hardBreak = lineRange.hardBreak;

                if (!string.IsNullOrEmpty(ellipsis) && !hardBreak && !_width.isInfinite() &&
                    (lineNumber == lineLimit - 1 || _paragraphStyle.maxLines == null)) {
                    maxTextCount += ellipsis.Length;
                }

                // Allocate the advances and positions to store the layout result
                // TODO: find a way to compute the maxTextCount for the entire paragraph, so that this allocation
                //       happens only once
                if (_advancesBuffer == null || _advancesBuffer.Length < maxTextCount) {
                    _advancesBuffer = new float[LayoutUtils.minPowerOfTwo(maxTextCount)];
                }

                if (_positionsBuffer == null || _positionsBuffer.Length < maxTextCount) {
                    _positionsBuffer = new float[LayoutUtils.minPowerOfTwo(maxTextCount)];
                }

                // Keep of the position in _glyphPositions before evaluating this line
                int glyphPositionLineStart = pGlyphPositions;

                if (lineStyleRunCount != 0) {
                    // Exclude trailing whitespace from right-justified lines so the last
                    // visible character in the line will be flush with the right margin.
                    int lineEndIndex = _paragraphStyle.textAlign == TextAlign.right ||
                                       _paragraphStyle.textAlign == TextAlign.center
                        ? lineRange.endExcludingWhitespace
                        : lineRange.end;
                    int lineStyleRunIndex = 0;

                    // Instead of computing all lineStyleRuns at once and store into an array and iterate through them,
                    // compute each lineStyleRun and deal with it on the fly, to save the storage for the runs
                    while (styleRunIndex < _runs.size) {
                        var styleRun = _runs.getRun(styleRunIndex);
                        // Compute the intersection between current style run intersects and the line
                        int start = Mathf.Max(styleRun.start, lineRange.start);
                        int end = Mathf.Min(styleRun.end, lineEndIndex);
                        // Make sure that each run is not empty
                        if (start < end) {
                            var style = styleRun.style;
                            string text = _text;
                            int textStart = start;
                            int textEnd = end;
                            int textCount = textEnd - textStart;
                            // Keep track of the pointer to _glyphPositions in the start of this run
                            int glyphPositionStyleRunStart = pGlyphPositions;

                            // Ellipsize the text if ellipsis string is set, and this is the last lineStyleRun of
                            // the current line, and this is the last line or max line is not set
                            if (!string.IsNullOrEmpty(ellipsis) && !hardBreak && !_width.isInfinite() &&
                                lineStyleRunIndex == lineStyleRunCount - 1 &&
                                (lineNumber == lineLimit - 1 || _paragraphStyle.maxLines == null)) {
                                float ellipsisWidth = Layout.measureText(ellipsis, style);

                                // Find the minimum number of characters to truncate, so that the truncated text
                                // appended with ellipsis is within the constraints of line width
                                int truncateCount = Layout.computeTruncateCount(runXOffset, text, textStart,
                                    textCount, style, _width - ellipsisWidth, _tabStops);

                                // If all the positions have not changed, use the cached ellipsized text
                                // else update the cache
                                if (!(_ellipsizedText != null &&
                                      _ellipsizedLength == textStart + textCount - truncateCount &&
                                      _ellipsizedText.Length == _ellipsizedLength + ellipsis.Length &&
                                      _ellipsizedText.EndsWith(ellipsis))) {
                                    _ellipsizedText =
                                        text.Substring(0, textStart + textCount - truncateCount) + ellipsis;
                                    _ellipsizedLength = _ellipsizedText.Length - ellipsis.Length;
                                }

                                text = _ellipsizedText;
                                textCount = text.Length - textStart;
                                D.assert(textCount != 0);
                                if (_paragraphStyle.maxLines == null) {
                                    lineLimit = lineNumber + 1;
                                    _didExceedMaxLines = true;
                                }
                            }

                            float advance = Layout.doLayout(runXOffset, text, textStart, textCount, style,
                                _advancesBuffer, _positionsBuffer, _tabStops, out var bounds);

                            builder.allocRunPos(style, text, textStart, textCount);
                            builder.setBounds(bounds);

                            // Update the max width of the words
                            // Fill in the glyph positions, and the positions of the text blob builder
                            float wordStartPosition = float.NaN;
                            for (int glyphIndex = 0; glyphIndex < textCount; ++glyphIndex) {
                                float glyphXOffset = _positionsBuffer[glyphIndex] + justifyXOffset;
                                float glyphAdvance = _advancesBuffer[glyphIndex];
                                builder.setPositionX(glyphIndex, glyphXOffset);
                                _glyphPositions[pGlyphPositions++] = new GlyphPosition(runXOffset + glyphXOffset,
                                    glyphAdvance, textStart + glyphIndex);
                                if (wordIndex < wordCount) {
                                    Range<int> word = _wordsBuffer[wordIndex];
                                    // Run into the start of current word, record the start position of this word
                                    if (word.start == start + glyphIndex) {
                                        wordStartPosition = runXOffset + glyphXOffset;
                                    }

                                    // Run into the end of current word
                                    if (word.end == start + glyphIndex + 1) {
                                        if (justifyLine) {
                                            justifyXOffset += wordGapWidth;
                                        }

                                        // Update the current word
                                        wordIndex++;
                                        // If the start position of this word has been recorded, calculate the
                                        // width of this word, and update the entire word
                                        if (!float.IsNaN(wordStartPosition)) {
                                            maxWordWidth = Mathf.Max(maxWordWidth,
                                                _glyphPositions[pGlyphPositions - 1].xPos.end - wordStartPosition);
                                            wordStartPosition = float.NaN;
                                        }
                                    }
                                }
                            }

                            // Create paint record
                            var font = FontManager.instance.getOrCreate(style.fontFamily,
                                style.fontWeight, style.fontStyle).font;
                            var metrics = FontMetrics.fromFont(font, style.UnityFontSize);
                            PaintRecord paintRecord = new PaintRecord(style, runXOffset, 0, builder.make(),
                                metrics, advance);
                            _paintRecords[_paintRecordsCount++] = paintRecord;
                            runXOffset += advance;

                            // Create code unit run
                            _codeUnitRuns[_codeUnitRunsCount++] = new CodeUnitRun(
                                new Range<int>(start, end),
                                new Range<float>(_glyphPositions[glyphPositionStyleRunStart].xPos.start,
                                    _glyphPositions[pGlyphPositions - 1].xPos.end),
                                lineNumber, TextDirection.ltr, glyphPositionStyleRunStart, textCount);

                            lineStyleRunIndex++;
                        }

                        if (styleRun.end >= lineEndIndex) {
                            break;
                        }

                        styleRunIndex++;
                    }
                }

                float maxLineSpacing = 0;
                float maxDescent = 0;

                void updateLineMetrics(FontMetrics metrics, float styleHeight) {
                    float lineSpacing = lineNumber == 0
                        ? -metrics.ascent * styleHeight
                        : (-metrics.ascent + metrics.leading) * styleHeight;
                    if (lineSpacing > maxLineSpacing) {
                        maxLineSpacing = lineSpacing;
                        if (lineNumber == 0) {
                            _alphabeticBaseline = lineSpacing;
                            _ideographicBaseline =
                                (metrics.underlinePosition ?? 0.0f - metrics.ascent) * styleHeight;
                        }
                    }

                    float descent = metrics.descent * styleHeight;
                    maxDescent = Mathf.Max(descent, maxDescent);
                }

                if (lineStyleRunCount != 0) {
                    for (int i = 0; i < lineStyleRunCount; i++) {
                        var paintRecord = _paintRecords[_paintRecordsCount - i - 1];
                        updateLineMetrics(paintRecord.metrics, paintRecord.style.height);
                    }
                }
                else {
                    var defaultFont = FontManager.instance.getOrCreate(
                        _paragraphStyle.fontFamily ?? TextStyle.kDefaultFontFamily,
                        _paragraphStyle.fontWeight ?? TextStyle.kDefaultFontWeight,
                        _paragraphStyle.fontStyle ?? TextStyle.kDefaultfontStyle).font;
                    var metrics = FontMetrics.fromFont(defaultFont,
                        (int) (_paragraphStyle.fontSize ?? TextStyle.kDefaultFontSize));
                    updateLineMetrics(metrics, _paragraphStyle.height ?? TextStyle.kDefaultHeight);
                }

                _lineHeights[lineNumber] = ((lineNumber == 0 ? 0 : _lineHeights[lineNumber - 1])
                                                 + Mathf.Round(maxLineSpacing + maxDescent));
                yOffset += Mathf.Round(maxLineSpacing + preMaxDescent);
                preMaxDescent = maxDescent;
                float lineXOffset = getLineXOffset(runXOffset);
                int count = pGlyphPositions - glyphPositionLineStart;
                if (lineXOffset != 0 && _glyphPositions != null) {
                    for (int i = 0; i < count; ++i) {
                        _glyphPositions[_glyphPositions.Length - i - 1].shiftSelf(lineXOffset);
                    }
                }

                _glyphLines[lineNumber] = new GlyphLine(glyphPositionLineStart, count);
                for (int i = 0; i < lineStyleRunCount; i++) {
                    var paintRecord = _paintRecords[_paintRecordsCount - 1 - i];
                    paintRecord.shift(lineXOffset, yOffset);
                    _paintRecords[_paintRecordsCount - 1 - i] = paintRecord;
                }
            }

            _lineCount = lineLimit;

            // min intrinsic width := minimum width this paragraph has to take, which equals the maximum word width
            if (_paragraphStyle.maxLines == 1 || (_paragraphStyle.maxLines == null &&
                                                       _paragraphStyle.ellipsized())) {
                _minIntrinsicWidth = maxIntrinsicWidth;
            }
            else {
                _minIntrinsicWidth = Mathf.Min(maxWordWidth, maxIntrinsicWidth);
            }
        }

        int _countLineStyleRuns(LineRange lineRange, int styleRunIndex, out int maxTextCount) {
            // Exclude trailing whitespace from right-justified lines so the last
            // visible character in the line will be flush with the right margin.
            int lineEndIndex = _paragraphStyle.textAlign == TextAlign.right ||
                               _paragraphStyle.textAlign == TextAlign.center
                ? lineRange.endExcludingWhitespace
                : lineRange.end;

            maxTextCount = 0;
            int lineStyleRunCount = 0;
            for (int i = styleRunIndex; i < _runs.size; i++) {
                var styleRun = _runs.getRun(i);
                int start = Mathf.Max(styleRun.start, lineRange.start);
                int end = Mathf.Min(styleRun.end, lineEndIndex);
                // Make sure that each line is not empty
                if (start < end) {
                    lineStyleRunCount++;
                    maxTextCount = Math.Max(end - start, maxTextCount);
                }

                if (styleRun.end >= lineEndIndex) {
                    break;
                }
            }

            return lineStyleRunCount;
        }

        internal int totalCodeUnitsInLine(int lineNumber) {
            int lineStart = _lineRanges[lineNumber].start;
            int nextLineStart = lineNumber < _lineRangeCount - 1
                ? _lineRanges[lineNumber + 1].start
                : _text.Length;
            return nextLineStart - lineStart;
        }

        internal void setText(string text, StyledRuns runs) {
            _text = text;
            _runs = runs;
            _needsLayout = true;
        }

        public void setParagraphStyle(ParagraphStyle style) {
            _needsLayout = true;
            _paragraphStyle = style;
        }

        public List<TextBox> getRectsForRange(int start, int end) {
            var lineBoxes = new SplayTree<int, List<TextBox>>();
            for (int runIndex = 0; runIndex < _codeUnitRunsCount; runIndex++) {
                var run = _codeUnitRuns[runIndex];
                if (run.codeUnits.start >= end) {
                    break;
                }

                if (run.codeUnits.end <= start) {
                    continue;
                }

                float top = (run.lineNumber == 0) ? 0 : _lineHeights[run.lineNumber - 1];
                float bottom = _lineHeights[run.lineNumber];
                float left, right;
                if (run.codeUnits.start >= start && run.codeUnits.end <= end) {
                    left = run.xPos.start;
                    right = run.xPos.end;
                }
                else {
                    left = float.MaxValue;
                    right = float.MinValue;
                    for (int i = 0; i < run.count; i++) {
                        var gp = run.get(i, _glyphPositions);
                        if (gp.codeUnit >= start && gp.codeUnit + 1 <= end) {
                            left = Mathf.Min(left, gp.xPos.start);
                            right = Mathf.Max(right, gp.xPos.end);
                        }
                    }

                    if (left == float.MaxValue || right == float.MinValue) {
                        continue;
                    }
                }

                List<TextBox> boxs;
                if (!lineBoxes.TryGetValue(run.lineNumber, out boxs)) {
                    boxs = new List<TextBox>();
                    lineBoxes.Add(run.lineNumber, boxs);
                }

                boxs.Add(TextBox.fromLTBD(left, top, right, bottom, run.direction));
            }

            for (int lineNumber = 0; lineNumber < _lineRangeCount; ++lineNumber) {
                var line = _lineRanges[lineNumber];
                if (line.start >= end) {
                    break;
                }

                if (line.endIncludingNewLine <= start) {
                    continue;
                }

                if (!lineBoxes.ContainsKey(lineNumber)) {
                    if (line.end != line.endIncludingNewLine && line.end >= start && line.endIncludingNewLine <= end) {
                        var x = _lineWidths[lineNumber];
                        var top = (lineNumber > 0) ? _lineHeights[lineNumber - 1] : 0;
                        var bottom = _lineHeights[lineNumber];
                        lineBoxes.Add(lineNumber, new List<TextBox> {
                            TextBox.fromLTBD(
                                x, top, x, bottom, TextDirection.ltr)
                        });
                    }
                }
            }

            var result = new List<TextBox>();
            foreach (var keyValuePair in lineBoxes) {
                result.AddRange(keyValuePair.Value);
            }

            return result;
        }

        public float? getNextLineStartRectTop() {
            if (_text.Length == 0 || _text[_text.Length - 1] != '\n') {
                return null;
            }

            var lineNumber = getLineCount() - 1;
            return lineNumber > 0 ? _lineHeights[lineNumber - 1] : 0;
        }

        internal PositionWithAffinity getGlyphPositionAtCoordinate(float dx, float dy) {
            if (_lineHeights == null) {
                return new PositionWithAffinity(0, TextAffinity.downstream);
            }

            int yIndex;
            for (yIndex = 0; yIndex < getLineCount() - 1; ++yIndex) {
                if (dy < _lineHeights[yIndex]) {
                    break;
                }
            }

            GlyphLine glyphLine = _glyphLines[yIndex];
            if (glyphLine.count == 0) {
                int lineStartIndex = 0;
                for (int i = 0; i < yIndex; i++) {
                    lineStartIndex += totalCodeUnitsInLine(i);
                }

                return new PositionWithAffinity(lineStartIndex, TextAffinity.downstream);
            }


            GlyphPosition gp = new GlyphPosition();
            bool gpSet = false;
            for (int xIndex = 0; xIndex < glyphLine.count; ++xIndex) {
                float glyphEnd = xIndex < glyphLine.count - 1
                    ? glyphLine.get(xIndex + 1, _glyphPositions).xPos.start
                    : glyphLine.get(xIndex, _glyphPositions).xPos.end;
                if (dx < glyphEnd) {
                    gp = glyphLine.get(xIndex, _glyphPositions);
                    gpSet = true;
                    break;
                }
            }

            if (!gpSet) {
                GlyphPosition lastGlyph = glyphLine.last(_glyphPositions);
                return new PositionWithAffinity(lastGlyph.codeUnit + 1, TextAffinity.upstream);
            }

            TextDirection direction = TextDirection.ltr;
            for (int runIndex = 0; runIndex < _codeUnitRunsCount; runIndex++) {
                var run = _codeUnitRuns[runIndex];
                if (gp.codeUnit >= run.codeUnits.start && gp.codeUnit + 1 <= run.codeUnits.end) {
                    direction = run.direction;
                    break;
                }
            }

            float glyphCenter = (gp.xPos.start + gp.xPos.end) / 2;
            if ((direction == TextDirection.ltr && dx < glyphCenter) ||
                (direction == TextDirection.rtl && dx >= glyphCenter)) {
                return new PositionWithAffinity(gp.codeUnit, TextAffinity.downstream);
            }

            return new PositionWithAffinity(gp.codeUnit + 1, TextAffinity.upstream);
        }

        public int getLine(TextPosition position) {
            D.assert(!_needsLayout);
            if (position.offset < 0) {
                return 0;
            }

            var offset = position.offset;
            if (position.affinity == TextAffinity.upstream && offset > 0) {
                offset = char.IsSurrogate(_text[offset - 1]) ? offset - 2 : offset - 1;
            }

            var lineCount = getLineCount();
            for (int lineIndex = 0; lineIndex < lineCount; ++lineIndex) {
                var line = _lineRanges[lineIndex];
                if ((offset >= line.start && offset < line.endIncludingNewLine)) {
                    return lineIndex;
                }
            }

            return Mathf.Max(lineCount - 1, 0);
        }

        internal LineRange getLineRange(int lineIndex) {
            return _lineRanges[lineIndex];
        }

        internal Range<int> getWordBoundary(int offset) {
            WordSeparate s = new WordSeparate(_text);
            return s.findWordRange(offset);
        }

        public int getLineCount() {
            return _lineCount;
        }

        int _computeLineBreak() {
            _lineRangeCount = 0;
            _lineWidthCount = 0;
            _maxIntrinsicWidth = 0;

            int lineLimit = _paragraphStyle.ellipsized()
                ? _paragraphStyle.maxLines ?? 1
                : _paragraphStyle.maxLines ?? 0;

            var newLinePositions = LineBreaker.newLinePositions(_text, out int newLineCount);

            var lineBreaker = LineBreaker.instance;
            int runIndex = 0;
            int countRuns = 0;
            for (var newlineIndex = 0; newlineIndex < newLineCount; ++newlineIndex) {
                if (lineLimit != 0 && _lineRangeCount >= lineLimit) {
                    break;
                }

                var blockStart = newlineIndex > 0 ? newLinePositions[newlineIndex - 1] + 1 : 0;
                var blockEnd = newLinePositions[newlineIndex];
                var blockSize = blockEnd - blockStart;
                if (blockSize == 0) {
                    _addEmptyLine(blockStart, blockEnd);
                    continue;
                }

                if (lineLimit != 0 && _lineRangeCount >= lineLimit) {
                    break;
                }

                _resetLineBreaker(lineBreaker, blockStart, blockSize,
                    lineLimit == 0 ? 0 : lineLimit - _lineRangeCount);
                countRuns += _addStyleRuns(lineBreaker, ref runIndex, blockStart, blockEnd);

                int breaksCount = lineBreaker.computeBreaks();
                countRuns += breaksCount - 1;
                _updateBreaks(lineBreaker, breaksCount, blockStart, blockEnd);

                lineBreaker.finish();
            }

            return countRuns;
        }

        void _addLineRangeAndWidth(LineRange lineRange, float width) {
            if (_lineRanges.Count <= _lineRangeCount) {
                _lineRanges.Add(lineRange);
                _lineRangeCount++;
            }
            else {
                _lineRanges[_lineRangeCount++] = lineRange;
            }

            if (_lineWidths.Count <= _lineWidthCount) {
                _lineWidths.Add(width);
                _lineWidthCount++;
            }
            else {
                _lineWidths[_lineWidthCount++] = width;
            }
        }

        void _addEmptyLine(int blockStart, int blockEnd) {
            _addLineRangeAndWidth(new LineRange(blockStart, blockEnd, blockEnd,
                blockEnd < _text.Length ? blockEnd + 1 : blockEnd, true), 0);
        }

        void _resetLineBreaker(LineBreaker lineBreaker, int blockStart, int blockSize, int lineLimit) {
            lineBreaker.setLineWidth(_width);
            lineBreaker.resize(blockSize);
            lineBreaker.setTabStops(_tabStops);
            lineBreaker.setText(_text, blockStart, blockSize);
            lineBreaker.lineLimit = lineLimit;
        }

        int _addStyleRuns(LineBreaker lineBreaker, ref int runIndex, int blockStart, int blockEnd) {
            int countRuns = 0;
            float lineBlockWidth = 0;
            while (runIndex < _runs.size) {
                var run = _runs.getRun(runIndex);
                if (run.start >= blockEnd) {
                    break;
                }

                if (lineBreaker.lineLimit != 0 && lineBreaker.getBreaksCount() >= lineBreaker.lineLimit) {
                    break;
                }

                if (run.end <= blockStart) {
                    runIndex++;
                    continue;
                }

                int runStart = Mathf.Max(run.start, blockStart) - blockStart;
                int runEnd = Mathf.Min(run.end, blockEnd) - blockStart;
                lineBlockWidth += lineBreaker.addStyleRun(run.style, runStart, runEnd);

                countRuns++;

                if (run.end > blockEnd) {
                    break;
                }

                runIndex++;
            }

            _maxIntrinsicWidth = Mathf.Max(lineBlockWidth, _maxIntrinsicWidth);

            return countRuns;
        }

        void _updateBreaks(LineBreaker lineBreaker, int breaksCount, int blockStart, int blockEnd) {
            for (int i = 0; i < breaksCount; ++i) {
                var breakStart = i > 0 ? lineBreaker.getBreak(i - 1) : 0;
                var lineStart = breakStart + blockStart;
                var lineEnd = lineBreaker.getBreak(i) + blockStart;
                bool hardBreak = lineEnd == blockEnd;
                var lineEndIncludingNewline =
                    hardBreak && lineEnd < _text.Length ? lineEnd + 1 : lineEnd;
                var lineEndExcludingWhitespace = lineEnd;
                while (lineEndExcludingWhitespace > lineStart &&
                       LayoutUtils.isLineEndSpace(_text[lineEndExcludingWhitespace - 1])) {
                    lineEndExcludingWhitespace--;
                }

                _addLineRangeAndWidth(new LineRange(lineStart, lineEnd,
                    lineEndExcludingWhitespace, lineEndIncludingNewline, hardBreak), lineBreaker.getWidth(i));
            }
        }

        int _computeMaxWordCount() {
            int max = 0;
            for (int lineNumber = 0; lineNumber < _lineRangeCount; lineNumber++) {
                var inWord = false;
                int wordCount = 0, start = _lineRanges[lineNumber].start, end = _lineRanges[lineNumber].end;
                for (int i = start; i < end; ++i) {
                    bool isSpace = LayoutUtils.isWordSpace(_text[i]);
                    if (!inWord && !isSpace) {
                        inWord = true;
                    }
                    else if (inWord && isSpace) {
                        inWord = false;
                        wordCount++;
                    }
                }

                if (inWord) {
                    wordCount++;
                }

                if (wordCount > max) {
                    max = wordCount;
                }
            }

            return max;
        }

        int _findWords(int start, int end, Range<int>[] words) {
            var inWord = false;
            int wordCount = 0;
            int wordStart = 0;

            for (int i = start; i < end; ++i) {
                bool isSpace = LayoutUtils.isWordSpace(_text[i]);
                if (!inWord && !isSpace) {
                    wordStart = i;
                    inWord = true;
                }
                else if (inWord && isSpace) {
                    inWord = false;
                    words[wordCount++] = new Range<int>(wordStart, i);
                }
            }

            if (inWord) {
                words[wordCount] = new Range<int>(wordStart, end);
            }

            return wordCount;
        }

        void paintDecorations(Canvas canvas, PaintRecord record, Offset baseOffset) {
            if (record.style.decoration == null || record.style.decoration == TextDecoration.none) {
                return;
            }

            if (record.style.decorationColor == null) {
                _textPaint.color = record.style.color;
            }
            else {
                _textPaint.color = record.style.decorationColor;
            }


            var width = record.runWidth;
            var metrics = record.metrics;
            float underLineThickness = metrics.underlineThickness ?? (record.style.fontSize / 14.0f);
            _textPaint.style = PaintingStyle.stroke;
            _textPaint.strokeWidth = underLineThickness;
            var recordOffset = record.shiftedOffset(baseOffset);
            var x = recordOffset.dx;
            var y = recordOffset.dy;

            int decorationCount = 1;
            switch (record.style.decorationStyle) {
                case TextDecorationStyle.doubleLine:
                    decorationCount = 2;
                    break;
            }


            var decoration = record.style.decoration;
            for (int i = 0; i < decorationCount; i++) {
                float yOffset = i * underLineThickness * kFloatDecorationSpacing;
                float yOffsetOriginal = yOffset;
                if (decoration != null && decoration.contains(TextDecoration.underline)) {
                    // underline
                    yOffset += metrics.underlinePosition ?? underLineThickness;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), _textPaint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(TextDecoration.overline)) {
                    yOffset += metrics.ascent;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), _textPaint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(TextDecoration.lineThrough)) {
                    yOffset += (decorationCount - 1.0f) * underLineThickness * kFloatDecorationSpacing / -2.0f;
                    yOffset += metrics.strikeoutPosition ?? (metrics.fxHeight ?? 0) / -2.0f;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), _textPaint);
                    yOffset = yOffsetOriginal;
                }
            }

            _textPaint.style = PaintingStyle.fill;
            _textPaint.strokeWidth = 0;
        }

        void paintBackground(Canvas canvas, PaintRecord record, Offset baseOffset) {
            if (record.style.background == null) {
                return;
            }

            var metrics = record.metrics;
            Rect rect = Rect.fromLTRB(0, metrics.ascent, record.runWidth, metrics.descent);
            rect = rect.shift(record.shiftedOffset(baseOffset));
            canvas.drawRect(rect, record.style.background);
        }

        float getLineXOffset(float lineTotalAdvance) {
            if (_width.isInfinite()) {
                return 0;
            }

            if (_paragraphStyle.textAlign == TextAlign.right) {
                return _width - lineTotalAdvance;
            }

            if (_paragraphStyle.textAlign == TextAlign.center) {
                return (_width - lineTotalAdvance) / 2;
            }

            return 0;
        }
    }


}