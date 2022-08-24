using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEditor;

namespace Unity.UIWidgets.gestures {
    public delegate void PointerDragFromEditorEnterEventListener(PointerDragFromEditorEnterEvent evt);

    public delegate void PointerDragFromEditorHoverEventListener(PointerDragFromEditorHoverEvent evt);

    public delegate void PointerDragFromEditorExitEventListener(PointerDragFromEditorExitEvent evt);

    public delegate void PointerDragFromEditorReleaseEventListener(PointerDragFromEditorReleaseEvent evt);
    
    public delegate EditorMouseTrackerAnnotation EditorMouseDetectorAnnotationFinder(Offset offset);

    public class EditorMouseTrackerAnnotation {
        public EditorMouseTrackerAnnotation(
            PointerDragFromEditorEnterEventListener onDragFromEditorEnter = null,
            PointerDragFromEditorHoverEventListener onDragFromEditorHover = null,
            PointerDragFromEditorExitEventListener onDragFromEditorExit = null,
            PointerDragFromEditorReleaseEventListener onDragFromEditorRelease = null
        ) {
            this.onDragFromEditorEnter = onDragFromEditorEnter;
            this.onDragFromEditorHover = onDragFromEditorHover;
            this.onDragFromEditorExit = onDragFromEditorExit;
            this.onDragFromEditorRelease = onDragFromEditorRelease;
        }

        public readonly PointerDragFromEditorEnterEventListener onDragFromEditorEnter;
        public readonly PointerDragFromEditorHoverEventListener onDragFromEditorHover;
        public readonly PointerDragFromEditorExitEventListener onDragFromEditorExit;
        public readonly PointerDragFromEditorReleaseEventListener onDragFromEditorRelease;
    }

    public class _EditorTrackedAnnotation {
        public _EditorTrackedAnnotation(EditorMouseTrackerAnnotation annotation) {
            this.annotation = annotation;
        }

        public readonly EditorMouseTrackerAnnotation annotation;

        public readonly HashSet<int> activeDevices = new HashSet<int>();
    }

    public class EditorMouseTracker : ChangeNotifier {
        public readonly Dictionary<int, PointerEvent> _lastMouseEvent = new Dictionary<int, PointerEvent>();
        public bool mouseIsConnected => _lastMouseEvent.isNotEmpty();

        public readonly EditorMouseDetectorAnnotationFinder annotationFinder;

        public readonly Dictionary<EditorMouseTrackerAnnotation, _EditorTrackedAnnotation> _trackedAnnotations =
            new Dictionary<EditorMouseTrackerAnnotation, _EditorTrackedAnnotation>();

        public void attachDragFromEditorAnnotation(EditorMouseTrackerAnnotation annotation) {
            _trackedAnnotations[annotation] = new _EditorTrackedAnnotation(annotation);
            _scheduleDragFromEditorMousePositionCheck();
        }

        public void detachDragFromEditorAnnotation(EditorMouseTrackerAnnotation annotation) {
            var trackedAnnotation = _findAnnotation(annotation);
            foreach (var deviceId in trackedAnnotation.activeDevices) {
                annotation.onDragFromEditorExit(PointerDragFromEditorExitEvent.fromDragFromEditorEvent(
                    _lastMouseEvent[deviceId]
                ));
            }

            _trackedAnnotations.Remove(annotation);
        }

        _EditorTrackedAnnotation _findAnnotation(EditorMouseTrackerAnnotation annotation) {
            if (!_trackedAnnotations.TryGetValue(annotation, out var trackedAnnotation)) {
                D.assert(false, () => $"Unable to find annotation {annotation} in tracked annotations.");
            }

            return trackedAnnotation;
        }

        public EditorMouseTracker(
            PointerRouter router,
            EditorMouseDetectorAnnotationFinder annotationFinder
        ) {
            D.assert(router != null);
            D.assert(annotationFinder != null);
            router.addGlobalRoute(route: _handleEvent);
            this.annotationFinder = annotationFinder;
        }

        void _handleEvent(PointerEvent evt) {
            int deviceId = 0;

            if (_trackedAnnotations.isEmpty()) {
                _lastMouseEvent.Remove(deviceId);
                return;
            }
            
            if (evt is PointerDragFromEditorReleaseEvent) {
                _scheduleDragFromEditorReleaseCheck();
                _lastMouseEvent.Remove(deviceId);
            }
            else if (evt is PointerDragFromEditorEnterEvent ||
                     evt is PointerDragFromEditorHoverEvent ||
                     evt is PointerDragFromEditorExitEvent) {
                if (!_lastMouseEvent.ContainsKey(deviceId) ||
                    _lastMouseEvent[deviceId].position != evt.position) {
                    _scheduleDragFromEditorMousePositionCheck();
                }

                _lastMouseEvent[deviceId] = evt;
            }
        }

        public void schedulePostFrameCheck() {
            
        }

