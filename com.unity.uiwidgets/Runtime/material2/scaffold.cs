using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    static class ScaffoldUtils {
        public static readonly FloatingActionButtonLocation _kDefaultFloatingActionButtonLocation =
            FloatingActionButtonLocation.endFloat;

        public static readonly FloatingActionButtonAnimator _kDefaultFloatingActionButtonAnimator =
            FloatingActionButtonAnimator.scaling;
    }

    enum _ScaffoldSlot {
        body,
        appBar,
        bottomSheet,
        snackBar,
        persistentFooter,
        bottomNavigationBar,
        floatingActionButton,
        drawer,
        endDrawer,
        statusBar
    }

    public class ScaffoldPrelayoutGeometry {
        public ScaffoldPrelayoutGeometry(
            Size bottomSheetSize = null,
            float? contentBottom = null,
            float? contentTop = null,
            Size floatingActionButtonSize = null,
            EdgeInsets minInsets = null,
            Size scaffoldSize = null,
            Size snackBarSize = null
        ) {
            D.assert(bottomSheetSize != null);
            D.assert(contentBottom != null);
            D.assert(contentTop != null);
            D.assert(floatingActionButtonSize != null);
            D.assert(minInsets != null);
            D.assert(scaffoldSize != null);
            D.assert(snackBarSize != null);

            this.bottomSheetSize = bottomSheetSize;
            this.contentBottom = contentBottom.Value;
            this.contentTop = contentTop.Value;
            this.floatingActionButtonSize = floatingActionButtonSize;
            this.minInsets = minInsets;
            this.scaffoldSize = scaffoldSize;
            this.snackBarSize = snackBarSize;
        }

        public readonly Size floatingActionButtonSize;

        public readonly Size bottomSheetSize;

        public readonly float contentBottom;

        public readonly float contentTop;

        public readonly EdgeInsets minInsets;

        public readonly Size scaffoldSize;

        public readonly Size snackBarSize;
    }

    class _TransitionSnapshotFabLocation : FloatingActionButtonLocation {
        public _TransitionSnapshotFabLocation(
            FloatingActionButtonLocation begin,
            FloatingActionButtonLocation end,
            FloatingActionButtonAnimator animator,
            float progress) {
            this.begin = begin;
            this.end = end;
            this.animator = animator;
            this.progress = progress;
        }

        public readonly FloatingActionButtonLocation begin;

        public readonly FloatingActionButtonLocation end;

        public readonly FloatingActionButtonAnimator animator;

        public readonly float progress;

        public override Offset getOffset(ScaffoldPrelayoutGeometry scaffoldGeometry) {
            return animator.getOffset(
                begin: begin.getOffset(scaffoldGeometry),
                end: end.getOffset(scaffoldGeometry),
                progress: progress
            );
        }

        public override string ToString() {
            return GetType() + "(begin: " + begin + ", end: " + end + ", progress: " + progress;
        }
    }

    public class ScaffoldGeometry {
        public ScaffoldGeometry(
            float? bottomNavigationBarTop = null,
            Rect floatingActionButtonArea = null) {
            this.bottomNavigationBarTop = bottomNavigationBarTop;
            this.floatingActionButtonArea = floatingActionButtonArea;
        }

        public readonly float? bottomNavigationBarTop;

        public readonly Rect floatingActionButtonArea;

        public ScaffoldGeometry _scaleFloatingActionButton(float scaleFactor) {
            if (scaleFactor == 1.0f) {
                return this;
            }

            if (scaleFactor == 0.0f) {
                return new ScaffoldGeometry(
                    bottomNavigationBarTop: bottomNavigationBarTop);
            }

            Rect scaledButton = Rect.lerp(
                floatingActionButtonArea.center & Size.zero,
                floatingActionButtonArea,
                scaleFactor);

            return copyWith(floatingActionButtonArea: scaledButton);
        }

        public ScaffoldGeometry copyWith(
            float? bottomNavigationBarTop = null,
            Rect floatingActionButtonArea = null
        ) {
            return new ScaffoldGeometry(
                bottomNavigationBarTop: bottomNavigationBarTop ?? this.bottomNavigationBarTop,
                floatingActionButtonArea: floatingActionButtonArea ?? this.floatingActionButtonArea);
        }
    }


    class _ScaffoldGeometryNotifier : ValueNotifier<ScaffoldGeometry> {
        public _ScaffoldGeometryNotifier(
            ScaffoldGeometry geometry, BuildContext context) : base(geometry) {
            D.assert(context != null);
            this.context = context;
            this.geometry = geometry;
        }

        public readonly BuildContext context;

        float floatingActionButtonScale;

        ScaffoldGeometry geometry;

        public override ScaffoldGeometry value {
            get {
                D.assert(() => {
                    RenderObject renderObject = context.findRenderObject();

                    if (renderObject == null || !renderObject.owner.debugDoingPaint) {
                        throw new UIWidgetsError(
                            "Scaffold.geometryOf() must only be accessed during the paint phase.\n" +
                            "The ScaffoldGeometry is only available during the paint phase, because\n" +
                            "its value is computed during the animation and layout phases prior to painting."
                        );
                    }

                    return true;
                });
                return geometry._scaleFloatingActionButton(floatingActionButtonScale);
            }
        }

        public void _updateWith(
            float? bottomNavigationBarTop = null,
            Rect floatingActionButtonArea = null,
            float? floatingActionButtonScale = null
        ) {
            this.floatingActionButtonScale = floatingActionButtonScale ?? this.floatingActionButtonScale;
            geometry = geometry.copyWith(
                bottomNavigationBarTop: bottomNavigationBarTop,
                floatingActionButtonArea: floatingActionButtonArea);
            notifyListeners();
        }
    }

    class _BodyBoxConstraints : BoxConstraints {
        public _BodyBoxConstraints(
            float minWidth = 0.0f,
            float maxWidth = float.PositiveInfinity,
            float minHeight = 0.0f,
            float maxHeight = float.PositiveInfinity,
            float? bottomWidgetsHeight = null
        ) : base(minWidth: minWidth, maxWidth: maxWidth, minHeight: minHeight, maxHeight: maxHeight) {
            D.assert(bottomWidgetsHeight != null);
            D.assert(bottomWidgetsHeight >= 0);
            this.bottomWidgetsHeight = bottomWidgetsHeight.Value;
        }

        public readonly float bottomWidgetsHeight;
        
        public bool Equals(_BodyBoxConstraints other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return bottomWidgetsHeight.Equals(other.bottomWidgetsHeight)
                   && base.Equals(other);
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

            return Equals((_BodyBoxConstraints) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ bottomWidgetsHeight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(_BodyBoxConstraints left, _BodyBoxConstraints right) {
            return Equals(left, right);
        }

        public static bool operator !=(_BodyBoxConstraints left, _BodyBoxConstraints right) {
            return !Equals(left, right);
        }
    }

    class _BodyBuilder : StatelessWidget {
        public _BodyBuilder(Key key = null, Widget body = null) : base(key: key) {
            this.body = body;
        }

        public readonly Widget body;

        public override Widget build(BuildContext context) {
            return new LayoutBuilder(
                builder: (ctx, constraints) => {
                    _BodyBoxConstraints bodyConstraints = (_BodyBoxConstraints) constraints;
                    MediaQueryData metrics = MediaQuery.of(context);
                    return new MediaQuery(
                        data: metrics.copyWith(
                            padding: metrics.padding.copyWith(
                                bottom: Mathf.Max(metrics.padding.bottom, bodyConstraints.bottomWidgetsHeight)
                            )
                        ),
                        child: body
                    );
                }
            );
        }
    }

    class _ScaffoldLayout : MultiChildLayoutDelegate {
        public _ScaffoldLayout(
            EdgeInsets minInsets,
            _ScaffoldGeometryNotifier geometryNotifier,
            FloatingActionButtonLocation previousFloatingActionButtonLocation,
            FloatingActionButtonLocation currentFloatingActionButtonLocation,
            float floatingActionButtonMoveAnimationProgress,
            FloatingActionButtonAnimator floatingActionButtonMotionAnimator,
            bool extendBody
        ) {
            D.assert(minInsets != null);
            D.assert(geometryNotifier != null);
            D.assert(previousFloatingActionButtonLocation != null);
            D.assert(currentFloatingActionButtonLocation != null);

            this.minInsets = minInsets;
            this.geometryNotifier = geometryNotifier;
            this.previousFloatingActionButtonLocation = previousFloatingActionButtonLocation;
            this.currentFloatingActionButtonLocation = currentFloatingActionButtonLocation;
            this.floatingActionButtonMoveAnimationProgress = floatingActionButtonMoveAnimationProgress;
            this.floatingActionButtonMotionAnimator = floatingActionButtonMotionAnimator;
            this.extendBody = extendBody;
        }

        public readonly bool extendBody;
        
        public readonly EdgeInsets minInsets;

        public readonly _ScaffoldGeometryNotifier geometryNotifier;

        public readonly FloatingActionButtonLocation previousFloatingActionButtonLocation;

        public readonly FloatingActionButtonLocation currentFloatingActionButtonLocation;

        public readonly float floatingActionButtonMoveAnimationProgress;

        public readonly FloatingActionButtonAnimator floatingActionButtonMotionAnimator;


        public override void performLayout(Size size) {
            BoxConstraints looseConstraints = BoxConstraints.loose(size);

            BoxConstraints fullWidthConstraints = looseConstraints.tighten(width: size.width);
            float bottom = size.height;
            float contentTop = 0.0f;
            float bottomWidgetsHeight = 0.0f;

            if (hasChild(_ScaffoldSlot.appBar)) {
                contentTop = layoutChild(_ScaffoldSlot.appBar, fullWidthConstraints).height;
                positionChild(_ScaffoldSlot.appBar, Offset.zero);
            }

            float bottomNavigationBarTop = 0.0f;
            if (hasChild(_ScaffoldSlot.bottomNavigationBar)) {
                float bottomNavigationBarHeight =
                    layoutChild(_ScaffoldSlot.bottomNavigationBar, fullWidthConstraints).height;
                bottomWidgetsHeight += bottomNavigationBarHeight;
                bottomNavigationBarTop = Mathf.Max(0.0f, bottom - bottomWidgetsHeight);
                positionChild(_ScaffoldSlot.bottomNavigationBar, new Offset(0.0f, bottomNavigationBarTop));
            }

            if (hasChild(_ScaffoldSlot.persistentFooter)) {
                BoxConstraints footerConstraints = new BoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: Mathf.Max(0.0f, bottom - bottomWidgetsHeight - contentTop)
                );
                float persistentFooterHeight =
                    layoutChild(_ScaffoldSlot.persistentFooter, footerConstraints).height;
                bottomWidgetsHeight += persistentFooterHeight;
                positionChild(_ScaffoldSlot.persistentFooter,
                    new Offset(0.0f, Mathf.Max(0.0f, bottom - bottomWidgetsHeight)));
            }

            float contentBottom = Mathf.Max(0.0f, bottom - Mathf.Max(minInsets.bottom, bottomWidgetsHeight));

            if (hasChild(_ScaffoldSlot.body)) {
                float bodyMaxHeight = Mathf.Max(0.0f, contentBottom - contentTop);
                if (extendBody) {
                    bodyMaxHeight += bottomWidgetsHeight;
                    D.assert(bodyMaxHeight <= Mathf.Max(0.0f, looseConstraints.maxHeight - contentTop));
                }
                BoxConstraints bodyConstraints = new _BodyBoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: bodyMaxHeight,
                    bottomWidgetsHeight: extendBody ? bottomWidgetsHeight : 0.0f
                );
                layoutChild(_ScaffoldSlot.body, bodyConstraints);
                positionChild(_ScaffoldSlot.body, new Offset(0.0f, contentTop));
            }

            Size bottomSheetSize = Size.zero;
            Size snackBarSize = Size.zero;

            if (hasChild(_ScaffoldSlot.bottomSheet)) {
                BoxConstraints bottomSheetConstraints = new BoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: Mathf.Max(0.0f, contentBottom - contentTop)
                );
                bottomSheetSize = layoutChild(_ScaffoldSlot.bottomSheet, bottomSheetConstraints);
                positionChild(_ScaffoldSlot.bottomSheet,
                    new Offset((size.width - bottomSheetSize.width) / 2.0f, contentBottom - bottomSheetSize.height));
            }

            if (hasChild(_ScaffoldSlot.snackBar)) {
                snackBarSize = layoutChild(_ScaffoldSlot.snackBar, fullWidthConstraints);
                positionChild(_ScaffoldSlot.snackBar, new Offset(0.0f, contentBottom - snackBarSize.height));
            }

            Rect floatingActionButtonRect = null;
            if (hasChild(_ScaffoldSlot.floatingActionButton)) {
                Size fabSize = layoutChild(_ScaffoldSlot.floatingActionButton, looseConstraints);
                ScaffoldPrelayoutGeometry currentGeometry = new ScaffoldPrelayoutGeometry(
                    bottomSheetSize: bottomSheetSize,
                    contentBottom: contentBottom,
                    contentTop: contentTop,
                    floatingActionButtonSize: fabSize,
                    minInsets: minInsets,
                    scaffoldSize: size,
                    snackBarSize: snackBarSize
                );
                Offset currentFabOffset = currentFloatingActionButtonLocation.getOffset(currentGeometry);
                Offset previousFabOffset = previousFloatingActionButtonLocation.getOffset(currentGeometry);
                Offset fabOffset = floatingActionButtonMotionAnimator.getOffset(
                    begin: previousFabOffset,
                    end: currentFabOffset,
                    progress: floatingActionButtonMoveAnimationProgress
                );
                positionChild(_ScaffoldSlot.floatingActionButton, fabOffset);
                floatingActionButtonRect = fabOffset & fabSize;
            }

            if (hasChild(_ScaffoldSlot.statusBar)) {
                layoutChild(_ScaffoldSlot.statusBar, fullWidthConstraints.tighten(height: minInsets.top));
                positionChild(_ScaffoldSlot.statusBar, Offset.zero);
            }

            if (hasChild(_ScaffoldSlot.drawer)) {
                layoutChild(_ScaffoldSlot.drawer, BoxConstraints.tight(size));
                positionChild(_ScaffoldSlot.drawer, Offset.zero);
            }

            if (hasChild(_ScaffoldSlot.endDrawer)) {
                layoutChild(_ScaffoldSlot.endDrawer, BoxConstraints.tight(size));
                positionChild(_ScaffoldSlot.endDrawer, Offset.zero);
            }

            geometryNotifier._updateWith(
                bottomNavigationBarTop: bottomNavigationBarTop,
                floatingActionButtonArea: floatingActionButtonRect
            );
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
            _ScaffoldLayout _oldDelegate = (_ScaffoldLayout) oldDelegate;
            return _oldDelegate.minInsets != minInsets
                   || _oldDelegate.floatingActionButtonMoveAnimationProgress !=
                   floatingActionButtonMoveAnimationProgress
                   || _oldDelegate.previousFloatingActionButtonLocation != previousFloatingActionButtonLocation
                   || _oldDelegate.currentFloatingActionButtonLocation != currentFloatingActionButtonLocation;
        }
    }


    class _FloatingActionButtonTransition : StatefulWidget {
        public _FloatingActionButtonTransition(
            Key key = null,
            Widget child = null,
            Animation<float> fabMoveAnimation = null,
            FloatingActionButtonAnimator fabMotionAnimator = null,
            _ScaffoldGeometryNotifier geometryNotifier = null
        ) : base(key: key) {
            D.assert(fabMoveAnimation != null);
            D.assert(fabMotionAnimator != null);
            this.child = child;
            this.fabMoveAnimation = fabMoveAnimation;
            this.fabMotionAnimator = fabMotionAnimator;
            this.geometryNotifier = geometryNotifier;
        }

        public readonly Widget child;

        public readonly Animation<float> fabMoveAnimation;

        public readonly FloatingActionButtonAnimator fabMotionAnimator;

        public readonly _ScaffoldGeometryNotifier geometryNotifier;

        public override State createState() {
            return new _FloatingActionButtonTransitionState();
        }
    }

    class _FloatingActionButtonTransitionState : TickerProviderStateMixin<_FloatingActionButtonTransition> {
        AnimationController _previousController;

        Animation<float> _previousScaleAnimation;

        Animation<float> _previousRotationAnimation;

        AnimationController _currentController;

        Animation<float> _currentScaleAnimation;

        Animation<float> _extendedCurrentScaleAnimation;

        Animation<float> _currentRotationAnimation;

        Widget _previousChild;

        public override void initState() {
            base.initState();

            _previousController = new AnimationController(
                duration: FloatingActionButtonLocationUtils.kFloatingActionButtonSegue,
                vsync: this);
            _previousController.addStatusListener(_handlePreviousAnimationStatusChanged);

            _currentController = new AnimationController(
                duration: FloatingActionButtonLocationUtils.kFloatingActionButtonSegue,
                vsync: this);

            _updateAnimations();

            if (widget.child != null) {
                _currentController.setValue(1.0f);
            }
            else {
                _updateGeometryScale(0.0f);
            }
        }

        public override void dispose() {
            _previousController.dispose();
            _currentController.dispose();
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);

            _FloatingActionButtonTransition _oldWidget = (_FloatingActionButtonTransition) oldWidget;
            bool oldChildIsNull = _oldWidget.child == null;
            bool newChildIsNull = widget.child == null;

            if (oldChildIsNull == newChildIsNull && _oldWidget.child?.key == widget.child?.key) {
                return;
            }

            if (_oldWidget.fabMotionAnimator != widget.fabMotionAnimator ||
                _oldWidget.fabMoveAnimation != widget.fabMoveAnimation) {
                _updateAnimations();
            }

            if (_previousController.status == AnimationStatus.dismissed) {
                float currentValue = _currentController.value;
                if (currentValue == 0.0f || _oldWidget.child == null) {
                    _previousChild = null;
                    if (widget.child != null) {
                        _currentController.forward();
                    }
                }
                else {
                    _previousChild = _oldWidget.child;
                    _previousController.setValue(currentValue);
                    _previousController.reverse();
                    _currentController.setValue(0.0f);
                }
            }
        }

        static readonly Animatable<float> _entranceTurnTween = new FloatTween(
            begin: 1.0f - FloatingActionButtonLocationUtils.kFloatingActionButtonTurnInterval,
            end: 1.0f
        ).chain(new CurveTween(curve: Curves.easeIn));

        void _updateAnimations() {
            CurvedAnimation previousExitScaleAnimation = new CurvedAnimation(
                parent: _previousController,
                curve: Curves.easeIn
            );
            Animation<float> previousExitRotationAnimation = new FloatTween(begin: 1.0f, end: 1.0f).animate(
                new CurvedAnimation(
                    parent: _previousController,
                    curve: Curves.easeIn
                )
            );

            CurvedAnimation currentEntranceScaleAnimation = new CurvedAnimation(
                parent: _currentController,
                curve: Curves.easeIn
            );
            Animation<float> currentEntranceRotationAnimation = _currentController.drive(_entranceTurnTween);
            Animation<float> moveScaleAnimation =
                widget.fabMotionAnimator.getScaleAnimation(parent: widget.fabMoveAnimation);
            Animation<float> moveRotationAnimation =
                widget.fabMotionAnimator.getRotationAnimation(parent: widget.fabMoveAnimation);

            _previousScaleAnimation = new AnimationMin(moveScaleAnimation, previousExitScaleAnimation);
            _currentScaleAnimation = new AnimationMin(moveScaleAnimation, currentEntranceScaleAnimation);
            _extendedCurrentScaleAnimation =
                _currentScaleAnimation.drive(new CurveTween(curve: new Interval(0.0f, 0.1f)));

            _previousRotationAnimation =
                new TrainHoppingAnimation(previousExitRotationAnimation, moveRotationAnimation);
            _currentRotationAnimation =
                new TrainHoppingAnimation(currentEntranceRotationAnimation, moveRotationAnimation);

            _currentScaleAnimation.addListener(_onProgressChanged);
            _previousScaleAnimation.addListener(_onProgressChanged);
        }

        void _handlePreviousAnimationStatusChanged(AnimationStatus status) {
            setState(() => {
                if (status == AnimationStatus.dismissed) {
                    D.assert(_currentController.status == AnimationStatus.dismissed);
                    if (widget.child != null) {
                        _currentController.forward();
                    }
                }
            });
        }

        bool _isExtendedFloatingActionButton(Widget widget) {
            if (!(widget is FloatingActionButton)) {
                return false;
            }

            FloatingActionButton fab = (FloatingActionButton) widget;
            return fab.isExtended;
        }

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget>();

            if (_previousController.status != AnimationStatus.dismissed) {
                if (_isExtendedFloatingActionButton(_previousChild)) {
                    children.Add(new FadeTransition(
                        opacity: _previousScaleAnimation,
                        child: _previousChild));
                }
                else {
                    children.Add(new ScaleTransition(
                        scale: _previousScaleAnimation,
                        child: new RotationTransition(
                            turns: _previousRotationAnimation,
                            child: _previousChild)));
                }
            }

            if (_isExtendedFloatingActionButton(widget.child)) {
                children.Add(new ScaleTransition(
                    scale: _extendedCurrentScaleAnimation,
                    child: new FadeTransition(
                        opacity: _currentScaleAnimation,
                        child: widget.child
                    )
                ));
            }
            else {
                children.Add(new ScaleTransition(
                    scale: _currentScaleAnimation,
                    child: new RotationTransition(
                        turns: _currentRotationAnimation,
                        child: widget.child
                    )
                ));
            }

            return new Stack(
                alignment: Alignment.centerRight,
                children: children
            );
        }


        void _onProgressChanged() {
            _updateGeometryScale(Mathf.Max(_previousScaleAnimation.value, _currentScaleAnimation.value));
        }

        void _updateGeometryScale(float scale) {
            widget.geometryNotifier._updateWith(
                floatingActionButtonScale: scale
            );
        }
    }

    public class Scaffold : StatefulWidget {
        public Scaffold(
            Key key = null,
            PreferredSizeWidget appBar = null,
            Widget body = null,
            Widget floatingActionButton = null,
            FloatingActionButtonLocation floatingActionButtonLocation = null,
            FloatingActionButtonAnimator floatingActionButtonAnimator = null,
            List<Widget> persistentFooterButtons = null,
            Widget drawer = null,
            Widget endDrawer = null,
            Widget bottomNavigationBar = null,
            Widget bottomSheet = null,
            Color backgroundColor = null,
            bool? resizeToAvoidBottomPadding = null,
            bool? resizeToAvoidBottomInset = null,
            bool primary = true,
            DragStartBehavior drawerDragStartBehavior = DragStartBehavior.start,
            bool extendBody = false
        ) : base(key: key) {
            this.appBar = appBar;
            this.body = body;
            this.floatingActionButton = floatingActionButton;
            this.floatingActionButtonLocation = floatingActionButtonLocation;
            this.floatingActionButtonAnimator = floatingActionButtonAnimator;
            this.persistentFooterButtons = persistentFooterButtons;
            this.drawer = drawer;
            this.endDrawer = endDrawer;
            this.bottomNavigationBar = bottomNavigationBar;
            this.bottomSheet = bottomSheet;
            this.backgroundColor = backgroundColor;
            this.resizeToAvoidBottomPadding = resizeToAvoidBottomPadding;
            this.resizeToAvoidBottomInset = resizeToAvoidBottomInset;
            this.primary = primary;
            this.drawerDragStartBehavior = drawerDragStartBehavior;
            this.extendBody = extendBody;
        }

        public readonly bool extendBody;

        public readonly PreferredSizeWidget appBar;

        public readonly Widget body;

        public readonly Widget floatingActionButton;

        public readonly FloatingActionButtonLocation floatingActionButtonLocation;

        public readonly FloatingActionButtonAnimator floatingActionButtonAnimator;

        public readonly List<Widget> persistentFooterButtons;

        public readonly Widget drawer;

        public readonly Widget endDrawer;

        public readonly Color backgroundColor;

        public readonly Widget bottomNavigationBar;

        public readonly Widget bottomSheet;

        public readonly bool? resizeToAvoidBottomPadding;

        public readonly bool? resizeToAvoidBottomInset;

        public readonly bool primary;

        public readonly DragStartBehavior drawerDragStartBehavior;

        public static ScaffoldState of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            ScaffoldState result = (ScaffoldState) context.ancestorStateOfType(new TypeMatcher<ScaffoldState>());
            if (nullOk || result != null) {
                return result;
            }

            throw new UIWidgetsError(
                "Scaffold.of() called with a context that does not contain a Scaffold.\n" +
                "No Scaffold ancestor could be found starting from the context that was passed to Scaffold.of(). " +
                "This usually happens when the context provided is from the same StatefulWidget as that " +
                "whose build function actually creates the Scaffold widget being sought.\n" +
                "There are several ways to avoid this problem. The simplest is to use a Builder to get a " +
                "context that is \"under\" the Scaffold. For an example of this, please see the " +
                "documentation for Scaffold.of():\n" +
                "  https://docs.flutter.io/flutter/material/Scaffold/of.html\n" +
                "A more efficient solution is to split your build function into several widgets. This " +
                "introduces a new context from which you can obtain the Scaffold. In this solution, " +
                "you would have an outer widget that creates the Scaffold populated by instances of " +
                "your new inner widgets, and then in these inner widgets you would use Scaffold.of().\n" +
                "A less elegant but more expedient solution is assign a GlobalKey to the Scaffold, " +
                "then use the key.currentState property to obtain the ScaffoldState rather than " +
                "using the Scaffold.of() function.\n" +
                "The context used was:\n" + context);
        }

        public static ValueListenable<ScaffoldGeometry> geometryOf(BuildContext context) {
            _ScaffoldScope scaffoldScope =
                (_ScaffoldScope) context.inheritFromWidgetOfExactType(typeof(_ScaffoldScope));
            if (scaffoldScope == null) {
                throw new UIWidgetsError(
                    "Scaffold.geometryOf() called with a context that does not contain a Scaffold.\n" +
                    "This usually happens when the context provided is from the same StatefulWidget as that " +
                    "whose build function actually creates the Scaffold widget being sought.\n" +
                    "There are several ways to avoid this problem. The simplest is to use a Builder to get a " +
                    "context that is \"under\" the Scaffold. For an example of this, please see the " +
                    "documentation for Scaffold.of():\n" +
                    "  https://docs.flutter.io/flutter/material/Scaffold/of.html\n" +
                    "A more efficient solution is to split your build function into several widgets. This " +
                    "introduces a new context from which you can obtain the Scaffold. In this solution, " +
                    "you would have an outer widget that creates the Scaffold populated by instances of " +
                    "your new inner widgets, and then in these inner widgets you would use Scaffold.geometryOf().\n" +
                    "The context used was:\n" + context);
            }

            return scaffoldScope.geometryNotifier;
        }

        static bool hasDrawer(BuildContext context, bool registerForUpdates = true) {
            D.assert(context != null);
            if (registerForUpdates) {
                _ScaffoldScope scaffold = (_ScaffoldScope) context.inheritFromWidgetOfExactType(typeof(_ScaffoldScope));
                return scaffold?.hasDrawer ?? false;
            }
            else {
                ScaffoldState scaffold = (ScaffoldState) context.ancestorStateOfType(new TypeMatcher<ScaffoldState>());
                return scaffold?.hasDrawer ?? false;
            }
        }

        public override State createState() {
            return new ScaffoldState();
        }
    }

    public class ScaffoldState : TickerProviderStateMixin<Scaffold> {
        // DRAWER API
        public readonly GlobalKey<DrawerControllerState> _drawerKey = GlobalKey<DrawerControllerState>.key();
        public readonly GlobalKey<DrawerControllerState> _endDrawerKey = GlobalKey<DrawerControllerState>.key();

        public bool hasDrawer {
            get { return widget.drawer != null; }
        }

        public bool hasEndDrawer {
            get { return widget.endDrawer != null; }
        }

        bool _drawerOpened = false;
        bool _endDrawerOpened = false;

        public bool isDrawerOpen {
            get { return _drawerOpened; }
        }

        public bool isEndDrawerOpen {
            get { return _endDrawerOpened; }
        }

        void _drawerOpenedCallback(bool isOpened) {
            setState(() => { _drawerOpened = isOpened; });
        }

        void _endDrawerOpenedCallback(bool isOpened) {
            setState(() => { _endDrawerOpened = isOpened; });
        }


        public void openDrawer() {
            if (_endDrawerKey.currentState != null && _endDrawerOpened) {
                _endDrawerKey.currentState.close();
            }

            _drawerKey.currentState?.open();
        }

        public void openEndDrawer() {
            if (_drawerKey.currentState != null && _drawerOpened) {
                _drawerKey.currentState.close();
            }

            _endDrawerKey.currentState?.open();
        }

        // SNACK BAR API
        readonly Queue<ScaffoldFeatureController<SnackBar, SnackBarClosedReason>> _snackBars =
            new Queue<ScaffoldFeatureController<SnackBar, SnackBarClosedReason>>();

        AnimationController _snackBarController;
        Timer _snackBarTimer;
        bool _accessibleNavigation;

        public ScaffoldFeatureController<SnackBar, SnackBarClosedReason> showSnackBar(SnackBar snackbar) {
            if (_snackBarController == null) {
                _snackBarController = SnackBar.createAnimationController(vsync: this);
                _snackBarController.addStatusListener(_handleSnackBarStatusChange);
            }

            if (_snackBars.isEmpty()) {
                D.assert(_snackBarController.isDismissed);
                _snackBarController.forward();
            }

            ScaffoldFeatureController<SnackBar, SnackBarClosedReason> controller = null;
            controller = new ScaffoldFeatureController<SnackBar, SnackBarClosedReason>(
                snackbar.withAnimation(_snackBarController, fallbackKey: new UniqueKey()),
                new Promise<SnackBarClosedReason>(),
                () => {
                    D.assert(_snackBars.First() == controller);
                    hideCurrentSnackBar(reason: SnackBarClosedReason.hide);
                },
                null);

            setState(() => { _snackBars.Enqueue(controller); });
            return controller;
        }


        void _handleSnackBarStatusChange(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.dismissed: {
                    D.assert(_snackBars.isNotEmpty());
                    setState(() => { _snackBars.Dequeue(); });
                    if (_snackBars.isNotEmpty()) {
                        _snackBarController.forward();
                    }

                    break;
                }
                case AnimationStatus.completed: {
                    setState(() => { D.assert(_snackBarTimer == null); });
                    break;
                }
                case AnimationStatus.forward:
                case AnimationStatus.reverse: {
                    break;
                }
            }
        }

        public void removeCurrentSnackBar(SnackBarClosedReason reason = SnackBarClosedReason.remove) {
            if (_snackBars.isEmpty()) {
                return;
            }

            Promise<SnackBarClosedReason> completer = _snackBars.First()._completer;
            if (!completer.isCompleted) {
                completer.Resolve(reason);
            }

            _snackBarTimer?.cancel();
            _snackBarTimer = null;
            _snackBarController.setValue(0.0f);
        }

        public void hideCurrentSnackBar(SnackBarClosedReason reason = SnackBarClosedReason.hide) {
            if (_snackBars.isEmpty() || _snackBarController.status == AnimationStatus.dismissed) {
                return;
            }

            MediaQueryData mediaQuery = MediaQuery.of(context);
            Promise<SnackBarClosedReason> completer = _snackBars.First()._completer;
            if (mediaQuery.accessibleNavigation) {
                _snackBarController.setValue(0.0f);
                completer.Resolve(reason);
            }
            else {
                _snackBarController.reverse().Then(() => {
                    D.assert(mounted);
                    if (!completer.isCompleted) {
                        completer.Resolve(reason);
                    }
                });
            }

            _snackBarTimer?.cancel();
            _snackBarTimer = null;
        }

        // PERSISTENT BOTTOM SHEET API
        readonly List<_PersistentBottomSheet> _dismissedBottomSheets = new List<_PersistentBottomSheet>();
        PersistentBottomSheetController<object> _currentBottomSheet;

        void _maybeBuildCurrentBottomSheet() {
            if (widget.bottomSheet != null) {
                AnimationController controller = BottomSheet.createAnimationController(this);
                controller.setValue(1.0f);
                _currentBottomSheet = _buildBottomSheet<object>(
                    (BuildContext context) => widget.bottomSheet,
                    controller,
                    false,
                    null);
            }
        }

        void _closeCurrentBottomSheet() {
            if (_currentBottomSheet != null) {
                _currentBottomSheet.close();
                D.assert(_currentBottomSheet == null);
            }
        }


        PersistentBottomSheetController<T> _buildBottomSheet<T>(WidgetBuilder builder, AnimationController controller,
            bool isLocalHistoryEntry, T resolveValue) {
            Promise<T> completer = new Promise<T>();
            GlobalKey<_PersistentBottomSheetState> bottomSheetKey = GlobalKey<_PersistentBottomSheetState>.key();
            _PersistentBottomSheet bottomSheet = null;

            void _removeCurrentBottomSheet() {
                D.assert(_currentBottomSheet._widget == bottomSheet);
                D.assert(bottomSheetKey.currentState != null);
                bottomSheetKey.currentState.close();
                if (controller.status != AnimationStatus.dismissed) {
                    _dismissedBottomSheets.Add(bottomSheet);
                }

                setState(() => { _currentBottomSheet = null; });
                completer.Resolve(resolveValue);
            }

            LocalHistoryEntry entry = isLocalHistoryEntry
                ? new LocalHistoryEntry(onRemove: _removeCurrentBottomSheet)
                : null;

            bottomSheet = new _PersistentBottomSheet(
                key: bottomSheetKey,
                animationController: controller,
                enableDrag: isLocalHistoryEntry,
                onClosing: () => {
                    D.assert(_currentBottomSheet._widget == bottomSheet);
                    if (isLocalHistoryEntry) {
                        entry.remove();
                    }
                    else {
                        _removeCurrentBottomSheet();
                    }
                },
                onDismissed: () => {
                    if (_dismissedBottomSheets.Contains(bottomSheet)) {
                        bottomSheet.animationController.dispose();
                        setState(() => { _dismissedBottomSheets.Remove(bottomSheet); });
                    }
                },
                builder: builder);

            if (isLocalHistoryEntry) {
                ModalRoute.of(context).addLocalHistoryEntry(entry);
            }

            return new PersistentBottomSheetController<T>(
                bottomSheet,
                completer,
                isLocalHistoryEntry ? (VoidCallback) entry.remove : _removeCurrentBottomSheet,
                (VoidCallback fn) => { bottomSheetKey.currentState?.setState(fn); },
                isLocalHistoryEntry);
        }

        public PersistentBottomSheetController<object> showBottomSheet(WidgetBuilder builder) {
            _closeCurrentBottomSheet();
            AnimationController controller = BottomSheet.createAnimationController(this);
            controller.forward();
            setState(() => {
                _currentBottomSheet = _buildBottomSheet<object>(builder, controller, true, null);
            });
            return _currentBottomSheet;
        }

        // FLOATING ACTION BUTTON API
        AnimationController _floatingActionButtonMoveController;
        FloatingActionButtonAnimator _floatingActionButtonAnimator;
        FloatingActionButtonLocation _previousFloatingActionButtonLocation;
        FloatingActionButtonLocation _floatingActionButtonLocation;

        void _moveFloatingActionButton(FloatingActionButtonLocation newLocation) {
            FloatingActionButtonLocation previousLocation = _floatingActionButtonLocation;
            float restartAnimationFrom = 0.0f;
            if (_floatingActionButtonMoveController.isAnimating) {
                previousLocation = new _TransitionSnapshotFabLocation(_previousFloatingActionButtonLocation,
                    _floatingActionButtonLocation,
                    _floatingActionButtonAnimator,
                    _floatingActionButtonMoveController.value);
                restartAnimationFrom =
                    _floatingActionButtonAnimator.getAnimationRestart(_floatingActionButtonMoveController
                        .value);
            }

            setState(() => {
                _previousFloatingActionButtonLocation = previousLocation;
                _floatingActionButtonLocation = newLocation;
            });

            _floatingActionButtonMoveController.forward(from: restartAnimationFrom);
        }

        // IOS FEATURES
        readonly ScrollController _primaryScrollController = new ScrollController();

        void _handleStatusBarTap() {
            if (_primaryScrollController.hasClients) {
                _primaryScrollController.animateTo(
                    to: 0.0f,
                    duration: new TimeSpan(0, 0, 0, 0, 300),
                    curve: Curves.linear);
            }
        }

        // INTERNALS
        _ScaffoldGeometryNotifier _geometryNotifier;

        bool _resizeToAvoidBottomInset {
            get { return widget.resizeToAvoidBottomInset ?? widget.resizeToAvoidBottomPadding ?? true; }
        }

        public override void initState() {
            base.initState();
            _geometryNotifier = new _ScaffoldGeometryNotifier(new ScaffoldGeometry(), context);
            _floatingActionButtonLocation = widget.floatingActionButtonLocation ??
                                                 ScaffoldUtils._kDefaultFloatingActionButtonLocation;
            _floatingActionButtonAnimator = widget.floatingActionButtonAnimator ??
                                                 ScaffoldUtils._kDefaultFloatingActionButtonAnimator;
            _previousFloatingActionButtonLocation = _floatingActionButtonLocation;
            _floatingActionButtonMoveController = new AnimationController(
                vsync: this,
                lowerBound: 0.0f,
                upperBound: 1.0f,
                value: 1.0f,
                duration: FloatingActionButtonLocationUtils.kFloatingActionButtonSegue +
                          FloatingActionButtonLocationUtils.kFloatingActionButtonSegue
            );

            _maybeBuildCurrentBottomSheet();
        }


        public override void didUpdateWidget(StatefulWidget oldWidget) {
            Scaffold _oldWidget = (Scaffold) oldWidget;
            if (widget.floatingActionButtonAnimator != _oldWidget.floatingActionButtonAnimator) {
                _floatingActionButtonAnimator = widget.floatingActionButtonAnimator ??
                                                     ScaffoldUtils._kDefaultFloatingActionButtonAnimator;
            }

            if (widget.floatingActionButtonLocation != _oldWidget.floatingActionButtonLocation) {
                _moveFloatingActionButton(widget.floatingActionButtonLocation ??
                                               ScaffoldUtils._kDefaultFloatingActionButtonLocation);
            }

            if (widget.bottomSheet != _oldWidget.bottomSheet) {
                D.assert(() => {
                    if (widget.bottomSheet != null && _currentBottomSheet?._isLocalHistoryEntry == true) {
                        throw new UIWidgetsError(
                            "Scaffold.bottomSheet cannot be specified while a bottom sheet displayed " +
                            "with showBottomSheet() is still visible.\n Use the PersistentBottomSheetController " +
                            "returned by showBottomSheet() to close the old bottom sheet before creating " +
                            "a Scaffold with a (non null) bottomSheet.");
                    }

                    return true;
                });
                _closeCurrentBottomSheet();
                _maybeBuildCurrentBottomSheet();
            }

            base.didUpdateWidget(oldWidget);
        }


        public override void didChangeDependencies() {
            MediaQueryData mediaQuery = MediaQuery.of(context);

            if (_accessibleNavigation
                && !mediaQuery.accessibleNavigation
                && _snackBarTimer != null) {
                hideCurrentSnackBar(reason: SnackBarClosedReason.timeout);
            }

            _accessibleNavigation = mediaQuery.accessibleNavigation;
            base.didChangeDependencies();
        }

        public override void dispose() {
            _snackBarController?.dispose();
            _snackBarTimer?.cancel();
            _snackBarTimer = null;
            _geometryNotifier.dispose();
            foreach (_PersistentBottomSheet bottomSheet in _dismissedBottomSheets) {
                bottomSheet.animationController.dispose();
            }

            if (_currentBottomSheet != null) {
                _currentBottomSheet._widget.animationController.dispose();
            }

            _floatingActionButtonMoveController.dispose();
            base.dispose();
        }

        void _addIfNonNull(List<LayoutId> children, Widget child, object childId,
            bool removeLeftPadding,
            bool removeTopPadding,
            bool removeRightPadding,
            bool removeBottomPadding,
            bool removeBottomInset = false
        ) {
            MediaQueryData data = MediaQuery.of(context).removePadding(
                removeLeft: removeLeftPadding,
                removeTop: removeTopPadding,
                removeRight: removeRightPadding,
                removeBottom: removeBottomPadding
            );
            if (removeBottomInset) {
                data = data.removeViewInsets(removeBottom: true);
            }

            if (child != null) {
                children.Add(
                    new LayoutId(
                        id: childId,
                        child: new MediaQuery(data: data, child: child)
                    )
                );
            }
        }

        void _buildEndDrawer(List<LayoutId> children) {
            if (widget.endDrawer != null) {
                D.assert(hasEndDrawer);
                _addIfNonNull(
                    children: children,
                    new DrawerController(
                        key: _endDrawerKey,
                        alignment: DrawerAlignment.end,
                        child: widget.endDrawer,
                        drawerCallback: _endDrawerOpenedCallback,
                        dragStartBehavior: widget.drawerDragStartBehavior
                    ),
                    childId: _ScaffoldSlot.endDrawer,
                    removeLeftPadding: true,
                    removeTopPadding: false,
                    removeRightPadding: false,
                    removeBottomPadding: false
                );
            }
        }

        void _buildDrawer(List<LayoutId> children) {
            if (widget.drawer != null) {
                D.assert(hasDrawer);
                _addIfNonNull(
                    children: children,
                    new DrawerController(
                        key: _drawerKey,
                        alignment: DrawerAlignment.start,
                        child: widget.drawer,
                        drawerCallback: _drawerOpenedCallback,
                        dragStartBehavior: widget.drawerDragStartBehavior
                    ),
                    childId: _ScaffoldSlot.drawer,
                    removeLeftPadding: false,
                    removeTopPadding: false,
                    removeRightPadding: true,
                    removeBottomPadding: false
                );
            }
        }

        public override Widget build(BuildContext context) {
            MediaQueryData mediaQuery = MediaQuery.of(context);
            ThemeData themeData = Theme.of(context);

            _accessibleNavigation = mediaQuery.accessibleNavigation;

            if (_snackBars.isNotEmpty()) {
                ModalRoute route = ModalRoute.of(context);
                if (route == null || route.isCurrent) {
                    if (_snackBarController.isCompleted && _snackBarTimer == null) {
                        SnackBar snackBar = _snackBars.First()._widget;
                        _snackBarTimer = Window.instance.run(snackBar.duration, () => {
                            D.assert(_snackBarController.status == AnimationStatus.forward ||
                                     _snackBarController.status == AnimationStatus.completed);
                            MediaQueryData subMediaQuery = MediaQuery.of(context);
                            if (subMediaQuery.accessibleNavigation && snackBar.action != null) {
                                return;
                            }

                            hideCurrentSnackBar(reason: SnackBarClosedReason.timeout);
                        });
                    }
                }
                else {
                    _snackBarTimer?.cancel();
                    _snackBarTimer = null;
                }
            }

            List<LayoutId> children = new List<LayoutId>();

            _addIfNonNull(
                children: children,
                child: widget.body != null && widget.extendBody
                    ? new _BodyBuilder(body: widget.body) : widget.body,
                childId: _ScaffoldSlot.body,
                removeLeftPadding: false,
                removeTopPadding: widget.appBar != null,
                removeRightPadding: false,
                removeBottomPadding: widget.bottomNavigationBar != null ||
                                     widget.persistentFooterButtons != null,
                removeBottomInset: _resizeToAvoidBottomInset
            );

            if (widget.appBar != null) {
                float topPadding = widget.primary ? mediaQuery.padding.top : 0.0f;
                float extent = widget.appBar.preferredSize.height + topPadding;
                D.assert(extent >= 0.0f && extent.isFinite());
                _addIfNonNull(
                    children: children,
                    new ConstrainedBox(
                        constraints: new BoxConstraints(maxHeight: extent),
                        child: FlexibleSpaceBar.createSettings(
                            currentExtent: extent,
                            child: widget.appBar
                        )
                    ),
                    childId: _ScaffoldSlot.appBar,
                    removeLeftPadding: false,
                    removeTopPadding: false,
                    removeRightPadding: false,
                    removeBottomPadding: true
                );
            }

            if (_snackBars.isNotEmpty()) {
                _addIfNonNull(
                    children: children,
                    child: _snackBars.First()._widget,
                    childId: _ScaffoldSlot.snackBar,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: widget.bottomNavigationBar != null ||
                                         widget.persistentFooterButtons != null
                );
            }

            if (widget.persistentFooterButtons != null) {
                _addIfNonNull(
                    children: children,
                    new Container(
                        decoration: new BoxDecoration(
                            border: new Border(
                                top: Divider.createBorderSide(context, width: 1.0f)
                            )
                        ),
                        child: new SafeArea(
                            child: ButtonTheme.bar(
                                child: new SafeArea(
                                    top: false,
                                    child: new ButtonBar(
                                        children: widget.persistentFooterButtons
                                    )
                                )
                            )
                        )
                    ),
                    childId: _ScaffoldSlot.persistentFooter,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: false
                );
            }

            if (widget.bottomNavigationBar != null) {
                _addIfNonNull(
                    children: children,
                    child: widget.bottomNavigationBar,
                    childId: _ScaffoldSlot.bottomNavigationBar,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: false
                );
            }

            if (_currentBottomSheet != null || _dismissedBottomSheets.isNotEmpty()) {
                List<Widget> bottomSheets = new List<Widget>();
                if (_dismissedBottomSheets.isNotEmpty()) {
                    bottomSheets.AddRange(_dismissedBottomSheets);
                }

                if (_currentBottomSheet != null) {
                    bottomSheets.Add(_currentBottomSheet._widget);
                }

                Widget stack = new Stack(
                    children: bottomSheets,
                    alignment: Alignment.bottomCenter
                );
                _addIfNonNull(
                    children: children,
                    child: stack,
                    childId: _ScaffoldSlot.bottomSheet,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: _resizeToAvoidBottomInset
                );
            }

            _addIfNonNull(
                children: children,
                new _FloatingActionButtonTransition(
                    child: widget.floatingActionButton,
                    fabMoveAnimation: _floatingActionButtonMoveController,
                    fabMotionAnimator: _floatingActionButtonAnimator,
                    geometryNotifier: _geometryNotifier
                ),
                childId: _ScaffoldSlot.floatingActionButton,
                removeLeftPadding: true,
                removeTopPadding: true,
                removeRightPadding: true,
                removeBottomPadding: true
            );

            switch (themeData.platform) {
                case RuntimePlatform.IPhonePlayer:
                    _addIfNonNull(
                        children: children,
                        new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onTap: _handleStatusBarTap
                        ),
                        childId: _ScaffoldSlot.statusBar,
                        removeLeftPadding: false,
                        removeTopPadding: true,
                        removeRightPadding: false,
                        removeBottomPadding: true
                    );
                    break;
            }

            if (_endDrawerOpened) {
                _buildDrawer(children);
                _buildEndDrawer(children);
            }
            else {
                _buildEndDrawer(children);
                _buildDrawer(children);
            }

            EdgeInsets minInsets = mediaQuery.padding.copyWith(
                bottom: _resizeToAvoidBottomInset ? mediaQuery.viewInsets.bottom : 0.0f
            );

            bool _extendBody = !(minInsets.bottom > 0) && widget.extendBody;
            
            return new _ScaffoldScope(
                hasDrawer: hasDrawer,
                geometryNotifier: _geometryNotifier,
                child: new PrimaryScrollController(
                    controller: _primaryScrollController,
                    child: new Material(
                        color: widget.backgroundColor ?? themeData.scaffoldBackgroundColor,
                        child: new AnimatedBuilder(animation: _floatingActionButtonMoveController,
                            builder: (BuildContext subContext, Widget child) => {
                                return new CustomMultiChildLayout(
                                    children: new List<Widget>(children),
                                    layoutDelegate: new _ScaffoldLayout(
                                        extendBody: _extendBody,
                                        minInsets: minInsets,
                                        currentFloatingActionButtonLocation: _floatingActionButtonLocation,
                                        floatingActionButtonMoveAnimationProgress: _floatingActionButtonMoveController.value,
                                        floatingActionButtonMotionAnimator: _floatingActionButtonAnimator,
                                        geometryNotifier: _geometryNotifier,
                                        previousFloatingActionButtonLocation: _previousFloatingActionButtonLocation
                                    )
                                );
                            }
                        )
                    )
                )
            );
        }
    }

    public class ScaffoldFeatureController<T, U> where T : Widget {
        public ScaffoldFeatureController(
            T _widget,
            Promise<U> _completer,
            VoidCallback close,
            StateSetter setState) {
            this._widget = _widget;
            this._completer = _completer;
            this.close = close;
            this.setState = setState;
        }

        public readonly T _widget;

        public readonly Promise<U> _completer;

        public IPromise<U> closed {
            get { return _completer; }
        }

        public readonly VoidCallback close;

        public readonly StateSetter setState;
    }


    public class _PersistentBottomSheet : StatefulWidget {
        public _PersistentBottomSheet(
            Key key = null,
            AnimationController animationController = null,
            bool enableDrag = true,
            VoidCallback onClosing = null,
            VoidCallback onDismissed = null,
            WidgetBuilder builder = null
        ) : base(key: key) {
            this.animationController = animationController;
            this.enableDrag = enableDrag;
            this.onClosing = onClosing;
            this.onDismissed = onDismissed;
            this.builder = builder;
        }

        public readonly AnimationController animationController;

        public readonly bool enableDrag;

        public readonly VoidCallback onClosing;

        public readonly VoidCallback onDismissed;

        public readonly WidgetBuilder builder;

        public override State createState() {
            return new _PersistentBottomSheetState();
        }
    }


    class _PersistentBottomSheetState : State<_PersistentBottomSheet> {
        public override void initState() {
            base.initState();
            D.assert(widget.animationController.status == AnimationStatus.forward
                     || widget.animationController.status == AnimationStatus.completed);
            widget.animationController.addStatusListener(_handleStatusChange);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            _PersistentBottomSheet _oldWidget = (_PersistentBottomSheet) oldWidget;
            D.assert(widget.animationController == _oldWidget.animationController);
        }

        public void close() {
            widget.animationController.reverse();
        }

        void _handleStatusChange(AnimationStatus status) {
            if (status == AnimationStatus.dismissed && widget.onDismissed != null) {
                widget.onDismissed();
            }
        }

        public override Widget build(BuildContext context) {
            return new AnimatedBuilder(
                animation: widget.animationController,
                builder: (BuildContext subContext, Widget child) => {
                    return new Align(
                        alignment: Alignment.topLeft,
                        heightFactor: widget.animationController.value,
                        child: child);
                },
                child: new BottomSheet(
                    animationController: widget.animationController,
                    enableDrag: widget.enableDrag,
                    onClosing: widget.onClosing,
                    builder: widget.builder));
        }
    }


    public class PersistentBottomSheetController<T> : ScaffoldFeatureController<_PersistentBottomSheet, T> {
        public PersistentBottomSheetController(
            _PersistentBottomSheet widget,
            Promise<T> completer,
            VoidCallback close,
            StateSetter setState,
            bool _isLocalHistoryEntry
        ) : base(widget, completer, close, setState) {
            this._isLocalHistoryEntry = _isLocalHistoryEntry;
        }

        public readonly bool _isLocalHistoryEntry;
    }

    class _ScaffoldScope : InheritedWidget {
        public _ScaffoldScope(
            bool? hasDrawer = null,
            _ScaffoldGeometryNotifier geometryNotifier = null,
            Widget child = null
        ) : base(child: child) {
            D.assert(hasDrawer != null);
            this.hasDrawer = hasDrawer.Value;
            this.geometryNotifier = geometryNotifier;
        }

        public readonly bool hasDrawer;
        public readonly _ScaffoldGeometryNotifier geometryNotifier;

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            _ScaffoldScope _oldWidget = (_ScaffoldScope) oldWidget;
            return hasDrawer != _oldWidget.hasDrawer;
        }
    }
}