using System;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class Texture : LeafRenderObjectWidget {
        public Texture( 
            Key key = null, 
            UnityEngine.Texture texture = null,
            int? textureId = null) : base(key: key) {
            D.assert(textureId != null);
            this.textureId = textureId;
            this.texture = texture;
            if (texture != null && texture.GetNativeTexturePtr() == IntPtr.Zero) {
                this.textureId = null;
            }
            else {
                this.textureId = UIWidgetsPanelWrapper.current.registerTexture(texture);
            }
        }

        public readonly UnityEngine.Texture texture;
        public readonly int? textureId;

        public override RenderObject createRenderObject(BuildContext context) {
            return new TextureBox(textureId: textureId);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((TextureBox) renderObject).textureId = textureId;
        }
    }
}