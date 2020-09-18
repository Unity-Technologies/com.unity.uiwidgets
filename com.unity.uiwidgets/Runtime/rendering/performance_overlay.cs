using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public enum PerformanceOverlayOption {
        drawFPS, //default
        drawFrameCost
    }


    public class RenderPerformanceOverlay : RenderBox {
        public RenderPerformanceOverlay(
            int optionsMask = 0
        ) {
            _optionMask = optionsMask;
        }

        public int optionsMask {
            get { return _optionMask; }
            set {
                if (value == _optionMask) {
                    return;
                }

                _optionMask = value;
                markNeedsPaint();
            }
        }

        int _optionMask;

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return 0.0f;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return 0.0f;
        }

        float _intrinsicHeight {
            get {
                const float kDefaultGraphHeight = 80.0f;
                float result = 20f;

                if ((optionsMask | (1 << (int) PerformanceOverlayOption.drawFrameCost)) > 0) {
                    result += kDefaultGraphHeight;
                }

                return result;
            }
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return _intrinsicHeight;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return _intrinsicHeight;
        }

        protected override void performResize() {
            size = constraints.constrain(new Size(float.PositiveInfinity, _intrinsicHeight));
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(needsCompositing);
            context.addLayer(new PerformanceOverlayLayer(
                overlayRect: Rect.fromLTWH(offset.dx, offset.dy, size.width, size.height),
                optionsMask: optionsMask
            ));
        }
    }
}