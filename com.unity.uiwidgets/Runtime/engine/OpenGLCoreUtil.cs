using NativeBindings = Unity.UIWidgets.ui.NativeBindings;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    public static class OpenGLCoreUtil {
        [DllImport(NativeBindings.dllName)]
        static extern System.IntPtr GetUnityContextEventFunc();

        public static void Init() {
            GL.IssuePluginEvent(GetUnityContextEventFunc(), 1);
        }
    }
}