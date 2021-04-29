using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.ui;
using Ticker = Unity.UIWidgets.scheduler.Ticker;
using TickerFuture = Unity.UIWidgets.scheduler.TickerFuture;
using TickerProvider = Unity.UIWidgets.scheduler.TickerProvider;

namespace Unity.UIWidgets.animation {
    enum _AnimationDirection {
        forward,
        reverse,
    }

    public enum AnimationBehavior {
        normal,
        preserve,
    }

    public class AnimationController :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<float> {
        public AnimationController(
            float? value = null,
            TimeSpan? duration = null,
            TimeSpan? reverseDuration = null,
            string debugLabel = null,
            float lowerBound = 0.0f,
            float upperBound = 1.0f,
            AnimationBehavior animationBehavior = AnimationBehavior.normal,
            TickerProvider vsync = null
        ) {
            D.assert(upperBound >= lowerBound);
            D.assert(vsync != null);
            _direction = _AnimationDirection.forward;

            this.duration = duration;
            this.reverseDuration = reverseDuration;
            this.debugLabel = debugLabel;
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
            this.animationBehavior = animationBehavior;

            _ticker = vsync.createTicker(_tick);
            _internalSetValue(value ?? lowerBound);
        }

        AnimationController(
            float value = 0.0f,
            TimeSpan? duration = null,
            TimeSpan? reverseDuration = null,
            string debugLabel = null,
            AnimationBehavior? animationBehavior = null,
            TickerProvider vsync = null
        ) {
            D.assert(vsync != null);
            D.assert(animationBehavior != null);
            lowerBound = float.NegativeInfinity;
            upperBound = float.PositiveInfinity;
            _direction = _AnimationDirection.forward;

            this.duration = duration;
            this.reverseDuration = reverseDuration;
            this.debugLabel = debugLabel;
            this.animationBehavior = animationBehavior.Value;

            _ticker = vsync.createTicker(_tick);
            _internalSetValue(value);
        }

        public static AnimationController unbounded(
            float value = 0.0f,
            TimeSpan? duration = null,
            TimeSpan? reverseDuration = null,
            string debugLabel = null,
            TickerProvider vsync = null,
            AnimationBehavior animationBehavior = AnimationBehavior.preserve
        ) {
            return new AnimationController(value, duration, reverseDuration, debugLabel, animationBehavior, vsync);
        }

        public readonly float lowerBound;

        public readonly float upperBound;

        public readonly AnimationBehavior animationBehavior;

        public readonly string debugLabel;

        public Animation<float> view {
            get { return this; }
        }

        public TimeSpan? duration;

        public TimeSpan? reverseDuration;
        
        Ticker _ticker;

        public void resync(TickerProvider vsync) {
            Ticker oldTicker = _ticker;
            _ticker = vsync.createTicker(_tick);
            _ticker.absorbTicker(oldTicker);
        }

        Simulation _simulation;

        public override float value {
            get { return _value; }
            
        }

        float _value;

        public void setValue(float newValue) {
            stop();
            _internalSetValue(newValue);
            notifyListeners();
            _checkStatusChanged();
        }


        public void reset() {
            setValue(lowerBound);
        }

        public float velocity {
            get {
                if (!isAnimating) {
                    return 0.0f;
                }

                return _simulation.dx((float) lastElapsedDuration.Value.Ticks / TimeSpan.TicksPerSecond);
            }
        }

        void _internalSetValue(float newValue) {
            _value = newValue.clamp(lowerBound, upperBound);
            if (foundation_.FloatEqual(_value, lowerBound)) {
                _status = AnimationStatus.dismissed;
            }
            else if (foundation_.FloatEqual(_value, upperBound)) {
                _status = AnimationStatus.completed;
            }
            else {
                _status = (_direction == _AnimationDirection.forward)
                    ? AnimationStatus.forward
                    : AnimationStatus.reverse;
            }
        }

        TimeSpan? lastElapsedDuration {
            get { return _lastElapsedDuration; }
        }

        TimeSpan? _lastElapsedDuration;

        public bool isAnimating {
            get { return _ticker != null && _ticker.isActive; }
        }

