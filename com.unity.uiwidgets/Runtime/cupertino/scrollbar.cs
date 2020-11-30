using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    class CupertinoScrollbarUtils {
        public static readonly Color _kScrollbarColor = new Color(0x99777777);
        public const float _kScrollbarThickness = 2.5f;
        public const float _kScrollbarMainAxisMargin = 4.0f;
        public const float _kScrollbarCrossAxisMargin = 2.5f;
        public const float _kScrollbarMinLength = 36.0f;
        public const float _kScrollbarMinOverscrollLength = 8.0f;
        public static readonly Radius _kScrollbarRadius = Radius.circular(1.25f);
        public static readonly TimeSpan _kScrollbarTimeToFade = new TimeSpan(0, 0, 0, 0, 50);
        public static readonly TimeSpan _kScrollbarFadeDuration = new TimeSpan(0, 0, 0, 0, 250);
    }

    public class CupertinoScrollbar : StatefulWidget {
        public CupertinoScrollbar(
            Widget child,
            Key key = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;
        public readonly ScrollController controller;
        public readonly bool isAlwaysShown;

        public override State createState() {
            return new _CupertinoScrollbarState();
        }
    }

    class _CupertinoScrollbarState : TickerProviderStateMixin<CupertinoScrollbar> {
        ScrollbarPainter _painter;
        
        TextDirection _textDirection;
        
        AnimationController _fadeoutAnimationController;
        Animation<float> _fadeoutOpacityAnimation;
        float _dragScrollbarPositionY;
        Timer _fadeoutTimer;
        
        

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
        }


        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _textDirection = Directionality.of(context);
            _painter = _buildCupertinoScrollbarPainter();
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

        bool _handleScrollNotification(ScrollNotification notification) {
            if (notification is ScrollUpdateNotification ||
                notification is OverscrollNotification) {
                if (_fadeoutAnimationController.status != AnimationStatus.forward) {
                    _fadeoutAnimationController.forward();
                }

                _fadeoutTimer?.cancel();
                _painter.update(notification.metrics, notification.metrics.axisDirection);
            }
            else if (notification is ScrollEndNotification) {
                if (_dragScrollbarPositionY.Equals(0f)) {
                    _startFadeoutTimer();
                }

                /*_fadeoutTimer?.cancel();
                _fadeoutTimer = Window.instance.run(CupertinoScrollbarUtils._kScrollbarTimeToFade, () => {
                    _fadeoutAnimationController.reverse();
                    _fadeoutTimer = null;
                });*/
            }

            return false;
        }


        public override void dispose() {
            _fadeoutAnimationController.dispose();
            _fadeoutTimer?.cancel();
            _painter.dispose();
            base.dispose();
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
        

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: _handleScrollNotification,
                child: new RepaintBoundary(
                    child: new CustomPaint(
                        foregroundPainter: _painter,
                        child: new RepaintBoundary(
                            child: widget.child
                        )
                    )
                )
            );
        }
    }
}