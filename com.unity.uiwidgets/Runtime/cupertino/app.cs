using System.Collections.Generic;
using Unity.UIWidgets.foundation;
//using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoApp : StatefulWidget {
        public CupertinoApp(
            Key key = null,
            GlobalKey<NavigatorState> navigatorKey = null,
            Widget home = null,
            CupertinoThemeData theme = null,
            Dictionary<string, WidgetBuilder> routes = null,
            string initialRoute = null,
            RouteFactory onGenerateRoute = null,
            InitialRouteListFactory onGenerateInitialRoutes = null,
            RouteFactory onUnknownRoute = null,
            List<NavigatorObserver> navigatorObservers = null,
            TransitionBuilder builder = null,
            string title = "",
            GenerateAppTitle onGenerateTitle = null,
            Color color = null,
            Locale locale = null,
            List<LocalizationsDelegate<CupertinoLocalizations>> localizationsDelegates = null,
            LocaleListResolutionCallback localeListResolutionCallback = null,
            LocaleResolutionCallback localeResolutionCallback = null,
            List<Locale> supportedLocales = null,
            bool showPerformanceOverlay = false,
            bool checkerboardRasterCacheImages = false, 
            bool checkerboardOffscreenLayers = false,
            bool showSemanticsDebugger = false,
            bool debugShowCheckedModeBanner = true,
            Dictionary<LogicalKeySet, Intent> shortcuts = null,
            Dictionary<LocalKey, ActionFactory> actions = null
        ) : base(key: key) {
            
            D.assert(title != null);
            supportedLocales = supportedLocales ?? new List<Locale> {new Locale("en", "US")};
            this.navigatorKey = navigatorKey;
            this.home = home;
            this.theme = theme;
            this.routes = routes ?? new Dictionary<string, WidgetBuilder>();
            this.initialRoute = initialRoute;
            this.onGenerateRoute = onGenerateRoute;
            this.onGenerateInitialRoutes = onGenerateInitialRoutes;
            this.onUnknownRoute = onUnknownRoute;
            this.navigatorObservers = navigatorObservers ?? new List<NavigatorObserver>();
            this.builder = builder;
            this.title = title;
            this.onGenerateTitle = onGenerateTitle;
            this.color = color;
            this.locale = locale;
            this.localizationsDelegates = localizationsDelegates;
            this.localeListResolutionCallback = localeListResolutionCallback;
            this.localeResolutionCallback = localeResolutionCallback;
            this.supportedLocales = supportedLocales;
            this.showPerformanceOverlay = showPerformanceOverlay;
            this.showSemanticsDebugger = showSemanticsDebugger;
            this.debugShowCheckedModeBanner = debugShowCheckedModeBanner;
            this.shortcuts = shortcuts;
            this.actions = actions;
        }

        public readonly GlobalKey<NavigatorState> navigatorKey;
        public readonly Widget home;
        public readonly CupertinoThemeData theme;
        public readonly Dictionary<string, WidgetBuilder> routes;
        public readonly string initialRoute;
        public readonly RouteFactory onGenerateRoute;
        public readonly InitialRouteListFactory onGenerateInitialRoutes;
        public readonly RouteFactory onUnknownRoute;
        public readonly List<NavigatorObserver> navigatorObservers;
        public readonly TransitionBuilder builder;
        public readonly string title;
        public readonly GenerateAppTitle onGenerateTitle;
        public readonly Color color;
        public readonly Locale locale;
        public readonly List<LocalizationsDelegate<CupertinoLocalizations>> localizationsDelegates;
        public readonly LocaleListResolutionCallback localeListResolutionCallback;
        public readonly LocaleResolutionCallback localeResolutionCallback;
        public readonly List<Locale> supportedLocales;
        public readonly bool showPerformanceOverlay;
        public readonly bool checkerboardRasterCacheImages;
        public readonly bool checkerboardOffscreenLayers;
        public readonly bool showSemanticsDebugger;
        public readonly bool debugShowCheckedModeBanner;
     
        public readonly Dictionary<LogicalKeySet, Intent> shortcuts;
        public readonly Dictionary<LocalKey, ActionFactory> actions;

        public override State createState() {
            return new _CupertinoAppState();
        }

        public static HeroController createCupertinoHeroController() {
            return new HeroController();
        }
    }


    public class _AlwaysCupertinoScrollBehavior : ScrollBehavior {
        public override Widget buildViewportChrome(BuildContext context, Widget child, AxisDirection axisDirection) {
            return child;
        }

        public override ScrollPhysics getScrollPhysics(BuildContext context) {
            return new BouncingScrollPhysics();
        }
    }

    class _CupertinoAppState : State<CupertinoApp> {
        HeroController _heroController;

        public override void initState() {
            base.initState();
            _heroController = CupertinoApp.createCupertinoHeroController();
            _updateNavigator();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (widget.navigatorKey != ((CupertinoApp) oldWidget).navigatorKey) {
                _heroController = CupertinoApp.createCupertinoHeroController();
            }

            _updateNavigator();
        }

        List<NavigatorObserver> _navigatorObservers;

        void _updateNavigator() {
            if (widget.home != null ||
                widget.routes.isNotEmpty() || 
                widget.onGenerateRoute != null ||
                widget.onUnknownRoute != null) {
                _navigatorObservers = new List<NavigatorObserver>();
                foreach (var item in widget.navigatorObservers) {
                    _navigatorObservers.Add(item);
                }
            }
            else {
                _navigatorObservers = new List<NavigatorObserver>();
            }
        }
        
        List<LocalizationsDelegate> _localizationsDelegates {
            get {
                var _delegates = new List<LocalizationsDelegate>();
                if (widget.localizationsDelegates != null) {
                    _delegates.AddRange(widget.localizationsDelegates);
                }
              
                _delegates.Add(DefaultCupertinoLocalizations.del);
                return new List<LocalizationsDelegate>(_delegates);
                
            }
        }

        public override Widget build(BuildContext context) {
            CupertinoThemeData effectiveThemeData = widget.theme ?? new CupertinoThemeData();
            return new ScrollConfiguration(
                behavior: new _AlwaysCupertinoScrollBehavior(),
                child: new CupertinoUserInterfaceLevel(
                    data: CupertinoUserInterfaceLevelData.baselayer,
                    child: new CupertinoTheme(
                        data: effectiveThemeData,
                        child: new Builder(
                            builder: (BuildContext context1)=> {
                                return new WidgetsApp(
                                    key: new GlobalObjectKey<State<StatefulWidget>>(value: this),
                                    navigatorKey: widget.navigatorKey,
                                    navigatorObservers: _navigatorObservers,
                                    pageRouteBuilder:(RouteSettings settings, WidgetBuilder builder) =>
                                        new CupertinoPageRoute(settings: settings, builder: builder),
                                    home: widget.home,
                                    routes: widget.routes,
                                    initialRoute: widget.initialRoute,
                                    onGenerateRoute: widget.onGenerateRoute,
                                    onGenerateInitialRoutes: widget.onGenerateInitialRoutes,
                                    onUnknownRoute: widget.onUnknownRoute,
                                    builder: widget.builder,
                                    title: widget.title,
                                    onGenerateTitle: widget.onGenerateTitle,
                                    textStyle: CupertinoTheme.of(context1).textTheme.textStyle,
                                    color: CupertinoDynamicColor.resolve(widget.color ?? effectiveThemeData.primaryColor, context1),
                                    locale: widget.locale,
                                    localizationsDelegates: _localizationsDelegates,
                                    localeResolutionCallback: widget.localeResolutionCallback,
                                    localeListResolutionCallback: widget.localeListResolutionCallback,
                                    supportedLocales: widget.supportedLocales,
                                    showPerformanceOverlay: widget.showPerformanceOverlay,
                                    checkerboardRasterCacheImages: widget.checkerboardRasterCacheImages,
                                    checkerboardOffscreenLayers: widget.checkerboardOffscreenLayers,
                                    showSemanticsDebugger: widget.showSemanticsDebugger,
                                    debugShowCheckedModeBanner: widget.debugShowCheckedModeBanner,
                                    inspectorSelectButtonBuilder: (BuildContext context3, VoidCallback onPressed) => {
                                      return CupertinoButton.filled(
                                        child: new Icon(
                                          CupertinoIcons.search,
                                          size: 28.0f,
                                          color: CupertinoColors.white
                                        ),
                                        padding: EdgeInsets.zero,
                                        onPressed: onPressed
                                      );
                                    },
                                    shortcuts: widget.shortcuts,
                                    actions: widget.actions
                                );  
                            }
                        )
                    )
                )
            );
       }
    }
}