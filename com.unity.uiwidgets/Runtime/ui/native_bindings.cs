using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui {
    public class NativeBindings {
#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        internal const string dllName = "__Internal";
#else
        internal const string dllName = "libUIWidgets";
#endif
    }

    public abstract class NativeWrapper {
        protected internal IntPtr _ptr { get; private set; }

        protected NativeWrapper() {
        }

        Isolate _isolate;

        protected NativeWrapper(IntPtr ptr) {
            _setPtr(ptr);
        }

        protected void _setPtr(IntPtr ptr) {
            D.assert(ptr != IntPtr.Zero);
            _ptr = ptr;

            _isolate = Isolate.current;
            _isolate.addNativeWrapper(this);
        }

        internal void _dispose(bool finalizer = false) {
            if (_ptr != IntPtr.Zero) {
            
                D.assert(_isolate.isValid);
                _isolate.removeNativeWrapper(_ptr);

                DisposePtr(_ptr);

                _ptr = IntPtr.Zero;

                if (!finalizer) {
                    GC.SuppressFinalize(this);
                }
            }
        }

        ~NativeWrapper() {
            _dispose(true);
        }

        public abstract void DisposePtr(IntPtr ptr);
    }

    public abstract class NativeWrapperDisposable : NativeWrapper, IDisposable {
        protected NativeWrapperDisposable() {
        }

        protected NativeWrapperDisposable(IntPtr ptr) : base(ptr) {
        }

        public void Dispose() {
            _dispose();
        }
    }
}