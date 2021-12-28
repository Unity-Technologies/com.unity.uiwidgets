using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.services;
using UnityEngine;
using SchedulerBinding = Unity.UIWidgets.scheduler.SchedulerBinding;
using SchedulerPhase = Unity.UIWidgets.scheduler.SchedulerPhase;

namespace Unity.UIWidgets.widgets {
    public delegate Route RouteFactory(RouteSettings settings);

    public delegate Route<T> RouteBuilder<T>(BuildContext context, RouteSettings settings);

    public delegate List<Route> RouteListFactory(NavigatorState navigator, string initialRoute);

    public delegate bool RoutePredicate(Route route);

    public delegate bool PopPageCallback(Route route, object result);

    public delegate Future<bool> WillPopCallback();



    public enum RoutePopDisposition {
        pop,
        doNotPop,
        bubble
    }

    public enum _RouteLifecycle {
        staging, // we will wait for transition delegate to decide what to do with this route.
        add, // we"ll want to run install, didAdd, etc; a route created by onGenerateInitialRoutes or by the initial widget.pages
        adding, // we"ll want to run install, didAdd, etc; a route created by onGenerateInitialRoutes or by the initial widget.pages

        // routes that are ready for transition.
        push, // we"ll want to run install, didPush, etc; a route added via push() and friends
        pushReplace, // we"ll want to run install, didPush, etc; a route added via pushReplace() and friends
        pushing, // we"re waiting for the future from didPush to complete
        replace, // we"ll want to run install, didReplace, etc; a route added via replace() and friends
        idle, // route is being harmless

        //
        // routes that are not present:
        //
        // routes that should be included in route announcement and should still listen to transition changes.
        pop, // we"ll want to call didPop
        remove, // we"ll want to run didReplace/didRemove etc

        // routes should not be included in route announcement but should still listen to transition changes.
        popping, // we"re waiting for the route to call finalizeRoute to switch to dispose
        removing, // we are waiting for subsequent routes to be done animating, then will switch to dispose

        // routes that are completely removed from the navigator and overlay.
        dispose, // we will dispose the route momentarily
        disposed, // we have disposed the route
    }

    public delegate bool _RouteEntryPredicate(_RouteEntry entry);

    public abstract class Route {
        public Route(RouteSettings settings = null) {
            _settings = settings ?? new RouteSettings();
        }

        internal NavigatorState _navigator;

        public NavigatorState navigator {
            get { return _navigator; }
        }

        public RouteSettings settings {
            get { return _settings; }
        }

        internal RouteSettings _settings;

        public void _updateSettings(RouteSettings newSettings) {
            D.assert(newSettings != null);
            if (_settings != newSettings) {
                _settings = newSettings;
                changedInternalState();
            }
        }

        public virtual List<OverlayEntry> overlayEntries {
            get { return new List<OverlayEntry>(); }
        }

        protected internal virtual void install() {
        }

        protected internal virtual TickerFuture didPush() {
            var future = TickerFuture.complete();
            future.then(_ => { navigator?.focusScopeNode?.requestFocus(); });
            return future;
        }

        protected internal virtual void didAdd() {

            TickerFuture.complete().then(_ => { navigator.focusScopeNode.requestFocus(); });
        }

        

        public virtual Future<RoutePopDisposition> willPop() {
            return Future.value(isFirst
            ? RoutePopDisposition.bubble
            : RoutePopDisposition.pop).to<RoutePopDisposition>();
        }

        public virtual bool willHandlePopInternally {
            get { return false; }
        }

        public virtual object currentResult {
            get { return null; }
        }
        public virtual Future popped {
            get { return _popCompleter.future; }
        }
        protected internal virtual bool didPop(object result) {
            didComplete(result);
            return true;
        }
        protected internal virtual void didComplete(object result ) {
            _popCompleter.complete(FutureOr.value(result ?? currentResult));
        }
       

        internal readonly Completer _popCompleter = Completer.create();

        protected internal virtual void didReplace(Route oldRoute) {
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

        public virtual void dispose() {
            _navigator = null;
        }

        public bool isCurrent {
            get {
                if (_navigator == null)
                    return false;
                _RouteEntry currentRouteEntry = null;
                foreach (var historyEntry in _navigator._history) {
                    if ( _RouteEntry.isPresentPredicate(historyEntry)) {
                        currentRouteEntry = historyEntry;
                    }
                }
                if (currentRouteEntry == null)
                    return false;
                return currentRouteEntry.route == this;
                
            }
        }

        public bool isFirst {
            get {
                if (_navigator == null)
                    return false;
                _RouteEntry currentRouteEntry = null;
                foreach (var historyEntry in _navigator._history) {
                    if ( _RouteEntry.isPresentPredicate(historyEntry)) {
                        currentRouteEntry = historyEntry;
                        break;
                    }
                }
                if (currentRouteEntry == null)
                    return false;
                return currentRouteEntry.route == this;
               
            }
        }

        public bool isActive {
            get {
                if (_navigator == null)
                    return false;
                _RouteEntry routeEntry = null;
                foreach (var historyEntry in _navigator._history) {
                     if (_RouteEntry.isRoutePredicate(this)(historyEntry)) {
                         routeEntry = historyEntry;
                         break;
                     }
                }
                return routeEntry?.isPresent == true;
            }
        }
    }

    public class Route<T> : Route {
        public Route(RouteSettings settings = null) {
            _settings = settings ?? new RouteSettings();
        }
        public override object currentResult {
            get { return default(T); }
        }
        public override Future popped {
            get { return _popCompleter.future.to<T>(); }
        }
        
        protected internal virtual bool didPop(T result) {
            didComplete(result);
            return true;
        }
        protected internal void didComplete(T result ) {
            if (default(T) == null) {
                _popCompleter.complete(FutureOr.value(currentResult));
            }
            else {
                _popCompleter.complete(FutureOr.value(result));
            }
            
        }
    }

    public class RouteSettings {
        public RouteSettings(
            string name = null,
            object arguments = null) {
            this.name = name;
            this.arguments = arguments;
        }

        RouteSettings copyWith(
            string name = null, 
            
            object arguments = null) {
            return new RouteSettings(
                name ?? this.name,
                arguments ?? this.arguments
            );
        }


        public readonly string name;

        public readonly object arguments;

        public override string ToString() {
            return $"{GetType()}(\"{name}\", {arguments})";
        }
    }

    public abstract class Page : RouteSettings {
        public Page(
            Key key = null,
            string name = null,
            object arguments = null
        ) : base(name: name, arguments: arguments) {
            this.key = (LocalKey) key;
        }

        public LocalKey key;

        public bool canUpdate(Page other) {
            return other.GetType() == GetType() &&
                   other.key == key;
        }
        public override string ToString() {
            return $"{GetType()}(\"{name}\",{key},{arguments})";
        }
    }

    public abstract class Page<T> : Page {
        public Page(
            Key key = null,
            string name = null,
            object arguments = null
        ) : base(name: name, arguments: arguments) {
            this.key = (LocalKey) key;
        }
        public abstract Route<T> createRoute(BuildContext context);
    }

    public class CustomBuilderPage<T> : Page<T> {
        public CustomBuilderPage(
            LocalKey key,
            RouteBuilder<T> routeBuilder,
            string name = null,
            object arguments = null
        ) : base(key: key, name: name, arguments: arguments) {
            this.routeBuilder = routeBuilder;
        }


        public readonly RouteBuilder<T> routeBuilder;


