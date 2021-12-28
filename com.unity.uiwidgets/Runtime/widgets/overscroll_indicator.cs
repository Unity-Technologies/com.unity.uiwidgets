using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class GlowingOverscrollIndicator : StatefulWidget {
        public GlowingOverscrollIndicator(
            Key key = null,
            bool showLeading = true,
            bool showTrailing = true,
            AxisDirection axisDirection = AxisDirection.up,
            Color color = null,
            ScrollNotificationPredicate notificationPredicate = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(color != null);
            this.showLeading = showLeading;
            this.showTrailing = showTrailing;
            this.axisDirection = axisDirection;
            this.child = child;
            this.color = color;
            this.notificationPredicate = notificationPredicate ?? ScrollNotification.defaultScrollNotificationPredicate;
        }

        public readonly bool showLeading;

        public readonly bool showTrailing;

        public readonly AxisDirection axisDirection;

        public Axis? axis {
            get { return AxisUtils.axisDirectionToAxis(axisDirection); }
        }

        public readonly Color color;

        public readonly ScrollNotificationPredicate notificationPredicate;

        public readonly Widget child;

        public override State createState() {
            return new _GlowingOverscrollIndicatorState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection>("axisDirection", axisDirection));
            string showDescription;
            if (showLeading && showTrailing) {
                showDescription = "both sides";
            }
            else if (showLeading) {
                showDescription = "leading side only";
            }
            else if (showTrailing) {
                showDescription = "trailing side only";
            }
            else {
                showDescription = "neither side (!)";
            }

            properties.add(new MessageProperty("show", showDescription));
            properties.add(new ColorProperty("color", color, showName: false));
        }
    }

    class _GlowingOverscrollIndicatorState : TickerProviderStateMixin<GlowingOverscrollIndicator> {
        _GlowController _leadingController;
        _GlowController _trailingController;
        Listenable _leadingAndTrailingListener;

        public override void initState() {
            base.initState();
            _leadingController =
                new _GlowController(vsync: this, color: widget.color, axis: widget.axis);
            _trailingController =
                new _GlowController(vsync: this, color: widget.color, axis: widget.axis);
            _leadingAndTrailingListener = ListenableUtils.merge(new List<Listenable>
                {_leadingController, _trailingController});
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            GlowingOverscrollIndicator oldWidget = _oldWidget as GlowingOverscrollIndicator;
            if (oldWidget.color != widget.color || oldWidget.axis != widget.axis) {
                D.assert(widget.axis != null);
                _leadingController.color = widget.color;
                _leadingController.axis = widget.axis.Value;
                _trailingController.color = widget.color;
                _trailingController.axis = widget.axis.Value;
            }
        }

        Type _lastNotificationType;
        Dictionary<bool, bool> _accepted = new Dictionary<bool, bool> {{false, true}, {true, true}};

        bool _handleScrollNotification(ScrollNotification notification) {
            if (!widget.notificationPredicate(notification)) {
                return false;
            }

            if (notification is OverscrollNotification) {
                _GlowController controller = null;
                OverscrollNotification _notification = notification as OverscrollNotification;
                if (_notification.overscroll < 0.0f) {
                    controller = _leadingController;
                }
                else if (_notification.overscroll > 0.0f) {
                    controller = _trailingController;
                }
                else {
                    D.assert(false, () =>"over scroll cannot be 0.0f.");
                }

                bool isLeading = controller == _leadingController;
                if (_lastNotificationType != typeof(OverscrollNotification)) {
                    OverscrollIndicatorNotification confirmationNotification =
                        new OverscrollIndicatorNotification(leading: isLeading);
                    confirmationNotification.dispatch(context);
                    _accepted[isLeading] = confirmationNotification._accepted;
                }

                D.assert(controller != null);
                D.assert(_notification.metrics.axis() == widget.axis);
                if (_accepted[isLeading]) {
                    if (_notification.velocity != 0.0f) {
                        D.assert(_notification.dragDetails == null);
                        controller.absorbImpact(_notification.velocity.abs());
                    }
                    else {
                        D.assert(_notification.overscroll != 0.0f);
                        if (_notification.dragDetails != null) {
                            D.assert(_notification.dragDetails.globalPosition != null);
                            RenderBox renderer = (RenderBox) _notification.context.findRenderObject();
                            D.assert(renderer != null);
                            D.assert(renderer.hasSize);
                            Size size = renderer.size;
                            Offset position = renderer.globalToLocal(_notification.dragDetails.globalPosition);
                            switch (_notification.metrics.axis()) {
                                case Axis.horizontal:
                                    controller.pull(_notification.overscroll.abs(), size.width,
                                        position.dy.clamp(0.0f, size.height), size.height);
                                    break;
                                case Axis.vertical:
                                    controller.pull(_notification.overscroll.abs(), size.height,
                                        position.dx.clamp(0.0f, size.width), size.width);
                                    break;
                            }
                        }
                    }
                }
            }
            else if (notification is ScrollEndNotification || notification is ScrollUpdateNotification) {
                if ((notification as ScrollEndNotification).dragDetails != null) {
                    _leadingController.scrollEnd();
                    _trailingController.scrollEnd();
                }
            }

            _lastNotificationType = notification.GetType();
            return false;
        }

        public override void dispose() {
            _leadingController.dispose();
            _trailingController.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: _handleScrollNotification,
                child: new RepaintBoundary(
                    child: new CustomPaint(
                        foregroundPainter: new _GlowingOverscrollIndicatorPainter(
                            leadingController: widget.showLeading ? _leadingController : null,
                            trailingController: widget.showTrailing ? _trailingController : null,
                            axisDirection: widget.axisDirection,
                            repaint: _leadingAndTrailingListener
                        ),
                        child: new RepaintBoundary(
                            child: widget.child
                        )
                    )
                )
            );
        }
    }


    enum _GlowState {
        idle,
        absorb,
        pull,
        recede
    }

    class _GlowController : ChangeNotifier {
        public _GlowController(
            TickerProvider vsync,
            Color color,
            Axis? axis
        ) {
            D.assert(vsync != null);
            D.assert(color != null);
            D.assert(axis != null);
            _color = color;
            _axis = axis.Value;
            _glowController = new AnimationController(vsync: vsync);
            _glowController.addStatusListener(_changePhase);
            Animation<float> decelerator = new CurvedAnimation(
                parent: _glowController,
                curve: Curves.decelerate
            );
            decelerator.addListener(notifyListeners);
            _glowOpacity = decelerator.drive(_glowOpacityTween);
            _glowSize = decelerator.drive(_glowSizeTween);
            _displacementTicker = vsync.createTicker(_tickDisplacement);
        }

        _GlowState _state = _GlowState.idle;
        AnimationController _glowController;
        Timer _pullRecedeTimer;

        FloatTween _glowOpacityTween = new FloatTween(begin: 0.0f, end: 0.0f);
        Animation<float> _glowOpacity;
        FloatTween _glowSizeTween = new FloatTween(begin: 0.0f, end: 0.0f);
        Animation<float> _glowSize;

        Ticker _displacementTicker;
        TimeSpan? _displacementTickerLastElapsed;
        float _displacementTarget = 0.5f;
        float _displacement = 0.5f;

        float _pullDistance = 0.0f;

        public Color color {
            get { return _color; }
            set {
                D.assert(color != null);
                if (color == value) {
                    return;
                }

                _color = value;
                notifyListeners();
            }
        }

        Color _color;

        public Axis axis {
            get { return _axis; }
            set {
                if (axis == value) {
                    return;
                }

                _axis = value;
                notifyListeners();
            }
        }

        Axis _axis;

        readonly TimeSpan _recedeTime = new TimeSpan(0, 0, 0, 0, 600);
        readonly TimeSpan _pullTime = new TimeSpan(0, 0, 0, 0, 167);
        readonly TimeSpan _pullHoldTime = new TimeSpan(0, 0, 0, 0, 167);
        readonly TimeSpan _pullDecayTime = new TimeSpan(0, 0, 0, 0, 2000);
        static readonly TimeSpan _crossAxisHalfTime = new TimeSpan(0, 0, 0, 0, (1000.0f / 60.0f).round());

        const float _maxOpacity = 0.5f;
        const float _pullOpacityGlowFactor = 0.8f;
        const float _velocityGlowFactor = 0.00006f;
        const float _sqrt3 = 1.73205080757f; // Mathf.Sqrt(3)
        const float _widthToHeightFactor = (3.0f / 4.0f) * (2.0f - _sqrt3);

        const float _minVelocity = 100.0f; // logical pixels per second
        const float _maxVelocity = 10000.0f; // logical pixels per second

        public override void dispose() {
            _glowController.dispose();
            _displacementTicker.Dispose();
            _pullRecedeTimer?.cancel();
            base.dispose();
        }

        public void absorbImpact(float velocity) {
            D.assert(velocity >= 0.0f);
            _pullRecedeTimer?.cancel();
            _pullRecedeTimer = null;
            velocity = velocity.clamp(_minVelocity, _maxVelocity);
            _glowOpacityTween.begin = _state == _GlowState.idle ? 0.3f : _glowOpacity.value;
            _glowOpacityTween.end =
                (velocity * _velocityGlowFactor).clamp(_glowOpacityTween.begin, _maxOpacity);
            _glowSizeTween.begin = _glowSize.value;
            _glowSizeTween.end = Mathf.Min(0.025f + 7.5e-7f * velocity * velocity, 1.0f);
            _glowController.duration = new TimeSpan(0, 0, 0, 0, (0.15f + velocity * 0.02f).round());
            _glowController.forward(from: 0.0f);
            _displacement = 0.5f;
            _state = _GlowState.absorb;
        }

        public void pull(float overscroll, float extent, float crossAxisOffset, float crossExtent) {
            _pullRecedeTimer?.cancel();
            _pullDistance +=
                overscroll / 200.0f; // This factor is magic. Not clear why we need it to match Android.
            _glowOpacityTween.begin = _glowOpacity.value;
            _glowOpacityTween.end =
                Mathf.Min(_glowOpacity.value + overscroll / extent * _pullOpacityGlowFactor, _maxOpacity);
            float height = Mathf.Min(extent, crossExtent * _widthToHeightFactor);
            _glowSizeTween.begin = _glowSize.value;
            _glowSizeTween.end = Mathf.Max(1.0f - 1.0f / (0.7f * Mathf.Sqrt(_pullDistance * height)),
                _glowSize.value);
            _displacementTarget = crossAxisOffset / crossExtent;
            if (_displacementTarget != _displacement) {
                if (!_displacementTicker.isTicking) {
                    D.assert(_displacementTickerLastElapsed == null);
                    _displacementTicker.start();
                }
            }
            else {
                _displacementTicker.stop();
                _displacementTickerLastElapsed = null;
            }

            _glowController.duration = _pullTime;
            if (_state != _GlowState.pull) {
                _glowController.forward(from: 0.0f);
                _state = _GlowState.pull;
            }
            else {
                if (!_glowController.isAnimating) {
                    D.assert(_glowController.value == 1.0f);
                    notifyListeners();
                }
            }

            _pullRecedeTimer =
                Timer.create(_pullHoldTime, () => {
                    _recede(_pullDecayTime);
                    return null;
                });
        }

        public void scrollEnd() {
            if (_state == _GlowState.pull) {
                _recede(_recedeTime);
            }
        }

        void _changePhase(AnimationStatus status) {
            if (status != AnimationStatus.completed) {
                return;
            }

            switch (_state) {
                case _GlowState.absorb:
                    _recede(_recedeTime);
                    break;
                case _GlowState.recede:
                    _state = _GlowState.idle;
                    _pullDistance = 0.0f;
                    break;
                case _GlowState.pull:
                case _GlowState.idle:
                    break;
            }
        }

        void _recede(TimeSpan duration) {
            if (_state == _GlowState.recede || _state == _GlowState.idle) {
                return;
            }

            _pullRecedeTimer?.cancel();
            _pullRecedeTimer = null;
            _glowOpacityTween.begin = _glowOpacity.value;
            _glowOpacityTween.end = 0.0f;
            _glowSizeTween.begin = _glowSize.value;
            _glowSizeTween.end = 0.0f;
            _glowController.duration = duration;
            _glowController.forward(from: 0.0f);
            _state = _GlowState.recede;
        }

        void _tickDisplacement(TimeSpan elapsed) {
            if (_displacementTickerLastElapsed != null) {
                float? t = elapsed.Milliseconds - _displacementTickerLastElapsed?.Milliseconds;
                _displacement = _displacementTarget - (_displacementTarget - _displacement) *
                    Mathf.Pow(2.0f, (-t ?? 0.0f) / _crossAxisHalfTime.Milliseconds);
                notifyListeners();
            }

            if (PhysicsUtils.nearEqual(_displacementTarget, _displacement,
                Tolerance.defaultTolerance.distance)) {
                _displacementTicker.stop();
                _displacementTickerLastElapsed = null;
            }
            else {
                _displacementTickerLastElapsed = elapsed;
            }
        }

        public void paint(Canvas canvas, Size size) {
            if (_glowOpacity.value == 0.0f) {
                return;
            }

            float baseGlowScale = size.width > size.height ? size.height / size.width : 1.0f;
            float radius = size.width * 3.0f / 2.0f;
            float height = Mathf.Min(size.height, size.width * _widthToHeightFactor);
            float scaleY = _glowSize.value * baseGlowScale;
            Rect rect = Rect.fromLTWH(0.0f, 0.0f, size.width, height);
            Offset center = new Offset((size.width / 2.0f) * (0.5f + _displacement), height - radius);
            Paint paint = new Paint();
            paint.color = color.withOpacity(_glowOpacity.value);
            canvas.save();
            canvas.scale(1.0f, scaleY);
            canvas.clipRect(rect);
            canvas.drawCircle(center, radius, paint);
            canvas.restore();
        }
    }

    class _GlowingOverscrollIndicatorPainter : AbstractCustomPainter {
        public _GlowingOverscrollIndicatorPainter(
            _GlowController leadingController,
            _GlowController trailingController,
            AxisDirection axisDirection,
            Listenable repaint
        ) : base(
            repaint: repaint
        ) {
            this.leadingController = leadingController;
            this.trailingController = trailingController;
            this.axisDirection = axisDirection;
        }

        public readonly _GlowController leadingController;

        public readonly _GlowController trailingController;

        public readonly AxisDirection axisDirection;

        const float piOver2 = Mathf.PI / 2.0f;

        void _paintSide(Canvas canvas, Size size, _GlowController controller, AxisDirection axisDirection,
            GrowthDirection growthDirection) {
            if (controller == null) {
                return;
            }

            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(axisDirection, growthDirection)) {
                case AxisDirection.up:
                    controller.paint(canvas, size);
                    break;
                case AxisDirection.down:
                    canvas.save();
                    canvas.translate(0.0f, size.height);
                    canvas.scale(1.0f, -1.0f);
                    controller.paint(canvas, size);
                    canvas.restore();
                    break;
                case AxisDirection.left:
                    canvas.save();
                    canvas.rotate(piOver2);
                    canvas.scale(1.0f, -1.0f);
                    controller.paint(canvas, new Size(size.height, size.width));
                    canvas.restore();
                    break;
                case AxisDirection.right:
                    canvas.save();
                    canvas.translate(size.width, 0.0f);
                    canvas.rotate(piOver2);
                    controller.paint(canvas, new Size(size.height, size.width));
                    canvas.restore();
                    break;
            }
        }

        public override void paint(Canvas canvas, Size size) {
            _paintSide(canvas, size, leadingController, axisDirection, GrowthDirection.reverse);
            _paintSide(canvas, size, trailingController, axisDirection, GrowthDirection.forward);
        }

        public override bool shouldRepaint(CustomPainter _oldDelegate) {
            _GlowingOverscrollIndicatorPainter oldDelegate = _oldDelegate as _GlowingOverscrollIndicatorPainter;
            return oldDelegate.leadingController != leadingController
                   || oldDelegate.trailingController != trailingController;
        }
    }

    public class OverscrollIndicatorNotification : ViewportNotificationMixinNotification {
        public OverscrollIndicatorNotification(
            bool leading
        ) {
            this.leading = leading;
        }

        public readonly bool leading;

        internal bool _accepted = true;

        public void disallowGlow() {
            _accepted = false;
        }

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add($"side: {(leading ? "leading edge" : "trailing edge")}");
        }
    }
}