using UnityEngine;
using AOT;
using System;
using System.Runtime.InteropServices;
using NativeBindings = Unity.UIWidgets.ui.NativeBindings;

public static class NativeConsole {
    internal delegate void LogDelegate(IntPtr message, int iSize);

    [DllImport(NativeBindings.dllName)]
    internal static extern void InitNativeConsoleDelegate(LogDelegate log);

    [MonoPInvokeCallback(typeof(LogDelegate))]
    internal static void LogMessageFromCpp(IntPtr message, int iSize) {
        Debug.Log(Marshal.PtrToStringAnsi(message, iSize));
    }

    public static void OnEnable()
    {
        InitNativeConsoleDelegate(LogMessageFromCpp);
    }
}