        public override Route<T> createRoute(BuildContext context) {
            Route<T> route = routeBuilder(context, this);
            D.assert(route.settings == this);
            return route;
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


    public abstract class RouteTransitionRecord {

        public abstract Route route { get; }

        public abstract bool isEntering { get; }

        public bool _debugWaitingForExitDecision = false;
        public abstract void markForPush();
        public abstract void markForAdd();
        public abstract void markForPop(object result = null);
        public abstract void markForComplete(object result = null);
        public abstract void markForRemove();
    }

    public abstract class TransitionDelegate<T> {
        public TransitionDelegate() {
        }

        public IEnumerable<RouteTransitionRecord> _transition(
            List<RouteTransitionRecord> newPageRouteHistory = null,
            Dictionary<RouteTransitionRecord, RouteTransitionRecord> locationToExitingPageRoute = null,
            Dictionary<RouteTransitionRecord, List<RouteTransitionRecord>> pageRouteToPagelessRoutes = null
        ) {
            IEnumerable<RouteTransitionRecord> results = resolve(
                newPageRouteHistory: newPageRouteHistory,
                locationToExitingPageRoute: locationToExitingPageRoute,
                pageRouteToPagelessRoutes: pageRouteToPagelessRoutes
            );

            D.assert(() => {
                List<RouteTransitionRecord> resultsToVerify = results.ToList();
                HashSet<RouteTransitionRecord> exitingPageRoutes = new HashSet<RouteTransitionRecord>();
                foreach(RouteTransitionRecord item in locationToExitingPageRoute.Values) {
                    exitingPageRoutes.Add(item);
                }
                foreach (RouteTransitionRecord exitingPageRoute in exitingPageRoutes) {
                    D.assert(!exitingPageRoute._debugWaitingForExitDecision);
                    if (pageRouteToPagelessRoutes.ContainsKey(exitingPageRoute)) {
                        foreach (RouteTransitionRecord pagelessRoute in pageRouteToPagelessRoutes[exitingPageRoute]) {
                            D.assert(!pagelessRoute._debugWaitingForExitDecision);
                        }
                    }
                }

                int indexOfNextRouteInNewHistory = 0;

                foreach (_RouteEntry routeEntry in resultsToVerify.Cast<_RouteEntry>()) {
                    D.assert(routeEntry != null);
                    D.assert(!routeEntry.isEntering && !routeEntry._debugWaitingForExitDecision);
                    if (
                        indexOfNextRouteInNewHistory >= newPageRouteHistory.Count ||
                        routeEntry != newPageRouteHistory[indexOfNextRouteInNewHistory]
                    ) {
                        D.assert(exitingPageRoutes.Contains(routeEntry));
                        exitingPageRoutes.Remove(routeEntry);
                    }
                    else {
                        indexOfNextRouteInNewHistory += 1;
                    }
                }

                D.assert(
                    indexOfNextRouteInNewHistory == newPageRouteHistory.Count &&
                    exitingPageRoutes.isEmpty()
                );
                return true;
            });

            return results;
        }

        public abstract IEnumerable<RouteTransitionRecord> resolve(
            List<RouteTransitionRecord> newPageRouteHistory = null,
            Dictionary<RouteTransitionRecord, RouteTransitionRecord> locationToExitingPageRoute = null,
            Dictionary<RouteTransitionRecord, List<RouteTransitionRecord>> pageRouteToPagelessRoutes = null
        );
    }


    public class DefaultTransitionDelegate<T> : TransitionDelegate<T> {
        public DefaultTransitionDelegate() : base() {
        }

        public override IEnumerable<RouteTransitionRecord> resolve(
            List<RouteTransitionRecord> newPageRouteHistory = null,
            Dictionary<RouteTransitionRecord, RouteTransitionRecord> locationToExitingPageRoute = null,
            Dictionary<RouteTransitionRecord, List<RouteTransitionRecord>> pageRouteToPagelessRoutes = null) {
            
            List<RouteTransitionRecord> results = new List<RouteTransitionRecord>();
            
            void handleExitingRoute(RouteTransitionRecord location, bool isLast) {
                RouteTransitionRecord exitingPageRoute = locationToExitingPageRoute[location];
                if (exitingPageRoute == null)
                    return;
                D.assert(exitingPageRoute._debugWaitingForExitDecision);
                bool hasPagelessRoute = pageRouteToPagelessRoutes.ContainsKey(exitingPageRoute);
                bool isLastExitingPageRoute = isLast && !locationToExitingPageRoute.ContainsKey(exitingPageRoute);
                if (isLastExitingPageRoute && !hasPagelessRoute) {
                    exitingPageRoute.markForPop(exitingPageRoute.route.currentResult);
                }
                else {
                    exitingPageRoute.markForComplete(exitingPageRoute.route.currentResult);
                }

                results.Add(exitingPageRoute);

                if (hasPagelessRoute) {
                    List<RouteTransitionRecord> pagelessRoutes = pageRouteToPagelessRoutes[exitingPageRoute];
                    foreach (RouteTransitionRecord pagelessRoute in pagelessRoutes) {
                        D.assert(pagelessRoute._debugWaitingForExitDecision);
                        if (isLastExitingPageRoute && pagelessRoute == pagelessRoutes.Last()) {
                            pagelessRoute.markForPop(pagelessRoute.route.currentResult);
                        }
                        else {
                            pagelessRoute.markForComplete(pagelessRoute.route.currentResult);
                        }
                    }
                }

                // It is possible there is another exiting route above this exitingPageRoute.
                handleExitingRoute(exitingPageRoute, isLast);
            }

            handleExitingRoute(null, newPageRouteHistory.isEmpty());

            foreach (RouteTransitionRecord pageRoute in newPageRouteHistory) {
                bool isLastIteration = newPageRouteHistory.Last() == pageRoute;
                if (pageRoute.isEntering) {
                    if (!locationToExitingPageRoute.ContainsKey(pageRoute) && isLastIteration) {
                        pageRoute.markForPush();
                    }
                    else {
                        pageRoute.markForAdd();
                    }
                }

                results.Add(pageRoute);
                handleExitingRoute(pageRoute, isLastIteration);
            }

            return results;
        }
    }


    public class Navigator : StatefulWidget {
        public Navigator(
            Key key = null,
            List<Page<object>> pages = null,
            PopPageCallback onPopPage = null,
            string initialRoute = null,
            RouteListFactory onGenerateInitialRoutes = null,
            RouteFactory onGenerateRoute = null,
            RouteFactory onUnknownRoute = null,
            TransitionDelegate<object> transitionDelegate = null,
            List<NavigatorObserver> observers = null) : base(key) {

            this.pages = pages ?? new List<Page<object>>();
            this.onPopPage = onPopPage;
            this.initialRoute = initialRoute;
            this.onGenerateInitialRoutes = onGenerateInitialRoutes ?? defaultGenerateInitialRoutes;
            this.onGenerateRoute = onGenerateRoute;
            this.onUnknownRoute = onUnknownRoute;
            this.transitionDelegate = transitionDelegate ?? new DefaultTransitionDelegate<object>();
            this.observers = observers ?? new List<NavigatorObserver>();
            
        }

        public readonly List<Page<object>> pages;

        public readonly PopPageCallback onPopPage;

        public readonly TransitionDelegate<object> transitionDelegate;

        public readonly string initialRoute;

        public readonly RouteListFactory onGenerateInitialRoutes;

        public readonly RouteFactory onGenerateRoute;

        public readonly RouteFactory onUnknownRoute;

        public readonly List<NavigatorObserver> observers;

        public static readonly string defaultRouteName = "/";


        public static Future<T> pushNamed<T>(BuildContext context, string routeName, object arguments = null) {
            return of(context).pushNamed<T>(routeName, arguments: arguments);
        }
        
        public static Future pushNamed(BuildContext context, string routeName, object arguments = null) {
            return of(context).pushNamed(routeName, arguments: arguments);
        }

        public static Future<T> pushReplacementNamed<T,TO>(BuildContext context, string routeName,
            TO result = default , object arguments = null) {
            return of(context).pushReplacementNamed<T,TO>(routeName, arguments: arguments,result: result);
        }

        public static Future pushReplacementNamed(BuildContext context, string routeName,
            object result = default , object arguments = null) {
            return of(context).pushReplacementNamed(routeName, arguments: arguments,result: result);
        }

        public static Future<T> popAndPushNamed<T,TO>(BuildContext context, string routeName,
            TO result = default,
            object arguments = null) {
            return of(context).popAndPushNamed<T,TO>(routeName, result: result, arguments: arguments);
        }

        public static Future<T> pushNamedAndRemoveUntil<T>(BuildContext context, string newRouteName,
            RoutePredicate predicate, object arguments = null) {
            return of(context).pushNamedAndRemoveUntil<T>(newRouteName, predicate, arguments: arguments);
        }

        public static Future<T> push<T>(BuildContext context, Route route) {
            return of(context).push<T>(route);
        }

        public static Future<T> pushReplacement<T,TO>(BuildContext context, Route<T> newRoute, TO result = default ) {
            return of(context).pushReplacement<T,TO>(newRoute, result);
        }

       
        public static Future<T> pushAndRemoveUntil<T>(BuildContext context, Route<T> newRoute, RoutePredicate predicate) {
            return of(context).pushAndRemoveUntil<T>(newRoute, predicate);
         }

        public static void replace<T>(BuildContext context,  Route oldRoute = null,  Route<T> newRoute = null) {
             of(context).replace<T>(oldRoute: oldRoute, newRoute: newRoute);
        }

        public static void replaceRouteBelow<T>(BuildContext context,  Route anchorRoute = null, Route<T> newRoute = null ) {
            of(context).replaceRouteBelow<T>(anchorRoute: anchorRoute, newRoute: newRoute);
          }

        public static bool canPop(BuildContext context) {
            NavigatorState navigator = of(context, nullOk: true);
            return navigator != null && navigator.canPop();
        }
        public static Future<bool> maybePop<T>(BuildContext context,  T result = default(T)) {
            return of(context).maybePop<T>(result);
        }
        public static Future<bool> maybePop(BuildContext context,  object result = null) {
            return of(context).maybePop(result);
        }

        public static void pop<T>(BuildContext context, T result = default(T) ) {
            of(context).pop<T>(result);
        }
        
        public static void pop(BuildContext context, object result = null ) {
            of(context).pop(result);
        }


        public static void popUntil(BuildContext context, RoutePredicate predicate) {
            of(context).popUntil(predicate);
          }

        public static void removeRoute(BuildContext context, Route route) {
             of(context).removeRoute(route);
        }

          
        public static void removeRouteBelow(BuildContext context, Route anchorRoute) {
             of(context).removeRouteBelow(anchorRoute);
          }
        
        public static NavigatorState of(
            BuildContext context,
            bool rootNavigator = false,
            bool nullOk = false
        ) {
            NavigatorState navigator = rootNavigator
                ? context.findRootAncestorStateOfType<NavigatorState>()
                : context.findAncestorStateOfType<NavigatorState>();
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

        public static List<Route> defaultGenerateInitialRoutes(NavigatorState navigator, string initialRouteName) {
            List<Route> result = new List<Route>();
            if (initialRouteName.StartsWith("/") && initialRouteName.Length > 1) {
                initialRouteName = initialRouteName.Substring(1); // strip leading "/"
                D.assert(defaultRouteName == "/");
                List<string> debugRouteNames = new List<string>();
                D.assert(() => {
                    debugRouteNames = new List<string> {defaultRouteName};
                    return true;
                });
                result.Add(navigator._routeNamed(defaultRouteName, arguments: null, allowNull: true));
                string[] routeParts = initialRouteName.Split('/');
                if (initialRouteName.isNotEmpty()) {
                    string routeName = "";
                    foreach (string part in routeParts) {
                        routeName += $"/{part}";
                        D.assert(() => {
                            debugRouteNames.Add(routeName);
                            return true;
                        });
                        result.Add(navigator._routeNamed(routeName, arguments: null, allowNull: true));
                    }
                }

                if (result.Last() == null) {
                    D.assert(() => {
                        throw new UIWidgetsError(
                            "Could not navigate to initial route.\n" +
                            $"The requested route name was: {initialRouteName} \n " +
                            "There was no corresponding route in the app, and therefore the initial route specified will be " +
                            $"ignored and {defaultRouteName} will be used instead."
                        );
                    });
                    result.Clear();
                }
            }
            else if (initialRouteName != defaultRouteName) {
                result.Add(navigator._routeNamed(initialRouteName, arguments: null, allowNull: true));
            }

            //result.RemoveWhere((Route<object> route) => route == null);
            foreach (var resultRoute in result) {
                if (resultRoute == null) {
                    result.Remove(resultRoute);
                }
            }
            if (result.isEmpty())
                result.Add(
                    navigator._routeNamed(
                        name:defaultRouteName,
                        arguments: null));
            return result;
        }



        public override State createState() {
            return new NavigatorState();
        }
    }

    public class _RouteEntry : RouteTransitionRecord {
        public _RouteEntry(
            Route route,
            _RouteLifecycle initialState
        ) {
            D.assert(
                initialState == _RouteLifecycle.staging ||
                initialState == _RouteLifecycle.add ||
                initialState == _RouteLifecycle.push ||
                initialState == _RouteLifecycle.pushReplace ||
                initialState == _RouteLifecycle.replace
            );
            this.route = route;
            currentState = initialState;
        }
        
        public _RouteLifecycle currentState;
        public Route lastAnnouncedPreviousRoute; // last argument to Route.didChangePrevious
        public Route lastAnnouncedPoppedNextRoute; // last argument to Route.didPopNext
        public Route lastAnnouncedNextRoute; // last argument to Route.didChangeNext

        public bool hasPage {
            get { return route.settings is Page; }
        }

        public bool canUpdateFrom(Page page) {
            if (currentState.GetHashCode() > _RouteLifecycle.idle.GetHashCode())
                return false;
            if (!hasPage)
                return false;
            Page routePage = route.settings as Page;
            return page.canUpdate(routePage);
        }

        public void handleAdd(NavigatorState navigator) {
            D.assert(currentState == _RouteLifecycle.add);
            D.assert(navigator != null);
            D.assert(navigator._debugLocked);
            D.assert(route._navigator == null);
            route._navigator = navigator;
            route.install();
            D.assert(route.overlayEntries.isNotEmpty());
            currentState = _RouteLifecycle.adding;
        }

        public void handlePush(NavigatorState navigator, bool isNewFirst, Route previous = null,
            Route previousPresent = null) {
            D.assert(currentState == _RouteLifecycle.push || currentState == _RouteLifecycle.pushReplace ||
                     currentState == _RouteLifecycle.replace);
            D.assert(navigator != null);
            D.assert(navigator._debugLocked);
            D.assert(route._navigator == null);
            _RouteLifecycle previousState = currentState;
            route._navigator = navigator;
            route.install();
            D.assert(route.overlayEntries.isNotEmpty());
            if (currentState == _RouteLifecycle.push || currentState == _RouteLifecycle.pushReplace) {
                TickerFuture routeFuture = route.didPush();
                currentState = _RouteLifecycle.pushing;
                routeFuture.whenCompleteOrCancel(() => {
                    if (currentState == _RouteLifecycle.pushing) {
                        currentState = _RouteLifecycle.idle;
                        D.assert(!navigator._debugLocked);
                        D.assert(() => {
                            navigator._debugLocked = true;
                            return true;
                        });
                        navigator._flushHistoryUpdates();
                        D.assert(() => {
                            navigator._debugLocked = false;
                            return true;
                        });
                    }
                });
            }
            else {
                D.assert(currentState == _RouteLifecycle.replace);
                route.didReplace(previous);
                currentState = _RouteLifecycle.idle;
            }

            if (isNewFirst) {
                route.didChangeNext(null);
            }

            if (previousState == _RouteLifecycle.replace || previousState == _RouteLifecycle.pushReplace) {
                foreach (NavigatorObserver observer in navigator.widget.observers)
                    observer.didReplace(newRoute: route, oldRoute: previous);
            }
            else {
                D.assert(previousState == _RouteLifecycle.push);
                foreach (NavigatorObserver observer in navigator.widget.observers)
                    observer.didPush(route, previousPresent);
            }
        }

        public void handleDidPopNext(Route poppedRoute) {
            route.didPopNext(poppedRoute);
            lastAnnouncedPoppedNextRoute = poppedRoute;
        }

        public void handlePop(NavigatorState navigator , Route previousPresent = null) {
            D.assert(navigator != null);
            D.assert(navigator._debugLocked);
            D.assert(route._navigator == navigator);
            currentState = _RouteLifecycle.popping;
            foreach (NavigatorObserver observer in navigator.widget.observers)
                observer.didPop(route, previousPresent);
        }

        public void handleRemoval(NavigatorState navigator, Route previousPresent = null) {
            D.assert(navigator != null);
            D.assert(navigator._debugLocked);
            D.assert(route._navigator == navigator);
            currentState = _RouteLifecycle.removing;
            if (_reportRemovalToObserver) {
                foreach (NavigatorObserver observer in navigator.widget.observers)
                    observer.didRemove(route, previousPresent);
            }
        }

        public bool doingPop = false;

        public void didAdd(
            NavigatorState navigator = null, 
            bool isNewFirst = false, 
            Route previous = null, 
            Route previousPresent = null) {
            
            route.didAdd();
            currentState = _RouteLifecycle.idle;
            if (isNewFirst) {
                route.didChangeNext(null);
            }

            foreach (NavigatorObserver observer in navigator.widget.observers)
                observer.didPush(route, previousPresent);
        }

        public void pop<T>(T result) {
            D.assert(isPresent);
            doingPop = true;
            if (route.didPop(result) && doingPop) {
                currentState = _RouteLifecycle.pop;
            }

            doingPop = false;
        }

        bool _reportRemovalToObserver = true;

       
        public void remove(bool isReplaced = false) {
            D.assert(
                !hasPage || _debugWaitingForExitDecision, () =>
                    "A page-based route cannot be completed using imperative api, provide a " +
                    "new list without the corresponding Page to Navigator.pages instead. "
            );
            if (currentState.GetHashCode() >= _RouteLifecycle.remove.GetHashCode())
                return;
            D.assert(isPresent);
            _reportRemovalToObserver = !isReplaced;
            currentState = _RouteLifecycle.remove;
        }

        
        public void complete<T>(T result, bool isReplaced = false) {
            D.assert(
                !hasPage || _debugWaitingForExitDecision, () =>
                    "A page-based route cannot be completed using imperative api, provide a " +
                    "new list without the corresponding Page to Navigator.pages instead. "
            );
            if (currentState.GetHashCode() >= _RouteLifecycle.remove.GetHashCode())
                return;
            D.assert(isPresent);
            _reportRemovalToObserver = !isReplaced;
            route.didComplete(result);
            D.assert(route._popCompleter.isCompleted); // implies didComplete was called
            currentState = _RouteLifecycle.remove;
        }

        public void finalize() {
            D.assert(currentState.GetHashCode() < _RouteLifecycle.dispose.GetHashCode());
            currentState = _RouteLifecycle.dispose;
        }

        public void dispose() {
            D.assert(currentState.GetHashCode() < _RouteLifecycle.disposed.GetHashCode());
            ((Route)route).dispose();
            currentState = _RouteLifecycle.disposed;
        }

        bool willBePresent {
            get {
                return currentState.GetHashCode() <= _RouteLifecycle.idle.GetHashCode() &&
                       currentState.GetHashCode() >= _RouteLifecycle.add.GetHashCode();
            }
        }

        public bool isPresent {

            get {
                return currentState.GetHashCode() <= _RouteLifecycle.remove.GetHashCode() &&
                       currentState.GetHashCode() >= _RouteLifecycle.add.GetHashCode();
            }
        }

        public bool suitableForAnnouncement {
            get {
                return currentState.GetHashCode() <= _RouteLifecycle.removing.GetHashCode() &&
                       currentState.GetHashCode() >= _RouteLifecycle.push.GetHashCode();
            }
        }

        bool suitableForTransitionAnimation {
            get {
                return currentState.GetHashCode() <= _RouteLifecycle.remove.GetHashCode() &&
                       currentState.GetHashCode() >= _RouteLifecycle.push.GetHashCode();
            }

        }

        public bool shouldAnnounceChangeToNext(Route nextRoute) {
            D.assert(nextRoute != lastAnnouncedNextRoute);
            return !(
                nextRoute == null &&
                lastAnnouncedPoppedNextRoute != null &&
                lastAnnouncedPoppedNextRoute == lastAnnouncedNextRoute
            );
        }

        public readonly static _RouteEntryPredicate isPresentPredicate = (_RouteEntry entry) => entry.isPresent;

        public readonly static _RouteEntryPredicate suitableForTransitionAnimationPredicate =
            (_RouteEntry entry) => entry.suitableForTransitionAnimation;

        public readonly static _RouteEntryPredicate willBePresentPredicate = (_RouteEntry entry) => entry.willBePresent;

        public static _RouteEntryPredicate isRoutePredicate(Route route) {
            return (_RouteEntry entry) => entry.route == route;
        }

        public override Route route { get; }

        public override bool isEntering {
            get { return currentState == _RouteLifecycle.staging; }
        }

        public override void markForPush() {
            D.assert(
                isEntering && !_debugWaitingForExitDecision, () =>
                    "This route cannot be marked for push. Either a decision has already been " +
                    "made or it does not require an explicit decision on how to transition in."
            );
            currentState = _RouteLifecycle.push;
        }


        public override void markForAdd() {
            D.assert(
                isEntering && !_debugWaitingForExitDecision, () =>
                    "This route cannot be marked for add. Either a decision has already been " +
                    "made or it does not require an explicit decision on how to transition in."
            );
            currentState = _RouteLifecycle.add;
        }


        public override void markForPop(object result = null) {
            D.assert(
                !isEntering && _debugWaitingForExitDecision, () =>
                    "This route cannot be marked for pop. Either a decision has already been " +
                    "made or it does not require an explicit decision on how to transition out."
            );
            pop(result);
            _debugWaitingForExitDecision = false;
        }


        public override void markForComplete(object result = null) {
            D.assert(
                !isEntering && _debugWaitingForExitDecision, () =>
                    "This route cannot be marked for complete. Either a decision has already " +
                    "been made or it does not require an explicit decision on how to transition " +
                    "out."
            );
            complete(result);
            _debugWaitingForExitDecision = false;
        }


        public override void markForRemove() {
            D.assert(
                !isEntering && _debugWaitingForExitDecision, () =>
                    "This route cannot be marked for remove. Either a decision has already " +
                    "been made or it does not require an explicit decision on how to transition " +
                    "out."
            );
            remove();
            _debugWaitingForExitDecision = false;
        }
    }

    public class NavigatorState : TickerProviderStateMixin<Navigator> {
        public readonly GlobalKey<OverlayState> _overlayKey = GlobalKey<OverlayState>.key();
        public List<_RouteEntry> _history = new List<_RouteEntry>();
        public readonly FocusScopeNode focusScopeNode = new FocusScopeNode(debugLabel: "Navigator Scope");
        public bool _debugLocked = false; // used to prevent re-entrant calls to push, pop, and friends

        public override void initState() {
            base.initState();
            D.assert(
                widget.pages.isEmpty() || widget.onPopPage != null, () =>
                    "The Navigator.onPopPage must be provided to use the Navigator.pages API"
            );
            foreach (NavigatorObserver observer in widget.observers) {
                D.assert(observer.navigator == null);
                observer._navigator = this;
            }

            string initialRoute = widget.initialRoute;
            if (widget.pages.isNotEmpty()) {
                foreach (Page<object> page in widget.pages) {
                    _history.Add(
                            new _RouteEntry(
                                page.createRoute(context),
                                initialState: _RouteLifecycle.add
                            )
                        );
                }
            }
            else {
                initialRoute = initialRoute ?? Navigator.defaultRouteName;
            }

            if (initialRoute != null) {
                _history.AddRange(
                    LinqUtils<_RouteEntry,Route>.SelectList(
                        widget.onGenerateInitialRoutes(
                        this,
                                widget.initialRoute ?? Navigator.defaultRouteName
                ), (Route route) =>
                         new _RouteEntry(
                             route, 
                             initialState: _RouteLifecycle.add
                             )
                        )
                    );
            }
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            _flushHistoryUpdates();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
        }


        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (Navigator) oldWidget;
            base.didUpdateWidget(oldWidget);
            D.assert(
                widget.pages.isEmpty() || widget.onPopPage != null, () =>
                    "The Navigator.onPopPage must be provided to use the Navigator.pages API"
            );
            if (!((Navigator)oldWidget).observers.equalsList(widget.observers)) {
                foreach (NavigatorObserver observer in ((Navigator)oldWidget).observers)
                    observer._navigator = null;
                foreach (NavigatorObserver observer in widget.observers) {
                    D.assert(observer.navigator == null);
                    observer._navigator = this;
                }
            }

            if (!((Navigator)oldWidget).pages.equalsList(widget.pages)) {
                D.assert(
                    widget.pages.isNotEmpty(), () =>
                        "To use the Navigator.pages, there must be at least one page in the list."
                );
                _updatePages();
            }

            foreach (_RouteEntry entry in _history)
                entry.route.changedExternalState();
        }

        void _debugCheckDuplicatedPageKeys() {
            D.assert(() => {
                List<Key> keyReservation = new List<Key>();
                foreach (Page page in widget.pages) {
                    if (page.key != null) {
                        D.assert(!keyReservation.Contains(page.key));
                        keyReservation.Add(page.key);
                    }
                }

                return true;
            });
        }


        public override void dispose() {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            foreach (NavigatorObserver observer in widget.observers)
                observer._navigator = null;
            focusScopeNode.dispose();
            foreach (_RouteEntry entry in _history)
                entry.dispose();
            base.dispose();
            D.assert(_debugLocked);
        }

        public OverlayState overlay {
            get { return _overlayKey.currentState; }
        }

        public List<OverlayEntry> _allRouteOverlayEntries { 
            get {
                List<OverlayEntry> overlayEntries = new  List<OverlayEntry>();
                foreach (var historyEntry in _history) {
                    overlayEntries.AddRange(historyEntry.route.overlayEntries);
                }
                return overlayEntries;
            }
        }

        string _lastAnnouncedRouteName;

        bool _debugUpdatingPage = false;

        void _updatePages() {
            D.assert(() => {
                D.assert(!_debugUpdatingPage);
                _debugCheckDuplicatedPageKeys();
                _debugUpdatingPage = true;
                return true;
            });

            bool needsExplicitDecision = false;
            int newPagesBottom = 0;
            int oldEntriesBottom = 0;
            int newPagesTop = widget.pages.Count - 1;
            int oldEntriesTop = _history.Count - 1;
            List<_RouteEntry> newHistory = new List<_RouteEntry>();
            Dictionary<_RouteEntry, List<_RouteEntry>> pageRouteToPagelessRoutes =
                new Dictionary<_RouteEntry, List<_RouteEntry>>();
            _RouteEntry previousOldPageRouteEntry = null;

            while (oldEntriesBottom <= oldEntriesTop) {
                _RouteEntry oldEntry = _history[oldEntriesBottom];
                D.assert(oldEntry != null && oldEntry.currentState != _RouteLifecycle.disposed);
                if (!oldEntry.hasPage) {
                    List<_RouteEntry> pagelessRoutes = pageRouteToPagelessRoutes.putIfAbsent(
                        previousOldPageRouteEntry,
                        () => new List<_RouteEntry>()
                    );
                    pagelessRoutes.Add(oldEntry);
                    oldEntriesBottom += 1;
                    continue;
                }

                if (newPagesBottom > newPagesTop)
                    break;
                Page newPage = widget.pages[newPagesBottom];
                if (!oldEntry.canUpdateFrom(newPage))
                    break;
                previousOldPageRouteEntry = oldEntry;
                oldEntry.route._updateSettings(newPage);
                newHistory.Add(oldEntry);
                newPagesBottom += 1;
                oldEntriesBottom += 1;
            }

            int pagelessRoutesToSkip = 0;
            while ((oldEntriesBottom <= oldEntriesTop) && (newPagesBottom <= newPagesTop)) {
                _RouteEntry oldEntry = _history[oldEntriesTop];
                D.assert(oldEntry != null && oldEntry.currentState != _RouteLifecycle.disposed);
                if (!oldEntry.hasPage) {
                    // This route might need to be skipped if we can not find a page above.
                    pagelessRoutesToSkip += 1;
                    oldEntriesTop -= 1;
                    continue;
                }

                Page newPage = widget.pages[newPagesTop];
                if (!oldEntry.canUpdateFrom(newPage))
                    break;
                pagelessRoutesToSkip = 0;
                oldEntriesTop -= 1;
                newPagesTop -= 1;
            }

            oldEntriesTop += pagelessRoutesToSkip;
            int oldEntriesBottomToScan = oldEntriesBottom;
            Dictionary<LocalKey, _RouteEntry> pageKeyToOldEntry = new Dictionary<LocalKey, _RouteEntry> { };
            while (oldEntriesBottomToScan <= oldEntriesTop) {
                _RouteEntry oldEntry = _history[oldEntriesBottomToScan];
                oldEntriesBottomToScan += 1;
                D.assert(
                    oldEntry != null &&
                    oldEntry.currentState != _RouteLifecycle.disposed
                );

                if (!oldEntry.hasPage)
                    continue;

                D.assert(oldEntry.hasPage);
                Page page = oldEntry.route.settings as Page;
                if (page.key == null)
                    continue;

                D.assert(!pageKeyToOldEntry.ContainsKey(page.key));
                pageKeyToOldEntry[page.key] = oldEntry;
            }

            while (newPagesBottom <= newPagesTop) {
                Page<object> nextPage = widget.pages[newPagesBottom];
                newPagesBottom += 1;
                if (
                    nextPage.key == null ||
                    !pageKeyToOldEntry.ContainsKey(nextPage.key) ||
                    !pageKeyToOldEntry[nextPage.key].canUpdateFrom(nextPage)
                ) {

                    _RouteEntry newEntry = new _RouteEntry(
                        nextPage.createRoute(context),
                        initialState: _RouteLifecycle.staging
                    );
                    needsExplicitDecision = true;
                    D.assert(
                        newEntry.route.settings == nextPage, () =>
                            "If a route is created from a page, its must have that page as its " +
                            "settings."
                    );
                    newHistory.Add(newEntry);
                }
                else {

                    _RouteEntry matchingEntry = pageKeyToOldEntry[nextPage.key];
                    pageKeyToOldEntry.Remove(nextPage.key);
                    D.assert(matchingEntry.canUpdateFrom(nextPage));
                    matchingEntry.route._updateSettings(nextPage);
                    newHistory.Add(matchingEntry);
                }
            }

            // Any remaining old routes that do not have a match will need to be removed.
            Dictionary<RouteTransitionRecord, RouteTransitionRecord> locationToExitingPageRoute =
                new Dictionary<RouteTransitionRecord, RouteTransitionRecord>();
            while (oldEntriesBottom <= oldEntriesTop) {
                _RouteEntry potentialEntryToRemove = _history[oldEntriesBottom];
                oldEntriesBottom += 1;

                if (!potentialEntryToRemove.hasPage) {
                    D.assert(previousOldPageRouteEntry != null);
                    List<_RouteEntry> pagelessRoutes = pageRouteToPagelessRoutes
                        .putIfAbsent(
                            previousOldPageRouteEntry,
                            () => new List<_RouteEntry>()
                        );
                    pagelessRoutes.Add(potentialEntryToRemove);
                    D.assert(() => {
                        potentialEntryToRemove._debugWaitingForExitDecision =
                            previousOldPageRouteEntry._debugWaitingForExitDecision;
                        return true;
                    });
                    continue;
                }

                Page potentialPageToRemove = potentialEntryToRemove.route.settings as Page;

                if (
                    potentialPageToRemove.key == null ||
                    pageKeyToOldEntry.ContainsKey(potentialPageToRemove.key)
                ) {
                    locationToExitingPageRoute[previousOldPageRouteEntry] = potentialEntryToRemove;
                    D.assert(() => {
                        potentialEntryToRemove._debugWaitingForExitDecision = true;
                        return true;
                    });
                }

                previousOldPageRouteEntry = potentialEntryToRemove;
            }

            // We"ve scanned the whole list.
            D.assert(oldEntriesBottom == oldEntriesTop + 1);
            D.assert(newPagesBottom == newPagesTop + 1);
            newPagesTop = widget.pages.Count - 1;
            oldEntriesTop = _history.Count - 1;

            D.assert(() => {
                if (oldEntriesBottom <= oldEntriesTop)
                    return newPagesBottom <= newPagesTop &&
                           _history[oldEntriesBottom].hasPage &&
                           _history[oldEntriesBottom].canUpdateFrom(widget.pages[newPagesBottom]);
                else
                    return newPagesBottom > newPagesTop;
            });


            while ((oldEntriesBottom <= oldEntriesTop) && (newPagesBottom <= newPagesTop)) {
                _RouteEntry oldEntry = _history[oldEntriesBottom];
                D.assert(oldEntry != null && oldEntry.currentState != _RouteLifecycle.disposed);
                if (!oldEntry.hasPage) {
                    D.assert(previousOldPageRouteEntry != null);
                    List<_RouteEntry> pagelessRoutes = pageRouteToPagelessRoutes
                        .putIfAbsent(
                            previousOldPageRouteEntry,
                            () => new List<_RouteEntry>()
                        );
                    pagelessRoutes.Add(oldEntry);
                    continue;
                }

                previousOldPageRouteEntry = oldEntry;
                Page newPage = widget.pages[newPagesBottom];
                D.assert(oldEntry.canUpdateFrom(newPage));
                oldEntry.route._updateSettings(newPage);
                newHistory.Add(oldEntry);
                oldEntriesBottom += 1;
                newPagesBottom += 1;
            }

            // Finally, uses transition delegate to make explicit decision if needed.
            needsExplicitDecision = needsExplicitDecision || locationToExitingPageRoute.isNotEmpty();
            IEnumerable<_RouteEntry> results = newHistory;
            if (needsExplicitDecision) {
                List<RouteTransitionRecord> newHistoryRecord = new List<RouteTransitionRecord>();
                for (int i = 0; i < newHistoryRecord.Count; i++) {
                    newHistoryRecord[i] = newHistory[i];
                }
                Dictionary<RouteTransitionRecord, List<RouteTransitionRecord>> pageRouteToPagelessRoutesRecord 
                    = new Dictionary<RouteTransitionRecord, List<RouteTransitionRecord>>();
                foreach (var pageRoute in pageRouteToPagelessRoutes) {
                    List<RouteTransitionRecord> newRecord = new List<RouteTransitionRecord>();
                    for(int i = 0; i < pageRoute.Value.Count;i++){
                        newRecord.Add(pageRoute.Value[i]);
                    }
                    pageRouteToPagelessRoutesRecord.Add(pageRoute.Key,newRecord);
                }
                results = widget.transitionDelegate._transition(
                    newPageRouteHistory:newHistoryRecord, 
                    locationToExitingPageRoute: locationToExitingPageRoute,
                    pageRouteToPagelessRoutes: pageRouteToPagelessRoutesRecord
                ).Cast<_RouteEntry>();
            }

            _history = new List<_RouteEntry>();
            // Adds the leading pageless routes if there is any.
            if (pageRouteToPagelessRoutes.ContainsKey(null)) {
                _history.AddRange(pageRouteToPagelessRoutes[null]);
            }

            foreach (_RouteEntry result in results) {
                _history.Add(result);
                if (pageRouteToPagelessRoutes.ContainsKey(result)) {
                    _history.AddRange(pageRouteToPagelessRoutes[result]);
                }
            }

            D.assert(() => {
                _debugUpdatingPage = false;
                return true;
            });
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            _flushHistoryUpdates();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
        }

        public void _flushHistoryUpdates(bool rearrangeOverlay = true) {
            D.assert(_debugLocked && !_debugUpdatingPage);

            int index = _history.Count - 1;
            _RouteEntry next = null;
            _RouteEntry entry = _history[index];
            _RouteEntry previous = index > 0 ? _history[index - 1] : null;
            bool canRemoveOrAdd = false; // Whether there is a fully opaque route on top to silently remove or add route underneath.
            Route poppedRoute = null; // The route that should trigger didPopNext on the top active route.
            bool seenTopActiveRoute = false; // Whether we"ve seen the route that would get didPopNext.
            List<_RouteEntry> toBeDisposed = new List<_RouteEntry>();
            while (index >= 0) {
                switch (entry.currentState) {
                    case _RouteLifecycle.add:
                        D.assert(rearrangeOverlay);
                        entry.handleAdd(
                            navigator: this
                        );
                        D.assert(entry.currentState == _RouteLifecycle.adding);
                        continue;
                    case _RouteLifecycle.adding:
                        if (canRemoveOrAdd || next == null) {
                            entry.didAdd(
                                navigator: this,
                                previous: previous?.route,
                                previousPresent: _getRouteBefore(index - 1, _RouteEntry.isPresentPredicate)?.route,
                                isNewFirst: next == null
                            );
                            D.assert(entry.currentState == _RouteLifecycle.idle);
                            continue;
                        }

                        break;
                    case _RouteLifecycle.push:
                    case _RouteLifecycle.pushReplace:
                    case _RouteLifecycle.replace:
                        D.assert(rearrangeOverlay);
                        entry.handlePush(
                            navigator: this,
                            previous: previous?.route,
                            previousPresent: _getRouteBefore(index - 1, _RouteEntry.isPresentPredicate)?.route,
                            isNewFirst: next == null
                        );
                        D.assert(entry.currentState != _RouteLifecycle.push);
                        D.assert(entry.currentState != _RouteLifecycle.pushReplace);
                        D.assert(entry.currentState != _RouteLifecycle.replace);
                        if (entry.currentState == _RouteLifecycle.idle) {
                            continue;
                        }
                        break;
                    case _RouteLifecycle.pushing: // Will exit this state when animation completes.
                        if (!seenTopActiveRoute && poppedRoute != null)
                            entry.handleDidPopNext(poppedRoute);
                        seenTopActiveRoute = true;
                        break;
                    case _RouteLifecycle.idle:
                        if (!seenTopActiveRoute && poppedRoute != null)
                            entry.handleDidPopNext(poppedRoute);
                        seenTopActiveRoute = true;
                        canRemoveOrAdd = true;
                        break;
                    case _RouteLifecycle.pop:
                        if (!seenTopActiveRoute) {
                            if (poppedRoute != null)
                                entry.handleDidPopNext(poppedRoute);
                            poppedRoute = entry.route;
                        }
                        entry.handlePop(
                            navigator: this,
                            previousPresent: _getRouteBefore(index, _RouteEntry.willBePresentPredicate)?.route
                        );
                        D.assert(entry.currentState == _RouteLifecycle.popping);
                        canRemoveOrAdd = true;
                        break;
                    case _RouteLifecycle.popping:
                        // Will exit this state when animation completes.
                        break;
                    case _RouteLifecycle.remove:
                        if (!seenTopActiveRoute) {
                            if (poppedRoute != null)
                                entry.route.didPopNext(poppedRoute);
                            poppedRoute = null;
                        }

                        entry.handleRemoval(
                            navigator: this,
                            previousPresent: _getRouteBefore(index, _RouteEntry.willBePresentPredicate)?.route
                        );
                        D.assert(entry.currentState == _RouteLifecycle.removing);
                        continue;
                    case _RouteLifecycle.removing:
                        if (!canRemoveOrAdd && next != null) {
                            // We aren"t allowed to remove this route yet.
                            break;
                        }

                        entry.currentState = _RouteLifecycle.dispose;
                        continue;
                    case _RouteLifecycle.dispose:
                        // Delay disposal until didChangeNext/didChangePrevious have been sent.
                        toBeDisposed.Add(_history[index]);
                        _history.RemoveAt(index);
                        entry = next;
                        break;
                    case _RouteLifecycle.disposed:
                    case _RouteLifecycle.staging:
                        D.assert(false);
                        break;
                }

                index -= 1;
                next = entry;
                entry = previous;
                previous = index > 0 ? _history[index - 1] : null;
            }
            _flushRouteAnnouncement();

           
           _RouteEntry lastEntry = null;//_history.lastWhere(_RouteEntry.isPresentPredicate, orElse: () => null);
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isPresentPredicate(historyEntry)) {
                    lastEntry = historyEntry;
                }
            }
            string routeName = lastEntry?.route?.settings?.name;
            if (routeName != _lastAnnouncedRouteName) {
               //RouteNotificationMessages.maybeNotifyRouteChange(routeName, _lastAnnouncedRouteName);
                _lastAnnouncedRouteName = routeName;
            }

            // Lastly, removes the overlay entries of all marked entries and disposes
            // them.
            foreach (_RouteEntry routeEntry in toBeDisposed) {
                foreach (OverlayEntry overlayEntry in routeEntry.route.overlayEntries) {
                    overlayEntry.remove();
                }
                routeEntry.dispose();
            }
            if (rearrangeOverlay)
                overlay?.rearrange(_allRouteOverlayEntries);
        }

