using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public delegate bool DragTargetWillAccept<T>(T data);

    public delegate void DragTargetAccept<T>(T data);

    public delegate Widget DragTargetBuilder<T>(BuildContext context, List<T> candidateData, List<T> rejectedData);

    public delegate void DraggableCanceledCallback(Velocity velocity, Offset offset);

    public delegate void DragEndCallback(DraggableDetails details);

    public delegate void DragTargetLeave<T>(T data);

    public enum DragAnchor {
        child,
        pointer
    }

    static class _DragUtils {
        public static List<T> _mapAvatarsToData<T>(List<_DragAvatar<T>> avatars) {
            List<T> ret = new List<T>(avatars.Count);
            foreach (var avatar in avatars) {
                ret.Add(avatar.data);
            }

            return ret;
        }
    }

    /**
     * @description: Define a draggable item(short press to drag).
     */
    public class Draggable<T> : StatefulWidget {
        readonly Axis? affinity;

        public readonly Axis? axis;

        public readonly Widget child;

        public readonly Widget childWhenDragging;

        public readonly T data;

        public readonly DragAnchor dragAnchor;

        public readonly Widget feedback;

        public readonly Offset feedbackOffset;

        public readonly int? maxSimultaneousDrags;

        public readonly VoidCallback onDragCompleted;

        public readonly DragEndCallback onDragEnd;

        public readonly DraggableCanceledCallback onDraggableCanceled;

        public readonly VoidCallback onDragStarted;

        public Draggable(
            Key key = null,
            Widget child = null,
            Widget feedback = null,
            T data = default,
            Axis? axis = null,
            Widget childWhenDragging = null,
            Offset feedbackOffset = null,
            DragAnchor dragAnchor = DragAnchor.child,
            Axis? affinity = null,
            int? maxSimultaneousDrags = null,
            VoidCallback onDragStarted = null,
            DraggableCanceledCallback onDraggableCanceled = null,
            DragEndCallback onDragEnd = null,
            VoidCallback onDragCompleted = null
        ) : base(key) {
            D.assert(child != null);
            D.assert(feedback != null);
            D.assert(maxSimultaneousDrags == null || maxSimultaneousDrags >= 0);

            this.child = child;
            this.feedback = feedback;
            this.data = data;
            this.axis = axis;
            this.childWhenDragging = childWhenDragging;
            this.feedbackOffset = feedbackOffset ?? Offset.zero;
            this.dragAnchor = dragAnchor;
            this.affinity = affinity;
            this.maxSimultaneousDrags = maxSimultaneousDrags;
            this.onDragStarted = onDragStarted;
            this.onDraggableCanceled = onDraggableCanceled;
            this.onDragEnd = onDragEnd;
            this.onDragCompleted = onDragCompleted;
        }


        public virtual GestureRecognizer createRecognizer(GestureMultiDragStartCallback onStart) {
            switch (affinity) {
                case Axis.horizontal: {
                    return new HorizontalMultiDragGestureRecognizer(this) {onStart = onStart};
                }
                case Axis.vertical: {
                    return new VerticalMultiDragGestureRecognizer(this) {onStart = onStart};
                }
            }

            return new ImmediateMultiDragGestureRecognizer(this) {onStart = onStart};
        }

        public override State createState() {
            return new _DraggableState<T>();
        }
    }

    /**
     * @description: Define a draggable item(long press to drag).
     */
    public class LongPressDraggable<T> : Draggable<T> {
        public LongPressDraggable(
            Key key = null,
            Widget child = null,
            Widget feedback = null,
            T data = default,
            Axis? axis = null,
            Widget childWhenDragging = null,
            Offset feedbackOffset = null,
            DragAnchor dragAnchor = DragAnchor.child,
            int? maxSimultaneousDrags = null,
            VoidCallback onDragStarted = null,
            DraggableCanceledCallback onDraggableCanceled = null,
            DragEndCallback onDragEnd = null,
            VoidCallback onDragCompleted = null
        ) : base(
            key: key,
            child: child,
            feedback: feedback,
            data: data,
            axis: axis,
            childWhenDragging: childWhenDragging,
            feedbackOffset: feedbackOffset,
            dragAnchor: dragAnchor,
            maxSimultaneousDrags: maxSimultaneousDrags,
            onDragStarted: onDragStarted,
            onDraggableCanceled: onDraggableCanceled,
            onDragEnd: onDragEnd,
            onDragCompleted: onDragCompleted
        ) {
        }

        public override GestureRecognizer createRecognizer(GestureMultiDragStartCallback onStart) {
            return new DelayedMultiDragGestureRecognizer(Constants.kLongPressTimeout) {
                onStart = (Offset position) => {
                    Drag result = onStart(position);
                    return result;
                }
            };
        }
    }

    /**
     * @description: Define the state of a draggable item
     */
    public class _DraggableState<T> : State<Draggable<T>> {
        int _activeCount;

        GestureRecognizer _recognizer;

        public override void initState() {
            base.initState();
            _recognizer = widget.createRecognizer(_startDrag);
        }

        public override void dispose() {
            _disposeRecognizerIfInactive();
            base.dispose();
        }

        void _disposeRecognizerIfInactive() {
            if (_activeCount > 0) {
                return;
            }

            _recognizer.dispose();
            _recognizer = null;
        }


        void _routePointer(PointerDownEvent pEvent) {
            if (widget.maxSimultaneousDrags != null &&
                _activeCount >= widget.maxSimultaneousDrags) {
                return;
            }

            if (pEvent is PointerDownEvent) {
                _recognizer.addPointer((PointerDownEvent) pEvent);
            }
        }

        _DragAvatar<T> _startDrag(Offset position) {
            if (widget.maxSimultaneousDrags != null &&
                _activeCount >= widget.maxSimultaneousDrags) {
                return null;
            }

            var dragStartPoint = Offset.zero;
            switch (widget.dragAnchor) {
                case DragAnchor.child:
                    RenderBox renderObject = context.findRenderObject() as RenderBox;
                    dragStartPoint = renderObject.globalToLocal(position);
                    break;
                case DragAnchor.pointer:
                    dragStartPoint = Offset.zero;
                    break;
            }

            setState(() => { _activeCount += 1; });

            _DragAvatar<T> avatar = new _DragAvatar<T>(
                overlayState: Overlay.of(context, debugRequiredFor: widget),
                data: widget.data,
                axis: widget.axis,
                initialPosition: position,
                dragStartPoint: dragStartPoint,
                feedback: widget.feedback,
                feedbackOffset: widget.feedbackOffset,
                onDragEnd: (Velocity velocity, Offset offset, bool wasAccepted) => {
                    if (mounted) {
                        setState(() => { _activeCount -= 1; });
                    }
                    else {
                        _activeCount -= 1;
                        _disposeRecognizerIfInactive();
                    }

                    if (mounted && widget.onDragEnd != null) {
                        widget.onDragEnd(new DraggableDetails(
                            wasAccepted: wasAccepted,
                            velocity: velocity,
                            offset: offset
                        ));
                    }

                    if (wasAccepted && widget.onDragCompleted != null) {
                        widget.onDragCompleted();
                    }

                    if (!wasAccepted && widget.onDraggableCanceled != null) {
                        widget.onDraggableCanceled(velocity, offset);
                    }
                }
            );
            if (widget.onDragStarted != null) {
                widget.onDragStarted();
            }

            return avatar;
        }

        public override Widget build(BuildContext context) {
            D.assert(Overlay.of(context, debugRequiredFor: widget) != null);
            bool canDrag = widget.maxSimultaneousDrags == null ||
                           _activeCount < widget.maxSimultaneousDrags;

            bool showChild = _activeCount == 0 || widget.childWhenDragging == null;
            if (canDrag) {
                return new Listener(
                    onPointerDown: _routePointer,
                    child: showChild ? widget.child : widget.childWhenDragging
                );
            }

            return new Listener(
                child: showChild ? widget.child : widget.childWhenDragging);
        }
    }

    /**
     * @description: Define details of a draggable item.
     */
    public class DraggableDetails {
        public readonly Offset offset;

        public readonly Velocity velocity;

        public readonly bool wasAccepted;

        public DraggableDetails(
            bool wasAccepted = false,
            Velocity velocity = null,
            Offset offset = null
        ) {
            D.assert(velocity != null);
            D.assert(offset != null);
            this.wasAccepted = wasAccepted;
            this.velocity = velocity;
            this.offset = offset;
        }
    }

    /**
     * @description: Define drag target. Could interact with a draggable item in the event of "enter", "leave" or "drop".
     *               When start dragging, _startDrag in _DraggableState is called and return a dragAvatar of that dragging item. 
     *               updateDrag in _DragAvatar is called whenever the item is dragged and not called when the item stay unmoved.
     *               In updateDrag, didEnter is called when new draggable item enter a drag target(detect by hittest)
     *               didLeave is called in updateDrag if draggable is dragged through the drag target, and is called in finishDrag(didDrop is called if finish dragging by dropping the item). 
     */
    public class DragTarget<T> : StatefulWidget {
        public readonly DragTargetBuilder<T> builder;

        public readonly DragTargetAccept<T> onAccept;

        public readonly DragTargetLeave<T> onLeave;

        public readonly DragTargetWillAccept<T> onWillAccept;

        public DragTarget(
            Key key = null,
            DragTargetBuilder<T> builder = null,
            DragTargetWillAccept<T> onWillAccept = null,
            DragTargetAccept<T> onAccept = null,
            DragTargetLeave<T> onLeave = null
        ) : base(key) {
            D.assert(builder != null);
            this.builder = builder;
            this.onWillAccept = onWillAccept;
            this.onAccept = onAccept;
            this.onLeave = onLeave;
        }

        public override State createState() {
            return new _DragTargetState<T>();
        }
    }

    /**
     * @description: Define drag target state. State change when draggable item enter, leave or drop on the drag target.
     */
    public class _DragTargetState<T> : State<DragTarget<T>> {
        readonly List<_DragAvatar<T>> _candidateAvatars = new List<_DragAvatar<T>>();
        readonly List<_DragAvatar<T>> _rejectedAvatars = new List<_DragAvatar<T>>();

        //Triggered if avatar entered the drag target
        public bool didEnter(_DragAvatar<T> avatar) {
            D.assert(!_candidateAvatars.Contains(avatar));
            D.assert(!_rejectedAvatars.Contains(avatar));

            if (avatar is _DragAvatar<T> && (widget.onWillAccept == null || widget.onWillAccept(avatar.data))) {
                setState(() => { _candidateAvatars.Add(avatar); });
                return true;
            }
            else {
                setState(() => { _rejectedAvatars.Add(avatar); });
                return false;
            }
        }

        //Triggered if avatar leaved the drag target
        public void didLeave(_DragAvatar<T> avatar) {
            D.assert(_candidateAvatars.Contains(avatar) || _rejectedAvatars.Contains(avatar));
            if (!mounted) {
                return;
            }

            setState(() => {
                _candidateAvatars.Remove(avatar);
                _rejectedAvatars.Remove(avatar);
            });
            if (widget.onLeave != null) {
                widget.onLeave(avatar.data);
            }
        }

        //Triggered if avatar is dropped at drag target
        public void didDrop(_DragAvatar<T> avatar) {
            D.assert(_candidateAvatars.Contains(avatar));
            if (!mounted) {
                return;
            }

            setState(() => { _candidateAvatars.Remove(avatar); });
            //If set the onAccept callback, return the avatar data
            if (widget.onAccept != null) {
                widget.onAccept(avatar.data);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(widget.builder != null);
            return new MetaData(
                metaData: this,
                behavior: HitTestBehavior.translucent,
                child: widget.builder(context, _DragUtils._mapAvatarsToData(_candidateAvatars),
                    _DragUtils._mapAvatarsToData(_rejectedAvatars)));
        }
    }


    public enum _DragEndKind {
        dropped,
        canceled
    }

    public delegate void _OnDragEnd(Velocity velocity, Offset offset, bool wasAccepted);

    /**
     * @description: Define drag avatar. Is created in _DraggableState when start dragging
     */
    public class _DragAvatar<T> : Drag {
        readonly List<_DragTargetState<T>> _enteredTargets = new List<_DragTargetState<T>>();

        readonly Axis? axis;

        public readonly T data;

        readonly Offset dragStartPoint;

        readonly Widget feedback;

        readonly Offset feedbackOffset;

        readonly _OnDragEnd onDragEnd;

        readonly OverlayState overlayState;

        _DragTargetState<T> _activeTarget;

        OverlayEntry _entry;

        Offset _lastOffset;

        Offset _position;

        public _DragAvatar(
            OverlayState overlayState,
            T data = default,
            Axis? axis = null,
            Offset initialPosition = null,
            Offset dragStartPoint = null,
            Widget feedback = null,
            Offset feedbackOffset = null,
            _OnDragEnd onDragEnd = null
        ) {
            if (feedbackOffset == null) {
                feedbackOffset = Offset.zero;
            }

            D.assert(overlayState != null);
            this.overlayState = overlayState;
            this.data = data;
            this.axis = axis;
            this.dragStartPoint = dragStartPoint ?? Offset.zero;
            this.feedback = feedback;
            this.feedbackOffset = feedbackOffset ?? Offset.zero;
            this.onDragEnd = onDragEnd;

            _entry = new OverlayEntry(_build);
            this.overlayState.insert(_entry);
            _position = initialPosition ?? Offset.zero;
            updateDrag(initialPosition);
        }

        public void update(DragUpdateDetails details) {
            _position += _restrictAxis(details.delta);
            updateDrag(_position);
        }

        public void end(DragEndDetails details) {
            finishDrag(_DragEndKind.dropped, _restrictVelocityAxis(details.velocity));
        }

        public void cancel() {
            finishDrag(_DragEndKind.canceled);
        }

        void updateDrag(Offset globalPosition) {
            _lastOffset = globalPosition - dragStartPoint;
            _entry.markNeedsBuild();

            HitTestResult result = new HitTestResult();
            WidgetsBinding.instance.hitTest(result, globalPosition + feedbackOffset);

            List<_DragTargetState<T>> targets = _getDragTargets(result.path);

            bool listsMatch = false;
            if (targets.Count >= _enteredTargets.Count && _enteredTargets.isNotEmpty()) {
                listsMatch = true;
                List<_DragTargetState<T>>.Enumerator iterator = targets.GetEnumerator();
                for (int i = 0; i < _enteredTargets.Count; i++) {
                    iterator.MoveNext();
                    if (iterator.Current != _enteredTargets[i]) {
                        listsMatch = false;
                        break;
                    }
                }
            }

            if (listsMatch) {
                return;
            }
            
            _leaveAllEntered();

            _DragTargetState<T> newTarget = null;
            foreach (var target in targets) {
                _enteredTargets.Add(target);
                if (target.didEnter(this)) {
                    newTarget = target;
                    break;
                }
            }

            _activeTarget = newTarget;
        }

        List<_DragTargetState<T>> _getDragTargets(IList<HitTestEntry> path) {
            List<_DragTargetState<T>> ret = new List<_DragTargetState<T>>();

            foreach (HitTestEntry entry in path) {
                if (entry.target is RenderMetaData) {
                    RenderMetaData renderMetaData = (RenderMetaData) entry.target;
                    if (renderMetaData.metaData is _DragTargetState<T>) {
                        ret.Add((_DragTargetState<T>) renderMetaData.metaData);
                    }
                }
            }

            return ret;
        }

        void _leaveAllEntered() {
            for (int i = 0; i < _enteredTargets.Count; i++) {
                _enteredTargets[i].didLeave(this);
            }

            _enteredTargets.Clear();
        }
        
        /**
         * @description: Finish dragging the draggable.
         */
        void finishDrag(_DragEndKind endKind, Velocity velocity = null) {
            bool wasAccepted = false;
            //If finish drag by dropping the avatar, and avatar has entered any drag target, trigger didDrop
            if (endKind == _DragEndKind.dropped && _activeTarget != null) {
                _activeTarget.didDrop(this);
                wasAccepted = true;
                _enteredTargets.Remove(_activeTarget);
            }

            _leaveAllEntered();
            _activeTarget = null;
            _entry.remove();
            _entry = null;

            if (onDragEnd != null) {
                onDragEnd(velocity == null ? Velocity.zero : velocity, _lastOffset, wasAccepted);
            }
        }

        public Widget _build(BuildContext context) {
            RenderBox box = overlayState.context.findRenderObject() as RenderBox;
            Offset overlayTopLeft = box.localToGlobal(Offset.zero);
            return new Positioned(
                left: _lastOffset.dx - overlayTopLeft.dx,
                top: _lastOffset.dy - overlayTopLeft.dy,
                child: new IgnorePointer(
                    child: feedback
                )
            );
        }

        Velocity _restrictVelocityAxis(Velocity velocity) {
            if (axis == null) {
                return velocity;
            }

            return new Velocity(
                _restrictAxis(velocity.pixelsPerSecond));
        }

        Offset _restrictAxis(Offset offset) {
            if (axis == null) {
                return offset;
            }

            if (axis == Axis.horizontal) {
                return new Offset(offset.dx, 0.0f);
            }

            return new Offset(0.0f, offset.dy);
        }
    }
}