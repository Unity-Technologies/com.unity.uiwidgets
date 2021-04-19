using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public class GestureBinding : BindingBase, HitTestable, HitTestDispatcher, HitTestTarget {

        protected override void initInstances() {
            base.initInstances();
            instance = this;
        }

        public static GestureBinding instance {
            get { return (GestureBinding) Window.instance._binding; }
            private set { Window.instance._binding = value; }
        }

        public GestureBinding() {
            Window.instance.onPointerDataPacket += _handlePointerDataPacket;

            gestureArena = new GestureArenaManager();
        }

        readonly Queue<PointerEvent> _pendingPointerEvents = new Queue<PointerEvent>();

        void _handlePointerDataPacket(PointerDataPacket packet) {
            foreach (var pointerEvent in PointerEventConverter.expand(packet.data, Window.instance.devicePixelRatio)) {
                _pendingPointerEvents.Enqueue(pointerEvent);
            }
 
            _flushPointerEventQueue();
        }

        public void cancelPointer(int pointer) {
            if (_pendingPointerEvents.isEmpty()) {
                Window.instance.scheduleMicrotask(_flushPointerEventQueue);
            }

            _pendingPointerEvents.Enqueue(
                new PointerCancelEvent(timeStamp: _Timer.timespanSinceStartup, pointer: pointer));
        }

        void _flushPointerEventQueue() {
            while (_pendingPointerEvents.Count != 0) {
                _handlePointerEvent(_pendingPointerEvents.Dequeue());
            }
        }

        public readonly PointerRouter pointerRouter = new PointerRouter();

        public readonly GestureArenaManager gestureArena;

        public readonly PointerSignalResolver pointerSignalResolver = new PointerSignalResolver();

        public readonly Dictionary<int, HitTestResult> _hitTests = new Dictionary<int, HitTestResult>();

        public readonly HashSet<HitTestTarget> lastMoveTargets = new HashSet<HitTestTarget>();

        readonly HashSet<HitTestEntry> _enteredTargets = new HashSet<HitTestEntry>();

        void _handlePointerEvent(PointerEvent evt) {
            HitTestResult hitTestResult = null;
            if (evt is PointerDownEvent || evt is PointerSignalEvent) {
                D.assert(!_hitTests.ContainsKey(evt.pointer));
                hitTestResult = new HitTestResult();
                hitTest(hitTestResult, evt.position);
                if (evt is PointerDownEvent) {
                    _hitTests[evt.pointer] = hitTestResult;
                }
                D.assert(() => {
                    if (D.debugPrintHitTestResults) {
                        Debug.LogFormat("{0}: {1}", evt, hitTestResult);
                    }

                    return true;
                });
            }
            else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                hitTestResult = _hitTests.getOrDefault(evt.pointer);
                _hitTests.Remove(evt.pointer);
            }
            else if (evt.down) {
                hitTestResult = _hitTests.getOrDefault(evt.pointer);
            }

            D.assert(() => {
                if (D.debugPrintMouseHoverEvents && evt is PointerHoverEvent) {
                    Debug.LogFormat("{0}", evt);
                }

                return true;
            });

            if (hitTestResult != null ||
                evt is PointerHoverEvent ||
                evt is PointerAddedEvent ||
                evt is PointerRemovedEvent ||
                evt is PointerDragFromEditorHoverEvent ||
                evt is PointerDragFromEditorReleaseEvent
            ) {
                dispatchEvent(evt, hitTestResult);
            }
        }

        void _handlePointerScrollEvent(PointerEvent evt) {
            pointerRouter.clearScrollRoute(evt.pointer);
            if (!pointerRouter.acceptScroll()) {
                return;
            }

            HitTestResult result = new HitTestResult();
            hitTest(result, evt.position);

            dispatchEvent(evt, result);
        }

        public virtual void hitTest(HitTestResult result, Offset position) {
            result.add(new HitTestEntry(this));
        }

        public void dispatchEvent(PointerEvent evt, HitTestResult hitTestResult) {
            if (hitTestResult == null) {
                D.assert(evt is PointerHoverEvent ||
                         evt is PointerAddedEvent ||
                         evt is PointerRemovedEvent ||
                         evt is PointerDragFromEditorHoverEvent ||
                         evt is PointerDragFromEditorReleaseEvent
                );
                try {
                    pointerRouter.route(evt);
                }
                catch (Exception ex) {
                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return new DiagnosticsProperty<PointerEvent>("Event", evt, style: DiagnosticsTreeStyle.errorProperty);
                    }
                    
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                            exception: ex,
                            library: "gesture library",
                            context: new ErrorDescription("while dispatching a non-hit-tested pointer event"),
                            informationCollector: infoCollector
                        )
                    );
                }

                return;
            }

            foreach (HitTestEntry entry in hitTestResult.path) {
                try {
                    entry.target.handleEvent(evt.transformed(entry.transform), entry);
                }
                catch (Exception ex) {
                    D.logError("Error while dispatching a pointer event: ", ex);
                }
            }
        }

        public void handleEvent(PointerEvent evt, HitTestEntry entry) {
            pointerRouter.route(evt);
            if (evt is PointerDownEvent) {
                gestureArena.close(evt.pointer);
            }
            else if (evt is PointerUpEvent) {
                gestureArena.sweep(evt.pointer);
            }
            else if (evt is PointerSignalEvent) {
                pointerSignalResolver.resolve((PointerSignalEvent) evt);
            }
        }
    }
}