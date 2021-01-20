using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public static class BottomSheetUtils {
        public static readonly TimeSpan _kBottomSheetDuration = new TimeSpan(0, 0, 0, 0, 200);
        public const float _kMinFlingVelocity = 700.0f;
        public const float _kCloseProgressThreshold = 0.5f;

        public static Future<T> showModalBottomSheet<T>(
            BuildContext context,
            WidgetBuilder builder
        ) {
            D.assert(context != null);
            D.assert(builder != null);
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            return Navigator.push<T>(
                context, 
                new _ModalBottomSheetRoute<T>(
                    builder: builder,
                    theme: Theme.of(context, shadowThemeOnly: true),
                    barrierLabel: MaterialLocalizations.of(context).modalBarrierDismissLabel
                ));
        }

        public static PersistentBottomSheetController<object> showBottomSheet(
            BuildContext context,
            WidgetBuilder builder
        ) {
            D.assert(context != null);
            D.assert(builder != null);
            return Scaffold.of(context).showBottomSheet(builder);
        }
    }


    public class BottomSheet : StatefulWidget {
        public BottomSheet(
            Key key = null,
            AnimationController animationController = null,
            bool enableDrag = true,
            float elevation = 0.0f,
            VoidCallback onClosing = null,
            WidgetBuilder builder = null
        ) : base(key: key) {
            D.assert(onClosing != null);
            D.assert(builder != null);
            D.assert(elevation >= 0.0f);
            this.animationController = animationController;
            this.enableDrag = enableDrag;
            this.elevation = elevation;
            this.onClosing = onClosing;
            this.builder = builder;
        }

        public readonly AnimationController animationController;

        public readonly VoidCallback onClosing;

        public readonly WidgetBuilder builder;

        public readonly bool enableDrag;

        public readonly float elevation;

        public override State createState() {
            return new _BottomSheetState();
        }

        public static AnimationController createAnimationController(TickerProvider vsync) {
            return new AnimationController(
                duration: BottomSheetUtils._kBottomSheetDuration,
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

        void _handleDragUpdate(DragUpdateDetails details) {
            if (_dismissUnderway) {
                return;
            }

            widget.animationController.setValue(
                widget.animationController.value -
                details.primaryDelta.Value / (_childHeight ?? details.primaryDelta.Value));
        }

        void _handleDragEnd(DragEndDetails details) {
            if (_dismissUnderway) {
                return;
            }

            if (details.velocity.pixelsPerSecond.dy > BottomSheetUtils._kMinFlingVelocity) {
                float flingVelocity = -details.velocity.pixelsPerSecond.dy / _childHeight.Value;
                if (widget.animationController.value > 0.0f) {
                    widget.animationController.fling(velocity: flingVelocity);
                }

                if (flingVelocity < 0.0f) {
                    widget.onClosing();
                }
            }
            else if (widget.animationController.value < BottomSheetUtils._kCloseProgressThreshold) {
                if (widget.animationController.value > 0.0f) {
                    widget.animationController.fling(velocity: -1.0f);
                }

                widget.onClosing();
            }
            else {
                widget.animationController.forward();
            }
        }

        public override Widget build(BuildContext context) {
            Widget bottomSheet = new Material(
                key: _childKey,
                elevation: widget.elevation,
                child: widget.builder(context)
            );

            return !widget.enableDrag
                ? bottomSheet
                : new GestureDetector(
                    onVerticalDragUpdate: _handleDragUpdate,
                    onVerticalDragEnd: _handleDragEnd,
                    child: bottomSheet
                );
        }
    }

    class _ModalBottomSheetLayout : SingleChildLayoutDelegate {
        public _ModalBottomSheetLayout(float progress) {
            this.progress = progress;
        }


        public readonly float progress;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: constraints.maxWidth,
                maxWidth: constraints.maxWidth,
                minHeight: 0.0f,
                maxHeight: constraints.maxHeight * 9.0f / 16.0f
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
        public _ModalBottomSheet(Key key = null, _ModalBottomSheetRoute<T> route = null) : base(key: key) {
            this.route = route;
        }

        public readonly _ModalBottomSheetRoute<T> route;

        public override State createState() {
            return new _ModalBottomSheetState<T>();
        }
    }

    class _ModalBottomSheetState<T> : State<_ModalBottomSheet<T>> {
        public override Widget build(BuildContext context) {
            MediaQueryData mediaQuery = MediaQuery.of(context);
            MaterialLocalizations localizations = MaterialLocalizations.of(context);

            return new GestureDetector(
                onTap: () => Navigator.pop<object>(context),
                child: new AnimatedBuilder(
                    animation: widget.route.animation,
                    builder: (BuildContext _context, Widget child) => {
                        float animationValue =
                            mediaQuery.accessibleNavigation ? 1.0f : widget.route.animation.value;
                        return new ClipRect(
                            child: new CustomSingleChildLayout(
                                layoutDelegate: new _ModalBottomSheetLayout(animationValue),
                                child: new BottomSheet(
                                    animationController: widget.route._animationController,
                                    onClosing: () => Navigator.pop<object>(_context),
                                    builder: widget.route.builder
                                )
                            )
                        );
                    }
                )
            );
        }
    }

    class _ModalBottomSheetRoute<T> : PopupRoute {
        public _ModalBottomSheetRoute(
            WidgetBuilder builder = null,
            ThemeData theme = null,
            string barrierLabel = null,
            RouteSettings settings = null
        ) : base(settings: settings) {
            this.builder = builder;
            this.theme = theme;
            this.barrierLabel = barrierLabel;
        }

        public readonly WidgetBuilder builder;
        public readonly ThemeData theme;

        public override TimeSpan transitionDuration {
            get { return BottomSheetUtils._kBottomSheetDuration; }
        }

        public override bool barrierDismissible {
            get { return true; }
        }

        public readonly string barrierLabel;

        public override Color barrierColor {
            get { return Colors.black54; }
        }

        public AnimationController _animationController;

        public override AnimationController createAnimationController() {
            D.assert(_animationController == null);
            _animationController = BottomSheet.createAnimationController(navigator.overlay);
            return _animationController;
        }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            Widget bottomSheet = MediaQuery.removePadding(
                context: context,
                removeTop: true,
                child: new _ModalBottomSheet<T>(route: this)
            );
            if (theme != null) {
                bottomSheet = new Theme(data: theme, child: bottomSheet);
            }

            return bottomSheet;
        }
    }
}