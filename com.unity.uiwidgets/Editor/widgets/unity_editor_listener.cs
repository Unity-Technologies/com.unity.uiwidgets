using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.gestures
{
    public class UnityEditorListener : SingleChildRenderObjectWidget
    {
        public UnityEditorListener(
            Key key = null,
            PointerDragFromEditorEnterEventListener onPointerDragFromEditorEnter = null,
            PointerDragFromEditorHoverEventListener onPointerDragFromEditorHover = null,
            PointerDragFromEditorExitEventListener onPointerDragFromEditorExit = null,
            PointerDragFromEditorReleaseEventListener onPointerDragFromEditorRelease = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            Widget child = null) : base(key: key, child: child)
        {
            this.onPointerDragFromEditorEnter = onPointerDragFromEditorEnter;
            this.onPointerDragFromEditorHover = onPointerDragFromEditorHover;
            this.onPointerDragFromEditorExit = onPointerDragFromEditorExit;
            this.onPointerDragFromEditorRelease = onPointerDragFromEditorRelease;
            this.behavior = behavior;
        }

        public readonly PointerDragFromEditorEnterEventListener onPointerDragFromEditorEnter;
        public readonly PointerDragFromEditorHoverEventListener onPointerDragFromEditorHover;
        public readonly PointerDragFromEditorExitEventListener onPointerDragFromEditorExit;
        public readonly PointerDragFromEditorReleaseEventListener onPointerDragFromEditorRelease;
        public readonly HitTestBehavior behavior;
        
        public override RenderObject createRenderObject(BuildContext context) {
            return new UnityEditorRenderPointerListener(
                onPointerDragFromEditorEnter: onPointerDragFromEditorEnter,
                onPointerDragFromEditorHover: onPointerDragFromEditorHover,
                onPointerDragFromEditorExit: onPointerDragFromEditorExit,
                onPointerDragFromEditorRelease: onPointerDragFromEditorRelease,
                behavior: behavior
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (UnityEditorRenderPointerListener) renderObjectRaw;
            renderObject.behavior = behavior;
            renderObject.onPointerDragFromEditorEnter = onPointerDragFromEditorEnter;
            renderObject.onPointerDragFromEditorHover = onPointerDragFromEditorHover;
            renderObject.onPointerDragFromEditorExit = onPointerDragFromEditorExit;
            renderObject.onPointerDragFromEditorRelease = onPointerDragFromEditorRelease;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            List<string> listeners = new List<string>();
            if (onPointerDragFromEditorEnter != null) {
                listeners.Add("dragFromEditorEnter");
            }

            if (onPointerDragFromEditorHover != null) {
                listeners.Add("dragFromEditorHover");
            }

            if (onPointerDragFromEditorExit != null) {
                listeners.Add("dragFromEditorExit");
            }

            if (onPointerDragFromEditorRelease != null) {
                listeners.Add("dragFromEditorRelease");
            }
            properties.add(new EnumerableProperty<string>("listeners", listeners, ifEmpty: "<none>"));
            properties.add(new EnumProperty<HitTestBehavior>("behavior", behavior));
        }
    }
    
    
    public class UnityEditorRenderPointerListener : RenderProxyBoxWithHitTestBehavior {
    public UnityEditorRenderPointerListener(
        PointerDragFromEditorEnterEventListener onPointerDragFromEditorEnter = null,
        PointerDragFromEditorHoverEventListener onPointerDragFromEditorHover = null,
        PointerDragFromEditorExitEventListener onPointerDragFromEditorExit = null,
        PointerDragFromEditorReleaseEventListener onPointerDragFromEditorRelease = null,
        HitTestBehavior behavior = HitTestBehavior.deferToChild,
        RenderBox child = null
    ) : base(behavior: behavior, child: child) {
        _onPointerDragFromEditorEnter = onPointerDragFromEditorEnter;
        _onPointerDragFromEditorHover = onPointerDragFromEditorHover;
        _onPointerDragFromEditorExit = onPointerDragFromEditorExit;
        _onPointerDragFromEditorRelease = onPointerDragFromEditorRelease;

        if (_onPointerDragFromEditorEnter != null ||
            _onPointerDragFromEditorHover != null ||
            _onPointerDragFromEditorExit != null ||
            _onPointerDragFromEditorRelease != null
        ) {
            _hoverAnnotation = new EditorMouseTrackerAnnotation(
                onDragFromEditorEnter: _onPointerDragFromEditorEnter,
                onDragFromEditorHover: _onPointerDragFromEditorHover,
                onDragFromEditorExit: _onPointerDragFromEditorExit,
                onDragFromEditorRelease: _onPointerDragFromEditorRelease
            );
        }
    }

    PointerDragFromEditorEnterEventListener _onPointerDragFromEditorEnter;

    public PointerDragFromEditorEnterEventListener onPointerDragFromEditorEnter {
        get { return _onPointerDragFromEditorEnter; }
        set {
            if (_onPointerDragFromEditorEnter != value) {
                _onPointerDragFromEditorEnter = value;
                _updateAnnotations();
            }
        }
    }

    PointerDragFromEditorExitEventListener _onPointerDragFromEditorExit;

    public PointerDragFromEditorExitEventListener onPointerDragFromEditorExit {
        get { return _onPointerDragFromEditorExit; }
        set {
            if (_onPointerDragFromEditorExit != value) {
                _onPointerDragFromEditorExit = value;
                _updateAnnotations();
            }
        }
    }

    PointerDragFromEditorHoverEventListener _onPointerDragFromEditorHover;

    public PointerDragFromEditorHoverEventListener onPointerDragFromEditorHover {
        get { return _onPointerDragFromEditorHover; }
        set {
            if (_onPointerDragFromEditorHover != value) {
                _onPointerDragFromEditorHover = value;
                _updateAnnotations();
            }
        }
    }

    PointerDragFromEditorReleaseEventListener _onPointerDragFromEditorRelease;

    public PointerDragFromEditorReleaseEventListener onPointerDragFromEditorRelease {
        get { return _onPointerDragFromEditorRelease; }
        set {
            if (_onPointerDragFromEditorRelease != value) {
                _onPointerDragFromEditorRelease = value;
                _updateAnnotations();
            }
        }
    }

    EditorMouseTrackerAnnotation _hoverAnnotation;

        public EditorMouseTrackerAnnotation hoverAnnotation {
            get { return _hoverAnnotation; }
        }

        void _updateAnnotations() {
            D.assert(_onPointerDragFromEditorEnter != _hoverAnnotation.onDragFromEditorEnter
                     || _onPointerDragFromEditorHover != _hoverAnnotation.onDragFromEditorHover
                     || _onPointerDragFromEditorExit != _hoverAnnotation.onDragFromEditorExit
                     || _onPointerDragFromEditorRelease != _hoverAnnotation.onDragFromEditorRelease
                     , () => "Shouldn't call _updateAnnotations if nothing has changed.");

            if (_hoverAnnotation != null && attached) {
                UiWidgetsEditorBinding.instance.editorMouseTracker.detachDragFromEditorAnnotation(_hoverAnnotation);
            }

            if (_onPointerDragFromEditorEnter != null
                || _onPointerDragFromEditorHover != null
                || _onPointerDragFromEditorExit != null
                || _onPointerDragFromEditorRelease != null
            ) {
                _hoverAnnotation = new EditorMouseTrackerAnnotation(
                    onDragFromEditorEnter: _onPointerDragFromEditorEnter
                    , onDragFromEditorHover: _onPointerDragFromEditorHover
                    , onDragFromEditorExit: _onPointerDragFromEditorExit
                    , onDragFromEditorRelease: _onPointerDragFromEditorRelease
                );

                if (attached) {
                    UiWidgetsEditorBinding.instance.editorMouseTracker.attachDragFromEditorAnnotation(_hoverAnnotation);
                }
            }
            else {
                _hoverAnnotation = null;
            }

            markNeedsPaint();
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (_hoverAnnotation != null) {
                UiWidgetsEditorBinding.instance.editorMouseTracker.attachDragFromEditorAnnotation(_hoverAnnotation);
            }
        }

        public override void detach() {
            base.detach();
            if (_hoverAnnotation != null) {
                UiWidgetsEditorBinding.instance.editorMouseTracker.detachDragFromEditorAnnotation(_hoverAnnotation);
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_hoverAnnotation != null) {
                AnnotatedRegionLayer<EditorMouseTrackerAnnotation> layer = new AnnotatedRegionLayer<EditorMouseTrackerAnnotation>(
                    _hoverAnnotation, size: size, offset: offset);

                context.pushLayer(layer, base.paint, offset);
            }
            else {
                base.paint(context, offset);
            }
        }

        protected override void performResize() {
            size = constraints.biggest;
        }
    }
}