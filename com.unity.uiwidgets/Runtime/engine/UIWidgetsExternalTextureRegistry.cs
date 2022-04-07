using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

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
                    D.assert(externalTexturePtr == IntPtr.Zero, () => $"The internal texture id {internalTextureId} is shared by different external textures {externalTexturePtr} and {curInternalTextureId}");
                    externalTexturePtr = texturePair.Key;
                }
            }
            if (externalTexturePtr != IntPtr.Zero) {
                ReleaseExternalTexture(externalTexturePtr);
            }
        }

        public int RegisterExternalTexture(IntPtr externalTexturePtr) {
            D.assert(externalTexturePtr != IntPtr.Zero, () => "Cannot register null external texture");
            if (!externalTextures.ContainsKey(externalTexturePtr)) {
                var internalTextureId = registerTexture(externalTexturePtr);
                D.assert(internalTextureId != -1, () => "Fail to register external texture. Possible cause is that you platform and graphics backend doesn't support external texture (e.g., Metal on Mac/iOS)");
                externalTextures[externalTexturePtr] = internalTextureId;
            }
            
            return externalTextures[externalTexturePtr];
        }
    }
}