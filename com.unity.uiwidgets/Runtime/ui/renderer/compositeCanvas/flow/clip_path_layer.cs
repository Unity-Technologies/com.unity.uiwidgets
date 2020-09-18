using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class ClipPathLayer : ContainerLayer {
        Path _clipPath;

        public Path clipPath {
            set { _clipPath = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var previousCullRect = context.cullRect;

            var clipPathBounds = _clipPath.getBounds();
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
            canvas.clipPath(_clipPath);

            try {
                paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}