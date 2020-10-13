using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class Texture : LeafRenderObjectWidget {
        public static void textureFrameAvailable(Window instance = null) {
            if (instance == null) {
                WindowAdapter.windowAdapters.ForEach(w => w.scheduleFrame(false));
            } else {
                // TODO: check whether following is needed 
                // instance.scheduleFrame(false);
            }
        }
        
        public Texture(Key key = null, UnityEngine.Texture texture = null) : base(key: key) {
            D.assert(texture != null);
            this.texture = texture;
        }

        public readonly UnityEngine.Texture texture;

        public override RenderObject createRenderObject(BuildContext context) {
            return new TextureBox(texture: texture);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((TextureBox) renderObject).texture = texture;
        }
    }
}
