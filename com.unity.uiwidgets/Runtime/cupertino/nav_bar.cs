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
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.cupertino {
    class NavBarUtils {
        public const float _kNavBarPersistentHeight = 44.0f;

        public const float _kNavBarLargeTitleHeightExtension = 52.0f;

        public const float _kNavBarShowLargeTitleThreshold = 10.0f;

        public const float _kNavBarEdgePadding = 16.0f;

        public const float _kNavBarBackButtonTapWidth = 50.0f;

        public static readonly TimeSpan _kNavBarTitleFadeDuration = TimeSpan.FromMilliseconds(150);

        public static readonly Color _kDefaultNavBarBorderColor = new Color(0x4C000000);

        public static readonly Border _kDefaultNavBarBorder = new Border(
            bottom: new BorderSide(
                color: _kDefaultNavBarBorderColor,
                0.0f, // One physical pixel.
                style: BorderStyle.solid
            )
        );

        public static readonly _HeroTag _defaultHeroTag = new _HeroTag(null);

        public static HeroFlightShuttleBuilder _navBarHeroFlightShuttleBuilder =
            (flightContext, animation, flightDirection, fromHeroContext, toHeroContext) => {
                D.assert(animation != null);
                D.assert(fromHeroContext != null);
                D.assert(toHeroContext != null);
                D.assert(fromHeroContext.widget is Hero);
                D.assert(toHeroContext.widget is Hero);

                var fromHeroWidget = (Hero) fromHeroContext.widget;
                var toHeroWidget = (Hero) toHeroContext.widget;

                D.assert(fromHeroWidget.child is _TransitionableNavigationBar);
                D.assert(toHeroWidget.child is _TransitionableNavigationBar);

                var fromNavBar = (_TransitionableNavigationBar) fromHeroWidget.child;
                var toNavBar = (_TransitionableNavigationBar) toHeroWidget.child;
                D.assert(fromNavBar.componentsKeys != null);
                D.assert(toNavBar.componentsKeys != null);

                D.assert(
                    fromNavBar.componentsKeys.navBarBoxKey.currentContext.owner != null,
                    () => "The from nav bar to Hero must have been mounted in the previous frame"
                );

                D.assert(
                    toNavBar.componentsKeys.navBarBoxKey.currentContext.owner != null,
                    () => "The to nav bar to Hero must have been mounted in the previous frame"
                );

                switch (flightDirection) {
                    case HeroFlightDirection.push:
                        return new _NavigationBarTransition(
                            animation: animation,
                            bottomNavBar: fromNavBar,
                            topNavBar: toNavBar
                        );
                    case HeroFlightDirection.pop:
                        return new _NavigationBarTransition(
                            animation: animation,
                            bottomNavBar: toNavBar,
                            topNavBar: fromNavBar
                        );
                }

                return null;
            };

        public static CreateRectTween _linearTranslateWithLargestRectSizeTween = (begin, end) => {
            var largestSize = new Size(
                Mathf.Max(a: begin.size.width, b: end.size.width),
                Mathf.Max(a: begin.size.height, b: end.size.height)
            );
            return new RectTween(
                begin.topLeft & largestSize,
                end.topLeft & largestSize
            );
        };

        public static HeroPlaceholderBuilder _navBarHeroLaunchPadBuilder = (context, heroSize, child) => {
            D.assert(child is _TransitionableNavigationBar);
            return new Visibility(
                maintainSize: true,
                maintainAnimation: true,
                maintainState: true,
                visible: false,
                child: child
            );
        };

        public static Widget _wrapWithBackground(
            Border border = null,
            Color backgroundColor = null,
            Brightness? brightness = null,
            Widget child = null,
            bool updateSystemUiOverlay = true
        ) {
            var result = child;
            if (updateSystemUiOverlay) {
                var isDark = backgroundColor.computeLuminance() < 0.179;
                var newBrightness = brightness ?? (isDark ? Brightness.dark : Brightness.light);
                SystemUiOverlayStyle overlayStyle;
                switch (newBrightness) {
                    case Brightness.dark:
                        overlayStyle = SystemUiOverlayStyle.light;
                        break;
                    case Brightness.light:
                    default:
                        overlayStyle = SystemUiOverlayStyle.dark;
                        break;
                }

                result = new AnnotatedRegion<SystemUiOverlayStyle>(
                    value: overlayStyle,
                    sized: true,
                    child: result
                );
            }

            var childWithBackground = new DecoratedBox(
                decoration: new BoxDecoration(
                    border: border,
                    color: backgroundColor
                ),
                child: result
            );
            if (backgroundColor.alpha == 0xFF) {
                return childWithBackground;
            }

            return new ClipRect(
                child: new BackdropFilter(
                    filter: ImageFilter.blur(10.0f, 10.0f),
                    child: childWithBackground
                )
            );
        }

        public static Widget _wrapActiveColor(Color color, BuildContext context, Widget child) {
            if (color == null) {
                return child;
            }

            return new CupertinoTheme(
                data: CupertinoTheme.of(context: context).copyWith(primaryColor: color),
                child: child
            );
        }

        public static bool _isTransitionable(BuildContext context) {
            var route = ModalRoute.of(context: context);
            return route is PageRoute && !(route as PageRoute).fullscreenDialog;
        }
    }


    class _HeroTag {
        public readonly NavigatorState navigator;

        public _HeroTag(
            NavigatorState navigator
        ) {
            this.navigator = navigator;
        }

        public override string ToString() {
            return $"Default Hero tag for Cupertino navigation bars with navigator {navigator}";
        }

        public bool Equals(_HeroTag other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return other is _HeroTag && navigator == other.navigator;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((_HeroTag) obj);
        }

        public override int GetHashCode() {
            return navigator.GetHashCode();
        }

        public static bool operator ==(_HeroTag left, _HeroTag right) {
            return Equals(objA: left, objB: right);
        }

        public static bool operator !=(_HeroTag left, _HeroTag right) {
            return !Equals(objA: left, objB: right);
        }
    }

    public class CupertinoNavigationBar : ObstructingPreferredSizeWidget {
        public readonly Color actionsForegroundColor;

        public readonly bool automaticallyImplyLeading;

        public readonly bool automaticallyImplyMiddle;
        public readonly Color backgroundColor;

        public readonly Border border;
        public readonly Brightness? brightness;

        public readonly object heroTag;

        public readonly Widget leading;

        public readonly Widget middle;

        public readonly EdgeInsetsDirectional padding;

        public readonly string previousPageTitle;

        public readonly Widget trailing;

        public readonly bool transitionBetweenRoutes;

        public CupertinoNavigationBar(
            Key key = null,
            Widget leading = null,
            bool automaticallyImplyLeading = true,
            bool automaticallyImplyMiddle = true,
            string previousPageTitle = null,
            Widget middle = null,
            Widget trailing = null,
            Border border = null,
            Color backgroundColor = null,
            Brightness? brightness = null,
            EdgeInsetsDirectional padding = null,
            Color actionsForegroundColor = null,
            bool transitionBetweenRoutes = true,
            object heroTag = null
        ) : base(key: key) {
            this.leading = leading;
            this.automaticallyImplyLeading = automaticallyImplyLeading;
            this.automaticallyImplyMiddle = automaticallyImplyMiddle;
            this.previousPageTitle = previousPageTitle;
            this.middle = middle;
            this.trailing = trailing;
            this.border = border ?? NavBarUtils._kDefaultNavBarBorder;
            this.backgroundColor = backgroundColor;
            this.brightness = brightness;
            this.padding = padding;
            this.actionsForegroundColor = actionsForegroundColor;
            this.transitionBetweenRoutes = transitionBetweenRoutes;
            this.heroTag = heroTag ?? NavBarUtils._defaultHeroTag;

            D.assert(
                this.heroTag != null,
                () => "heroTag cannot be null. Use transitionBetweenRoutes = false to " +
                      "disable Hero transition on this navigation bar."
            );

            D.assert(
                !transitionBetweenRoutes || ReferenceEquals(objA: this.heroTag, objB: NavBarUtils._defaultHeroTag),
                () => "Cannot specify a heroTag override if this navigation bar does not " +
                      "transition due to transitionBetweenRoutes = false."
            );
        }

        public override Size preferredSize {
            get { return Size.fromHeight(height: NavBarUtils._kNavBarPersistentHeight); }
        }

        public override bool shouldFullyObstruct(BuildContext context) {
            var backgroundColor = CupertinoDynamicColor.resolve(resolvable: this.backgroundColor, context: context)
                                  ?? CupertinoTheme.of(context: context).barBackgroundColor;
            return backgroundColor.alpha == 0xFF;
        }

        public override State createState() {
            return new _CupertinoNavigationBarState();
        }
    }

    class _CupertinoNavigationBarState : State<CupertinoNavigationBar> {
        _NavigationBarStaticComponentsKeys keys;

        public override void initState() {
            base.initState();
            keys = new _NavigationBarStaticComponentsKeys();
        }

        public override Widget build(BuildContext context) {
            var backgroundColor =
                CupertinoDynamicColor.resolve(resolvable: widget.backgroundColor, context: context) ??
                CupertinoTheme.of(context: context).barBackgroundColor;

            var components = new _NavigationBarStaticComponents(
                keys: keys,
                route: ModalRoute.of(context: context),
                userLeading: widget.leading,
                automaticallyImplyLeading: widget.automaticallyImplyLeading,
                automaticallyImplyTitle: widget.automaticallyImplyMiddle,
                previousPageTitle: widget.previousPageTitle,
                userMiddle: widget.middle,
                userTrailing: widget.trailing,
                padding: widget.padding,
                userLargeTitle: null,
                large: false
            );

            var navBar = NavBarUtils._wrapWithBackground(
                border: widget.border,
                backgroundColor: backgroundColor,
                brightness: widget.brightness,
                new DefaultTextStyle(
                    style: CupertinoTheme.of(context: context).textTheme.textStyle,
                    child: new _PersistentNavigationBar(
                        components: components,
                        padding: widget.padding
                    )
                )
            );
            var actionsForegroundColor = CupertinoDynamicColor.resolve(
                resolvable: widget.actionsForegroundColor, // ignore: deprecated_member_use_from_same_package
                context: context
            );

            if (!widget.transitionBetweenRoutes || !NavBarUtils._isTransitionable(context: context)) {
                return NavBarUtils._wrapActiveColor(color: actionsForegroundColor, context: context, child: navBar);
            }

            return NavBarUtils._wrapActiveColor(
                color: actionsForegroundColor,
                context: context,
                new Builder(
                    builder: _context => {
                        return new Hero(
                            tag: widget.heroTag as _HeroTag == NavBarUtils._defaultHeroTag
                                ? new _HeroTag(Navigator.of(context: _context))
                                : widget.heroTag,
                            createRectTween: NavBarUtils._linearTranslateWithLargestRectSizeTween,
                            placeholderBuilder: NavBarUtils._navBarHeroLaunchPadBuilder,
                            flightShuttleBuilder: NavBarUtils._navBarHeroFlightShuttleBuilder,
                            transitionOnUserGestures: true,
                            child: new _TransitionableNavigationBar(
                                componentsKeys: keys,
                                backgroundColor: backgroundColor,
                                backButtonTextStyle: CupertinoTheme.of(context: _context).textTheme.navActionTextStyle,
                                titleTextStyle: CupertinoTheme.of(context: _context).textTheme.navTitleTextStyle,
                                null,
                                border: widget.border,
                                widget.middle != null,
                                false,
                                child: navBar
                            )
                        );
                    }
                )
            );
        }
    }

    public class CupertinoSliverNavigationBar : StatefulWidget {
        public readonly Color actionsForegroundColor;

        public readonly bool automaticallyImplyLeading;

        public readonly bool automaticallyImplyTitle;

        public readonly Color backgroundColor;

        public readonly Border border;

        public readonly Brightness? brightness;

        public readonly object heroTag;

        public readonly Widget largeTitle;

        public readonly Widget leading;

        public readonly Widget middle;

        public readonly EdgeInsetsDirectional padding;

        public readonly string previousPageTitle;

        public readonly Widget trailing;

        public readonly bool transitionBetweenRoutes;

        public CupertinoSliverNavigationBar(
            Key key = null,
            Widget largeTitle = null,
            Widget leading = null,
            bool automaticallyImplyLeading = true,
            bool automaticallyImplyTitle = true,
            string previousPageTitle = null,
            Widget middle = null,
            Widget trailing = null,
            Border border = null,
            Color backgroundColor = null,
            Brightness? brightness = null,
            EdgeInsetsDirectional padding = null,
            Color actionsForegroundColor = null,
            bool transitionBetweenRoutes = true,
            object heroTag = null
        ) : base(key: key) {
            D.assert(
                automaticallyImplyTitle || largeTitle != null,
                () => "No largeTitle has been provided but automaticallyImplyTitle is also " +
                      "false. Either provide a largeTitle or set automaticallyImplyTitle to " +
                      "true."
            );
            this.largeTitle = largeTitle;
            this.leading = leading;
            this.automaticallyImplyLeading = automaticallyImplyLeading;
            this.automaticallyImplyTitle = automaticallyImplyTitle;
            this.previousPageTitle = previousPageTitle;
            this.middle = middle;
            this.trailing = trailing;
            this.border = border ?? NavBarUtils._kDefaultNavBarBorder;
            this.backgroundColor = backgroundColor;
            this.brightness = brightness;
            this.padding = padding;
            this.actionsForegroundColor = actionsForegroundColor;
            this.transitionBetweenRoutes = transitionBetweenRoutes;
            this.heroTag = heroTag ?? NavBarUtils._defaultHeroTag;
        }

        public bool opaque {
            get { return backgroundColor.alpha == 0xFF; }
        }

        public override State createState() {
            return new _CupertinoSliverNavigationBarState();
        }
    }

    class _CupertinoSliverNavigationBarState : State<CupertinoSliverNavigationBar> {
        _NavigationBarStaticComponentsKeys keys;

        public override void initState() {
            base.initState();
            keys = new _NavigationBarStaticComponentsKeys();
        }

        public override Widget build(BuildContext context) {
            var actionsForegroundColor =
                CupertinoDynamicColor.resolve(resolvable: widget.actionsForegroundColor,
                    context: context) // ignore: deprecated_member_use_from_same_package
                ?? CupertinoTheme.of(context: context).primaryColor;
            var components = new _NavigationBarStaticComponents(
                keys: keys,
                route: ModalRoute.of(context: context),
                userLeading: widget.leading,
                automaticallyImplyLeading: widget.automaticallyImplyLeading,
                automaticallyImplyTitle: widget.automaticallyImplyTitle,
                previousPageTitle: widget.previousPageTitle,
                userMiddle: widget.middle,
                userTrailing: widget.trailing,
                userLargeTitle: widget.largeTitle,
                padding: widget.padding,
                large: true
            );

            return NavBarUtils._wrapActiveColor(
                color: actionsForegroundColor,
                context: context,
                new MediaQuery(
                    data: MediaQuery.of(context: context).copyWith(textScaleFactor: 1),
                    child: new SliverPersistentHeader(
                        pinned: true, // iOS navigation bars are always pinned.
                        del: new _LargeTitleNavigationBarSliverDelegate(
                            keys: keys,
                            components: components,
                            userMiddle: widget.middle,
                            CupertinoDynamicColor.resolve(resolvable: widget.backgroundColor, context: context) ??
                            CupertinoTheme.of(context: context).barBackgroundColor,
                            brightness: widget.brightness,
                            border: widget.border,
                            padding: widget.padding,
                            actionsForegroundColor: actionsForegroundColor,
                            transitionBetweenRoutes: widget.transitionBetweenRoutes,
                            heroTag: widget.heroTag,
                            NavBarUtils._kNavBarPersistentHeight + MediaQuery.of(context: context).padding.top,
                            widget.middle != null
                        )
                    )
                )
            );
        }
    }

    class _LargeTitleNavigationBarSliverDelegate : SliverPersistentHeaderDelegate {
        public readonly Color actionsForegroundColor;
        public readonly bool alwaysShowMiddle;
        public readonly Color backgroundColor;
        public readonly Border border;
        public readonly Brightness? brightness;
        public readonly _NavigationBarStaticComponents components;
        public readonly object heroTag;

        public readonly _NavigationBarStaticComponentsKeys keys;
        public readonly EdgeInsetsDirectional padding;
        public readonly float persistentHeight;
        public readonly bool transitionBetweenRoutes;
        public readonly Widget userMiddle;

        public _LargeTitleNavigationBarSliverDelegate(
            _NavigationBarStaticComponentsKeys keys = null,
            _NavigationBarStaticComponents components = null,
            Widget userMiddle = null,
            Color backgroundColor = null,
            Brightness? brightness = null,
            Border border = null,
            EdgeInsetsDirectional padding = null,
            Color actionsForegroundColor = null,
            bool transitionBetweenRoutes = false,
            object heroTag = null,
            float persistentHeight = 0.0f,
            bool alwaysShowMiddle = false
        ) {
            this.keys = keys;
            this.components = components;
            this.userMiddle = userMiddle;
            this.backgroundColor = backgroundColor;
            this.border = border;
            this.brightness = brightness;
            this.padding = padding;
            this.actionsForegroundColor = actionsForegroundColor;
            this.transitionBetweenRoutes = transitionBetweenRoutes;
            this.heroTag = heroTag;
            this.persistentHeight = persistentHeight;
            this.alwaysShowMiddle = alwaysShowMiddle;
        }

        public override float? minExtent {
            get { return persistentHeight; }
        }

        public override float? maxExtent {
            get { return persistentHeight + NavBarUtils._kNavBarLargeTitleHeightExtension; }
        }

        public override Widget build(BuildContext context, float shrinkOffset, bool overlapsContent) {
            var showLargeTitle =
                shrinkOffset < maxExtent - minExtent - NavBarUtils._kNavBarShowLargeTitleThreshold;

            var persistentNavigationBar =
                new _PersistentNavigationBar(
                    components: components,
                    padding: padding,
                    middleVisible: alwaysShowMiddle ? null : (bool?) !showLargeTitle
                );

            var navBar = NavBarUtils._wrapWithBackground(
                border: border,
                CupertinoDynamicColor.resolve(resolvable: backgroundColor, context: context),
                brightness: brightness,
                new DefaultTextStyle(
                    style: CupertinoTheme.of(context: context).textTheme.textStyle,
                    child: new Stack(
                        fit: StackFit.expand,
                        children: new List<Widget> {
                            new Positioned(
                                top: persistentHeight,
                                left: 0.0f,
                                right: 0.0f,
                                bottom: 0.0f,
                                child: new ClipRect(
                                    child: new OverflowBox(
                                        minHeight: 0.0f,
                                        maxHeight: float.PositiveInfinity,
                                        alignment: AlignmentDirectional.bottomStart,
                                        child: new Padding(
                                            padding: EdgeInsetsDirectional.only(
                                                start: NavBarUtils._kNavBarEdgePadding,
                                                bottom: 8.0f
                                            ),
                                            child: new SafeArea(
                                                top: false,
                                                bottom: false,
                                                child: new AnimatedOpacity(
                                                    opacity: showLargeTitle ? 1.0f : 0.0f,
                                                    duration: NavBarUtils._kNavBarTitleFadeDuration,
                                                    child: new DefaultTextStyle(
                                                        style: CupertinoTheme.of(context: context).textTheme
                                                            .navLargeTitleTextStyle,
                                                        maxLines: 1,
                                                        overflow: TextOverflow.ellipsis,
                                                        child: components.largeTitle
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            ),
                            new Positioned(
                                left: 0.0f,
                                right: 0.0f,
                                top: 0.0f,
                                child: persistentNavigationBar
                            )
                        }
                    )
                )
            );

            if (!transitionBetweenRoutes || !NavBarUtils._isTransitionable(context: context)) {
                return navBar;
            }

            return new Hero(
                tag: heroTag as _HeroTag == NavBarUtils._defaultHeroTag
                    ? new _HeroTag(Navigator.of(context: context))
                    : heroTag,
                createRectTween: NavBarUtils._linearTranslateWithLargestRectSizeTween,
                flightShuttleBuilder: NavBarUtils._navBarHeroFlightShuttleBuilder,
                placeholderBuilder: NavBarUtils._navBarHeroLaunchPadBuilder,
                transitionOnUserGestures: true,
                child: new _TransitionableNavigationBar(
                    componentsKeys: keys,
                    CupertinoDynamicColor.resolve(resolvable: backgroundColor, context: context),
                    backButtonTextStyle: CupertinoTheme.of(context: context).textTheme.navActionTextStyle,
                    titleTextStyle: CupertinoTheme.of(context: context).textTheme.navTitleTextStyle,
                    largeTitleTextStyle: CupertinoTheme.of(context: context).textTheme.navLargeTitleTextStyle,
                    border: border,
                    userMiddle != null,
                    largeExpanded: showLargeTitle,
                    child: navBar
                )
            );
        }

        public override bool shouldRebuild(SliverPersistentHeaderDelegate _oldDelegate) {
            var oldDelegate = _oldDelegate as _LargeTitleNavigationBarSliverDelegate;
            return components != oldDelegate.components
                   || userMiddle != oldDelegate.userMiddle
                   || backgroundColor != oldDelegate.backgroundColor
                   || border != oldDelegate.border
                   || padding != oldDelegate.padding
                   || actionsForegroundColor != oldDelegate.actionsForegroundColor
                   || transitionBetweenRoutes != oldDelegate.transitionBetweenRoutes
                   || persistentHeight != oldDelegate.persistentHeight
                   || alwaysShowMiddle != oldDelegate.alwaysShowMiddle
                   || heroTag != oldDelegate.heroTag;
        }
    }

    class _PersistentNavigationBar : StatelessWidget {
        public readonly _NavigationBarStaticComponents components;
        public readonly bool? middleVisible;
        public readonly EdgeInsetsDirectional padding;

        public _PersistentNavigationBar(
            Key key = null,
            _NavigationBarStaticComponents components = null,
            EdgeInsetsDirectional padding = null,
            bool? middleVisible = null
        ) : base(key: key) {
            this.components = components;
            this.padding = padding;
            this.middleVisible = middleVisible;
        }

        public override Widget build(BuildContext context) {
            Widget middle = components.middle;

            if (middle != null) {
                middle = new DefaultTextStyle(
                    style: CupertinoTheme.of(context: context).textTheme.navTitleTextStyle,
                    child: middle
                );
                middle = middleVisible == null
                    ? middle
                    : new AnimatedOpacity(
                        opacity: middleVisible.Value ? 1.0f : 0.0f,
                        duration: NavBarUtils._kNavBarTitleFadeDuration,
                        child: middle
                    );
            }

            Widget leading = components.leading;
            Widget backChevron = components.backChevron;
            Widget backLabel = components.backLabel;

            if (leading == null && backChevron != null && backLabel != null) {
                leading = CupertinoNavigationBarBackButton._assemble(
                    _backChevron: backChevron,
                    _backLabel: backLabel
                );
            }

            Widget paddedToolbar = new NavigationToolbar(
                leading: leading,
                middle: middle,
                trailing: components.trailing,
                centerMiddle: true,
                middleSpacing: 6.0f
            );

            if (padding != null) {
                paddedToolbar = new Padding(
                    padding: EdgeInsets.only(
                        top: padding.top,
                        bottom: padding.bottom
                    ),
                    child: paddedToolbar
                );
            }

            return new SizedBox(
                height: NavBarUtils._kNavBarPersistentHeight + MediaQuery.of(context: context).padding.top,
                child: new SafeArea(
                    bottom: false,
                    child: paddedToolbar
                )
            );
        }
    }

    class _NavigationBarStaticComponentsKeys {
        public readonly GlobalKey backChevronKey;
        public readonly GlobalKey backLabelKey;
        public readonly GlobalKey largeTitleKey;
        public readonly GlobalKey leadingKey;
        public readonly GlobalKey middleKey;

        public readonly GlobalKey navBarBoxKey;
        public readonly GlobalKey trailingKey;

        public _NavigationBarStaticComponentsKeys() {
            navBarBoxKey = GlobalKey.key("Navigation bar render box");
            leadingKey = GlobalKey.key("Leading");
            backChevronKey = GlobalKey.key("Back chevron");
            backLabelKey = GlobalKey.key("Back label");
            middleKey = GlobalKey.key("Middle");
            trailingKey = GlobalKey.key("Trailing");
            largeTitleKey = GlobalKey.key("Large title");
        }
    }

    class _NavigationBarStaticComponents {
        public readonly KeyedSubtree backChevron;

        public readonly KeyedSubtree backLabel;

        public readonly KeyedSubtree largeTitle;

        public readonly KeyedSubtree leading;

        public readonly KeyedSubtree middle;

        public readonly KeyedSubtree trailing;

        public _NavigationBarStaticComponents(
            bool automaticallyImplyLeading,
            bool automaticallyImplyTitle,
            bool large,
            _NavigationBarStaticComponentsKeys keys = null,
            ModalRoute route = null,
            Widget userLeading = null,
            string previousPageTitle = null,
            Widget userMiddle = null,
            Widget userTrailing = null,
            Widget userLargeTitle = null,
            EdgeInsetsDirectional padding = null
        ) {
            leading = createLeading(
                leadingKey: keys.leadingKey,
                userLeading: userLeading,
                route: route,
                automaticallyImplyLeading: automaticallyImplyLeading,
                padding: padding
            );
            backChevron = createBackChevron(
                backChevronKey: keys.backChevronKey,
                userLeading: userLeading,
                route: route,
                automaticallyImplyLeading: automaticallyImplyLeading
            );
            backLabel = createBackLabel(
                backLabelKey: keys.backLabelKey,
                userLeading: userLeading,
                route: route,
                previousPageTitle: previousPageTitle,
                automaticallyImplyLeading: automaticallyImplyLeading
            );
            middle = createMiddle(
                middleKey: keys.middleKey,
                userMiddle: userMiddle,
                userLargeTitle: userLargeTitle,
                route: route,
                automaticallyImplyTitle: automaticallyImplyTitle,
                large: large
            );
            trailing = createTrailing(
                trailingKey: keys.trailingKey,
                userTrailing: userTrailing,
                padding: padding
            );
            largeTitle = createLargeTitle(
                largeTitleKey: keys.largeTitleKey,
                userLargeTitle: userLargeTitle,
                route: route,
                automaticImplyTitle: automaticallyImplyTitle,
                large: large
            );
        }

        static Widget _derivedTitle(
            bool automaticallyImplyTitle,
            ModalRoute currentRoute = null
        ) {
            if (automaticallyImplyTitle &&
                currentRoute is CupertinoPageRoute route &&
                route.title != null) {
                return new Text(data: route.title);
            }

            return null;
        }

        static KeyedSubtree createLeading(
            bool automaticallyImplyLeading,
            GlobalKey leadingKey = null,
            Widget userLeading = null,
            ModalRoute route = null,
            EdgeInsetsDirectional padding = null
        ) {
            Widget leadingContent = null;

            if (userLeading != null) {
                leadingContent = userLeading;
            }
            else if (
                automaticallyImplyLeading &&
                route is PageRoute pageRoute &&
                route.canPop &&
                pageRoute.fullscreenDialog
            ) {
                leadingContent = new CupertinoButton(
                    child: new Text("Close"),
                    padding: EdgeInsets.zero,
                    onPressed: () => { route.navigator.maybePop<object>(); }
                );
            }

            if (leadingContent == null) {
                return null;
            }

            return new KeyedSubtree(
                key: leadingKey,
                new Padding(
                    padding: EdgeInsetsDirectional.only(
                        padding?.start ?? NavBarUtils._kNavBarEdgePadding
                    ),
                    child: IconTheme.merge(
                        data: new IconThemeData(
                            size: 32.0f
                        ),
                        child: leadingContent
                    )
                )
            );
        }

        static KeyedSubtree createBackChevron(
            bool automaticallyImplyLeading,
            GlobalKey backChevronKey = null,
            Widget userLeading = null,
            ModalRoute route = null
        ) {
            if (
                userLeading != null ||
                !automaticallyImplyLeading ||
                route == null ||
                !route.canPop ||
                route is PageRoute pageRoute && pageRoute.fullscreenDialog
            ) {
                return null;
            }

            return new KeyedSubtree(key: backChevronKey, new _BackChevron());
        }

        static KeyedSubtree createBackLabel(
            bool automaticallyImplyLeading,
            GlobalKey backLabelKey = null,
            Widget userLeading = null,
            ModalRoute route = null,
            string previousPageTitle = null
        ) {
            if (
                userLeading != null ||
                !automaticallyImplyLeading ||
                route == null ||
                !route.canPop ||
                route is PageRoute pageRoute && pageRoute.fullscreenDialog
            ) {
                return null;
            }

            return new KeyedSubtree(
                key: backLabelKey,
                new _BackLabel(
                    specifiedPreviousTitle: previousPageTitle,
                    route: route
                )
            );
        }

        static KeyedSubtree createMiddle(
            bool large,
            bool automaticallyImplyTitle,
            GlobalKey middleKey = null,
            Widget userMiddle = null,
            Widget userLargeTitle = null,
            ModalRoute route = null
        ) {
            var middleContent = userMiddle;

            if (large) {
                middleContent = middleContent ?? userLargeTitle;
            }

            middleContent = middleContent ??
                            _derivedTitle(automaticallyImplyTitle: automaticallyImplyTitle, currentRoute: route);

            if (middleContent == null) {
                return null;
            }

            return new KeyedSubtree(
                key: middleKey,
                child: middleContent
            );
        }

        static KeyedSubtree createTrailing(
            GlobalKey trailingKey = null,
            Widget userTrailing = null,
            EdgeInsetsDirectional padding = null
        ) {
            if (userTrailing == null) {
                return null;
            }

            return new KeyedSubtree(
                key: trailingKey,
                new Padding(
                    padding: EdgeInsetsDirectional.only(
                        end: padding?.end ?? NavBarUtils._kNavBarEdgePadding
                    ),
                    child: IconTheme.merge(
                        data: new IconThemeData(
                            size: 32.0f
                        ),
                        child: userTrailing
                    )
                )
            );
        }

        static KeyedSubtree createLargeTitle(
            bool large,
            bool automaticImplyTitle,
            GlobalKey largeTitleKey = null,
            Widget userLargeTitle = null,
            ModalRoute route = null
        ) {
            if (!large) {
                return null;
            }

            var largeTitleContent = userLargeTitle ?? _derivedTitle(
                automaticallyImplyTitle: automaticImplyTitle,
                currentRoute: route);
            D.assert(
                largeTitleContent != null,
                () => "largeTitle was not provided and there was no title from the route."
            );
            return new KeyedSubtree(
                key: largeTitleKey,
                child: largeTitleContent
            );
        }
    }

    public class CupertinoNavigationBarBackButton : StatelessWidget {
        public readonly Widget _backChevron;

        public readonly Widget _backLabel;

        public readonly Color color;

        public readonly VoidCallback onPressed;

        public readonly string previousPageTitle;

        public CupertinoNavigationBarBackButton(
            Key key = null,
            Color color = null,
            string previousPageTitle = null,
            VoidCallback onPressed = null
        ) : base(key: key) {
            _backChevron = null;
            _backLabel = null;
            this.color = color;
            this.previousPageTitle = previousPageTitle;
            this.onPressed = onPressed;
        }

        internal CupertinoNavigationBarBackButton(
            Widget backChevron,
            Widget backLabel,
            Color color = null,
            string previousPageTitle = null,
            VoidCallback onPressed = null
        ) {
            _backChevron = backChevron;
            _backLabel = backLabel;
            this.color = color;
            this.previousPageTitle = previousPageTitle;
            this.onPressed = onPressed;
        }

        public static CupertinoNavigationBarBackButton _assemble(
            Widget _backChevron,
            Widget _backLabel
        ) {
            return new CupertinoNavigationBarBackButton(
                backChevron: _backChevron,
                backLabel: _backLabel
            );
        }

        public override Widget build(BuildContext context) {
            var currentRoute = ModalRoute.of(context: context);
            if (onPressed == null) {
                D.assert(
                    currentRoute?.canPop == true,
                    () => "CupertinoNavigationBarBackButton should only be used in routes that can be popped"
                );
            }

            var actionTextStyle = CupertinoTheme.of(context: context).textTheme.navActionTextStyle;
            if (color != null) {
                actionTextStyle =
                    actionTextStyle.copyWith(color: CupertinoDynamicColor.resolve(resolvable: color, context: context));
            }

            return new CupertinoButton(
                child: new DefaultTextStyle(
                    style: actionTextStyle,
                    child: new ConstrainedBox(
                        constraints: new BoxConstraints(minWidth: NavBarUtils._kNavBarBackButtonTapWidth),
                        child: new Row(
                            mainAxisSize: MainAxisSize.min,
                            mainAxisAlignment: MainAxisAlignment.start,
                            children: new List<Widget> {
                                new Padding(padding: EdgeInsetsDirectional.only(8.0f)),
                                _backChevron ?? new _BackChevron(),
                                new Padding(padding: EdgeInsetsDirectional.only(6.0f)),
                                new Flexible(
                                    child: _backLabel ?? new _BackLabel(
                                        specifiedPreviousTitle: previousPageTitle,
                                        route: currentRoute
                                    )
                                )
                            }
                        )
                    )
                ),
                padding: EdgeInsets.zero,
                onPressed: () => {
                    if (onPressed != null) {
                        onPressed();
                    }
                    else {
                        Navigator.maybePop<object>(context: context);
                    }
                }
            );
        }
    }


    class _BackChevron : StatelessWidget {
        public _BackChevron(Key key = null) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            var textDirection = Directionality.of(context: context);
            var textStyle = DefaultTextStyle.of(context: context).style;

            Widget iconWidget = Text.rich(
                new TextSpan(
                    new string(new[] {(char) CupertinoIcons.back.codePoint}),
                    new TextStyle(
                        false,
                        color: textStyle.color,
                        fontSize: 34.0f,
                        fontFamily: CupertinoIcons.back.fontFamily
                        //package: CupertinoIcons.back.fontPackage
                    )
                )
            );
            var matrix = Matrix4.identity();
            matrix.scale(-1.0f, 1.0f, 1.0f);
            switch (textDirection) {
                case TextDirection.rtl:
                    iconWidget = new Transform(
                        transform: matrix,
                        alignment: Alignment.center,
                        transformHitTests: false,
                        child: iconWidget
                    );
                    break;
                case TextDirection.ltr:
                    break;
            }

            return iconWidget;
        }
    }

    class _BackLabel : StatelessWidget {
        public readonly ModalRoute route;

        public readonly string specifiedPreviousTitle;

        public _BackLabel(
            Key key = null,
            string specifiedPreviousTitle = null,
            ModalRoute route = null
        ) : base(key: key) {
            this.specifiedPreviousTitle = specifiedPreviousTitle;
            this.route = route;
        }

        Widget _buildPreviousTitleWidget(BuildContext context, string previousTitle, Widget child) {
            if (previousTitle == null) {
                return new SizedBox(height: 0.0f, width: 0.0f);
            }

            var textWidget = new Text(
                data: previousTitle,
                maxLines: 1,
                overflow: TextOverflow.ellipsis
            );

            if (previousTitle.Length > 12) {
                textWidget = new Text("Back");
            }

            return new Align(
                alignment: AlignmentDirectional.centerStart,
                widthFactor: 1.0f,
                child: textWidget
            );
        }

        public override Widget build(BuildContext context) {
            if (specifiedPreviousTitle != null) {
                return _buildPreviousTitleWidget(context: context, previousTitle: specifiedPreviousTitle, null);
            }

            if (route is CupertinoPageRoute && !route.isFirst) {
                var cupertinoRoute = route as CupertinoPageRoute;
                return new ValueListenableBuilder<string>(
                    valueListenable: cupertinoRoute.previousTitle,
                    builder: _buildPreviousTitleWidget
                );
            }

            return new SizedBox(height: 0.0f, width: 0.0f);
        }
    }

    class _TransitionableNavigationBar : StatelessWidget {
        public readonly TextStyle backButtonTextStyle;
        public readonly Color backgroundColor;
        public readonly Border border;
        public readonly Widget child;

        public readonly _NavigationBarStaticComponentsKeys componentsKeys;
        public readonly bool? hasUserMiddle;
        public readonly bool? largeExpanded;
        public readonly TextStyle largeTitleTextStyle;
        public readonly TextStyle titleTextStyle;

        public _TransitionableNavigationBar(
            _NavigationBarStaticComponentsKeys componentsKeys = null,
            Color backgroundColor = null,
            TextStyle backButtonTextStyle = null,
            TextStyle titleTextStyle = null,
            TextStyle largeTitleTextStyle = null,
            Border border = null,
            bool? hasUserMiddle = null,
            bool? largeExpanded = null,
            Widget child = null
        ) : base(key: componentsKeys.navBarBoxKey) {
            D.assert(largeExpanded != null);
            D.assert(!largeExpanded.Value || largeTitleTextStyle != null);

            this.componentsKeys = componentsKeys;
            this.backgroundColor = backgroundColor;
            this.backButtonTextStyle = backButtonTextStyle;
            this.titleTextStyle = titleTextStyle;
            this.largeTitleTextStyle = largeTitleTextStyle;
            this.border = border;
            this.hasUserMiddle = hasUserMiddle;
            this.largeExpanded = largeExpanded;
            this.child = child;
        }

        public RenderBox renderBox {
            get {
                var box = componentsKeys.navBarBoxKey.currentContext.findRenderObject() as RenderBox;
                D.assert(
                    result: box.attached,
                    () => "_TransitionableNavigationBar.renderBox should be called when building " +
                          "hero flight shuttles when the from and the to nav bar boxes are already " +
                          "laid out and painted."
                );
                return box;
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(() => {
                bool? inHero = null;
                context.visitAncestorElements(ancestor => {
                    if (ancestor is ComponentElement) {
                        D.assert(
                            ancestor.widget.GetType() != typeof(_NavigationBarTransition),
                            () => "_TransitionableNavigationBar should never re-appear inside " +
                                  "_NavigationBarTransition. Keyed _TransitionableNavigationBar should " +
                                  "only serve as anchor points in routes rather than appearing inside " +
                                  "Hero flights themselves."
                        );
                        if (ancestor.widget.GetType() == typeof(Hero)) {
                            inHero = true;
                        }
                    }

                    inHero = inHero ?? false;
                    return true;
                });
                D.assert(
                    inHero == true,
                    () => "_TransitionableNavigationBar should only be added as the immediate " +
                          "child of Hero widgets."
                );
                return true;
            });
            return child;
        }
    }

    class _NavigationBarTransition : StatelessWidget {
        public readonly Animation<float> animation;
        public readonly ColorTween backgroundTween;
        public readonly BorderTween borderTween;
        public readonly _TransitionableNavigationBar bottomNavBar;

        public readonly FloatTween heightTween;
        public readonly _TransitionableNavigationBar topNavBar;

        public _NavigationBarTransition(
            Animation<float> animation = null,
            _TransitionableNavigationBar topNavBar = null,
            _TransitionableNavigationBar bottomNavBar = null
        ) {
            this.animation = animation;
            this.topNavBar = topNavBar;
            this.bottomNavBar = bottomNavBar;

            heightTween = new FloatTween(
                begin: bottomNavBar.renderBox.size.height,
                end: topNavBar.renderBox.size.height
            );
            backgroundTween = new ColorTween(
                begin: bottomNavBar.backgroundColor,
                end: topNavBar.backgroundColor
            );
            borderTween = new BorderTween(
                begin: bottomNavBar.border,
                end: topNavBar.border
            );
        }

        public override Widget build(BuildContext context) {
            var componentsTransition = new _NavigationBarComponentsTransition(
                animation: animation,
                bottomNavBar: bottomNavBar,
                topNavBar: topNavBar,
                Directionality.of(context: context)
            );

            var children = new List<Widget> {
                new AnimatedBuilder(
                    animation: animation,
                    (_context, child) => {
                        return NavBarUtils._wrapWithBackground(
                            updateSystemUiOverlay: false,
                            backgroundColor: backgroundTween.evaluate(animation: animation),
                            border: borderTween.evaluate(animation: animation),
                            child: new SizedBox(
                                height: heightTween.evaluate(animation: animation),
                                width: float.PositiveInfinity
                            )
                        );
                    }
                ),
                componentsTransition.bottomBackChevron,
                componentsTransition.bottomBackLabel,
                componentsTransition.bottomLeading,
                componentsTransition.bottomMiddle,
                componentsTransition.bottomLargeTitle,
                componentsTransition.bottomTrailing,
                componentsTransition.topLeading,
                componentsTransition.topBackChevron,
                componentsTransition.topBackLabel,
                componentsTransition.topMiddle,
                componentsTransition.topLargeTitle,
                componentsTransition.topTrailing
            };

            children.RemoveAll(child => child == null);

            return new SizedBox(
                height: Mathf.Max(a: heightTween.begin, b: heightTween.end) +
                        MediaQuery.of(context: context).padding.top,
                width: float.PositiveInfinity,
                child: new Stack(
                    children: children
                )
            );
        }
    }

    class _NavigationBarComponentsTransition {
        public static Animatable<float> fadeOut = new FloatTween(
            1.0f,
            0.0f
        );

        public static Animatable<float> fadeIn = new FloatTween(
            0.0f,
            1.0f
        );

        public readonly Animation<float> animation;

        public readonly TextStyle bottomBackButtonTextStyle;
        public readonly _NavigationBarStaticComponentsKeys bottomComponents;

        public readonly bool? bottomHasUserMiddle;
        public readonly bool? bottomLargeExpanded;
        public readonly TextStyle bottomLargeTitleTextStyle;

        public readonly RenderBox bottomNavBarBox;
        public readonly TextStyle bottomTitleTextStyle;

        public readonly float forwardDirection;
        public readonly TextStyle topBackButtonTextStyle;
        public readonly _NavigationBarStaticComponentsKeys topComponents;
        public readonly bool? topHasUserMiddle;
        public readonly bool? topLargeExpanded;
        public readonly TextStyle topLargeTitleTextStyle;
        public readonly RenderBox topNavBarBox;
        public readonly TextStyle topTitleTextStyle;

        public readonly Rect transitionBox;

        public _NavigationBarComponentsTransition(
            Animation<float> animation = null,
            _TransitionableNavigationBar bottomNavBar = null,
            _TransitionableNavigationBar topNavBar = null,
            TextDirection? directionality = null
        ) {
            this.animation = animation;
            bottomComponents = bottomNavBar.componentsKeys;
            topComponents = topNavBar.componentsKeys;
            bottomNavBarBox = bottomNavBar.renderBox;
            topNavBarBox = topNavBar.renderBox;
            bottomBackButtonTextStyle = bottomNavBar.backButtonTextStyle;
            topBackButtonTextStyle = topNavBar.backButtonTextStyle;
            bottomTitleTextStyle = bottomNavBar.titleTextStyle;
            topTitleTextStyle = topNavBar.titleTextStyle;
            bottomLargeTitleTextStyle = bottomNavBar.largeTitleTextStyle;
            topLargeTitleTextStyle = topNavBar.largeTitleTextStyle;
            bottomHasUserMiddle = bottomNavBar.hasUserMiddle;
            topHasUserMiddle = topNavBar.hasUserMiddle;
            bottomLargeExpanded = bottomNavBar.largeExpanded;
            topLargeExpanded = topNavBar.largeExpanded;
            transitionBox =
                bottomNavBar.renderBox.paintBounds.expandToInclude(other: topNavBar.renderBox.paintBounds);
            forwardDirection = directionality == TextDirection.ltr ? 1.0f : -1.0f;
        }

        public Widget bottomLeading {
            get {
                var bottomLeading = bottomComponents.leadingKey.currentWidget as KeyedSubtree;
                if (bottomLeading == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(key: bottomComponents.leadingKey, from: bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.4f),
                        child: bottomLeading.child
                    )
                );
            }
        }

        public Widget bottomBackChevron {
            get {
                var bottomBackChevron = bottomComponents.backChevronKey.currentWidget as KeyedSubtree;
                if (bottomBackChevron == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(key: bottomComponents.backChevronKey, from: bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.6f),
                        child: new DefaultTextStyle(
                            style: bottomBackButtonTextStyle,
                            child: bottomBackChevron.child
                        )
                    )
                );
            }
        }

        public Widget bottomBackLabel {
            get {
                var bottomBackLabel = bottomComponents.backLabelKey.currentWidget as KeyedSubtree;

                if (bottomBackLabel == null) {
                    return null;
                }

                var from =
                    positionInTransitionBox(key: bottomComponents.backLabelKey, from: bottomNavBarBox);

                var positionTween = new RelativeRectTween(
                    begin: from,
                    from.shift(
                        new Offset(forwardDirection * (-bottomNavBarBox.size.width / 2.0f),
                            0.0f
                        )
                    )
                );

                return new PositionedTransition(
                    animation.drive(child: positionTween),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.2f),
                        child: new DefaultTextStyle(
                            style: bottomBackButtonTextStyle,
                            child: bottomBackLabel.child
                        )
                    )
                );
            }
        }

        public Widget bottomMiddle {
            get {
                var bottomMiddle = bottomComponents.middleKey.currentWidget as KeyedSubtree;
                var topBackLabel = topComponents.backLabelKey.currentWidget as KeyedSubtree;
                var topLeading = topComponents.leadingKey.currentWidget as KeyedSubtree;

                if (bottomHasUserMiddle != true && bottomLargeExpanded == true) {
                    return null;
                }

                if (bottomMiddle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        animation.drive(slideFromLeadingEdge(
                            fromKey: bottomComponents.middleKey,
                            fromNavBarBox: bottomNavBarBox,
                            toKey: topComponents.backLabelKey,
                            toNavBarBox: topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: fadeOutBy(bottomHasUserMiddle == true ? 0.4f : 0.7f),
                            child: new Align(
                                alignment: AlignmentDirectional.centerStart,
                                child: new DefaultTextStyleTransition(
                                    animation.drive(new TextStyleTween(
                                        begin: bottomTitleTextStyle,
                                        end: topBackButtonTextStyle
                                    )),
                                    child: bottomMiddle.child
                                )
                            )
                        )
                    );
                }

                if (bottomMiddle != null && topLeading != null) {
                    return Positioned.fromRelativeRect(
                        rect: positionInTransitionBox(key: bottomComponents.middleKey, from: bottomNavBarBox),
                        child: new FadeTransition(
                            opacity: fadeOutBy(bottomHasUserMiddle == true ? 0.4f : 0.7f),
                            child: new DefaultTextStyle(
                                style: bottomTitleTextStyle,
                                child: bottomMiddle.child
                            )
                        )
                    );
                }

                return null;
            }
        }

        public Widget bottomLargeTitle {
            get {
                var bottomLargeTitle = (KeyedSubtree) bottomComponents.largeTitleKey.currentWidget;
                var topBackLabel = (KeyedSubtree) topComponents.backLabelKey.currentWidget;
                var topLeading = (KeyedSubtree) topComponents.leadingKey.currentWidget;

                if (bottomLargeTitle == null || bottomLargeExpanded != true) {
                    return null;
                }

                if (bottomLargeTitle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        animation.drive(slideFromLeadingEdge(
                            fromKey: bottomComponents.largeTitleKey,
                            fromNavBarBox: bottomNavBarBox,
                            toKey: topComponents.backLabelKey,
                            toNavBarBox: topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: fadeOutBy(0.6f),
                            child: new Align(
                                alignment: AlignmentDirectional.centerStart,
                                child: new DefaultTextStyleTransition(
                                    animation.drive(new TextStyleTween(
                                        begin: bottomLargeTitleTextStyle,
                                        end: topBackButtonTextStyle
                                    )),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                    child: bottomLargeTitle.child
                                )
                            )
                        )
                    );
                }

                if (bottomLargeTitle != null && topLeading != null) {
                    var from = positionInTransitionBox(key: bottomComponents.largeTitleKey, from: bottomNavBarBox);

                    var positionTween = new RelativeRectTween(
                        begin: from,
                        from.shift(
                            new Offset(forwardDirection * bottomNavBarBox.size.width / 4.0f,
                                0.0f
                            )
                        )
                    );

                    return new PositionedTransition(
                        animation.drive(child: positionTween),
                        child: new FadeTransition(
                            opacity: fadeOutBy(0.4f),
                            child: new DefaultTextStyle(
                                style: bottomLargeTitleTextStyle,
                                child: bottomLargeTitle.child
                            )
                        )
                    );
                }

                return null;
            }
        }

        public Widget bottomTrailing {
            get {
                var bottomTrailing = (KeyedSubtree) bottomComponents.trailingKey.currentWidget;

                if (bottomTrailing == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(key: bottomComponents.trailingKey, from: bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.6f),
                        child: bottomTrailing.child
                    )
                );
            }
        }

        public Widget topLeading {
            get {
                var topLeading = (KeyedSubtree) topComponents.leadingKey.currentWidget;

                if (topLeading == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(key: topComponents.leadingKey, from: topNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.6f),
                        child: topLeading.child
                    )
                );
            }
        }

        public Widget topBackChevron {
            get {
                var topBackChevron = (KeyedSubtree) topComponents.backChevronKey.currentWidget;
                var bottomBackChevron = (KeyedSubtree) bottomComponents.backChevronKey.currentWidget;

                if (topBackChevron == null) {
                    return null;
                }

                var to = positionInTransitionBox(key: topComponents.backChevronKey, from: topNavBarBox);
                var from = to;

                if (bottomBackChevron == null) {
                    var topBackChevronBox =
                        (RenderBox) topComponents.backChevronKey.currentContext.findRenderObject();
                    from = to.shift(
                        new Offset(forwardDirection * topBackChevronBox.size.width * 2.0f,
                            0.0f
                        )
                    );
                }

                var positionTween = new RelativeRectTween(
                    begin: from,
                    end: to
                );

                return new PositionedTransition(
                    animation.drive(child: positionTween),
                    child: new FadeTransition(
                        opacity: fadeInFrom(bottomBackChevron == null ? 0.7f : 0.4f),
                        child: new DefaultTextStyle(
                            style: topBackButtonTextStyle,
                            child: topBackChevron.child
                        )
                    )
                );
            }
        }

        public Widget topBackLabel {
            get {
                var bottomMiddle = (KeyedSubtree) bottomComponents.middleKey.currentWidget;
                var bottomLargeTitle = (KeyedSubtree) bottomComponents.largeTitleKey.currentWidget;
                var topBackLabel = (KeyedSubtree) topComponents.backLabelKey.currentWidget;

                if (topBackLabel == null) {
                    return null;
                }

                var topBackLabelOpacity =
                    topComponents.backLabelKey.currentContext?.findAncestorRenderObjectOfType<RenderAnimatedOpacity>();

                Animation<float> midClickOpacity = null;
                if (topBackLabelOpacity != null && topBackLabelOpacity.opacity.value < 1.0f) {
                    midClickOpacity = animation.drive(new FloatTween(
                        0.0f,
                        end: topBackLabelOpacity.opacity.value
                    ));
                }

                if (bottomLargeTitle != null &&
                    topBackLabel != null && bottomLargeExpanded.Value) {
                    return new PositionedTransition(
                        animation.drive(slideFromLeadingEdge(
                            fromKey: bottomComponents.largeTitleKey,
                            fromNavBarBox: bottomNavBarBox,
                            toKey: topComponents.backLabelKey,
                            toNavBarBox: topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: midClickOpacity ?? fadeInFrom(0.4f),
                            child: new DefaultTextStyleTransition(
                                animation.drive(new TextStyleTween(
                                    begin: bottomLargeTitleTextStyle,
                                    end: topBackButtonTextStyle
                                )),
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                child: topBackLabel.child
                            )
                        )
                    );
                }

                if (bottomMiddle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        animation.drive(slideFromLeadingEdge(
                            fromKey: bottomComponents.middleKey,
                            fromNavBarBox: bottomNavBarBox,
                            toKey: topComponents.backLabelKey,
                            toNavBarBox: topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: midClickOpacity ?? fadeInFrom(0.3f),
                            child: new DefaultTextStyleTransition(
                                animation.drive(new TextStyleTween(
                                    begin: bottomTitleTextStyle,
                                    end: topBackButtonTextStyle
                                )),
                                child: topBackLabel.child
                            )
                        )
                    );
                }

                return null;
            }
        }

        public Widget topMiddle {
            get {
                var topMiddle = (KeyedSubtree) topComponents.middleKey.currentWidget;

                if (topMiddle == null) {
                    return null;
                }

                if (topHasUserMiddle != true && topLargeExpanded == true) {
                    return null;
                }

                var to = positionInTransitionBox(key: topComponents.middleKey, from: topNavBarBox);

                var positionTween = new RelativeRectTween(
                    to.shift(
                        new Offset(forwardDirection * topNavBarBox.size.width / 2.0f,
                            0.0f
                        )
                    ),
                    end: to
                );

                return new PositionedTransition(
                    animation.drive(child: positionTween),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.25f),
                        child: new DefaultTextStyle(
                            style: topTitleTextStyle,
                            child: topMiddle.child
                        )
                    )
                );
            }
        }

        public Widget topTrailing {
            get {
                var topTrailing = (KeyedSubtree) topComponents.trailingKey.currentWidget;

                if (topTrailing == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(key: topComponents.trailingKey, from: topNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.4f),
                        child: topTrailing.child
                    )
                );
            }
        }

        public Widget topLargeTitle {
            get {
                var topLargeTitle = (KeyedSubtree) topComponents.largeTitleKey.currentWidget;

                if (topLargeTitle == null || topLargeExpanded != true) {
                    return null;
                }

                var to = positionInTransitionBox(key: topComponents.largeTitleKey, from: topNavBarBox);

                var positionTween = new RelativeRectTween(
                    to.shift(
                        new Offset(forwardDirection * topNavBarBox.size.width,
                            0.0f
                        )
                    ),
                    end: to
                );

                return new PositionedTransition(
                    animation.drive(child: positionTween),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.3f),
                        child: new DefaultTextStyle(
                            style: topLargeTitleTextStyle,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            child: topLargeTitle.child
                        )
                    )
                );
            }
        }

        public RelativeRect positionInTransitionBox(
            GlobalKey key = null,
            RenderBox from = null
        ) {
            var componentBox = key.currentContext.findRenderObject() as RenderBox;
            D.assert(result: componentBox.attached);
            return RelativeRect.fromRect(
                componentBox.localToGlobal(point: Offset.zero, ancestor: from) & componentBox.size,
                container: transitionBox
            );
        }

        public RelativeRectTween slideFromLeadingEdge(
            GlobalKey fromKey = null,
            RenderBox fromNavBarBox = null,
            GlobalKey toKey = null,
            RenderBox toNavBarBox = null
        ) {
            var fromRect = positionInTransitionBox(key: fromKey, from: fromNavBarBox);
            var fromBox = fromKey.currentContext.findRenderObject() as RenderBox;
            var toBox = toKey.currentContext.findRenderObject() as RenderBox;
            var toRect =
                toBox.localToGlobal(
                    point: Offset.zero,
                    ancestor: toNavBarBox
                ).translate(
                    0.0f,
                    -fromBox.size.height / 2 + toBox.size.height / 2
                ) & fromBox.size; // Keep the from render object"s size.

            if (forwardDirection < 0) {
                toRect = toRect.translate(-fromBox.size.width + toBox.size.width, 0.0f);
            }

            return new RelativeRectTween(
                begin: fromRect,
                RelativeRect.fromRect(rect: toRect, container: transitionBox)
            );
        }

        public Animation<float> fadeInFrom(float t, Curve curve = null) {
            return animation.drive(fadeIn.chain(
                new CurveTween(new Interval(begin: t, 1.0f, curve ?? Curves.easeIn))
            ));
        }

        public Animation<float> fadeOutBy(float t, Curve curve = null) {
            return animation.drive(fadeOut.chain(
                new CurveTween(new Interval(0.0f, end: t, curve ?? Curves.easeOut))
            ));
        }
    }
}