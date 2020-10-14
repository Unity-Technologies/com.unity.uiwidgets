using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class TextureBox : RenderBox {

        public TextureBox(int textureId) {
            D.assert(textureId != null);
            _textureId = textureId;
        }

        public int textureId {
            get { return _textureId; }
            set {
                D.assert(value != null);
                if (value != _textureId) {
                    _textureId = value;
                    markNeedsPaint();
                }                
            }
        }
        
        int _textureId;

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
            if (_textureId == null) {
                return;
            }
            
            context.addLayer(new TextureLayer(
                rect: Rect.fromLTWH(offset.dx, offset.dy, size.width, size.height),
                textureId: _textureId
            ));
        }
    }
}
