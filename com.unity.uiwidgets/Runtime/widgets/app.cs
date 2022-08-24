using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public delegate Locale LocaleListResolutionCallback(List<Locale> locales, List<Locale> supportedLocales);

    public delegate Locale LocaleResolutionCallback(Locale locale, List<Locale> supportedLocales);

    public delegate string GenerateAppTitle(BuildContext context);

    public delegate PageRoute PageRouteFactory(RouteSettings settings, WidgetBuilder builder);

    public delegate List<Route> InitialRouteListFactory(string initialRoute);

    public class WidgetsApp : StatefulWidget {
        public static bool showPerformanceOverlayOverride = false;
        public static bool debugShowWidgetInspectorOverride = false;
        public static bool debugAllowBannerOverride = true;

        public static readonly Dictionary<LogicalKeySet, Intent> _defaultShortcuts =
            new Dictionary<LogicalKeySet, Intent> {
                // Activation
                {new LogicalKeySet(key1: LogicalKeyboardKey.enter), new Intent(key: ActivateAction.key)},
                {new LogicalKeySet(key1: LogicalKeyboardKey.space), new Intent(key: ActivateAction.key)},
                {new LogicalKeySet(key1: LogicalKeyboardKey.gameButtonA), new Intent(key: ActivateAction.key)},

                // Keyboard traversal.
                {new LogicalKeySet(key1: LogicalKeyboardKey.tab), new Intent(key: NextFocusAction.key)}, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.shift, key2: LogicalKeyboardKey.tab),
                    new Intent(key: PreviousFocusAction.key)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowLeft),
                    new DirectionalFocusIntent(direction: TraversalDirection.left)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowRight),
                    new DirectionalFocusIntent(direction: TraversalDirection.right)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowDown),
                    new DirectionalFocusIntent(direction: TraversalDirection.down)
                },
                {new LogicalKeySet(key1: LogicalKeyboardKey.arrowUp), new DirectionalFocusIntent()},

                // Scrolling
                {
                    new LogicalKeySet(key1: LogicalKeyboardKey.control, key2: LogicalKeyboardKey.arrowUp),
                    new ScrollIntent(direction: AxisDirection.up)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.control, key2: LogicalKeyboardKey.arrowDown),
                    new ScrollIntent(direction: AxisDirection.down)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.control, key2: LogicalKeyboardKey.arrowLeft),
                    new ScrollIntent(direction: AxisDirection.left)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.control, key2: LogicalKeyboardKey.arrowRight),
                    new ScrollIntent(direction: AxisDirection.right)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.pageUp),
                    new ScrollIntent(direction: AxisDirection.up, type: ScrollIncrementType.page)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.pageDown),
                    new ScrollIntent(direction: AxisDirection.down, type: ScrollIncrementType.page)
                }
            };

        // Default shortcuts for the web platform.
        public static readonly Dictionary<LogicalKeySet, Intent> _defaultWebShortcuts =
            new Dictionary<LogicalKeySet, Intent> {
                // Activation
                {new LogicalKeySet(key1: LogicalKeyboardKey.space), new Intent(key: ActivateAction.key)},

                // Keyboard traversal.
                {new LogicalKeySet(key1: LogicalKeyboardKey.tab), new Intent(key: NextFocusAction.key)}, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.shift, key2: LogicalKeyboardKey.tab),
                    new Intent(key: PreviousFocusAction.key)
                },

                // Scrolling
                {new LogicalKeySet(key1: LogicalKeyboardKey.arrowUp), new ScrollIntent(direction: AxisDirection.up)}, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowDown),
                    new ScrollIntent(direction: AxisDirection.down)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowLeft),
                    new ScrollIntent(direction: AxisDirection.left)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowRight),
                    new ScrollIntent(direction: AxisDirection.right)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.pageUp),
                    new ScrollIntent(direction: AxisDirection.up, type: ScrollIncrementType.page)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.pageDown),
                    new ScrollIntent(direction: AxisDirection.down, type: ScrollIncrementType.page)
                }
            };

        // Default shortcuts for the macOS platform.
        public static readonly Dictionary<LogicalKeySet, Intent> _defaultMacOsShortcuts =
            new Dictionary<LogicalKeySet, Intent> {
                // Activation
                {new LogicalKeySet(key1: LogicalKeyboardKey.enter), new Intent(key: ActivateAction.key)},
                {new LogicalKeySet(key1: LogicalKeyboardKey.space), new Intent(key: ActivateAction.key)},

                // Keyboard traversal
                {new LogicalKeySet(key1: LogicalKeyboardKey.tab), new Intent(key: NextFocusAction.key)}, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.shift, key2: LogicalKeyboardKey.tab),
                    new Intent(key: PreviousFocusAction.key)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowLeft),
                    new DirectionalFocusIntent(direction: TraversalDirection.left)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowRight),
                    new DirectionalFocusIntent(direction: TraversalDirection.right)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.arrowDown),
                    new DirectionalFocusIntent(direction: TraversalDirection.down)
                },
                {new LogicalKeySet(key1: LogicalKeyboardKey.arrowUp), new DirectionalFocusIntent()},

                // Scrolling
                {
                    new LogicalKeySet(key1: LogicalKeyboardKey.meta, key2: LogicalKeyboardKey.arrowUp),
                    new ScrollIntent(direction: AxisDirection.up)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.meta, key2: LogicalKeyboardKey.arrowDown),
                    new ScrollIntent(direction: AxisDirection.down)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.meta, key2: LogicalKeyboardKey.arrowLeft),
                    new ScrollIntent(direction: AxisDirection.left)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.meta, key2: LogicalKeyboardKey.arrowRight),
                    new ScrollIntent(direction: AxisDirection.right)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.pageUp),
                    new ScrollIntent(direction: AxisDirection.up, type: ScrollIncrementType.page)
                }, {
                    new LogicalKeySet(key1: LogicalKeyboardKey.pageDown),
                    new ScrollIntent(direction: AxisDirection.down, type: ScrollIncrementType.page)
                }
            };

        /// The default value of [WidgetsApp.actions].
        public static readonly Dictionary<LocalKey, ActionFactory> defaultActions =
            new Dictionary<LocalKey, ActionFactory> {
                {DoNothingAction.key, () => new DoNothingAction()},
                {RequestFocusAction.key, () => new RequestFocusAction()},
                {NextFocusAction.key, () => new NextFocusAction()},
                {PreviousFocusAction.key, () => new PreviousFocusAction()},
                {DirectionalFocusAction.key, () => new DirectionalFocusAction()},
                {ScrollAction.key, () => new ScrollAction()}
            };

        public readonly Dictionary<LocalKey, ActionFactory> actions;
        public readonly TransitionBuilder builder;
        public readonly bool checkerboardOffscreenLayers;
        public readonly bool checkerboardRasterCacheImages;
        public readonly Color color;
        public readonly bool debugShowCheckedModeBanner;
        public readonly bool debugShowWidgetInspector;
        public readonly Widget home;
        public readonly string initialRoute;
        public readonly InspectorSelectButtonBuilder inspectorSelectButtonBuilder;
        public readonly Locale locale;
        public readonly LocaleListResolutionCallback localeListResolutionCallback;
        public readonly LocaleResolutionCallback localeResolutionCallback;
        public readonly List<LocalizationsDelegate> localizationsDelegates;

        public readonly GlobalKey<NavigatorState> navigatorKey;
        public readonly List<NavigatorObserver> navigatorObservers;
        public readonly InitialRouteListFactory onGenerateInitialRoutes;
        public readonly RouteFactory onGenerateRoute;
        public readonly GenerateAppTitle onGenerateTitle;
        public readonly RouteFactory onUnknownRoute;
        public readonly PageRouteFactory pageRouteBuilder;
        public readonly Dictionary<string, WidgetBuilder> routes;
        public readonly Dictionary<LogicalKeySet, Intent> shortcuts;
        public readonly bool showPerformanceOverlay;
        public readonly bool showSemanticsDebugger;
        public readonly List<Locale> supportedLocales;
        public readonly TextStyle textStyle;

        public readonly string title;
        public readonly Window window;

        public WidgetsApp(
            Key key = null,
            GlobalKey<NavigatorState> navigatorKey = null,
            RouteFactory onGenerateRoute = null,
            InitialRouteListFactory onGenerateInitialRoutes = null,
            RouteFactory onUnknownRoute = null,
            List<NavigatorObserver> navigatorObservers = null,
            string initialRoute = null,
            PageRouteFactory pageRouteBuilder = null,
            Widget home = null,
            Dictionary<string, WidgetBuilder> routes = null,
            TransitionBuilder builder = null,
            string title = "",
            GenerateAppTitle onGenerateTitle = null,
            TextStyle textStyle = null,
            Color color = null,
            Locale locale = null,
            List<LocalizationsDelegate> localizationsDelegates = null,
            LocaleListResolutionCallback localeListResolutionCallback = null,
            LocaleResolutionCallback localeResolutionCallback = null,
            List<Locale> supportedLocales = null,
            bool showPerformanceOverlay = false,
            bool checkerboardRasterCacheImages = false,
            bool checkerboardOffscreenLayers = false,
            bool showSemanticsDebugger = false,
            bool debugShowWidgetInspector = false,
            bool debugShowCheckedModeBanner = true,
            InspectorSelectButtonBuilder inspectorSelectButtonBuilder = null,
            Dictionary<LogicalKeySet, Intent> shortcuts = null,
            Dictionary<LocalKey, ActionFactory> actions = null
        ) : base(key: key) {
            routes = routes ?? new Dictionary<string, WidgetBuilder>();
            supportedLocales = supportedLocales ?? new List<Locale> {new Locale("en", "US")};
            window = Window.instance;
            D.assert(routes != null);
            D.assert(color != null);
            D.assert(supportedLocales != null && supportedLocales.isNotEmpty());
            this.home = home;
            this.navigatorKey = navigatorKey;
            this.onGenerateRoute = onGenerateRoute;
            this.onGenerateInitialRoutes = onGenerateInitialRoutes;
            this.onUnknownRoute = onUnknownRoute;
            this.pageRouteBuilder = pageRouteBuilder;
            this.routes = routes;
            this.navigatorObservers = navigatorObservers ?? new List<NavigatorObserver>();
            this.initialRoute = initialRoute;
            this.builder = builder;
            this.textStyle = textStyle;
            this.locale = locale;
            this.localizationsDelegates = localizationsDelegates;
            this.localeListResolutionCallback = localeListResolutionCallback;
            this.localeResolutionCallback = localeResolutionCallback;
            this.supportedLocales = supportedLocales;
            this.showPerformanceOverlay = showPerformanceOverlay;
            this.checkerboardOffscreenLayers = checkerboardOffscreenLayers;
            this.checkerboardRasterCacheImages = checkerboardRasterCacheImages;
            this.showSemanticsDebugger = showSemanticsDebugger;
            this.debugShowWidgetInspector = debugShowWidgetInspector;
            this.debugShowCheckedModeBanner = debugShowCheckedModeBanner;
            this.onGenerateTitle = onGenerateTitle;
            this.title = title;
            this.color = color;
            this.inspectorSelectButtonBuilder = inspectorSelectButtonBuilder;
            this.shortcuts = shortcuts;
            this.actions = actions;

            D.assert(
                home == null ||
                onGenerateInitialRoutes == null,
                () => "If onGenerateInitialRoutes is specifiied, the home argument will be redundant."
            );
            D.assert(
                home == null ||
                !this.routes.ContainsKey(key: Navigator.defaultRouteName),
                () => "If the home property is specified, the routes table " +
                      "cannot include an entry for \" / \", since it would be redundant."
            );

            D.assert(
                builder != null ||
                home != null ||
                this.routes.ContainsKey(key: Navigator.defaultRouteName) ||
                onGenerateRoute != null ||
                onUnknownRoute != null,
                () => "Either the home property must be specified, " +
                      "or the routes table must include an entry for \"/\", " +
                      "or there must be on onGenerateRoute callback specified, " +
                      "or there must be an onUnknownRoute callback specified, " +
                      "or the builder property must be specified, " +
                      "because otherwise there is nothing to fall back on if the " +
                      "app is started with an intent that specifies an unknown route."
            );
            D.assert(
                home != null ||
                routes.isNotEmpty() ||
                onGenerateRoute != null ||
                onUnknownRoute != null
                ||
                builder != null &&
                navigatorKey == null &&
                initialRoute == null &&
                navigatorObservers.isEmpty(), () =>
                    "If no route is provided using " +
                    "home, routes, onGenerateRoute, or onUnknownRoute, " +
                    "a non-null callback for the builder property must be provided, " +
                    "and the other navigator-related properties, " +
                    "navigatorKey, initialRoute, and navigatorObservers, " +
                    "must have their initial values " +
                    "(null, null, and the empty list, respectively).");

            D.assert(
                builder != null ||
                onGenerateRoute != null ||
                pageRouteBuilder != null,
                () => "If neither builder nor onGenerateRoute are provided, the " +
                      "pageRouteBuilder must be specified so that the default handler " +
                      "will know what kind of PageRoute transition to build."
            );
        }


        public static Dictionary<LogicalKeySet, Intent> defaultShortcuts {
            get {
#pragma warning disable CS0162
                if (foundation_.kIsWeb) {
                    return _defaultWebShortcuts;
                }
#pragma warning restore CS0162

                switch (Application.platform) {
                    case RuntimePlatform.Android:
                    //case TargetPlatform.fuchsia:
                    case RuntimePlatform.LinuxPlayer:
                    case RuntimePlatform.WindowsPlayer:
                        return _defaultShortcuts;
                    case RuntimePlatform.OSXPlayer:
                        return _defaultMacOsShortcuts;
                    case RuntimePlatform.IPhonePlayer:
                        // No keyboard support on iOS yet.
                        break;
                }

                return new Dictionary<LogicalKeySet, Intent>();
            }
        }

        public override State createState() {
            return new _WidgetsAppState();
        }
    }


    class _WidgetsAppState : State<WidgetsApp>, WidgetsBindingObserver {
        Locale _locale;

        GlobalKey<NavigatorState> _navigator;

        List<LocalizationsDelegate> _localizationsDelegates {
            get {
                var _delegates = new List<LocalizationsDelegate>();
                if (widget.localizationsDelegates != null) {
                    _delegates.AddRange(collection: widget.localizationsDelegates);
                }

                _delegates.Add(item: DefaultWidgetsLocalizations.del);
                return _delegates;
            }
        }

        public void didChangeMetrics() {
            setState();
        }

        public void didChangeTextScaleFactor() {
            setState();
        }

        public void didChangePlatformBrightness() {
            setState(() => { });
        }

        public void didChangeLocales(List<Locale> locale) {
            var newLocale = _resolveLocales(preferredLocales: locale, supportedLocales: widget.supportedLocales);
            if (newLocale != _locale) {
                setState(() => { _locale = newLocale; });
            }
        }

        public Future<bool> didPopRoute() {
            ///async
            D.assert(result: mounted);
            var navigator = _navigator?.currentState;
            if (navigator == null) {
                return Future.value(false).to<bool>();
            }

            return navigator.maybePop<bool>();
        }

        public Future<bool> didPushRoute(string route) {
            D.assert(result: mounted);
            var navigator = _navigator?.currentState;
            if (navigator == null) {
                return Future.value(false).to<bool>();
            }

            navigator.pushNamed<bool>(routeName: route);
            return Future.value(true).to<bool>();
        }

        public void didChangeAccessibilityFeatures() {
        }

        public override void initState() {
            base.initState();
            _updateNavigator();
            _locale =
                _resolveLocales(new List<Locale> {new Locale("en", "US")}, supportedLocales: widget.supportedLocales);
            WidgetsBinding.instance.addObserver(this);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget: oldWidget);
            if (widget.navigatorKey != ((WidgetsApp) oldWidget).navigatorKey) {
                _updateNavigator();
            }
        }

        public override void dispose() {
            WidgetsBinding.instance.removeObserver(this);

            base.dispose();
        }

        void _updateNavigator() {
            _navigator = widget.navigatorKey ?? new GlobalObjectKey<NavigatorState>(this);
        }

        Route _onGenerateRoute(RouteSettings settings) {
            var name = settings.name;
            var pageContentBuilder = name == Navigator.defaultRouteName && widget.home != null
                ? context => widget.home
                : widget.routes.getOrDefault(key: name);

            if (pageContentBuilder != null) {
                D.assert(widget.pageRouteBuilder != null,
                    () => "The default onGenerateRoute handler for WidgetsApp must have a " +
                          "pageRouteBuilder set if the home or routes properties are set.");
                var route = widget.pageRouteBuilder(
                    settings: settings,
                    builder: pageContentBuilder
                );
                D.assert(route != null,
                    () => "The pageRouteBuilder for WidgetsApp must return a valid non-null Route.");
                return route;
            }

            if (widget.onGenerateRoute != null) {
                return widget.onGenerateRoute(settings: settings);
            }

            return null;
        }

        Route<object> _onUnknownRoute(RouteSettings settings) {
            D.assert(() => {
                if (widget.onUnknownRoute == null) {
                    throw new UIWidgetsError(
                        $"Could not find a generator for route {settings} in the {GetType()}.\n" +
                        "Generators for routes are searched for in the following order:\n" +
                        " 1. For the \"/\" route, the \"home\" property, if non-null, is used.\n" +
                        " 2. Otherwise, the \"routes\" table is used, if it has an entry for " +
                        "the route.\n" +
                        " 3. Otherwise, onGenerateRoute is called. It should return a " +
                        "non-null value for any valid route not handled by \"home\" and \"routes\".\n" +
                        " 4. Finally if all else fails onUnknownRoute is called.\n" +
                        "Unfortunately, onUnknownRoute was not set."
                    );
                }

                return true;
            });
            var result = widget.onUnknownRoute(settings: settings) as Route<object>;
            D.assert(() => {
                if (result == null) {
                    throw new UIWidgetsError(
                        "The onUnknownRoute callback returned null.\n" +
                        "When the $runtimeType requested the route $settings from its " +
                        "onUnknownRoute callback, the callback returned null. Such callbacks " +
                        "must never return null."
                    );
                }

                return true;
            });
            return result;
        }

        Locale _resolveLocales(List<Locale> preferredLocales, List<Locale> supportedLocales) {
            if (widget.localeListResolutionCallback != null) {
                var locale =
                    widget.localeListResolutionCallback(locales: preferredLocales,
                        supportedLocales: widget.supportedLocales);
                if (locale != null) {
                    return locale;
                }
            }

            if (widget.localeResolutionCallback != null) {
                var locale = widget.localeResolutionCallback(
                    preferredLocales != null && preferredLocales.isNotEmpty() ? preferredLocales.first() : null,
                    supportedLocales: widget.supportedLocales
                );
                if (locale != null) {
                    return locale;
                }
            }

            return basicLocaleListResolution(preferredLocales: preferredLocales, supportedLocales: supportedLocales);
        }


        static Locale basicLocaleListResolution(List<Locale> preferredLocales, List<Locale> supportedLocales) {
            if (preferredLocales == null || preferredLocales.isEmpty()) {
                return supportedLocales.first();
            }

            var allSupportedLocales = new Dictionary<string, Locale>();
            var languageAndCountryLocales = new Dictionary<string, Locale>();
            var languageAndScriptLocales = new Dictionary<string, Locale>();
            var languageLocales = new Dictionary<string, Locale>();
            var countryLocales = new Dictionary<string, Locale>();
            foreach (var locale in supportedLocales) {
                allSupportedLocales.putIfAbsent(
                    locale.languageCode + "_" + locale.scriptCode + "_" + locale.countryCode, () => locale);
                languageLocales.putIfAbsent(key: locale.languageCode, () => locale);
                countryLocales.putIfAbsent(key: locale.countryCode, () => locale);
                languageAndScriptLocales.putIfAbsent(locale.languageCode + "_" + locale.scriptCode, () => locale);
                languageAndCountryLocales.putIfAbsent(locale.languageCode + "_" + locale.countryCode, () => locale);
            }

            Locale matchesLanguageCode = null;
            Locale matchesCountryCode = null;

            for (var localeIndex = 0; localeIndex < preferredLocales.Count; localeIndex++) {
                var userLocale = preferredLocales[index: localeIndex];
                if (allSupportedLocales.ContainsKey(userLocale.languageCode + "_" + userLocale.scriptCode + "_" +
                                                    userLocale.countryCode)) {
                    return userLocale;
                }

                if (userLocale.scriptCode != null) {
                    Locale match = null;
                    if (languageAndScriptLocales.TryGetValue(userLocale.languageCode + "_" + userLocale.scriptCode,
                        value: out match)) {
                        if (match != null) {
                            return match;
                        }
                    }
                }

                if (userLocale.countryCode != null) {
                    //Locale match = languageAndCountryLocales['${userLocale.languageCode}_${userLocale.countryCode}'];
                    Locale match = null;
                    if (languageAndCountryLocales.TryGetValue(userLocale.languageCode + "_" + userLocale.countryCode,
                        value: out match)) {
                        if (match != null) {
                            return match;
                        }
                    }
                }

                if (matchesLanguageCode != null) {
                    return matchesLanguageCode;
                }

                if (languageLocales.ContainsKey(key: userLocale.languageCode)) {
                    matchesLanguageCode = languageLocales[key: userLocale.languageCode];

                    if (localeIndex == 0 &&
                        !(localeIndex + 1 < preferredLocales.Count && preferredLocales[localeIndex + 1].languageCode ==
                            userLocale.languageCode)) {
                        return matchesLanguageCode;
                    }
                }


                if (matchesCountryCode == null && userLocale.countryCode != null) {
                    if (countryLocales.ContainsKey(key: userLocale.countryCode)) {
                        matchesCountryCode = countryLocales[key: userLocale.countryCode];
                    }
                }
            }

            var resolvedLocale = matchesLanguageCode ?? matchesCountryCode ?? supportedLocales.first();
            return resolvedLocale;
        }


        void inspectorShowChanged() {
            setState();
        }


        bool _debugCheckLocalizations(Locale appLocale) {
            D.assert(() => {
                var unsupportedTypes = new HashSet<Type>();
                foreach (var _delegate in _localizationsDelegates) {
                    unsupportedTypes.Add(item: _delegate.type);
                }

                foreach (var _delegate in _localizationsDelegates) {
                    if (!unsupportedTypes.Contains(item: _delegate.type)) {
                        continue;
                    }

                    if (_delegate.isSupported(locale: appLocale)) {
                        unsupportedTypes.Remove(item: _delegate.type);
                    }
                }

                if (unsupportedTypes.isEmpty()) {
                    return true;
                }

                var list = new List<string> {"CupertinoLocalizations"};
                var unsupportedTypesList = new List<string>();
                foreach (var type in unsupportedTypes) {
                    unsupportedTypesList.Add(type.ToString());
                }

                if (unsupportedTypesList.SequenceEqual(second: list)) {
                    return true;
                }

                var message = new StringBuilder();
                message.Append('\u2550' * 8);
                message.Append(
                    "Warning: This application's locale, $appLocale, is not supported by all of its\n" +
                    "localization delegates."
                );
                foreach (var unsupportedType in unsupportedTypes) {
                    if (unsupportedType.ToString() == "CupertinoLocalizations") {
                        continue;
                    }

                    message.Append(
                        "> A " + unsupportedType + " delegate that supports the " + appLocale + "locale was not found."
                    );
                }

                message.Append(
                    "See https://flutter.dev/tutorials/internationalization/ for more\n" +
                    "information about configuring an app's locale, supportedLocales,\n" +
                    "and localizationsDelegates parameters."
                );
                message.Append('\u2550' * 8);

                return true;
            });
            return true;
        }

        public override Widget build(BuildContext context) {
            Widget navigator = null;
            if (_navigator != null) {
                RouteListFactory routeListFactory = (state, route) => {
                    return widget.onGenerateInitialRoutes(initialRoute: route);
                };
                navigator = new Navigator(
                    key: _navigator,
                    initialRoute: WidgetsBinding.instance.window.defaultRouteName != Navigator.defaultRouteName
                        ? WidgetsBinding.instance.window.defaultRouteName
                        : widget.initialRoute ?? WidgetsBinding.instance.window.defaultRouteName,
                    onGenerateRoute: _onGenerateRoute,
                    onGenerateInitialRoutes: widget.onGenerateInitialRoutes == null
                        ? Navigator.defaultGenerateInitialRoutes
                        : routeListFactory,
                    onUnknownRoute: _onUnknownRoute,
                    observers: widget.navigatorObservers
                );
            }

            Widget result;
            if (widget.builder != null) {
                result = new Builder(
                    builder: context1 => { return widget.builder(context: context1, child: navigator); });
            }
            else {
                D.assert(navigator != null);
                result = navigator;
            }

            if (widget.textStyle != null) {
                result = new DefaultTextStyle(
                    style: widget.textStyle,
                    child: result
                );
            }

            PerformanceOverlay performanceOverlay = null;

            if (widget.showPerformanceOverlay || WidgetsApp.showPerformanceOverlayOverride) {
                performanceOverlay = PerformanceOverlay.allEnabled(
                    checkerboardRasterCacheImages: widget.checkerboardRasterCacheImages,
                    checkerboardOffscreenLayers: widget.checkerboardOffscreenLayers
                );
            }
            else if (widget.checkerboardRasterCacheImages || widget.checkerboardOffscreenLayers) {
                performanceOverlay = new PerformanceOverlay(
                    checkerboardRasterCacheImages: widget.checkerboardRasterCacheImages,
                    checkerboardOffscreenLayers: widget.checkerboardOffscreenLayers
                );
            }


            if (performanceOverlay != null) {
                result = new Stack(
                    children: new List<Widget> {
                        result,
                        new Positioned(top: 0.0f, left: 0.0f, right: 0.0f, child: performanceOverlay)
                    }
                );
            }


            D.assert(() => {
                if (widget.debugShowWidgetInspector || WidgetsApp.debugShowWidgetInspectorOverride) {
                    result = new WidgetInspector(
                        child: result,
                        selectButtonBuilder: widget.inspectorSelectButtonBuilder
                    );
                }

                if (widget.debugShowCheckedModeBanner && WidgetsApp.debugAllowBannerOverride) {
                    result = new CheckedModeBanner(
                        child: result
                    );
                }

                return true;
            });
            
            //Fix Me !!
            //TODO: the following line is a work-around for some potential TextDirection bug
            //In the home page of the Shrine sample, the positions of the buttons are not correct, which is 
            //determined by the TextDirection of some widget. we should fix it!
            result = new Directionality(child: result, TextDirection.ltr);

            Widget title = null;
            if (widget.onGenerateTitle != null) {
                title = new Builder(
                    builder: context2 => {
                        var _title = widget.onGenerateTitle(context: context2);
                        D.assert(_title != null, () => "onGenerateTitle must return a non-null String");
                        return new Title(
                            title: _title,
                            color: widget.color,
                            child: result
                        );
                    }
                );
            }
            else {
                title = new Title(
                    title: widget.title,
                    color: widget.color,
                    child: result
                );
            }

            var appLocale = widget.locale != null
                ? _resolveLocales(new List<Locale> {widget.locale}, supportedLocales: widget.supportedLocales)
                : _locale;

            D.assert(_debugCheckLocalizations(appLocale: appLocale));

            return new Shortcuts(
                shortcuts: widget.shortcuts ?? WidgetsApp.defaultShortcuts,
                debugLabel: "<Default WidgetsApp Shortcuts>",
                child: new Actions(
                    actions: widget.actions ?? WidgetsApp.defaultActions,
                    child: new FocusTraversalGroup(
                        policy: new ReadingOrderTraversalPolicy(),
                        child: new _MediaQueryFromWindow(
                            child: new Localizations(
                                locale: appLocale,
                                delegates: _localizationsDelegates.ToList(),
                                child: title
                            )
                        )
                    )
                )
            );
        }

        Widget _InspectorSelectButtonBuilder(BuildContext context, VoidCallback onPressed) {
            return new _InspectorSelectButton(onPressed: onPressed);
        }
    }

    public class _MediaQueryFromWindow : StatefulWidget {
        public readonly Widget child;

        public _MediaQueryFromWindow(Key key = null, Widget child = null) : base(key: key) {
            this.child = child;
        }

        public override State createState() {
            return new _MediaQueryFromWindowsState();
        }
    }

    class _MediaQueryFromWindowsState : State<_MediaQueryFromWindow>, WidgetsBindingObserver {
        public void didChangeAccessibilityFeatures() {
            setState(() => { });
        }

        public void didChangeMetrics() {
            setState(() => { });
        }

        public void didChangeTextScaleFactor() {
            setState(() => { });
        }

        public void didChangePlatformBrightness() {
            setState(() => { });
        }

        public void didChangeLocales(List<Locale> locale) {
            throw new NotImplementedException();
        }

        public Future<bool> didPopRoute() {
            throw new NotImplementedException();
        }

        public Future<bool> didPushRoute(string route) {
            throw new NotImplementedException();
        }

        public override void initState() {
            base.initState();
            WidgetsBinding.instance.addObserver(this);
        }

        public override Widget build(BuildContext context) {
            return new MediaQuery(
                data: MediaQueryData.fromWindow(window: WidgetsBinding.instance.window),
                child: widget.child
            );
        }

        public override void dispose() {
            WidgetsBinding.instance.removeObserver(this);
            base.dispose();
        }
    }

    class _InspectorSelectButton : StatelessWidget {
        public readonly GestureTapCallback onPressed;

        public _InspectorSelectButton(
            VoidCallback onPressed,
            Key key = null
        ) : base(key: key) {
            this.onPressed = () => onPressed();
        }

        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onTap: onPressed,
                child: new Container(
                    color: Color.fromARGB(255, 0, 0, 255),
                    padding: EdgeInsets.all(10),
                    child: new Text("Select", style: new TextStyle(color: Color.fromARGB(255, 255, 255, 255)))
                )
            );
        }
    }
}