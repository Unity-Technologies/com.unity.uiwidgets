namespace Unity.UIWidgets.ui {
    struct WordSeparate {
        internal enum CharacterType {
            LetterLike,
            Symbol,
            WhiteSpace
        }

        string _text;

        public WordSeparate(string text) {
            _text = text;
        }

        public Range<int> findWordRange(int index) {
            if (index >= _text.Length) {
                return new Range<int>(0, 0);
            }

            var t = classifyChar(_text, index);
            int start = index;
            for (int i = index; i >= 0; --i) {
                if (!char.IsLowSurrogate(_text[start])) {
                    if (classifyChar(_text, i) != t) {
                        break;
                    }

                    start = i;
                }
            }

            int end = index;
            for (int i = index; i < _text.Length; ++i) {
                if (!char.IsLowSurrogate(_text[i])) {
                    if (classifyChar(_text, i) != t) {
                        break;
                    }

                    end = i;
                }
            }

            return new Range<int>(start, end + 1);
        }


        internal static CharacterType classifyChar(string text, int index) {
            return classifyChar(text[index]);
        }
        
        internal static CharacterType classifyChar(char ch) {
            if (char.IsWhiteSpace(ch)) {
                return CharacterType.WhiteSpace;
            }

            if (char.IsLetterOrDigit(ch) || ch == '\'') {
                return CharacterType.LetterLike;
            }

            return CharacterType.Symbol;
        }
    }
}