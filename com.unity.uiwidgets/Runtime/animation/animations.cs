using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.animation {
    
    class _AlwaysCompleteAnimation : Animation<float> {
        internal _AlwaysCompleteAnimation() {
        }

        public override void addListener(VoidCallback listener) {
        }

        public override void removeListener(VoidCallback listener) {
        }

        public override void addStatusListener(AnimationStatusListener listener) {
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
        }

        public override AnimationStatus status {
            get { return AnimationStatus.completed; }
        }


        public override float value {
            get { return 1.0f; }
        }

        public override string ToString() {
            return "kAlwaysCompleteAnimation";
        }
    }

    class _AlwaysDismissedAnimation : Animation<float> {
        internal _AlwaysDismissedAnimation() {
        }

        public override void addListener(VoidCallback listener) {
        }

        public override void removeListener(VoidCallback listener) {
        }

        public override void addStatusListener(AnimationStatusListener listener) {
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
        }

        public override AnimationStatus status {
            get { return AnimationStatus.dismissed; }
        }


        public override float value {
            get { return 0.0f; }
        }

        public override string ToString() {
            return "kAlwaysDismissedAnimation";
        }
    }

    public class AlwaysStoppedAnimation<T> : Animation<T> {
        public AlwaysStoppedAnimation(T value) {
            _value = value;
        }

        public override T value {
            get { return _value; }
        }

        readonly T _value;

        public override void addListener(VoidCallback listener) {
        }

        public override void removeListener(VoidCallback listener) {
        }

        public override void addStatusListener(AnimationStatusListener listener) {
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
        }

        public override AnimationStatus status {
            get { return AnimationStatus.forward; }
        }

        public override string toStringDetails() {
            return $"{base.toStringDetails()} {value}; paused";
        }
    }

    public abstract class AnimationWithParentMixin<TParent, T> : Animation<T> {
        public abstract Animation<TParent> parent { get; }

        public override void addListener(VoidCallback listener) {
            parent.addListener(listener);
        }

        public override void removeListener(VoidCallback listener) {
            parent.removeListener(listener);
        }

        public override void addStatusListener(AnimationStatusListener listener) {
            parent.addStatusListener(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            parent.removeStatusListener(listener);
        }

        public override AnimationStatus status {
            get { return parent.status; }
        }
    }

    public class ProxyAnimation :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<float> {
        public ProxyAnimation(Animation<float> animation = null) {
            _parent = animation;
            if (_parent == null) {
                _status = AnimationStatus.dismissed;
                _value = 0.0f;
            }
        }

        AnimationStatus _status;

        float _value;

        public Animation<float> parent {
            get { return _parent; }
            set {
                if (value == _parent) {
                    return;
                }

                if (_parent != null) {
                    _status = _parent.status;
                    _value = _parent.value;
                    if (isListening) {
                        didStopListening();
                    }
                }

                _parent = value;
                if (_parent != null) {
                    if (isListening) {
                        didStartListening();
                    }

                    if (_value != _parent.value) {
                        notifyListeners();
                    }

                    if (_status != _parent.status) {
                        notifyStatusListeners(_parent.status);
                    }

                    _status = AnimationStatus.dismissed;
                    _value = 0;
                }
            }
        }

        Animation<float> _parent;

        protected override void didStartListening() {
            if (_parent != null) {
                _parent.addListener(notifyListeners);
                _parent.addStatusListener(notifyStatusListeners);
            }
        }

        protected override void didStopListening() {
            if (_parent != null) {
                _parent.removeListener(notifyListeners);
                _parent.removeStatusListener(notifyStatusListeners);
            }
        }

        public override AnimationStatus status {
            get { return _parent != null ? _parent.status : _status; }
        }

        public override float value {
            get { return _parent != null ? _parent.value : _value; }
        }

        public override string ToString() {
            if (parent == null) {
                return $"{GetType()}(null; {toStringDetails()} {value:F3}";
            }

            return $"{parent}\u27A9{GetType()}";
        }
    }


    public class ReverseAnimation : AnimationLocalStatusListenersMixinAnimationLazyListenerMixinAnimation<float> {
        public ReverseAnimation(Animation<float> parent) {
            D.assert(parent != null);
            _parent = parent;
        }

        public Animation<float> parent {
            get { return _parent; }
        }

        readonly Animation<float> _parent;

        public override void addListener(VoidCallback listener) {
            didRegisterListener();
            parent.addListener(listener);
        }

        public override void removeListener(VoidCallback listener) {
            parent.removeListener(listener);
            didUnregisterListener();
        }

        protected override void didStartListening() {
            parent.addStatusListener(_statusChangeHandler);
        }

        protected override void didStopListening() {
            parent.removeStatusListener(_statusChangeHandler);
        }

        void _statusChangeHandler(AnimationStatus status) {
            notifyStatusListeners(_reverseStatus(status));
        }

        public override AnimationStatus status {
            get { return _reverseStatus(parent.status); }
        }

        public override float value {
            get { return 1.0f - parent.value; }
        }

        AnimationStatus _reverseStatus(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.forward: return AnimationStatus.reverse;
                case AnimationStatus.reverse: return AnimationStatus.forward;
                case AnimationStatus.completed: return AnimationStatus.dismissed;
                case AnimationStatus.dismissed: return AnimationStatus.completed;
            }

            D.assert(false);
            return default(AnimationStatus);
        }

        public override string ToString() {
            return parent + "\u27AA" + GetType();
        }
    }

    public class CurvedAnimation : AnimationWithParentMixin<float, float> {
        public CurvedAnimation(
            Animation<float> parent = null,
            Curve curve = null,
            Curve reverseCurve = null
        ) {
            D.assert(parent != null);
            D.assert(curve != null);
            _parent = parent;
            this.curve = curve;
            this.reverseCurve = reverseCurve;

            _updateCurveDirection(parent.status);
            parent.addStatusListener(_updateCurveDirection);
        }

        public override Animation<float> parent {
            get { return _parent; }
        }

        readonly Animation<float> _parent;

        public Curve curve;

        public Curve reverseCurve;

        AnimationStatus? _curveDirection;

        void _updateCurveDirection(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.dismissed:
                case AnimationStatus.completed:
                    _curveDirection = null;
                    break;
                case AnimationStatus.forward:
                    _curveDirection = _curveDirection ?? AnimationStatus.forward;
                    break;
                case AnimationStatus.reverse:
                    _curveDirection = _curveDirection ?? AnimationStatus.reverse;
                    break;
            }
        }

        bool _useForwardCurve {
            get {
                return reverseCurve == null ||
                       (_curveDirection ?? parent.status) != AnimationStatus.reverse;
            }
        }

        public override float value {
            get {
                Curve activeCurve = _useForwardCurve ? curve : reverseCurve;

                float t = parent.value;
                if (activeCurve == null) {
                    return t;
                }

                if (t == 0.0 || t == 1.0) {
                    D.assert(() => {
                        float transformedValue = activeCurve.transform(t);
                        float roundedTransformedValue = transformedValue.round();
                        if (roundedTransformedValue != t) {
                            throw new UIWidgetsError(
                                string.Format(
                                    "Invalid curve endpoint at {0}.\n" +
                                    "Curves must map 0.0 to near zero and 1.0 to near one but " +
                                    "{1} mapped {0} to {2}, which " +
                                    "is near {3}.",
                                    t, activeCurve.GetType(), transformedValue, roundedTransformedValue)
                            );
                        }

                        return true;
                    });
                    return t;
                }

                return activeCurve.transform(t);
            }
        }

        public override string ToString() {
            if (reverseCurve == null) {
                return parent + "\u27A9" + curve;
            }

            if (_useForwardCurve) {
                return parent + "\u27A9" + curve + "\u2092\u2099/" + reverseCurve;
            }

            return parent + "\u27A9" + curve + "/" + reverseCurve + "\u2092\u2099";
        }
    }

    enum _TrainHoppingMode {
        minimize,
        maximize
    }

    public class TrainHoppingAnimation :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<float> {
        public TrainHoppingAnimation(
            Animation<float> currentTrain = null,
            Animation<float> nextTrain = null,
            VoidCallback onSwitchedTrain = null) {
            D.assert(currentTrain != null);
            _currentTrain = currentTrain;
            _nextTrain = nextTrain;
            this.onSwitchedTrain = onSwitchedTrain;

            if (_nextTrain != null) {
                if (_currentTrain.value > _nextTrain.value) {
                    _mode = _TrainHoppingMode.maximize;
                }
                else {
                    _mode = _TrainHoppingMode.minimize;
                    if (_currentTrain.value == _nextTrain.value) {
                        _currentTrain = _nextTrain;
                        _nextTrain = null;
                    }
                }
            }

            _currentTrain.addStatusListener(_statusChangeHandler);
            _currentTrain.addListener(_valueChangeHandler);
            if (_nextTrain != null) {
                _nextTrain.addListener(_valueChangeHandler);
            }
        }

        public Animation<float> currentTrain {
            get { return _currentTrain; }
        }

        Animation<float> _currentTrain;
        Animation<float> _nextTrain;
        _TrainHoppingMode _mode;

        public VoidCallback onSwitchedTrain;

        AnimationStatus? _lastStatus;

        void _statusChangeHandler(AnimationStatus status) {
            D.assert(_currentTrain != null);

            if (status != _lastStatus) {
                notifyListeners();
                _lastStatus = status;
            }

            D.assert(_lastStatus != null);
        }

        public override AnimationStatus status {
            get { return _currentTrain.status; }
        }

        float? _lastValue;

        void _valueChangeHandler() {
            D.assert(_currentTrain != null);

            bool hop = false;
            if (_nextTrain != null) {
                switch (_mode) {
                    case _TrainHoppingMode.minimize:
                        hop = _nextTrain.value <= _currentTrain.value;
                        break;
                    case _TrainHoppingMode.maximize:
                        hop = _nextTrain.value >= _currentTrain.value;
                        break;
                }

                if (hop) {
                    _currentTrain.removeStatusListener(_statusChangeHandler);
                    _currentTrain.removeListener(_valueChangeHandler);
                    _currentTrain = _nextTrain;
                    _nextTrain = null;
                    _currentTrain.addStatusListener(_statusChangeHandler);
                    _statusChangeHandler(_currentTrain.status);
                }
            }

            float newValue = value;
            if (newValue != _lastValue) {
                notifyListeners();
                _lastValue = newValue;
            }

            D.assert(_lastValue != null);

            if (hop && onSwitchedTrain != null) {
                onSwitchedTrain();
            }
        }

        public override float value {
            get { return _currentTrain.value; }
        }

        public override void dispose() {
            D.assert(_currentTrain != null);

            _currentTrain.removeStatusListener(_statusChangeHandler);
            _currentTrain.removeListener(_valueChangeHandler);
            _currentTrain = null;

            if (_nextTrain != null) {
                _nextTrain.removeListener(_valueChangeHandler);
                _nextTrain = null;
            }
            base.dispose();
        }

        public override string ToString() {
            if (_nextTrain != null) {
                return $"{currentTrain}\u27A9{GetType()}(next: {_nextTrain})";
            }

            return $"{currentTrain}\u27A9{GetType()}(no next)";
        }
    }

    public abstract class CompoundAnimation<T> :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T> {
        public CompoundAnimation(
            Animation<T> first = null,
            Animation<T> next = null
        ) {
            D.assert(first != null);
            D.assert(next != null);
            this.first = first;
            this.next = next;
        }

        public readonly Animation<T> first;

        public readonly Animation<T> next;

        protected override void didStartListening() {
            first.addListener(_maybeNotifyListeners);
            first.addStatusListener(_maybeNotifyStatusListeners);
            next.addListener(_maybeNotifyListeners);
            next.addStatusListener(_maybeNotifyStatusListeners);
        }

        protected override void didStopListening() {
            first.removeListener(_maybeNotifyListeners);
            first.removeStatusListener(_maybeNotifyStatusListeners);
            next.removeListener(_maybeNotifyListeners);
            next.removeStatusListener(_maybeNotifyStatusListeners);
        }

        public override AnimationStatus status {
            get {
                if (next.status == AnimationStatus.forward || next.status == AnimationStatus.reverse) {
                    return next.status;
                }

                return first.status;
            }
        }

        public override string ToString() {
            return $"{GetType()}({first}, {next})";
        }

        AnimationStatus _lastStatus;

        void _maybeNotifyStatusListeners(AnimationStatus _) {
            if (status != _lastStatus) {
                _lastStatus = status;
                notifyStatusListeners(status);
            }
        }

        T _lastValue;

        void _maybeNotifyListeners() {
            if (!Equals(value, _lastValue)) {
                _lastValue = value;
                notifyListeners();
            }
        }
    }

    public class AnimationMean : CompoundAnimation<float> {
        public AnimationMean(
            Animation<float> left = null,
            Animation<float> right = null
        ) : base(first: left, next: right) {
        }

        public override float value {
            get { return (first.value + next.value) / 2.0f; }
        }
    }

    public class AnimationMax : CompoundAnimation<float> {
        public AnimationMax(
            Animation<float> left = null,
            Animation<float> right = null
        ) : base(first: left, next: right) {
        }

        public override float value {
            get { return Mathf.Max(first.value, next.value); }
        }
    }

    public class AnimationMin : CompoundAnimation<float> {
        public AnimationMin(
            Animation<float> left = null,
            Animation<float> right = null
        ) : base(first: left, next: right) {
        }

        public override float value {
            get { return Mathf.Min(first.value, next.value); }
        }
    }

    public static class Animations {
        public static readonly Animation<float> kAlwaysCompleteAnimation = new _AlwaysCompleteAnimation();

        public static readonly Animation<float> kAlwaysDismissedAnimation = new _AlwaysDismissedAnimation();
    }
}