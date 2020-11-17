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

namespace Unity.UIWidgets.cupertino {
    class NavBarUtils {
        public const float _kNavBarPersistentHeight = 44.0f;

        public const float _kNavBarLargeTitleHeightExtension = 52.0f;

        public const float _kNavBarShowLargeTitleThreshold = 10.0f;

        public const float _kNavBarEdgePadding = 16.0f;

        public const float _kNavBarBackButtonTapWidth = 50.0f;

        public static readonly TimeSpan _kNavBarTitleFadeDuration = new TimeSpan(0, 0, 0, 0, 150);

        public static readonly Color _kDefaultNavBarBorderColor = new Color(0x4C000000);

        public static readonly Border _kDefaultNavBarBorder = new Border(
            bottom: new BorderSide(
                color: _kDefaultNavBarBorderColor,
                width: 0.0f, // One physical pixel.
                style: BorderStyle.solid
            )
        );

        public static readonly _HeroTag _defaultHeroTag = new _HeroTag(null);

        public static Widget _wrapWithBackground(
            Border border = null,
            Color backgroundColor = null,
            Widget child = null,
            bool updateSystemUiOverlay = true
        ) {
            Widget result = child;
            if (updateSystemUiOverlay) {
                bool darkBackground = backgroundColor.computeLuminance() < 0.179f;
                SystemUiOverlayStyle overlayStyle = darkBackground
                    ? SystemUiOverlayStyle.light
                    : SystemUiOverlayStyle.dark;
                result = new AnnotatedRegion<SystemUiOverlayStyle>(
                    value: overlayStyle,
                    sized: true,
                    child: result
                );
            }

            DecoratedBox childWithBackground = new DecoratedBox(
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
                    filter: ImageFilter.blur(sigmaX: 10.0f, sigmaY: 10.0f),
                    child: childWithBackground
                )
            );
        }

        public static Widget _wrapActiveColor(Color color, BuildContext context, Widget child) {
            if (color == null) {
                return child;
            }

            return new CupertinoTheme(
                data: CupertinoTheme.of(context).copyWith(primaryColor: color),
                child: child
            );
        }

        public static bool _isTransitionable(BuildContext context) {
            ModalRoute route = ModalRoute.of(context);
            return route is PageRoute && !(route as PageRoute).fullscreenDialog;
        }

        public static HeroFlightShuttleBuilder _navBarHeroFlightShuttleBuilder = (
            BuildContext flightContext,
            Animation<float> animation,
            HeroFlightDirection flightDirection,
            BuildContext fromHeroContext,
            BuildContext toHeroContext
        ) => {
            D.assert(animation != null);

            D.assert(fromHeroContext != null);
            D.assert(toHeroContext != null);
            D.assert(fromHeroContext.widget is Hero);
            D.assert(toHeroContext.widget is Hero);
            Hero fromHeroWidget = (Hero) fromHeroContext.widget;
            Hero toHeroWidget = (Hero) toHeroContext.widget;
            D.assert(fromHeroWidget.child is _TransitionableNavigationBar);
            D.assert(toHeroWidget.child is _TransitionableNavigationBar);
            _TransitionableNavigationBar fromNavBar = (_TransitionableNavigationBar) fromHeroWidget.child;
            _TransitionableNavigationBar toNavBar = (_TransitionableNavigationBar) toHeroWidget.child;
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

            throw new UIWidgetsError($"Unknown flight direction: {flightDirection}");
        };

        public static CreateRectTween _linearTranslateWithLargestRectSizeTween = (Rect begin, Rect end) => {
            Size largestSize = new Size(
                Mathf.Max(begin.size.width, end.size.width),
                Mathf.Max(begin.size.height, end.size.height)
            );
            return new RectTween(
                begin: begin.topLeft & largestSize,
                end: end.topLeft & largestSize
            );
        };

