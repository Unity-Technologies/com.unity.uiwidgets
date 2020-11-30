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

        public WidgetsApp(
            Key key = null,
            GlobalKey<NavigatorState> navigatorKey = null,
            RouteFactory onGenerateRoute = null,
            InitialRouteListFactory onGenerateInitialRoutes = null,
            RouteFactory onUnknownRoute = null,
            PageRouteFactory pageRouteBuilder = null,
            List<NavigatorObserver> navigatorObservers = null,
            string initialRoute = null,
            Dictionary<string, WidgetBuilder> routes = null,
            TransitionBuilder builder = null,
            TextStyle textStyle = null,
            Widget home = null,
            Locale locale = null,
            List<LocalizationsDelegate> localizationsDelegates = null,
            LocaleListResolutionCallback localeListResolutionCallback = null,
            LocaleResolutionCallback localeResolutionCallback = null,
            List<Locale> supportedLocales = null,
            bool showPerformanceOverlay = false,
            GenerateAppTitle onGenerateTitle = null,
            string title = "",
            Color color = null,
            InspectorSelectButtonBuilder inspectorSelectButtonBuilder = null
        ) : base(key) {
            routes = routes ?? new Dictionary<string, WidgetBuilder>();
            supportedLocales = supportedLocales ?? new List<Locale> {new Locale("en", "US")};
            window = Window.instance;
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
        GlobalKey<NavigatorState> _navigator;

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

            D.assert(() => {
                WidgetInspectorService.instance.inspectorShowCallback += inspectorShowChanged;
                return true;
            });

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

            D.assert(() => {
                WidgetInspectorService.instance.inspectorShowCallback -= inspectorShowChanged;
                return true;
            });
            base.dispose();
        }

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

        Route _onUnknownRoute(RouteSettings settings) {
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
            var result = widget.onUnknownRoute(settings);
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
            Dictionary<string, Locale> languageLocales = new Dictionary<string, Locale>();
            Dictionary<string, Locale> countryLocales = new Dictionary<string, Locale>();
            foreach (Locale locale in supportedLocales) {
                allSupportedLocales.putIfAbsent(locale.languageCode + "_" + locale.countryCode, () => locale);
                languageLocales.putIfAbsent(locale.languageCode, () => locale);
                countryLocales.putIfAbsent(locale.countryCode, () => locale);
            }

            Locale matchesLanguageCode = null;
            Locale matchesCountryCode = null;

            for (int localeIndex = 0; localeIndex < preferredLocales.Count; localeIndex++) {
                Locale userLocale = preferredLocales[localeIndex];
                if (allSupportedLocales.ContainsKey(userLocale.languageCode + "_" + userLocale.countryCode)) {
                    return userLocale;
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

        public override Widget build(BuildContext context) {
            Widget navigator = null;
            if (_navigator != null) {
                navigator = new Navigator(
                    key: _navigator,
                    //initialRoute: widget.initialRoute ?? Navigator.defaultRouteName,
                    //onGenerateRoute: _onGenerateRoute,
                    initialRoute: WidgetsBinding.instance.window.defaultRouteName != Navigator.defaultRouteName
                        ? WidgetsBinding.instance.window.defaultRouteName
                        : widget.initialRoute ?? WidgetsBinding.instance.window.defaultRouteName,
                    onGenerateRoute: _onGenerateRoute,
                    onGenerateInitialRoutes: 
                        widget.onGenerateInitialRoutes == null
                        ? Navigator.defaultGenerateInitialRoutes
                        : (NavigatorState navigator1, string initialRouteName) => {
                        return widget.onGenerateInitialRoutes(initialRouteName);
                    },
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

            D.assert(() => {
                if (WidgetInspectorService.instance.debugShowInspector) {
                    result = new WidgetInspector(null, result, _InspectorSelectButtonBuilder);
                }

                return true;
            });

            result = new Directionality(child: result, TextDirection.ltr);
            result = new WindowProvider(
                window: widget.window,
                child: result
            );

            Locale appLocale = widget.locale != null
                ? _resolveLocales(new List<Locale> {widget.locale}, widget.supportedLocales)
                : _locale;

            result = new MediaQuery(
                data: MediaQueryData.fromWindow(widget.window),
                child: new Localizations(
                    locale: appLocale,
                    delegates: _localizationsDelegates,
                    child: result)
            );

            return result;
        }

        Widget _InspectorSelectButtonBuilder(BuildContext context, VoidCallback onPressed) {
            return new _InspectorSelectButton(onPressed);
        }
    }

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