        _AnimationDirection _direction;

        public override AnimationStatus status {
            get { return _status; }
        }

        AnimationStatus _status;

        public TickerFuture forward(float? from = null) {
            D.assert(() => {
                if (duration == null) {
                    throw new UIWidgetsError(
                        "AnimationController.forward() called with no default duration.\n" +
                        "The \"duration\" property should be set, either in the constructor or later, before " +
                        "calling the forward() function."
                    );
                }

                return true;
            });
            D.assert(
                _ticker != null,
                () => "AnimationController.forward() called after AnimationController.dispose()\n" +
                "AnimationController methods should not be used after calling dispose."
            );
            _direction = _AnimationDirection.forward;
            if (from != null) {
                setValue(from.Value);
            }

            return _animateToInternal(upperBound);
        }

        public TickerFuture reverse(float? from = null) {
            D.assert(() => {
                if (duration == null && reverseDuration == null) {
                    throw new UIWidgetsError(
                        "AnimationController.reverse() called with no default duration or reverseDuration.\n" +
                        "The \"duration\" or \"reverseDuration\" property should be set, either in the constructor or later, before " +
                        "calling the reverse() function."
                    );
                }

                return true;
            });
            D.assert(
                _ticker != null,
                () => "AnimationController.reverse() called after AnimationController.dispose()\n" +
                "AnimationController methods should not be used after calling dispose."
            );
            _direction = _AnimationDirection.reverse;
            if (from != null) {
                setValue(from.Value);
            }

            return _animateToInternal(lowerBound);
        }

        public TickerFuture animateTo(float target, TimeSpan? duration = null, Curve curve = null) {
            D.assert(
                _ticker != null,
                () => "AnimationController.animateTo() called after AnimationController.dispose()\n" +
                "AnimationController methods should not be used after calling dispose."
            );
            curve = curve ?? Curves.linear;

            _direction = _AnimationDirection.forward;
            return _animateToInternal(target, duration: duration, curve: curve);
        }

        public TickerFuture animateBack(float target, TimeSpan? duration, Curve curve = null) {
            D.assert(
                _ticker != null,
                () => "AnimationController.animateBack() called after AnimationController.dispose()\n" +
                "AnimationController methods should not be used after calling dispose."
            );
            curve = curve ?? Curves.linear;
            _direction = _AnimationDirection.reverse;
            return _animateToInternal(target, duration, curve);
        }

        TickerFuture _animateToInternal(float target, TimeSpan? duration = null, Curve curve = null) {
            curve = curve ?? Curves.linear;

            TimeSpan? simulationDuration = duration;
            if (simulationDuration == null) {
                D.assert(() => {
                    if ((this.duration == null && _direction == _AnimationDirection.reverse && reverseDuration == null) || 
                        (this.duration == null && _direction == _AnimationDirection.forward)) {
                        throw new UIWidgetsError(
                            "AnimationController.animateTo() called with no explicit Duration and no default duration or reverseDuration.\n" +
                            "Either the \"duration\" argument to the animateTo() method should be provided, or the " +
                            "\"duration\" and/or \"reverseDuration\" property should be set, either in the constructor or later, before " +
                            "calling the animateTo() function."
                        );
                    }

                    return true;
                });
                float range = upperBound - lowerBound;
                float remainingFraction = range.isFinite() ? (target - _value).abs() / range : 1.0f;
                TimeSpan directionDuration = (_direction == _AnimationDirection.reverse && reverseDuration != null)
                    ? reverseDuration.Value
                    : this.duration.Value;
                
                simulationDuration = TimeSpan.FromTicks((long) (directionDuration.Ticks * remainingFraction));
            }
            else if (target == value) {
                simulationDuration = TimeSpan.Zero;
            }

            stop();

            if (simulationDuration == TimeSpan.Zero) {
                if (_value != target) {
                    _value = target.clamp(lowerBound, upperBound);
                    notifyListeners();
                }

                _status = (_direction == _AnimationDirection.forward)
                    ? AnimationStatus.completed
                    : AnimationStatus.dismissed;
                _checkStatusChanged();
                return TickerFuture.complete();
            }

            D.assert(simulationDuration > TimeSpan.Zero);
            D.assert(!isAnimating);
            return _startSimulation(
                new _InterpolationSimulation(_value, target, simulationDuration.Value, curve));
        }

