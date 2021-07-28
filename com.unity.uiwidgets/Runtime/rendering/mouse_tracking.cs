using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void PointerHoverEventListener(PointerHoverEvent evt);

    public delegate void PointerEnterEventListener(PointerEnterEvent evt);

    public delegate void PointerExitEventListener(PointerExitEvent evt);

    public delegate void _UpdatedDeviceHandler(_MouseState mouseState,
        HashSet<MouseTrackerAnnotation> previousAnnotations);


    public class _MouseState {
        PointerEvent _latestEvent;

        public _MouseState(PointerEvent initialEvent = null
        ) {
            D.assert(initialEvent != null);
            _latestEvent = initialEvent;
        }

        public HashSet<MouseTrackerAnnotation> annotations { get; private set; } =
            new HashSet<MouseTrackerAnnotation>();

        // The most recently processed mouse event observed from this device.
        public PointerEvent latestEvent {
            get { return _latestEvent; }
            set {
                D.assert(value != null);
                _latestEvent = value;
            }
        }

        public int device {
            get { return latestEvent.device; }
        }

        public HashSet<MouseTrackerAnnotation> replaceAnnotations(HashSet<MouseTrackerAnnotation> value) {
            var previous = annotations;
            annotations = value;
            return previous;
        }

        public override string ToString() {
            string describeEvent(PointerEvent Event) {
                return Event == null ? "null" : describeIdentity(Object: Event);
            }

            var describeLatestEvent = $"latestEvent: {describeEvent(Event: latestEvent)}";
            var describeAnnotations = $"annotations: [list of {annotations.Count}]";
            return $"{describeIdentity(this)}" + $"({describeLatestEvent}, {describeAnnotations})";
        }

        public string describeIdentity(object Object) {
            return $"{Object.GetType()}" + $"{Object.GetHashCode()}";
        }
    }

    public class MouseTrackerAnnotation : Diagnosticable {
        public readonly PointerEnterEventListener onEnter;

        public readonly PointerExitEventListener onExit;

        public readonly PointerHoverEventListener onHover;

        public MouseTrackerAnnotation(
            PointerEnterEventListener onEnter = null,
            PointerHoverEventListener onHover = null,
            PointerExitEventListener onExit = null
        ) {
            this.onEnter = onEnter;
            this.onHover = onHover;
            this.onExit = onExit;
        }


        public override string ToString() {
            return
                $"{GetType()}#{GetHashCode()}{(onEnter == null ? "" : " onEnter")}{(onHover == null ? "" : " onHover")}{(onExit == null ? "" : " onExit")}";
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties: properties);
            properties.add(new FlagsSummary<Delegate>(
                "callbacks",
                new Dictionary<string, Delegate> {
                    {"enter", onEnter},
                    {"hover", onHover},
                    {"exit", onExit}
                },
                "<none>"
            ));
        }
    }

    public class _TrackedAnnotation {
        public readonly MouseTrackerAnnotation annotation;

        public HashSet<int> activeDevices = new HashSet<int>();

        public _TrackedAnnotation(
            MouseTrackerAnnotation annotation) {
            this.annotation = annotation;
        }
    }

    public delegate IEnumerable<MouseTrackerAnnotation> MouseDetectorAnnotationFinder(Offset offset);

    public class MouseTracker : ChangeNotifier {
        public readonly Dictionary<int, _MouseState> _mouseStates = new Dictionary<int, _MouseState>();
        readonly PointerRouter _router;

        public readonly Dictionary<MouseTrackerAnnotation, _TrackedAnnotation> _trackedAnnotations =
            new Dictionary<MouseTrackerAnnotation, _TrackedAnnotation>();

        public readonly MouseDetectorAnnotationFinder annotationFinder;

        bool _duringDeviceUpdate;
        public bool _hasScheduledPostFrameCheck;

        public MouseTracker(
            PointerRouter router,
            MouseDetectorAnnotationFinder annotationFinder
        ) {
            D.assert(router != null);
            D.assert(annotationFinder != null);
            _router = router;
            router.addGlobalRoute(route: _handleEvent);
            this.annotationFinder = annotationFinder;
        }

        public bool mouseIsConnected {
            get { return _mouseStates.isNotEmpty(); }
        }


        public static bool _duringBuildPhase {
            get { return SchedulerBinding.instance.schedulerPhase == SchedulerPhase.persistentCallbacks; }
        }

        public override void dispose() {
            base.dispose();
            _router.removeGlobalRoute(route: _handleEvent);
        }

        public static bool _shouldMarkStateDirty(_MouseState state, PointerEvent value) {
            if (state == null) {
                return true;
            }

            D.assert(value != null);
            var lastEvent = state.latestEvent;
            D.assert(value.device == lastEvent.device);

            D.assert(value is PointerAddedEvent == lastEvent is PointerRemovedEvent);
            if (value is PointerSignalEvent) {
                return false;
            }

            return lastEvent is PointerAddedEvent
                   || value is PointerRemovedEvent
                   || lastEvent.position != value.position;
        }

        public void _handleEvent(PointerEvent Event) {
            if (Event.kind != PointerDeviceKind.mouse) {
                return;
            }

            if (Event is PointerSignalEvent) {
                return;
            }

            var device = Event.device;
            var existingState = _mouseStates.getOrDefault(key: device);
            if (!_shouldMarkStateDirty(state: existingState, value: Event)) {
                return;
            }

            var previousEvent = existingState?.latestEvent;
            _updateDevices(
                targetEvent: Event,
                (mouseState, previousAnnotations) => {
                    D.assert(mouseState.device == Event.device);
                    _dispatchDeviceCallbacks(
                        lastAnnotations: previousAnnotations,
                        nextAnnotations: mouseState.annotations,
                        previousEvent: previousEvent,
                        unhandledEvent: Event
                    );
                }
            );
        }

        public HashSet<MouseTrackerAnnotation> _findAnnotations(_MouseState state) {
            var globalPosition = state.latestEvent.position;
            var device = state.device;
            var result = new HashSet<MouseTrackerAnnotation>();
            foreach (var values in annotationFinder(offset: globalPosition)) {
                result.Add(item: values);
            }

            return _mouseStates.ContainsKey(key: device)
                ? result
                : new HashSet<MouseTrackerAnnotation>();
        }

        public void _updateAllDevices() {
            _updateDevices(
                handleUpdatedDevice: (mouseState, previousAnnotations) => {
                    _dispatchDeviceCallbacks(
                        lastAnnotations: previousAnnotations,
                        nextAnnotations: mouseState.annotations,
                        previousEvent: mouseState.latestEvent
                    );
                }
            );
        }

        void _updateDevices(
            PointerEvent targetEvent = null,
            _UpdatedDeviceHandler handleUpdatedDevice = null) {
            D.assert(handleUpdatedDevice != null);
            D.assert(result: !_duringBuildPhase);
            D.assert(result: !_duringDeviceUpdate);
            var mouseWasConnected = mouseIsConnected;

            _MouseState targetState = null;
            if (targetEvent != null) {
                targetState = _mouseStates.getOrDefault(key: targetEvent.device);
                if (targetState == null) {
                    targetState = new _MouseState(initialEvent: targetEvent);
                    _mouseStates[key: targetState.device] = targetState;
                }
                else {
                    D.assert(!(targetEvent is PointerAddedEvent));
                    targetState.latestEvent = targetEvent;
                    if (targetEvent is PointerRemovedEvent) {
                        _mouseStates.Remove(key: targetEvent.device);
                    }
                }
            }

            D.assert(targetState == null == (targetEvent == null));

            D.assert(() => {
                _duringDeviceUpdate = true;
                return true;
            });

            var dirtyStates = targetEvent == null
                ? (IEnumerable<_MouseState>) _mouseStates.Values
                : new List<_MouseState> {targetState};
            foreach (var dirtyState in dirtyStates) {
                var nextAnnotations = _findAnnotations(state: dirtyState);
                var lastAnnotations = dirtyState.replaceAnnotations(value: nextAnnotations);
                handleUpdatedDevice(mouseState: dirtyState, previousAnnotations: lastAnnotations);
            }

            D.assert(() => {
                _duringDeviceUpdate = false;
                return true;
            });

            if (mouseWasConnected != mouseIsConnected) {
                notifyListeners();
            }
        }

        public static void _dispatchDeviceCallbacks(
            HashSet<MouseTrackerAnnotation> lastAnnotations,
            HashSet<MouseTrackerAnnotation> nextAnnotations,
            PointerEvent previousEvent = null,
            PointerEvent unhandledEvent = null
        ) {
            D.assert(lastAnnotations != null);
            D.assert(nextAnnotations != null);
            var latestEvent = unhandledEvent ?? previousEvent;
            D.assert(latestEvent != null);
            IEnumerable<MouseTrackerAnnotation> exitingAnnotations = new List<MouseTrackerAnnotation>();
            var exiting = new List<MouseTrackerAnnotation>();
            foreach (var lastAnnotation in lastAnnotations) {
                if (!nextAnnotations.Contains(item: lastAnnotation)) {
                    exiting.Add(item: lastAnnotation);
                }
            }

            exitingAnnotations = exiting;
            foreach (var annotation in exitingAnnotations) {
                if (annotation.onExit != null) {
                    annotation.onExit(PointerExitEvent.fromMouseEvent(hover: latestEvent));
                }
            }

            IEnumerable<MouseTrackerAnnotation> enteringAnnotations = new List<MouseTrackerAnnotation>();
            var entering = new List<MouseTrackerAnnotation>();
            foreach (var nextAnnotation in nextAnnotations) {
                if (!lastAnnotations.Contains(item: nextAnnotation)) {
                    entering.Add(item: nextAnnotation);
                }
            }

            entering.ToList().Reverse();
            enteringAnnotations = entering;
            foreach (var annotation in enteringAnnotations) {
                if (annotation.onEnter != null) {
                    annotation.onEnter(PointerEnterEvent.fromMouseEvent(hover: latestEvent));
                }
            }

            if (unhandledEvent is PointerHoverEvent) {
                var lastHoverPosition = previousEvent is PointerHoverEvent ? previousEvent.position : null;
                var pointerHasMoved = lastHoverPosition == null || lastHoverPosition != unhandledEvent.position;
                nextAnnotations.ToList().Reverse();
                var hoveringAnnotations = pointerHasMoved ? nextAnnotations : enteringAnnotations;
                foreach (var annotation in hoveringAnnotations) {
                    if (annotation.onHover != null) {
                        annotation.onHover((PointerHoverEvent) unhandledEvent);
                    }
                }
            }
        }


        public void schedulePostFrameCheck() {
            D.assert(result: _duringBuildPhase);
            D.assert(result: !_duringDeviceUpdate);
            if (!mouseIsConnected) {
                return;
            }

            if (!_hasScheduledPostFrameCheck) {
                _hasScheduledPostFrameCheck = true;
                SchedulerBinding.instance.addPostFrameCallback(stamp => {
                    D.assert(result: _hasScheduledPostFrameCheck);
                    _hasScheduledPostFrameCheck = false;
                    _updateAllDevices();
                });
            }
        }
    }
}