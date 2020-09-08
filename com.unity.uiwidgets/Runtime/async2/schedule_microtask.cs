using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.ui2;

namespace Unity.UIWidgets.async2 {
    class _AsyncCallbackEntry {
        public readonly ZoneCallback callback;
        public _AsyncCallbackEntry next;

        internal _AsyncCallbackEntry(ZoneCallback callback) {
            this.callback = callback;
        }
    }

    public static partial class async_ {
        static _AsyncCallbackEntry _nextCallback;
        static _AsyncCallbackEntry _lastCallback;
        static _AsyncCallbackEntry _lastPriorityCallback;

        static bool _isInCallbackLoop = false;

        static void _microtaskLoop() {
            while (_nextCallback != null) {
                _lastPriorityCallback = null;
                _AsyncCallbackEntry entry = _nextCallback;
                _nextCallback = entry.next;
                if (_nextCallback == null) _lastCallback = null;
                entry.callback();
            }
        }

        static object _startMicrotaskLoop() {
            _isInCallbackLoop = true;
            try {
                // Moved to separate function because try-finally prevents
                // good optimization.
                _microtaskLoop();
            }
            finally {
                _lastPriorityCallback = null;
                _isInCallbackLoop = false;
                if (_nextCallback != null) {
                    _AsyncRun._scheduleImmediate(_startMicrotaskLoop);
                }
            }

            return null;
        }

        static void _scheduleAsyncCallback(ZoneCallback callback) {
            _AsyncCallbackEntry newEntry = new _AsyncCallbackEntry(callback);
            if (_nextCallback == null) {
                _nextCallback = _lastCallback = newEntry;
                if (!_isInCallbackLoop) {
                    _AsyncRun._scheduleImmediate(_startMicrotaskLoop);
                }
            }
            else {
                _lastCallback.next = newEntry;
                _lastCallback = newEntry;
            }
        }

        static void _schedulePriorityAsyncCallback(ZoneCallback callback) {
            if (_nextCallback == null) {
                _scheduleAsyncCallback(callback);
                _lastPriorityCallback = _lastCallback;
                return;
            }

            _AsyncCallbackEntry entry = new _AsyncCallbackEntry(callback);
            if (_lastPriorityCallback == null) {
                entry.next = _nextCallback;
                _nextCallback = _lastPriorityCallback = entry;
            }
            else {
                entry.next = _lastPriorityCallback.next;
                _lastPriorityCallback.next = entry;
                _lastPriorityCallback = entry;
                if (entry.next == null) {
                    _lastCallback = entry;
                }
            }
        }

        public static void scheduleMicrotask(ZoneCallback callback) {
            _Zone currentZone = (_Zone) Zone.current;
            if (ReferenceEquals(_rootZone, currentZone)) {
                // No need to bind the callback. We know that the root's scheduleMicrotask
                // will be invoked in the root zone.
                _rootScheduleMicrotask(null, null, _rootZone, callback);
                return;
            }

            _ZoneFunction<ScheduleMicrotaskHandler> implementation = currentZone._scheduleMicrotask;
            if (ReferenceEquals(_rootZone, implementation.zone) &&
                _rootZone.inSameErrorZone(currentZone)) {
                _rootScheduleMicrotask(
                    null, null, currentZone, currentZone.registerCallback(callback));
                return;
            }

            Zone.current.scheduleMicrotask(Zone.current.bindCallbackGuarded(callback));
        }
    }

    class _AsyncRun {
        internal static void _scheduleImmediate(ZoneCallback callback) {
            GCHandle callabackHandle = GCHandle.Alloc(callback);
            UIMonoState_scheduleMicrotask(_scheduleMicrotask, (IntPtr) callabackHandle);
        }

        [MonoPInvokeCallback(typeof(UIMonoState_scheduleMicrotaskCallback))]
        static void _scheduleMicrotask(IntPtr callbackHandle) {
            GCHandle handle = (GCHandle) callbackHandle;
            var callback = (ZoneCallback) handle.Target;
            handle.Free();

            callback();
        }

        delegate void UIMonoState_scheduleMicrotaskCallback(IntPtr callbackHandle);

        [DllImport(NativeBindings.dllName)]
        static extern void UIMonoState_scheduleMicrotask(UIMonoState_scheduleMicrotaskCallback callback,
            IntPtr callbackHandle);
    }
}