        public TickerFuture repeat(float? min = null, float? max = null, bool reverse = false,
            TimeSpan? period = null) {
            min = min ?? lowerBound;
            max = max ?? upperBound;
            period = period ?? duration;
            D.assert(() => {
                if (period == null) {
                    throw new UIWidgetsError(
                        "AnimationController.repeat() called without an explicit period and with no default Duration.\n" +
                        "Either the \"period\" argument to the repeat() method should be provided, or the " +
                        "\"duration\" property should be set, either in the constructor or later, before " +
                        "calling the repeat() function."
                    );
                }

                return true;
            });

            D.assert(max >= min);
            D.assert(max <= upperBound && min >= lowerBound);
            stop();
            return _startSimulation(new _RepeatingSimulation(_value, min.Value, max.Value, reverse, period.Value, _directionSetter));
        }

        void _directionSetter(_AnimationDirection direction) {
            _direction = direction;
            _status = (_direction == _AnimationDirection.forward) ? AnimationStatus.forward : AnimationStatus.reverse;
            _checkStatusChanged();
        }

        public TickerFuture fling(float velocity = 1.0f) {
            _direction = velocity < 0.0 ? _AnimationDirection.reverse : _AnimationDirection.forward;
            float target = velocity < 0.0f
                ? lowerBound - _kFlingTolerance.distance
                : upperBound + _kFlingTolerance.distance;
            Simulation simulation = new SpringSimulation(_kFlingSpringDescription, value,
                target, velocity);
            simulation.tolerance = _kFlingTolerance;
            stop();
            return _startSimulation(simulation);
        }


        public TickerFuture animateWith(Simulation simulation) {
            D.assert(
                _ticker != null,
                () => "AnimationController.animateWith() called after AnimationController.dispose()\n" +
                "AnimationController methods should not be used after calling dispose."
            );
            stop();
            _direction = _AnimationDirection.forward;
            return _startSimulation(simulation);
        }

        TickerFuture _startSimulation(Simulation simulation) {
            D.assert(simulation != null);
            D.assert(!isAnimating);
            _simulation = simulation;
            _lastElapsedDuration = TimeSpan.Zero;
            _value = simulation.x(0.0f).clamp(lowerBound, upperBound);
            var result = _ticker.start();
            _status = (_direction == _AnimationDirection.forward)
                ? AnimationStatus.forward
                : AnimationStatus.reverse;
            _checkStatusChanged();
            return result;
        }

        public void stop(bool canceled = true) {
            D.assert(
                _ticker != null,
                () => "AnimationController.stop() called after AnimationController.dispose()\n" +
                "AnimationController methods should not be used after calling dispose."
            );
            _simulation = null;
            _lastElapsedDuration = null;
            _ticker.stop(canceled: canceled);
        }