        void _flushRouteAnnouncement() {
            int index = _history.Count - 1;
            while (index >= 0) {
                _RouteEntry entry = _history[index];
                if (!entry.suitableForAnnouncement) {
                    index -= 1;
                    continue;
                }

                _RouteEntry next = _getRouteAfter(index + 1, _RouteEntry.suitableForTransitionAnimationPredicate);

                if (next?.route != entry.lastAnnouncedNextRoute) {
                    if (entry.shouldAnnounceChangeToNext(next?.route)) {
                        entry.route.didChangeNext(next?.route);
                    }

                    entry.lastAnnouncedNextRoute = next?.route;
                }

                _RouteEntry previous = _getRouteBefore(index - 1, _RouteEntry.suitableForTransitionAnimationPredicate);
                if (previous?.route != entry.lastAnnouncedPreviousRoute) {
                    entry.route.didChangePrevious(previous?.route);
                    entry.lastAnnouncedPreviousRoute = previous?.route;
                }

                index -= 1;
            }
        }

        _RouteEntry _getRouteBefore(int index, _RouteEntryPredicate predicate) {
            index = _getIndexBefore(index, predicate);
            return index >= 0 ? _history[index] : null;
        }

        int _getIndexBefore(int index, _RouteEntryPredicate predicate) {
            while (index >= 0 && !predicate(_history[index])) {
                index -= 1;
            }

            return index;
        }

