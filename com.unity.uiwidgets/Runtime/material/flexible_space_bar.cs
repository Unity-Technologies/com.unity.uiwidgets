using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.material {
    public enum CollapseMode {
        parallax,
        pin,
        none
    }

    public enum StretchMode {
        zoomBackground,

        blurBackground,

        fadeTitle,
    }

    public class FlexibleSpaceBar : StatefulWidget {
        public FlexibleSpaceBar(
            Key key = null,
            Widget title = null,
            Widget background = null,
            bool? centerTitle = null,
            EdgeInsets titlePadding = null,
            CollapseMode collapseMode = CollapseMode.parallax,
            List<StretchMode> stretchModes = null
        ) : base(key: key) {
            this.title = title;
            this.background = background;
            this.centerTitle = centerTitle;
            this.titlePadding = titlePadding;
            this.collapseMode = collapseMode;
            this.stretchModes = stretchModes ?? new List<StretchMode> {StretchMode.zoomBackground};
        }

        public readonly Widget title;

        public readonly Widget background;

        public readonly bool? centerTitle;

        public readonly CollapseMode collapseMode;

        public readonly List<StretchMode> stretchModes;

        public readonly EdgeInsetsGeometry titlePadding;

        public static Widget createSettings(
            float? toolbarOpacity = null,
            float? minExtent = null,
            float? maxExtent = null,
            float? currentExtent = null,
            Widget child = null) {
            D.assert(currentExtent != null);
            D.assert(child != null);
            return new FlexibleSpaceBarSettings(
                toolbarOpacity: toolbarOpacity ?? 1.0f,
                minExtent: minExtent ?? currentExtent,
                maxExtent: maxExtent ?? currentExtent,
                currentExtent: currentExtent,
                child: child
            );
        }

        public override State createState() {
            return new _FlexibleSpaceBarState();
        }
    }


    class _FlexibleSpaceBarState : State<FlexibleSpaceBar> {
        bool? _getEffectiveCenterTitle(ThemeData themeData) {
            if (widget.centerTitle != null) {
                return widget.centerTitle;
            }

            switch (themeData.platform) {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return true;
                default:
                    return false;
            }
        }


        Alignment _getTitleAlignment(bool effectiveCenterTitle) {
            if (effectiveCenterTitle) {
                return Alignment.bottomCenter;
            }

            return Alignment.bottomLeft;
        }

        float? _getCollapsePadding(float t, FlexibleSpaceBarSettings settings) {
            switch (widget.collapseMode) {
                case CollapseMode.pin:
                    return -(settings.maxExtent.Value - settings.currentExtent.Value);
                case CollapseMode.none:
                    return 0.0f;
                case CollapseMode.parallax:
                    float deltaExtent = settings.maxExtent.Value - settings.minExtent.Value;
                    return -new FloatTween(begin: 0.0f, end: deltaExtent / 4.0f).lerp(t);
            }

            return null;
        }

        public override Widget build(BuildContext context) {
            return new LayoutBuilder(
                builder: (BuildContext _context, BoxConstraints constraints) => {
                    FlexibleSpaceBarSettings settings =
                        _context.dependOnInheritedWidgetOfExactType<FlexibleSpaceBarSettings>();
                    D.assert(settings != null,
                        () =>
                            "A FlexibleSpaceBar must be wrapped in the widget returned by FlexibleSpaceBar.createSettings().");

                    List<Widget> children = new List<Widget>();
                    float deltaExtent = settings.maxExtent.Value - settings.minExtent.Value;

                    float t = (1.0f - (settings.currentExtent.Value - settings.minExtent.Value) / deltaExtent)
                        .clamp(0.0f, 1.0f);

                    if (widget.background != null) {
                        float fadeStart = Mathf.Max(0.0f, 1.0f - material_.kToolbarHeight / deltaExtent);
                        float fadeEnd = 1.0f;
                        D.assert(fadeStart <= fadeEnd);

                        float opacity = 1.0f - new Interval(fadeStart, fadeEnd).transform(t);
                        if (opacity > 0.0f) {
                            float height = settings.maxExtent ?? 0;

                            if (widget.stretchModes.Contains(StretchMode.zoomBackground) &&
                                constraints.maxHeight > height) {
                                height = constraints.maxHeight;
                            }

                            children.Add(new Positioned(
                                    top: _getCollapsePadding(t, settings),
                                    left: 0.0f,
                                    right: 0.0f,
                                    height: height,
                                    child: new Opacity(
                                        opacity: opacity,
                                        child: widget.background)
                                )
                            );

                            if (widget.stretchModes.Contains(StretchMode.blurBackground) &&
                                constraints.maxHeight > settings.maxExtent) {
                                float blurAmount = (constraints.maxHeight - settings.maxExtent) / 10 ?? 0;
                                children.Add(Positioned.fill(
                                    child: new BackdropFilter(
                                        child: new Container(
                                            color: Colors.transparent
                                        ),
                                        filter: ui.ImageFilter.blur(
                                            sigmaX: blurAmount,
                                            sigmaY: blurAmount
                                        )
                                    )
                                ));
                            }
                        }
                    }

                    Widget title = null;
                    if (widget.title != null) {
                        switch (Application.platform) {
                            case RuntimePlatform.IPhonePlayer:
                                title = widget.title;
                                break;
                            default:
                                title = widget.title;
                                break;
                        }
                    }

                    if (widget.stretchModes.Contains(StretchMode.fadeTitle) &&
                        constraints.maxHeight > settings.maxExtent) {
                        float stretchOpacity =
                            1 - (((constraints.maxHeight - settings.maxExtent) / 100)?.clamp(0.0f, 1.0f) ?? 0);
                        title = new Opacity(
                            opacity: stretchOpacity,
                            child: title
                        );
                    }

                    ThemeData theme = Theme.of(_context);
                    float toolbarOpacity = settings.toolbarOpacity.Value;
                    if (toolbarOpacity > 0.0f) {
                        TextStyle titleStyle = theme.primaryTextTheme.title;
                        titleStyle = titleStyle.copyWith(
                            color: titleStyle.color.withOpacity(toolbarOpacity));

                        bool effectiveCenterTitle = _getEffectiveCenterTitle(theme).Value;
                        EdgeInsetsGeometry padding = widget.titlePadding ??
                                             EdgeInsets.only(
                                                 left: effectiveCenterTitle ? 0.0f : 72.0f,
                                                 bottom: 16.0f
                                             );
                        float scaleValue = new FloatTween(begin: 1.5f, end: 1.0f).lerp(t);
                        Matrix4 scaleTransform = Matrix4.diagonal3Values(scaleValue, scaleValue, 1);
                        Alignment titleAlignment = _getTitleAlignment(effectiveCenterTitle);

                        children.Add(new Container(
                                padding: padding,
                                child: new Transform(
                                    alignment: titleAlignment,
                                    transform: scaleTransform,
                                    child: new Align(
                                        alignment: titleAlignment,
                                        child: new DefaultTextStyle(
                                            style: titleStyle,
                                            child: new LayoutBuilder(
                                                builder: (BuildContext __context, BoxConstraints _constraints) => {
                                                    return new Container(
                                                        width: _constraints.maxWidth / scaleValue,
                                                        alignment: titleAlignment,
                                                        child: title
                                                    );
                                                }
                                            )
                                        )
                                    )
                                )
                            )
                        );
                    }

                    return new ClipRect(
                        child: new Stack(
                            children: children)
                    );
                }
            );
        }
    }


    public class FlexibleSpaceBarSettings : InheritedWidget {
        public FlexibleSpaceBarSettings(
            Key key = null,
            float? toolbarOpacity = null,
            float? minExtent = null,
            float? maxExtent = null,
            float? currentExtent = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(toolbarOpacity != null);
            D.assert(minExtent != null && minExtent >= 0);
            D.assert(maxExtent != null && maxExtent >= 0);
            D.assert(currentExtent != null && currentExtent >= 0);
            D.assert(toolbarOpacity >= 0.0);
            D.assert(minExtent <= maxExtent);
            D.assert(minExtent <= currentExtent);
            D.assert(currentExtent <= maxExtent);
            this.toolbarOpacity = toolbarOpacity;
            this.minExtent = minExtent;
            this.maxExtent = maxExtent;
            this.currentExtent = currentExtent;
        }

        public readonly float? toolbarOpacity;

        public readonly float? minExtent;

        public readonly float? maxExtent;

        public readonly float? currentExtent;


        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            FlexibleSpaceBarSettings _oldWidget = (FlexibleSpaceBarSettings) oldWidget;
            return toolbarOpacity != _oldWidget.toolbarOpacity
                   || minExtent != _oldWidget.minExtent
                   || maxExtent != _oldWidget.maxExtent
                   || currentExtent != _oldWidget.currentExtent;
        }
    }
}