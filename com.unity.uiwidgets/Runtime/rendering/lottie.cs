using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public class RenderLottie : RenderBox {
        public RenderLottie(
            Skottie skottie,
            float? width = null,
            float? height = null,
            float scale = 1.0f,
            float frame = 0
        ) {
            _width = width;
            _height = height;
            _scale = scale;
            _skottie = skottie;
            _frame = frame;
            _duration = skottie.duration();
        }

        Skottie _skottie;
        float _frame = 0;
        float _duration = 0;

        public float frame {
            get { return _frame; }
            set {
                while (value > _duration) {
                    value -= _duration;
                }
                if (value == _frame) {
                    return;
                }

                _frame = value;
                markNeedsLayout();
            }
        }

        float? _width;

        public float? width {
            get { return _width; }
            set {
                if (value == _width) {
                    return;
                }

                _width = value;
                markNeedsLayout();
            }
        }

        float? _height;

        public float? height {
            get { return _height; }
            set {
                if (value == _height) {
                    return;
                }

                _height = value;
                markNeedsLayout();
            }
        }


        float _scale;

        public float scale {
            get { return _scale; }
            set {
                if (value == _scale) {
                    return;
                }

                _scale = value;
                markNeedsLayout();
            }
        }

        Size _sizeForConstraints(BoxConstraints constraints) {
            constraints = BoxConstraints.tightFor(
                _width,
                _height
            ).enforce(constraints);

            return constraints.smallest;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            D.assert(height >= 0.0);
            if (_width == null && _height == null) {
                return 0.0f;
            }

            return _sizeForConstraints(BoxConstraints.tightForFinite(height: height)).width;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            D.assert(height >= 0.0);
            return _sizeForConstraints(BoxConstraints.tightForFinite(height: height)).width;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            D.assert(width >= 0.0);
            if (_width == null && _height == null) {
                return 0.0f;
            }

            return _sizeForConstraints(BoxConstraints.tightForFinite(width: width)).height;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            D.assert(width >= 0.0);
            return _sizeForConstraints(BoxConstraints.tightForFinite(width: width)).height;
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected override void performLayout() {
            size = _sizeForConstraints(constraints);
        }

        public override void paint(PaintingContext context, Offset offset) {
            _skottie.paint(context.canvas, offset, _width ?? 0, _height ?? 0, _frame);
        }
    }
}