namespace Unity.UIWidgets.ui {
    struct PaintRecord {
        public PaintRecord(TextStyle style, float dx, float dy, TextBlob text, FontMetrics metrics, float runWidth) {
            _style = style;
            _text = text;
            _runWidth = runWidth;
            _metrics = metrics;
            _dx = dx;
            _dy = dy;
        }

        public TextBlob text {
            get { return _text; }
        }

        public TextStyle style {
            get { return _style; }
        }

        public float runWidth {
            get { return _runWidth; }
        }

        public FontMetrics metrics {
            get { return _metrics; }
        }

        public void shift(float x, float y) {
            _dx += x;
            _dy += y;
        }

        public Offset shiftedOffset(Offset other) {
            return new Offset(_dx + other.dx, _dy + other.dy);
        }

        TextStyle _style;
        TextBlob _text;
        float _runWidth;
        float _dx;
        float _dy;
        FontMetrics _metrics;
    }
}