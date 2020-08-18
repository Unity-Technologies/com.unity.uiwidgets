using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui2 {
    public class NativeBindings {
#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        internal const string dllName = "__Internal";
#else
        internal const string dllName = "libUIWidgets_d";
#endif

    }

    public abstract class NativeWrapper {
        protected internal IntPtr _ptr { get; protected set; }
        
        public IntPtr ptr {
            get { return _ptr; }
        }

        protected NativeWrapper() {
        }

        protected NativeWrapper(IntPtr ptr) {
            D.assert(ptr != IntPtr.Zero);
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