using System;
using System.Collections.Generic;
using Unity.UIWidgets.DevTools.inspector;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{

    public delegate Widget UrlParametersBuilder(BuildContext buildContext, Dictionary<string, string> dictionary,
        SnapshotArguments args);

    static class AppUtils
    {
        public static string homeRoute = "/";
        public static string snapshotRoute = "/snapshot";

        public static List<DevToolsScreen<object>> defaultScreens
        {
            get
            {
                return new List<DevToolsScreen<object>>
                {
                    new DevToolsScreen<object>(
                        screen: new InspectorScreen(),
                        createController: null
                    )
                };
            }
        }
    }


    class DevToolsApp : StatefulWidget
    {
        public DevToolsApp(List<DevToolsScreen<object>> screens, PreferencesController preferences)
        {
            this.screens = screens;
            this.preferences = preferences;
        }

        public readonly List<DevToolsScreen<object>> screens;
        public readonly PreferencesController preferences;

        public override State createState() => new DevToolsAppState();

        public static DevToolsAppState of(BuildContext context)
        {
            return context.findAncestorStateOfType<DevToolsAppState>();
        }
    }

    class DevToolsAppState : State<DevToolsApp>
    {
        List<Screen> _screens
        {
            get
            {
                List<Screen> screensList = new List<Screen>();
                foreach (var screen in widget.screens)
                {
                    screensList.Add(screen.screen);
                }

                return screensList;
            }
        }

        Route _generateRoute(RouteSettings settings)
        {
            // var uri = new Uri(settings.name);
            // var path = uri.PathAndQuery.isEmpty() ? AppUtils.homeRoute : uri.PathAndQuery;
            var args = (SnapshotArguments) settings.arguments;
            
            var path = AppUtils.snapshotRoute;
            
            // Provide the appropriate page route.
            if (routes.ContainsKey(path))
            {
                WidgetBuilder builder = (context2) => routes[path](
                    context2,
                    null,
                    args
                );
                D.assert(() =>
                {
                    // builder = (context2) => _AlternateCheckedModeBanner(
                    //     builder: (context3) => routes[path](
                    //         context3,
                    //         null,
                    //         args
                    //     )
                    // );
                    return true;
                });
                return new MaterialPageRoute(settings: settings, builder: builder);
            }
            
            return new MaterialPageRoute(
                settings: settings,
                builder: (BuildContext context) => {
                return DevToolsScaffold.withChild(
                    child: new Container()
                );
            }
            );
            
            
           
        }

        Dictionary<string, UrlParametersBuilder> routes
        {

            get
            {
                if (_routes != null)
                {
                    return _routes;
                }

                Dictionary<string, UrlParametersBuilder> builders = new Dictionary<string, UrlParametersBuilder>();
                builders.Add(AppUtils.homeRoute, (_, _params, __) =>
                {
                    if (_params.ContainsKey("uri"))
                    {
                        var embed = _params["embed"] == "true";
                        var page = _params["page"];
                        return new Initializer(
                            url: _params["uri"].ToString(),
                            allowConnectionScreenOnDisconnect: !embed,
                            builder: null
                        );
                    }
                    
                    // return DevToolsScaffold.withChild(child: ConnectScreenBody());
                    return new Container(child: new Text("this is a text!"));
                });
                builders.Add(AppUtils.snapshotRoute, (_, __, args) =>
                {
                    return DevToolsScaffold.withChild(
                        child: _providedControllers(
                            offline: true,
                            child: new SnapshotScreenBody(args, _screens)
                        )
                    );
                });
                
                return builders;
            }

        }

        Dictionary<string, UrlParametersBuilder> _routes;

        Widget _providedControllers(Widget child = null, bool offline = false)
        {
            List<SingleChildWidgetMixinStatelessWidget> _providers = new List<SingleChildWidgetMixinStatelessWidget>();
            foreach (var screen in widget.screens)
            {
                if (screen.createController != null)
                {
                    _providers.Add((SingleChildWidgetMixinStatelessWidget)screen.controllerProvider);
                }
            }

            return new MultiProvider(
                providers: _providers,
                child: child
            );
        }
        
        
        
        public override Widget build(BuildContext context)
        {
            return new ValueListenableBuilder<bool>(
                valueListenable: widget.preferences.darkModeTheme,
                builder: (subContext, value, _) =>
                {
                    return new MaterialApp(
                        debugShowCheckedModeBanner: false,
                        theme: ThemeUtils.themeFor(isDarkTheme: true),
                        builder: (subsubContext, child) => new Notifications(child: child),
                        onGenerateRoute: _generateRoute
                    );
                }
            );
        }
    }



    class DevToolsScreen<C>
    {
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
        public Provider<C> controllerProvider {
            get
            {
                D.assert(createController != null);
                return new Provider<C>(create: (_) => createController());
            }
            
        }

    }
}