        _RouteEntry _getRouteAfter(int index, _RouteEntryPredicate predicate) {
            while (index < _history.Count && !predicate(_history[index])) {
                index += 1;
            }

            return index < _history.Count ? _history[index] : null;
        }
        
        public Route _routeNamed(string name, object arguments, bool allowNull = false) {
            D.assert(!_debugLocked);
            D.assert(name != null);
            if (allowNull && widget.onGenerateRoute == null)
                return null;
            D.assert(() => {
                if (widget.onGenerateRoute == null) {
                    throw new UIWidgetsError(
                        "Navigator.onGenerateRoute was null, but the route named " + $"{name} was referenced.\n" +
                        "To use the Navigator API with named routes (pushNamed, pushReplacementNamed, or " +
                        "pushNamedAndRemoveUntil), the Navigator must be provided with an " +
                        "onGenerateRoute handler.\n" +
                        "The Navigator was:\n" +
                        $"  {this}"
                    );
                }

                return true;
            });
            RouteSettings settings = new RouteSettings(
                name: name,
                arguments: arguments
            );
            Route route = widget.onGenerateRoute(settings) as Route;
            if (route == null && !allowNull) {
                D.assert(() => {
                    if (widget.onUnknownRoute == null) {
                        throw new UIWidgetsError(
                           new List<DiagnosticsNode>() {
                               new ErrorSummary($"Navigator.onGenerateRoute returned null when requested to build route \"{name}\"."),
                               new ErrorDescription(
                                   "The onGenerateRoute callback must never return null, unless an onUnknownRoute "+
                               "callback is provided as well."
                               ),
                               new DiagnosticsProperty<NavigatorState>("The Navigator was", this, style: DiagnosticsTreeStyle.errorProperty),
                           }
                        );
                    }

                    return true;
                });
                route = widget.onUnknownRoute(settings) as Route;
                D.assert(() => {
                    if (route == null) {
                        throw new UIWidgetsError(
                            new List<DiagnosticsNode>() {
                                new ErrorSummary($"Navigator.onUnknownRoute returned null when requested to build route \"{name}\"."),
                                new ErrorDescription("The onUnknownRoute callback must never return null."),
                                new DiagnosticsProperty<NavigatorState>("The Navigator was", this, style: DiagnosticsTreeStyle.errorProperty),

                            }
                        );
                    }

                    return true;
                });
            }

            D.assert(route != null || allowNull);
            return route;
        }