        public override void dispose() {
            D.assert(() => {
                if (_ticker == null) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>() {
                        new ErrorSummary("AnimationController.dispose() called more than once."),
                        new ErrorDescription($"A given {GetType()} cannot be disposed more than once.\n"),
                        new DiagnosticsProperty<AnimationController>(
                            $"The following {GetType()} object was disposed multiple times",
                            this,
                            style: DiagnosticsTreeStyle.errorProperty)
                    });
                }

                return true;
            });
            _ticker.Dispose();
            _ticker = null;
            base.dispose();
        }

        AnimationStatus _lastReportedStatus = AnimationStatus.dismissed;

        void _checkStatusChanged() {
            AnimationStatus newStatus = status;
            if (_lastReportedStatus != newStatus) {
                _lastReportedStatus = newStatus;
                notifyStatusListeners(newStatus);
            }
        }

        void _tick(TimeSpan elapsed) {
            _lastElapsedDuration = elapsed;
            float elapsedInSeconds = (float) elapsed.Ticks / TimeSpan.TicksPerSecond;
            D.assert(elapsedInSeconds >= 0.0);
            _value = _simulation.x(elapsedInSeconds).clamp(lowerBound, upperBound);
            if (_simulation.isDone(elapsedInSeconds)) {
                _status = (_direction == _AnimationDirection.forward)
                    ? AnimationStatus.completed
                    : AnimationStatus.dismissed;
                stop(canceled: false);
            }

            notifyListeners();
            _checkStatusChanged();
        }

        public override string toStringDetails() {
            string paused = isAnimating ? "" : "; paused";
            string ticker = _ticker == null ? "; DISPOSED" : (_ticker.muted ? "; silenced" : "");
            string label = debugLabel == null ? "" : "; for " + debugLabel;
            string more = $"{base.toStringDetails()} {value:F3}";
            return more + paused + ticker + label;
        }

        static readonly SpringDescription _kFlingSpringDescription = SpringDescription.withDampingRatio(
            mass: 1.0f,
            stiffness: 500.0f,
            ratio: 1.0f
        );

        static readonly Tolerance _kFlingTolerance = new Tolerance(
            velocity: float.PositiveInfinity,
            distance: 0.01f
        );
    }


    class _InterpolationSimulation : Simulation {
        internal _InterpolationSimulation(float begin, float end, TimeSpan duration, Curve curve) {
            _begin = begin;
            _end = end;
            _curve = curve;

            D.assert(duration.Ticks > 0);
            _durationInSeconds = (float) duration.Ticks / TimeSpan.TicksPerSecond;
        }

        readonly float _durationInSeconds;
        readonly float _begin;
        readonly float _end;
        readonly Curve _curve;

        public override float x(float timeInSeconds) {
            float t = (timeInSeconds / _durationInSeconds).clamp(0.0f, 1.0f);
            if (t == 0.0f) {
                return _begin;
            }
            else if (t == 1.0f) {
                return _end;
            }
            else {
                return _begin + (_end - _begin) * _curve.transform(t);
            }
        }

        public override float dx(float timeInSeconds) {
            float epsilon = tolerance.time;
            return (x(timeInSeconds + epsilon) - x(timeInSeconds - epsilon)) / (2 * epsilon);
        }

        public override bool isDone(float timeInSeconds) {
            return timeInSeconds > _durationInSeconds;
        }
    }

    delegate void _DirectionSetter(_AnimationDirection direction);

    class _RepeatingSimulation : Simulation {
        internal _RepeatingSimulation(float initialValue, float min, float max, bool reverse, TimeSpan period, _DirectionSetter directionSetter) {
            _min = min;
            _max = max;
            _periodInSeconds = (float) period.Ticks / TimeSpan.TicksPerSecond;
            _initialT =
                (max == min) ? 0.0f : (initialValue / (max - min)) * (period.Ticks / TimeSpan.TicksPerSecond);
            _reverse = reverse;
            this.directionSetter = directionSetter;
            D.assert(_periodInSeconds > 0.0f);
            D.assert(_initialT >= 0.0f);
        }

        readonly float _min;
        readonly float _max;
        readonly float _periodInSeconds;
        readonly bool _reverse;
        readonly float _initialT;
        readonly _DirectionSetter directionSetter;

        public override float x(float timeInSeconds) {
            D.assert(timeInSeconds >= 0.0f);
            float totalTimeInSeconds = timeInSeconds + _initialT;
            float t = (totalTimeInSeconds / _periodInSeconds) % 1.0f;
            bool _isPlayingReverse = ((int) (totalTimeInSeconds / _periodInSeconds)) % 2 == 1;

            if (_reverse && _isPlayingReverse) {
                directionSetter(_AnimationDirection.reverse);
                return MathUtils.lerpNullableFloat(_max, _min, t);
            }
            else {
                directionSetter(_AnimationDirection.forward);
                return MathUtils.lerpNullableFloat(_min, _max, t);
            }
        }

        public override float dx(float timeInSeconds) {
            return (_max - _min) / _periodInSeconds;
        }

        public override bool isDone(float timeInSeconds) {
            return false;
        }
    }
}