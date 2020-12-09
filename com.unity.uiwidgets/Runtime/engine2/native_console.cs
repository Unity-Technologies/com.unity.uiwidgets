using UnityEngine;
using AOT;
using System;
using System.Runtime.InteropServices;
using NativeBindings = Unity.UIWidgets.ui.NativeBindings;

public static class NativeConsole {
    public delegate void LogDelegate(IntPtr message, int iSize);

    [DllImport(NativeBindings.dllName)]
    public static extern void InitNativeConsoleDelegate(LogDelegate log);

    [MonoPInvokeCallback(typeof(LogDelegate))]
    public static void LogMessageFromCpp(IntPtr message, int iSize) {
        Debug.Log(Marshal.PtrToStringAnsi(message, iSize));
    }

}
