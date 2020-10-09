using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
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

        Matrix4 _paintTransform;

        protected override void performLayout() {
            this._paintTransform = null;
            if (this.child != null) {
                this.child.layout(this._isVertical ? this.constraints.flipped : this.constraints, parentUsesSize: true);
                this.size = this._isVertical
                    ? new Size(this.child.size.height, this.child.size.width)
                    : this.child.size;
                this._paintTransform = new Matrix4().identity();
                this._paintTransform.translate(this.size.width / 2.0f, this.size.height / 2.0f);
                this._paintTransform.rotateZ(RotatedBoxUtils._kQuarterTurnsInRadians * (this.quarterTurns % 4));
                this._paintTransform.translate(-this.child.size.width / 2.0f, -this.child.size.height / 2.0f);
            }
            else {
                performResize();
            }
        }

        protected override bool hitTestChildren(
            BoxHitTestResult result,
            Offset position = null
        ) {
            D.assert(_paintTransform != null || debugNeedsLayout || child == null);
            if (child == null || _paintTransform == null) {
                return false;
            }

            return result.addWithPaintTransform(
                transform: this._paintTransform,
                position: position,
                hitTest: (BoxHitTestResult resultIn, Offset positionIn) => {
                    return this.child.hitTest(resultIn, position: positionIn);
                }
            );
        }

        void _paintChild(PaintingContext context, Offset offset) {
            context.paintChild(child, offset);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                context.pushTransform(needsCompositing, offset, _paintTransform, _paintChild);
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            if (this._paintTransform != null) {
                transform.multiply(this._paintTransform);
            }

            base.applyPaintTransform(child, transform);
        }
    }
}