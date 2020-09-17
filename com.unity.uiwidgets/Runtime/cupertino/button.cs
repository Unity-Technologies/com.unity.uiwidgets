using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoButtonUtils {
        public static readonly Color _kDisabledBackground = new Color(0xFFA9A9A9);
        public static readonly Color _kDisabledForeground = new Color(0xFFD1D1D1);
        public static readonly EdgeInsets _kButtonPadding = EdgeInsets.all(16.0f);
        public static readonly EdgeInsets _kBackgroundButtonPadding = EdgeInsets.symmetric(vertical: 14.0f, horizontal: 64.0f);
    }

    public class CupertinoButton : StatefulWidget {
        public CupertinoButton(
            Widget child,
            VoidCallback onPressed,
            Key key = null,
            EdgeInsets padding = null,
            Color color = null,
            Color disabledColor = null,
            float minSize = 44.0f,
            float pressedOpacity = 0.1f,
            BorderRadius borderRadius = null,
            bool filled = false
        ) : base(key: key) {
            D.assert(pressedOpacity >= 0.0 && pressedOpacity <= 1.0);
            _filled = filled;
            this.child = child;
            this.onPressed = onPressed;
            this.padding = padding;
            this.color = color;
            this.disabledColor = disabledColor;
            this.minSize = minSize;
            this.pressedOpacity = pressedOpacity;
            this.borderRadius = borderRadius ?? BorderRadius.all(Radius.circular(8.0f));
        }

        public static CupertinoButton filled(
            Widget child,
            VoidCallback onPressed,
            Key key = null,
            EdgeInsets padding = null,
            Color disabledColor = null,
            float minSize = 44.0f,
            float pressedOpacity = 0.1f,
            BorderRadius borderRadius = null
        ) {
            D.assert(pressedOpacity >= 0.0 && pressedOpacity <= 1.0);
            return new CupertinoButton(
                key: key,
                color: null,
                child: child,
                onPressed: onPressed,
                padding: padding,
                disabledColor: disabledColor,
                minSize: minSize,
                pressedOpacity: pressedOpacity,
                borderRadius: borderRadius,
                filled: true
            );
        }

        public readonly Widget child;

        public readonly EdgeInsets padding;

        public readonly Color color;

        public readonly Color disabledColor;

        public readonly VoidCallback onPressed;

        public readonly float minSize;

        public readonly float? pressedOpacity;

        public readonly BorderRadius borderRadius;
        public readonly bool _filled;

        public bool enabled {
            get { return onPressed != null; }
        }

        public override State createState() {
            return new _CupertinoButtonState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("enabled", value: enabled, ifFalse: "disabled"));
        }
    }

    class _CupertinoButtonState : SingleTickerProviderStateMixin<CupertinoButton> {
        static readonly TimeSpan kFadeOutDuration = new TimeSpan(0, 0, 0, 0, 10);
        static readonly TimeSpan kFadeInDuration = new TimeSpan(0, 0, 0, 0, 100);
        public readonly FloatTween _opacityTween = new FloatTween(begin: 1.0f, end: 0.0f);
        AnimationController _animationController;
        Animation<float> _opacityAnimation;

        public override void initState() {
            base.initState();
            _animationController = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 200),
                value: 0.0f,
                vsync: this);
            _opacityAnimation = _animationController
                .drive(new CurveTween(curve: Curves.decelerate))
                .drive(_opacityTween);
            _setTween();
        }

        public override void didUpdateWidget(StatefulWidget old) {
            base.didUpdateWidget(old);
            _setTween();
        }

        void _setTween() {
            if (widget != null) {
                _opacityTween.end = widget.pressedOpacity ?? 1.0f;
            }
        }

        public override void dispose() {
            _animationController.dispose();
            _animationController = null;
            base.dispose();
        }

        bool _buttonHeldDown = false;

        void _handleTapDown(TapDownDetails evt) {
            if (!_buttonHeldDown) {
                _buttonHeldDown = true;
                _animate();
            }
        }

        void _handleTapUp(TapUpDetails evt) {
            if (_buttonHeldDown) {
                _buttonHeldDown = false;
                _animate();
            }
        }

        void _handleTapCancel() {
            if (_buttonHeldDown) {
                _buttonHeldDown = false;
                _animate();
            }
        }

        void _animate() {
            if (_animationController.isAnimating) {
                return;
            }

            bool wasHeldDown = _buttonHeldDown;

            TickerFuture ticker = _buttonHeldDown
                ? _animationController.animateTo(1.0f, duration: kFadeOutDuration)
                : _animationController.animateTo(0.0f, duration: kFadeInDuration);

            ticker.Then(() => {
                if (mounted && wasHeldDown != _buttonHeldDown) {
                    _animate();
                }
            });
        }

        public override Widget build(BuildContext context) {
            bool enabled = widget.enabled;
            Color primaryColor = CupertinoTheme.of(context).primaryColor;
            Color backgroundColor = widget.color ?? (widget._filled ? primaryColor : null);

            Color foregroundColor = backgroundColor != null
                ? CupertinoTheme.of(context).primaryContrastingColor
                : enabled
                    ? primaryColor
                    : CupertinoButtonUtils._kDisabledForeground;

            TextStyle textStyle =
                CupertinoTheme.of(context).textTheme.textStyle.copyWith(color: foregroundColor);
            return new GestureDetector(
                behavior: HitTestBehavior.opaque,
                onTapDown: enabled ? _handleTapDown : (GestureTapDownCallback) null,
                onTapUp: enabled ? _handleTapUp : (GestureTapUpCallback) null,
                onTapCancel: enabled ? _handleTapCancel : (GestureTapCancelCallback) null,
                onTap: widget.onPressed == null
                    ? (GestureTapCallback) null
                    : () => {
                        if (widget.onPressed != null) {
                            widget.onPressed();
                        }
                    },
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(
                        minWidth: widget.minSize,
                        minHeight: widget.minSize
                    ),
                    child: new FadeTransition(
                        opacity: _opacityAnimation,
                        child: new DecoratedBox(
                            decoration: new BoxDecoration(
                                borderRadius: widget.borderRadius,
                                color: backgroundColor != null && !enabled
                                    ? widget.disabledColor ?? CupertinoButtonUtils._kDisabledBackground
                                    : backgroundColor
                            ),
                            child: new Padding(
                                padding: widget.padding ?? (backgroundColor != null
                                             ? CupertinoButtonUtils._kBackgroundButtonPadding
                                             : CupertinoButtonUtils._kButtonPadding),
                                child: new Center(
                                    widthFactor: 1.0f,
                                    heightFactor: 1.0f,
                                    child: new DefaultTextStyle(
                                        style: textStyle,
                                        child: new IconTheme(
                                            data: new IconThemeData(color: foregroundColor),
                                            child: widget.child
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}