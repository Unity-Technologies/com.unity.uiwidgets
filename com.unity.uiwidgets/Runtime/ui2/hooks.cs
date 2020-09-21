using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.ui2 {
    public static class Hooks {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        static unsafe void hook() {
            Mono_hook(
                Mono_throwException,
                Mono_shutdown);

            Window_hook(
                Window_constructor,
                Window_dispose,
                Window_updateWindowMetrics,
                Window_beginFrame,
                Window_drawFrame,
                ui_._dispatchPlatformMessage);
        }

        delegate void Mono_ThrowExceptionCallback(IntPtr exception);

        [MonoPInvokeCallback(typeof(Mono_ThrowExceptionCallback))]
        static void Mono_throwException(IntPtr exception) {
            throw new Exception(Marshal.PtrToStringAnsi(exception));
        }

        delegate void Mono_ShutdownCallback(IntPtr isolate);

        [MonoPInvokeCallback(typeof(Mono_ShutdownCallback))]
        static void Mono_shutdown(IntPtr isolate) {
            try {
                Isolate.shutdown(isolate);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [DllImport(NativeBindings.dllName)]
        static extern void Mono_hook(Mono_ThrowExceptionCallback throwException, Mono_ShutdownCallback shutdown);

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
            try {
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
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        delegate void Window_beginFrameCallback(long microseconds);

        [MonoPInvokeCallback(typeof(Window_beginFrameCallback))]
        static void Window_beginFrame(long microseconds) {
            try {
                Window.instance.onBeginFrame?.Invoke(TimeSpan.FromMilliseconds(microseconds / 1000.0));
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        delegate void Window_drawFrameCallback();

        [MonoPInvokeCallback(typeof(Window_drawFrameCallback))]
        static void Window_drawFrame() {
            try {
                Window.instance.onDrawFrame?.Invoke();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [DllImport(NativeBindings.dllName)]
        static extern void Window_hook(
            Window_constructorCallback Window_constructor,
            Window_disposeCallback Window_dispose,
            Window_updateWindowMetricsCallback Window_updateWindowMetrics,
            Window_beginFrameCallback Window_beginFrame,
            Window_drawFrameCallback Window_drawFrame,
            ui_.Window_dispatchPlatformMessageCallback Window_dispatchPlatformMessage);
    }

    public static partial class ui_ {
        internal unsafe delegate void Window_dispatchPlatformMessageCallback(string name, byte* dataRaw, int dataLength,
            int responseId);

        [MonoPInvokeCallback(typeof(Window_dispatchPlatformMessageCallback))]
        internal static unsafe void _dispatchPlatformMessage(
            string name, byte* dataRaw, int dataLength, int responseId) {
            try {
                var data = new byte[dataLength];
                Marshal.Copy((IntPtr) dataRaw, data, 0, dataLength);

                if (name == ChannelBuffers.kControlChannelName) {
                    try {
                        channelBuffers.handleMessage(data);
                    }
                    catch (Exception ex) {
                        Debug.LogError($"Message to \"{name}\" caused exception {ex}");
                    }
                    finally {
                        Window.instance._respondToPlatformMessage(responseId, null);
                    }
                }
                else if (Window.instance.onPlatformMessage != null) {
                    _invoke3<string, byte[], PlatformMessageResponseCallback>(
                        (name1, data1, callback1) => Window.instance.onPlatformMessage(name1, data1, callback1),
                        Window.instance._onPlatformMessageZone,
                        name, data, responseData =>
                            Window.instance._respondToPlatformMessage(responseId, responseData)
                    );
                }
                else {
                    channelBuffers.push(name, data, responseData
                        => Window.instance._respondToPlatformMessage(responseId, responseData));
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        internal static void _invoke3<A1, A2, A3>(Action<A1, A2, A3> callback, Zone zone, A1 arg1, A2 arg2, A3 arg3) {
            if (callback == null)
                return;

            D.assert(zone != null);

            if (ReferenceEquals(zone, Zone.current)) {
                callback(arg1, arg2, arg3);
            }
            else {
                zone.runGuarded(() => {
                    callback(arg1, arg2, arg3);
                    return null;
                });
            }
        }
    }
}