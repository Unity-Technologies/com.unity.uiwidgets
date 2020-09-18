using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class ClipRRectLayer : ContainerLayer {
        RRect _clipRRect;

        public RRect clipRRect {
            set { _clipRRect = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var previousCullRect = context.cullRect;

            var clipPathBounds = _clipRRect.outerRect;
            context.cullRect = context.cullRect.intersect(clipPathBounds);

            paintBounds = Rect.zero;
            if (!context.cullRect.isEmpty) {
                var childPaintBounds = Rect.zero;
                prerollChildren(context, matrix, ref childPaintBounds);
                childPaintBounds = childPaintBounds.intersect(clipPathBounds);

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
            canvas.clipRRect(_clipRRect);

            try {
                paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}