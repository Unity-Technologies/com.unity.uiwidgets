using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public static readonly EdgeInsets _defaultInsetPadding =
            EdgeInsets.symmetric(horizontal: 40.0f, vertical: 24.0f);
    }

    public class Dialog : StatelessWidget {
        public Dialog(
            Key key = null,
            Color backgroundColor = null,
            float? elevation = null,
            TimeSpan? insetAnimationDuration = null,
            Curve insetAnimationCurve = null,
            EdgeInsets insetPadding = null,
            Clip clipBehavior = Clip.none,
            ShapeBorder shape = null,
            Widget child = null
        ) : base(key: key) {
            if (insetPadding == null) {
                insetPadding = material_._defaultInsetPadding;
            }

            this.child = child;
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.insetAnimationDuration = insetAnimationDuration ?? new TimeSpan(0, 0, 0, 0, 100);
            this.insetAnimationCurve = insetAnimationCurve ?? Curves.decelerate;
            this.insetPadding = insetPadding;
            this.clipBehavior = clipBehavior;
            this.shape = shape;
        }

        public readonly Color backgroundColor;

        public readonly float? elevation;

        public readonly TimeSpan insetAnimationDuration;

        public readonly Curve insetAnimationCurve;

        public readonly EdgeInsets insetPadding;

        public readonly Clip clipBehavior;

        public readonly ShapeBorder shape;

        public readonly Widget child;

        public static readonly RoundedRectangleBorder _defaultDialogShape =
            new RoundedRectangleBorder(borderRadius: BorderRadius.all(Radius.circular(2.0f)));

        const float _defaultElevation = 24.0f;

        public override Widget build(BuildContext context) {
            DialogTheme dialogTheme = DialogTheme.of(context);
            EdgeInsets effectivePadding = MediaQuery.of(context).viewInsets + (insetPadding ?? EdgeInsets.all(0.0f));

            return new AnimatedPadding(
                padding: effectivePadding,
                duration: insetAnimationDuration,
                curve: insetAnimationCurve,
                child: MediaQuery.removeViewInsets(
                    removeLeft: true,
                    removeTop: true,
                    removeRight: true,
                    removeBottom: true,
                    context: context,
                    child: new Center(
                        child: new ConstrainedBox(
                            constraints: new BoxConstraints(minWidth: 280.0f),
                            child: new Material(
                                color: backgroundColor ?? dialogTheme.backgroundColor ??
                                Theme.of(context).dialogBackgroundColor,
                                elevation: elevation ?? dialogTheme.elevation ?? _defaultElevation,
                                shape: shape ?? dialogTheme.shape ?? _defaultDialogShape,
                                type: MaterialType.card,
                                clipBehavior: clipBehavior,
                                child: child
                            )
                        )
                    )
                )
            );
        }
    }

    public class AlertDialog : StatelessWidget {
        public AlertDialog(
            Key key = null,
            Widget title = null,
            EdgeInsetsGeometry titlePadding = null,
            TextStyle titleTextStyle = null,
            Widget content = null,
            EdgeInsetsGeometry contentPadding = null,
            TextStyle contentTextStyle = null,
            List<Widget> actions = null,
            EdgeInsetsGeometry actionsPadding = null,
            VerticalDirection actionsOverflowDirection = VerticalDirection.up,
            float actionsOverflowButtonSpacing = 0,
            EdgeInsetsGeometry buttonPadding = null,
            Color backgroundColor = null,
            float? elevation = null,
            EdgeInsets insetPadding = null,
            Clip clipBehavior = Clip.none,
            ShapeBorder shape = null,
            bool scrollable = false
        ) : base(key: key) {
            this.title = title;
            this.titlePadding = titlePadding;
            this.titleTextStyle = titleTextStyle;
            this.content = content;
            this.contentPadding = contentPadding ?? EdgeInsets.fromLTRB(24.0f, 20.0f, 24.0f, 24.0f);
            this.contentTextStyle = contentTextStyle;
            this.actions = actions;
            this.actionsPadding = actionsPadding ?? EdgeInsets.zero;
            this.actionsOverflowDirection = actionsOverflowDirection;
            this.actionsOverflowButtonSpacing = actionsOverflowButtonSpacing;
            this.buttonPadding = buttonPadding;
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.insetPadding = insetPadding ?? material_._defaultInsetPadding;
            this.clipBehavior = clipBehavior;
            this.shape = shape;
            this.scrollable = scrollable;
        }

        public readonly Widget title;
        public readonly EdgeInsetsGeometry titlePadding;
        public readonly TextStyle titleTextStyle;
        public readonly Widget content;
        public readonly EdgeInsetsGeometry contentPadding;
        public readonly TextStyle contentTextStyle;
        public readonly List<Widget> actions;
        public readonly EdgeInsetsGeometry actionsPadding;
        public readonly VerticalDirection actionsOverflowDirection;
        public readonly float actionsOverflowButtonSpacing;
        public readonly EdgeInsetsGeometry buttonPadding;

        public readonly Color backgroundColor;
        public readonly float? elevation;
        public readonly EdgeInsets insetPadding;
        public readonly Clip clipBehavior;

        public readonly ShapeBorder shape;
        public readonly bool scrollable;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));

            ThemeData theme = Theme.of(context);
            DialogTheme dialogTheme = DialogTheme.of(context);
            
            Widget titleWidget = null;
            Widget contentWidget = null;
            Widget actionsWidget = null;
            if (title != null) {
                titleWidget = new Padding(
                    padding: titlePadding ?? EdgeInsets.fromLTRB(24.0f, 24.0f, 24.0f, content == null ? 20.0f : 0.0f),
                    child: new DefaultTextStyle(
                        style: titleTextStyle ?? dialogTheme.titleTextStyle ?? theme.textTheme.headline6,
                        child: title
                    )
                );
            }

            if (content != null) {
                contentWidget = new Padding(
                    padding: contentPadding,
                    child: new DefaultTextStyle(
                        style: contentTextStyle ?? dialogTheme.contentTextStyle ?? theme.textTheme.subtitle1,
                        child: content
                    )
                );
            }

            if (actions != null) {
                actionsWidget = new Padding(
                    padding: actionsPadding,
                    child: new ButtonBar(
                        buttonPadding: buttonPadding,
                        overflowDirection: actionsOverflowDirection,
                        overflowButtonSpacing: actionsOverflowButtonSpacing,
                        children: actions
                    )
                );
            }

            List<Widget> columnChildren;
            if (scrollable) {
                var titleList = new List<Widget>();

                if (title != null) {
                    titleList.Add(titleWidget);
                }

                if (content != null) {
                    titleList.Add(contentWidget);
                }

                columnChildren = new List<Widget>();

                if (title != null || content != null) {
                    columnChildren.Add(new Flexible(
                        child: new SingleChildScrollView(
                            child: new Column(
                                mainAxisSize: MainAxisSize.min,
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children: titleList
                            )
                        )
                    ));
                }

                if (actions != null) {
                    columnChildren.Add(actionsWidget);
                }
            }
            else {
                columnChildren = new List<Widget>();
                if (title != null) {
                    columnChildren.Add(titleWidget);
                }

                if (content != null) {
                    columnChildren.Add(new Flexible(child: contentWidget));
                }

                if (actions != null) {
                    columnChildren.Add(actionsWidget);
                }
            }

            Widget dialogChild = new IntrinsicWidth(
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: columnChildren
                )
            );

            return new Dialog(
                backgroundColor: backgroundColor,
                elevation: elevation,
                insetPadding: insetPadding,
                clipBehavior: clipBehavior,
                shape: shape,
                child: dialogChild
            );
        }
    }


    public class SimpleDialogOption : StatelessWidget {
        public SimpleDialogOption(
            Key key = null,
            VoidCallback onPressed = null,
            EdgeInsets padding = null,
            Widget child = null
        ) : base(key: key) {
            this.onPressed = onPressed;
            this.padding = padding;
            this.child = child;
        }

        public readonly VoidCallback onPressed;

        public readonly Widget child;
        public readonly EdgeInsets padding;

        public override Widget build(BuildContext context) {
            return new InkWell(
                onTap: () => onPressed?.Invoke(),
                child: new Padding(
                    padding: padding ?? EdgeInsets.symmetric(vertical: 8.0f, horizontal: 24.0f),
                    child: child
                )
            );
        }
    }

    public class SimpleDialog : StatelessWidget {
        public SimpleDialog(
            Key key = null,
            Widget title = null,
            EdgeInsets titlePadding = null,
            List<Widget> children = null,
            EdgeInsets contentPadding = null,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null
        ) : base(key: key) {
            this.title = title;
            this.titlePadding = titlePadding ?? EdgeInsets.fromLTRB(24.0f, 24.0f, 24.0f, 0.0f);
            this.children = children;
            this.contentPadding = contentPadding ?? EdgeInsets.fromLTRB(0.0f, 12.0f, 0.0f, 16.0f);
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.shape = shape;
        }

        public readonly Widget title;

        public readonly EdgeInsets titlePadding;

        public readonly List<Widget> children;

        public readonly EdgeInsets contentPadding;

        public readonly Color backgroundColor;

        public readonly float? elevation;

        public readonly ShapeBorder shape;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            ThemeData theme = Theme.of(context);

            List<Widget> body = new List<Widget>();

            if (title != null) {
                body.Add(new Padding(
                    padding: titlePadding,
                    child: new DefaultTextStyle(
                        style: theme.textTheme.headline6,
                        child: title
                    )
                ));
            }

            if (children != null) {
                body.Add(new Flexible(
                    child: new SingleChildScrollView(
                        padding: contentPadding,
                        child: new ListBody(children: children)
                    )
                ));
            }

            Widget dialogChild = new IntrinsicWidth(
                stepWidth: 56.0f,
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(minWidth: 280.0f),
                    child: new Column(
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: body
                    )
                )
            );

            return new Dialog(
                backgroundColor: backgroundColor,
                elevation: elevation,
                shape: shape,
                child: dialogChild
            );
        }
    }

    public partial class material_ {
        static Widget _buildMaterialDialogTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return new FadeTransition(
                opacity: new CurvedAnimation(
                    parent: animation,
                    curve: Curves.easeOut
                ),
                child: child
            );
        }

        public static Future<T> showDialog<T>(
            BuildContext context = null,
            bool barrierDismissible = true,
            Widget child = null,
            WidgetBuilder builder = null,
            bool useRootNavigator = true,
            RouteSettings routeSettings = null
        ) {
            D.assert(child == null || builder == null);
            D.assert(debugCheckHasMaterialLocalizations(context));

            ThemeData theme = Theme.of(context, shadowThemeOnly: true);
            return widgets.DialogUtils.showGeneralDialog<T>(
                context: context,
                pageBuilder: (buildContext, animation, secondaryAnimation) => {
                    Widget pageChild = child ?? new Builder(builder: builder);
                    return new SafeArea(
                        child: new Builder(
                            builder: (_) => theme != null
                                ? new Theme(data: theme, child: pageChild)
                                : pageChild)
                    );
                },
                barrierDismissible: barrierDismissible,
                barrierColor: Colors.black54,
                transitionDuration: new TimeSpan(0, 0, 0, 0, 150),
                transitionBuilder: _buildMaterialDialogTransitions,
                useRootNavigator: useRootNavigator,
                routeSettings: routeSettings
            );
        }
    }
}