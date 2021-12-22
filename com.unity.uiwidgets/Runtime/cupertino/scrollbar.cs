using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    class CupertinoScrollbarUtils {
        public const float _kScrollbarMinLength = 36.0f;
        public const float _kScrollbarMinOverscrollLength = 8.0f;

        public static readonly TimeSpan _kScrollbarTimeToFade = TimeSpan.FromMilliseconds(1200);
        public static readonly TimeSpan _kScrollbarFadeDuration = TimeSpan.FromMilliseconds(250);
        public static readonly TimeSpan _kScrollbarResizeDuration = TimeSpan.FromMilliseconds(100);
        
        public static readonly Color _kScrollbarColor = CupertinoDynamicColor.withBrightness(
            color: new Color(0x59000000),
            darkColor: new Color(0x80FFFFFF)
        );
        public const float _kScrollbarThickness = 3f;
        public const float _kScrollbarThicknessDragging = 8.0f;
        public static Radius _kScrollbarRadius = Radius.circular(1.5f);
        public static Radius _kScrollbarRadiusDragging = Radius.circular(4.0f);
        public const float _kScrollbarMainAxisMargin = 3.0f;
        public const float _kScrollbarCrossAxisMargin = 3.0f;

        public static bool _hitTestInteractive(GlobalKey customPaintKey, Offset offset) {
            if (customPaintKey.currentContext == null) {
                return false;
            }
             CustomPaint customPaint = customPaintKey.currentContext.widget as CustomPaint;
             ScrollbarPainter painter = customPaint.foregroundPainter as ScrollbarPainter;
             RenderBox renderBox = customPaintKey.currentContext.findRenderObject() as RenderBox;
             Offset localOffset = renderBox.globalToLocal(offset);
             return painter.hitTestInteractive(localOffset);
        }
    }

    public class CupertinoScrollbar : StatefulWidget {
        public CupertinoScrollbar(
            Key key = null,
            ScrollController controller = null,
            bool isAlwaysShown = false,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
            this.controller = controller;
            this.isAlwaysShown = isAlwaysShown;
        }

        public readonly Widget child;
        public readonly ScrollController controller;
        public readonly bool isAlwaysShown;

        public override State createState() {
            return new _CupertinoScrollbarState();
        }
    }

    class _CupertinoScrollbarState : TickerProviderStateMixin<CupertinoScrollbar> {
        
        GlobalKey _customPaintKey = GlobalKey.key();
        ScrollbarPainter _painter;
        
        TextDirection _textDirection;
        
        AnimationController _fadeoutAnimationController;
        Animation<float> _fadeoutOpacityAnimation;
        AnimationController _thicknessAnimationController;
        
        float? _dragScrollbarPositionY;
        Timer _fadeoutTimer;
        Drag _drag;

        float _thickness {
            get {
                return CupertinoScrollbarUtils._kScrollbarThickness + _thicknessAnimationController.value * (CupertinoScrollbarUtils._kScrollbarThicknessDragging - CupertinoScrollbarUtils._kScrollbarThickness);
            }
        }

        Radius _radius {
            get {
                return Radius.lerp(CupertinoScrollbarUtils._kScrollbarRadius, CupertinoScrollbarUtils._kScrollbarRadiusDragging, _thicknessAnimationController.value);
            }
        }

        ScrollController _currentController;
        ScrollController _controller {
            get { 
                return widget.controller ?? PrimaryScrollController.of(context);
                
            }
        }

        public override void initState() {
            base.initState();
            _fadeoutAnimationController = new AnimationController(
                vsync: this,
                duration: CupertinoScrollbarUtils._kScrollbarFadeDuration
            );
            _fadeoutOpacityAnimation = new CurvedAnimation(
                parent: _fadeoutAnimationController,
                curve: Curves.fastOutSlowIn
            );
            _thicknessAnimationController = new AnimationController(
                vsync: this,
                duration: CupertinoScrollbarUtils._kScrollbarResizeDuration
            );
            _thicknessAnimationController.addListener(() => {
                _painter.updateThickness(_thickness, _radius);
            });
        }
        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (_painter == null) {
                _painter = _buildCupertinoScrollbarPainter(context);
            } else {
                _painter.textDirection = Directionality.of(context);
                _painter.color = CupertinoDynamicColor.resolve(CupertinoScrollbarUtils._kScrollbarColor, context);
                _painter.padding = MediaQuery.of(context).padding;
            }
            WidgetsBinding.instance.addPostFrameCallback((TimeSpan duration)=> {
                if (widget.isAlwaysShown) {
                    D.assert(widget.controller != null);
                    widget.controller.position.didUpdateScrollPositionBy(0);
                }
            });
        }
        
        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CupertinoScrollbar) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (widget.isAlwaysShown != ((CupertinoScrollbar)oldWidget).isAlwaysShown) {
                if (widget.isAlwaysShown == true) {
                    D.assert(widget.controller != null);
                    _fadeoutAnimationController.animateTo(1.0f);
                } else {
                    _fadeoutAnimationController.reverse();
                }
            }
        }




        public ScrollbarPainter _buildCupertinoScrollbarPainter(BuildContext context) {
            return new ScrollbarPainter(
                color: CupertinoDynamicColor.resolve(CupertinoScrollbarUtils._kScrollbarColor, context),
                textDirection: Directionality.of(context),
                thickness: _thickness,
                fadeoutOpacityAnimation: _fadeoutOpacityAnimation,
                mainAxisMargin: CupertinoScrollbarUtils._kScrollbarMainAxisMargin,
                crossAxisMargin: CupertinoScrollbarUtils._kScrollbarCrossAxisMargin,
                radius: _radius,
                padding: MediaQuery.of(context).padding,
                minLength: CupertinoScrollbarUtils._kScrollbarMinLength,
                minOverscrollLength: CupertinoScrollbarUtils._kScrollbarMinOverscrollLength
            );
        }
        
        void _dragScrollbar(float primaryDelta) {
            D.assert(_currentController != null);

            float scrollOffsetLocal = _painter.getTrackToScroll(primaryDelta);
            float scrollOffsetGlobal = scrollOffsetLocal + _currentController.position.pixels;

            if (_drag == null) {
                _drag = _currentController.position.drag(
                    new DragStartDetails(
                        globalPosition: new Offset(0.0f, scrollOffsetGlobal)
                    ),
                    () =>{}
                );
            } else {
               
                _drag.update(
                    new DragUpdateDetails(
                        delta: new Offset(0.0f, -scrollOffsetLocal),
                        primaryDelta: (float?) -1f * scrollOffsetLocal,
                        globalPosition: new Offset(0.0f, scrollOffsetGlobal)
                        )
                    );
            }
        }

        void _startFadeoutTimer() {
            if (!widget.isAlwaysShown) {
                _fadeoutTimer?.cancel();
                _fadeoutTimer = Timer.create(CupertinoScrollbarUtils._kScrollbarTimeToFade, () => {
                    _fadeoutAnimationController.reverse();
                    _fadeoutTimer = null;
                });
            }
        }

        bool _checkVertical() {
            return _currentController.position.axis() == Axis.vertical;
        }

        float _pressStartY = 0.0f;
        
        void _handleLongPressStart(LongPressStartDetails details) {
            _currentController = _controller;
            if (!_checkVertical()) {
                return;
            }
            _pressStartY = details.localPosition.dy;
            _fadeoutTimer?.cancel();
            _fadeoutAnimationController.forward();
            _dragScrollbar(details.localPosition.dy);
            _dragScrollbarPositionY = details.localPosition.dy;
        }

        void _handleLongPress() {
            if (!_checkVertical()) {
                return;
            }
            _fadeoutTimer?.cancel();
            _thicknessAnimationController.forward().then(
                (_) => { return; }
            );
        }

        void _handleLongPressMoveUpdate(LongPressMoveUpdateDetails details) {
            if (!_checkVertical()) {
                return;
            }
            D.assert(_dragScrollbarPositionY != null);
            _dragScrollbar(details.localPosition.dy - _dragScrollbarPositionY.Value);
            _dragScrollbarPositionY = details.localPosition.dy;
        }

        void _handleLongPressEnd(LongPressEndDetails details) {
            if (!_checkVertical()) {
                return;
            }
            _handleDragScrollEnd(details.velocity.pixelsPerSecond.dy);
            if (details.velocity.pixelsPerSecond.dy.abs() < 10 &&
                (details.localPosition.dy - _pressStartY).abs() > 0) {
                //HapticFeedback.mediumImpact();
            }
            _currentController = null;
        }
        
        void _handleDragScrollEnd(float trackVelocityY) {
            _startFadeoutTimer();
            _thicknessAnimationController.reverse();
            _dragScrollbarPositionY = null;
            float scrollVelocityY = _painter.getTrackToScroll(trackVelocityY);
            _drag?.end(new DragEndDetails(
              primaryVelocity: -scrollVelocityY,
              velocity: new Velocity(
                pixelsPerSecond: new Offset(
                  0.0f,
                  -scrollVelocityY
                )
              )
            ));
            _drag = null;
        }
        bool _handleScrollNotification(ScrollNotification notification) {
            ScrollMetrics metrics = notification.metrics;
            if (metrics.maxScrollExtent <= metrics.minScrollExtent) {
              return false;
            }

            if (notification is ScrollUpdateNotification ||
                notification is OverscrollNotification) {
                if (_fadeoutAnimationController.status != AnimationStatus.forward) {
                    _fadeoutAnimationController.forward();
                }
                _fadeoutTimer?.cancel();
                _painter.update(notification.metrics, notification.metrics.axisDirection);
            } else if (notification is ScrollEndNotification) {
                if (_dragScrollbarPositionY == null) {
                    _startFadeoutTimer();
                }
            }
            return false;
        }

        Dictionary<Type, GestureRecognizerFactory>  _gestures {
            get {
                Dictionary<Type, GestureRecognizerFactory> gestures =
                    new Dictionary<Type, GestureRecognizerFactory>();

                gestures[typeof(_ThumbPressGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<_ThumbPressGestureRecognizer>(
                        () => new _ThumbPressGestureRecognizer(
                            debugOwner: this,
                            customPaintKey: _customPaintKey
                        ),
                        (_ThumbPressGestureRecognizer instance)=> {
                            instance.onLongPressStart = _handleLongPressStart;
                            instance.onLongPress = _handleLongPress;
                            instance.onLongPressMoveUpdate = _handleLongPressMoveUpdate;
                            instance.onLongPressEnd = _handleLongPressEnd;
                }
                );

                return gestures;
            }
        }

        public override void dispose() {
            _fadeoutAnimationController.dispose();
            _thicknessAnimationController.dispose();
            _fadeoutTimer?.cancel();
            _painter.dispose();
            base.dispose();
        }


        ScrollbarPainter _buildCupertinoScrollbarPainter() {
            return new ScrollbarPainter(
                color: CupertinoScrollbarUtils._kScrollbarColor,
                textDirection: _textDirection,
                thickness: CupertinoScrollbarUtils._kScrollbarThickness,
                fadeoutOpacityAnimation: _fadeoutOpacityAnimation,
                mainAxisMargin: CupertinoScrollbarUtils._kScrollbarMainAxisMargin,
                crossAxisMargin: CupertinoScrollbarUtils._kScrollbarCrossAxisMargin,
                radius: CupertinoScrollbarUtils._kScrollbarRadius,
                minLength: CupertinoScrollbarUtils._kScrollbarMinLength,
                minOverscrollLength: CupertinoScrollbarUtils._kScrollbarMinOverscrollLength
            );
        }

        


        

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: _handleScrollNotification,
                child: new RepaintBoundary(
                    child: new RawGestureDetector(
                        gestures: _gestures,
                        child: new CustomPaint(
                            key: _customPaintKey,
                            foregroundPainter: _painter,
                            child: new RepaintBoundary(child: widget.child)
                        )
                    )
                )
            );
        }
    }
    public class _ThumbPressGestureRecognizer : LongPressGestureRecognizer {
        public _ThumbPressGestureRecognizer(
            float? postAcceptSlopTolerance = null,
            PointerDeviceKind kind = default,
            object debugOwner = null,
            GlobalKey customPaintKey = null
        ) : base(
            postAcceptSlopTolerance: postAcceptSlopTolerance,
            kind: kind,
            debugOwner: debugOwner,
            duration: TimeSpan.FromMilliseconds(100)
        ) {
            _customPaintKey = customPaintKey;
        }


        public readonly  GlobalKey _customPaintKey;

        protected override bool isPointerAllowed(PointerDownEvent _event) {
            if (!CupertinoScrollbarUtils._hitTestInteractive(_customPaintKey, _event.position)) {
                return false;
            }
            return base.isPointerAllowed(_event);
        }
    }

    
}