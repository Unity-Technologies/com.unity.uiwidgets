using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public delegate Widget ViewportBuilder(BuildContext context, ViewportOffset position);

    public class Scrollable : StatefulWidget {
        public Scrollable(
            Key key = null,
            AxisDirection axisDirection = AxisDirection.down,
            ScrollController controller = null,
            ScrollPhysics physics = null,
            ViewportBuilder viewportBuilder = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(viewportBuilder != null);

            this.axisDirection = axisDirection;
            this.controller = controller;
            this.physics = physics;
            this.viewportBuilder = viewportBuilder;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly AxisDirection axisDirection;

        public readonly ScrollController controller;

        public readonly ScrollPhysics physics;

        public readonly ViewportBuilder viewportBuilder;

        public readonly DragStartBehavior dragStartBehavior;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(axisDirection); }
        }

        public override State createState() {
            return new ScrollableState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection>("axisDirection", axisDirection));
            properties.add(new DiagnosticsProperty<ScrollPhysics>("physics", physics));
        }

        public static ScrollableState of(BuildContext context) {
            _ScrollableScope widget = (_ScrollableScope) context.inheritFromWidgetOfExactType(typeof(_ScrollableScope));
            return widget == null ? null : widget.scrollable;
        }

        public static Future ensureVisible(
            BuildContext context, 
            float alignment = 0.0f,
            TimeSpan? duration = null,
            Curve curve = null,
            ScrollPositionAlignmentPolicy alignmentPolicy = ScrollPositionAlignmentPolicy.explicitPolicy
        ) {
            duration = duration ?? TimeSpan.Zero;
            curve = curve ?? Curves.ease;
            List<Future> futures = new List<Future>();

            ScrollableState scrollable = of(context);
            while (scrollable != null) {
                futures.Add(scrollable.position.ensureVisible(
                    context.findRenderObject(),
                    alignment: alignment,
                    duration: duration,
                    curve: curve,
                    alignmentPolicy: alignmentPolicy
                ));
                context = scrollable.context;
                scrollable = of(context);
            }

            if (futures.isEmpty() || duration == TimeSpan.Zero) {
                return Future.value();
            }

            if (futures.Count == 1) {
                return futures.Single();
            }

            return Future.wait<object>(futures);
        }
    }

    class _ScrollableScope : InheritedWidget {
        internal _ScrollableScope(
            Key key = null,
            ScrollableState scrollable = null,
            ScrollPosition position = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(scrollable != null);
            D.assert(child != null);
            this.scrollable = scrollable;
            this.position = position;
        }

        public readonly ScrollableState scrollable;

        public readonly ScrollPosition position;

        public override bool updateShouldNotify(InheritedWidget old) {
            return position != ((_ScrollableScope) old).position;
        }
    }

    public class ScrollableState : TickerProviderStateMixin<Scrollable>, ScrollContext {
        public ScrollPosition position {
            get { return _position; }
        }

        ScrollPosition _position;

        public AxisDirection axisDirection {
            get { return widget.axisDirection; }
        }

        ScrollBehavior _configuration;

        ScrollPhysics _physics;

        void _updatePosition() {
            _configuration = ScrollConfiguration.of(context);
            _physics = _configuration.getScrollPhysics(context);
            if (widget.physics != null) {
                _physics = widget.physics.applyTo(_physics);
            }

            ScrollController controller = widget.controller;
            ScrollPosition oldPosition = position;
            if (oldPosition != null) {
                if (controller != null) {
                    controller.detach(oldPosition);
                }

                async_.scheduleMicrotask(()=> {
                    oldPosition.dispose();
                    return null;
                });
            }

            _position = controller == null
                ? null
                : controller.createScrollPosition(_physics, this, oldPosition);
            _position = _position
                             ?? new ScrollPositionWithSingleContext(physics: _physics, context: this,
                                 oldPosition: oldPosition);
            D.assert(position != null);
            if (controller != null) {
                controller.attach(position);
            }
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _updatePosition();
        }

        bool _shouldUpdatePosition(Scrollable oldWidget) {
            ScrollPhysics newPhysics = widget.physics;
            ScrollPhysics oldPhysics = oldWidget.physics;
            do {
                Type newPhysicsType = newPhysics != null ? newPhysics.GetType() : null;
                Type oldPhysicsType = oldPhysics != null ? oldPhysics.GetType() : null;

                if (newPhysicsType != oldPhysicsType) {
                    return true;
                }

                if (newPhysics != null) {
                    newPhysics = newPhysics.parent;
                }

                if (oldPhysics != null) {
                    oldPhysics = oldPhysics.parent;
                }
            } while (newPhysics != null || oldPhysics != null);

            Type controllerType = widget.controller == null ? null : widget.controller.GetType();
            Type oldControllerType = oldWidget.controller == null ? null : oldWidget.controller.GetType();
            return controllerType != oldControllerType;
        }

        public override void didUpdateWidget(StatefulWidget oldWidgetRaw) {
            Scrollable oldWidget = (Scrollable) oldWidgetRaw;
            base.didUpdateWidget(oldWidget);

            if (widget.controller != oldWidget.controller) {
                if (oldWidget.controller != null) {
                    oldWidget.controller.detach(position);
                }

                if (widget.controller != null) {
                    widget.controller.attach(position);
                }
            }

            if (_shouldUpdatePosition(oldWidget)) {
                _updatePosition();
            }
        }

        public override void dispose() {
            if (widget.controller != null) {
                widget.controller.detach(position);
            }

            position.dispose();
            base.dispose();
        }

        readonly GlobalKey<RawGestureDetectorState> _gestureDetectorKey = GlobalKey<RawGestureDetectorState>.key();
        readonly GlobalKey _ignorePointerKey = GlobalKey.key();

        Dictionary<Type, GestureRecognizerFactory> _gestureRecognizers =
            new Dictionary<Type, GestureRecognizerFactory>();

        bool _shouldIgnorePointer = false;

        bool _lastCanDrag;
        Axis _lastAxisDirection;

        public void setCanDrag(bool canDrag) {
            if (canDrag == _lastCanDrag && (!canDrag || widget.axis == _lastAxisDirection)) {
                return;
            }

            if (!canDrag) {
                _gestureRecognizers = new Dictionary<Type, GestureRecognizerFactory>();
            }
            else {
                switch (widget.axis) {
                    case Axis.vertical:
                        _gestureRecognizers = new Dictionary<Type, GestureRecognizerFactory>();
                        _gestureRecognizers.Add(typeof(VerticalDragGestureRecognizer),
                            new GestureRecognizerFactoryWithHandlers<VerticalDragGestureRecognizer>(
                                () => new VerticalDragGestureRecognizer(),
                                instance => {
                                    instance.onDown = _handleDragDown;
                                    instance.onStart = _handleDragStart;
                                    instance.onUpdate = _handleDragUpdate;
                                    instance.onEnd = _handleDragEnd;
                                    instance.onCancel = _handleDragCancel;
                                    instance.minFlingDistance =
                                        _physics == null ? (float?) null : _physics.minFlingDistance;
                                    instance.minFlingVelocity =
                                        _physics == null ? (float?) null : _physics.minFlingVelocity;
                                    instance.maxFlingVelocity =
                                        _physics == null ? (float?) null : _physics.maxFlingVelocity;
                                    instance.dragStartBehavior = widget.dragStartBehavior;
                                }
                            ));
                        break;
                    case Axis.horizontal:
                        _gestureRecognizers = new Dictionary<Type, GestureRecognizerFactory>();
                        _gestureRecognizers.Add(typeof(HorizontalDragGestureRecognizer),
                            new GestureRecognizerFactoryWithHandlers<HorizontalDragGestureRecognizer>(
                                () => new HorizontalDragGestureRecognizer(),
                                instance => {
                                    instance.onDown = _handleDragDown;
                                    instance.onStart = _handleDragStart;
                                    instance.onUpdate = _handleDragUpdate;
                                    instance.onEnd = _handleDragEnd;
                                    instance.onCancel = _handleDragCancel;
                                    instance.minFlingDistance =
                                        _physics == null ? (float?) null : _physics.minFlingDistance;
                                    instance.minFlingVelocity =
                                        _physics == null ? (float?) null : _physics.minFlingVelocity;
                                    instance.maxFlingVelocity =
                                        _physics == null ? (float?) null : _physics.maxFlingVelocity;
                                    instance.dragStartBehavior = widget.dragStartBehavior;
                                }
                            ));
                        break;
                }
            }

            _lastCanDrag = canDrag;
            _lastAxisDirection = widget.axis;
            if (_gestureDetectorKey.currentState != null) {
                _gestureDetectorKey.currentState.replaceGestureRecognizers(_gestureRecognizers);
            }
        }

        public TickerProvider vsync {
            get { return this; }
        }

        public void setIgnorePointer(bool value) {
            if (_shouldIgnorePointer == value) {
                return;
            }

            _shouldIgnorePointer = value;
            if (_ignorePointerKey.currentContext != null) {
                var renderBox = (RenderIgnorePointer) _ignorePointerKey.currentContext.findRenderObject();
                renderBox.ignoring = _shouldIgnorePointer;
            }
        }

        public BuildContext notificationContext {
            get { return _gestureDetectorKey.currentContext; }
        }

        public BuildContext storageContext {
            get { return context; }
        }

        Drag _drag;

        ScrollHoldController _hold;

        void _handleDragDown(DragDownDetails details) {
            D.assert(_drag == null);
            D.assert(_hold == null);
            _hold = position.hold(_disposeHold);
        }

        void _handleDragStart(DragStartDetails details) {
            D.assert(_drag == null);
            _drag = position.drag(details, _disposeDrag);
            D.assert(_drag != null);
            D.assert(_hold == null);
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            D.assert(_hold == null || _drag == null);
            if (_drag != null) {
                _drag.update(details);
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            D.assert(_hold == null || _drag == null);
            if (_drag != null) {
                _drag.end(details);
            }

            D.assert(_drag == null);
        }

        void _handleDragCancel() {
            D.assert(_hold == null || _drag == null);
            if (_hold != null) {
                _hold.cancel();
            }

            if (_drag != null) {
                _drag.cancel();
            }

            D.assert(_hold == null);
            D.assert(_drag == null);
        }
        
        float _targetScrollOffsetForPointerScroll(PointerScrollEvent e) {
            float delta = widget.axis == Axis.horizontal ? e.delta.dx : e.delta.dy;
            return Mathf.Min(Mathf.Max(position.pixels + delta, position.minScrollExtent),
                position.maxScrollExtent);
        }
        
        void _receivedPointerSignal(PointerSignalEvent e) {
            if (e is PointerScrollEvent && position != null) {
                float targetScrollOffset = _targetScrollOffsetForPointerScroll(e as PointerScrollEvent);
                if (targetScrollOffset != position.pixels) {
                    GestureBinding.instance.pointerSignalResolver.register(e, _handlePointerScroll);
                }
            }
        }

        void _handlePointerScroll(PointerEvent e) {
            D.assert(e is PointerScrollEvent);
            float targetScrollOffset = _targetScrollOffsetForPointerScroll(e as PointerScrollEvent);
            if (targetScrollOffset != position.pixels) {
                position.jumpTo(targetScrollOffset);
            }
        }

        void _disposeHold() {
            _hold = null;
        }

        void _disposeDrag() {
            _drag = null;
        }

        public override Widget build(BuildContext context) {
            D.assert(position != null);

            Widget result = new _ScrollableScope(
                scrollable: this,
                position: position,
                child: new Listener(
                    onPointerSignal: _receivedPointerSignal,
                    child: new RawGestureDetector(
                        key: _gestureDetectorKey,
                        gestures: _gestureRecognizers,
                        behavior: HitTestBehavior.opaque,
                        child: new IgnorePointer(
                            key: _ignorePointerKey,
                            ignoring: _shouldIgnorePointer,
                            child: widget.viewportBuilder(context, position)
                        )
                    )
                )
            );

            return _configuration.buildViewportChrome(context, result, widget.axisDirection);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ScrollPosition>("position", position));
        }
    }
}