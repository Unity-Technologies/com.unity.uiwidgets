using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.widgets {
    public delegate List<Widget> NestedScrollViewHeaderSliversBuilder(BuildContext context, bool innerBoxIsScrolled);

    public class NestedScrollView : StatefulWidget {
        public NestedScrollView(
            Key key = null,
            ScrollController controller = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollPhysics physics = null,
            NestedScrollViewHeaderSliversBuilder headerSliverBuilder = null,
            Widget body = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(headerSliverBuilder != null);
            D.assert(body != null);
            this.controller = controller;
            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.physics = physics;
            this.headerSliverBuilder = headerSliverBuilder;
            this.body = body;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly ScrollController controller;

        public readonly Axis scrollDirection;

        public readonly bool reverse;

        public readonly ScrollPhysics physics;

        public readonly NestedScrollViewHeaderSliversBuilder headerSliverBuilder;

        public readonly Widget body;

        public readonly DragStartBehavior dragStartBehavior;

        public static SliverOverlapAbsorberHandle sliverOverlapAbsorberHandleFor(BuildContext context) {
            _InheritedNestedScrollView target = context.dependOnInheritedWidgetOfExactType<_InheritedNestedScrollView>();
            D.assert(target != null,
                () => "NestedScrollView.sliverOverlapAbsorberHandleFor must be called with a context that contains a NestedScrollView."
                );
            return target.state._absorberHandle;
        }

        internal List<Widget> _buildSlivers(BuildContext context, ScrollController innerController,
            bool bodyIsScrolled) {
            List<Widget> slivers = new List<Widget>();
            slivers.AddRange(headerSliverBuilder(context, bodyIsScrolled));
            slivers.Add(new SliverFillRemaining(
                child: new PrimaryScrollController(
                    controller: innerController,
                    child: body
                )
            ));
            return slivers;
        }

        public override State createState() {
            return new NestedScrollViewState();
        }
    }

    class NestedScrollViewState : State<NestedScrollView> {
        internal readonly SliverOverlapAbsorberHandle _absorberHandle = new SliverOverlapAbsorberHandle();

        ScrollController innerController => _coordinator._innerController;

        ScrollController outerController => _coordinator._outerController;

        _NestedScrollCoordinator _coordinator;

        public override void initState() {
            base.initState();
            _coordinator =
                new _NestedScrollCoordinator(this, widget.controller, _handleHasScrolledBodyChanged);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _coordinator.setParent(widget.controller);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            NestedScrollView oldWidget = _oldWidget as NestedScrollView;
            base.didUpdateWidget(oldWidget);
            if (oldWidget.controller != widget.controller) {
                _coordinator.setParent(widget.controller);
            }
        }

        public override void dispose() {
            _coordinator.dispose();
            _coordinator = null;
            base.dispose();
        }

        bool _lastHasScrolledBody;

        void _handleHasScrolledBodyChanged() {
            if (!mounted) {
                return;
            }

            bool newHasScrolledBody = _coordinator.hasScrolledBody;
            if (_lastHasScrolledBody != newHasScrolledBody) {
                setState(() => { });
            }
        }

        public override Widget build(BuildContext context) {
            return new _InheritedNestedScrollView(
                state: this,
                child: new Builder(
                    builder: (BuildContext _context) => {
                        _lastHasScrolledBody = _coordinator.hasScrolledBody;
                        return new _NestedScrollViewCustomScrollView(
                            dragStartBehavior: widget.dragStartBehavior,
                            scrollDirection: widget.scrollDirection,
                            reverse: widget.reverse,
                            physics: widget.physics != null
                                ? widget.physics.applyTo(new ClampingScrollPhysics())
                            : new ClampingScrollPhysics(),
                        controller: _coordinator._outerController,
                        slivers: widget._buildSlivers(
                            _context,
                            _coordinator._innerController,
                            _lastHasScrolledBody
                        ),
                        handle: _absorberHandle
                            );
                    }
                )
            );
        }
    }

    class _NestedScrollViewCustomScrollView : CustomScrollView {
        public _NestedScrollViewCustomScrollView(
            Axis scrollDirection, 
            bool reverse,
            ScrollPhysics physics,
            ScrollController controller,
            List<Widget> slivers,
            SliverOverlapAbsorberHandle handle,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            scrollDirection: scrollDirection,
            reverse: reverse,
            physics: physics,
            controller: controller,
            slivers: slivers,
            dragStartBehavior: dragStartBehavior
        ) {
            this.handle = handle;
        }

        public readonly SliverOverlapAbsorberHandle handle;

        protected override Widget buildViewport(
            BuildContext context,
            ViewportOffset offset,
            AxisDirection? axisDirection,
            List<Widget> slivers
        ) {
            D.assert(!shrinkWrap);
            return new NestedScrollViewViewport(
                axisDirection: axisDirection,
                offset: offset,
                slivers: slivers,
                handle: handle
            );
        }
    }

    class _InheritedNestedScrollView : InheritedWidget {
        public _InheritedNestedScrollView(
            Key key = null,
            NestedScrollViewState state = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(state != null);
            D.assert(child != null);
            this.state = state;
        }

        public readonly NestedScrollViewState state;

        public override bool updateShouldNotify(InheritedWidget _old) {
            _InheritedNestedScrollView old = _old as _InheritedNestedScrollView;
            return state != old.state;
        }
    }

    class _NestedScrollMetrics : FixedScrollMetrics {
        public _NestedScrollMetrics(
            float minScrollExtent,
            float maxScrollExtent,
            float pixels,
            float viewportDimension,
            AxisDirection axisDirection,
            float minRange,
            float maxRange,
            float correctionOffset
        ) : base(
            minScrollExtent: minScrollExtent,
            maxScrollExtent: maxScrollExtent,
            pixels: pixels,
            viewportDimension: viewportDimension,
            axisDirection: axisDirection
        ) {
            this.minRange = minRange;
            this.maxRange = maxRange;
            this.correctionOffset = correctionOffset;
        }

        public _NestedScrollMetrics copyWith(
            float? minScrollExtent = null,
            float? maxScrollExtent = null,
            float? pixels = null,
            float? viewportDimension = null,
            AxisDirection? axisDirection = null,
            float? minRange = null,
            float? maxRange = null,
            float? correctionOffset = null
        ) {
            return new _NestedScrollMetrics(
                minScrollExtent: minScrollExtent ?? this.minScrollExtent,
                maxScrollExtent: maxScrollExtent ?? this.maxScrollExtent,
                pixels: pixels ?? this.pixels,
                viewportDimension: viewportDimension ?? this.viewportDimension,
                axisDirection: axisDirection ?? this.axisDirection,
                minRange: minRange ?? this.minRange,
                maxRange: maxRange ?? this.maxRange,
                correctionOffset: correctionOffset ?? this.correctionOffset
            );
        }

        public readonly float minRange;

        public readonly float maxRange;

        public readonly float correctionOffset;
    }

    delegate ScrollActivity _NestedScrollActivityGetter(_NestedScrollPosition position);

    class _NestedScrollCoordinator : ScrollActivityDelegate, ScrollHoldController {
        public _NestedScrollCoordinator(
            NestedScrollViewState _state,
            ScrollController _parent,
            VoidCallback _onHasScrolledBodyChanged) {
            float initialScrollOffset = _parent?.initialScrollOffset ?? 0.0f;
            _outerController =
                new _NestedScrollController(this, initialScrollOffset: initialScrollOffset, debugLabel: "outer");
            _innerController = new _NestedScrollController(this, initialScrollOffset: 0.0f, debugLabel: "inner");
            this._state = _state;
            this._parent = _parent;
            this._onHasScrolledBodyChanged = _onHasScrolledBodyChanged;
        }

        public readonly NestedScrollViewState _state;
        ScrollController _parent;
        public readonly VoidCallback _onHasScrolledBodyChanged;

        internal _NestedScrollController _outerController;
        internal _NestedScrollController _innerController;

        _NestedScrollPosition _outerPosition {
            get {
                if (!_outerController.hasClients) {
                    return null;
                }
                return _outerController.nestedPositions.Single();
            }
        }

        IEnumerable<_NestedScrollPosition> _innerPositions {
            get { return _innerController.nestedPositions; }
        }

        public bool canScrollBody {
            get {
                _NestedScrollPosition outer = _outerPosition;
                if (outer == null) {
                    return true;
                }

                return outer.haveDimensions && outer.extentAfter() == 0.0f;
            }
        }

        public bool hasScrolledBody {
            get {
                foreach (_NestedScrollPosition position in _innerPositions) {
                    if (position.pixels > position.minScrollExtent) {
                        return true;
                    }
                }

                return false;
            }
        }

        public void updateShadow() {
            if (_onHasScrolledBodyChanged != null) {
                _onHasScrolledBodyChanged();
            }
        }

        public ScrollDirection userScrollDirection {
            get { return _userScrollDirection; }
        }

        ScrollDirection _userScrollDirection = ScrollDirection.idle;

        public void updateUserScrollDirection(ScrollDirection value) {
            if (userScrollDirection == value) {
                return;
            }
            _userScrollDirection = value;
            _outerPosition.didUpdateScrollDirection(value);
            foreach (_NestedScrollPosition position in _innerPositions) {
                position.didUpdateScrollDirection(value);
            }
        }

        ScrollDragController _currentDrag;

        public void beginActivity(ScrollActivity newOuterActivity, _NestedScrollActivityGetter innerActivityGetter) {
            _outerPosition.beginActivity(newOuterActivity);
            bool scrolling = newOuterActivity.isScrolling;
            foreach (_NestedScrollPosition position in _innerPositions) {
                ScrollActivity newInnerActivity = innerActivityGetter(position);
                position.beginActivity(newInnerActivity);
                scrolling = scrolling && newInnerActivity.isScrolling;
            }

            _currentDrag?.dispose();
            _currentDrag = null;
            if (!scrolling) {
                updateUserScrollDirection(ScrollDirection.idle);
            }
        }

        public AxisDirection axisDirection {
            get { return _outerPosition.axisDirection; }
        }

        static IdleScrollActivity _createIdleScrollActivity(_NestedScrollPosition position) {
            return new IdleScrollActivity(position);
        }

        public void goIdle() {
            beginActivity(_createIdleScrollActivity(_outerPosition), _createIdleScrollActivity);
        }

        public void goBallistic(float velocity) {
            beginActivity(
                createOuterBallisticScrollActivity(velocity),
                (_NestedScrollPosition position) => createInnerBallisticScrollActivity(position, velocity)
            );
        }

        public ScrollActivity createOuterBallisticScrollActivity(float velocity) {
            _NestedScrollPosition innerPosition = null;
            if (velocity != 0.0f) {
                foreach (_NestedScrollPosition position in _innerPositions) {
                    if (innerPosition != null) {
                        if (velocity > 0.0f) {
                            if (innerPosition.pixels < position.pixels) {
                                continue;
                            }
                        }
                        else {
                            D.assert(velocity < 0.0f);
                            if (innerPosition.pixels > position.pixels) {
                                continue;
                            }
                        }
                    }

                    innerPosition = position;
                }
            }

            if (innerPosition == null) {
                return _outerPosition.createBallisticScrollActivity(
                    _outerPosition.physics.createBallisticSimulation(_outerPosition, velocity),
                    mode: _NestedBallisticScrollActivityMode.independent
                );
            }

            _NestedScrollMetrics metrics = _getMetrics(innerPosition, velocity);

            return _outerPosition.createBallisticScrollActivity(
                _outerPosition.physics.createBallisticSimulation(metrics, velocity),
                mode: _NestedBallisticScrollActivityMode.outer,
                metrics: metrics
            );
        }

        protected internal ScrollActivity createInnerBallisticScrollActivity(_NestedScrollPosition position,
            float velocity) {
            return position.createBallisticScrollActivity(
                position.physics.createBallisticSimulation(
                    velocity == 0f ? (ScrollMetrics) position : _getMetrics(position, velocity),
                    velocity
                ),
                mode: _NestedBallisticScrollActivityMode.inner
            );
        }

        _NestedScrollMetrics _getMetrics(_NestedScrollPosition innerPosition, float velocity) {
            D.assert(innerPosition != null);
            float pixels, minRange, maxRange, correctionOffset, extra;
            if (innerPosition.pixels == innerPosition.minScrollExtent) {
                pixels = _outerPosition.pixels.clamp(_outerPosition.minScrollExtent,
                    _outerPosition.maxScrollExtent); // TODO(ianh): gracefully handle out-of-range outer positions
                minRange = _outerPosition.minScrollExtent;
                maxRange = _outerPosition.maxScrollExtent;
                D.assert(minRange <= maxRange);
                correctionOffset = 0.0f;
                extra = 0.0f;
            }
            else {
                D.assert(innerPosition.pixels != innerPosition.minScrollExtent);
                if (innerPosition.pixels < innerPosition.minScrollExtent) {
                    pixels = innerPosition.pixels - innerPosition.minScrollExtent + _outerPosition.minScrollExtent;
                }
                else {
                    D.assert(innerPosition.pixels > innerPosition.minScrollExtent);
                    pixels = innerPosition.pixels - innerPosition.minScrollExtent + _outerPosition.maxScrollExtent;
                }

                if ((velocity > 0.0f) && (innerPosition.pixels > innerPosition.minScrollExtent)) {
                    extra = _outerPosition.maxScrollExtent - _outerPosition.pixels;
                    D.assert(extra >= 0.0f);
                    minRange = pixels;
                    maxRange = pixels + extra;
                    D.assert(minRange <= maxRange);
                    correctionOffset = _outerPosition.pixels - pixels;
                }
                else if ((velocity < 0.0f) && (innerPosition.pixels < innerPosition.minScrollExtent)) {
                    extra = _outerPosition.pixels - _outerPosition.minScrollExtent;
                    D.assert(extra >= 0.0f);
                    minRange = pixels - extra;
                    maxRange = pixels;
                    D.assert(minRange <= maxRange);
                    correctionOffset = _outerPosition.pixels - pixels;
                }
                else {
                    if (velocity > 0.0f) {
                        extra = _outerPosition.minScrollExtent - _outerPosition.pixels;
                    }
                    else {
                        D.assert(velocity < 0.0f);
                        extra = _outerPosition.pixels -
                                (_outerPosition.maxScrollExtent - _outerPosition.minScrollExtent);
                    }

                    D.assert(extra <= 0.0f);
                    minRange = _outerPosition.minScrollExtent;
                    maxRange = _outerPosition.maxScrollExtent + extra;
                    D.assert(minRange <= maxRange);
                    correctionOffset = 0.0f;
                }
            }

            return new _NestedScrollMetrics(
                minScrollExtent: _outerPosition.minScrollExtent,
                maxScrollExtent: _outerPosition.maxScrollExtent + innerPosition.maxScrollExtent -
                                 innerPosition.minScrollExtent + extra,
                pixels: pixels,
                viewportDimension: _outerPosition.viewportDimension,
                axisDirection: _outerPosition.axisDirection,
                minRange: minRange,
                maxRange: maxRange,
                correctionOffset: correctionOffset
            );
        }

        public float unnestOffset(float value, _NestedScrollPosition source) {
            if (source == _outerPosition) {
                return value.clamp(_outerPosition.minScrollExtent, _outerPosition.maxScrollExtent);
            }

            if (value < source.minScrollExtent) {
                return value - source.minScrollExtent + _outerPosition.minScrollExtent;
            }

            return value - source.minScrollExtent + _outerPosition.maxScrollExtent;
        }

        public float nestOffset(float value, _NestedScrollPosition target) {
            if (target == _outerPosition) {
                return value.clamp(_outerPosition.minScrollExtent, _outerPosition.maxScrollExtent);
            }

            if (value < _outerPosition.minScrollExtent) {
                return value - _outerPosition.minScrollExtent + target.minScrollExtent;
            }

            if (value > _outerPosition.maxScrollExtent) {
                return value - _outerPosition.maxScrollExtent + target.minScrollExtent;
            }

            return target.minScrollExtent;
        }

        public void updateCanDrag() {
            if (!_outerPosition.haveDimensions) {
                return;
            }

            float maxInnerExtent = 0.0f;
            foreach (_NestedScrollPosition position in _innerPositions) {
                if (!position.haveDimensions) {
                    return;
                }

                maxInnerExtent = Mathf.Max(maxInnerExtent, position.maxScrollExtent - position.minScrollExtent);
            }

            _outerPosition.updateCanDrag(maxInnerExtent);
        }

        public Future animateTo(
            float to,
            TimeSpan duration,
            Curve curve
        ) {
            DrivenScrollActivity outerActivity = _outerPosition.createDrivenScrollActivity(
                nestOffset(to, _outerPosition),
                duration,
                curve
            );
            List<Future> resultFutures = new List<Future> {outerActivity.done};
            beginActivity(
                outerActivity,
                (_NestedScrollPosition position) => {
                    DrivenScrollActivity innerActivity = position.createDrivenScrollActivity(
                        nestOffset(to, position),
                        duration,
                        curve
                    );
                    resultFutures.Add(innerActivity.done);
                    return innerActivity;
                }
            );
            return Future.wait<object>(resultFutures);
        }

        public void jumpTo(float to) {
            goIdle();
            _outerPosition.localJumpTo(nestOffset(to, _outerPosition));
            foreach (_NestedScrollPosition position in _innerPositions) {
                position.localJumpTo(nestOffset(to, position));
            }

            goBallistic(0.0f);
        }

        public float setPixels(float newPixels) {
            D.assert(false);
            return 0.0f;
        }

        public ScrollHoldController hold(VoidCallback holdCancelCallback) {
            beginActivity(
                new HoldScrollActivity(del: _outerPosition, onHoldCanceled: holdCancelCallback),
                (_NestedScrollPosition position) => new HoldScrollActivity(del: position)
            );
            return this;
        }

        public void cancel() {
            goBallistic(0.0f);
        }

        public Drag drag(DragStartDetails details, VoidCallback dragCancelCallback) {
            ScrollDragController drag = new ScrollDragController(
                del: this,
                details: details,
                onDragCanceled: dragCancelCallback
            );
            beginActivity(
                new DragScrollActivity(_outerPosition, drag),
                (_NestedScrollPosition position) => new DragScrollActivity(position, drag)
            );
            D.assert(_currentDrag == null);
            _currentDrag = drag;
            return drag;
        }

        public void applyUserOffset(float delta) {
            updateUserScrollDirection(delta > 0.0f ? ScrollDirection.forward : ScrollDirection.reverse);
            D.assert(delta != 0.0f);
            if (!_innerPositions.Any()) {
                _outerPosition.applyFullDragUpdate(delta);
            }
            else if (delta < 0.0f) {
                float innerDelta = _outerPosition.applyClampedDragUpdate(delta);
                if (innerDelta != 0.0f) {
                    foreach (_NestedScrollPosition position in _innerPositions) {
                        position.applyFullDragUpdate(innerDelta);
                    }
                }
            }
            else {
                float outerDelta = 0.0f; // it will go positive if it changes
                List<float> overscrolls = new List<float> { };
                List<_NestedScrollPosition> innerPositions = _innerPositions.ToList();
                foreach (_NestedScrollPosition position in innerPositions) {
                    float overscroll = position.applyClampedDragUpdate(delta);
                    outerDelta = Mathf.Max(outerDelta, overscroll);
                    overscrolls.Add(overscroll);
                }

                if (outerDelta != 0.0f) {
                    outerDelta -= _outerPosition.applyClampedDragUpdate(outerDelta);
                }

                for (int i = 0; i < innerPositions.Count; ++i) {
                    float remainingDelta = overscrolls[i] - outerDelta;
                    if (remainingDelta > 0.0f) {
                        innerPositions[i].applyFullDragUpdate(remainingDelta);
                    }
                }
            }
        }

        public void applyUserScrollOffset(float delta) {
            // TODO: replace with real implementation
            applyUserOffset(delta);
        }

        public void setParent(ScrollController value) {
            _parent = value;
            updateParent();
        }

        public void updateParent() {
            _outerPosition?.setParent(_parent ?? PrimaryScrollController.of(_state.context));
        }

        public void dispose() {
            _currentDrag?.dispose();
            _currentDrag = null;
            _outerController.dispose();
            _innerController.dispose();
        }

        public override string ToString() {
            return $"{GetType()}(outer={_outerController}; inner={_innerController})";
        }
    }

    class _NestedScrollController : ScrollController {
        public _NestedScrollController(
            _NestedScrollCoordinator coordinator,
            float initialScrollOffset = 0.0f,
            string debugLabel = null
        ) : base(initialScrollOffset: initialScrollOffset, debugLabel: debugLabel) {
            this.coordinator = coordinator;
        }

        public readonly _NestedScrollCoordinator coordinator;

        public override ScrollPosition createScrollPosition(
            ScrollPhysics physics,
            ScrollContext context,
            ScrollPosition oldPosition
        ) {
            return new _NestedScrollPosition(
                coordinator: coordinator,
                physics: physics,
                context: context,
                initialPixels: initialScrollOffset,
                oldPosition: oldPosition,
                debugLabel: debugLabel
            );
        }

        public override void attach(ScrollPosition position) {
            D.assert(position is _NestedScrollPosition);
            base.attach(position);
            coordinator.updateParent();
            coordinator.updateCanDrag();
            position.addListener(_scheduleUpdateShadow);
            _scheduleUpdateShadow();
        }

        public override void detach(ScrollPosition position) {
            D.assert(position is _NestedScrollPosition);
            position.removeListener(_scheduleUpdateShadow);
            base.detach(position);
            _scheduleUpdateShadow();
        }

        void _scheduleUpdateShadow() {
            SchedulerBinding.instance.addPostFrameCallback(
                (TimeSpan timeStamp) => { coordinator.updateShadow(); }
            );
        }

        public IEnumerable<_NestedScrollPosition> nestedPositions {
            get {
                foreach (var scrollPosition in positions) {
                    yield return (_NestedScrollPosition) scrollPosition;
                }
            }
        }
    }

    class _NestedScrollPosition : ScrollPosition, ScrollActivityDelegate {
        public _NestedScrollPosition(
            ScrollPhysics physics,
            ScrollContext context,
            float initialPixels = 0.0f,
            ScrollPosition oldPosition = null,
            string debugLabel = null,
            _NestedScrollCoordinator coordinator = null
        ) : base(
            physics: physics,
            context: context,
            oldPosition: oldPosition,
            debugLabel: debugLabel,
            coordinator: coordinator
        ) {
            D.assert(coordinator != null);
            if (!havePixels) {
                correctPixels(initialPixels);
            }

            if (activity == null) {
                goIdle();
            }

            D.assert(activity != null);
            saveScrollOffset(); // in case we didn't restore but could, so that we don't restore it later
        }

        public _NestedScrollCoordinator coordinator {
            get { return (_NestedScrollCoordinator) _coordinator; }
        }

        public TickerProvider vsync {
            get { return context.vsync; }
        }

        ScrollController _parent;

        public void setParent(ScrollController value) {
            _parent?.detach(this);
            _parent = value;
            _parent?.attach(this);
        }

        public override AxisDirection axisDirection {
            get { return context.axisDirection; }
        }

        protected override void absorb(ScrollPosition other) {
            base.absorb(other);
            activity.updateDelegate(this);
        }

        protected override void restoreScrollOffset() {
            if (coordinator.canScrollBody) {
                base.restoreScrollOffset();
            }
        }

        public float applyClampedDragUpdate(float delta) {
            D.assert(delta != 0.0f);
            float min = delta < 0.0f ? -float.PositiveInfinity : Mathf.Min(minScrollExtent, pixels);
            float max = delta > 0.0f ? float.PositiveInfinity : Mathf.Max(maxScrollExtent, pixels);
            float oldPixels = pixels;
            float newPixels = (pixels - delta).clamp(min, max);
            float clampedDelta = newPixels - pixels;
            if (clampedDelta == 0.0f) {
                return delta;
            }

            float overscroll = physics.applyBoundaryConditions(this, newPixels);
            float actualNewPixels = newPixels - overscroll;
            float offset = actualNewPixels - oldPixels;
            if (offset != 0.0f) {
                forcePixels(actualNewPixels);
                didUpdateScrollPositionBy(offset);
            }

            return delta + offset;
        }

        public float applyFullDragUpdate(float delta) {
            D.assert(delta != 0.0f);
            float oldPixels = pixels;
            float newPixels = pixels - physics.applyPhysicsToUserOffset(this, delta);
            if (oldPixels == newPixels) {
                return 0.0f; // delta must have been so small we dropped it during floating point addition
            }

            float overscroll = physics.applyBoundaryConditions(this, newPixels);
            float actualNewPixels = newPixels - overscroll;
            if (actualNewPixels != oldPixels) {
                forcePixels(actualNewPixels);
                didUpdateScrollPositionBy(actualNewPixels - oldPixels);
            }

            if (overscroll != 0.0f) {
                didOverscrollBy(overscroll);
                return overscroll;
            }

            return 0.0f;
        }

        public float applyClampedPointerSignalUpdate(float delta) {
            D.assert(delta != 0.0f);

            float min = delta > 0.0f
                ? float.NegativeInfinity
                : Mathf.Min(minScrollExtent, pixels);
            // The logic for max is equivalent but on the other side.
            float max = delta < 0.0f
                ? float.PositiveInfinity
                : Mathf.Max(maxScrollExtent, pixels);
            float newPixels = (pixels + delta).clamp(min, max);
            float clampedDelta = newPixels - pixels;
            if (clampedDelta == 0.0f)
                return delta;
            forcePixels(newPixels);
            didUpdateScrollPositionBy(clampedDelta);
            return delta - clampedDelta;
        }
        
        public override ScrollDirection userScrollDirection {
            get { return coordinator.userScrollDirection; }
        }

        public DrivenScrollActivity createDrivenScrollActivity(float to, TimeSpan duration, Curve curve) {
            return new DrivenScrollActivity(
                this,
                from: pixels,
                to: to,
                duration: duration,
                curve: curve,
                vsync: vsync
            );
        }

        public void applyUserOffset(float delta) {
            D.assert(false);
        }

        public void applyUserScrollOffset(float delta) {
            // TODO: replace with real implementation
            applyUserOffset(delta);
        }

        public void goIdle() {
            beginActivity(new IdleScrollActivity(this));
        }

        public void goBallistic(float velocity) {
            Simulation simulation = null;
            if (velocity != 0.0f || this.outOfRange()) {
                simulation = physics.createBallisticSimulation(this, velocity);
            }

            beginActivity(createBallisticScrollActivity(
                simulation,
                mode: _NestedBallisticScrollActivityMode.independent
            ));
        }

        public ScrollActivity createBallisticScrollActivity(
            Simulation simulation = null,
            _NestedBallisticScrollActivityMode? mode = null,
            _NestedScrollMetrics metrics = null
        ) {
            if (simulation == null) {
                return new IdleScrollActivity(this);
            }

            D.assert(mode != null);
            switch (mode) {
                case _NestedBallisticScrollActivityMode.outer:
                    D.assert(metrics != null);
                    if (metrics.minRange == metrics.maxRange) {
                        return new IdleScrollActivity(this);
                    }

                    return new _NestedOuterBallisticScrollActivity(coordinator, this, metrics, simulation,
                        context.vsync);
                case _NestedBallisticScrollActivityMode.inner:
                    return new _NestedInnerBallisticScrollActivity(coordinator, this, simulation,
                        context.vsync);
                case _NestedBallisticScrollActivityMode.independent:
                    return new BallisticScrollActivity(this, simulation, context.vsync);
            }

            return null;
        }

        public override Future animateTo(float to,
            TimeSpan duration,
            Curve curve
        ) {
            return coordinator.animateTo(coordinator.unnestOffset(to, this), duration: duration,
                curve: curve);
        }

        public override void jumpTo(float value) {
            coordinator.jumpTo(coordinator.unnestOffset(value, this));
        }

        public void localJumpTo(float value) {
            if (pixels != value) {
                float oldPixels = pixels;
                forcePixels(value);
                didStartScroll();
                didUpdateScrollPositionBy(pixels - oldPixels);
                didEndScroll();
            }
        }

        protected override void applyNewDimensions() {
            base.applyNewDimensions();
            coordinator.updateCanDrag();
        }

        public void updateCanDrag(float totalExtent) {
            context.setCanDrag(totalExtent > (viewportDimension - maxScrollExtent) ||
                                    minScrollExtent != maxScrollExtent);
        }

        public override ScrollHoldController hold(VoidCallback holdCancelCallback) {
            return coordinator.hold(holdCancelCallback);
        }

        public override Drag drag(DragStartDetails details, VoidCallback dragCancelCallback) {
            return coordinator.drag(details, dragCancelCallback);
        }

        public override void dispose() {
            _parent?.detach(this);
            base.dispose();
        }
    }

    enum _NestedBallisticScrollActivityMode {
        outer,
        inner,
        independent
    }

    class _NestedInnerBallisticScrollActivity : BallisticScrollActivity {
        public _NestedInnerBallisticScrollActivity(
            _NestedScrollCoordinator coordinator,
            _NestedScrollPosition position,
            Simulation simulation,
            TickerProvider vsync
        ) : base(position, simulation, vsync) {
            this.coordinator = coordinator;
        }

        public readonly _NestedScrollCoordinator coordinator;

        public new _NestedScrollPosition del {
            get { return (_NestedScrollPosition) base.del; }
        }

        public override void resetActivity() {
            del.beginActivity(coordinator.createInnerBallisticScrollActivity(del, velocity));
        }

        public override void applyNewDimensions() {
            del.beginActivity(coordinator.createInnerBallisticScrollActivity(del, velocity));
        }

        protected override bool applyMoveTo(float value) {
            return base.applyMoveTo(coordinator.nestOffset(value, del));
        }
    }

    class _NestedOuterBallisticScrollActivity : BallisticScrollActivity {
        public _NestedOuterBallisticScrollActivity(
            _NestedScrollCoordinator coordinator,
            _NestedScrollPosition position,
            _NestedScrollMetrics metrics,
            Simulation simulation,
            TickerProvider vsync
        ) : base(position, simulation, vsync) {
            D.assert(metrics.minRange != metrics.maxRange);
            D.assert(metrics.maxRange > metrics.minRange);
            this.coordinator = coordinator;
            this.metrics = metrics;
        }

        public readonly _NestedScrollCoordinator coordinator;
        public readonly _NestedScrollMetrics metrics;

        public new _NestedScrollPosition del {
            get { return (_NestedScrollPosition) base.del; }
        }

        public override void resetActivity() {
            del.beginActivity(coordinator.createOuterBallisticScrollActivity(velocity));
        }

        public override void applyNewDimensions() {
            del.beginActivity(coordinator.createOuterBallisticScrollActivity(velocity));
        }

        protected override bool applyMoveTo(float value) {
            bool done = false;
            if (velocity > 0.0f) {
                if (value < metrics.minRange) {
                    return true;
                }

                if (value > metrics.maxRange) {
                    value = metrics.maxRange;
                    done = true;
                }
            }
            else if (velocity < 0.0f) {
                if (value > metrics.maxRange) {
                    return true;
                }

                if (value < metrics.minRange) {
                    value = metrics.minRange;
                    done = true;
                }
            }
            else {
                value = value.clamp(metrics.minRange, metrics.maxRange);
                done = true;
            }

            bool result = base.applyMoveTo(value + metrics.correctionOffset);
            D.assert(result); // since we tried to pass an in-range value, it shouldn"t ever overflow
            return !done;
        }

        public override string ToString() {
            return
                $"{GetType()}({metrics.minRange} .. {metrics.maxRange}; correcting by {metrics.correctionOffset})";
        }
    }

    public class SliverOverlapAbsorberHandle : ChangeNotifier {
        internal int _writers = 0;

        public float layoutExtent {
            get { return _layoutExtent; }
        }

        float _layoutExtent;

        public float scrollExtent {
            get { return _scrollExtent; }
        }

        float _scrollExtent;

        internal void _setExtents(float layoutValue, float scrollValue) {
            D.assert(_writers == 1,
                () => "Multiple RenderSliverOverlapAbsorbers have been provided the same SliverOverlapAbsorberHandle.");
            _layoutExtent = layoutValue;
            _scrollExtent = scrollValue;
        }

        internal void _markNeedsLayout() {
            notifyListeners();
        }

        public override string ToString() {
            string extra = "";
            switch (_writers) {
                case 0:
                    extra = ", orphan";
                    break;
                case 1:
                    break;
                default:
                    extra = ", $_writers WRITERS ASSIGNED";
                    break;
            }

            return $"{GetType()}({layoutExtent}{extra})";
        }
    }

    public class SliverOverlapAbsorber : SingleChildRenderObjectWidget {
        public SliverOverlapAbsorber(
            Key key = null,
            SliverOverlapAbsorberHandle handle = null,
            Widget child = null,
            Widget sliver = null
        ) : base(key: key, child: sliver ?? child) {
            D.assert(handle != null);
            D.assert(child == null || sliver == null);
            this.handle = handle;
        }

        public readonly SliverOverlapAbsorberHandle handle;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverOverlapAbsorber(
                handle: handle
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderSliverOverlapAbsorber renderObject = _renderObject as RenderSliverOverlapAbsorber;
            renderObject.handle = handle;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", handle));
        }
    }

    public class RenderSliverOverlapAbsorber : RenderObjectWithChildMixinRenderSliver<RenderSliver> {
        public RenderSliverOverlapAbsorber(
            SliverOverlapAbsorberHandle handle,
            RenderSliver child = null,
            RenderSliver sliver = null
        ) {
            D.assert(handle != null);
            D.assert(child == null || sliver == null);
            _handle = handle;
            this.child = sliver ?? child;
        }

        public SliverOverlapAbsorberHandle handle {
            get { return _handle; }
            set {
                D.assert(value != null);
                if (handle == value) {
                    return;
                }

                if (attached) {
                    handle._writers -= 1;
                    value._writers += 1;
                    value._setExtents(handle.layoutExtent, handle.scrollExtent);
                }

                _handle = value;
            }
        }

        SliverOverlapAbsorberHandle _handle;

        public override void attach(object owner) {
            base.attach(owner);
            handle._writers += 1;
        }

        public override void detach() {
            handle._writers -= 1;
            base.detach();
        }

        protected override void performLayout() {
            D.assert(handle._writers == 1,
                () =>
                    "A SliverOverlapAbsorberHandle cannot be passed to multiple RenderSliverOverlapAbsorber objects at the same time.");
            if (child == null) {
                geometry = new SliverGeometry();
                return;
            }

            child.layout(constraints, parentUsesSize: true);
            SliverGeometry childLayoutGeometry = child.geometry;
            geometry = new SliverGeometry(
                scrollExtent: childLayoutGeometry.scrollExtent - childLayoutGeometry.maxScrollObstructionExtent,
                paintExtent: childLayoutGeometry.paintExtent,
                paintOrigin: childLayoutGeometry.paintOrigin,
                layoutExtent: Mathf.Max(0, childLayoutGeometry.paintExtent - childLayoutGeometry.maxScrollObstructionExtent),
                maxPaintExtent: childLayoutGeometry.maxPaintExtent,
                maxScrollObstructionExtent: childLayoutGeometry.maxScrollObstructionExtent,
                hitTestExtent: childLayoutGeometry.hitTestExtent,
                visible: childLayoutGeometry.visible,
                hasVisualOverflow: childLayoutGeometry.hasVisualOverflow,
                scrollOffsetCorrection: childLayoutGeometry.scrollOffsetCorrection
            );
            handle._setExtents(childLayoutGeometry.maxScrollObstructionExtent,
                childLayoutGeometry.maxScrollObstructionExtent);
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
        }

        protected override bool hitTestChildren(
            SliverHitTestResult result,
            float mainAxisPosition = 0,
            float crossAxisPosition = 0
        ) {
            if (child != null) {
                return child.hitTest(result, mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition);
            }

            return false;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                context.paintChild(child, offset);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", handle));
        }
    }

    public class SliverOverlapInjector : SingleChildRenderObjectWidget {
        public SliverOverlapInjector(
            Key key = null,
            SliverOverlapAbsorberHandle handle = null,
            Widget child = null,
            Widget sliver = null
        ) : base(key: key, child: sliver ?? child) {
            D.assert(handle != null);
            D.assert(child == null || sliver == null);
            this.handle = handle;
        }

        public readonly SliverOverlapAbsorberHandle handle;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverOverlapInjector(
                handle: handle
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderSliverOverlapInjector renderObject = _renderObject as RenderSliverOverlapInjector;
            renderObject.handle = handle;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", handle));
        }
    }

    public class RenderSliverOverlapInjector : RenderSliver {
        public RenderSliverOverlapInjector(
            SliverOverlapAbsorberHandle handle
        ) {
            D.assert(handle != null);
            _handle = handle;
        }

        float _currentLayoutExtent;
        float _currentMaxExtent;

        public SliverOverlapAbsorberHandle handle {
            get { return _handle; }
            set {
                D.assert(value != null);
                if (handle == value) {
                    return;
                }

                if (attached) {
                    handle.removeListener(markNeedsLayout);
                }

                _handle = value;
                if (attached) {
                    handle.addListener(markNeedsLayout);
                    if (handle.layoutExtent != _currentLayoutExtent ||
                        handle.scrollExtent != _currentMaxExtent) {
                        markNeedsLayout();
                    }
                }
            }
        }

        SliverOverlapAbsorberHandle _handle;

        public override void attach(object owner) {
            base.attach(owner);
            handle.addListener(markNeedsLayout);
            if (handle.layoutExtent != _currentLayoutExtent ||
                handle.scrollExtent != _currentMaxExtent) {
                markNeedsLayout();
            }
        }

        public override void detach() {
            handle.removeListener(markNeedsLayout);
            base.detach();
        }

        protected override void performLayout() {
            _currentLayoutExtent = handle.layoutExtent;
            _currentMaxExtent = handle.layoutExtent;
            float clampedLayoutExtent = Mathf.Min(_currentLayoutExtent - constraints.scrollOffset,
                constraints.remainingPaintExtent);
            geometry = new SliverGeometry(
                scrollExtent: _currentLayoutExtent,
                paintExtent: Mathf.Max(0.0f, clampedLayoutExtent),
                maxPaintExtent: _currentMaxExtent
            );
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    Paint paint = new Paint();
                    paint.color = new Color(0xFFCC9933);
                    paint.strokeWidth = 3.0f;
                    paint.style = PaintingStyle.stroke;
                    Offset start, end, delta;
                    switch (constraints.axis) {
                        case Axis.vertical:
                            float x = offset.dx + constraints.crossAxisExtent / 2.0f;
                            start = new Offset(x, offset.dy);
                            end = new Offset(x, offset.dy + geometry.paintExtent);
                            delta = new Offset(constraints.crossAxisExtent / 5.0f, 0.0f);
                            break;
                        case Axis.horizontal:
                            float y = offset.dy + constraints.crossAxisExtent / 2.0f;
                            start = new Offset(offset.dx, y);
                            end = new Offset(offset.dy + geometry.paintExtent, y);
                            delta = new Offset(0.0f, constraints.crossAxisExtent / 5.0f);
                            break;
                        default:
                            throw new Exception("");
                    }

                    for (int index = -2; index <= 2; index += 1) {
                        PaintingUtilities.paintZigZag(context.canvas, paint, start - delta * (float) index,
                            end - delta * (float) index, 10, 10.0f);
                    }
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", handle));
        }
    }

    public class NestedScrollViewViewport : Viewport {
        public NestedScrollViewViewport(
            Key key = null,
            AxisDirection? axisDirection = AxisDirection.down,
            AxisDirection crossAxisDirection = AxisDirection.right,
            float anchor = 0.0f,
            ViewportOffset offset = null,
            Key center = null,
            List<Widget> slivers = null,
            SliverOverlapAbsorberHandle handle = null
        ) : base(
            key: key,
            axisDirection: axisDirection,
            crossAxisDirection: crossAxisDirection,
            anchor: anchor,
            offset: offset,
            center: center,
            slivers: slivers ?? new List<Widget>()
        ) {
            D.assert(handle != null);
            D.assert(this.offset != null);
            this.handle = handle;
        }

        public readonly SliverOverlapAbsorberHandle handle;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderNestedScrollViewViewport(
                axisDirection: axisDirection,
                crossAxisDirection: crossAxisDirection ??
                                    getDefaultCrossAxisDirection(context, axisDirection),
                anchor: anchor,
                offset: offset,
                handle: handle
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderNestedScrollViewViewport renderObject = _renderObject as RenderNestedScrollViewViewport;
            renderObject.axisDirection = axisDirection.Value;
            renderObject.crossAxisDirection = (crossAxisDirection ?? getDefaultCrossAxisDirection(context, axisDirection)).Value;
            if (crossAxisDirection == null) {
                renderObject.anchor = anchor;
            }

            renderObject.offset = offset;
            renderObject.handle = handle;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", handle));
        }
    }

    public class RenderNestedScrollViewViewport : RenderViewport {
        public RenderNestedScrollViewViewport(
            AxisDirection? axisDirection = AxisDirection.down,
            AxisDirection? crossAxisDirection = AxisDirection.right,
            ViewportOffset offset = null,
            float anchor = 0.0f,
            List<RenderSliver> children = null,
            RenderSliver center = null,
            SliverOverlapAbsorberHandle handle = null
        ) : base(
            axisDirection: axisDirection,
            crossAxisDirection: crossAxisDirection,
            offset: offset,
            anchor: anchor,
            children: children,
            center: center
        ) {
            D.assert(handle != null);
            D.assert(offset != null);
            _handle = handle;
        }

        public SliverOverlapAbsorberHandle handle {
            get { return _handle; }
            set {
                D.assert(value != null);
                if (handle == value) {
                    return;
                }

                _handle = value;
                handle._markNeedsLayout();
            }
        }

        SliverOverlapAbsorberHandle _handle;

        public override void markNeedsLayout() {
            handle._markNeedsLayout();
            base.markNeedsLayout();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", handle));
        }
    }
}