using System.Runtime.InteropServices;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace engine2 {
    public class AndroidGLInit {
        [DllImport(NativeBindings.dllName)]
        static extern System.IntPtr GetUnityContextEventFunc();

        public static void Init() {
            GL.IssuePluginEvent(GetUnityContextEventFunc(), 1);
        }
    }
}