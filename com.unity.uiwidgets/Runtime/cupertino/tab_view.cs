using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoTabView : StatefulWidget {
        public CupertinoTabView(
            Key key = null,
            WidgetBuilder builder = null,
            GlobalKey<NavigatorState> navigatorKey = null,
            string defaultTitle = null,
            Dictionary<string, WidgetBuilder> routes = null,
            RouteFactory onGenerateRoute = null,
            RouteFactory onUnknownRoute = null,
            List<NavigatorObserver> navigatorObservers = null
        ) : base(key: key) {
            this.builder = builder;
            this.navigatorKey = navigatorKey;
            this.defaultTitle = defaultTitle;
            this.routes = routes;
            this.onGenerateRoute = onGenerateRoute;
            this.onUnknownRoute = onUnknownRoute;
            this.navigatorObservers = navigatorObservers ?? new List<NavigatorObserver>();
        }

        public readonly WidgetBuilder builder;

        public readonly GlobalKey<NavigatorState> navigatorKey;

        public readonly string defaultTitle;

        public readonly Dictionary<string, WidgetBuilder> routes;

        public readonly RouteFactory onGenerateRoute;

        public readonly RouteFactory onUnknownRoute;

        public readonly List<NavigatorObserver> navigatorObservers;

        public override State createState() {
            return new _CupertinoTabViewState();
        }
    }

    class _CupertinoTabViewState : State<CupertinoTabView> {
        HeroController _heroController;
        List<NavigatorObserver> _navigatorObservers;

        public override void initState() {
            base.initState();
            _heroController = CupertinoApp.createCupertinoHeroController();
            _updateObservers();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            CupertinoTabView _oldWidget = (CupertinoTabView) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (widget.navigatorKey != _oldWidget.navigatorKey
                || widget.navigatorObservers != _oldWidget.navigatorObservers) {
                _updateObservers();
            }
        }

        void _updateObservers() {
            _navigatorObservers =
                new List<NavigatorObserver>(widget.navigatorObservers);
            _navigatorObservers.Add(_heroController);
        }

        public override Widget build(BuildContext context) {
            return new Navigator(
                key: widget.navigatorKey,
                onGenerateRoute: _onGenerateRoute,
                onUnknownRoute: _onUnknownRoute,
                observers: _navigatorObservers
            );
        }

        Route _onGenerateRoute(RouteSettings settings) {
            string name = settings.name;
            WidgetBuilder routeBuilder = null;
            string title = null;
            if (name == Navigator.defaultRouteName && widget.builder != null) {
                routeBuilder = widget.builder;
                title = widget.defaultTitle;
            }
            else if (widget.routes != null) {
                routeBuilder = widget.routes[name];
            }

            if (routeBuilder != null) {
                return new CupertinoPageRoute(
                    builder: routeBuilder,
                    title: title,
                    settings: settings
                );
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
                        "Generators for routes are searched for in the following order:\n" +
                        " 1. For the \"/\" route, the \"builder\" property, if non-null, is used.\n" +
                        " 2. Otherwise, the \"routes\" table is used, if it has an entry for " +
                        "the route.\n" +
                        " 3. Otherwise, onGenerateRoute is called. It should return a " +
                        "non-null value for any valid route not handled by \"builder\" and \"routes\".\n" +
                        " 4. Finally if all else fails onUnknownRoute is called.\n" +
                        "Unfortunately, onUnknownRoute was not set."
                    );
                }

                return true;
            });

            Route result = widget.onUnknownRoute(settings);
            D.assert(() => {
                if (result == null) {
                    throw new UIWidgetsError(
                        "The onUnknownRoute callback returned null.\n" +
                        $"When the {GetType()} requested the route {settings} from its " +
                        "onUnknownRoute callback, the callback returned null. Such callbacks " +
                        "must never return null."
                    );
                }

                return true;
            });
            return result ;
        }
    }
}