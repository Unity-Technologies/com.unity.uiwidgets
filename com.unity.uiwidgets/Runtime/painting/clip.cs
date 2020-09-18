using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public abstract class ClipContext {
        public abstract Canvas canvas { get; }

        void _clipAndPaint(Action<bool> canvasClipCall, Clip clipBehavior, Rect bounds, Action painter) {
            D.assert(canvasClipCall != null);

            canvas.save();

            switch (clipBehavior) {
                case Clip.none:
                    break;
                case Clip.hardEdge:
                    canvasClipCall(false);
                    break;
                case Clip.antiAlias:
                    canvasClipCall(true);
                    break;
                case Clip.antiAliasWithSaveLayer:
                    canvasClipCall(true);
                    canvas.saveLayer(bounds, new Paint());
                    break;
            }

            painter();

            if (clipBehavior == Clip.antiAliasWithSaveLayer) {
                canvas.restore();
            }

            canvas.restore();
        }

        public void clipPathAndPaint(Path path, Clip clipBehavior, Rect bounds, Action painter) {
            _clipAndPaint((bool doAntiAias) => canvas.clipPath(path),
                clipBehavior, bounds, painter);
        }

        public void clipRRectAndPaint(RRect rrect, Clip clipBehavior, Rect bounds, Action painter) {
            _clipAndPaint(doAntiAias => canvas.clipRRect(rrect),
                clipBehavior, bounds, painter);
        }

        public void clipRectAndPaint(Rect rect, Clip clipBehavior, Rect bounds, Action painter) {
            _clipAndPaint(doAntiAias => canvas.clipRect(rect),
                clipBehavior, bounds, painter);
        }
    }
}