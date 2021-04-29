using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.material {
    public class _FadeUpwardsPageTransition : StatelessWidget {
        public _FadeUpwardsPageTransition(
            Key key = null,
            Animation<float> routeAnimation = null,
            Widget child = null) : base(key: key) {
            D.assert(routeAnimation != null);
            D.assert(child != null);
            _positionAnimation = routeAnimation.drive(_bottomUpTween.chain(_fastOutSlowInTween));
            _opacityAnimation = routeAnimation.drive(_easeInTween);
            this.child = child;
        }

        static readonly Tween<Offset> _bottomUpTween = new OffsetTween(
            begin: new Offset(0.0f, 0.25f),
            end: Offset.zero
        );

        static readonly Animatable<float> _fastOutSlowInTween = new CurveTween(
            curve: Curves.fastOutSlowIn);

        static readonly Animatable<float> _easeInTween = new CurveTween(
            curve: Curves.easeIn);

        public readonly Animation<Offset> _positionAnimation;
        public readonly Animation<float> _opacityAnimation;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new SlideTransition(
                position: _positionAnimation,
                child: new FadeTransition(
                    opacity: _opacityAnimation,
                    child: child));
        }
    }

    class _OpenUpwardsPageTransition : StatelessWidget {
        public _OpenUpwardsPageTransition(
            Key key = null,
            Animation<float> animation = null,
            Animation<float> secondaryAnimation = null,
            Widget child = null
        ) : base(key: key) {
            this.animation = animation;
            this.secondaryAnimation = secondaryAnimation;
            this.child = child;
        }

        static readonly OffsetTween _primaryTranslationTween = new OffsetTween(
            begin: new Offset(0.0f, 0.05f),
            end: Offset.zero
        );

        static readonly OffsetTween _secondaryTranslationTween = new OffsetTween(
            begin: Offset.zero,
            end: new Offset(0.0f, -0.025f)
        );

        static readonly FloatTween _scrimOpacityTween = new FloatTween(
            begin: 0.0f,
            end: 0.25f
        );

        static readonly Curve _transitionCurve = new Cubic(0.20f, 0.00f, 0.00f, 1.00f);

        public readonly Animation<float> animation;
        public readonly Animation<float> secondaryAnimation;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new LayoutBuilder(
                builder: (BuildContext _context, BoxConstraints constraints) => {
                    Size size = constraints.biggest;

                    CurvedAnimation primaryAnimation = new CurvedAnimation(
                        parent: animation,
                        curve: _transitionCurve,
                        reverseCurve: _transitionCurve.flipped
                    );

                    Animation<float> clipAnimation = new FloatTween(
                        begin: 0.0f,
                        end: size.height
                    ).animate(primaryAnimation);

                    Animation<float> opacityAnimation = _scrimOpacityTween.animate(primaryAnimation);
                    Animation<Offset> primaryTranslationAnimation = _primaryTranslationTween.animate(primaryAnimation);

                    Animation<Offset> secondaryTranslationAnimation = _secondaryTranslationTween.animate(
                        new CurvedAnimation(
                            parent: secondaryAnimation,
                            curve: _transitionCurve,
                            reverseCurve: _transitionCurve.flipped
                        )
                    );

                    return new AnimatedBuilder(
                        animation: animation,
                        builder: (BuildContext _, Widget child) => {
                            return new Container(
                                color: Colors.black.withOpacity(opacityAnimation.value),
                                alignment: Alignment.bottomLeft,
                                child: new ClipRect(
                                    child: new SizedBox(
                                        height: clipAnimation.value,
                                        child: new OverflowBox(
                                            alignment: Alignment.bottomLeft,
                                            maxHeight: size.height,
                                            child: child
                                        )
                                    )
                                )
                            );
                        },
                        child: new AnimatedBuilder(
                            animation: secondaryAnimation,
                            child: new FractionalTranslation(
                                translation: primaryTranslationAnimation.value,
                                child: this.child
                            ),
                            builder: (BuildContext _, Widget child) => {
                                return new FractionalTranslation(
                                    translation: secondaryTranslationAnimation.value,
                                    child: child
                                );
                            }
                        )
                    );
                }
            );
        }
    }

    class _ZoomPageTransition : StatefulWidget {
        internal _ZoomPageTransition(
            Key key = null,
            Animation<float> animation = null,
            Animation<float> secondaryAnimation = null,
            Widget child = null
        ) : base(key: key) {
            this.animation = animation;
            this.secondaryAnimation = secondaryAnimation;
            this.child = child;
        }

        // The scrim obscures the old page by becoming increasingly opaque.
        internal static readonly Tween<float> _scrimOpacityTween = new FloatTween(
            begin: 0.0f,
            end: 0.60f
        );

        // A curve sequence that is similar to the 'fastOutExtraSlowIn' curve used in
        // the native transition.
        public static readonly List<TweenSequenceItem<float>> fastOutExtraSlowInTweenSequenceItems =
            new List<TweenSequenceItem<float>> {
                new TweenSequenceItem<float>(
                    tween: new FloatTween(begin: 0.0f, end: 0.4f)
                        .chain(new CurveTween(curve: new Cubic(0.05f, 0.0f, 0.133333f, 0.06f))),
                    weight: 0.166666f
                ),
                new TweenSequenceItem<float>(
                    tween: new FloatTween(begin: 0.4f, end: 1.0f)
                        .chain(new CurveTween(curve: new Cubic(0.208333f, 0.82f, 0.25f, 1.0f))),
                    weight: 1.0f - 0.166666f
                )
            };

        internal static readonly TweenSequence<float> _scaleCurveSequence =
            new TweenSequence<float>(fastOutExtraSlowInTweenSequenceItems);

        internal static readonly FlippedTweenSequence _flippedScaleCurveSequence =
            new FlippedTweenSequence(fastOutExtraSlowInTweenSequenceItems);

        public readonly Animation<float> animation;
        public readonly Animation<float> secondaryAnimation;
        public readonly Widget child;

        public override State createState() => new __ZoomPageTransitionState();
    }


    class __ZoomPageTransitionState : State<_ZoomPageTransition> {
        AnimationStatus _currentAnimationStatus;
        AnimationStatus _lastAnimationStatus;

        public override void initState() {
            base.initState();
            widget.animation.addStatusListener((AnimationStatus animationStatus) => {
                _lastAnimationStatus = _currentAnimationStatus;
                _currentAnimationStatus = animationStatus;
            });
        }

        // This check ensures that the animation reverses the original animation if
        // the transition were interruped midway. This prevents a disjointed
        // experience since the reverse animation uses different fade and scaling
        // curves.
        bool _transitionWasInterrupted {
            get {
                bool wasInProgress = false;
                bool isInProgress = false;

                switch (_currentAnimationStatus) {
                    case AnimationStatus.completed:
                    case AnimationStatus.dismissed:
                        isInProgress = false;
                        break;
                    case AnimationStatus.forward:
                    case AnimationStatus.reverse:
                        isInProgress = true;
                        break;
                }

                switch (_lastAnimationStatus) {
                    case AnimationStatus.completed:
                    case AnimationStatus.dismissed:
                        wasInProgress = false;
                        break;
                    case AnimationStatus.forward:
                    case AnimationStatus.reverse:
                        wasInProgress = true;
                        break;
                }

                return wasInProgress && isInProgress;
            }
        }

        public override Widget build(BuildContext context) {
            Animation<float> _forwardScrimOpacityAnimation = widget.animation.drive(
                _ZoomPageTransition._scrimOpacityTween
                    .chain(new CurveTween(curve: new Interval(0.2075f, 0.4175f))));

            Animation<float> _forwardEndScreenScaleTransition = widget.animation.drive(
                new FloatTween(begin: 0.85f, end: 1.00f)
                    .chain(_ZoomPageTransition._scaleCurveSequence));

            Animation<float> _forwardStartScreenScaleTransition = widget.secondaryAnimation.drive(
                new FloatTween(begin: 1.00f, end: 1.05f)
                    .chain(_ZoomPageTransition._scaleCurveSequence));

            Animation<float> _forwardEndScreenFadeTransition = widget.animation.drive(
                new FloatTween(begin: 0.0f, end: 1.00f)
                    .chain(new CurveTween(curve: new Interval(0.125f, 0.250f))));

            Animation<float> _reverseEndScreenScaleTransition = widget.secondaryAnimation.drive(
                new FloatTween(begin: 1.00f, end: 1.10f)
                    .chain(_ZoomPageTransition._flippedScaleCurveSequence));

            Animation<float> _reverseStartScreenScaleTransition = widget.animation.drive(
                new FloatTween(begin: 0.9f, end: 1.0f)
                    .chain(_ZoomPageTransition._flippedScaleCurveSequence));

            Animation<float> _reverseStartScreenFadeTransition = widget.animation.drive(
                new FloatTween(begin: 0.0f, end: 1.00f)
                    .chain(new CurveTween(curve: new Interval(1 - 0.2075f, 1 - 0.0825f))));

            return new AnimatedBuilder(
                animation: widget.animation,
                builder: (BuildContext _context, Widget child) => {
                    if (widget.animation.status == AnimationStatus.forward || _transitionWasInterrupted) {
                        return new Container(
                            color: Colors.black.withOpacity(_forwardScrimOpacityAnimation.value),
                            child: new FadeTransition(
                                opacity: _forwardEndScreenFadeTransition,
                                child: new ScaleTransition(
                                    scale: _forwardEndScreenScaleTransition,
                                    child: child
                                )
                            )
                        );
                    }
                    else if (widget.animation.status == AnimationStatus.reverse) {
                        return new ScaleTransition(
                            scale: _reverseStartScreenScaleTransition,
                            child: new FadeTransition(
                                opacity: _reverseStartScreenFadeTransition,
                                child: child
                            )
                        );
                    }

                    return child;
                },
                child: new AnimatedBuilder(
                    animation: widget.secondaryAnimation,
                    builder: (BuildContext _context, Widget child) => {
                        if (widget.secondaryAnimation.status == AnimationStatus.forward || _transitionWasInterrupted) {
                            return new ScaleTransition(
                                scale: _forwardStartScreenScaleTransition,
                                child: child
                            );
                        }
                        else if (widget.secondaryAnimation.status == AnimationStatus.reverse) {
                            return new ScaleTransition(
                                scale: _reverseEndScreenScaleTransition,
                                child: child
                            );
                        }

                        return child;
                    },
                    child: widget.child
                )
            );
        }
    }


    public abstract class PageTransitionsBuilder {
        public PageTransitionsBuilder() {
        }

        public abstract Widget buildTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child);
    }

    public class ZoomPageTransitionsBuilder : PageTransitionsBuilder {
        public ZoomPageTransitionsBuilder() {
        }

        public override Widget buildTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            return new _ZoomPageTransition(
                animation: animation,
                secondaryAnimation: secondaryAnimation,
                child: child
            );
        }
    }

    public class FadeUpwardsPageTransitionsBuilder : PageTransitionsBuilder {
        public FadeUpwardsPageTransitionsBuilder() {
        }

        public override Widget buildTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child) {
            return new _FadeUpwardsPageTransition(
                routeAnimation: animation,
                child: child);
        }
    }

    public class OpenUpwardsPageTransitionsBuilder : PageTransitionsBuilder {
        public OpenUpwardsPageTransitionsBuilder() {
        }

        public override Widget buildTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            return new _OpenUpwardsPageTransition(
                animation: animation,
                secondaryAnimation: secondaryAnimation,
                child: child
            );
        }
    }

    public class CupertinoPageTransitionsBuilder : PageTransitionsBuilder {
        public CupertinoPageTransitionsBuilder() {
        }
        
        public override Widget buildTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            return CupertinoPageRoute.buildPageTransitions(route, context, animation, secondaryAnimation, child);
        }
    }

    public class PageTransitionsTheme : Diagnosticable, IEquatable<PageTransitionsTheme> {
        public PageTransitionsTheme(
            PageTransitionsBuilder builder = null) {
            _builder = builder;
        }

        static readonly Dictionary<RuntimePlatform, PageTransitionsBuilder> _defaultBuilders =
            new Dictionary<RuntimePlatform, PageTransitionsBuilder> {
                {RuntimePlatform.Android, new FadeUpwardsPageTransitionsBuilder()},
                {RuntimePlatform.IPhonePlayer, new CupertinoPageTransitionsBuilder()},
                {RuntimePlatform.LinuxEditor, new FadeUpwardsPageTransitionsBuilder()},
                {RuntimePlatform.LinuxPlayer, new FadeUpwardsPageTransitionsBuilder()},
                {RuntimePlatform.OSXEditor, new CupertinoPageTransitionsBuilder()},
                {RuntimePlatform.OSXPlayer, new CupertinoPageTransitionsBuilder()},
                {RuntimePlatform.WindowsEditor, new FadeUpwardsPageTransitionsBuilder()},
                {RuntimePlatform.WindowsPlayer, new FadeUpwardsPageTransitionsBuilder()}
            };

        static PageTransitionsBuilder _defaultBuilder = new FadeUpwardsPageTransitionsBuilder();

        public PageTransitionsBuilder builder {
            get { return _builder ?? _defaultBuilder; }
        }

        readonly PageTransitionsBuilder _builder;

        public Widget buildTranstions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child) {
            PageTransitionsBuilder matchingBuilder = builder;
            return matchingBuilder.buildTransitions(route, context, animation, secondaryAnimation, child);
        }

        PageTransitionsBuilder _all(PageTransitionsBuilder builder) {
            return builder;
        }

        public bool Equals(PageTransitionsTheme other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return _all(builder) == _all(other.builder);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((PageTransitionsTheme) obj);
        }

        public static bool operator ==(PageTransitionsTheme left, PageTransitionsTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(PageTransitionsTheme left, PageTransitionsTheme right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _all(builder).GetHashCode();
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<PageTransitionsBuilder>("builder", builder,
                defaultValue: _defaultBuilder));
        }
    }
}