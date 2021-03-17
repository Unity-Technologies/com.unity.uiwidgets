using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate Tween<Rect> CreateRectTween(Rect begin, Rect end);
    public delegate Widget HeroPlaceholderBuilder(
        BuildContext context,
        Size heroSize,
        Widget child
    );
    public delegate Widget HeroFlightShuttleBuilder(
        BuildContext flightContext,
        Animation<float> animation,
        HeroFlightDirection flightDirection,
        BuildContext fromHeroContext,
        BuildContext toHeroContext
    );

    delegate void _OnFlightEnded(_HeroFlight flight);

    public enum HeroFlightDirection {
        push,
        pop
    }

    class HeroUtils {
        public  static Rect _boundingBoxFor(BuildContext context, BuildContext ancestorContext = null) {
            RenderBox box = context.findRenderObject() as RenderBox;
            D.assert(box != null && box.hasSize);
            return MatrixUtils.transformRect(
                box.getTransformTo(ancestorContext?.findRenderObject()),
                Offset.zero & box.size
            );
        }
    }


    public class Hero : StatefulWidget {
        public Hero(
            Key key = null,
            object tag = null,
            CreateRectTween createRectTween = null,
            HeroFlightShuttleBuilder flightShuttleBuilder = null,
            HeroPlaceholderBuilder placeholderBuilder = null,
            bool transitionOnUserGestures = false,
            Widget child = null
        ) : base(key: key) {
            D.assert(tag != null);
            D.assert(child != null);
            this.tag = tag;
            this.createRectTween = createRectTween;
            this.child = child;
            this.flightShuttleBuilder = flightShuttleBuilder;
            this.placeholderBuilder = placeholderBuilder;
            this.transitionOnUserGestures = transitionOnUserGestures;
        }


        public readonly object tag;
        public readonly CreateRectTween createRectTween;
        public readonly Widget child;
        public readonly HeroFlightShuttleBuilder flightShuttleBuilder;
        public readonly HeroPlaceholderBuilder placeholderBuilder;
        

        public readonly bool transitionOnUserGestures;

        internal static Dictionary<object, _HeroState> _allHeroesFor(
            BuildContext context, 
            bool isUserGestureTransition, 
            NavigatorState navigator) {
            
            D.assert(context != null);
            D.assert(navigator != null);
            
            Dictionary<object, _HeroState> result = new Dictionary<object, _HeroState> { };
            
            void inviteHero(StatefulElement hero, object tag) {
                D.assert(()=> {
                    if (result.ContainsKey(tag)) {
                        throw new UIWidgetsError(
                            new List<DiagnosticsNode>()
                            {
                                new ErrorSummary("There are multiple heroes that share the same tag within a subtree."),
                                new ErrorDescription(
                                    "Within each subtree for which heroes are to be animated (i.e. a PageRoute subtree), "+
                                "each Hero must have a unique non-null tag.\n"+
                                $"In this case, multiple heroes had the following tag: {tag}\n"
                                ),
                                new DiagnosticsProperty<StatefulElement>("Here is the subtree for one of the offending heroes", hero, linePrefix: "# ", style: DiagnosticsTreeStyle.dense),

                                
                            });
                    }
                    return true;
                });
                Hero heroWidget = hero.widget as Hero;
                _HeroState heroState = hero.state as _HeroState;
                if (!isUserGestureTransition || heroWidget.transitionOnUserGestures) {
                    result[tag] = heroState;
                } else {
                    heroState.ensurePlaceholderIsHidden();
                }
            }

            void visitor(Element element) {
                Widget widget = element.widget;
                if (widget is Hero) {
                    StatefulElement hero = element as StatefulElement;
                    object tag = ((Hero)widget).tag;
                    D.assert(tag != null);
                    if (Navigator.of(hero) == navigator) {
                        inviteHero(hero, tag);
                    } else {
                        ModalRoute heroRoute = ModalRoute.of(hero);
                        if (heroRoute != null && heroRoute is PageRoute && heroRoute.isCurrent) {
                            inviteHero(hero, tag);
                        }
                    }
                }

                element.visitChildren(visitor);
            }

            context.visitChildElements(visitor);
            return result;
        }

        public override State createState() {
            return new _HeroState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("tag", tag));
        }
    }

    class _HeroState : State<Hero> {
        
        GlobalKey _key = GlobalKey.key();
        Size _placeholderSize;
        bool _shouldIncludeChild = true;

        public void startFlight( bool shouldIncludedChildInPlaceholder = false ) {
            _shouldIncludeChild = shouldIncludedChildInPlaceholder;
            D.assert(mounted);
            RenderBox box = (RenderBox) context.findRenderObject();
            D.assert(box != null && box.hasSize);
            setState(() => { _placeholderSize = box.size; });
        }


        public void ensurePlaceholderIsHidden() {
            if (mounted) {
                setState(() => { _placeholderSize = null; });
            }
        }
        public void endFlight(bool keepPlaceholder = false ) {
            if (!keepPlaceholder) {
                ensurePlaceholderIsHidden();
            }
        }
        public override Widget build(BuildContext context) {
            D.assert(
                context.findAncestorWidgetOfExactType<Hero>() == null,
                () => "A Hero widget cannot be the descendant of another Hero widget.");

            bool showPlaceholder = _placeholderSize != null;

            if (showPlaceholder && widget.placeholderBuilder != null) {
                return widget.placeholderBuilder(context, _placeholderSize, widget.child);
            }

            if (showPlaceholder && !_shouldIncludeChild) {
                    return new SizedBox(
                        width: _placeholderSize.width,
                        height: _placeholderSize.height
                    );
            }
            return new SizedBox(
                width: _placeholderSize?.width,
                height: _placeholderSize?.height,
                child: new Offstage(
                    offstage: showPlaceholder,
                    child: new TickerMode(
                        enabled: !showPlaceholder,
                        child: new KeyedSubtree(key: _key, child: widget.child)
                    )
                )
            );
        }
    }

    class _HeroFlightManifest {
        public _HeroFlightManifest(
            HeroFlightDirection type = default,
            OverlayState overlay = null ,
            Rect navigatorRect = null,
            PageRoute fromRoute = null,
            PageRoute toRoute = null,
            _HeroState fromHero = null, 
            _HeroState toHero = null,
            CreateRectTween createRectTween = null,
            HeroFlightShuttleBuilder shuttleBuilder = null,
            bool? isUserGestureTransition = null,
            bool? isDiverted = null
        ) {
            D.assert(fromHero.widget.tag.Equals(toHero.widget.tag));
            this.type = type;
            this.overlay = overlay;
            this.navigatorRect = navigatorRect;
            this.fromRoute = fromRoute;
            this.toRoute = toRoute;
            this.fromHero = fromHero;
            this.toHero = toHero;
            this.createRectTween = createRectTween;
            this.shuttleBuilder = shuttleBuilder;
            this.isUserGestureTransition = isUserGestureTransition ?? false;
            this.isDiverted = isDiverted ?? false;
        }

        public readonly HeroFlightDirection type;
        public readonly OverlayState overlay;
        public readonly Rect navigatorRect;
        public readonly PageRoute fromRoute;
        public readonly PageRoute toRoute;
        public readonly _HeroState fromHero;
        public readonly _HeroState toHero;
        public readonly CreateRectTween createRectTween;
        public readonly HeroFlightShuttleBuilder shuttleBuilder;
        public readonly bool isUserGestureTransition;
        public readonly bool isDiverted;

        public object tag {
            get { return fromHero.widget.tag; }
        }

        public Animation<float> animation {
            get {
                return new CurvedAnimation(
                    parent: (type == HeroFlightDirection.push) ? toRoute.animation : fromRoute.animation,
                    curve: Curves.fastOutSlowIn, 
                    reverseCurve: isDiverted ? null : Curves.fastOutSlowIn.flipped
                );
            }
        }

        public override string ToString() {
            return $"_HeroFlightManifest({type} tag: {tag} from route: {fromRoute.settings} " +
                   $"to route: {toRoute.settings} with hero: {fromHero} to {toHero})";
        }
    }

    class _HeroFlight {
        public _HeroFlight(_OnFlightEnded onFlightEnded ) {
            this.onFlightEnded = onFlightEnded;
            _proxyAnimation = new ProxyAnimation();
            _proxyAnimation.addStatusListener(_handleAnimationUpdate);
        }

        public readonly _OnFlightEnded onFlightEnded;

        Tween<Rect> heroRectTween;
        Widget shuttle;

        Animation<float> _heroOpacity = Animations.kAlwaysCompleteAnimation;
        public ProxyAnimation _proxyAnimation;
        public _HeroFlightManifest manifest;
        public OverlayEntry overlayEntry;
        bool _aborted = false;

        Tween<Rect> _doCreateRectTween(Rect begin, Rect end) {
            CreateRectTween createRectTween =
                manifest.toHero.widget.createRectTween ?? manifest.createRectTween;
            if (createRectTween != null) {
                return createRectTween(begin, end);
            }

            return new RectTween(begin: begin, end: end);
        }

        static readonly Animatable<float> _reverseTween = new FloatTween(begin: 1.0f, end: 0.0f);

        Widget _buildOverlay(BuildContext context) {
            D.assert(manifest != null);
            shuttle = shuttle ?? manifest.shuttleBuilder(
                               context, 
                               manifest.animation, 
                               manifest.type, 
                               manifest.fromHero.context,
                               manifest.toHero.context);
            D.assert(shuttle != null);

            return new AnimatedBuilder(
                animation: _proxyAnimation,
                child: shuttle,
                builder: (BuildContext _, Widget child) => {
                    RenderBox toHeroBox = manifest.toHero.context?.findRenderObject() as RenderBox;
                    if (_aborted || toHeroBox == null || !toHeroBox.attached) {
                        if (_heroOpacity.isCompleted) {
                            _heroOpacity = _proxyAnimation.drive(
                                _reverseTween.chain(
                                    new CurveTween(curve: new Interval(_proxyAnimation.value, 1.0f)))
                            );
                        }
                    }
                    else if (toHeroBox.hasSize) {
                        RenderBox finalRouteBox = manifest.toRoute.subtreeContext?.findRenderObject() as RenderBox;
                        Offset toHeroOrigin = toHeroBox.localToGlobal(Offset.zero, ancestor: finalRouteBox);
                        if (toHeroOrigin != heroRectTween.end.topLeft) {
                            Rect heroRectEnd = toHeroOrigin & heroRectTween.end.size;
                            heroRectTween = _doCreateRectTween(heroRectTween.begin, heroRectEnd);
                        }
                    }

                    Rect rect = heroRectTween.evaluate(_proxyAnimation);
                    Size size = manifest.navigatorRect.size;
                    RelativeRect offsets = RelativeRect.fromSize(rect, size);

                    return new Positioned(
                        top: offsets.top,
                        right: offsets.right,
                        bottom: offsets.bottom,
                        left: offsets.left,
                        child: new IgnorePointer(
                            child: new RepaintBoundary(
                                child: new Opacity(
                                    opacity: _heroOpacity.value,
                                    child: child
                                )
                            )
                        )
                    );
                }
            );
        }

        public void _handleAnimationUpdate(AnimationStatus status) {
            if (status == AnimationStatus.completed || status == AnimationStatus.dismissed) {
                _proxyAnimation.parent = null;
                D.assert(overlayEntry != null);
                overlayEntry.remove();
                overlayEntry = null;
                manifest.fromHero.endFlight(keepPlaceholder: status == AnimationStatus.completed);
                manifest.toHero.endFlight(keepPlaceholder: status == AnimationStatus.dismissed);
                onFlightEnded(this);
            }
        }

        public void start(_HeroFlightManifest initialManifest) {
            D.assert(!_aborted);
            D.assert(() => {
                Animation<float> initial = initialManifest.animation;
                D.assert(initial != null);
                HeroFlightDirection type = initialManifest.type;
                switch (type) {
                    case HeroFlightDirection.pop:
                        return initial.value == 1.0f && 
                               initialManifest.isUserGestureTransition 
                            ? initial.status == AnimationStatus.completed
                            : initial.status == AnimationStatus.reverse;
                    case HeroFlightDirection.push:
                        return initial.value == 0.0f && initial.status == AnimationStatus.forward;
                }

                throw new Exception("Unknown type: " + type);
            });

            manifest = initialManifest;

            if (manifest.type == HeroFlightDirection.pop) {
                _proxyAnimation.parent = new ReverseAnimation(manifest.animation);
            }
            else {
                _proxyAnimation.parent = manifest.animation;
            }
            manifest.fromHero.startFlight(shouldIncludedChildInPlaceholder: manifest.type == HeroFlightDirection.push);
            manifest.toHero.startFlight();
            
            heroRectTween = _doCreateRectTween(
                HeroUtils._boundingBoxFor(manifest.fromHero.context, manifest.fromRoute.subtreeContext),
                HeroUtils._boundingBoxFor(manifest.toHero.context, manifest.toRoute.subtreeContext)

            );

            overlayEntry = new OverlayEntry(builder: _buildOverlay);
            manifest.overlay.insert(overlayEntry);
        }

        public void divert(_HeroFlightManifest newManifest) {
            D.assert(manifest.tag == newManifest.tag);
            if (manifest.type == HeroFlightDirection.push && newManifest.type == HeroFlightDirection.pop) {
                D.assert(newManifest.animation.status == AnimationStatus.reverse);
                D.assert(manifest.fromHero == newManifest.toHero);
                D.assert(manifest.toHero == newManifest.fromHero);
                D.assert(manifest.fromRoute == newManifest.toRoute);
                D.assert(manifest.toRoute == newManifest.fromRoute);

                _proxyAnimation.parent = new ReverseAnimation(newManifest.animation);
                heroRectTween = new ReverseTween<Rect>(heroRectTween);
            }
            else if (manifest.type == HeroFlightDirection.pop && newManifest.type == HeroFlightDirection.push) {
                D.assert(newManifest.animation.status == AnimationStatus.forward);
                D.assert(manifest.toHero == newManifest.fromHero);
                D.assert(manifest.toRoute == newManifest.fromRoute);

                _proxyAnimation.parent = newManifest.animation.drive(
                    new FloatTween(
                        begin: manifest.animation.value,
                        end: 1.0f
                    )
                );

                if (manifest.fromHero != newManifest.toHero) {
                    manifest.fromHero.endFlight(keepPlaceholder: true);
                    newManifest.toHero.startFlight();
                    heroRectTween = _doCreateRectTween(
                        heroRectTween.end,
                        HeroUtils._boundingBoxFor(newManifest.toHero.context, newManifest.toRoute.subtreeContext)
                    );
                }
                else {
                    heroRectTween = _doCreateRectTween(heroRectTween.end, heroRectTween.begin);
                }
            }
            else {
                D.assert(manifest.fromHero != newManifest.fromHero);
                D.assert(manifest.toHero != newManifest.toHero);
                
                heroRectTween = _doCreateRectTween(
                    heroRectTween.evaluate(_proxyAnimation),
                    HeroUtils._boundingBoxFor(newManifest.toHero.context, newManifest.toRoute.subtreeContext)
                );
                shuttle = null;

                if (newManifest.type == HeroFlightDirection.pop) {
                    _proxyAnimation.parent = new ReverseAnimation(newManifest.animation);
                }
                else {
                    _proxyAnimation.parent = newManifest.animation;
                }
                manifest.fromHero.endFlight(keepPlaceholder: true);
                manifest.toHero.endFlight(keepPlaceholder: true);

                // Let the heroes in each of the routes rebuild with their placeholders.
                newManifest.fromHero.startFlight(shouldIncludedChildInPlaceholder: newManifest.type == HeroFlightDirection.push);
                newManifest.toHero.startFlight();

                overlayEntry.markNeedsBuild();
            }

            _aborted = false;
            manifest = newManifest;
        }

        public void abort() {
            _aborted = true;
        }

        public override string ToString() {
            RouteSettings from = manifest.fromRoute.settings;
            RouteSettings to = manifest.toRoute.settings;
            object tag = manifest.tag;
            return $"HeroFlight(for: {tag}, from: {from}, to: {to} {_proxyAnimation.parent})";
        }
    }

    public class HeroController : NavigatorObserver {
        public HeroController(CreateRectTween createRectTween = null) {
            this.createRectTween = createRectTween;
        }

        public readonly CreateRectTween createRectTween;

        Dictionary<object, _HeroFlight> _flights = new Dictionary<object, _HeroFlight>();

        public override void didPush(Route route, Route previousRoute) {
            D.assert(navigator != null);
            D.assert(route != null);
            _maybeStartHeroTransition(previousRoute, route, HeroFlightDirection.push, false);
        }

        public override void didPop(Route route, Route previousRoute) {
            D.assert(navigator != null);
            D.assert(route != null);
            if (!navigator.userGestureInProgress) {
                _maybeStartHeroTransition(route, previousRoute, HeroFlightDirection.pop, false);
            }
        }

        public override void didReplace(Route newRoute = null, Route oldRoute = null) {
            D.assert(navigator != null);
            if (newRoute?.isCurrent == true) {
                _maybeStartHeroTransition(oldRoute, newRoute, HeroFlightDirection.push, false);
            }
        }

        public override void didStartUserGesture(Route route, Route previousRoute) {
            D.assert(navigator != null);
            D.assert(route != null);
            _maybeStartHeroTransition(route, previousRoute, HeroFlightDirection.pop, true);
        }
        public override void didStopUserGesture() {
            if (navigator.userGestureInProgress)
                return;

            bool isInvalidFlight(_HeroFlight flight) {
                return flight.manifest.isUserGestureTransition
                       && flight.manifest.type == HeroFlightDirection.pop
                       && flight._proxyAnimation.isDismissed;
            }

            List<_HeroFlight> invalidFlights = LinqUtils<_HeroFlight>.WhereList(_flights.Values, isInvalidFlight);
             foreach ( _HeroFlight flight in invalidFlights) {
                flight._handleAnimationUpdate(AnimationStatus.dismissed);
            }
        }


        void _maybeStartHeroTransition(
            Route fromRoute,
            Route toRoute,
            HeroFlightDirection flightType,
            bool isUserGestureTransition
        ) {
            if (toRoute != fromRoute && toRoute is PageRoute && fromRoute is PageRoute) {
                PageRoute from = (PageRoute) fromRoute;
                PageRoute to = (PageRoute) toRoute;
                Animation<float> animation = (flightType == HeroFlightDirection.push) ? to.animation : from.animation;

                switch (flightType) {
                    case HeroFlightDirection.pop:
                        if (animation.value == 0.0f) {
                            return;
                        }

                        break;
                    case HeroFlightDirection.push:
                        if (animation.value == 1.0f) {
                            return;
                        }

                        break;
                }

                if (isUserGestureTransition && flightType == HeroFlightDirection.pop && to.maintainState) {
                    _startHeroTransition(from, to, animation, flightType, isUserGestureTransition);
                }
                else {
                    to.offstage = to.animation.value == 0.0f;

                    WidgetsBinding.instance.addPostFrameCallback((TimeSpan value) => {
                        _startHeroTransition(from, to, animation, flightType, isUserGestureTransition);
                    });
                }
            }
        }

        void _startHeroTransition(
            PageRoute from,
            PageRoute to,
            Animation<float> animation,
            HeroFlightDirection flightType,
            bool isUserGestureTransition
        ) {
            if (navigator == null || from.subtreeContext == null || to.subtreeContext == null) {
                to.offstage = false; // in case we set this in _maybeStartHeroTransition
                return;
            }

            Rect navigatorRect = HeroUtils._boundingBoxFor(navigator.context);//_globalBoundingBoxFor(navigator.context);

            Dictionary<object, _HeroState> fromHeroes =
                Hero._allHeroesFor(from.subtreeContext, isUserGestureTransition, navigator);
            Dictionary<object, _HeroState> toHeroes =
                Hero._allHeroesFor(to.subtreeContext, isUserGestureTransition, navigator);

            to.offstage = false;

            foreach (object tag in fromHeroes.Keys) {
                if (toHeroes.ContainsKey(tag)) {
                    HeroFlightShuttleBuilder fromShuttleBuilder = fromHeroes[tag].widget.flightShuttleBuilder;
                    HeroFlightShuttleBuilder toShuttleBuilder = toHeroes[tag].widget.flightShuttleBuilder;
                    bool isDiverted = _flights.ContainsKey(tag);
                    _HeroFlightManifest manifest = new _HeroFlightManifest(
                        type: flightType,
                        overlay: navigator.overlay,
                        navigatorRect: navigatorRect,
                        fromRoute: from,
                        toRoute: to,
                        fromHero: fromHeroes[tag],
                        toHero: toHeroes[tag],
                        createRectTween: createRectTween,
                        shuttleBuilder:
                        toShuttleBuilder ?? fromShuttleBuilder ?? _defaultHeroFlightShuttleBuilder,
                        isUserGestureTransition: isUserGestureTransition,
                        isDiverted: isDiverted
                    );

                    //if (_flights.TryGetValue(tag, out var result)) {
                    if (isDiverted){
                        _flights[tag].divert(manifest);
                    }
                    else {
                        _flights[tag] = new _HeroFlight(_handleFlightEnded);
                        _flights[tag].start(manifest);
                    }
                }
                else if (_flights.TryGetValue(tag, out var result)) {
                    result.abort();
                }
            }
            foreach (object tag in toHeroes.Keys) {
                _HeroState heroState = null;
                if (!fromHeroes.TryGetValue(tag, out heroState)) {
                    toHeroes[tag].ensurePlaceholderIsHidden();
                }
                
            }
        }

        void _handleFlightEnded(_HeroFlight flight) {
            _flights.Remove(flight.manifest.tag);
        }

        static readonly HeroFlightShuttleBuilder _defaultHeroFlightShuttleBuilder = (
            BuildContext flightContext,
            Animation<float> animation,
            HeroFlightDirection flightDirection,
            BuildContext fromHeroContext,
            BuildContext toHeroContext
        ) => {
            Hero toHero = (Hero) toHeroContext.widget;
            return toHero.child;
        };
    }
}
