using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    static class AppUtils
    {
        public static string homeRoute = "/";
        public static string snapshotRoute = "/snapshot";
    }
    
    class DevToolsApp : StatefulWidget {
        public DevToolsApp(List<DevToolsScreen<object>> screens, PreferencesController preferences)
        {
            this.screens = screens;
            this.preferences = preferences;
        }

        public readonly List<DevToolsScreen<object>> screens;
        public readonly PreferencesController preferences;

        public override State createState() => new DevToolsAppState();

    public static DevToolsAppState of(BuildContext context) {
        return context.findAncestorStateOfType<DevToolsAppState>();
    }
    }
    
    class DevToolsAppState : State<DevToolsApp> {
  List<Screen> _screens => widget.screens.Select(s => s.screen).ToList();

  PreferencesController preferences => widget.preferences;

  public override void initState() {
    base.initState();

    Globals.serviceManager.isolateManager.onSelectedIsolateChanged.listen((_) => {
      setState(() => {
        _clearCachedRoutes();
      });
    });
  }

  public override void didUpdateWidget(StatefulWidget oldWidget) {
    base.didUpdateWidget(oldWidget);
    _clearCachedRoutes();
  }

  /// Generates routes, separating the path from URL query parameters.
  Route _generateRoute(RouteSettings settings) {
    var uri = settings.name;
    var path = uri.isEmpty() ? AppUtils.homeRoute : uri;
    var args = settings.arguments as SnapshotArguments;

    // Provide the appropriate page route.
    if (routes.ContainsKey(path)) {
      WidgetBuilder builder = (context) => routes[path](
            context,
            uri.queryParameters,
            args
          );
      D.assert(() => {
        builder = (context) => new _AlternateCheckedModeBanner(
              builder: (subContext) => routes[path](
                subContext,
                uri.queryParameters,
                args
              )
            );
        return true;
      }());
      
      return new MaterialPageRoute(settings: settings, builder: builder);
    }

    // Return a page not found.
    return new MaterialPageRoute(
      settings: settings,
      builder: (BuildContext context) => {
        return DevToolsScaffold.withChild(
          child: new CenteredMessage("'$uri' not found.")
        );
      }
    );
  }

  /// The routes that the app exposes.
  Dictionary<string, UrlParametersBuilder> routes => {
    _routes = _routes ?? new Dictionary<string, UrlParametersBuilder>(){
      {AppUtils.homeRoute, (_, buildParam, __) => {
        if (buildParam.getOrDefault("uri")?.isNotEmpty() ?? false) {
          var embed = buildParam["embed"] == "true";
          var page = buildParam["page"];
          var tabs = embed && page != null
              ? _visibleScreens().Where((p) => p.screenId == page)
              : _visibleScreens();
          return new Initializer(
            url: buildParam["uri"],
            allowConnectionScreenOnDisconnect: !embed,
            builder: (_) => _providedControllers(
              child: new DevToolsScaffold(
                embed: embed,
                initialPage: page,
                tabs: tabs,
                actions: [
                  if (serviceManager.connectedApp.isFlutterAppNow) ...[
                    HotReloadButton(),
                    HotRestartButton(),
                  ],
                  OpenSettingsAction(),
                  OpenAboutAction(),
                ],
              ),
            ),
          );
        } else {
          return DevToolsScaffold.withChild(child: ConnectScreenBody());
        }
      },
      snapshotRoute: (_, __, args) => {
        return DevToolsScaffold.withChild(
          child: _providedControllers(
            offline: true,
            child: SnapshotScreenBody(args, _screens),
          ),
        );
      },
    }};

    return _routes;
  }

  Dictionary<string, UrlParametersBuilder> _routes;

  void _clearCachedRoutes() {
    _routes = null;
  }

  List<Screen> _visibleScreens() {
    var visibleScreens = new List<Screen>();
    foreach (var screen in _screens) {
      if (screen.conditionalLibrary != null) {
        if (Globals.serviceManager.isServiceAvailable &&
            Globals.serviceManager
                .isolateManager.selectedIsolateAvailable.isCompleted &&
            Globals.serviceManager.libraryUriAvailableNow(screen.conditionalLibrary)) {
          visibleScreens.Add(screen);
        }
      } else {
        visibleScreens.Add(screen);
      }
    }
    return visibleScreens;
  }

  Widget _providedControllers(Widget child, bool offline = false)
  {
    var _providers = widget.screens
      .Where((s) =>
        s.createController != null && (offline ? s.supportsOffline : true))
      .Select((s) => s.controllerProvider);

    return new MultiProvider(
      providers: _providers,
      child: child
    );
  }

  public override Widget build(BuildContext context) {
    return new ValueListenableBuilder<bool>(
      valueListenable: widget.preferences.darkModeTheme,
      builder: (subContext, value, _) => {
        return new MaterialApp(
          debugShowCheckedModeBanner: false,
          theme: ThemeUtils.themeFor(isDarkTheme: value),
          builder: (subsubContext, child) => new Notifications(child: child),
          onGenerateRoute: _generateRoute
        );
      }
    );
  }
}
    
    class DevToolsScreen<C> {
      public DevToolsScreen(
      Screen screen,
      Func<C> createController,
        bool supportsOffline = false
      )
      {
        this.screen = screen;
        this.createController = createController;
        this.supportsOffline = supportsOffline;
      }

    public readonly Screen screen;
    
    public readonly Func<C> createController;
    
    public readonly bool supportsOffline;

    internal Provider<C> controllerProvider => {
    D.assert(createController != null);
      return new Provider<C>(create: (_) => createController());
    }
}  
public delegate Widget UrlParametersBuilder(
    BuildContext buildContext,
    Dictionary<string, string> route,
    SnapshotArguments args
    );
}