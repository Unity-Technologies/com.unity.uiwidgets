/*using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    
    public delegate void PointerEnterEventListener(PointerEnterEvent _event);
    public delegate void PointerExitEventListener(PointerExitEvent _event);
    public delegate void PointerHoverEventListener(PointerHoverEvent _event);
    
    public class MouseTrackerAnnotation : Diagnosticable {

        public MouseTrackerAnnotation(
            PointerEnterEventListener onEnter, 
            PointerHoverEventListener onHover,
            PointerExitEventListener onExit) {
            this.onEnter = onEnter;
            this.onHover = onHover;
            this.onExit = onExit;
        }

        public readonly PointerEnterEventListener onEnter;
        public readonly PointerHoverEventListener onHover;
        public readonly PointerExitEventListener onExit;

      
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagsSummary<Delegate>(
              "callbacks",
              new Dictionary<string, Delegate> {
                  {"enter", onEnter},
                  {"hover", onHover},
                  {"exit", onExit},
              },
              ifEmpty: "<none>"
            ));
        }
    }


    public delegate IEnumerable<MouseTrackerAnnotation> MouseDetectorAnnotationFinder(Offset offset);
    public delegate void _UpdatedDeviceHandler(_MouseState mouseState, HashSet<MouseTrackerAnnotation> previousAnnotations);

    public class _MouseState {
        public _MouseState(PointerEvent initialEvent) {
            D.assert(initialEvent != null);
            _latestEvent = initialEvent;
        }

        public HashSet<MouseTrackerAnnotation> annotations {
            get { return _annotations; }
        }

        HashSet<MouseTrackerAnnotation> _annotations = new HashSet<MouseTrackerAnnotation>();

        public HashSet<MouseTrackerAnnotation> replaceAnnotations(HashSet<MouseTrackerAnnotation> value) {
            HashSet<MouseTrackerAnnotation> previous = _annotations;
            _annotations = value;
            return previous;
        }
        
        public PointerEvent latestEvent {
            get {
                return _latestEvent;
            }
            set {
                D.assert(value != null);
                _latestEvent = value;
            }
        }

        PointerEvent _latestEvent;
      

        public int device {
            get {
                return latestEvent.device;
            }
        }

      
        public override string ToString() {
            string describeEvent(PointerEvent _event) {
                return _event == null ? "null" : foundation_.describeIdentity(_event);
            }
            string describeLatestEvent = $"latestEvent: {describeEvent(latestEvent)}";
            string describeAnnotations = $"annotations: [list of {annotations.Count}]";
            return $"{foundation_.describeIdentity(this)}({describeLatestEvent}, {describeAnnotations})";
        }
    }

public class MouseTracker : ChangeNotifier {
  
    public MouseTracker(PointerRouter _router, MouseDetectorAnnotationFinder annotationFinder) {
        D.assert(_router != null);
        D.assert(annotationFinder != null);
        _router.addGlobalRoute(_handleEvent);
    }

  
    public override void dispose() {
        base.dispose();
        _router.removeGlobalRoute(_handleEvent);
    }
    
    public readonly MouseDetectorAnnotationFinder annotationFinder;
    public readonly PointerRouter _router;
    public readonly Dictionary<int, _MouseState> _mouseStates = new Dictionary<int, _MouseState>();
    
    static bool _shouldMarkStateDirty(_MouseState state, PointerEvent value) {
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

    void _handleEvent(PointerEvent _event) {
        if (_event.kind != PointerDeviceKind.mouse)
            return;
        if (_event is PointerSignalEvent)
            return;
        int device = _event.device;
        _MouseState existingState = _mouseStates.getOrDefault(device);
        if (!_shouldMarkStateDirty(existingState, _event))
            return;

        PointerEvent previousEvent = existingState?.latestEvent;
        _updateDevices(
        targetEvent: _event,
        handleUpdatedDevice: (_MouseState mouseState, HashSet<MouseTrackerAnnotation> previousAnnotations) =>{
            D.assert(mouseState.device == _event.device);
            _dispatchDeviceCallbacks(
            lastAnnotations: previousAnnotations,
            nextAnnotations: mouseState.annotations,
            previousEvent: previousEvent,
            unhandledEvent: _event
            );
        });
    }
  
    HashSet<MouseTrackerAnnotation> _findAnnotations(_MouseState state) {
        Offset globalPosition = state.latestEvent.position;
        int device = state.device;
        HashSet<MouseTrackerAnnotation> temp = new HashSet<MouseTrackerAnnotation>();
        foreach (var set in annotationFinder(globalPosition)) {
            temp.Add(set);
        }
        return (_mouseStates.ContainsKey(device)) ? temp : new HashSet<MouseTrackerAnnotation>();
    }

    static bool _duringBuildPhase {
        get {
            return SchedulerBinding.instance.schedulerPhase == SchedulerPhase.persistentCallbacks;
        }
    }
    
    void _updateAllDevices() {
        _updateDevices(
            handleUpdatedDevice: (_MouseState mouseState, HashSet<MouseTrackerAnnotation> previousAnnotations) => {
            _dispatchDeviceCallbacks(
                lastAnnotations: previousAnnotations,
                nextAnnotations: mouseState.annotations,
                previousEvent: mouseState.latestEvent,
                unhandledEvent: null
            );
        });
    }

    bool _duringDeviceUpdate = false;
  
    void _updateDevices(
        PointerEvent targetEvent = null,
        _UpdatedDeviceHandler handleUpdatedDevice = null
    ) {
        D.assert(handleUpdatedDevice != null);
        D.assert(!_duringBuildPhase);
        D.assert(!_duringDeviceUpdate);
        bool mouseWasConnected = mouseIsConnected;

        _MouseState targetState = null;
        if (targetEvent != null) {
            targetState = _mouseStates.getOrDefault(targetEvent.device);
            if (targetState == null) {
                targetState = new _MouseState(initialEvent: targetEvent);
                _mouseStates[targetState.device] = targetState;
            } else {
                D.assert(!(targetEvent is PointerAddedEvent));
                targetState.latestEvent = targetEvent;
                if (targetEvent is PointerRemovedEvent)
                    _mouseStates.Remove(targetEvent.device);
            }
        }

        D.assert(() => {
            _duringDeviceUpdate = true;
            return true;
        });
        IEnumerable<_MouseState> dirtyStates = targetEvent == null ? _mouseStates.Values : new List<_MouseState>{targetState};
        foreach (_MouseState dirtyState in dirtyStates) {
            HashSet<MouseTrackerAnnotation> nextAnnotations = _findAnnotations(dirtyState);
            HashSet<MouseTrackerAnnotation> lastAnnotations = dirtyState.replaceAnnotations(nextAnnotations);
            handleUpdatedDevice(dirtyState, lastAnnotations);
        }
        D.assert(() => {
          _duringDeviceUpdate = false;
          return true;
        });

        if (mouseWasConnected != mouseIsConnected)
            notifyListeners();
    }
    
    static void _dispatchDeviceCallbacks(
        HashSet<MouseTrackerAnnotation> lastAnnotations = null,
        HashSet<MouseTrackerAnnotation> nextAnnotations = null,
        PointerEvent previousEvent = null,
        PointerEvent unhandledEvent = null
    ) {
        D.assert(lastAnnotations != null);
        D.assert(nextAnnotations != null);
        PointerEvent latestEvent = unhandledEvent ?? previousEvent;
        D.assert(latestEvent != null);

        
        
        List<MouseTrackerAnnotation> exitingAnnotations = new List<MouseTrackerAnnotation>();
        foreach (var annotation in lastAnnotations) {
            if (!nextAnnotations.Contains(annotation)) {
                exitingAnnotations.Add(annotation);
            }
        }
        foreach (MouseTrackerAnnotation annotation in exitingAnnotations) {
            annotation.onExit?.Invoke(PointerExitEvent.fromMouseEvent(latestEvent));
        }

        List<MouseTrackerAnnotation> enteringAnnotations = new List<MouseTrackerAnnotation>();
        foreach (var annotation in nextAnnotations) {
            if (!lastAnnotations.Contains(annotation)) {
                enteringAnnotations.Add(annotation);
            }
        }
        enteringAnnotations.Reverse();
        foreach (MouseTrackerAnnotation annotation in enteringAnnotations) {
            annotation.onEnter?.Invoke(PointerEnterEvent.fromMouseEvent(latestEvent));
        }
    
        if (unhandledEvent is PointerHoverEvent) {
            Offset lastHoverPosition = previousEvent is PointerHoverEvent ? previousEvent.position : null;
            bool pointerHasMoved = lastHoverPosition == null || lastHoverPosition != unhandledEvent.position;
            List<MouseTrackerAnnotation> nextAnnotationsList = new List<MouseTrackerAnnotation>();
            foreach (var annotation in nextAnnotations) {
                nextAnnotationsList.Add(annotation);
            }

            nextAnnotationsList.Reverse();
            IEnumerable<MouseTrackerAnnotation> hoveringAnnotations = pointerHasMoved ? nextAnnotationsList : enteringAnnotations;
            foreach (MouseTrackerAnnotation annotation in hoveringAnnotations) {
                annotation.onHover?.Invoke((PointerHoverEvent)unhandledEvent);
            }
        }
    }

    bool _hasScheduledPostFrameCheck = false;

    public void schedulePostFrameCheck() {
        D.assert(_duringBuildPhase);
        D.assert(!_duringDeviceUpdate);
        if (!mouseIsConnected)
            return;
        if (!_hasScheduledPostFrameCheck) {
            _hasScheduledPostFrameCheck = true;
            SchedulerBinding.instance.addPostFrameCallback((TimeSpan timeSpan) => {
            D.assert(_hasScheduledPostFrameCheck);
            _hasScheduledPostFrameCheck = false;
            _updateAllDevices();
            });
        }
    }
  
        public bool mouseIsConnected {
            get {
                return _mouseStates.isNotEmpty();
            }
        }
    }
}*/