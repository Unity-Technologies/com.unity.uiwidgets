using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    public static class UIWidgetsExternalTextureHelper {
        public static RenderTexture createCompatibleExternalTexture(RenderTextureDescriptor descriptor) {
#if UNITY_2020_3_OR_NEWER && !UNITY_2020_3_1 && !UNITY_2020_3_2 && !UNITY_2020_3_3 && !UNITY_2020_3_4&& !UNITY_2020_3_5 && !UNITY_2020_3_6 && !UNITY_2020_3_7 && !UNITY_2020_3_8 && !UNITY_2020_3_9 && !UNITY_2020_3_10&& !UNITY_2020_3_11&& !UNITY_2020_3_12 && !UNITY_2020_3_13 && !UNITY_2020_3_14 && !UNITY_2020_3_15 && !UNITY_2020_3_16 && !UNITY_2020_3_17 && !UNITY_2020_3_18 && !UNITY_2020_3_19 && !UNITY_2020_3_20 && !UNITY_2020_3_21 && !UNITY_2020_3_22 && !UNITY_2020_3_23 && !UNITY_2020_3_24 && !UNITY_2020_3_25 && !UNITY_2020_3_26 && !UNITY_2020_3_27 && !UNITY_2020_3_28 && !UNITY_2020_3_29 && !UNITY_2020_3_30 && !UNITY_2020_3_31 && !UNITY_2020_3_32 && !UNITY_2020_3_33 && !UNITY_2020_3_34 && !UNITY_2020_3_35 && !UNITY_2020_3_36
            //this API is only available for 2020.3.37 and later Unity
            return UIWidgetsInternal.CreateBindableRenderTexture(descriptor);
#else
            return new RenderTexture(descriptor);
#endif
        }
    }
    
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
                D.assert(internalTextureId != -1, () => "Fail to register external texture. Possible cause is that your platform or graphics backend doesn't support external texture (e.g., Metal on Mac/iOS)");
                externalTextures[externalTexturePtr] = internalTextureId;
            }
            
            return externalTextures[externalTexturePtr];
        }
    }
}