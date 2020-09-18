using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using RSG.Promises;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.widgets {
    public delegate Route RouteFactory(RouteSettings settings);

    public delegate bool RoutePredicate(Route route);

    public delegate IPromise<bool> WillPopCallback();

    public enum RoutePopDisposition {
        pop,
        doNotPop,
        bubble
    }

    public abstract class Route {
        public Route(RouteSettings settings = null) {
            this.settings = settings ?? new RouteSettings();
        }

        internal NavigatorState _navigator;

        public NavigatorState navigator {
            get { return _navigator; }
        }

        public readonly RouteSettings settings;

        public virtual List<OverlayEntry> overlayEntries {
            get { return new List<OverlayEntry>(); }
        }

        protected internal virtual void install(OverlayEntry insertionPoint) {
        }

        protected internal virtual TickerFuture didPush() {
            return TickerFutureImpl.complete();
        }

        protected internal virtual void didReplace(Route oldRoute) {
        }

        public virtual IPromise<RoutePopDisposition> willPop() {
            return Promise<RoutePopDisposition>.Resolved(isFirst
                ? RoutePopDisposition.bubble
                : RoutePopDisposition.pop);
        }

        public virtual bool willHandlePopInternally {
            get { return false; }
        }

        public virtual object currentResult {
            get { return null; }
        }

        public IPromise<object> popped {
            get { return _popCompleter; }
        }

        internal readonly Promise<object> _popCompleter = new Promise<object>();

        protected internal virtual bool didPop(object result) {
            didComplete(result);
            return true;
        }

        protected internal virtual void didComplete(object result) {
            _popCompleter.Resolve(result);
        }

        protected internal virtual void didPopNext(Route nextRoute) {
        }

        protected internal virtual void didChangeNext(Route nextRoute) {
        }

        protected internal virtual void didChangePrevious(Route previousRoute) {
        }

        protected internal virtual void changedInternalState() {
        }

        protected internal virtual void changedExternalState() {
        }

        protected internal virtual void dispose() {
            _navigator = null;
        }

        public bool isCurrent {
            get { return _navigator != null && _navigator._history.last() == this; }
        }

        public bool isFirst {
            get { return _navigator != null && _navigator._history.first() == this; }
        }

        public bool isActive {
            get { return _navigator != null && _navigator._history.Contains(this); }
        }
    }

    public class RouteSettings {
        public RouteSettings(string name = null, bool isInitialRoute = false, object arguments = null) {
            this.name = name;
            this.isInitialRoute = isInitialRoute;
            this.arguments = arguments;
        }

        RouteSettings copyWith(string name = null, bool? isInitialRoute = null, object arguments = null) {
            return new RouteSettings(
                name ?? this.name,
                isInitialRoute ?? this.isInitialRoute,
                arguments ?? this.arguments
            );
        }

        public readonly bool isInitialRoute;

        public readonly string name;

        public readonly object arguments;

        public override string ToString() {
            return $"{GetType()}(\"{name}\", {arguments})";
        }
    }

    public class NavigatorObserver {
        internal NavigatorState _navigator;

        public NavigatorState navigator {
            get { return _navigator; }
        }

        public virtual void didPush(Route route, Route previousRoute) {
        }

        public virtual void didPop(Route route, Route previousRoute) {
        }

        public virtual void didRemove(Route route, Route previousRoute) {
        }

        public virtual void didReplace(Route newRoute = null, Route oldRoute = null) {
        }

        public virtual void didStartUserGesture(Route route, Route previousRoute) {
        }

        public virtual void didStopUserGesture() {
        }
    }

    public class Navigator : StatefulWidget {
        public Navigator(
            Key key = null,
            string initialRoute = null,
            RouteFactory onGenerateRoute = null,
            RouteFactory onUnknownRoute = null,
            List<NavigatorObserver> observers = null) : base(key) {
            D.assert(onGenerateRoute != null);
            this.initialRoute = initialRoute;
            this.onGenerateRoute = onGenerateRoute;
            this.onUnknownRoute = onUnknownRoute;
            this.observers = observers ?? new List<NavigatorObserver>();
        }

        public readonly string initialRoute;

        public readonly RouteFactory onGenerateRoute;

        public readonly RouteFactory onUnknownRoute;

        public readonly List<NavigatorObserver> observers;

        public static readonly string defaultRouteName = "/";

        public static IPromise<object> pushNamed(BuildContext context, string routeName, object arguments = null) {
            return of(context).pushNamed(routeName, arguments: arguments);
        }

        public static IPromise<object> pushReplacementNamed(BuildContext context, string routeName,
            object result = null, object arguments = null) {
            return of(context).pushReplacementNamed(routeName, result: result, arguments: arguments);
        }

        public static IPromise<object> popAndPushNamed(BuildContext context, string routeName, object result = null,
            object arguments = null) {
            return of(context).popAndPushNamed(routeName, result: result, arguments: arguments);
        }

        public static IPromise<object> pushNamedAndRemoveUntil(BuildContext context, string newRouteName,
            RoutePredicate predicate, object arguments = null) {
            return of(context).pushNamedAndRemoveUntil(newRouteName, predicate, arguments: arguments);
        }

        public static IPromise<object> push(BuildContext context, Route route) {
            return of(context).push(route);
        }

        public static IPromise<object> pushReplacement(BuildContext context, Route newRoute, object result = null) {
            return of(context).pushReplacement(newRoute, result);
        }

        public static IPromise<object> pushAndRemoveUntil(BuildContext context, Route newRoute,
            RoutePredicate predicate) {
            return of(context).pushAndRemoveUntil(newRoute, predicate);
        }

        public static void replace(BuildContext context, Route oldRoute = null, Route newRoute = null) {
            D.assert(oldRoute != null);
            D.assert(newRoute != null);
            of(context).replace(oldRoute: oldRoute, newRoute: newRoute);
        }

        public static void replaceRouteBelow(BuildContext context, Route anchorRoute = null, Route newRoute = null) {
            D.assert(anchorRoute != null);
            D.assert(newRoute != null);
            of(context).replaceRouteBelow(anchorRoute: anchorRoute, newRoute: newRoute);
        }

        public static bool canPop(BuildContext context) {
            NavigatorState navigator = of(context, nullOk: true);
            return navigator != null && navigator.canPop();
        }

        public static IPromise<bool> maybePop(BuildContext context, object result = null) {
            return of(context).maybePop(result);
        }

        public static bool pop(BuildContext context, object result = null) {
            return of(context).pop(result);
        }

        public static void popUntil(BuildContext context, RoutePredicate predicate) {
            of(context).popUntil(predicate);
        }

        public static void removeRoute(BuildContext context, Route route) {
            of(context).removeRoute(route);
        }

        static void removeRouteBelow(BuildContext context, Route anchorRoute) {
            of(context).removeRouteBelow(anchorRoute);
        }

        public static NavigatorState of(
            BuildContext context,
            bool rootNavigator = false,
            bool nullOk = false
        ) {
            var navigator = rootNavigator
                ? (NavigatorState) context.rootAncestorStateOfType(new TypeMatcher<NavigatorState>())
                : (NavigatorState) context.ancestorStateOfType(new TypeMatcher<NavigatorState>());
            D.assert(() => {
                if (navigator == null && !nullOk) {
                    throw new UIWidgetsError(
                        "Navigator operation requested with a context that does not include a Navigator.\n" +
                        "The context used to push or pop routes from the Navigator must be that of a " +
                        "widget that is a descendant of a Navigator widget."
                    );
                }

                return true;
            });
            return navigator;
        }

        public override State createState() {
            return new NavigatorState();
        }
    }

    public class NavigatorState : TickerProviderStateMixin<Navigator> {
        readonly GlobalKey<OverlayState> _overlayKey = GlobalKey<OverlayState>.key();
        internal readonly List<Route> _history = new List<Route>();
        readonly HashSet<Route> _poppedRoutes = new HashSet<Route>();

        public readonly FocusScopeNode focusScopeNode = new FocusScopeNode();

        readonly List<OverlayEntry> _initialOverlayEntries = new List<OverlayEntry>();

        public override void initState() {
            base.initState();
            foreach (var observer in widget.observers) {
                D.assert(observer.navigator == null);
                observer._navigator = this;
            }

            var initialRouteName = widget.initialRoute ?? Navigator.defaultRouteName;
            if (initialRouteName.StartsWith("/") && initialRouteName.Length > 1) {
                initialRouteName = initialRouteName.Substring(1);
                D.assert(Navigator.defaultRouteName == "/");
                var plannedInitialRouteNames = new List<string> {
                    Navigator.defaultRouteName
                };
                var plannedInitialRoutes = new List<Route> {
                    _routeNamed(Navigator.defaultRouteName, arguments: null, true)
                };

                var routeParts = initialRouteName.Split('/');
                if (initialRouteName.isNotEmpty()) {
                    var routeName = "";
                    foreach (var part in routeParts) {
                        routeName += $"/{part}";
                        plannedInitialRouteNames.Add(routeName);
                        plannedInitialRoutes.Add(_routeNamed(routeName, arguments: null, true));
                    }
                }

                if (plannedInitialRoutes.Contains(null)) {
                    D.assert(() => {
                        UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                            exception: new Exception(
                                "Could not navigate to initial route.\n" +
                                $"The requested route name was: \"{initialRouteName}\n" +
                                "The following routes were therefore attempted:\n" +
                                $" * {string.Join("\n * ", plannedInitialRouteNames)}\n" +
                                "This resulted in the following objects:\n" +
                                $" * {string.Join("\n * ", plannedInitialRoutes)}\n" +
                                "One or more of those objects was null, and therefore the initial route specified will be " +
                                $"ignored and \"{Navigator.defaultRouteName}\" will be used instead.")));
                        return true;
                    });
                    push(_routeNamed(Navigator.defaultRouteName, arguments: null));
                }
                else {
                    plannedInitialRoutes.Each(route => { push(route); });
                }
            }
            else {
                Route route = null;
                if (initialRouteName != Navigator.defaultRouteName) {
                    route = _routeNamed(initialRouteName, arguments: null, true);
                }

                route = route ?? _routeNamed(Navigator.defaultRouteName, arguments: null);
                push(route);
            }

            foreach (var route in _history) {
                _initialOverlayEntries.AddRange(route.overlayEntries);
            }
        }


        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (((Navigator) oldWidget).observers != widget.observers) {
                foreach (var observer in ((Navigator) oldWidget).observers) {
                    observer._navigator = null;
                }

                foreach (var observer in widget.observers) {
                    D.assert(observer.navigator == null);
                    observer._navigator = this;
                }
            }

            foreach (var route in _history) {
                route.changedExternalState();
            }
        }

        public override void dispose() {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            foreach (var observer in widget.observers) {
                observer._navigator = null;
            }

            var doomed = _poppedRoutes.ToList();
            doomed.AddRange(_history);
            foreach (var route in doomed) {
                route.dispose();
            }

            _poppedRoutes.Clear();
            _history.Clear();
            focusScopeNode.detach();
            base.dispose();

            D.assert(() => {
                _debugLocked = false;
                return true;
            });
        }

        public OverlayState overlay {
            get { return _overlayKey.currentState; }
        }

        OverlayEntry _currentOverlayEntry {
            get {
                var route = _history.FindLast(r => r.overlayEntries.isNotEmpty());
                return route?.overlayEntries.last();
            }
        }

        bool _debugLocked;

        Route _routeNamed(string name, object arguments, bool allowNull = false) {
            D.assert(!_debugLocked);
            D.assert(name != null);
            var settings = new RouteSettings(
                name: name,
                isInitialRoute: _history.isEmpty(),
                arguments: arguments
            );

            var route = (Route) widget.onGenerateRoute(settings);
            if (route == null && !allowNull) {
                D.assert(() => {
                    if (widget.onUnknownRoute == null) {
                        throw new UIWidgetsError(
                            "If a Navigator has no onUnknownRoute, then its onGenerateRoute must never return null.\n" +
                            $"When trying to build the route \"{name}\", onGenerateRoute returned null, but there was no " +
                            "onUnknownRoute callback specified.\n" +
                            "The Navigator was:\n" +
                            $"  {this}");
                    }

                    return true;
                });

                route = (Route) widget.onUnknownRoute(settings);
                D.assert(() => {
                    if (route == null) {
                        throw new UIWidgetsError(
                            "A Navigator\'s onUnknownRoute returned null.\n" +
                            $"When trying to build the route \"{name}\", both onGenerateRoute and onUnknownRoute returned " +
                            "null. The onUnknownRoute callback should never return null.\n" +
                            "The Navigator was:\n" +
                            $"  {this}"
                        );
                    }

                    return true;
                });
            }

            return route;
        }

        public IPromise<object> pushNamed(string routeName, object arguments = null) {
            return push(_routeNamed(routeName, arguments: arguments));
        }

        public IPromise<object> pushReplacementNamed(string routeName, object result = null, object arguments = null) {
            return pushReplacement(_routeNamed(routeName, arguments: arguments), result);
        }

        public IPromise<object> popAndPushNamed(string routeName, object result = null, object arguments = null) {
            pop(result);
            return pushNamed(routeName, arguments: arguments);
        }

        public IPromise<object> pushNamedAndRemoveUntil(string newRouteName, RoutePredicate predicate,
            object arguments = null) {
            return pushAndRemoveUntil(_routeNamed(newRouteName, arguments: arguments), predicate);
        }

        public IPromise<object> push(Route route) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(route != null);
            D.assert(route._navigator == null);
            var oldRoute = _history.isNotEmpty() ? _history.last() : null;
            route._navigator = this;
            route.install(_currentOverlayEntry);
            _history.Add(route);
            route.didPush();
            route.didChangeNext(null);
            if (oldRoute != null) {
                oldRoute.didChangeNext(route);
                route.didChangePrevious(oldRoute);
            }

            foreach (var observer in widget.observers) {
                observer.didPush(route, oldRoute);
            }

            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation();
            return route.popped;
        }

        void _afterNavigation() {
            _cancelActivePointers();
        }

        public IPromise<object> pushReplacement(Route newRoute, object result = null) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            var oldRoute = _history.last();
            D.assert(oldRoute != null && oldRoute._navigator == this);
            D.assert(oldRoute.overlayEntries.isNotEmpty());
            D.assert(newRoute._navigator == null);
            D.assert(newRoute.overlayEntries.isEmpty());
            var index = _history.Count - 1;
            D.assert(index >= 0);
            D.assert(_history.IndexOf(oldRoute) == index);
            newRoute._navigator = this;
            newRoute.install(_currentOverlayEntry);
            _history[index] = newRoute;
            newRoute.didPush().whenCompleteOrCancel(() => {
                if (mounted) {
                    oldRoute.didComplete(result ?? oldRoute.currentResult);
                    oldRoute.dispose();
                }
            });
            newRoute.didChangeNext(null);
            if (index > 0) {
                _history[index - 1].didChangeNext(newRoute);
                newRoute.didChangePrevious(_history[index - 1]);
            }

            foreach (var observer in widget.observers) {
                observer.didReplace(newRoute: newRoute, oldRoute: oldRoute);
            }

            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation();
            return newRoute.popped;
        }

        public IPromise<object> pushAndRemoveUntil(Route newRoute, RoutePredicate predicate) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            var removedRoutes = new List<Route>();
            while (_history.isNotEmpty() && !predicate(_history.last())) {
                var removedRoute = _history.last();
                _history.RemoveAt(_history.Count - 1);
                D.assert(removedRoute != null && removedRoute._navigator == this);
                D.assert(removedRoute.overlayEntries.isNotEmpty());
                removedRoutes.Add(removedRoute);
            }

            D.assert(newRoute._navigator == null);
            D.assert(newRoute.overlayEntries.isEmpty());
            var oldRoute = _history.isNotEmpty() ? _history.last() : null;
            newRoute._navigator = this;
            newRoute.install(_currentOverlayEntry);
            _history.Add(newRoute);
            newRoute.didPush().whenCompleteOrCancel(() => {
                if (mounted) {
                    foreach (var route in removedRoutes) {
                        route.dispose();
                    }
                }
            });
            newRoute.didChangeNext(null);
            if (oldRoute != null) {
                oldRoute.didChangeNext(newRoute);
            }

            foreach (var observer in widget.observers) {
                observer.didPush(newRoute, oldRoute);
                foreach (var removedRoute in removedRoutes) {
                    observer.didRemove(removedRoute, oldRoute);
                }
            }

            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation();
            return newRoute.popped;
        }

        public void replace(Route oldRoute = null, Route newRoute = null) {
            D.assert(!_debugLocked);
            D.assert(oldRoute != null);
            D.assert(newRoute != null);
            if (oldRoute == newRoute) {
                return;
            }

            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(oldRoute._navigator == this);
            D.assert(newRoute._navigator == null);
            D.assert(oldRoute.overlayEntries.isNotEmpty());
            D.assert(newRoute.overlayEntries.isEmpty());
            D.assert(!overlay.debugIsVisible(oldRoute.overlayEntries.last()));
            var index = _history.IndexOf(oldRoute);
            D.assert(index >= 0);
            newRoute._navigator = this;
            newRoute.install(oldRoute.overlayEntries.last());
            _history[index] = newRoute;
            newRoute.didReplace(oldRoute);
            if (index + 1 < _history.Count) {
                newRoute.didChangeNext(_history[index + 1]);
                _history[index + 1].didChangePrevious(newRoute);
            }
            else {
                newRoute.didChangeNext(null);
            }

            if (index > 0) {
                _history[index - 1].didChangeNext(newRoute);
                newRoute.didChangePrevious(_history[index - 1]);
            }

            foreach (var observer in widget.observers) {
                observer.didReplace(newRoute: newRoute, oldRoute: oldRoute);
            }

            oldRoute.dispose();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
        }

        public void replaceRouteBelow(Route anchorRoute = null, Route newRoute = null) {
            D.assert(anchorRoute != null);
            D.assert(anchorRoute._navigator == this);
            D.assert(_history.IndexOf(anchorRoute) > 0);
            replace(_history[_history.IndexOf(anchorRoute) - 1], newRoute);
        }

        public bool canPop() {
            D.assert(_history.isNotEmpty);
            return _history.Count > 1 || _history[0].willHandlePopInternally;
        }

        public IPromise<bool> maybePop(object result = null) {
            var route = _history.last();
            D.assert(route._navigator == this);
            return route.willPop().Then(disposition => {
                if (disposition != RoutePopDisposition.bubble && mounted) {
                    if (disposition == RoutePopDisposition.pop) {
                        pop(result);
                    }

                    return true;
                }

                return false;
            });
        }

        public bool pop(object result = null) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            var route = _history.last();
            D.assert(route._navigator == this);
            var debugPredictedWouldPop = false;
            D.assert(() => {
                debugPredictedWouldPop = !route.willHandlePopInternally;
                return true;
            });
            if (route.didPop(result ?? route.currentResult)) {
                D.assert(debugPredictedWouldPop);
                if (_history.Count > 1) {
                    _history.removeLast();
                    if (route._navigator != null) {
                        _poppedRoutes.Add(route);
                    }

                    _history.last().didPopNext(route);
                    foreach (var observer in widget.observers) {
                        observer.didPop(route, _history.last());
                    }
                }
                else {
                    D.assert(() => {
                        _debugLocked = false;
                        return true;
                    });
                    return false;
                }
            }
            else {
                D.assert(!debugPredictedWouldPop);
            }

            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation();
            return true;
        }

        public void popUntil(RoutePredicate predicate) {
            while (!predicate(_history.last())) {
                pop();
            }
        }

        public void removeRoute(Route route) {
            D.assert(route != null);
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(route._navigator == this);
            var index = _history.IndexOf(route);
            D.assert(index != -1);
            var previousRoute = index > 0 ? _history[index - 1] : null;
            var nextRoute = index + 1 < _history.Count ? _history[index + 1] : null;
            _history.RemoveAt(index);
            previousRoute?.didChangeNext(nextRoute);
            nextRoute?.didChangePrevious(previousRoute);
            foreach (var observer in widget.observers) {
                observer.didRemove(route, previousRoute);
            }

            route.dispose();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation();
        }

        public void removeRouteBelow(Route anchorRoute) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(anchorRoute._navigator == this);
            var index = _history.IndexOf(anchorRoute) - 1;
            D.assert(index >= 0);
            var targetRoute = _history[index];
            D.assert(targetRoute._navigator == this);
            D.assert(targetRoute.overlayEntries.isEmpty() ||
                     !overlay.debugIsVisible(targetRoute.overlayEntries.last()));
            _history.RemoveAt(index);
            var nextRoute = index < _history.Count ? _history[index] : null;
            var previousRoute = index > 0 ? _history[index - 1] : null;
            if (previousRoute != null) {
                previousRoute.didChangeNext(nextRoute);
            }

            if (nextRoute != null) {
                nextRoute.didChangePrevious(previousRoute);
            }

            targetRoute.dispose();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
        }

        public void finalizeRoute(Route route) {
            _poppedRoutes.Remove(route);
            route.dispose();
        }

        int _userGesturesInProgress = 0;

        public bool userGestureInProgress {
            get { return _userGesturesInProgress > 0; }
        }

        public void didStartUserGesture() {
            _userGesturesInProgress += 1;
            if (_userGesturesInProgress == 1) {
                var route = _history.last();
                var previousRoute = !route.willHandlePopInternally && _history.Count > 1
                    ? _history[_history.Count - 2]
                    : null;

                foreach (var observer in widget.observers) {
                    observer.didStartUserGesture(route, previousRoute);
                }
            }
        }

        public void didStopUserGesture() {
            D.assert(_userGesturesInProgress > 0);
            _userGesturesInProgress -= 1;
            if (_userGesturesInProgress == 0) {
                foreach (var observer in widget.observers) {
                    observer.didStopUserGesture();
                }
            }
        }

        readonly HashSet<int> _activePointers = new HashSet<int>();

        void _handlePointerDown(PointerDownEvent evt) {
            _activePointers.Add(evt.pointer);
        }

        void _handlePointerUpOrCancel(PointerEvent evt) {
            _activePointers.Remove(evt.pointer);
        }

        void _cancelActivePointers() {
            // TODO flutter issue https://github.com/flutter/flutter/issues/4770
            if (SchedulerBinding.instance.schedulerPhase == SchedulerPhase.idle) {
                // If we're between frames (SchedulerPhase.idle) then absorb any
                // subsequent pointers from this frame. The absorbing flag will be
                // reset in the next frame, see build().
                var absorber = (RenderAbsorbPointer) _overlayKey.currentContext?
                    .ancestorRenderObjectOfType(new TypeMatcher<RenderAbsorbPointer>());
                setState(() => {
                    if (absorber != null) {
                        absorber.absorbing = true;
                    }
                });
            }

            foreach (var pointer in _activePointers) {
                WidgetsBinding.instance.cancelPointer(pointer);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(!_debugLocked);
            D.assert(_history.isNotEmpty());
            return new Listener(
                onPointerDown: _handlePointerDown,
                onPointerUp: _handlePointerUpOrCancel,
                onPointerCancel: _handlePointerUpOrCancel,
                child: new AbsorbPointer(
                    absorbing: false, // it's mutated directly by _cancelActivePointers above
                    child: new FocusScope(
                        focusScopeNode,
                        autofocus: true,
                        child: new Overlay(
                            key: _overlayKey,
                            initialEntries: _initialOverlayEntries
                        )
                    )
                )
            );
        }
    }
}