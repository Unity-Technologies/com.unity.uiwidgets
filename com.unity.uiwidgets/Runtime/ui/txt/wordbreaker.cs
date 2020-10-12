namespace Unity.UIWidgets.uiOld{
    struct WordBreaker {
        public const uint U16_SURROGATE_OFFSET = ((0xd800 << 10) + 0xdc00 - 0x10000);
        TextBuff _text;
        int _current;
        int _last;
        int _scanOffset;
        bool _inEmailOrUrl;


        public int next() {
            _last = _current;
            _detectEmailOrUrl();
            if (_inEmailOrUrl) {
                _current = _findNextBreakInEmailOrUrl();
            }
            else {
                _current = _findNextBoundaryNormal();
            }

            return _current;
        }

        public void setText(TextBuff text) {
            _text = text;
            _last = 0;
            _current = 0;
            _scanOffset = 0;
            _inEmailOrUrl = false;
            // this.nextUntilCodePoint();
        }

        public int current() {
            return _current;
        }

        public int wordStart() {
            if (_inEmailOrUrl) {
                return _last;
            }

            var result = _last;
            while (result < _current) {
                int ix = result;
                uint c = nextCode(_text, ref ix, _current);
                if (!LayoutUtils.isLineEndSpace((char) c)) {
                    break;
                }

                result = ix;
            }

            return result;
        }

        public int wordEnd() {
            if (_inEmailOrUrl) {
                return _last;
            }

            int result = _current;
            while (result > _last) {
                int ix = result;
                uint ch = preCode(_text, ref ix, _last);
                if (!LayoutUtils.isLineEndSpace((char) ch)) {
                    break;
                }

                result = ix;
            }

            return result;
        }

        public int breakBadness() {
            return (_inEmailOrUrl && _current < _scanOffset) ? 1 : 0;
        }

        public void finish() {
            _text = default;
        }

        int _findNextBreakInEmailOrUrl() {
            return 0;
        }

        int _findNextBoundaryNormal() {
            if (_current == _text.size) {
                return -1;
            }

            char c = _text.charAt(_current);
            bool preWhiteSpace = char.IsWhiteSpace(c);
            bool preBoundaryChar = isBoundaryChar(c);
            _current++;
            if (preBoundaryChar) {
                return _current;
            }

            _findBoundaryCharOrTypeChange(preWhiteSpace);

            return _current;
        }

        void _findBoundaryCharOrTypeChange(bool preWhiteSpace) {
            for (; _current < _text.size; ++_current) {
                // this.nextUntilCodePoint();
                if (_current >= _text.size) {
                    break;
                }

                char c = _text.charAt(_current);
                if (isBoundaryChar(c)) {
                    break;
                }

                bool currentType = char.IsWhiteSpace(c);
                if (currentType != preWhiteSpace) {
                    break;
                }

                preWhiteSpace = currentType;
            }
        }

        void _detectEmailOrUrl() {
        }

        static uint nextCode(TextBuff text, ref int index, int end) {
            uint ch = text.charAt(index);
            index++;
            if (isLeadSurrogate(ch)) {
                if (index < end && isTrailSurrogate(text.charAt(index))) {
                    char ch2 = text.charAt(index);
                    index++;
                    ch = getSupplementary(ch, ch2);
                }
            }

            return ch;
        }

        static uint preCode(TextBuff text, ref int index, int start) {
            --index;
            uint ch = text.charAt(index);
            if (isTrailSurrogate(ch)) {
                if (index > start && isLeadSurrogate(text.charAt(index - 1))) {
                    ch = getSupplementary(text.charAt(index - 1), ch);
                    --index;
                }
            }

            return ch;
        }

        public static bool isLeadSurrogate(uint c) {
            return (c & 0xfffffc00) == 0xd800;
        }


        public static bool isTrailSurrogate(uint c) {
            return (c & 0xfffffc00) == 0xdc00;
        }

        public static uint getSupplementary(uint lead, uint trail) {
            return (char) ((lead << 10) + (trail - U16_SURROGATE_OFFSET));
        }

        public static bool isBoundaryChar(char code) {
            return (code >= 0x4E00 && code <= 0x9FFF) || (code >= 0x3040 && code <= 0x30FF) || char.IsPunctuation(code);
        }

        void nextUntilCodePoint() {
            while (_current < _text.size
                   && (char.IsLowSurrogate(_text.charAt(_current))
                       || char.IsHighSurrogate(_text.charAt(_current)))) {
                _current++;
            }
        }
    }
}