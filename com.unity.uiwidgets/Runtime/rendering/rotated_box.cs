using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.Runtime.rendering {
    class RotatedBoxUtils {
        public const float _kQuarterTurnsInRadians = Mathf.PI / 2.0f;
    }

    public class RenderRotatedBox : RenderObjectWithChildMixinRenderBox<RenderBox> {
        public RenderRotatedBox(
            int quarterTurns,
            RenderBox child = null
        ) {
            this.child = child;
            _quarterTurns = quarterTurns;
        }

        public int quarterTurns {
            get { return _quarterTurns; }
            set {
                if (_quarterTurns == value) {
                    return;
                }

                _quarterTurns = value;
                markNeedsLayout();
            }
        }

        int _quarterTurns;

        bool _isVertical {
            get { return quarterTurns % 2 == 1; }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            if (child == null) {
                return 0.0f;
            }

            return _isVertical
                ? child.getMinIntrinsicHeight(height)
                : child.getMinIntrinsicWidth(height);
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            if (child == null) {
                return 0.0f;
            }

            return _isVertical
                ? child.getMaxIntrinsicHeight(height)
                : child.getMaxIntrinsicWidth(height);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (child == null) {
                return 0.0f;
            }

            return _isVertical ? child.getMinIntrinsicWidth(width) : child.getMinIntrinsicHeight(width);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (child == null) {
                return 0.0f;
            }

            return _isVertical ? child.getMaxIntrinsicWidth(width) : child.getMaxIntrinsicHeight(width);
        }

        Matrix3 _paintTransform;

        protected override void performLayout() {
            _paintTransform = null;
            if (child != null) {
                child.layout(_isVertical ? constraints.flipped : constraints, parentUsesSize: true);
                size = _isVertical
                    ? new Size(child.size.height, child.size.width)
                    : child.size;
                _paintTransform = Matrix3.I();
                _paintTransform.preTranslate(size.width / 2.0f, size.height / 2.0f);
                _paintTransform.preRotate(RotatedBoxUtils._kQuarterTurnsInRadians * (quarterTurns % 4));
                _paintTransform.preTranslate(-child.size.width / 2.0f, -child.size.height / 2.0f);
            }
            else {
                performResize();
            }
        }

        protected override bool hitTestChildren(
            HitTestResult result,
            Offset position = null
        ) {
            D.assert(_paintTransform != null || debugNeedsLayout || child == null);
            if (child == null || _paintTransform == null) {
                return false;
            }


            Matrix3 inverse = Matrix3.I();
            _paintTransform.invert(inverse);
            return child.hitTest(result, position: inverse.mapPoint(position));
        }

        void _paintChild(PaintingContext context, Offset offset) {
            context.paintChild(child, offset);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                context.pushTransform(needsCompositing, offset, _paintTransform, _paintChild);
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            if (_paintTransform != null) {
                transform.preConcat(_paintTransform);
            }

            base.applyPaintTransform(child, transform);
        }
    }
}