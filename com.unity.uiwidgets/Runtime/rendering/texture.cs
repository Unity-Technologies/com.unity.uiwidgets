using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class TextureBox : RenderBox {

        public TextureBox(Texture texture = null) {
            D.assert(texture != null);
            _texture = texture;
        }

        public Texture texture {
            get { return _texture; }
            set {
                D.assert(value != null);
                if (value != _texture) {
                    _texture = value;
                    markNeedsPaint();
                }                
            }
        }
        
        Texture _texture;

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override bool  isRepaintBoundary {
            get { return true; }
        }

        protected override void performResize() {
            size = constraints.biggest;
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_texture == null) {
                return;
            }
            
            context.addLayer(new TextureLayer(
                rect: Rect.fromLTWH(offset.dx, offset.dy, size.width, size.height),
                texture: _texture
            ));
        }
    }
}
