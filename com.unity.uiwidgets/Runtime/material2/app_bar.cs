using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace Unity.UIWidgets.material {
    static class AppBarUtils {
        internal const float _kLeadingWidth = Constants.kToolbarHeight;
    }

    class _ToolbarContainerLayout : SingleChildLayoutDelegate {
        public _ToolbarContainerLayout() {
        }

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints.tighten(height: Constants.kToolbarHeight);
        }

        public override Size getSize(BoxConstraints constraints) {
            return new Size(constraints.maxWidth, Constants.kToolbarHeight);
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            return new Offset(0.0f, size.height - childSize.height);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            return false;
        }
    }

    public class AppBar : PreferredSizeWidget {
        public AppBar(
            Key key = null,
            Widget leading = null,
            bool automaticallyImplyLeading = true,
            Widget title = null,
            List<Widget> actions = null,
            Widget flexibleSpace = null,
            PreferredSizeWidget bottom = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Color backgroundColor = null,
            Brightness? brightness = null,
            IconThemeData iconTheme = null,
            IconThemeData actionsIconTheme = null,
            TextTheme textTheme = null,
            bool primary = true,
            bool? centerTitle = null,
            float titleSpacing = NavigationToolbar.kMiddleSpacing,
            float toolbarOpacity = 1.0f,
            float bottomOpacity = 1.0f
        ) : base(key: key) {
            D.assert(elevation == null || elevation >= 0.0);
            this.leading = leading;
            this.automaticallyImplyLeading = automaticallyImplyLeading;
            this.title = title;
            this.actions = actions;
            this.flexibleSpace = flexibleSpace;
            this.bottom = bottom;
            this.elevation = elevation;
            this.shape = shape;
            this.backgroundColor = backgroundColor;
            this.brightness = brightness;
            this.iconTheme = iconTheme;
            this.actionsIconTheme = actionsIconTheme;
            this.textTheme = textTheme;
            this.primary = primary;
            this.centerTitle = centerTitle;
            this.titleSpacing = titleSpacing;
            this.toolbarOpacity = toolbarOpacity;
            this.bottomOpacity = bottomOpacity;
            preferredSize = Size.fromHeight(Constants.kToolbarHeight + (bottom?.preferredSize?.height ?? 0.0f));
        }

        public readonly Widget leading;

        public readonly bool automaticallyImplyLeading;

        public readonly Widget title;

        public readonly List<Widget> actions;

        public readonly Widget flexibleSpace;

        public readonly PreferredSizeWidget bottom;

        public readonly float? elevation;

        public readonly ShapeBorder shape;

        public readonly Color backgroundColor;

        public readonly Brightness? brightness;

        public readonly IconThemeData iconTheme;

        public readonly IconThemeData actionsIconTheme;

        public readonly TextTheme textTheme;

        public readonly bool primary;

        public readonly bool? centerTitle;

        public readonly float titleSpacing;

        public readonly float toolbarOpacity;

        public readonly float bottomOpacity;

        public override Size preferredSize { get; }

        public bool? _getEffectiveCenterTitle(ThemeData themeData) {
            if (centerTitle != null) {
                return centerTitle;
            }

            switch (themeData.platform) {
                case RuntimePlatform.IPhonePlayer:
                    return actions == null || actions.Count < 2;
                default:
                    return false;
            }
        }

        public override State createState() {
            return new _AppBarState();
        }
    }


    class _AppBarState : State<AppBar> {
        const float _defaultElevation = 4.0f;

        void _handleDrawerButton() {
            Scaffold.of(context).openDrawer();
        }

        void _handleDrawerButtonEnd() {
            Scaffold.of(context).openEndDrawer();
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            ThemeData themeData = Theme.of(context);
            AppBarTheme appBarTheme = AppBarTheme.of(context);
            ScaffoldState scaffold = Scaffold.of(context, nullOk: true);
            ModalRoute parentRoute = ModalRoute.of(context);

            bool hasDrawer = scaffold?.hasDrawer ?? false;
            bool hasEndDrawer = scaffold?.hasEndDrawer ?? false;
            bool canPop = parentRoute?.canPop ?? false;
            bool useCloseButton = parentRoute is PageRoute && ((PageRoute) parentRoute).fullscreenDialog;

            IconThemeData overallIconTheme = widget.iconTheme
                                            ?? appBarTheme.iconTheme
                                            ?? themeData.primaryIconTheme;
            IconThemeData actionsIconTheme = widget.actionsIconTheme
                                             ?? appBarTheme.actionsIconTheme
                                             ?? overallIconTheme;
            TextStyle centerStyle = widget.textTheme?.title
                                    ?? appBarTheme.textTheme?.title
                                    ?? themeData.primaryTextTheme.title;
            TextStyle sideStyle = widget.textTheme?.body1
                                  ?? appBarTheme.textTheme?.body1
                                  ?? themeData.primaryTextTheme.body1;

            if (widget.toolbarOpacity != 1.0f) {
                float opacity =
                    new Interval(0.25f, 1.0f, curve: Curves.fastOutSlowIn).transform(widget.toolbarOpacity);
                if (centerStyle?.color != null) {
                    centerStyle = centerStyle.copyWith(color: centerStyle.color.withOpacity(opacity));
                }

                if (sideStyle?.color != null) {
                    sideStyle = sideStyle.copyWith(color: sideStyle.color.withOpacity(opacity));
                }

                overallIconTheme = overallIconTheme.copyWith(
                    opacity: opacity * (overallIconTheme.opacity ?? 1.0f)
                );
                actionsIconTheme = actionsIconTheme.copyWith(
                    opacity: opacity * (actionsIconTheme.opacity ?? 1.0f)
                );
            }

            Widget leading = widget.leading;
            if (leading == null && widget.automaticallyImplyLeading) {
                if (hasDrawer) {
                    leading = new IconButton(
                        icon: new Icon(Icons.menu),
                        onPressed: _handleDrawerButton,
                        tooltip: MaterialLocalizations.of(context).openAppDrawerTooltip);
                }
                else {
                    if (canPop) {
                        leading = useCloseButton ? (Widget) new CloseButton() : new BackButton();
                    }
                }
            }

            if (leading != null) {
                leading = new ConstrainedBox(
                    constraints: BoxConstraints.tightFor(width: AppBarUtils._kLeadingWidth),
                    child: leading);
            }

            Widget title = widget.title;
            if (title != null) {
                title = new DefaultTextStyle(
                    style: centerStyle,
                    softWrap: false,
                    overflow: TextOverflow.ellipsis,
                    child: title);
            }

            Widget actions = null;
            if (widget.actions != null && widget.actions.isNotEmpty()) {
                actions = new Row(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: widget.actions);
            }
            else if (hasEndDrawer) {
                actions = new IconButton(
                    icon: new Icon(Icons.menu),
                    onPressed: _handleDrawerButtonEnd,
                    tooltip: MaterialLocalizations.of(context).openAppDrawerTooltip);
            }

            if (actions != null) {
                actions = IconTheme.merge(
                    data: actionsIconTheme,
                    child: actions
                );
            }

            Widget toolbar = new NavigationToolbar(
                leading: leading,
                middle: title,
                trailing: actions,
                centerMiddle: widget._getEffectiveCenterTitle(themeData).Value,
                middleSpacing: widget.titleSpacing);

            Widget appBar = new ClipRect(
                child: new CustomSingleChildLayout(
                    layoutDelegate: new _ToolbarContainerLayout(),
                    child: IconTheme.merge(
                        data: overallIconTheme,
                        child: new DefaultTextStyle(
                            style: sideStyle,
                            child: toolbar)
                    )
                )
            );

            if (widget.bottom != null) {
                appBar = new Column(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new Flexible(
                            child: new ConstrainedBox(
                                constraints: new BoxConstraints(maxHeight: Constants.kToolbarHeight),
                                child: appBar
                            )
                        ),
                        widget.bottomOpacity == 1.0f
                            ? (Widget) widget.bottom
                            : new Opacity(
                                opacity: new Interval(0.25f, 1.0f, curve: Curves.fastOutSlowIn).transform(widget
                                    .bottomOpacity),
                                child: widget.bottom
                            )
                    }
                );
            }

            if (widget.primary) {
                appBar = new SafeArea(
                    top: true,
                    child: appBar);
            }

            appBar = new Align(
                alignment: Alignment.topCenter,
                child: appBar);

            if (widget.flexibleSpace != null) {
                appBar = new Stack(
                    fit: StackFit.passthrough,
                    children: new List<Widget> {
                        widget.flexibleSpace,
                        appBar
                    }
                );
            }

            Brightness brightness = widget.brightness
                                    ?? appBarTheme.brightness
                                    ?? themeData.primaryColorBrightness;
            SystemUiOverlayStyle overlayStyle = brightness == Brightness.dark
                ? SystemUiOverlayStyle.light
                : SystemUiOverlayStyle.dark;

            return new AnnotatedRegion<SystemUiOverlayStyle>(
                value: overlayStyle,
                child: new Material(
                    color: widget.backgroundColor
                           ?? appBarTheme.color
                           ?? themeData.primaryColor,
                    elevation: widget.elevation
                               ?? appBarTheme.elevation
                               ?? _defaultElevation,
                    shape: widget.shape,
                    child: appBar
                ));
        }
    }

    class _FloatingAppBar : StatefulWidget {
        public _FloatingAppBar(Key key = null, Widget child = null) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _FloatingAppBarState();
        }
    }

    class _FloatingAppBarState : State<_FloatingAppBar> {
        ScrollPosition _position;

        public _FloatingAppBarState() {
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (_position != null) {
                _position.isScrollingNotifier.removeListener(_isScrollingListener);
            }

            _position = Scrollable.of(context)?.position;
            if (_position != null) {
                _position.isScrollingNotifier.addListener(_isScrollingListener);
            }
        }

        public override void dispose() {
            if (_position != null) {
                _position.isScrollingNotifier.removeListener(_isScrollingListener);
            }

            base.dispose();
        }

        RenderSliverFloatingPersistentHeader _headerRenderer() {
            return (RenderSliverFloatingPersistentHeader) context.ancestorRenderObjectOfType(
                new TypeMatcher<RenderSliverFloatingPersistentHeader>());
        }

        void _isScrollingListener() {
            if (_position == null) {
                return;
            }

            RenderSliverFloatingPersistentHeader header = _headerRenderer();
            if (_position.isScrollingNotifier.value) {
                header?.maybeStopSnapAnimation(_position.userScrollDirection);
            }
            else {
                header?.maybeStartSnapAnimation(_position.userScrollDirection);
            }
        }

        public override Widget build(BuildContext context) {
            return widget.child;
        }
    }

    class _SliverAppBarDelegate : SliverPersistentHeaderDelegate {
        public _SliverAppBarDelegate(
            Widget leading,
            bool automaticallyImplyLeading,
            Widget title,
            List<Widget> actions,
            Widget flexibleSpace,
            PreferredSizeWidget bottom,
            float? elevation,
            bool forceElevated,
            Color backgroundColor,
            Brightness? brightness,
            IconThemeData iconTheme,
            IconThemeData actionsIconTheme,
            TextTheme textTheme,
            bool primary,
            bool? centerTitle,
            float titleSpacing,
            float? expandedHeight,
            float? collapsedHeight,
            float? topPadding,
            bool floating,
            bool pinned,
            FloatingHeaderSnapConfiguration snapConfiguration
        ) {
            D.assert(primary || topPadding == 0.0);
            this.leading = leading;
            this.automaticallyImplyLeading = automaticallyImplyLeading;
            this.title = title;
            this.actions = actions;
            this.flexibleSpace = flexibleSpace;
            this.bottom = bottom;
            this.elevation = elevation;
            this.forceElevated = forceElevated;
            this.backgroundColor = backgroundColor;
            this.brightness = brightness;
            this.iconTheme = iconTheme;
            this.actionsIconTheme = actionsIconTheme;
            this.textTheme = textTheme;
            this.primary = primary;
            this.centerTitle = centerTitle;
            this.titleSpacing = titleSpacing;
            this.expandedHeight = expandedHeight;
            this.collapsedHeight = collapsedHeight;
            this.topPadding = topPadding;
            this.floating = floating;
            this.pinned = pinned;
            _snapConfiguration = snapConfiguration;
            _bottomHeight = bottom?.preferredSize?.height ?? 0.0f;
        }

        public readonly Widget leading;
        public readonly bool automaticallyImplyLeading;
        public readonly Widget title;
        public readonly List<Widget> actions;
        public readonly Widget flexibleSpace;
        public readonly PreferredSizeWidget bottom;
        public readonly float? elevation;
        public readonly bool forceElevated;
        public readonly Color backgroundColor;
        public readonly Brightness? brightness;
        public readonly IconThemeData iconTheme;
        public readonly IconThemeData actionsIconTheme;
        public readonly TextTheme textTheme;
        public readonly bool primary;
        public readonly bool? centerTitle;
        public readonly float titleSpacing;
        public readonly float? expandedHeight;
        public readonly float? collapsedHeight;
        public readonly float? topPadding;
        public readonly bool floating;
        public readonly bool pinned;

        readonly float _bottomHeight;

        public override float? minExtent {
            get { return collapsedHeight ?? (topPadding + Constants.kToolbarHeight + _bottomHeight); }
        }

        public override float? maxExtent {
            get {
                return Mathf.Max(
                    (topPadding ?? 0.0f) + (expandedHeight ?? Constants.kToolbarHeight + _bottomHeight),
                    minExtent ?? 0.0f);
            }
        }

        public override FloatingHeaderSnapConfiguration snapConfiguration {
            get { return _snapConfiguration; }
        }

        FloatingHeaderSnapConfiguration _snapConfiguration;

        public override Widget build(BuildContext context, float shrinkOffset, bool overlapsContent) {
            float? visibleMainHeight = maxExtent - shrinkOffset - topPadding;
            float toolbarOpacity = !pinned || (!floating && bottom != null)
                ? ((visibleMainHeight - _bottomHeight) / Constants.kToolbarHeight)?.clamp(0.0f, 1.0f) ?? 1.0f
                : 1.0f;
            Widget appBar = FlexibleSpaceBar.createSettings(
                minExtent: minExtent,
                maxExtent: maxExtent,
                currentExtent: Mathf.Max(minExtent ?? 0.0f, maxExtent ?? 0.0f - shrinkOffset),
                toolbarOpacity: toolbarOpacity,
                child: new AppBar(
                    leading: leading,
                    automaticallyImplyLeading: automaticallyImplyLeading,
                    title: title,
                    actions: actions,
                    flexibleSpace: flexibleSpace,
                    bottom: bottom,
                    elevation: forceElevated || overlapsContent ||
                               (pinned && shrinkOffset > maxExtent - minExtent)
                        ? elevation ?? 4.0f
                        : 0.0f,
                    backgroundColor: backgroundColor,
                    brightness: brightness,
                    iconTheme: iconTheme,
                    textTheme: textTheme,
                    primary: primary,
                    centerTitle: centerTitle,
                    titleSpacing: titleSpacing,
                    toolbarOpacity: toolbarOpacity,
                    bottomOpacity: pinned
                        ? 1.0f
                        : (visibleMainHeight / _bottomHeight)?.clamp(0.0f, 1.0f) ?? 1.0f
                )
            );
            return floating ? new _FloatingAppBar(child: appBar) : appBar;
        }

        public override bool shouldRebuild(SliverPersistentHeaderDelegate _oldDelegate) {
            _SliverAppBarDelegate oldDelegate = _oldDelegate as _SliverAppBarDelegate;
            return leading != oldDelegate.leading
                   || automaticallyImplyLeading != oldDelegate.automaticallyImplyLeading
                   || title != oldDelegate.title
                   || actions != oldDelegate.actions
                   || flexibleSpace != oldDelegate.flexibleSpace
                   || bottom != oldDelegate.bottom
                   || _bottomHeight != oldDelegate._bottomHeight
                   || elevation != oldDelegate.elevation
                   || backgroundColor != oldDelegate.backgroundColor
                   || brightness != oldDelegate.brightness
                   || iconTheme != oldDelegate.iconTheme
                   || actionsIconTheme != oldDelegate.actionsIconTheme
                   || textTheme != oldDelegate.textTheme
                   || primary != oldDelegate.primary
                   || centerTitle != oldDelegate.centerTitle
                   || titleSpacing != oldDelegate.titleSpacing
                   || expandedHeight != oldDelegate.expandedHeight
                   || topPadding != oldDelegate.topPadding
                   || pinned != oldDelegate.pinned
                   || floating != oldDelegate.floating
                   || snapConfiguration != oldDelegate.snapConfiguration;
        }

        public override string ToString() {
            return
                $"{foundation_.describeIdentity(this)}(topPadding: {topPadding?.ToString("F1")}, bottomHeight: {_bottomHeight.ToString("F1")}, ...)";
        }
    }

    public class SliverAppBar : StatefulWidget {
        public SliverAppBar(
            Key key = null,
            Widget leading = null,
            bool automaticallyImplyLeading = true,
            Widget title = null,
            List<Widget> actions = null,
            Widget flexibleSpace = null,
            PreferredSizeWidget bottom = null,
            float? elevation = null,
            bool forceElevated = false,
            Color backgroundColor = null,
            Brightness? brightness = null,
            IconThemeData iconTheme = null,
            IconThemeData actionsIconTheme = null,
            TextTheme textTheme = null,
            bool primary = true,
            bool? centerTitle = null,
            float titleSpacing = NavigationToolbar.kMiddleSpacing,
            float? expandedHeight = null,
            bool floating = false,
            bool pinned = false,
            bool snap = false
        ) : base(key: key) {
            D.assert(floating || !snap, () => "The 'snap' argument only makes sense for floating app bars.");
            this.leading = leading;
            this.automaticallyImplyLeading = true;
            this.title = title;
            this.actions = actions;
            this.flexibleSpace = flexibleSpace;
            this.bottom = bottom;
            this.elevation = elevation;
            this.forceElevated = forceElevated;
            this.backgroundColor = backgroundColor;
            this.brightness = brightness;
            this.iconTheme = iconTheme;
            this.actionsIconTheme = actionsIconTheme;
            this.textTheme = textTheme;
            this.primary = primary;
            this.centerTitle = centerTitle;
            this.titleSpacing = NavigationToolbar.kMiddleSpacing;
            this.expandedHeight = expandedHeight;
            this.floating = floating;
            this.pinned = pinned;
            this.snap = snap;
        }


        public readonly Widget leading;

        public readonly bool automaticallyImplyLeading;

        public readonly Widget title;

        public readonly List<Widget> actions;

        public readonly Widget flexibleSpace;

        public readonly PreferredSizeWidget bottom;

        public readonly float? elevation;

        public readonly bool forceElevated;

        public readonly Color backgroundColor;

        public readonly Brightness? brightness;

        public readonly IconThemeData iconTheme;
        
        public readonly IconThemeData actionsIconTheme;

        public readonly TextTheme textTheme;

        public readonly bool primary;

        public readonly bool? centerTitle;

        public readonly float titleSpacing;

        public readonly float? expandedHeight;

        public readonly bool floating;

        public readonly bool pinned;

        public readonly bool snap;

        public override State createState() {
            return new _SliverAppBarState();
        }
    }

    class _SliverAppBarState : TickerProviderStateMixin<SliverAppBar> {
        FloatingHeaderSnapConfiguration _snapConfiguration;

        void _updateSnapConfiguration() {
            if (widget.snap && widget.floating) {
                _snapConfiguration = new FloatingHeaderSnapConfiguration(
                    vsync: this,
                    curve: Curves.easeOut,
                    duration: new TimeSpan(0, 0, 0, 0, 200)
                );
            }
            else {
                _snapConfiguration = null;
            }
        }

        public override void initState() {
            base.initState();
            _updateSnapConfiguration();
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            SliverAppBar oldWidget = _oldWidget as SliverAppBar;
            if (widget.snap != oldWidget.snap || widget.floating != oldWidget.floating) {
                _updateSnapConfiguration();
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(!widget.primary || WidgetsD.debugCheckHasMediaQuery(context));
            float? topPadding = widget.primary ? MediaQuery.of(context).padding.top : 0.0f;
            float? collapsedHeight = (widget.pinned && widget.floating && widget.bottom != null)
                ? widget.bottom.preferredSize.height + topPadding
                : null;

            return MediaQuery.removePadding(
                context: context,
                removeBottom: true,
                child: new SliverPersistentHeader(
                    floating: widget.floating,
                    pinned: widget.pinned,
                    del: new _SliverAppBarDelegate(
                        leading: widget.leading,
                        automaticallyImplyLeading: widget.automaticallyImplyLeading,
                        title: widget.title,
                        actions: widget.actions,
                        flexibleSpace: widget.flexibleSpace,
                        bottom: widget.bottom,
                        elevation: widget.elevation,
                        forceElevated: widget.forceElevated,
                        backgroundColor: widget.backgroundColor,
                        brightness: widget.brightness,
                        iconTheme: widget.iconTheme,
                        actionsIconTheme: widget.actionsIconTheme,
                        textTheme: widget.textTheme,
                        primary: widget.primary,
                        centerTitle: widget.centerTitle,
                        titleSpacing: widget.titleSpacing,
                        expandedHeight: widget.expandedHeight,
                        collapsedHeight: collapsedHeight,
                        topPadding: topPadding,
                        floating: widget.floating,
                        pinned: widget.pinned,
                        snapConfiguration: _snapConfiguration
                    )
                )
            );
        }
    }
}