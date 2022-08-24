using System;
using NativeBindings = Unity.UIWidgets.ui.NativeBindings;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
    public static class OpenGLCoreUtil {
        [DllImport(NativeBindings.dllName)]
        static extern System.IntPtr GetUnityContextEventFunc();

        public static void Init() {
            GL.IssuePluginEvent(GetUnityContextEventFunc(), 1);
        }

        /**
         * For some unknown reason, the nativePtr of the created renderTexture on OpenGLCore backend always turns out to be 0x0
         * at the first time after calling RenderTexture.Create. It will leads to crash when we tried to pass the null nativePtr
         * to the UIWidgets library.
         *
         * To work around this issue, we call RenderTexture.Create here to swallow this bad case. It turns out that after this
         * first call, we can safely use RenderTexture.Create to create renderTextures with valid nativePtr.
         */
        public static void RenderTextureCreateFailureWorkaround() {
            RenderTexture _tempTexture = null;
            try {
                var desc = new RenderTextureDescriptor(
                    2, 2, RenderTextureFormat.ARGB32, 0) {
                    useMipMap = false,
                    autoGenerateMips = false,
                };

                _tempTexture = new RenderTexture(desc) {hideFlags = HideFlags.HideAndDontSave};
                _tempTexture.Create();
            }
            catch (Exception e) {

            }
            finally {
                if (_tempTexture != null) {
                    _tempTexture.Release();
                }
            }
        }
    }
#endif
}