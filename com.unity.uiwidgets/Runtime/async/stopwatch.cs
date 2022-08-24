using System;

namespace Unity.UIWidgets.core {
   public  class Stopwatch {
        static int? _frequency;

        // The _start and _stop fields capture the time when [start] and [stop]
        // are called respectively.
        // If _stop is null, the stopwatch is running.
        int? _start = 0;
        int? _stop = 0;

        public Stopwatch() {
            if (_frequency == null) _initTicker();
        }

        public int? frequency {
            get { return _frequency; }
        }

        public void start() {
            if (_stop != null) {
                // (Re)start this stopwatch.
                // Don't count the time while the stopwatch has been stopped.
                _start += _now() - _stop;
                _stop = null;
            }
        }

        public void stop() {
            _stop = _stop ?? _now();
        }

        public void reset() {
            _start = _stop ?? _now();
        }

        public int? elapsedTicks {
            get { return (_stop ?? _now()) - _start; }
        }

        public TimeSpan elapsed {
            get { return TimeSpan.FromMilliseconds(elapsedMicroseconds); }
        }

        // This is external, we might need to reimplement it 
        int elapsedMicroseconds { get; }

        // This is external, we might need to reimplement it 
        int elapsedMilliseconds { get; }

        bool isRunning {
            get { return _stop == null; }
        }

        // This is external, we might need to reimplement it 
        static void _initTicker() {
        }

        // This is external, we might need to reimplement it 
        static int _now() {
            return DateTime.Now.Millisecond;
        }
    }
}