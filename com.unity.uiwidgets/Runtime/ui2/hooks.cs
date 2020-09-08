using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.ui2 {
    public static class Hooks {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        static void hook() {
            Mono_hook(
                Mono_throwException);

            Window_hook(
                Window_constructor,
                Window_dispose,
                Window_updateWindowMetrics,
                Window_beginFrame,
                Window_drawFrame);
        }

        delegate void Mono_ThrowExceptionCallback(IntPtr exception);

        [MonoPInvokeCallback(typeof(Mono_ThrowExceptionCallback))]
        static void Mono_throwException(IntPtr exception) {
            throw new Exception(Marshal.PtrToStringAnsi(exception));
        }

        [DllImport(NativeBindings.dllName)]
        static extern void Mono_hook(Mono_ThrowExceptionCallback throwException);

        delegate IntPtr Window_constructorCallback(IntPtr ptr);

        [MonoPInvokeCallback(typeof(Window_constructorCallback))]
        static IntPtr Window_constructor(IntPtr ptr) {
            Window window = new Window();
            window._ptr = ptr;
            return (IntPtr) GCHandle.Alloc(window);
        }

        delegate void Window_disposeCallback(IntPtr handle);

        [MonoPInvokeCallback(typeof(Window_disposeCallback))]
        static void Window_dispose(IntPtr handle) {
            GCHandle gcHandle = (GCHandle) handle;
            Window window = (Window) gcHandle.Target;
            window._ptr = IntPtr.Zero;
            gcHandle.Free();
        }

        delegate void Window_updateWindowMetricsCallback(
            float devicePixelRatio,
            float width,
            float height,
            float depth,
            float viewPaddingTop,
            float viewPaddingRight,
            float viewPaddingBottom,
            float viewPaddingLeft,
            float viewInsetTop,
            float viewInsetRight,
            float viewInsetBottom,
            float viewInsetLeft,
            float systemGestureInsetTop,
            float systemGestureInsetRight,
            float systemGestureInsetBottom,
            float systemGestureInsetLeft
        );

        [MonoPInvokeCallback(typeof(Window_updateWindowMetricsCallback))]
        static void Window_updateWindowMetrics(
            float devicePixelRatio,
            float width,
            float height,
            float depth,
            float viewPaddingTop,
            float viewPaddingRight,
            float viewPaddingBottom,
            float viewPaddingLeft,
            float viewInsetTop,
            float viewInsetRight,
            float viewInsetBottom,
            float viewInsetLeft,
            float systemGestureInsetTop,
            float systemGestureInsetRight,
            float systemGestureInsetBottom,
            float systemGestureInsetLeft
        ) {
            var window = Window.instance;

            window.devicePixelRatio = devicePixelRatio;

            window.physicalSize = new Size(width, height);
            window.physicalDepth = depth;

            window.viewPadding = new WindowPadding(
                top: viewPaddingTop,
                right: viewPaddingRight,
                bottom: viewPaddingBottom,
                left: viewPaddingLeft);

            window.viewInsets = new WindowPadding(
                top: viewInsetTop,
                right: viewInsetRight,
                bottom: viewInsetBottom,
                left: viewInsetLeft);

            window.padding = new WindowPadding(
                top: Mathf.Max(0.0f, viewPaddingTop - viewInsetTop),
                right: Mathf.Max(0.0f, viewPaddingRight - viewInsetRight),
                bottom: Mathf.Max(0.0f, viewPaddingBottom - viewInsetBottom),
                left: Mathf.Max(0.0f, viewPaddingLeft - viewInsetLeft));

            window.systemGestureInsets = new WindowPadding(
                top: Mathf.Max(0.0f, systemGestureInsetTop),
                right: Mathf.Max(0.0f, systemGestureInsetRight),
                bottom: Mathf.Max(0.0f, systemGestureInsetBottom),
                left: Mathf.Max(0.0f, systemGestureInsetLeft));

            window.onMetricsChanged?.Invoke();
        }

        delegate void Window_beginFrameCallback(long microseconds);

        [MonoPInvokeCallback(typeof(Window_beginFrameCallback))]
        static void Window_beginFrame(long microseconds) {
            Window.instance.onBeginFrame?.Invoke(TimeSpan.FromMilliseconds(microseconds / 1000.0));
        }

        delegate void Window_drawFrameCallback();

        [MonoPInvokeCallback(typeof(Window_drawFrameCallback))]
        static void Window_drawFrame() {
            Window.instance.onDrawFrame?.Invoke();
        }

        [DllImport(NativeBindings.dllName)]
        static extern void Window_hook(
            Window_constructorCallback Window_constructor,
            Window_disposeCallback Window_dispose,
            Window_updateWindowMetricsCallback Window_updateWindowMetrics,
            Window_beginFrameCallback Window_beginFrame,
            Window_drawFrameCallback Window_drawFrame);
    }
}