using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.async {
    public abstract class Timer : IDisposable {
        public static Timer create(TimeSpan duration, ZoneCallback callback) {
            if (Zone.current == Zone.root) {
                return Zone.current.createTimer(duration, callback);
            }

            return Zone.current
                .createTimer(duration, Zone.current.bindCallbackGuarded(callback));
        }

       

        public static Timer create(TimeSpan duration, Action callback) {
            return create(duration, () => {
                callback.Invoke();
                return null;
            });
        }

        public static Timer periodic(TimeSpan duration, ZoneUnaryCallback callback) {
            if (Zone.current == Zone.root) {
                return Zone.current.createPeriodicTimer(duration, callback);
            }

            var boundCallback = Zone.current.bindUnaryCallbackGuarded(callback);
            return Zone.current.createPeriodicTimer(duration, boundCallback);
        }

        public static void run(ZoneCallback callback) {
            create(TimeSpan.Zero, callback);
        }

        public abstract void cancel();

        public void Dispose() {
            cancel();
        }

        public abstract long tick { get; }

        public abstract bool isActive { get; }

        internal static Timer _createTimer(TimeSpan duration, ZoneCallback callback) {
            return _Timer._createTimer(_ => callback(), (int) duration.TotalMilliseconds, false);
        }

        internal static Timer _createPeriodicTimer(
            TimeSpan duration, ZoneUnaryCallback callback) {
            return _Timer._createTimer(callback, (int) duration.TotalMilliseconds, true);
        }
    }

    class _Timer : Timer {
        long _tick = 0;

        ZoneUnaryCallback _callback;
        long _wakeupTime;
        readonly int _milliSeconds;
        readonly bool _repeating;

        _Timer(ZoneUnaryCallback callback, long wakeupTime, int milliSeconds, bool repeating) {
            _callback = callback;
            _wakeupTime = wakeupTime;
            _milliSeconds = milliSeconds;
            _repeating = repeating;
        }
        
                
        public static TimeSpan timespanSinceStartup {
            get { return TimeSpan.FromMilliseconds(UIMonoState_timerMillisecondClock()); }
        }

        internal static _Timer _createTimer(ZoneUnaryCallback callback, int milliSeconds, bool repeating) {
            if (milliSeconds < 0) {
                milliSeconds = 0;
            }

            long now = UIMonoState_timerMillisecondClock();
            long wakeupTime = (milliSeconds == 0) ? now : (now + 1 + milliSeconds);

            _Timer timer = new _Timer(callback, wakeupTime, milliSeconds, repeating);
            timer._enqueue();

            return timer;
        }

        public override void cancel() {
            _callback = null;
        }

        public override bool isActive => _callback != null;

        public override long tick => _tick;

        void _advanceWakeupTime() {
            if (_milliSeconds > 0) {
                _wakeupTime += _milliSeconds;
            }
            else {
                _wakeupTime = UIMonoState_timerMillisecondClock();
            }
        }

        const long MILLI_TO_NANO = 1000000L;
        
        void _enqueue() {
            Isolate.ensureExists();
            
            GCHandle callbackHandle = GCHandle.Alloc(this);
            UIMonoState_postTaskForTime(_postTaskForTime, (IntPtr) callbackHandle, _wakeupTime * MILLI_TO_NANO);
        }

        [MonoPInvokeCallback(typeof(UIMonoState_postTaskForTimeCallback))]
        static void _postTaskForTime(IntPtr callbackHandle) {
            GCHandle timerHandle = (GCHandle) callbackHandle;
            var timer = (_Timer) timerHandle.Target;
            timerHandle.Free();

            try {
                if (timer._callback != null) {
                    var callback = timer._callback;
                    if (!timer._repeating) {
                        timer._callback = null;
                    }
                    else if (timer._milliSeconds > 0) {
                        var ms = timer._milliSeconds;
                        long overdue = UIMonoState_timerMillisecondClock() - timer._wakeupTime;
                        if (overdue > ms) {
                            long missedTicks = overdue / ms;
                            timer._wakeupTime += missedTicks * ms;
                            timer._tick += missedTicks;
                        }
                    }

                    timer._tick += 1;

                    callback(timer);

                    if (timer._repeating && (timer._callback != null)) {
                        timer._advanceWakeupTime();
                        timer._enqueue();
                    }
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [DllImport(NativeBindings.dllName)]
        static extern long UIMonoState_timerMillisecondClock();

        delegate void UIMonoState_postTaskForTimeCallback(IntPtr callbackHandle);

        [DllImport(NativeBindings.dllName)]
        static extern void UIMonoState_postTaskForTime(UIMonoState_postTaskForTimeCallback callback,
            IntPtr callbackHandle, long targetTimeNanos);
    }
}