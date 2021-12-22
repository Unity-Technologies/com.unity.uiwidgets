using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    enum _ForceState {
        ready,
        possible,
        accepted,
        started,
        peaked,
    }

    public delegate void GestureForcePressStartCallback(ForcePressDetails details);

    public delegate void GestureForcePressPeakCallback(ForcePressDetails details);

    public delegate void GestureForcePressUpdateCallback(ForcePressDetails details);

    public delegate void GestureForcePressEndCallback(ForcePressDetails details);

    public delegate float GestureForceInterpolation(float pressureMin, float pressureMax, float pressure);

    public class ForcePressDetails {
        public ForcePressDetails(
            Offset globalPosition,
            float pressure,
            Offset localPosition = null
        ) {
            D.assert(globalPosition != null);
            this.localPosition = localPosition ?? globalPosition;
            this.globalPosition = globalPosition;
            this.pressure = pressure;
        }

        public readonly Offset globalPosition;
        public readonly Offset localPosition;
        public readonly float pressure;
    }

    public class ForcePressGestureRecognizer : OneSequenceGestureRecognizer {
        public ForcePressGestureRecognizer(
            object debugOwner = null,
            PointerDeviceKind kind = default,
            float startPressure = 0.4f,
            float peakPressure = 0.85f,
            GestureForceInterpolation interpolation = null )
            : base(debugOwner: debugOwner, kind: kind) {
            interpolation = interpolation ?? _inverseLerp;
            D.assert(peakPressure > startPressure);
            this.startPressure = startPressure;
            this.peakPressure = peakPressure;
            this.interpolation = interpolation;
           
        }

        public GestureForcePressStartCallback onStart;
        public GestureForcePressUpdateCallback onUpdate;
        public GestureForcePressPeakCallback onPeak;
        public GestureForcePressEndCallback onEnd;
        public readonly float startPressure;
        public readonly float peakPressure;
        public readonly GestureForceInterpolation interpolation;
        OffsetPair _lastPosition;
        float _lastPressure;
        _ForceState _state = _ForceState.ready;

        public override void addAllowedPointer(PointerEvent Event) {
            if (!(Event is PointerUpEvent) && Event.pressureMax <= 1.0f) {
                resolve(GestureDisposition.rejected);
            }
            else {
                startTrackingPointer(Event.pointer, Event.transform);
                if (_state == _ForceState.ready) {
                    _state = _ForceState.possible;
                    _lastPosition = OffsetPair.fromEventPosition(Event);
                }
            }
        }

        protected override void handleEvent(PointerEvent Event) {
            D.assert(_state != _ForceState.ready);
            if (Event is PointerMoveEvent || Event is PointerDownEvent) {
                if (Event.pressure > Event.pressureMax || Event.pressure < Event.pressureMin) {
                    //debugPrint(
                    UnityEngine.Debug.Log(
                        "The reported device pressure " + Event.pressure.ToString() +
                        " is outside of the device pressure range where: " +
                        Event.pressureMin.ToString() + " <= pressure <= " + Event.pressureMax.ToString()
                    );
                }

                float pressure = interpolation(Event.pressureMin, Event.pressureMax, Event.pressure);
                D.assert((pressure >= 0.0 && pressure <= 1.0) || pressure.isNaN());
                _lastPosition = OffsetPair.fromEventPosition(Event);
                _lastPressure = pressure;
                if (_state == _ForceState.possible) {
                    if (pressure > startPressure) {
                        _state = _ForceState.started;
                        resolve(GestureDisposition.accepted);
                    }
                    else if (Event.delta.distanceSquared > 18.0f) {
                        resolve(GestureDisposition.rejected);
                    }
                }

                if (pressure > startPressure && _state == _ForceState.accepted) {
                    _state = _ForceState.started;
                    if (onStart != null) {
                        invokeCallback("onStart", () => onStart(
                            new ForcePressDetails(
                                pressure: pressure,
                                globalPosition: _lastPosition.global,
                                localPosition: _lastPosition.local)));
                    }
                }

                if (onPeak != null && pressure > peakPressure && (_state == _ForceState.started)) {
                    _state = _ForceState.peaked;
                    if (onPeak != null) {
                        invokeCallback("onPeak", () => onPeak(new ForcePressDetails(
                            pressure: pressure,
                            globalPosition: Event.position,
                            localPosition: Event.localPosition
                        )));
                    }
                }

                if (onUpdate != null && !pressure.isNaN() &&
                    (_state == _ForceState.started || _state == _ForceState.peaked)) {
                    if (onUpdate != null) {
                        invokeCallback("onUpdate", () => onUpdate(new ForcePressDetails(
                            pressure: pressure,
                            globalPosition: Event.position,
                            localPosition: Event.localPosition
                        )));
                    }
                }
            }

            stopTrackingIfPointerNoLongerDown(Event);
        }

        public override void acceptGesture(int pointer) {
            if (_state == _ForceState.possible)
                _state = _ForceState.accepted;

            if (onStart != null && _state == _ForceState.started) {
                invokeCallback("onStart", () => onStart(new ForcePressDetails(
                    pressure: _lastPressure,
                    globalPosition: _lastPosition.global,
                    localPosition: _lastPosition.local
                )));
            }
        }

        protected override void didStopTrackingLastPointer(int pointer) {
            bool wasAccepted = _state == _ForceState.started || _state == _ForceState.peaked;
            if (_state == _ForceState.possible) {
                resolve(GestureDisposition.rejected);
                return;
            }

            if (wasAccepted && onEnd != null) {
                if (onEnd != null) {
                    invokeCallback("onEnd", () => onEnd(new ForcePressDetails(
                        pressure: 0.0f,
                        globalPosition: _lastPosition.global,
                        localPosition: _lastPosition.local
                    )));
                }
            }

            _state = _ForceState.ready;
        }

        public override void rejectGesture(int pointer) {
            stopTrackingPointer(pointer);
            didStopTrackingLastPointer(pointer);
        }

        public static float _inverseLerp(float min, float max, float t) {
            D.assert(min <= max);
            float value = (t - min) / (max - min);


            if (!value.isNaN())
                value = value.clamp(0.0f, 1.0f);
            return value;
        }

        public override string debugDescription {
            get { return "force press"; }
        }
    }
}