        public Route _routeNamed<T>(string name, object arguments, bool allowNull = false) {
            D.assert(!_debugLocked);
            D.assert(name != null);
            if (allowNull && widget.onGenerateRoute == null)
                return null;
            D.assert(() => {
                if (widget.onGenerateRoute == null) {
                    throw new UIWidgetsError(
                        "Navigator.onGenerateRoute was null, but the route named " + $"{name} was referenced.\n" +
                        "To use the Navigator API with named routes (pushNamed, pushReplacementNamed, or " +
                        "pushNamedAndRemoveUntil), the Navigator must be provided with an " +
                        "onGenerateRoute handler.\n" +
                        "The Navigator was:\n" +
                        $"  {this}"
                    );
                }

                return true;
            });
            RouteSettings settings = new RouteSettings(
                name: name,
                arguments: arguments
            );

            Route route = widget.onGenerateRoute(settings);
            if (route == null && !allowNull) {
                D.assert(() => {
                    if (widget.onUnknownRoute == null) {
                        throw new UIWidgetsError(
                            "Navigator.onGenerateRoute returned null when requested to build route " + $"{name}." +
                            "The onGenerateRoute callback must never return null, unless an onUnknownRoute " +
                            "callback is provided as well." +
                            new DiagnosticsProperty<NavigatorState>("The Navigator was", this,
                                style: DiagnosticsTreeStyle.errorProperty)
                        );
                    }

                    return true;
                });
                route = widget.onUnknownRoute(settings);
                D.assert(() => {
                    if (route == null) {
                        throw new UIWidgetsError(
                            "Navigator.onUnknownRoute returned null when requested to build route " + $"{name}." +
                            "The onUnknownRoute callback must never return null." +
                            new DiagnosticsProperty<NavigatorState>("The Navigator was", this,
                                style: DiagnosticsTreeStyle.errorProperty)
                        );
                    }

                    return true;
                });
            }

            D.assert(route != null || allowNull);
            return route;
        }

