using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public static class Hooks {
        
        static bool hooked = false;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static unsafe void hook() {
            D.assert(!hooked);

            hooked = true;
            Mono_hook(
                Mono_throwException,
                Mono_shutdown);

            Window_hook(
                Window_constructor,
                Window_dispose,
                Window_updateWindowMetrics,
                Window_beginFrame,
                Window_drawFrame,
                ui_._dispatchPlatformMessage,
                ui_._dispatchPointerDataPacket);
        }

        
        public static void tryHook() {
            if (hooked) {
                return;
            }
            hook();
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
            ui_.Window_dispatchPlatformMessageCallback Window_dispatchPlatformMessage,
            ui_.Window_dispatchPointerDataPacketCallback Window_dispatchPointerDataPacket);
    }

    public static partial class ui_ {
        internal unsafe delegate void Window_dispatchPlatformMessageCallback(string name, byte* dataRaw, int dataLength,
            int responseId);

        [MonoPInvokeCallback(typeof(Window_dispatchPlatformMessageCallback))]
        internal static unsafe void _dispatchPlatformMessage(
            string name, byte* dataRaw, int dataLength, int responseId) {
            try {
                var window = Window.instance;

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
                        window._respondToPlatformMessage(responseId, null);
                    }
                }
                else if (window.onPlatformMessage != null) {
                    _invoke3<string, byte[], PlatformMessageResponseCallback>(
                        (name1, data1, callback1) => window.onPlatformMessage(name1, data1, callback1),
                        window._onPlatformMessageZone,
                        name, data, responseData => window._respondToPlatformMessage(responseId, responseData)
                    );
                }
                else {
                    channelBuffers.push(name, data, responseData
                        => window._respondToPlatformMessage(responseId, responseData));
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        internal unsafe delegate void Window_dispatchPointerDataPacketCallback(byte* bytes, int length);

        [MonoPInvokeCallback(typeof(Window_dispatchPointerDataPacketCallback))]
        internal static unsafe void _dispatchPointerDataPacket(byte* bytes, int length) {
            try {
                var window = Window.instance;
                if (window.onPointerDataPacket != null)
                    _invoke1<PointerDataPacket>(
                        p => window.onPointerDataPacket(p),
                        window._onPointerDataPacketZone,
                        _unpackPointerDataPacket(bytes, length));
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        internal static void _invoke1<A>(Action<A> callback, Zone zone, A arg) {
            if (callback == null)
                return;

            D.assert(zone != null);

            if (ReferenceEquals(zone, Zone.current)) {
                callback(arg);
            }
            else {
                zone.runUnaryGuarded((a) => {
                    callback((A) a);
                    return null;
                }, arg);
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

        const int _kPointerDataFieldCount = 29;

        static unsafe PointerDataPacket _unpackPointerDataPacket(byte* packet, int packetLength) {
            const int kStride = 8;
            const int kBytesPerPointerData = _kPointerDataFieldCount * kStride;
            int length = packetLength / kBytesPerPointerData;
            D.assert(length * kBytesPerPointerData == packetLength);

            List<PointerData> data = new List<PointerData>(length);
            for (int i = 0; i < length; ++i) {
                int offset = i * _kPointerDataFieldCount;
                data.Add(new PointerData(
                    timeStamp: TimeSpan.FromMilliseconds((*(long*) (packet + kStride * offset++)) / 1000),
                    change: (PointerChange) (*(long*) (packet + kStride * offset++)),
                    kind: (PointerDeviceKind) (*(long*) (packet + kStride * offset++)),
                    signalKind: (PointerSignalKind) (*(long*) (packet + kStride * offset++)),
                    device: (int) *(long*) (packet + kStride * offset++),
                    pointerIdentifier: (int) (*(long*) (packet + kStride * offset++)),
                    physicalX: (float) *(double*) (packet + kStride * offset++),
                    physicalY: (float) *(double*) (packet + kStride * offset++),
                    physicalDeltaX: (float) *(double*) (packet + kStride * offset++),
                    physicalDeltaY: (float) *(double*) (packet + kStride * offset++),
                    buttons: (int) *(long*) (packet + kStride * offset++),
                    modifier: (int) *(long*) (packet + kStride * offset++),
                    obscured: *(long*) (packet + kStride * offset++) != 0,
                    synthesized: *(long*) (packet + kStride * offset++) != 0,
                    pressure: (float) *(double*) (packet + kStride * offset++),
                    pressureMin: (float) *(double*) (packet + kStride * offset++),
                    pressureMax: (float) *(double*) (packet + kStride * offset++),
                    distance: (float) *(double*) (packet + kStride * offset++),
                    distanceMax: (float) *(double*) (packet + kStride * offset++),
                    size: (float) *(double*) (packet + kStride * offset++),
                    radiusMajor: (float) *(double*) (packet + kStride * offset++),
                    radiusMinor: (float) *(double*) (packet + kStride * offset++),
                    radiusMin: (float) *(double*) (packet + kStride * offset++),
                    radiusMax: (float) *(double*) (packet + kStride * offset++),
                    orientation: (float) *(double*) (packet + kStride * offset++),
                    tilt: (float) *(double*) (packet + kStride * offset++),
                    platformData: (int) *(long*) (packet + kStride * offset++),
                    scrollDeltaX: (float) *(double*) (packet + kStride * offset++),
                    scrollDeltaY: (float) *(double*) (packet + kStride * offset++)
                ));
                D.assert(offset == (i + 1) * _kPointerDataFieldCount);
            }

            return new PointerDataPacket(data: data);
        }
    }
}