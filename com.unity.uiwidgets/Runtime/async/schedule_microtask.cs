using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.async {
    class _AsyncCallbackEntry {
        public readonly ZoneCallback callback;
        public _AsyncCallbackEntry next;

        internal _AsyncCallbackEntry(ZoneCallback callback) {
            this.callback = callback;
        }
    }

    class _AsyncCallbackState {
        internal _AsyncCallbackEntry _nextCallback;
        internal _AsyncCallbackEntry _lastCallback;
        internal _AsyncCallbackEntry _lastPriorityCallback;
        internal bool _isInCallbackLoop;
    }

    public static partial class async_ {
        static _AsyncCallbackState _getState()
        {
            return Window.instance._asyncCallbackState ??
                   (Window.instance._asyncCallbackState = new _AsyncCallbackState());
        }

        static void _microtaskLoop() {
            var state = _getState();
            while (state._nextCallback != null) {
                state._lastPriorityCallback = null;
                _AsyncCallbackEntry entry = state._nextCallback;
                state._nextCallback = entry.next;
                if (state._nextCallback == null) state._lastCallback = null;
                entry.callback();
            }
        }

        static object _startMicrotaskLoop() {
            var state = _getState();
            state._isInCallbackLoop = true;
            try {
                // Moved to separate function because try-finally prevents
                // good optimization.
                _microtaskLoop();
            }
            finally {
                state._lastPriorityCallback = null;
                state._isInCallbackLoop = false;
                if (state._nextCallback != null) {
                    _AsyncRun._scheduleImmediate(_startMicrotaskLoop);
                }
            }

            return null;
        }

        static void _scheduleAsyncCallback(ZoneCallback callback) {
            var state = _getState();
            _AsyncCallbackEntry newEntry = new _AsyncCallbackEntry(callback);
            if (state._nextCallback == null) {
                state._nextCallback = state._lastCallback = newEntry;
                if (!state._isInCallbackLoop) {
                    _AsyncRun._scheduleImmediate(_startMicrotaskLoop);
                }
            }
            else {
                state._lastCallback.next = newEntry;
                state._lastCallback = newEntry;
            }
        }

        static void _schedulePriorityAsyncCallback(ZoneCallback callback) {
            var state = _getState();
            if (state._nextCallback == null) {
                _scheduleAsyncCallback(callback);
                state._lastPriorityCallback = state._lastCallback;
                return;
            }

            _AsyncCallbackEntry entry = new _AsyncCallbackEntry(callback);
            if (state._lastPriorityCallback == null) {
                entry.next = state._nextCallback;
                state._nextCallback = state._lastPriorityCallback = entry;
            }
            else {
                entry.next = state._lastPriorityCallback.next;
                state._lastPriorityCallback.next = entry;
                state._lastPriorityCallback = entry;
                if (entry.next == null) {
                    state._lastCallback = entry;
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
            Isolate.ensureExists();

            GCHandle callabackHandle = GCHandle.Alloc(callback);
            UIMonoState_scheduleMicrotask(_scheduleMicrotask, (IntPtr) callabackHandle);
        }

        [MonoPInvokeCallback(typeof(UIMonoState_scheduleMicrotaskCallback))]
        static void _scheduleMicrotask(IntPtr callbackHandle) {
            GCHandle handle = (GCHandle) callbackHandle;
            var callback = (ZoneCallback) handle.Target;
            handle.Free();

            try {
                callback();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        delegate void UIMonoState_scheduleMicrotaskCallback(IntPtr callbackHandle);

        [DllImport(NativeBindings.dllName)]
        static extern void UIMonoState_scheduleMicrotask(UIMonoState_scheduleMicrotaskCallback callback,
            IntPtr callbackHandle);
    }
}