        public Future<T> pushNamed<T>(
            string routeName,
            object arguments = null
        ) {
            return push<T>(_routeNamed<T>(routeName, arguments: arguments));
        }
        
        public Future pushNamed(
            string routeName,
            object arguments = null
        ) {
            return push(_routeNamed(routeName, arguments: arguments));
        }

        public Future<T> pushReplacementNamed<T, TO>(
            string routeName,
            TO result = default,
            object arguments = null
        ) {
            return pushReplacement<T, TO>(_routeNamed<T>(routeName, arguments: arguments), result: result);
        }

        public Future pushReplacementNamed(
            string routeName,
            object result = default,
            object arguments = null
        ) {
            return pushReplacement(_routeNamed(routeName, arguments: arguments), result: result);
        }


        public Future<T> popAndPushNamed<T, TO>(
            string routeName,
            TO result = default,
            object arguments = null
        ) {
            pop<TO>(result);
            return pushNamed<T>(routeName, arguments: arguments);
        }

        public Future<T> pushNamedAndRemoveUntil<T>(
            string newRouteName,
            RoutePredicate predicate,
            object arguments = null
        ) {
            return pushAndRemoveUntil<T>(_routeNamed<T>(newRouteName, arguments: arguments), predicate);
        }
        public Future push(Route route) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(route != null);
            D.assert(route._navigator == null);
            _history.Add(new _RouteEntry(route, initialState: _RouteLifecycle.push));
            _flushHistoryUpdates();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation(route);
            return route.popped;
        }

