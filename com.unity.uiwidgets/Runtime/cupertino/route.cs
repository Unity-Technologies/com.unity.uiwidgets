using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.async;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoRouteUtils {
        public const float _kBackGestureWidth = 20.0f;
        public const float _kMinFlingVelocity = 1.0f;

        public const int _kMaxDroppedSwipePageForwardAnimationTime = 800; // Milliseconds.

        public const int _kMaxPageBackAnimationTime = 300; // Milliseconds.

        public static readonly Color _kModalBarrierColor = CupertinoDynamicColor.withBrightness(
            color: new Color(0x33000000),
            darkColor: new Color(0x7A000000)
        );

        public static readonly TimeSpan _kModalPopupTransitionDuration = TimeSpan.FromMilliseconds(335);

        public static readonly Animatable<Offset> _kRightMiddleTween = new OffsetTween(
            begin: new Offset(1.0f, 0.0f),
            end: Offset.zero
        );

        public static readonly Animatable<Offset> _kMiddleLeftTween = new OffsetTween(
            begin: Offset.zero,
            end: new Offset(-1.0f / 3.0f, 0.0f)
        );

        public static readonly Animatable<Offset> _kBottomUpTween = new OffsetTween(
            begin: new Offset(0.0f, 1.0f),
            end: Offset.zero
        );

        public static readonly DecorationTween _kGradientShadowTween = new DecorationTween(
            begin: _CupertinoEdgeShadowDecoration.none,
            end: new _CupertinoEdgeShadowDecoration(
                edgeGradient: new LinearGradient(
                    begin: new Alignment(0.9f, 0.0f),
                    end: Alignment.centerRight,
                    colors: new List<Color> {
                        new Color(0x00000000),
                        new Color(0x04000000),
                        new Color(0x12000000),
                        new Color(0x38000000),
                    },
                    stops: new List<float> {0.0f, 0.3f, 0.6f, 1.0f}
                )
            )
        );


        public static Future showCupertinoModalPopup(
            BuildContext context = null,
            WidgetBuilder builder = null,
            ImageFilter filter = null,
            bool useRootNavigator = true,
            bool? semanticsDismissible =null
        ) {
            return Navigator.of(context, rootNavigator: useRootNavigator).push(
                new _CupertinoModalPopupRoute(
                    barrierColor: CupertinoDynamicColor.resolve(_kModalBarrierColor, context),
                    barrierLabel: "Dismiss",
                    builder: builder,
                    filter: filter
                    )
            );
        }
        

        public static readonly Animatable<float> _dialogScaleTween = new FloatTween(begin: 1.3f, end: 1.0f)
            .chain(new CurveTween(curve: Curves.linearToEaseOut));

        public static Widget _buildCupertinoDialogTransitions(
            BuildContext context, 
            Animation<float> animation,
            Animation<float> secondaryAnimation, 
            Widget child) {
            
            CurvedAnimation fadeAnimation = new CurvedAnimation(
                parent: animation,
                curve: Curves.easeInOut
            );
            if (animation.status == AnimationStatus.reverse) {
                return new FadeTransition(
                    opacity: fadeAnimation,
                    child: child
                );
            }

            return new FadeTransition(
                opacity: fadeAnimation,
                child: new ScaleTransition(
                    child: child,
                    scale: animation.drive(_dialogScaleTween)
                )
            );
        }

        public static Future showCupertinoDialog(
            BuildContext context = null,
            WidgetBuilder builder =null,
            bool useRootNavigator = true,
            RouteSettings routeSettings = null
        ) {
            D.assert(builder != null);
           
            return DialogUtils.showGeneralDialog<object>(
                context: context,
                barrierDismissible: false,
                barrierColor: CupertinoDynamicColor.resolve(_kModalBarrierColor, context),
                transitionDuration: TimeSpan.FromMilliseconds(250),
                pageBuilder: (BuildContext context1, Animation<float> animation, Animation<float> secondaryAnimation)=> {
                    return builder(context1);
                },
                transitionBuilder: _buildCupertinoDialogTransitions,
                useRootNavigator: useRootNavigator,
                routeSettings: routeSettings
            );
        }
        
        public static Future<T> showCupertinoDialog<T>(
            BuildContext context = null,
            WidgetBuilder builder =null,
            bool? useRootNavigator = true,
            RouteSettings routeSettings = null
        ) {
            D.assert(builder != null);
            D.assert(useRootNavigator != null);
            return  DialogUtils.showGeneralDialog<T>(
                context: context,
                barrierDismissible: false,
                barrierColor: CupertinoDynamicColor.resolve(_kModalBarrierColor, context),
                // This transition duration was eyeballed comparing with iOS
                transitionDuration: TimeSpan.FromMilliseconds(250),
            pageBuilder: (BuildContext context1, Animation<float> animation, Animation<float> secondaryAnimation)=> {
                return builder(context1);
            },
            transitionBuilder: _buildCupertinoDialogTransitions,
            useRootNavigator: useRootNavigator.Value,
            routeSettings: routeSettings
                );
        }
        public static Future<T> showCupertinoModalPopup<T>(
            BuildContext context = null,
            WidgetBuilder builder =null,
                ImageFilter filter = null,
            bool? useRootNavigator = true
        ) {
           D.assert(useRootNavigator != null);
            return Navigator.of(context, rootNavigator: useRootNavigator.Value).push(
               new _CupertinoModalPopupRoute(
                    barrierColor: CupertinoDynamicColor.resolve(_kModalBarrierColor, context),
                    barrierLabel: "Dismiss",
                    builder: builder,
                    filter: filter
                    )
            ).to<T>();
        }

    }

    class _CupertinoEdgeShadowDecoration : Decoration, IEquatable<_CupertinoEdgeShadowDecoration> {
        public _CupertinoEdgeShadowDecoration(
            LinearGradient edgeGradient = null
        ) {
            this.edgeGradient = edgeGradient;
        }

        public static _CupertinoEdgeShadowDecoration none =
            new _CupertinoEdgeShadowDecoration();

        public readonly LinearGradient edgeGradient;

        static _CupertinoEdgeShadowDecoration lerpCupertino(
            _CupertinoEdgeShadowDecoration a,
            _CupertinoEdgeShadowDecoration b,
            float t
        ) {
            if (a == null && b == null) {
                return null;
            }

            return new _CupertinoEdgeShadowDecoration(
                edgeGradient: LinearGradient.lerp(a?.edgeGradient, b?.edgeGradient, t)
            );
        }

        public override Decoration lerpFrom(Decoration a, float t) {
            if (!(a is _CupertinoEdgeShadowDecoration)) {
                return lerpCupertino(null, this, t);
            }

            return lerpCupertino((_CupertinoEdgeShadowDecoration) a, this, t);
        }

        public override Decoration lerpTo(Decoration b, float t) {
            if (!(b is _CupertinoEdgeShadowDecoration)) {
                return lerpCupertino(this, null, t);
            }

            return lerpCupertino(this, (_CupertinoEdgeShadowDecoration) b, t);
        }

        public override BoxPainter createBoxPainter(VoidCallback onChanged = null) {
            return new _CupertinoEdgeShadowPainter(this, onChanged);
        }

        public override int GetHashCode() {
            return edgeGradient.GetHashCode();
        }

        public bool Equals(_CupertinoEdgeShadowDecoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return  other is _CupertinoEdgeShadowDecoration && Equals(edgeGradient, other.edgeGradient);
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

            return Equals((_CupertinoEdgeShadowDecoration) obj);
        }

        public static bool operator ==(_CupertinoEdgeShadowDecoration left, _CupertinoEdgeShadowDecoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(_CupertinoEdgeShadowDecoration left, _CupertinoEdgeShadowDecoration right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LinearGradient>("edgeGradient", edgeGradient));
        }
    }

    class _CupertinoEdgeShadowPainter : BoxPainter {
        public _CupertinoEdgeShadowPainter(
            _CupertinoEdgeShadowDecoration decoration = null,
            VoidCallback onChange = null
        ) : base(onChange) {
            D.assert(decoration != null);
            _decoration = decoration;
        }

        readonly _CupertinoEdgeShadowDecoration _decoration;

        public override void paint(Canvas canvas, Offset offset, ImageConfiguration configuration) {
            LinearGradient gradient = _decoration.edgeGradient;
            if (gradient == null) {
                return;
            }
            TextDirection textDirection = configuration.textDirection;
            float deltaX = 0.0f;
            switch (textDirection) {
                case TextDirection.rtl:
                    deltaX = configuration.size.width;
                    break;
                case TextDirection.ltr:
                    deltaX = -configuration.size.width;
                    break;
            }
            Rect rect = (offset & configuration.size).translate(deltaX, 0.0f);
            Paint paint = new Paint() {
                shader = gradient.createShader(rect, textDirection: textDirection)
            };
            canvas.drawRect(rect, paint);
        }
    }

    public class CupertinoPageRoute : PageRoute {
        public CupertinoPageRoute(
            WidgetBuilder builder,
            RouteSettings settings = null,
            string title = "",
            bool maintainState = true,
            bool fullscreenDialog = false
        ) : base(settings: settings, fullscreenDialog: fullscreenDialog) {
            D.assert(builder != null);
            D.assert(opaque);
            this.builder = builder;
            this.title = title;
            this.maintainState = maintainState;
        }

        public readonly WidgetBuilder builder;
        public readonly string title;
        
        ValueNotifier<string> _previousTitle;

        public ValueListenable<string> previousTitle {
            get {
                D.assert(
                    _previousTitle != null,
                    () => "Cannot read the previousTitle for a route that has not yet been installed"
                );
                return _previousTitle;
            }
        }

        protected internal override void didChangePrevious(Route previousRoute) {
            string previousTitleString = previousRoute is CupertinoPageRoute
                ? ((CupertinoPageRoute) previousRoute).title
                : null;
            if (_previousTitle == null) {
                _previousTitle = new ValueNotifier<string>(previousTitleString);
            }

            else {
                _previousTitle.value = previousTitleString;
            }

            base.didChangePrevious(previousRoute);
        }

        public override bool maintainState { get; }

        public override TimeSpan transitionDuration {
            get { return TimeSpan.FromMilliseconds(400); }
        }

        public override Color barrierColor {
            get { return null; }
        }


        public override string barrierLabel {
            get { return null; }
        }

        public override bool canTransitionTo(TransitionRoute nextRoute) {
            return nextRoute is CupertinoPageRoute && !((CupertinoPageRoute) nextRoute).fullscreenDialog;
        }

        static bool isPopGestureInProgress(PageRoute route) {
            return route.navigator.userGestureInProgress;
        }


        public bool popGestureInProgress {
            get { return isPopGestureInProgress(this as PageRoute); }
        }

        public bool popGestureEnabled {
            get { return _isPopGestureEnabled(this as PageRoute); }
        }

        static bool _isPopGestureEnabled(PageRoute route) {
            if (route.isFirst) {
                return false;
            }

            if (route.willHandlePopInternally) {
                return false;
            }

            if (route.hasScopedWillPopCallback) {
                return false;
            }

            if (route.fullscreenDialog) {
                return false;
            }

            if (route.animation.status != AnimationStatus.completed) {
                return false;
            }

            if (route.secondaryAnimation.status != AnimationStatus.dismissed) {
                return false;
            }

            if (isPopGestureInProgress(route )) {
                return false;
            }

            return true;
        }


        public override Widget buildPage(BuildContext context, Animation<float> animation, Animation<float> secondaryAnimation) {
            Widget result = builder(context);
            D.assert(() =>{
                if (result == null) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary($"The builder for route \"{settings.name}\" returned null."),
                        new ErrorDescription("Route builders must never return null."),
                    });
                }
                return true;
            });
            return result;
        }


        static _CupertinoBackGestureController _startPopGesture(PageRoute route) {
            D.assert(_isPopGestureEnabled(route));
            return new _CupertinoBackGestureController(
                navigator: route.navigator,
                controller: route.controller
            );
        }

        public static Widget buildPageTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            bool linearTransition = isPopGestureInProgress(route);
            if (route.fullscreenDialog) {
                return new CupertinoFullscreenDialogTransition(
                    primaryRouteAnimation: animation,
                    secondaryRouteAnimation: secondaryAnimation,
                    child: child,
                    linearTransition: linearTransition
                );
            }

            else {
                return new CupertinoPageTransition(
                    primaryRouteAnimation: animation,
                    secondaryRouteAnimation: secondaryAnimation,
                    linearTransition: linearTransition,
                    child: new _CupertinoBackGestureDetector(
                        enabledCallback: () => _isPopGestureEnabled(route),
                        onStartPopGesture: () => _startPopGesture(route),
                        child: child
                    )
                );
            }
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return buildPageTransitions(this, context, animation, secondaryAnimation, child);
        }

        public new string debugLabel {
            get { return $"{base.debugLabel}({settings.name})"; }
        }
    }

    class CupertinoPageTransition : StatelessWidget {
        public CupertinoPageTransition(
            bool linearTransition,
            Animation<float> primaryRouteAnimation = null,
            Animation<float> secondaryRouteAnimation = null,
            Widget child = null,
            Key key = null
        ) : base(key: key) {
            _primaryPositionAnimation =
                (linearTransition
                    ? primaryRouteAnimation
                    : new CurvedAnimation(
                        parent: primaryRouteAnimation,
                        curve: Curves.linearToEaseOut,
                        reverseCurve: Curves.easeInToLinear
                    )
                ).drive(CupertinoRouteUtils._kRightMiddleTween);

            _secondaryPositionAnimation =
                (linearTransition
                    ? secondaryRouteAnimation
                    : new CurvedAnimation(
                        parent: secondaryRouteAnimation,
                        curve: Curves.linearToEaseOut,
                        reverseCurve: Curves.easeInToLinear
                    )
                ).drive(CupertinoRouteUtils._kMiddleLeftTween);
            _primaryShadowAnimation =
                (linearTransition
                    ? primaryRouteAnimation
                    : new CurvedAnimation(
                        parent: primaryRouteAnimation,
                        curve: Curves.linearToEaseOut
                    )
                ).drive(CupertinoRouteUtils._kGradientShadowTween);
            this.child = child;
        }

        public readonly Animation<Offset> _primaryPositionAnimation;
        public readonly Animation<Offset> _secondaryPositionAnimation;
        public readonly Animation<Decoration> _primaryShadowAnimation;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            TextDirection textDirection = Directionality.of(context);
            return new SlideTransition(
                position: _secondaryPositionAnimation,
                textDirection: textDirection,
                transformHitTests: false,
                child: new SlideTransition(
                    position: _primaryPositionAnimation,
                    textDirection: textDirection,
                    child: new DecoratedBoxTransition(
                        decoration: _primaryShadowAnimation,
                        child: child
                    )
                )
            );
        }
    }

    class CupertinoFullscreenDialogTransition : StatelessWidget {
        public CupertinoFullscreenDialogTransition(
            Key key = null,
            Animation<float> primaryRouteAnimation = null,
            Animation<float> secondaryRouteAnimation = null,
            Widget child = null,
            bool linearTransition = false
            
        ) : base(key: key) {
            _positionAnimation = new CurvedAnimation(
                parent: primaryRouteAnimation,
                curve: Curves.linearToEaseOut,
                reverseCurve: Curves.linearToEaseOut.flipped
            ).drive(CupertinoRouteUtils._kBottomUpTween);
            this.child = child;
            _secondaryPositionAnimation =
                (linearTransition
                    ? secondaryRouteAnimation
                    : new CurvedAnimation(
                        parent: secondaryRouteAnimation,
                        curve: Curves.linearToEaseOut,
                        reverseCurve: Curves.easeInToLinear
                    )
                ).drive(CupertinoRouteUtils._kMiddleLeftTween);
        }

        public readonly Animation<Offset> _positionAnimation;
        public readonly Animation<Offset> _secondaryPositionAnimation;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            TextDirection textDirection = Directionality.of(context);
            return new SlideTransition(
                position: _secondaryPositionAnimation,
                textDirection: textDirection,
                transformHitTests: false,
                child: new SlideTransition(
                    position: _positionAnimation,
                    child: child
                )
            );
        }
    }

    class _CupertinoBackGestureDetector : StatefulWidget {
        public _CupertinoBackGestureDetector(
            Key key = null,
            ValueGetter<bool> enabledCallback = null,
            ValueGetter<_CupertinoBackGestureController> onStartPopGesture = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(enabledCallback != null);
            D.assert(onStartPopGesture != null);
            D.assert(child != null);
            this.child = child;
            this.enabledCallback = enabledCallback;
            this.onStartPopGesture = onStartPopGesture;
        }

        public readonly Widget child;

        public readonly ValueGetter<bool> enabledCallback;

        public readonly ValueGetter<_CupertinoBackGestureController> onStartPopGesture;

        public override State createState() {
            return new _CupertinoBackGestureDetectorState();
        }
    }

    class _CupertinoBackGestureDetectorState : State<_CupertinoBackGestureDetector> {
        _CupertinoBackGestureController _backGestureController;
        HorizontalDragGestureRecognizer _recognizer;

        public override void initState() {
            base.initState();
            _recognizer = new HorizontalDragGestureRecognizer(debugOwner: this);
            _recognizer.onStart = _handleDragStart;
            _recognizer.onUpdate = _handleDragUpdate;
            _recognizer.onEnd = _handleDragEnd;
            _recognizer.onCancel = _handleDragCancel;
        }

        public override void dispose() {
            _recognizer.dispose();
            base.dispose();
        }

        void _handleDragStart(DragStartDetails details) {
            D.assert(mounted);
            D.assert(_backGestureController == null);
            _backGestureController = widget.onStartPopGesture();
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            D.assert(mounted);
            D.assert(_backGestureController != null);
            _backGestureController.dragUpdate(
                _convertToLogical(details.primaryDelta / context.size.width));
        }

        void _handleDragEnd(DragEndDetails details) {
            D.assert(mounted);
            D.assert(_backGestureController != null);
            _backGestureController.dragEnd(_convertToLogical(details.velocity.pixelsPerSecond.dx / context.size.width) ?? 0.0f);
            _backGestureController = null;
        }

        void _handleDragCancel() {
            D.assert(mounted);
            _backGestureController?.dragEnd(0.0f);
            _backGestureController = null;
        }

        void _handlePointerDown(PointerDownEvent evt) {
            if (widget.enabledCallback()) {
                _recognizer.addPointer(evt);
            }
        }

        float? _convertToLogical(float? value) {
            switch (Directionality.of(context)) {
                case TextDirection.rtl:
                    return -value;
                case TextDirection.ltr:
                    return value;
            }

            return value;
        }


        public override Widget build(BuildContext context) {
            float dragAreaWidth = Directionality.of(context) == TextDirection.ltr
                ? MediaQuery.of(context).padding.left
                : MediaQuery.of(context).padding.right;
            dragAreaWidth = Mathf.Max(dragAreaWidth, CupertinoRouteUtils._kBackGestureWidth);
            return new Stack(
                fit: StackFit.passthrough,
                children: new List<Widget> {
                    widget.child,
                    new PositionedDirectional(
                        start: 0.0f,
                        width: dragAreaWidth,
                        top: 0.0f,
                        bottom: 0.0f,
                        child: new Listener(
                            onPointerDown: _handlePointerDown,
                            behavior: HitTestBehavior.translucent
                        )
                    ),
                }
            );
        }
    }

    class _CupertinoBackGestureController {
        public _CupertinoBackGestureController(
            NavigatorState navigator,
            AnimationController controller
        ) {
            D.assert(navigator != null);
            D.assert(controller != null);

            this.navigator = navigator;
            this.controller = controller;
            this.navigator.didStartUserGesture();
        }

        public readonly AnimationController controller;
        public readonly NavigatorState navigator;

        public void dragUpdate(float? delta) {
            if (delta != null) {
                controller.setValue(controller.value - (float) delta);
            }
        }

        public void dragEnd(float velocity) {
            Curve animationCurve = Curves.fastLinearToSlowEaseIn;
            bool animateForward;

            if (velocity.abs() >= CupertinoRouteUtils._kMinFlingVelocity) {
                animateForward = velocity <= 0;
            }
            else {
                animateForward = controller.value > 0.5f;
            }
            if (animateForward) {
                int droppedPageForwardAnimationTime = Mathf.Min(
                    MathUtils.lerpNullableFloat(CupertinoRouteUtils._kMaxDroppedSwipePageForwardAnimationTime, 0f,
                        controller.value).floor(),
                    CupertinoRouteUtils._kMaxPageBackAnimationTime
                );
                controller.animateTo(1.0f, duration: TimeSpan.FromMilliseconds(droppedPageForwardAnimationTime),
                    curve: animationCurve);
            }
            else {
                navigator.pop<object>();

                if (controller.isAnimating) {
                    int droppedPageBackAnimationTime =
                        MathUtils.lerpNullableFloat(0f, CupertinoRouteUtils._kMaxDroppedSwipePageForwardAnimationTime,
                            controller.value).floor();
                    controller.animateBack(0.0f, duration: TimeSpan.FromMilliseconds(droppedPageBackAnimationTime),
                        curve: animationCurve);
                }
            }

            if (controller.isAnimating) {
                AnimationStatusListener animationStatusCallback = null;
                animationStatusCallback = (AnimationStatus status) => {
                    navigator.didStopUserGesture();
                    controller.removeStatusListener(animationStatusCallback);
                };
                controller.addStatusListener(animationStatusCallback);
            }
            else {
                navigator.didStopUserGesture();
            }
        }
    }

    class _CupertinoModalPopupRoute : PopupRoute{
        public _CupertinoModalPopupRoute(
            Color barrierColor = null,
            string barrierLabel = null,
            WidgetBuilder builder = null,
            ImageFilter filter = null,
            RouteSettings settings = null
        ) : base(filter:filter,settings: settings) {
            this.barrierColor = barrierColor;
            this.builder = builder;
            this.barrierLabel = barrierLabel;
        }

        public readonly WidgetBuilder builder;

        public override Color barrierColor { get; }
        public override string barrierLabel { get; }

        public override bool barrierDismissible {
            get { return true; }
        }

        public override TimeSpan transitionDuration {
            get { return CupertinoRouteUtils._kModalPopupTransitionDuration; }
        }

        new Animation<float> _animation;

        Tween<Offset> _offsetTween;

        public override Animation<float> createAnimation() {
            D.assert(_animation == null);
            _animation = new CurvedAnimation(
                parent: base.createAnimation(),
                curve: Curves.linearToEaseOut,
                reverseCurve: Curves.linearToEaseOut.flipped
            );
            _offsetTween = new OffsetTween(
                begin: new Offset(0.0f, 1.0f),
                end: new Offset(0.0f, 0.0f)
            );
            return _animation;
        }


        public override Widget buildPage(BuildContext context, Animation<float> animation, Animation<float> secondaryAnimation) {
            return new CupertinoUserInterfaceLevel(
                data: CupertinoUserInterfaceLevelData.elevatedlayer,
                child: new Builder(builder: builder)
            );
        }


        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return new Align(
                alignment: Alignment.bottomCenter,
                child: new FractionalTranslation(
                    translation: _offsetTween.evaluate(_animation),
                    child: child
                )
            );
        }
    }
    

}
