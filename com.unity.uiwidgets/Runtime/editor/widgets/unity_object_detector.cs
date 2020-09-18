using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.editor {
    public delegate void DragFromEditorEnterCallback();

    public delegate void DragFromEditorHoverCallback();

    public delegate void DragFromEditorExitCallback();

    public delegate void DragFromEditorReleaseCallback(DragFromEditorDetails details);

    public class DragFromEditorDetails {
        public DragFromEditorDetails(Object[] objectReferences) {
            this.objectReferences = objectReferences;
        }

        public readonly Object[] objectReferences;
    }

    public class UnityObjectDetector : StatefulWidget {
        public UnityObjectDetector(
            Key key = null,
            Widget child = null,
            DragFromEditorEnterCallback onEnter = null,
            DragFromEditorHoverCallback onHover = null,
            DragFromEditorExitCallback onExit = null,
            DragFromEditorReleaseCallback onRelease = null,
            HitTestBehavior? behavior = null
        ) : base(key: key) {
            this.child = child;
            onDragFromEditorEnter = onEnter;
            onDragFromEditorHover = onHover;
            onDragFromEditorExit = onExit;
            onDragFromEditorRelease = onRelease;
            this.behavior = behavior;
        }

        public readonly Widget child;

        public readonly DragFromEditorEnterCallback onDragFromEditorEnter;
        public readonly DragFromEditorHoverCallback onDragFromEditorHover;
        public readonly DragFromEditorExitCallback onDragFromEditorExit;
        public readonly DragFromEditorReleaseCallback onDragFromEditorRelease;

        public readonly HitTestBehavior? behavior;

        public override State createState() {
            return new UnityObjectDetectorState();
        }
    }

    public class UnityObjectDetectorState : State<UnityObjectDetector> {
        HitTestBehavior _defaultBehavior {
            get { return widget.child == null ? HitTestBehavior.translucent : HitTestBehavior.deferToChild; }
        }

        public override Widget build(BuildContext context) {
            Widget result = new Listener(
                child: widget.child,
                onPointerDragFromEditorEnter: widget.onDragFromEditorEnter == null
                    ? ((PointerDragFromEditorEnterEventListener) null)
                    : (evt) => { widget.onDragFromEditorEnter.Invoke(); },
                onPointerDragFromEditorHover: widget.onDragFromEditorHover == null
                    ? ((PointerDragFromEditorHoverEventListener) null)
                    : (evt) => { widget.onDragFromEditorHover.Invoke(); },
                onPointerDragFromEditorExit: widget.onDragFromEditorExit == null
                    ? ((PointerDragFromEditorExitEventListener) null)
                    : (evt) => { widget.onDragFromEditorExit.Invoke(); },
                onPointerDragFromEditorRelease: widget.onDragFromEditorRelease == null
                    ? ((PointerDragFromEditorReleaseEventListener) null)
                    : (evt) => {
                        widget.onDragFromEditorRelease.Invoke(new DragFromEditorDetails(evt.objectReferences));
                    },
                behavior: widget.behavior ?? _defaultBehavior
            );
            return result;
        }
    }
}