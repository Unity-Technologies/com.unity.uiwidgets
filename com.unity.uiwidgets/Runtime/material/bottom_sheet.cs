using System;
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

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public static readonly TimeSpan _bottomSheetEnterDuration = new TimeSpan(0, 0, 0, 0, 250);
        public static readonly TimeSpan _bottomSheetExitDuration = new TimeSpan(0, 0, 0, 0, 200);

        public static Curve _modalBottomSheetCurve {
            get => decelerateEasing;
        }

        public const float _minFlingVelocity = 700.0f;
        public const float _closeProgressThreshold = 0.5f;

        public delegate void BottomSheetDragStartHandler(DragStartDetails details);

        public delegate void BottomSheetDragEndHandler(
            DragEndDetails details,
            bool? isClosing = false
        );

        public static Future<T> showModalBottomSheet<T>(
            BuildContext context,
            WidgetBuilder builder,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            Color barrierColor = null,
            bool isScrollControlled = false,
            bool useRootNavigator = false,
            bool isDismissible = true,
            bool enableDrag = true
        ) {
            D.assert(context != null);
            D.assert(builder != null);
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            return Navigator.of(context, rootNavigator: useRootNavigator).push(new _ModalBottomSheetRoute<T>(
                builder: builder,
                theme: Theme.of(context, shadowThemeOnly: true),
                isScrollControlled: isScrollControlled,
                barrierLabel: MaterialLocalizations.of(context).modalBarrierDismissLabel,
                backgroundColor: backgroundColor,
                elevation: elevation,
                shape: shape,
                clipBehavior: clipBehavior,
                isDismissible: isDismissible,
                modalBarrierColor: barrierColor,
                enableDrag: enableDrag
            )).to<T>();
        }

        public static PersistentBottomSheetController<object> showBottomSheet(
            BuildContext context,
            WidgetBuilder builder,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null
        ) {
            D.assert(context != null);
            D.assert(builder != null);
            D.assert(debugCheckHasScaffold(context));
            return Scaffold.of(context).showBottomSheet(
                builder,
                backgroundColor: backgroundColor,
                elevation: elevation,
                shape: shape,
                clipBehavior: clipBehavior
            );
        }
    }


    public class BottomSheet : StatefulWidget {
        public BottomSheet(
            Key key = null,
            AnimationController animationController = null,
            bool enableDrag = true,
            material_.BottomSheetDragStartHandler onDragStart = null,
            material_.BottomSheetDragEndHandler onDragEnd = null,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            VoidCallback onClosing = null,
            WidgetBuilder builder = null
        ) : base(key: key) {
            D.assert(onClosing != null);
            D.assert(builder != null);
            D.assert(elevation == null || elevation >= 0.0);

            this.animationController = animationController;
            this.enableDrag = enableDrag;
            this.elevation = elevation;
            this.onClosing = onClosing;
            this.builder = builder;
            this.onDragStart = onDragStart;
            this.onDragEnd = onDragEnd;
            this.backgroundColor = backgroundColor;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
        }

        public readonly AnimationController animationController;

        public readonly VoidCallback onClosing;

        public readonly WidgetBuilder builder;

        public readonly bool enableDrag;

        public readonly material_.BottomSheetDragStartHandler onDragStart;

        public readonly material_.BottomSheetDragEndHandler onDragEnd;

        public readonly Color backgroundColor;

        public readonly float? elevation;

        public readonly ShapeBorder shape;

        public readonly Clip? clipBehavior;

        public override State createState() {
            return new _BottomSheetState();
        }

        public static AnimationController createAnimationController(TickerProvider vsync) {
            return new AnimationController(
                duration: material_._bottomSheetEnterDuration,
                reverseDuration: material_._bottomSheetExitDuration,
                debugLabel: "BottomSheet",
                vsync: vsync
            );
        }
    }


    class _BottomSheetState : State<BottomSheet> {
        readonly GlobalKey _childKey = GlobalKey.key(debugLabel: "BottomSheet child");

        float? _childHeight {
            get {
                RenderBox renderBox = (RenderBox) _childKey.currentContext.findRenderObject();
                return renderBox.size.height;
            }
        }

        bool _dismissUnderway {
            get { return widget.animationController.status == AnimationStatus.reverse; }
        }

        void _handleDragStart(DragStartDetails details) {
            if (widget.onDragStart != null) {
                widget.onDragStart(details);
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            D.assert(widget.enableDrag);
            if (_dismissUnderway) {
                return;
            }

            widget.animationController.setValue(
                widget.animationController.value -
                details.primaryDelta.Value / (_childHeight ?? details.primaryDelta.Value));
        }

        void _handleDragEnd(DragEndDetails details) {
            D.assert(widget.enableDrag);
            if (_dismissUnderway) {
                return;
            }

            bool isClosing = false;
            if (details.velocity.pixelsPerSecond.dy > material_._minFlingVelocity) {
                float? flingVelocity = -details.velocity.pixelsPerSecond.dy / _childHeight;
                if (widget.animationController.value > 0.0) {
                    widget.animationController.fling(velocity: flingVelocity ?? 0);
                }

                if (flingVelocity < 0.0) {
                    isClosing = true;
                }
            }
            else if (widget.animationController.value < material_._closeProgressThreshold) {
                if (widget.animationController.value > 0.0)
                    widget.animationController.fling(velocity: -1.0f);
                isClosing = true;
            }
            else {
                widget.animationController.forward();
            }

            if (widget.onDragEnd != null) {
                widget.onDragEnd(
                    details,
                    isClosing: isClosing
                );
            }

            if (isClosing) {
                widget.onClosing();
            }
        }

        public bool extentChanged(DraggableScrollableNotification notification) {
            if (notification.extent == notification.minExtent) {
                widget.onClosing();
            }

            return false;
        }

        public override Widget build(BuildContext context) {
            BottomSheetThemeData bottomSheetTheme = Theme.of(context).bottomSheetTheme;
            Color color = widget.backgroundColor ?? bottomSheetTheme?.backgroundColor;
            float elevation = widget.elevation ?? bottomSheetTheme?.elevation ?? 0;
            ShapeBorder shape = widget.shape ?? bottomSheetTheme?.shape;
            Clip clipBehavior = widget.clipBehavior ?? bottomSheetTheme?.clipBehavior ?? Clip.none;

            Widget bottomSheet = new Material(
                key: _childKey,
                color: color,
                elevation: elevation,
                shape: shape,
                clipBehavior: clipBehavior,
                child: new NotificationListener<DraggableScrollableNotification>(
                    onNotification: extentChanged,
                    child: widget.builder(context)
                )
            );

            return !widget.enableDrag
                ? bottomSheet
                : new GestureDetector(
                    onVerticalDragStart: _handleDragStart,
                    onVerticalDragUpdate: _handleDragUpdate,
                    onVerticalDragEnd: _handleDragEnd,
                    child: bottomSheet
                );
        }
    }

    class _ModalBottomSheetLayout : SingleChildLayoutDelegate {
        public _ModalBottomSheetLayout(float progress, bool isScrollControlled) {
            this.progress = progress;
            this.isScrollControlled = isScrollControlled;
        }


        public readonly float progress;
        public readonly bool isScrollControlled;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: constraints.maxWidth,
                maxWidth: constraints.maxWidth,
                minHeight: 0.0f,
                maxHeight: isScrollControlled
                    ? constraints.maxHeight
                    : constraints.maxHeight * 9.0f / 16.0f
            );
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            return new Offset(0.0f, size.height - childSize.height * progress);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate _oldDelegate) {
            _ModalBottomSheetLayout oldDelegate = _oldDelegate as _ModalBottomSheetLayout;
            return progress != oldDelegate.progress;
        }
    }

    class _ModalBottomSheet<T> : StatefulWidget {
        public _ModalBottomSheet(
            Key key = null,
            _ModalBottomSheetRoute<T> route = null,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            bool isScrollControlled = false,
            bool enableDrag = true
        ) : base(key: key) {
            this.route = route;
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.isScrollControlled = isScrollControlled;
            this.enableDrag = enableDrag;
        }

        public readonly _ModalBottomSheetRoute<T> route;
        public readonly bool isScrollControlled;
        public readonly Color backgroundColor;
        public readonly float? elevation;
        public readonly ShapeBorder shape;
        public readonly Clip? clipBehavior;
        public readonly bool enableDrag;

        public override State createState() {
            return new _ModalBottomSheetState<T>();
        }
    }

    class _ModalBottomSheetState<T> : State<_ModalBottomSheet<T>> {
        ParametricCurve<float> animationCurve = material_._modalBottomSheetCurve;

        string _getRouteLabel(MaterialLocalizations localizations) {
            switch (Theme.of(context).platform) {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "";
                case RuntimePlatform.Android:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return localizations.dialogLabel;
            }

            return null;
        }

        void handleDragStart(DragStartDetails details) {
            animationCurve = Curves.linear;
        }

        void handleDragEnd(DragEndDetails details, bool? isClosing = null) {
            animationCurve = new _BottomSheetSuspendedCurve(
                widget.route.animation.value,
                curve: material_._modalBottomSheetCurve
            );
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            MediaQueryData mediaQuery = MediaQuery.of(context);
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            string routeLabel = _getRouteLabel(localizations);

            return new AnimatedBuilder(
                animation: widget.route.animation,
                builder: (BuildContext _context, Widget child) => {
                    float animationValue = animationCurve.transform(
                        mediaQuery.accessibleNavigation ? 1.0f : widget.route.animation.value
                    );
                    return new ClipRect(
                        child: new CustomSingleChildLayout(
                            layoutDelegate: new _ModalBottomSheetLayout(animationValue, widget.isScrollControlled),
                            child: new BottomSheet(
                                animationController: widget.route._animationController,
                                onClosing: () => {
                                    if (widget.route.isCurrent) {
                                        Navigator.pop<object>(_context);
                                    }
                                },
                                builder: widget.route.builder,
                                backgroundColor: widget.backgroundColor,
                                elevation: widget.elevation,
                                shape: widget.shape,
                                clipBehavior: widget.clipBehavior,
                                enableDrag: widget.enableDrag,
                                onDragStart: handleDragStart,
                                onDragEnd: handleDragEnd
                            )
                        )
                    );
                }
            );
        }
    }

    class _ModalBottomSheetRoute<T> : PopupRoute {
        public _ModalBottomSheetRoute(
            WidgetBuilder builder = null,
            ThemeData theme = null,
            string barrierLabel = null,
            Color backgroundColor = null,
            float? elevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            Color modalBarrierColor = null,
            bool isDismissible = true,
            bool enableDrag = true,
            bool? isScrollControlled = null,
            RouteSettings settings = null
        ) : base(settings: settings) {
            D.assert(isScrollControlled != null);
            this.builder = builder;
            this.theme = theme;
            this.barrierLabel = barrierLabel;
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.modalBarrierColor = modalBarrierColor;
            this.isDismissible = isDismissible;
            this.enableDrag = enableDrag;
        }

        public readonly WidgetBuilder builder;
        public readonly ThemeData theme;
        public readonly bool? isScrollControlled;
        public readonly Color backgroundColor;
        public readonly float? elevation;
        public readonly ShapeBorder shape;
        public readonly Clip? clipBehavior;
        public readonly Color modalBarrierColor;
        public readonly bool isDismissible;
        public readonly bool enableDrag;

        public override TimeSpan transitionDuration {
            get { return material_._bottomSheetEnterDuration; }
        }

        public override
            TimeSpan reverseTransitionDuration {
            get { return material_._bottomSheetExitDuration; }
        }

        public override bool barrierDismissible {
            get { return isDismissible; }
        }


        public override string barrierLabel { get; }

        public override Color barrierColor {
            get { return modalBarrierColor ?? Colors.black54; }
        }

        public AnimationController _animationController;

        public override AnimationController createAnimationController() {
            D.assert(_animationController == null);
            _animationController = BottomSheet.createAnimationController(navigator.overlay);
            return _animationController;
        }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            BottomSheetThemeData sheetTheme = theme?.bottomSheetTheme ?? Theme.of(context).bottomSheetTheme;

            Widget bottomSheet = MediaQuery.removePadding(
                context: context,
                removeTop: true,
                child: new _ModalBottomSheet<T>(
                    route: this,
                    backgroundColor: backgroundColor ?? sheetTheme?.modalBackgroundColor ?? sheetTheme?.backgroundColor,
                    elevation: elevation ?? sheetTheme?.modalElevation ?? sheetTheme?.elevation,
                    shape: shape,
                    clipBehavior: clipBehavior,
                    isScrollControlled: isScrollControlled ?? false,
                    enableDrag: enableDrag
                    )
            );
            if (theme != null) {
                bottomSheet = new Theme(data: theme, child: bottomSheet);
            }

            return bottomSheet;
        }
    }
}