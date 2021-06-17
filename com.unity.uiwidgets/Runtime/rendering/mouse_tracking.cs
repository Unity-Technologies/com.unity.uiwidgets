using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void PointerHoverEventListener(PointerHoverEvent evt);

    public delegate void PointerEnterEventListener(PointerEnterEvent evt);

    public delegate void PointerExitEventListener(PointerExitEvent evt);

    public delegate void PointerDragFromEditorEnterEventListener(PointerDragFromEditorEnterEvent evt);

    public delegate void PointerDragFromEditorHoverEventListener(PointerDragFromEditorHoverEvent evt);

    public delegate void PointerDragFromEditorExitEventListener(PointerDragFromEditorExitEvent evt);

    public delegate void PointerDragFromEditorReleaseEventListener(PointerDragFromEditorReleaseEvent evt);

    public delegate void _UpdatedDeviceHandler(_MouseState mouseState, HashSet<MouseTrackerAnnotation> previousAnnotations);

   
    public class _MouseState {
        public _MouseState(PointerEvent initialEvent = null
        ) {
            D.assert(initialEvent != null);
            _latestEvent = initialEvent;
        }

        public HashSet<MouseTrackerAnnotation> annotations {
            get { return _annotations;}
        }
        HashSet<MouseTrackerAnnotation> _annotations = new HashSet<MouseTrackerAnnotation>();

        public HashSet<MouseTrackerAnnotation> replaceAnnotations(HashSet<MouseTrackerAnnotation> value) {
            HashSet<MouseTrackerAnnotation> previous = _annotations;
            _annotations = value;
            return previous;
        }

        // The most recently processed mouse event observed from this device.
        public PointerEvent latestEvent {
            get { return _latestEvent; }
            set {
                D.assert(value != null);
                _latestEvent = value;
            }
        }
        PointerEvent _latestEvent;

        public int device {
            get { return latestEvent.device;}
        }

        public override string ToString() {
            string describeEvent(PointerEvent Event) {
                return Event == null ? "null" : describeIdentity(Event);
            }
            string describeLatestEvent = $"latestEvent: {describeEvent(latestEvent)}";
            string describeAnnotations = $"annotations: [list of {annotations.Count}]";
            return $"{describeIdentity(this)}"+$"({describeLatestEvent}, {describeAnnotations})";
        }

        public string describeIdentity(object Object) {
            return $"{Object.GetType()}" + $"{Object.GetHashCode()}";

        }
    }
    public class MouseTrackerAnnotation {
        public MouseTrackerAnnotation(
            PointerEnterEventListener onEnter = null,
            PointerHoverEventListener onHover = null,
            PointerExitEventListener onExit = null,
            PointerDragFromEditorEnterEventListener onDragFromEditorEnter = null,
            PointerDragFromEditorHoverEventListener onDragFromEditorHover = null,
            PointerDragFromEditorExitEventListener onDragFromEditorExit = null,
            PointerDragFromEditorReleaseEventListener onDragFromEditorRelease = null
        ) {
            this.onEnter = onEnter;
            this.onHover = onHover;
            this.onExit = onExit;

            this.onDragFromEditorEnter = onDragFromEditorEnter;
            this.onDragFromEditorHover = onDragFromEditorHover;
            this.onDragFromEditorExit = onDragFromEditorExit;
            this.onDragFromEditorRelease = onDragFromEditorRelease;
        }

        public readonly PointerEnterEventListener onEnter;

        public readonly PointerHoverEventListener onHover;

        public readonly PointerExitEventListener onExit;

        public readonly PointerDragFromEditorEnterEventListener onDragFromEditorEnter;
        public readonly PointerDragFromEditorHoverEventListener onDragFromEditorHover;
        public readonly PointerDragFromEditorExitEventListener onDragFromEditorExit;
        public readonly PointerDragFromEditorReleaseEventListener onDragFromEditorRelease;

        public override string ToString() {
            return
                $"{GetType()}#{GetHashCode()}{(onEnter == null ? "" : " onEnter")}{(onHover == null ? "" : " onHover")}{(onExit == null ? "" : " onExit")}";
        }
    }

    public class _TrackedAnnotation {
        public _TrackedAnnotation(
            MouseTrackerAnnotation annotation) {
            this.annotation = annotation;
        }

        public readonly MouseTrackerAnnotation annotation;

        public HashSet<int> activeDevices = new HashSet<int>();
    }

    public delegate IEnumerable<MouseTrackerAnnotation> MouseDetectorAnnotationFinder(Offset offset);

    public class MouseTracker : ChangeNotifier{
        public MouseTracker(
            PointerRouter router,
            MouseDetectorAnnotationFinder annotationFinder
            
        ) {
            D.assert(router != null);
            D.assert(annotationFinder != null);
            _router = router;
            router.addGlobalRoute(_handleEvent);
            this.annotationFinder = annotationFinder;
        }
        public override void dispose() {
            base.dispose();
            _router.removeGlobalRoute(_handleEvent);
        }
       
        public readonly MouseDetectorAnnotationFinder annotationFinder;
        readonly Dictionary<int, PointerEvent> _lastMouseEvent = new Dictionary<int, PointerEvent>();
        readonly PointerRouter _router;
        public readonly Dictionary<int, _MouseState> _mouseStates = new Dictionary<int, _MouseState>();
        
        public bool mouseIsConnected {
            get { return _lastMouseEvent.isNotEmpty(); }
        }
        public static bool _shouldMarkStateDirty(_MouseState state, PointerEvent value) {
            if (state == null)
                return true;
            D.assert(value != null);
            PointerEvent lastEvent = state.latestEvent;
            D.assert(value.device == lastEvent.device);
            
            D.assert((value is PointerAddedEvent) == (lastEvent is PointerRemovedEvent));
            if (value is PointerSignalEvent)
                return false;
            return lastEvent is PointerAddedEvent
                   || value is PointerRemovedEvent
                   || lastEvent.position != value.position;
        }

        public readonly Dictionary<MouseTrackerAnnotation, _TrackedAnnotation> _trackedAnnotations =
            new Dictionary<MouseTrackerAnnotation, _TrackedAnnotation>();

        public void _handleEvent(PointerEvent Event) {
            if (Event.kind != PointerDeviceKind.mouse)
                return;
            if (Event is PointerSignalEvent)
                return;
            int device = Event.device;
            _MouseState existingState = _mouseStates.getOrDefault(device);
            if (!_shouldMarkStateDirty(existingState, Event))
                return;
            PointerEvent previousEvent = existingState?.latestEvent;
            _updateDevices(
                targetEvent: Event,
                handleUpdatedDevice: (_MouseState mouseState, HashSet<MouseTrackerAnnotation> previousAnnotations) =>{
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
        public static void _dispatchDeviceCallbacks(
            HashSet<MouseTrackerAnnotation> lastAnnotations,
            HashSet<MouseTrackerAnnotation> nextAnnotations,
            PointerEvent previousEvent = null,
            PointerEvent unhandledEvent = null
        ) { 
            D.assert(lastAnnotations != null);
            D.assert(nextAnnotations != null);
            PointerEvent latestEvent = unhandledEvent ?? previousEvent;
            D.assert(latestEvent != null);
            IEnumerable<MouseTrackerAnnotation> exitingAnnotations = new List<MouseTrackerAnnotation>();
            foreach (var lastAnnotation in lastAnnotations) {
                if (!nextAnnotations.Contains(lastAnnotation)) {
                    exitingAnnotations.Append(lastAnnotation);
                }
            }
            foreach ( MouseTrackerAnnotation annotation in exitingAnnotations) { 
                if (annotation.onExit != null) {
                    annotation.onExit(PointerExitEvent.fromMouseEvent(latestEvent));
                }
            }
            IEnumerable<MouseTrackerAnnotation> enteringAnnotations = new List<MouseTrackerAnnotation>();
            List<MouseTrackerAnnotation> entering = new List<MouseTrackerAnnotation>();
            foreach (var nextAnnotation in nextAnnotations) {
                if (!lastAnnotations.Contains(nextAnnotation)) {
                    entering.Add(nextAnnotation);
                }
            }    
            entering.ToList().Reverse();
            enteringAnnotations = entering;
            foreach ( MouseTrackerAnnotation annotation in enteringAnnotations) { 
                if (annotation.onEnter != null) {
                    annotation.onEnter(PointerEnterEvent.fromMouseEvent(latestEvent));
                }
            }
            if (unhandledEvent is PointerHoverEvent) {
               Offset lastHoverPosition = previousEvent is PointerHoverEvent ? previousEvent.position : null;
               bool pointerHasMoved = lastHoverPosition == null || lastHoverPosition != unhandledEvent.position;
               nextAnnotations.ToList().Reverse();
               IEnumerable<MouseTrackerAnnotation> hoveringAnnotations = pointerHasMoved ? nextAnnotations : enteringAnnotations;
               foreach ( MouseTrackerAnnotation annotation in hoveringAnnotations) { 
                   if (annotation.onHover != null) {
                    annotation.onHover((PointerHoverEvent)unhandledEvent);
                   } 
               }
            }
        }

        public HashSet<MouseTrackerAnnotation> _findAnnotations(_MouseState state) {
            Offset globalPosition = state.latestEvent.position;
            int device = state.device;
            HashSet<MouseTrackerAnnotation> result = new HashSet<MouseTrackerAnnotation>();
            foreach (var values in annotationFinder(globalPosition)) {
                result.Add(values);
            }
            return (_mouseStates.ContainsKey(device))
                ? result
                : new  HashSet<MouseTrackerAnnotation>{} as HashSet<MouseTrackerAnnotation>;
        }


        public static bool _duringBuildPhase {
            get {
                return SchedulerBinding.instance.schedulerPhase == SchedulerPhase.persistentCallbacks;
            }
        }
        public void _updateAllDevices() {
            _updateDevices(
                handleUpdatedDevice: (_MouseState mouseState, HashSet<MouseTrackerAnnotation> previousAnnotations)=> {
                    _dispatchDeviceCallbacks(
                        lastAnnotations: previousAnnotations,
                        nextAnnotations: mouseState.annotations,
                        previousEvent: mouseState.latestEvent,
                        unhandledEvent: null
                    );
                }
            );
        }
        bool _duringDeviceUpdate = false;
        
        void _updateDevices(
            PointerEvent targetEvent = null, 
            _UpdatedDeviceHandler handleUpdatedDevice = null) { 
            D.assert(handleUpdatedDevice != null);
            D.assert(!_duringBuildPhase);
            D.assert(!_duringDeviceUpdate);
            bool mouseWasConnected = mouseIsConnected;
            
            _MouseState targetState = null;
            if (targetEvent != null) {
              targetState = _mouseStates.getOrDefault(targetEvent.device);
              if (targetState == null) {
                targetState = new  _MouseState(initialEvent: targetEvent);
                _mouseStates[targetState.device] = targetState;
              } else {
                D.assert(!(targetEvent is PointerAddedEvent));
                targetState.latestEvent = targetEvent;
                if (targetEvent is PointerRemovedEvent)
                  _mouseStates.Remove(targetEvent.device);
              }
            }
            D.assert((targetState == null) == (targetEvent == null));

            D.assert(()=> {
              _duringDeviceUpdate = true;
              return true;
            });
           
            IEnumerable<_MouseState> dirtyStates = targetEvent == null ? (IEnumerable<_MouseState>) _mouseStates.Values : new  List<_MouseState>{targetState};
            foreach ( _MouseState dirtyState in dirtyStates) {
              HashSet<MouseTrackerAnnotation> nextAnnotations = _findAnnotations(dirtyState); 
              HashSet<MouseTrackerAnnotation> lastAnnotations = dirtyState.replaceAnnotations(nextAnnotations);
              handleUpdatedDevice(dirtyState, lastAnnotations);
            }
            D.assert(() =>{
              _duringDeviceUpdate = false;
              return true;
            });

            if (mouseWasConnected != mouseIsConnected)
              notifyListeners();
        }
        public bool _hasScheduledPostFrameCheck = false;
        public void schedulePostFrameCheck() {
            D.assert(_duringBuildPhase);
            D.assert(!_duringDeviceUpdate);
            if (!mouseIsConnected)
              return;
            if (!_hasScheduledPostFrameCheck) {
              _hasScheduledPostFrameCheck = true;
              SchedulerBinding.instance.addPostFrameCallback((stamp =>  {
                D.assert(_hasScheduledPostFrameCheck);
                _hasScheduledPostFrameCheck = false;
                _updateAllDevices();
              }));
            }
        }
    }
}