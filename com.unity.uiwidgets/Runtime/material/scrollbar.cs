using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    static class ScrollbarUtils {
        public static readonly TimeSpan _kScrollbarFadeDuration = TimeSpan.FromMilliseconds(300);

        public static readonly TimeSpan _kScrollbarTimeToFade = TimeSpan.FromMilliseconds(600);

        public const float _kScrollbarThickness = 6.0f;
    }


    public class Scrollbar : StatefulWidget {
        public Scrollbar(
            Key key = null,
            Widget child = null,
            ScrollController controller = null,
            bool isAlwaysShown = false) : base(key: key) {
            this.child = child;
            this.controller = controller;
            this.isAlwaysShown = isAlwaysShown;
        }

        public readonly Widget child;

        public readonly ScrollController controller;

        public readonly bool isAlwaysShown;

        public override State createState() {
            return new _ScrollbarState();
        }
    }

    public class _ScrollbarState : TickerProviderStateMixin<Scrollbar> {
        public ScrollbarPainter _materialPainter;
        public TextDirection _textDirection;
        public Color _themeColor;

        bool? _useCupertinoScrollbar = null;

        public AnimationController _fadeoutAnimationController;
        public Animation<float> _fadeoutOpacityAnimation;
        public Timer _fadeoutTimer;

        public override void initState() {
            base.initState();
            _fadeoutAnimationController = new AnimationController(
                vsync: this,
                duration: ScrollbarUtils._kScrollbarFadeDuration
            );
            _fadeoutOpacityAnimation = new CurvedAnimation(
                parent: _fadeoutAnimationController,
                curve: Curves.fastOutSlowIn
            );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();

            D.assert((() => {
                _useCupertinoScrollbar = null;
                return true;
            }));

            ThemeData theme = Theme.of(context);

            switch (theme.platform) {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    _fadeoutTimer?.cancel();
                    _fadeoutTimer = null;
                    _fadeoutAnimationController.reset();
                    _useCupertinoScrollbar = true;
                    break;
                default:
                    _themeColor = theme.highlightColor.withOpacity(1.0f);
                    _textDirection = Directionality.of(context);
                    _materialPainter = _buildMaterialScrollbarPainter();
                    _useCupertinoScrollbar = false;
                    WidgetsBinding.instance.addPostFrameCallback((TimeSpan duration) => {
                        if (widget.isAlwaysShown) {
                            D.assert(widget.controller != null);
                            widget.controller.position.didUpdateScrollPositionBy(0);
                        }
                    });
                    break;
            }

            D.assert(_useCupertinoScrollbar != null);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = (Scrollbar) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (widget.isAlwaysShown != _oldWidget.isAlwaysShown) {
                D.assert(widget.controller != null);
                if (widget.isAlwaysShown == false) {
                    _fadeoutAnimationController.reverse();
                }
                else {
                    _fadeoutAnimationController.animateTo(1.0f);
                }
            }
        }

        public ScrollbarPainter _buildMaterialScrollbarPainter() {
            return new ScrollbarPainter(
                color: _themeColor,
                      textDirection: _textDirection,
                  thickness: ScrollbarUtils._kScrollbarThickness,
                 fadeoutOpacityAnimation: _fadeoutOpacityAnimation,
                  padding: MediaQuery.of(context).padding
            );
        }

        bool _handleScrollNotification(ScrollNotification notification) {
            ScrollMetrics metrics = notification.metrics;
            if (metrics.maxScrollExtent <= metrics.minScrollExtent) {
                return false;
            }
            if (!_useCupertinoScrollbar.Value &&
                (notification is ScrollUpdateNotification ||
                 notification is OverscrollNotification)) {
                if (_fadeoutAnimationController.status != AnimationStatus.forward) {
                    _fadeoutAnimationController.forward();
                }

                _materialPainter.update(
                    notification.metrics,
                    notification.metrics.axisDirection
                );
                if (!widget.isAlwaysShown) {
                    _fadeoutTimer?.cancel();
                    _fadeoutTimer = Timer.create(ScrollbarUtils._kScrollbarTimeToFade, () => {
                        _fadeoutAnimationController.reverse();
                        _fadeoutTimer = null;
                    });
                }
            }
            return false;
        }

        public override void dispose() {
            _fadeoutAnimationController.dispose();
            _fadeoutTimer?.cancel();
            _materialPainter?.dispose();

            base.dispose();
        }

        public override Widget build(BuildContext context) {
            if (_useCupertinoScrollbar.Value) {
                return new CupertinoScrollbar(
                    child: widget.child,
                    isAlwaysShown: widget.isAlwaysShown,
                    controller: widget.controller
                );
            }
            
            return new NotificationListener<ScrollNotification>(
                onNotification: _handleScrollNotification,
                child: new RepaintBoundary(
                    child: new CustomPaint(
                        foregroundPainter: _materialPainter,
                        child: new RepaintBoundary(
                            child: widget.child
                        )
                    )
                )
            );
        }
    }
}