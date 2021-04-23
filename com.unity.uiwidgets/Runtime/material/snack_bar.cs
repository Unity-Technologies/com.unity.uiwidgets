using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    static class SnackBarUtils {
        public const float _singleLineVerticalPadding = 14.0f;

        public static readonly TimeSpan _snackBarTransitionDuration = new TimeSpan(0, 0, 0, 0, 250);
        public static readonly TimeSpan _snackBarDisplayDuration = new TimeSpan(0, 0, 0, 0, 4000);
        public static readonly Curve _snackBarHeightCurve = Curves.fastOutSlowIn;
        public static readonly Curve _snackBarFadeInCurve = new Interval(0.45f, 1.0f, curve: Curves.fastOutSlowIn);
        public static readonly Curve _snackBarFadeOutCurve = new Interval(0.72f, 1.0f, curve: Curves.fastOutSlowIn);
    }

    public enum SnackBarClosedReason {
        action,
        dismiss,
        swipe,
        hide,
        remove,
        timeout
    }

    public class SnackBarAction : StatefulWidget {
        public SnackBarAction(
            Key key = null,
            Color textColor = null,
            Color disabledTextColor = null,
            string label = null,
            VoidCallback onPressed = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(onPressed != null);
            this.textColor = textColor;
            this.disabledTextColor = disabledTextColor;
            this.label = label;
            this.onPressed = onPressed;
        }

        public readonly Color textColor;

        public readonly Color disabledTextColor;

        public readonly string label;

        public readonly VoidCallback onPressed;

        public override State createState() {
            return new _SnackBarActionState();
        }
    }


    class _SnackBarActionState : State<SnackBarAction> {
        bool _haveTriggeredAction = false;

        void _handlePressed() {
            if (_haveTriggeredAction) {
                return;
            }

            setState(() => { _haveTriggeredAction = true; });

            widget.onPressed();
            Scaffold.of(context).hideCurrentSnackBar(reason: SnackBarClosedReason.action);
        }

        public override Widget build(BuildContext context) {
            SnackBarThemeData snackBarTheme = Theme.of(context).snackBarTheme;
            Color textColor = widget.textColor ?? snackBarTheme.actionTextColor;
            Color disabledTextColor = widget.disabledTextColor ?? snackBarTheme.disabledActionTextColor;

            return new FlatButton(
                onPressed: _haveTriggeredAction ? (VoidCallback) null : _handlePressed,
                child: new Text(widget.label),
                textColor: textColor,
                disabledTextColor: disabledTextColor
            );
        }
    }

    public class SnackBar : StatefulWidget {
        public SnackBar(
            Key key = null,
            Widget content = null,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            SnackBarBehavior? behavior = null,
            SnackBarAction action = null,
            TimeSpan? duration = null,
            Animation<float> animation = null,
            VoidCallback onVisible = null
        ) : base(key: key) {
            duration = duration ?? SnackBarUtils._snackBarDisplayDuration;
            D.assert(content != null);
            D.assert(elevation == null || elevation >= 0.0);
            this.content = content;
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.shape = shape;
            this.behavior = behavior;
            this.action = action;
            this.duration = duration.Value;
            this.animation = animation;
            this.onVisible = onVisible;
        }

        public readonly Widget content;

        public readonly Color backgroundColor;

        public readonly float? elevation;

        public readonly ShapeBorder shape;

        public readonly SnackBarBehavior? behavior;

        public readonly SnackBarAction action;

        public readonly TimeSpan duration;

        public readonly Animation<float> animation;

        public readonly VoidCallback onVisible;

        internal static AnimationController createAnimationController(TickerProvider vsync) {
            return new AnimationController(
                duration: SnackBarUtils._snackBarTransitionDuration,
                debugLabel: "SnackBar",
                vsync: vsync
            );
        }

        internal SnackBar withAnimation(Animation<float> newAnimation, Key fallbackKey = null) {
            return new SnackBar(
                key: key ?? fallbackKey,
                content: content,
                backgroundColor: backgroundColor,
                elevation: elevation,
                shape: shape,
                behavior: behavior,
                action: action,
                duration: duration,
                animation: newAnimation,
                onVisible: onVisible
            );
        }

        public override State createState() {
            return new _SnackBarState();
        }
    }

    class _SnackBarState : State<SnackBar> {
        bool _wasVisible = false;

        public override void initState() {
            base.initState();
            widget.animation.addStatusListener(_onAnimationStatusChanged);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = (SnackBar) oldWidget;
            if (widget.animation != _oldWidget.animation) {
                _oldWidget.animation.removeStatusListener(_onAnimationStatusChanged);
                widget.animation.addStatusListener(_onAnimationStatusChanged);
            }

            base.didUpdateWidget(oldWidget);
        }

        public override void dispose() {
            widget.animation.removeStatusListener(_onAnimationStatusChanged);
            base.dispose();
        }

        void _onAnimationStatusChanged(AnimationStatus animationStatus) {
            switch (animationStatus) {
                case AnimationStatus.dismissed:
                case AnimationStatus.forward:
                case AnimationStatus.reverse:
                    break;
                case AnimationStatus.completed:
                    if (widget.onVisible != null && !_wasVisible) {
                        widget.onVisible();
                    }

                    _wasVisible = true;
                    break;
            }
        }

        public override Widget build(BuildContext context) {
            MediaQueryData mediaQueryData = MediaQuery.of(context);
            D.assert(widget.animation != null);
            ThemeData theme = Theme.of(context);
            ColorScheme colorScheme = theme.colorScheme;
            SnackBarThemeData snackBarTheme = theme.snackBarTheme;
            bool isThemeDark = theme.brightness == Brightness.dark;

            Brightness brightness = isThemeDark ? Brightness.light : Brightness.dark;
            Color themeBackgroundColor = isThemeDark
                ? colorScheme.onSurface
                : Color.alphaBlend(colorScheme.onSurface.withOpacity(0.80f), colorScheme.surface);
            ThemeData inverseTheme = new ThemeData(
                brightness: brightness,
                backgroundColor: themeBackgroundColor,
                colorScheme: new ColorScheme(
                    primary: colorScheme.onPrimary,
                    primaryVariant: colorScheme.onPrimary,
                    secondary: isThemeDark ? colorScheme.primaryVariant : colorScheme.secondary,
                    secondaryVariant: colorScheme.onSecondary,
                    surface: colorScheme.onSurface,
                    background: themeBackgroundColor,
                    error: colorScheme.onError,
                    onPrimary: colorScheme.primary,
                    onSecondary: colorScheme.secondary,
                    onSurface: colorScheme.surface,
                    onBackground: colorScheme.background,
                    onError: colorScheme.error,
                    brightness: brightness
                ),
                snackBarTheme: snackBarTheme
            );

            TextStyle contentTextStyle = snackBarTheme.contentTextStyle ?? inverseTheme.textTheme.subtitle1;
            SnackBarBehavior snackBarBehavior = widget.behavior ?? snackBarTheme.behavior ?? SnackBarBehavior.fix;
            bool isFloatingSnackBar = snackBarBehavior == SnackBarBehavior.floating;
            float snackBarPadding = isFloatingSnackBar ? 16.0f : 24.0f;

            CurvedAnimation heightAnimation =
                new CurvedAnimation(parent: widget.animation, curve: SnackBarUtils._snackBarHeightCurve);
            CurvedAnimation fadeInAnimation =
                new CurvedAnimation(parent: widget.animation, curve: SnackBarUtils._snackBarFadeInCurve);
            CurvedAnimation fadeOutAnimation = new CurvedAnimation(
                parent: widget.animation,
                curve: SnackBarUtils._snackBarFadeOutCurve,
                reverseCurve: new Threshold(0.0f)
            );

            var childrenList = new List<Widget>() {
                new SizedBox(width: snackBarPadding),
                new Expanded(
                    child: new Container(
                        padding: EdgeInsets.symmetric(vertical: SnackBarUtils._singleLineVerticalPadding),
                        child: new DefaultTextStyle(
                            style: contentTextStyle,
                            child: widget.content
                        )
                    )
                )
            };
            if (widget.action != null) {
                childrenList.Add(new ButtonTheme(
                    textTheme: ButtonTextTheme.accent,
                    minWidth: 64.0f,
                    padding: EdgeInsets.symmetric(horizontal: snackBarPadding),
                    child: widget.action
                ));
            }
            else {
                childrenList.Add(new SizedBox(width: snackBarPadding));
            }

            Widget snackBar = new SafeArea(
                top: false,
                bottom: !isFloatingSnackBar,
                child: new Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: childrenList
                )
            );

            float elevation = widget.elevation ?? snackBarTheme.elevation ?? 6.0f;
            Color backgroundColor =
                widget.backgroundColor ?? snackBarTheme.backgroundColor ?? inverseTheme.backgroundColor;
            ShapeBorder shape = widget.shape
                                ?? snackBarTheme.shape
                                ?? (isFloatingSnackBar
                                    ? new RoundedRectangleBorder(borderRadius: BorderRadius.circular(4.0f))
                                    : null);

            snackBar = new Material(
                shape: shape,
                elevation: elevation,
                color: backgroundColor,
                child: new Theme(
                    data: inverseTheme,
                    child: mediaQueryData.accessibleNavigation
                        ? snackBar
                        : new FadeTransition(
                            opacity: fadeOutAnimation,
                            child: snackBar
                        )
                )
            );

            if (isFloatingSnackBar) {
                snackBar = new Padding(
                    padding: EdgeInsets.fromLTRB(15.0f, 5.0f, 15.0f, 10.0f),
                    child: snackBar
                );
            }

            snackBar = new Dismissible(
                key: Key.key("dismissible"),
                direction: DismissDirection.down,
                resizeDuration: null,
                onDismissed: (DismissDirection? direction) => {
                    Scaffold.of(context).removeCurrentSnackBar(reason: SnackBarClosedReason.swipe);
                },
                child: snackBar
            );

            Widget snackBarTransition = null;
            if (mediaQueryData.accessibleNavigation) {
                snackBarTransition = snackBar;
            }
            else if (isFloatingSnackBar) {
                snackBarTransition = new FadeTransition(
                    opacity: fadeInAnimation,
                    child: snackBar
                );
            }
            else {
                snackBarTransition = new AnimatedBuilder(
                    animation: heightAnimation,
                    builder: (BuildContext subContext, Widget subChild) => {
                        return new Align(
                            alignment: AlignmentDirectional.topStart,
                            heightFactor: heightAnimation.value,
                            child: subChild
                        );
                    },
                    child: snackBar
                );
            }

            return new ClipRect(child: snackBarTransition);
        }
    }
}