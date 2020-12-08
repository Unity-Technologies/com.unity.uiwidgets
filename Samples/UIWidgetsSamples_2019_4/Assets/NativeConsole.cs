using UnityEngine;
using AOT;
using System;
using System.Runtime.InteropServices;

public static class NativeConsole
{
    public delegate void LogDelegate(IntPtr message, int iSize);

    [DllImport("libUIWidgets_d.dll")]
    public static extern void InitLogMessageFromCppDelegate(LogDelegate log);

    //C# Function for C++'s call
    [MonoPInvokeCallback(typeof(LogDelegate))]
    public static void LogMessageFromCpp(IntPtr message, int iSize)
    {
        Debug.Log(Marshal.PtrToStringAnsi(message, iSize));
    }

}

