using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class ClipRectLayer : ContainerLayer {
        Rect _clipRect;

        public Rect clipRect {
            set { _clipRect = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var previousCullRect = context.cullRect;

            context.cullRect = context.cullRect.intersect(_clipRect);

            paintBounds = Rect.zero;
            if (!context.cullRect.isEmpty) {
                var childPaintBounds = Rect.zero;
                prerollChildren(context, matrix, ref childPaintBounds);
                childPaintBounds = childPaintBounds.intersect(_clipRect);

                if (!childPaintBounds.isEmpty) {
                    paintBounds = childPaintBounds;
                }
            }

            context.cullRect = previousCullRect;
        }

        public override void paint(PaintContext context) {
            D.assert(needsPainting);

            var canvas = context.canvas;

            canvas.save();
            canvas.clipRect(paintBounds);

            try {
                paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}