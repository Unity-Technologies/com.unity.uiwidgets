using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    public partial class UIWidgetsPanelWrapper {
        readonly Dictionary<IntPtr, int> externalTextures = new Dictionary<IntPtr, int>();

        void ReleaseExternalTextures() {
            foreach(var texturePair in externalTextures) {
                var internalTextureId = texturePair.Value;
                unregisterTexture(internalTextureId);
            }
            
            externalTextures.Clear();
        }

        void ReleaseExternalTexture(IntPtr externalTexturePtr) {
            if (externalTextures.ContainsKey(externalTexturePtr)) {
                var internalTextureId = externalTextures[externalTexturePtr];
                unregisterTexture(internalTextureId);

                externalTextures.Remove(externalTexturePtr);
            }
        }
        
        public void ReleaseExternalTexture(int internalTextureId) {
            var externalTexturePtr = IntPtr.Zero;
            foreach(var texturePair in externalTextures) {
                var curInternalTextureId = texturePair.Value;
                if (curInternalTextureId == internalTextureId) {
                    if (externalTexturePtr != IntPtr.Zero) {
                        Debug.LogError($"The internal texture id {internalTextureId} is shared by different external textures {externalTexturePtr} amd {curInternalTextureId}");
                        return;
                    }
                    externalTexturePtr = texturePair.Key;
                }
            }

            ReleaseExternalTexture(externalTexturePtr);
        }

        public int RegisterExternalTexture(IntPtr externalTexturePtr) {
            if (!externalTextures.ContainsKey(externalTexturePtr)) {
                var internalTextureId = registerTexture(externalTexturePtr);
                externalTextures[externalTexturePtr] = internalTextureId;
            }
            
            return externalTextures[externalTexturePtr];
        }
    }
}