        public Future<T> push<T>(Route route) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(route != null);
            D.assert(route._navigator == null);
            _history.Add(new _RouteEntry(route, initialState: _RouteLifecycle.push));
            _flushHistoryUpdates();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation(route);
            return route.popped.to<T>();
        }
        
        void _afterNavigation(Route route) {
            if (!foundation_.kReleaseMode) {
                Dictionary<string, object> routeJsonable = new Dictionary<string, object>();
                if (route != null) {
                    routeJsonable = new Dictionary<string, object>();

                    string description;
                    if (route is TransitionRoute ){
                        description = ((TransitionRoute)route).debugLabel;
                    }
                    else {
                        description = $"{route}";
                    }

                    routeJsonable["description"] = description;

                    RouteSettings settings = route.settings;
                    Dictionary<string, object> settingsJsonable = new Dictionary<string, object> {
                        {"name", settings.name}
                    };
                    if (settings.arguments != null) {
                        settingsJsonable["arguments"] = JsonUtility.ToJson(settings.arguments);
                    }

                    routeJsonable["settings"] = settingsJsonable;
                }
                developer.developer_.postEvent("Flutter.Navigation", new Hashtable{
                    {"route", routeJsonable}
                });
            }

            _cancelActivePointers();
        }

        

        public Future<T> pushReplacement<T, TO>(Route newRoute, TO result) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(newRoute != null);
            D.assert(newRoute._navigator == null);
            D.assert(_history.isNotEmpty());
            
