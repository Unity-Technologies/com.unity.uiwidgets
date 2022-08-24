using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using TickerFuture = Unity.UIWidgets.scheduler.TickerFuture;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoButtonUtils {
        public static readonly EdgeInsets _kButtonPadding = EdgeInsets.all(16.0f);
        public static readonly EdgeInsets _kBackgroundButtonPadding = EdgeInsets.symmetric(
            vertical: 14.0f,
            horizontal: 64.0f);
        public static readonly float kMinInteractiveDimensionCupertino = 44.0f;
    }

    public class CupertinoButton : StatefulWidget {
        public CupertinoButton(
            Key key = null,
            Widget child = null,
            EdgeInsetsGeometry padding = null,
            Color color = null,
            Color disabledColor = null,
            float? minSize = 44.0f,
            float pressedOpacity = 0.4f,
            BorderRadius borderRadius = null,
            VoidCallback onPressed = null
        ) : base(key: key) {
            D.assert((pressedOpacity >= 0.0 && pressedOpacity <= 1.0));
             _filled = false;
            this.child = child;
            this.onPressed = onPressed;
            this.padding = padding;
            this.color = color;
            this.disabledColor = disabledColor ?? CupertinoColors.quaternarySystemFill;
            this.minSize = minSize;
            this.pressedOpacity = pressedOpacity;
            this.borderRadius = borderRadius ?? BorderRadius.all(Radius.circular(8.0f));
        }

        public static CupertinoButton filled(
            Key key = null,
            Widget child = null,
            EdgeInsetsGeometry padding = null,
            Color disabledColor = null,
            float? minSize = 44.0f,
            float pressedOpacity = 0.4f,
            BorderRadius borderRadius = null,
            VoidCallback onPressed = null
        ) {
            disabledColor = disabledColor ?? CupertinoColors.quaternarySystemFill;
            D.assert(pressedOpacity >= 0.0 && pressedOpacity <= 1.0); 
            var btn = new CupertinoButton(
                key: key,
                color: null,
                child: child,
                padding: padding,
                disabledColor: disabledColor,
                minSize: minSize,
                pressedOpacity: pressedOpacity,
                borderRadius: borderRadius,
                onPressed: onPressed
            );
            btn._filled = true;
            return btn;
        }

        public readonly Widget child;

        public readonly EdgeInsetsGeometry padding;

        public readonly Color color;

        public readonly Color disabledColor;

        public readonly VoidCallback onPressed;

        public readonly float? minSize;

        public readonly float pressedOpacity;

        public readonly BorderRadius borderRadius;
        public bool _filled;

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
        public readonly Tween<float> _opacityTween = new FloatTween(begin: 1.0f, end: 0.0f);//Tween<Float>
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
                _opacityTween.end = 1.0f;
                if (!widget.pressedOpacity.Equals(0f)) {
                    _opacityTween.end = widget.pressedOpacity;
                }
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

            ticker.then(_ => {
                if (mounted && wasHeldDown != _buttonHeldDown) {
                    _animate();
                }
            });
        }

        public override Widget build(BuildContext context) {
            bool enabled = widget.enabled;
            CupertinoThemeData themeData = CupertinoTheme.of(context);
            Color primaryColor = themeData.primaryColor;
            Color backgroundColor = (widget.color == null) 
                ? (widget._filled ? primaryColor : null) 
                : CupertinoDynamicColor.resolve(widget.color, context);

            Color foregroundColor = backgroundColor != null
                ? themeData.primaryContrastingColor
                : enabled
                    ? primaryColor
                    : CupertinoDynamicColor.resolve(CupertinoColors.placeholderText, context);

            TextStyle textStyle =
                themeData.textTheme.textStyle.copyWith(color: foregroundColor);

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
                    constraints: widget.minSize == null
                    ? new BoxConstraints() : 
                    new BoxConstraints(
                        minWidth: widget.minSize.Value,
                        minHeight: widget.minSize.Value
                    ),
                    child: new FadeTransition(
                        opacity: _opacityAnimation,
                        child: new DecoratedBox(
                            decoration: new BoxDecoration(
                                borderRadius: widget.borderRadius,
                                color: backgroundColor != null && !enabled
                                    ? CupertinoDynamicColor.resolve(widget.disabledColor, context)
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