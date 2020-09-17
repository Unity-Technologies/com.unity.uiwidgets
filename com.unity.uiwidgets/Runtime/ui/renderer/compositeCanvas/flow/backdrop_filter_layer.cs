using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class BackdropFilterLayer : ContainerLayer {
        ImageFilter _filter;

        public ImageFilter filter {
            set { _filter = value; }
        }

        public override void paint(PaintContext context) {
            D.assert(needsPainting);

            var canvas = context.canvas;
            canvas.saveLayer(paintBounds, new Paint {backdrop = _filter});

            try {
                paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}