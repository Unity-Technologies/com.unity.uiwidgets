using System;
using System.Runtime.InteropServices;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui2 {
    public static class Isolate {
        
        public static IntPtr current {
            get => Isolate_current();
        }
        
        public static IDisposable getScope(IntPtr isolate) {
            return new _IsolateDisposable(isolate);
        }

        class _IsolateDisposable : IDisposable {
            IntPtr _isolate;
            IntPtr _previous;

            public _IsolateDisposable(IntPtr isolate) {
                _isolate = isolate;
                _previous = Isolate_current();
                if (_previous == _isolate) {
                    return;
                }

                if (_previous != IntPtr.Zero) {
                    Isolate_exit();
                }

                Isolate_enter(_isolate);
            }

            public void Dispose() {
                var current = Isolate_current();
                D.assert(current == IntPtr.Zero || current == _isolate);
                if (_previous == _isolate) {
                    return;
                }

                if (current != IntPtr.Zero) {
                    Isolate_exit();
                }

                if (_previous != IntPtr.Zero) {
                    Isolate_enter(_previous);
                }
            }
        }
        
        
        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Isolate_current();

        [DllImport(NativeBindings.dllName)]
        static extern void Isolate_enter(IntPtr isolate);

        [DllImport(NativeBindings.dllName)]
        static extern void Isolate_exit();

    }
}
