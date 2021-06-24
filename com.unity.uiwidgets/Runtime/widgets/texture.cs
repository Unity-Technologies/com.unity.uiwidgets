using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class Texture : LeafRenderObjectWidget {
        public Texture( 
            Key key = null, 
            UnityEngine.Texture texture = null,
            int? textureId = null) : base(key: key) {
            D.assert(textureId != null);
            this.textureId = textureId;
            this.texture = texture;
            if (texture != null && texture.GetNativeTexturePtr() != IntPtr.Zero) {
                this.textureId = UIWidgetsPanelWrapper.current.registerTexture(texture);
            }
        }

        public readonly UnityEngine.Texture texture;
        public readonly int? textureId;

        public override RenderObject createRenderObject(BuildContext context) {
            return new TextureBox(textureId: textureId, texture);;
        }

        public override bool Equals(object obj) {
            return false;
        }
        
        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((TextureBox) renderObject).textureId = textureId;
        }
    }
}