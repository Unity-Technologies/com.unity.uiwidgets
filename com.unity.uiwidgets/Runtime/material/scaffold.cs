using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidget.material;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
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

        public static readonly Curve _standardBottomSheetCurve = material_.standardEasing;

        public const float _kBottomSheetDominatesPercentage = 0.3f;
        public const float _kMinBottomSheetScrimOpacity = 0.1f;
        public const float _kMaxBottomSheetScrimOpacity = 0.6f;
    }

    enum _ScaffoldSlot {
        body,
        appBar,
        bodyScrim,
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
                            "The ScaffoldGeometry is only available during the paint phase, because" +
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
            float? bottomWidgetsHeight = null,
            float? appBarHeight = null
        ) : base(minWidth: minWidth, maxWidth: maxWidth, minHeight: minHeight, maxHeight: maxHeight) {
            D.assert(bottomWidgetsHeight != null);
            D.assert(bottomWidgetsHeight >= 0);
            D.assert(appBarHeight != null);
            D.assert(appBarHeight >= 0);
            this.bottomWidgetsHeight = bottomWidgetsHeight.Value;
            this.appBarHeight = appBarHeight.Value;
        }

        public readonly float bottomWidgetsHeight;
        public readonly float appBarHeight;

        public bool Equals(_BodyBoxConstraints other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return bottomWidgetsHeight.Equals(other.bottomWidgetsHeight)
                   && appBarHeight.Equals(other.appBarHeight)
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
                hashCode = (hashCode * 397) ^ appBarHeight.GetHashCode();
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
        public _BodyBuilder(
            Key key = null,
            bool? extendBody = null,
            bool? extendBodyBehindAppBar = null,
            Widget body = null) : base(key: key) {
            D.assert(extendBody != null);
            D.assert(extendBodyBehindAppBar != null);
            this.body = body;
            this.extendBody = extendBody.Value;
            this.extendBodyBehindAppBar = extendBodyBehindAppBar.Value;
        }

        public readonly Widget body;
        public readonly bool extendBody;
        public readonly bool extendBodyBehindAppBar;

        public override Widget build(BuildContext context) {
            if (!extendBody && !extendBodyBehindAppBar) {
                return body;
            }

            return new LayoutBuilder(
                builder: (ctx, constraints) => {
                    _BodyBoxConstraints bodyConstraints = (_BodyBoxConstraints) constraints;
                    MediaQueryData metrics = MediaQuery.of(context);
                    float bottom = extendBody
                        ? Mathf.Max(metrics.padding.bottom, bodyConstraints.bottomWidgetsHeight)
                        : metrics.padding.bottom;

                    float top = extendBodyBehindAppBar
                        ? Mathf.Max(metrics.padding.top, bodyConstraints.appBarHeight)
                        : metrics.padding.top;
                    return new MediaQuery(
                        data: metrics.copyWith(
                            padding: metrics.padding.copyWith(
                                top: top,
                                bottom: bottom
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
            bool isSnackBarFloating,
            bool extendBody,
            bool extendBodyBehindAppBar
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
            this.isSnackBarFloating = isSnackBarFloating;
            this.extendBody = extendBody;
            this.extendBodyBehindAppBar = extendBodyBehindAppBar;
        }

        public readonly bool extendBody;

        public readonly bool extendBodyBehindAppBar;

        public readonly EdgeInsets minInsets;

        public readonly _ScaffoldGeometryNotifier geometryNotifier;

        public readonly FloatingActionButtonLocation previousFloatingActionButtonLocation;

        public readonly FloatingActionButtonLocation currentFloatingActionButtonLocation;

        public readonly float floatingActionButtonMoveAnimationProgress;

        public readonly FloatingActionButtonAnimator floatingActionButtonMotionAnimator;

        public readonly bool isSnackBarFloating;


        public override void performLayout(Size size) {
            BoxConstraints looseConstraints = BoxConstraints.loose(size);

            BoxConstraints fullWidthConstraints = looseConstraints.tighten(width: size.width);
            float bottom = size.height;
            float contentTop = 0.0f;
            float bottomWidgetsHeight = 0.0f;
            float appBarHeight = 0.0f;

            if (hasChild(_ScaffoldSlot.appBar)) {
                appBarHeight = layoutChild(_ScaffoldSlot.appBar, fullWidthConstraints).height;
                contentTop = extendBodyBehindAppBar ? 0.0f : appBarHeight;
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
                    bodyMaxHeight = bodyMaxHeight.clamp(0.0f, looseConstraints.maxHeight - contentTop);
                    D.assert(bodyMaxHeight <= Mathf.Max(0.0f, looseConstraints.maxHeight - contentTop));
                }

                BoxConstraints bodyConstraints = new _BodyBoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: bodyMaxHeight,
                    bottomWidgetsHeight: extendBody ? bottomWidgetsHeight : 0.0f,
                    appBarHeight: appBarHeight
                );
                layoutChild(_ScaffoldSlot.body, bodyConstraints);
                positionChild(_ScaffoldSlot.body, new Offset(0.0f, contentTop));
            }

            Size bottomSheetSize = Size.zero;
            Size snackBarSize = Size.zero;

            if (hasChild(_ScaffoldSlot.bodyScrim)) {
                BoxConstraints bottomSheetScrimConstraints = new BoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: contentBottom
                );
                layoutChild(_ScaffoldSlot.bodyScrim, bottomSheetScrimConstraints);
                positionChild(_ScaffoldSlot.bodyScrim, Offset.zero);
            }

            if (hasChild(_ScaffoldSlot.snackBar) && !isSnackBarFloating) {
                snackBarSize = layoutChild(_ScaffoldSlot.snackBar, fullWidthConstraints);
            }

            if (hasChild(_ScaffoldSlot.bottomSheet)) {
                BoxConstraints bottomSheetConstraints = new BoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: Mathf.Max(0.0f, contentBottom - contentTop)
                );
                bottomSheetSize = layoutChild(_ScaffoldSlot.bottomSheet, bottomSheetConstraints);
                positionChild(_ScaffoldSlot.bottomSheet,
                    new Offset((size.width - bottomSheetSize.width) / 2.0f, contentBottom - bottomSheetSize.height));
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

            if (hasChild(_ScaffoldSlot.snackBar)) {
                if (snackBarSize == Size.zero) {
                    snackBarSize = layoutChild(_ScaffoldSlot.snackBar, fullWidthConstraints);
                }

                float snackBarYOffsetBase = 0f;
                if (Scaffold.shouldSnackBarIgnoreFABRect) {
                    if (floatingActionButtonRect.size != Size.zero && isSnackBarFloating) {
                        snackBarYOffsetBase = floatingActionButtonRect.top;
                    }
                    else {
                        snackBarYOffsetBase = contentBottom;
                    }
                }
                else {
                    snackBarYOffsetBase = floatingActionButtonRect != null && isSnackBarFloating
                        ? floatingActionButtonRect.top
                        : contentBottom;
                }

                positionChild(_ScaffoldSlot.snackBar, new Offset(0.0f, snackBarYOffsetBase - snackBarSize.height));
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
                   || _oldDelegate.currentFloatingActionButtonLocation != currentFloatingActionButtonLocation
                   || _oldDelegate.extendBody != extendBody
                   || _oldDelegate.extendBodyBehindAppBar != extendBodyBehindAppBar;
        }
    }


    class _FloatingActionButtonTransition : StatefulWidget {
        public _FloatingActionButtonTransition(
            Key key = null,
            Widget child = null,
            Animation<float> fabMoveAnimation = null,
            FloatingActionButtonAnimator fabMotionAnimator = null,
            _ScaffoldGeometryNotifier geometryNotifier = null,
            AnimationController currentController = null
        ) : base(key: key) {
            D.assert(fabMoveAnimation != null);
            D.assert(fabMotionAnimator != null);
            D.assert(currentController != null);
            this.child = child;
            this.fabMoveAnimation = fabMoveAnimation;
            this.fabMotionAnimator = fabMotionAnimator;
            this.geometryNotifier = geometryNotifier;
            this.currentController = currentController;
        }

        public readonly Widget child;

        public readonly Animation<float> fabMoveAnimation;

        public readonly FloatingActionButtonAnimator fabMotionAnimator;

        public readonly _ScaffoldGeometryNotifier geometryNotifier;

        public readonly AnimationController currentController;

        public override State createState() {
            return new _FloatingActionButtonTransitionState();
        }
    }

    class _FloatingActionButtonTransitionState : TickerProviderStateMixin<_FloatingActionButtonTransition> {
        AnimationController _previousController;

        Animation<float> _previousScaleAnimation;

        Animation<float> _previousRotationAnimation;

        Animation<float> _currentScaleAnimation;

        Animation<float> _extendedCurrentScaleAnimation;

        Animation<float> _currentRotationAnimation;

        Widget _previousChild;

        public override void initState() {
            base.initState();

            _previousController = new AnimationController(
                duration: material_.kFloatingActionButtonSegue,
                vsync: this);
            _previousController.addStatusListener(_handlePreviousAnimationStatusChanged);

            _updateAnimations();

            if (widget.child != null) {
                widget.currentController.setValue(1.0f);
            }
            else {
                _updateGeometryScale(0.0f);
            }
        }

        public override void dispose() {
            _previousController.dispose();
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
                float currentValue = widget.currentController.value;
                if (currentValue == 0.0f || _oldWidget.child == null) {
                    _previousChild = null;
                    if (widget.child != null) {
                        widget.currentController.forward();
                    }
                }
                else {
                    _previousChild = _oldWidget.child;
                    _previousController.setValue(currentValue);
                    _previousController.reverse();
                    widget.currentController.setValue(0.0f);
                }
            }
        }

        static readonly Animatable<float> _entranceTurnTween = new FloatTween(
            begin: 1.0f - material_.kFloatingActionButtonTurnInterval,
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
                parent: widget.currentController,
                curve: Curves.easeIn
            );
            Animation<float> currentEntranceRotationAnimation = widget.currentController.drive(_entranceTurnTween);
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
                    D.assert(widget.currentController.status == AnimationStatus.dismissed);
                    if (widget.child != null) {
                        widget.currentController.forward();
                    }
                }
            });
        }

        bool _isExtendedFloatingActionButton(Widget widget) {
            return widget is FloatingActionButton && ((FloatingActionButton) widget).isExtended;
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
                            child: _previousChild
                        )
                    ));
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
            bool extendBody = false,
            bool extendBodyBehindAppBar = false,
            Color drawerScrimColor = null,
            float? drawerEdgeDragWidth = null,
            bool drawerEnableOpenDragGesture = true,
            bool endDrawerEnableOpenDragGesture = true
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
            this.extendBodyBehindAppBar = extendBodyBehindAppBar;
            this.drawerScrimColor = drawerScrimColor;
            this.drawerEdgeDragWidth = drawerEdgeDragWidth;
            this.drawerEnableOpenDragGesture = drawerEnableOpenDragGesture;
            this.endDrawerEnableOpenDragGesture = endDrawerEnableOpenDragGesture;
        }

        public readonly bool extendBody;

        public readonly bool extendBodyBehindAppBar;

        public readonly PreferredSizeWidget appBar;

        public readonly Widget body;

        public readonly Widget floatingActionButton;

        public readonly FloatingActionButtonLocation floatingActionButtonLocation;

        public readonly FloatingActionButtonAnimator floatingActionButtonAnimator;

        public readonly List<Widget> persistentFooterButtons;

        public readonly Widget drawer;

        public readonly Widget endDrawer;

        public readonly Color drawerScrimColor;

        public readonly Color backgroundColor;

        public readonly Widget bottomNavigationBar;

        public readonly Widget bottomSheet;

        public readonly bool? resizeToAvoidBottomPadding;

        public readonly bool? resizeToAvoidBottomInset;

        public readonly bool primary;

        public readonly DragStartBehavior drawerDragStartBehavior;

        public readonly float? drawerEdgeDragWidth;

        public readonly bool drawerEnableOpenDragGesture;

        public readonly bool endDrawerEnableOpenDragGesture;

        public static readonly bool shouldSnackBarIgnoreFABRect = false;

        public static ScaffoldState of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            ScaffoldState result = context.findAncestorStateOfType<ScaffoldState>();
            if (nullOk || result != null) {
                return result;
            }

            throw new UIWidgetsError(new List<DiagnosticsNode> {
                new ErrorSummary("Scaffold.of() called with a context that does not contain a Scaffold."),
                new ErrorDescription(
                    "No Scaffold ancestor could be found starting from the context that was passed to Scaffold.of(). " +
                    "This usually happens when the context provided is from the same StatefulWidget as that " +
                    "whose build function actually creates the Scaffold widget being sought."),
                new ErrorHint(
                    "There are several ways to avoid this problem. The simplest is to use a Builder to get a " +
                    "context that is \"under\" the Scaffold. For an example of this, please see the " +
                    "documentation for Scaffold.of()"),
                new ErrorHint("A more efficient solution is to split your build function into several widgets. This " +
                              "introduces a new context from which you can obtain the Scaffold. In this solution, " +
                              "you would have an outer widget that creates the Scaffold populated by instances of " +
                              "your new inner widgets, and then in these inner widgets you would use Scaffold.of().\n" +
                              "A less elegant but more expedient solution is assign a GlobalKey to the Scaffold, " +
                              "then use the key.currentState property to obtain the ScaffoldState rather than " +
                              "using the Scaffold.of() function."),
                context.describeElement("The context used was")
            });
        }

        public static ValueListenable<ScaffoldGeometry> geometryOf(BuildContext context) {
            _ScaffoldScope scaffoldScope = context.dependOnInheritedWidgetOfExactType<_ScaffoldScope>();
            if (scaffoldScope == null) {
                throw new UIWidgetsError(new List<DiagnosticsNode> {
                        new ErrorSummary(
                            "Scaffold.geometryOf() called with a context that does not contain a Scaffold."),
                        new ErrorDescription(
                            "This usually happens when the context provided is from the same StatefulWidget as that " +
                            "whose build function actually creates the Scaffold widget being sought."),
                        new ErrorHint(
                            "There are several ways to avoid this problem. The simplest is to use a Builder to get a " +
                            "context that is \"under\" the Scaffold. For an example of this, please see the " +
                            "documentation for Scaffold.of():"),
                        new ErrorHint(
                            "A more efficient solution is to split your build function into several widgets. This " +
                            "introduces a new context from which you can obtain the Scaffold. In this solution, " +
                            "you would have an outer widget that creates the Scaffold populated by instances of " +
                            "your new inner widgets, and then in these inner widgets you would use Scaffold.geometryOf()."),
                        context.describeElement("The context used was")
                    }
                );
            }

            return scaffoldScope.geometryNotifier;
        }

        static bool hasDrawer(BuildContext context, bool registerForUpdates = true) {
            D.assert(context != null);
            if (registerForUpdates) {
                _ScaffoldScope scaffold = context.dependOnInheritedWidgetOfExactType<_ScaffoldScope>();
                return scaffold?.hasDrawer ?? false;
            }
            else {
                ScaffoldState scaffold = context.findAncestorStateOfType<ScaffoldState>();
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

        bool hasAppBar {
            get { return widget.appBar != null; }
        }

        public bool hasDrawer {
            get { return widget.drawer != null; }
        }

        public bool hasEndDrawer {
            get { return widget.endDrawer != null; }
        }

        bool hasFloatingActionButton {
            get { return widget.floatingActionButton != null; }
        }

        float _appBarMaxHeight;

        float appBarMaxHeight {
            get { return _appBarMaxHeight; }
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
                Completer.create(),
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

            Completer completer = _snackBars.First()._completer;
            if (!completer.isCompleted) {
                completer.complete(FutureOr.value(reason));
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
            Completer completer = _snackBars.First()._completer;
            if (mediaQuery.accessibleNavigation) {
                _snackBarController.setValue(0.0f);
                completer.complete(FutureOr.value(reason));
            }
            else {
                _snackBarController.reverse().then((value) => {
                    D.assert(mounted);
                    if (!completer.isCompleted) {
                        completer.complete(FutureOr.value(reason));
                    }
                });
            }

            _snackBarTimer?.cancel();
            _snackBarTimer = null;
        }

        // PERSISTENT BOTTOM SHEET API
        readonly List<_StandardBottomSheet> _dismissedBottomSheets = new List<_StandardBottomSheet>();
        PersistentBottomSheetController<object> _currentBottomSheet;

        void _maybeBuildPersistentBottomSheet() {
            if (widget.bottomSheet != null && _currentBottomSheet == null) {
                AnimationController animationController = BottomSheet.createAnimationController(this);
                animationController.setValue(1.0f);
                LocalHistoryEntry _persistentSheetHistoryEntry = null;

                bool _persistentBottomSheetExtentChanged(DraggableScrollableNotification notification) {
                    if (notification.extent > notification.initialExtent) {
                        if (_persistentSheetHistoryEntry == null) {
                            _persistentSheetHistoryEntry = new LocalHistoryEntry(onRemove: () => {
                                if (notification.extent > notification.initialExtent) {
                                    DraggableScrollableActuator.reset(notification.context);
                                }

                                showBodyScrim(false, 0.0f);
                                _floatingActionButtonVisibilityValue = 1.0f;
                                _persistentSheetHistoryEntry = null;
                            });
                            ModalRoute.of(context).addLocalHistoryEntry(_persistentSheetHistoryEntry);
                        }
                    }
                    else if (_persistentSheetHistoryEntry != null) {
                        ModalRoute.of(context).removeLocalHistoryEntry(_persistentSheetHistoryEntry);
                    }

                    return false;
                }

                _currentBottomSheet = _buildBottomSheet<object>(
                    (BuildContext context) => {
                        return new NotificationListener<DraggableScrollableNotification>(
                            onNotification: _persistentBottomSheetExtentChanged,
                            child: new DraggableScrollableActuator(
                                child: widget.bottomSheet
                            )
                        );
                    },
                    true,
                    animationController: animationController,
                    resolveValue: null
                );
            }
        }

        void _closeCurrentBottomSheet() {
            if (_currentBottomSheet != null) {
                if (!_currentBottomSheet._isLocalHistoryEntry) {
                    _currentBottomSheet.close();
                }

                D.assert(() => {
                    _currentBottomSheet?._completer?.future?.whenComplete(() => {
                        D.assert(_currentBottomSheet == null);
                    });
                    return true;
                });
            }
        }


        PersistentBottomSheetController<T> _buildBottomSheet<T>(WidgetBuilder builder, bool isPersistent,
            AnimationController animationController = null,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            T resolveValue = default) {
            D.assert(() => {
                if (widget.bottomSheet != null && isPersistent && _currentBottomSheet != null) {
                    throw new UIWidgetsError(
                        "Scaffold.bottomSheet cannot be specified while a bottom sheet " +
                        "displayed with showBottomSheet() is still visible.\n" +
                        "Rebuild the Scaffold with a null bottomSheet before calling showBottomSheet()."
                    );
                }

                return true;
            });

            Completer completer = Completer.create();
            GlobalKey<_StandardBottomSheetState> bottomSheetKey = GlobalKey<_StandardBottomSheetState>.key();
            _StandardBottomSheet bottomSheet = null;

            bool removedEntry = false;

            void _removeCurrentBottomSheet() {
                removedEntry = true;
                if (_currentBottomSheet == null) {
                    return;
                }

                D.assert(_currentBottomSheet._widget == bottomSheet);
                D.assert(bottomSheetKey.currentState != null);
                _showFloatingActionButton();

                void _closed(object value) {
                    setState(() => { _currentBottomSheet = null; });

                    if (animationController.status != AnimationStatus.dismissed) {
                        _dismissedBottomSheets.Add(bottomSheet);
                    }

                    completer.complete();
                }

                Future closing = bottomSheetKey.currentState.close();
                if (closing != null) {
                    closing.then(_closed);
                }
                else {
                    _closed(null);
                }
            }

            LocalHistoryEntry entry = isPersistent
                ? null
                : new LocalHistoryEntry(onRemove: () => {
                    if (!removedEntry) {
                        _removeCurrentBottomSheet();
                    }
                });

            bottomSheet = new _StandardBottomSheet(
                key: bottomSheetKey,
                animationController: animationController,
                enableDrag: !isPersistent,
                onClosing: () => {
                    if (_currentBottomSheet == null) {
                        return;
                    }

                    D.assert(_currentBottomSheet._widget == bottomSheet);
                    if (!isPersistent && !removedEntry) {
                        D.assert(entry != null);
                        entry.remove();
                        removedEntry = true;
                    }
                },
                onDismissed: () => {
                    if (_dismissedBottomSheets.Contains(bottomSheet)) {
                        setState(() => { _dismissedBottomSheets.Remove(bottomSheet); });
                    }
                },
                builder: builder,
                isPersistent: isPersistent,
                backgroundColor: backgroundColor,
                elevation: elevation,
                shape: shape,
                clipBehavior: clipBehavior
            );
            if (!isPersistent) {
                ModalRoute.of(context).addLocalHistoryEntry(entry);
            }

            return new PersistentBottomSheetController<T>(
                bottomSheet,
                completer,
                entry != null
                    ? (VoidCallback) entry.remove
                    : _removeCurrentBottomSheet,
                (VoidCallback fn) => { bottomSheetKey.currentState?.setState(fn); },
                !isPersistent
            );
        }

        public PersistentBottomSheetController<object> showBottomSheet(WidgetBuilder builder,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null) {
            D.assert(() => {
                if (widget.bottomSheet != null) {
                    throw new UIWidgetsError(
                        "Scaffold.bottomSheet cannot be specified while a bottom sheet " +
                        "displayed with showBottomSheet() is still visible.\n" +
                        "Rebuild the Scaffold with a null bottomSheet before calling showBottomSheet()."
                    );
                }

                return true;
            });

            D.assert(WidgetsD.debugCheckHasMediaQuery(context));

            _closeCurrentBottomSheet();
            AnimationController controller = BottomSheet.createAnimationController(this);
            controller.forward();
            setState(() => {
                _currentBottomSheet = _buildBottomSheet<object>(
                    builder,
                    false,
                    animationController: controller,
                    backgroundColor: backgroundColor,
                    elevation: elevation,
                    shape: shape,
                    clipBehavior: clipBehavior,
                    null
                );
            });
            return _currentBottomSheet;
        }

        // FLOATING ACTION BUTTON API
        AnimationController _floatingActionButtonMoveController;
        FloatingActionButtonAnimator _floatingActionButtonAnimator;
        FloatingActionButtonLocation _previousFloatingActionButtonLocation;
        FloatingActionButtonLocation _floatingActionButtonLocation;

        AnimationController _floatingActionButtonVisibilityController;

        internal float _floatingActionButtonVisibilityValue {
            get { return _floatingActionButtonVisibilityController.value; }
            set {
                _floatingActionButtonVisibilityController.setValue(value.clamp(
                    _floatingActionButtonVisibilityController.lowerBound,
                    _floatingActionButtonVisibilityController.upperBound
                ));
            }
        }

        TickerFuture _showFloatingActionButton() {
            return _floatingActionButtonVisibilityController.forward();
        }

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
                duration: material_.kFloatingActionButtonSegue +
                          material_.kFloatingActionButtonSegue
            );

            _floatingActionButtonVisibilityController = new AnimationController(
                duration: material_.kFloatingActionButtonSegue,
                vsync: this
            );
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
                        throw new UIWidgetsError(new List<DiagnosticsNode> {
                            new ErrorSummary(
                                "Scaffold.bottomSheet cannot be specified while a bottom sheet displayed " +
                                "with showBottomSheet() is still visible."),
                            new ErrorHint("Use the PersistentBottomSheetController " +
                                          "returned by showBottomSheet() to close the old bottom sheet before creating " +
                                          "a Scaffold with a (non null) bottomSheet.")
                        });
                    }

                    return true;
                });
                _closeCurrentBottomSheet();
                _maybeBuildPersistentBottomSheet();
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
            _maybeBuildPersistentBottomSheet();
            base.didChangeDependencies();
        }

        public override void dispose() {
            _snackBarController?.dispose();
            _snackBarTimer?.cancel();
            _snackBarTimer = null;
            _geometryNotifier.dispose();
            foreach (_StandardBottomSheet bottomSheet in _dismissedBottomSheets) {
                bottomSheet.animationController?.dispose();
            }

            if (_currentBottomSheet != null) {
                _currentBottomSheet._widget.animationController?.dispose();
            }

            _floatingActionButtonVisibilityController.dispose();
            base.dispose();
        }

        void _addIfNonNull(List<LayoutId> children, Widget child, object childId,
            bool removeLeftPadding,
            bool removeTopPadding,
            bool removeRightPadding,
            bool removeBottomPadding,
            bool removeBottomInset = false,
            bool maintainBottomViewPadding = false
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

            if (maintainBottomViewPadding && data.viewInsets.bottom != 0.0f) {
                data = data.copyWith(
                    padding: data.padding.copyWith(bottom: data.viewPadding.bottom)
                );
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
                        dragStartBehavior: widget.drawerDragStartBehavior,
                        scrimColor: widget.drawerScrimColor,
                        edgeDragWidth: widget.drawerEdgeDragWidth,
                        enableOpenDragGesture: widget.endDrawerEnableOpenDragGesture
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
                        dragStartBehavior: widget.drawerDragStartBehavior,
                        scrimColor: widget.drawerScrimColor,
                        edgeDragWidth: widget.drawerEdgeDragWidth,
                        enableOpenDragGesture: widget.drawerEnableOpenDragGesture
                    ),
                    childId: _ScaffoldSlot.drawer,
                    removeLeftPadding: false,
                    removeTopPadding: false,
                    removeRightPadding: true,
                    removeBottomPadding: false
                );
            }
        }

        bool _showBodyScrim = false;
        Color _bodyScrimColor = Colors.black;

        internal void showBodyScrim(bool value, float opacity) {
            if (_showBodyScrim == value && _bodyScrimColor.opacity == opacity) {
                return;
            }

            setState(() => {
                _showBodyScrim = value;
                _bodyScrimColor = Colors.black.withOpacity(opacity);
            });
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            MediaQueryData mediaQuery = MediaQuery.of(context);
            ThemeData themeData = Theme.of(context);
            TextDirection textDirection = Directionality.of(context);
            _accessibleNavigation = mediaQuery.accessibleNavigation;

            if (_snackBars.isNotEmpty()) {
                ModalRoute route = ModalRoute.of(context);
                if (route == null || route.isCurrent) {
                    if (_snackBarController.isCompleted && _snackBarTimer == null) {
                        SnackBar snackBar = _snackBars.First()._widget;
                        _snackBarTimer = Timer.create(snackBar.duration, () => {
                            D.assert(_snackBarController.status == AnimationStatus.forward ||
                                     _snackBarController.status == AnimationStatus.completed);

                            MediaQueryData InnerMediaQuery = MediaQuery.of(context);
                            if (InnerMediaQuery.accessibleNavigation && snackBar.action != null) {
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
                children,
                widget.body == null
                    ? null
                    : new _BodyBuilder(
                        extendBody: widget.extendBody,
                        extendBodyBehindAppBar: widget.extendBodyBehindAppBar,
                        body: widget.body
                    ),
                _ScaffoldSlot.body,
                removeLeftPadding: false,
                removeTopPadding: widget.appBar != null,
                removeRightPadding: false,
                removeBottomPadding: widget.bottomNavigationBar != null || widget.persistentFooterButtons != null,
                removeBottomInset: _resizeToAvoidBottomInset
            );

            if (_showBodyScrim) {
                _addIfNonNull(
                    children,
                    new ModalBarrier(
                        dismissible: false,
                        color: _bodyScrimColor
                    ),
                    _ScaffoldSlot.bodyScrim,
                    removeLeftPadding: true,
                    removeTopPadding: true,
                    removeRightPadding: true,
                    removeBottomPadding: true
                );
            }

            if (widget.appBar != null) {
                float topPadding = widget.primary ? mediaQuery.padding.top : 0.0f;
                _appBarMaxHeight = widget.appBar.preferredSize.height + topPadding;
                D.assert(_appBarMaxHeight >= 0.0f && _appBarMaxHeight.isFinite());
                _addIfNonNull(
                    children,
                    new ConstrainedBox(
                        constraints: new BoxConstraints(maxHeight: _appBarMaxHeight),
                        child: FlexibleSpaceBar.createSettings(
                            currentExtent: _appBarMaxHeight,
                            child: widget.appBar
                        )
                    ),
                    _ScaffoldSlot.appBar,
                    removeLeftPadding: false,
                    removeTopPadding: false,
                    removeRightPadding: false,
                    removeBottomPadding: true
                );
            }

            bool isSnackBarFloating = false;
            if (_snackBars.isNotEmpty()) {
                SnackBarBehavior snackBarBehavior = _snackBars.First()._widget.behavior
                                                    ?? themeData.snackBarTheme.behavior
                                                    ?? SnackBarBehavior.fix;
                isSnackBarFloating = snackBarBehavior == SnackBarBehavior.floating;

                _addIfNonNull(
                    children,
                    _snackBars.First()._widget,
                    _ScaffoldSlot.snackBar,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: widget.bottomNavigationBar != null || widget.persistentFooterButtons != null,
                    maintainBottomViewPadding: !_resizeToAvoidBottomInset
                );
            }

            if (widget.persistentFooterButtons != null) {
                _addIfNonNull(
                    children,
                    new Container(
                        decoration: new BoxDecoration(
                            border: new Border(
                                top: Divider.createBorderSide(context, width: 1.0f)
                            )
                        ),
                        child: new SafeArea(
                            top: false,
                            child: new ButtonBar(
                                children: widget.persistentFooterButtons
                            )
                        )
                    ),
                    _ScaffoldSlot.persistentFooter,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: false,
                    maintainBottomViewPadding: !_resizeToAvoidBottomInset
                );
            }

            if (widget.bottomNavigationBar != null) {
                _addIfNonNull(
                    children,
                    widget.bottomNavigationBar,
                    _ScaffoldSlot.bottomNavigationBar,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: false,
                    maintainBottomViewPadding: !_resizeToAvoidBottomInset
                );
            }

            if (_currentBottomSheet != null || _dismissedBottomSheets.isNotEmpty()) {
                List<Widget> bottomSheetChildren = new List<Widget>();
                bottomSheetChildren.AddRange(_dismissedBottomSheets);
                if (_currentBottomSheet != null) {
                    bottomSheetChildren.Add(_currentBottomSheet._widget);
                }

                Widget stack = new Stack(
                    alignment: Alignment.bottomCenter,
                    children: bottomSheetChildren
                );

                _addIfNonNull(
                    children,
                    stack,
                    _ScaffoldSlot.bottomSheet,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: _resizeToAvoidBottomInset
                );
            }

            _addIfNonNull(
                children,
                new _FloatingActionButtonTransition(
                    child: widget.floatingActionButton,
                    fabMoveAnimation: _floatingActionButtonMoveController,
                    fabMotionAnimator: _floatingActionButtonAnimator,
                    geometryNotifier: _geometryNotifier,
                    currentController: _floatingActionButtonVisibilityController
                ),
                _ScaffoldSlot.floatingActionButton,
                removeLeftPadding: true,
                removeTopPadding: true,
                removeRightPadding: true,
                removeBottomPadding: true
            );

            switch (themeData.platform) {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    _addIfNonNull(
                        children,
                        new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onTap: _handleStatusBarTap
                        ),
                        _ScaffoldSlot.statusBar,
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

            bool _extendBody = minInsets.bottom <= 0 && widget.extendBody;

            return new _ScaffoldScope(
                hasDrawer: hasDrawer,
                geometryNotifier: _geometryNotifier,
                child: new PrimaryScrollController(
                    controller: _primaryScrollController,
                    child: new Material(
                        color: widget.backgroundColor ?? themeData.scaffoldBackgroundColor,
                        child: new AnimatedBuilder(animation: _floatingActionButtonMoveController,
                            builder: (BuildContext subContext, Widget subChild) => {
                                return new CustomMultiChildLayout(
                                    children: new List<Widget>(children),
                                    layoutDelegate: new _ScaffoldLayout(
                                        extendBody: _extendBody,
                                        extendBodyBehindAppBar: widget.extendBodyBehindAppBar,
                                        minInsets: minInsets,
                                        currentFloatingActionButtonLocation: _floatingActionButtonLocation,
                                        floatingActionButtonMoveAnimationProgress: _floatingActionButtonMoveController
                                            .value,
                                        floatingActionButtonMotionAnimator: _floatingActionButtonAnimator,
                                        geometryNotifier: _geometryNotifier,
                                        previousFloatingActionButtonLocation: _previousFloatingActionButtonLocation,
                                        isSnackBarFloating: isSnackBarFloating
                                    )
                                );
                            })
                    )
                )
            );
        }
    }


    public class ScaffoldFeatureController<T, U> where T : Widget {
        public ScaffoldFeatureController(
            T _widget,
            Completer _completer,
            VoidCallback close,
            StateSetter setState) {
            this._widget = _widget;
            this._completer = _completer;
            this.close = close;
            this.setState = setState;
        }

        public readonly T _widget;

        public readonly Completer _completer;

        public Future closed {
            get { return _completer.future; }
        }

        public readonly VoidCallback close;

        public readonly StateSetter setState;
    }

    class _BottomSheetSuspendedCurve : ParametricCurve<float> {
        public _BottomSheetSuspendedCurve(
            float startingPoint,
            Curve curve = null
        ) {
            curve = curve ?? Curves.easeOutCubic;
            this.startingPoint = startingPoint;
            this.curve = curve;
        }

        public readonly float startingPoint;

        public readonly Curve curve;

        public override float transform(float t) {
            D.assert(t >= 0.0f && t <= 1.0f);
            D.assert(startingPoint >= 0.0 && startingPoint <= 1.0);

            if (t < startingPoint) {
                return t;
            }

            if (t == 1.0f) {
                return t;
            }

            float curveProgress = (t - startingPoint) / (1 - startingPoint);
            float transformed = curve.transform(curveProgress);
            return MathUtils.lerpNullableFloat(startingPoint, 1, transformed);
        }

        public override string ToString() {
            return $"{foundation_.describeIdentity(this)}({startingPoint}, {curve})";
        }
    }

    public class _StandardBottomSheet : StatefulWidget {
        public _StandardBottomSheet(
            Key key = null,
            AnimationController animationController = null,
            bool enableDrag = true,
            VoidCallback onClosing = null,
            VoidCallback onDismissed = null,
            WidgetBuilder builder = null,
            bool isPersistent = false,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null
        ) : base(key: key) {
            this.animationController = animationController;
            this.enableDrag = enableDrag;
            this.onClosing = onClosing;
            this.onDismissed = onDismissed;
            this.builder = builder;
            this.isPersistent = isPersistent;
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
        }

        public readonly AnimationController animationController;
        public readonly bool enableDrag;
        public readonly VoidCallback onClosing;
        public readonly VoidCallback onDismissed;
        public readonly WidgetBuilder builder;
        public readonly bool isPersistent;
        public readonly Color backgroundColor;
        public readonly float? elevation;
        public readonly ShapeBorder shape;
        public readonly Clip? clipBehavior;

        public override State createState() {
            return new _StandardBottomSheetState();
        }
    }


    class _StandardBottomSheetState : State<_StandardBottomSheet> {
        ParametricCurve<float> animationCurve = ScaffoldUtils._standardBottomSheetCurve;

        public override void initState() {
            base.initState();
            D.assert(widget.animationController != null);
            D.assert(widget.animationController.status == AnimationStatus.forward
                     || widget.animationController.status == AnimationStatus.completed);
            widget.animationController.addStatusListener(_handleStatusChange);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            _StandardBottomSheet _oldWidget = (_StandardBottomSheet) oldWidget;
            D.assert(widget.animationController == _oldWidget.animationController);
        }

        public Future close() {
            D.assert(widget.animationController != null);
            widget.animationController.reverse();
            if (widget.onClosing != null) {
                widget.onClosing();
            }

            return null;
        }

        void _handleDragStart(DragStartDetails details) {
            animationCurve = Curves.linear;
        }

        void _handleDragEnd(DragEndDetails details, bool? isClosing = null) {
            // Allow the bottom sheet to animate smoothly from its current position.
            animationCurve = new _BottomSheetSuspendedCurve(
                widget.animationController.value,
                curve: ScaffoldUtils._standardBottomSheetCurve
            );
        }

        void _handleStatusChange(AnimationStatus status) {
            if (status == AnimationStatus.dismissed && widget.onDismissed != null) {
                widget.onDismissed();
            }
        }

        bool extentChanged(DraggableScrollableNotification notification) {
            float extentRemaining = 1.0f - notification.extent;
            ScaffoldState scaffold = Scaffold.of(context);
            if (extentRemaining < ScaffoldUtils._kBottomSheetDominatesPercentage) {
                scaffold._floatingActionButtonVisibilityValue =
                    extentRemaining * ScaffoldUtils._kBottomSheetDominatesPercentage * 10;
                scaffold.showBodyScrim(true, Mathf.Max(
                    ScaffoldUtils._kMinBottomSheetScrimOpacity,
                    ScaffoldUtils._kMaxBottomSheetScrimOpacity - scaffold._floatingActionButtonVisibilityValue
                ));
            }
            else {
                scaffold._floatingActionButtonVisibilityValue = 1.0f;
                scaffold.showBodyScrim(false, 0.0f);
            }

            if (notification.extent == notification.minExtent && scaffold.widget.bottomSheet == null) {
                close();
            }

            return false;
        }

        Widget _wrapBottomSheet(Widget bottomSheet) {
            return new NotificationListener<DraggableScrollableNotification>(
                onNotification: extentChanged,
                child: bottomSheet
            );
        }

        public override Widget build(BuildContext context) {
            if (widget.animationController != null) {
                return new AnimatedBuilder(
                    animation: widget.animationController,
                    builder: (BuildContext subContext, Widget subChild) => {
                        return new Align(
                            alignment: AlignmentDirectional.topStart,
                            heightFactor: animationCurve.transform(widget.animationController.value),
                            child: subChild
                        );
                    },
                    child: _wrapBottomSheet(
                        new BottomSheet(
                            animationController: widget.animationController,
                            enableDrag: widget.enableDrag,
                            onDragStart: _handleDragStart,
                            onDragEnd: _handleDragEnd,
                            onClosing: widget.onClosing,
                            builder: widget.builder,
                            backgroundColor: widget.backgroundColor,
                            elevation: widget.elevation,
                            shape: widget.shape,
                            clipBehavior: widget.clipBehavior
                        )
                    )
                );
            }

            return _wrapBottomSheet(
                new BottomSheet(
                    onClosing: widget.onClosing,
                    builder: widget.builder,
                    backgroundColor: widget.backgroundColor
                )
            );
        }
    }


    public class PersistentBottomSheetController<T> : ScaffoldFeatureController<_StandardBottomSheet, T> {
        public PersistentBottomSheetController(
            _StandardBottomSheet widget,
            Completer completer,
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
            Key key = null,
            bool? hasDrawer = null,
            _ScaffoldGeometryNotifier geometryNotifier = null,
            Widget child = null
        ) : base(key: key, child: child) {
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