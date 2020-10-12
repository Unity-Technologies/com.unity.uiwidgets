using System;
using System.Runtime.InteropServices;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.uiOld{
    class NativeBindings {
#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string dllName = "__Internal";
#else
        const string dllName = "libUIWidgets_d";
#endif

        [DllImport(dllName)]
        public static extern IntPtr ImageShader_constructor();

        [DllImport(dllName)]
        public static extern void ImageShader_dispose(IntPtr ptr);

        [DllImport(dllName)]
        public static extern unsafe void ImageShader_initWithImage(IntPtr ptr,
            IntPtr image, int tmx, int tmy, float* matrix4);

        [DllImport(dllName)]
        public static extern IntPtr Gradient_constructor();

        [DllImport(dllName)]
        public static extern void Gradient_dispose(IntPtr ptr);

        [DllImport(dllName)]
        public static extern unsafe void Gradient_initLinear(IntPtr ptr,
            float* endPoints, int endPointsLength,
            uint* colors, int colorsLength,
            float* colorStops, int colorStopsLength,
            int tileMode, float* matrix4);

        [DllImport(dllName)]
        public static extern unsafe void Gradient_initRadial(IntPtr ptr,
            float centerX, float centerY, float radius,
            uint* colors, int colorsLength,
            float* colorStops, int colorStopsLength,
            int tileMode, float* matrix4);

        [DllImport(dllName)]
        public static extern unsafe void Gradient_initConical(IntPtr ptr,
            float startX, float startY, float startRadius,
            float endX, float endY, float endRadius,
            uint* colors, int colorsLength,
            float* colorStops, int colorStopsLength,
            int tileMode, float* matrix4);

        [DllImport(dllName)]
        public static extern unsafe void Gradient_initSweep(IntPtr ptr,
            float centerX, float centerY,
            uint* colors, int colorsLength,
            float* colorStops, int colorStopsLength,
            int tileMode, float startAngle, float endAngle, float* matrix4);

        [DllImport(dllName)]
        public static extern IntPtr ColorFilter_constructor();

        [DllImport(dllName)]
        public static extern void ColorFilter_dispose(IntPtr ptr);

        [DllImport(dllName)]
        public static extern void ColorFilter_initMode(IntPtr ptr, uint color, int blendMode);

        [DllImport(dllName)]
        public static extern unsafe void ColorFilter_initMatrix(IntPtr ptr, float* matrix4);

        [DllImport(dllName)]
        public static extern unsafe void ColorFilter_initLinearToSrgbGamma(IntPtr ptr);

        [DllImport(dllName)]
        public static extern unsafe void ColorFilter_initSrgbToLinearGamma(IntPtr ptr);

        [DllImport(dllName)]
        public static extern IntPtr ImageFilter_constructor();

        [DllImport(dllName)]
        public static extern void ImageFilter_dispose(IntPtr ptr);

        [DllImport(dllName)]
        public static extern void ImageFilter_initBlur(IntPtr ptr, float sigmaX, float sigmaY);

        [DllImport(dllName)]
        public static extern unsafe void ImageFilter_initMatrix(IntPtr ptr, float* matrix4, int filterQuality);

        [DllImport(dllName)]
        public static extern IntPtr Canvas_constructor(IntPtr recorder,
            double left,
            double top,
            double right,
            double bottom);

        [DllImport(dllName)]
        public static extern void Canvas_dispose(IntPtr ptr);

        [DllImport(dllName)]
        public static extern void Canvas_save(IntPtr ptr);

        [DllImport(dllName)]
        public static extern void Canvas_restore(IntPtr ptr);

        [DllImport(dllName)]
        public static extern void Image_dispose(IntPtr ptr);

        [DllImport(dllName)]
        public static extern int Image_width(IntPtr ptr);

        [DllImport(dllName)]
        public static extern int Image_height(IntPtr ptr);

        public delegate void Image_toByteDataCallback(IntPtr callbackHandle, IntPtr data, int length);

        [DllImport(dllName)]
        public static extern string Image_toByteData(IntPtr ptr, int format, Image_toByteDataCallback callback,
            IntPtr callbackHandle);

        [DllImport(dllName)]
        public static extern void Picture_dispose(IntPtr ptr);

        [DllImport(dllName)]
        public static extern int Picture_GetAllocationSize(IntPtr ptr);

        public delegate void Picture_toImageCallback(IntPtr callbackHandle, IntPtr result);

        [DllImport(dllName)]
        public static extern string Picture_toImage(IntPtr ptr, int width, int height, Picture_toImageCallback callback,
            IntPtr callbackHandle);

        [DllImport(dllName)]
        public static extern IntPtr PictureRecorder_constructor();

        [DllImport(dllName)]
        public static extern void PictureRecorder_dispose(IntPtr ptr);

        [DllImport(dllName)]
        public static extern bool PictureRecorder_isRecording(IntPtr ptr);

        [DllImport(dllName)]
        public static extern IntPtr PictureRecorder_endRecording(IntPtr ptr);
    }

    public abstract class NativeWrapper {
        protected internal IntPtr _ptr { get; protected set; }

        protected NativeWrapper() {
        }

        protected NativeWrapper(IntPtr ptr) {
            D.assert(_ptr != IntPtr.Zero);
            _ptr = ptr;
        }

        ~NativeWrapper() {
            if (_ptr != IntPtr.Zero) {
                DisposePtr(_ptr);
                _ptr = IntPtr.Zero;
            }
        }

        protected abstract void DisposePtr(IntPtr ptr);
    }

    public abstract class NativeWrapperDisposable : IDisposable {
        protected internal IntPtr _ptr { get; protected set; }

        protected NativeWrapperDisposable() {
        }

        protected NativeWrapperDisposable(IntPtr ptr) {
            D.assert(_ptr != IntPtr.Zero);
            _ptr = ptr;
        }

        ~NativeWrapperDisposable() {
            if (_ptr != IntPtr.Zero) {
                DisposePtr(_ptr);
                _ptr = IntPtr.Zero;
            }
        }

        protected abstract void DisposePtr(IntPtr ptr);

        public void Dispose() {
            if (_ptr != IntPtr.Zero) {
                DisposePtr(_ptr);
                _ptr = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
        }
    }
}