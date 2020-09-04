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
            this._quarterTurns = quarterTurns;
        }

        public int quarterTurns {
            get { return this._quarterTurns; }
            set {
                if (this._quarterTurns == value) {
                    return;
                }

                this._quarterTurns = value;
                this.markNeedsLayout();
            }
        }

        int _quarterTurns;

        bool _isVertical {
            get { return this.quarterTurns % 2 == 1; }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            if (this.child == null) {
                return 0.0f;
            }

            return this._isVertical
                ? this.child.getMinIntrinsicHeight(height)
                : this.child.getMinIntrinsicWidth(height);
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            if (this.child == null) {
                return 0.0f;
            }

            return this._isVertical
                ? this.child.getMaxIntrinsicHeight(height)
                : this.child.getMaxIntrinsicWidth(height);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (this.child == null) {
                return 0.0f;
            }

            return this._isVertical ? this.child.getMinIntrinsicWidth(width) : this.child.getMinIntrinsicHeight(width);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (this.child == null) {
                return 0.0f;
            }

            return this._isVertical ? this.child.getMaxIntrinsicWidth(width) : this.child.getMaxIntrinsicHeight(width);
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
                this.performResize();
            }
        }

        protected override bool hitTestChildren(
            BoxHitTestResult result,
            Offset position = null
        ) {
            D.assert(this._paintTransform != null || this.debugNeedsLayout || this.child == null);
            if (this.child == null || this._paintTransform == null) {
                return false;
            }
            
            var inverse = new Matrix4().inverted(this._paintTransform);
            return this.child.hitTest(result, position:MatrixUtils.transformPoint(inverse, position));
        }

        void _paintChild(PaintingContext context, Offset offset) {
            context.paintChild(this.child, offset);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                context.pushTransform(this.needsCompositing, offset, this._paintTransform, this._paintChild);
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