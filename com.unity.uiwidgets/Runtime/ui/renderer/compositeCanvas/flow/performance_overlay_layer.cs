using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class PerformanceOverlayLayer : Layer {
        public PerformanceOverlayLayer(int options) {
            _options = options;
        }

        readonly int _options;

        public override void paint(PaintContext context) {
            D.assert(needsPainting);
            const int padding = 8;
            const int fpsHeight = 20;

            Canvas canvas = context.canvas;
            canvas.save();

            float x = paintBounds.left + padding;
            float y = paintBounds.top + padding;
            float width = paintBounds.width - padding * 2;
            float height = paintBounds.height;

            _drawFPS(canvas, x, y);

            if ((_options & (int) PerformanceOverlayOption.drawFrameCost) == 1) {
                context.frameTime.visualize(canvas,
                    Rect.fromLTWH(x, y + fpsHeight, width, height - padding - fpsHeight));
            }

            canvas.restore();
        }


        void _drawFPS(Canvas canvas, float x, float y) {
            var pb = new ParagraphBuilder(new ParagraphStyle { });
            pb.addText("FPS = " + Window.instance.getFPS());
            var paragraph = pb.build();
            paragraph.layout(new ParagraphConstraints(width: 300));

            canvas.drawParagraph(paragraph, new Offset(x, y));
            Paragraph.release(ref paragraph);
        }
    }
}