using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void GestureDoubleTapCallback();

    public delegate void GestureMultiTapDownCallback(int pointer, TapDownDetails details);

    public delegate void GestureMultiTapUpCallback(int pointer, TapUpDetails details);

    public delegate void GestureMultiTapCallback(int pointer);

    public delegate void GestureMultiTapCancelCallback(int pointer);

    class _CountdownZoned {
        public _CountdownZoned(TimeSpan duration) {
            D.assert(duration != null);
            _timer = Timer.create(duration, _onTimeout);
        }

        public bool _timeout = false;
        public Timer _timer;

        public bool timeout {
            get { return _timeout; }
        }

        void _onTimeout() {
            _timeout = true;
        }
    }

    public class DoubleTapDetails {
        public DoubleTapDetails(Offset firstGlobalPosition = null) {
            this.firstGlobalPosition = firstGlobalPosition ?? Offset.zero;
        }

        public readonly Offset firstGlobalPosition;
    }

    class _TapTracker {
        internal _TapTracker(
            PointerDownEvent evt,
            TimeSpan doubleTapMinTime,
            GestureArenaEntry entry = null
        ) {
            D.assert(evt != null);
            pointer = evt.pointer;
            _initialGlobalPosition = evt.position;
            initialButtons = evt.buttons;
            _doubleTapMinTimeCountdown = new _CountdownZoned(duration: doubleTapMinTime);
            this.entry = entry;
        }

        public readonly int pointer;
        public readonly GestureArenaEntry entry;
        internal readonly Offset _initialGlobalPosition;
        internal readonly int initialButtons;
        internal readonly _CountdownZoned _doubleTapMinTimeCountdown;

        bool _isTrackingPointer = false;

        public void startTrackingPointer(PointerRoute route, Matrix4 transform) {
            if (!_isTrackingPointer) {
                _isTrackingPointer = true;
                GestureBinding.instance.pointerRouter.addRoute(pointer, route, transform);
            }
        }

        public virtual void stopTrackingPointer(PointerRoute route) {
            if (_isTrackingPointer) {
                _isTrackingPointer = false;
                GestureBinding.instance.pointerRouter.removeRoute(pointer, route);
            }
        }

        public bool isWithinGlobalTolerance(PointerEvent evt, float tolerance) {
            Offset offset = evt.position - _initialGlobalPosition;
            return offset.distance <= tolerance;
        }

        public bool hasElapsedMinTime() {
            return _doubleTapMinTimeCountdown.timeout;
        }

        public bool hasSameButton(PointerDownEvent evt) {
            return evt.buttons == initialButtons;
        }
    }


    public class DoubleTapGestureRecognizer : GestureRecognizer {
        public DoubleTapGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null)
            : base(debugOwner: debugOwner, kind: kind) {
        }

        public GestureDoubleTapCallback onDoubleTap;

        protected override bool isPointerAllowed(PointerDownEvent evt) {
            if (_firstTap == null) {
                switch (evt.buttons) {
                    case gesture_.kPrimaryButton:
                        if (onDoubleTap == null)
                            return false;
                        break;
                    default:
                        return false;
                }
            }

            return base.isPointerAllowed(evt as PointerDownEvent);
        }

        Timer _doubleTapTimer;
        _TapTracker _firstTap;
        readonly Dictionary<int, _TapTracker> _trackers = new Dictionary<int, _TapTracker>();

        public override void addAllowedPointer(PointerEvent evt) {
            if (_firstTap != null) {
                if (!_firstTap.isWithinGlobalTolerance(evt, Constants.kDoubleTapSlop)) {
                    // Ignore out-of-bounds second taps.
                    return;
                }
                else if (!_firstTap.hasElapsedMinTime() ||
                         !_firstTap.hasSameButton(evt as PointerDownEvent)) {
                    // Restart when the second tap is too close to the first, or when buttons
                    // mismatch.
                    _reset();
                    _trackFirstTap(evt);
                    return;
                }
            }

            _trackFirstTap(evt);
        }

        void _trackFirstTap(PointerEvent evt) {
            _stopDoubleTapTimer();
            _TapTracker tracker = new _TapTracker(
                evt: evt as PointerDownEvent,
                entry: GestureBinding.instance.gestureArena.add(evt.pointer, this),
                doubleTapMinTime: Constants.kDoubleTapMinTime
            );
            _trackers[evt.pointer] = tracker;
            tracker.startTrackingPointer(_handleEvent, evt.transform);
        }

        void _handleEvent(PointerEvent evt) {
            _TapTracker tracker = _trackers[evt.pointer];
            D.assert(tracker != null);
            if (evt is PointerUpEvent) {
                if (_firstTap == null) {
                    _registerFirstTap(tracker);
                }
                else {
                    _registerSecondTap(tracker);
                }
            }
            else if (evt is PointerMoveEvent) {
                if (!tracker.isWithinGlobalTolerance(evt, Constants.kDoubleTapTouchSlop)) {
                    _reject(tracker);
                }
            }
            else if (evt is PointerCancelEvent) {
                _reject(tracker);
            }
        }

        public override void acceptGesture(int pointer) {
        }

        public override void rejectGesture(int pointer) {
            _TapTracker tracker;
            _trackers.TryGetValue(pointer, out tracker);

            if (tracker == null &&
                _firstTap != null &&
                _firstTap.pointer == pointer) {
                tracker = _firstTap;
            }

            if (tracker != null) {
                _reject(tracker);
            }
        }

        void _reject(_TapTracker tracker) {
            _trackers.Remove(tracker.pointer);
            tracker.entry.resolve(GestureDisposition.rejected);
            _freezeTracker(tracker);
            if (_firstTap != null &&
                (_trackers.isEmpty() || tracker == _firstTap)) {
                _reset();
            }
        }

        public override void dispose() {
            _reset();
            base.dispose();
        }

        void _reset() {
            _stopDoubleTapTimer();
            if (_firstTap != null) {
                _TapTracker tracker = _firstTap;
                _firstTap = null;
                _reject(tracker);
                GestureBinding.instance.gestureArena.release(tracker.pointer);
            }

            _clearTrackers();
        }

        void _registerFirstTap(_TapTracker tracker) {
            _startDoubleTapTimer();
            GestureBinding.instance.gestureArena.hold(tracker.pointer);
            _freezeTracker(tracker);
            _trackers.Remove(tracker.pointer);
            _clearTrackers();
            _firstTap = tracker;
        }

        void _registerSecondTap(_TapTracker tracker) {
            _firstTap.entry.resolve(GestureDisposition.accepted);
            tracker.entry.resolve(GestureDisposition.accepted);
            _freezeTracker(tracker);
            _trackers.Remove(tracker.pointer);
            _checkUp(tracker.initialButtons);

            _reset();
        }

        void _clearTrackers() {
            foreach (var tracker in _trackers.Values) {
                _reject(tracker);
            }

            D.assert(_trackers.isEmpty());
        }

        void _freezeTracker(_TapTracker tracker) {
            tracker.stopTrackingPointer(_handleEvent);
        }

        void _startDoubleTapTimer() {
            _doubleTapTimer =
                _doubleTapTimer
                ?? Timer.create(Constants.kDoubleTapTimeout, _reset);
        }

        void _stopDoubleTapTimer() {
            if (_doubleTapTimer != null) {
                _doubleTapTimer.cancel();
                _doubleTapTimer = null;
            }
        }

        void _checkUp(int buttons) {
            D.assert(buttons == gesture_.kPrimaryButton);
            if (onDoubleTap != null) {
                invokeCallback("onDoubleTap",() => onDoubleTap());
            }
        }

        public override string debugDescription {
            get { return "double tap"; }
        }
    }

    class _TapGesture : _TapTracker {
        public _TapGesture(
            MultiTapGestureRecognizer gestureRecognizer,
            PointerEvent evt,
            TimeSpan longTapDelay
        ) : base(
            evt: (PointerDownEvent) evt,
            entry: GestureBinding.instance.gestureArena.add(evt.pointer, gestureRecognizer),
            doubleTapMinTime: Constants.kDoubleTapMinTime
        ) {
            this.gestureRecognizer = gestureRecognizer;
            _lastPosition = OffsetPair.fromEventPosition(evt);
            startTrackingPointer(handleEvent, evt.transform);
            if (longTapDelay > TimeSpan.Zero) {
                _timer = Timer.create(longTapDelay, () => {
                    _timer = null;
                    this.gestureRecognizer._dispatchLongTap(evt.pointer, _lastPosition);
                });
            }
        }

        public readonly MultiTapGestureRecognizer gestureRecognizer;

        bool _wonArena = false;
        Timer _timer;

        OffsetPair _lastPosition;
        OffsetPair _finalPosition;

        void handleEvent(PointerEvent evt) {
            D.assert(evt.pointer == pointer);
            if (evt is PointerMoveEvent) {
                if (!isWithinGlobalTolerance(evt, Constants.kTouchSlop)) {
                    cancel();
                }
                else {
                    _lastPosition = OffsetPair.fromEventPosition(evt);
                }
            }
            else if (evt is PointerCancelEvent) {
                cancel();
            }
            else if (evt is PointerUpEvent) {
                stopTrackingPointer(handleEvent);
                _finalPosition = OffsetPair.fromEventPosition(evt);
                _check();
            }
        }

        public override void stopTrackingPointer(PointerRoute route) {
            _timer?.cancel();
            _timer = null;
            base.stopTrackingPointer(route);
        }

        public void accept() {
            _wonArena = true;
            _check();
        }

        public void reject() {
            stopTrackingPointer(handleEvent);
            gestureRecognizer._dispatchCancel(pointer);
        }

        public void cancel() {
            if (_wonArena) {
                reject();
            }
            else {
                entry.resolve(GestureDisposition.rejected);
            }
        }

        void _check() {
            if (_wonArena && _finalPosition != null) {
                gestureRecognizer._dispatchTap(pointer, _finalPosition);
            }
        }
    }

    public class MultiTapGestureRecognizer : GestureRecognizer {
        public MultiTapGestureRecognizer(
            TimeSpan? longTapDelay = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null
        ) : base(debugOwner: debugOwner, kind: kind) {
            this.longTapDelay = longTapDelay ?? TimeSpan.Zero;
        }

        public GestureMultiTapDownCallback onTapDown;

        public GestureMultiTapUpCallback onTapUp;

        public GestureMultiTapCallback onTap;

        public GestureMultiTapCancelCallback onTapCancel;

        public TimeSpan longTapDelay;

        public GestureMultiTapDownCallback onLongTapDown;

        readonly Dictionary<int, _TapGesture> _gestureMap = new Dictionary<int, _TapGesture>();

        public override void addAllowedPointer(PointerEvent evt) {
            D.assert(!_gestureMap.ContainsKey(evt.pointer));
            _gestureMap[evt.pointer] = new _TapGesture(
                gestureRecognizer: this,
                evt: evt,
                longTapDelay: longTapDelay
            );
            if (onTapDown != null) {
                invokeCallback<object>("onTapDown", () => {
                    onTapDown(evt.pointer, new TapDownDetails(globalPosition: evt.position,
                        localPosition: evt.localPosition,
                        kind: evt.kind
                    ));
                    return null;
                });
            }
        }

        public override void acceptGesture(int pointer) {
            D.assert(_gestureMap.ContainsKey(pointer));
            _gestureMap[pointer].accept();
        }

        public override void rejectGesture(int pointer) {
            D.assert(_gestureMap.ContainsKey(pointer));
            _gestureMap[pointer].reject();
            D.assert(!_gestureMap.ContainsKey(pointer));
        }

        public void _dispatchCancel(int pointer) {
            D.assert(_gestureMap.ContainsKey(pointer));
            _gestureMap.Remove(pointer);
            if (onTapCancel != null) {
                invokeCallback<object>("onTapCancel", () => {
                    onTapCancel(pointer);
                    return null;
                });
            }
        }

        public void _dispatchTap(int pointer, OffsetPair position) {
            D.assert(_gestureMap.ContainsKey(pointer));
            _gestureMap.Remove(pointer);
            if (onTapUp != null) {
                invokeCallback<object>("onTapUp",
                    () => {
                        onTapUp(pointer, new TapUpDetails(
                            localPosition: position.local,
                            globalPosition: position.global
                        ));
                        return null;
                    });
            }

            if (onTap != null) {
                invokeCallback<object>("onTap", () => {
                    onTap(pointer);
                    return null;
                });
            }
        }

        public void _dispatchLongTap(int pointer, OffsetPair lastPosition) {
            D.assert(_gestureMap.ContainsKey(pointer));
            if (onLongTapDown != null) {
                invokeCallback<object>("onLongTapDown",
                    () => {
                        onLongTapDown(pointer, new TapDownDetails(
                            globalPosition: lastPosition.global,
                            localPosition: lastPosition.local
                        ));
                        return null;
                    });
            }
        }

        public override void dispose() {
            List<_TapGesture> localGestures = new List<_TapGesture>();
            foreach (var item in _gestureMap) {
                localGestures.Add(item.Value);
            }

            foreach (_TapGesture gesture in localGestures) {
                gesture.cancel();
            }

            D.assert(_gestureMap.isEmpty);
            base.dispose();
        }

        public override string debugDescription {
            get { return "multitap"; }
        }
    }

    delegate bool _RouteEntryPredicate(_RouteEntry entry);

    class _RouteEntry {
        public _RouteEntry(
            PointerRoute route,
            Matrix4 transform
        ) {
            this.route = route;
            this.transform = transform;
        }

        public readonly PointerRoute route;
        public readonly Matrix4 transform;

        static _RouteEntryPredicate isRoutePredicate(PointerRoute route) {
            return (_RouteEntry entry) => entry.route == route;
        }
    }
}