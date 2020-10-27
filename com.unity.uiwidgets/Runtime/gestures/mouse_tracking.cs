using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void PointerHoverEventListener(PointerHoverEvent evt);

    public delegate void PointerEnterEventListener(PointerEnterEvent evt);

    public delegate void PointerExitEventListener(PointerExitEvent evt);

    public delegate void PointerDragFromEditorEnterEventListener(PointerDragFromEditorEnterEvent evt);

    public delegate void PointerDragFromEditorHoverEventListener(PointerDragFromEditorHoverEvent evt);

    public delegate void PointerDragFromEditorExitEventListener(PointerDragFromEditorExitEvent evt);

    public delegate void PointerDragFromEditorReleaseEventListener(PointerDragFromEditorReleaseEvent evt);

    /// The annotation object used to annotate layers that are interested in mouse
    /// movements.
    /// This is added to a layer and managed by the [Listener] widget.
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

    public delegate MouseTrackerAnnotation MouseDetectorAnnotationFinder(Offset offset);

    public partial class MouseTracker {
        public MouseTracker(
            PointerRouter router,
            MouseDetectorAnnotationFinder annotationFinder,
            bool inEditorWindow = false
        ) {
            router.addGlobalRoute(_handleEvent);
            this.annotationFinder = annotationFinder;
            this.inEditorWindow = inEditorWindow;
        }

        readonly bool inEditorWindow;

        readonly Dictionary<int, PointerEvent> _lastMouseEvent = new Dictionary<int, PointerEvent>();

        public bool mouseIsConnected {
            get { return _lastMouseEvent.isNotEmpty(); }
        }

        public readonly MouseDetectorAnnotationFinder annotationFinder;

        public readonly Dictionary<MouseTrackerAnnotation, _TrackedAnnotation> _trackedAnnotations =
            new Dictionary<MouseTrackerAnnotation, _TrackedAnnotation>();

        public void attachAnnotation(MouseTrackerAnnotation annotation) {
            _trackedAnnotations[annotation] = new _TrackedAnnotation(annotation);
            _scheduleMousePositionCheck();

#if UNITY_EDITOR
            //_scheduleDragFromEditorMousePositionCheck();
#endif
        }

        public void detachAnnotation(MouseTrackerAnnotation annotation) {
            _TrackedAnnotation trackedAnnotation = _findAnnotation(annotation);
            foreach (int deviceId in trackedAnnotation.activeDevices) {
                if (annotation.onExit != null) {
                    annotation.onExit(
                        PointerExitEvent.fromMouseEvent(_lastMouseEvent[deviceId]));
                }
#if UNITY_EDITOR
                //detachDragFromEditorAnnotation(annotation, deviceId);
#endif
            }

            _trackedAnnotations.Remove(annotation);
        }

        void _scheduleMousePositionCheck() {
            SchedulerBinding.instance.addPostFrameCallback(_ => { collectMousePositions(); });
            SchedulerBinding.instance.scheduleFrame();
        }

        // Handler for events coming from the PointerRouter.
        void _handleEvent(PointerEvent evt) {
            if (evt.kind != PointerDeviceKind.mouse) {
                return;
            }

            int deviceId = evt.device;
            if (_trackedAnnotations.isEmpty()) {
                // If we are adding the device again, then we're not removing it anymore.
                _lastMouseEvent.Remove(deviceId);
                return;
            }

            if (evt is PointerRemovedEvent) {
                _lastMouseEvent.Remove(deviceId);
                // If the mouse was removed, then we need to schedule one more check to
                // exit any annotations that were active.
                _scheduleMousePositionCheck();
            }
            else {
                if (evt is PointerMoveEvent ||
                    evt is PointerHoverEvent ||
                    evt is PointerDownEvent) {
                    if (!_lastMouseEvent.ContainsKey(deviceId) ||
                        _lastMouseEvent[deviceId].position != evt.position) {
                        _scheduleMousePositionCheck();
                    }

                    _lastMouseEvent[deviceId] = evt;
                }
            }

#if UNITY_EDITOR
            //_handleDragFromEditorEvent(evt, deviceId);
#endif
        }

        _TrackedAnnotation _findAnnotation(MouseTrackerAnnotation annotation) {
            if (!_trackedAnnotations.TryGetValue(annotation, out var trackedAnnotation)) {
                D.assert(false, () => "Unable to find annotation $annotation in tracked annotations. " +
                                      "Check that attachAnnotation has been called for all annotated layers.");
            }

            return trackedAnnotation;
        }

        bool isAnnotationAttached(MouseTrackerAnnotation annotation) {
            return _trackedAnnotations.ContainsKey(annotation);
        }

        /// Tells interested objects that a mouse has entered, exited, or moved, given
        /// a callback to fetch the [MouseTrackerAnnotation] associated with a global
        /// offset.
        ///
        /// This is called from a post-frame callback when the layer tree has been
        /// updated, right after rendering the frame.
        ///
        /// This function is only public to allow for proper testing of the
        /// MouseTracker. Do not call in other contexts.
        public void collectMousePositions() {
            void exitAnnotation(_TrackedAnnotation trackedAnnotation, int deviceId) {
                if (trackedAnnotation.activeDevices.Contains(deviceId)) {
                    if (trackedAnnotation.annotation?.onExit != null) {
                        trackedAnnotation.annotation.onExit(
                            PointerExitEvent.fromMouseEvent(_lastMouseEvent[deviceId]));
                    }

                    trackedAnnotation.activeDevices.Remove(deviceId);
                }
            }

            void exitAllDevices(_TrackedAnnotation trackedAnnotation) {
                if (trackedAnnotation.activeDevices.isNotEmpty()) {
                    HashSet<int> deviceIds = new HashSet<int>(trackedAnnotation.activeDevices);
                    foreach (int deviceId in deviceIds) {
                        exitAnnotation(trackedAnnotation, deviceId);
                    }
                }
            }

            if (!mouseIsConnected) {
                foreach (var annotation in _trackedAnnotations.Values) {
                    exitAllDevices(annotation);
                }

                return;
            }

            foreach (int deviceId in _lastMouseEvent.Keys) {
                PointerEvent lastEvent = _lastMouseEvent[deviceId];
                MouseTrackerAnnotation hit = annotationFinder(lastEvent.position);

                if (hit == null) {
                    foreach (_TrackedAnnotation trackedAnnotation in _trackedAnnotations.Values) {
                        exitAnnotation(trackedAnnotation, deviceId);
                    }

                    return;
                }

                _TrackedAnnotation hitAnnotation = _findAnnotation(hit);

                //enter
                if (!hitAnnotation.activeDevices.Contains(deviceId)) {
                    hitAnnotation.activeDevices.Add(deviceId);
                    if (hitAnnotation.annotation?.onEnter != null) {
                        hitAnnotation.annotation.onEnter(PointerEnterEvent.fromMouseEvent(lastEvent));
                    }
                }

                //hover
                if (hitAnnotation.annotation?.onHover != null && lastEvent is PointerHoverEvent) {
                    hitAnnotation.annotation.onHover(lastEvent as PointerHoverEvent);
                }

                //leave
                foreach (_TrackedAnnotation trackedAnnotation in _trackedAnnotations.Values) {
                    if (hitAnnotation == trackedAnnotation) {
                        continue;
                    }

                    if (trackedAnnotation.activeDevices.Contains(deviceId)) {
                        if (trackedAnnotation.annotation?.onExit != null) {
                            trackedAnnotation.annotation.onExit(
                                PointerExitEvent.fromMouseEvent(lastEvent));
                        }

                        trackedAnnotation.activeDevices.Remove(deviceId);
                    }
                }
            }
        }
    }
}