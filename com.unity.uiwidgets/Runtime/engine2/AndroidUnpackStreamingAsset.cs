using UnityEngine;
using AOT;
using System;
using System.IO;
using System.Runtime.InteropServices;
using NativeBindings = Unity.UIWidgets.ui.NativeBindings;

public static class AndroidUnpackStreamingAssets {

    [DllImport(NativeBindings.dllName)]
    internal static extern void InitUnpackFile(UnpackFileCallback unpack);
    
    internal delegate string UnpackFileCallback(string file);
    
    [MonoPInvokeCallback(typeof(UnpackFileCallback))]
    internal static string unpackFile(string file) {
        if (Application.platform == RuntimePlatform.Android) {
            var dir = Application.temporaryCachePath + "/";
            if (!File.Exists(dir + file)) {
                WWW unpackerWWW = new WWW("jar:file://" + Application.dataPath + "!/assets/" + file);
                while (!unpackerWWW.isDone) {
                } // This will block in the webplayer.

                if (!string.IsNullOrEmpty(unpackerWWW.error)) {
                    Debug.Log("Error unpacking 'jar:file://" + Application.dataPath + "!/assets/" + file +
                              "'");
                    dir = "";
                    return dir + file;
                }
                File.WriteAllBytes(dir + file, unpackerWWW.bytes); // 64MB limit on File.WriteAllBytes.
            }
            return dir + file;
        }

        return "";
    }
    
    public static void OnEnable()
    {
        InitUnpackFile(unpackFile);
    }
}
