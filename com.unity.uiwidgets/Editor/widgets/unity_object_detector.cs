using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace Unity.UIWidgets.gestures
{
    public delegate void DragInEditorEnterCallback(DragFromEditorDetails details);

    public delegate void DragInEditorHoverCallback(DragFromHoverEditorDetails details);

    public delegate void DragInEditorExitCallback();

    public delegate void DragInEditorReleaseCallback(DragFromEditorDetails details);

    public class DragFromEditorDetails {
        public DragFromEditorDetails(Object[] objectReferences, string[] paths, Offset position) {
            this.objectReferences = objectReferences;
            this.paths = paths;
            this.position = position;
        }

        public readonly Object[] objectReferences;
        public readonly string[] paths;
        public readonly Offset position;
    }
    public class DragFromHoverEditorDetails {
        public DragFromHoverEditorDetails( Offset position) {
        
            this.position = position;
        }
        public readonly Offset position;
    }

    public class UnityObjectDetector : StatefulWidget {
        public UnityObjectDetector(
            Key key = null,
            Widget child = null,
            DragInEditorEnterCallback onEnter = null,
            DragInEditorHoverCallback onHover = null,
            DragInEditorExitCallback onExit = null,
            DragInEditorReleaseCallback onRelease = null,
            HitTestBehavior? behavior = null
        ) : base(key: key) {
            this.child = child;
            onDragInEditorEnter = onEnter;
            onDragInEditorHover = onHover;
            onDragInEditorExit = onExit;
            onDragInEditorRelease = onRelease;
            this.behavior = behavior;
        }

        public readonly Widget child;

        public readonly DragInEditorEnterCallback onDragInEditorEnter;
        public readonly DragInEditorHoverCallback onDragInEditorHover;
        public readonly DragInEditorExitCallback onDragInEditorExit;
        public readonly DragInEditorReleaseCallback onDragInEditorRelease;

        public readonly HitTestBehavior? behavior;

        public override State createState() {
            return new UnityObjectDetectorState();
        }
    }

    public class UnityObjectDetectorState : State<UnityObjectDetector> {
        HitTestBehavior _defaultBehavior {
            get { return widget.child == null ? HitTestBehavior.translucent : HitTestBehavior.deferToChild; }
        }
        
        private Object[] objectReferences;

        private string[] paths;

        private Offset position;

        private bool isUnityObjectDragging()
        {
            var dragObjects = DragAndDrop.objectReferences;
            var dragPaths = DragAndDrop.paths;

            if (dragObjects != null && dragObjects.Length == 1 ||
                dragPaths != null && dragPaths.Length == 1)
            {
                return true;
            }

            return false;
        }

        public override Widget build(BuildContext context) 
        {
            Widget result = new UnityEditorListener(
                child: widget.child,
                onPointerDragFromEditorEnter: widget.onDragInEditorEnter == null
                    ? ((PointerDragFromEditorEnterEventListener) null)
                    : evt => { widget.onDragInEditorEnter.Invoke(new DragFromEditorDetails(evt.objectReferences, evt.paths, evt.position)); },
                onPointerDragFromEditorHover: widget.onDragInEditorHover == null
                    ? ((PointerDragFromEditorHoverEventListener) null)
                    : evt => {
                       widget.onDragInEditorHover.Invoke(new DragFromHoverEditorDetails(evt.position));
                    },
                onPointerDragFromEditorExit: widget.onDragInEditorExit == null
                    ? ((PointerDragFromEditorExitEventListener) null)
                    : evt => { widget.onDragInEditorExit.Invoke(); },
                onPointerDragFromEditorRelease: widget.onDragInEditorRelease == null
                    ? ((PointerDragFromEditorReleaseEventListener) null)
                    : evt => {
                        widget.onDragInEditorRelease.Invoke(new DragFromEditorDetails(evt.objectReferences, evt.paths, evt.position));
                    },
                behavior: widget.behavior ?? _defaultBehavior
            );
            return result;
        }
    }
}