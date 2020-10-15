using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public abstract class OverlayRoute : Route {
        readonly List<OverlayEntry> _overlayEntries = new List<OverlayEntry>();

        public OverlayRoute(
            RouteSettings settings = null
        ) : base(settings) {
        }

        public override List<OverlayEntry> overlayEntries {
            get { return _overlayEntries; }
        }

        protected virtual bool finishedWhenPopped {
            get { return true; }
        }

        public abstract ICollection<OverlayEntry> createOverlayEntries();

        protected internal override void install(OverlayEntry insertionPoint) {
            D.assert(_overlayEntries.isEmpty());
            _overlayEntries.AddRange(createOverlayEntries());
            navigator.overlay?.insertAll(_overlayEntries, above: insertionPoint);
            base.install(insertionPoint);
        }

        protected internal override bool didPop(object result) {
            var returnValue = base.didPop(result);
            D.assert(returnValue);
            if (finishedWhenPopped) {
                navigator.finalizeRoute(this);
            }

            return returnValue;
        }

        protected internal override void dispose() {
            foreach (var entry in _overlayEntries) {
                entry.remove();
            }

            _overlayEntries.Clear();
            base.dispose();
        }
    }

    public abstract class TransitionRoute : OverlayRoute {
        public TransitionRoute(
            RouteSettings settings = null
        ) : base(settings) {
        }

        public Future completed {
            get { return _transitionCompleter.future; }
        }

        internal readonly Completer _transitionCompleter = Completer.create();

        public virtual TimeSpan transitionDuration { get; }

        public virtual bool opaque { get; }


        protected override bool finishedWhenPopped {
            get { return controller.status == AnimationStatus.dismissed; }
        }

        public virtual Animation<float> animation {
            get { return _animation; }
        }

        internal Animation<float> _animation;

        public AnimationController controller {
            get { return _controller; }
        }

        internal AnimationController _controller;

        public virtual AnimationController createAnimationController() {
            D.assert(!_transitionCompleter.isCompleted,
                () => $"Cannot reuse a {GetType()} after disposing it.");
            TimeSpan duration = transitionDuration;
            D.assert(duration >= TimeSpan.Zero);
            return new AnimationController(
                duration: duration,
                debugLabel: debugLabel,
                vsync: navigator
            );
        }

        public virtual Animation<float> createAnimation() {
            D.assert(!_transitionCompleter.isCompleted,
                () => $"Cannot reuse a {GetType()} after disposing it.");
            D.assert(_controller != null);
            return _controller.view;
        }

        object _result;

        internal void _handleStatusChanged(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.completed:
                    if (overlayEntries.isNotEmpty()) {
                        overlayEntries.first().opaque = opaque;
                    }

                    break;
                case AnimationStatus.forward:
                case AnimationStatus.reverse:
                    if (overlayEntries.isNotEmpty()) {
                        overlayEntries.first().opaque = false;
                    }

                    break;
                case AnimationStatus.dismissed:
                    // We might still be an active route if a subclass is controlling the
                    // the transition and hits the dismissed status. For example, the iOS
                    // back gesture drives this animation to the dismissed status before
                    // popping the navigator.
                    if (!isActive) {
                        navigator.finalizeRoute(this);
                        D.assert(overlayEntries.isEmpty());
                    }

                    break;
            }

            changedInternalState();
        }

        public virtual Animation<float> secondaryAnimation {
            get { return _secondaryAnimation; }
        }

        readonly ProxyAnimation _secondaryAnimation = new ProxyAnimation(Animations.kAlwaysDismissedAnimation);

        protected internal override void install(OverlayEntry insertionPoint) {
            D.assert(!_transitionCompleter.isCompleted, () => $"Cannot install a {GetType()} after disposing it.");
            _controller = createAnimationController();
            D.assert(_controller != null, () => $"{GetType()}.createAnimationController() returned null.");
            _animation = createAnimation();
            D.assert(_animation != null, () => $"{GetType()}.createAnimation() returned null.");
            base.install(insertionPoint);
        }

        protected internal override TickerFuture didPush() {
            D.assert(_controller != null,
                () => $"{GetType()}.didPush called before calling install() or after calling dispose().");
            D.assert(!_transitionCompleter.isCompleted, () => $"Cannot reuse a {GetType()} after disposing it.");
            _animation.addStatusListener(_handleStatusChanged);
            return _controller.forward();
        }

        protected internal override void didReplace(Route oldRoute) {
            D.assert(_controller != null,
                () => $"{GetType()}.didReplace called before calling install() or after calling dispose().");
            D.assert(!_transitionCompleter.isCompleted, () => $"Cannot reuse a {GetType()} after disposing it.");
            if (oldRoute is TransitionRoute route) {
                _controller.setValue(route._controller.value);
            }

            _animation.addStatusListener(_handleStatusChanged);
            base.didReplace(oldRoute);
        }

        protected internal override bool didPop(object result) {
            D.assert(_controller != null,
                () => $"{GetType()}.didPop called before calling install() or after calling dispose().");
            D.assert(!_transitionCompleter.isCompleted, () => $"Cannot reuse a {GetType()} after disposing it.");
            _result = result;
            _controller.reverse();
            return base.didPop(result);
        }

        protected internal override void didPopNext(Route nextRoute) {
            D.assert(_controller != null,
                () => $"{GetType()}.didPopNext called before calling install() or after calling dispose().");
            D.assert(!_transitionCompleter.isCompleted, () => $"Cannot reuse a {GetType()} after disposing it.");
            _updateSecondaryAnimation(nextRoute);
            base.didPopNext(nextRoute);
        }

        protected internal override void didChangeNext(Route nextRoute) {
            D.assert(_controller != null,
                () => $"{GetType()}.didChangeNext called before calling install() or after calling dispose().");
            D.assert(!_transitionCompleter.isCompleted, () => $"Cannot reuse a {GetType()} after disposing it.");
            _updateSecondaryAnimation(nextRoute);
            base.didChangeNext(nextRoute);
        }

        void _updateSecondaryAnimation(Route nextRoute) {
            if (nextRoute is TransitionRoute && canTransitionTo((TransitionRoute) nextRoute) &&
                ((TransitionRoute) nextRoute).canTransitionFrom(this)) {
                Animation<float> current = _secondaryAnimation.parent;
                if (current != null) {
                    if (current is TrainHoppingAnimation) {
                        TrainHoppingAnimation newAnimation = null;
                        newAnimation = new TrainHoppingAnimation(
                            ((TrainHoppingAnimation) current).currentTrain,
                            ((TransitionRoute) nextRoute)._animation,
                            onSwitchedTrain: () => {
                                D.assert(_secondaryAnimation.parent == newAnimation);
                                D.assert(newAnimation.currentTrain == ((TransitionRoute) nextRoute)._animation);
                                _secondaryAnimation.parent = newAnimation.currentTrain;
                                newAnimation.dispose();
                            }
                        );
                        _secondaryAnimation.parent = newAnimation;
                        ((TrainHoppingAnimation) current).dispose();
                    }
                    else {
                        _secondaryAnimation.parent =
                            new TrainHoppingAnimation(current, ((TransitionRoute) nextRoute)._animation);
                    }
                }
                else {
                    _secondaryAnimation.parent = ((TransitionRoute) nextRoute)._animation;
                }
            }
            else {
                _secondaryAnimation.parent = Animations.kAlwaysDismissedAnimation;
            }
        }


        public virtual bool canTransitionTo(TransitionRoute nextRoute) {
            return true;
        }

        public virtual bool canTransitionFrom(TransitionRoute previousRoute) {
            return true;
        }

        protected internal override void dispose() {
            D.assert(!_transitionCompleter.isCompleted, () => $"Cannot dispose a {GetType()} twice.");
            _controller?.dispose();
            _transitionCompleter.complete(FutureOr.value(_result));
            base.dispose();
        }

        public string debugLabel {
            get { return $"{GetType()}"; }
        }

        public override string ToString() {
            return $"{GetType()}(animation: {_controller}";
        }
    }

    public class LocalHistoryEntry {
        public LocalHistoryEntry(VoidCallback onRemove = null) {
            this.onRemove = onRemove;
        }

        public readonly VoidCallback onRemove;

        internal LocalHistoryRoute _owner;

        public void remove() {
            _owner.removeLocalHistoryEntry(this);
            D.assert(_owner == null);
        }

        internal void _notifyRemoved() {
            onRemove?.Invoke();
        }
    }


    public interface LocalHistoryRoute {
        void addLocalHistoryEntry(LocalHistoryEntry entry);
        void removeLocalHistoryEntry(LocalHistoryEntry entry);

        Route route { get; }
    }

    // todo make it to mixin
    public abstract class LocalHistoryRouteTransitionRoute : TransitionRoute, LocalHistoryRoute {
        List<LocalHistoryEntry> _localHistory;

        protected LocalHistoryRouteTransitionRoute(RouteSettings settings = null) : base(settings: settings) {
        }

        public void addLocalHistoryEntry(LocalHistoryEntry entry) {
            D.assert(entry._owner == null);
            entry._owner = this;
            _localHistory = _localHistory ?? new List<LocalHistoryEntry>();
            var wasEmpty = _localHistory.isEmpty();
            _localHistory.Add(entry);
            if (wasEmpty) {
                changedInternalState();
            }
        }

        public void removeLocalHistoryEntry(LocalHistoryEntry entry) {
            D.assert(entry != null);
            D.assert(entry._owner == this);
            D.assert(_localHistory.Contains(entry));
            _localHistory.Remove(entry);
            entry._owner = null;
            entry._notifyRemoved();
            if (_localHistory.isEmpty()) {
                changedInternalState();
            }
        }

        public override Future<RoutePopDisposition> willPop() {
            if (willHandlePopInternally) {
                return Future<RoutePopDisposition>.value(RoutePopDisposition.pop).to<RoutePopDisposition>();
            }

            return base.willPop();
        }

        protected internal override bool didPop(object result) {
            if (_localHistory != null && _localHistory.isNotEmpty()) {
                var entry = _localHistory.removeLast();
                D.assert(entry._owner == this);
                entry._owner = null;
                entry._notifyRemoved();
                if (_localHistory.isEmpty()) {
                    changedInternalState();
                }

                return false;
            }

            return base.didPop(result);
        }

        public override bool willHandlePopInternally {
            get { return _localHistory != null && _localHistory.isNotEmpty(); }
        }

        public Route route {
            get { return this; }
        }
    }


    public class _ModalScopeStatus : InheritedWidget {
        public _ModalScopeStatus(Key key = null, bool isCurrent = false,
            bool canPop = false, Route route = null, Widget child = null) : base(key: key, child: child) {
            D.assert(route != null);
            D.assert(child != null);

            this.isCurrent = isCurrent;
            this.canPop = canPop;
            this.route = route;
        }

        public readonly bool isCurrent;
        public readonly bool canPop;
        public readonly Route route;

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return isCurrent != ((_ModalScopeStatus) oldWidget).isCurrent ||
                   canPop != ((_ModalScopeStatus) oldWidget).canPop ||
                   route != ((_ModalScopeStatus) oldWidget).route;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new FlagProperty("isCurrent", value: isCurrent, ifTrue: "active",
                ifFalse: "inactive"));
            description.add(new FlagProperty("canPop", value: canPop, ifTrue: "can pop"));
        }
    }

    public class _ModalScope : StatefulWidget {
        public _ModalScope(Key key = null, ModalRoute route = null) : base(key) {
            this.route = route;
        }

        public readonly ModalRoute route;

        public override State createState() {
            return new _ModalScopeState();
        }
    }

    public class _ModalScopeState : State<_ModalScope> {
        Widget _page;

        Listenable _listenable;

        public override void initState() {
            base.initState();
            var animations = new List<Listenable> { };
            if (widget.route.animation != null) {
                animations.Add(widget.route.animation);
            }

            if (widget.route.secondaryAnimation != null) {
                animations.Add(widget.route.secondaryAnimation);
            }

            _listenable = ListenableUtils.merge(animations);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            D.assert(widget.route == ((_ModalScope) oldWidget).route);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _page = null;
        }

        internal void _forceRebuildPage() {
            setState(() => { _page = null; });
        }

        internal void _routeSetState(VoidCallback fn) {
            setState(fn);
        }

        public override Widget build(BuildContext context) {
            _page = _page ?? new RepaintBoundary(
                key: widget.route._subtreeKey, // immutable
                child: new Builder(
                    builder: (BuildContext _context) => widget.route.buildPage(
                        _context,
                        widget.route.animation,
                        widget.route.secondaryAnimation
                    ))
            );

            return new _ModalScopeStatus(
                route: widget.route,
                isCurrent: widget.route.isCurrent,
                canPop: widget.route.canPop,
                child: new Offstage(
                    offstage: widget.route.offstage,
                    child: new PageStorage(
                        bucket: widget.route._storageBucket,
                        child: new FocusScope(
                            node: widget.route.focusScopeNode,
                            child: new RepaintBoundary(
                                child: new AnimatedBuilder(
                                    animation: _listenable, // immutable
                                    builder: (BuildContext _context, Widget child) =>
                                        widget.route.buildTransitions(
                                            _context,
                                            widget.route.animation,
                                            widget.route.secondaryAnimation,
                                            new IgnorePointer(
                                                ignoring: widget.route.animation?.status ==
                                                          AnimationStatus.reverse,
                                                child: child
                                            )
                                        ),
                                    child: _page
                                )
                            )
                        )
                    )
                )
            );
        }
    }

    public abstract class ModalRoute : LocalHistoryRouteTransitionRoute {
        protected ModalRoute() {
        }

        protected ModalRoute(RouteSettings settings) : base(settings) {
        }

        public static Color _kTransparent = new Color(0x00000000);

        public static ModalRoute of(BuildContext context) {
            _ModalScopeStatus widget =
                (_ModalScopeStatus) context.inheritFromWidgetOfExactType(typeof(_ModalScopeStatus));
            return (ModalRoute) widget?.route;
        }

        protected virtual void setState(VoidCallback fn) {
            if (_scopeKey.currentState != null) {
                _scopeKey.currentState._routeSetState(fn);
            }
            else {
                fn();
            }
        }

        public RoutePredicate withName(string name) {
            return (Route route) => !route.willHandlePopInternally
                                    && route is ModalRoute
                                    && route.settings.name == name;
        }

        public abstract Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation);

        public virtual Widget buildTransitions(
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            return child;
        }

        public readonly FocusScopeNode focusScopeNode = new FocusScopeNode();

        protected internal override void install(OverlayEntry insertionPoint) {
            base.install(insertionPoint);
            _animationProxy = new ProxyAnimation(base.animation);
            _secondaryAnimationProxy = new ProxyAnimation(base.secondaryAnimation);
        }

        protected internal override TickerFuture didPush() {
            navigator.focusScopeNode.setFirstFocus(focusScopeNode);
            return base.didPush();
        }

        protected internal override void dispose() {
            focusScopeNode.detach();
            base.dispose();
        }

        public virtual bool barrierDismissible { get; }

        public virtual Color barrierColor { get; }

        public virtual bool maintainState { get; }

        public bool offstage {
            get { return _offstage; }
            set {
                if (_offstage == value) {
                    return;
                }

                setState(() => { _offstage = value; });
                _animationProxy.parent = _offstage ? Animations.kAlwaysCompleteAnimation : base.animation;
                _secondaryAnimationProxy.parent =
                    _offstage ? Animations.kAlwaysDismissedAnimation : base.secondaryAnimation;
            }
        }

        bool _offstage = false;

        public BuildContext subtreeContext {
            get { return _subtreeKey.currentContext; }
        }

        public override Animation<float> animation {
            get { return _animationProxy; }
        }

        ProxyAnimation _animationProxy;

        public override Animation<float> secondaryAnimation {
            get { return _secondaryAnimationProxy; }
        }

        ProxyAnimation _secondaryAnimationProxy;

        readonly List<WillPopCallback> _willPopCallbacks = new List<WillPopCallback>();

        public override Future<RoutePopDisposition> willPop() {
            _ModalScopeState scope = _scopeKey.currentState;
            D.assert(scope != null);

            bool result = false;
            foreach (WillPopCallback callback in _willPopCallbacks) {
                callback.Invoke().then(v => result = !(bool)v);
                if (result) {
                    return  Future<RoutePopDisposition>.value(RoutePopDisposition.doNotPop).to<RoutePopDisposition>();
                }
            }
            return base.willPop();
            
            // var callbacks = new List<WillPopCallback>(_willPopCallbacks);
            // Promise<RoutePopDisposition> result = new Promise<RoutePopDisposition>();
            // Action<int> fn = null;
            // fn = (int index) => {
            //     if (index < callbacks.Count) {
            //         callbacks[index]().Then((pop) => {
            //             if (!pop) {
            //                 result.Resolve(RoutePopDisposition.doNotPop);
            //             }
            //             else {
            //                 fn(index + 1);
            //             }
            //         });
            //     }
            //     else {
            //         base.willPop().Then((pop) => result.Resolve(pop));
            //     }
            // };
            // fn(0);
            // return result;
        }

        public void addScopedWillPopCallback(WillPopCallback callback) {
            D.assert(_scopeKey.currentState != null,
                () => "Tried to add a willPop callback to a route that is not currently in the tree.");
            _willPopCallbacks.Add(callback);
        }

        public void removeScopedWillPopCallback(WillPopCallback callback) {
            D.assert(_scopeKey.currentState != null,
                () => "Tried to remove a willPop callback from a route that is not currently in the tree.");
            _willPopCallbacks.Remove(callback);
        }

        public bool hasScopedWillPopCallback {
            get { return _willPopCallbacks.isNotEmpty(); }
        }

        protected internal override void didChangePrevious(Route previousRoute) {
            base.didChangePrevious(previousRoute);
            changedInternalState();
        }

        protected internal override void changedInternalState() {
            base.changedInternalState();
            setState(() => { });
            _modalBarrier.markNeedsBuild();
        }

        protected internal override void changedExternalState() {
            base.changedExternalState();
            _scopeKey.currentState?._forceRebuildPage();
        }

        public bool canPop {
            get { return !isFirst || willHandlePopInternally; }
        }


        readonly GlobalKey<_ModalScopeState> _scopeKey = new LabeledGlobalKey<_ModalScopeState>();
        internal readonly GlobalKey _subtreeKey = new LabeledGlobalKey<_ModalScopeState>();
        internal readonly PageStorageBucket _storageBucket = new PageStorageBucket();

        static readonly Animatable<float> _easeCurveTween = new CurveTween(curve: Curves.ease);
        OverlayEntry _modalBarrier;

        Widget _buildModalBarrier(BuildContext context) {
            Widget barrier;
            if (barrierColor != null && !offstage) {
                // changedInternalState is called if these update
                D.assert(barrierColor != _kTransparent);
                Animation<Color> color =
                    new ColorTween(
                        begin: _kTransparent,
                        end: barrierColor // changedInternalState is called if this updates
                    ).chain(_easeCurveTween).animate(animation);
                barrier = new AnimatedModalBarrier(
                    color: color,
                    dismissible: barrierDismissible
                );
            }
            else {
                barrier = new ModalBarrier(
                    dismissible: barrierDismissible
                );
            }

            return new IgnorePointer(
                ignoring: animation.status == AnimationStatus.reverse ||
                          animation.status == AnimationStatus.dismissed,
                child: barrier
            );
        }

        Widget _modalScopeCache;

        Widget _buildModalScope(BuildContext context) {
            return _modalScopeCache = _modalScopeCache ?? new _ModalScope(
                key: _scopeKey,
                route: this
                // _ModalScope calls buildTransitions() and buildChild(), defined above
            );
        }

        public override ICollection<OverlayEntry> createOverlayEntries() {
            _modalBarrier = new OverlayEntry(builder: _buildModalBarrier);
            var content = new OverlayEntry(
                builder: _buildModalScope, maintainState: maintainState
            );
            return new List<OverlayEntry> {_modalBarrier, content};
        }

        public override string ToString() {
            return $"{GetType()}({settings}, animation: {_animation})";
        }
    }

    public abstract class PopupRoute : ModalRoute {
        protected PopupRoute(
            RouteSettings settings = null
        ) : base(settings: settings) {
        }

        public override bool opaque {
            get { return false; }
        }

        public override bool maintainState {
            get { return true; }
        }
    }

    public class RouteObserve<R> : NavigatorObserver where R : Route {
        readonly Dictionary<R, HashSet<RouteAware>> _listeners = new Dictionary<R, HashSet<RouteAware>>();

        public void subscribe(RouteAware routeAware, R route) {
            D.assert(routeAware != null);
            D.assert(route != null);
            HashSet<RouteAware> subscribers = _listeners.putIfAbsent(route, () => new HashSet<RouteAware>());
            if (subscribers.Add(routeAware)) {
                routeAware.didPush();
            }
        }

        public void unsubscribe(RouteAware routeAware) {
            D.assert(routeAware != null);
            foreach (R route in _listeners.Keys) {
                HashSet<RouteAware> subscribers = _listeners[route];
                subscribers?.Remove(routeAware);
            }
        }

        public override void didPop(Route route, Route previousRoute) {
            if (route is R && previousRoute is R) {
                var previousSubscribers = _listeners.getOrDefault((R) previousRoute);

                if (previousSubscribers != null) {
                    foreach (RouteAware routeAware in previousSubscribers) {
                        routeAware.didPopNext();
                    }
                }

                var subscribers = _listeners.getOrDefault((R) route);

                if (subscribers != null) {
                    foreach (RouteAware routeAware in subscribers) {
                        routeAware.didPop();
                    }
                }
            }
        }

        public override void didPush(Route route, Route previousRoute) {
            if (route is R && previousRoute is R) {
                var previousSubscribers = _listeners.getOrDefault((R) previousRoute);

                if (previousSubscribers != null) {
                    foreach (RouteAware routeAware in previousSubscribers) {
                        routeAware.didPushNext();
                    }
                }
            }
        }
    }

    public interface RouteAware {
        void didPopNext();

        void didPush();

        void didPop();

        void didPushNext();
    }

    class _DialogRoute : PopupRoute {
        internal _DialogRoute(RoutePageBuilder pageBuilder = null, bool barrierDismissible = true,
            Color barrierColor = null,
            TimeSpan? transitionDuration = null,
            RouteTransitionsBuilder transitionBuilder = null,
            RouteSettings setting = null) : base(settings: setting) {
            _pageBuilder = pageBuilder;
            this.barrierDismissible = barrierDismissible;
            this.barrierColor = barrierColor ?? new Color(0x80000000);
            this.transitionDuration = transitionDuration ?? TimeSpan.FromMilliseconds(200);
            _transitionBuilder = transitionBuilder;
        }

        readonly RoutePageBuilder _pageBuilder;

        public override bool barrierDismissible { get; }

        public override Color barrierColor { get; }

        public override TimeSpan transitionDuration { get; }

        readonly RouteTransitionsBuilder _transitionBuilder;

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return _pageBuilder(context, animation, secondaryAnimation);
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            if (_transitionBuilder == null) {
                return new FadeTransition(
                    opacity: new CurvedAnimation(
                        parent: animation,
                        curve: Curves.linear
                    ),
                    child: child);
            }

            return _transitionBuilder(context, animation, secondaryAnimation, child);
        }
    }

    public static class DialogUtils {
        public static Future<object> showGeneralDialog(
            BuildContext context = null,
            RoutePageBuilder pageBuilder = null,
            bool barrierDismissible = false,
            Color barrierColor = null,
            TimeSpan? transitionDuration = null,
            RouteTransitionsBuilder transitionBuilder = null
        ) {
            D.assert(pageBuilder != null);
            return Navigator.of(context, rootNavigator: true).push(new _DialogRoute(
                pageBuilder: pageBuilder,
                barrierDismissible: barrierDismissible,
                barrierColor: barrierColor,
                transitionDuration: transitionDuration,
                transitionBuilder: transitionBuilder
            )).to<object>();
        }
    }

    public delegate Widget RoutePageBuilder(BuildContext context, Animation<float> animation,
        Animation<float> secondaryAnimation);

    public delegate Widget RouteTransitionsBuilder(BuildContext context, Animation<float> animation,
        Animation<float> secondaryAnimation, Widget child);
}