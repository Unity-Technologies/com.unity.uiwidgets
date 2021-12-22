using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate void DismissDirectionCallback(DismissDirection? direction);

    public delegate Future<bool> ConfirmDismissCallback(DismissDirection? direction);

    public enum DismissDirection {
        vertical,

        horizontal,

        endToStart,

        startToEnd,

        up,

        down
    }

    public class Dismissible : StatefulWidget {
        public Dismissible(
            Key key = null,
            Widget child = null,
            Widget background = null,
            Widget secondaryBackground = null,
            ConfirmDismissCallback confirmDismiss = null,
            VoidCallback onResize = null,
            DismissDirectionCallback onDismissed = null,
            DismissDirection direction = DismissDirection.horizontal,
            TimeSpan? resizeDuration = null,
            Dictionary<DismissDirection?, float?> dismissThresholds = null,
            TimeSpan? movementDuration = null,
            float crossAxisEndOffset = 0.0f,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(key != null);
            D.assert(secondaryBackground == null || background != null);
            this.resizeDuration = resizeDuration ?? new TimeSpan(0, 0, 0, 0, 300);
            this.dismissThresholds = dismissThresholds ?? new Dictionary<DismissDirection?, float?>();
            this.movementDuration = movementDuration ?? new TimeSpan(0, 0, 0, 0, 200);
            this.child = child;
            this.background = background;
            this.secondaryBackground = secondaryBackground;
            this.confirmDismiss = confirmDismiss;
            this.onResize = onResize;
            this.onDismissed = onDismissed;
            this.direction = direction;
            this.crossAxisEndOffset = crossAxisEndOffset;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly Widget child;

        public readonly Widget background;

        public readonly Widget secondaryBackground;

        public readonly VoidCallback onResize;

        public readonly DismissDirectionCallback onDismissed;

        public readonly ConfirmDismissCallback confirmDismiss;

        public readonly DismissDirection direction;

        public readonly TimeSpan? resizeDuration;

        public readonly Dictionary<DismissDirection?, float?> dismissThresholds;

        public readonly TimeSpan? movementDuration;

        public readonly float crossAxisEndOffset;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _DismissibleState();
        }
    }

    class _AutomaticWidgetTicker<T> : Ticker where T : StatefulWidget {
        internal _AutomaticWidgetTicker(
            TickerCallback onTick,
            AutomaticKeepAliveClientWithTickerProviderStateMixin<T> creator,
            string debugLabel = null) :
            base(onTick: onTick, debugLabel: debugLabel) {
            _creator = creator;
        }

        readonly AutomaticKeepAliveClientWithTickerProviderStateMixin<T> _creator;

        public override void Dispose() {
            _creator._removeTicker(this);
            base.Dispose();
        }
    }

    class _DismissibleClipper : CustomClipper<Rect> {
        public _DismissibleClipper(
            Axis axis,
            Animation<Offset> moveAnimation
        ) : base(reclip: moveAnimation) {
            this.axis = axis;
            this.moveAnimation = moveAnimation;
        }

        public readonly Axis axis;
        public readonly Animation<Offset> moveAnimation;

        public override Rect getClip(Size size) {
            switch (axis) {
                case Axis.horizontal:
                    float offset1 = moveAnimation.value.dx * size.width;
                    if (offset1 < 0) {
                        return Rect.fromLTRB(size.width + offset1, 0.0f, size.width, size.height);
                    }

                    return Rect.fromLTRB(0.0f, 0.0f, offset1, size.height);
                case Axis.vertical:
                    float offset = moveAnimation.value.dy * size.height;
                    if (offset < 0) {
                        return Rect.fromLTRB(0.0f, size.height + offset, size.width, size.height);
                    }

                    return Rect.fromLTRB(0.0f, 0.0f, size.width, offset);
            }

            return null;
        }

        public override Rect getApproximateClipRect(Size size) {
            return getClip(size);
        }

        public override bool shouldReclip(CustomClipper<Rect> oldClipper) {
            //D.assert(oldClipper is _DismissibleClipper);
            _DismissibleClipper clipper = oldClipper as _DismissibleClipper;
            return clipper.axis != axis
                   || clipper.moveAnimation.value != moveAnimation.value;
        }
    }

    enum _FlingGestureKind {
        none,
        forward,
        reverse
    }

    public class _DismissibleState : AutomaticKeepAliveClientWithTickerProviderStateMixin<Dismissible> {
        static readonly Curve _kResizeTimeCurve = new Interval(0.4f, 1.0f, curve: Curves.ease);
        const float _kMinFlingVelocity = 700.0f;
        const float _kMinFlingVelocityDelta = 400.0f;
        const float _kFlingVelocityScale = 1.0f / 300.0f;
        const float _kDismissThreshold = 0.4f;

        public override void initState() {
            base.initState();
            _moveController = new AnimationController(duration: widget.movementDuration, vsync: this);
            _moveController.addStatusListener(_handleDismissStatusChanged);
            _updateMoveAnimation();
        }

        AnimationController _moveController;
        Animation<Offset> _moveAnimation;

        AnimationController _resizeController;
        Animation<float> _resizeAnimation;

        float _dragExtent = 0.0f;
        bool _dragUnderway = false;
        Size _sizePriorToCollapse;

        protected override bool wantKeepAlive {
            get { return _moveController?.isAnimating == true || _resizeController?.isAnimating == true; }
        }

        public override void dispose() {
            _moveController.dispose();
            _resizeController?.dispose();
            base.dispose();
        }

        bool _directionIsXAxis {
            get {
                return widget.direction == DismissDirection.horizontal
                       || widget.direction == DismissDirection.endToStart
                       || widget.direction == DismissDirection.startToEnd;
            }
        }

        DismissDirection? _extentToDirection(float? extent) {
            if (extent == 0.0) {
                return null;
            }

            if (_directionIsXAxis) {
                switch (Directionality.of(context)) {
                    case TextDirection.rtl:
                        return extent < 0 ? DismissDirection.startToEnd : DismissDirection.endToStart;
                    case TextDirection.ltr:
                        return extent > 0 ? DismissDirection.startToEnd : DismissDirection.endToStart;
                }

                D.assert(false);
                return null;
            }

            return extent > 0 ? DismissDirection.down : DismissDirection.up;
        }

        DismissDirection? _dismissDirection {
            get { return _extentToDirection(_dragExtent); }
        }

        bool _isActive {
            get { return _dragUnderway || _moveController.isAnimating; }
        }

        float _overallDragAxisExtent {
            get {
                Size size = context.size;
                return _directionIsXAxis ? size.width : size.height;
            }
        }

        void _handleDragStart(DragStartDetails details) {
            _dragUnderway = true;
            if (_moveController.isAnimating) {
                _dragExtent = _moveController.value * _overallDragAxisExtent * _dragExtent.sign();
                _moveController.stop();
            }
            else {
                _dragExtent = 0.0f;
                _moveController.setValue(0.0f);
            }

            setState(() => { _updateMoveAnimation(); });
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (!_isActive || _moveController.isAnimating) {
                return;
            }

            float delta = details.primaryDelta ?? 0.0f;
            float oldDragExtent = _dragExtent;
            switch (widget.direction) {
                case DismissDirection.horizontal:
                case DismissDirection.vertical:
                    _dragExtent += delta;
                    break;

                case DismissDirection.up:
                    if (_dragExtent + delta < 0) {
                        _dragExtent += delta;
                    }

                    break;

                case DismissDirection.down:
                    if (_dragExtent + delta > 0) {
                        _dragExtent += delta;
                    }

                    break;

                case DismissDirection.endToStart:
                    switch (Directionality.of(context)) {
                        case TextDirection.rtl:
                            if (_dragExtent + delta > 0) {
                                _dragExtent += delta;
                            }

                            break;
                        case TextDirection.ltr:
                            if (_dragExtent + delta < 0) {
                                _dragExtent += delta;
                            }

                            break;
                    }

                    break;

                case DismissDirection.startToEnd:
                    switch (Directionality.of(context)) {
                        case TextDirection.rtl:
                            if (_dragExtent + delta < 0) {
                                _dragExtent += delta;
                            }

                            break;
                        case TextDirection.ltr:
                            if (_dragExtent + delta > 0) {
                                _dragExtent += delta;
                            }

                            break;
                    }

                    break;
            }

            if (oldDragExtent.sign() != _dragExtent.sign()) {
                setState(() => { _updateMoveAnimation(); });
            }

            if (!_moveController.isAnimating) {
                _moveController.setValue(_dragExtent.abs() / _overallDragAxisExtent);
            }
        }

        void _updateMoveAnimation() {
            float end = _dragExtent.sign();
            _moveAnimation = _moveController.drive(
                new OffsetTween(
                    begin: Offset.zero,
                    end: _directionIsXAxis
                        ? new Offset(end, widget.crossAxisEndOffset)
                        : new Offset(widget.crossAxisEndOffset, end)
                )
            );
        }

        _FlingGestureKind _describeFlingGesture(Velocity velocity) {
            if (_dragExtent == 0.0f) {
                return _FlingGestureKind.none;
            }

            float vx = velocity.pixelsPerSecond.dx;
            float vy = velocity.pixelsPerSecond.dy;
            DismissDirection? flingDirection;
            if (_directionIsXAxis) {
                if (vx.abs() - vy.abs() < _kMinFlingVelocityDelta || vx.abs() < _kMinFlingVelocity) {
                    return _FlingGestureKind.none;
                }

                D.assert(vx != 0.0f);
                flingDirection = _extentToDirection(vx);
            }
            else {
                if (vy.abs() - vx.abs() < _kMinFlingVelocityDelta || vy.abs() < _kMinFlingVelocity) {
                    return _FlingGestureKind.none;
                }

                D.assert(vy != 0.0);
                flingDirection = _extentToDirection(vy);
            }

            D.assert(_dismissDirection != null);
            if (flingDirection == _dismissDirection) {
                return _FlingGestureKind.forward;
            }

            return _FlingGestureKind.reverse;
        }

        void _handleDragEnd(DragEndDetails details) {
            if (!_isActive || _moveController.isAnimating) {
                return;
            }

            _dragUnderway = false;
            if (_moveController.isCompleted) {
                _confirmStartResizeAnimation().then_((value) => {
                    if (value) {
                        _startResizeAnimation();
                        return;
                    }
                });
            }

            float flingVelocity = _directionIsXAxis
                ? details.velocity.pixelsPerSecond.dx
                : details.velocity.pixelsPerSecond.dy;
            switch (_describeFlingGesture(details.velocity)) {
                case _FlingGestureKind.forward:
                    D.assert(_dragExtent != 0.0f);
                    D.assert(!_moveController.isDismissed);
                    if ((widget.dismissThresholds.getOrDefault(_dismissDirection) ??
                         _kDismissThreshold) >= 1.0) {
                        _moveController.reverse();
                        break;
                    }

                    _dragExtent = flingVelocity.sign();
                    _moveController.fling(velocity: flingVelocity.abs() * _kFlingVelocityScale);
                    break;
                case _FlingGestureKind.reverse:
                    D.assert(_dragExtent != 0.0f);
                    D.assert(!_moveController.isDismissed);
                    _dragExtent = flingVelocity.sign();
                    _moveController.fling(velocity: -flingVelocity.abs() * _kFlingVelocityScale);
                    break;
                case _FlingGestureKind.none:
                    if (!_moveController.isDismissed) {
                        // we already know it's not completed, we check that above
                        if (_moveController.value >
                            (widget.dismissThresholds.getOrDefault(_dismissDirection) ??
                             _kDismissThreshold)) {
                            _moveController.forward();
                        }
                        else {
                            _moveController.reverse();
                        }
                    }

                    break;
            }
        }

        void _handleDismissStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.completed && !_dragUnderway) {
                _confirmStartResizeAnimation().then_((value) => {
                    if (value) {
                        _startResizeAnimation();
                    }
                    else {
                        _moveController.reverse();
                    }

                    updateKeepAlive();
                });
            }
        }

        Future<bool> _confirmStartResizeAnimation() {
            if (widget.confirmDismiss != null) {
                DismissDirection? direction = _dismissDirection;
                D.assert(direction != null);
                return widget.confirmDismiss(direction);
            }

            return Future.value(true).to<bool>();
        }

        void _startResizeAnimation() {
            D.assert(_moveController != null);
            D.assert(_moveController.isCompleted);
            D.assert(_resizeController == null);
            D.assert(_sizePriorToCollapse == null);
            if (widget.resizeDuration == null) {
                if (widget.onDismissed != null) {
                    DismissDirection? direction = _dismissDirection;
                    D.assert(direction != null);
                    widget.onDismissed(direction);
                }
            }
            else {
                _resizeController = new AnimationController(duration: widget.resizeDuration, vsync: this);
                _resizeController.addListener(_handleResizeProgressChanged);
                _resizeController.addStatusListener((AnimationStatus status) => updateKeepAlive());
                _resizeController.forward();
                setState(() => {
                    _sizePriorToCollapse = context.size;
                    _resizeAnimation = _resizeController.drive(
                        new CurveTween(
                            curve: _kResizeTimeCurve
                        )
                    ).drive(
                        new FloatTween(
                            begin: 1.0f,
                            end: 0.0f
                        )
                    );
                });
            }
        }

        void _handleResizeProgressChanged() {
            if (_resizeController.isCompleted) {
                if (widget.onDismissed != null) {
                    DismissDirection? direction = _dismissDirection;
                    D.assert(direction != null);
                    widget.onDismissed(direction);
                }
            }
            else {
                if (widget.onResize != null) {
                    widget.onResize();
                }
            }
        }

        public override Widget build(BuildContext context) {
            base.build(context); // See AutomaticKeepAliveClientMixin.

            D.assert(!_directionIsXAxis || WidgetsD.debugCheckHasDirectionality(context));

            Widget background = widget.background;
            if (widget.secondaryBackground != null) {
                DismissDirection? direction = _dismissDirection;
                if (direction == DismissDirection.endToStart || direction == DismissDirection.up) {
                    background = widget.secondaryBackground;
                }
            }

            if (_resizeAnimation != null) {
                // we've been dragged aside, and are now resizing.
                D.assert(() => {
                    if (_resizeAnimation.status != AnimationStatus.forward) {
                        D.assert(_resizeAnimation.status == AnimationStatus.completed);
                        throw new UIWidgetsError(new List<DiagnosticsNode> {
                            new ErrorSummary("A dismissed Dismissible widget is still part of the tree."),
                            new ErrorHint(
                                "Make sure to implement the onDismissed handler and to immediately remove the Dismissible " +
                                "widget from the application once that handler has fired."
                            )
                        });
                    }

                    return true;
                });

                return new SizeTransition(
                    sizeFactor: _resizeAnimation,
                    axis: _directionIsXAxis ? Axis.vertical : Axis.horizontal,
                    child: new SizedBox(
                        width: _sizePriorToCollapse.width,
                        height: _sizePriorToCollapse.height,
                        child: background
                    )
                );
            }

            Widget content = new SlideTransition(
                position: _moveAnimation,
                child: widget.child
            );

            if (background != null) {
                List<Widget> children = new List<Widget>();

                if (!_moveAnimation.isDismissed) {
                    children.Add(Positioned.fill(
                        child: new ClipRect(
                            clipper: new _DismissibleClipper(
                                axis: _directionIsXAxis ? Axis.horizontal : Axis.vertical,
                                moveAnimation: _moveAnimation
                            ),
                            child: background
                        )
                    ));
                }

                children.Add(content);
                content = new Stack(children: children);
            }

            return new GestureDetector(
                onHorizontalDragStart: _directionIsXAxis ? (GestureDragStartCallback) _handleDragStart : null,
                onHorizontalDragUpdate: _directionIsXAxis
                    ? (GestureDragUpdateCallback) _handleDragUpdate
                    : null,
                onHorizontalDragEnd: _directionIsXAxis ? (GestureDragEndCallback) _handleDragEnd : null,
                onVerticalDragStart: _directionIsXAxis ? null : (GestureDragStartCallback) _handleDragStart,
                onVerticalDragUpdate: _directionIsXAxis
                    ? null
                    : (GestureDragUpdateCallback) _handleDragUpdate,
                onVerticalDragEnd: _directionIsXAxis ? null : (GestureDragEndCallback) _handleDragEnd,
                behavior: HitTestBehavior.opaque,
                child: content,
                dragStartBehavior: widget.dragStartBehavior
            );
        }
    }
}