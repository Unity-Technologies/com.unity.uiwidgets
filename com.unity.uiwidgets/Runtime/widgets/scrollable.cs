using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public delegate Widget ViewportBuilder(BuildContext context, ViewportOffset position);

    public class Scrollable : StatefulWidget {
        public Scrollable(
            Key key = null,
            AxisDirection? axisDirection = AxisDirection.down,
            ScrollController controller = null,
            ScrollPhysics physics = null,
            ViewportBuilder viewportBuilder = null,
            ScrollIncrementCalculator incrementCalculator = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(axisDirection != null);
            D.assert(viewportBuilder != null);

            this.axisDirection = axisDirection.Value;
            this.controller = controller;
            this.physics = physics;
            this.viewportBuilder = viewportBuilder;
            this.incrementCalculator = incrementCalculator;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly AxisDirection axisDirection;

        public readonly ScrollController controller;

        public readonly ScrollPhysics physics;

        public readonly ViewportBuilder viewportBuilder;

        public readonly ScrollIncrementCalculator incrementCalculator;

        public readonly DragStartBehavior dragStartBehavior;

        public Axis? axis {
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
            _ScrollableScope widget = context.dependOnInheritedWidgetOfExactType<_ScrollableScope>();
            return widget == null ? null : widget.scrollable;
        }

        public static bool recommendDeferredLoadingForContext(BuildContext context) {
            _ScrollableScope widget = context.getElementForInheritedWidgetOfExactType<_ScrollableScope>()?.widget as _ScrollableScope;
            if (widget == null) {
                return false;
            }

            return widget.position.recommendDeferredLoading(context);
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

            if (controller == null) {
                _position = new ScrollPositionWithSingleContext(physics: _physics, context: this, oldPosition: oldPosition);
                
            }
            else {
                _position = controller?.createScrollPosition(_physics, this, oldPosition)
                            ?? new ScrollPositionWithSingleContext(physics: _physics, context: this, oldPosition: oldPosition);
            }

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
        Axis? _lastAxisDirection;

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
            float delta = widget.axis == Axis.horizontal
                ? e.scrollDelta.dx
                : e.scrollDelta.dy;

            if (AxisUtils.axisDirectionIsReversed(widget.axisDirection)) {
                delta *= -1;
            }

            return Mathf.Min(Mathf.Max(position.pixels + delta, position.minScrollExtent),
                position.maxScrollExtent);
        }
        
        float _pointerSignalEventDelta(PointerScrollEvent evt) {
            float delta = widget.axis == Axis.horizontal
                ? evt.scrollDelta.dx
                : evt.scrollDelta.dy;

            if (AxisUtils.axisDirectionIsReversed(widget.axisDirection)) {
                delta *= -1;
            }
            return delta;
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

            if (_physics != null && !_physics.shouldAcceptUserOffset(position)) {
                return;
            }
            
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

    public delegate float ScrollIncrementCalculator(ScrollIncrementDetails details);

    public enum ScrollIncrementType {
        line,
        page
    }

    public class ScrollIncrementDetails {
        public ScrollIncrementDetails(
            ScrollIncrementType type,
            ScrollMetrics metrics
        ) {
            D.assert(metrics != null);
            this.type = type;
            this.metrics = metrics;
        }

        public readonly ScrollIncrementType type;

        public readonly ScrollMetrics metrics;
    }

    public class ScrollIntent : Intent {
        public ScrollIntent(
            AxisDirection direction,
            ScrollIncrementType type = ScrollIncrementType.line
        ) : base (ScrollAction.key) {
            this.direction = direction;
            this.type = type;
        }

        public readonly AxisDirection direction;

        public readonly ScrollIncrementType type;

        public override bool isEnabled(BuildContext context) {
            return Scrollable.of(context) != null;
        }
    }

    public class ScrollAction : UiWidgetAction {
        public ScrollAction() : base(key) {
        }
        
        public static readonly LocalKey key = new ValueKey<Type>(typeof(ScrollAction));

        float _calculateScrollIncrement(ScrollableState state, ScrollIncrementType type = ScrollIncrementType.line) {
            D.assert(state.position != null);
            D.assert(state.position.havePixels);
            D.assert(state.widget.physics == null || state.widget.physics.shouldAcceptUserOffset(state.position));

            if (state.widget.incrementCalculator != null) {
                return state.widget.incrementCalculator(
                    new ScrollIncrementDetails(
                        type: type,
                        metrics: state.position
                    )
                );
            }

            switch (type) {
                case ScrollIncrementType.line:
                    return 50.0f;
                case ScrollIncrementType.page:
                    return 0.8f * state.position.viewportDimension;
            }

            return 0.0f;
        }
        
        float _getIncrement(ScrollableState state, ScrollIntent intent) {
            float increment = _calculateScrollIncrement(state, type: intent.type);
            switch (intent.direction) {
                case AxisDirection.down:
                    switch (state.axisDirection) {
                        case AxisDirection.up:
                            return -increment;
                        case AxisDirection.down:
                            return increment;
                        case AxisDirection.right:
                        case AxisDirection.left:
                            return 0.0f;
                    }
                    break;
                case AxisDirection.up:
                    switch (state.axisDirection) {
                        case AxisDirection.up:
                            return increment;
                        case AxisDirection.down:
                            return -increment;
                        case AxisDirection.right:
                        case AxisDirection.left:
                            return 0.0f;
                    }
                    break;
                case AxisDirection.left:
                    switch (state.axisDirection) {
                        case AxisDirection.right:
                            return -increment;
                        case AxisDirection.left:
                            return increment;
                        case AxisDirection.up:
                        case AxisDirection.down:
                            return 0.0f;
                    }
                    break;
                case AxisDirection.right:
                    switch (state.axisDirection) {
                        case AxisDirection.right:
                            return increment;
                        case AxisDirection.left:
                            return -increment;
                        case AxisDirection.up:
                        case AxisDirection.down:
                            return 0.0f;
                    }
                    break;
            }
            return 0.0f;
        }

        public override void invoke(FocusNode node, Intent intent) {
            ScrollableState state = Scrollable.of(node.context);
            D.assert(state != null, () => "ScrollAction was invoked on a context that has no scrollable parent");
            D.assert(state.position.havePixels, () => "Scrollable must be laid out before it can be scrolled via a ScrollAction");
            if (state.widget.physics != null && !state.widget.physics.shouldAcceptUserOffset(state.position)) {
                return;
            }
            float increment = _getIncrement(state, intent as ScrollIntent);
            if (increment == 0.0f) {
                return;
            }
            state.position.moveTo(
                state.position.pixels + increment,
                duration: TimeSpan.FromMilliseconds(100),
                curve: Curves.easeInOut
                );
        }
    }
}