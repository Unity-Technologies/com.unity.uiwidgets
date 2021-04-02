#if UNITY_ANDROID
using UnityEngine;
using AOT;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using NativeBindings = Unity.UIWidgets.ui.NativeBindings;

namespace Unity.UIWidgets.engine2 {
    public static class AndroidPlatformUtil {
        [DllImport(NativeBindings.dllName)]
        internal static extern void InitUnpackFile(UnpackFileCallback unpack);

        internal delegate string UnpackFileCallback(string file);

        [MonoPInvokeCallback(typeof(UnpackFileCallback))]
        internal static string unpackFile(string file) {
            var dir = Application.temporaryCachePath + "/";
            if (!File.Exists(dir + file)) {
                var path = "jar:file://" + Application.dataPath + "!/assets/" + file;
                using (var unpackerWWW = UnityWebRequest.Get(path)) {
                    unpackerWWW.SendWebRequest();
                    while (!unpackerWWW.isDone) {
                    } // This will block in the webplayer.

                    if (unpackerWWW.isNetworkError || unpackerWWW.isHttpError) {
                        Debug.Log($"Failed to get file \"{path}\": {unpackerWWW.error}");
                        return "";
                    }

                    var data = unpackerWWW.downloadHandler.data;
                    FileInfo fileInfo = new System.IO.FileInfo(dir + file);
                    fileInfo.Directory.Create();
                    File.WriteAllBytes(fileInfo.FullName, data);
                }
            }

            return dir + file;
        }

        [DllImport(NativeBindings.dllName)]
        static extern System.IntPtr GetUnityContextEventFunc();

        public static void Init() {
            InitUnpackFile(unpackFile);
            GL.IssuePluginEvent(GetUnityContextEventFunc(), 1);
        }

        public static void SetStatusBarValue(bool value) {
#if !UNITY_EDITOR
            using (var util = new AndroidJavaClass("com.unity.uiwidgets.plugin.Utils")) {
                util.CallStatic("SetStatusBarState", value);
            }
#endif
        }
    }
}
#endif