        void _scheduleDragFromEditorReleaseCheck() {
            DragAndDrop.AcceptDrag();

            var lastMouseEvent = new List<PointerEvent>();
            foreach (int deviceId in _lastMouseEvent.Keys) {
                var _deviceId = deviceId;
                var deviceEvent = _lastMouseEvent[_deviceId];

                //only process PointerEditorDragEvents
                if (!(deviceEvent is PointerDragFromEditorEnterEvent ||
                      deviceEvent is PointerDragFromEditorHoverEvent ||
                      deviceEvent is PointerDragFromEditorExitEvent)) {
                    continue;
                }

                lastMouseEvent.Add(_lastMouseEvent[_deviceId]);
                SchedulerBinding.instance.addPostFrameCallback(_ => {
                    foreach (var lastEvent in lastMouseEvent) {
                        EditorMouseTrackerAnnotation hit = annotationFinder(lastEvent.position);

                        if (hit == null) {
                            foreach (_EditorTrackedAnnotation trackedAnnotation in _trackedAnnotations.Values) {
                                if (trackedAnnotation.activeDevices.Contains(_deviceId)) {
                                    trackedAnnotation.activeDevices.Remove(_deviceId);
                                }
                            }

                            return;
                        }

                        _EditorTrackedAnnotation hitAnnotation = _findAnnotation(hit);

                        // release
                        if (hitAnnotation.activeDevices.Contains(_deviceId)) {
                            if (hitAnnotation.annotation?.onDragFromEditorRelease != null) {
                                hitAnnotation.annotation.onDragFromEditorRelease(
                                    PointerDragFromEditorReleaseEvent
                                        .fromDragFromEditorEvent(
                                            lastEvent, DragAndDrop.objectReferences, DragAndDrop.paths));
                            }

                            hitAnnotation.activeDevices.Remove(_deviceId);
                        }
                    }
                });
            }

            SchedulerBinding.instance.scheduleFrame();
        }

        void _scheduleDragFromEditorMousePositionCheck() {
            SchedulerBinding.instance.addPostFrameCallback(_ => { collectDragFromEditorMousePositions(); });
            SchedulerBinding.instance.scheduleFrame();
        }

        public void collectDragFromEditorMousePositions() {
            void exitAnnotation(_EditorTrackedAnnotation trackedAnnotation, int deviceId) {
                if (trackedAnnotation.activeDevices.Contains(deviceId)) {
                    if (trackedAnnotation.annotation?.onDragFromEditorExit != null) {
                        trackedAnnotation.annotation.onDragFromEditorExit(
                            PointerDragFromEditorExitEvent.fromDragFromEditorEvent(
                                _lastMouseEvent[deviceId]));
                    }

                    trackedAnnotation.activeDevices.Remove(deviceId);
                }
            }

            void exitAllDevices(_EditorTrackedAnnotation trackedAnnotation) {
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
                EditorMouseTrackerAnnotation hit = annotationFinder(lastEvent.position);

                if (hit == null) {
                    foreach (_EditorTrackedAnnotation trackedAnnotation in _trackedAnnotations.Values) {
                        exitAnnotation(trackedAnnotation, deviceId);
                    }

                    return;
                }

                _EditorTrackedAnnotation hitAnnotation = _findAnnotation(hit);

                // While acrossing two areas, set the flag to true to prevent setting the Pointer Copy VisualMode to None
                // enter
                if (!hitAnnotation.activeDevices.Contains(deviceId)) {
                    hitAnnotation.activeDevices.Add(deviceId);
                    // Both onRelease or onEnter event will enable Copy VisualMode
                    if (hitAnnotation.annotation?.onDragFromEditorRelease != null ||
                        hitAnnotation.annotation?.onDragFromEditorEnter != null) {
                        if (hitAnnotation.annotation?.onDragFromEditorEnter != null) {
                            hitAnnotation.annotation.onDragFromEditorEnter(
                                PointerDragFromEditorEnterEvent
                                    .fromDragFromEditorEvent(lastEvent, DragAndDrop.objectReferences, DragAndDrop.paths));
                        }
                    }
                }

                // hover
                if (hitAnnotation.annotation?.onDragFromEditorHover != null) {
                    hitAnnotation.annotation.onDragFromEditorHover(
                        PointerDragFromEditorHoverEvent.fromDragFromEditorEvent(lastEvent));
                }

                // leave
                foreach (_EditorTrackedAnnotation trackedAnnotation in _trackedAnnotations.Values) {
                    if (hitAnnotation == trackedAnnotation) {
                        continue;
                    }

                    if (trackedAnnotation.activeDevices.Contains(deviceId)) {
                        if (trackedAnnotation.annotation?.onDragFromEditorExit != null) {
                            trackedAnnotation.annotation.onDragFromEditorExit(
                                PointerDragFromEditorExitEvent
                                    .fromDragFromEditorEvent(lastEvent));
                        }

                        trackedAnnotation.activeDevices.Remove(deviceId);
                    }
                }
            }
        }
    }
}