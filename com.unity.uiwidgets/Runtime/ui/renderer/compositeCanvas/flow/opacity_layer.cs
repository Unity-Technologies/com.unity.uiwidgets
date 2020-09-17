using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class OpacityLayer : ContainerLayer {
        Offset _offset;

        public Offset offset {
            set { _offset = value; }
        }

        int _alpha;

        public int alpha {
            set { _alpha = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var childMatrix = new Matrix3(matrix);
            childMatrix.preTranslate(_offset.dx,
                _offset.dy); // TOOD: pre or post? https://github.com/flutter/engine/pull/7945

            base.preroll(context, childMatrix);

            var bounds = paintBounds.shift(_offset);
            paintBounds = bounds;
        }

        public override void paint(PaintContext context) {
            D.assert(needsPainting);

            var canvas = context.canvas;

            canvas.save();
            canvas.translate(_offset.dx, _offset.dy);

            canvas.alignToPixel();

            var saveLayerBounds = paintBounds.shift(-_offset).roundOut();
            var paint = new Paint {color = Color.fromARGB(_alpha, 255, 255, 255)};
            canvas.saveLayer(saveLayerBounds, paint);

            try {
                paintChildren(context);
            }
            finally {
                canvas.restore();
                canvas.restore();
            }
        }
    }
}