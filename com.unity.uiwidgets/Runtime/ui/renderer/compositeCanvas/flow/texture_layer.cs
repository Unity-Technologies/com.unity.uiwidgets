using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.flow {
    public class TextureLayer : Layer {
        Offset _offset = Offset.zero;

        public Offset offset {
            set { _offset = value ?? Offset.zero; }
        }

        Size _size;

        public Size size {
            set { _size = value; }
        }

        Texture _texture;

        public Texture texture {
            set { _texture = value; }
        }

        bool _freeze = false;

        public bool freeze {
            set { _freeze = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            paintBounds = Rect.fromLTWH(
                _offset.dx, _offset.dy, _size.width, _size.height);
        }

        public override void paint(PaintContext context) {
            D.assert(needsPainting);

            if (_texture == null) {
                return;
            }

            var image = new Image(_texture, noDispose: true);

            var canvas = context.canvas;
            canvas.drawImageRect(image, paintBounds, new Paint());
        }
    }
}