            bool anyEntry = false;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isPresentPredicate(historyEntry)) {
                    anyEntry = true;
                }
            }
            D.assert(anyEntry,()=> "Navigator has no active routes to replace.");
            _RouteEntry lastEntry = null;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isPresentPredicate(historyEntry)) {
                    lastEntry = historyEntry;
                }
            }
            lastEntry.complete(result, isReplaced: true);
            
            _history.Add(new _RouteEntry(newRoute, initialState: _RouteLifecycle.pushReplace));
            _flushHistoryUpdates();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation(newRoute);
            return newRoute.popped.to<T>();
        }

        public Future pushReplacement(Route newRoute, object result) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(newRoute != null);
            D.assert(newRoute._navigator == null);
            D.assert(_history.isNotEmpty());
            
            bool anyEntry = false;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isPresentPredicate(historyEntry)) {
                    anyEntry = true;
                }
            }
            D.assert(anyEntry,()=> "Navigator has no active routes to replace.");
            _RouteEntry lastEntry = null;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isPresentPredicate(historyEntry)) {
                    lastEntry = historyEntry;
                }
            }
            lastEntry.complete(result, isReplaced: true);
            
            _history.Add(new _RouteEntry(newRoute, initialState: _RouteLifecycle.pushReplace));
            _flushHistoryUpdates();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation(newRoute);
            return newRoute.popped.to<object>();
        }


        public Future<T> pushAndRemoveUntil<T>(Route newRoute, RoutePredicate predicate) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(newRoute != null);
            D.assert(newRoute._navigator == null);
            D.assert(newRoute.overlayEntries.isEmpty());
            D.assert(predicate != null);
            int index = _history.Count - 1;
            _history.Add(new _RouteEntry(newRoute, initialState: _RouteLifecycle.push));
            while (index >= 0) {
                _RouteEntry entry = _history[index];
                if (entry.isPresent && !predicate(entry.route))
                    _history[index].remove();
                index -= 1;
            }

            _flushHistoryUpdates();

            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation(newRoute);
            return newRoute.popped.to<T>();
        }

        public void replace<T>(Route oldRoute, Route<T> newRoute) {
            D.assert(!_debugLocked);
            D.assert(oldRoute != null);
            D.assert(newRoute != null);
            if (oldRoute == newRoute)
                return;
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(oldRoute._navigator == this);
            D.assert(newRoute._navigator == null);

            int index = -1;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isRoutePredicate(oldRoute)(historyEntry)) {
                    index = _history.IndexOf(historyEntry);
                    break;
                }
            }

            D.assert(index >= 0, () => "This Navigator does not contain the specified oldRoute.");
            D.assert(_history[index].isPresent,
                () => "The specified oldRoute has already been removed from the Navigator.");
            bool wasCurrent = oldRoute.isCurrent;
            _history.Insert(index + 1, new _RouteEntry(newRoute, initialState: _RouteLifecycle.replace));
            _history.RemoveAt(index);
            _flushHistoryUpdates();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            if (wasCurrent)
                _afterNavigation(newRoute);
        }

        public void replaceRouteBelow<T>(Route anchorRoute, Route<T> newRoute) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(anchorRoute != null);
            D.assert(anchorRoute._navigator == this);
            D.assert(newRoute != null);
            D.assert(newRoute._navigator == null);
            int anchorIndex = -1;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isRoutePredicate(anchorRoute)(historyEntry)) {
                    anchorIndex = _history.IndexOf(historyEntry);
                    break;
                }
            }

            D.assert(anchorIndex >= 0, () => "This Navigator does not contain the specified anchorRoute.");
            D.assert(_history[anchorIndex].isPresent,
                () => "The specified anchorRoute has already been removed from the Navigator.");
            int index = anchorIndex - 1;
            while (index >= 0) {
                if (_history[index].isPresent)
                    break;
                index -= 1;
            }

            D.assert(index >= 0, () => "There are no routes below the specified anchorRoute.");
            _history.Insert(index + 1, new _RouteEntry(newRoute, initialState: _RouteLifecycle.replace));
           
            _history.RemoveAt(index);
            _flushHistoryUpdates();
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
        }

        public bool canPop() {
            IEnumerable<_RouteEntry> iterator = new List<_RouteEntry>();
            
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isPresentPredicate(historyEntry)) {
                    iterator.Append(historyEntry);
                }
            }
            if (!iterator.GetEnumerator().MoveNext())
                return false; // we have no active routes, so we can"t pop
            if(iterator.GetEnumerator().Current.route.willHandlePopInternally)
                return true; // the first route can handle pops itself, so we can pop
            if (!iterator.GetEnumerator().MoveNext())
                return false; // there"s only one route, so we can"t pop
            return true; // there"s at least two routes, so we can pop
        }

        public Future<bool> maybePop<T>(T result = default(T)) {
            ///asyn
            _RouteEntry lastEntry = null; 
            foreach (_RouteEntry routeEntry in _history) {
                if (_RouteEntry.isPresentPredicate(routeEntry)) {
                    lastEntry = routeEntry;
                }
            }
            
            D.assert(lastEntry.route._navigator == this);
            // this is asynchronous // await
             return lastEntry.route.willPop().then_(disposition => {
                 if (lastEntry == null) {
                     return false;
                 }
                 if (!mounted)
                     return true; // forget about this pop, we were disposed in the meantime
                 _RouteEntry newLastEntry = null;

                 foreach (_RouteEntry history in _history) {
                     if (_RouteEntry.isPresentPredicate(history)) {
                         newLastEntry = history;
                     }
                 }

                 if (lastEntry != newLastEntry)
                     return true; // forget about this pop, something happened to our history in the meantime
                 switch (disposition) {
                     case RoutePopDisposition.bubble:
                         return false;
                     case RoutePopDisposition.pop:
                         pop(result);
                         return true;
                     case RoutePopDisposition.doNotPop:
                         return true;
                 }

                 return false;

             }).to<bool>(); 
           
        }

        public void pop<T>(T result = default) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });

            _RouteEntry entry = null; 
            foreach (_RouteEntry route in _history) {
                if (_RouteEntry.isPresentPredicate(route)) {
                    entry = route;
                }
            }

            if (entry.hasPage) {
                if (widget.onPopPage(entry.route, result))
                    entry.currentState = _RouteLifecycle.pop;
            }
            else {
                entry.pop<T>(result);
            }

            if (entry.currentState == _RouteLifecycle.pop) {

                _flushHistoryUpdates(rearrangeOverlay: false);
                D.assert(entry.route._popCompleter.isCompleted);
            }

            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            _afterNavigation(entry.route);
        }


        public void popUntil(RoutePredicate predicate) {
            _RouteEntry routeEntry = null;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isPresentPredicate(historyEntry)) {
                    routeEntry = historyEntry;
                }
            }

            while (!predicate(routeEntry.route)) {
                pop<object>();
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
            bool wasCurrent = route.isCurrent;
            _RouteEntry entry = null; 
            foreach (_RouteEntry routeEntry in _history) {
                if (_RouteEntry.isRoutePredicate(route)(routeEntry)) {
                    entry = routeEntry;
                    break;
                }
            }

            D.assert(entry != null);
            entry.remove();
            _flushHistoryUpdates(rearrangeOverlay: false);
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
            if (wasCurrent) {
                _RouteEntry lastEntry = null;
                foreach (_RouteEntry routeEntry in _history) {
                    if (_RouteEntry.isPresentPredicate(routeEntry)) {
                        lastEntry = routeEntry;
                    }
                }

                _afterNavigation(
                    lastEntry?.route
                );
            }


        }


        public void removeRouteBelow(Route anchorRoute) {
            D.assert(!_debugLocked);
            D.assert(() => {
                _debugLocked = true;
                return true;
            });
            D.assert(anchorRoute != null);
            D.assert(anchorRoute._navigator == this);
            int anchorIndex = -1;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isRoutePredicate(anchorRoute)(historyEntry)) {
                    anchorIndex = _history.IndexOf(historyEntry);
                    break;
                }
            }

            D.assert(anchorIndex >= 0, () => "This Navigator does not contain the specified anchorRoute.");
            D.assert(_history[anchorIndex].isPresent,
                () => "The specified anchorRoute has already been removed from the Navigator.");
            int index = anchorIndex - 1;
            while (index >= 0) {
                if (_history[index].isPresent)
                    break;
                index -= 1;
            }

            D.assert(index >= 0, () => "There are no routes below the specified anchorRoute.");
            _history.RemoveAt(index);
            _flushHistoryUpdates(rearrangeOverlay: false);
            D.assert(() => {
                _debugLocked = false;
                return true;
            });
        }

        public void finalizeRoute(Route route) {
            bool wasDebugLocked = false;
            D.assert(() => {
                wasDebugLocked = _debugLocked;
                _debugLocked = true;
                return true;
            });

            List<_RouteEntry> routeEntries = new List<_RouteEntry>();
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isRoutePredicate(route)(historyEntry)) {
                    routeEntries.Add(historyEntry);
                }
            }
            D.assert(routeEntries.Count == 1);
            
            _RouteEntry entry = null;
            foreach (var historyEntry in _history) {
                if (_RouteEntry.isRoutePredicate(route)(historyEntry)) {
                    entry = historyEntry;
                    break;
                }
            }

            if (entry.doingPop) {
                entry.currentState = _RouteLifecycle.pop;
                _flushHistoryUpdates(rearrangeOverlay: false);
            }

            D.assert(entry.currentState != _RouteLifecycle.pop);
            entry.finalize();
            _flushHistoryUpdates(rearrangeOverlay: false);
            D.assert(() => {
                _debugLocked = wasDebugLocked;
                return true;
            });
        }

        public int _userGesturesInProgress {
            get { return _userGesturesInProgressCount; }
            set {
                _userGesturesInProgressCount = value;
                userGestureInProgressNotifier.value = _userGesturesInProgress > 0;
            }
        }

        int _userGesturesInProgressCount = 0;


        public bool userGestureInProgress {
            get { return userGestureInProgressNotifier.value; }

        }


        public ValueNotifier<bool> userGestureInProgressNotifier = new ValueNotifier<bool>(false);


        public void didStartUserGesture() {
            _userGesturesInProgress += 1;
            if (_userGesturesInProgress == 1) {
                int routeIndex = _getIndexBefore(
                    _history.Count - 1,
                    _RouteEntry.willBePresentPredicate
                );
                Route route = _history[routeIndex].route;
                Route previousRoute = null;
                if (!route.willHandlePopInternally && routeIndex > 0) {
                    previousRoute = (Route) _getRouteBefore(
                        routeIndex - 1,
                        _RouteEntry.willBePresentPredicate
                    ).route;
                }

                foreach (NavigatorObserver observer in widget.observers)
                    observer.didStartUserGesture(route, previousRoute);
            }
        }

        public void didStopUserGesture() {
            D.assert(_userGesturesInProgress > 0);
            _userGesturesInProgress -= 1;
            if (_userGesturesInProgress == 0) {
                foreach (NavigatorObserver observer in widget.observers)
                    observer.didStopUserGesture();
            }
        }

        public readonly HashSet<int> _activePointers = new HashSet<int>();

        void _handlePointerDown(PointerDownEvent evt) {
            _activePointers.Add(evt.pointer);
        }

        void _handlePointerUpOrCancel(PointerEvent evt) {
            _activePointers.Remove(evt.pointer);
        }

        void _cancelActivePointers() {
            // TODO(abarth): This mechanism is far from perfect. See https://github.com/flutter/flutter/issues/4770
            if (SchedulerBinding.instance.schedulerPhase == SchedulerPhase.idle) {

                RenderAbsorbPointer absorber =
                    _overlayKey.currentContext?.findAncestorRenderObjectOfType<RenderAbsorbPointer>();
                setState(() => {
                    if(absorber != null)
                        absorber.absorbing = true;
                });
            }

            _activePointers.ToList().ForEach(WidgetsBinding.instance.cancelPointer);
        }


        public override Widget build(BuildContext context) {
            D.assert(!_debugLocked);
            D.assert(_history.isNotEmpty());
            return new Listener(
                onPointerDown: _handlePointerDown,
                onPointerUp: _handlePointerUpOrCancel,
                onPointerCancel: _handlePointerUpOrCancel,
                child: new AbsorbPointer(
                    absorbing: false, // it"s mutated directly by _cancelActivePointers above
                    child: new FocusScope(
                        node: focusScopeNode,
                        autofocus: true,
                        child: new Overlay(
                            key: _overlayKey,
                            initialEntries: overlay == null
                                ? _allRouteOverlayEntries.ToList()
                                : new List<OverlayEntry>())
                    )
                )
            );

        }
    }

}