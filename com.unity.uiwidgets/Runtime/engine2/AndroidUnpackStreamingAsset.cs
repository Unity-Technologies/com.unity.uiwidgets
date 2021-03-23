using UnityEngine;
using AOT;
using System;
using System.IO;
using System.Runtime.InteropServices;
using NativeBindings = Unity.UIWidgets.ui.NativeBindings;

public static class AndroidUnpackStreamingAssets {

    [DllImport(NativeBindings.dllName)]
    internal static extern void InitUnpackFile(UnpackFileCallback unpack);
    
    internal delegate bool UnpackFileCallback(string file);
    
    [MonoPInvokeCallback(typeof(UnpackFileCallback))]
    internal static bool unpackFile(string file) {
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
                    return false;
                }
                File.WriteAllBytes(dir + file, unpackerWWW.bytes); // 64MB limit on File.WriteAllBytes.
            }
            return true;
        }

        return false;
    }
    
    public static void OnEnable()
    {
        InitUnpackFile(unpackFile);
    }
}
