namespace Unity.UIWidgets.ui {
    public struct TextBlob {
        internal TextBlob(string text, int textOffset, int textSize, float[] positionXs,
            UnityEngine.Rect bounds, TextStyle style) {
            instanceId = ++_nextInstanceId;
            _positionXs = positionXs;
            this.text = text;
            this.textOffset = textOffset;
            this.textSize = textSize;
            this.style = style;
            _bounds = bounds;
            _boundsInText = null;
        }

        public Rect boundsInText {
            get {
                if (_boundsInText == null) {
                    var pos = getPositionX(0);
                    _boundsInText = Rect.fromLTWH(_bounds.xMin + pos, _bounds.yMin,
                        _bounds.width, _bounds.height);
                }

                return _boundsInText;
            }
        }

        public Rect shiftedBoundsInText(float dx, float dy) {
            var pos = getPositionX(0);
            return Rect.fromLTWH(_bounds.xMin + pos + dx, _bounds.yMin + dy,
                _bounds.width, _bounds.height);
        }

        public float getPositionX(int i) {
            return _positionXs[textOffset + i];
        }

        static long _nextInstanceId;
        internal readonly long instanceId;
        internal readonly string text;
        internal readonly int textOffset;
        internal readonly int textSize;
        internal readonly TextStyle style;
        readonly UnityEngine.Rect _bounds; // bounds with positions[start] as origin       
        readonly float[] _positionXs;

        Rect _boundsInText;
    }

    public struct TextBlobBuilder {
        TextStyle _style;
        float[] _positionXs;
        string _text;
        int _textOffset;
        int _size;
        UnityEngine.Rect _bounds;

        public void allocRunPos(painting.TextStyle style, string text, int offset, int size,
            float textScaleFactor = 1.0f) {
            allocRunPos(TextStyle.applyStyle(null, style, textScaleFactor), text, offset, size);
        }

        internal void allocRunPos(TextStyle style, string text, int offset, int size) {
            _style = style;
            _text = text;
            _textOffset = offset;
            _size = size;
            // Allocate a single buffer for all text blobs that share this text, to save memory and GC.
            // It is assumed that all of `text` is being used. This may cause great waste if a long text is passed
            // but only a small part of it is to be rendered, which is not the case for now.
            allocPos(text.Length);
        }

        internal void allocPos(int size) {
            if (_positionXs == null || _positionXs.Length < size) {
                _positionXs = new float[size];
            }
        }

        public void setPositionX(int i, float positionX) {
            _positionXs[_textOffset + i] = positionX;
        }

        public void setPositionXs(float[] positionXs) {
            _positionXs = positionXs;
        }

        public void setBounds(UnityEngine.Rect bounds) {
            _bounds = bounds;
        }

        public TextBlob make() {
            var result = new TextBlob(_text, _textOffset,
                _size, _positionXs, _bounds, _style);
            return result;
        }
    }
}