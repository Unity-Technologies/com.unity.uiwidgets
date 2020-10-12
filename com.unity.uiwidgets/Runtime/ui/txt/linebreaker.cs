using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.uiOld{
    class TabStops {
        int _tabWidth = int.MaxValue;

        Font _font;

        int _fontSize;

        int _spaceAdvance;

        const int kTabSpaceCount = 4;

        public void setFont(Font font, int size) {
            if (_font != font || _fontSize != size) {
                _tabWidth = int.MaxValue;
            }

            _font = font;
            // Recompute the advance of space (' ') if font size changes
            if (_fontSize != size) {
                _fontSize = size;
                _font.RequestCharactersInTextureSafe(" ", _fontSize);
                _font.getGlyphInfo(' ', out var glyphInfo, _fontSize, UnityEngine.FontStyle.Normal);
                _spaceAdvance = glyphInfo.advance;
            }
        }

        public float nextTab(float widthSoFar) {
            if (_tabWidth == int.MaxValue) {
                if (_fontSize > 0) {
                    _tabWidth = _spaceAdvance * kTabSpaceCount;
                }
            }

            if (_tabWidth == 0) {
                return widthSoFar;
            }

            return (Mathf.Floor(widthSoFar / _tabWidth + 1) * _tabWidth);
        }
    }

    struct Candidate {
        public int offset;
        public int pre;
        public float preBreak;
        public float penalty;

        public float postBreak;
        public int preSpaceCount;
        public int postSpaceCount;
    }

    class LineBreaker {
        const float ScoreInfty = float.MaxValue;
        const float ScoreDesperate = 1e10f;

        int _lineLimit = 0;

        // Limit number of lines, 0 means no limit
        public int lineLimit {
            get { return _lineLimit; }
            set { _lineLimit = value; }
        }

        public static LineBreaker instance {
            get {
                if (_instance == null) {
                    _instance = new LineBreaker();
                }

                return _instance;
            }
        }

        static LineBreaker _instance;

        public static int[] newLinePositions(string text, out int count) {
            count = 0;
            for (var i = 0; i < text.Length; i++) {
                if (text[i] == '\n') {
                    count++;
                }
            }

            count++;

            if (_newLinePositions == null || _newLinePositions.Length < count) {
                _newLinePositions = new int[count];
            }

            count = 0;
            for (var i = 0; i < text.Length; i++) {
                if (text[i] == '\n') {
                    _newLinePositions[count++] = i;
                }
            }

            _newLinePositions[count++] = text.Length;

            return _newLinePositions;
        }

        static int[] _newLinePositions;

        TextBuff _textBuf;
        float[] _charWidths;
        List<int> _breaks = new List<int>();
        int _breaksCount = 0;
        List<float> _widths = new List<float>();
        int _widthsCount = 0;
        WordBreaker _wordBreaker = new WordBreaker();
        float _width = 0.0f;
        float _preBreak;
        float _lineWidth;
        int _lastBreak;
        int _bestBreak;
        float _bestScore;
        int _spaceCount;
        TabStops _tabStops;
        int mFirstTabIndex;
        List<Candidate> _candidates = new List<Candidate>();
        int _candidatesCount = 0;

        public int computeBreaks() {
            int nCand = _candidatesCount;
            if (nCand > 0 && (nCand == 1 || _lastBreak != nCand - 1)) {
                var cand = _candidates[_candidatesCount - 1];
                _pushBreak(cand.offset, (cand.postBreak - _preBreak));
            }

            return _breaksCount;
        }

        public int getBreaksCount() {
            return _breaksCount;
        }

        public int getBreak(int i) {
            return _breaks[i];
        }

        public float getWidth(int i) {
            return _widths[i];
        }

        public void resize(int size) {
            if (_charWidths == null || _charWidths.Length < size) {
                _charWidths = new float[LayoutUtils.minPowerOfTwo(size)];
            }
        }

        public void setText(string text, int textOffset, int textLength) {
            _textBuf = new TextBuff(text, textOffset, textLength);
            _wordBreaker.setText(_textBuf);
            _wordBreaker.next();
            _candidatesCount = 0;
            Candidate can = new Candidate {
                offset = 0, postBreak = 0, preBreak = 0, postSpaceCount = 0, preSpaceCount = 0, pre = 0
            };
            _addCandidateToList(can);
            _lastBreak = 0;
            _bestBreak = 0;
            _bestScore = ScoreInfty;
            _preBreak = 0;
            mFirstTabIndex = int.MaxValue;
            _spaceCount = 0;
        }

        public void setLineWidth(float lineWidth) {
            _lineWidth = lineWidth;
        }

        public float addStyleRun(TextStyle style, int start, int end) {
            float width = 0;
            if (style != null) {
//                Layout.measureText(this._width - this._preBreak, this._textBuf,
//                    start, end - start, style,
//                    this._charWidths, start, this._tabStops);
                width = Layout.computeCharWidths(_width - _preBreak, _textBuf.text,
                    _textBuf.offset + start, end - start, style,
                    _charWidths, start, _tabStops);
            }

            int current = _wordBreaker.current();
            float postBreak = _width;
            int postSpaceCount = _spaceCount;

            for (int i = start; i < end; i++) {
                char c = _textBuf.charAt(i);
                if (c == '\t') {
                    _width = _preBreak + _tabStops.nextTab(_width - _preBreak);
                    if (mFirstTabIndex == int.MaxValue) {
                        mFirstTabIndex = i;
                    }
                }
                else {
                    if (LayoutUtils.isWordSpace(c)) {
                        _spaceCount += 1;
                    }

                    _width += _charWidths[i];
                    if (!LayoutUtils.isLineEndSpace(c)) {
                        postBreak = _width;
                        postSpaceCount = _spaceCount;
                    }
                }

                if (i + 1 == current) {
                    if (style != null || current == end || _charWidths[current] > 0) {
                        _addWordBreak(current, _width, postBreak, _spaceCount, postSpaceCount, 0);
                    }

                    current = _wordBreaker.next();
                }
            }

            return width;
        }

        public void finish() {
            _wordBreaker.finish();
            _width = 0;
            _candidatesCount = 0;
            _breaksCount = 0;
            _widthsCount = 0;
            _textBuf = default;
        }

        public int getWidthsCount() {
            return _widthsCount;
        }

        public void setTabStops(TabStops tabStops) {
            _tabStops = tabStops;
        }

        void _addWordBreak(int offset, float preBreak, float postBreak, int preSpaceCount, int postSpaceCount,
            float penalty) {
            float width = _candidates[_candidatesCount - 1].preBreak;
            if (postBreak - width > _lineWidth) {
                _addCandidatesInsideWord(width, offset, postSpaceCount);
            }

            _addCandidate(new Candidate {
                offset = offset,
                preBreak = preBreak,
                postBreak = postBreak,
                preSpaceCount = preSpaceCount,
                postSpaceCount = postSpaceCount,
                penalty = penalty
            });
        }

        void _addCandidatesInsideWord(float width, int offset, int postSpaceCount) {
            int i = _candidates[_candidatesCount - 1].offset;
            width += _charWidths[i++];
            for (; i < offset; i++) {
                float w = _charWidths[i];
                if (w > 0) {
                    _addCandidate(new Candidate {
                        offset = i,
                        preBreak = width,
                        postBreak = width,
                        preSpaceCount = postSpaceCount,
                        postSpaceCount = postSpaceCount,
                        penalty = ScoreDesperate,
                    });
                    width += w;
                }
            }
        }

        void _addCandidateToList(Candidate cand) {
            if (_candidates.Count == _candidatesCount) {
                _candidates.Add(cand);
                _candidatesCount++;
            }
            else {
                _candidates[_candidatesCount++] = cand;
            }
        }

        void _addCandidate(Candidate cand) {
            int candIndex = _candidatesCount;
            _addCandidateToList(cand);
            if (cand.postBreak - _preBreak > _lineWidth) {
                if (_bestBreak == _lastBreak) {
                    _bestBreak = candIndex;
                }

                _pushGreedyBreak();
            }

            while (_lastBreak != candIndex && cand.postBreak - _preBreak > _lineWidth) {
                for (int i = _lastBreak + 1; i < candIndex; i++) {
                    float penalty = _candidates[i].penalty;
                    if (penalty <= _bestScore) {
                        _bestBreak = i;
                        _bestScore = penalty;
                    }
                }

                if (_bestBreak == _lastBreak) {
                    _bestBreak = candIndex;
                }

                _pushGreedyBreak();
            }

            if (cand.penalty <= _bestScore) {
                _bestBreak = candIndex;
                _bestScore = cand.penalty;
            }
        }

        void _pushGreedyBreak() {
            var bestCandidate = _candidates[_bestBreak];
            _pushBreak(bestCandidate.offset, bestCandidate.postBreak - _preBreak);
            _bestScore = ScoreInfty;
            _lastBreak = _bestBreak;
            _preBreak = bestCandidate.preBreak;
        }

        void _pushBreak(int offset, float width) {
            if (lineLimit == 0 || _breaksCount < lineLimit) {
                if (_breaks.Count == _breaksCount) {
                    _breaks.Add(offset);
                    _breaksCount++;
                }
                else {
                    _breaks[_breaksCount++] = offset;
                }

                if (_widths.Count == _widthsCount) {
                    _widths.Add(width);
                    _widthsCount++;
                }
                else {
                    _widths[_widthsCount++] = width;
                }
            }
        }
    }
}