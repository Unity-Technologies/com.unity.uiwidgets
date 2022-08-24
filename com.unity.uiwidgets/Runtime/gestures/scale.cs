using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    enum _ScaleState {
        ready,
        possible,
        accepted,
        started
    }

    public class ScaleStartDetails {
        public ScaleStartDetails(
            Offset focalPoint = null,
            Offset localFocalPoint = null
        ) {
            this.focalPoint = focalPoint ?? Offset.zero;
            this.localFocalPoint = localFocalPoint ?? this.focalPoint;
        }

        public readonly Offset focalPoint;
        public readonly Offset localFocalPoint;

        public override string ToString() {
            return $"ScaleStartDetails(focalPoint: {focalPoint}, localFocalPoint: {localFocalPoint})";
        }
    }


    public class ScaleUpdateDetails {
        public ScaleUpdateDetails(
            Offset focalPoint = null,
            Offset localFocalPoint = null,
            float scale = 1.0f,
            float horizontalScale = 1.0f,
            float verticalScale = 1.0f,
            float rotation = 0.0f
        ) {
            focalPoint = focalPoint ?? Offset.zero;
            localFocalPoint = localFocalPoint ?? this.focalPoint;

            D.assert(scale >= 0.0f);
            D.assert(horizontalScale >= 0.0f);
            D.assert(verticalScale >= 0.0f);

            this.focalPoint = focalPoint;
            this.scale = scale;
            this.horizontalScale = horizontalScale;
            this.verticalScale = verticalScale;
            this.rotation = rotation;
        }

        public readonly Offset focalPoint;
        public readonly Offset localFocalPoint;

        public readonly float scale;

        public readonly float horizontalScale;

        public readonly float verticalScale;

        public readonly float rotation;

        public override string ToString() {
            return
                $"ScaleUpdateDetails(focalPoint: {focalPoint}, localFocalPoint: {localFocalPoint}, scale: {scale}, horizontalScale: {horizontalScale}, verticalScale: {verticalScale}, rotation: {rotation}";
        }
    }

    public class ScaleEndDetails {
        public ScaleEndDetails(Velocity velocity = null) {
            this.velocity = velocity ?? Velocity.zero;
        }

        public readonly Velocity velocity;

        public override string ToString() {
            return $"ScaleEndDetails(velocity: {velocity}";
        }
    }

    public delegate void GestureScaleStartCallback(ScaleStartDetails details);

    public delegate void GestureScaleUpdateCallback(ScaleUpdateDetails details);

    public delegate void GestureScaleEndCallback(ScaleEndDetails details);

    static class _ScaleGestureUtils {
        public static bool _isFlingGesture(Velocity velocity) {
            D.assert(velocity != null);
            float speedSquared = velocity.pixelsPerSecond.distanceSquared;
            return speedSquared > Constants.kMinFlingVelocity * Constants.kMinFlingVelocity;
        }
    }

    class _LineBetweenPointers {
        public _LineBetweenPointers(
            Offset pointerStartLocation = null,
            int pointerStartId = 0,
            Offset pointerEndLocation = null,
            int pointerEndId = 1) {
            pointerStartLocation = pointerStartLocation ?? Offset.zero;
            pointerEndLocation = pointerEndLocation ?? Offset.zero;

            D.assert(pointerStartId != pointerEndId);

            this.pointerStartLocation = pointerStartLocation;
            this.pointerStartId = pointerStartId;
            this.pointerEndLocation = pointerEndLocation;
            this.pointerEndId = pointerEndId;
        }

        public readonly Offset pointerStartLocation;

        public readonly int pointerStartId;

        public readonly Offset pointerEndLocation;

        public readonly int pointerEndId;
    }


    public class ScaleGestureRecognizer : OneSequenceGestureRecognizer {
        public ScaleGestureRecognizer(object debugOwner, PointerDeviceKind? kind = null) : base(debugOwner: debugOwner,
            kind: kind) {
        }

        public GestureScaleStartCallback onStart;

        public GestureScaleUpdateCallback onUpdate;

        public GestureScaleEndCallback onEnd;

        _ScaleState _state = _ScaleState.ready;

        Matrix4 _lastTransform;

        Offset _initialFocalPoint;
        Offset _currentFocalPoint;
        float _initialSpan;
        float _currentSpan;
        float _initialHorizontalSpan;
        float _currentHorizontalSpan;
        float _initialVerticalSpan;
        float _currentVerticalSpan;
        _LineBetweenPointers _initialLine;
        _LineBetweenPointers _currentLine;
        Dictionary<int, Offset> _pointerLocations;
        List<int> _pointerQueue;
        readonly Dictionary<int, VelocityTracker> _velocityTrackers = new Dictionary<int, VelocityTracker>();

        float _scaleFactor {
            get { return _initialSpan > 0.0f ? _currentSpan / _initialSpan : 1.0f; }
        }

        float _horizontalScaleFactor {
            get {
                return _initialHorizontalSpan > 0.0f
                    ? _currentHorizontalSpan / _initialHorizontalSpan
                    : 1.0f;
            }
        }

        float _verticalScaleFactor {
            get {
                return _initialVerticalSpan > 0.0f ? _currentVerticalSpan / _initialVerticalSpan : 1.0f;
            }
        }

        float _computeRotationFactor() {
            if (_initialLine == null || _currentLine == null) {
                return 0.0f;
            }

            float fx = _initialLine.pointerStartLocation.dx;
            float fy = _initialLine.pointerStartLocation.dy;
            float sx = _initialLine.pointerEndLocation.dx;
            float sy = _initialLine.pointerEndLocation.dy;

            float nfx = _currentLine.pointerStartLocation.dx;
            float nfy = _currentLine.pointerStartLocation.dy;
            float nsx = _currentLine.pointerEndLocation.dx;
            float nsy = _currentLine.pointerEndLocation.dy;

            float angle1 = Mathf.Atan2(fy - sy, fx - sx);
            float angle2 = Mathf.Atan2(nfy - nsy, nfx - nsx);

            return angle2 - angle1;
        }

        public override void addAllowedPointer(PointerEvent evt) {
            startTrackingPointer(evt.pointer, evt.transform);
            _velocityTrackers[evt.pointer] = new VelocityTracker();
            if (_state == _ScaleState.ready) {
                _state = _ScaleState.possible;
                _initialSpan = 0.0f;
                _currentSpan = 0.0f;
                _initialHorizontalSpan = 0.0f;
                _currentHorizontalSpan = 0.0f;
                _initialVerticalSpan = 0.0f;
                _currentVerticalSpan = 0.0f;
                _pointerLocations = new Dictionary<int, Offset>();
                _pointerQueue = new List<int>();
            }
        }

        protected override void handleEvent(PointerEvent evt) {
            D.assert(_state != _ScaleState.ready);
            bool didChangeConfiguration = false;
            bool shouldStartIfAccepted = false;

            if (evt is PointerMoveEvent) {
                VelocityTracker tracker = _velocityTrackers[evt.pointer];
                D.assert(tracker != null);
                if (!evt.synthesized) {
                    tracker.addPosition(evt.timeStamp, evt.position);
                }

                _pointerLocations[evt.pointer] = evt.position;
                shouldStartIfAccepted = true;      
                _lastTransform = evt.transform;
            }
            else if (evt is PointerDownEvent) {
                _pointerLocations[evt.pointer] = evt.position;
                _pointerQueue.Add(evt.pointer);
                didChangeConfiguration = true;
                shouldStartIfAccepted = true;
                _lastTransform = evt.transform;
            }
            else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                _pointerLocations.Remove(evt.pointer);
                _pointerQueue.Remove(evt.pointer);
                didChangeConfiguration = true;
                _lastTransform = evt.transform;
            }

            _updateLines();
            _update();

            if (!didChangeConfiguration || _reconfigure(evt.pointer)) {
                _advanceStateMachine(shouldStartIfAccepted);
            }

            stopTrackingIfPointerNoLongerDown(evt);
        }

        void _update() {
            int count = _pointerLocations.Keys.Count;

            Offset focalPoint = Offset.zero;
            foreach (int pointer in _pointerLocations.Keys) {
                focalPoint += _pointerLocations[pointer];
            }

            _currentFocalPoint = count > 0 ? focalPoint / count : Offset.zero;

            float totalDeviation = 0.0f;
            float totalHorizontalDeviation = 0.0f;
            float totalVerticalDeviation = 0.0f;

            foreach (int pointer in _pointerLocations.Keys) {
                totalDeviation += (_currentFocalPoint - _pointerLocations[pointer]).distance;
                totalHorizontalDeviation += (_currentFocalPoint.dx - _pointerLocations[pointer].dx).abs();
                totalVerticalDeviation += (_currentFocalPoint.dy - _pointerLocations[pointer].dy).abs();
            }

            _currentSpan = count > 0 ? totalDeviation / count : 0.0f;
            _currentHorizontalSpan = count > 0 ? totalHorizontalDeviation / count : 0.0f;
            _currentVerticalSpan = count > 0 ? totalVerticalDeviation / count : 0.0f;
        }

        void _updateLines() {
            int count = _pointerLocations.Keys.Count;
            D.assert(_pointerQueue.Count >= count);

            if (count < 2) {
                _initialLine = _currentLine;
            }
            else if (_initialLine != null &&
                     _initialLine.pointerStartId == _pointerQueue[0] &&
                     _initialLine.pointerEndId == _pointerQueue[1]) {
                _currentLine = new _LineBetweenPointers(
                    pointerStartId: _pointerQueue[0],
                    pointerStartLocation: _pointerLocations[_pointerQueue[0]],
                    pointerEndId: _pointerQueue[1],
                    pointerEndLocation: _pointerLocations[_pointerQueue[1]]
                );
            }
            else {
                _initialLine = new _LineBetweenPointers(
                    pointerStartId: _pointerQueue[0],
                    pointerStartLocation: _pointerLocations[_pointerQueue[0]],
                    pointerEndId: _pointerQueue[1],
                    pointerEndLocation: _pointerLocations[_pointerQueue[1]]
                );
                _currentLine = null;
            }
        }

        bool _reconfigure(int pointer) {
            _initialFocalPoint = _currentFocalPoint;
            _initialSpan = _currentSpan;
            _initialLine = _currentLine;
            _initialHorizontalSpan = _currentHorizontalSpan;
            _initialVerticalSpan = _currentVerticalSpan;
            if (_state == _ScaleState.started) {
                if (onEnd != null) {
                    VelocityTracker tracker = _velocityTrackers[pointer];
                    D.assert(tracker != null);

                    Velocity velocity = tracker.getVelocity();
                    if (_ScaleGestureUtils._isFlingGesture(velocity)) {
                        Offset pixelsPerSecond = velocity.pixelsPerSecond;
                        if (pixelsPerSecond.distanceSquared >
                            Constants.kMaxFlingVelocity * Constants.kMaxFlingVelocity) {
                            velocity = new Velocity(
                                pixelsPerSecond: (pixelsPerSecond / pixelsPerSecond.distance) *
                                                 Constants.kMaxFlingVelocity);
                        }

                        invokeCallback<object>("onEnd", () => {
                            onEnd(new ScaleEndDetails(velocity: velocity));
                            return null;
                        });
                    }
                    else {
                        invokeCallback<object>("onEnd", () => {
                            onEnd(new ScaleEndDetails(velocity: Velocity.zero));
                            return null;
                        });
                    }
                }

                _state = _ScaleState.accepted;
                return false;
            }

            return true;
        }

        void _advanceStateMachine(bool shouldStartIfAccepted) {
            if (_state == _ScaleState.ready) {
                _state = _ScaleState.possible;
            }

            if (_state == _ScaleState.possible) {
                float spanDelta = (_currentSpan - _initialSpan).abs();
                float focalPointDelta = (_currentFocalPoint - _initialFocalPoint).distance;
                if (spanDelta > Constants.kScaleSlop || focalPointDelta > Constants.kPanSlop) {
                    resolve(GestureDisposition.accepted);
                }
            }
            else if (_state >= _ScaleState.accepted) {
                resolve(GestureDisposition.accepted);
            }

            if (_state == _ScaleState.accepted && shouldStartIfAccepted) {
                _state = _ScaleState.started;
                _dispatchOnStartCallbackIfNeeded();
            }

            if (_state == _ScaleState.started && onUpdate != null) {
                invokeCallback<object>("onUpdate", () => {
                    onUpdate(new ScaleUpdateDetails(
                        scale: _scaleFactor,
                        horizontalScale: _horizontalScaleFactor,
                        verticalScale: _verticalScaleFactor,
                        focalPoint: _currentFocalPoint,
                        localFocalPoint: PointerEvent.transformPosition(_lastTransform, _currentFocalPoint),
                        rotation: _computeRotationFactor()
                    ));
                    return null;
                });
            }
        }

        void _dispatchOnStartCallbackIfNeeded() {
            D.assert(_state == _ScaleState.started);
            if (onStart != null) {
                invokeCallback<object>("onStart", () => {
                    onStart(new ScaleStartDetails(
                        focalPoint: _currentFocalPoint,
                        localFocalPoint: PointerEvent.transformPosition(_lastTransform, _currentFocalPoint)
                    ));
                    return null;
                });
            }
        }

        public override void acceptGesture(int pointer) {
            if (_state == _ScaleState.possible) {
                _state = _ScaleState.started;
                _dispatchOnStartCallbackIfNeeded();
            }
        }

        public override void rejectGesture(int pointer) {
            stopTrackingPointer(pointer);
        }

        protected override void didStopTrackingLastPointer(int pointer) {
            switch (_state) {
                case _ScaleState.possible:
                    resolve(GestureDisposition.rejected);
                    break;
                case _ScaleState.ready:
                    D.assert(false);
                    break;
                case _ScaleState.accepted:
                    break;
                case _ScaleState.started:
                    D.assert(false);
                    break;
            }

            _state = _ScaleState.ready;
        }

        public override void dispose() {
            _velocityTrackers.Clear();
            base.dispose();
        }

        public override string debugDescription {
            get { return "scale"; }
        }
    }
}