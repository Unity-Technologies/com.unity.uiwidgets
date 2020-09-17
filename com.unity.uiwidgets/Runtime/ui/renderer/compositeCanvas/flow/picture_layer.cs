using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class PictureLayer : Layer {
        Offset _offset = Offset.zero;

        public Offset offset {
            set { _offset = value ?? Offset.zero; }
        }

        Picture _picture;

        public Picture picture {
            set { _picture = value; }
        }

        bool _isComplex = false;

        public bool isComplex {
            set { _isComplex = value; }
        }

        bool _willChange = false;

        public bool willChange {
            set { _willChange = value; }
        }

        RasterCacheResult _rasterCacheResult;

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            if (context.rasterCache != null) {
                Matrix3 ctm = new Matrix3(matrix);
                ctm.preTranslate(_offset.dx,
                    _offset.dy); // TOOD: pre or post? https://github.com/flutter/engine/pull/7945
                ctm[2] = ctm[2].alignToPixel(context.devicePixelRatio);
                ctm[5] = ctm[5].alignToPixel(context.devicePixelRatio);

                _rasterCacheResult = context.rasterCache.getPrerolledImage(
                    _picture, ctm, context.devicePixelRatio, context.antiAliasing, _isComplex,
                    _willChange);
            }
            else {
                _rasterCacheResult = null;
            }

            var bounds = _picture.paintBounds.shift(_offset);
            paintBounds = bounds;
        }

        public override void paint(PaintContext context) {
            D.assert(_picture != null);
            D.assert(needsPainting);

            var canvas = context.canvas;

            canvas.save();
            canvas.translate(_offset.dx, _offset.dy);

            canvas.alignToPixel();

            try {
                if (_rasterCacheResult != null) {
                    _rasterCacheResult.draw(canvas);
                }
                else {
                    canvas.drawPicture(_picture);
                }
            }
            finally {
                canvas.restore();
            }
        }
    }
}