        public static TransitionBuilder _navBarHeroLaunchPadBuilder = (
            BuildContext context,
            Widget child
        ) => {
            D.assert(child is _TransitionableNavigationBar);
            return new Visibility(
                maintainSize: true,
                maintainAnimation: true,
                maintainState: true,
                visible: false,
                child: child
            );
        };
    }


    class _HeroTag {
        public _HeroTag(
            NavigatorState navigator
        ) {
            this.navigator = navigator;
        }

        public readonly NavigatorState navigator;

        public override string ToString() {
            return $"Default Hero tag for Cupertino navigation bars with navigator {navigator}";
        }

        public bool Equals(_HeroTag other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return navigator == other.navigator;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
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
            return Equals(left, right);
        }

        public static bool operator !=(_HeroTag left, _HeroTag right) {
            return !Equals(left, right);
        }
    }

    public class CupertinoNavigationBar : StatefulWidget {
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
            EdgeInsets padding = null,
            Color actionsForegroundColor = null,
            bool transitionBetweenRoutes = true,
            object heroTag = null
        ) : base(key: key) {
            //D.assert(automaticallyImplyLeading != null);
            //D.assert(automaticallyImplyMiddle != null);
            //D.assert(transitionBetweenRoutes != null);

            this.leading = leading;
            this.automaticallyImplyLeading = automaticallyImplyLeading;
            this.automaticallyImplyMiddle = automaticallyImplyMiddle;
            this.previousPageTitle = previousPageTitle;
            this.middle = middle;
            this.trailing = trailing;
            this.border = border ?? NavBarUtils._kDefaultNavBarBorder;
            this.backgroundColor = backgroundColor;
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
                !transitionBetweenRoutes || ReferenceEquals(this.heroTag, NavBarUtils._defaultHeroTag),
                () => "Cannot specify a heroTag override if this navigation bar does not " +
                      "transition due to transitionBetweenRoutes = false."
            );

        }

        public readonly Widget leading;

        public readonly bool automaticallyImplyLeading;

        public readonly bool automaticallyImplyMiddle;

        public readonly string previousPageTitle;

        public readonly Widget middle;

        public readonly Widget trailing;


        public readonly Color backgroundColor;

        public readonly EdgeInsets padding;

        public readonly Border border;

        public readonly Color actionsForegroundColor;

        public readonly bool transitionBetweenRoutes;

        public readonly object heroTag;

        //public override bool? fullObstruction {
        public  bool? fullObstruction {
            get { return backgroundColor == null ? null : (bool?) (backgroundColor.alpha == 0xFF); }
        }

       //public override Size preferredSize {
        public Size preferredSize {
            get { return Size.fromHeight(NavBarUtils._kNavBarPersistentHeight); }
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
            Color backgroundColor = widget.backgroundColor ?? CupertinoTheme.of(context).barBackgroundColor;

            _NavigationBarStaticComponents components = new _NavigationBarStaticComponents(
                keys: keys,
                route: ModalRoute.of(context),
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

            Widget navBar = NavBarUtils._wrapWithBackground(
                border: widget.border,
                backgroundColor: backgroundColor,
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new _PersistentNavigationBar(
                        components: components,
                        padding: widget.padding
                    )
                )
            );

            if (!widget.transitionBetweenRoutes || !NavBarUtils._isTransitionable(context)) {
                return NavBarUtils._wrapActiveColor(widget.actionsForegroundColor, context,
                    navBar); // ignore: deprecated_member_use_from_same_package
            }

            return NavBarUtils._wrapActiveColor(
                widget.actionsForegroundColor, // ignore: deprecated_member_use_from_same_package
                context,
                new Builder(
                    builder: (BuildContext _context) => {
                        return new Hero(
                            tag: widget.heroTag as _HeroTag == NavBarUtils._defaultHeroTag
                                ? new _HeroTag(Navigator.of(_context))
                                : widget.heroTag,
                            createRectTween: NavBarUtils._linearTranslateWithLargestRectSizeTween,
                            placeholderBuilder: NavBarUtils._navBarHeroLaunchPadBuilder,
                            flightShuttleBuilder: NavBarUtils._navBarHeroFlightShuttleBuilder,
                            transitionOnUserGestures: true,
                            child: new _TransitionableNavigationBar(
                                componentsKeys: keys,
                                backgroundColor: backgroundColor,
                                backButtonTextStyle: CupertinoTheme.of(_context).textTheme.navActionTextStyle,
                                titleTextStyle: CupertinoTheme.of(_context).textTheme.navTitleTextStyle,
                                largeTitleTextStyle: null,
                                border: widget.border,
                                hasUserMiddle: widget.middle != null,
                                largeExpanded: false,
                                child: navBar
                            )
                        );
                    }
                )
            );
        }
    }

    public class CupertinoSliverNavigationBar : StatefulWidget {
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
            EdgeInsets padding = null,
            Color actionsForegroundColor = null,
            bool transitionBetweenRoutes = true,
            object heroTag = null
        ) : base(key: key) {
            D.assert(
                automaticallyImplyTitle == true || largeTitle != null,
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
            this.padding = padding;
            this.actionsForegroundColor = actionsForegroundColor;
            this.transitionBetweenRoutes = transitionBetweenRoutes;
            this.heroTag = heroTag ?? NavBarUtils._defaultHeroTag;
        }

        public readonly Widget largeTitle;

        public readonly Widget leading;

        public readonly bool automaticallyImplyLeading;

        public readonly bool automaticallyImplyTitle;

        public readonly string previousPageTitle;

        public readonly Widget middle;

        public readonly Widget trailing;

        public readonly Color backgroundColor;

        public readonly EdgeInsets padding;

        public readonly Border border;

        public readonly Color actionsForegroundColor;

        public readonly bool transitionBetweenRoutes;

        public readonly object heroTag;

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
            Color actionsForegroundColor =
                widget.actionsForegroundColor ??
                CupertinoTheme.of(context).primaryColor; // ignore: deprecated_member_use_from_same_package

            _NavigationBarStaticComponents components = new _NavigationBarStaticComponents(
                keys: keys,
                route: ModalRoute.of(context),
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
                widget.actionsForegroundColor, // ignore: deprecated_member_use_from_same_package
                context,
                new SliverPersistentHeader(
                    pinned: true, // iOS navigation bars are always pinned.
                    del: new _LargeTitleNavigationBarSliverDelegate(
                        keys: keys,
                        components: components,
                        userMiddle: widget.middle,
                        backgroundColor: widget.backgroundColor ?? CupertinoTheme.of(context).barBackgroundColor,
                        border: widget.border,
                        padding: widget.padding,
                        actionsForegroundColor: actionsForegroundColor,
                        transitionBetweenRoutes: widget.transitionBetweenRoutes,
                        heroTag: widget.heroTag,
                        persistentHeight: NavBarUtils._kNavBarPersistentHeight + MediaQuery.of(context).padding.top,
                        alwaysShowMiddle: widget.middle != null
                    )
                )
            );
        }
    }

    class _LargeTitleNavigationBarSliverDelegate
        : SliverPersistentHeaderDelegate {
        public _LargeTitleNavigationBarSliverDelegate(
            _NavigationBarStaticComponentsKeys keys,
            _NavigationBarStaticComponents components,
            Widget userMiddle,
            Color backgroundColor,
            Border border,
            EdgeInsets padding,
            Color actionsForegroundColor,
            bool transitionBetweenRoutes,
            object heroTag,
            float persistentHeight,
            bool alwaysShowMiddle
        ) {
            this.keys = keys;
            this.components = components;
            this.userMiddle = userMiddle;
            this.backgroundColor = backgroundColor;
            this.border = border;
            this.padding = padding;
            this.actionsForegroundColor = actionsForegroundColor;
            this.transitionBetweenRoutes = transitionBetweenRoutes;
            this.heroTag = heroTag;
            this.persistentHeight = persistentHeight;
            this.alwaysShowMiddle = alwaysShowMiddle;
        }

        public readonly _NavigationBarStaticComponentsKeys keys;
        public readonly _NavigationBarStaticComponents components;
        public readonly Widget userMiddle;
        public readonly Color backgroundColor;
        public readonly Border border;
        public readonly EdgeInsets padding;
        public readonly Color actionsForegroundColor;
        public readonly bool transitionBetweenRoutes;
        public readonly object heroTag;
        public readonly float persistentHeight;
        public readonly bool alwaysShowMiddle;

        public override float? minExtent {
            get { return persistentHeight; }
        }

        public override float? maxExtent {
            get { return persistentHeight + NavBarUtils._kNavBarLargeTitleHeightExtension; }
        }

        public override Widget build(BuildContext context, float shrinkOffset, bool overlapsContent) {
            bool showLargeTitle =
                shrinkOffset < maxExtent - minExtent - NavBarUtils._kNavBarShowLargeTitleThreshold;

            _PersistentNavigationBar persistentNavigationBar =
                new _PersistentNavigationBar(
                    components: components,
                    padding: padding,
                    middleVisible: alwaysShowMiddle ? null : (bool?) !showLargeTitle
                );

            Widget navBar = NavBarUtils._wrapWithBackground(
                border: border,
                backgroundColor: backgroundColor,
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
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
                                        alignment: Alignment.bottomLeft,
                                        child: new Padding(
                                            padding: EdgeInsets.only(
                                                left: NavBarUtils._kNavBarEdgePadding,
                                                bottom: 8.0f
                                            ),
                                            child: new SafeArea(
                                                top: false,
                                                bottom: false,
                                                child: new AnimatedOpacity(
                                                    opacity: showLargeTitle ? 1.0f : 0.0f,
                                                    duration: NavBarUtils._kNavBarTitleFadeDuration,
                                                    child: new DefaultTextStyle(
                                                        style: CupertinoTheme.of(context).textTheme
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

            if (!transitionBetweenRoutes || !NavBarUtils._isTransitionable(context)) {
                return navBar;
            }

            return new Hero(
                tag: heroTag as _HeroTag == NavBarUtils._defaultHeroTag
                    ? new _HeroTag(Navigator.of(context))
                    : heroTag,
                createRectTween: NavBarUtils._linearTranslateWithLargestRectSizeTween,
                flightShuttleBuilder: NavBarUtils._navBarHeroFlightShuttleBuilder,
                placeholderBuilder: NavBarUtils._navBarHeroLaunchPadBuilder,
                transitionOnUserGestures: true,
                child: new _TransitionableNavigationBar(
                    componentsKeys: keys,
                    backgroundColor: backgroundColor,
                    backButtonTextStyle: CupertinoTheme.of(context).textTheme.navActionTextStyle,
                    titleTextStyle: CupertinoTheme.of(context).textTheme.navTitleTextStyle,
                    largeTitleTextStyle: CupertinoTheme.of(context).textTheme.navLargeTitleTextStyle,
                    border: border,
                    hasUserMiddle: userMiddle != null,
                    largeExpanded: showLargeTitle,
                    child: navBar
                )
            );
        }

        public override bool shouldRebuild(SliverPersistentHeaderDelegate _oldDelegate) {
            _LargeTitleNavigationBarSliverDelegate oldDelegate = _oldDelegate as _LargeTitleNavigationBarSliverDelegate;
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
        public _PersistentNavigationBar(
            Key key = null,
            _NavigationBarStaticComponents components = null,
            EdgeInsets padding = null,
            bool? middleVisible = null
        ) : base(key: key) {
            this.components = components;
            this.padding = padding;
            this.middleVisible = middleVisible ?? true;
        }

        public readonly _NavigationBarStaticComponents components;

        public readonly EdgeInsets padding;
        public readonly bool middleVisible;

        public override Widget build(BuildContext context) {
            Widget middle = components.middle;

            if (middle != null) {
                middle = new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.navTitleTextStyle,
                    child: middle
                );
                middle = new AnimatedOpacity(
                    opacity: middleVisible ? 1.0f : 0.0f,
                    duration: NavBarUtils._kNavBarTitleFadeDuration,
                    child: middle
                );
            }

            Widget leading = components.leading;
            Widget backChevron = components.backChevron;
            Widget backLabel = components.backLabel;

            if (leading == null && backChevron != null && backLabel != null) {
                leading = CupertinoNavigationBarBackButton._assemble(
                    backChevron,
                    backLabel
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
                height: NavBarUtils._kNavBarPersistentHeight + MediaQuery.of(context).padding.top,
                child: new SafeArea(
                    bottom: false,
                    child: paddedToolbar
                )
            );
        }
    }

    class _NavigationBarStaticComponentsKeys {
        public _NavigationBarStaticComponentsKeys() {
            navBarBoxKey = GlobalKey.key(debugLabel: "Navigation bar render box");
            leadingKey = GlobalKey.key(debugLabel: "Leading");
            backChevronKey = GlobalKey.key(debugLabel: "Back chevron");
            backLabelKey = GlobalKey.key(debugLabel: "Back label");
            middleKey = GlobalKey.key(debugLabel: "Middle");
            trailingKey = GlobalKey.key(debugLabel: "Trailing");
            largeTitleKey = GlobalKey.key(debugLabel: "Large title");
        }

        public readonly GlobalKey navBarBoxKey;
        public readonly GlobalKey leadingKey;
        public readonly GlobalKey backChevronKey;
        public readonly GlobalKey backLabelKey;
        public readonly GlobalKey middleKey;
        public readonly GlobalKey trailingKey;
        public readonly GlobalKey largeTitleKey;
    }

    class _NavigationBarStaticComponents {
        public _NavigationBarStaticComponents(
            _NavigationBarStaticComponentsKeys keys,
            ModalRoute route,
            Widget userLeading,
            bool automaticallyImplyLeading,
            bool automaticallyImplyTitle,
            string previousPageTitle,
            Widget userMiddle,
            Widget userTrailing,
            Widget userLargeTitle,
            EdgeInsets padding,
            bool large
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
            ModalRoute currentRoute
        ) {
            if (automaticallyImplyTitle &&
                currentRoute is CupertinoPageRoute route &&
                route.title != null) {
                return new Text(route.title);
            }

            return null;
        }

        public readonly KeyedSubtree leading;

        static KeyedSubtree createLeading(
            GlobalKey leadingKey,
            Widget userLeading,
            ModalRoute route,
            bool automaticallyImplyLeading,
            EdgeInsets padding
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
                    onPressed: () => { route.navigator.maybePop(); }
                );
            }

            if (leadingContent == null) {
                return null;
            }

            return new KeyedSubtree(
                key: leadingKey,
                child: new Padding(
                    padding: EdgeInsets.only(
                        left: padding?.left ?? NavBarUtils._kNavBarEdgePadding
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

        public readonly KeyedSubtree backChevron;

        static KeyedSubtree createBackChevron(
            GlobalKey backChevronKey,
            Widget userLeading,
            ModalRoute route,
            bool automaticallyImplyLeading
        ) {
            if (
                userLeading != null ||
                !automaticallyImplyLeading ||
                route == null ||
                !route.canPop ||
                (route is PageRoute pageRoute && pageRoute.fullscreenDialog)
            ) {
                return null;
            }

            return new KeyedSubtree(key: backChevronKey, child: new _BackChevron());
        }

        public readonly KeyedSubtree backLabel;

        static KeyedSubtree createBackLabel(
            GlobalKey backLabelKey,
            Widget userLeading,
            ModalRoute route,
            bool automaticallyImplyLeading,
            string previousPageTitle
        ) {
            if (
                userLeading != null ||
                !automaticallyImplyLeading ||
                route == null ||
                !route.canPop ||
                (route is PageRoute pageRoute && pageRoute.fullscreenDialog)
            ) {
                return null;
            }

            return new KeyedSubtree(
                key: backLabelKey,
                child: new _BackLabel(
                    specifiedPreviousTitle: previousPageTitle,
                    route: route
                )
            );
        }

        public readonly KeyedSubtree middle;

        static KeyedSubtree createMiddle(
            GlobalKey middleKey,
            Widget userMiddle,
            Widget userLargeTitle,
            bool large,
            bool automaticallyImplyTitle,
            ModalRoute route
        ) {
            Widget middleContent = userMiddle;

            if (large) {
                middleContent = middleContent ?? userLargeTitle;
            }

            middleContent = middleContent ?? _derivedTitle(
                                automaticallyImplyTitle: automaticallyImplyTitle,
                                currentRoute: route
                            );

            if (middleContent == null) {
                return null;
            }

            return new KeyedSubtree(
                key: middleKey,
                child: middleContent
            );
        }

        public readonly KeyedSubtree trailing;

        static KeyedSubtree createTrailing(
            GlobalKey trailingKey,
            Widget userTrailing,
            EdgeInsets padding
        ) {
            if (userTrailing == null) {
                return null;
            }

            return new KeyedSubtree(
                key: trailingKey,
                child: new Padding(
                    padding: EdgeInsets.only(
                        right: padding?.right ?? NavBarUtils._kNavBarEdgePadding
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

        public readonly KeyedSubtree largeTitle;

        static KeyedSubtree createLargeTitle(
            GlobalKey largeTitleKey,
            Widget userLargeTitle,
            bool large,
            bool automaticImplyTitle,
            ModalRoute route
        ) {
            if (!large) {
                return null;
            }

            Widget largeTitleContent = userLargeTitle ?? _derivedTitle(
                                           automaticallyImplyTitle: automaticImplyTitle,
                                           currentRoute: route
                                       );

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
        public CupertinoNavigationBarBackButton(
            Color color,
            string previousPageTitle
        ) {
            _backChevron = null;
            _backLabel = null;
            this.color = color;
            this.previousPageTitle = previousPageTitle;
        }

        internal CupertinoNavigationBarBackButton(
            Color color,
            string previousPageTitle,
            Widget backChevron,
            Widget backLabel
        ) {
            _backChevron = backChevron;
            _backLabel = backLabel;
            this.color = color;
            this.previousPageTitle = previousPageTitle;
        }

        public static CupertinoNavigationBarBackButton _assemble(
            Widget _backChevron,
            Widget _backLabel
        ) {
            return new CupertinoNavigationBarBackButton(
                backChevron: _backChevron,
                backLabel: _backLabel,
                color: null,
                previousPageTitle: null
            );
        }

        public readonly Color color;

        public readonly string previousPageTitle;

        public readonly Widget _backChevron;

        public readonly Widget _backLabel;

        public override Widget build(BuildContext context) {
            ModalRoute currentRoute = ModalRoute.of(context);
            D.assert(
                currentRoute?.canPop == true,
                () => "CupertinoNavigationBarBackButton should only be used in routes that can be popped"
            );

            TextStyle actionTextStyle = CupertinoTheme.of(context).textTheme.navActionTextStyle;
            if (color != null) {
                actionTextStyle = actionTextStyle.copyWith(color: color);
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
                                new Padding(padding: EdgeInsets.only(left: 8.0f)),
                                _backChevron ?? new _BackChevron(),
                                new Padding(padding: EdgeInsets.only(left: 6.0f)),
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
                onPressed: () => { Navigator.maybePop(context); }
            );
        }
    }


    class _BackChevron : StatelessWidget {
        public _BackChevron(Key key = null) : base(key: key) { }

        public override Widget build(BuildContext context) {
            TextStyle textStyle = DefaultTextStyle.of(context).style;

            Widget iconWidget = Text.rich(
                new TextSpan(
                    text: char.ConvertFromUtf32(CupertinoIcons.back.codePoint),
                    style: new TextStyle(
                        inherit: false,
                        color: textStyle.color,
                        fontSize: 34.0f,
                        fontFamily: CupertinoIcons.back.fontFamily
                    )
                )
            );

            return iconWidget;
        }
    }

    class _BackLabel : StatelessWidget {
        public _BackLabel(
            Key key = null,
            string specifiedPreviousTitle = null,
            ModalRoute route = null
        ) : base(key: key) {
            D.assert(route != null);
            this.specifiedPreviousTitle = specifiedPreviousTitle;
            this.route = route;
        }

        public readonly string specifiedPreviousTitle;
        public readonly ModalRoute route;

        Widget _buildPreviousTitleWidget(BuildContext context, string previousTitle, Widget child) {
            if (previousTitle == null) {
                return new SizedBox(height: 0.0f, width: 0.0f);
            }

            Text textWidget = new Text(
                previousTitle,
                maxLines: 1,
                overflow: TextOverflow.ellipsis
            );

            if (previousTitle.Length > 12) {
                textWidget = new Text("Back");
            }

            return new Align(
                alignment: Alignment.centerLeft,
                widthFactor: 1.0f,
                child: textWidget
            );
        }

        public override Widget build(BuildContext context) {
            if (specifiedPreviousTitle != null) {
                return _buildPreviousTitleWidget(context, specifiedPreviousTitle, null);
            }
            else if (route is CupertinoPageRoute cupertinoRoute) {
                return new ValueListenableBuilder<string>(
                    valueListenable: cupertinoRoute.previousTitle,
                    builder: _buildPreviousTitleWidget
                );
            }
            else {
                return new SizedBox(height: 0.0f, width: 0.0f);
            }
        }
    }

    class _TransitionableNavigationBar : StatelessWidget {
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

        public readonly _NavigationBarStaticComponentsKeys componentsKeys;
        public readonly Color backgroundColor;
        public readonly TextStyle backButtonTextStyle;
        public readonly TextStyle titleTextStyle;
        public readonly TextStyle largeTitleTextStyle;
        public readonly Border border;
        public readonly bool? hasUserMiddle;
        public readonly bool? largeExpanded;
        public readonly Widget child;

        public RenderBox renderBox {
            get {
                RenderBox box = (RenderBox) componentsKeys.navBarBoxKey.currentContext.findRenderObject();
                D.assert(
                    box.attached,
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
                context.visitAncestorElements((Element ancestor) => {
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
        public _NavigationBarTransition(
            Animation<float> animation,
            _TransitionableNavigationBar topNavBar,
            _TransitionableNavigationBar bottomNavBar
        ) {
            this.animation = animation;
            this.topNavBar = topNavBar;
            this.bottomNavBar = bottomNavBar;
            heightTween = new FloatTween(
                begin: this.bottomNavBar.renderBox.size.height,
                end: this.topNavBar.renderBox.size.height
            );
            backgroundTween = new ColorTween(
                begin: this.bottomNavBar.backgroundColor,
                end: this.topNavBar.backgroundColor
            );
            borderTween = new BorderTween(
                begin: this.bottomNavBar.border,
                end: this.topNavBar.border
            );
        }

        public readonly Animation<float> animation;
        public readonly _TransitionableNavigationBar topNavBar;
        public readonly _TransitionableNavigationBar bottomNavBar;

        public readonly FloatTween heightTween;
        public readonly ColorTween backgroundTween;
        public readonly BorderTween borderTween;

        public override Widget build(BuildContext context) {
            _NavigationBarComponentsTransition componentsTransition = new _NavigationBarComponentsTransition(
                animation: animation,
                bottomNavBar: bottomNavBar,
                topNavBar: topNavBar,
                directionality: Directionality.of(context)
            );

            List<Widget> children = new List<Widget> {
                new AnimatedBuilder(
                    animation: animation,
                    builder: (BuildContext _context, Widget child) => {
                        return NavBarUtils._wrapWithBackground(
                            updateSystemUiOverlay: false,
                            backgroundColor: backgroundTween.evaluate(animation),
                            border: borderTween.evaluate(animation),
                            child: new SizedBox(
                                height: heightTween.evaluate(animation),
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

            children.RemoveAll((Widget child) => child == null);

            return new SizedBox(
                height: Mathf.Max(heightTween.begin, heightTween.end) + MediaQuery.of(context).padding.top,
                width: float.PositiveInfinity,
                child: new Stack(
                    children: children
                )
            );
        }
    }

    class _NavigationBarComponentsTransition {
        public _NavigationBarComponentsTransition(
            Animation<float> animation,
            _TransitionableNavigationBar bottomNavBar,
            _TransitionableNavigationBar topNavBar,
            TextDirection directionality
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
                bottomNavBar.renderBox.paintBounds.expandToInclude(topNavBar.renderBox.paintBounds);
            forwardDirection = directionality == TextDirection.ltr ? 1.0f : -1.0f;
        }

        public static Animatable<float> fadeOut = new FloatTween(
            begin: 1.0f,
            end: 0.0f
        );

        public static Animatable<float> fadeIn = new FloatTween(
            begin: 0.0f,
            end: 1.0f
        );

        public readonly Animation<float> animation;
        public readonly _NavigationBarStaticComponentsKeys bottomComponents;
        public readonly _NavigationBarStaticComponentsKeys topComponents;

        public readonly RenderBox bottomNavBarBox;
        public readonly RenderBox topNavBarBox;

        public readonly TextStyle bottomBackButtonTextStyle;
        public readonly TextStyle topBackButtonTextStyle;
        public readonly TextStyle bottomTitleTextStyle;
        public readonly TextStyle topTitleTextStyle;
        public readonly TextStyle bottomLargeTitleTextStyle;
        public readonly TextStyle topLargeTitleTextStyle;

        public readonly bool? bottomHasUserMiddle;
        public readonly bool? topHasUserMiddle;
        public readonly bool? bottomLargeExpanded;
        public readonly bool? topLargeExpanded;

        public readonly Rect transitionBox;

        public readonly float forwardDirection;

        public RelativeRect positionInTransitionBox(
            GlobalKey key,
            RenderBox from
        ) {
            RenderBox componentBox = (RenderBox) key.currentContext.findRenderObject();
            D.assert(componentBox.attached);

            return RelativeRect.fromRect(
                componentBox.localToGlobal(Offset.zero, ancestor: from) & componentBox.size, transitionBox
            );
        }

        public RelativeRectTween slideFromLeadingEdge(
            GlobalKey fromKey,
            RenderBox fromNavBarBox,
            GlobalKey toKey,
            RenderBox toNavBarBox
        ) {
            RelativeRect fromRect = positionInTransitionBox(fromKey, from: fromNavBarBox);

            RenderBox fromBox = (RenderBox) fromKey.currentContext.findRenderObject();
            RenderBox toBox = (RenderBox) toKey.currentContext.findRenderObject();

            Rect toRect =
                toBox.localToGlobal(
                    Offset.zero,
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
                end: RelativeRect.fromRect(toRect, transitionBox)
            );
        }

        public Animation<float> fadeInFrom(float t, Curve curve = null) {
            return animation.drive(fadeIn.chain(
                new CurveTween(curve: new Interval(t, 1.0f, curve: curve ?? Curves.easeIn))
            ));
        }

        public Animation<float> fadeOutBy(float t, Curve curve = null) {
            return animation.drive(fadeOut.chain(
                new CurveTween(curve: new Interval(0.0f, t, curve: curve ?? Curves.easeOut))
            ));
        }

        public Widget bottomLeading {
            get {
                KeyedSubtree bottomLeading = (KeyedSubtree) bottomComponents.leadingKey.currentWidget;

                if (bottomLeading == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(bottomComponents.leadingKey, from: bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.4f),
                        child: bottomLeading.child
                    )
                );
            }
        }

        public Widget bottomBackChevron {
            get {
                KeyedSubtree bottomBackChevron = (KeyedSubtree) bottomComponents.backChevronKey.currentWidget;

                if (bottomBackChevron == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(bottomComponents.backChevronKey,
                        from: bottomNavBarBox),
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
                KeyedSubtree bottomBackLabel = (KeyedSubtree) bottomComponents.backLabelKey.currentWidget;

                if (bottomBackLabel == null) {
                    return null;
                }

                RelativeRect from =
                    positionInTransitionBox(bottomComponents.backLabelKey, from: bottomNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: from,
                    end: from.shift(
                        new Offset(forwardDirection * (-bottomNavBarBox.size.width / 2.0f),
                            0.0f
                        )
                    )
                );

                return new PositionedTransition(
                    rect: animation.drive(positionTween),
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
                KeyedSubtree bottomMiddle = (KeyedSubtree) bottomComponents.middleKey.currentWidget;
                KeyedSubtree topBackLabel = (KeyedSubtree) topComponents.backLabelKey.currentWidget;
                KeyedSubtree topLeading = (KeyedSubtree) topComponents.leadingKey.currentWidget;

                if (bottomHasUserMiddle != true && bottomLargeExpanded == true) {
                    return null;
                }

                if (bottomMiddle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        rect: animation.drive(slideFromLeadingEdge(
                            fromKey: bottomComponents.middleKey,
                            fromNavBarBox: bottomNavBarBox,
                            toKey: topComponents.backLabelKey,
                            toNavBarBox: topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: fadeOutBy(bottomHasUserMiddle == true ? 0.4f : 0.7f),
                            child: new Align(
                                alignment: Alignment.centerLeft,
                                child: new DefaultTextStyleTransition(
                                    style: animation.drive(new TextStyleTween(
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
                        rect: positionInTransitionBox(bottomComponents.middleKey, from: bottomNavBarBox),
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
                KeyedSubtree bottomLargeTitle = (KeyedSubtree) bottomComponents.largeTitleKey.currentWidget;
                KeyedSubtree topBackLabel = (KeyedSubtree) topComponents.backLabelKey.currentWidget;
                KeyedSubtree topLeading = (KeyedSubtree) topComponents.leadingKey.currentWidget;

                if (bottomLargeTitle == null || bottomLargeExpanded != true) {
                    return null;
                }

                if (bottomLargeTitle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        rect: animation.drive(slideFromLeadingEdge(
                            fromKey: bottomComponents.largeTitleKey,
                            fromNavBarBox: bottomNavBarBox,
                            toKey: topComponents.backLabelKey,
                            toNavBarBox: topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: fadeOutBy(0.6f),
                            child: new Align(
                                alignment: Alignment.centerLeft,
                                child: new DefaultTextStyleTransition(
                                    style: animation.drive(new TextStyleTween(
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
                    RelativeRect from = positionInTransitionBox(bottomComponents.largeTitleKey,
                        from: bottomNavBarBox);

                    RelativeRectTween positionTween = new RelativeRectTween(
                        begin: from,
                        end: from.shift(
                            new Offset(forwardDirection * bottomNavBarBox.size.width / 4.0f,
                                0.0f
                            )
                        )
                    );

                    return new PositionedTransition(
                        rect: animation.drive(positionTween),
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
                KeyedSubtree bottomTrailing = (KeyedSubtree) bottomComponents.trailingKey.currentWidget;

                if (bottomTrailing == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(bottomComponents.trailingKey, from: bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.6f),
                        child: bottomTrailing.child
                    )
                );
            }
        }

        public Widget topLeading {
            get {
                KeyedSubtree topLeading = (KeyedSubtree) topComponents.leadingKey.currentWidget;

                if (topLeading == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(topComponents.leadingKey, from: topNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.6f),
                        child: topLeading.child
                    )
                );
            }
        }

        public Widget topBackChevron {
            get {
                KeyedSubtree topBackChevron = (KeyedSubtree) topComponents.backChevronKey.currentWidget;
                KeyedSubtree bottomBackChevron = (KeyedSubtree) bottomComponents.backChevronKey.currentWidget;

                if (topBackChevron == null) {
                    return null;
                }

                RelativeRect to =
                    positionInTransitionBox(topComponents.backChevronKey, from: topNavBarBox);
                RelativeRect from = to;

                if (bottomBackChevron == null) {
                    RenderBox topBackChevronBox =
                        (RenderBox) topComponents.backChevronKey.currentContext.findRenderObject();
                    from = to.shift(
                        new Offset(forwardDirection * topBackChevronBox.size.width * 2.0f,
                            0.0f
                        )
                    );
                }

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: from,
                    end: to
                );

                return new PositionedTransition(
                    rect: animation.drive(positionTween),
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
                KeyedSubtree bottomMiddle = (KeyedSubtree) bottomComponents.middleKey.currentWidget;
                KeyedSubtree bottomLargeTitle = (KeyedSubtree) bottomComponents.largeTitleKey.currentWidget;
                KeyedSubtree topBackLabel = (KeyedSubtree) topComponents.backLabelKey.currentWidget;

                if (topBackLabel == null) {
                    return null;
                }

                RenderAnimatedOpacity topBackLabelOpacity =
                    (RenderAnimatedOpacity) topComponents.backLabelKey.currentContext?.ancestorRenderObjectOfType(
                        new TypeMatcher<RenderAnimatedOpacity>()
                    );

                Animation<float> midClickOpacity = null;
                if (topBackLabelOpacity != null && topBackLabelOpacity.opacity.value < 1.0f) {
                    midClickOpacity = animation.drive(new FloatTween(
                        begin: 0.0f,
                        end: topBackLabelOpacity.opacity.value
                    ));
                }

                if (bottomLargeTitle != null &&
                    topBackLabel != null && bottomLargeExpanded.Value) {
                    return new PositionedTransition(
                        rect: animation.drive(slideFromLeadingEdge(
                            fromKey: bottomComponents.largeTitleKey,
                            fromNavBarBox: bottomNavBarBox,
                            toKey: topComponents.backLabelKey,
                            toNavBarBox: topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: midClickOpacity ?? fadeInFrom(0.4f),
                            child: new DefaultTextStyleTransition(
                                style: animation.drive(new TextStyleTween(
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
                        rect: animation.drive(slideFromLeadingEdge(
                            fromKey: bottomComponents.middleKey,
                            fromNavBarBox: bottomNavBarBox,
                            toKey: topComponents.backLabelKey,
                            toNavBarBox: topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: midClickOpacity ?? fadeInFrom(0.3f),
                            child: new DefaultTextStyleTransition(
                                style: animation.drive(new TextStyleTween(
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
                KeyedSubtree topMiddle = (KeyedSubtree) topComponents.middleKey.currentWidget;

                if (topMiddle == null) {
                    return null;
                }

                if (topHasUserMiddle != true && topLargeExpanded == true) {
                    return null;
                }

                RelativeRect to = positionInTransitionBox(topComponents.middleKey, from: topNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: to.shift(
                        new Offset(forwardDirection * topNavBarBox.size.width / 2.0f,
                            0.0f
                        )
                    ),
                    end: to
                );

                return new PositionedTransition(
                    rect: animation.drive(positionTween),
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
                KeyedSubtree topTrailing = (KeyedSubtree) topComponents.trailingKey.currentWidget;

                if (topTrailing == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(topComponents.trailingKey, from: topNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.4f),
                        child: topTrailing.child
                    )
                );
            }
        }

        public Widget topLargeTitle {
            get {
                KeyedSubtree topLargeTitle = (KeyedSubtree) topComponents.largeTitleKey.currentWidget;

                if (topLargeTitle == null || topLargeExpanded != true) {
                    return null;
                }

                RelativeRect to =
                    positionInTransitionBox(topComponents.largeTitleKey, from: topNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: to.shift(
                        new Offset(forwardDirection * topNavBarBox.size.width,
                            0.0f
                        )
                    ),
                    end: to
                );

                return new PositionedTransition(
                    rect: animation.drive(positionTween),
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
    }
}