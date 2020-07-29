using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui2 {
    public static class Isolate {
        public static IntPtr current() {
            return NativeBindings.Isolate_current();
        }

        public static IDisposable getScope(IntPtr isolate) {
            return new _IsolateDisposable(isolate);
        }

        class _IsolateDisposable : IDisposable {
            IntPtr _isolate;
            IntPtr _previous;

            public _IsolateDisposable(IntPtr isolate) {
                _isolate = isolate;
                _previous = NativeBindings.Isolate_current();
                if (_previous == _isolate) {
                    return;
                }

                if (_previous != IntPtr.Zero) {
                    NativeBindings.Isolate_exit();
                }

                NativeBindings.Isolate_enter(_isolate);
            }

            public void Dispose() {
                var current = NativeBindings.Isolate_current();
                D.assert(current == IntPtr.Zero || current == _isolate);
                if (_previous == _isolate) {
                    return;
                }

                if (current != IntPtr.Zero) {
                    NativeBindings.Isolate_exit();
                }

                if (_previous != IntPtr.Zero) {
                    NativeBindings.Isolate_enter(_previous);
                }
            }
        }
    }
}
