using System;
using System.Collections.Generic;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
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
        public readonly TransitionBuilder builder;
        public readonly Widget home;
        public readonly string initialRoute;

        public readonly GlobalKey<NavigatorState> navigatorKey;
        public readonly List<NavigatorObserver> navigatorObservers;
        public readonly RouteFactory onGenerateRoute;
        public readonly InitialRouteListFactory onGenerateInitialRoutes;
        public readonly RouteFactory onUnknownRoute;
        public readonly PageRouteFactory pageRouteBuilder;
        public readonly Dictionary<string, WidgetBuilder> routes;
        public readonly TextStyle textStyle;
        public readonly Window window;
        public readonly bool showPerformanceOverlay;
        public readonly Locale locale;
        public readonly List<LocalizationsDelegate> localizationsDelegates;
        public readonly LocaleListResolutionCallback localeListResolutionCallback;
        public readonly LocaleResolutionCallback localeResolutionCallback;
        public readonly List<Locale> supportedLocales;

        public readonly string title;
        public readonly GenerateAppTitle onGenerateTitle;
        public readonly Color color;
        public readonly InspectorSelectButtonBuilder inspectorSelectButtonBuilder;
        
        public static bool showPerformanceOverlayOverride = false;
        public static bool debugShowWidgetInspectorOverride = false;
        public static bool debugAllowBannerOverride = true;
        public readonly bool debugShowCheckedModeBanner;
        public readonly bool checkerboardRasterCacheImages;
        public readonly bool checkerboardOffscreenLayers;
        public readonly bool showSemanticsDebugger;
        public readonly bool debugShowWidgetInspector;

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
            InspectorSelectButtonBuilder inspectorSelectButtonBuilder = null
            //shortcuts
            //actions
        ) : base(key) {
            
            routes = routes ?? new Dictionary<string, WidgetBuilder>();
            supportedLocales = supportedLocales ?? new List<Locale> {new Locale("en", "US")};
            window = Window.instance;
            D.assert(navigatorObservers != null);
            //D.assert(routes != null);
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
            
            D.assert(
                home == null ||
                onGenerateInitialRoutes == null,
                () => "If onGenerateInitialRoutes is specifiied, the home argument will be redundant."
                );
            D.assert(
                home == null ||
                !this.routes.ContainsKey(Navigator.defaultRouteName),
                () => "If the home property is specified, the routes table " +
                      "cannot include an entry for \" / \", since it would be redundant."
            );

            D.assert(
                builder != null ||
                home != null ||
                this.routes.ContainsKey(Navigator.defaultRouteName) ||
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
                (home != null ||
                 routes.isNotEmpty() ||
                 onGenerateRoute != null ||
                 onUnknownRoute != null)
                ||
                (builder != null &&
                 navigatorKey == null &&
                 initialRoute == null &&
                 navigatorObservers.isEmpty()),()=>
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

        public override State createState() {
            return new _WidgetsAppState();
        }
    }

    public class WindowProvider : InheritedWidget {
        public readonly Window window;

        public WindowProvider(Key key = null, Window window = null, Widget child = null) :
            base(key, child) {
            D.assert(window != null);
            this.window = window;
        }

        public static Window of(BuildContext context) {
            var provider = (WindowProvider) context.inheritFromWidgetOfExactType(typeof(WindowProvider));
            if (provider == null) {
                throw new UIWidgetsError("WindowProvider is missing");
            }

            return provider.window;
        }

        public static Window of(GameObject gameObject) {
            D.assert(false, () => "window.Of is not implemented yet!");
            return null;
            
            /*
            var panel = gameObject.GetComponent<UIWidgetsPanel>();
            return panel == null ? null : panel.window;
            */
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            D.assert(window == ((WindowProvider) oldWidget).window);
            return false;
        }
    }

    class _WidgetsAppState : State<WidgetsApp>, WidgetsBindingObserver {
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
            Locale newLocale = _resolveLocales(locale, widget.supportedLocales);
            if (newLocale != _locale) {
                setState(() => { _locale = newLocale; });
            }
        }

        List<LocalizationsDelegate> _localizationsDelegates {
            get {
                List<LocalizationsDelegate> _delegates = new List<LocalizationsDelegate>();
                if (widget.localizationsDelegates != null) {
                    _delegates.AddRange(widget.localizationsDelegates);
                }

                _delegates.Add(DefaultWidgetsLocalizations.del);
                return _delegates;
            }
        }

        public override void initState() {
            base.initState();
            _updateNavigator();

            //todo: xingwei.zhu: change the default locale to ui.Window.locale
            _locale =
                _resolveLocales(new List<Locale> {new Locale("en", "US")}, widget.supportedLocales);

            /*D.assert(() => {
                WidgetInspectorService.instance.inspectorShowCallback += inspectorShowChanged;
                return true;
            });*/

            WidgetsBinding.instance.addObserver(this);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (widget.navigatorKey != ((WidgetsApp) oldWidget).navigatorKey) {
                _updateNavigator();
            }
        }

        public override void dispose() {
            WidgetsBinding.instance.removeObserver(this);

            /*D.assert(() => {
                WidgetInspectorService.instance.inspectorShowCallback -= inspectorShowChanged;
                return true;
            });*/
            base.dispose();
        }

        GlobalKey<NavigatorState> _navigator;

        void _updateNavigator() {
            _navigator = widget.navigatorKey ?? new GlobalObjectKey<NavigatorState>(this);
        }

        Route _onGenerateRoute(RouteSettings settings) {
            var name = settings.name;
            var pageContentBuilder = name == Navigator.defaultRouteName && widget.home != null
                ? context => widget.home
                : widget.routes.getOrDefault(name);

            if (pageContentBuilder != null) {
                D.assert(widget.pageRouteBuilder != null,
                    () => "The default onGenerateRoute handler for WidgetsApp must have a " +
                          "pageRouteBuilder set if the home or routes properties are set.");
                var route = widget.pageRouteBuilder(
                    settings,
                    pageContentBuilder
                );
                D.assert(route != null,
                    () => "The pageRouteBuilder for WidgetsApp must return a valid non-null Route.");
                return route;
            }

            if (widget.onGenerateRoute != null) {
                return widget.onGenerateRoute(settings);
            }

            return null;
        }

        public Future<bool> didPopRoute() {
            ///async
            D.assert(mounted);
            var navigator = _navigator?.currentState;
            if (navigator == null) {
                return Future<bool>.value(false).to<bool>();
            }

            return navigator.maybePop<bool>();
        }

        public Future<bool> didPushRoute(string route) {
            D.assert(mounted);
            var navigator = _navigator?.currentState;
            if (navigator == null) {
                return Future<bool>.value(false).to<bool>();
            }

            navigator.pushNamed<bool>(route);
            return Future<bool>.value(true).to<bool>();
        }

        Route<object> _onUnknownRoute(RouteSettings settings) {
            D.assert(() => {
                if (widget.onUnknownRoute == null) {
                    throw new UIWidgetsError(
                        $"Could not find a generator for route {settings} in the {GetType()}.\n" +
                        $"Generators for routes are searched for in the following order:\n" +
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
            var result = widget.onUnknownRoute(settings) as Route<object>;
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

        Locale _locale;

        Locale _resolveLocales(List<Locale> preferredLocales, List<Locale> supportedLocales) {
            if (widget.localeListResolutionCallback != null) {
                Locale locale =
                    widget.localeListResolutionCallback(preferredLocales, widget.supportedLocales);
                if (locale != null) {
                    return locale;
                }
            }

            if (widget.localeResolutionCallback != null) {
                Locale locale = widget.localeResolutionCallback(
                    preferredLocales != null && preferredLocales.isNotEmpty() ? preferredLocales.first() : null,
                    widget.supportedLocales
                );
                if (locale != null) {
                    return locale;
                }
            }

            return basicLocaleListResolution(preferredLocales, supportedLocales);
        }


        static Locale basicLocaleListResolution(List<Locale> preferredLocales, List<Locale> supportedLocales) {
            if (preferredLocales == null || preferredLocales.isEmpty()) {
                return supportedLocales.first();
            }

            Dictionary<string, Locale> allSupportedLocales = new Dictionary<string, Locale>();
            Dictionary<string, Locale> languageAndCountryLocales = new Dictionary<string, Locale>();
            Dictionary<string, Locale> languageAndScriptLocales = new Dictionary<string, Locale>();
            Dictionary<string, Locale> languageLocales = new Dictionary<string, Locale>();
            Dictionary<string, Locale> countryLocales = new Dictionary<string, Locale>();
            foreach (Locale locale in supportedLocales) {
                allSupportedLocales.putIfAbsent(
                    locale.languageCode + "_" + locale.scriptCode + "_" + locale.countryCode, () => locale);
                languageLocales.putIfAbsent(locale.languageCode, () => locale);
                countryLocales.putIfAbsent(locale.countryCode, () => locale);
                languageAndScriptLocales.putIfAbsent(locale.languageCode + "_" + locale.scriptCode, () => locale);
                languageAndCountryLocales.putIfAbsent(locale.languageCode + "_" + locale.countryCode, () => locale);

            }

            Locale matchesLanguageCode = null;
            Locale matchesCountryCode = null;

            for (int localeIndex = 0; localeIndex < preferredLocales.Count; localeIndex++) {
                Locale userLocale = preferredLocales[localeIndex];
                if (allSupportedLocales.ContainsKey(userLocale.languageCode + "_" + userLocale.scriptCode + "_" +
                                                    userLocale.countryCode)) {
                    return userLocale;
                }

                if (userLocale.scriptCode != null) {
                    Locale match = null;
                    if (languageAndScriptLocales.TryGetValue(userLocale.languageCode + "_" + userLocale.scriptCode,
                        out match)) {
                        if (match != null) {
                            return match;
                        }
                    }
                }

                if (userLocale.countryCode != null) {
                    //Locale match = languageAndCountryLocales['${userLocale.languageCode}_${userLocale.countryCode}'];
                    Locale match = null;
                    if (languageAndCountryLocales.TryGetValue(userLocale.languageCode + "_" + userLocale.countryCode,
                        out match)) {
                        if (match != null) {
                            return match;
                        }
                    }
                }

                if (matchesLanguageCode != null) {
                    return matchesLanguageCode;
                }

                if (languageLocales.ContainsKey(userLocale.languageCode)) {
                    matchesLanguageCode = languageLocales[userLocale.languageCode];

                    if (localeIndex == 0 &&
                        !(localeIndex + 1 < preferredLocales.Count && preferredLocales[localeIndex + 1].languageCode ==
                            userLocale.languageCode)) {
                        return matchesLanguageCode;
                    }
                }


                if (matchesCountryCode == null && userLocale.countryCode != null) {
                    if (countryLocales.ContainsKey(userLocale.countryCode)) {
                        matchesCountryCode = countryLocales[userLocale.countryCode];
                    }
                }
            }

            Locale resolvedLocale = matchesLanguageCode ?? matchesCountryCode ?? supportedLocales.first();
            return resolvedLocale;
        }


        void inspectorShowChanged() {
            setState();
        }
        

        /*bool _debugCheckLocalizations(Locale appLocale) {
            D.assert(() =>{
                HashSet<Type> unsupportedTypes =
                    _localizationsDelegates.map<Type>((LocalizationsDelegate delegate) => delegate.type).toSet();
                foreach ( LocalizationsDelegate<dynamic> delegate in _localizationsDelegates) {
                    if (!unsupportedTypes.contains(delegate.type))
                        continue;
                    if (delegate.isSupported(appLocale))
                        unsupportedTypes.remove(delegate.type);
                }
                if (unsupportedTypes.isEmpty())
                    return true;

                if (listEquals(unsupportedTypes.map((Type type) => type.toString()).toList(), <String>['CupertinoLocalizations']))
                return true;

                StringBuffer message = new StringBuffer();
                message.writeln('\u2550' * 8);
                message.writeln(
                    "Warning: This application's locale, $appLocale, is not supported by all of its\n"
                'localization delegates.'
                    );
                foreach ( Type unsupportedType in unsupportedTypes) {
                    // Currently the Cupertino library only provides english localizations.
                    // Remove this when https://github.com/flutter/flutter/issues/23847
                    // is fixed.
                    if (unsupportedType.toString() == 'CupertinoLocalizations')
                        continue;
                    message.writeln(
                        '> A $unsupportedType delegate that supports the $appLocale locale was not found.'
                    );
                }
                message.writeln(
                    'See https://flutter.dev/tutorials/internationalization/ for more\n'
                "information about configuring an app's locale, supportedLocales,\n"
                'and localizationsDelegates parameters.'
                    );
                message.writeln('\u2550' * 8);
                debugPrint(message.toString());
                return true;
            });
            return true;
        }*/

        public override Widget build(BuildContext context) {
            Widget navigator = null;
            if (_navigator != null) {
                RouteListFactory routeListFactory = (state, route) => {return widget.onGenerateInitialRoutes(route); };
                navigator = new Navigator(
                    key: _navigator,
                    initialRoute: widget.initialRoute ?? Navigator.defaultRouteName,
                    /*WidgetsBinding.instance.window.defaultRouteName != Navigator.defaultRouteName
                        ? WidgetsBinding.instance.window.defaultRouteName
                        : widget.initialRoute ?? WidgetsBinding.instance.window.defaultRouteName,*/
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
                    builder: _context => { return widget.builder(_context, navigator); }
                );
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
            if (widget.showPerformanceOverlay) {
                performanceOverlay = PerformanceOverlay.allEnabled();
            }
            /*if (widget.showPerformanceOverlay || WidgetsApp.showPerformanceOverlayOverride) {
                performanceOverlay = PerformanceOverlay.allEnabled(
                    checkerboardRasterCacheImages: widget.checkerboardRasterCacheImages,
                    checkerboardOffscreenLayers: widget.checkerboardOffscreenLayers
                );
            } else if (widget.checkerboardRasterCacheImages || widget.checkerboardOffscreenLayers) {
                performanceOverlay = new PerformanceOverlay(
                    checkerboardRasterCacheImages: widget.checkerboardRasterCacheImages,
                    checkerboardOffscreenLayers: widget.checkerboardOffscreenLayers
                );
            }*/

            if (performanceOverlay != null) {
                result = new Stack(
                    children: new List<Widget> {
                        result,
                        new Positioned(top: 0.0f,
                            left: 0.0f,
                            right: 0.0f,
                            child: performanceOverlay)
                    });
            }

            /*if (widget.showSemanticsDebugger) {
                result = SemanticsDebugger(
                    child: result,
                );
            }*/
            D.assert(() => {
                if (WidgetInspectorService.instance.debugShowInspector) {
                    result = new WidgetInspector(null, result, _InspectorSelectButtonBuilder);
                }

                /*if (widget.debugShowWidgetInspector || WidgetsApp.debugShowWidgetInspectorOverride) {
                    result = new WidgetInspector(
                        child: result,
                        selectButtonBuilder: widget.inspectorSelectButtonBuilder
                    );
                }
                if (widget.debugShowCheckedModeBanner && WidgetsApp.debugAllowBannerOverride) {
                    result = new CheckedModeBanner(
                        child: result
                    );
                }*/
                return true;
            });

            result = new Directionality(child: result, TextDirection.ltr);
            result = new WindowProvider(
                window: widget.window,
                child: result
            );
            /*Widget title = null;
            if (widget.onGenerateTitle != null) {
                title = new Builder(
                    builder: (BuildContext context1)=> {
                    string title1 = widget.onGenerateTitle(context1);
                    D.assert(title1 != null,()=> "onGenerateTitle must return a non-null String");
                    return new Title(
                        title: title1,
                        color: widget.color,
                        child: result
                    );
                }
                );
            } else {
                title = new Title(
                    title: widget.title,
                    color: widget.color,
                    child: result
                );
            }*/

            Locale appLocale = widget.locale != null
                ? _resolveLocales(new List<Locale> {widget.locale}, widget.supportedLocales)
                : _locale;
            //D.assert(_debugCheckLocalizations(appLocale));
            result = new MediaQuery(
                data: MediaQueryData.fromWindow(widget.window),
                child: new Localizations(
                    locale: appLocale,
                    delegates: _localizationsDelegates,
                    child: result)
            );
            /////todo
            /// 
            /*result = new Shortcuts(
                shortcuts: widget.shortcuts ?? WidgetsApp.defaultShortcuts,
                debugLabel: "<Default WidgetsApp Shortcuts>",
                child: new Actions(
                    actions: widget.actions ?? WidgetsApp.defaultActions,
                    child: FocusTraversalGroup(
                        policy: ReadingOrderTraversalPolicy(),
                        child: _MediaQueryFromWindow(
                            child: new Localizations(
                                locale: appLocale,
                                delegates: _localizationsDelegates,
                                child: title
                            )
                        )
                    )
                )
            );*/

            return result;
        }

        Widget _InspectorSelectButtonBuilder(BuildContext context, VoidCallback onPressed) {
            return new _InspectorSelectButton(onPressed);
        }
    }

    /*public class _MediaQueryFromWindow : StatefulWidget {
        public _MediaQueryFromWindow(Key key = null, Widget child = null) : base(key: key) {
        }
        public readonly Widget child;

        public override State createState() {
            return new _MediaQueryFromWindowsState();
        }
    
    }

    class _MediaQueryFromWindowsState : State<_MediaQueryFromWindow>,WidgetsBindingObserver {
        public override void initState() {
            base.initState();
            WidgetsBinding.instance.addObserver(this);
        }
        public  void didChangeAccessibilityFeatures() {
            setState(()=> {
            });
        }
        public  void didChangeMetrics() {
            setState(()=>{
              
            });
        }
        public  void didChangeTextScaleFactor() {
            setState(()=> {
            });
        }
        public void didChangePlatformBrightness() {
            setState(()=> {
            });
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

        public override Widget build(BuildContext context) {
            return new MediaQuery(
                data: MediaQueryData.fromWindow(WidgetsBinding.instance.window),
                child: widget.child
            );
        }
        public override void dispose() {
            WidgetsBinding.instance.removeObserver(this);
            base.dispose();
        }
    }
*/
    class _InspectorSelectButton : StatelessWidget {
        public readonly GestureTapCallback onPressed;

        public _InspectorSelectButton(
            VoidCallback onPressed,
            Key key = null
        ) : base(key) {
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